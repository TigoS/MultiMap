using System.Collections;
using System.Collections.Concurrent;

namespace MultiMap
{
    public class ConcurrentMultiMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : notnull
        where TValue : notnull
    {
        private readonly ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>> _dictionary;

        public ConcurrentMultiMap()
        {
            _dictionary = new ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>();
        }

        // Add value (returns false if duplicate)
        public bool Add(TKey key, TValue value)
        {
            var concurrentSet = _dictionary.GetOrAdd(
                key,
                _ => new ConcurrentDictionary<TValue, byte>());

            return concurrentSet.TryAdd(value, 0);
        }

        public void AddRange(TKey key, IEnumerable<TValue> values)
        {
            var concurrentSet = _dictionary.GetOrAdd(
                key,
                _ => new ConcurrentDictionary<TValue, byte>());

            foreach (var value in values)
                concurrentSet.TryAdd(value, 0);
        }

        // Get values for a key
        public IEnumerable<TValue> Get(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var concurrentSet))
                return concurrentSet.Keys;

            return Enumerable.Empty<TValue>();
        }

        // Remove specific value
        public bool Remove(TKey key, TValue value)
        {
            if (_dictionary.TryGetValue(key, out var concurrentSet))
            {
                bool removed = concurrentSet.TryRemove(value, out _);

                if (removed && concurrentSet.IsEmpty)
                    _dictionary.TryRemove(key, out _);

                return removed;
            }

            return false;
        }

        // Remove entire key
        public bool RemoveKey(TKey key)
        {
            return _dictionary.TryRemove(key, out _);
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool Contains(TKey key, TValue value)
        {
            return _dictionary.TryGetValue(key, out var concurrentSet) && concurrentSet.ContainsKey(value);
        }

        public int Count =>
            _dictionary.Sum(kvp => kvp.Value.Count);

        // Enumerator
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var kvp in _dictionary)
            {
                foreach (var value in kvp.Value.Keys)
                {
                    yield return new KeyValuePair<TKey, TValue>(kvp.Key, value);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override bool Equals(object? obj)
        {
            return obj is ConcurrentMultiMap<TKey, TValue> map &&
                   EqualityComparer<ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>>.Default.Equals(_dictionary, map._dictionary);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_dictionary);
        }
    }
}
