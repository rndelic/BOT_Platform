using BOT_Platform.Interfaces;
using MyFunctions.Exceptions;
using System;
using System.Collections.Generic;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace BOT_Platform
{
    public struct MyComandStruct
    {
        readonly string description;
        readonly CommandsList.Function MyFunction;
        public string Description
        {
            get
            {
                return this.description;
            }
        }
        public CommandsList.Function Function
        {
            get
            {
                return this.MyFunction;
            }
        }

        public bool Hidden { get; }

        public MyComandStruct(string desc, CommandsList.Function func, bool isHidden = false)
        {
            this.Hidden = isHidden;
            this.description = desc;
            this.MyFunction = new CommandsList.Function(func);
        }
    }

    public static partial class CommandsList
    {

            static CommandsList()
            {
                BOT_API.ClearCommands += BOT_API_ClearCommands;
            }

        public delegate void Function(Message message, params object[] p);

        static SortedDictionary<string, MyComandStruct> commandList =
            new SortedDictionary<string, MyComandStruct>();

        static SortedDictionary<string, MyComandStruct> consoleCommandList =
            new SortedDictionary<string, MyComandStruct>();

        static Dictionary<string, string> banList = new Dictionary<string, string>();

        internal static void AddConsoleCommand(string command, MyComandStruct mcs)
        {
            if (!consoleCommandList.ContainsKey(command))
                consoleCommandList.Add(command, mcs);
        }

        public static void TryBanUser(Message message, string id, string description)
        {
            if (banList.ContainsKey(id))
            {
                Console.WriteLine("---------------------------------------------------------------------");
                Console.WriteLine("[ERROR] Пользователь https://vk.com/id" + id + " уже был забанен");
                Console.WriteLine("---------------------------------------------------------------------");
                Functions.SendMessage(message, "[ERROR] Пользователь https://vk.com/id" + id + " уже был забанен",
                                      message.ChatId != null);
            }

            else
            {
                banList.Add(id, description);
                Functions.SendMessage(message, "Пользователь https://vk.com/id" + id + " ЗАбанен!",
                                      message.ChatId != null);
            }
        }
        public static void TryUnBanUser(Message message, string id)
        {
            if (banList.ContainsKey(id))
            {
                banList.Remove(id);
                Functions.SendMessage(message, "Пользователь https://vk.com/id" + id + " был РАЗбанен!",
                                      message.ChatId != null);
            }

            else
            {
                Console.WriteLine("---------------------------------------------------------------------");
                Console.WriteLine("[ERROR] Пользователь https://vk.com/id" + id + " не был забанен");
                Console.WriteLine("---------------------------------------------------------------------");
                Functions.SendMessage(message, "[ERROR] Пользователь https://vk.com/id" + id + " не был забанен",
                                      message.ChatId != null);
            }
        }

        private static void BOT_API_ClearCommands(object sender, EventArgs e)
        {
            commandList.Clear();
            banList.Clear();
        }

        internal static void ConsoleCommand(string command)
        {
            if (!String.IsNullOrEmpty(command) && consoleCommandList.ContainsKey(command)) 
                consoleCommandList[command].Function(new Message() { Body = command});
        }

        internal static void TryCommand(Message message, params object[] obj)
        {
            if (banList.ContainsKey(message.UserId.ToString()))
            {
                Functions.SendMessage(message, banList[message.UserId.ToString()],
                                          message.ChatId != null);
                return;
            }
            if (commandList.ContainsKey(message.Body))
            {
                try
                {
                  commandList[message.Body].Function(message, obj);
                }
                catch (BotPlatformException ex)
                {
                    WriteErrorInfo(message, ex);
                    Functions.SendMessage(message, ex.Message + "\n\n" +
                                         $"Для получения справки по команде напишите {BOT_API.GetSettings().BotName[0]}, {message.Body}",
                                         message.ChatId != null);
                }
                catch (Exception ex)
                {
                    WriteErrorInfo(message, ex);
                    Functions.SendMessage(message, "Произошла ошибка при выполнении команды ¯\\_(ツ)_/¯.\n" +
                                         "Убедитесь, что параметры переданы правильно (инфо: " + BOT_API.GetSettings().BotName[0] + ", команды) " +
                                         "или повторите запрос позже.\n\n" +

                                          $"Для получения справки по команде напишите {BOT_API.GetSettings().BotName[0]}, {message.Body}",
                                          message.ChatId != null);
                }
            }

            else
            {
              if(message.UserId != BOT_API.GetApi().UserId)
                    Functions.SendMessage(message, "Команда \"" + message.Body +"\" не распознана ¯\\_(ツ)_/¯. \nПроверьте правильность написания " +
                                    "или воспользуйтесь командой " + BOT_API.GetSettings().BotName[0] + ", команды.",
                                    message.ChatId != null);
            }
        }
    
        internal static void TryAddCommand(string command, MyComandStruct mcs)
        {
            if (!commandList.ContainsKey(command))
                {
                 commandList.Add(command, mcs);
                }
            else Console.WriteLine("Не удалось добавить команду \\" + command + ".\n" +
                                   "Команда уже определена.\n");
        }

        //TODO: МОЖНО ВЫНЕСТИ В CONST STRING
        internal static List<string> GetCommandList(bool isBotCommands = false)
        {
            List<string> list = new List<string>();

            SortedDictionary<string, MyComandStruct> temp = isBotCommands == true ? commandList : 
                                                                              consoleCommandList;
           
                foreach (string key in temp.Keys)
                {
                    MyComandStruct mcs = temp[key];

                    if(mcs.Hidden != true)
                    list.Add(key + " - " + mcs.Description);
                }

            return list;
        }

        static void WriteErrorInfo(Message message, Exception ex)
        {
            string text = "---------------------------------------------------------------------\n" +
                          $"[ERROR { DateTime.Now.ToLongTimeString()}]\n" +
                          $"От: https://vk.com/id{message.UserId}\n" +
                          $"Команда: \"" + message.Title + "\"\n" + ex.Message + "\n" +
                          "[STACK_TRACE] " + ex.StackTrace + "\n" +
                          "---------------------------------------------------------------------\n";
            Console.WriteLine(text);

            Log log = new Log();
            log.WriteLog(text);
        }
    }
}
