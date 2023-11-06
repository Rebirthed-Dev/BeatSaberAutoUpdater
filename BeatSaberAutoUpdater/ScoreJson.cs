using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeatSaberAutoUpdater
{
    public struct ScoreJson
    {
        [JsonProperty("id")]
        public int id { get; private set; }

        [JsonProperty("rank")]
        public int rank { get; private set; }

        [JsonProperty("modifiedScore")]
        public int modifiedScore { get; private set; }

        [JsonProperty("pp")]
        public float pp { get; private set; }

        [JsonProperty("weight")]
        public float weight { get; private set; }

        [JsonProperty("missedNotes")]
        public int missedNotes { get; private set; }

        [JsonProperty("badCuts")]
        public int badCuts { get; private set; }

        [JsonProperty("maxCombo")]
        public int maxCombo { get; private set; }

        [JsonProperty("fullCombo")]
        public bool fullCombo { get; private set; }

        [JsonProperty("timeSet")]
        public DateTime timeSet { get; private set; }
    }

    public struct Difficulty
    {
        [JsonProperty("difficultyRaw")]
        public string difficultyRaw { get; private set; }
    }

    public struct Leaderboard
    {
        [JsonProperty("id")]
        public int songId { get; private set; }

        [JsonProperty("songName")]
        public string songName { get; private set; }

        [JsonProperty("songSubName")]
        public string songSubName { get; private set; }

        [JsonProperty("songAuthorName")]
        public string songAuthorName { get; private set; }

        [JsonProperty("levelAuthorName")]
        public string levelAuthorName { get; private set; }

        [JsonProperty("difficulty")]
        public Difficulty difficulty { get; private set; }

        [JsonProperty("ranked")]
        public bool ranked { get; private set; }

        [JsonProperty("stars")]
        public float stars { get; private set; }

        [JsonProperty("coverImage")]
        public string coverImage { get; private set; }
    }

    public struct ScoreHolder
    {
        [JsonProperty("score")]
        public ScoreJson score { get; private set; }

        [JsonProperty("leaderboard")]
        public Leaderboard leaderboard { get; private set; }
    }

    public struct ScoreList
    {
        [JsonProperty("playerScores")]
        public List<ScoreHolder> scorelist { get; private set; }
    }
}
