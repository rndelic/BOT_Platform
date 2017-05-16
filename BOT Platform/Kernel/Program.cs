#define PARALLEL

using System;
using VkNet;
using System.Text.RegularExpressions;
using System.Threading;
using System.Reflection;
using System.Linq;
using VkNet.Model;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using BOT_Platform.Interfaces;
using System.Threading.Tasks;

namespace BOT_Platform
{
    static partial class BOT_API
    {
        private static volatile VkApi _app = new VkApi();       /* Обьект самого приложения VK.NET */
        public static VkApi GetApi()
        {
            return _app;
        }

        private static volatile PlatformSettings platformSett; /* Здесь хранятся все настройки бота */
        public static PlatformSettings GetSettings()
        {
            return platformSett;
        }

        const string DevNamespace = "MyFunctions";   /* Пространоство имён, содержащее только
                                                      * пользовательские функции
                                                      */
        internal static Thread botThread;
        internal static Thread consoleThread;

        static int currentTimerTime = 0;
        static volatile List<Message> lastMessages;

        public static void Main()
        {
            Console.WriteLine("[Инициализация консоли...]");
            /* Подключаем стандартный модуль с базовыми командами */
            StandartCommands sC = new StandartCommands();

            /* Запускаем обратчик команд */
            CommandsList.ConsoleCommand("restart");
        }

        public static event EventHandler ClearCommands;
        /// <summary>
        /// Функция - обработчик команд, поступающих в консоль
        /// </summary>
        internal static void ConsoleCommander()
        {
            /* Считываем настройки бота из файла настроек */
            Console.WriteLine("[Загружаются параметры платформы...]");
            platformSett = new PlatformSettings();

            CommandsList.ConsoleCommand("start"); //заглушка
            ClearCommands.Invoke(new object(), null);
            /* Подключаем модули */
            ExecuteModules();

            /* Создаём поток обработки ботом поступающих сообщений */
            CommandsList.ConsoleCommand("undebug");
            /* Обрабатываем команды, поступающие в консоль */
            while (true)
            {
                try
                {
                    string newCommand = ReadFromConsole(platformSett.CommandRegex);

                    if (FindBotNameInMessage(newCommand) == "[NOT_FOUND]" || platformSett.IsDebug == false)
                    {
                        CommandsList.ConsoleCommand(newCommand);
                    }
                    else
                    {
                        MessagesGetObject messages = new MessagesGetObject();
                        List<Message> lM = new List<Message>();
                        lM.Add(new Message() { Body = newCommand });
                        messages.Messages = new ReadOnlyCollection<Message>(lM);

                        ExecuteCommand(messages);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("[ERROR][SYSTEM " + DateTime.Now.ToLongTimeString() + "]:\n" + ex.Message + "\n");
                }
            }

        }
        /// <summary>
        /// Функция - обработчик сообщений, поступающих боту
        /// </summary>
        internal static void BotWork()
        {
            /* Подключаемся к VK, запускаем бота */
            try
            {
                Console.WriteLine("[Запуск бота...]");
                _app.Authorize(platformSett.AuthParams);

                Console.WriteLine("Бот запущен.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR][SYSTEM]:\n" + ex.Message + "\n");
                CommandsList.ConsoleCommand("debug");
            }
            lastMessages = new List<Message>();
            currentTimerTime = Environment.TickCount;

            while (true)
            {
                MessagesGetObject messages;
                try
                {
                    /* Получаем нужное количество сообщений для обработки согласно настройкам бота*/
                    if (platformSett.IsDebug == false) messages = _app.Messages.Get(platformSett.MesGetParams);
                    else
                    {
                        return;
                    } 
                    ExecuteCommand(messages);
                    Thread.Sleep(platformSett.Delay);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR][SYSTEM " + DateTime.Now.ToLongTimeString() + "]:\n" + ex.Message + "\n");

                    TryToRestartSystem();
                    Thread.Sleep(platformSett.Delay);
                    continue;
                }
            }

        }

        static void TryToRestartSystem()
        {
            Console.WriteLine("[NET_INFO " + DateTime.Now.ToLongTimeString() + "] Попытка подключиться к vk.com...");
            var answer = ConnectivityChecker.CheckConnection();
            if (!answer.status)
            {
                do
                {
                    Console.WriteLine(answer.info);
                    Thread.Sleep(millisecondsTimeout: 30000);

                 	Console.WriteLine("[NET_INFO " + DateTime.Now.ToLongTimeString() + "] Попытка подключиться к vk.com...");
                 	answer = ConnectivityChecker.CheckConnection();
                    //TODO: Заменить на botThread
                } while(!answer.status);

                CommandsList.ConsoleCommand("restart");
            }
        }

        static object lockObject = new object();

        public static string DevNamespace1 => DevNamespace;

        static void ExecuteCommand(MessagesGetObject messages)
        {
            Parallel.ForEach(messages.Messages, Message =>
                {
                    if (String.IsNullOrEmpty(Message.Body)) return;

                    string botName = FindBotNameInMessage(Message);

                    if (Message.ChatId != null && botName == "[NOT_FOUND]")
                        return;

                    lock (lockObject)
                        if (Functions.ContainsMessage(Message, lastMessages)) return;

                    CommandInfo comInfo = GetCommandFromMessage(Message, botName);
                    
                    lock (lockObject)
                    {
                        if (lastMessages.Count >= platformSett.MesRemembCount)
                        {
                            for (int j = 0; j < lastMessages.Count / 2; j++) lastMessages.RemoveAt(0);
                        }
                        lastMessages.Add(Message);
                    }
                    string temp = Message.Body;
                    Message.Body = comInfo.Command;
                    Message.Title = temp;
                    CommandsList.TryCommand(Message,
                                          comInfo.Param);
                    Message.Body = temp;
                }
            );
            Thread.Sleep(platformSett.Delay);
#if !PARALLEL
            for (int i = 0; i < messages.Messages.Count; i++)
            {
                if (String.IsNullOrEmpty(messages.Messages[i].Body)) continue;

                string botName = FindBotNameInMessage(messages.Messages[i]);

                if (messages.Messages[i].ChatId != null && botName == "[NOT_FOUND]")
                    continue;

                if (Functions.ContainsMessage(messages.Messages[i], lastMessages)) continue;

                CommandInfo comInfo = GetCommandFromMessage(messages.Messages[i], botName);

                if (lastMessages.Count >= platformSett.MesRemembCount)
                {
                    for (int j = 0; j < lastMessages.Count / 2; j++) lastMessages.RemoveAt(0);
                }
                lastMessages.Add(messages.Messages[i]);


                string temp = messages.Messages[i].Body;
                messages.Messages[i].Body = comInfo.command;
                messages.Messages[i].Title = temp;
                CommandsList.TryCommand(messages.Messages[i],
                                        comInfo.param);
                messages.Messages[i].Body = temp;

                Thread.Sleep(platformSett.Delay);

            }
#endif
        }

        struct CommandInfo
        {
            public string Command { get; private set; }
            public string Param { get; private set; }

            public CommandInfo(string command, string param)
            {
                this.Command = command;
                this.Param = param;
            }
        }
        static CommandInfo GetCommandFromMessage(Message message, string botName)
        {
            int index = message.Body.IndexOf('(');
            int botNameIndex = message.Body.IndexOf(botName + ",");

            string command = message.Body;
            string param = null;

            if (index != -1)
            {
                command = message.Body.Substring(0, index);
                param = message.Body.Substring(index + 1);
                int ind = param.LastIndexOf(')');
                if (ind == -1)
                {
                    param += ')';
                    param = param.Remove(param.Length - 1);
                }
                else param = param.Remove(ind);
            }
            if (message.ChatId != null || (botNameIndex >= 0 && botNameIndex < command.Length))
                command = command.Substring(botNameIndex + botName.Length + (1));

            Functions.RemoveSpaces(ref command);

            if (Char.IsUpper(command[0])) command = Char.ToLower(command[0]) + command.Substring(1);
            return new CommandInfo(command, param);
        }

        static string FindBotNameInMessage(Message message)
        {
                for (int y = 0; y < platformSett.BotName.Length; y++)
                {
                    /* Если входящее сообщение содержит обращение к боту - "бот, ..." */
                    int botNameIndex = message.Body.IndexOf(platformSett.BotName[y] + ",");
                    if (botNameIndex != -1)
                    {
                        return platformSett.BotName[y];
                    }
                }
                return "[NOT_FOUND]";
        }

        static string FindBotNameInMessage(string message)
        {
            for (int y = 0; y < platformSett.BotName.Length; y++)
            {
                /* Если входящее сообщение содержит обращение к боту - "бот, ..." */
                int botNameIndex = message.IndexOf(platformSett.BotName[y] + ",");
                if (botNameIndex != -1)
                {
                    return platformSett.BotName[y];
                }
            }
            return "[NOT_FOUND]";
        }
        
        /// <summary>
        /// Функция, подключающая все модули бота
        /// </summary>
        static void ExecuteModules()
        {
            Console.WriteLine("[Подключение модулей...]");

            /* Подключаем модули, создавая обьекты их классов */
            Type[] typelist = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == DevNamespace1).ToArray();
            foreach (Type type in typelist)
            {
                Activator.CreateInstance(Type.GetType(type.FullName));
                Console.WriteLine("Подключение " + type.FullName + "...");
            }

            Console.WriteLine("Модули подключены.");
        }

        /// <summary>
        /// Функция, ограничивающая ввод символов в консоль
        /// </summary>
        /// <param name="reg"> Допустимые символы ввода </param>
        /// <param name="length"> Максимальная длина строки ввода </param>
        /// <returns> Возвращает введённую строку из допустимых в Regex символов </returns>
        static string ReadFromConsole(Regex reg, int length = int.MaxValue)
        {
            int i = 0;
            string result = "";
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Backspace:
                        if (result.Length > 0)
                        {
                            result = result.Remove(result.Length - 1, 1);
                            Console.Write(key.KeyChar + " " + key.KeyChar);
                            --i;
                        }
                        break;
                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        return result;
                    default:
                        if (reg.IsMatch(key.KeyChar.ToString()) == true && i <= length)
                        {
                            Console.Write(key.KeyChar);
                            result += key.KeyChar;
                            ++i;
                        }
                        break;
                }
            }
        }
    }
}

