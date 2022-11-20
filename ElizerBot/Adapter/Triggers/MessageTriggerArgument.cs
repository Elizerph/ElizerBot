namespace ElizerBot.Adapter.Triggers
{
    public class MessageTriggerArgument : TriggerArgument
    {
        public PostedMessageAdapter Message { get; }

        public MessageTriggerArgument(IBotAdapter bot, PostedMessageAdapter message) 
            : base(bot)
        {
            Message = message;
        }
    }
}
