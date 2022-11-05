namespace ElizerBot.Adapter
{
    public class PostedMessageAdapter : NewMessageAdapter
    {
        public string Id { get; }
        public UserAdapter User { get; }

        public PostedMessageAdapter(ChatAdapter chat, string id, UserAdapter user)
            : base(chat)
        {
            Id = id;
            User = user;
        }
    }
}
