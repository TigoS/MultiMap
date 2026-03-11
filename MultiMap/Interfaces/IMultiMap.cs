namespace MultiMap.Interfaces
{
    public  interface IMultiMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : notnull
        where TValue : notnull
    {
        // Add value (returns false if duplicate)
        public bool Add(TKey key, TValue value);

        public void AddRange(TKey key, IEnumerable<TValue> values);

        // Get all values for a key
        public IEnumerable<TValue> Get(TKey key);

        // Remove a specific value under a key
        public bool Remove(TKey key, TValue value);

        // Remove all values for a key
        public bool RemoveKey(TKey key);

        // Check if key exists
        public bool ContainsKey(TKey key);

        // Check if key contains a specific value
        public bool Contains(TKey key, TValue value);

        // Get total number of key-value pairs
        public int Count { get; }

        public void Clear();
    }
}
