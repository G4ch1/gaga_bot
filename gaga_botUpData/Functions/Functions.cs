using Discord;
using Discord.WebSocket;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace gaga_bot.Functions
{
    public static class Functions
    {
        private static IConfiguration _config;

        /*public static async Task UpdateServerBaner(DiscordSocketClient client)
        {
            // create the configuration
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json");

            // build the configuration and assign to _config          
            _config = _builder.Build();

            int voiceUsersCount = 0;
            // Получаем количество пользователей в голосовом канале
            var guild = client.GetGuild(ulong.Parse(_config["serverId"]));
            if (guild != null)
            {
                // Получаем список голосовых каналов на сервере
                var voiceChannels = guild.VoiceChannels;

                // Вычисляем общее количество пользователей в голосовых каналах
                voiceUsersCount = voiceChannels.Sum(channel => channel.Users.Count);// Возвращаем количество пользователей в голосовых чатах
            }

            // Загружаем картинку баннера
            using (var imageStream = new FileStream("baner.png", FileMode.Open))
            {
                // Обновляем баннер сервера
                var bannerBytes = new byte[imageStream.Length];
                await imageStream.ReadAsync(bannerBytes, 0, bannerBytes.Length);

                // Преобразуем картинку в формат Base64
                string bannerBase64 = Convert.ToBase64String(bannerBytes);

                // Добавляем текст с количеством пользователей на картинку
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync($"https://some-image-api.com?text={voiceUsersCount}&image={bannerBase64}");
                    if (response.IsSuccessStatusCode)
                    {
                        var modifiedBannerBytes = await response.Content.ReadAsByteArrayAsync();
                        var modifiedBannerStream = new MemoryStream(modifiedBannerBytes);

                        // Обновляем баннер сервера
                        await guild.ModifyAsync(x => x.Banner = new Discord.Image(modifiedBannerStream));
                    }
                }
            }
        }*/

        public static async Task SetBotStatusAsync(DiscordSocketClient client)
        {
            JObject config = GetConfig();

            string currently = config["currently"]?.Value<string>().ToLower();
            string statusText = config["playing_status"]?.Value<string>();
            string onlineStatus = config["status"]?.Value<string>().ToLower();
            string streamurl = config["streamurl"]?.Value<string>();

            // Set the online status
            if (!string.IsNullOrEmpty(onlineStatus))
            {
                UserStatus userStatus = onlineStatus switch
                {
                    "dnd" => UserStatus.DoNotDisturb,
                    "idle" => UserStatus.Idle,
                    "offline" => UserStatus.Invisible,
                    _ => UserStatus.Online
                };

                await client.SetStatusAsync(userStatus);
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} | Online status set | {userStatus}");
            }

            // Set the playing status
            if (!string.IsNullOrEmpty(currently) && !string.IsNullOrEmpty(statusText))
            {
                ActivityType activity = currently switch
                {
                    "listening" => ActivityType.Listening,
                    "watching" => ActivityType.Watching,
                    "streaming" => ActivityType.Streaming,
                    _ => ActivityType.Playing
                };
                
                await client.SetGameAsync(statusText, streamurl, type: activity);
                //await client.SetGameAsync(statusText, streamurl, type: activity);
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} | Playing status set | {activity}: {statusText}");
            }       
        }



        public static JObject GetConfig()
        {
            // Get the config file.
            using StreamReader configJson = new StreamReader(Directory.GetCurrentDirectory() + @"/Config.json");
                return (JObject)JsonConvert.DeserializeObject(configJson.ReadToEnd());
        }

        public static string GetAvatarUrl(SocketUser user, ushort size = 1024)
        {
            // Get user avatar and resize it. If the user has no avatar, get the default Discord avatar.
            return user.GetAvatarUrl(size: size) ?? user.GetDefaultAvatarUrl(); 
        }
    }
}
