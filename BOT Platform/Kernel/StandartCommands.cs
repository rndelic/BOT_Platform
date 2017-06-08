using System;
using System.Collections.Generic;
using System.Text;
using VkNet.Model;
using System.Threading;
using VkNet.Model.RequestParams;
using BOT_Platform.Interfaces;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using BOT_Platform.Kernel;
using BOT_Platform.Kernel.Bots;

namespace BOT_Platform
{
    class StandartCommands
    {
        private const string BOTS_NAMESPACE = "BOT_Platform.Kernel.Bots";
        public void AddMyCommandInPlatform()
        {
            CommandsList.AddConsoleCommand("help", new MyComandStruct("Показывает список всех комманд",CShowCommands));
            CommandsList.AddConsoleCommand("exit", new MyComandStruct("Закрыть приложение", CExit));
            CommandsList.AddConsoleCommand("cls" , new MyComandStruct("Очистить консоль", ClearConsole));
            CommandsList.AddConsoleCommand("log", new MyComandStruct("Открывает лог", СLog));
            CommandsList.AddConsoleCommand("settings", new MyComandStruct("Открывает файл настроек", CSettings));
            CommandsList.AddConsoleCommand("class", new MyComandStruct("Показывает текущий список Bot-классов", Classes));
            CommandsList.AddConsoleCommand("!", new MyComandStruct("Тест бота", CDebugCommand));
            CommandsList.AddConsoleCommand("restart", new MyComandStruct("Перезапуск системы", CRestart));
            CommandsList.AddConsoleCommand("bots", new MyComandStruct("Список ботов", CBots));
            CommandsList.AddConsoleCommand("debug", new MyComandStruct("Активирует режим отладки", CDebug));
            CommandsList.AddConsoleCommand("undebug", new MyComandStruct("Деактивирует режим отладки", CDebug));
        }

        private void CDebugCommand(Message message, string args, Bot bot)
        {
            string[] param = args.Split(new char[] {','}, 2, StringSplitOptions.RemoveEmptyEntries);

            if (!Program.Bots.ContainsKey(param[0]))
            {
                Console.WriteLine($"Бот {param[0]} отсутствует в системе.\n");
                return;
            }
            Program.Bots[param[0]].DebugExecuteCommand(param[1]);
        }

        private void CBots(Message message, string args, Bot Bot)
        {
            StringBuilder sB = new StringBuilder();
            sB.AppendLine("---------------------------------------------------------------------");
            sB.AppendLine("Список загруженных ботов:");
            foreach (var bot in Program.Bots.Values)
            {
                                    sB.Append($"[{bot.Name}] - ");
                                    sB.Append($"Тип: {bot.GetType().Name}, ");
                                    sB.AppendLine($"Статус IsDebug: {bot.GetSettings().GetIsDebug().ToString().ToUpper()} ");
            }
            sB.AppendLine("---------------------------------------------------------------------");
            Console.WriteLine(sB.ToString());
        }

        private void Classes(Message message, string args, Bot Bot)
        {
            Type[] typelist = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace == BOTS_NAMESPACE 
                && (t.BaseType == typeof(Bot) || t.BaseType == typeof(UserBot) || t.BaseType == typeof(GroupBot)))
                .ToArray();

            StringBuilder sB = new StringBuilder();
            sB.AppendLine("---------------------------------------------------------------------");
            sB.AppendLine("Список текущих Bot-классов: ");
            foreach (Type type in typelist)
            {
                sB.AppendLine($"- {type.Name}");
            }
            sB.AppendLine("---------------------------------------------------------------------");

            Console.WriteLine(sB.ToString());
        }

        private void CSettings(Message message, string args, Bot bot)
        {
            if (bot == null && String.IsNullOrEmpty(args)) return;

            if (Program.Bots.ContainsKey(args))
                Process.Start(Path.Combine(Program.Bots[args].Directory, PlatfromSettings.PATH));
            else Console.WriteLine($"Бот {args} отсутствует в системе.\n");
        }

        private void СLog(Message message, string args, Bot bot)
        {
            if (bot == null && String.IsNullOrEmpty(args)) return;

            if (Program.Bots.ContainsKey(args))
                Process.Start(Path.Combine(Program.Bots[args].Directory, CommandsList.Log.logFile));
            else Console.WriteLine($"Бот {args} отсутствует в системе.\n");
        }

        public StandartCommands()
        {
            AddMyCommandInPlatform();
        }

        void SetDebug(Bot Bot)
        {
            void ThreadDebug(Bot bot)
            {
                bot.GetSettings().SetIsDebug(true);
                Console.WriteLine(
                    "---------------------" +
                    $"Решим отладки бота [{bot.Name}] ВКЛЮЧЁН(ON)" +
                    "---------------------");
            }
            if (Bot != null)
            {
               ThreadDebug(Bot);
            }
            else
            {
                foreach (var bot in Program.Bots.Values)
                {
                    ThreadDebug(bot);
                }
            }

        }
        void SetUndebug(Bot Bot)
        {
            void UndebugThread(Bot bot)
            {
                bot.GetSettings().SetIsDebug(false);
                Thread abortThread = bot.botThread;

                bot.botThread = new Thread(bot.BotWork)
                {
                    Priority = ThreadPriority.AboveNormal,
                    Name = bot.Name
                };

                bot.botThread.Start();
                if (abortThread != null)
                {
                    Console.WriteLine($"Перезапуск потока botThread в [{bot.Name}]");
                    Console.WriteLine(
                        "---------------------" +
                        $"Решим отладки бота [{bot.Name}] ВЫКЛЮЧЕН(OFF)" +
                        "---------------------");
                    abortThread.Abort($"Перезапуск потока botThread в [{bot.Name}]");
                }
            }

            if (Bot != null)
            {
                UndebugThread(Bot);
            }
            else
            {
                foreach (var bot in Program.Bots.Values)
                {
                    UndebugThread(bot);
                }
            }
        }
        void CDebug(Message message, string args, Bot Bot)
        {

            Bot = String.IsNullOrEmpty(args) ? Bot : 
                (Program.Bots.ContainsKey(args) ? Program.Bots[args] 
                : throw new ArgumentException($"Бот {args} отсутствует в системе."));

            if (message.Body == "debug")
            {
                SetDebug(Bot);
            }
            
            else if (message.Body == "undebug")
            {
                SetUndebug(Bot);
            }
        }
        void CRestart(Message message, string args, Bot bot)
        {
            Thread needToAbortThread = Program.consoleThread;
            Program.consoleThread = new Thread(Program.ConsoleCommander)
            {
                Priority = ThreadPriority.Highest
            };
            Program.consoleThread.Start();
            if (needToAbortThread != null)
            {
                Console.WriteLine("Платформа была перезапущена!");
                needToAbortThread.Abort("Платформа была перезапущена!");
            }
            
        }

        void CShowCommands(Message message, string args, Bot bot)
        {
            List<string> com = CommandsList.GetCommandList();
            Console.WriteLine("------------------Список команд------------------");
            foreach(string value in com)
            {
                Console.WriteLine(value);
            }
            Console.WriteLine("-------------------------------------------------\n");
        }
        void CExit(Message message, string args, Bot bot)
        {
            Environment.Exit(1);
        }
        void ClearConsole(Message message, string args, Bot bot)
        {
            Console.Clear();
        }

    }
}