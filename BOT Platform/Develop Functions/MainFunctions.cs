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
using BOT_Platform.Kernel;
using BOT_Platform.Kernel.Bots;
using MyFunctions.Exceptions;

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
        void ShowCommands(Message message, string args, Bot bot)
        {
            List<string> com = CommandsList.GetCommandList(true);
            StringBuilder sb = new StringBuilder();
            sb.Append("Напоминание! Все параметры пишутся внутри единых скобок через разделитель (,)!\nСписок команд:\n");

            foreach (string value in com)
            {
                sb.Append(bot.GetSettings().BotName[0] + ", " + value + "\n");
            }

            sb.Append(
                "\nДля получения более подробной справки по команде напишите её без параметров. Например: бот, вики() или бот, вики");

            Functions.SendMessage(bot, message, sb.ToString(), message.ChatId != null);
        }
        void Solve(Message message, string args, Bot bot)
        {
            if (NeedCommandInfo(message, args, bot)) return;
            Functions.SendMessage(bot, message, SolveExample(args), message.ChatId != null);
        }

        void Like(Message message, string args, Bot bot)
        {
            if (NeedCommandInfo(message, args, bot)) return;
            string http = args.ToString();

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
                if (bot is GroupBot)
                {
                    if (Program.Bots.ContainsKey(Program.MainBot))
                        Program.Bots[Program.MainBot].GetApi().Likes.Add(lAP);
                    else
                        throw new WrongParamsException(
                            "В данный момент бот не может лайкать ваши записи ;c");
                }

                else bot.GetApi().Likes.Add(lAP);
            }
            catch (VkNet.Exception.VkApiException ex)
            {
                Functions.SendMessage(bot, message, "К сожалению, обьект находится в частном доступе :/", message.ChatId != null);
                return;
            }

            Functions.SendMessage(bot, message, "👌", message.ChatId != null);
        }

        void What(Message message, string args, Bot bot)
        {
            if (NeedCommandInfo(message, args, bot)) return;
            string[] param = args.Split(new char[1] { ',' }, StringSplitOptions.None);
            Random rand = new Random();

            Functions.SendMessage(bot, message, param[rand.Next(0, param.Length)], message.ChatId != null);
        }

        void Support(Message message, string args, Bot bot)
        {
            Functions.SendMessage(bot, message, "https://vk.com/dedsec_alexberezhnyh", message.ChatId != null);
        }

        void BRandom(Message message, string args, Bot bot)
        {
            if (NeedCommandInfo(message, args, bot)) return;
            Random rand = new Random();
            if(args == null) Functions.SendMessage(bot, message, "🎲 " + rand.Next().ToString(), message.ChatId != null);
            else
            {
                string[] param = args.Split(new char[1]{ ',' }, 2, StringSplitOptions.RemoveEmptyEntries); 
                if(param.Length == 1)
                    Functions.SendMessage(bot, message, "🎲 " + rand.Next(Convert.ToInt32(param[0])+1).ToString(), message.ChatId != null);
                else
                    Functions.SendMessage(bot, message, "🎲 " + rand.Next(Convert.ToInt32(param[0]), Convert.ToInt32(param[1])+1).ToString(), message.ChatId != null);
            }
        }

        void Say(Message message, string args, Bot bot)
        {
            if (NeedCommandInfo(message, args, bot)) return;
            string text = Functions.RemoveSpaces(args);
            if (text[0] == '!' && text.Length >= 2)
            {
                string textToSpeech = text.Substring(1);
                if(!String.IsNullOrWhiteSpace(textToSpeech))
                    SpeechText.Speech(message, textToSpeech, bot);

                else Functions.SendMessage(bot, message, text, message.ChatId != null);
            }
            else Functions.SendMessage(bot, message, text, message.ChatId != null);
        }

        void AnonimSend(Message message, string args, Bot bot)
        {
            if (NeedCommandInfo(message, args, bot)) return;
            string[] param = args.Split(new char[1] { ',' }, 2, StringSplitOptions.None);

            long? answerId = message.UserId;

            if (message.Attachments.Count != 0 && param.Length == 1)
            {
                param = new string[] {param[0], "[тут должно быть вложение, если его не оказалось - произошла непредвиденная ошибка]" };
            }
            Functions.RemoveSpaces(ref param[1]);

            if (param[0].Contains("\""))
            {
                AnonChat.ChatSend(message, param[0], param[1], bot);
                return;
            }

            param[0] = Functions.GetUserId(param[0], bot);
            message.UserId = Convert.ToInt32(param[0]); ;

            try
            {
                if (param[1][0] != '!')
                    Functions.SendMessage(bot, message, "Служба анонимной почты, вам письмо:\n" + param[1], false, true);
                
                else
                {
                    Functions.SendMessage(bot, message, SpeechText.MakeSpeechAttachment(param[1].Substring(1), message, bot),
                        "Служба анонимной почты, вам аудиосообщение:\n");
                }   
            }
            catch (VkNet.Exception.VkApiException ex)
            {
                message.UserId = answerId;
                Functions.SendMessage(bot, message, "К сожалению, я не могу отправить сообщение этому человеку.\n" +
                                     "Такие дела ¯\\_(ツ)_/¯.", message.ChatId != null);
                return;
            }
            message.UserId = answerId;
            Functions.SendMessage(bot, message, "Доставлено!", message.ChatId!=null);
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

        public bool NeedCommandInfo(Message message, string args, Bot bot)
        {
            string info = "";
            switch (message.Body)
            {
                case "анонимно":
                    info =
                    $"Справка по команде \"{message.Body}\":\n\n" +
                    "Команда отправляет пользователю анонимное сообщение, где не будет указано, от кого оно.\n\n" +
                    $"Для того, чтобы отправить сообщение пользователю, напишите {bot.GetSettings().BotName[0]}, {message.Body}(ссылка или id пользователя, текст сообщения)\n" +
                    $"Пример: {bot.GetSettings().BotName[0]}, {message.Body}(vk.com/hello_bot, привет)\n" +
                    $"Пользователь получит сообщение: \n" +
                    $"Служба анонимной почты, вам письмо: привет\n\n" +
                    $"Для того, чтобы отправить аудиосообщение пользователю, напишите {bot.GetSettings().BotName[0]}, {message.Body}(ссылка или id пользователя, !текст сообщения) - поставьте ! перед текстом сообщения\n" +
                    $"Пример: {bot.GetSettings().BotName[0]}, {message.Body}(vk.com/hello_bot, !привет)\n" +
                    $"Пользователь получит сообщение: \n" +
                    $"Служба анонимной почты, вам аудиосообщение: *{SpeechText.NAME} - привет* <- аудиозапись.\n\n" +
                    $"ВНИМАНИЕ! Если сообщение было успешно доставлено, бот ответит вам \"Доставлено!\"";
                    break;

                case "вычисли":
                    info =
                    $"Справка по команде \"{message.Body}\":\n\n" +
                    "Бот вычисляет математическое выражение (в том числе длинную дробную арифметику) со стандартными операциями (+-*/), указанное в скобках.\n\n" +
                    $"Пример: {bot.GetSettings().BotName[0]}, {message.Body}( ((25/5) - 3) *2) )\n" +
                    $"Пользователь получит ответ: 4\n\n" +
                    "Обратите внимание, вне зависимости от того, есть ли в выражении свои скобки или нет  (25/5) - 3) * 2, всё выражение должно быть указано в главных скобках:  ((25/5) - 3) * 2)";
                    break;
                default:
                    info = $"Справка для команды \"{message.Body}\" отсутствует. Обратитесь к разработчику: https://vk.com/dedsec_alexberezhnyh";
                    break;
            }
            if (args == null || String.IsNullOrEmpty(args) || String.IsNullOrWhiteSpace(args))
            {
                Functions.SendMessage(bot, message, info, message.ChatId != null);
                return true;
            }
            return false;
        }
    }
}