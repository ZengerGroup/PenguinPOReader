using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace PenguinPOReader
{
    internal static class Configurator
    {
        public static string LogPath = ConfigurationManager.AppSettings["LogPath"];
        public static string IssuePath = ConfigurationManager.AppSettings["IssuePath"];
        public static string CredentialsPath = ConfigurationManager.AppSettings["Credentials"];
        public static string SheetId = ConfigurationManager.AppSettings["SheetId"];
    }
}
