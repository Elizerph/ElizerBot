namespace ElizerBot.Adapter.Triggers
{
    public class ParametrizedButtonTrigger : Trigger<ButtonTriggerArgument>
    {
        private readonly string _id;
        private readonly string _dataSeparator;

        public ParametrizedButtonTrigger(string id, string dataSeparator, Func<ButtonTriggerArgument, Task> action)
            : base(action)
        {
            _id = id;
            _dataSeparator = dataSeparator;
        }

        public override bool Validate(ButtonTriggerArgument arg)
        {
            var dataParts = arg.Data.Split(_dataSeparator);
            if (dataParts.Length < 1)
                return false;
            return string.Equals(_id, dataParts[0]);
        }
    }
    public class ParametrizedButtonTrigger<TContext> : Trigger<TContext, ButtonTriggerArgument>
    {
        private readonly string _id;
        private readonly string _dataSeparator;

        public ParametrizedButtonTrigger(string id, string dataSeparator, Func<TContext, ButtonTriggerArgument, Task> action)
            : base(action)
        {
            _id = id;
            _dataSeparator = dataSeparator;
        }

        public override bool Validate(ButtonTriggerArgument arg)
        {
            var dataParts = arg.Data.Split(_dataSeparator);
            if (dataParts.Length < 1)
                return false;
            return string.Equals(_id, dataParts[0]);
        }
    }
}
