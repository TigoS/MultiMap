using MultiMap.Interfaces;
using System.Collections;

namespace MultiMap.Entities
{
    /// <summary>
    /// Represents a collection that associates multiple values with each key,
    /// allowing efficient retrieval and management of grouped data.
    /// </summary>
    /// <remarks>Use this class to store and access sets of values for each key, where duplicate values per
    /// key are not allowed. The map is suitable for scenarios where one-to-many relationships are required, such as
    /// grouping items or indexing data.</remarks>
    /// <typeparam name="TKey">The type of keys in the map. Must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-nullable.</typeparam>
    public class SimpleMultiMap<TKey, TValue> : ISimpleMultiMap<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        private readonly Dictionary<TKey, HashSet<TValue>> _dictionary;

        public SimpleMultiMap()
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>();
        }

        /// <inheritdoc />
        bool ISimpleMultiMap<TKey, TValue>.Add(TKey key, TValue value)
        {
            if (!_dictionary.TryGetValue(key, out var hashset))
            {
                hashset = new HashSet<TValue>();
                _dictionary[key] = hashset;
            }

            return hashset.Add(value);
        }

        /// <inheritdoc />
        IEnumerable<TValue> ISimpleMultiMap<TKey, TValue>.Get(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
                return hashset;

            throw new KeyNotFoundException($"The key '{key}' was not found in the multimap.");
        }

        /// <inheritdoc />
        IEnumerable<TValue> ISimpleMultiMap<TKey, TValue>.GetOrDefault(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
                return hashset;

            return Enumerable.Empty<TValue>();
        }

        /// <inheritdoc />
        void ISimpleMultiMap<TKey, TValue>.Remove(TKey key, TValue value)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
            {
                hashset.Remove(value);

                if (hashset.Count == 0)
                    _dictionary.Remove(key);
            }
        }

        /// <inheritdoc />
        void ISimpleMultiMap<TKey, TValue>.Clear(TKey key)
        {
            _dictionary.Remove(key);
        }

        /// <inheritdoc />
        IEnumerable<KeyValuePair<TKey, TValue>> ISimpleMultiMap<TKey, TValue>.Flatten()
        {
            foreach (var kvp in _dictionary)
            {
                foreach (var value in kvp.Value)
                {
                    yield return new KeyValuePair<TKey, TValue>(kvp.Key, value);
                }
            }
        }

        /// <inheritdoc />
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return ((ISimpleMultiMap<TKey, TValue>)this).Flatten().GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<TKey, TValue>>)this).GetEnumerator();
        }
    }
}
