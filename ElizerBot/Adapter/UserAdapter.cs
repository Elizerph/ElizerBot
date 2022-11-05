namespace ElizerBot.Adapter
{
    public class UserAdapter
    {
        public string Id { get; }
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public UserAdapter(string id)
        {
            Id = id;
        }
    }
}
