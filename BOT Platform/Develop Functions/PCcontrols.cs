using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BOT_Platform;
using VkNet.Model;
using BOT_Platform.Interfaces;
using VkNet.Model.RequestParams;
using System.Collections.ObjectModel;
using VkNet.Model.Attachments;
using static BOT_Platform.CommandsList;

namespace MyFunctions
{
    class PCcontrols
    {
        [DllImport("user32.dll", EntryPoint = "BlockInput")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BlockInput([MarshalAs(UnmanagedType.Bool)] bool fBlockIt);


    public void AddMyCommandInPlatform()
        {
            CommandsList.TryAddCommand("сон", new MyComandStruct(
                                       "дистанционно выключить ПК", OffPC, true));
            CommandsList.TryAddCommand("бан", new MyComandStruct(
                                       "бан юзера", Ban, true));
            CommandsList.TryAddCommand("разбан", new MyComandStruct(
                                       "разбан юзера", UnBan, true));
            CommandsList.TryAddCommand("блок", new MyComandStruct(
                                       "блокировать ввод", Block, true));
            CommandsList.TryAddCommand("разблок", new MyComandStruct(
                                       "разблокировать ввод", UnBlock, true));
            CommandsList.TryAddCommand("лог", new MyComandStruct(
                                       "отправляет лог-файл", SendLog, true));
        }

        private void SendLog(Message message, object[] p)
        {
            if (!CheckRoots(message.UserId))
            {
                Functions.SendMessage(message, "Ты не админ :D", message.ChatId != null);
                return;
            }

            List<Document> docList = new List<Document>();
            docList.Add(Functions.UploadDocumentInMessage(Log.logFile, $"log {DateTime.Now.ToLocalTime()}"));

            MessagesSendParams param = new MessagesSendParams();
            param.Attachments = new ReadOnlyCollection<Document>(docList);

            Functions.SendMessage(message, param, "", message.ChatId != null);
        }

        private void Ban(Message message, object[] p)
        {
            if (!CheckRoots(message.UserId))
            {
                Functions.SendMessage(message, "Ты не админ :D", message.ChatId != null);
                return;
            }
            string[] param = p[0].ToString().Split(new char[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries);
            CommandsList.TryBanUser(message, Functions.GetUserId(param[0]), param[1]);
        }

        private void UnBan(Message message, object[] p)
        {
            if (!CheckRoots(message.UserId))
            {
                Functions.SendMessage(message, "Ты не админ :D", message.ChatId != null);
                return;
            }
            CommandsList.TryUnBanUser(message, Functions.GetUserId(p[0].ToString()));
        }

        bool CheckRoots(long? UserId)
        {
            if (UserId != 150887062 && UserId != BOT_API.GetApi().UserId) return false;
            return true;
        }
        public PCcontrols()
        {
            AddMyCommandInPlatform();
        }


        void Block(Message message, params object[] p)
        {
            if (!CheckRoots(message.UserId))
            {
                Functions.SendMessage(message, "Ты не админ :D", message.ChatId != null);
                return;
            }
            BlockInput(true);
            Console.WriteLine("Input ЗАблокирован.");
            Functions.SendMessage(message, "Input заблокирован.", message.ChatId != null);
        }
        void UnBlock(Message message, params object[] p)
        {
            if (!CheckRoots(message.UserId))
            {
                Functions.SendMessage(message, "Ты не админ :D", message.ChatId != null);
                return;
            }
            BlockInput(false);
            Console.WriteLine("Input РАЗблокирован.");
            Functions.SendMessage(message, "Input разблокирован.", message.ChatId != null);
        }

        void OffPC(Message message, params object[] p)
        {
            if (!CheckRoots(message.UserId))
            {
                Functions.SendMessage(message, "Ты не админ :D", message.ChatId != null);
                return;
            }

            bool isHibernate = System.Windows.Forms.Application.SetSuspendState(System.Windows.Forms.PowerState.Hibernate, false, false);
            if (isHibernate == false)
            {
                Console.WriteLine("Невозможно перевести ПК в спящий режим");
                Functions.SendMessage(message, "Невозможно перевести ПК в спящий режим", message.ChatId != null);
            }
            //else Functions.SendMessage(message, "ПК был переведён в спящий режим", message.ChatId != null);
        }
    }
}
