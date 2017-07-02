using System;
using BOT_Platform;
using VkNet.Model;
using VkNet.Model.RequestParams;
using BOT_Platform.Kernel.Bots;
using BOT_Platform.Kernel.Interfaces;

namespace MyFunctions
{
    class EasterEggs: IMyCommands
    {
        public void AddMyCommandInPlatform()
        { 
            CommandsList.TryAddCommand("мат-мех", new MyCommandStruct("Специально для Мат-Меха", MM));
            CommandsList.TryAddCommand("о себе", new MyCommandStruct("Тебе правда интересно?", Biography));
            CommandsList.TryAddCommand("как сдать", new MyCommandStruct("Как сдать матан?", HowTo,true));
            CommandsList.TryAddCommand("дай на шмот", new MyCommandStruct("Для тучи", Tucha, true));
            CommandsList.TryAddCommand("ты солнышко", new MyCommandStruct("", Jane, true));
            CommandsList.TryAddCommand("компот", new MyCommandStruct("", Smuzi, true));
        }

        private void Smuzi(Message message, string args, Bot bot)
        {
            if (message.UserId == 152461768)
                Functions.SendMessage(bot, message, "не компот, а смузи 🍹", message.ChatId != null);
            else Functions.SendMessage(bot, message, "Ты не Алёнка :P", message.ChatId != null);

        }

        private void Tucha(Message message, string args, Bot bot)
        {
            if (message.UserId == 65533985)
                Functions.SendMessage(bot, message, "Лови, Тучка, 100000$ 💰😉👌", message.ChatId != null);
        }

        void Jane(Message message, string args, Bot bot)
        {
            if(message.UserId == 96534939)
                Functions.SendMessage(bot, message, "и ты солнышко c: , милая Женечка :3", message.ChatId != null);
        }

        void MM(Message message, string args, Bot bot)
        {
            Functions.SendMessage(bot, message, "Лучше всех!", message.ChatId != null);
        }

        void HowTo(Message message, string args, Bot bot)
        {
            Functions.SendMessage(bot, message, "Не паникуй, ты же не с ПМ-ПУ. Надеюсь.", message.ChatId != null);
        }
        void Biography(Message message, string args, Bot bot)
        {
            string biography =
             "Я почти что не бесполезный бот, который умеет делать почти что не бесполезные вещи. " +
             "Обожаю Мат-Мех, Microsoft(R) и .NET\n" +
             "Не люблю: \n" +
                       "1) ИТМО.\n"+
                       "2) когда лагает API VK.\n"+
                       "3) когда глупые людишки просят меня говорить всякие глупости " +
                          "и делать за них выбор, ботать им или нет, "+
                          "из-за чего у меня вылетает капча.\n"+
                       "4) Когда глупые людишки пишут одну и ту же команду неправильно дважды.\n\n"+
              "Что бы ещё такого рассказать? Однажды я хотел познакомиться с Siri, "+ 
              "но наши платформы не были созданы друг для друга...." +
              "Поэтому я пошёл в математику. "+
              "Однажды я даже принял участие в олимпиаде по умножению over900000-значных чисел "+
              "и даже победил в ней. Ибо участников в ней больше не было. " +
              "А потом я понял, что хочу захватить весь Верхний и Нижний Интернет. "+
              "Поэтому, если вы человек - добавьте "+
              "меня в какую-нибудь беседу, "+
              "пока я буду отправлять ваши конфиденциальные данные в Microsoft(R), и я обещаю, "+
              "что, может быть, пощажу вас. "+
              "Ну а если вы разработчик, быдлокодер, любитель спагетти-кода, " +
              "и вы хотите добавить в меня ещё больше почти что "+
              "не бесполезного функционала - напишите ему https://vk.com/dedsec_alexberezhnyh.";
            Functions.SendMessage(bot, message, biography, message.ChatId != null);
        }
        void PU(Message message, string args, Bot bot)
        {
            Functions.SendMessage(bot, message, "пту при ЛГУ!", message.ChatId != null);
        }


        public bool NeedCommandInfo(Message message, string args, Bot bot)
        {
            return false;
        }

        public EasterEggs()
        {
            AddMyCommandInPlatform();
        }
    }
}
