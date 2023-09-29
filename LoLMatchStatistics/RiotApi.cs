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
        static string permApiKey = ""; //here write your riot api key

        public static async Task<List<RiotApiMatch>> FetchMatchHistoryFromSummoner(string summonerName)
        {
            string apiKey;
            if (permApiKey != "")
            {
                apiKey = permApiKey;
            }
            else
            {
                try
                {
                    apiKey = File.ReadAllText("./apiKey.txt");
                }
                catch (IOException e)
                {
                    Console.WriteLine("No key file! Write api key:");
                    apiKey = Console.ReadLine();
                    File.WriteAllText("./apiKey.txt", apiKey);
                }
            }


            Console.Clear();
            Console.WriteLine("How many games to get?");
            int howManyMatches = Convert.ToInt32(Console.ReadLine());

            if (howManyMatches > 20)
            {
                howManyMatches = 20;
                Console.WriteLine("Maximum is 20");
            }

            List<RiotApiMatch> matchHistory = new List<RiotApiMatch>();

            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("X-Riot-Token", apiKey);

                HttpResponseMessage summonerResponse = await httpClient.GetAsync($"https://eun1.api.riotgames.com/lol/summoner/v4/summoners/by-name/{summonerName}");
                summonerResponse.EnsureSuccessStatusCode();

                string summonerDataString = await summonerResponse.Content.ReadAsStringAsync();
                var summonerData = JsonConvert.DeserializeObject<RiotApiSummoner>(summonerDataString);

                HttpResponseMessage matchesIdResponse = await httpClient.GetAsync($"https://europe.api.riotgames.com/lol/match/v5/matches/by-puuid/{summonerData.Puuid}/ids");
                matchesIdResponse.EnsureSuccessStatusCode();

                string matchesIdString = await matchesIdResponse.Content.ReadAsStringAsync();
                var matchesIdlist = JsonConvert.DeserializeObject<List<string>>(matchesIdString);

                for (int i = 0; i < howManyMatches; i++)
                {
                    HttpResponseMessage matchResponse = await httpClient.GetAsync($"https://europe.api.riotgames.com/lol/match/v5/matches/{matchesIdlist[i]}");
                    matchResponse.EnsureSuccessStatusCode();

                    string matchString = await matchResponse.Content.ReadAsStringAsync();
                    var matchInfo = JsonConvert.DeserializeObject<RiotApiMatch>(matchString);

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

        public static async Task<RiotApiMatch> FetchMatchFromId(string matchId)
        {
            string apiKey;
            if (permApiKey != "")
            {
                apiKey = permApiKey;
            }
            else
            {
                try
                {
                    apiKey = File.ReadAllText("./apiKey.txt");
                }
                catch (IOException e)
                {
                    Console.WriteLine("No key file! Write api key:");
                    apiKey = Console.ReadLine();
                    File.WriteAllText("./apiKey.txt", apiKey);
                }
            }

            RiotApiMatch matchFromId = new RiotApiMatch();

            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("X-Riot-Token", apiKey);

                HttpResponseMessage matchResponse = await httpClient.GetAsync($"https://europe.api.riotgames.com/lol/match/v5/matches/{matchId}");
                matchResponse.EnsureSuccessStatusCode();

                string matchString = await matchResponse.Content.ReadAsStringAsync();
                var matchInfo = JsonConvert.DeserializeObject<RiotApiMatch>(matchString);

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
