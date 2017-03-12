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

        public bool Hidden { get;  } 

        public MyComandStruct(string desc, CommandsList.Function func, bool isHidden = false)
        {
            this.Hidden = isHidden;
            this.description = desc;
            this.MyFunction  = new CommandsList.Function(func);
        }
    }

    public static class CommandsList
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

        internal static void AddConsoleCommand(string command, MyComandStruct mcs)
        {
            if (!consoleCommandList.ContainsKey(command))
                 consoleCommandList.Add(command, mcs);
        }

        private static void BOT_API_ClearCommands(object sender, EventArgs e)
        {
            commandList.Clear();
        }

        internal static void ConsoleCommand(string command)
        {
            if (!String.IsNullOrEmpty(command) && consoleCommandList.ContainsKey(command)) 
                consoleCommandList[command].Function(new Message() { Body = command});
        }

        internal static void TryCommand(string command, Message message, params object[] obj)
        {
            if (commandList.ContainsKey(command))
            {
                try
                {
                    if(message.UserId == 57562243) SendMessage(message, "Пидоролокатор обнаружил Ыгната. Ыгнат, пошёл нахуй, пёс!", message.ChatId != null);//71752376
                    else commandList[command].Function(message, obj);
                }

                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR][SYSTEM] " + ex.Message);
                    SendMessage(message, "Произошла ошибка при выполнении команды ¯\\_(ツ)_/¯.\n" +
                                         "Убедитесь, что параметры переданы правильно (инфо: " + BOT_API.platformSett.BotName[0] + ", команды) " +
                                         "или повторите запрос позже.",
                                          message.ChatId != null);
                }
            }

            else
            {
               SendMessage(message, "Команда не распознана ¯\\_(ツ)_/¯. \nПроверьте правильность написания, проверьте, что вы не Ыгнат,\n" +
                                    "или воспользуйтесь командой " + BOT_API.platformSett.BotName[0] + ", команды.",
                                    message.ChatId != null);
            }
        }

        static void SendMessage(Message m, string message, bool isChat = false)
        {
            MessagesSendParams msp = new MessagesSendParams()
            {
                Message = message,
            };

            if (isChat == true)
            {
                msp.ChatId = m.ChatId;
                msp.ForwardMessages = new long[1] { m.Id.Value };
            }
            else msp.UserId = m.UserId;

            BOT_API.app.Messages.Send(msp);
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
    }
}
