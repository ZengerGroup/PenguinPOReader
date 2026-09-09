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
        public string UV;
        public string LAM;
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
            string[] PoDate = Regex.Match(pdfText, @"[0-9]{4,}/[0-9]{2}/[0-9]{2}/[0-9]{4}").Value.Split(@"/");
            Title = Regex.Match(pdfText, @"(?<=Title - Short:)(.*?)(?=\s{2,})").Value.Trim();
            PO = PoDate[0];
            Date = String.Join(@"/", PoDate[1..4]);
            Quantity = Regex.Match(pdfText, @"^*[0-9]*?\,*?[0-9]{1,3} Each").Value.Trim().Split(" ")[0];
            ISBN = Regex.Match(pdfText, @"(\s){3,}ISBN: [0-9]*").Value.Split(":")[1].Trim();
            Color = GetColor(pdfText);
            Stock = Regex.Match(pdfText, @"(?<=Vendor Suppl Cv/Jk Stock:\s*)(\b[\w\s#]+\b)(?=\s{2,})").Value;
            (UV,LAM) = GetUvAndLam(Regex.Match(pdfText, @"(?<=Cvr/Jkt\s+Coat\s+1:\s+)(\b[\w\s]+\b)(?=\s{2,})").Value);
            Binder = Regex.Match(pdfText, @"(?<=Binder:\s*)(\b[\w\s]+\b)(?=\s{2,})").Value;
            HardCover = Regex.Match(pdfText, @"(?<=Format:\s+)(\b[\w\s]+\b)(?=\s{2,})").Value.Contains("Hardcover");
        }
        private (string uv, string lam) GetUvAndLam(string value)
        {
            if (value.Contains("UV")) return (Regex.Match(value, @"^.*?(?=\bUV\b)").Value.Trim(), "No");
            else if (value.Contains("LAM")) return ("No", Regex.Match(value, @"^.*?(?=\bLAM\b)").Value.Trim());
            else return ("No", "No");
        }
        private string GetColor(string pdfText)
        {
            string colorBase = Regex.Match(pdfText, @"(?<=Cvr/Jkt Colors:\s*)(.*?)(?=\s{2,})").Value.Trim();
            string colorDesc = Regex.Match(pdfText, @"(?<= Colors Desc:\s*)(.*?)(?=\s{2,})").Value.Trim();
            //NOT SURE WHERE TO USE THE BELOW INFORMATION, BUT IT WAS HIGHLIGHTED?
            string blackInk = Regex.Match(pdfText, @"(?<=Black Ink:\s*)(.*?)(?=\s{2,})").Value.Trim();
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
            Console.WriteLine(UV);
            Console.WriteLine(LAM);
            Console.WriteLine(Binder);
            Console.WriteLine(Status);
            Console.WriteLine(HardCover);
        }
    }
}
