#define PARALLEL

/*
 * TODO:
 * 2) Добавить бан-лист для каждой группы
 * 7) Синхонизированный вывод
 * 8) Проверить чат, в частности, как работает уведомление о недобавлении (там return) + свой чат для каждого бота
 * 12) ОГРАНИЧЕНИЕ В ЧАТ НА СПЛИТ
 * 14) Ответ на непрочитанные
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using BOT_Platform.Kernel.Bots;
using BOT_Platform.Kernel.CIO;
using BOT_Platform.Kernel.Interfaces;
using VkNet;
using System.Text;

namespace BOT_Platform.Kernel
{
    internal static partial class Program
    {
        const string DevNamespace = "MyFunctions";   /* Пространоство имён, содержащее только
                                                      * пользовательские функции
                                                      */
        const string UsersBots = @"Bots\UsersBots";
        const string GroupsBots = @"Bots\GroupsBots";
        const string OtherBots = @"Bots\OtherBots";

        private const string BotClasses = @"BOT_Platform.Kernel.Bots";

        public const string MainBot = "MainBot";

        /// <summary>
        /// Словарь всех ботов в системе
        /// </summary>
        public static volatile Dictionary<string, Bot> Bots;

        internal static Thread consoleThread;

        public static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            if (!Directory.Exists(UsersBots)) Directory.CreateDirectory(UsersBots);
            if (!Directory.Exists(GroupsBots)) Directory.CreateDirectory(GroupsBots);
            if (!Directory.Exists(OtherBots)) Directory.CreateDirectory(OtherBots);

            BotConsole.Write("---------------------------------------------------------------------");
            BotConsole.Write($"BOT_Platfrom v{Assembly.GetExecutingAssembly().GetName().Version}");
            BotConsole.Write("---------------------------------------------------------------------");

            BotConsole.Write("[Инициализация консоли...]");
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
            /* Считываем настройки бота из файла настроек */
            BotConsole.Write("[Загружаются параметры платформы...]");

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
                    string newCommand = BotConsole.Read();
                    Bot.CommandInfo info = GetCommandFromMessage(newCommand);
                    if(!String.IsNullOrEmpty(info.Command) ||
                       !String.IsNullOrEmpty(info.Param)) CommandsList.ConsoleCommand(info.Command, info.Param, null);
                }
                catch(Exception ex)
                {
                    BotConsole.Write("[ERROR][SYSTEM " + DateTime.Now.ToLongTimeString() + "]:\n" + ex.Message + "\n");
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
                if (!File.Exists(dirInfo.FullName + $@"\{Banlist.FILENAME}"))
                    File.Create(dirInfo.FullName + $@"\{Banlist.FILENAME}").Close();

                if (!File.Exists(dirInfo.FullName + $@"\{PlatfromSettings.PATH}"))
                {
                    File.Create(dirInfo.FullName + $@"\{PlatfromSettings.PATH}").Close();
                    res = true;
                }
                return res;
            }

            try
            {
                BotConsole.Write("[Подключение UserBots...]");
                foreach (DirectoryInfo dirInfo in (new DirectoryInfo(Environment.CurrentDirectory + $"\\{UsersBots}"))
                    .GetDirectories())
                {
                    if (AddSystem(dirInfo))
                        File.WriteAllText(dirInfo.FullName + $@"\{PlatfromSettings.PATH}",
                            Properties.Settings.Default.SettingsTemplateUserBot);
                    UserBot bot = new UserBot(dirInfo.Name, dirInfo.FullName);
                    if (bot.InitalizeBot()) Bots.Add(dirInfo.Name, bot);
                }
                BotConsole.Write("Done.");
                BotConsole.Write("[Подключение GroupsBots...]");
                foreach (DirectoryInfo dirInfo in (new DirectoryInfo(Environment.CurrentDirectory + $"\\{GroupsBots}"))
                    .GetDirectories())
                {
                    if (AddSystem(dirInfo))
                        File.WriteAllText(dirInfo.FullName + $@"\{PlatfromSettings.PATH}",
                            Properties.Settings.Default.SettingsTemplateGroupBot);
                    GroupBot bot = new GroupBot(dirInfo.Name, dirInfo.FullName);
                    if (bot.InitalizeBot()) Bots.Add(dirInfo.Name, bot);
                }
                BotConsole.Write("Done.");
                BotConsole.Write("[Подключение OthersBots...]");
                foreach (DirectoryInfo dirInfo in (new DirectoryInfo(Environment.CurrentDirectory + $"\\{OtherBots}"))
                    .GetDirectories())
                {
                    try
                    {
                        if (AddSystem(dirInfo))
                            File.WriteAllText(dirInfo.FullName + $@"\{PlatfromSettings.PATH}",
                                Properties.Settings.Default.SettingsTemplateGroupBot);

                        Type BotType = null;

                        BotType = Type.GetType(
                            $"{BotClasses}.{dirInfo.Name.Substring(1, dirInfo.Name.IndexOf(']') - 1)}",
                            false, true);

                        if (BotType != null)
                        {
                            ConstructorInfo ci = BotType.GetConstructor(
                                new Type[] {typeof(string), typeof(string)});

                            object bot = Activator.CreateInstance(type: BotType);

                            ci.Invoke(bot,
                                new object[]
                                {
                                    dirInfo.Name.Substring(dirInfo.Name.IndexOf(']') + 1).Replace(" ", ""),
                                    dirInfo.FullName
                                });
                            if ((bot as Bot).InitalizeBot()) Bots.Add(dirInfo.Name, (Bot) bot);
                        }
                        else
                        {
                            BotConsole.Write("---------------------------------------------------------------------");
                            BotConsole.Write($"[REFLECTION ERROR] Класс " +
                                              $"{dirInfo.Name.Substring(1, dirInfo.Name.IndexOf(']') - 1)} не найден");
                            BotConsole.Write("---------------------------------------------------------------------");
                        }
                    }
                    catch (Exception ex)
                    {
                        BotConsole.Write("---------------------------------------------------------------------");
                        BotConsole.Write($"[ERROR] Ошибка в записи {dirInfo.Name}: {ex.Message}");
                        BotConsole.Write("---------------------------------------------------------------------");
                    }
                }
                BotConsole.Write("Done.");
            }
            catch {}
            if (!Bots.ContainsKey(MainBot))
            {
                BotConsole.Write("---------------------------------------------------------------------");
                BotConsole.Write($"[FATAL ERROR] Не удалось подключить {MainBot}, многие функции могут быть недоступны.\n");
                BotConsole.Write("---------------------------------------------------------------------");
            }
        }

        /// <summary>
        /// Функция, подключающая все модули бота
        /// </summary>
        static void ExecuteModules()
        {
            BotConsole.Write("[Подключение модулей...]");

            /* Подключаем модули, создавая обьекты их классов */
            Type[] typelist = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == DevNamespace).ToArray();
            foreach (Type type in typelist)
            {
                Activator.CreateInstance(Type.GetType(type.FullName));
                BotConsole.Write("Подключение " + type.FullName + "...");
            }

            BotConsole.Write("Модули подключены.");
        }

    }
}

