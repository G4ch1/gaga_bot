using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gaga_bot.Attributes
{
    public class Requests
    {
        public static string connectionString = "Server=(LocalDB)\\MSSQLLocalDB;Database=TestBD;Trusted_Connection=True;";
        public static string sqlExpression;

        public static async Task RequestsBan(UserBan userBan)
        {
            DateTime date = DateTime.Now;
            DateTime dateTime = DateTime.Now;
            userBan.date = date.ToShortDateString().Replace('.', '-');
            int value;

            switch (userBan.time.Substring(userBan.time.Length - 1))
            {
                case "m":
                    int.TryParse(string.Join("", userBan.time.Where(c => char.IsDigit(c))), out value);
                    date = date.AddMinutes(value);
                    break;
                case "h":
                    int.TryParse(string.Join("", userBan.time.Where(c => char.IsDigit(c))), out value);
                    date = date.AddHours(value);
                    break;
                case "d":
                    int.TryParse(string.Join("", userBan.time.Where(c => char.IsDigit(c))), out value);
                    date = date.AddDays(value);
                    break;
            }


            string sqlExpression = $"INSERT INTO UserBan (userID, reason, date, time) " +
                $"VALUES ({userBan.userID}, '{userBan.reason}', '{dateTime.ToString("yyyy-MM-dd").Replace('.', '-')}', '{date.ToString("yyyy-MM-dd H:mm:ss").Replace('.','-')}')";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                int number = await command.ExecuteNonQueryAsync();
                Console.WriteLine($"Добавлено объектов: {number}");
            }
        }

        public static async Task RequestsWarn(UserWarn userWarn)
        {
            DateTime date = DateTime.Now;
            DateTime dateTime = DateTime.Now;
            userWarn.date = date.ToShortDateString().Replace('.', '-');

            if (userWarn.valid == true)
                sqlExpression = $"INSERT INTO UserWarn (userID, reason, date, valid) " +
                    $"VALUES ({userWarn.userID}, '{userWarn.reason}', '{dateTime.ToString("yyyy-MM-dd").Replace('.', '-')}', '{userWarn.valid}')";
            if (userWarn.valid == false)
                sqlExpression = $"UPDATE UserWarn SET valid = '{userWarn.valid}' WHERE userID = {userWarn.userID} AND warnID = {userWarn.warnID}";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                int number = await command.ExecuteNonQueryAsync();
                Console.WriteLine($"Добавлено объектов: {number}");
            }
        }

        public static async Task RequestsMute(UserMute userMute)
        {
            DateTime date = DateTime.Now;
            DateTime dateTime = DateTime.Now;
            userMute.date = date.ToShortDateString().Replace('.', '-');

            int value;

            switch (userMute.time.Substring(userMute.time.Length - 1))
            {
                case "m":
                    int.TryParse(string.Join("", userMute.time.Where(c => char.IsDigit(c))), out value);
                    date = date.AddMinutes(value);
                    break;
                case "h":
                    int.TryParse(string.Join("", userMute.time.Where(c => char.IsDigit(c))), out value);
                    date = date.AddHours(value);
                    break;
                case "d":
                    int.TryParse(string.Join("", userMute.time.Where(c => char.IsDigit(c))), out value);
                    date = date.AddDays(value);
                    break;
            }

            string sqlExpression = $"INSERT INTO UserMute (userID, reason, date, time) " +
                $"VALUES ({userMute.userID}, '{userMute.reason}', '{date.ToString("yyyy-MM-dd H:mm:ss").Replace('.', '-')}', '{date.ToString("yyyy-MM-dd H:mm:ss").Replace('.', '-')}')";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                int number = await command.ExecuteNonQueryAsync();
                Console.WriteLine($"Добавлено объектов: {number}");
            }
        }

        public static async Task AllWarnUser(UserWarn userWarn)
        {
            string sqlExpression = $"SELECT [userID], [reason], [date], [issued], [warnID], [valid] " +
                $"FROM            UserWarn WHERE userID = {userWarn.userID}";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows) // если есть данные
                    {
                        // выводим названия столбцов
                        string columnName1 = reader.GetName(0);
                        string columnName2 = reader.GetName(1);
                        string columnName3 = reader.GetName(2);

                        Console.WriteLine($"{columnName1}\t{columnName3}\t{columnName2}");

                        while (await reader.ReadAsync()) // построчно считываем данные
                        {
                            object id = reader.GetValue(0);
                            object name = reader.GetValue(2);
                            object age = reader.GetValue(1);

                            Console.WriteLine($"{id} \t{name} \t{age}");
                        }
                    }
                }
            }

        }
    }
}
