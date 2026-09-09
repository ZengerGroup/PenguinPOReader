namespace PenguinPOReader
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Check that args[0] exists
            if (args.Length < 1) Logger.ErrorExit(["No filepath passed to arguments"], 10);
            if (!File.Exists(args[0])) Logger.ErrorExit(["File passed does not exist."], 11);
            WorkbookHandler Workbook = new WorkbookHandler();
            if (!Workbook.VerifyConnection().Result) Logger.ErrorExit(["Unable to connect to google API."], 12);
            else Workbook.SetupApi();
            Logger.WriteLog("Begining to work on: {0}", true, args[0]);
            PdfReader Reader = new PdfReader(args[0]);
            Workbook.Update(Reader);
        }
    }
}
