namespace ElizerBot.Adapter
{
    public class NewMessageAdapter
    {
        public ChatAdapter Chat { get; }
        public string? Text { get; set; }
        public IReadOnlyList<IReadOnlyList<ButtonAdapter>>? Buttons { get; set; }
        public FileDescriptorAdapter? Attachment { get; set; }

        public NewMessageAdapter(ChatAdapter chat)
        {
            Chat = chat;
        }
    }
}
