﻿using BOT_Platform;
using BOT_Platform.Interfaces;
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
using MyFunctions.Exceptions;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

namespace MyFunctions
{
    class Mems: IMyCommands
    {
        const string OUT_FILENAME = "Data\\out_mem.jpg";
        const string IN_FILENAME = "Data\\in_mem.jpg";
        public void AddMyCommandInPlatform()
        {
            CommandsList.TryAddCommand("мем", new MyComandStruct("Делает мемасик с подписью (не забудьте прикрепить картинку)", MakeMem));
        }
        private void MakeMem(Message message, object[] p)
        {
            if (NeedCommandInfo(message, p)) return;

            List<Photo> photoList = new List<Photo>();
            var webClient = new WebClient();
            string[] text = p[0].ToString().Split(new char[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries)
                                           .Reverse().ToArray();

            if(text.Length == 0 || String.IsNullOrWhiteSpace(text[0]) ||
                                   String.IsNullOrEmpty(text[0]) ||

                                   ( text.Length == 2 && (String.IsNullOrWhiteSpace(text[1]) || 
                                   String.IsNullOrEmpty(text[1]))))
            {
                Functions.SendMessage(message, "Подпись не может содержать пустые строки :/", message.ChatId != null);
                return;
            }

            if (message.Attachments.Count == 0)
            {
                Functions.SendMessage(message, "Ну и на чём мне твои анекдоты писать? Где картинка?", message.ChatId != null);
                return;
            }
            if (message.Attachments.Count > 1)
            {
                Functions.SendMessage(message, "А не многовато ли картинок? Оставь одну, и, так уж и быть, я сделаю тебе мемасик.", message.ChatId != null);
                return;
            }

            Photo photo = ((message.Attachments[0].Instance) as Photo);
            if (photo == null)
            {
                Functions.SendMessage(message, "Прикрепи изображение!", message.ChatId != null);
                return;
            }
                    if      (photo.Photo2560 != null) webClient.DownloadFile(photo.Photo2560, IN_FILENAME);
                    else if (photo.Photo1280 != null) webClient.DownloadFile(photo.Photo1280, IN_FILENAME);
                    else if (photo.Photo807  != null) webClient.DownloadFile(photo.Photo807,  IN_FILENAME);
                    else if (photo.Photo604  != null) webClient.DownloadFile(photo.Photo604,  IN_FILENAME);
                    else if (photo.Photo130  != null) webClient.DownloadFile(photo.Photo130,  IN_FILENAME);
                    else if (photo.Photo75   != null) webClient.DownloadFile(photo.Photo75,   IN_FILENAME);

                    webClient.Dispose();
                    DrawAndSave(text);
                    photoList.Add(Functions.UploadImageInMessage(OUT_FILENAME));

                    Upload(message, photoList);

                    File.Delete(OUT_FILENAME);
                    File.Delete(IN_FILENAME);
        }

        void DrawAndSave(string[] text, float mlp = 1.1f)
        {
            Image image = Image.FromFile(IN_FILENAME);
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

            graphics.Dispose();
            image.Save(OUT_FILENAME);
            image.Dispose();
        }
        void Upload(Message message, List<Photo> photoList)
        {

            MessagesSendParams param = new MessagesSendParams();
            param.Attachments = new ReadOnlyCollection<Photo>(photoList);

            Functions.SendMessage(message, param, "", message.ChatId != null);
        }

        public bool NeedCommandInfo(Message message, params object[] p)
        {
            string info = $"Справка по команде \"{message.Body}\":\n\n" +
               "Бот комбинирует текст, указанный в скобках, и прикреплённое сообщение.\n\n" +
               "Учтите, что прикреплять нужно одно изображение, а текст не должен быть намного длиннее ширины изображения, иначе бот выдаст ошибку.\n\n" +
               "Для того, чтобы текст был написан лишь на нижней части изображения, напишите его в скобках без разделителя ,(запятой).\n" +
               $"Пример: {BOT_API.GetSettings().BotName[0]}, {message.Body}(когда купил айфон) - бот отправит картинку с текстом внизу.\n\n" +
               "Для того, чтобы текст был написан и сверху картинки, и снизу, поставьте в нужном месте запятую (если по какой-то причине запятых в тексте несколько, за разделитель бот примет самую первую (,)запятую.\n" +
               $"Пример: {BOT_API.GetSettings().BotName[0]}, {message.Body}(когда купил айфон, но понял, что любишь андроид) - внизу будет написано \"когда купил айфон\", а внизу - \"но понял, что люишь андроид\". Разделитель в изображение не попадает.";

            if (p[0] == null || String.IsNullOrEmpty(p[0].ToString()) || String.IsNullOrWhiteSpace(p[0].ToString()))
            {
                Functions.SendMessage(message, info, message.ChatId != null);
                return true;
            }
            return false;
        }

        public Mems()
        {
            AddMyCommandInPlatform();
        }
    }
}