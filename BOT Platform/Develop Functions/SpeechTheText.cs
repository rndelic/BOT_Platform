using System;
using BOT_Platform;
using VkNet.Model;
using VkNet.Model.RequestParams;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;
using System.Net;
using System.Text;
using VkNet.Model.Attachments;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using WaveLib;
using Yeti.Lame;
using Yeti.MMedia.Mp3;
using System.IO;
using MyFunctions.Exceptions;

namespace MyFunctions
{
    class SpeechText: IMyCommands
    {
        const string FILENAME    = "file.wav";
        const string FILENAMEMP3 = "file.mp3";
        public const string NAME        = "mmBOT";

        const string ENDING = ". ахахах этот бот лучше всех";

        public void AddMyCommandInPlatform()
        { 
            CommandsList.TryAddCommand("произнеси", new MyComandStruct("Возвращает аудиозапись с озвученным текстом", Speech));

        }

        
        public static void Speech(Message message, params object[] p)
        {
            if (NeedCommandInfo1(message, p)) return;

            MessagesSendParams param = MakeSpeechAttachment(p[0].ToString(), message);

            Functions.SendMessage(message, param,"", message.ChatId != null);

        }

        public static MessagesSendParams MakeSpeechAttachment(string text, Message message)
        {
            SpeechSynthesizer speechSynth = new SpeechSynthesizer(); // создаём объект
            speechSynth.Volume = 100; // устанавливаем уровень звука
            speechSynth.Rate = 2;
            //speechSynth.SelectVoice("Microsoft Pavel Mobile");
            Functions.RemoveSpaces(ref text);
            speechSynth.SetOutputToWaveFile(FILENAME,
                                        new SpeechAudioFormatInfo(16000, AudioBitsPerSample.Sixteen, AudioChannel.Mono));

            if (text[0] != '@')
                speechSynth.Speak(text + ENDING); // озвучиваем переданный текст
            else
            {
                if(text.Length - 1 <= 12)
                {
                    //Functions.SendMessage(message, "Текст аудиосообщения слишком короткий!" , message.ChatId != null);
                    //return default(MessagesSendParams);
                    speechSynth.SetOutputToNull();
                    throw new WrongParamsException("Текст аудиосообщения слишком короткий!");
                }
                text = text.Substring(1);
                speechSynth.Speak(text);
            }
            speechSynth.SetOutputToNull();

            ConverWavToMp3();

            List<Audio> audioList = new List<Audio>();
            audioList.Add(UploadAndSave(text));

            MessagesSendParams param = new MessagesSendParams();
            param.Attachments = new ReadOnlyCollection<Audio>(audioList);

            return param;
        }
        static Audio UploadAndSave(string title)
        {
            Uri uploadServer = BOT_API.app.Audio.GetUploadServer();

            var wc = new WebClient();
            var response = Encoding.Default.GetString(wc.UploadFile(uploadServer.AbsoluteUri,FILENAMEMP3));

            Audio savedAudio;
            string t = title.Substring(0, title.Length > 50 ? 50 : title.Length);
            savedAudio = BOT_API.app.Audio.Save(response, NAME, t);

            if (title.Length > 50)
            {
                BOT_API.app.Audio.Edit(savedAudio.Id.Value, BOT_API.app.UserId.Value, NAME, t , title);
            }

            return savedAudio;
        }


        public SpeechText()
        {
            AddMyCommandInPlatform();
        }

        static void ConverWavToMp3()
        {
            WaveStream InStr = new WaveStream(FILENAME);
            try
            {
                Mp3Writer writer = new Mp3Writer(new FileStream(FILENAMEMP3,
                                                    FileMode.Create), InStr.Format);
                try
                {
                    byte[] buff = new byte[writer.OptimalBufferSize];
                    int read = 0;
                    while ((read = InStr.Read(buff, 0, buff.Length)) > 0)
                    {
                        writer.Write(buff, 0, read);
                    }
                }
                finally
                {
                    writer.Close();
                }
            }
            finally
            {
                InStr.Close();
            }
        }

        public static bool NeedCommandInfo1(Message message, params object[] p)
        {
            string info = $"Справка по команде \"{message.Body}\":\n\n" +
                "Команда голосом бота озвучивает указанный в скобках текст.\n\n" +
                $"Пример: {BOT_API.platformSett.BotName[0]}, {message.Body}(я тебя люблю, не знаю почему) - бот озвучит и отправит аудиозапись с озвученным текстом.\n\n" +
                $"Данная функция реализована с помощью The Microsoft Cognitive Toolkit (Speech)";

            if (p[0] == null || String.IsNullOrEmpty(p[0].ToString()) || String.IsNullOrWhiteSpace(p[0].ToString()))
            {
                Functions.SendMessage(message, info, message.ChatId != null);
                return true;
            }
            return false;
        }

        public bool NeedCommandInfo(Message message, params object[] p)
        {
            throw new NotImplementedException();
        }
    }
}
