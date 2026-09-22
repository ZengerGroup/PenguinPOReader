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
        public string Author;
        public string Color;
        public string Stock;
        public string Coat;
        public string Binder;
        public string Status;
        public string Buyer;
        public string Imprint;
        public string PrintNumber;
        public string PriceUS;
        public string PriceCAN;
        public bool HardCover;
        public List<IList<object>>[] AddRowData 
        {
            get 
            {
                return 
                    [
                    new List<IList<object>>{ new List<object> { DateTime.Now.ToString("MM/dd"), PO, "", Buyer, Quantity, ISBN, Title, Color, Stock, Coat,
                    Binder, Status, Date, Date }}, 
                    new List<IList<object>>{ new List<object> { PO, Title, Author, ISBN, Imprint, PrintNumber, Buyer, Quantity, 
                        PriceUS, PriceCAN, Binder, Configurator.CsrDefault, "", ""}}
                    ];
            }
        }
        public List<IList<object>>[] UpdateRowData
        {
            get
            {
                return
                    [
                    //B to 
                    new List<IList<object>>{ new List<object> { Buyer, Quantity, ISBN, Title }},
                    new List<IList<object>>{ new List<object> { Coat, Binder} },
                    new List<IList<object>>{ new List<object> { PO, Title, Author, ISBN, Imprint, PrintNumber, Buyer, Quantity, PriceUS, 
                        PriceCAN, Binder, Configurator.CsrDefault} }
                    ];
            }
        }
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
            Date = GetDeliveryDate(Regex.Match(pdfText, @"(?<=Component Del Date:\s*)(.*?)(?=\s{2,})").Value.Trim());
            Quantity = Regex.Match(pdfText, @"^*[0-9]*?\,*?[0-9]{1,3} Each").Value.Trim().Split(" ")[0];
            ISBN = Regex.Match(pdfText, @"(\s){3,}ISBN: [0-9]*").Value.Split(":")[1].Trim();
            Color = GetColor(pdfText);
            Stock = Regex.Match(pdfText, @"(?<=Vendor Suppl Cv/Jk Stock:\s*)(.*?)(?=\s{2,})").Value.Trim();
            if (Regex.Match(pdfText, @"(?<=Cvr with Flap:\s*)(.*?)(?=\s{2,})").Value.Trim() == "Yes") Stock = String.Format("{0}: {1}",
                Regex.Match(pdfText, @"(?<=Scored/Perf Flaps:\s*)(.*?)(?=\s{2,})").Value.Trim(), Stock);
            Buyer = Regex.Match(pdfText, @"(?<=Production Manager:\s*)(.*?)(?=\s{2,})").Value.Trim();
            Imprint = Regex.Match(pdfText, @"(?<=Imprint:\s*)(.*?)(?=\s{2,})").Value.Trim();
            Coat = Regex.Match(pdfText, @"(?<=Coat 1:)(.*?)(?=\s{2,})").Value.Trim();
            Binder = Regex.Match(pdfText, @"(?<=Binder:\s*)(.*?)(?=\s{2,})").Value.Trim();
            HardCover = Regex.Match(pdfText, @"(?<=Format:\s+)(\b[\w\s]+\b)(?=\s{2,})").Value.Contains("Hardcover");
            Author = Regex.Match(pdfText, @"(?<=Contrib1:\s*)(.*?)(?=\s{2,})").Value.Trim();
            PrintNumber = Regex.Match(pdfText, @"(?<=Printing Number:\s*)(.*?)(?=\s{2,})").Value.Trim();
            PriceUS = Regex.Match(pdfText, @"(?<=Retail Price USA:\s*)(.*?)(?=\s{2,})").Value.Trim();
            PriceCAN = Regex.Match(pdfText, @"(?<=Retail Price Can:\s*)(.*?)(?=\s{2,})").Value.Trim();
        }
        private string GetColor(string pdfText)
        {
            string colorBase = Regex.Match(pdfText, @"(?<=Cvr/Jkt Colors:\s*)(.*?)(?=\s{2,})").Value.Trim();
            string colorDesc = Regex.Match(pdfText, @"(?<= Colors Desc:\s*)(.*?)(?=\s{2,})").Value.Trim();
            return String.Format("{0} | {1}", colorBase, colorDesc);
        }
        private string GetDeliveryDate(string poDate)
        {
            DateTime poDateTime = DateTime.Parse(poDate);
            string dayString = poDateTime.AddDays(-1).ToString("dddd");
            if (dayString == "Sunday") return poDateTime.AddDays(-3).ToString("MM/dd/yyyy");
            else return poDateTime.AddDays(-1).ToString("MM/dd/yyyy");
        }
        
    }
}
