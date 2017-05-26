﻿using BOT_Platform;
using System;
using System.Text;
using VkNet.Model;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using VkNet.Model.RequestParams;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using VkNet.Model.Attachments;
using BOT_Platform.Interfaces;

namespace MyFunctions
{
    class Translate : IMyCommands
    {
        public void AddMyCommandInPlatform()
        {
            CommandsList.TryAddCommand("перевод", new MyComandStruct("Переводит обьект (alfa).", TranslateIt, true));
        }

        private void TranslateIt(Message message, string args, Bot bot)
        {
            if (NeedCommandInfo(message, args, bot)) return;
            string[] param = args.Split(new char[] { ';' }, 2, StringSplitOptions.RemoveEmptyEntries);

            Regex regex = new Regex("((\\W)*(\\d)*)*");

            if (regex.Match(args).Value == args)
            {
                Functions.SendMessage(bot, message, "Я умею переводить пустые запросы на русский: \n\"НЕ ИСПЫТЫВАЙ МОЁ ТЕРПЕНИЕ\"", message.ChatId != null);
                return;
            }

            if(param.Length >= 1) TranslateText(message, bot, param);

        }

        private void TranslateText(Message message, Bot bot, string[] param)
        {
            YandexTranslator YT = new YandexTranslator();
            string result = "";

            if (param.Length == 1)
            {
                Functions.RemoveSpaces(ref param[0]);
                result = param[0][0] == '!' ? YT.Translate(Functions.RemoveSpaces(param[0].Substring(1)), "ru") :
                                              YT.Translate(Functions.RemoveSpaces(param[0]), "en");
            }
            else result = YT.Translate(Functions.RemoveSpaces(param[1]), param[0]);
          
            Functions.SendMessage(bot, message,"✎ " + result, message.ChatId != null);
        }


        public bool NeedCommandInfo(Message message, string args, Bot bot)
        {
            string info = $"Справка по команде \"{message.Body}\":\n\n" +

                "Команда переводит указанный в скобках текст.\n" +
                "Для того, чтобы перевести текст с любого языка на английский - просто напишите текст в скобочках.\n" +
                $"Пример: {bot.GetSettings().BotName[0]}, {message.Body}(привет) - бот напишет \"hello\".\n\n" +
                "Для того, чтобы перевести текст с любого языка на русский - поставьте перед текстом !\n" +
                $"Пример: {bot.GetSettings().BotName[0]}, {message.Body}(!hello) - бот напишет \"привет\"\n\n" +
                "Для того, чтобы перевести текст на любой язык - укажите в скобках код языка и поставьте ; а затем напишите текст.\n" +
                $"Пример для украинского языка (uk): {bot.GetSettings().BotName[0]}, {message.Body}(uk; привет) - бот напишет \"привiт\"\n\n" +
                "Коды языков описаны в прикреплённых к этому сообщению скринах.\n\n" +
                "Данная функция реализована с помощью Yandex.Translate API";

            List<Photo> photoList = new List<Photo>();
            photoList.Add(Functions.UploadImageInMessage(Environment.CurrentDirectory + "\\Data\\TranslateLanguages\\1.png", bot));
            photoList.Add(Functions.UploadImageInMessage(Environment.CurrentDirectory + "\\Data\\TranslateLanguages\\2.png", bot));

            MessagesSendParams param = new MessagesSendParams();
            param.Attachments = new ReadOnlyCollection<Photo>(photoList);

            if (args == null || String.IsNullOrEmpty(args) || String.IsNullOrWhiteSpace(args))
            {
                Functions.SendMessage(bot, message, param, info, message.ChatId != null);
                return true;
            }
            return false;
            
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