namespace ElizerBot.Adapter.Triggers
{
    public class TriggerArgument
    {
        public IBotAdapter Bot { get; }

        public TriggerArgument(IBotAdapter bot)
        {
            Bot = bot;
        }
    }
}
