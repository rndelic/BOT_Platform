using MyFunctions.Exceptions;
using System;
using System.Collections.Generic;
using VkNet.Model;
using VkNet.Model.RequestParams;
using  System.Threading;
using System.Threading.Tasks;
using BOT_Platform.Kernel;
using BOT_Platform.Kernel.Bots;
using BOT_Platform.Kernel.CIO;
using BOT_Platform.Kernel.Interfaces;

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
                Program.ClearCommands += BOT_API_ClearCommands;
            }

        public delegate void Function(Message message, string args, Bot bot);

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

        public static void TryBanUser(Message message, string id, string description, Bot bot)
        {
            if (banList.ContainsKey(id))
            {
                BotConsole.Write("---------------------------------------------------------------------");
                BotConsole.Write("[ERROR] Пользователь https://vk.com/id" + id + " уже был забанен");
                BotConsole.Write("---------------------------------------------------------------------");
                Functions.SendMessage(bot, message, "[ERROR] Пользователь https://vk.com/id" + id + " уже был забанен",
                                      message.ChatId != null);
            }

            else
            {
                banList.Add(id, description);
                Functions.SendMessage(bot, message, "Пользователь https://vk.com/id" + id + " ЗАбанен!",
                                      message.ChatId != null);
            }
        }
        public static void TryUnBanUser(Message message, string id, Bot bot)
        {
            if (banList.ContainsKey(id))
            {
                banList.Remove(id);
                Functions.SendMessage(bot, message, "Пользователь https://vk.com/id" + id + " был РАЗбанен!",
                                      message.ChatId != null);
            }

            else
            {
                BotConsole.Write("---------------------------------------------------------------------");
                BotConsole.Write("[ERROR] Пользователь https://vk.com/id" + id + " не был ЗАбанен");
                BotConsole.Write("---------------------------------------------------------------------");
                Functions.SendMessage(bot, message, "[ERROR] Пользователь https://vk.com/id" + id + " не был ЗАбанен",
                                      message.ChatId != null);
            }
        }

        private static void BOT_API_ClearCommands(object sender, EventArgs e)
        {
            commandList.Clear();
            banList.Clear();
        }

        internal static void ConsoleCommand(string command, string args, Bot bot)
        {
            if (!String.IsNullOrEmpty(command) && consoleCommandList.ContainsKey(command)) 
                consoleCommandList[command].Function(new Message() { Body = command}, args, bot);
        }

        internal static void TryCommand(Message message, string args, Bot bot)
        {

            if (banList.ContainsKey(message.UserId.ToString()))
            {
                Functions.SendMessage(bot, message, banList[message.UserId.ToString()],
                                          message.ChatId != null);
                return;
            }
            if (commandList.ContainsKey(message.Body))
            {
                try
                {
                    commandList[message.Body].Function(message, args, bot);
                }
                catch (BotPlatformException ex)
                {
                    WriteErrorInfo(message, ex, bot);
                    Functions.SendMessage(bot, message, ex.Message + "\n\n" +
                                                        $"Для получения справки по команде напишите {bot.GetSettings().BotName[0]}, {message.Body}",
                        message.ChatId != null);
                }
                catch (Exception ex)
                {
                    WriteErrorInfo(message, ex, bot);
                    if (ex.InnerException is BotPlatformException)
                    {
                        Functions.SendMessage(bot, message, ex.InnerException.Message + "\n\n" +
                                                            $"Для получения справки по команде напишите {bot.GetSettings().BotName[0]}, {message.Body}",
                            message.ChatId != null);
                    }
                    else
                    {
                        Functions.SendMessage(bot, message, "Произошла ошибка при выполнении команды ¯\\_(ツ)_/¯.\n" +
                                                            "Убедитесь, что параметры переданы правильно (инфо: " +
                                                            bot.GetSettings().BotName[0] + ", команды) " +
                                                            "или повторите запрос позже.\n\n" +

                                                            $"Для получения справки по команде напишите {bot.GetSettings().BotName[0]}, {message.Body}",
                            message.ChatId != null);
                    }

                }
            }

            else
            {
              if(message.UserId != bot.GetApi().UserId)
                    Functions.SendMessage(bot, message, "Команда \"" + message.Body +"\" не распознана ¯\\_(ツ)_/¯. \nПроверьте правильность написания " +
                                    "или воспользуйтесь командой " + bot.GetSettings().BotName[0] + ", команды.",
                                    message.ChatId != null);
            }
        }
    
        internal static void TryAddCommand(string command, MyComandStruct mcs)
        {
            if (!commandList.ContainsKey(command))
                {
                 commandList.Add(command, mcs);
                }
            else BotConsole.Write("Не удалось добавить команду \\" + command + ".\n" +
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

        static void WriteErrorInfo(Message message, Exception ex, Bot bot)
        {
            string text = "---------------------------------------------------------------------\n" +
                          $"[ERROR { DateTime.Now.ToLongTimeString()}]\n" +
                          $"От: https://vk.com/id{message.UserId}\n" +
                          $"Команда: \"" + message.Title + "\"\n" + ex.Message + "\n" +
                          "[STACK_TRACE] " + ex.StackTrace + "\n" +
                          "---------------------------------------------------------------------\n";
            Parallel.Invoke(() =>
            {
                BotConsole.Write(text);
            },()=>
            {
                Log.WriteLog(text, bot.Directory);
            });
        }
    }
}
