using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFunctions
{
    class LongAr
    {
        public static string Plus(string a, string b)
        {
            ////////////////////////////
            //Обработать нули на входе!
            bool minus = false;
            string first = "none", second = String.Empty;
            string result = String.Empty;
            ////////////////////////////
            #region
            try
            {
                int tempDouble;

                if (a.Contains(',') == true && b.Contains(',') == false)
                {
                    b += ',';
                }

                else if (a.Contains(',') == false && b.Contains(',') == true)
                {
                    a += ',';
                }

                if (a.Contains(',') == true && b.Contains(',') == true)
                {
                    tempDouble = a.Substring(a.LastIndexOf(',') + 1).Length -
                                      b.Substring(b.LastIndexOf(',') + 1).Length;
                    if (tempDouble > 0)
                    {
                        while (a.Substring(a.LastIndexOf(',') + 1).Length -
                                     b.Substring(b.LastIndexOf(',') + 1).Length > 0) b += "0";
                    }
                    else if (tempDouble < 0)
                    {
                        while (a.Substring(a.LastIndexOf(',') + 1).Length -
                                    b.Substring(b.LastIndexOf(',') + 1).Length < 0) a += "0";
                    }
                }

            #endregion

                if (a[0] == '-' && b[0] == '-')
                {
                    a = a.Substring(1);
                    b = b.Substring(1);
                    minus = true;
                }

                #region
                if (a[0] != '-' && b[0] != '-')
                {

                    if (a.Length <= b.Length)
                    {
                        first = a;
                        second = b;
                    }
                    else
                    {
                        first = b;
                        second = a;
                    }

                    int next = 0;
                    for (int i = 0; i < first.Length; i++)
                    {
                        if (first[first.Length - 1 - i] == ',')
                        {
                            result = ',' + result;
                            continue;
                        }
                        int sum = Convert.ToInt16(first[first.Length - 1 - i].ToString())
                                + Convert.ToInt16(second[second.Length - 1 - i].ToString());
                        result = ((sum + next) % 10).ToString() + result;

                        next = (sum + next) / 10;

                        if (first.Length - 1 - i == 0 && second.Length > first.Length)
                        {
                            first = "0" + first;
                            continue;
                        }
                        if (first.Length - 1 - i == 0 && second.Length == first.Length && next != 0) result = next.ToString() + result;
                    }


                }
                #endregion


                if (b[0] == '-' && minus == false)
                {
                    string temp;
                    temp = a;
                    a = b;
                    b = temp;
                }

                if (a[0] == '-' && minus == false)
                {
                    a = a.Substring(1);
                    if (a.Length > b.Length)
                    {
                        while (a.Length -
                                         b.Length > 0) b = "0" + b;
                    }
                    else
                    {
                        while (a.Length -
                                         b.Length < 0) a = "0" + a;
                    }

                    for (int i = 0; i < a.Length; i++)
                    {
                        if (a[i] > b[i])
                        {
                            first = a;
                            second = b;
                            minus = true;
                            break;
                        }
                        if (a[i] < b[i])
                        {
                            first = b;
                            second = a;
                            break;
                        }
                    }
                    if (first == "none") return "0";

                    int next = 0;
                    for (int i = 0; i < first.Length; i++)
                    {
                        int razn;

                        if (first[first.Length - 1 - i] == ',')
                        {
                            result = ',' + result;
                            continue;
                        }

                        if (Convert.ToInt16(first[first.Length - 1 - i].ToString()) + next < 0)
                        {
                            razn = Convert.ToInt16(first[first.Length - 1 - i].ToString()) + next + 10
                                - Convert.ToInt16(second[second.Length - 1 - i].ToString());
                        }
                        else
                        {
                            razn = Convert.ToInt16(first[first.Length - 1 - i].ToString()) + next
                               - Convert.ToInt16(second[second.Length - 1 - i].ToString());

                            next = 0;
                        }


                        if (razn < 0)
                        {
                            next = -1;

                            razn += 10;
                            // else
                            // {

                            //razn = Convert.ToInt16(second[second.Length - 1 - i].ToString())
                            //    - (Convert.ToInt16(first[first.Length - 1 - i].ToString()));

                            // }

                        }

                        result = razn.ToString() + result;


                    }
                }
            }
            catch (Exception ex)
            {
                return "WRITING ERROR";
            }

            if (minus == true) return "-" + result;
            return result;
        }


        public static string Multiply(string a, string b)
        {
            string result = "0";

            bool minus = false;

            try
            {
                #region
                int tempDouble;

                if (a.Contains(',') == true && b.Contains(',') == false)
                {
                    b += ',';
                }

                else if (a.Contains(',') == false && b.Contains(',') == true)
                {
                    a += ',';
                }

                if (a.Contains(',') == true && b.Contains(',') == true)
                {
                    tempDouble = a.Substring(a.LastIndexOf(',') + 1).Length -
                                      b.Substring(b.LastIndexOf(',') + 1).Length;
                    if (tempDouble > 0)
                    {
                        while (a.Substring(a.LastIndexOf(',') + 1).Length -
                                     b.Substring(b.LastIndexOf(',') + 1).Length > 0) b += "0";
                    }
                    else if (tempDouble < 0)
                    {
                        while (a.Substring(a.LastIndexOf(',') + 1).Length -
                                    b.Substring(b.LastIndexOf(',') + 1).Length < 0) a += "0";
                    }
                }

                if (a[0] == '-' && b[0] == '-')
                {
                    a = a.Substring(1);
                    b = b.Substring(1);
                }
                else if (a[0] == '-')
                {
                    a = a.Substring(1);
                    minus = true;
                }
                else if (b[0] == '-')
                {
                    b = b.Substring(1);
                    minus = true;
                }
                #endregion

                string nulls = "";
                int next = 0;

                for (int i = 0; i < b.Length; i++)
                {
                    next = 0;
                    string number = string.Empty;

                    for (int y = 0; y < a.Length; y++)
                    {
                        if (b[b.Length - 1 - i] == ',') break;
                        if (a[a.Length - 1 - y] == ',')
                        {
                            continue;
                        }

                        if (b[b.Length - 1 - i] == '0') break;
                        int mult = Convert.ToInt16(a[a.Length - 1 - y].ToString())
                          * Convert.ToInt16(b[b.Length - 1 - i].ToString());
                        number = ((mult + next) % 10).ToString() + number;

                        next = (mult + next) / 10;
                    }
                    if (next != 0) number = next + number;

                    if (b[b.Length - 1 - i] == ',') continue;
                    if (i > 0) nulls += "0";
                    if (b[b.Length - 1 - i] == '0')
                    {
                        result = "0" + result;
                        continue;
                    }

                    result = Plus(result, number + nulls);
                }

                if (a.Contains(',') == true)
                {
                    string after = result.Substring(result.Length - (a.Length - 1 - a.LastIndexOf(',')) * 2);
                    string before = result.Substring(0, result.Length - (a.Length - 1 - a.LastIndexOf(',')) * 2);

                    result = before + "," + after;
                }

            }
            catch (Exception ex)
            {
                return "Ошибка в записи примера! :|";
            }

            if (minus == true) return "-" + result;
            return result;
        }
    }
}
