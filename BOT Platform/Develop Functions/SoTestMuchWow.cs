using BOT_Platform;
using System;
using System.Text;
using VkNet.Model;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using VkNet.Model.RequestParams;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using VkNet.Model.Attachments;
using BOT_Platform.Kernel.Bots;
using BOT_Platform.Kernel.Interfaces;

namespace MyFunctions
{
    class SoTestMuchWow : IMyCommands
    {
        
        public void AddMyCommandInPlatform()
        {
            CommandsList.TryAddCommand("doge", new MyComandStruct("woof", DogeWoof, true)); 
        }
        /// <summary>
        /// Собака лает 
        /// </summary>
        /// <param name="message">Сообщение, принятое от пользователя</param>
        /// <param name="args">Аргумент сообщения</param>
        /// <param name="bot">Бот, которому направлено сообщение</param>
        private void DogeWoof(Message message, string args, Bot bot)
        {   
            if (NeedCommandInfo(message, args, bot)) return;
            if (args == "Ебись оно всё конём")
            {
                Functions.SendMessage(bot, message, "Я сегодня хорошо поработал, Гав!", message.ChatId != null);
            }
        }

        public bool NeedCommandInfo(Message message, string args, Bot bot)
        {
            string info = $"Справка по команде {message.Body}.\n" +
                $"Гав.";
            if(String.IsNullOrEmpty(args))
            {
                Functions.SendMessage(bot, message, info, message.ChatId != null);
                return true; 
            }
            return false; 
        }
        
        public SoTestMuchWow()
        {
            AddMyCommandInPlatform();
        }


    }
}
