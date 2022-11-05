namespace ElizerBot.Example
{
    internal class Program
    {
        private const string TelegramTokenVariableName = "telegrambottoken";
        private const string DiscordTokenVariableName = "discordbottoken";

        static async Task Main(string[] args)
        {
            var telegramBotToken = Environment.GetEnvironmentVariable(TelegramTokenVariableName);
            var discordToken = Environment.GetEnvironmentVariable(DiscordTokenVariableName);

            var updateHandler = new ExampleUpdateHandler();
            var bots = new[]
            { 
                BotFactory.Create(SupportedMessenger.Telegram, telegramBotToken, updateHandler),
                BotFactory.Create(SupportedMessenger.Discord, discordToken, updateHandler)
            };
            Console.WriteLine("Bot starting…");
            foreach (var bot in bots)
            {
                await bot.Init();
                await bot.SetCommands(new Dictionary<string, string> 
                {
                    { "showbuttons", "Shows buttons example" }
                });
            }
            Console.WriteLine("Bots ready");
            await Task.Delay(-1);
        }
    }
}