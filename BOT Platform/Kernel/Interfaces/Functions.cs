using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        
        public static void SendMessage(Message message, MessagesSendParams m, string body, bool isChat = false)
        {
            m.Message = body;
            if (isChat == true)
            {
                m.ChatId = message.ChatId;
                m.ForwardMessages = new long[1] { message.Id.Value };
            }
            else m.UserId = message.UserId;

            if (BOT_API.GetSettings().IsDebug == false) BOT_API.GetApi().Messages.Send(m);
            else Console.WriteLine(m.Message);
        }
        public static void SendMessage(Message m, string message, bool isChat = false)
        {
            MessagesSendParams msp = new MessagesSendParams()
            {
                Message = message,
                Attachments = m.Attachments as IEnumerable<MediaAttachment>
            };

            if (isChat == true)
            {
                msp.ChatId = m.ChatId;
                msp.ForwardMessages = new long[1] { m.Id.Value };
            }
            else msp.UserId = m.UserId;

            if (BOT_API.GetSettings().IsDebug == false) BOT_API.GetApi().Messages.Send(msp);
            else Console.WriteLine(msp.Message);
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
        public static void GetUserId(ref string url)
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
                    url = BOT_API.GetApi().Users.Get(url).Id.ToString();
                }
            }

            else if (reg.IsMatch(url))
            {
                url = BOT_API.GetApi().Users.Get(url).Id.ToString();
            }
        }
        public static string GetUserId(string url)
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
                    url = BOT_API.GetApi().Users.Get(url).Id.ToString();
                }
            }

            else if (reg.IsMatch(url))
            {
                url = BOT_API.GetApi().Users.Get(url).Id.ToString();
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
        public static Photo UploadImageInMessage(string photoName)
        {
            var uploadServer = BOT_API.GetApi().Photo.GetMessagesUploadServer();
            // Загрузить фотографию.
            var wc = new WebClient();
            var responseImg = Encoding.ASCII.GetString(wc.UploadFile(uploadServer.UploadUrl, photoName));
            // Сохранить загруженную фотографию
            return BOT_API.GetApi().Photo.SaveMessagesPhoto(responseImg)[0];
        }

        public static Document UploadDocumentInMessage(string filename, string docName)
        {
            var uploadServer = BOT_API.GetApi().Docs.GetWallUploadServer();
            // Загрузить документ.
            var wc = new WebClient();
            var responseDoc = Encoding.ASCII.GetString(wc.UploadFile(uploadServer.UploadUrl, filename));
            // Сохранить загруженный документ
            return BOT_API.GetApi().Docs.Save(responseDoc, docName)[0];
        }
    }
}
