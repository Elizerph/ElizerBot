using ElizerBot.Adapter;

using Telegram.Bot.Types;

namespace ElizerBot.Example
{
    public class ExampleUpdateHandler : IBotAdapterUpdateHandler
    {
        public async Task HandleButtonPress(IBotAdapter bot, PostedMessageAdapter message, UserAdapter user, string buttonData)
        {
            var pressedButton = message.Buttons.SelectMany(e => e).First(e => string.Equals(buttonData, e.Data));
            var response = new NewMessageAdapter(message.Chat)
            {
                Text = $"Buttons removed. This button was pressed: {pressedButton.Label}"
            };
            await bot.SendMessage(response);
            message.Buttons = null;
            await bot.EditMessage(message);
        }

        public async Task HandleCommand(IBotAdapter bot, ChatAdapter sourceChat, UserAdapter sourceUser, string command)
        {
            if (string.Equals(command, "showbuttons"))
            {
                var response = new NewMessageAdapter(sourceChat)
                {
                    Text = "Buttons menu",
                    Buttons = new[]
                    {
                        new[]
                        {
                            new ButtonAdapter
                            {
                                Data = "0",
                                Label = "Zero"
                            },
                            new ButtonAdapter
                            {
                                Data = "1",
                                Label = "One"
                            }
                        },
                        new[]
                        {
                            new ButtonAdapter
                            {
                                Data = "2",
                                Label = "Two"
                            },
                            new ButtonAdapter
                            {
                                Data = "3",
                                Label = "Three"
                            }
                        }
                    }
                };
                await bot.SendMessage(response);
            }
        }

        private static async Task<Stream> GetTextStream(string text)
        {
            var memo = new MemoryStream();
            var writer = new StreamWriter(memo);
            await writer.WriteLineAsync(text);
            await writer.FlushAsync();
            return memo;
        }

        public async Task HandleIncomingMessage(IBotAdapter bot, PostedMessageAdapter message)
        {
            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] {message.Chat.Id}.{message.User.Username}: {message.Text}");

            var fileResponse = new NewMessageAdapter(message.Chat)
            {
                Text = "File response",
                Attachments = new[]
                {
                    new FileDescriptorAdapter("response1.txt", () => GetTextStream(message.Text)),
                    new FileDescriptorAdapter("response2.txt", () => GetTextStream(message.Text))
                }
            };
            await bot.SendMessage(fileResponse);
            if (message.Attachments != null && message.Attachments.Any())
            {
                var filesInfo = new List<string>();
                foreach (var attachment in message.Attachments)
                {
                    using var stream = await attachment.ReadFile();
                    using var reader = new StreamReader(stream);
                    var fileHead = await reader.ReadToEndAsync();
                    filesInfo.Add(new string(fileHead.Take(40).ToArray()));
                }
                var filesInfoResponse = new NewMessageAdapter(message.Chat)
                {
                    Text = string.Join(Environment.NewLine, filesInfo)
                };
                await bot.SendMessage(filesInfoResponse);
            }
        }
    }
}
