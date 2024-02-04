using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace gaga_bot.Modules.SlashCommands
{
    public class PrivateVoiceChanels : InteractionModuleBase<SocketInteractionContext>
    {
        // можно получить с помощью внедрения свойств, общедоступные свойства с общедоступными сеттерами будут установлены пакетами зависимостей
        public InteractionService _commands { get; set; }
        private readonly CommandHandler _handler;
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _config;

        private const int delay = 1000;

        // внедрение конструктора также является допустимым способом доступа к зависимостям
        public PrivateVoiceChanels(DiscordSocketClient client, CommandHandler handler)
        {
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json");

            // build the configuration and assign to _config          
            _config = _builder.Build();

            _handler = handler;
            _client = client;
        }

        /*[EnabledInDm(false)]
        [SlashCommand("ограничить-участников", "test")]
        public async Task UserLimitVoice()
        {
            await RespondAsync($"Ну не сделал, и чо?", ephemeral: true);
        }*/

        [EnabledInDm(false)]
        [SlashCommand("закрыть-руму", "test")]
        public async Task CloseVoiceChanels()
        {
            try
            {
                var user = Context.User as SocketGuildUser;
                var voiceChannel = user.VoiceChannel;

                if (Context.Channel.Id != ulong.Parse(_config["privateParamChanels"]))
                {
                    await RespondAsync($"Эту команду нужно писать в <#{ulong.Parse(_config["privateParamChanels"])}> 👉👈😳", ephemeral: true);
                    return;
                }
                else if (voiceChannel.Name != user.Username)
                {
                    Console.WriteLine(voiceChannel.Name + "\n" + user.Username);
                    await RespondAsync($"А ты точно главный в руме? 👉👈😳", ephemeral: true);
                    return;
                }
                else
                {
                    //var chanel = Context.Channel as SocketMessage;
                    OverwritePermissions permissions = new OverwritePermissions(
                                                connect: PermValue.Deny, // Разрешено подключаться к голосовому каналу
                                                speak: PermValue.Allow // Разрешено говорить в голосовом канале
                                            );
                    // Применяем изменения к каналу
                    await voiceChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, permissions);

                    await RespondAsync($"закрыл.", ephemeral: true);
                }

            }
            catch (Exception ex)
            {
                await RespondAsync($"Ты чо дурак блин 👉👈😳?", ephemeral: true);
                Console.WriteLine($"Exception | CloseVoiceChanels | {ex.Message}");
            }
        }

        [EnabledInDm(false)]
        [SlashCommand("открыть-руму", "test")]
        public async Task OpenVoiceChanels()
        {
            try
            {
                var user = Context.User as SocketGuildUser;
                var voiceChannel = user.VoiceChannel;

                if (Context.Channel.Id != ulong.Parse(_config["privateParamChanels"]))
                {
                    await RespondAsync($"Эту команду нужно писать в <#{ulong.Parse(_config["privateParamChanels"])}> 👉👈😳", ephemeral: true);
                    return;
                }
                else if (voiceChannel.Name != user.Username)
                {
                    await RespondAsync($"А ты точно главный в руме? 👉👈😳", ephemeral: true);
                    return;
                }
                else
                {
                    //var chanel = Context.Channel as SocketMessage;
                    OverwritePermissions permissions = new OverwritePermissions(
                                                connect: PermValue.Allow, // Разрешено подключаться к голосовому каналу
                                                speak: PermValue.Allow // Разрешено говорить в голосовом канале
                                            );
                    // Применяем изменения к каналу
                    await voiceChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, permissions);

                    await RespondAsync($"открыл.", ephemeral: true);
                }
            }
            catch (Exception ex)
            {
                await RespondAsync($"Ты чо дурак блин 👉👈😳?", ephemeral: true);
                Console.WriteLine($"Exception | OpenVoiceChanels | {ex.Message}");
            }
        }

        [EnabledInDm(false)]
        [SlashCommand("забрать-доступ", "test")]
        public async Task UserCloseVoiceChanels(IUser in_user)
        {
            try
            {
                var user = Context.User as SocketGuildUser;
                var voiceChannel = user.VoiceChannel;
                if (Context.Channel.Id != ulong.Parse(_config["privateParamChanels"]))
                {
                    await RespondAsync($"Эту команду нужно писать в <#{ulong.Parse(_config["privateParamChanels"])}> 👉👈😳", ephemeral: true);
                    return;
                }// Проверяем, является ли пользователь тега ботом
                else if (in_user.Id == ulong.Parse(_config["botId"]))
                {
                    await RespondAsync($"Не проказничай 👉👈😳", ephemeral: true);
                    return;
                }
                else if (voiceChannel.Name != user.Username)
                {
                    await RespondAsync($"А ты точно главный в руме? 👉👈😳", ephemeral: true);
                    return;
                }
                else
                {
                    //var chanel = Context.Channel as SocketMessage;
                    OverwritePermissions permissions = new OverwritePermissions(
                                            connect: PermValue.Deny, // Разрешено подключаться к голосовому каналу
                                            speak: PermValue.Allow // Разрешено говорить в голосовом канале
                                        );
                    // Применяем изменения к каналу
                    await voiceChannel.AddPermissionOverwriteAsync(in_user, permissions);

                    await RespondAsync($"забрал.", ephemeral: true);
                }
            }
            catch (Exception ex)
            {
                await RespondAsync($"Ты чо дурак блин 👉👈😳?", ephemeral: true);
                Console.WriteLine($"Exception | UserCloseVoiceChanels | {ex.Message}");
            }
        }

        [EnabledInDm(false)]
        [SlashCommand("дать-доступ", "test")]
        public async Task UserOpenVoiceChanels(IUser in_user)
        {
            try
            {
                var user = Context.User as SocketGuildUser;
                var voiceChannel = user.VoiceChannel;
                if(Context.Channel.Id != ulong.Parse(_config["privateParamChanels"]))
                {
                    await RespondAsync($"Эту команду нужно писать в <#{ulong.Parse(_config["privateParamChanels"])}> 👉👈😳", ephemeral: true);
                    return;
                }// Проверяем, является ли пользователь тега ботом
                else if(in_user.Id == ulong.Parse(_config["botId"]))
                {
                    await RespondAsync($"Не проказничай 👉👈😳", ephemeral: true);
                    return;
                }
                else if(voiceChannel.Name != user.Username)
                {
                    await RespondAsync($"А ты точно главный в руме? 👉👈😳", ephemeral: true);
                    return;
                }
                else
                {
                    //var chanel = Context.Channel as SocketMessage;
                    OverwritePermissions permissions = new OverwritePermissions(
                                            connect: PermValue.Allow, // Разрешено подключаться к голосовому каналу
                                            speak: PermValue.Allow // Разрешено говорить в голосовом канале
                                        );
                    // Применяем изменения к каналу
                    await voiceChannel.AddPermissionOverwriteAsync(in_user, permissions);

                    await RespondAsync($"отдал.", ephemeral: true);
                }
            }
            catch (Exception ex)
            {
                await RespondAsync($"Ты чо дурак блин 👉👈😳?", ephemeral: true);
                Console.WriteLine($"Exception | UserOpenVoiceChanels | {ex.Message}");
            }
        }
    }
}
