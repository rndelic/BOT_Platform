#define PARALLEL

/*
 * TODO:
 * 1) Улучшить описание
 * 2) Добавить бан-лист для каждой группы
 * 7) Синхонизированный вывод
 * 8) Проверить чат, в частности, как работает уведомление о недобавлении (там return) + свой чат для каждого бота
 * 9) Перенаправление для невыполнимых функций (+)
 * 10) Фикс консоли
 * 11) Рестарт отдельного потока
 * 12) ОГРАНИЧЕНИЕ В ЧАТ НА СПЛИТ
 * 13) Action для синхронизации между ботами
 * 14) Ответ на непрочитанные
 */
using System;
using VkNet;
using System.Text.RegularExpressions;
using System.Threading;
using System.Reflection;
using System.Linq;
using VkNet.Model;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using BOT_Platform.Interfaces;
using System.Threading.Tasks;
using VkNet.Enums.Filters;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace BOT_Platform
{
    static partial class BOT_API
    {
        //TODO: REMOVE
        public static VkApi GetApi()
        {
            return new VkApi();
        }

        public static PlatfromSettings GetSettings()
        {
            return null;
        }

        const string DevNamespace = "MyFunctions";   /* Пространоство имён, содержащее только
                                                      * пользовательские функции
                                                      */
        const string UsersBots = "UsersBots";
        const string GroupsBots = "GroupsBots";
        public const string MainBot = "MainBot";

        public static volatile Dictionary<string, Bot> Bots;

        internal static Thread consoleThread;

        public static void Main()
        {
            if (!Directory.Exists(UsersBots)) Directory.CreateDirectory(UsersBots);
            if (!Directory.Exists(GroupsBots)) Directory.CreateDirectory(GroupsBots);

            Console.WriteLine("[Инициализация консоли...]");
            /* Подключаем стандартный модуль с базовыми командами */
            StandartCommands sC = new StandartCommands();

            /* Запускаем обратчик команд */
            CommandsList.ConsoleCommand("restart", null, null);
        }

        public static event EventHandler ClearCommands;
        /// <summary>
        /// Функция - обработчик команд, поступающих в консоль
        /// </summary>
        internal static void ConsoleCommander()
        {
            //TODO: CHANGE TITLE
            /* Считываем настройки бота из файла настроек */
            Console.WriteLine("[Загружаются параметры платформы...]");

            if(Bots != null)
                foreach (var bot in Bots.Values)
                {
                    bot.Abort();
                }
            Bots = new Dictionary<string, Bot>();

            string s = CommandsList.Log.logFile;  //заглушка
            CommandsList.ConsoleCommand("start", null, null); //заглушка
            ClearCommands.Invoke(new object(), null);

            /* Подключаем модули */
            ExecuteModules();
            /* Подключаем ботов */
            ExecuteBots();

            /* Создаём поток обработки ботом поступающих сообщений */
            CommandsList.ConsoleCommand("undebug", null, null);
            /* Обрабатываем команды, поступающие в консоль */
            
            while (true)
            {
                try
                {
                    string newCommand = Console.ReadLine();
                    Bot.CommandInfo info = GetCommandFromMessage(newCommand);
                    if(!String.IsNullOrEmpty(info.Command) ||
                       !String.IsNullOrEmpty(info.Param)) CommandsList.ConsoleCommand(info.Command, info.Param, null);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("[ERROR][SYSTEM " + DateTime.Now.ToLongTimeString() + "]:\n" + ex.Message + "\n");
                }
            }
            
        }
        static Bot.CommandInfo GetCommandFromMessage(string consoleMessage)
        {
            int index = consoleMessage.IndexOf('(');

            string command = consoleMessage;
            string param = null;

            if (index != -1)
            {
                command = consoleMessage.Substring(0, index);
                param = consoleMessage.Substring(index + 1);
                int ind = param.LastIndexOf(')');
                if (ind == -1)
                {
                    param += ')';
                    param = param.Remove(param.Length - 1);
                }
                else param = param.Remove(ind);
            }
            Functions.RemoveSpaces(ref command);

            if (!String.IsNullOrEmpty(command) && 
                Char.IsUpper(command[0])) command = Char.ToLower(command[0]) + command.Substring(1);
            return new Bot.CommandInfo(command, param);
        }
        private static void ExecuteBots()
        {
            bool AddSystem(DirectoryInfo dirInfo)
            {
                bool res = false;
                if (!Directory.Exists(dirInfo.FullName + @"\Data"))
                    Directory.CreateDirectory(dirInfo.FullName + @"\Data");
                if (!Directory.Exists(dirInfo.FullName + @"\Data\System"))
                    Directory.CreateDirectory(dirInfo.FullName + @"\Data\System");

                if (!File.Exists(dirInfo.FullName + $@"\{CommandsList.Log.logFile}"))
                    File.Create(dirInfo.FullName + $@"\{CommandsList.Log.logFile}").Close();
                if (!File.Exists(dirInfo.FullName + $@"\{PlatfromSettings.PATH}"))
                {
                    File.Create(dirInfo.FullName + $@"\{PlatfromSettings.PATH}").Close();
                    res = true;
                }
                return res;
            }

            try
            {
                foreach (DirectoryInfo dirInfo in (new DirectoryInfo(Environment.CurrentDirectory + $"\\{UsersBots}")).GetDirectories())
                {
                    if(AddSystem(dirInfo))
                        File.WriteAllText(dirInfo.FullName + $@"\{PlatfromSettings.PATH}",
                            Properties.Settings.Default.SettingsTemplateUserBot);
                    UserBot bot = new UserBot(dirInfo.Name, dirInfo.FullName);
                    if(bot.InitalizeBot()) Bots.Add(dirInfo.Name, bot);
                }
                foreach (DirectoryInfo dirInfo in (new DirectoryInfo(Environment.CurrentDirectory + $"\\{GroupsBots}")).GetDirectories())
                {
                    if(AddSystem(dirInfo))
                        File.WriteAllText(dirInfo.FullName + $@"\{PlatfromSettings.PATH}", 
                            Properties.Settings.Default.SettingsTemplateGroupBot);
                    GroupBot bot = new GroupBot(dirInfo.Name, dirInfo.FullName);
                    if (bot.InitalizeBot()) Bots.Add(dirInfo.Name, bot);
                }
            }
            catch
            {
            }
            if (!Bots.ContainsKey(MainBot))
            {
                Console.WriteLine($"[FATAL ERROR] Не удалось подключить {MainBot}, многие функции могут быть недоступны.\n");
            }
        }

        /// <summary>
        /// Функция, подключающая все модули бота
        /// </summary>
        static void ExecuteModules()
        {
            Console.WriteLine("[Подключение модулей...]");

            /* Подключаем модули, создавая обьекты их классов */
            Type[] typelist = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == DevNamespace).ToArray();
            foreach (Type type in typelist)
            {
                Activator.CreateInstance(Type.GetType(type.FullName));
                Console.WriteLine("Подключение " + type.FullName + "...");
            }

            Console.WriteLine("Модули подключены.");
        }

    }
}

