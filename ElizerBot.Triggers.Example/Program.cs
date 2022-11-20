using ElizerBot.Adapter;
using ElizerBot.Adapter.Triggers;

namespace ElizerBot.Triggers.Example
{
    internal class Program
    {
        private const string TelegramTokenVariableName = "telegrambottoken";
        private const string DiscordTokenVariableName = "discordbottoken";

        static async Task Main(string[] args)
        {
            var telegramBotToken = Environment.GetEnvironmentVariable(TelegramTokenVariableName);
            var discordToken = Environment.GetEnvironmentVariable(DiscordTokenVariableName);

            var context = new ExampleContext();
            var buttonTriggers = new Trigger<ExampleContext, ButtonTriggerArgument>[]
            {
                new ParametrizedButtonTrigger<ExampleContext>("change", "$", async (c, a) =>
                {
                    var dataParts = a.Data.Split('$');
                    if (dataParts.Length == 3)
                        if (a.User.Id != dataParts[2])
                        {
                            Console.WriteLine("Access denied");
                            return;
                        }
                    var value = int.Parse(dataParts[1]);
                    c.Counter += value;
                    await a.Bot.SendMessage(new NewMessageAdapter(a.Message.Chat)
                    {
                        Text = $"Counter: {c.Counter}"
                    });
                    a.Message.Buttons = null;
                    await a.Bot.EditMessage(a.Message);
                }),
                new ButtonTrigger<ExampleContext>("reset", (c, a) =>
                {
                    c.Counter = 0;
                    Console.WriteLine("Silent reset");
                    return Task.CompletedTask;
                })
            };
            var commandTriggers = new[]
            {
                new CommandTrigger<ExampleContext>("changecounteranyone", async (c, a) =>
                {
                    await a.Bot.SendMessage(new NewMessageAdapter(a.Chat)
                    {
                        Text = "Change counter: for anyone",
                        Buttons = new[]
                        {
                            new[]
                            {
                                new ButtonAdapter { Label = "+1", Data = "change$1" },
                                new ButtonAdapter { Label = "-1", Data = "change$-1" }
                            }
                        }
                    });
                }),
                new CommandTrigger<ExampleContext>("changecounterforme", async (c, a) =>
                {
                    await a.Bot.SendMessage(new NewMessageAdapter(a.Chat)
                    {
                        Text = "Change counter: for me only",
                        Buttons = new[]
                        {
                            new[]
                            {
                                new ButtonAdapter { Label = "+10", Data = $"change$10${a.User.Id}" },
                                new ButtonAdapter { Label = "-10", Data = $"change$-10${a.User.Id}" }
                            }
                        }
                    });
                }),
                new CommandTrigger<ExampleContext>("reset", async (c, a) =>
                {
                    await a.Bot.SendMessage(new NewMessageAdapter(a.Chat)
                    {
                        Text = "Reset to 0. Are you sure?",
                        Buttons = new[]
                        {
                            new[]
                            {
                                new ButtonAdapter { Label = "Reset", Data = "reset" }
                            }
                        }
                    });
                })
            };
            var updateHandler = new TriggerBasedBotUpdateHandler<ExampleContext>(context, buttonTriggers, commandTriggers, null);
            var bots = new Dictionary<SupportedMessenger, string>
            {
                { SupportedMessenger.Telegram, telegramBotToken },
                { SupportedMessenger.Discord, discordToken }
            }.Select(e => updateHandler.BuildAdapter(e.Key, e.Value)).ToArray();
            Console.WriteLine("Bots starting");
            foreach (var bot in bots)
            {
                await bot.Init();
                await bot.SetCommands(new Dictionary<string, string>
                {
                    { "changecounteranyone", "Shows buttons to change counter. Anyone can press" },
                    { "changecounterforme", "Shows buttons to change counter. Only I can press" },
                    { "reset", "Resets counter to 0" },
                });
            }
            Console.WriteLine("Bots ready");
            await Task.Delay(-1);
        }
    }
}