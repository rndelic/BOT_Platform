using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using VkNet;
using VkNet.Enums.Filters;
using System.Text.RegularExpressions;
using VkNet.Model.RequestParams;

namespace BOT_Platform
{
    public class PlatformSettings
    {
        const char COMMENTS   = '$';

        MessagesGetParams mesParams;
        ApiAuthParams     apiParams;
        string[]          botName;
        string            botStatus;
        Regex             comRegex;
        Int16             mesRemeberCount;
        Int16             delay;

        public static string DATA_FILENAME = "Data\\BotData\\data.ini"; /* Файл с настройками. 
                                                                           * Распологается в одной папке с исполняемым файлом
                                                                           */

        public PlatformSettings()
        {
            FileInfo data = new FileInfo(DATA_FILENAME);
            try
            {
                if (!data.Exists) throw new FileLoadException("Отсутствует файл настроек!", "Ошибка");

                StringBuilder dataLine = new StringBuilder();
                using (StreamReader reader = data.OpenText())
                {

                    string tempText = reader.ReadToEnd();
                    #region Обработка настроек
                    bool findComments = false;
                    for (int i = 0; i < tempText.Length; i++)
                    {
                        if (tempText[i] == '\r' ||
                            tempText[i] == ' '   ||
                            tempText[i] == '\n' ||
                            tempText[i] == '\t'   ) continue;

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
                        throw new FileLoadException("Файл настроек пуст!", "Ошибка");
                    }
                }

                //dataLine.Replace("[SPACE]", " ");

                string[] splitSettings = {
                    "[LOGIN]","[PASSWORD]","[APP_ID]","[REGEX]","[PAUSE]",
                    "[MCOUNT]", "[MTOFF]", "[BNAME]", "[BSTATUS]","[BMESMEM]"
    };
                string[] appParams = dataLine.ToString().Split(splitSettings,
                                                         StringSplitOptions.RemoveEmptyEntries);

                if (appParams.Length != splitSettings.Length) throw new Exception("Ошибка при считывании настроек.\n" +
                                                                                  "Убедитесь, что они записаны верно.");

                this.apiParams = new ApiAuthParams()
                {
                    Login         = appParams[0],
                    Password      = appParams[1],
                    ApplicationId = Convert.ToUInt32(appParams[2]),
                    Settings      = Settings.All
                };
                this.mesParams = new MessagesGetParams()
                {
                    Count      = Convert.ToUInt16(appParams[3]),
                    Out        = 0,
                    TimeOffset = Convert.ToUInt32(appParams[4])//15
                };
                this.comRegex                = new Regex(appParams[5]);
                this.delay                   = Convert.ToInt16(appParams[6]);
                this.botName                 = appParams[7].Split(',');
                this.mesRemeberCount         = Convert.ToInt16(appParams[8]);
                this.IsDebug                 = true; 
            }

            catch (Exception ex)
            {
                Console.WriteLine("[ERROR][System]:\n" + ex.Message);
                Console.ReadKey();
                Environment.Exit(1);
            }
        }

        public Regex CommandRegex
        {
            get
            {
                return this.comRegex;
            }
        }
        public ApiAuthParams AuthParams
        {
            get
            {
                return this.apiParams;
            }
        }
        public MessagesGetParams MesGetParams
        {
            get
            {
                return this.mesParams;
            }
        }
        public Int16 Delay
        {
            get
            {
                return this.delay;
            }
        }
        public String[] BotName
        {
            get
            {
                return this.botName;
            }
        }

        public Int16 MesRemembCount
        {
            get { return this.mesRemeberCount; }
        }
        public bool IsDebug { get; set; }

    }
}
