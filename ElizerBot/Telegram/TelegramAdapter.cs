using ElizerBot.Adapter;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace ElizerBot.Telegram
{
    internal class TelegramAdapter : BotAdapter
    {
        private readonly TelegramBotClient _client;
        private string? _botUsername;

        public TelegramAdapter(string token, IBotAdapterUpdateHandler updateHandler)
            : base(token, updateHandler)
        {
            _client = new TelegramBotClient(token);
        }

        public override async Task Init()
        {
            var updateHandler = new TelegramUpdateHandler(
            (c, e) =>
            {
                Console.WriteLine(string.Join(Environment.NewLine, new[] { e.GetType().ToString(), e.Message, e.StackTrace }));
                return Task.CompletedTask;
            },
            async (c, u) =>
            {
                switch (u.Type)
                {
                    case UpdateType.Message:
                        var message = u.Message ?? throw new InvalidOperationException($"{nameof(u.Message)} is null");
                        var messageText = message.Text;
                        if (string.IsNullOrEmpty(messageText))
                            throw new InvalidOperationException($"{nameof(messageText)} is null");

                        var messageAuthor = message.From ?? throw new InvalidOperationException($"{nameof(message.From)} is null");

                        var entities = message.Entities;
                        if (entities != null 
                            && entities.Length == 1 
                            && entities[0].Type == MessageEntityType.BotCommand 
                            && (messageText.Contains($"@{_botUsername}") || message.Chat.Type == ChatType.Private))
                            await _updateHandler.HandleCommand(this, GetChatAdapter(message.Chat), GetUserAdapter(messageAuthor), messageText.Replace($"@{_botUsername}", string.Empty).TrimStart('/'));
                        else
                            await _updateHandler.HandleIncomingMessage(this, GetIncomingMessageAdapter(message));
                        break;
                    case UpdateType.CallbackQuery:
                        var queryMessage = u.CallbackQuery?.Message ?? throw new InvalidOperationException();
                        await _updateHandler.HandleButtonPress(this, GetIncomingMessageAdapter(queryMessage), GetUserAdapter(u.CallbackQuery.From), u.CallbackQuery.Data);
                        break;
                    default:
                        break;
                }
            });
            var botMe = await _client.GetMeAsync();
            _botUsername = botMe.Username;
            _client.StartReceiving(updateHandler);
        }

        private static ChatAdapter GetChatAdapter(Chat chat)
        {
            return new ChatAdapter(chat.Id.ToString(), chat.Type == ChatType.Private)
            {
                Title = chat.Title
            };
        }

        private static UserAdapter GetUserAdapter(User user)
        {
            return new UserAdapter(user.Id.ToString())
            {
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        private static ButtonAdapter GetButtonAdapter(InlineKeyboardButton button)
        {
            return new ButtonAdapter
            {
                Data = button.CallbackData,
                Label = button.Text
            };
        }

        private static IReadOnlyList<IReadOnlyList<ButtonAdapter>>? GetButtonAdapters(InlineKeyboardMarkup? markup)
        {
            return markup?.InlineKeyboard.Select(line => line.Select(GetButtonAdapter).ToArray()).ToArray();
        }

        private static PostedMessageAdapter GetIncomingMessageAdapter(Message message)
        {
            return new PostedMessageAdapter(GetChatAdapter(message.Chat), message.MessageId.ToString(), GetUserAdapter(message.From))
            {
                Text = message.Text,
                Buttons = GetButtonAdapters(message.ReplyMarkup)
            };
        }

        public override async Task<PostedMessageAdapter> SendMessage(NewMessageAdapter message)
        {
            Message feedback;
            if (message.Attachment == null)
                feedback = await _client.SendTextMessageAsync(message.Chat.Id, message.Text, replyMarkup: GetMarkup(message.Buttons));
            else
                feedback = await _client.SendDocumentAsync(message.Chat.Id, GetDocument(message.Attachment), caption: message.Text, replyMarkup: GetMarkup(message.Buttons));
            return GetIncomingMessageAdapter(feedback);
        }

        private static InputOnlineFile GetDocument(FileDescriptorAdapter attachment)
        {
            var stream = attachment.ReadFile();
            stream.Position = 0;
            return new InputOnlineFile(stream, attachment.FileName);
        }

        public override async Task<PostedMessageAdapter> EditMessage(PostedMessageAdapter message)
        {
            var feedbck = await _client.EditMessageTextAsync(message.Chat.Id, int.Parse(message.Id), message.Text, replyMarkup: GetMarkup(message.Buttons));
            return GetIncomingMessageAdapter(feedbck);
        }

        private static InlineKeyboardMarkup? GetMarkup(IReadOnlyList<IReadOnlyList<ButtonAdapter>>? buttonAdapters)
        {
            if (buttonAdapters == null)
                return null;

            var buttons = buttonAdapters.Select(line => line.Select(b => InlineKeyboardButton.WithCallbackData(b.Label, b.Data)).ToArray()).ToArray();
            return new InlineKeyboardMarkup(buttons);
        }

        public override Task ClearCommands()
        {
            return _client.DeleteMyCommandsAsync();
        }

        public override Task SetCommands(Dictionary<string, string> commands)
        {
            return _client.SetMyCommandsAsync(commands.Select(p => new BotCommand { Command = p.Key, Description = p.Value }));
        }
    }
}
