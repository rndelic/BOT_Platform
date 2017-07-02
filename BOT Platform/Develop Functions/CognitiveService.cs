using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BOT_Platform;
using BOT_Platform.Kernel.Bots;
using BOT_Platform.Kernel.Interfaces;
using MyFunctions.Exceptions;
using Newtonsoft.Json;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using Message = VkNet.Model.Message;

namespace MyFunctions
{
    class CognitiveService : IMyCommands
    {
        const string KEY = "dbc8402263584379b93925f7ab10d841";

        private const string DIRECTORY_PATH = "CognitiveServices_Combo";

        public void AddMyCommandInPlatform()
        {
            CommandsList.TryAddCommand("комбо", new MyCommandStruct("Заменяет все лица из первого фото на вторую картинку", Combine));
            CommandsList.TryAddCommand("эмоции", new MyCommandStruct("Бот определяет наиболее ярко выраженные эмоции на лицах на фото", FEmoji));
        }

        private void FEmoji(Message message, string args, Bot bot)
        {
            if (NeedCommandInfo(message, args, bot)) return;

            List<Photo> photoList = new List<Photo>();

          Attachment[] photos = message.Attachments.Where(t => t.Instance is Photo).ToArray();
            if (photos.Length != 1)
            {
                Functions.SendMessage(bot, message, "Нужна одна картинка с лицом(ами).", message.ChatId != null);
                return;
            }

            Photo photo = (Photo) photos[0].Instance;
            WebRequest request = null;
            if (photo.Photo807 != null) request = WebRequest.Create(photo.Photo807);
            else if (photo.Photo604 != null) request = WebRequest.Create(photo.Photo604);
            else if (photo.Photo130 != null) request = WebRequest.Create(photo.Photo130);
            else if (photo.Photo75 != null) request = WebRequest.Create(photo.Photo75);

            var response = request.GetResponse();
            Bitmap loadedBitmap = null;
            using (var responseStream = response.GetResponseStream())
            {
                loadedBitmap = new Bitmap(responseStream);
            }

            Task<List<Emoji>> resTask = MakeEmojiRequest(loadedBitmap, message, bot);
            resTask.Wait();
            if (resTask.Result.Count == 0)
            {
                Functions.SendMessage(bot, message, "Лица не найдены :c", message.ChatId != null);
                return;
            }
            
            StringBuilder sB = new StringBuilder(); int index = 1;
            foreach (var face in resTask.Result)
            {
                KeyValuePair<string, float> res = GetMaxEmojiValue(face);
                sB.AppendLine($"[{index} лицо] {res.Key}: {res.Value}");
                ++index;
            }
            string outFileName = NumerateFaces(loadedBitmap, resTask.Result, bot.Directory);

            try
            {
                photoList.Add(Functions.UploadImageInMessage(outFileName, bot));
                Upload(message, photoList, bot, sB.ToString());
            }
            finally { File.Delete(outFileName); }
        }

        public bool NeedCommandInfo(Message message, string args, Bot bot)
        {
            string info = "";
            switch (message.Body)
            {
                case "комбо":
                    info =
                        $"Справка по команде \"{message.Body}\":\n\n" +
                        "Команда заменяет все лица на первом прикреплённом к сообщению фото на изображение со второй картинки.\n\n" +
                        "Внимание! Если лица не будут обнаружены, бот не сможет выполнить команду.";
                    break;
                case "эмоции":
                    info =
                        $"Справка по команде \"{message.Body}\":\n\n" +
                        "Бот определяет наиболее ярко выраженные эмоции на лицах на фото.\n\n" +
                        "Внимание! Если лица не будут обнаружены, бот не сможет выполнить команду.";
                    break;
            }
            info += "\n\nСоздано с помощью Microsoft Cognitive Services.";
            if (message.Attachments.Where(t => t.Instance is Photo).ToArray().Length == 0)
            {
                Functions.SendMessage(bot, message, info, message.ChatId != null);
                return true;
            }
            return false;
        }

        private void Combine(Message message, string args, Bot bot)
        {
            if (NeedCommandInfo(message, args, bot)) return;

            string path = Path.Combine(bot.Directory, DIRECTORY_PATH);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            List<Photo> photoList = new List<Photo>();

            Attachment[] photos = message.Attachments.Where(t => t.Instance is Photo).ToArray();
            if (photos.Length < 2)
            {
                Functions.SendMessage(bot, message, "Картинок должно быть 2: (1) где заменять лица, (2) на что заменять лица", message.ChatId != null);
                return;
            }
            if (photos.Length > 2)
            {
                Functions.SendMessage(bot, message, "А не многовато ли картинок? " +
                                                    "\nКартинок должно быть 2: (1) где заменять лица, (2) на что заменять лица.", message.ChatId != null);
                return;
            }

            Bitmap[] images = new Bitmap[photos.Length];

            Parallel.For(0, photos.Length, i =>
            {
                Photo photo = (Photo)photos[i].Instance;
                WebRequest request = null;
                if (photo.Photo807 != null) request = WebRequest.Create(photo.Photo807);
                else if (photo.Photo604 != null) request = WebRequest.Create(photo.Photo604);
                else if (photo.Photo130 != null) request = WebRequest.Create(photo.Photo130);
                else if (photo.Photo75 != null) request = WebRequest.Create(photo.Photo75);

                var response = request.GetResponse();
                Bitmap loadedBitmap = null;
                using (var responseStream = response.GetResponseStream())
                {
                    loadedBitmap = new Bitmap(responseStream);
                }
                images[i] = loadedBitmap;

            });

            Task<List<Emoji>> resTask = MakeEmojiRequest(images[0], message, bot);
            resTask.Wait();

            if (resTask.Result.Count == 0)
            {
                Functions.SendMessage(bot, message,"Лица не найдены :c", message.ChatId != null);
                return;
            }
            string outFileName = CombinePhotos(images, resTask.Result, bot.Directory);

            try
            {
                photoList.Add(Functions.UploadImageInMessage(outFileName, bot));
                Upload(message, photoList, bot);
            }
            finally {File.Delete(outFileName);}
        }

        void Upload(Message message, List<Photo> photoList, Bot bot, string text = "♻️")
        {

            MessagesSendParams param = new MessagesSendParams();
            param.Attachments = new ReadOnlyCollection<Photo>(photoList);

            Functions.SendMessage(bot, message, param, text, message.ChatId != null);
        }

        static byte[] GetImageAsByteArray(Bitmap image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }
        static async Task<List<Emoji>> MakeEmojiRequest(Bitmap image, Message message, Bot bot)
        {
            var client = new HttpClient();

            // Request headers - replace this example key with your valid key.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", KEY);

            // NOTE: You must use the same region in your REST call as you used to obtain your subscription keys.
            //   For example, if you obtained your subscription keys from westcentralus, replace "westus" in the 
            //   URI below with "westcentralus".
            string uri = "https://westus.api.cognitive.microsoft.com/emotion/v1.0/recognize?";
            HttpResponseMessage response;
            string responseContent;

            // Request body. Try this sample with a locally stored JPEG image.
            byte[] byteData = GetImageAsByteArray(image);

            using (var content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json" and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(uri, content);
                responseContent = response.Content.ReadAsStringAsync().Result;
            }
            try
            {
                var data = JsonConvert.DeserializeObject<List<Emoji>>(responseContent);
                return data;
            }
            catch {throw new WrongParamsException("Не удалось распознать лицо.\n" +
                                                 "Изображение слишком маленькое :c");}



        }

        /* TWO BITMAPS ONLY GARANTY */
        string CombinePhotos(Bitmap[] bitmaps, List<Emoji> data, string botDirectory)
        {
            double multiply = 2.5;
            Graphics graphics = Graphics.FromImage(bitmaps[0]);
            foreach (var emoji in data)
            {
                int width = (int) (emoji.faceRectangle.width * multiply);
                int height = (int) (emoji.faceRectangle.height * multiply);
                Bitmap newFace = new Bitmap(bitmaps[1], new Size(width, height));
                graphics.DrawImage(newFace, new Point( emoji.faceRectangle.left + emoji.faceRectangle.width/2 - width/3,
                                                       emoji.faceRectangle.top + emoji.faceRectangle.height/2 - height/3));
            }
            string outFilename =
                Path.Combine(botDirectory, Path.Combine(DIRECTORY_PATH,
                    String.Format("{0}.png", Guid.NewGuid())));
            graphics.Dispose();
            bitmaps[0].Save(outFilename, ImageFormat.Png);

            return outFilename;
        }

        public CognitiveService()
        {
            AddMyCommandInPlatform();
        }

        KeyValuePair<string, float> GetMaxEmojiValue(Emoji data)
        {
            KeyValuePair<string, float>[] allEmoji =
            {
                new KeyValuePair<string, float>("😡 злость (гнев)", data.scores.anger),
                new KeyValuePair<string, float>("😤 презрение", data.scores.contempt),
                new KeyValuePair<string, float>("🤢 отвращение", data.scores.disgust),
                new KeyValuePair<string, float>("😖 страх", data.scores.fear),
                new KeyValuePair<string, float>("☺️ счастье", data.scores.happiness),
                new KeyValuePair<string, float>("😐 нейтральность", data.scores.neutral),
                new KeyValuePair<string, float>("😢 грусть", data.scores.sadness),
                new KeyValuePair<string, float>("😲 удивление", data.scores.surprise),
            };

            KeyValuePair<string, float> maxEmoji = new KeyValuePair<string, float>($"❓ эмоция не найдена", 0f);
            for (int i = 0; i < allEmoji.Length; i++)
            {
                if (allEmoji[i].Value > maxEmoji.Value) maxEmoji = allEmoji[i];
            }
            return maxEmoji;
        }

        string NumerateFaces(Bitmap image, List<Emoji> data, string botDirectory)
        {
            const string FONT_NAME = "Arial Black";

            Graphics g = Graphics.FromImage(image);
            g.SmoothingMode = SmoothingMode.HighQuality;
            FontFamily ff = new FontFamily(FONT_NAME);

            int i = 1;
            foreach (var emoji in data)
            {
                Font font = new Font(ff, Math.Min(emoji.faceRectangle.width / i.ToString().Length, emoji.faceRectangle.height / i.ToString().Length),
                    FontStyle.Regular);
                StringFormat sf = new StringFormat();

                GraphicsPath gp = new GraphicsPath();
                gp.AddString(i.ToString(), ff, (int)FontStyle.Regular,
                    font.SizeInPoints + 1,
                    new PointF(emoji.faceRectangle.left, emoji.faceRectangle.top), sf);

                GraphicsPath outlinePath = (GraphicsPath)gp.Clone();
                outlinePath.Widen(new Pen(Color.Black, font.Size * 0.093f)); //6

                g.FillPath(Brushes.Black, outlinePath);
                g.FillPath(Brushes.White, gp);

                i++;
            }
            string outFilename =
                Path.Combine(botDirectory, Path.Combine(DIRECTORY_PATH,
                    String.Format("{0}.png", Guid.NewGuid())));
            g.Dispose();
            image.Save(outFilename, ImageFormat.Png);

            return outFilename;
        }
    }

    public class Emoji
    {
        public Facerectangle faceRectangle { get; set; }
        public Scores scores { get; set; }
    }

    public class Facerectangle
    {
        public int height { get; set; }
        public int left { get; set; }
        public int top { get; set; }
        public int width { get; set; }
    }

    public class Scores
    {
        public float anger { get; set; }
        public float contempt { get; set; }
        public float disgust { get; set; }
        public float fear { get; set; }
        public float happiness { get; set; }
        public float neutral { get; set; }
        public float sadness { get; set; }
        public float surprise { get; set; }
    }

}
