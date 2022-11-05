using System.Diagnostics.CodeAnalysis;

namespace ElizerBot.Telegram
{
    internal class TelegramCommandComparer : IEqualityComparer<string>
    {
        private readonly string _botUsername;

        public TelegramCommandComparer(string botUsername)
        {
            _botUsername = botUsername;
        }

        private string? EscapeBotUsername(string? value)
        {
            return value?.Replace(_botUsername, string.Empty);
        }

        public bool Equals(string? x, string? y)
        {
            return string.Equals(EscapeBotUsername(x), EscapeBotUsername(y));
        }

        public int GetHashCode([DisallowNull] string obj)
        {
            return EscapeBotUsername(obj).GetHashCode();
        }
    }
}
