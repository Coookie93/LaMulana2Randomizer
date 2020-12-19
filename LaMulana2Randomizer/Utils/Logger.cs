using System;
using System.IO;
using System.Windows;

namespace LaMulana2Randomizer.Utils
{
    public static class Logger
    {
        private static StreamWriter sw;

        public static void Log(string message)
        {
            try
            {
                if(sw == null)
                    sw = new StreamWriter("log.txt", true);

                sw.WriteLine(message);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error with logging, {ex.Message}");
            }
        }

        public static void Flush()
        {
            if (sw != null)
                sw.Flush();
        }

        public static void LogAndFlush(string message)
        {
            Log(message);
            Flush();
        }
    }
}
