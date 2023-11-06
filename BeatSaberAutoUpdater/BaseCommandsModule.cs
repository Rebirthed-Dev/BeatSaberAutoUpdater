using System;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberAutoUpdater
{
    public class BaseCommandsModule : ApplicationCommandModule
    {
        public SqliteDatabaseInterface SQLInterface { private get; set; }

        [SlashCommand("HelloWorld", "Is it alive?????")]
        public async Task Greetings_World(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("What's up"));
        }

        [SlashCommand("NotifSub", "Begin notifications 4 ScoreSaber UserID.")]
        public async Task Begin_Notifications(InteractionContext ctx, [Option("ScoresaberID", "The ID for your ScoreSaber profile.", false)] string Idstring)
        {
            long Id = Convert.ToInt64(Idstring);
            if (ctx.Member.Id == 185056654956560384)
            {
                var users = await SQLInterface.Get_Subscribed_UserIDs();
                if (users.Contains(Id) == false)
                {
                    ulong threadId;
                    ulong channelId;
                    if (ctx.Channel.IsThread != false)
                    {
                        threadId = ctx.Channel.Id;
                        channelId = (ulong)ctx.Channel.ParentId;
                    }
                    else
                    {
                        threadId = ctx.Channel.Id;
                        channelId = ctx.Channel.Id;
                    }
                    await SQLInterface.Create_Subscription(Id, channelId, threadId);
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("A subscription has been started for this channel."));
                }
                else
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("This User is already registered, another subscription may not be started."));
                }
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You are not allowed to use this command.").AsEphemeral());
            }
        }
    }
}
