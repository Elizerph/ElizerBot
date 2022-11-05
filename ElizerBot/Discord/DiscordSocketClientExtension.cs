using Discord.WebSocket;

namespace ElizerBot.Discord
{
    internal static class DiscordSocketClientExtension
    {
        public static Task ReadyAsync(this DiscordSocketClient client)
        {
            var awaiter = new DiscordReadyAwaiter(client);
            return awaiter.Execute();
        }
    }
}
