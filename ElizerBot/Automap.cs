namespace ElizerBot
{
    public class Automap<TKey, TValue>
        where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _dictionary;
        private readonly Func<TKey, TValue> _generator;

        public TValue this[TKey key]
        {
            get
            {
                if (!_dictionary.TryGetValue(key, out var result))
                {
                    result = _generator(key);
                    _dictionary[key] = result;
                }
                return result;
            }
        }

        public Automap(Func<TKey, TValue> generator, IEqualityComparer<TKey>? keyComparer = null)
        {
            _generator = generator;
            _dictionary = new Dictionary<TKey, TValue>(keyComparer);
        }
    }
}
