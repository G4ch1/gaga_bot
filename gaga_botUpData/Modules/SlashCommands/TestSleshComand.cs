using Discord.Interactions;
using Discord;
using Discord.WebSocket;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using gaga_bot.Attributes;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace gaga_bot.Modules.SlashCommands
{
    public class TestSleshComand : InteractionModuleBase<SocketInteractionContext>
    {
        // можно получить с помощью внедрения свойств, общедоступные свойства с общедоступными сеттерами будут установлены пакетами зависимостей
        public InteractionService _commands { get; set; }
        private readonly CommandHandler _handler;
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _config;

        private const int delay = 1000;

        // внедрение конструктора также является допустимым способом доступа к зависимостям
        public TestSleshComand(DiscordSocketClient client, CommandHandler handler)
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

        [EnabledInDm(false)]
        [SlashCommand("test", "test")]
        public async Task Test()
        {
            var chanel = Context.Channel;
            var user = Context.User;
            await RespondAsync($"Ну прожал ты тест, и чо, типо тестер дохуя?", ephemeral: true);
            //EmbedMessages.SendEmbed(user,chanel);
        }

        // Эта команда будет выглядеть следующим образом
        // group-name subcommand-group-name echo
        [EnabledInDm(false)]
        [RequireRole("котенок-поваренок")]
        //[SlashCommand("echo", "Echo an input")]
        public async Task EchoSubcommand(string input)
            => await RespondAsync(input, components: new ComponentBuilder().WithButton("Echo", $"echoButton_{input}").Build());

        // Взаимодействие компонентов с игнорированием имен групп (ignoreGroupNames), установленным на true
        [ComponentInteraction("echoButton_*", true)]
        public async Task EchoButton(string input)
            => await RespondAsync(input);

        /// <summary>
        /// Модальные команды
        /// </summary>
        /// <returns>
        /// Последний параметр модальных команд должен быть реализацией IModal.
        /// Модальная реализация будет выглядеть так:
        /// </returns>
        // Регистрирует команду, которая будет отвечать модальным сообщением.
        [EnabledInDm(false)]
        //[RequireRole("котенок-поваренок")]
        [SlashCommand("предложка", "Тут вы можете предложить свою идею.")]
        public async Task Command()
            => await Context.Interaction.RespondWithModalAsync<FoodModal>("предложка");

        // Определяет модальность, которая будет отправлена.
        public class FoodModal : IModal
        {
            public string Title => "Предложка";
            // Строки с атрибутом ModalTextInput автоматически становятся компонентами.

            [InputLabel("Тема")]
            [ModalTextInput("Тема", placeholder: "...", maxLength: 20)]
            public string Food { get; set; }

            // Для дальнейшей настройки ввода можно указать дополнительные параметры.    
            // Параметры могут быть необязательными
            [RequiredInput(false)]
            [InputLabel("Описание")]
            [ModalTextInput("Описание", TextInputStyle.Paragraph, "...", maxLength: 500)]
            public string Reason { get; set; }
        }

        //Отвечает на модальный.

        [ModalInteraction("предложка")]
        public async Task ModalResponse(FoodModal modal)
        {
            // Проверьте, заполнено ли поле "Почему?

            string reason = string.IsNullOrWhiteSpace(modal.Reason)
                ? "."
                : $" because {modal.Reason}";

            // Постройте сообщение для отправки..
            string message = "Ваше предложение отправленно на расмотрение 😎";

            // Укажите AllowedMentions, чтобы мы не пинговали всех подряд..
            AllowedMentions mentions = new();
            mentions.AllowedTypes = AllowedMentionTypes.Users;

            // Ответить на модальный.
            await RespondAsync(message, allowedMentions: mentions, ephemeral: true);
        }





        /// <summary>
        /// Автозаполнение команд
        /// </summary>
        /// <returns></returns>
        /// [EnabledInDm(true)]
        [AutocompleteCommand("parameter_name", "опросник")]
        public async Task Autocomplete()
        {
            string userInput = (Context.Interaction as SocketAutocompleteInteraction).Data.Current.Value.ToString();

            IEnumerable<AutocompleteResult> results = new[]
            {
        new AutocompleteResult("1", "Вы выбрали 1"),
        new AutocompleteResult("2", "Вы выбрали 2"),
        new AutocompleteResult("3", "Вы выбрали 3"),
    }.Where(x => x.Name.StartsWith(userInput, StringComparison.InvariantCultureIgnoreCase)); // отправлять только те предложения, которые начинаются с ввода пользователя; использовать нечувствительное к регистру соответствие


            //максимум - 25 предложений одновременно
            await (Context.Interaction as SocketAutocompleteInteraction).RespondAsync(results.Take(25));
        }

        // вам нужно добавить атрибут `Autocomplete` перед параметром, чтобы добавить к нему автозавершение
        //[RequireOwner]
        [EnabledInDm(false)]
        [SlashCommand("опросник", "Пройди опрос")]
        public async Task ExampleCommand([Summary("parameter_name"), Autocomplete] string parameterWithAutocompletion)
            => await RespondAsync($"Вы ввели свое значение: {parameterWithAutocompletion}", ephemeral: true);





        public class Vector3
        {
            public int X { get; }
            public int Y { get; }
            public int Z { get; }

            public Vector3()
            {
                X = 0;
                Y = 0;
                Z = 0;
            }

            [ComplexParameterCtor]
            public Vector3(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        // Both of the commands below are displayed to the users identically.

        // With complex parameter
        [EnabledInDm(false)]
        //[SlashCommand("create-vector", "Create a 3D vector.")]
        public async Task CreateVector([ComplexParameter] Vector3 vector3)
        {
            await RespondAsync($"Test", ephemeral: true);
        }






        //Типы каналов для параметра IChannel также можно ограничить с помощью атрибута типов каналов .
        [RequireOwner]
        [EnabledInDm(false)]
        //[SlashCommand("name", "Description")]
        public async Task Command([ChannelTypes(ChannelType.Stage, ChannelType.Text)] IChannel channel)
        {
            await RespondAsync("Test", ephemeral: true);
        }

        public enum Animal
        {
            Cat,
            Dog,
            // You can also use the ChoiceDisplay attribute to change how they appear in the choice menu.
            [ChoiceDisplay("Guinea pig")]
            GuineaPig
        }

        /*ITextChannel channel = Context.Client.GetChannel(Context.Channel.Id) as ITextChannel;
        var EmbedBuilderLog = new EmbedBuilder()
            .WithDescription($"Test\n test\n Test\n test\n Test\n test\n")
            .WithFooter(footer =>
            {
                footer
                .WithText($"{Context.User.Username}")
                .WithIconUrl(Context.User.GetAvatarUrl());
            });

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

        await RespondAsync(null, embed: EmbedBuilderLog.Build(), components: builder.Build());*/
    }
}
