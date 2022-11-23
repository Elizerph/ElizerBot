namespace ElizerBot.Adapter.Triggers
{
    public class TriggerBasedBotUpdateHandler : IBotAdapterUpdateHandler
    {
        private readonly IReadOnlyCollection<Trigger<ButtonTriggerArgument>> _buttonTriggers;
        private readonly IReadOnlyCollection<Trigger<CommandTriggerArgument>> _commandsTriggers;
        private readonly IReadOnlyCollection<Trigger<MessageTriggerArgument>> _messagesTriggers;

        public TriggerBasedBotUpdateHandler(
            IReadOnlyCollection<Trigger<ButtonTriggerArgument>>? buttonTriggers, 
            IReadOnlyCollection<Trigger<CommandTriggerArgument>>? commandsTriggers,
            IReadOnlyCollection<Trigger<MessageTriggerArgument>>? messagesTriggers)
        {
            _buttonTriggers = buttonTriggers ?? Array.Empty<Trigger<ButtonTriggerArgument>>();
            _commandsTriggers = commandsTriggers ?? Array.Empty<Trigger<CommandTriggerArgument>>();
            _messagesTriggers = messagesTriggers ?? Array.Empty<Trigger<MessageTriggerArgument>>();
        }

        private static async Task Handle<T>(IEnumerable<Trigger<T>> triggers, T argument)
            where T : TriggerArgument
        {
            foreach (var trigger in triggers)
                if (await trigger.Validate(argument))
                    await trigger.Execute(argument);
        }

        public Task HandleButtonPress(IBotAdapter bot, PostedMessageAdapter message, UserAdapter user, string buttonData)
        {
            var argument = new ButtonTriggerArgument(bot, message, user, buttonData);
            return Handle(_buttonTriggers, argument);
        }

        public Task HandleCommand(IBotAdapter bot, ChatAdapter sourceChat, UserAdapter sourceUser, string command)
        {
            var argument = new CommandTriggerArgument(bot, sourceChat, sourceUser, command);
            return Handle(_commandsTriggers, argument);
        }

        public Task HandleIncomingMessage(IBotAdapter bot, PostedMessageAdapter message)
        {
            var argument = new MessageTriggerArgument(bot, message);
            return Handle(_messagesTriggers, argument);
        }
    }

    public class TriggerBasedBotUpdateHandler<TContext> : IBotAdapterUpdateHandler
    {
        private readonly TContext _context;
        private readonly IReadOnlyCollection<Trigger<TContext, ButtonTriggerArgument>> _buttonTriggers;
        private readonly IReadOnlyCollection<Trigger<TContext, CommandTriggerArgument>> _commandTriggers;
        private readonly IReadOnlyCollection<Trigger<TContext, MessageTriggerArgument>> _messageTriggers;

        public TriggerBasedBotUpdateHandler(TContext context, 
            IReadOnlyCollection<Trigger<TContext, ButtonTriggerArgument>>? buttonTriggers,
            IReadOnlyCollection<Trigger<TContext, CommandTriggerArgument>>? commandtriggers,
            IReadOnlyCollection<Trigger<TContext, MessageTriggerArgument>>? messageTriggers)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _buttonTriggers = buttonTriggers ?? Array.Empty<Trigger<TContext, ButtonTriggerArgument>>();
            _commandTriggers = commandtriggers ?? Array.Empty<Trigger<TContext, CommandTriggerArgument>>();
            _messageTriggers = messageTriggers ?? Array.Empty<Trigger<TContext, MessageTriggerArgument>>();
        }

        private async Task Handle<T>(IEnumerable<Trigger<TContext, T>> triggers, T arg)
            where T : TriggerArgument
        {
            foreach (var trigger in triggers)
                if (await trigger.Validate(arg))
                    await trigger.Execute(_context, arg);
        }

        public async Task HandleButtonPress(IBotAdapter bot, PostedMessageAdapter message, UserAdapter user, string buttonData)
        {
            var argument = new ButtonTriggerArgument(bot, message, user, buttonData);
            await Handle(_buttonTriggers, argument);
        }

        public async Task HandleCommand(IBotAdapter bot, ChatAdapter sourceChat, UserAdapter sourceUser, string command)
        {
            var argument = new CommandTriggerArgument(bot, sourceChat, sourceUser, command);
            await Handle(_commandTriggers, argument);
        }

        public async Task HandleIncomingMessage(IBotAdapter bot, PostedMessageAdapter message)
        {
            var argument = new MessageTriggerArgument(bot, message);
            await Handle(_messageTriggers, argument);
        }
    }
}
