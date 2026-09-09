using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Text;

namespace PenguinPOReader
{
    internal class WorkbookHandler
    {
        ServiceAccountCredential serviceCredentials;
        GoogleCredential googleCredentials;
        GoogleCredential scopedCredentials;
        SheetsService Sheets;
        Spreadsheet Workbook;

        public WorkbookHandler()
        {
            serviceCredentials = CredentialFactory.FromJson<ServiceAccountCredential>(File.ReadAllText(Configurator.CredentialsPath));
            googleCredentials = GoogleCredential.FromServiceAccountCredential(serviceCredentials);
            scopedCredentials = googleCredentials.CreateScoped(Google.Apis.Sheets.v4.SheetsService.Scope.Spreadsheets);
        }
        public async Task<bool> VerifyConnection()
        {
            try
            {
                string token = await scopedCredentials.UnderlyingCredential.GetAccessTokenForRequestAsync();
                Logger.WriteLog("Token received!", false);
                return !string.IsNullOrEmpty(token);
            }
            catch (Exception e)
            {
                Logger.WriteLog(e.Message, false);
                return false; 
            }
        }
        public void SetupApi()
        {
            try
            {
                Sheets = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = scopedCredentials,
                    ApplicationName = "Penguin PO Tracker"
                });
                Workbook = Sheets.Spreadsheets.Get(Configurator.SheetId).Execute();
            }
            catch (Exception e)
            {
                Logger.ErrorExit([e.Message], 15);
            }
        }
        public void Update(PdfReader reader)
        {
            try
            {
                string sheetName = (reader.HardCover) ? "Jackets" : "Covers";
                string row = GetRow(sheetName).Result;
                if (row == String.Empty) Logger.ErrorExit(["Unable to determine last used row."], 14);
                var valueRange = new ValueRange();
                valueRange.Values = new List<IList<object>> { GetRowData(reader) };
                var updateRequest = Sheets.Spreadsheets.Values.Update(valueRange, Configurator.SheetId, String.Format("{0}!A{1}:L{1}", sheetName, row));
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                var appendResponse = updateRequest.Execute();
            }
            catch
            {
                Logger.ErrorExit(["Failed to update spreadsheet!"], 13);
            }
        }
        private async Task<string> GetRow(string sheetName)
        {
            try
            {
                SpreadsheetsResource.ValuesResource.GetRequest request = Sheets.Spreadsheets.Values.Get(Configurator.SheetId, $"{sheetName}!A:A");
                ValueRange response = await request.ExecuteAsync();
                IList<IList<object>> values = response.Values;
                if (values != null & values.Count > 0) return (values.Count + 1).ToString();
                else return String.Empty;
            }
            catch
            {
                return String.Empty;
            }
        }
        private List<object> GetRowData(PdfReader reader)
        {
            return new List<object>
            {
                DateTime.Now.ToString("MM/dd"), reader.PO, "", reader.Quantity, reader.ISBN, reader.Title, reader.Color, reader.Stock, reader.Coat, 
                reader.Binder, reader.Status, reader.Date
            };
        }
    }
}
