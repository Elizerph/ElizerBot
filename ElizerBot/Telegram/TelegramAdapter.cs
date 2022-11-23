using ElizerBot.Adapter;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ElizerBot.Telegram
{
    internal class TelegramAdapter : BotAdapter
    {
        private readonly TelegramBotClient _client;
        private string? _botUsername;
        private DocumentMessageBuffer _documentBuffer;

        public TelegramAdapter(string token, IBotAdapterUpdateHandler updateHandler)
            : base(token, updateHandler)
        {
            _client = new TelegramBotClient(token);
            _documentBuffer = new DocumentMessageBuffer(updateHandler, TimeSpan.FromSeconds(2));
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
                        var messageText = message.Text ?? string.Empty;

                        var messageAuthor = message.From ?? throw new InvalidOperationException($"{nameof(message.From)} is null");

                        var entities = message.Entities;
                        if (entities != null
                            && entities.Length == 1
                            && entities[0].Type == MessageEntityType.BotCommand
                            && (messageText.Contains($"@{_botUsername}") || message.Chat.Type == ChatType.Private))
                            await _updateHandler.HandleCommand(this, GetChatAdapter(message.Chat), GetUserAdapter(messageAuthor), messageText.Replace($"@{_botUsername}", string.Empty).TrimStart('/'));
                        else
                        {
                            if (message.Chat.Type != ChatType.Private)
                                if (entities == null
                                || entities.Length != 1
                                || entities[0].Type != MessageEntityType.Mention
                                || !string.Equals(message.Text.Substring(entities[0].Offset, entities[0].Length), $"@{_botUsername}"))
                                    return;
                            await _documentBuffer.HandleIncomingMessage(this, message.MediaGroupId, GetIncomingMessageAdapter(message));
                        }
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

        private PostedMessageAdapter GetIncomingMessageAdapter(Message message)
        {
            var result = new PostedMessageAdapter(GetChatAdapter(message.Chat), message.MessageId.ToString(), GetUserAdapter(message.From))
            {
                Text = message.Text?.Replace($"@{_botUsername}", string.Empty) ?? string.Empty,
                Buttons = GetButtonAdapters(message.ReplyMarkup)
            };
            if (message.Document != null)
            {
                result.Attachments = new[]
                {
                    new FileDescriptorAdapter(message.Document.FileName, async () =>
                    {
                        var stream = new MemoryStream();
                        await _client.GetInfoAndDownloadFileAsync(message.Document.FileId, stream);
                        stream.Position = 0;
                        return stream;
                    })
                };
            }
            return result;
        }

        public override async Task<PostedMessageAdapter> SendMessage(NewMessageAdapter message)
        {
            var regularFeedback = await _client.SendTextMessageAsync(message.Chat.Id, message.Text, replyMarkup: GetMarkup(message.Buttons));
            var result = new PostedMessageAdapter(GetChatAdapter(regularFeedback.Chat), regularFeedback.MessageId.ToString(), GetUserAdapter(regularFeedback.From))
            {
                Text = message.Text,
                Buttons = GetButtonAdapters(regularFeedback.ReplyMarkup)
            };

            if (message.Attachments != null && message.Attachments.Any())
            {
                var documents = await GetDocuments(message.Attachments).ToListAsync();
                var fileFeedbacks = await _client.SendMediaGroupAsync(message.Chat.Id, documents);
                var resultAttachments = fileFeedbacks.Select(e => new FileDescriptorAdapter(e.Document.FileName, async () => 
                {
                    var stream = new MemoryStream();
                    await _client.GetInfoAndDownloadFileAsync(e.Document.FileId, stream);
                    stream.Position = 0;
                    return stream;
                })).ToArray();
                result.Attachments = resultAttachments;
            }
            return result;
        }

        private static async IAsyncEnumerable<IAlbumInputMedia> GetDocuments(IReadOnlyCollection<FileDescriptorAdapter> adapters)
        {
            foreach (var adapter in adapters)
            {
                var stream = await adapter.ReadFile();
                stream.Position = 0;
                yield return new InputMediaDocument(new InputMedia(stream, adapter.FileName));
            }
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
