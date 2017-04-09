using BOT_Platform;
using BOT_Platform.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using VkNet.Model;

namespace MyFunctions
{
    class WikiPedia : IMyCommands
    {
        public void AddMyCommandInPlatform()
        {
            CommandsList.TryAddCommand("вики", new MyComandStruct("Находит статью в википедии по запросу", FindWiki));
        }

        private void FindWiki(Message message, object[] p)
        {
            if (NeedCommandInfo(message, p) == true) return;

            string request = Functions.CheckURL(p[0].ToString());

            var webClient = new WebClient();
            string local = "ru";
            string http = ("http://[local].wikipedia.org/w/api.php?format=xml&action=query&prop=extracts&titles=" + request + "&redirects=true")
                .Replace("[local]", local);
            var pageSourceCode = Encoding.UTF8.GetString(webClient.DownloadData(http));

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(pageSourceCode);

            var fnode = xml.GetElementsByTagName("extract")[0];

            if (fnode == null)
            {
                Functions.SendMessage(message, "Не удалось найти статью по заданной теме! Проверьте правильность написания запроса :(\n\n" +
                    "Обратите внимание, что имена собственные рекомендуется писать с заглавной буквы.", message.ChatId != null);
                return;
            }

            string ss = fnode.InnerText;
            Regex regex = new Regex("\\<[^\\>]*\\>");

            String.Format("Before:{0}", ss);
            ss = regex.Replace(ss, string.Empty);

            if (ss.Length > 4096) ss = ss.Substring(0, 4000) + "...";
            ss += "\n\nПодробнее: " + "https://ru.wikipedia.org/wiki/" + request;

            Functions.SendMessage(message, ss, message.ChatId != null);

        }

        public bool NeedCommandInfo(Message message, params object[] p)
        {
            string info = $"Справка по команде \"{message.Body}\":\n\n" +
                "Команда находит статью в Википедии по заданному в скобках тексту.\n\n" +
                "Обратите внимание, что имена собственные рекомендуется писать с заглавной буквы.\n\n" +
                $"Пример: {BOT_API.platformSett.BotName[0]}, {message.Body}(Галилео Галилей) - бот найдёт и отправит статью о Галилее в Википедии";

            if (p[0] == null || String.IsNullOrEmpty(p[0].ToString()) || String.IsNullOrWhiteSpace(p[0].ToString()))
            {
                Functions.SendMessage(message, info , message.ChatId != null);
                return true;
            }
            return false;
        }

        public WikiPedia()
        {
            AddMyCommandInPlatform();
        }
    }
}