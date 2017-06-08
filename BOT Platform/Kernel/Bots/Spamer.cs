using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BOT_Platform.Interfaces;
using BOT_Platform.Kernel.Bots;
using VkNet.Model;

namespace BOT_Platform.Kernel.Bots
{
    class Spamer : UserBot
    {
        public Spamer(string botName, string directory) : base(botName, directory)
        {}

        public Spamer() : base(null, null)
        {
        }

        protected override void ExecuteCommand(MessagesGetObject messages)
        {
            Parallel.ForEach(messages.Messages, Message =>
                {
                    if (String.IsNullOrEmpty(Message.Body) || Message.ChatId == null) return;
            
                    
                }
            );
            Thread.Sleep(platformSett.Delay);
        }
    }
}
