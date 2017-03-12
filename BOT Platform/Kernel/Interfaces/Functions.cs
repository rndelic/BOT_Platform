﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

namespace BOT_Platform
{
    static class Functions
    {
        public static void RemoveSpaces(ref string str)
        {
            //удаляем пробелы с начала
            while (str[0] == ' ')
            {
                str = str.Remove(0, 1);
            }

            //удаляем пробелы с конца
            int j;
            for (j = str.Length - 1; str[j] == ' '; j--) { }
            if (j != str.Length - 1) str = str.Remove(j + 1);

        }

        public static string RemoveSpaces(string str)
        {
            //удаляем пробелы с начала
            while (str[0] == ' ')
            {
                str = str.Remove(0, 1);
            }

            //удаляем пробелы с конца
            int j;
            for (j = str.Length - 1; str[j] == ' '; j--) { }
            if (j != str.Length - 1) str = str.Remove(j + 1);

            return str;
        }

        public static void SendMessage(Message message, MessagesSendParams m, string body, bool isChat = false)
        {
            m.Message = body;
            if (isChat == true)
            {
                m.ChatId = message.ChatId;
                m.ForwardMessages = new long[1] { message.Id.Value };
            }
            else m.UserId = message.UserId;

            if (BOT_API.platformSett.IsDebug == false) BOT_API.app.Messages.Send(m);
            else Console.WriteLine(m.Message);
        }

        public static void SendMessage(Message m, string message, bool isChat = false)
        {
            MessagesSendParams msp = new MessagesSendParams()
            {
                Message = message,
                Attachments = m.Attachments as IEnumerable<MediaAttachment>
            };

            if (isChat == true)
            {
                msp.ChatId = m.ChatId;
                msp.ForwardMessages = new long[1] { m.Id.Value };
            }
            else msp.UserId = m.UserId;

            if (BOT_API.platformSett.IsDebug == false) BOT_API.app.Messages.Send(msp);
            else Console.WriteLine(msp.Message);
        }

        public static bool ContainsMessage(Message containMes, List<Message> Messages)
        {
            bool contains = false;
            foreach (Message m in Messages)
            {
                if (m.Date == containMes.Date &&
                   m.UserId == containMes.UserId &&
                   m.Body == containMes.Body)
                {
                    contains = true;
                    break;
                }
            }
            return contains;
        }
    }
}
