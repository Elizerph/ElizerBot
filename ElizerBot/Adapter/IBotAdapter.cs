namespace ElizerBot.Adapter
{
    public interface IBotAdapter
    {
        Task<PostedMessageAdapter> SendMessage(NewMessageAdapter message);
        Task<PostedMessageAdapter> EditMessage(PostedMessageAdapter message);
    }
}
