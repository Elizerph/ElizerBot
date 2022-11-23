namespace ElizerBot.Adapter.Triggers
{
    public class CommandTrigger : Trigger<CommandTriggerArgument>
    {
        private readonly string _triggeringCommand;

        public CommandTrigger(string triggeringCommand, Func<CommandTriggerArgument, Task> action)
            : base(action)
        {
            _triggeringCommand = triggeringCommand;
        }

        public override Task<bool> Validate(CommandTriggerArgument arg)
        {
            return Task.FromResult(string.Equals(_triggeringCommand, arg.Command));
        }
    }

    public class CommandTrigger<TContext> : Trigger<TContext, CommandTriggerArgument>
    {
        private readonly string _triggeringCommand;

        public CommandTrigger(string triggeringCommand, Func<TContext, CommandTriggerArgument, Task> action)
            : base(action)
        {
            _triggeringCommand = triggeringCommand;
        }

        public override Task<bool> Validate(CommandTriggerArgument arg)
        {
            return Task.FromResult(string.Equals(_triggeringCommand, arg.Command));
        }
    }
}
