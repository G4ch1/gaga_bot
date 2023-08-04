using Discord.Interactions;
using Discord.WebSocket;
using Discord;

using System;
using System.Threading.Tasks;

using static LibraryAttributes.EnumValues;
using gaga_bot.Attributes;
using Microsoft.Extensions.Configuration;
using System.Drawing;
using System.Linq;

namespace gaga_bot.Modules.SlashCommands
{
    public class InteractionSleschCommands : InteractionModuleBase<SocketInteractionContext>
    {
        // create the configuration
        private static IConfigurationBuilder _builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(path: "config.json");

        public static readonly IConfiguration _config = _builder.Build();

        /// <summary>
        /// Группы модулей
        /// </summary>
        /// Группы модулей позволяют создавать подкоманды и группы подкоманд. 
        /// Вкладывая команды в модуль, помеченный атрибутом группы , вы можете создавать команды с префиксом.
        ///

        // Объединяет команды в группы
        [Group("interaction", "Взаимодействие")]
        public class CommandGroupModule : InteractionModuleBase<SocketInteractionContext>
        {
            // можно получить с помощью внедрения свойств, общедоступные свойства с общедоступными сеттерами будут установлены пакетами зависимостей
            public InteractionService _commands { get; set; }
            private readonly CommandHandler _handler;
            private DiscordSocketClient _client;
            // внедрение конструктора также является допустимым способом доступа к зависимостям
            public CommandGroupModule(DiscordSocketClient client, CommandHandler handler)
            {
                _handler = handler;
                _client = client;
                //_client.JoinedGuild += UserJoinAsync;
            }
            // Эта команда будет выглядеть следующим образом
            // group-name ping
            [SlashCommand("ping", "Получить Pong!")]
            public async Task PongSubcommand()
            {
                await RespondAsync("Pong!");
            }

            // И даже в подкомандных группах
            [Group("user", "пользователи")]
            public class UserGroupModule : InteractionModuleBase<SocketInteractionContext>
            {
                [EnabledInDm(true)]
                [RequireRole("Модератор")]
                [SlashCommand("role", "Выдать роль")]
                public async Task GiveRolesUser(RoleInteraction roleInteraction, SocketRole role, SocketGuildUser user)
                {
                    switch (roleInteraction)
                    {
                        case RoleInteraction.Give:
                            await user.AddRoleAsync(role);
                            await RespondAsync($"Пользователю {user.Mention} выдана роль: {role.Mention}.", ephemeral: true);
                            break;
                        case RoleInteraction.Remove:
                            await user.RemoveRoleAsync(role);
                            await RespondAsync($"У пользователя {user.Mention} удолена роль: {role.Mention}.", ephemeral: true);
                            break;
                    }
                }

                [EnabledInDm(true)]
                [RequireRole("Модератор")]
                [SlashCommand("sound", "заглушить")]
                public async Task SoundOffUser(SocketGuildUser user, OnOff onOff, TimeEnum time, int timeSpan)
                {
                    switch (onOff)
                    {
                        case OnOff.On:
                            if (user.IsMuted)
                                switch (time)
                                {
                                    case TimeEnum.Minutes:
                                        await user.SetTimeOutAsync(TimeSpan.FromMinutes(timeSpan));
                                        await RespondAsync($"Пользователь {user.Mention} заглушен на {timeSpan} минут.", ephemeral: true);
                                        break;
                                    case TimeEnum.Hours:
                                        await user.SetTimeOutAsync(TimeSpan.FromMinutes(timeSpan));
                                        await RespondAsync($"Пользователь {user.Mention} заглушен на {timeSpan} часов.", ephemeral: true);
                                        break;
                                    case TimeEnum.Days:
                                        await user.SetTimeOutAsync(TimeSpan.FromMinutes(timeSpan));
                                        await RespondAsync($"Пользователь  {user.Mention}  заглушен на  {timeSpan}  дней.", ephemeral: true);
                                        break;
                                }
                            break;
                        case OnOff.Off:
                            if (!user.IsMuted)
                            {
                                await user.RemoveTimeOutAsync();
                                await RespondAsync($"Пользователь {user.Mention} больше не заглушен.", ephemeral: true);
                            }
                            else
                                await RespondAsync($"Пользователь {user.Mention} не заглушен.", ephemeral: true);
                            break;
                    }
                }

                [SlashCommand("estimate", "оценить")]
                public async Task EstimateUser(SocketGuildUser user, Estimate estimate, string reason = "null")
                {
                    RequestHandlers.ExecuteWrite($"INSERT INTO Estimate(UserId, LikedUser, Sympathy, Reason, DateLiked) " +
                        $"VALUES ({user.Id}, {Context.User.Id}, {(int)estimate}, '{reason}', GETDATE())");
                    await RespondAsync($"Вы оценили пользователя {user.Username}. Спасибо что помогаете развивать сервер.", ephemeral: true);
                }
            }

            [Group("server", "сервер")]
            public class ServerGroupModules : InteractionModuleBase<SocketInteractionContext>
            {
                // можно получить с помощью внедрения свойств, общедоступные свойства с общедоступными сеттерами будут установлены пакетами зависимостей
                public InteractionService _commands { get; set; }
                private readonly CommandHandler _handler;
                private DiscordSocketClient _client;
                // внедрение конструктора также является допустимым способом доступа к зависимостям
                public ServerGroupModules(DiscordSocketClient client, CommandHandler handler)
                {
                    _handler = handler;
                    _client = client;
                    //_client.JoinedGuild += UserJoinAsync;
                }

                [SlashCommand("modify-private", "настройка приватного канала")]
                public async Task PrivateModifyVoiceChanel()
                {
                    var user = Context.User as SocketGuildUser;
                    var voiceChannel = user.VoiceChannel;

                    if (voiceChannel == null)
                    {
                        await ReplyAsync("Вы должны находиться в голосовом канале, чтобы изменить его параметры.");
                        return;
                    }

                    ITextChannel channel = Context.Client.GetChannel(ulong.Parse(_config["logChanel"])) as ITextChannel;
                    var EmbedBuilderLog = new EmbedBuilder()
                        .WithDescription($"⚙️ Управление приватными комнатами\n" +
                        $"Вы можете изменить конфигурацию своей комнаты с помощью взаимодействий.\n " +
                        $"🤴 — назначить нового создателя комнаты\n " +
                        $"👥 — ограничить/выдать доступ к комнате\n" +
                        $"📛 — задать новый лимит участников\n" +
                        $"🚫 — закрыть/открыть комнату\n" +
                        $"✏️ — изменить название комнаты\n" +
                        $"🚷 — скрыть/открыть комнату\n" +
                        $"♿ — выгнать участника из комнаты\n" +
                        $"🔇 — ограничить/выдать право говорить")
                        .WithFooter(footer =>
                        {
                            footer
                            .WithText("User ban log")
                            .WithIconUrl(Context.User.GetAvatarUrl());
                        });
                    Embed embedLog = EmbedBuilderLog.Build();

                    Emoji[] emoji = new Emoji[] { "\U0001f934", "👥", "📛", "🚫", "✏️", "🚷", "♿", "🔇"};

                    var button1 = new ButtonBuilder()
                    .WithStyle(ButtonStyle.Primary)
                    .WithEmote(emoji[0])
                    .WithCustomId("button_click1");

                    var button2 = new ButtonBuilder()
                    .WithStyle(ButtonStyle.Primary)
                    .WithEmote(emoji[1])
                    .WithCustomId("button_click2");

                    var button3 = new ButtonBuilder()
                    .WithStyle(ButtonStyle.Primary)
                    .WithEmote(emoji[2])
                    .WithCustomId("button_click3");

                    var button4 = new ButtonBuilder()
                    .WithStyle(ButtonStyle.Primary)
                    .WithEmote(emoji[3])
                    .WithCustomId("button_click4");

                    var button5 = new ButtonBuilder()
                    .WithStyle(ButtonStyle.Primary)
                    .WithEmote(emoji[4])
                    .WithCustomId("button_click5");

                    var button6 = new ButtonBuilder()
                    .WithStyle(ButtonStyle.Primary)
                    .WithEmote(emoji[5])
                    .WithCustomId("button_click6");

                    var button7 = new ButtonBuilder()
                    .WithStyle(ButtonStyle.Primary)
                    .WithEmote(emoji[6])
                    .WithCustomId("button_click7");

                    var button8 = new ButtonBuilder()
                    .WithStyle(ButtonStyle.Primary)
                    .WithEmote(emoji[7])
                    .WithCustomId("button_click8");

                    var builder = new ComponentBuilder()
                        .WithButton(button1)
                        .WithButton(button2)
                        .WithButton(button3)
                        .WithButton(button4)
                        .WithButton(button5)
                        .WithButton(button6)
                        .WithButton(button7)
                        .WithButton(button8);

                    // Код для обработки события нажатия на кнопку
                    // _client экземпляр DiscordSocketClient
                    _client.InteractionCreated += async interaction =>
                    {
                        if (interaction is SocketMessageComponent messageComponent && messageComponent.Data.CustomId == "button_click4")
                        {
                            // Получаем голосовой канал, в котором находится пользователь
                            var voiceChannel = (messageComponent.User as IGuildUser)?.VoiceChannel as IVoiceChannel;

                            if (voiceChannel != null)
                            {
                                // Создаем объект OverwritePermissions для установки прав доступа
                                var permissions = new OverwritePermissions(viewChannel: PermValue.Deny, connect: PermValue.Deny);

                                // Получаем текущие права доступа голосового канала
                                var channelPermissions = voiceChannel.GetPermissionOverwrite(messageComponent.User) ?? new OverwritePermissions();

                                // Обновляем права доступа
                                channelPermissions = channelPermissions.Modify(viewChannel: PermValue.Deny, connect: PermValue.Deny);

                                // Устанавливаем права доступа для конкретного пользователя
                                await voiceChannel.AddPermissionOverwriteAsync(messageComponent.User, channelPermissions);

                                // Устанавливаем права доступа для определенной роли
                                var roleId = 1234567890; // Замените на ID роли
                                var role = voiceChannel.Guild.GetRole((ulong)roleId);
                                await voiceChannel.AddPermissionOverwriteAsync(role, channelPermissions);
                            }                        
                        }
                    };

                    await RespondAsync(null, embed: EmbedBuilderLog.Build(), components: builder.Build());
                }
            }

            /*[Group("music", "музыка")]
            public class MusicGroupModule : InteractionModuleBase<SocketInteractionContext>
            {
                [SlashCommand("join", "Добавить бота в голосовой чат")]
                public async Task JoinVoiceChannel(IVoiceChannel voiceChannel)
                {                  
                    await RespondAsync($"Бот успешно присоединился к голосовому каналу: {user.VoiceChannel.Name}", ephemeral: true);
                }

                CommandContext ctx;

                [SlashCommand("play", "Воспроизводит музыку с YouTube")]
                public async Task PlayMusic(IVoiceChannel voiceChannel,string url)
                {
                    
                }
            }*/
        }
    }
}
