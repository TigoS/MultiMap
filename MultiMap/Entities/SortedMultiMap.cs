using MultiMap.Interfaces;
using System.Collections;

namespace MultiMap.Entities
{
    public class SortedMultiMap<TKey, TValue> : IMultiMap<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        private SortedDictionary<TKey, SortedSet<TValue>> _dictionary;

        public SortedMultiMap()
        {
            _dictionary = new SortedDictionary<TKey, SortedSet<TValue>>();
        }

        public bool Add(TKey key, TValue value)
        {
            if (!_dictionary.TryGetValue(key, out var set))
            {
                set = new SortedSet<TValue>();
                _dictionary[key] = set;
            }

            return set.Add(value);
        }

        public void AddRange(TKey key, IEnumerable<TValue> values)
        {
            if (!_dictionary.TryGetValue(key, out var set))
            {
                set = new SortedSet<TValue>();
                _dictionary[key] = set;
            }

            foreach (var value in values)
                set.Add(value);
        }

        public IEnumerable<TValue> Get(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var set))
                return set;

            return Enumerable.Empty<TValue>();
        }

        // Remove a specific value under a key
        public bool Remove(TKey key, TValue value)
        {
            if (_dictionary.TryGetValue(key, out var set))
            {
                bool removed = set.Remove(value);

                if (set.Count == 0)
                    _dictionary.Remove(key);

                return removed;
            }

            return false;
        }

        public bool RemoveKey(TKey key)
        {
            return _dictionary.Remove(key);
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        // Check if key contains a specific value
        public bool Contains(TKey key, TValue value)
        {
            return _dictionary.TryGetValue(key, out var set) && set.Contains(value);
        }

        // Get total number of key-value pairs
        public int Count => _dictionary.Sum(kvp => kvp.Value.Count);

        public void Clear() => _dictionary.Clear();

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
            return obj is SortedMultiMap<TKey, TValue> map &&
                   EqualityComparer<SortedDictionary<TKey, SortedSet<TValue>>>.Default.Equals(_dictionary, map._dictionary);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_dictionary);
        }
    }
}
