using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BOT_Platform;
using VkNet.Model;
using System.IO;
using VkNet.Model.RequestParams;
using BOT_Platform.Interfaces;
using BOT_Platform.Kernel.Bots;

namespace MyFunctions
{
    class GroupWall : IMyCommands
    {
        static SortedDictionary<string, string> groupBD;

        public void AddMyCommandInPlatform()
        {
           CommandsList.TryAddCommand("найди", new MyComandStruct("найди(группа, список ключевых слов в постах)", Find));
        }

        void Find(Message message, string args, Bot bot)
        {
            if (NeedCommandInfo(message, args, bot)) return;
            #region
            string[] param = args.Split(new char[1] { ',' }, 2, StringSplitOptions.None);

            string[] data = param[0].Split('=');
            string domain = string.Empty;

            if (data.Length != 1) {
                domain = data[1];
                if (!groupBD.ContainsKey(data[0])) AddInGroupBD(data[0], data[1]);
            }
            else {
                if (groupBD.ContainsKey(data[0])) domain = groupBD[data[0]];
                else domain = data[0];
            }
            #endregion
            int indexSlash = domain.LastIndexOf('/');
            if (indexSlash != -1) domain = domain.Substring(indexSlash + 1);

            string club = "club";
            string publicC = "public";
            //string id   = "id";

            int indexClub = domain.IndexOf(club);
            int publicIndex = domain.IndexOf(publicC);
            //int indexId   = domain.IndexOf(id);
            long finalID;

            if (indexClub != -1) finalID = -Convert.ToInt32(domain.Substring(club.Length));
            else if (publicIndex != -1) finalID = -Convert.ToInt32(domain.Substring(publicC.Length));
            else                {finalID = -bot.GetApi().Groups.GetById(new string[1] { domain })[0].Id; }

            StringBuilder sB = new StringBuilder();
            sB.Append("[https://vk.com/wall");
            sB.Append(finalID );
            sB.Append("?owners_only=1&q=");
            sB.Append(Functions.CheckURL(param[1]) + "]");
            Functions.SendMessage(bot, message, sB.ToString(), message.ChatId != null);

        }

        private void AddInGroupBD(string key, string name)
        {
            groupBD.Add(key, name);
            using (StreamWriter file = new StreamWriter(
                   Environment.CurrentDirectory + "\\Data\\group_wall.txt", true))
            {
                file.WriteLine(key + "=" + name);
            }
        }

        public bool NeedCommandInfo(Message message, string args, Bot bot)
        {
            string info = info =
                $"Справка по команде \"{message.Body}\":\n\n" +
               "Бот ищет в группе, указанной в скобках (ссылке на группу), все посты, содержащие заданными ключевые слова/словосочетания/предложения.\n\n" +

               $"Пример: {bot.GetSettings().BotName[0]}, {message.Body}(https://vk.com/panda.panda, красные панды) - " +
               $"бот отправит ссылку на все посты, где упоминается \"красные панды\".\n" +
               $"Пример ответа для данного запроса:\n[https://vk.com/wall-29439161?owners_only=1&q=%20%D0%BA%D1%80%D0%B0%D1%81%D0%BD%D1%8B%D0%B5%20%D0%BF%D0%B0%D0%BD%D0%B4%D1%8B]";

            if (args == null || String.IsNullOrEmpty(args) || String.IsNullOrWhiteSpace(args))
            {
                Functions.SendMessage(bot, message, info, message.ChatId != null);
                return true;
            }
            return false;
        }

        public GroupWall()
        {
            AddMyCommandInPlatform();
            groupBD = new SortedDictionary<string, string>();

            FileInfo data = new FileInfo(Environment.CurrentDirectory +"\\Data\\group_wall.txt");
            if (!data.Exists)
            {
                File.Create(Environment.CurrentDirectory + "\\Data\\group_wall.txt");
                data = new FileInfo(Environment.CurrentDirectory + "\\Data\\group_wall.txt");
            }

            using(StreamReader sr = new StreamReader(data.OpenRead()))
            {
                string[] param;
                while (!sr.EndOfStream)
                {
                    param = sr.ReadLine().Split('=');
                    groupBD.Add(param[0], param[1]);
                }
            }
        }
    }
}
