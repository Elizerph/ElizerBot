using ElizerBot.Adapter;
using ElizerBot.Discord;
using ElizerBot.Telegram;

namespace ElizerBot
{
    public static class BotAdapterUpdateHandlerExtension
    {
        public static BotAdapter BuildAdapter(this IBotAdapterUpdateHandler updateHandler,
            SupportedMessenger messenger,
            string token)
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
