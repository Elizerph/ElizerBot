namespace ElizerBot.Adapter
{
    public abstract class BotAdapter : IBotAdapter
    {
        protected readonly string _token;
        protected readonly IBotAdapterUpdateHandler _updateHandler;

        public BotAdapter(string token, IBotAdapterUpdateHandler updateHandler)
        {
            _token = token ?? throw new ArgumentNullException(nameof(token));
            _updateHandler = updateHandler ?? throw new ArgumentNullException(nameof(updateHandler));
        }

        public abstract Task Init();
        public abstract Task ClearCommands();
        public abstract Task SetCommands(Dictionary<string, string> commands);
        public abstract Task<PostedMessageAdapter> EditMessage(PostedMessageAdapter message);
        public abstract Task<PostedMessageAdapter> SendMessage(NewMessageAdapter message);
    }
}
