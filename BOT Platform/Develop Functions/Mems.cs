using BOT_Platform;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using BOT_Platform.Kernel.Bots;
using BOT_Platform.Kernel.Interfaces;
using MyFunctions.Exceptions;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

namespace MyFunctions
{
    class Mems: IMyCommands
    {
        private const string DIRECTORY_PATH = @"Data\Mems";
        private char[] SEPARATOR = new char[] {'\n'};
        public void AddMyCommandInPlatform()
        {
            CommandsList.TryAddCommand("мем", new MyComandStruct("Делает мемасик с подписью (не забудьте прикрепить картинку)", MakeMem));
        }
        private void MakeMem(Message message, string args, Bot bot)
        {
            if (NeedCommandInfo(message, args, bot)) return;

            List<Photo> photoList = new List<Photo>();
            string[] text = args.Split(SEPARATOR, 2, StringSplitOptions.RemoveEmptyEntries)
                                           .Reverse().ToArray();
            for (int i = 0; i < text.Length; i++)
            {
                text[i] = text[i].Replace("\\n", " ");
            }

            if(text.Length == 0 || String.IsNullOrWhiteSpace(text[0]) ||
                                   String.IsNullOrEmpty(text[0]) ||

                                   ( text.Length == 2 && (String.IsNullOrWhiteSpace(text[1]) || 
                                   String.IsNullOrEmpty(text[1]))))
            {
                Functions.SendMessage(bot, message, "Подпись не может содержать пустые строки :/", message.ChatId != null);
                return;
            }

            if (message.Attachments.Count == 0)
            {
                Functions.SendMessage(bot, message, "Ну и на чём мне твои анекдоты писать? Где картинка?", message.ChatId != null);
                return;
            }
            if (message.Attachments.Count > 1)
            {
                Functions.SendMessage(bot, message, "А не многовато ли картинок? Оставь одну, и, так уж и быть, я сделаю тебе мемасик.", message.ChatId != null);
                return;
            }

            Photo photo = ((message.Attachments[0].Instance) as Photo);
            if (photo == null)
            {
                Functions.SendMessage(bot, message, "Прикрепи изображение!", message.ChatId != null);
                return;
            }

            WebRequest request = null;
            if      (photo.Photo2560 != null) request = WebRequest.Create(photo.Photo2560);
            else if (photo.Photo1280 != null) request = WebRequest.Create(photo.Photo1280);
            else if (photo.Photo807 != null)  request = WebRequest.Create(photo.Photo807);
            else if (photo.Photo604 != null)  request = WebRequest.Create(photo.Photo604);
            else if (photo.Photo130 != null)  request = WebRequest.Create(photo.Photo130);
            else if (photo.Photo75 != null)   request = WebRequest.Create(photo.Photo75);

            var response = request.GetResponse();
            Bitmap loadedBitmap = null;
            using (var responseStream = response.GetResponseStream())
            {
                loadedBitmap = new Bitmap(responseStream);
            }
            string outFilename = DrawAndSave((Image)loadedBitmap, text);
            loadedBitmap.Dispose();

            photoList.Add(Functions.UploadImageInMessage(outFilename, bot));
            Upload(message, photoList, bot);
            File.Delete(outFilename);
        }

        string DrawAndSave(Image image, string[] text, float mlp = 1.1f)
        {
            Graphics graphics = Graphics.FromImage(image);
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            const string FONT_NAME = "Arial Black";

            for (int i = 0; i < text.Length; i++)
            {
                text[i] = text[i].ToUpperInvariant();
                Functions.RemoveSpaces(ref text[i]);
            }

            using (Graphics g = Graphics.FromImage(image))
            {
                //g.InterpolationMode = InterpolationMode.High;
                g.SmoothingMode = SmoothingMode.HighQuality;
                //g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                //g.CompositingQuality = CompositingQuality.HighQuality;

                FontFamily ff = new FontFamily(FONT_NAME);

                if (Math.Min((image.Width / text[0].Length), image.Height / 4) * mlp <= 0)
                {
                    throw new WrongParamsException("Текст слишком длинный для данного изображения.");
                }

                Font font = new Font(ff, Math.Min((image.Width / text[0].Length), image.Height / 4) * mlp,
                    FontStyle.Regular);
                StringFormat sf = new StringFormat();

                SizeF textSize = graphics.MeasureString(text[0], font);

                GraphicsPath gp = new GraphicsPath();
                gp.AddString(text[0], ff, (int) FontStyle.Regular,
                    font.SizeInPoints + 1,
                    new PointF(Math.Abs(image.Width - textSize.Width) / 2, image.Height - textSize.Height), sf);

                GraphicsPath outlinePath = (GraphicsPath) gp.Clone();
                // outline the path
                outlinePath.Widen(new Pen(Color.Black, font.Size * 0.093f)); //6

                g.FillPath(Brushes.Black, outlinePath);
                g.FillPath(Brushes.White, gp);

                if (text.Length > 1)
                {
                    if (Math.Min((image.Width / text[1].Length), image.Height / 4) * mlp <= 0)
                    {
                        throw new WrongParamsException("Текст слишком длинный для данного изображения.");
                    }

                    font = new Font(ff, Math.Min((image.Width / text[1].Length), image.Height / 4) * mlp, FontStyle.Regular);
                    sf = new StringFormat();

                    textSize = graphics.MeasureString(text[1], font);
                    gp = new GraphicsPath();
                    gp.AddString(text[1], ff, (int)FontStyle.Regular, font.SizeInPoints + 1, new PointF(Math.Abs(image.Width - textSize.Width) / 2, 0), sf); //image.Height * 0.04f

                    outlinePath = (GraphicsPath)gp.Clone();
                    outlinePath.Widen(new Pen(Color.Black, font.Size * 0.093f));

                    g.FillPath(Brushes.Black, outlinePath);
                    g.FillPath(Brushes.White, gp);
                }

                //g.Flush(FlushIntention.Sync);
            }
            string outFilename = String.Format(@"Data\Mems\{0}.png", Guid.NewGuid());

            graphics.Dispose();
            image.Save(outFilename);
            image.Dispose();

            return outFilename;
        }
        void Upload(Message message, List<Photo> photoList, Bot bot)
        {

            MessagesSendParams param = new MessagesSendParams();
            param.Attachments = new ReadOnlyCollection<Photo>(photoList);

            Functions.SendMessage(bot, message, param, "", message.ChatId != null);
        }

        public bool NeedCommandInfo(Message message, string args, Bot bot)
        {
            string info = $"Справка по команде \"{message.Body}\":\n\n" +
               "Бот комбинирует текст, указанный в скобках, и прикреплённое сообщение.\n\n" +
               "Учтите, что прикреплять нужно одно изображение, а текст не должен быть намного длиннее ширины изображения, иначе бот выдаст ошибку.\n\n" +
               "Для того, чтобы текст был написан лишь на нижней части изображения, напишите его, не начиная текст с новой строки.\n" +
               $"Пример: {bot.GetSettings().BotName[0]}, {message.Body}(когда купил айфон) - бот отправит картинку с текстом внизу.\n\n" +
               "Для того, чтобы текст был написан и сверху картинки, и снизу, перейдите на новую строку (учитывается только первый переход на новую строку).\n" +
               $"Пример: {bot.GetSettings().BotName[0]}, {message.Body}(когда купил айфон,\n но понял, что любишь андроид) - внизу будет написано \"когда купил айфон,\", а внизу - \"но понял, что любишь андроид\". Текст автоматически становится заглавным";

            if (args == null || String.IsNullOrEmpty(args) || String.IsNullOrWhiteSpace(args))
            {
                Functions.SendMessage(bot, message, info, message.ChatId != null);
                return true;
            }
            return false;
        }

        public Mems()
        {
            AddMyCommandInPlatform();
            if (!Directory.Exists(DIRECTORY_PATH)) Directory.CreateDirectory(DIRECTORY_PATH);
        }
    }
}