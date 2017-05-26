using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using VkNet;
using VkNet.Enums.Filters;
using System.Text.RegularExpressions;
using BOT_Platform.Interfaces;
using VkNet.Model.RequestParams;

namespace BOT_Platform
{
    public class PlatfromSettings
    {
        protected const char COMMENTS = '$';
        protected static string DATA_FILENAME;
        public static string PATH = @"Data\System\settings.ini";

        protected MessagesGetParams mesParams;
        protected ApiAuthParams     apiParams;
        protected string[]          botName;
        protected Int16             mesRemeberCount;
        protected Int16             delay;
        protected Dictionary<long, bool> adminList;

        protected virtual void ReadSettings()
        {
            FileInfo data = new FileInfo(DATA_FILENAME);

                if (!data.Exists) throw new FileLoadException($"Отсутствует файл настроек {DATA_FILENAME}!", "Ошибка");

                StringBuilder dataLine = new StringBuilder();
                using (StreamReader reader = data.OpenText())
                {

                    string tempText = reader.ReadToEnd();
                    #region Обработка настроек
                    bool findComments = false;
                    for (int i = 0; i < tempText.Length; i++)
                    {
                        if (tempText[i] == '\r' ||
                            tempText[i] == ' ' ||
                            tempText[i] == '\n' ||
                            tempText[i] == '\t') continue;

                        if (tempText[i] == COMMENTS)
                        {
                            findComments = !findComments;
                            continue;
                        }
                        if (findComments == true) continue;

                        dataLine.Append(tempText[i]);
                    }
                    #endregion

                    if (String.IsNullOrEmpty(dataLine.ToString()) == true)
                    {
                        throw new FileLoadException($"Файл настроек {DATA_FILENAME} пуст!", "Ошибка");
                    }
                }

                //dataLine.Replace("[SPACE]", " ");

                string[] splitSettings = {
                    "[LOGIN]","[PASSWORD]","[APP_ID]","[PAUSE]",
                    "[MCOUNT]", "[MTOFF]", "[BNAME]","[BMESMEM]", "[ADMIN_LIST]"
                };
                string[] appParams = dataLine.ToString().Split(splitSettings,
                    StringSplitOptions.RemoveEmptyEntries);

                if (appParams.Length != splitSettings.Length) throw new Exception($"Ошибка при считывании настроек {DATA_FILENAME}.\n" +
                                                                                  "Убедитесь, что они записаны верно.");

                this.apiParams = new ApiAuthParams()
                {
                    Login = appParams[0],
                    Password = appParams[1],
                    ApplicationId = Convert.ToUInt32(appParams[2]),
                    Settings = Settings.All

                };

                this.mesParams = new MessagesGetParams()
                {
                    Count = Convert.ToUInt16(appParams[3]),
                    Out = 0,
                    TimeOffset = Convert.ToUInt32(appParams[4])//15
                };

                this.delay = Convert.ToInt16(appParams[5]);
                this.botName = appParams[6].Split(',');
            for (int i = 0; i < botName.Length; i++)
            {
                botName[i] = Functions.RemoveSpaces(botName[i]);
            }

            this.mesRemeberCount = Convert.ToInt16(appParams[7]);

                adminList = new Dictionary<long, bool>();
                string[] admins = appParams[8].Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < admins.Length; i++)
                {
                    adminList.Add(Convert.ToInt32(admins[i]), true);
                }

                this.SetIsDebug(true);
        }
        public PlatfromSettings(string filename)
        {
            DATA_FILENAME = filename;
            ReadSettings();
        }
        public ApiAuthParams AuthParams => apiParams;

        public MessagesGetParams MesGetParams => this.mesParams;

        public Int16 Delay => this.delay;

        public String[] BotName => this.botName;

        public long[] Admins => this.adminList.Keys.ToArray();

        public Int16 MesRemembCount => this.mesRemeberCount;

        private bool isDebug;

        public bool GetIsDebug()
        {
            return isDebug;
        }
        public void SetIsDebug(bool value)
        {
            isDebug = value;
        }
    }
    public class GroupSettings : PlatfromSettings
    {
        public string Token { get; private set; }
        public ulong ApplicationId { get; private set; }

        protected override void ReadSettings()
        {
            FileInfo data = new FileInfo(DATA_FILENAME);
                if (!data.Exists) throw new FileLoadException($"Отсутствует файл настроек {DATA_FILENAME}!", "Ошибка");

                StringBuilder dataLine = new StringBuilder();
                using (StreamReader reader = data.OpenText())
                {

                    string tempText = reader.ReadToEnd();
                    #region Обработка настроек
                    bool findComments = false;
                    for (int i = 0; i < tempText.Length; i++)
                    {
                        if (tempText[i] == '\r' ||
                            tempText[i] == ' ' ||
                            tempText[i] == '\n' ||
                            tempText[i] == '\t') continue;

                        if (tempText[i] == COMMENTS)
                        {
                            findComments = !findComments;
                            continue;
                        }
                        if (findComments == true) continue;

                        dataLine.Append(tempText[i]);
                    }
                    #endregion

                    if (String.IsNullOrEmpty(dataLine.ToString()) == true)
                    {
                        throw new FileLoadException($"Файл настроек {DATA_FILENAME} пуст!", "Ошибка");
                    }
                }

                //dataLine.Replace("[SPACE]", " ");

                string[] splitSettings = {
                    "[TOKEN]", "[APP_ID]","[PAUSE]",
                    "[MCOUNT]", "[MTOFF]", "[BNAME]", "[BMESMEM]", "[ADMIN_LIST]"
                };
                string[] appParams = dataLine.ToString().Split(splitSettings,
                    StringSplitOptions.RemoveEmptyEntries);

                if (appParams.Length != splitSettings.Length) throw new Exception($"Ошибка при считывании настроек {DATA_FILENAME}.\n" +
                                                                                  "Убедитесь, что они записаны верно.");

                Token         = appParams[0];
                ApplicationId = Convert.ToUInt32(appParams[1]);
                this.delay    = Convert.ToInt16(appParams[2]);

                this.mesParams = new MessagesGetParams()
                {
                    Count = Convert.ToUInt16(appParams[3]),
                    Out = 0,
                    TimeOffset = Convert.ToUInt32(appParams[4])
                };

                this.botName = appParams[5].Split(',');
            for (int i = 0; i < botName.Length; i++)
            {
                botName[i] = Functions.RemoveSpaces(botName[i]);
            }

                this.mesRemeberCount = Convert.ToInt16(appParams[6]);
                SetIsDebug(true);

            apiParams = new ApiAuthParams()
            {
                Settings = Settings.All
            };

                adminList = new Dictionary<long, bool>();
                string[] admins = appParams[7].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < admins.Length; i++)
            {
                adminList.Add(Convert.ToInt32(admins[i]), true);
            }
        }

        public GroupSettings(string filename) :base(filename)
        {
        }
    }


}
