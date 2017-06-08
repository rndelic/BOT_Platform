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
using BOT_Platform.Kernel.Bots;
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

        private void SendLog(Message message, string args, Bot bot)
        {
            if (!CheckRoots(message.UserId, bot))
            {
                Functions.SendMessage(bot, message, "Недостаточно прав доступа для просмотра данной команды ⛔️", message.ChatId != null);
                return;
            }

            List<Document> docList = new List<Document>();
            docList.Add(Functions.UploadDocumentInMessage(bot.Directory + $@"\{Log.logFile}", $"log {DateTime.Now.ToLocalTime()}", bot));

            MessagesSendParams param = new MessagesSendParams();
            param.Attachments = new ReadOnlyCollection<Document>(docList);

            Functions.SendMessage(bot, message, param, "", message.ChatId != null);
        }

        private void Ban(Message message, string args, Bot bot)
        {
            if (!CheckRoots(message.UserId, bot))
            {
                Functions.SendMessage(bot, message, "Недостаточно прав доступа для просмотра данной команды ⛔️", message.ChatId != null);
                return;
            }
            string[] param = args.Split(new char[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries);
            CommandsList.TryBanUser(message, Functions.GetUserId(param[0], bot), param[1], bot);
        }

        private void UnBan(Message message, string args, Bot bot)
        {
            if (!CheckRoots(message.UserId, bot))
            {
                Functions.SendMessage(bot, message, "Недостаточно прав доступа для просмотра данной команды ⛔️", message.ChatId != null);
                return;
            }
            CommandsList.TryUnBanUser( message, Functions.GetUserId(args, bot), bot);
        }

        bool CheckRoots(long? UserId, Bot bot)
        {
            if (bot.GetAdmins.Where(adminId => adminId == UserId).Any())
                return true;
            return false;

        }
        public PCcontrols()
        {
            AddMyCommandInPlatform();
        }


        void Block(Message message, string args, Bot bot)
        {
            if (!CheckRoots(message.UserId, bot))
            {
                Functions.SendMessage(bot, message, "Недостаточно прав доступа для просмотра данной команды ⛔️", message.ChatId != null);
                return;
            }
            BlockInput(true);
            Console.WriteLine("Input ЗАблокирован.");
            Functions.SendMessage(bot, message, "Input заблокирован.", message.ChatId != null);
        }
        void UnBlock(Message message, string args, Bot bot)
        {
            if (!CheckRoots(message.UserId, bot))
            {
                Functions.SendMessage(bot, message, "Ты не админ :D", message.ChatId != null);
                return;
            }
            BlockInput(false);
            Console.WriteLine("Input РАЗблокирован.");
            Functions.SendMessage(bot, message, "Input разблокирован.", message.ChatId != null);
        }

        void OffPC(Message message, string args, Bot bot)
        {
            if (!CheckRoots(message.UserId, bot))
            {
                Functions.SendMessage(bot, message, "Недостаточно прав доступа для просмотра данной команды ⛔️", message.ChatId != null);
                return;
            }

            bool isHibernate = System.Windows.Forms.Application.SetSuspendState(System.Windows.Forms.PowerState.Hibernate, false, false);
            if (isHibernate == false)
            {
                Console.WriteLine("Невозможно перевести бота в спящий режим");
                Functions.SendMessage(bot, message, "Невозможно перевести ПК в спящий режим", message.ChatId != null);
            }
            //else Functions.SendMessage(message, "ПК был переведён в спящий режим", message.ChatId != null);
        }
    }
}
