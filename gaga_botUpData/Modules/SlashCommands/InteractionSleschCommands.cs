using Discord.Interactions;
using Discord.WebSocket;
using Discord;

using System;
using System.Threading.Tasks;

using gaga_bot.Attributes;
using Microsoft.Extensions.Configuration;
using System.Drawing;
using System.Linq;
using static gaga_bot.Modules.SlashCommands.TestSleshComand;
using Discord.Net;

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
        [Group("взаимодействие", "Взаимодействие")]
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

            /*[EnabledInDm(false)]
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
                [RequireRole("Старший")]
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

                [EnabledInDm(false)]
                [RequireRole("Старший")]
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

                [EnabledInDm(false)]
                [RequireRole("Старший")]
                [SlashCommand("estimate", "оценить")]
                public async Task EstimateUser(SocketGuildUser user, Estimate estimate, string reason = "null")
                {
                    RequestHandlers.ExecuteWrite($"INSERT INTO Estimate(UserId, LikedUser, Sympathy, Reason, DateLiked) " +
                        $"VALUES ({user.Id}, {Context.User.Id}, {(int)estimate}, '{reason}', GETDATE())");
                    await RespondAsync($"Вы оценили пользователя {user.Username}. Спасибо что помогаете развивать сервер.", ephemeral: true);
                }
            }*/

            [Group("серверные", "сервер")]
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

                [EnabledInDm(true)]
                //[RequireRole("Старший")]
                [SlashCommand("выбор-гендера", "можешь получить гендерную роль")]
                public async Task TakeGenderRoles()
                {
                    try
                    {
                        var user = Context.User as SocketGuildUser;
                        var voiceChannel = user.VoiceChannel;

                        /*if (user.VoiceChannel.Id != ulong.Parse(_config["privateVoiceChanels"]) && Context.Channel.Id != ulong.Parse(_config["privateParamChanels"]))
                        {
                            await RespondAsync("Вы не можете сейчас использовать эту команду.", ephemeral: true);
                            return;
                        }*/

                        Console.WriteLine($"Log | TakeGenderRoles | Send embed to gender variables.");

                        ITextChannel channel = Context.Client.GetChannel(ulong.Parse(_config["systeamLogChanel"])) as ITextChannel;
                        var EmbedBuilderLog = new EmbedBuilder()
                            .WithDescription($"Кто ты, воин? <:xsaritoshkaWink:1200455852457459835>\n" +
                            $"Вы можете выбрать свой гендер только один раз.\n " +
                            $"При указании неверного гендера, модер будет зол😡\n " +
                            $"👦🏿 - <@&1198226663318757538>\n" +
                            $"👧🏿 - <@&1198226451917439017>")
                            .WithFooter(footer =>
                            {
                                footer
                                .WithText("Взаимодействие")
                                .WithIconUrl(Context.User.GetAvatarUrl());
                            });
                        Embed embedLog = EmbedBuilderLog.Build();

                        Emoji[] emoji = new Emoji[] { "👦", "👧" };

                        var button1 = new ButtonBuilder()
                        .WithStyle(ButtonStyle.Primary)
                        .WithEmote(emoji[0])
                        .WithCustomId("button_click1");

                        var button2 = new ButtonBuilder()
                        .WithStyle(ButtonStyle.Primary)
                        .WithEmote(emoji[1])
                        .WithCustomId("button_click2");

                        var builder = new ComponentBuilder()
                            .WithButton(button1)
                            .WithButton(button2);

                        await RespondAsync(null, embed: EmbedBuilderLog.Build(), components: builder.Build(), ephemeral: true);
                    }
                    catch (RateLimitedException ex)
                    {
                        // Получаем время ожидания из исключения
                        Console.WriteLine(ex.Message);

                        // Ожидаем указанное время и повторяем запрос
                        await Task.Delay(1800);
                        // Повторяем запрос здесь

                        await TakeGenderRoles();
                    }
                    catch (Exception ex) 
                    {
                        await RespondAsync($"Ты чо дурак блин 👉👈😳?", ephemeral: true);
                        Console.WriteLine($"Exception | TakeGenderRoles | {ex.Message}");
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

                    }*/
            }
        }
    }
}
