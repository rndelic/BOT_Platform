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
using BOT_Platform.Kernel.Bots;
using MyFunctions.Exceptions;

namespace MyFunctions
{
    class GifAnimation : IMyCommands
    {
        private const string DIRECTORY_PATH = @"Data\Gifs";
        private const int MAX_DELAY_LIMIT = 5000;

        public GifAnimation()
        {
            AddMyCommandInPlatform();
            if (!Directory.Exists(DIRECTORY_PATH)) Directory.CreateDirectory(DIRECTORY_PATH);
        }

        public void AddMyCommandInPlatform()
        {
            CommandsList.TryAddCommand("гиф", new MyComandStruct("Делает гифку из прикреплённых изображений", MakeGif));
        }

        private void MakeGif(Message message, string args, Bot bot)
        {
            if (NeedCommandInfo(message, args, bot)) return;

            if (message.Attachments.Count <= 1)
            {
                Functions.SendMessage(bot, message, "Ну и из чего мне тебе гифку делать? Где картинки? Их должно быть не менее 2-х.", message.ChatId != null);
                return;
            }
            Regex reg = new Regex(@"\d{0," + MAX_DELAY_LIMIT.ToString().Length + "}");
            if (reg.Match(args).Value != args )
            {
                Functions.SendMessage(bot, message, $"Введено недопустимое значение! Введите целое число от 0 до {MAX_DELAY_LIMIT}.", message.ChatId != null);
                return;
            }


            Attachment[] photos = message.Attachments.Where(t => t.Instance is Photo).ToArray();
            if (photos.Length == 0)
            {
                throw new WrongParamsException("Ну и из чего мне тебе гифку делать? Где картинки? Их должно быть не менее 2-х.");
            }

            Image[] images = new Image[photos.Length];

            Parallel.For(0, photos.Length, i =>
            {
                Photo photo = (Photo) photos[i].Instance;
                WebRequest request = null;
                if (photo.Photo2560 != null) request = WebRequest.Create(photo.Photo2560);
                else if (photo.Photo1280 != null) request = WebRequest.Create(photo.Photo1280);
                else if (photo.Photo807 != null) request = WebRequest.Create(photo.Photo807);
                else if (photo.Photo604 != null) request = WebRequest.Create(photo.Photo604);
                else if (photo.Photo130 != null) request = WebRequest.Create(photo.Photo130);
                else if (photo.Photo75 != null) request = WebRequest.Create(photo.Photo75);

                var response = request.GetResponse();
                Bitmap loadedBitmap = null;
                using (var responseStream = response.GetResponseStream())
                {
                    loadedBitmap = new Bitmap(responseStream);
                }
                images[i] = (Image) loadedBitmap;

                SIZE_HEIGHT += images[i].Height;
                SIZE_WIDTH += images[i].Width;
            });
            string outFilename = ToGif(Convert.ToInt32(args), images);
            List<Document> docList = new List<Document>();

            try
            {
                docList.Add(Functions.UploadDocumentInMessage(outFilename, $"gif{DateTime.Now.ToLocalTime()}", bot));
            }
            finally { File.Delete(outFilename); }

            MessagesSendParams param = new MessagesSendParams();
            param.Attachments = new ReadOnlyCollection<Document>(docList);

            Functions.SendMessage(bot, message, param, "", message.ChatId != null);

        }

        private int SIZE_HEIGHT = 0;
        private int SIZE_WIDTH = 0;

        string ToGif(int delay, Image[] images)
        {
            AnimatedGifEncoder e = new AnimatedGifEncoder();
            string outFilename = String.Format(@"Data\Gifs\{0}.gif", Guid.NewGuid());
            e.Start(outFilename);
            e.SetDelay(delay);
            //-1:no repeat,0:always repeat
            e.SetRepeat(0);

            SIZE_HEIGHT = SIZE_HEIGHT / images.Length;
            SIZE_WIDTH = SIZE_WIDTH / images.Length;

            for (int i = 0, count = images.Length; i < count; i++)
            {
                images[i] = new Bitmap(images[i], SIZE_WIDTH, SIZE_HEIGHT);
                e.AddFrame(images[i]);
                images[i].Dispose();
            }
            e.Finish();
            e.SetDispose(0);

            return outFilename;
        }


        public bool NeedCommandInfo(Message message, string args, Bot bot)
        {
            string info = info =
                $"Справка по команде \"{message.Body}\":\n\n" +
                "Бот делает гифку с задержкой между кадрами, указанной в скобках (в миллисекундах, 1с = 1000мс), из прикреплённых изображений (не менее 2-х).\n\n" +

                $"Замечание: задержка представляет собой целое число от 0 до {MAX_DELAY_LIMIT} (до {MAX_DELAY_LIMIT / 1000} секунд)\n\nПример: бот, гиф(500)";

            if (args == null || String.IsNullOrEmpty(args) || String.IsNullOrWhiteSpace(args))
            {
                Functions.SendMessage(bot, message, info, message.ChatId != null);
                return true;
            }
            return false;
        }
    }
}
