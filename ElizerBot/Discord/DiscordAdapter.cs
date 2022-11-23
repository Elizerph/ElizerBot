using Discord;
using Discord.WebSocket;

using ElizerBot.Adapter;

namespace ElizerBot.Discord
{
    internal class DiscordAdapter : BotAdapter
    {
        private readonly static HttpClient _httpClient = new();
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
                    var message = new PostedMessageAdapter(GetChatAdapter(arg.Channel), arg.Message.Id.ToString(), GetUserAdapter(arg.Message.Author))
                    {
                        Text = arg.Message.Content
                    };
                    return _updateHandler.HandleButtonPress(this, message, GetUserAdapter(arg.User), arg.Data.CustomId);
                default:
                    return Task.CompletedTask;
            }
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg.Author.IsBot)
                return;

            if (arg.Channel is not IPrivateChannel)
                if (arg.MentionedUsers.Count != 1 || arg.MentionedUsers.First().Id != _client.CurrentUser.Id)
                    return;

            var message = new PostedMessageAdapter(GetChatAdapter(arg.Channel), arg.Id.ToString(), GetUserAdapter(arg.Author))
            {
                Text = arg.CleanContent?.Replace($"@{_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator}", string.Empty).Trim() ?? string.Empty,
                Buttons = GetButtonAdapters(arg.Components),
                Attachments = arg.Attachments?.Select(e => new FileDescriptorAdapter(e.Filename, () => _httpClient.GetStreamAsync(e.Url))).ToArray()
            };
            await _updateHandler.HandleIncomingMessage(this, message); 
        }

        public override async Task<PostedMessageAdapter> SendMessage(NewMessageAdapter message)
        {
            var channel = await _client.GetChannelAsync(ulong.Parse(message.Chat.Id));
            if (channel is IMessageChannel messageChannel)
            {
                var buttonsComponent = GetMessageButtons(message.Buttons);
                IUserMessage feedback;
                if (message.Attachments == null || message.Attachments.Count == 0)
                    feedback = await messageChannel.SendMessageAsync(message.Text, components: buttonsComponent);
                else
                {
                    var attachments = await GetFileAttachments(message.Attachments).ToListAsync();
                    feedback = await messageChannel.SendFilesAsync(attachments, message.Text, components: buttonsComponent);
                }
                return new PostedMessageAdapter(message.Chat, feedback.Id.ToString(), GetUserAdapter(feedback.Author))
                {
                    Text = feedback.Content,
                    Buttons = GetButtonAdapters(feedback.Components.OfType<ActionRowComponent>().ToArray())
                };
            }
            throw new InvalidOperationException($"{nameof(messageChannel)} is not {typeof(IMessageChannel)}");
        }

        private static async IAsyncEnumerable<FileAttachment> GetFileAttachments(IReadOnlyCollection<FileDescriptorAdapter> adapters)
        {
            foreach (var adapter in adapters)
            {
                var stream = await adapter.ReadFile();
                yield return new FileAttachment(stream, adapter.FileName);
            }
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

        private static ChatAdapter GetChatAdapter(ISocketMessageChannel channel)
        { 
            return new ChatAdapter(channel.Id.ToString(), channel is IPrivateChannel)
            { 
                Title = channel.Name
            };
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
                return new PostedMessageAdapter(message.Chat, feedback.Id.ToString(), GetUserAdapter(feedback.Author))
                {
                    Text = feedback.Content,
                    Buttons = GetButtonAdapters(feedback.Components.OfType<ActionRowComponent>().ToArray())
                };
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
