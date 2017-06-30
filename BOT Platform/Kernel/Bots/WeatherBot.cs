using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
    class WeatherBot : UserBot
    {
        public WeatherBot(string botName, string directory) : base(botName, directory)
        { }

        public WeatherBot() : base(null, null)
        {
        }
        // 24 по лабе =  20 по ноуту
        private string time = $"0 9 * * *"; 

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
                long[] ids = { 7, 4, 3 };

                for (int i = 0; i < ids.Length; i++)
                {
                    Message message = new Message();
                    message.ChatId = ids[i];

                    string weather = GetWeather();

                    Functions.SendMessage(this, message,
                        weather, true);
                    Thread.Sleep(platformSett.Delay);
                }
                //});
                return null;
            });
            schedulingService.Start();
        }

        private string GetWeather()
        {
            WebRequest request;
            request = WebRequest.Create(@"http://www.meteoservice.ru/weather/now/sankt-peterburg.html");
            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string data = reader.ReadToEnd();
                    string town = new Regex(@"<h1>(?<town>.*)</h1>").Match(data).Groups["town"].Value;
                    string temp = new Regex(@"<span class=""temperature"">(?<temp>[^<]+)").Match(data).Groups["temp"].Value.Replace(@"&deg;", "°");
                    string osadki = new Regex(@"<td class=""title"">Облачность:</td>[^<]*?<td>(?<osadki>[^<]+)</td>").Match(data).Groups["osadki"].Value;
                    return (town + "\n🌡 Температура воздуха: " + temp + "\n☔️ Осадки: " + osadki);
                    Console.ReadLine();
                }
            }
        }
    }
}
