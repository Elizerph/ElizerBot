using ElizerBot.Adapter;

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

        public async Task HandleIncomingMessage(IBotAdapter bot, PostedMessageAdapter message)
        {
            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] {message.Chat.Id}.{message.User.Username}: {message.Text}");
        }
    }
}
