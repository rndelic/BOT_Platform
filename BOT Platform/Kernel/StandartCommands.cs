using System;
using System.Collections.Generic;
using System.Text;
using VkNet.Model;
using System.Threading;
using VkNet.Model.RequestParams;
using BOT_Platform.Interfaces;
using System.Diagnostics;

namespace BOT_Platform
{
    class StandartCommands
    {
        public void AddMyCommandInPlatform()
        {
            CommandsList.AddConsoleCommand("help", new MyComandStruct("Показывает список всех комманд",CShowCommands));
            CommandsList.AddConsoleCommand("exit", new MyComandStruct("Закрыть приложение", CExit));
            CommandsList.AddConsoleCommand("cls" , new MyComandStruct("Очистить консоль", ClearConsole));
            CommandsList.AddConsoleCommand("log", new MyComandStruct("Открывает лог", СLog));
            CommandsList.AddConsoleCommand("settings", new MyComandStruct("Открывает файл настроек", CSettings));
            CommandsList.AddConsoleCommand("test", new MyComandStruct("Тест бота", CTest));
            CommandsList.AddConsoleCommand("restart", new MyComandStruct("Перезапуск системы", CRestart));
            CommandsList.AddConsoleCommand("debug", new MyComandStruct("Активирует режим отладки", CDebug));
            CommandsList.AddConsoleCommand("undebug", new MyComandStruct("Деактивирует режим отладки", CDebug));
        }

        private void CSettings(Message message, object[] p)
        {
            Process.Start(PlatformSettings.DATA_FILENAME);
        }

        private void СLog(Message message, object[] p)
        {
            Process.Start(CommandsList.Log.logFile);
        }

        public StandartCommands()
        {
            AddMyCommandInPlatform();
        }

        void CDebug(Message message, params object[] p)
        {
            if (message.Body == "debug" && BOT_API.platformSett.IsDebug == false)
            {
                BOT_API.platformSett.IsDebug = true;
                Console.Title = "DEBUG VERSION";
                Console.WriteLine(
                    "---------------------" +
                    "Решим отладки ВКЛЮЧЁН(ON)" +
                    "---------------------");
            }

            else if (message.Body == "undebug")
            {
                BOT_API.platformSett.IsDebug = false;
                if (BOT_API.botThread != null)
                {

                    BOT_API.botThread.Abort("Перезапуск потока botThread");
                    Console.WriteLine("Перезапуск потока botThread");
                }

                BOT_API.botThread = new Thread(BOT_API.BotWork)
                {
                    Priority = ThreadPriority.AboveNormal
                };
                BOT_API.botThread.Start();
                Console.Title = "ACTIVE VERSION";
                Console.WriteLine(
                    "---------------------" +
                    "Решим отладки ВЫКЛЮЧЕН(OFF)" +
                    "---------------------");
            }
        }
        void CRestart(Message message, params object[] p)
        {
            Thread needToAbortThread = BOT_API.consoleThread;
            BOT_API.consoleThread = new Thread(BOT_API.ConsoleCommander)
            {
                Priority = ThreadPriority.Highest
            };
            BOT_API.consoleThread.Start();
            if(needToAbortThread != null) needToAbortThread.Abort("Платформа была перезапущена!");
            
        }
        void CTest(Message message, params object[] p)
        {
            Message m = new Message()
            {
                UserId = BOT_API.app.UserId,
                ChatId = null
            };
            Functions.SendMessage(m, "Test");
            Console.WriteLine("Успешно.");
        }
        void CShowCommands(Message message, params object[] p)
        {
            List<string> com = CommandsList.GetCommandList();
            Console.WriteLine("\n------------------Список команд------------------");
            foreach(string value in com)
            {
                Console.WriteLine(value);
            }
            Console.WriteLine("-------------------------------------------------\n");
        }
        void CExit(Message message, params object[] p)
        {
            Environment.Exit(1);
        }
        void ClearConsole(Message message, params object[] p)
        {
            Console.Clear();
        }

    }
}