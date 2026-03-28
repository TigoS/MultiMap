using MultiMap.Interfaces;
using System.Collections;

namespace MultiMap.Entities
{
    /// <summary>
    /// Represents a collection that associates multiple values with each key, allowing efficient storage and retrieval of key-value pairs where keys may map to zero or more values.
    /// </summary>
    /// <remarks>
    /// This class is useful for scenarios where a key can have multiple associated values, such as grouping or indexing.
    /// The collection maintains insertion order for values under each key.
    /// Keys and values must be non-null. Thread safety is not guaranteed;
    /// external synchronization is required for concurrent access.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multi-map. Must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-nullable.</typeparam>
    public class MultiMapList<TKey, TValue> : IMultiMap<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        private readonly Dictionary<TKey, List<TValue>> _dictionary;

        /// <summary>
        /// Initializes a new instance of the MultiMapList class with an empty mapping.
        /// </summary>
        /// <remarks>Use this constructor to create a MultiMapList that starts with no key-value associations.
        /// The internal dictionary is initialized and ready for adding keys and values.</remarks>
        public MultiMapList()
        {
            _dictionary = new Dictionary<TKey, List<TValue>>();
        }

        /// <inheritdoc/>
        public bool Add(TKey key, TValue value)
        {
            if (!_dictionary.TryGetValue(key, out var list))
            {
                list = new List<TValue>();
                _dictionary[key] = list;
            }

            list.Add(value);

            return true;
        }

        /// <inheritdoc/>
        public void AddRange(TKey key, IEnumerable<TValue> values)
        {
            if (!_dictionary.TryGetValue(key, out var list))
            {
                list = new List<TValue>();
                _dictionary[key] = list;
            }

            list.AddRange(values);
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> Get(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var list))
                return list;

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
            return _dictionary.TryGetValue(key, out var list) && list.Contains(value);
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
            return obj is MultiMapList<TKey, TValue> map &&
                   EqualityComparer<Dictionary<TKey, List<TValue>>>.Default.Equals(_dictionary, map._dictionary);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(_dictionary);
        }
    }
}
