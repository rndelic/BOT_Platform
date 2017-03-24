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

namespace MyFunctions
{
    class SpeechText: IMyCommands
    {
        const string FILENAME    = "file.wav";
        const string FILENAMEMP3 = "file.mp3";
        const string NAME        = "mmBOT";

        const string ENDING = ". ахахах этот бот лучше всех";

        public void AddMyCommandInPlatform()
        { 
            CommandsList.TryAddCommand("произнеси", new MyComandStruct("Возвращает аудиозапись с озвученным текстом", Speech));

        }


        void Speech(Message message, params object[] p)
        {
         
            SpeechSynthesizer speechSynth = new SpeechSynthesizer(); // создаём объект
            speechSynth.Volume = 100; // устанавливаем уровень звука
            speechSynth.Rate   = 2;

            speechSynth.SetOutputToWaveFile(FILENAME, 
                                        new SpeechAudioFormatInfo(16000, AudioBitsPerSample.Sixteen, AudioChannel.Mono));
            //speechSynth.SelectVoice("Microsoft Pavel Mobile");

            speechSynth.Speak(p[0].ToString() + ENDING); // озвучиваем переданный текст
            speechSynth.SetOutputToNull();

            ConverWavToMp3();

            List<Audio> audioList = new List<Audio>();
            audioList.Add(UploadAndSave(p[0].ToString()));

            MessagesSendParams param = new MessagesSendParams();
            param.Attachments = new ReadOnlyCollection<Audio>(audioList);

            Functions.SendMessage(message, param,"", message.ChatId != null);

        }
        Audio UploadAndSave(string title)
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

        void ConverWavToMp3()
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

    }
}
