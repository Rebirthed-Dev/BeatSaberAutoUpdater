# BeatSaberAutoUpdater
A Discord Bot built in C# that queries ScoreSaber, a community-made website for Beat Saber scores, and provides updates to a player's rankings in a Discord channel.

# Dependencies
Programmed in C# .NET Core 3.1

Packages used:
DSharpPlus ver. 4.2.0
DSharpPlus.SlashCommands ver. 4.2.0
Microsoft.Data.Sqlite ver. 5.0.12

# Usage
There are placeholder values in "Config.json" that need to be filled before the bot will function correctly.
Set "token" to your Discord Bot's token. "testingguildid" is unused, but it is suggested that it can be used for testing changes to commands, as it is easier to sync commands to a single guild instead of globally.

In a channel or thread, /notifsub <ScoresaberID> can be used to begin a subscription to a Scoresaber profile. The bot will periodically check for new plays, and if found, post them.
Every 24 hours since the bot's launch, it will post a summary for the profile.

The bot should have the "Send Messages" permission in the channel that it is being used in, otherwise errors may occur.

The bot's database is stored in resources/data.db
The database is using SQLite.