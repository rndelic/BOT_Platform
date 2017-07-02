using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BOT_Platform.Kernel.CIO;
using BOT_Platform.Kernel.Interfaces;
using NCron;
using NCron.Fluent;
using NCron.Service;
using NCron.Fluent.Crontab;
using VkNet.Model;
using Message = VkNet.Model.Message;

namespace BOT_Platform.Kernel.Bots
{
    sealed class LalkaOfTheDay: UserBot
    {
        public LalkaOfTheDay(string botName, string directory) : base(botName, directory)
        { }

        public LalkaOfTheDay() : base(null, null)
        {
        }
        private string time = $"0 21 * * *";

        public override void BotWork()
        {
            /* Подключаемся к VK, запускаем бота */
            try
            {
                BotConsole.Write($"[Запуск бота {Name}...]");
                _app.Authorize(platformSett.AuthParams);
                BotConsole.Write($"Бот {Name} запущен.");
            }
            catch (Exception ex)
            {
                BotConsole.Write($"[ERROR][{Name}]:\n" + ex.Message + "\n");
                CommandsList.ConsoleCommand("debug", null, this);
                Task.Run(() => TryToRestartSystem());
                return;
            }

                try
                {
                    StartTask();
                }
                catch (Exception ex)
                {
                    BotConsole.Write($"[ERROR][{Name} " + DateTime.Now.ToLongTimeString() + "]:\n" + ex.Message + "\n");
                    if (ex.Message == "User authorization failed: access_token has expired.")
                    {
                        this._app.RefreshToken();
                        BotConsole.Write($"[ERROR][{Name} " + DateTime.Now.ToLongTimeString() + "]: Токен обновлён.\n");
                    }
                    else TryToRestartSystem();

                    Thread.Sleep(platformSett.Delay);
                }
        }

        private void StartTask()
        {
            var schedulingService = new SchedulingService();
            schedulingService.At(time).Run(() =>
            {
                long[] ids = {7, 4, 3};

                for (int i = 0; i < ids.Length; i++)
                {
                    Message message = new Message();
                    message.ChatId = ids[i];
                    ReadOnlyCollection<long> users = _app.Messages.GetChatUsers(ids[i]);
                    Random rand = new Random();

                    int lalaka = rand.Next(0, users.Count);
                    while (users[lalaka] == 150887062 || users[lalaka] == 262045406 || users[lalaka] == 65533985 ||
                           users[lalaka] == 96534939)
                        lalaka = rand.Next(0, users.Count);

                    int top = lalaka;
                    while (top == lalaka) top = rand.Next(0, users.Count);

                    User lalakaUser = _app.Users.Get(users[lalaka]);
                    User topUser = _app.Users.Get(users[top]);

                    Functions.SendMessage(this, message,
                        $"[Неслучайный выбор дня]\n" +
                        $"Лалка дня 👎🏾: [id{users[lalaka]}|{lalakaUser.FirstName} {lalakaUser.LastName}]\n" +
                        $"Топ дня 👍: [id{users[top]}|{topUser.FirstName} {topUser.LastName}]", true);

                    Thread.Sleep(platformSett.Delay);
                }
                //});
                return null;
            });
            schedulingService.Start();
        }
    }
}
