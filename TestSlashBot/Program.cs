using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using gaga_bot.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace TestSlashBot
{
    public class Program
    {
        private static string connectionString = "Server=(LocalDB)\\MSSQLLocalDB;Database=TestBD;Trusted_Connection=True;";

        public static async Task Main()
        {
            UserWarn userWarn = new UserWarn();

            string sqlExpression = $"SELECT [userID], [reason], [date], [issued], [warnID], [valid] " +
                $"FROM            UserWarn WHERE userID = 334387634761760769";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows) // если есть данные
                    {
                        /*// выводим названия столбцов
                        string columnName1 = reader.GetName(0);
                        string columnName2 = reader.GetName(1);
                        string columnName3 = reader.GetName(2);*/

                        //Console.WriteLine($"{columnName1}\t{columnName3}\t{columnName2}");

                        while (await reader.ReadAsync()) // построчно считываем данные
                        {
                            object userID = reader.GetValue(0);
                            object reason = reader.GetValue(1);
                            object date = reader.GetValue(2);
                            object issued = reader.GetValue(3);
                            object warnID = reader.GetString(4);
                            object valid = reader.GetValue(5);

                            Console.WriteLine($"{userID} {reason} {date} {issued} {warnID} {valid}");
                        }
                    }
                }
            }

        }
    }
}