using Discord.Interactions;
using Discord.WebSocket;
using Discord;

using System;
using System.Threading.Tasks;

using static LibraryAttributes.EnumValues;
using gaga_bot.Attributes;
using Microsoft.Extensions.Configuration;

namespace gaga_bot.Modules.SlashCommands
{
    public class InteractionSleschCommands : InteractionModuleBase<SocketInteractionContext>
    {
        // можно получить с помощью внедрения свойств, общедоступные свойства с общедоступными сеттерами будут установлены пакетами зависимостей
        public InteractionService _commands { get; set; }
        private readonly CommandHandler _handler;
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _config;

        // внедрение конструктора также является допустимым способом доступа к зависимостям
        public InteractionSleschCommands(DiscordSocketClient client, CommandHandler handler)
        {
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json");

            // build the configuration and assign to _config          
            _config = _builder.Build();

            _handler = handler;
            _client = client;
            //_client.JoinedGuild += UserJoinAsync;
        }

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
                [RequireOwner]
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

                [RequireOwner]
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

            /*[Group("music", "музыка")]
            public class ModerationGroupModule : InteractionModuleBase<SocketInteractionContext>
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
