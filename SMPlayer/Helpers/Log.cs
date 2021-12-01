using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Helpers
{
    public class Log
    {
        //private static ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<Log>();

        private static void AppendText(string filename, string message)
        {
            File.AppendAllText(filename, message + Environment.NewLine);
        }
    }
}
