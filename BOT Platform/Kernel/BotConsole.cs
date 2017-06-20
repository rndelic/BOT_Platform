using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BOT_Platform.Kernel.Interfaces;

namespace BOT_Platform.Kernel.CIO /* CIO - Console In-Out */
{
    class PCConsole : IConsole
    {
        public string Read()
        {
            return Console.ReadLine();
        }
        public void Write(string text)
        {
            Console.WriteLine(text);
        }
    }

    class BotConsole 
    {
        static PCConsole pcConsole = new PCConsole();
        public static string Read()
        {
            return pcConsole.Read();
        }

        public static void Write(string text)
        {
            pcConsole.Write(text);
        }
    }
}
