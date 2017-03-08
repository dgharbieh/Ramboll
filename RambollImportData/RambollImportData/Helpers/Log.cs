using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Web;
using Sitecore.Configuration;

namespace RambollImportData.Helpers
{
    public static class Log
    {
        public static void WriteToFile(string text)
        {
            string path = Settings.GetSetting("LogPath");
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                string message = string.Format(text, DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
                Console.WriteLine(message);
                writer.WriteLine(message);
                writer.Close();

            }
        }
    }
}