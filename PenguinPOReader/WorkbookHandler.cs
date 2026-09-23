using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Spire.Pdf.Security;
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
        private async Task<bool> UpdateRange(List<IList<object>> data, string cellRange)
        {
            try
            {
                var valueRange = new ValueRange();
                valueRange.Values = data;
                var updateRequest = Sheets.Spreadsheets.Values.Update(valueRange, Configurator.SheetId, cellRange);
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                var appendResponse = updateRequest.Execute();
                return appendResponse.UpdatedCells > 0;
            }
            catch (Exception e)
            {
                Logger.ErrorExit(["Failed to write to spreadsheet.", e.Message], 400);
                return false;
            }
        }
        private void AddRow(List<IList<object>>[] data)
        {
            if (UpdateRange(data[0], String.Format("Jobs!A{0}:N{0}", GetNewRow("Jobs").Result)).Result)
            {
                if (UpdateRange(data[1], String.Format("Reporting!A{0}:O{0}", GetNewRow("Reporting").Result)).Result)
                    Logger.Display("Update Successful", false);
                else Logger.Display("Failed to update reporting tab.", false);
            }
            else Logger.Display("Failed to update sheet.", false);
        }
        private void UpdateRow(List<IList<object>>[] data, string reportingRow, string jobRow)
        {
            if (UpdateRange(data[0], String.Format("Jobs!D{0}:G{0}", jobRow)).Result &&
                UpdateRange(data[1], String.Format("Jobs!J{0}:K{0}", jobRow)).Result)
            {
                if (UpdateRange(data[2], String.Format("Reporting!A{0}:L{0}", reportingRow)).Result &&
                    UpdateRange(data[3], String.Format("Reporting!O{0}:O{0}", reportingRow)).Result)
                    Logger.WriteLog("Update Successful", false);
                else Logger.WriteLog("Failed to update reporting tab.", false);
            }
            else Logger.Display("Failed to update sheet.", false);
        }
        private async Task<string> GetReportingRow(string po)
        {
            try
            {
                SpreadsheetsResource.ValuesResource.GetRequest request = Sheets.Spreadsheets.Values.Get(Configurator.SheetId, $"Reporting!A:A");
                ValueRange response = await request.ExecuteAsync();
                IList<IList<object>> values = response.Values;
                int matchedRow = -1;
                if (values != null & values.Count > 0)
                {
                    var columnValues = values.Select(row => row.FirstOrDefault() ?? "").ToArray();
                    for (int i = 0; i < columnValues.Length; i++) if ((string)columnValues[i] == po) matchedRow = i;
                }
                if (matchedRow >= 0)
                {
                    Logger.WriteLog("Found match for PO: {0}", false, po);
                    return (matchedRow + 1).ToString();
                }
                Logger.WriteLog("Did not find match for PO: {0}", false, po);
                return null;
            }
            catch
            {
                Logger.WriteLog("Failed to read sheet.", false);
                return null;
            }
        }
        private async Task<string> GetJobRow(string po)
        {
            try
            {
                SpreadsheetsResource.ValuesResource.GetRequest request = Sheets.Spreadsheets.Values.Get(Configurator.SheetId, $"Jobs!B:B");
                ValueRange response = await request.ExecuteAsync();
                IList<IList<object>> values = response.Values;
                int matchedRow = -1;
                if (values != null & values.Count > 0)
                {
                    var columnValues = values.Select(row => row.FirstOrDefault() ?? "").ToArray();
                    for (int i = 0; i < columnValues.Length; i++) if ((string)columnValues[i] == po) matchedRow = i;
                }
                if (matchedRow >= 0) return (matchedRow + 1).ToString();
                Logger.WriteLog("Failed to find match for PO: {0}. Please check sheet.", false, po);
                return null;
            }
            catch
            {
                Logger.WriteLog("Failed to connect to workbook. Please update manually.", false);
                return null;
            }

        }
        private async Task<string> GetNewRow(string sheetName)
        {
            try
            {
                SpreadsheetsResource.ValuesResource.GetRequest request = Sheets.Spreadsheets.Values.Get(Configurator.SheetId, $"{sheetName}!A:A");
                ValueRange response = request.ExecuteAsync().Result;
                IList<IList<object>> values = response.Values;
                if (values != null & values.Count > 0) return (values.Count + 1).ToString();
                else return String.Empty;
            }
            catch
            {
                return String.Empty;
            }
        }
        public void UpdateWorkbook(PdfReader reader)
        {
            try
            {
                string reportingRow = GetReportingRow(reader.PO).Result;
                if (reportingRow != null) UpdateRow(reader.UpdateRowData, reportingRow, GetJobRow(reader.PO).Result);
                else AddRow(reader.AddRowData);
            }
            catch
            {
                Logger.ErrorExit(["Failed to update spreadsheet!"], 130);
            }
        }
    }
}
