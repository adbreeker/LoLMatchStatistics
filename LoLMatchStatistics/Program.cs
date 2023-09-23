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
            summonerName = GetCurrentSummonerName();

            List<Match> matchesHistory = await FetchMatchHistory(summonerName);

            MatchStatistics chosenMatch = ChooseMatchFromHistory(matchesHistory);

            ManageChosenMatch(chosenMatch);
            
            Thread.Sleep(1000);
        }

        static string GetCurrentSummonerName()
        {
            string sn;
            try
            {
                sn = File.ReadAllText("./summonerName.txt");
            }
            catch (IOException e)
            {
                Console.WriteLine("No summoner name file! Write summoner name:");
                sn = Console.ReadLine();
                File.WriteAllText("./summonerName.txt", summonerName);
            }
            return sn;
        }

        static MatchStatistics ChooseMatchFromHistory(List<Match> matchesHistory)
        {
            Console.Clear();

            if (matchesHistory == null || matchesHistory.Count == 0)
            {
                Environment.Exit(1);
            }

            MatchStatistics chosenMatch;

            if (matchesHistory.Count == 1)
            {
                MatchStatistics ms = new MatchStatistics(matchesHistory[0], summonerName);
                chosenMatch = ms;
            }
            else
            {
                for (int i = 0; i < matchesHistory.Count; i++)
                {
                    MatchStatistics ms = new MatchStatistics(matchesHistory[i], summonerName);
                    Console.Write(i + 1 + ". ");
                    ms.WriteHistoryEntry();
                }

                while (true)
                {
                    Console.WriteLine("\n\nChoose game from history:");
                    int chosenIndex = Convert.ToInt32(Console.ReadLine());
                    if (chosenIndex > 0 && chosenIndex <= matchesHistory.Count)
                    {
                        chosenMatch = new MatchStatistics(matchesHistory[chosenIndex - 1], summonerName);
                        break;
                    }
                }
            }

            return chosenMatch;
            
        }

        static void ManageChosenMatch(MatchStatistics chosenMatch)
        {
            Console.Clear();
            chosenMatch.WriteBaseStatistic();

            Console.WriteLine("Click any button to copy statistics to clipboard");
            Console.ReadKey();
            chosenMatch.CopyBaseStats();
            Console.WriteLine("\nStatistics copied! Good bye");
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

            if(howManyMatches > 20)
            {
                howManyMatches = 20;
                Console.WriteLine("Maximum is 20");
            }

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
                Console.ReadKey();
            }
            return matchHistory;
        }
    }

}
