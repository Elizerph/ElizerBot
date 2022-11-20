namespace ElizerBot.Adapter.Triggers
{
    public class ButtonTriggerArgument : MessageTriggerArgument
    {
        public string Data { get; }
        public UserAdapter User { get; }

        public ButtonTriggerArgument(IBotAdapter bot, PostedMessageAdapter message, UserAdapter user, string data) 
            : base(bot, message)
        {
            Data = data;
            User = user;
        }
    }
}
