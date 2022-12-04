using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using gaga_bot.Attributes;

namespace gaga_bot.Modules.SlashCommands
{
    public class ExampleModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Доступ к зависимостям можно получить с помощью внедрения свойств, общедоступные свойства
        // с общедоступными сеттерами будут установлены поставщиком услуг
        public InteractionService Commands { get; set; }

        private CommandHandler _handler;

        // Внедрение конструктора также является допустимым способом доступа к зависимостям
        public ExampleModule(CommandHandler handler)
        {
            _handler = handler;
        }

        [SlashCommand("test", "test comand")]
        public async Task TestAscyncCommand(SocketGuildUser guildUser)
        {
            await RespondAsync(guildUser.Guild.ToString());
        }





        



        //
        // КОМАНДА С АВТОЗАПОЛНЕНИЕМ
        //
        [SlashCommand("command_name", "command_description")]
        public async Task ExampleCommand([Summary("parameter_name"), Autocomplete] string parameterWithAutocompletion)
    => await RespondAsync($"Your choice: {parameterWithAutocompletion}");

        [AutocompleteCommand("parameter_name", "command_name")]
        public async Task Autocomplete()
        {
            string userInput = (Context.Interaction as SocketAutocompleteInteraction).Data.Current.Value.ToString();

            IEnumerable<AutocompleteResult> results = new[]
            {
        new AutocompleteResult("foo", "foo_value"),
        new AutocompleteResult("bar", "bar_value"),
        new AutocompleteResult("baz", "baz_value"),
    }.Where(x => x.Name.StartsWith(userInput, StringComparison.InvariantCultureIgnoreCase)); // only send suggestions that starts with user's input; use case insensitive matching


            // max - 25 suggestions at a time
            await (Context.Interaction as SocketAutocompleteInteraction).RespondAsync(results.Take(25));
        }

        //
        // ДИАЛОГОВЫЕ ОКНА
        // Регистрирует команду, которая будет отвечать модальным окном.
        [SlashCommand("food", "Tell us about your favorite food.")]
        public async Task Command()
            => await Context.Interaction.RespondWithModalAsync<FoodModal>("food_menu");


        public class FoodModal : IModal
        {
            public string Title => "Fav Food";
            // Строки с атрибутом ModalTextInput автоматически станут компонентами.
            [InputLabel("What??")]
            [ModalTextInput("food_name", placeholder: "Pizza", maxLength: 20)]
            public string Food { get; set; }

            // Можно указать дополнительные параметры для дальнейшей настройки ввода.
            // Параметры могут быть необязательными
            [RequiredInput(false)]
            [InputLabel("Why??")]
            [ModalTextInput("food_reason", TextInputStyle.Paragraph, "Kuz it's tasty", maxLength: 500)]
            public string Reason { get; set; }
        }

        // Отвечает на модальное окно.
        [ModalInteraction("food_menu")]
        public async Task ModalResponse(FoodModal modal)
        {
            // Проверить, если "Почему??" поле заполнено
            string reason = string.IsNullOrWhiteSpace(modal.Reason)
                ? "."
                : $" because {modal.Reason}";

            // Создаем сообщение для отправки.
            string message = "hey @everyone, I just learned " +
                $"{Context.User.Mention}'s favorite food is " +
                $"{modal.Food}{reason}";

            // Укажите AllowedMentions, чтобы мы фактически не пинговали всех.
            AllowedMentions mentions = new();
            mentions.AllowedTypes = AllowedMentionTypes.Users;

            // Ответ на модальное окно.
            await RespondAsync(message, allowedMentions: mentions, ephemeral: true);
        }
        // Вы можете использовать несколько типов параметров в обработчиках Slash Command
        // (string, int, double, bool, IUser, IChannel, IMentionable, IRole, Enums) по умолчанию. Необязательно,
        // вы можете реализовать свои собственные преобразователи типов для поддержки более
        // широкого диапазона типов параметров. Для получения дополнительной информации обратитесь к документации библиотеки.
        // Необязательные параметры метода (параметры со значением по умолчанию) также будут
        // отображаться как необязательные в Discord.

        // [Group] создаст группу команд. [SlashCommand] и [ComponentInteraction] будут зарегистрированы с префиксом группы.
        [Group("test_group", "This is a command group")]
        public class GroupExample : InteractionModuleBase<SocketInteractionContext>
        {
            // Вы можете создавать варианты команд либо с помощью атрибута [Choice],
            // либо путем создания перечисления. Каждое перечисление с 25 или менее значениями будет зарегистрировано как кратное
            // вариант выбора
            [SlashCommand("choice_example", "Enums create choices")]
            public async Task ChoiceExample(ExampleEnum input)
                => await RespondAsync(input.ToString());
        }

        // Используйте [ComponentInteraction] для обработки взаимодействия компонентов сообщения.
        // Будет выполнено взаимодействие компонента сообщения с соответствующим customId.
        // Кроме того, вы можете создать подстановочный знак, используя символ '*'. Interaction
        // Service выполнит отложенный поиск регулярных выражений и захватит совпадающие строки.
        // Затем вы можете получить доступ к этим группам захвата из параметров метода в том
        // порядке, в котором они были захвачены. Используя шаблон подстановочных знаков, вы
        // можете выбирать взаимодействия компонентов.
        [ComponentInteraction("musicSelect:*,*")]
        public async Task ButtonPress(string id, string name)
        {
            // ...
            await RespondAsync($"Playing song: {name}/{id}");
        }

        // Выберите взаимодействия с меню, содержат идентификаторы пунктов меню, которые были
        // выбраны пользователем. Вы можете получить доступ к идентификаторам опций из параметров метода.
        // Вы также можете использовать шаблон подстановочных знаков с меню выбора, в этом случае
        // захваты подстановочных знаков будут переданы методу сначала, а затем идентификаторы параметров.
        [ComponentInteraction("roleSelect")]
        public async Task RoleSelect(string[] selections)
        {
            throw new NotImplementedException();
        }

        // С помощью атрибута DoUserCheck вы можете убедиться, что только пользователь, на которого
        // нацелена эта кнопка, может щелкнуть ее. Это определяется первым подстановочным знаком: *.
        // Подробную информацию см. в разделе Attributes/DoUserCheckAttribute.cs.
        [DoUserCheck]
        [ComponentInteraction("myButton:*")]
        public async Task ClickButtonAsync(string userId)
            => await RespondAsync(text: ":thumbsup: Clicked!");

        // Эта команда будет приветствовать целевого пользователя в канале, в котором она была выполнена.
        [UserCommand("greet")]
        public async Task GreetUserAsync(IUser user)
            => await RespondAsync(text: $":wave: {Context.User} said hi to you, <@{user.Id}>!");

        // Закрепляем сообщение в канале, в котором оно находится.
        [MessageCommand("pin")]
        public async Task PinMessageAsync(IMessage message)
        {
            // делаем безопасное приведение, чтобы проверить, является ли сообщение ISystem- или IUserMessage
            if (message is not IUserMessage userMessage)
                await RespondAsync(text: ":x: You cant pin system messages!");

            // если булавки в этом канале равны или выше 50, сообщения больше не могут быть закреплены.
            else if ((await Context.Channel.GetPinnedMessagesAsync()).Count >= 50)
                await RespondAsync(text: ":x: You cant pin any more messages, the max has already been reached in this channel!");

            else
            {
                await userMessage.PinAsync();
                await RespondAsync(":white_check_mark: Successfully pinned message!");
            }
        }
        //ГРУПОВЫЕ КОМАНДЫ
        /*// You can put commands in groups
        [Group("group-name", "Group description")]
        public class CommandGroupModule : InteractionModuleBase<SocketInteractionContext>
        {
            // This command will look like
            // group-name ping
            [SlashCommand("ping", "Get a pong")]
            public async Task PongSubcommand()
                => await RespondAsync("Pong!");

            // And even in sub-command groups
            [Group("subcommand-group-name", "Subcommand group description")]
            public class SubСommandGroupModule : InteractionModuleBase<SocketInteractionContext>
            {
                // This command will look like
                // group-name subcommand-group-name echo
                [SlashCommand("echo", "Echo an input")]
                public async Task EchoSubcommand(string input)
                    => await RespondAsync(input, components: new ComponentBuilder().WithButton("Echo", $"echoButton_{input}").Build());

                // Component interaction with ignoreGroupNames set to true
                [ComponentInteraction("echoButton_*", true)]
                public async Task EchoButton(string input)
                    => await RespondAsync(input);
            }
        }*/
    }
}
