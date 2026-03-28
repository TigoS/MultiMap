using MultiMap.Interfaces;
using System.Collections;

namespace MultiMap.Entities
{
    /// <summary>
    /// Represents a collection that maps keys to sets of values, allowing multiple values to be associated with each key.
    /// Provides set semantics for values, ensuring that each value is unique per key.
    /// </summary>
    /// <remarks>
    /// This implementation uses a dictionary of hash sets to store values for each key, providing efficient lookup and uniqueness enforcement.
    /// Values associated with a key are unordered and duplicates are not allowed.
    /// The class is not thread-safe; external synchronization is required for concurrent access.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multimap. Must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The type of values in the multimap. Must be non-nullable.</typeparam>
    public class MultiMapSet<TKey, TValue> : IMultiMap<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        private readonly Dictionary<TKey, HashSet<TValue>> _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapSet{TKey, TValue}"/> class.
        /// </summary>
        public MultiMapSet()
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>();
        }

        /// <inheritdoc/>
        public bool Add(TKey key, TValue value)
        {
            if (!_dictionary.TryGetValue(key, out var hashset))
            {
                hashset = new HashSet<TValue>();
                _dictionary[key] = hashset;
            }

            return hashset.Add(value);
        }

        /// <inheritdoc/>
        public void AddRange(TKey key, IEnumerable<TValue> values)
        {
            if (!_dictionary.TryGetValue(key, out var hashset))
            {
                hashset = new HashSet<TValue>();
                _dictionary[key] = hashset;
            }

            foreach (var value in values)
                hashset.Add(value);
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> Get(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
                return hashset;

            return Enumerable.Empty<TValue>();
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public bool RemoveKey(TKey key)
        {
            return _dictionary.Remove(key);
        }

        /// <inheritdoc/>
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <inheritdoc/>
        public bool Contains(TKey key, TValue value)
        {
            return _dictionary.TryGetValue(key, out var hashset) && hashset.Contains(value);
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
                foreach (var value in kvp.Value)
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
            return obj is MultiMapSet<TKey, TValue> map &&
                   EqualityComparer<Dictionary<TKey, HashSet<TValue>>>.Default.Equals(_dictionary, map._dictionary);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(_dictionary);
        }
    }
}
