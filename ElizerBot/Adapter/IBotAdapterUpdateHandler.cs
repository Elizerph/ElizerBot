namespace ElizerBot.Adapter
{
    public interface IBotAdapterUpdateHandler
    {
        Task HandleIncomingMessage(IBotAdapter bot, PostedMessageAdapter message);
        Task HandleButtonPress(IBotAdapter bot, PostedMessageAdapter message, UserAdapter user, string buttonData);
        Task HandleCommand(IBotAdapter bot, ChatAdapter sourceChat, UserAdapter sourceUser, string command);
    }
}
