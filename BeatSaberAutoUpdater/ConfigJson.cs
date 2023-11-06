using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeatSaberAutoUpdater
{
    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("testingguildid")]
        public ulong TestGuildID { get; private set; }
    }
}
