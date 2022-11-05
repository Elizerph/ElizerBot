using Discord.WebSocket;

namespace ElizerBot.Discord
{
    internal class DiscordReadyAwaiter
    {
        private readonly DiscordSocketClient _client;
        private readonly TaskCompletionSource<bool> _tcs = new();

        public DiscordReadyAwaiter(DiscordSocketClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public Task Execute()
        {
            _client.Ready += Client_Ready;
            return _tcs.Task;
        }

        private Task Client_Ready()
        {
            _client.Ready -= Client_Ready;
            _tcs.SetResult(true);
            return Task.CompletedTask;
        }
    }
}
