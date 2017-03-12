using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using MySql.Data;
using System.Data;
using System.Windows.Forms;

namespace BOT_Platform
{
    class MysqlData
    {
       internal static MySqlConnectionStringBuilder mysqlCSB;
        // Тут будут храниться данные авторизации

       internal static void InitMysqlData()
       {
           mysqlCSB = new MySqlConnectionStringBuilder();
           mysqlCSB.Server = "db.radiushost.net";           // IP сервера
           mysqlCSB.Database = "yan1506_789";               // название базы данных
           mysqlCSB.UserID = "yan1506_123";                 // логин
           mysqlCSB.Password = "Qwertyuiop[]123";           // пароль
       }

        /// <summary>
        ///     Функция, выполняющая MySql-запросы
        /// </summary>
        /// <param name="request">
        ///     MySql-запрос в виде строки
        /// </param>
        /// <returns>
        ///     Возвращает таблицу с результатом запроса.
        /// </returns>
        internal static DataTable MysqlCommand(string request)
        {
            using (MySqlConnection connection = new MySqlConnection())
            {
                connection.ConnectionString = MysqlData.mysqlCSB.ConnectionString;

                MySqlCommand command = new MySqlCommand(request, connection);

                try
                {
                    connection.Open();
                    using (MySqlDataReader dataReader = command.ExecuteReader())
                    {
                        if (dataReader.HasRows == true)
                        {
                            DataTable table = new DataTable();
                            table.Load(dataReader);
                            return table;
                        }
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return null;
                }
            }
            return null;
        }
    }
}
