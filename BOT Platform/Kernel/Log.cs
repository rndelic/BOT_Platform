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
        public class Log
        {
            public static string logFile = "Data\\BotData\\log.txt";
            Thread writeThread;
            object lockObj = new object();
            void InLog(object text)
            {
                lock(lockObj)
                {
                    using (StreamWriter sw = new StreamWriter(logFile, true))
                    {
                        sw.Write(text.ToString());
                    }
                }
            }

            public Log()
            {
                writeThread = new Thread(new ParameterizedThreadStart(InLog));
            }
            public void WriteLog(string text)
            {
                writeThread.Start(text);
            }
        }
    }
}
