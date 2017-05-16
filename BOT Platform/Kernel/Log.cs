using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BOT_Platform
{
    public static partial class CommandsList
    {
        public static class Log
        {
            public static readonly string logFile = "Data\\BotData\\log.txt";
            static object lockObj = new object();
            static void InLog(object text)
            {
                lock(lockObj)
                {
                    using (StreamWriter sw = new StreamWriter(logFile, true))
                    {
                        sw.Write(text.ToString());
                    }
                }
            }
            public static void WriteLog(string text)
            {
                Task.Run(()=> InLog(text));
            }
        }
    }
}
