using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BOT_Platform;
using VkNet.Model;
using VkNet.Model.RequestParams;
using BOT_Platform.Interfaces;
using MyFunctions;
using MyFunctions.Exceptions;

namespace MyFunctions
{
    class AnonChat : IMyCommands
    {
        public void AddMyCommandInPlatform()
        {
            CommandsList.TryAddCommand("анонимный чат", new MyComandStruct("Чтобы создать чат, отправьте боту команду бот," +
                " анонимный чат(\"название чата в кавычках!\", ссылки на анонимных собеседников " +
                "(не более " + countOfUsers + " пользователей) )\n" +
                "Учтите, что ваши имена будут скрыты. Be secure.", StartChat));

            CommandsList.TryAddCommand("мои чаты", new MyComandStruct("Список анонимных чатов, в которых состоит пользователь", MyChats));
        }

        private void MyChats(Message message, string args, Bot bot)
        {
            ChatList[] myChats = Chats.Values.Where(t => t.usersId.ContainsKey(message.UserId.Value)).ToArray();

            if(myChats.Length == 0)
            {
                Functions.SendMessage(bot, message, "Вы не состоите ни в одном анонимном чате ¯\\_(ツ)_/¯.", message.ChatId != null);
            }
            else
            {
                StringBuilder sB = new StringBuilder();
                sB.Append("Список ваших чатов:\n");
                for(int i=0; i<myChats.Length; i++)
                {
                    sB.Append((i + 1) + $") {myChats[i].Title}\n");
                }
                Functions.SendMessage(bot, message, sB.ToString(), message.ChatId != null);
            }
        }

        string description = "С вами был создан анонимный чат [TITLE]\n\n" +
            "Для того, чтобы отправить в него сообщение, напишите бот, анонимно(\"название чата в кавычках!\", ваше сообщение)\n"
			+"Например: бот, анонимно([TITLE], привет всем!)";

        static string DATA_FILE = Environment.CurrentDirectory + "\\Data\\chats.dat";
        int countOfUsers          = 2;

        static SortedDictionary<string, ChatList> Chats;

        object lockObject = new object();
        void StartChat(Message message, string param, Bot bot)
        {
            if (NeedCommandInfo(message, param, bot)) return;

            string[] args = param.Split(new char[1] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries);
            string chatTitle = Functions.RemoveSpaces(args[0]);
            string[] userId = args[1].Split(new char[1] { ',' }, countOfUsers, StringSplitOptions.RemoveEmptyEntries);

            long? answerId = message.UserId;

            if (Chats.ContainsKey(chatTitle))
            {
                if (!Chats[chatTitle].usersId.ContainsKey(answerId.Value) || Chats[chatTitle].usersId[answerId.Value] != 1)
                {

                    Functions.SendMessage(bot, message, "Чат " + chatTitle + " уже был создан, или у вас недостаточно прав для создания чата с таким названием\n" +
                                                   "¯\\_(ツ)_/¯", message.ChatId != null);
                    return;
                }
            }
            if (userId.Length > countOfUsers)
            {
                Functions.SendMessage(bot, message, "Добавьте в беседу не более, чем " + countOfUsers + " пользователей.", message.ChatId != null);
                return;
            }
            ChatList chat = new ChatList(chatTitle);

            try
            {
                Functions.SendMessage(bot, message, description.Replace("[TITLE]", chatTitle) + "\nВы создатель данной беседы.\nПопытка добавить других собеседников...", message.ChatId != null);
            }
            catch (VkNet.Exception.VkApiException ex)
            {
                Functions.SendMessage(bot, message, "К сожалению, я не могу добавить вас чат \n" +
                                               "Такие дела ¯\\_(ツ)_/¯.\n" +
                                               "Чат " + chatTitle + " не удалось создать.", message.ChatId != null);
                return;
            }

            chat.usersId.Add(message.UserId.Value, 1);

            uint count = 2;
            for (int i = 0; i < userId.Length; i++)
            {
                int index = userId[i].ToString().LastIndexOf('/');
                if (index != -1) userId[i] = userId[i].ToString().Substring(index + 1);

                Regex reg = new Regex("[а-я|А-Я|a-z|A-Z]");
                bool regexIsFoundInFull = reg.IsMatch(userId[i]);

                //Если нашёл "id"
                userId[i] = userId[i].Replace(" ", "");

                index = userId[i].ToString().IndexOf("id");
                if (index == 0)
                {
                    if (!reg.IsMatch(userId[i].ToString().Substring(2)))
                    {
                        userId[i] = userId[i].ToString().Substring(2);
                    }
                    else
                    {
                        userId[i] = bot.GetApi().Users.Get(userId[i]).Id.ToString();
                    }
                }

                else if (reg.IsMatch(userId[i]))
                {
                    try
                    {
                        userId[i] = bot.GetApi().Users.Get(userId[i]).Id.ToString();
                    }
                    catch
                    {
                        Functions.SendMessage(bot, message, "Неверная ссылка: \"" + userId[i] + "\" не является пользователем ¯\\_(ツ)_/¯.", message.ChatId != null);
                        continue;
                    }
                }

                message.UserId = Convert.ToInt32(userId[i]);
                if (chat.usersId.ContainsKey(message.UserId.Value)) continue;

                try
                {
                    Functions.SendMessage(bot, message, description.Replace("[TITLE]", chatTitle));
                }
                catch (Exception ex)
                {
                    message.UserId = answerId;
                    User user = bot.GetApi().Users.Get(Convert.ToInt32(userId[i]));
                    Functions.SendMessage(bot, message, "К сожалению, я не могу добавить в чат \n" + user.FirstName + " " + user.LastName + "\n" +
                                                   "Такие дела ¯\\_(ツ)_/¯.", message.ChatId != null);
                    continue;
                }

                chat.usersId.Add(message.UserId.Value, count);
                ++count;
            }
            message.UserId = answerId;

            if (chat.UsersCount > 1)
            {
                #region Замещение чата
                if (Chats.ContainsKey(chatTitle)) Chats.Remove(chatTitle);
                #endregion
                Chats.Add(chatTitle, chat);
                Functions.SendMessage(bot, message, "Чат " + chatTitle + " успешно создан!", message.ChatId != null);

                Serialize();
            }
            else Functions.SendMessage(bot, message, "Чат " + chatTitle + " не удалось создать ¯\\_(ツ)_/¯.", message.ChatId != null);
        }

        public static void ChatSend(Message message, string chatTitle, string mesBody, Bot bot) 
        {
            Functions.RemoveSpaces(ref chatTitle);
            if (!Chats.ContainsKey(chatTitle) || Chats[chatTitle].usersId.ContainsKey(message.UserId.Value) == false)
            {
                Functions.SendMessage(bot, message, "Вы не состоите в чате с таким названием\n" +
                                     "¯\\_(ツ)_/¯.", message.ChatId != null);
                return;
            }
            long answerId = message.UserId.Value;

            MessagesSendParams sendParams = new MessagesSendParams();

            if (mesBody[0] == '!') sendParams = SpeechText.MakeSpeechAttachment(mesBody.Substring(1), message, bot);
            KeyValuePair<long, uint>[] usersId = Chats[chatTitle].usersId.Where(t => t.Key != message.UserId.Value).ToArray();
            for (int i = 0; i < usersId.Length; i++)
            {
                try
                {
                    Message m = new Message()
                    {
                        UserId = usersId[i].Key,
                        Attachments = message.Attachments
                    };
                    if(mesBody[0] != '!')
                        Functions.SendMessage(bot, m, "(" + chatTitle + ") " + Chats[chatTitle].usersId[message.UserId.Value] + " аноним: " + mesBody, m.ChatId != null, true);
                    else
                        Functions.SendMessage(bot, m, sendParams, "(" + chatTitle + ") " + Chats[chatTitle].usersId[message.UserId.Value] + " аноним: [аудиоосообщение]", m.ChatId != null, true);
                }
                catch (Exception ex)
                {
                    User user = bot.GetApi().Users.Get(usersId[i].Value);
                    Functions.SendMessage(bot, message, "Не удалось доставить сообщение " +
                                                   usersId[i].Value + " анониму ¯\\_(ツ)_/¯.", message.ChatId != null);
                }
                //Thread.Sleep(bot.GetSettings().Delay);
            }
            Functions.SendMessage(bot, message, "Отправка завершена.", message.ChatId != null);

        }

        public AnonChat()
        {
            AddMyCommandInPlatform();

            FileInfo data = new FileInfo(DATA_FILE);
            if (!data.Exists)
            {
                File.Create(DATA_FILE);
            }

            Deserialize();
            if(Chats == null) Chats = new SortedDictionary<string, ChatList>();
        }

        [Serializable]
        struct ChatList
        {
            public string Title;
            public SortedDictionary<long, uint> usersId;

            public int UsersCount
            {
                get
                {
                    return this.usersId.Count;
                }
            }


            public ChatList(string title)
            {
                this.Title   = title;
                this.usersId = new SortedDictionary<long,uint>();
            }

        }

        static void Serialize()
        { 
            FileStream fs = new FileStream(DATA_FILE, FileMode.Create);

            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, Chats);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("[SYSTEM][ERROR]:\n" +
                                  "Ошибка при сериализации : " + e.Message);
            }
            finally
            {
                fs.Close();
            }
        }

        static void Deserialize()
        {
           
            FileStream fs = new FileStream(DATA_FILE, FileMode.Open);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();

                if (Chats != null) Chats.Clear();
                Chats = (SortedDictionary<string, ChatList>)formatter.Deserialize(fs);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("[SYSTEM][ERROR]:\n" +
                                  "Ошибка при десериализации чата: " + e.Message);
            }
            finally
            {
                fs.Close();
            }

        }

        public bool NeedCommandInfo(Message message, string args, Bot bot)
        {
            string info = $"Справка по команде \"{message.Body}\":\n\n" +

                $"Команда создаёт анонимный чат из не более, чем {countOfUsers} пользователей, не считая создателя.\n" +
                $"Имена всех пользователей в чате будут скрыты и заменены на \"1 аноним\", \"2 аноним\" и тд.\n\n" +
                "Для того, чтобы создать анонимный чат, укажите в параметрах название чата в кавычках (\" \") и ссылки на пользователей. Все параметры отделяются запятыми.\n" +
                $"Пример: {bot.GetSettings().BotName[0]}, {message.Body}(\"Тестовый чат\", https://vk.com/id1, https://vk.com/hello_bot) - если операция пройдёт успешно, создаётся чат \"Тестовый чат\", в который можно отправлять сообщения.\n\n" +
                $"Для того, чтобы посмотреть список анонимных чатов, в которых вы состоите - напишите {bot.GetSettings().BotName[0]}, мои чаты - в данном примере бот выведет:\n" +
                $"Список чатов:\n1) \"Тестовый чат\"\n\n" +
                $"Для того, чтобы отправить сообщение в чат, напишите {bot.GetSettings().BotName[0]}, анонимно(название чата в кавычках, текст сообщения)\n" +
                $"Пример: {bot.GetSettings().BotName[0]}, анонимно(\"Тестовый чат\", всем приветик)\n" +
                $"Данное сообщение отправится всем участникам этого чата (каждому пользователю будет указано, из какого чата пришло сообщение)\n\n" +
                $"Для того, чтобы отправить аудиосообщение в чат, напишите { bot.GetSettings().BotName[0]}, анонимно(название чата в кавычках, !текст сообщения) - поставьте ! перед текстом сообщения.\n" +
                $"Пример: {bot.GetSettings().BotName[0]}, анонимно(\"Тестовый чат\", !всем приветик)\n\n" +
                $"ВНИМАНИЕ! Если сообщение было успешно доставлено, бот ответит вам \"Доставлено!\"";
                
            
            if (args == null || String.IsNullOrEmpty(args) || String.IsNullOrWhiteSpace(args))
            {
                Functions.SendMessage(bot, message, info, message.ChatId != null);
                return true;
            }
            return false;
        }
    }
}

/*
 * void StartChat(Message message, string param, Bot bot)
        {
            Message ansMessage = new Message()
            {
                ChatId =  message.ChatId,
                UserId =  message.UserId,
            };
            if (NeedCommandInfo(message, param, bot)) return;

            string[] args = param.Split(new char[1] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries);
            string chatTitle = Functions.RemoveSpaces(args[0]);
            string[] userId  = args[1].Split(new char[1] { ',' }, countOfUsers, StringSplitOptions.RemoveEmptyEntries);

            long? answerId = message.UserId;

            if (Chats.ContainsKey(chatTitle)){
                if (!Chats[chatTitle].usersId.ContainsKey(answerId.Value) || Chats[chatTitle].usersId[answerId.Value] != 1)
                {

                    Functions.SendMessage(bot, message, "Чат " + chatTitle + " уже был создан, или у вас недостаточно прав для создания чата с таким названием\n" +
                                                   "¯\\_(ツ)_/¯", message.ChatId != null);
                    return;
                }
            }
            if (userId.Length > countOfUsers)
            {
                Functions.SendMessage(bot, message, "Добавьте в беседу не более, чем " + countOfUsers + " пользователей.", message.ChatId != null);
                return;
            }
            ChatList chat = new ChatList(chatTitle);

            try
            {
                Functions.SendMessage(bot, message, description.Replace("[TITLE]", chatTitle) + "\nВы создатель данной беседы.\nПопытка добавить других собеседников...", message.ChatId != null);
            }
            catch (VkNet.Exception.VkApiException ex)
            {
                Functions.SendMessage(bot, message, "К сожалению, я не могу добавить вас чат \n" +
                                               "Такие дела ¯\\_(ツ)_/¯.\n" +
                                               "Чат " + chatTitle + " не удалось создать.", message.ChatId != null);
                return;
            }

            chat.usersId.Add(message.UserId.Value, 1);

            int count = 2;
            Parallel.For(0, userId.Length, i =>
            {
                int index = userId[i].ToString().LastIndexOf('/');
                if (index != -1) userId[i] = userId[i].ToString().Substring(index + 1);

                Regex reg = new Regex("[а-я|А-Я|a-z|A-Z]");
                bool regexIsFoundInFull = reg.IsMatch(userId[i]);

                //Если нашёл "id"
                userId[i] = userId[i].Replace(" ", "");

                index = userId[i].ToString().IndexOf("id");
                if (index == 0)
                {
                    if (!reg.IsMatch(userId[i].ToString().Substring(2)))
                    {
                        userId[i] = userId[i].ToString().Substring(2);
                    }
                    else
                    {
                        userId[i] = bot.GetApi().Users.Get(userId[i]).Id.ToString();
                    }
                }

                else if (reg.IsMatch(userId[i]))
                {
                    try
                    {
                        userId[i] = Functions.GetUserId(userId[i], bot);
                    }
                    catch (BotPlatformException ex)
                    {
                        throw;
                    }
                    catch
                    {
                        Functions.SendMessage(bot, message,
                            "Неверная ссылка: \"" + userId[i] + "\" не является пользователем ¯\\_(ツ)_/¯.",
                            message.ChatId != null);
                        return;
                    }
                }

                message.UserId = Convert.ToInt32(userId[i]);
                if (chat.usersId.ContainsKey(message.UserId.Value)) return;

                try
                {
                    Functions.SendMessage(bot, message, description.Replace("[TITLE]", chatTitle));
                }
                catch (Exception ex)
                {
                    User user = null;
                    if (bot is GroupBot)
                    {
                        if (BOT_API.Bots.ContainsKey(BOT_API.MainBot))
                            user = BOT_API.Bots[BOT_API.MainBot].GetApi().Users.Get(Convert.ToInt32(userId[i]));
                        else
                        {
                            Functions.SendMessage(bot, ansMessage,
                                $"К сожалению, я не могу добавить в чат id{userId[i]}\n" +
                                "Такие дела ¯\\_(ツ)_/¯.", ansMessage.ChatId != null);
                            return;
                        }
                    }

                    Functions.SendMessage(bot, ansMessage, "К сожалению, я не могу добавить в чат \n" + user.FirstName +
                                                        " " + user.LastName + "\n" +
                                                        "Такие дела ¯\\_(ツ)_/¯.", ansMessage.ChatId != null);
                    return;
                }

                lock(lockObject){chat.usersId.Add(message.UserId.Value, (uint)count);}
                Interlocked.Increment(ref count);
            });
            message.UserId = ansMessage.UserId;

            if (chat.UsersCount > 1)
            {
                #region Замещение чата
                if(Chats.ContainsKey(chatTitle)) Chats.Remove(chatTitle);
                #endregion
                Chats.Add(chatTitle, chat);
                 Functions.SendMessage(bot, message, "Чат " + chatTitle + " успешно создан!", message.ChatId != null);

                Serialize();
            }
            else Functions.SendMessage(bot, message, "Чат " + chatTitle + " не удалось создать ¯\\_(ツ)_/¯.", message.ChatId != null);
        }
 */
