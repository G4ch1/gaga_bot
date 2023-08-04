using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using gaga_bot.Attributes;
using Microsoft.Extensions.Configuration;
using static LibraryAttributes.EnumValues;

namespace gaga_bot.Modules.SlashCommands
{
    public class ModSlashCommands : InteractionModuleBase<SocketInteractionContext>
    {
        // можно получить с помощью внедрения свойств, общедоступные свойства с общедоступными сеттерами будут установлены пакетами зависимостей
        public InteractionService _commands { get; set; }
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _config;

        // внедрение конструктора также является допустимым способом доступа к зависимостям
        public ModSlashCommands(DiscordSocketClient client, CommandHandler handler)
        {
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json");

            // build the configuration and assign to _config          
            _config = _builder.Build();

            _client = client;
            //_client.JoinedGuild += UserJoinAsync;
        }

        [EnabledInDm(true)]
        [RequireRole("Модератор")]
        [SlashCommand("ban", "Забанить")]
        public async Task BanUser(SocketGuildUser user, string reason, TimeEnum timeEnum, int time)
        {
            // Проверяем, является ли пользователь ботом
            if (user.IsBot)
            {
                await Task.CompletedTask;
            }
            try
            {
                if (user.Roles.Contains(user.Guild.Roles.FirstOrDefault(x => x.Id == ulong.Parse(_config["banRoles"]))))
                {
                    await RespondAsync($"Пользователь уже находиться в бане", ephemeral: true);
                    await Task.CompletedTask;
                }
                if (!user.Roles.Contains(user.Guild.Roles.FirstOrDefault(x => x.Id == ulong.Parse(_config["banRoles"]))))
                {
                    await user.AddRoleAsync(ulong.Parse(_config["banRoles"]));
                    DateTime timeSpent;
                    switch (timeEnum)
                    {
                        case TimeEnum.Minutes:
                            timeSpent = DateTime.UtcNow + TimeSpan.FromMinutes(time);
                            RequestHandlers.ExecuteWrite($"INSERT INTO Mutes(UserId, ModeratorId, Reason, StartTime, EndTime)" +
                        $"\r\nVALUES ({user.Id} ,{Context.User.Id}, '{reason}', GETDATE(), '{timeSpent.ToString(@"G")}')");
                            await RespondAsync($"В бан {user.Mention}, по причине {reason} на {time} минут.", ephemeral: true);
                            break;
                        case TimeEnum.Hours:
                            timeSpent = DateTime.UtcNow + TimeSpan.FromHours(time);
                            RequestHandlers.ExecuteWrite($"INSERT INTO Mutes(UserId, ModeratorId, Reason, StartTime, EndTime)" +
                        $"\r\nVALUES ({user.Id} ,{Context.User.Id}, '{reason}', GETDATE(), '{timeSpent.ToString(@"G")}')");
                            await RespondAsync($"В бан {user.Mention}, по причине {reason} на {time} часов.", ephemeral: true);
                            break;
                        case TimeEnum.Days:
                            timeSpent = DateTime.UtcNow + TimeSpan.FromDays(time);
                            RequestHandlers.ExecuteWrite($"INSERT INTO Mutes(UserId, ModeratorId, Reason, StartTime, EndTime)" +
                        $"\r\nVALUES ({user.Id} ,{Context.User.Id}, '{reason}', GETDATE(), '{timeSpent.ToString(@"G")}')");
                            await RespondAsync($"В бан {user.Mention}, по причине {reason} на {time} дней.", ephemeral: true);
                            break;
                    }

                    ITextChannel channel = Context.Client.GetChannel(ulong.Parse(_config["logChanel"])) as ITextChannel;
                    var EmbedBuilderLog = new EmbedBuilder()
                        .WithDescription($"{user.Mention} был забанен \n**Причина** {reason}\n**Модератором** {Context.User.Mention}")
                        .WithFooter(footer =>
                        {
                            footer
                            .WithText("User ban log")
                            .WithIconUrl(Context.User.GetAvatarUrl());
                        });
                    Embed embedLog = EmbedBuilderLog.Build();
                    await channel.SendMessageAsync(embed: embedLog);
                }
            }
            catch (Exception e)
            {
                await RespondAsync($"Не удалось забанить пользователя {user.Mention}: {e.Message}.", ephemeral: true);
                await Task.CompletedTask;
            }
            await Task.CompletedTask;
        }

        [EnabledInDm(true)]
        [RequireRole("Модератор")]
        [SlashCommand("unban", "Разбан")]
        public async Task UnBanUser(SocketGuildUser user)
        {
            // Проверяем, является ли пользователь ботом
            if (user.IsBot)
            {
                await Task.CompletedTask;
            }

            if (user.Roles.Contains(user.Guild.Roles.FirstOrDefault(x => x.Id == ulong.Parse(_config["banRoles"])))) 
            {
                await user.RemoveRoleAsync(ulong.Parse(_config["banRoles"]));
                ITextChannel channel = Context.Client.GetChannel(ulong.Parse(_config["logChanel"])) as ITextChannel;
                var EmbedBuilderLog = new EmbedBuilder()
                    .WithDescription($"{user.Mention} был разбанен \n**Модератором** {Context.User.Mention}")
                    .WithFooter(footer =>
                    {
                        footer
                        .WithText("User mut log")
                        .WithIconUrl(Context.User.GetAvatarUrl());
                    });
                Embed embedLog = EmbedBuilderLog.Build();
                await channel.SendMessageAsync(embed: embedLog);

                await RespondAsync($"Пользователь {user.Mention} был разбанен.", ephemeral: true);
            }
            else 
            {
                await RespondAsync($"Пользователь {user.Mention} не в бане.", ephemeral: true);
                await Task.CompletedTask;
            }
        }

        [SlashCommand("mut", "Мут")]
        [EnabledInDm(true)]
        [RequireRole("Модератор")]
        public async Task MutUser(SocketGuildUser user, string reason, TimeEnum timeEnum, int time)
        {
            // Проверяем, является ли пользователь ботом
            if (user.IsBot)
            {
                await Task.CompletedTask;
            }

            try
            {
                if (user.Roles.Contains(user.Guild.Roles.FirstOrDefault(x => x.Id == ulong.Parse(_config["muteRoles"]))))
                {
                    await RespondAsync($"Пользователь {user.Mention} уже находиться в муте", ephemeral: true);
                    await Task.CompletedTask;
                }
                if (!user.Roles.Contains(user.Guild.Roles.FirstOrDefault(x => x.Id == ulong.Parse(_config["muteRoles"]))))
                {
                    await user.AddRoleAsync(ulong.Parse(_config["muteRoles"]));
                    DateTime timeSpent;
                    switch (timeEnum)
                    {
                        case TimeEnum.Minutes:
                            timeSpent = DateTime.UtcNow + TimeSpan.FromMinutes(time);
                            RequestHandlers.ExecuteWrite($"INSERT INTO Mutes(UserId, ModeratorId, Reason, StartTime, EndTime)" +
                        $"\r\nVALUES ( {user.Id}  , {Context.User.Id} , '{reason}', GETDATE(), '{timeSpent.ToString(@"G")}')");
                            await RespondAsync($"В мут {user.Mention}, по причине {reason} на {time} минут.", ephemeral: true);
                            break;
                        case TimeEnum.Hours:
                            timeSpent = DateTime.UtcNow + TimeSpan.FromHours(time);
                            RequestHandlers.ExecuteWrite($"INSERT INTO Mutes(UserId, ModeratorId, Reason, StartTime, EndTime)" +
                        $"\r\nVALUES ( {user.Id}  , {Context.User.Id} , '{reason}', GETDATE(), '{timeSpent.ToString(@"G")}')");
                            await RespondAsync($"В мут {user.Mention}, по причине {reason} на {time} часов.", ephemeral: true);
                            break;
                        case TimeEnum.Days:
                            timeSpent = DateTime.UtcNow + TimeSpan.FromDays(time);
                            RequestHandlers.ExecuteWrite($"INSERT INTO Mutes(UserId, ModeratorId, Reason, StartTime, EndTime)" +
                        $"\r\nVALUES ( {user.Id}  , {Context.User.Id} , '{reason}', GETDATE(), '{timeSpent.ToString(@"G")}')");
                            await RespondAsync($"В мут {user.Mention}, по причине {reason} на {time} дней.", ephemeral: true);
                            break;
                    }

                    ITextChannel channel = Context.Client.GetChannel(ulong.Parse(_config["logChanel"])) as ITextChannel;
                    var EmbedBuilderLog = new EmbedBuilder()
                        .WithDescription($"{user.Mention} был замючен \n**Причина** {reason}\n**Модератором** {Context.User.Mention}")
                        .WithFooter(footer =>
                        {
                            footer
                            .WithText("User mut log")
                            .WithIconUrl(Context.User.GetAvatarUrl());
                        });
                    Embed embedLog = EmbedBuilderLog.Build();
                    await channel.SendMessageAsync(embed: embedLog);

                    await user.AddRoleAsync(ulong.Parse(_config["muteRoles"]));
                }
            }
            catch (Exception ex) 
            {
                await RespondAsync($"Не удалось замутить пользователя {user.Mention}: {ex.Message}.", ephemeral: true);
            }
        }

        [SlashCommand("unmut", "Размут")]
        [EnabledInDm(true)]
        [RequireRole("Модератор")]
        public async Task UnMutUser(SocketGuildUser user)
        {
            // Проверяем, является ли пользователь ботом
            if (user.IsBot)
            {
                await Task.CompletedTask;
            }

            if (user.Roles.Contains(user.Guild.Roles.FirstOrDefault(x => x.Id == ulong.Parse(_config["muteRoles"]))))
            {
                await user.RemoveRoleAsync(ulong.Parse(_config["muteRoles"]));
                ITextChannel channel = Context.Client.GetChannel(ulong.Parse(_config["logChanel"])) as ITextChannel;
                var EmbedBuilderLog = new EmbedBuilder()
                    .WithDescription($"{user.Mention} был размючен \n**Модератором** {Context.User.Mention}")
                    .WithFooter(footer =>
                    {
                        footer
                        .WithText("User unmut log")
                        .WithIconUrl(Context.User.GetAvatarUrl());
                    });
                Embed embedLog = EmbedBuilderLog.Build();
                await channel.SendMessageAsync(embed: embedLog);
                await RespondAsync($"С пользователя {user.Mention}, был снят мут.", ephemeral: true);
            }
            else
            {
                await RespondAsync($"Пользователь {user.Mention} не в муте.", ephemeral: true);
                await Task.CompletedTask;
            }
        }

        [SlashCommand("allmut", "Показать людей с мутами")]
        [EnabledInDm(true)]
        [RequireRole("Модератор")]
        public async Task AllmutMember()
        {
            var query = RequestHandlers.ExecuteReader($"SELECT TOP 10 UserId, StartTime, EndTime, ModeratorId FROM Mutes");

            var tableBuilder = new StringBuilder();

            while (query.Read())
            {
                var discordId = await _client.GetUserAsync((ulong)query.GetInt64(0));
                var moderatorId = await _client.GetUserAsync((ulong)query.GetInt64(3));

                tableBuilder.AppendLine($":person_in_manual_wheelchair: **Пользователь:** {discordId.Username}");
                tableBuilder.AppendLine($":no_entry: **Начало:** {query.GetDateTime(1)} | :end: **Конец:** {query.GetDateTime(2)}");
                tableBuilder.AppendLine($":beginner: **Модератор:** {moderatorId.Username}");
                tableBuilder.AppendLine();
            }

            var description = tableBuilder.ToString();

            ITextChannel channel = Context.Client.GetChannel(Context.Channel.Id) as ITextChannel;
            var EmbedBuilderLog = new EmbedBuilder()
                .WithDescription(description)
                .WithFooter(footer =>
                {
                    footer
                    .WithText("Список заглушенных пользователей")
                    .WithIconUrl(Context.User.GetAvatarUrl());
                });
            Embed embedLog = EmbedBuilderLog.Build();

            var emoji = new Emoji("\u2705");

            var button = new ButtonBuilder()
            .WithStyle(ButtonStyle.Primary)
            .WithEmote(emoji)
            .WithCustomId("button_click");

            var builder = new ComponentBuilder()
                .WithButton(button);

            // Код для обработки события нажатия на кнопку
            // _client экземпляр DiscordSocketClient
            _client.InteractionCreated += async interaction =>
            {
                if (interaction is SocketMessageComponent messageComponent && messageComponent.Data.CustomId == "button_click")
                {
                    var messages = await messageComponent.Channel.GetMessagesAsync(2).FlattenAsync(); // Получаем последние 2 сообщения
                    var lastMessage = messages.ElementAt(0); // Берем последнее сообщение
                    await lastMessage.DeleteAsync(); // Удаляем последнее сообщение
                }
            };

            await RespondAsync(null, embed: EmbedBuilderLog.Build(), components: builder.Build());
        }

        [SlashCommand("warn", "Предупреждение пользователя")]
        [EnabledInDm(true)]
        [RequireRole("Модератор")]
        public async Task WarnMember(SocketGuildUser user, string reason)
        {
            // Проверяем, является ли пользователь ботом
            if (user.IsBot)
            {
                await Task.CompletedTask;
            }

            DataTable query = new DataTable();

            query.Load(RequestHandlers.ExecuteReader($"SELECT id FROM Warnings WHERE UserId = {user.Id} and Valid = 1"));

            if(query.Rows.Count <= 2)
            {
                RequestHandlers.ExecuteWrite($"INSERT INTO Warnings(UserId, ModeratorId, Reason, Time, Valid) " +
                $"VALUES({user.Id}, {Context.User.Id}, '{reason}', GETDATE(), 1)");
                Console.WriteLine("Entry added warn.");

                await RespondAsync($"Пользователь {user.Username} был предупреждён." , ephemeral: true);
            }
            else if (query.Rows.Count == 3)
            {
                RequestHandlers.ExecuteWrite($"INSERT INTO Warnings(UserId, ModeratorId, Reason, Time, Valid) " +
                $"VALUES({user.Id}, {Context.User.Id}, '{reason}', GETDATE(), 1)");

                await user.AddRoleAsync(ulong.Parse(_config["muteRoles"]));

                await RespondAsync($"Пользователь {user.Username} получил 3 предупреждения.", ephemeral: true);
            }
            else if (query.Rows.Count >= 4)
            {
                await user.AddRoleAsync(ulong.Parse(_config["banRoles"]));

                await RespondAsync($"Пользователь {user.Username} получил 4 предупреждения.", ephemeral: true);
            }

            ITextChannel channel = Context.Client.GetChannel(ulong.Parse(_config["logChanel"])) as ITextChannel;
            var EmbedBuilderLog = new EmbedBuilder()
                .WithDescription($"{user.Mention} был предупреждён \n**Причина** {reason} \n**Модератором** {Context.User.Mention}")
                .WithFooter(footer =>
                {
                    footer
                    .WithText("User warn log")
                    .WithIconUrl(Context.User.GetAvatarUrl());
                });
            Embed embedLog = EmbedBuilderLog.Build();
            await channel.SendMessageAsync(embed: embedLog);
        }

        [SlashCommand("rewarn", "Убрать предупреждение")]
        [EnabledInDm(true)]
        [RequireRole("Модератор")]
        public async Task RewarnMember(SocketGuildUser user)
        {
            // Проверяем, является ли пользователь ботом
            if (user.IsBot)
            {
                await Task.CompletedTask;
            }

            if (RequestHandlers.ExecuteReader($"SELECT id FROM Warnings WHERE UserId = {user.Id} and Valid = 1").HasRows)
            {
                RequestHandlers.ExecuteWrite($"UPDATE Warnings SET Valid = 0 WHERE Id = (SELECT TOP 1 id FROM Warnings WHERE UserId = {user.Id} and Valid = 1)");
                ITextChannel channel = Context.Client.GetChannel(ulong.Parse(_config["logChanel"])) as ITextChannel;
                var EmbedBuilderLog = new EmbedBuilder()
                    .WithDescription($"{user.Mention} снято предупреждение \n**Модератором** {Context.User.Mention}")
                    .WithFooter(footer =>
                    {
                        footer
                        .WithText("User warn log")
                        .WithIconUrl(Context.User.GetAvatarUrl());
                    });
                Embed embedLog = EmbedBuilderLog.Build();
                await channel.SendMessageAsync(embed: embedLog);

                await RespondAsync($"Пользователь {user.Username} оправдан.", ephemeral: true);
            }
            else
            {
                await RespondAsync($"Пользователь {user.Mention} не имеет предупреждений.", ephemeral: true);
                await Task.CompletedTask;
            }
        }

        [SlashCommand("allwarn", "Показать все варны")]
        [EnabledInDm(true)]
        [RequireRole("Модератор")]
        public async Task AllwarnMember()
        {
            var query = RequestHandlers.ExecuteReader($"SELECT UserId, Reason, Time FROM Warnings WHERE Valid = 1 ");

            var tableBuilder = new StringBuilder();

            while (query.Read())
            {
                var discordId = await _client.GetUserAsync((ulong)query.GetInt64(0));

                tableBuilder.AppendLine($"**Пользователь:** {discordId.Username}");
                tableBuilder.AppendLine($"**Причина:** {query.GetString(1)} | **Дата:** {query.GetDateTime(2)}");
                tableBuilder.AppendLine();
            }

            var description = tableBuilder.ToString();

            ITextChannel channel = Context.Client.GetChannel(Context.Channel.Id) as ITextChannel;
            var EmbedBuilderLog = new EmbedBuilder()
                .WithDescription(description)
                .WithFooter(footer =>
                {
                    footer
                    .WithText("Список предупреждений")
                    .WithIconUrl(Context.User.GetAvatarUrl());
                });
            Embed embedLog = EmbedBuilderLog.Build();

            var emoji = new Emoji("\u2705");

            var button = new ButtonBuilder()
            .WithStyle(ButtonStyle.Primary)
            .WithEmote(emoji)
            .WithCustomId("button_click");

            var builder = new ComponentBuilder()
                .WithButton(button);

            // Код для обработки события нажатия на кнопку
            // _client экземпляр DiscordSocketClient
            _client.InteractionCreated += async interaction =>
            {
                if (interaction is SocketMessageComponent messageComponent && messageComponent.Data.CustomId == "button_click")
                {
                    var messages = await messageComponent.Channel.GetMessagesAsync(2).FlattenAsync(); // Получаем последние 2 сообщения
                    var lastMessage = messages.ElementAt(0); // Берем последнее сообщение
                    await lastMessage.DeleteAsync(); // Удаляем последнее сообщение
                }
            };

            await RespondAsync(null, embed: EmbedBuilderLog.Build(), components: builder.Build());
        }

        [SlashCommand("clear", "Удаление сообщений")]
        [EnabledInDm(true)]
        [RequireRole("Модератор")]
        public async Task ClearChat(int amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
            await RespondAsync("Сообщения удалены", ephemeral: true);
        }
    }
}
