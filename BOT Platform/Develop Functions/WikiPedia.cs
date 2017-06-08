using BOT_Platform;
using BOT_Platform.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using BOT_Platform.Kernel.Bots;
using VkNet.Model;

namespace MyFunctions
{
    class WikiPedia : IMyCommands
    {
        public void AddMyCommandInPlatform()
        {
            CommandsList.TryAddCommand("вики", new MyComandStruct("Находит статью в википедии по запросу", FindWiki));
        }

        private void FindWiki(Message message, string args, Bot bot)
        {
            if (NeedCommandInfo(message, args, bot) == true) return;

            string request = Functions.CheckURL(args);

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
                Functions.SendMessage(bot, message, "Не удалось найти статью по заданной теме! Проверьте правильность написания запроса :(\n\n" +
                    "Обратите внимание, что имена собственные рекомендуется писать с заглавной буквы.", message.ChatId != null);
                return;
            }

            string ss = fnode.InnerText;
            Regex regex = new Regex("\\<[^\\>]*\\>");

            String.Format("Before:{0}", ss);
            ss = regex.Replace(ss, string.Empty);

            if (ss.Length > 4096) ss = ss.Substring(0, 4000) + "...";
            ss += "\n\nПодробнее: " + "https://ru.wikipedia.org/wiki/" + request;

            Functions.SendMessage(bot, message, ss, message.ChatId != null);

        }

        public bool NeedCommandInfo(Message message, string args, Bot bot)
        {
            string info = $"Справка по команде \"{message.Body}\":\n\n" +
                "Команда находит статью в Википедии по заданному в скобках тексту.\n\n" +
                "Обратите внимание, что имена собственные рекомендуется писать с заглавной буквы.\n\n" +
                $"Пример: {bot.GetSettings().BotName[0]}, {message.Body}(Галилео Галилей) - бот найдёт и отправит статью о Галилее в Википедии";

            if (args == null || String.IsNullOrEmpty(args) || String.IsNullOrWhiteSpace(args))
            {
                Functions.SendMessage(bot, message, info , message.ChatId != null);
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