using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BOT_Platform.Kernel.CIO;
using BOT_Platform.Kernel.Interfaces;
using VkNet;
using VkNet.Model;

namespace BOT_Platform.Kernel.Bots
{

    public abstract class Bot
    {
        /// <summary>
        /// Имя бота в системе
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Физический полный пусть к боту
        /// </summary>
        public string Directory { get; private set; }
        /// <summary>
        /// </summary>
        /// <param name="name">Имя бота в системе</param>
        /// <param name="directory">Физический полный путь к боту</param>
        public Bot(string name, string directory)
        {
            Name = name;
            Directory = directory;
            _app = new VkApi();
        }
        /// <summary>
        /// Получить список пользователей с правами
        /// администратора для этого бота
        /// </summary>
        public long[] GetAdmins
        {
            get
            {
                return platformSett.Admins;
            }
        }
        /// <summary>
        /// Загрузка параметров бота из файла настроек,
        /// указанного по физическому пути бота
        /// </summary>
        /// <returns></returns>
        public virtual bool InitalizeBot()
        {
            try
            {
                platformSett = new PlatfromSettings(Directory + $@"\{PlatfromSettings.PATH}");
            }
            catch (Exception ex)
            {
                BotConsole.Write("---------------------------------------------------------------------");
                BotConsole.Write($"[ERROR][{Name}]:\n" + ex.Message);
                BotConsole.Write("---------------------------------------------------------------------");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Поток бота
        /// </summary>
        public Thread botThread;
        /// <summary>
        /// Обьект для работы с VK API
        /// (Обьект самого приложения VK.NET)
        /// </summary>
        protected volatile VkApi _app;    
        /// <summary>
        /// Возвращает обьект для работы с VK API
        /// </summary>
        /// <returns></returns>
        public VkApi GetApi()
        {
            return _app;
        }
        /// <summary>
        /// Настройки бота
        /// </summary>
        protected volatile PlatfromSettings platformSett; /* Здесь хранятся все настройки бота */
        /// <summary>
        /// Возвращает настройки бота
        /// </summary>
        /// <returns></returns>
        public  PlatfromSettings GetSettings()
        {
            return platformSett;
        }
        
        protected int currentTimerTime = 0;
        protected  List<Message> lastMessages;

        protected object lockObject = new object();
        /// <summary>
        /// Функция - обработчик сообщений, поступающих боту
        /// </summary>
        public virtual void BotWork()
        {
       
            /* Подключаемся к VK, запускаем бота */
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
            lastMessages = new List<Message>();
            currentTimerTime = Environment.TickCount;

            while (true)
            {
                MessagesGetObject messages;
                try
                {
                    /* Получаем нужное количество сообщений для обработки согласно настройкам бота*/
                    if (platformSett.GetIsDebug() == false) messages = _app.Messages.Get(platformSett.MesGetParams);
                    else
                    {
                        return;
                    }
                    ExecuteCommand(messages);
                    Thread.Sleep(platformSett.Delay);
                }
                catch (Exception ex)
                {
                    BotConsole.Write($"[ERROR][{Name} " + DateTime.Now.ToLongTimeString() + "]:\n" + ex.Message + "\n");
                    if (ex.Message == "User authorization failed: access_token has expired.")
                    {
                        this._app.RefreshToken();
                        BotConsole.Write($"[ERROR][{Name} " + DateTime.Now.ToLongTimeString() + "]: Токен обновлён.\n");
                    }
                    else TryToRestartSystem();

                    Thread.Sleep(platformSett.Delay);
                    continue;
                }
            }

        }
        /// <summary>
        /// Пытается подключиться к vk.com в случае потери соединения и перезагружает бота в случае необходимости
        /// </summary>
        protected void TryToRestartSystem()
        {
            if(!platformSett.GetIsDebug())
                BotConsole.Write($"[NET_INFO {Name} " + DateTime.Now.ToLongTimeString() + "] Попытка подключиться к vk.com...");
            var answer = ConnectivityChecker.CheckConnection();
            if (!platformSett.GetIsDebug())
                BotConsole.Write($"[NET_INFO {Name} " + DateTime.Now.ToLongTimeString() + "] " + answer.info);
            if (!answer.status)
            {
                do
                {
                    Thread.Sleep(millisecondsTimeout: 30000);
                    answer = ConnectivityChecker.CheckConnection();

                } while (!answer.status);

                if (!platformSett.GetIsDebug())
                {
                    BotConsole.Write($"[NET_INFO {Name} " + DateTime.Now.ToLongTimeString() +
                                      "] Попытка подключиться к vk.com...");
                    BotConsole.Write($"[NET_INFO {Name} " + DateTime.Now.ToLongTimeString() + "] " + answer.info);
                }

                CommandsList.ConsoleCommand("undebug", null, this);
            }
        }
        /// <summary>
        /// Определяет, каким образом будет обрабатываться команда
        /// </summary>
        /// <param name="messages">Список последних сообщений, полученных ботом</param>
        protected virtual void ExecuteCommand(MessagesGetObject messages)
        {
            Parallel.ForEach(messages.Messages, Message =>
                {
                    if (String.IsNullOrEmpty(Message.Body)) return;

                    string botName = FindBotNameInMessage(Message);

                    //if (Message.ChatId != null && botName == "[NOT_FOUND]")
                    if (botName == "[NOT_FOUND]")
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
                        comInfo.Param, this);
                    Message.Body = temp;
                }
            );
            Thread.Sleep(platformSett.Delay);
        }
        /// <summary>
        /// Определяет, каким образом будет обрабатываться команда в DebugMode бота
        /// </summary>
        /// <param name="consoleCommand">команда</param>
        public virtual void DebugExecuteCommand(string consoleCommand)
        {
            Task.Run(() =>
                {
                    Message Message = new Message {Body = consoleCommand};

                    if (String.IsNullOrEmpty(Message.Body)) return;

                    string botName = FindBotNameInMessage(Message);

                    if (botName == "[NOT_FOUND]")
                        return;

                    CommandInfo comInfo = GetCommandFromMessage(Message, botName);

                    string temp = Message.Body;
                    Message.Body = comInfo.Command;
                    Message.Title = temp;
                    CommandsList.TryCommand(Message,
                        comInfo.Param, this);
                    Message.Body = temp;
                }
            );
        }
        public struct CommandInfo
        {
            public string Command { get; private set; }
            public string Param { get; private set; }

            public CommandInfo(string command, string param)
            {
                this.Command = command;
                this.Param = param;
            }
        }
        protected CommandInfo GetCommandFromMessage(Message message, string botName)
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
        protected string FindBotNameInMessage(Message message)
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
        protected string FindBotNameInMessage(string message)
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
        public void Abort()
        {
            botThread.Abort();
        }
    }

    public class UserBot : Bot
    {
        public UserBot(string name, string directory) : base(name, directory)
        {
        }
    }

    public class GroupBot : Bot
    {
        public GroupBot(string name, string directory) : base(name, directory)
        {
        }

        public override bool InitalizeBot()
        {
            try
            {
                platformSett = new GroupSettings(Directory + $@"\{GroupSettings.PATH}");
            }
            catch (Exception ex)
            {
                BotConsole.Write("---------------------------------------------------------------------");
                BotConsole.Write($"[ERROR][{Name}]:\n" + ex.Message);
                BotConsole.Write("---------------------------------------------------------------------");
                return false;
            }
            return true;
        }

        public override void BotWork()
        {
            /* Подключаемся к VK, запускаем бота */

                BotConsole.Write($"[Запуск бота {Name}...]");
                _app.Authorize((platformSett as GroupSettings).Token, null, 0);

            if (!ConnectivityChecker.CheckConnection().status)
            {
                BotConsole.Write($"[ERROR][{Name}]:\n" + "Ошибк подключения к vk.com" + "\n");
                CommandsList.ConsoleCommand("debug", null, this);
                Task.Run(() => TryToRestartSystem());
                return;
            }

            BotConsole.Write($"Бот {Name} запущен.");


            lastMessages = new List<Message>();
            currentTimerTime = Environment.TickCount;

            while (true)
            {
                MessagesGetObject messages;
                try
                {
                    /* Получаем нужное количество сообщений для обработки согласно настройкам бота*/
                    if (platformSett.GetIsDebug() == false) messages = _app.Messages.Get(platformSett.MesGetParams);
                    else
                    {
                        return;
                    }
                    ExecuteCommand(messages);
                    Thread.Sleep(platformSett.Delay);
                }
                catch (Exception ex)
                {
                    BotConsole.Write($"[ERROR][{Name} " + DateTime.Now.ToLongTimeString() + "]:\n" + ex.Message + "\n");

                    TryToRestartSystem();
                    Thread.Sleep(platformSett.Delay);
                    continue;
                }
            }
        }
        protected override void ExecuteCommand(MessagesGetObject messages)
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
                        comInfo.Param, this);
                    Message.Body = temp;
                }
            );
            Thread.Sleep(platformSett.Delay);
        }
    }


}
