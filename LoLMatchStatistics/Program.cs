using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace LoLMatchStatistics
{
    class Program
    {
        
        static List<string> summonersNames = new List<string>();
        static string matchId;

        static async Task Main(string[] args)
        {

            if(IsManualSearch()) //choosing app mode - manual
            {
                Console.Clear();
                Console.WriteLine("Write summoner name to search:");
                summonersNames.Add(Console.ReadLine());
                List<RiotApiMatch> matchesHistory = await RiotApi.FetchMatchHistoryFromSummoner(summonersNames[0]);
                MatchStatistics chosenMatch = ChooseMatchFromHistory(matchesHistory);
                ManageChosenMatch(chosenMatch);
            }
            else //choosing app mode - by id
            {
                summonersNames = GetSummonersNamesFromFile();
                RiotApiMatch matchFromId = await RiotApi.FetchMatchFromId(matchId);
                if(summonersNames.Count == 1) //if only one summoner then display stats and enable copying
                {
                    MatchStatistics matchStatistics = new MatchStatistics(matchFromId, summonersNames[0]);
                    ManageChosenMatch(matchStatistics);
                }
                else //if more than one summoner then display stats and send them to google spreadsheet
                {
                    Console.Clear();
                    GoogleSApi googleSApi = new GoogleSApi();
                    List<IList<Object>> spreadsheet = googleSApi.GetSpreasheet("A1:GZ200");

                    foreach (string summonerName in summonersNames) 
                    {
                        MatchStatistics matchStatistics = new MatchStatistics(matchFromId, summonerName);
                        matchStatistics.WriteBaseStatistic();
                        if(matchStatistics.playerFoundInGame)
                        {
                            spreadsheet = FillSpreadsheetWithStatistics(spreadsheet, matchStatistics);
                        }
                    }
                    googleSApi.UpdateSpreadsheet(spreadsheet, "A1:GZ200");
                    Console.ReadKey();
                }
                
            }

            Thread.Sleep(1000);
        }

        static bool IsManualSearch() //choose app mode
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


        static MatchStatistics ChooseMatchFromHistory(List<RiotApiMatch> matchesHistory)
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

        static void ManageChosenMatch(MatchStatistics chosenMatch) //write stats od match and copy them after any key
        {
            Console.Clear();
            chosenMatch.WriteBaseStatistic();

            Console.WriteLine("Click any button to copy statistics to clipboard");
            Console.ReadKey();
            chosenMatch.CopyBaseStats();
            Console.WriteLine("\nStatistics copied! Good bye");
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

        static Tuple<int, int> GetStartingFillingPosition(List<IList<Object>> spreadsheet, string summonerName, string championName) //get starting pos for filling spreadsheet with statistics
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
                    if (spreadsheet[row][col].ToString().ToLower() == championName.ToLower())
                    {
                        championRow = row;
                    }
                }
            }

            return new Tuple<int,int>(championRow, summonerCol);
        }

        static List<IList<Object>> FillSpreadsheetWithStatistics(List<IList<Object>> spreadsheet, MatchStatistics statistics) //fill spreadsheet list with statistics
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
