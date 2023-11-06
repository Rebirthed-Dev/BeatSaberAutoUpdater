using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberAutoUpdater
{
    //userID integer PRIMARY KEY,
    //previouslyCheckedScore date,
	//oldPP decimal DEFAULT 0.0,
	//oldGlobalRank integer DEFAULT 0,
	//oldCountryRank integer DEFAULT 0,
	//oldAverageAccuracy decimal DEFAULT 0.0,
	//channelToPostIn integer,
    //threadToPostIn integer
    public class SqliteDatabaseInterface
    {
        public void Initialize_Database()
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = "resources/data.db" };
            var connectionString = connectionStringBuilder.ToString();

            Console.WriteLine(connectionString);

            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();
            var build_file = string.Empty;

            using (var fs = File.OpenRead("resources/build.sql"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                build_file = sr.ReadToEnd();

            var startup_command = connection.CreateCommand();
            startup_command.CommandText = build_file;

            startup_command.ExecuteNonQuery();
            connection.Close();
        }

        public async Task<List<long>> Get_Subscribed_UserIDs()
        {
            List<long> listOfIds = new List<long>();

            SqliteConnection connection = new SqliteConnection("Data Source = resources/data.db");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
                @"SELECT UserID
                FROM UserData
                ";
            var reader = command.ExecuteReader();

            bool result = reader.HasRows;

            if (result == true)
            {
                while (reader.Read())
                {
                    long userID = reader.GetInt64(0);
                    listOfIds.Add(userID);
                }
            }

            await connection.CloseAsync();

            return listOfIds;
        }

        public async Task<List<(float, int, int, float)>> Get_Previous_Daily_Stats(long user_id)
        {
            //oldPP decimal DEFAULT 0.0,
            //oldGlobalRank integer DEFAULT 0,
            //oldCountryRank integer DEFAULT 0,
            //oldAverageAccuracy decimal DEFAULT 0.0,
            SqliteConnection connection = new SqliteConnection("Data Source = resources/data.db");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
                @"SELECT oldPP, oldGlobalRank, oldCountryRank, oldAverageAccuracy
                FROM UserData
                WHERE UserID = $UserID
                ";
            command.Parameters.AddWithValue("$UserID", user_id);

            var reader = command.ExecuteReader();

            var result = reader.HasRows;
            float oldPP = 0;
            int oldGlobalRank = 0;
            int oldCountryRank = 0;
            float oldAverageAccuracy = 0;

            if (result == true)
            {
                while (reader.Read())
                {
                    oldPP = reader.GetFloat(0);
                    oldGlobalRank = reader.GetInt32(1);
                    oldCountryRank = reader.GetInt32(2);
                    oldAverageAccuracy = reader.GetFloat(3);
                }
            }

            List<(float, int, int, float)> output = new List<(float, int, int, float)>
            {
                (oldPP, oldGlobalRank, oldCountryRank, oldAverageAccuracy)
            };

            await connection.CloseAsync();

            return output;
        }
        public async Task Remove_Subscription(long user_id)
        {
            SqliteConnection connection = new SqliteConnection("Data Source = resources/data.db");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
                @"DELETE FROM UserData
                WHERE UserID = $UserID
                ";
            command.Parameters.AddWithValue("$UserID", user_id);

            command.ExecuteNonQuery();
            await connection.CloseAsync();
        }

        public async Task Create_Subscription(long user_id, ulong channel_id, ulong thread_id)
        {
            SqliteConnection connection = new SqliteConnection("Data Source = resources/data.db");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
                @"INSERT INTO UserData (UserID, channelToPostIn, threadToPostIn, previouslyCheckedScore)
                VALUES ($UserID, $channelToPostIn, $threadToPostIn, $previouslyCheckedScore)
                ";
            command.Parameters.AddWithValue("$UserID", user_id);
            command.Parameters.AddWithValue("$channelToPostIn", channel_id);
            command.Parameters.AddWithValue("$threadToPostIn", thread_id);
            command.Parameters.AddWithValue("$previouslyCheckedScore", 0);

            command.ExecuteNonQuery();
            await connection.CloseAsync();
        }

        public async Task<long> Get_Previous_Score_Time(long user_id)
        {
            SqliteConnection connection = new SqliteConnection("Data Source = resources/data.db");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
                @"SELECT previouslyCheckedScore
                FROM UserData
                WHERE UserID = $UserID
                ";
            command.Parameters.AddWithValue("$UserID", user_id);

            var reader = command.ExecuteReader();

            bool result = reader.HasRows;
            long output = 0;

            if (result == true)
            {
                while (reader.Read())
                {
                    output = reader.GetInt64(0);
                }
            }

            await connection.CloseAsync();

            return output;
        }

        public async Task Set_Score_Time(long user_id, DateTimeOffset score_time)
        {
            SqliteConnection connection = new SqliteConnection("Data Source = resources/data.db");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
                @"UPDATE UserData
                SET previouslyCheckedScore = $scoreTime
                WHERE UserID = $UserID
                ";
            command.Parameters.AddWithValue("$scoreTime", score_time.ToUnixTimeSeconds());
            command.Parameters.AddWithValue("$UserID", user_id);

            command.ExecuteNonQuery();

            await connection.CloseAsync();
        }

        public async Task Set_Daily_Stats(long user_id, float PP, int GlobalRank, int CountryRank, float AverageAccuracy)
        {
            //oldPP decimal DEFAULT 0.0,
            //oldGlobalRank integer DEFAULT 0,
            //oldCountryRank integer DEFAULT 0,
            //oldAverageAccuracy decimal DEFAULT 0.0,
            SqliteConnection connection = new SqliteConnection("Data Source = resources/data.db");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
                @"UPDATE UserData
                SET oldPP = $PP, oldGlobalRank = $GlobalRank, oldCountryRank = $CountryRank, oldAverageAccuracy = $AverageAccuracy
                WHERE UserID = $UserID
                ";
            command.Parameters.AddWithValue("$PP", PP);
            command.Parameters.AddWithValue("$GlobalRank", GlobalRank);
            command.Parameters.AddWithValue("$CountryRank", CountryRank);
            command.Parameters.AddWithValue("$AverageAccuracy", AverageAccuracy);
            command.Parameters.AddWithValue("$UserID", user_id);

            command.ExecuteNonQuery();

            await connection.CloseAsync();
        }

        public async Task<List<(long, long)>> Get_Channel_Ids(long user_id)
        {
            List<(long, long)> listOfIds = new List<(long, long)>();

            SqliteConnection connection = new SqliteConnection("Data Source = resources/data.db");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
                @"SELECT channelToPostIn, threadToPostIn
                FROM UserData
                WHERE UserID = $UserID
                ";
            command.Parameters.AddWithValue("$UserID", user_id);
            var reader = command.ExecuteReader();

            bool result = reader.HasRows;

            if (result == true)
            {
                while (reader.Read())
                {
                    long channelID = reader.GetInt64(0);
                    long threadID = reader.GetInt64(1);
                    listOfIds.Add((channelID, threadID));
                }
            }

            await connection.CloseAsync();

            return listOfIds;
        }
    }
}
