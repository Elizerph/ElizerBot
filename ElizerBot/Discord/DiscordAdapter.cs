using Discord;
using Discord.WebSocket;

using ElizerBot.Adapter;

namespace ElizerBot.Discord
{
    internal class DiscordAdapter : BotAdapter
    {
        private readonly DiscordSocketClient _client;

        public DiscordAdapter(string token, IBotAdapterUpdateHandler updateHandler) 
            : base(token, updateHandler)
        {
            var config = new DiscordSocketConfig
            { 
               
            };
            _client = new DiscordSocketClient(config);
        }

        public override async Task Init()
        {
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();
            await _client.ReadyAsync();

            _client.MessageReceived += Client_MessageReceived;
            _client.ButtonExecuted += Client_ButtonExecuted;
            _client.SlashCommandExecuted += Client_SlashCommandExecuted;
        }

        private Task Client_ButtonExecuted(SocketMessageComponent arg)
        {
            switch (arg.Type)
            {
                case InteractionType.MessageComponent:
                    var message = GetIncomingMessageAdapter(arg.Message);
                    return _updateHandler.HandleButtonPress(this, message, arg.Data.CustomId);
                default:
                    return Task.CompletedTask;
            }
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg.Author.IsBot)
                return;

            if (string.IsNullOrEmpty(arg.Content))
                return;

            var message = GetIncomingMessageAdapter(arg);
            await _updateHandler.HandleIncomingMessage(this, message); 
        }

        public override async Task<PostedMessageAdapter> SendMessage(NewMessageAdapter message)
        {
            var channel = await _client.GetChannelAsync(ulong.Parse(message.Chat.Id));
            if (channel is IMessageChannel messageChannel)
            {
                var buttonsComponent = GetMessageButtons(message.Buttons);
                var messageFeedback = await messageChannel.SendMessageAsync(message.Text, components: buttonsComponent);
                return new PostedMessageAdapter(message.Chat, messageFeedback.Id.ToString(), GetUserAdapter(messageFeedback.Author));
            }
            throw new InvalidOperationException($"{nameof(messageChannel)} is not {typeof(IMessageChannel)}");
        }

        private static MessageComponent? GetMessageButtons(IReadOnlyList<IReadOnlyList<ButtonAdapter>>? buttons)
        {
            if (buttons == null)
                return null;

            var builder = new ComponentBuilder();
            foreach (var buttonRow in buttons)
            {
                var rowBuilder = new ActionRowBuilder();
                foreach (var button in buttonRow)
                {
                    rowBuilder.WithButton(button.Label, button.Data);
                }
                builder.AddRow(rowBuilder);
            }
            return builder.Build();
        }

        private static UserAdapter GetUserAdapter(IUser user)
        {
            return new UserAdapter(user.Id.ToString())
            {
                Username = user.Username
            };
        }

        private static ChatAdapter GetChatAdapter(IChannel channel)
        { 
            return new ChatAdapter(channel.Id.ToString());
        }

        private static ButtonAdapter GetButtonAdapter(ButtonComponent component)
        {
            return new ButtonAdapter
            {
                Data = component.CustomId,
                Label = component.Label
            };
        }

        private static IReadOnlyList<IReadOnlyList<ButtonAdapter>>? GetButtonAdapters(IReadOnlyCollection<ActionRowComponent> components)
        {
            return components.Select(e => e.Components.Cast<ButtonComponent>().Select(GetButtonAdapter).ToArray()).ToArray();
        }

        private static PostedMessageAdapter GetIncomingMessageAdapter(SocketMessage message)
        {
            return new PostedMessageAdapter(GetChatAdapter(message.Channel), message.Id.ToString(), GetUserAdapter(message.Author))
            {
                Text = message.Content,
                Buttons = GetButtonAdapters(message.Components)
            };
        }

        public override async Task<PostedMessageAdapter> EditMessage(PostedMessageAdapter message)
        {
            var channel = await _client.GetChannelAsync(ulong.Parse(message.Chat.Id));
            if (channel is IMessageChannel messageChannel)
            {
                var feedback = await messageChannel.ModifyMessageAsync(ulong.Parse(message.Id), ps => 
                {
                    ps.Content = message.Text;
                    ps.Components = GetMessageButtons(message.Buttons);
                });
                return new PostedMessageAdapter(message.Chat, feedback.Id.ToString(), GetUserAdapter(feedback.Author));
            }
            throw new InvalidOperationException($"{nameof(messageChannel)} is not {typeof(IMessageChannel)}");
        }

        public override Task ClearCommands()
        {
            return _client.BulkOverwriteGlobalApplicationCommandsAsync(Array.Empty<ApplicationCommandProperties>());
        }

        public override async Task SetCommands(Dictionary<string, string> commands)
        {
            var currentCommands = await _client.GetGlobalApplicationCommandsAsync();
            if (commands.Select(p => p.Key).SequenceEqual(currentCommands.Select(e => e.Name)))
                return;

            var builders = commands.Select(p => new SlashCommandBuilder()
                .WithName(p.Key)
                .WithDescription(p.Value)
                .WithDMPermission(true)
                .WithDefaultPermission(true));
            await _client.BulkOverwriteGlobalApplicationCommandsAsync(builders.Select(b => b.Build()).ToArray());
        }

        private async Task Client_SlashCommandExecuted(SocketSlashCommand arg)
        {
            await _updateHandler.HandleCommand(this, GetChatAdapter(arg.Channel), GetUserAdapter(arg.User), arg.CommandName);
            await arg.RespondAsync("OK");
        }
    }
}
