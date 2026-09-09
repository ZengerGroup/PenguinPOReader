using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Spire.Pdf;
using Spire.Pdf.Texts;

namespace PenguinPOReader
{
    internal class PdfReader
    {
        PdfDocument Document;
        public string PO;
        public string Date;
        public string Quantity;
        public string ISBN;
        public string Title;
        public string Color;
        public string Stock;
        public string Coat;
        public string Binder;
        public string Status;
        public bool HardCover;
        public PdfReader(string path)
        {
            Document = new PdfDocument();
            Document.LoadFromFile(path);
            string text = "";
            Status = "New";
            for (int i = 0; i < Document.Pages.Count; i++)
            {
                PdfPageBase page = Document.Pages[i];
                PdfTextExtractor TextExtractor = new PdfTextExtractor(page);
                PdfTextExtractOptions TEOptions = new PdfTextExtractOptions();
                TEOptions.IsExtractAllText = true;
                text += TextExtractor.ExtractText(TEOptions);
            }
            GetData(text);
            Document.Close();
            //Logem();
        }
        private void GetData(string pdfText)
        {
            Title = Regex.Match(pdfText, @"(?<=Title - Short:)(.*?)(?=\s{2,})").Value.Trim();
            PO = Regex.Match(pdfText, @"(?<=\s+)\d*(?=/\d{2}/\d{2}/\d{4}\s{2,})").Value.Trim();
            Date = Regex.Match(pdfText, @"(?<=Delivery date\s*)\d{2}/\d{2}/\d{4}(?=\s{2,})").Value.Trim();
            Quantity = Regex.Match(pdfText, @"^*[0-9]*?\,*?[0-9]{1,3} Each").Value.Trim().Split(" ")[0];
            ISBN = Regex.Match(pdfText, @"(\s){3,}ISBN: [0-9]*").Value.Split(":")[1].Trim();
            Color = GetColor(pdfText);
            Stock = Regex.Match(pdfText, @"(?<=Vendor Suppl Cv/Jk Stock:\s*)(.*?)(?=\s{2,})").Value.Trim();
            Coat = Regex.Match(pdfText, @"(?<=Coat 1:)(.*?)(?=\s{2,})").Value.Trim();
            Binder = Regex.Match(pdfText, @"(?<=Binder:\s*)(.*?)(?=\s{2,})").Value.Trim();
            HardCover = Regex.Match(pdfText, @"(?<=Format:\s+)(\b[\w\s]+\b)(?=\s{2,})").Value.Contains("Hardcover");
        }
        private string GetColor(string pdfText)
        {
            string colorBase = Regex.Match(pdfText, @"(?<=Cvr/Jkt Colors:\s*)(.*?)(?=\s{2,})").Value.Trim();
            string colorDesc = Regex.Match(pdfText, @"(?<= Colors Desc:\s*)(.*?)(?=\s{2,})").Value.Trim();
            return String.Format("{0} | {1}", colorBase, colorDesc);
        }
        private void Logem()
        {
            Console.WriteLine(PO);
            Console.WriteLine(Date);
            Console.WriteLine(Quantity);
            Console.WriteLine(ISBN);
            Console.WriteLine(Title);
            Console.WriteLine(Color);
            Console.WriteLine(Stock);
            Console.WriteLine(Coat);
            Console.WriteLine(Binder);
            Console.WriteLine(Status);
            Console.WriteLine(HardCover);
        }
    }
}
