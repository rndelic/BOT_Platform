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
using System.Linq;
using MyFunctions.Exceptions;
using BOT_Platform.Kernel;
using BOT_Platform.Kernel.Bots;
using BOT_Platform.Kernel.Interfaces;

namespace MyFunctions
{
    class SpeechText: IMyCommands
    {
        private const string DIRECTORY_PATH = @"Data\SpeechText";
        public const string NAME = "mmBOT";

        const string ENDING = ". ахахах этот бот лучше всех";

        public void AddMyCommandInPlatform()
        { 
            CommandsList.TryAddCommand("произнеси", new MyCommandStruct("Возвращает аудиозапись с озвученным текстом", Speech));
        }

        public static void Speech(Message message, string args, Bot bot)
        {
            if (NeedCommandInfoStatic(message, args, bot)) return;

            MessagesSendParams param = MakeSpeechAttachment(args, message, bot);

            Functions.SendMessage(bot, message, param,"", message.ChatId != null);

        }

        public static MessagesSendParams MakeSpeechAttachment(string text, Message message, Bot bot)
        {
            SpeechSynthesizer speechSynth = new SpeechSynthesizer
            {
                Volume = 100,
                Rate = 2
            }; // создаём объект
            // устанавливаем уровень звука
            //speechSynth.SelectVoice("Microsoft Pavel Mobile");
            Functions.RemoveSpaces(ref text);
            string outFilename = String.Format(@"Data\SpeechText\{0}.waw", Guid.NewGuid());
            try
            {
                speechSynth.SetOutputToWaveFile(outFilename,
                                            new SpeechAudioFormatInfo(16000, AudioBitsPerSample.Sixteen, AudioChannel.Mono));
            }
            catch
            {
                speechSynth.SetOutputToNull();
                throw;
            }

            if (text[0] != '@')
                speechSynth.Speak(text + ENDING); // озвучиваем переданный текст
            else
            {
                text = text.Substring(1);
                speechSynth.Speak(text);
            }
            
            speechSynth.SetOutputToNull();

            string outFilenameMP3 = ConverWavToMp3(outFilename);
            File.Delete(outFilename);

            List<Audio> audioList = new List<Audio>();
            try
            {
                audioList.Add(UploadAndSave(text, outFilenameMP3, bot));
            }
            catch (Exception ex)
            {
                if (ex.Message == "Access denied: filename is incorrect")
                    throw new WrongParamsException("Текст аудиосообщения слишком короткий!");
                else throw;
            }
            finally
            {
                File.Delete(outFilenameMP3);
            }

            MessagesSendParams param = new MessagesSendParams
            { Attachments = new ReadOnlyCollection<Audio>(audioList)};

            return param;
        }
        static Audio UploadAndSave(string title, string outFilenameMP3, Bot BOT)
        {
            Bot bot = Program.Bots.ContainsKey(Program.MainBot) ? Program.Bots[Program.MainBot] : BOT;
            Uri uploadServer = bot.GetApi().Audio.GetUploadServer();

            var wc = new WebClient();
            var response = Encoding.Default.GetString(wc.UploadFile(uploadServer.AbsoluteUri,outFilenameMP3));

            Audio savedAudio;
            string t = title.Substring(0, title.Length > 50 ? 50 : title.Length);
            savedAudio = bot.GetApi().Audio.Save(response, NAME, t);

            if (title.Length > 50)
            {
                bot.GetApi().Audio.Edit(savedAudio.Id.Value, bot.GetApi().UserId.Value, NAME, t , title);
            }

            return savedAudio;
        }


        public SpeechText()
        {
            AddMyCommandInPlatform();
            if (!Directory.Exists(DIRECTORY_PATH)) Directory.CreateDirectory(DIRECTORY_PATH);
        }

        static string ConverWavToMp3(string outFilename)
        {
            WaveStream InStr = new WaveStream(outFilename);
            string outFilenameMP3 = String.Format(@"Data\SpeechText\{0}.mp3", Guid.NewGuid()); 
            try
            {
                
                Mp3Writer writer = new Mp3Writer(new FileStream(outFilenameMP3,
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
            return outFilenameMP3;
        }

        public static bool NeedCommandInfoStatic(Message message, string args, Bot bot)
        {
            string info = $"Справка по команде \"{message.Body}\":\n\n" +
                "Команда голосом бота озвучивает указанный в скобках текст.\n\n" +
                $"Пример: {bot.GetSettings().BotName[0]}, {message.Body}(я тебя люблю, не знаю почему) - бот озвучит и отправит аудиозапись с озвученным текстом.\n\n" +
                $"Данная функция реализована с помощью The Microsoft Cognitive Toolkit (Speech)";

            if (args == null || String.IsNullOrEmpty(args) || String.IsNullOrWhiteSpace(args))
            {
                Functions.SendMessage(bot, message, info, message.ChatId != null);
                return true;
            }
            return false;
        }

        bool IMyCommands.NeedCommandInfo(Message message, string args, Bot bot)
        {
            return NeedCommandInfoStatic(message, args, bot);
        }
    }
}
