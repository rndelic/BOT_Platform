using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gif.Components;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using BOT_Platform.Interfaces;
using VkNet.Model;
using BOT_Platform;
using System.Net;
using System.Threading;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using MyFunctions.Exceptions;

namespace MyFunctions
{
    class GifAnimation : IMyCommands
    {
        private string IN_FILENAME = "Data\\Gif\\photo";
        private const int MAX_DELAY_LIMIT = 5000;

        public GifAnimation()
        {
            AddMyCommandInPlatform();

        }

        public void AddMyCommandInPlatform()
        {
            CommandsList.TryAddCommand("гиф", new MyComandStruct("Делает гифку из прикреплённых изображений", MakeGif));
        }

        private void MakeGif(Message message, object[] p)
        {
            if (NeedCommandInfo(message, p)) return;

            if (message.Attachments.Count <= 1)
            {
                Functions.SendMessage(message, "Ну и из чего мне тебе гифку делать? Где картинки? Их должно быть не менее 2-х.", message.ChatId != null);
                return;
            }
            Regex reg = new Regex(@"\d{0," + MAX_DELAY_LIMIT.ToString().Length + "}");
            if (reg.Match(p[0].ToString()).Value != p[0].ToString() )
            {
                Functions.SendMessage(message, $"Введено недопустимое значение! Введите целое число от 0 до {MAX_DELAY_LIMIT}.", message.ChatId != null);
                return;
            }

            int maxCount = 0;
            Parallel.For(0, message.Attachments.Count, i =>
            {
                Photo photo = ((message.Attachments[i].Instance) as Photo);
                if (photo != null)
                {
                    WebClient webClient = new WebClient();
                    if (photo.Photo2560 != null) webClient.DownloadFile(photo.Photo2560, IN_FILENAME + i + ".png");
                    else if (photo.Photo1280 != null) webClient.DownloadFile(photo.Photo1280, IN_FILENAME + i + ".png");
                    else if (photo.Photo807 != null) webClient.DownloadFile(photo.Photo807, IN_FILENAME + i + ".png");
                    else if (photo.Photo604 != null) webClient.DownloadFile(photo.Photo604, IN_FILENAME + i + ".png");
                    else if (photo.Photo130 != null) webClient.DownloadFile(photo.Photo130, IN_FILENAME + i + ".png");
                    else if (photo.Photo75 != null) webClient.DownloadFile(photo.Photo75, IN_FILENAME + i + ".png");

                    Image img = Image.FromFile(IN_FILENAME + i + ".png");
                    SIZE_HEIGHT += img.Height;
                    SIZE_WIDTH += img.Width;
                    img.Dispose();
                    Interlocked.Increment(ref maxCount);
                }
            });
            ToGif(Convert.ToInt32(p[0]), maxCount);

            List<Document> docList = new List<Document>();
            docList.Add(Functions.UploadDocumentInMessage(OUTPUT_FILEPATH, $"gif{DateTime.Now.ToLocalTime()}"));

            MessagesSendParams param = new MessagesSendParams();
            param.Attachments = new ReadOnlyCollection<Document>(docList);

            Functions.SendMessage(message, param, "", message.ChatId != null);
            /*
            Task.Run(() =>
            {
                Parallel.For(0, maxCount, i =>
                {
                    File.Delete(IN_FILENAME + i + ".png");
                });
                File.Delete(OUTPUT_FILEPATH);
            });*/

        }

        private int SIZE_HEIGHT = 0;
        private int SIZE_WIDTH = 0;

        private void ToGif(int delay, int maxCount)
        {
            if (maxCount == 0)
            {
                throw  new WrongParamsException("Ну и из чего мне тебе гифку делать? Где картинки? Их должно быть не менее 2-х.");
            }
            AnimatedGifEncoder e = new AnimatedGifEncoder();
            e.Start(OUTPUT_FILEPATH);
            e.SetDelay(delay);
            //-1:no repeat,0:always repeat
            e.SetRepeat(0);

            SIZE_HEIGHT = SIZE_HEIGHT / maxCount;
            SIZE_WIDTH = SIZE_WIDTH / maxCount;

            for (int i = 0, count = maxCount; i < count; i++)
            {
                Image im = Image.FromFile(IN_FILENAME + i + ".png");
                im = new Bitmap(im, SIZE_WIDTH, SIZE_HEIGHT);
                e.AddFrame(im);
                im.Dispose();
            }
            e.Finish();
            e.SetDispose(0);
        }

        public string OUTPUT_FILEPATH = "Data\\Gif\\gif.gif";

        public bool NeedCommandInfo(Message message, params object[] p)
        {
            string info = info =
                $"Справка по команде \"{message.Body}\":\n\n" +
                "Бот делает гифку с задержкой между кадрами, указанной в скобках (в миллисекундах, 1с = 1000мс), из прикреплённых изображений (не менее 2-х).\n\n" +

                $"Замечание: задержка представляет собой целое число от 0 до {MAX_DELAY_LIMIT} (до {MAX_DELAY_LIMIT / 1000} секунд)\n\nПример: бот, гиф(500)";

            if (p[0] == null || String.IsNullOrEmpty(p[0].ToString()) || String.IsNullOrWhiteSpace(p[0].ToString()))
            {
                Functions.SendMessage(message, info, message.ChatId != null);
                return true;
            }
            return false;
        }
    }
}
