using System;
using System.Collections.Generic;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace LoLMatchStatistics
{
    internal class GoogleSApi
    {
        string spreadsheetId = ""; //here write your spreadsheet id

        SheetsService sheetsService;

        public GoogleSApi() 
        {
            string credentialsPath = "./spreadsheetCredentials.json"; //Google credentials
            if(spreadsheetId == "")
            {
                try
                {
                    spreadsheetId = File.ReadAllText("./spreadsheetId.txt");
                }
                catch (IOException e)
                {
                    Console.WriteLine("No spreadsheet id file! Write spreadsheet id:");
                    spreadsheetId = Console.ReadLine();
                    File.WriteAllText("./spreadsheetId.txt", spreadsheetId);
                    Console.Clear();
                }
            }
            sheetsService = InitializeSheetsService(credentialsPath); //initialize google api service
        }

        public List<IList<object>> GetSpreasheet(string range) //fetch data from spreadsheet
        {
            IList<IList<object>> data = GetDataFromSheet(sheetsService, spreadsheetId, range);

            List<IList<object>> spreadsheetData = new List<IList<object>>();

            //populate data with empty strings
            for (int i = 0; i < 200; i++)
            {
                IList<object> row = new List<object>();
                for (int j = 0; j < 200; j++)
                {
                    row.Add(string.Empty);
                }
                spreadsheetData.Add(row);
            }

            //keep existing data
            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data[i].Count; j++)
                {
                    spreadsheetData[i][j] = data[i][j];
                }
            }

            return spreadsheetData;
        }

        public void UpdateSpreadsheet(List<IList<object>> newSpreadsheetData, string range) //update data in google spreadsheet
        {
            ValueRange valueRange = new ValueRange
            {
                Values = newSpreadsheetData
            };

            SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest =
                sheetsService.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            UpdateValuesResponse updateResponse = updateRequest.Execute();
        }

        private static SheetsService InitializeSheetsService(string credentialsPath)
        {
            GoogleCredential credential;

            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(SheetsService.Scope.Spreadsheets);
            }

            // Create the Google Sheets API service
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "LoLMatchStatistics",
            });

            return service;
        }

        private static IList<IList<object>> GetDataFromSheet(SheetsService service, string spreadsheetId, string range)
        {
            SpreadsheetsResource.ValuesResource.GetRequest request =
                service.Spreadsheets.Values.Get(spreadsheetId, range);

            ValueRange response = request.Execute();
            IList<IList<object>> values = response.Values;

            return values;
        }
    }
}
