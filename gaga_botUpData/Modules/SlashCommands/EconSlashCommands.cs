using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using gaga_bot.Attributes;
using Microsoft.Extensions.Configuration;
using Discord.Net;

namespace gaga_bot.Modules.SlashCommands
{
    public class EconSlashCommands : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService commands { get; set; }
        private CommandHandler _handler;
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _config;

        public EconSlashCommands(DiscordSocketClient client, CommandHandler handler)
        {
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json");
         
            _config = _builder.Build();
            _handler = handler;
            _client = client;
        }

        //[EnabledInDm(false)]
        [SlashCommand("profiles", "Посмотреть профиль")]
        public async Task ChekBalance()
        {
            try
            {
                var user = Context.User;
                Console.WriteLine("Log | ChekBalance | Start commands");
                var query = RequestHandlers.ExecuteReader($"SELECT DiscordId, Currency, (SELECT COUNT(UserId) FROM Estimate " +
                    $"WHERE UserId = {user.Id}) AS Likes, VoiceTime FROM Users WHERE DiscordId = {user.Id}");

                var tableBuilder = new StringBuilder();

                while (query.Read())
                {
                    var discordId = Context.Guild.GetUser((ulong)query.GetInt64(0));

                    tableBuilder.AppendLine($"**Пользователь:** {discordId.Username}");
                    tableBuilder.AppendLine($"**Баланс:** {query.GetDecimal(1)} | **:thumbsup:** {query.GetInt32(2)} | **:microphone:** {query.GetTimeSpan(3)}");
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
            catch (RateLimitedException ex)
            {
                // Получаем время ожидания из исключения
                Console.WriteLine(ex.Message);

                // Ожидаем указанное время и повторяем запрос
                await Task.Delay(1800);
                // Повторяем запрос здесь

                await ChekBalance();
            }
            catch (Exception ex) { Console.WriteLine($"Exception | ChekBalance | Exceptions {ex.Message}"); }
        }
    }
}
