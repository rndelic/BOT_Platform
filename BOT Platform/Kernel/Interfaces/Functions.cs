using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MyFunctions.Exceptions;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

namespace BOT_Platform.Interfaces
{
    static class Functions
    {
        public static void RemoveSpaces(ref string str)
        {
            //удаляем пробелы с начала
            while (!string.IsNullOrEmpty(str) && str[0] == ' ')
            {
                str = str.Remove(0, 1);
            }

            if (string.IsNullOrEmpty(str)) return;
            //удаляем пробелы с конца
            int j;
            for (j = str.Length - 1; str[j] == ' '; j--) { }
            if (j != str.Length - 1) str = str.Remove(j + 1);

        }
        public static string RemoveSpaces(string str)
        {
            //удаляем пробелы с начала
            while (!string.IsNullOrEmpty(str) && str[0] == ' ')
            {
                str = str.Remove(0, 1);
            }

            if (string.IsNullOrEmpty(str)) return str;
            //удаляем пробелы с конца
            int j;
            for (j = str.Length - 1; str[j] == ' '; j--) { }
            if (j != str.Length - 1) str = str.Remove(j + 1);

            return str;
        }
        
        public static void SendMessage(Bot bot, Message message, MessagesSendParams m, 
            string body, bool isChat = false, bool needAttachments = false)
        {

            m.Message = body;
            if (isChat == true)
            {
                m.ChatId = message.ChatId;
                m.ForwardMessages = new long[1] { message.Id.Value };
                //m.Attachments = needAttachments ? GetAttachments(message) : null;
            }
            else m.UserId = message.UserId;

            if (bot.GetSettings().GetIsDebug() == false)
            {
                if (bot is GroupBot && BOT_API.Bots.ContainsKey(BOT_API.MainBot))
                {
                    try
                    {
                        bot.GetApi().Messages.Send(m);
                    }
                    catch
                    {
                        SendMessage(BOT_API.Bots[BOT_API.MainBot], message, m, body, isChat, needAttachments);
                    }
                }
                else bot.GetApi().Messages.Send(m);
            }
            else Console.WriteLine($"(ответ){bot.Name}: " +  m.Message);
        }

        private static List<MediaAttachment> GetAttachments(Message m)
        {
            List<MediaAttachment> mediaAttachments = new List<MediaAttachment>();
            foreach (var attach in m.Attachments)
            {
                MediaAttachment mAt;
                if (attach.Instance is Photo)
                {
                    mAt = (attach.Instance as Photo);
                    mediaAttachments.Add(mAt);
                }
                else if (attach.Instance is Audio)
                {
                    mAt = (attach.Instance as Audio);
                    mediaAttachments.Add(mAt);
                }
                else if (attach.Instance is Document)
                {
                    mAt = (attach.Instance as Document);
                    mediaAttachments.Add(mAt);
                }
                else if (attach.Instance is Sticker)
                {
                    mAt = (attach.Instance as Sticker);
                    mediaAttachments.Add(mAt);
                }
                else if (attach.Instance is Video)
                {
                    mAt = (attach.Instance as Video);
                    mediaAttachments.Add(mAt);
                }
                else if (attach.Instance is Link)
                {
                    mAt = (attach.Instance as Link);
                    mediaAttachments.Add(mAt);
                }
                else if (attach.Instance is Poll)
                {
                    mAt = (attach.Instance as Poll);
                    mediaAttachments.Add(mAt);
                }
                else if (attach.Instance is Wall)
                {
                    mAt = (attach.Instance as Wall);
                    mediaAttachments.Add(mAt);
                }
                else if (attach.Instance is WallReply)
                {
                    mAt = (attach.Instance as WallReply);
                    mediaAttachments.Add(mAt);
                }

            }
            return mediaAttachments;
        }
        public static void SendMessage(Bot bot, Message m,
            string message, bool isChat = false, bool needAttachments = false)
        {

            MessagesSendParams msp = new MessagesSendParams()
            { 
                Message = message,
                Attachments = needAttachments ? GetAttachments(m) : null
            };

            if (isChat)
            {
                msp.ChatId = m.ChatId;
                msp.ForwardMessages = new long[1] { m.Id.Value };
            }
            else msp.UserId = m.UserId;

            if (bot.GetSettings().GetIsDebug() == false)
            {
                if (bot is GroupBot && BOT_API.Bots.ContainsKey(BOT_API.MainBot))
                {
                    try
                    {
                        bot.GetApi().Messages.Send(msp);
                    }
                    catch
                    {
                        SendMessage(BOT_API.Bots[BOT_API.MainBot], m, message, isChat, needAttachments);
                    }
                }
                else bot.GetApi().Messages.Send(msp);
            }
            else Console.WriteLine($"(ответ){bot.Name}: " + msp.Message);
        }
        public static bool ContainsMessage(Message containMes, IEnumerable<Message> Messages)
        {
            bool contains = false;
            foreach (Message m in Messages)
            {
                if (m.Date == containMes.Date &&
                   m.UserId == containMes.UserId &&
                   m.Body == containMes.Body)
                {
                    contains = true;
                    break;
                }
            }
            return contains; 
        }
        public static void GetUserId(ref string url, Bot bot)
        {
            url = url.Replace(" ", "");
            int index = url.ToString().LastIndexOf('/');
            if (index != -1) url = url.ToString().Substring(index + 1);

            Regex reg = new Regex("[a-z|A-Z]");
            bool regexIsFoundInFull = reg.IsMatch(url);

            //Если нашёл "id"
            index = url.ToString().IndexOf("id");
            if (index == 0)
            {
                if (!reg.IsMatch(url.ToString().Substring(2)))
                {
                    url = url.ToString().Substring(2);
                }
                else
                {
                    url = bot.GetApi().Users.Get(url).Id.ToString();
                }
            }

            else if (reg.IsMatch(url))
            {
                if (bot is GroupBot)
                {
                    if (BOT_API.Bots.ContainsKey(BOT_API.MainBot))
                        url = BOT_API.Bots[BOT_API.MainBot].GetApi().Users.Get(url).Id.ToString();
                    else throw new WrongParamsException("В данный момент бот не может отправлять сообщения по короткой ссылке пользователя." +
                                                        "\nПожалуйста, укажите вместо ссылки численное значение без \"id\"");
                }

                else url = bot.GetApi().Users.Get(url).Id.ToString();
            }
        }
        public static string GetUserId(string url, Bot bot)
        {
            url = url.Replace(" ", "");
            int index = url.ToString().LastIndexOf('/');
            if (index != -1) url = url.ToString().Substring(index + 1);

            Regex reg = new Regex("[a-z|A-Z]");
            bool regexIsFoundInFull = reg.IsMatch(url);

            //Если нашёл "id"
            index = url.ToString().IndexOf("id");
            if (index == 0)
            {
                if (!reg.IsMatch(url.ToString().Substring(2)))
                {
                    url = url.ToString().Substring(2);
                }
                else
                {
                    url = bot.GetApi().Users.Get(url).Id.ToString();
                }
            }

            else if (reg.IsMatch(url))
            {
                if (bot is GroupBot)
                {
                    if (BOT_API.Bots.ContainsKey(BOT_API.MainBot))
                        url = BOT_API.Bots[BOT_API.MainBot].GetApi().Users.Get(url).Id.ToString();
                    else throw new WrongParamsException("В данный момент бот не может отправлять сообщения по короткой ссылке пользователя." +
                                                        "\nПожалуйста, укажите вместо ссылки численное значение без \"id\"");
                }

                else url = bot.GetApi().Users.Get(url).Id.ToString();
            }
            return url;
        }
        public static void CheckURL(ref string s)
        {
           s = s.Replace(" ", "%20")
                .Replace("&", "%26")
                .Replace("#", "%23")
                .Replace("\\", "%26%23092%3B")
                .Replace(",", "%2C")
                .Replace("/", "%2F");
        }
        public static string CheckURL(string s)
        {
            return s.Replace(" ", "%20")
                 .Replace("&", "%26")
                 .Replace("#", "%23")
                 .Replace("\\", "%26%23092%3B")
                 .Replace(",", "%2C")
                 .Replace("/", "%2F");
        }
        public static Photo UploadImageInMessage(string photoName, Bot bot)
        {
            var uploadServer = bot.GetApi().Photo.GetMessagesUploadServer();
            // Загрузить фотографию.
            var wc = new WebClient();
            var responseImg = Encoding.ASCII.GetString(wc.UploadFile(uploadServer.UploadUrl, photoName));
            // Сохранить загруженную фотографию
            return bot.GetApi().Photo.SaveMessagesPhoto(responseImg)[0];
        }

        public static Document UploadDocumentInMessage(string filename, string docName, Bot bot)
        {
            UploadServerInfo uploadServer;

            bot = bot is GroupBot && BOT_API.Bots.ContainsKey(BOT_API.MainBot) ?
                BOT_API.Bots[BOT_API.MainBot] : bot;

            if (bot is GroupBot) uploadServer = bot.GetApi().Docs.GetWallUploadServer();
            else uploadServer = bot.GetApi().Docs.GetUploadServer();
            // Загрузить документ.
            var wc = new WebClient();
            var responseDoc = Encoding.ASCII.GetString(wc.UploadFile(uploadServer.UploadUrl, filename));
            // Сохранить загруженный документ
            return bot.GetApi().Docs.Save(responseDoc, docName)[0];
        }
    }
}
