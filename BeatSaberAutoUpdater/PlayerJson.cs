using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeatSaberAutoUpdater
{
    public struct PlayerJson
    {
        [JsonProperty("pp")]
        public float pp { get; private set; }

        [JsonProperty("rank")]
        public int rank { get; private set; }

        [JsonProperty("countryRank")]
        public int countryRank { get; private set; }

        [JsonProperty("profilePicture")]
        public string profilePicture { get; private set; }

        [JsonProperty("name")]
        public string name { get; private set; }

        [JsonProperty("id")]
        public string id { get; private set; }

        [JsonProperty("country")]
        public string country { get; private set; }

        [JsonProperty("scoreStats")]
        public scoreStats scoreStats { get; private set; }
    }
    public struct scoreStats
    {
        [JsonProperty("averageRankedAccuracy")]
        public float averageRankedAccuracy { get; private set; }
    }
}
