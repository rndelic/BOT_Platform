using System;
using BOT_Platform;
using VkNet.Model;
using VkNet.Model.RequestParams;
using BOT_Platform.Interfaces;

namespace MyFunctions
{
    class EasterEggs: IMyCommands
    {
        public void AddMyCommandInPlatform()
        { 
            CommandsList.TryAddCommand("мат-мех", new MyComandStruct("Специально для Мат-Меха", MM));
            CommandsList.TryAddCommand("о себе", new MyComandStruct("Тебе правда интересно?", Biography));
            CommandsList.TryAddCommand("как сдать", new MyComandStruct("Как сдать матан?", HowTo,true));
            CommandsList.TryAddCommand("дай на шмот", new MyComandStruct("Для тучи", Tucha, true));
            CommandsList.TryAddCommand("ты солнышко", new MyComandStruct("", Jane, true));
        }

        private void Tucha(Message message, object[] p)
        {
            if (message.UserId == 65533985)
                Functions.SendMessage(message, "Лови, Тучка, 100000$ 💰😉👌", message.ChatId != null);
            else 
            {
                Functions.SendMessage(message, "Ты не Тучка ;/ Скорее всего, ты Кверти, ко-ко-ко.", message.ChatId != null);
                SpeechText.Speech(message, "Кверти-петух, ко-ко-ко");
            }
        }

        void Jane(Message message, object[] p)
        {
            if(message.UserId == 96534939)
                Functions.SendMessage(message, "и ты солнышко c: , милая Женечка :3", message.ChatId != null);
        }

        void MM(Message message, params object[] p)
        {
            Functions.SendMessage(message, "Лучше всех!", message.ChatId != null);
        }

        void HowTo(Message message, params object[] p)
        {
            Functions.SendMessage(message, "Не паникуй, ты же не с ПМ-ПУ. Надеюсь.", message.ChatId != null);
        }
        void Biography(Message message, params object[] p)
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
            Functions.SendMessage(message, biography, message.ChatId != null);
        }
        void PU(Message message, params object[] p)
        {
            Functions.SendMessage(message, "пту при ЛГУ!", message.ChatId != null);
        }


        public bool NeedCommandInfo(Message message, params object[] p)
        {
            throw new NotImplementedException();
        }

        public EasterEggs()
        {
            AddMyCommandInPlatform();
        }
    }
}
