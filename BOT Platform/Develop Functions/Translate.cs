using BOT_Platform;
using System;
using System.Text;
using VkNet.Model;
using Google.API.Translate;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace MyFunctions
{
    class Translate : IMyCommands
    {
        public void AddMyCommandInPlatform()
        {
            CommandsList.TryAddCommand("перевод", new MyComandStruct("Переводит обьект (alfa).", TranslateIt, true));
        }

        private void TranslateIt(Message message, object[] p)
        {
            string[] param = p[0].ToString().Split(new char[] { ';' }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (p[0] == null || String.IsNullOrEmpty(p[0].ToString()) || String.IsNullOrWhiteSpace(p[0].ToString())|| p[0].ToString() == "!" || param.Length == 0)
            {
                Functions.SendMessage(message, "Я умею переводить пустые запросы на русский: \"НЕ ИСПЫТЫВАЙ МОЁ ТЕРПЕНИЕ\"", message.ChatId != null);
                return;
            }

            if(param.Length >= 1) TranslateText(message, param);

        }

        private void TranslateText(Message message, string[] param)
        {
            YandexTranslator YT = new YandexTranslator();
            string result = "";

            if (param.Length == 1)
            {
                Functions.RemoveSpaces(ref param[0]);
                if (param[0][0] == '!') result = YT.Translate(Functions.RemoveSpaces(param[0].Substring(1)), "ru");
                else                    result = YT.Translate(Functions.RemoveSpaces(param[0]), "en");
            }
            else result = YT.Translate(Functions.RemoveSpaces(param[1]), param[0]);
          
            Functions.SendMessage(message,"✎ " + result, message.ChatId != null);
        }

        class YandexTranslator
        {
            const string YANDEX_KEY = @"trnsl.1.1.20170329T112343Z.5eed7612877250b4.8fc25de1217ed7b944cc7feaa84a74f9ba3e52dd";
            public string Translate(string s, string lang)
            {
                if (s.Length > 0)
                {
                    WebRequest request = WebRequest.Create("https://translate.yandex.net/api/v1.5/tr.json/translate?key="
                        + YANDEX_KEY
                        + "&text=" + s
                        + "&lang=" + lang);

                    WebResponse response = request.GetResponse();

                    using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                    {
                        string line;

                        if ((line = stream.ReadLine()) != null)
                        {
                            Translation translation = JsonConvert.DeserializeObject<Translation>(line);

                            s = "";

                            foreach (string str in translation.text)
                            {
                                s += str;
                            }
                        }
                    }

                    return s;
                }
                else
                    return "[Ошибка] Ошибка перевода";
            }
        }

        struct Translation
        {
            public string code { get; set; }
            public string lang { get; set; }
            public string[] text { get; set; }
        }
        public Translate()
        {
            AddMyCommandInPlatform();
        }
    }
}