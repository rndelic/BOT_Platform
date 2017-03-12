using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BOT_Platform;
using VkNet.Model;

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
            CommandsList.TryAddCommand("блок", new MyComandStruct(
                                       "блокировать ввод", Block, true));
            CommandsList.TryAddCommand("разблок", new MyComandStruct(
                                       "разблокировать ввод", UnBlock, true));
        }

        public PCcontrols()
        {
            AddMyCommandInPlatform();
        }
        void Block(Message message, params object[] p)
        {
            if (message.UserId != 150887062 && message.UserId != 262045406)
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
            if (message.UserId != 150887062 && message.UserId != 262045406)
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
            if (message.UserId != 150887062 && message.UserId != 262045406)
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
            else Functions.SendMessage(message, "ПК был переведён в спящий режим", message.ChatId != null);
        }
    }
}
