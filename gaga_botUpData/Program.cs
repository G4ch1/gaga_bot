using System;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using Discord.Interactions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Discord.Rest;

using System.Linq;
using gaga_bot.Functions;
using gaga_bot.Attributes;
using System.Collections.Generic;
using System.Reflection;
using Discord.Net;

namespace gaga_bot
{
    class Program
    {
        // setup our fields we assign later
        private readonly IConfiguration _config;
        private DiscordSocketClient _client;
        private InteractionService _handler;
        private IServiceProvider _services;
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
                var config = new DiscordSocketConfig()
                {
                    GatewayIntents = GatewayIntents.Guilds |
                                 GatewayIntents.GuildMembers |
                                 GatewayIntents.GuildBans |
                                 GatewayIntents.GuildIntegrations |
                                 GatewayIntents.GuildVoiceStates |
                                 GatewayIntents.GuildMessages |
                                 GatewayIntents.GuildMessageTyping |
                                 GatewayIntents.MessageContent |
                                 GatewayIntents.DirectMessages
                };
                _client = new DiscordSocketClient(config);
                _handler = new InteractionService(_client.Rest);

                // setup logging and the ready event
                _client.Log += LogAsync;
                _handler.Log += LogAsync;
                _client.Ready += ReadyAsync;
                _client.UserVoiceStateUpdated += UserVoiceStateUpdated;
                _client.UserJoined += ClientUserJoined;
                _client.UserLeft += ClientUserLeft;
                _client.MessageReceived += ClientMessageReceived;

                // this is where we get the Token value from the configuration file, and start the bot
                await _client.LoginAsync(TokenType.Bot, _config["token"]);
                await _client.StartAsync();

                // we get the CommandHandler class here and call the InitializeAsync method to start things up for the CommandHandler service
                await services.GetRequiredService<CommandHandler>().InitializeAsync();
                _services = services;
                await Task.Delay(Timeout.Infinite);
            }
        }

        private Dictionary<ulong, DateTime> _userLastMessageTimes = new Dictionary<ulong, DateTime>();
        private Dictionary<ulong, int> _userMessageCount = new Dictionary<ulong, int>();
        private Dictionary<ulong, int> _userActivityPoints = new Dictionary<ulong, int>();

        private async Task ClientMessageReceived(SocketMessage message)
        {
            try
            {
                // Проверяем, является ли пользователь ботом
                if (message.Author.IsBot)
                    return;

                if (!(message.Author is SocketGuildUser user))
                    return;

                // Проверяем, если пользователь уже в муте, не начисляем баллы
                if (user.Roles.Any(r => r.Id == ulong.Parse(_config["muteRoles"])))
                    return;

                // Проверяем, есть ли пользователь в словаре
                if (!_userLastMessageTimes.ContainsKey(user.Id))
                {
                    _userLastMessageTimes.Add(user.Id, DateTime.UtcNow);
                    _userMessageCount.Add(user.Id, 1);
                    return;
                }


                var currentTime = DateTime.UtcNow;
                var lastMessageTime = _userLastMessageTimes[user.Id];
                var messageCount = _userMessageCount[user.Id];

                // Проверяем, если пользователь пишет в канале для флуда, не начисляем баллы
                if (message.Channel.Id != ulong.Parse(_config["fludChanels"]))
                {
                    // Проверяем, если пользователь отправляет больше 3 сообщений подряд, не начисляем баллы
                    var userActivityPoints = _userActivityPoints.ContainsKey(user.Id) ? _userActivityPoints[user.Id] : 0;
                    if (userActivityPoints >= 3)
                        return;
                    else
                    {
                        // Начисляем баллы активности пользователю
                        var pointsToAdd = 1; // Количество баллов, которое будет начислено за каждое сообщение
                        _userActivityPoints[user.Id] = userActivityPoints + pointsToAdd;
                        RequestHandlers.ExecuteWrite($"UPDATE Users SET  Currency = Currency + 1 WHERE DiscordId = '{user.Id}'");
                    }
                }


                // Проверяем, если прошло более 30 секунд, сбрасываем счетчик
                if ((currentTime - lastMessageTime).TotalSeconds > 30)
                {
                    _userLastMessageTimes[user.Id] = currentTime;
                    _userMessageCount[user.Id] = 1;
                    return;
                }

                // Увеличиваем счетчик сообщений пользователя
                _userMessageCount[user.Id]++;

                // Если пользователь превысил лимит сообщений, выдаем роль "мут" на 10 минут
                if (_userMessageCount[user.Id] > 10)
                {
                    var muteRole = user.Guild.Roles.FirstOrDefault(r => r.Id == ulong.Parse(_config["muteRoles"]));
                    if (muteRole != null)
                    {
                        await user.AddRoleAsync(muteRole);
                        await Task.Delay(TimeSpan.FromSeconds(30));
                        await user.RemoveRoleAsync(muteRole);
                    }

                    // Сбрасываем счетчик и время последнего сообщения пользователя
                    _userLastMessageTimes[user.Id] = currentTime;
                    _userMessageCount[user.Id] = 1;
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        private async Task ClientUserLeft(SocketGuild guild, SocketUser user)
        {
            try
            {
                //await new RequestHandler().ExecuteWriteAsync($"DELETE FROM Users WHERE DiscordId = '{user.Id}'");
                ITextChannel channel = _client.GetChannel(ulong.Parse(_config["inviteLogChanel"])) as ITextChannel;
                var EmbedBuilderLog = new EmbedBuilder()
                    .WithColor(Color.Red)
                    .WithThumbnailUrl(user.GetAvatarUrl())
                    .WithDescription($"{user.Mention} вышел с сервера.")
                    .WithFooter(footer =>
                    {
                        footer
                        .WithText("User left log")
                        .WithIconUrl(user.GetAvatarUrl());
                    });
                Embed embedLog = EmbedBuilderLog.Build();
                await channel.SendMessageAsync(embed: embedLog);

                Console.WriteLine("Log | ClientUserLeft | User left");
            }
            catch (Exception ex) { Console.WriteLine($"Exception | ClientUserLeft | {ex.Message}"); }
        }

        private async Task ClientUserJoined(SocketGuildUser user)
        {
            try
            {
                Console.WriteLine("Log | ClientUserJoined | User joined.");

                var query = RequestHandlers.ExecuteReader($"SELECT * FROM Users WHERE DiscordId = '{user.Id}'");

                if (!query.HasRows) // построчно считываем данные
                {
                    RequestHandlers.ExecuteWrite($"INSERT INTO Users (DiscordId, MessagesCount, Currency, LastActivity, VoiceTime)" +
                    $"\r\n VALUES ('{user.Id}', 0, 0, GETDATE(), '00:00:01')");

                    Console.WriteLine("Log | ClientUserJoined | User added to database.");
                }

                // получаем роль, которую необходимо выдать
                var role = user.Guild.GetRole(ulong.Parse(_config["newUserRole"]));

                // выдаем роль пользователю
                await user.AddRoleAsync(role);

                ITextChannel channel = _client.GetChannel(ulong.Parse(_config["inviteLogChanel"])) as ITextChannel;
                var EmbedBuilderLog = new EmbedBuilder()
                    .WithColor(Color.Red)
                    .WithDescription($"{user.Mention} присоеденился к серверу.")
                    .WithThumbnailUrl(user.GetAvatarUrl())
                    .WithFooter(footer =>
                    {
                        footer
                        .WithText("User left log")
                        .WithIconUrl(user.GetAvatarUrl());
                    });
                Embed embedLog = EmbedBuilderLog.Build();
                await channel.SendMessageAsync(embed: embedLog);
            }
            catch (Exception ex) { Console.WriteLine($"Exception | ClientUserJoined | {ex.Message}"); }
        }

        private async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            try
            {
                if (after.VoiceChannel != null && before.VoiceChannel is null)
                {
                    await TimeVoice.OnVoiceStateUpdatedAsync(user, before, after);
                    Console.WriteLine($"Log | UserVoiceStateUpdated | User connected voice chanel {after.VoiceChannel.Name}");

                    ITextChannel channel = _client.GetChannel(ulong.Parse(_config["voiceLogChanels"])) as ITextChannel;
                    var EmbedBuilderLog = new EmbedBuilder()
                        .WithAuthor(user.Username, user.GetAvatarUrl())
                        .WithThumbnailUrl(user.GetAvatarUrl())
                        .WithDescription($"{user.Mention} присоединился к голосовому чату `{after.VoiceChannel.Name}`.")
                        .WithFooter(footer =>
                        {
                            footer
                            .WithIconUrl(channel.Guild.BannerUrl)
                            .WithText(channel.Guild.Name);
                        }).WithCurrentTimestamp();
                    Embed embedLog = EmbedBuilderLog.Build();
                    await channel.SendMessageAsync(embed: embedLog);


                    //создам приватную комнату
                    if (after.VoiceChannel.Id == ulong.Parse(_config["privateVoiceChanels"]))
                    {
                        Console.WriteLine($"Log | UserVoiceStateUpdated | Create chanels");

                        // Создаем объект разрешений
                        OverwritePermissions permissions = new OverwritePermissions(
                            viewChannel: PermValue.Allow, // Разрешено просматривать канал
                            connect: PermValue.Allow, // Разрешено подключаться к голосовому каналу
                            speak: PermValue.Allow, // Разрешено говорить в голосовом канале
                            useVoiceActivation: PermValue.Allow
                        );

                        var category = _client.GetGuild(ulong.Parse(_config["serverId"])).CategoryChannels.FirstOrDefault(x => x.Name == "приват");
                        var voiceChannel = await _client.GetGuild(ulong.Parse(_config["serverId"])).CreateVoiceChannelAsync(user.Username, properties =>
                        {
                            properties.CategoryId = category.Id;
                        });// Создаем голосовой канал в указанной категории

                        var existingPermissions = voiceChannel.GetPermissionOverwrite(user) ?? new OverwritePermissions();// Получаем или создаем объект разрешений для пользователя               
                        await (voiceChannel as RestVoiceChannel).AddPermissionOverwriteAsync(user, permissions); // Применяем изменения к каналу                  
                        await (user as IGuildUser).ModifyAsync(properties => properties.Channel = voiceChannel); // Перемещаем пользователя в другой голосовой канал

                        OverwritePermissions permissionsTextChanels = new OverwritePermissions(
                            viewChannel: PermValue.Allow, // Разрешено просматривать канал
                            sendMessages: PermValue.Allow
                        );
                        await (_client.GetGuild(ulong.Parse(_config["serverId"])).GetChannel(ulong.Parse(_config["privateParamChanels"])) as ITextChannel).AddPermissionOverwriteAsync(user, permissionsTextChanels);
                    }
                }
                else if (after.VoiceChannel is null && before.VoiceChannel != null)
                {
                    await TimeVoice.OnVoiceStateUpdatedAsync(user, before, after);
                    Console.WriteLine($"Log | UserVoiceStateUpdated | User disconected voice chanel {before.VoiceChannel.Name}");

                    ITextChannel channel = _client.GetChannel(ulong.Parse(_config["voiceLogChanels"])) as ITextChannel;
                    var EmbedBuilderLog = new EmbedBuilder()
                        .WithAuthor(user.Username, user.GetAvatarUrl())
                        .WithThumbnailUrl(user.GetAvatarUrl())
                        .WithDescription($"{user.Mention} отключился от голосового канала `{before.VoiceChannel.Name}`.")
                        .WithFooter(footer =>
                        {
                            footer
                            .WithIconUrl(channel.Guild.BannerUrl)
                            .WithText(channel.Guild.Name);
                        }).WithCurrentTimestamp();
                    Embed embedLog = EmbedBuilderLog.Build();
                    await channel.SendMessageAsync(embed: embedLog);

                    if (before.VoiceChannel.Name.ToString() == user.Username)
                    {
                        OverwritePermissions permissionsTextChanels = new OverwritePermissions(
                            viewChannel: PermValue.Deny, // Разрешено просматривать канал
                            sendMessages: PermValue.Deny
                        );
                        await (_client.GetGuild(ulong.Parse(_config["serverId"])).GetChannel(ulong.Parse(_config["privateParamChanels"])) as ITextChannel).AddPermissionOverwriteAsync(user, permissionsTextChanels);

                        if (before.VoiceChannel.ConnectedUsers.Count() >= 1)
                        {
                            Console.WriteLine($"Log | UserVoiceStateUpdated | Remove permission chanels");
                            // Создаем объект разрешений
                            OverwritePermissions permissions = new OverwritePermissions(
                                viewChannel: PermValue.Allow, // Разрешено просматривать канал
                                connect: PermValue.Allow, // Разрешено подключаться к голосовому каналу
                                speak: PermValue.Allow // Разрешено говорить в голосовом канале
                            );
                            // Получаем или создаем объект разрешений для пользователя

                            var newOwner = before.VoiceChannel.ConnectedUsers.First() as IUser;

                            var existingPermissions = before.VoiceChannel.GetPermissionOverwrite(newOwner) ?? new OverwritePermissions();
                            // Применяем изменения к каналу
                            await before.VoiceChannel.AddPermissionOverwriteAsync(newOwner, permissions);
                            await before.VoiceChannel.ModifyAsync(properties => properties.Name = newOwner.ToString());

                            OverwritePermissions permissionsTextChanelsView = new OverwritePermissions(
                            viewChannel: PermValue.Allow, // Разрешено просматривать канал
                            sendMessages: PermValue.Allow
                            );

                            await (_client.GetGuild(ulong.Parse(_config["serverId"])).GetChannel(ulong.Parse(_config["privateParamChanels"])) as ITextChannel).AddPermissionOverwriteAsync(newOwner, permissionsTextChanelsView);

                            Console.WriteLine($"Log | UserVoiceStateUpdated | Deleted chanels {before.VoiceChannel.Name}");
                            channel = _client.GetChannel(ulong.Parse(_config["serverLogChanels"])) as ITextChannel;
                            EmbedBuilderLog = new EmbedBuilder()
                                .WithAuthor(user.Username, user.GetAvatarUrl())
                                .WithDescription($"Изменен владелец приватного канала {before.VoiceChannel.Name}.")
                                .WithFooter(footer =>
                                {
                                    footer
                                    .WithIconUrl(channel.Guild.BannerUrl)
                                    .WithText(channel.Guild.Name);
                                }).WithCurrentTimestamp();
                            embedLog = EmbedBuilderLog.Build();
                            await channel.SendMessageAsync(embed: embedLog);
                        }
                        else
                        {
                            Console.WriteLine($"Log | UserVoiceStateUpdated | Deleted chanels {before.VoiceChannel.Name}");
                            SocketGuildChannel socketGuildChannel = _client.GetGuild(ulong.Parse(_config["serverId"])).GetChannel(before.VoiceChannel.Id);
                            await socketGuildChannel.DeleteAsync();
                        }
                    }
                }
            }
            catch (RateLimitedException ex)
            {
                // Получаем время ожидания из исключения
                Console.WriteLine( ex.Message );

                // Ожидаем указанное время и повторяем запрос
                await Task.Delay(1800);
                // Повторяем запрос здесь

                await UserVoiceStateUpdated(user, before, after);
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.ToString()); 
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
            _handler = new InteractionService(_client);
            await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            await _handler.RegisterCommandsToGuildAsync(_testGuildId);

            _client.InteractionCreated += async interaction =>
            {
                var scope = _services.CreateScope();
                var ctx = new SocketInteractionContext(_client, interaction);
                await _handler.ExecuteCommandAsync(ctx, scope.ServiceProvider);
            };

            // Код для обработки события нажатия на кнопку
            // _client экземпляр DiscordSocketClient
            /*_client.InteractionCreated += async interaction =>
            {
                if (interaction is SocketMessageComponent messageComponent)
                {
                    switch (messageComponent.Data.CustomId)
                    {
                        case "button_click1":
                            Console.WriteLine("pidoras");
                            //await new RequestHandler().ExecuteWriteAsync($"DELETE FROM Users WHERE DiscordId = '{user.Id}'");
                            ITextChannel channel = _client.GetChannel(ulong.Parse(_config["inviteLogChanel"])) as ITextChannel;
                            var EmbedBuilderLog = new EmbedBuilder()
                                .WithColor(Color.Red)
                                .WithThumbnailUrl(user.GetAvatarUrl())
                                .WithDescription($"{user.Mention} вышел с сервера.")
                                .WithFooter(footer =>
                                {
                                    footer
                                    .WithText("User left log")
                                    .WithIconUrl(user.GetAvatarUrl());
                                });
                            Embed embedLog = EmbedBuilderLog.Build();
                            await channel.SendMessageAsync(embed: embedLog);
                            break;

                        case "button_click2":
                            Console.WriteLine("girls");
                            break;

                        default:

                            break;
                    }
                }
            };*/

            /*if (IsDebug())
            {
                // this is where you put the id of the test discord guild
                Console.WriteLine($"In debug mode, adding commands to {_testGuildId}...");
                await _handler.RegisterCommandsToGuildAsync(_testGuildId);
            }
            else
            {
                // this method will add commands globally, but can take around an hour
                await _handler.RegisterCommandsGloballyAsync(true);
            }
            Console.WriteLine($"Connected as -> [{_client.CurrentUser}] :)");*/
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