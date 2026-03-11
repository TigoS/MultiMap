using System.Collections;

namespace MultiMap
{
    public class MultiMapSet<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : notnull
        where TValue : notnull
    {
        private readonly Dictionary<TKey, HashSet<TValue>> _dictionary;

        public MultiMapSet()
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>();
        }

        public bool Add(TKey key, TValue value)
        {
            if (!_dictionary.TryGetValue(key, out var hashset))
            {
                hashset = new HashSet<TValue>();
                _dictionary[key] = hashset;
            }

            return hashset.Add(value); // Returns false if duplicate
        }

        public void AddRange(TKey key, IEnumerable<TValue> values)
        {
            if (!_dictionary.TryGetValue(key, out var hashset))
            {
                hashset = new HashSet<TValue>();
                _dictionary[key] = hashset;
            }

            foreach (var value in values)
            {
                hashset.Add(value); // Ignores duplicates
            }
        }

        // Get all values for a key
        public IEnumerable<TValue> Get(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
                return hashset;

            return Enumerable.Empty<TValue>();
        }

        // Remove a specific value under a key
        public bool Remove(TKey key, TValue value)
        {
            if (_dictionary.TryGetValue(key, out var list))
            {
                bool removed = list.Remove(value);

                if (list.Count == 0)
                    _dictionary.Remove(key);

                return removed;
            }

            return false;
        }

        // Remove all values for a key
        public bool RemoveKey(TKey key)
        {
            return _dictionary.Remove(key);
        }

        // Check if key exists
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        // Check if key contains a specific value
        public bool Contains(TKey key, TValue value)
        {
            return _dictionary.TryGetValue(key, out var hashset) && hashset.Contains(value);
        }

        // Get total number of key-value pairs
        public int Count => _dictionary.Sum(kvp => kvp.Value.Count);

        public void Clear() => _dictionary.Clear();

        // Enumerator to iterate through all key-value pairs
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var kvp in _dictionary)
            {
                foreach (var value in kvp.Value)
                {
                    yield return new KeyValuePair<TKey, TValue>(kvp.Key, value);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override bool Equals(object? obj)
        {
            return obj is MultiMapSet<TKey, TValue> map &&
                   EqualityComparer<Dictionary<TKey, HashSet<TValue>>>.Default.Equals(_dictionary, map._dictionary);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_dictionary);
        }
    }
}
