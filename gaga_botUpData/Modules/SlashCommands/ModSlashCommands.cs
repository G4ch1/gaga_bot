using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

using gaga_bot.Attributes;
using Sentry;

namespace gaga_bot.Modules.SlashCommands
{
    public class ModSlashCommands : InteractionModuleBase<SocketInteractionContext>
    {
        // можно получить с помощью внедрения свойств, общедоступные свойства с общедоступными сеттерами будут установлены поставщиком услуг
        public InteractionService commands { get; set; }
        private CommandHandler _handler;
        private readonly DiscordSocketClient _client;

        public UserBan userBan = new UserBan();
        public UserWarn userWarn = new UserWarn();
        public UserMute userMute = new UserMute();


        // внедрение конструктора также является допустимым способом доступа к зависимостям
        public ModSlashCommands(DiscordSocketClient client, CommandHandler handler)
        {
            _handler = handler;
            _client = client;



            //_client.JoinedGuild += UserJoinAsync;
        }

       


        [EnabledInDm(true)]
        [SlashCommand("ban", "бан блять")]
        [DefaultMemberPermissions(GuildPermission.BanMembers)]
        public async Task BanUser(SocketGuildUser user, string reason, string time)
        {
            //ulong guildRole = 1015567150536204339;
            await user.AddRoleAsync((ulong)StaticVariables.banRoles);

            userBan.reason = reason;
            userBan.userID = user.Id.ToString();
            userBan.time = time;

            if (userMute.time != null)
                await Requests.RequestsBan(userBan);

            ITextChannel channel = Context.Client.GetChannel((ulong)StaticVariables.logChanel) as ITextChannel;
            var EmbedBuilderLog = new EmbedBuilder()
                .WithDescription($"{user.Mention} был забанен \n**Причина** {reason}\n**Модератором** {Context.User.Mention}")
                .WithFooter(footer =>
                {
                    footer
                    .WithText("User ban log")
                    .WithIconUrl(Context.User.GetAvatarUrl());
                });
            Embed embedLog = EmbedBuilderLog.Build();
            await channel.SendMessageAsync(embed:embedLog);

            await RespondAsync($"В бан нахуй {user.Username}, по причине {reason} на {time}");
        }

        [SlashCommand("unban", "разбан")]
        [DefaultMemberPermissions(GuildPermission.BanMembers)]
        public async Task UnBanUser(SocketGuildUser user)
        {
            //ulong guildRole = 1015567150536204339;
            await user.RemoveRoleAsync((ulong)StaticVariables.banRoles);
            await RespondAsync($"Солнце встало для {user.Username}");
        }



        [SlashCommand("mut", "мут блять")]
        [DefaultMemberPermissions(GuildPermission.MuteMembers)]
        public async Task MutUser(SocketGuildUser user, string reason, string time)
        {
            userMute.reason = reason;
            userMute.userID = user.Id.ToString();
            userMute.time = time;

            if (userMute.time != null)
                await Requests.RequestsMute(userMute);

            //ulong guildRole = 1015567148220952626;
            await user.AddRoleAsync((ulong)StaticVariables.muteRoles);
            await RespondAsync($"В мут нахуй {user.Username}, по причине {reason} на {time}");
        }

        [SlashCommand("unmut", "размут блять")]
        [DefaultMemberPermissions(GuildPermission.MuteMembers)]
        public async Task UnMutUser(SocketGuildUser user)
        {
            //ulong guildRole = 1015567148220952626;
            await user.RemoveRoleAsync((ulong)StaticVariables.muteRoles);
            await RespondAsync($"Солнце встало для {user.Username}");
        }

        [SlashCommand("allmut", "Показать людей с мутами")]
        [DefaultMemberPermissions(GuildPermission.MuteMembers)]
        public async Task AllmutMember(SocketGuildUser user)
        {
            await RespondAsync($"Команда в разработке");
        }

        [SlashCommand("warn", "предупреждение чела")]
        [DefaultMemberPermissions(GuildPermission.BanMembers)]
        public async Task WarnMember(SocketGuildUser user, string reason)
        {
            userWarn.reason = reason;
            userWarn.userID = user.Id.ToString();
            userWarn.valid = true;

            await Requests.RequestsWarn(userWarn);

            await RespondAsync($"Чепух {user.Username} был предупреждён.");
        }

        [SlashCommand("rewarn", "убрать пред")]
        [DefaultMemberPermissions(GuildPermission.BanMembers)]
        public async Task RewarnMember(SocketGuildUser user, string warnid)
        {
            userWarn.warnID = warnid;
            userWarn.userID = user.Id.ToString();
            userWarn.valid = false;

            await Requests.RequestsWarn(userWarn);

            await RespondAsync($"Кент на {user.Username} оправдан.");
        }

        [SlashCommand("allwarn", "показать все варны")]
        [DefaultMemberPermissions(GuildPermission.BanMembers)]
        public async Task AllwarnMember(SocketGuildUser user)
        {
            await Requests.AllWarnUser(userWarn);

            ITextChannel channel = Context.Client.GetChannel((ulong)StaticVariables.logChanel) as ITextChannel;
            var EmbedBuilderLog = new EmbedBuilder()
                .WithDescription($"Предупреждения участника **{user.Username}:**")
                .WithFooter(footer =>
                {
                    footer
                    .WithText($"Случай {userWarn.warnID} от _**{userWarn.date}**_ числа, выдан {userWarn.issued}")
                    .WithIconUrl(Context.User.GetAvatarUrl());
                });
            Embed embedLog = EmbedBuilderLog.Build();
            await channel.SendMessageAsync(embed: embedLog);

            await RespondAsync($"");
        }

        [SlashCommand("clear", "модер уберись блять")]
        [DefaultMemberPermissions(GuildPermission.BanMembers)]
        public async Task ClearChat(ITextChannel textChannel, int amount)
        {
            IEnumerable<IMessage> messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
            const int delay = 3000;
            IUserMessage m = await ReplyAsync($"Модер убрал {amount} сообщений.");
            await Task.Delay(delay);
            await m.DeleteAsync();
        }

        /*[SlashCommand("kick", "нахуй чела.")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task BanGuild(SocketGuildUser targetUser, [Discord.Commands.Remainder] string reason = "Нету такова.")
        {
            await targetUser.KickAsync(reason);
            await ReplyAsync($"**{targetUser}** Забанен. Bye bye :wave:");
        }

        [SlashCommand("unkick", "Разбан ебла")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task UnBanGuild(SocketGuildUser targetUser)
        {
            IUser user = (IUser)targetUser.Id.ToDictionary();
            await targetUser.Guild.RemoveBanAsync(user);
            await ReplyAsync($"**{targetUser}** Unbanned");
        }*/

        /*[SlashCommand("register", "регаца тута")]
        public async Task RegisterUser(SocketGuildUser socketGuildUser)
        {



            User user = new User();
            user.Id = socketGuildUser.Id.ToString();
            user.Coin = "0";

            //"SELECT id FROM users where id={member.id}")
            //"INSERT INTO users VALUES ({member.id}, '{member.name}', '<@{member.id}>', 50000, 'S','[]',0,0)")

            Requests requests = new Requests();
            requests.RequestsUser($"INSERT INTO User (id, coin) VALUES ('{user.Id}', {user.Coin})");
        }*/

        /*[SlashCommand("colors", "поменять цвет кастомки")]
        public async Task ColorsRoles(IRole role, string roleName, Color color)
        {
            await role.ModifyAsync(x =>
            {
                x.Name = roleName;
                x.Color = color;
            });
            //await RespondAsync($"Команда в разработке");
        }*/

        /*[SlashCommand("timerole", "кастомная роль")]
        public async Task TimeRoleMember(SocketGuildUser socketGuildUser, string nameRole, GuildPermissions? guildPermissions , Color? color)
        {
            await Context.Guild.CreateRoleAsync(nameRole, guildPermissions, color);
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == nameRole);
            await socketGuildUser.AddRoleAsync(role);
            //await RespondAsync($"Команда в разработке");
        }*/
        /*[Command("Role"]
        public async Task RoleAsync(SocketGuildUser socketGuildUser)
        {
            string roleName = "Name"; //Имя роли которое хочешь выдать
                                      //Получение списка ролей на сервере
            var guildRoles = socketGuildUser.Guild.Roles;
            //Поиск роли с именем Name
            foreach (var guildRole in guildRoles)
            {
                //Если нашёл роль, то выдаст пользователю которого мы отметили([User ID])
                if (guildRole.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase))
                {
                    await socketGuildUser.AddRoleAsync(guildRole);
                    break;
                }
            }
            return;
        }*/
    }
}
