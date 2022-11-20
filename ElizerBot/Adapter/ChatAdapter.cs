namespace ElizerBot.Adapter
{
    public class ChatAdapter
    {
        public string Id { get; }
        public bool IsPrivate { get; }
        public string? Title { get; set; }
        public ChatAdapter(string id, bool isPrivate)
        {
            Id = id;
            IsPrivate = isPrivate;
        }
    }
}
