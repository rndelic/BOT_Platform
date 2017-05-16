using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model;
using VkNet.Model.RequestParams;
using BOT_Platform;
using System.Text.RegularExpressions;
using VkNet.Enums.SafetyEnums;
using BOT_Platform.Interfaces;

namespace MyFunctions
{
    class MainFunctions : IMyCommands
    {
        public void AddMyCommandInPlatform()
        { 
            CommandsList.TryAddCommand("анонимно", new MyComandStruct(
                                       "Пример: анонимно(id получателя или ссылка, текст сообщения)", AnonimSend));
            CommandsList.TryAddCommand("скажи", new MyComandStruct(
                                       "Пример: скажи(текст сообщения).", Say));
            CommandsList.TryAddCommand("команды", new MyComandStruct(
                                       "Показывает список всех команд", ShowCommands));

            CommandsList.TryAddCommand("рандом", new MyComandStruct(
                                       "Пример: рандом, или рандом(макс знач.), или рандом(мин,макс)", BRandom));
            CommandsList.TryAddCommand("что", new MyComandStruct(
                                       "Пример: что(1 вариант,2вариант,...)", What));
            CommandsList.TryAddCommand("лайкни", new MyComandStruct(
                                       "Пример: лайкни(открытая ссылка на фото или пост)", Like));
            CommandsList.TryAddCommand("вычисли", new MyComandStruct(
                                       "Пример: вычисли(выражение с /,*,+,-) beta = баги", Solve));
            CommandsList.TryAddCommand("поддержка", new MyComandStruct(
                                       "Обратиться к разработчику", Support));
        }

        public MainFunctions()
        {
            AddMyCommandInPlatform();
        }
        void ShowCommands(Message message, params object[] p)
        {
            List<string> com = CommandsList.GetCommandList(true);
            StringBuilder sb = new StringBuilder();
            sb.Append("Напоминание! Все параметры пишутся внутри единых скобок через разделитель!\nСписок команд:\n");

            foreach (string value in com)
            {
                sb.Append(BOT_API.GetSettings().BotName[0] + ", " + value + "\n");
            }

            Functions.SendMessage(message, sb.ToString(), message.ChatId != null);
        }
        void Solve(Message message, params object[] p)
        {
            if (NeedCommandInfo(message, p)) return;
            Functions.SendMessage(message, SolveExample(p[0].ToString()), message.ChatId != null);
        }

        void Like(Message message, params object[] p)
        {
            if (NeedCommandInfo(message, p)) return;
            string http = p[0].ToString();

            string foundPhoto = "photo";
            string foundWall  = "wall";

            LikeObjectType lType;

            int photoIndex = http.IndexOf(foundPhoto);
            int wallIndex = http.IndexOf(foundWall);

            if (photoIndex != -1)
            {
                lType = LikeObjectType.Photo;
                http = http.Substring(photoIndex + foundPhoto.Length);
            }
            else if (wallIndex != -1)
            {
                lType = LikeObjectType.Post;
                http = http.Substring(wallIndex + foundWall.Length);
            }
            else return;

            string[] param = http.Split(new char[1] { '_' },2, StringSplitOptions.None);

            StringBuilder sB = new StringBuilder();
            for (int i = 0; i < param[1].Length; i++)
            {
                if (Char.IsDigit(param[1][i])) sB.Append(param[1][i]);
                else break;
            }
            LikesAddParams lAP = new LikesAddParams
            {
                Type = lType,
                OwnerId = Convert.ToInt32(param[0]),
                ItemId  = Convert.ToInt32(sB.ToString())
            };
            try
            {
                BOT_API.GetApi().Likes.Add(lAP);
            }
            catch (VkNet.Exception.VkApiException ex)
            {
                Functions.SendMessage(message, "К сожалению, обьект находится в частном доступе :/", message.ChatId != null);
                return;
            }

            Functions.SendMessage(message, "👌", message.ChatId != null);
        }

        void What(Message message, params object[] p)
        {
            if (NeedCommandInfo(message, p)) return;
            string[] param = p[0].ToString().Split(new char[1] { ',' }, StringSplitOptions.None);
            Random rand = new Random();

            Functions.SendMessage(message, param[rand.Next(0, param.Length)], message.ChatId != null);
        }

        void Support(Message message, params object[] p)
        {
            Functions.SendMessage(message, "https://vk.com/dedsec_alexberezhnyh", message.ChatId != null);
        }

        void BRandom(Message message, params object[] p)
        {
            if (NeedCommandInfo(message, p)) return;
            Random rand = new Random();
            if(p[0] == null) Functions.SendMessage(message, "🎲 " + rand.Next().ToString(), message.ChatId != null);
            else
            {
                string[] param = p[0].ToString().Split(new char[1]{ ',' }, 2, StringSplitOptions.RemoveEmptyEntries); 
                if(param.Length == 1)
                    Functions.SendMessage(message, "🎲 " + rand.Next(Convert.ToInt32(param[0])+1).ToString(), message.ChatId != null);
                else
                    Functions.SendMessage(message, "🎲 " + rand.Next(Convert.ToInt32(param[0]), Convert.ToInt32(param[1])+1).ToString(), message.ChatId != null);
            }
        }

        void Say(Message message, params object[] p)
        {
            if (NeedCommandInfo(message, p)) return;
            string text = Functions.RemoveSpaces(p[0].ToString());
            if (text[0] == '!' && text.Length >= 2)
            {
                string textToSpeech = text.Substring(1);
                if(!String.IsNullOrWhiteSpace(textToSpeech))
                    SpeechText.Speech(message, textToSpeech);

                else Functions.SendMessage(message, text, message.ChatId != null);
            }
            else Functions.SendMessage(message, text, message.ChatId != null);
        }

        void AnonimSend(Message message, params object[] p)
        {
            if (NeedCommandInfo(message, p)) return;
            string[] param = p[0].ToString().Split(new char[1] { ',' }, 2, StringSplitOptions.None);

            long? answerId = message.UserId;
            Functions.RemoveSpaces(ref param[1]);

            if (param[0].Contains("\""))
            {
                AnonChat.ChatSend(message, param[0], param[1]);
                return;
            }
            #region
            /*
            param[0] = param[0].Replace(" ", "");
            int index = param[0].ToString().LastIndexOf('/');
            if (index != -1) param[0] = param[0].ToString().Substring(index + 1);
            
            Regex reg = new Regex("[a-z|A-Z]");
            bool regexIsFoundInFull = reg.IsMatch(param[0]);

            //Если нашёл "id"
            index = param[0].ToString().IndexOf("id");
            if (index == 0)
            {
                if (!reg.IsMatch(param[0].ToString().Substring(2)))
                {
                    param[0] = param[0].ToString().Substring(2);
                }
                else
                {
                    param[0] = BOT_API.GetApi().Users.Get(param[0]).Id.ToString();
                }
            }

            else if (reg.IsMatch(param[0]))
            {
                param[0] = BOT_API.GetApi().Users.Get(param[0]).Id.ToString();
            }
            */
            #endregion
            Functions.GetUserId(ref param[0]);
            message.UserId = Convert.ToInt32(param[0]); ;

            try
            {
                if (param[1][0] != '!')
                    Functions.SendMessage(message, "Служба анонимной почты, вам письмо:\n" + param[1]);
                
                else
                {
                    Functions.SendMessage(message, SpeechText.MakeSpeechAttachment(param[1].Substring(1), message),
                        "Служба анонимной почты, вам аудиосообщение:\n");
                }   
            }
            catch (VkNet.Exception.VkApiException ex)
            {
                message.UserId = answerId;
                Functions.SendMessage(message, "К сожалению, я не могу отправить сообщение этому человеку.\n" +
                                     "Такие дела ¯\\_(ツ)_/¯.", message.ChatId != null);
                return;
            }
            message.UserId = answerId;
            Functions.SendMessage(message, "Доставлено!", message.ChatId!=null);
            }

        /* Этот говнкод я писал оооочень давно */
        private string SolveExample(string test)
        {

            try
            {
                Regex priority1 = new Regex("[*/]");
                Regex priority2 = new Regex("[+-]");
                Regex numb = new Regex("[0-9]");
                if (numb.IsMatch(test) == false
                    && (priority1.IsMatch(test) == false || priority2.IsMatch(test) == false))
                {
                    return "Решение не найдено. Проверьте, введено ли хотя бы одно число или знак";
                }

                test = test.Replace("e", Math.E.ToString());
                test = test.Replace("е", Math.E.ToString());
                test = test.Replace("Pi", Math.PI.ToString());

                #region

                string example = test;
                int _startSearch = 0, _finishSearch = 0;
                bool newOperatorBool = false, startParse = false;
                bool scoba = false;
                #endregion
                List<string> numbersList = new List<string>();

                while (numbersList.Count != 1)
                {
                    #region очистка
                    numbersList.Clear();
                    string _operator = " ";
                    string before = "", after = "";

                    int startPos = 0, lastIndex = 0;
                    int NextNumber = 0, PrevNumber = 0;
                    #endregion


                    while (true)
                    {
                        if (example.Contains(')') == true && (example.Contains('(') == false
                    || (example.LastIndexOf('(') > example.IndexOf(')') &&
                        example.IndexOf('(') > example.IndexOf(')'))))
                        {
                            example = example.Remove(example.IndexOf(')'), 1);
                        }

                        if (example.Contains('(') == true && (example.Contains(')') == false
                  || (example.IndexOf(')') < example.LastIndexOf('(')
                      && example.LastIndexOf(')') < example.LastIndexOf(')'))))
                        {
                            example = example.Remove(example.LastIndexOf('('), 1);
                        }
                        else
                        {
                            #region Replace
                            example = example.Replace("=", "");
                            example = example.Replace(" ", "");
                            example = example.Replace("/+", "/");
                            example = example.Replace("*+", "*");
                            example = example.Replace("++", "+");
                            example = example.Replace("(+", "(");
                            example = example.Replace("%", "");
                            example = example.Replace("^", "");
                            example = example.Replace("&", "");
                            example = example.Replace("<", "");
                            example = example.Replace(">", "");
                            example = example.Replace("?", "");
                            example = example.Replace("!", "");
                            example = example.Replace("@", "");
                            example = example.Replace("#", "");
                            example = example.Replace("$", "");
                            example = example.Replace(";", "");
                            example = example.Replace(":", "");
                            example = example.Replace("'", "");
                            example = example.Replace("\\", "");
                            example = example.Replace("{", "");
                            example = example.Replace("}", "");
                            example = example.Replace("_", "");
                            example = example.Replace("[", "");
                            example = example.Replace("]", "");
                            example = example.Replace("|", "");

                            example = example.Replace("--", "+");
                            example = example.Replace("-", " -");
                            example = example.Replace("+", " + ");
                            example = example.Replace("/", " / ");
                            example = example.Replace("*", " * ");
                            example = example.Replace("(", "( ");
                            example = example.Replace(")", " ) ");
                            example = example.Replace(".", ",");
                            example = example + " ";

                            Regex temp = new Regex("\\s*[+-]\\s*\\d*\\s*");
                            if(temp.Match(example).Value == example)
                            {
                                example = example.Replace(" ", "").Replace("+", "");
                                return example;
                            }

                            #endregion
                            break;
                            

                        }
                    }

                    if (example.IndexOf(')') >= 0 && example.IndexOf(')') < example.Length && example.Contains('(') == true)
                    {
                        scoba = true;
                        startPos = _startSearch = example.Substring(0, example.IndexOf(')')).LastIndexOf('(') + 1;
                        _finishSearch = example.IndexOf(')');
                        before = example.Substring(0, _startSearch);
                        after = example.Substring(_finishSearch, example.Length - _finishSearch);
                    }

                    else
                    {
                        _startSearch = 0;
                        _finishSearch = example.Length;
                    }

                    for (int i = _startSearch; i < _finishSearch; i++)
                    {
                        if (example[i] == ' ')
                        {
                            string newObjectToNumberList = example.Substring(startPos, i - startPos);


                            if (newObjectToNumberList.Contains(' ') ||
                                String.IsNullOrEmpty(newObjectToNumberList) == true)
                            {

                                continue;
                            }


                            numbersList.Add(newObjectToNumberList);
                            startPos = _startSearch;
                            startParse = false;


                            if (priority1.IsMatch(numbersList[lastIndex])
                                && numbersList[lastIndex].Length == 1)
                            {
                                if (priority1.IsMatch(_operator) == false)
                                {
                                    newOperatorBool = true;
                                }
                            }
                            if (priority2.IsMatch(numbersList[lastIndex])
                                && priority1.IsMatch(_operator) == false
                                && numbersList[lastIndex].Length == 1)
                            {
                                if (priority2.IsMatch(_operator) == false)
                                {
                                    newOperatorBool = true;
                                }
                            }

                            if (newOperatorBool == true)
                            {
                                _operator = numbersList[lastIndex];
                                NextNumber = lastIndex + 1;
                                PrevNumber = lastIndex - 1;
                                newOperatorBool = !newOperatorBool;


                            }

                            ++lastIndex;
                            continue;
                        }
                        if ((startPos == 0 || (startPos == _startSearch && scoba == true)) && startParse == false)
                        {
                            startPos = i;
                            startParse = true;
                        }

                    }


                    if (numbersList.Count == 1)
                    {

                        if (scoba == false)
                        {
                            break;
                        }
                        else
                        {
                            example = example.Remove(_startSearch - 1, 1);
                            example = example.Remove(_finishSearch - 1, 1);
                            example = before.Substring(0, before.Length - 1) + example.Substring(_startSearch, _finishSearch - _startSearch)
                                                                                              + after.Substring(1, after.Length - 1);
                            scoba = false;
                            numbersList.Clear();
                            continue;
                        }

                    }
                    if (numbersList[NextNumber] == "0" && _operator == "/")
                    {
                        return "Нельзя делить на нуль!";
                    }

                    string a = numbersList[PrevNumber];
                    string b;
                    if (numbersList.Count != 2)
                    {
                        b = numbersList[NextNumber];
                    }
                    else b = numbersList[PrevNumber + 1];

                    if (a[0] == ',') a = "0" + a;
                    if (b[0] == ',') b = "0" + b;

                    string c = string.Empty;

                    switch (_operator)
                    {

                        case "+":
                            c = LongAr.Plus(a, b);

                            break;

                        case "-":
                            // c = a - b;
                            break;

                        case "/":
                            double q = Convert.ToDouble(a), e = Convert.ToDouble(b);
                            double res = q / e;
                            c = res.ToString();
                            break;

                        case "*":
                            c = LongAr.Multiply(a, b);
                            break;
                        default:

                            if (numbersList[PrevNumber + 1][0] == '-')
                            {
                                c = LongAr.Plus(a, b);
                                numbersList.Add(numbersList[PrevNumber + 1]);

                            }
                            else
                            {
                                return "Необрабатываемый символ!";
                            }
                            break;
                    }
                    while (c[0] == '0' && c.IndexOf(',') != 1 && c.Length != 1) c = c.Substring(1);
                    while ((c[c.Length - 1] == '0' || c[c.Length - 1] == ',') && c.IndexOf(",") != -1) c = c.Substring(0, c.Length - 1);

                    if (c.Length > 2)
                    {
                        while (((c[0] == '-' && c[1] == '0' && c[2] != ',') || (c[c.Length - 1] == '0' && c.Contains(','))))
                        {
                            if (c[0] == '-' && c[1] == '0' && c[2] != ',') c = "-" + c.Substring(2);
                            if (c[c.Length - 1] == '0' && c.Contains(',')) c = c.Substring(0, c.Length - 1);
                        }
                        if (c.Length == 2 && c.Contains(',') == true) c = c.Substring(0, 1);
                    }

                    numbersList[PrevNumber + 1] = c.ToString();
                    if (numbersList[PrevNumber][0] == '-')
                    {
                        numbersList[PrevNumber + 1] = "+" + numbersList[PrevNumber + 1];
                    }
                    numbersList.RemoveAt(PrevNumber);
                    numbersList.RemoveAt(PrevNumber + 1);

                    example = "";
                    foreach (string str in numbersList)
                    {
                        example += str + " ";
                    }
                    if (scoba == true)
                    {
                        example = before + example + after;
                        numbersList.Clear();
                        continue;
                    }



                    if (example[0] == '+') example = example.Remove(0, 1);
                    example = " " + example;

                }

                return example;
            }
            catch (Exception ex)
            {
                return "Пример записан неверно!\n\n" + ex.Message;  
            }

        }

        public bool NeedCommandInfo(Message message, params object[] p)
        {
            string info = "";
            switch (message.Body)
            {
                case "анонимно":
                    info =
                    $"Справка по команде \"{message.Body}\":\n\n" +
                    "Команда отправляет пользователю анонимное сообщение, где не будет указано, от кого оно.\n\n" +
                    $"Для того, чтобы отправить сообщение пользователю, напишите {BOT_API.GetSettings().BotName[0]}, {message.Body}(ссылка или id пользователя, текст сообщения)\n" +
                    $"Пример: {BOT_API.GetSettings().BotName[0]}, {message.Body}(vk.com/hello_bot, привет)\n" +
                    $"Пользователь получит сообщение: \n" +
                    $"Служба анонимной почты, вам письмо: привет\n\n" +
                    $"Для того, чтобы отправить аудиосообщение пользователю, напишите {BOT_API.GetSettings().BotName[0]}, {message.Body}(ссылка или id пользователя, !текст сообщения) - поставьте ! перед текстом сообщения\n" +
                    $"Пример: {BOT_API.GetSettings().BotName[0]}, {message.Body}(vk.com/hello_bot, !привет)\n" +
                    $"Пользователь получит сообщение: \n" +
                    $"Служба анонимной почты, вам аудиосообщение: *{SpeechText.NAME} - привет* <- аудиозапись.\n\n" +
                    $"ВНИМАНИЕ! Если сообщение было успешно доставлено, бот ответит вам \"Доставлено!\"";
                    break;

                case "вычисли":
                    info =
                    $"Справка по команде \"{message.Body}\":\n\n" +
                    "Бот вычисляет математическое выражение (в том числе длинную дробную арифметику) со стандартными операциями (+-*/), указанное в скобках.\n\n" +
                    $"Пример: {BOT_API.GetSettings().BotName[0]}, {message.Body}( ((25/5) - 3) *2) )\n" +
                    $"Пользователь получит ответ: 4\n\n" +
                    "Обратите внимание, вне зависимости от того, есть ли в выражении свои скобки или нет  (25/5) - 3) * 2, всё выражение должно быть указано в главных скобках:  ((25/5) - 3) * 2)";
                    break;
                default:
                    info = $"Справка для команды \"{message.Body}\" отсутствует. Обратитесь к разработчику: https://vk.com/dedsec_alexberezhnyh";
                    break;
            }
            if (p[0] == null || String.IsNullOrEmpty(p[0].ToString()) || String.IsNullOrWhiteSpace(p[0].ToString()))
            {
                Functions.SendMessage(message, info, message.ChatId != null);
                return true;
            }
            return false;
        }
    }
}