using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LM2Randomiser.Logging
{
    public sealed class Logger
    {
        private static readonly Logger instance = new Logger();

        private const string fileName = "log.txt";
        private string path;

        static Logger() { }

        private Logger()
        {
            path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        }

        public void Log(string message, params object[] args)
        {
            using(StreamWriter sw = new StreamWriter(path, true))
            {
                sw.WriteLine(String.Format(message, args));
            }
        }

        public static Logger GetLogger
        {
            get 
                {
                return instance;
            }
        }
    }
}
