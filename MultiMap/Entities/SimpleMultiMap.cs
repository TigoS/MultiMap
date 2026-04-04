using MultiMap.Interfaces;
using System.Collections;
using System.Runtime.InteropServices;

namespace MultiMap.Entities
{
    /// <summary>
    /// Represents a collection that associates multiple values with each key, allowing efficient retrieval and management of grouped data.
    /// </summary>
    /// <remarks>
    /// Use this class to store and access sets of values for each key, where duplicate values per key are not allowed.
    /// The map is suitable for scenarios where one-to-many relationships are required, such as grouping items or indexing data.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the map. Must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-nullable.</typeparam>
    public class SimpleMultiMap<TKey, TValue> : ISimpleMultiMap<TKey, TValue>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull
    {
        private readonly Dictionary<TKey, HashSet<TValue>> _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleMultiMap{TKey, TValue}"/> class.
        /// </summary>
        public SimpleMultiMap()
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>();
        }

        /// <inheritdoc />
        public bool Add(TKey key, TValue value)
        {
            ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, key, out bool exists);
            hashset ??= new HashSet<TValue>();

            return hashset.Add(value);
        }

        /// <inheritdoc />
        public IEnumerable<TValue> Get(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
                return hashset;

            throw new KeyNotFoundException($"The key '{key}' was not found in the multimap.");
        }

        /// <inheritdoc />
        public IEnumerable<TValue> GetOrDefault(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
                return hashset;

            return [];
        }

        /// <inheritdoc />
        public void Remove(TKey key, TValue value)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
            {
                hashset.Remove(value);

                if (hashset.Count == 0)
                    _dictionary.Remove(key);
            }
        }

        /// <inheritdoc />
        public void Clear(TKey key)
        {
            _dictionary.Remove(key);
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<TKey, TValue>> Flatten()
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
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return Flatten().GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
