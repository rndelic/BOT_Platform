﻿using BOT_Platform;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

namespace MyFunctions
{
    class Mems
    {
        const string OUT_FILENAME = "out_mem.jpg";
        const string IN_FILENAME = "in_mem.jpg";
        public void AddMyCommandInPlatform()
        {
            CommandsList.TryAddCommand("мем", new MyComandStruct("Делает мемасик с подписью (не забудьте прикрепить картинку)", MakeMem));
        }

        private void MakeMem(Message message, object[] p)
        {
            List<Photo> photoList = new List<Photo>();
            var webClient = new WebClient();
            string[] text = p[0].ToString().Split(new char[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries);

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

                    if      (photo.Photo2560 != null) webClient.DownloadFile(photo.Photo2560, IN_FILENAME);
                    else if (photo.Photo1280 != null) webClient.DownloadFile(photo.Photo1280, IN_FILENAME);
                    else if (photo.Photo807  != null) webClient.DownloadFile(photo.Photo807,  IN_FILENAME);
                    else if (photo.Photo604  != null) webClient.DownloadFile(photo.Photo604,  IN_FILENAME);
                    else if (photo.Photo130  != null) webClient.DownloadFile(photo.Photo130,  IN_FILENAME);
                    else if (photo.Photo75   != null) webClient.DownloadFile(photo.Photo75,   IN_FILENAME);

                    webClient.Dispose();
                    DrawAndSave(text);
                    photoList.Add(SavePhoto());

                    Upload(message, photoList);

                    File.Delete(OUT_FILENAME);
        }

        void DrawAndSave(string[] text)
        {
            Image image = Image.FromFile(IN_FILENAME);
            Graphics graphics = Graphics.FromImage(image);
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            const string FONT_NAME = "Arial Black";
            const float mlp = 1f;

            for (int i = 0; i < text.Length; i++)
            {
                text[i] = text[i].ToUpperInvariant();
                Functions.RemoveSpaces(ref text[i]);
            }

            if (text.Length == 1)
            {
                using (Graphics g = Graphics.FromImage(image))
                {
                    //g.InterpolationMode = InterpolationMode.High;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    //g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    //g.CompositingQuality = CompositingQuality.HighQuality;

                    FontFamily ff = new FontFamily(FONT_NAME);
                    Font font = new Font(ff, Math.Min((image.Width / text[0].Length), image.Height / 4) * mlp, FontStyle.Regular);
                    StringFormat sf = new StringFormat();

                    SizeF textSize = graphics.MeasureString(text[0], font);

                    GraphicsPath gp = new GraphicsPath();
                    gp.AddString(text[0], ff, (int)FontStyle.Regular, Math.Min((image.Width / text[0].Length), image.Height / 4) * mlp + 1, new PointF((image.Width - textSize.Width) / 2, image.Height - textSize.Height), sf);

                    GraphicsPath outlinePath = (GraphicsPath)gp.Clone();
                    // outline the path
                    outlinePath.Widen(new Pen(Color.Black, font.Size * 0.093f));//6

                    g.FillPath(Brushes.Black, outlinePath);
                    g.FillPath(Brushes.White, gp);

                    //g.Flush(FlushIntention.Sync);
                    image.Save(OUT_FILENAME);
                    g.Dispose();
                    gp.Dispose();
                    outlinePath.Dispose();
                }
            }

            else
            {
                using (Graphics g = Graphics.FromImage(image))
                {
                    //g.InterpolationMode = InterpolationMode.High;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    //g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    //g.CompositingQuality = CompositingQuality.HighQuality;

                    FontFamily ff = new FontFamily(FONT_NAME);
                    Font font = new Font(ff, Math.Min((image.Width / text[1].Length), image.Height / 4) * mlp, FontStyle.Regular);
                    StringFormat sf = new StringFormat();

                    SizeF textSize = graphics.MeasureString(text[1], font);
                    GraphicsPath gp = new GraphicsPath();
                    gp.AddString(text[1], ff, (int)FontStyle.Regular, Math.Min((image.Width / text[1].Length), image.Height / 4) * mlp + 1, new PointF((image.Width - textSize.Width) / 2, image.Height - textSize.Height), sf);

                    GraphicsPath outlinePath = (GraphicsPath)gp.Clone();
                    // outline the path
                    outlinePath.Widen(new Pen(Color.Black, font.Size * 0.093f));

                    g.FillPath(Brushes.Black, outlinePath);
                    g.FillPath(Brushes.White, gp);

                    //g.Flush(FlushIntention.Sync);
                    image.Save(OUT_FILENAME);
                    g.Dispose();
                    gp.Dispose();
                    outlinePath.Dispose();
                }
                using (Graphics g = Graphics.FromImage(image))
                {
                    //g.InterpolationMode = InterpolationMode.High;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    //g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    //g.CompositingQuality = CompositingQuality.HighQuality;

                    FontFamily ff = new FontFamily(FONT_NAME);
                    Font font = new Font(ff, Math.Min((image.Width / text[0].Length), image.Height / 4) * mlp, FontStyle.Regular);
                    StringFormat sf = new StringFormat();

                    SizeF textSize = graphics.MeasureString(text[0], font);
                    GraphicsPath gp = new GraphicsPath();
                    gp.AddString(text[0], ff, (int)FontStyle.Regular, Math.Min((image.Width / text[0].Length), image.Height / 4) * mlp + 1, new PointF((image.Width - textSize.Width) / 2, 0), sf); //image.Height * 0.04f

                    GraphicsPath outlinePath = (GraphicsPath)gp.Clone();
                    // outline the path
                    outlinePath.Widen(new Pen(Color.Black, font.Size * 0.093f));

                    g.FillPath(Brushes.Black, outlinePath);
                    g.FillPath(Brushes.White, gp);

                    //g.Flush(FlushIntention.Sync);
                    image.Save(OUT_FILENAME);
                    g.Dispose();
                    gp.Dispose();
                    outlinePath.Dispose();
                }
            }

            graphics.Dispose();
            image.Save(OUT_FILENAME);
            image.Dispose();
        }

        Photo SavePhoto()
        {
            var uploadServer = BOT_API.app.Photo.GetMessagesUploadServer();
            // Загрузить фотографию.
            var wc = new WebClient();
            var responseImg = Encoding.ASCII.GetString(wc.UploadFile(uploadServer.UploadUrl, OUT_FILENAME));
            // Сохранить загруженную фотографию
            return BOT_API.app.Photo.SaveMessagesPhoto(responseImg)[0];
        }
        void Upload(Message message, List<Photo> photoList)
        {

            MessagesSendParams param = new MessagesSendParams();
            param.Attachments = new ReadOnlyCollection<Photo>(photoList);

            Functions.SendMessage(message, param, "", message.ChatId != null);
        }


        public Mems()
        {
            AddMyCommandInPlatform();
        }
    }
}