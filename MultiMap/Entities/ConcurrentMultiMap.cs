using MultiMap.Interfaces;
using System.Collections;
using System.Collections.Concurrent;

namespace MultiMap.Entities
{
    /// <summary>
    /// Represents a thread-safe multi-map collection that associates each key with a set of values.
    /// Supports concurrent operations for adding, removing, and querying key-value pairs.
    /// </summary>
    /// <remarks>
    /// ConcurrentMultiMap is designed for scenarios where multiple threads may add or remove key-value pairs simultaneously.
    /// Each key maps to a set of unique values, and all operations are safe for concurrent access.
    /// This class is useful for managing collections where keys can have multiple associated values and thread safety is required.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multi-map. Must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-nullable.</typeparam>
    public class ConcurrentMultiMap<TKey, TValue> : IMultiMap<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        private readonly ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>> _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class.
        /// </summary>
        public ConcurrentMultiMap()
        {
            _dictionary = new ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>();
        }

        /// <inheritdoc/>
        public bool Add(TKey key, TValue value)
        {
            var concurrentSet = _dictionary.GetOrAdd(
                key,
                _ => new ConcurrentDictionary<TValue, byte>());

            return concurrentSet.TryAdd(value, 0);
        }

        /// <inheritdoc/>
        public void AddRange(TKey key, IEnumerable<TValue> values)
        {
            var concurrentSet = _dictionary.GetOrAdd(
                key,
                _ => new ConcurrentDictionary<TValue, byte>());

            foreach (var value in values)
                concurrentSet.TryAdd(value, 0);
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> Get(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var concurrentSet))
                return concurrentSet.Keys;

            return Enumerable.Empty<TValue>();
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public bool RemoveKey(TKey key)
        {
            return _dictionary.TryRemove(key, out _);
        }

        /// <inheritdoc/>
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <inheritdoc/>
        public bool Contains(TKey key, TValue value)
        {
            return _dictionary.TryGetValue(key, out var concurrentSet) && concurrentSet.ContainsKey(value);
        }

        /// <inheritdoc/>
        public int Count => _dictionary.Sum(kvp => kvp.Value.Count);

        /// <inheritdoc/>
        public IEnumerable<TKey> Keys => _dictionary.Keys;

        /// <inheritdoc/>
        public void Clear() => _dictionary.Clear();

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is ConcurrentMultiMap<TKey, TValue> map &&
                   EqualityComparer<ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>>.Default.Equals(_dictionary, map._dictionary);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(_dictionary);
        }
    }
}
