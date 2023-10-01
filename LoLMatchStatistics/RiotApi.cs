using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace LoLMatchStatistics
{
    internal class RiotApi
    {
        public static async Task<List<RiotApi_Match>> FetchMatchHistoryFromSummoner(string summonerName)
        {

            string apiKey = ConfigFilesManager.GetRiotApiKey();

            Console.Clear();
            Console.WriteLine("How many games to get?");
            int howManyMatches = Convert.ToInt32(Console.ReadLine());

            if (howManyMatches > 20)
            {
                howManyMatches = 20;
                Console.WriteLine("Maximum is 20");
            }

            List<RiotApi_Match> matchHistory = new List<RiotApi_Match>();

            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("X-Riot-Token", apiKey);

                HttpResponseMessage summonerResponse = await httpClient.GetAsync($"https://eun1.api.riotgames.com/lol/summoner/v4/summoners/by-name/{summonerName}");
                summonerResponse.EnsureSuccessStatusCode();

                string summonerDataString = await summonerResponse.Content.ReadAsStringAsync();
                var summonerData = JsonConvert.DeserializeObject<RiotApi_Summoner>(summonerDataString);

                HttpResponseMessage matchesIdResponse = await httpClient.GetAsync($"https://europe.api.riotgames.com/lol/match/v5/matches/by-puuid/{summonerData.Puuid}/ids");
                matchesIdResponse.EnsureSuccessStatusCode();

                string matchesIdString = await matchesIdResponse.Content.ReadAsStringAsync();
                var matchesIdlist = JsonConvert.DeserializeObject<List<string>>(matchesIdString);

                for (int i = 0; i < howManyMatches; i++)
                {
                    HttpResponseMessage matchResponse = await httpClient.GetAsync($"https://europe.api.riotgames.com/lol/match/v5/matches/{matchesIdlist[i]}");
                    matchResponse.EnsureSuccessStatusCode();

                    string matchString = await matchResponse.Content.ReadAsStringAsync();
                    var matchInfo = JsonConvert.DeserializeObject<RiotApi_Match>(matchString);

                    matchHistory.Add(matchInfo);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.ReadKey();
            }
            return matchHistory;
        }

        public static async Task<RiotApi_Match> FetchMatchFromId(string matchId)
        {
            string apiKey = ConfigFilesManager.GetRiotApiKey();

            RiotApi_Match matchFromId = new RiotApi_Match();

            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("X-Riot-Token", apiKey);

                HttpResponseMessage matchResponse = await httpClient.GetAsync($"https://europe.api.riotgames.com/lol/match/v5/matches/{matchId}");
                matchResponse.EnsureSuccessStatusCode();

                string matchString = await matchResponse.Content.ReadAsStringAsync();
                var matchInfo = JsonConvert.DeserializeObject<RiotApi_Match>(matchString);

                matchFromId = matchInfo;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.ReadKey();
            }
            return matchFromId;
        }
    }
}
