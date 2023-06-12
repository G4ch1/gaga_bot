using System;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using Discord.Interactions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace gaga_bot
{
    class Program
    {
        // setup our fields we assign later
        private readonly IConfiguration _config;
        private DiscordSocketClient _client;
        private InteractionService _handler;
        private ulong _testGuildId;

        public Program()
        {
            // create the configuration
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json");

            // build the configuration and assign to _config          
            _config = _builder.Build();
            _testGuildId = ulong.Parse(_config["serverID"]);
        }

        static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            // call ConfigureServices to create the ServiceCollection/Provider for passing around the services
            using (var services = ConfigureServices()) 
            {             
                // get the client and assign to client 
                // you get the services via GetRequiredService<T>
                var client = services.GetRequiredService<DiscordSocketClient>();
                var commands = services.GetRequiredService<InteractionService>();

                _client = client;
                _handler = commands;

                // setup logging and the ready event
                _client.Log += LogAsync;
                _handler.Log += LogAsync;
                _client.Ready += ReadyAsync;
                /*_client.GuildScheduledEventCreated += OnGuildScheduledEventCreated;
                _client.InviteCreated += OnInviteCreated;*/

                // this is where we get the Token value from the configuration file, and start the bot
                await _client.LoginAsync(TokenType.Bot, _config["token"]);
                await _client.StartAsync();

                // we get the CommandHandler class here and call the InitializeAsync method to start things up for the CommandHandler service
                await services.GetRequiredService<CommandHandler>().InitializeAsync();

                await Task.Delay(Timeout.Infinite);
            }
        }

        /*private async Task OnInviteCreated(SocketInvite invite)
        {
            // Получаем канал для логов (замените CHANNEL_ID на ID вашего канала)
            var logChannel = _client.GetChannel(ulong.Parse(_config["logChanel"])) as SocketTextChannel;

            // Проверяем, что канал для логов найден
            if (logChannel == null)
            {
                Console.WriteLine("Канал для логов не найден.");
                return;
            }

            // Создаем и настраиваем эмбед
            var embed = new EmbedBuilder()
                .WithTitle("Приглашение создано")
                .WithDescription($"Код приглашения: {invite.Code}\n" +
                                 $"Создатель: {invite.Inviter.Username}#{invite.Inviter.Discriminator}\n" +
                                 $"Количество использований: {invite.Uses}")
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .Build();

            // Отправляем эмбед в канал для логов
            await logChannel.SendMessageAsync(embed: embed);
        }

        private async Task OnGuildScheduledEventCreated(SocketGuildEvent guildEvent)
        {
            // Получение канала для логов по его ID или имени
            var logChannel = guildEvent.Guild.GetTextChannel(ulong.Parse(_config["logChanel"]));

            // Проверка, что канал для логов существует
            if (logChannel == null)
            {
                // Если канал для логов не найден, можно выполнить соответствующие действия или вывести сообщение об ошибке
                Console.WriteLine("Канал для логов не найден!");
                return;
            }

            // Создание эмбед-сообщения
            var embed = new EmbedBuilder()
                .WithTitle("Создан новый эвент")
                .WithDescription($"Название эвента: {guildEvent.Name}\nВремя начала: {guildEvent.StartTime}")
                .WithColor(Color.Green)
                .Build();

            // Отправка эмбед-сообщения в канал для логов
            await logChannel.SendMessageAsync(embed: embed);
        }
*/
        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private async Task ReadyAsync()
        {
            if (IsDebug())
            {
                // this is where you put the id of the test discord guild
                System.Console.WriteLine($"In debug mode, adding commands to {_testGuildId}...");
                await _handler.RegisterCommandsToGuildAsync(_testGuildId);
            }
            else
            {
                // this method will add commands globally, but can take around an hour
                await _handler.RegisterCommandsGloballyAsync(true);
            }
            Console.WriteLine($"Connected as -> [{_client.CurrentUser}] :)");
        }

        // this method handles the ServiceCollection creation/configuration, and builds out the service provider we can call on later
        private ServiceProvider ConfigureServices()
        {
            // this returns a ServiceProvider that is used later to call for those services
            // we can add types we have access to here, hence adding the new using statement:
            // using csharpi.Services;
            return new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<CommandHandler>()
                .BuildServiceProvider();
        }


        static bool IsDebug()
        {
            #if DEBUG
                return true;
            #else
                return false;
            #endif
        }
    }
}