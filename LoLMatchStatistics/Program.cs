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
using LoLMatchStatistics;
using Google.Apis.Sheets.v4.Data;

namespace LoLScraper
{
    class Program
    {
        static List<string> summonersNames = new List<string>();
        static string matchId;

        static bool usePermApiKey = true;
        static string permApiKey = ""; //here write your riot api key

        static async Task Main(string[] args)
        {
            
            if(IsManualSearch())
            {
                Console.Clear();
                Console.WriteLine("Write summoner name to search:");
                summonersNames.Add(Console.ReadLine());
                List<Match> matchesHistory = await FetchMatchHistoryFromSummoner(summonersNames[0]);
                MatchStatistics chosenMatch = ChooseMatchFromHistory(matchesHistory);
                ManageChosenMatch(chosenMatch);
            }
            else
            {
                summonersNames = GetSummonersNamesFromFile();
                Match matchFromId = await FetchMatchFromId(matchId);
                if(summonersNames.Count == 1)
                {
                    MatchStatistics matchStatistics = new MatchStatistics(matchFromId, summonersNames[0]);
                    ManageChosenMatch(matchStatistics);
                }
                else
                {
                    Console.Clear();
                    GoogleSApi googleSApi = new GoogleSApi();
                    List<IList<Object>> spreadsheet = googleSApi.GetSpreasheet("A1:GZ200");

                    foreach (string summonerName in summonersNames) 
                    {
                        MatchStatistics matchStatistics = new MatchStatistics(matchFromId, summonerName);
                        matchStatistics.WriteBaseStatistic();
                        spreadsheet = FillSpreadsheetWithStatistics(spreadsheet, matchStatistics);
                    }
                    googleSApi.UpdateSpreadsheet(spreadsheet, "A1:GZ200");
                    Console.ReadLine();
                }
                
            }

            Thread.Sleep(1000);
        }

        static bool IsManualSearch()
        {
            Console.WriteLine("Program have 2 action options.\nBelow, enter the match id or the command \"manual\" to search for the match manually:");
            matchId = Console.ReadLine();
            if(matchId == "manual")
            {
                return true;
            }
            else
            {
                matchId = "EUN1_" + matchId;
                return false;
            }
        }

        //manual searching ------------------------------------------------------------------------------------------------------------------ manual searching

        

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
                MatchStatistics ms = new MatchStatistics(matchesHistory[0], summonersNames[0]);
                chosenMatch = ms;
            }
            else
            {
                for (int i = 0; i < matchesHistory.Count; i++)
                {
                    MatchStatistics ms = new MatchStatistics(matchesHistory[i], summonersNames[0]);
                    Console.Write(i + 1 + ". ");
                    ms.WriteHistoryEntry();
                }

                while (true)
                {
                    Console.WriteLine("\n\nChoose game from history:");
                    int chosenIndex = Convert.ToInt32(Console.ReadLine());
                    if (chosenIndex > 0 && chosenIndex <= matchesHistory.Count)
                    {
                        chosenMatch = new MatchStatistics(matchesHistory[chosenIndex - 1], summonersNames[0]);
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

        static async Task<List<Match>> FetchMatchHistoryFromSummoner(string summonerName)
        {
            string apiKey;
            if(usePermApiKey)
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

        //searching by game id ------------------------------------------------------------------------------------------------------------- searching by game id

        static async Task<Match> FetchMatchFromId(string matchId)
        {
            string apiKey;
            if (usePermApiKey)
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

            Match matchFromId = new Match();

            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("X-Riot-Token", apiKey);

                HttpResponseMessage matchResponse = await httpClient.GetAsync($"https://europe.api.riotgames.com/lol/match/v5/matches/{matchId}");
                matchResponse.EnsureSuccessStatusCode();

                string matchString = await matchResponse.Content.ReadAsStringAsync();
                var matchInfo = JsonConvert.DeserializeObject<Match>(matchString);

                matchFromId = matchInfo;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.ReadKey();
            }
            return matchFromId;
        }

        static List<string> GetSummonersNamesFromFile()
        {
            List<string> sn = new List<string>();
            try
            {
                sn = File.ReadAllLines("./summonersNames.txt").ToList();
            }
            catch (IOException e)
            {
                Console.WriteLine("No summoners names file! How many summoners would you like to check?:");
                int howManySummoners = int.Parse(Console.ReadLine());
                Console.Clear();
                for (int i = 0; i < howManySummoners; i++)
                {
                    Console.WriteLine($"{i + 1}.Write summoner name:");
                    sn.Add(Console.ReadLine());
                }
                File.WriteAllLines("./summonersNames.txt", sn);
            }
            return sn;
        }

        static Tuple<int, int> GetStartingFillingPosition(List<IList<Object>> spreadsheet, string summonerName, string championName)
        {
            int championRow = 0;
            int summonerCol = 0;

            for(int row = 0; row < spreadsheet.Count(); row++)
            {
                for(int col = 0; col < spreadsheet[row].Count(); col++)
                {
                    if (spreadsheet[row][col].ToString() == summonerName)
                    {
                        summonerCol = col;
                    }
                    if (spreadsheet[row][col].ToString() == championName)
                    {
                        championRow = row;
                    }
                }
            }

            return new Tuple<int,int>(championRow, summonerCol);
        }

        static List<IList<Object>> FillSpreadsheetWithStatistics(List<IList<Object>> spreadsheet, MatchStatistics statistics)
        {
            Tuple<int, int> startingPos = GetStartingFillingPosition(spreadsheet, statistics.GetMyParticipantInfo().SummonerName, statistics.GetMyParticipantInfo().ChampionName);
            if(startingPos.Item1 == 0 || startingPos.Item2 == 0)
            {
                return spreadsheet;
            }
            else
            {
                for(int i = 0; i < statistics.GetBaseStatisticsAsList().Count; i++) 
                {
                    spreadsheet[startingPos.Item1][startingPos.Item2+i] = statistics.GetBaseStatisticsAsList()[i];
                }
            }
            
            return spreadsheet;
        }
    }

}
