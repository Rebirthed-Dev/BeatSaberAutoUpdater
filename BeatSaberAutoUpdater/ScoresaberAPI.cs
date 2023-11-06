using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace BeatSaberAutoUpdater
{
    public class ScoresaberAPI
    {
        public HttpClient client = new HttpClient();
        public async Task<ScoreList> getScores(long playerId)
        {
            ScoreList Scoreholder = new ScoreList();
            string parameters = $"player/{playerId}/scores?limit=1&sort=recent&page=1&withMetadata=false";

            HttpResponseMessage response = await client.GetAsync(parameters).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                string jsonString = await response.Content.ReadAsStringAsync();
                Scoreholder = JsonConvert.DeserializeObject<ScoreList>(jsonString);
            }

            return Scoreholder;
        }

        public async Task<PlayerJson> getRanking(long playerId)
        {
            PlayerJson playerJson = new PlayerJson();
            string parameters = $"player/{playerId}/full";

            HttpResponseMessage response = await client.GetAsync(parameters).ConfigureAwait(false);


            if (response.IsSuccessStatusCode)
            {
                string jsonString = await response.Content.ReadAsStringAsync();
                playerJson = JsonConvert.DeserializeObject<PlayerJson>(jsonString);
            }

            return playerJson;
        }

        public ScoresaberAPI()
        {
            client.BaseAddress = new Uri("https://www.scoresaber.com/api/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

    }
}
