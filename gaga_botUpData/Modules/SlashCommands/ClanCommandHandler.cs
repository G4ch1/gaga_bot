using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gaga_bot.Modules.SlashCommands
{
    public class ClanCommandHandler : InteractionModuleBase<SocketInteractionContext>
    {
        // можно получить с помощью внедрения свойств, общедоступные свойства с общедоступными сеттерами будут установлены пакетами зависимостей
        public InteractionService _commands { get; set; }
        private readonly CommandHandler _handler;
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _config;

        // внедрение конструктора также является допустимым способом доступа к зависимостям
        public ClanCommandHandler(DiscordSocketClient client, CommandHandler handler)
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

        /*[Group("clan", "кланы")]
        public class CommandGroupModule : InteractionModuleBase<SocketInteractionContext>
        {
            [RequireOwner]
            [SlashCommand("create", "Создать клан")]
            public async Task CreateClan()
            {
                //проверяет, есть ли у пользователя средства на создание канала
                //создаёт клановую роль
                //создаёт клановый канал
                await RespondAsync("В разработке", ephemeral: true);
            }

            [RequireOwner]
            [SlashCommand("request", "подать заявку")]
            public async Task RequestToJoin()
            {
                //запрос на вступление в клан
                await RespondAsync("В разработке", ephemeral: true);
            }

            [RequireOwner]
            [SlashCommand("accept-in-clan", "принять заявку")]
            public async Task AcceptIntoClan()
            {
                //запрос на вступление в клан
                await RespondAsync("В разработке", ephemeral: true);
            }
        }*/

    }
}
