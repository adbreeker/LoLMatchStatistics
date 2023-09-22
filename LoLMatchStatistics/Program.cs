using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading;

namespace LoLScraper
{
    class Program
    {
        static string summonerName;

        static async Task Main(string[] args)
        {
            try
            {
                summonerName = File.ReadAllText("./summonerName.txt");
            }
            catch(IOException e)
            {
                Console.WriteLine("No summoner name file! Write summoner name:");
                summonerName = Console.ReadLine();
                File.WriteAllText("./summonerName.txt", summonerName);
            }

            List<Match> matchesHistory = await FetchMatchHistory(summonerName);

            if(matchesHistory == null || matchesHistory.Count == 0)
            {
                Environment.Exit(1);
            }

            MatchStatistics choosenMatch;

            if(matchesHistory.Count == 1) 
            {
                MatchStatistics ms = new MatchStatistics(matchesHistory[0], summonerName);
                choosenMatch = ms;
            }
            else
            {
                for(int i = 0; i<matchesHistory.Count; i++) 
                {
                    MatchStatistics ms = new MatchStatistics(matchesHistory[i], summonerName);
                    Console.Write(i+1 + ". ");
                    ms.WriteHistoryEntry();
                }

                while(true)
                {
                    Console.WriteLine("\n\nChoose game from history");
                    int choosenIndex = Convert.ToInt32(Console.ReadLine());
                    if(choosenIndex > 0 && choosenIndex <= matchesHistory.Count)
                    {
                        choosenMatch = new MatchStatistics(matchesHistory[choosenIndex - 1], summonerName);
                        break;
                    }
                }
            }
            Console.Clear();
            choosenMatch.WriteBaseStatistic();

            Console.WriteLine("Click any button to copy statistics to clipboard");
            Console.ReadKey();
            choosenMatch.CopyBaseStats();
            Console.WriteLine("Statistics copied! Good bye");
            Thread.Sleep(1000);
        }

        static async Task<List<Match>> FetchMatchHistory(string summonerName)
        {
            string apiKey;
            try
            {
                apiKey = File.ReadAllText("./apiKey.txt");
            }
            catch(IOException e)
            {
                Console.WriteLine("No key file! Write api key:");
                apiKey = Console.ReadLine();
                File.WriteAllText("./apiKey.txt", apiKey);
            }

            Console.WriteLine("How many games to get?");
            int howManyMatches = Convert.ToInt32(Console.ReadLine());
            Console.Clear();

            List<Match> matchHistory = new List<Match>();

            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("X-Riot-Token", apiKey);

                HttpResponseMessage summonerResponse = await httpClient.GetAsync($"https://eun1.api.riotgames.com/lol/summoner/v4/summoners/by-name/{summonerName}");
                summonerResponse.EnsureSuccessStatusCode();

                string summonerDataString = await summonerResponse.Content.ReadAsStringAsync();
                var summonerData = JsonConvert.DeserializeObject<Summoner>(summonerDataString);

                HttpResponseMessage matchesIdResponse = await httpClient.GetAsync($"https://europe.api.riotgames.com/lol/match/v5/matches/by-puuid/{summonerData.Puuid}/ids");
                matchesIdResponse.EnsureSuccessStatusCode();

                string matchesIdString = await matchesIdResponse.Content.ReadAsStringAsync();
                var matchesIdlist = JsonConvert.DeserializeObject<List<string>>(matchesIdString);

                for (int i = 0; i < howManyMatches; i++)
                {
                    HttpResponseMessage matchResponse = await httpClient.GetAsync($"https://europe.api.riotgames.com/lol/match/v5/matches/{matchesIdlist[i]}");
                    matchResponse.EnsureSuccessStatusCode();

                    string matchString = await matchResponse.Content.ReadAsStringAsync();
                    var matchInfo = JsonConvert.DeserializeObject<Match>(matchString);

                    matchHistory.Add(matchInfo);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return matchHistory;
        }
    }

}
