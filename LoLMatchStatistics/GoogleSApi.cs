using System;
using System.Collections.Generic;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Win32;

namespace LoLMatchStatistics
{
    internal class GoogleSApi
    {
        string spreadsheetId;

        SheetsService sheetsService;

        public GoogleSApi() 
        {
            spreadsheetId = ConfigFilesManager.GetMatchByIdConfig().SpreadsheetId;
            sheetsService = InitializeSheetsService(); //initialize google api service
        }

        public List<IList<object>> GetSpreasheet(string range) //fetch data from spreadsheet
        {
            IList<IList<object>> data = GetDataFromSheet(sheetsService, spreadsheetId, range);

            List<IList<object>> spreadsheetData = new List<IList<object>>();

            //populate data with empty strings
            for (int i = 0; i < ParseSpreadsheetRangeToInts().Item2; i++)
            {
                IList<object> row = new List<object>();
                for (int j = 0; j < ParseSpreadsheetRangeToInts().Item1; j++)
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
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            UpdateValuesResponse updateResponse = updateRequest.Execute();
        }

        private static SheetsService InitializeSheetsService()
        {
            

            // Create the Google Sheets API service
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = ConfigFilesManager.GetGoogleCredential(),
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

        Tuple<int,int> ParseSpreadsheetRangeToInts()
        {
            int cols = 0;
            int rows = 0;
            string range = ConfigFilesManager.GetMatchByIdConfig().SpreadsheetRange;
            string[] ranges = range.Split(':');

            //Calculating rows
            int[] rangeRows = new int[2];
            for(int i = 0; i<=1; i++)
            {
                foreach (char c in ranges[i])
                {
                    if (Char.IsDigit(c))
                    {
                        string[] pom = ranges[i].Split(new[] { c }, 2);
                        ranges[i] = pom[0];
                        rangeRows[i] = int.Parse(c.ToString() + pom[1]);
                        break;
                    }
                }
            }
            rows = Math.Abs(rangeRows[0] - rangeRows[1]) + 1;

            //Calculating columns
            int[] rangeCols = {0, 0};
            for (int i = 0; i<=1; i++)
            {
                for(int c = 0; c < ranges[i].Length ; c++)
                {
                    rangeCols[i] += (int)(ranges[i][ranges[i].Length - c - 1] - 64) * (int)Math.Pow(26, c); 
                }
            }
            cols = Math.Abs(rangeCols[0] - rangeCols[1]) + 1;

            return new Tuple<int, int>(cols, rows);
        }
    }
}
