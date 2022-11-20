namespace ElizerBot.Adapter.Triggers
{
    public abstract class MessageTrigger : Trigger<MessageTriggerArgument>
    {
        public MessageTrigger(Func<MessageTriggerArgument, Task> action)
            : base(action)
        {
        }
    }
    public abstract class MessageTrigger<TContext> : Trigger<TContext, MessageTriggerArgument>
    {
        public MessageTrigger(Func<TContext, MessageTriggerArgument, Task> action)
            : base(action)
        {
        }
    }
}
