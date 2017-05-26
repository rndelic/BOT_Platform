﻿using BOT_Platform.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VkNet;
using VkNet.Model;

namespace BOT_Platform
{
    public abstract class Bot
    {
        public string Name { get; private set; }
        public string Directory { get; private set; }
        public Bot(string name, string directory)
        {
            Name = name;
            Directory = directory;
            _app = new VkApi();
        }

        public long[] GetAdmins
        {
            get
            {
                return platformSett.Admins;
            }
        }

        public virtual bool InitalizeBot()
        {
            try
            {
                platformSett = new PlatfromSettings(Directory + $@"\{PlatfromSettings.PATH}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR][{Name}]:\n" + ex.Message);
                return false;
            }
            return true;
        }

        public Thread botThread;
        protected  volatile VkApi _app;      /* Обьект самого приложения VK.NET */
        public VkApi GetApi()
        {
            return _app;
        }
        protected volatile PlatfromSettings platformSett; /* Здесь хранятся все настройки бота */
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
                Console.WriteLine($"[Запуск бота {Name}...]");
                _app.Authorize(platformSett.AuthParams);
                //_app.Authorize("68ff377c0c907bd8194f0e70ba81d8b4a641b4c05cadbc4e3cfca4d3b6cf4a6deaa511b88196e3af60f21", null, 0);
                Console.WriteLine($"Бот {Name} запущен.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR][{Name}]:\n" + ex.Message + "\n");
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
                    Console.WriteLine($"[ERROR][{Name} " + DateTime.Now.ToLongTimeString() + "]:\n" + ex.Message + "\n");

                    TryToRestartSystem();
                    Thread.Sleep(platformSett.Delay);
                    continue;
                }
            }

        }
        protected void TryToRestartSystem()
        {
            if(!platformSett.GetIsDebug())
                Console.WriteLine($"[NET_INFO {Name} " + DateTime.Now.ToLongTimeString() + "] Попытка подключиться к vk.com...");
            var answer = ConnectivityChecker.CheckConnection();
            if (!platformSett.GetIsDebug())
                Console.WriteLine(answer.info);
            if (!answer.status)
            {
                do
                {
                    Thread.Sleep(millisecondsTimeout: 30000);

                    if (!platformSett.GetIsDebug())
                        Console.WriteLine($"[NET_INFO {Name} " + DateTime.Now.ToLongTimeString() + "] Попытка подключиться к vk.com...");
                    answer = ConnectivityChecker.CheckConnection();
                    if (!platformSett.GetIsDebug())
                        Console.WriteLine(answer.info);

                } while (!answer.status);
 
                CommandsList.ConsoleCommand("undebug", null, this);
            }
        }
        protected void ExecuteCommand(MessagesGetObject messages)
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
        public void DebugExecuteCommand(string consoleCommand)
        {
            Task.Run(() =>
                {
                    Message Message = new Message();
                    Message.Body = consoleCommand;

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
                Console.WriteLine($"[ERROR][{Name}]:\n" + ex.Message);
                return false;
            }
            return true;
        }

        public override void BotWork()
        {
            /* Подключаемся к VK, запускаем бота */

                Console.WriteLine($"[Запуск бота {Name}...]");
                _app.Authorize((platformSett as GroupSettings).Token, null, 0);
                Console.WriteLine($"Бот {Name} запущен.");

            if (!ConnectivityChecker.CheckConnection().status)
            {
                Console.WriteLine($"[ERROR][{Name}]:\n" + "Ошибк подключения к vk.com" + "\n");
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
                    Console.WriteLine($"[ERROR][{Name} " + DateTime.Now.ToLongTimeString() + "]:\n" + ex.Message + "\n");

                    TryToRestartSystem();
                    Thread.Sleep(platformSett.Delay);
                    continue;
                }
            }
        }
    }


}
