namespace ElizerBot.Adapter.Triggers
{
    public class ButtonTrigger : Trigger<ButtonTriggerArgument>
    {
        private readonly string _triggeringData;

        public ButtonTrigger(string triggeringData, Func<ButtonTriggerArgument, Task> action)
            : base(action)
        {
            _triggeringData = triggeringData;
        }

        public override bool Validate(ButtonTriggerArgument arg)
        {
            return string.Equals(_triggeringData, arg.Data);
        }
    }

    public class ButtonTrigger<TContext> : Trigger<TContext, ButtonTriggerArgument>
    {
        private readonly string _triggeringData;

        public ButtonTrigger(string triggeringData, Func<TContext, ButtonTriggerArgument, Task> action) 
            : base(action)
        {
            _triggeringData = triggeringData;
        }

        public override bool Validate(ButtonTriggerArgument arg)
        {
            return string.Equals(_triggeringData, arg.Data);
        }
    }
}
