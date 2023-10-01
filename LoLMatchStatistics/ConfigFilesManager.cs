using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoLMatchStatistics
{
    internal class ConfigFilesManager
    {
        const string credentialsPath = "./Config/spreadsheetCredentials.json"; //Google credentials
        const string apiKeyPath = "./Config/apiKey.txt"; //Riot api key
        const string matchByIdConfigPath = "./Config/matchByIdConfig.json"; // Match by id configuration file

        static MatchByIdConfig config = null;

        public static GoogleCredential GetGoogleCredential()
        {
            GoogleCredential credential;

            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(SheetsService.Scope.Spreadsheets);
            }

            return credential;
        }

        public static string GetRiotApiKey() 
        {
            string apiKey;
            try
            {
                apiKey = File.ReadAllText(apiKeyPath);
            }
            catch (IOException e)
            {
                Console.WriteLine("No key file! Write api key:");
                apiKey = Console.ReadLine();
                File.WriteAllText(apiKeyPath, apiKey);
            }
            return apiKey;
        }

        public static MatchByIdConfig GetMatchByIdConfig()
        {
            if(config != null)
            {
                return config;
            }

            MatchByIdConfig configJson;
            try
            {
                configJson = JsonConvert.DeserializeObject<MatchByIdConfig>(File.ReadAllText(matchByIdConfigPath));
            }
            catch (IOException e)
            {
                Console.WriteLine("No match by id config file! Create one now.\n");

                List<string> summonerNames = new List<string>();
                Console.WriteLine("How many summoners would you like to check?:");
                int howManySummoners = Convert.ToInt32(Console.ReadLine());
                for (int i = 0; i < howManySummoners; i++)
                {
                    Console.WriteLine($"{i + 1}.Write summoner name:");
                    summonerNames.Add(Console.ReadLine());
                }

                bool saveToSpreadsheet;
                while(true)
                {
                    Console.WriteLine("\n Do you want to save statistics to spreadsheet? [y/n]:");
                    if(Console.ReadLine() == "y")
                    {
                        saveToSpreadsheet = true;
                        break;
                    }
                    if(Console.ReadLine() == "n")
                    {
                        saveToSpreadsheet = false;
                        break;
                    }
                }

                string spreadsheetId = "";
                string spreadsheetRange = "";
                if(saveToSpreadsheet)
                {
                    Console.WriteLine("\n Write your spreadsheet id:");
                    spreadsheetId = Console.ReadLine();
                    Console.WriteLine("\n Write your spreadsheet range (e.g. A1:GZ200):");
                    spreadsheetRange = Console.ReadLine();
                }

                configJson = new MatchByIdConfig()
                {
                    SummonersNames = summonerNames,
                    SaveToSpreadsheet = saveToSpreadsheet,
                    SpreadsheetId = spreadsheetId,
                    SpreadsheetRange = spreadsheetRange
                };

                File.WriteAllText(matchByIdConfigPath, JsonConvert.SerializeObject(configJson));
            }

            config = configJson;
            return configJson;
        }
    }

    class MatchByIdConfig
    {
        public List<string> SummonersNames;
        public bool SaveToSpreadsheet;
        public string SpreadsheetId;
        public string SpreadsheetRange;
    }
}
