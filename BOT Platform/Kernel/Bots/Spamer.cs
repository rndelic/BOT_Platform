using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BOT_Platform.Kernel.Bots;
using BOT_Platform.Kernel.CIO;
using VkNet.Model;

namespace BOT_Platform.Kernel.Bots
{
    class Spamer : UserBot
    {
        public Spamer(string botName, string directory) : base(botName, directory)
        {}

        public Spamer() : base(null, null)
        {
        }

        public override void BotWork()
        {
            try
            {
                BotConsole.Write($"[Запуск бота {Name}...]");
                _app.Authorize(platformSett.AuthParams);
                BotConsole.Write($"Бот {Name} запущен.");
            }
            catch (Exception ex)
            {
                BotConsole.Write($"[ERROR][{Name}]:\n" + ex.Message + "\n");
                CommandsList.ConsoleCommand("debug", null, this);
                Task.Run(() => TryToRestartSystem());
                return;
            }

            string TITLE = "ЗА МАТАН И ДВОР ИНТЕГРИРУЮ В УПОР";
            while (true)
            {
                try
                {
                    if (_app.Messages.GetChat(7).Title != TITLE)
                        _app.Messages.EditChat(7, TITLE);
                    Thread.Sleep(200);
                }
                catch
                {
                    Thread.Sleep(200);
                }
            }
        }
    }
}
