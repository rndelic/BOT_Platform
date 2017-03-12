using System;
using VkNet;
using System.Text.RegularExpressions;
using System.Threading;
using System.Reflection;
using System.Linq;
using VkNet.Model;
using System.Collections.ObjectModel;
using System.Collections.Generic;

/*
     copy.Save(Filename, ImageFormat.Png);
                var uploadServer = vk.Photo.GetMessagesUploadServer();
        
                var wc = new WebClient();
                var responseImg = Encoding.Default.GetString(wc.UploadFile(uploadServer.UploadUrl, Filename));           
                ReadOnlyCollection<Photo> photo  = vk.Photo.SaveMessagesPhoto(responseImg);

                VkNet.Model.RequestParams.MessagesSendParams param = new MessagesSendParams();

                param.Attachments = photo;
                if (id == vk.UserId) param.UserId = id;
                if (id < 0) param.ChatId = -id;
                else param.UserId = id;
                if (Message == "") Message = "\n";
                param.Message = Message;
                take = 0;
                vk.Messages.Send(param);
                notify_hide.BalloonTipTitle = "Скриншот был успешно отправлен :)";
                notify_hide.BalloonTipText = "Скриншот был отправлен пользователю " + form3.dialog_list.SelectedItem;
                notify_hide.ShowBalloonTip(300);

                form3.Close();
                form3 = null;
                File.Delete(Filename);
                gkh.hook();
 */


namespace BOT_Platform
{
    static partial class BOT_API
    {
        static volatile string DATA_FILENAME = "data.ini"; /* Файл с настройками. 
                                                            * Распологается в одной папке с исполняемым файлом
                                                            */
        internal static volatile VkApi app = new VkApi();  /* Обьект самого приложения VK.NET */
        internal static PlatformSettings platformSett;     /* Здесь хранятся все настройки бота */

        const string DevNamespace = "MyFunctions";         /* Пространоство имён, содержащее только
                                                            * пользовательские функции
                                                            */

        internal static Thread botThread;
        internal static Thread consoleThread;

        static int currentTimerTime = 0;
        static List<Message> lastMessages;

        public static void Main()
        {
            Console.WriteLine("[Инициализация консоли...]");

            /* Запускаем обратчик команд */
            consoleThread = new Thread(ConsoleCommander)
            {
                Priority = ThreadPriority.Highest
            };
            consoleThread.Start();
        }

        public static event EventHandler ClearCommands;
        /// <summary>
        /// Функция - обработчик команд, поступающих в консоль
        /// </summary>
        internal static void ConsoleCommander()
        {
            /* Считываем настройки бота из файла настроек */
            Console.WriteLine("[Загружаются параметры платформы...]");
            platformSett = new PlatformSettings(DATA_FILENAME);

            CommandsList.ConsoleCommand("заглушка"); //заглушка
            ClearCommands.Invoke(new object(), null);
            Thread.Sleep(200);
            /* Подключаем модули */
            ExecuteModules();

            /* Создаём поток обработки ботом поступающих сообщений */
            CommandsList.ConsoleCommand("undebug");
            //Thread.CurrentThread.Join(3000);

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

                        List<Message> lm = new List<Message>();

                        ExecuteCommand(messages, ref lm);
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
                app.Authorize(platformSett.AuthParams);

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
                    if (platformSett.IsDebug == false) messages = app.Messages.Get(platformSett.MesGetParams);
                    else
                    {
                        return;
                    } 
                    ExecuteCommand(messages, ref lastMessages);
                    Thread.Sleep(platformSett.Delay);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR][SYSTEM " + DateTime.Now.ToLongTimeString() + "]:\n" + ex.Message + "\n");
                    Thread.Sleep(platformSett.Delay);
                    continue;
                }
            }

        }

        static void ExecuteCommand(MessagesGetObject messages, ref List<Message> lastMessages)
        {
            for (int i = 0; i < messages.Messages.Count; i++)
            {
                #region  УДАЛИТЬ ИЗ РЕЛИЗА
                /*
                if ((messages.Messages[i].UserId == 169756316 || messages.Messages[i].UserId == 121290303) && messages.Messages[i].ChatId != null)
                {
                    bool contains1 = false;
                    foreach (Message m in lastMessages)
                    {
                        if (m.Date == messages.Messages[i].Date &&
                           m.UserId == messages.Messages[i].UserId &&
                           m.Body == messages.Messages[i].Body)
                        {
                            contains1 = true;
                            break;
                        }
                    }
                    if (contains1) continue;

                    Functions.SendMessage(messages.Messages[i], "бот-переводчик: ко-ко-ко, куд-кудах", messages.Messages[i].ChatId != null);
                    lastMessages.Add(messages.Messages[i]);
                    continue;
                }*/
                #endregion

                string botName = FindBotNameInMessage(messages.Messages[i]);

                if (messages.Messages[i].ChatId != null && botName == "[NOT_FOUND]")
                    continue;

                if (Functions.ContainsMessage(messages.Messages[i], lastMessages)) continue;

                int index = messages.Messages[i].Body.IndexOf('(');
                int botNameIndex = messages.Messages[i].Body.IndexOf(botName + ",");

                string command = messages.Messages[i].Body;
                string param = null;

                if (index != -1)
                {
                    command = messages.Messages[i].Body.Substring(0, index);
                    param = messages.Messages[i].Body.Substring(index + 1);
                    int ind = param.LastIndexOf(')');
                    if (ind == -1)
                    {
                        param += ')';
                        param = param.Remove(param.Length-1);
                    }
                    else param = param.Remove(ind);
                }
                if (messages.Messages[i].ChatId != null || (botNameIndex >=0 && botNameIndex < command.Length))
                    command = command.Substring(botNameIndex + botName.Length + 1);

                Functions.RemoveSpaces(ref command);

                if (Char.IsUpper(command[0])) command = Char.ToLower(command[0]) + command.Substring(1);

                if (lastMessages.Count >= platformSett.MesRemembCount)
                {
                    for (int j = 0; j < lastMessages.Count / 2; j++) lastMessages.RemoveAt(0);
                }
                lastMessages.Add(messages.Messages[i]);

                CommandsList.TryCommand(command,
                                        messages.Messages[i],
                                        param);

                Thread.Sleep(platformSett.Delay);

            }
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

            /* Подключаем стандартный модуль с базовыми командами */
            StandartCommands sC = new StandartCommands();

            /* Подключаем модули, создавая обьекты их классов */
            Type[] typelist = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == DevNamespace).ToArray();
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

