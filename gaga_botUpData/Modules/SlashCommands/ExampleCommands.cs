using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;
using gaga_bot.Attributes;


namespace gaga_bot.Modules.SlashCommands
{
    // должны быть общедоступными и наследоваться от IInterationModuleBase
    public class ExampleCommands : InteractionModuleBase<SocketInteractionContext>
    {
        // можно получить с помощью внедрения свойств, общедоступные свойства с общедоступными сеттерами будут установлены поставщиком услуг
        public InteractionService Commands { get; set; }
        private CommandHandler _handler;
        private readonly DiscordSocketClient _client;

        // внедрение конструктора также является допустимым способом доступа к зависимостям
        public ExampleCommands(DiscordSocketClient client, CommandHandler handler)
        {
            _handler = handler;
            _client = client;

            //_client.JoinedGuild += UserJoinedAsync;

            //_client.JoinedGuild += UserJoinAsync;
        }


       

        [SlashCommand("list-roles", "показать все твои роли")]
        public async Task HandleListRoleCommand(SocketGuildUser socketGuildUser)
        {
            var roleList = string.Join(",\n", socketGuildUser.Roles.Where(x => !x.IsEveryone).Select(x => x.Mention));

            var embedBuiler = new EmbedBuilder()
                .WithAuthor(socketGuildUser.ToString(), socketGuildUser.GetAvatarUrl() ?? socketGuildUser.GetDefaultAvatarUrl())
                .WithTitle("Roles")
                .WithDescription(roleList)
                .WithColor(Color.Green)
                .WithCurrentTimestamp();

            await RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
        }


        // дальше идут команды
        [SlashCommand("8ball", "Найди ответ!")]
        public async Task EightBall(string question)
        {
            // создать список возможных ответов
            var replies = new List<string>();

            // добавить возможные ответы
            replies.Add("Да");
            replies.Add("Нет");
            replies.Add("Может быть");
            replies.Add("hazzzzy....");

            // получить ответ
            var answer = replies[new Random().Next(replies.Count - 1)];

            // отослать ответ
            await RespondAsync($"Ты спросил: [**{question}**], и твой ответ: [**{answer}**]");
        }

        [SlashCommand("echo", "Повтори")]
        public async Task Echo(string echo, [Summary(description: "упомянуть пользователя")] bool mention = false)
            => await RespondAsync(echo + (mention ? Context.User.Mention : string.Empty));

        [SlashCommand("role", "выдать роль")]
        [RequireRole(1015567128235102238)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task RoleAsync(SocketGuildUser socketGuildUser, IRole role)
        {
            string roleName = role.ToString(); //Имя роли которое хочешь выдать
                                               //Получение списка ролей на сервере
            var guildRoles = socketGuildUser.Guild.Roles;
            //Поиск роли с именем Name
            foreach (var guildRole in guildRoles)
            {
                //Если нашёл роль, то выдаст пользователю которого мы отметили([User ID])
                if (guildRole.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase))
                {
                    await socketGuildUser.AddRoleAsync(guildRole);
                    await RespondAsync(text: $":sunglasses: Лох получил роль {roleName}!", ephemeral: true);
                }
            }
            return;
        }

        [SlashCommand("ping", "Пингует бота и возвращает его задержку.")]
        public async Task GreetUserAsync()
            => await RespondAsync(text: $":ping_pong: У меня заняло {Context.Client.Latency}мс ответить вам!", ephemeral: true);

        [SlashCommand("bitrate", "Получает битрейт определенного голосового канала.")]
        public async Task GetBitrateAsync([ChannelTypes(ChannelType.Voice, ChannelType.Stage)] IChannel channel)
            => await RespondAsync(text: $"Этот голосовой канал имеет битрейт {(channel as IVoiceChannel).Bitrate}");

        // Use [ComponentInteraction] to handle message component interactions. Message component interaction with the matching customId will be executed.
        // Alternatively, you can create a wild card pattern using the '*' character. Interaction Service will perform a lazy regex search and capture the matching strings.
        // You can then access these capture groups from the method parameters, in the order they were captured. Using the wild card pattern, you can cherry pick component interactions.
        /*[ComponentInteraction("musicSelect:*,*")]
        public async Task ButtonPress(string id, string name)
        {
            // ...
            await RespondAsync($"Playing song: {name}/{id}");
        }*/

        /*private async Task UserJoinedAsync(SocketGuild socketGuild)
        {
            

            //Без ID...
            //string roleName = "The role name to add to user"; //Или другое имущество
            //Получить список ролей в гильдии.
            *//*var guildRoles = socketGuildUser.Guild.Roles;
            //Прокрутите список ролей в гильдии
            foreach (var guildRole in guildRoles)
            {
                //Если текущая итерация роли соответствует имени роли
                if (guildRole.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase))
                {
                    //Назначьте роль пользователю.
                    await socketGuildUser.AddRoleAsync(guildRole);
                    //Выход из цикла.
                    break;
                }
            }*//*
        }*/

        /*private async Task UserJoinAsync(SocketGuild guild)
        {
            JObject config = Functions.GetConfig();


        }*/

    }
}