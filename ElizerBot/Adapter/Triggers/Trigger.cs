namespace ElizerBot.Adapter.Triggers
{
    public abstract class TriggerBase<TArg>
        where TArg : TriggerArgument
    {
        public abstract Task<bool> Validate(TArg arg);
    }

    public abstract class Trigger<TArg> : TriggerBase<TArg>
        where TArg : TriggerArgument
    { 
        private readonly Func<TArg, Task> _action;

        public Trigger(Func<TArg, Task> action)
        {
            _action = action;
        }

        public Task Execute(TArg argument)
        {
            return _action(argument);
        }
    }

    public abstract class Trigger<TContext, TArg> : TriggerBase<TArg>
        where TArg : TriggerArgument
    {
        private readonly Func<TContext, TArg, Task> _action;

        public Trigger(Func<TContext, TArg, Task> action)
        {
            _action = action;
        }

        public Task Execute(TContext context, TArg argument)
        {
            return _action(context, argument);
        }
    }
}
