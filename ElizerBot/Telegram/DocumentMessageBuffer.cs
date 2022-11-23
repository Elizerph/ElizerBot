using ElizerBot.Adapter;

namespace ElizerBot.Telegram
{
    internal class DocumentMessageBuffer
    {
        private readonly IBotAdapterUpdateHandler _updateHandler;
        private readonly TimeSpan _timeout;
        private readonly Dictionary<string, Group> _groups = new();

        private class Group
        { 
            public CancellationTokenSource Cts { get; set; }
            public List<PostedMessageAdapter> Messages { get; set; }
        }

        public DocumentMessageBuffer(IBotAdapterUpdateHandler updateHandler, TimeSpan delay)
        {
            _updateHandler = updateHandler;
            _timeout = delay;
        }

        internal async Task HandleIncomingMessage(IBotAdapter bot, string? mediaGroup, PostedMessageAdapter message)
        {
            if (mediaGroup == null)
            {
                await _updateHandler.HandleIncomingMessage(bot, message);
                return;
            }
            else
            {
                if (_groups.TryGetValue(mediaGroup, out var group))
                    group.Cts.Cancel();
                else
                {
                    group = new Group()
                    { 
                        Messages = new List<PostedMessageAdapter>()
                    };
                    _groups[mediaGroup] = group;
                }

                var cts = new CancellationTokenSource();
                group.Cts = cts;
                group.Messages.Add(message);
                Task.Run(async () =>
                {
                    await Task.Delay(_timeout, cts.Token);
                    cts.Token.ThrowIfCancellationRequested();
                    _groups.Remove(mediaGroup);
                    var firstMessage = group.Messages.First();
                    var resultMessage = new PostedMessageAdapter(firstMessage.Chat, firstMessage.Id, firstMessage.User)
                    {
                        Buttons = firstMessage.Buttons,
                        Attachments = group.Messages.SelectMany(e => e.Attachments).ToArray()
                    };
                    await _updateHandler.HandleIncomingMessage(bot, resultMessage);
                });
            }
        }
    }
}
