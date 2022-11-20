namespace ElizerBot.Adapter.Triggers
{
    public class CommandTriggerArgument : TriggerArgument
    {
        public ChatAdapter Chat { get; }
        public UserAdapter User { get; }
        public string Command { get; }
        public CommandTriggerArgument(IBotAdapter bot, ChatAdapter chat, UserAdapter user, string command) 
            : base(bot)
        {
            Chat = chat;
            User = user;
            Command = command;
        }
    }
}
