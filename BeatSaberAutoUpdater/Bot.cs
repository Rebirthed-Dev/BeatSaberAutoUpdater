using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BeatSaberAutoUpdater
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public SlashCommandsExtension Commands { get; private set; }
        public ScoresaberAPI SSAPI { get; private set; }
        public SqliteDatabaseInterface SQLInterface { get; private set; }
        private static Timer dailyTimer;
        private static Timer checkTimer;
        public async Task RunAsync()
        {
            SQLInterface = new SqliteDatabaseInterface();
            SSAPI = new ScoresaberAPI();
            var json = string.Empty;

            using (var fs = File.OpenRead("Config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var ConfigJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var Config = new DiscordConfiguration
            {
                Token = ConfigJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.AllUnprivileged
            };

            Client = new DiscordClient(Config);

            var services = new ServiceCollection()
                    .AddSingleton<SqliteDatabaseInterface>()
                    .BuildServiceProvider();

            Client.Ready += Client_Ready;

            var Commands_Config = new SlashCommandsConfiguration
            {
                Services = services
            };

            SQLInterface.Initialize_Database();

            Commands = Client.UseSlashCommands(Commands_Config);

            Commands.RegisterCommands<BaseCommandsModule>();

            // Minutely check for new scores
            checkTimer = new Timer(60000);
            // Hook up the Elapsed event for the timer. 
            checkTimer.Elapsed += TimedCheck;
            checkTimer.AutoReset = true;
            checkTimer.Enabled = true;

            // Daily rank change timer
            dailyTimer = new Timer(86400000);
            // Hook up the Elapsed event for the timer. 
            dailyTimer.Elapsed += DailyCheck;
            dailyTimer.AutoReset = true;
            dailyTimer.Enabled = true;

            DiscordActivity status = new DiscordActivity("bad plays", ActivityType.Watching);

            await Client.ConnectAsync(status, UserStatus.Online);

            await Task.Delay(-1);
        }

        private Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }

        private async void TimedCheck(Object source, ElapsedEventArgs e)
        {
            List<long> registered_users = await SQLInterface.Get_Subscribed_UserIDs();
            foreach (long id in registered_users)
            {
                await Check_For_New_Score(id);
            }
        }

        private async void DailyCheck(Object source, ElapsedEventArgs e)
        {
            // Get ranking, compare The Main 4, and output embed as needed + update SQL
            // For all registered users, make API reference, and do above.
            List<long> registered_users = await SQLInterface.Get_Subscribed_UserIDs();
            foreach (long id in registered_users)
            {
                await Daily_Task(id);
            }
        }

        private async Task Daily_Task(long id)
        {
            PlayerJson plrstats = await SSAPI.getRanking(id);
            if (plrstats.Equals(new PlayerJson()) == false)
            {
                List<(float, int, int, float)> result = await SQLInterface.Get_Previous_Daily_Stats(id);
                // 1: oldPP, 2: oldGlobalRank, 3: oldCountryRank, 4: oldAverageAccuracy
                float ppComparison = Math.Abs(plrstats.pp - result[0].Item1);
                float globalChange = Math.Abs(plrstats.rank - result[0].Item2);
                float countryChange = Math.Abs(plrstats.countryRank - result[0].Item3);
                float accuracyChange = Math.Abs(plrstats.scoreStats.averageRankedAccuracy - result[0].Item4);

                // create embed and send
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Title = "Daily Summary",
                    Description = $"What's changed in the past day!",
                    Url = $"https://scoresaber.com/u/" + plrstats.id.ToString(),
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    { Url = plrstats.profilePicture },
                    Color = DiscordColor.Teal,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"made by @paddz_",
                    },
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = $"{plrstats.name}'s",
                    }
                };
                if (globalChange < 0)
                {
                    _ = embed.AddField("Global Rank", $"#{plrstats.rank} (+{globalChange})", true);
                }
                else
                {
                    _ = embed.AddField("Global Rank", $"#{plrstats.rank} (-{globalChange})", true);
                }
                if (countryChange < 0)
                {
                    _ = embed.AddField($"Country Rank ({plrstats.country})", $"#{plrstats.countryRank} (+{countryChange})", true);
                }
                else
                {
                    _ = embed.AddField($"Country Rank ({plrstats.country})", $"#{plrstats.countryRank} (-{countryChange})", true);
                }
                if (ppComparison < 0)
                {
                    _ = embed.AddField("PP", $"{plrstats.pp:0.00} (-{ppComparison:0.00})", true);
                }
                else
                {
                    _ = embed.AddField("PP", $"{plrstats.pp:0.00} (+{ppComparison:0.00})", true);
                }
                if (accuracyChange < 0)
                {
                    _ = embed.AddField("Average Accuracy", $"{plrstats.scoreStats.averageRankedAccuracy:0.00}% (-{accuracyChange:0.00}%)", true);
                }
                else
                {
                    _ = embed.AddField("Average Accuracy", $"{plrstats.scoreStats.averageRankedAccuracy:0.00}% (+{accuracyChange:0.00}%)", true);
                }
                try
                {
                    List<(long, long)> ids = await SQLInterface.Get_Channel_Ids(id);
                    // Channel > Thread > Server
                    DiscordChannel destination_server = await Client.GetChannelAsync((ulong)ids[0].Item1);
                    if (ids[0].Item1 != ids[0].Item2)
                    {
                        // need to go to a thread
                        foreach (DiscordThreadChannel thread in destination_server.Threads)
                        {
                            if (thread.Id == (ulong)ids[0].Item2)
                            {
                                await thread.JoinThreadAsync();
                                await thread.SendMessageAsync(embed);
                            }
                        }
                    }
                    else
                    {
                        await destination_server.SendMessageAsync(embed);
                    }
                }
                finally
                {
                    await SQLInterface.Set_Daily_Stats(id, plrstats.pp, plrstats.rank, plrstats.countryRank, plrstats.scoreStats.averageRankedAccuracy);
                }
            }
        }

        private async Task Check_For_New_Score(long id)
        {
            // Get scores, check if score is more recent than previous score in SQL
            // If score is also ranked, output embed and update SQL#
            ScoreList scorestats = await SSAPI.getScores(id);
            if (scorestats.Equals(new ScoreJson()) == false)
            {
                long result = await SQLInterface.Get_Previous_Score_Time(id);
                DateTimeOffset parsedResult = DateTimeOffset.FromUnixTimeSeconds(result);
                DateTimeOffset songResult = scorestats.scorelist[0].score.timeSet;
                if ((scorestats.scorelist[0].leaderboard.ranked == true) && (songResult.ToUnixTimeSeconds() > parsedResult.ToUnixTimeSeconds())) {
                    PlayerJson playerstats = await SSAPI.getRanking(id);

                    StringBuilder difficultystring = new StringBuilder($"{scorestats.scorelist[0].leaderboard.difficulty.difficultyRaw.Split("_")[1]}");

                    // create embed and send
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                    {
                        Title = $"{scorestats.scorelist[0].leaderboard.songName} - {scorestats.scorelist[0].leaderboard.songAuthorName} - {difficultystring} at {scorestats.scorelist[0].leaderboard.stars} :star:",
                        Description = $"{scorestats.scorelist[0].leaderboard.songSubName} - Mapped by {scorestats.scorelist[0].leaderboard.levelAuthorName}",
                        Url = $"https://scoresaber.com/leaderboard/" + scorestats.scorelist[0].leaderboard.songId.ToString(),
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                        { Url = scorestats.scorelist[0].leaderboard.coverImage },
                        Color = DiscordColor.Teal,
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"made by @paddz_",
                        },
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = $"New score uploaded by {playerstats.name}!",
                            Url = $"https://scoresaber.com/u/" + playerstats.id.ToString(),
                            IconUrl = $"{playerstats.profilePicture}"
                        }
                    };
                    StringBuilder combotext = new StringBuilder($"{scorestats.scorelist[0].score.maxCombo}");
                    if (scorestats.scorelist[0].score.fullCombo == true)
                    {
                        combotext.Append(" (**Full Combo!**)");
                    }
                    _ = embed.AddField("Rank", $"#{scorestats.scorelist[0].score.rank}", true);
                    _ = embed.AddField("PP", $"#{scorestats.scorelist[0].score.pp:0.00} ({(scorestats.scorelist[0].score.pp * scorestats.scorelist[0].score.weight):0.00})", true);
                    _ = embed.AddField("Score", $"{scorestats.scorelist[0].score.modifiedScore}", true);
                    _ = embed.AddField("Max Combo", $"{combotext}", true);
                    _ = embed.AddField("Missed Notes / Bad Cuts", $"{scorestats.scorelist[0].score.missedNotes} / {scorestats.scorelist[0].score.badCuts}", true);
                    try
                    {
                        List<(long, long)> ids = await SQLInterface.Get_Channel_Ids(id);
                        // Channel > Thread > Server
                        DiscordChannel destination_server = await Client.GetChannelAsync((ulong)ids[0].Item1);
                        if (ids[0].Item1 != ids[0].Item2)
                        {
                            // need to go to a thread
                            foreach (DiscordThreadChannel thread in destination_server.Threads)
                            {
                                if (thread.Id == (ulong)ids[0].Item2)
                                {
                                    await thread.JoinThreadAsync();
                                    await thread.SendMessageAsync(embed);
                                }
                            }
                        }
                        else
                        {
                            await destination_server.SendMessageAsync(embed);
                        }
                    }
                    finally
                    {
                        await SQLInterface.Set_Score_Time(id, songResult);
                    }
                }
            }
        }
    }
}
