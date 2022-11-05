using ElizerBot.Adapter;
using ElizerBot.Discord;
using ElizerBot.Telegram;

namespace ElizerBot
{
    public static class BotFactory
    {
        public static BotAdapter Create(SupportedMessenger messenger, string token, IBotAdapterUpdateHandler updateHandler)
        {
            return messenger switch
            {
                SupportedMessenger.Telegram => new TelegramAdapter(token, updateHandler),
                SupportedMessenger.Discord => new DiscordAdapter(token, updateHandler),
                _ => throw new ArgumentOutOfRangeException(nameof(messenger)),
            };
        }
    }
}
