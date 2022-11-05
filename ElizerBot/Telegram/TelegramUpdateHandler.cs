using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace ElizerBot.Telegram
{
    internal class TelegramUpdateHandler : IUpdateHandler
    {
        private readonly Func<ITelegramBotClient, Exception, Task> _handlePollingErrorAsync;
        private readonly Func<ITelegramBotClient, Update, Task> _handleUpdateAsync;

        public TelegramUpdateHandler(Func<ITelegramBotClient, Exception, Task> handlePollingErrorAsync, Func<ITelegramBotClient, Update, Task> handleUpdateAsync)
        {
            _handlePollingErrorAsync = handlePollingErrorAsync ?? throw new ArgumentNullException(nameof(handlePollingErrorAsync));
            _handleUpdateAsync = handleUpdateAsync ?? throw new ArgumentNullException(nameof(handleUpdateAsync));
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            return _handlePollingErrorAsync(botClient, exception);
        }

        public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            return _handleUpdateAsync(botClient, update);
        }
    }
}
