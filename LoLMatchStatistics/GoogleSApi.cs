using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace LoLMatchStatistics
{
    internal class GoogleSApi
    {
        
        const string credentialsPath = "./spreadsheetCredentials.json"; // Your Google Sheets API credentials JSON file path
        string spreadsheetId = ""; // ID of the Google Sheets spreadsheet

        SheetsService sheetsService;

        public GoogleSApi() 
        {
            // Initialize the Google Sheets service
            sheetsService = InitializeSheetsService(credentialsPath);
        }

        public List<IList<object>> GetSpreasheet(string range)
        {

            // Define the range you want to retrieve data from (e.g., "Sheet1!A1:D10")

            // Fetch data from the specified range
            IList<IList<object>> data = GetDataFromSheet(sheetsService, spreadsheetId, range);

            // Store the data in a variable
            List<IList<object>> spreadsheetData = new List<IList<object>>();

            // Populate data with empty strings
            for (int i = 0; i < 200; i++)
            {
                IList<object> row = new List<object>();
                for (int j = 0; j < 200; j++)
                {
                    row.Add(string.Empty);
                }
                spreadsheetData.Add(row);
            }

            // Keep existing data
            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data[i].Count; j++)
                {
                    spreadsheetData[i][j] = data[i][j];
                }
            }

            return spreadsheetData;
        }

        public void UpdateSpreadsheet(List<IList<object>> newSpreadsheetData, string range)
        {
            // Tworzenie ValueRange
            ValueRange valueRange = new ValueRange
            {
                Values = newSpreadsheetData
            };

            // Aktualizacja danych w arkuszu
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
