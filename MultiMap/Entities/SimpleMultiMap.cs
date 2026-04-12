using MultiMap.Interfaces;
using System.Collections;
#if NET6_0_OR_GREATER
using System.Runtime.InteropServices;
#endif

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
        where TKey : notnull
        where TValue : notnull
    {
        private readonly Dictionary<TKey, HashSet<TValue>> _dictionary;
        private readonly IEqualityComparer<TValue>? _valueComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleMultiMap{TKey, TValue}"/> class.
        /// </summary>
        public SimpleMultiMap()
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleMultiMap{TKey, TValue}"/> class
        /// with the specified initial capacity for keys.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        public SimpleMultiMap(int capacity)
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleMultiMap{TKey, TValue}"/> class
        /// with the specified initial capacity for keys and equality comparer for values.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public SimpleMultiMap(int capacity, IEqualityComparer<TValue>? valueComparer)
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>(capacity);
            _valueComparer = valueComparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleMultiMap{TKey, TValue}"/> class
        /// with the specified equality comparer for values.
        /// </summary>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public SimpleMultiMap(IEqualityComparer<TValue>? valueComparer)
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>();
            _valueComparer = valueComparer;
        }

        /// <inheritdoc />
        public bool Add(TKey key, TValue value)
        {
#if NET6_0_OR_GREATER
            ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, key, out bool exists);
            hashset ??= new HashSet<TValue>(_valueComparer);
#else
            if (!_dictionary.TryGetValue(key, out var hashset))
            {
                hashset = new HashSet<TValue>(_valueComparer);
                _dictionary[key] = hashset;
            }
#endif

            return hashset.Add(value);
        }

        /// <inheritdoc />
        public IEnumerable<TValue> Get(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
                return hashset.ToArray();

            throw new KeyNotFoundException($"The key '{key}' was not found in the multimap.");
        }

        /// <inheritdoc />
        public IEnumerable<TValue> GetOrDefault(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
                return hashset.ToArray();

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
        public IEnumerable<KeyValuePair<TKey, TValue>> Flatten() => this;

        /// <inheritdoc />
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

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is not SimpleMultiMap<TKey, TValue> other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (_dictionary.Count != other._dictionary.Count)
                return false;

            foreach (var kvp in _dictionary)
            {
                if (!other._dictionary.TryGetValue(kvp.Key, out var otherSet))
                    return false;

                if (!kvp.Value.SetEquals(otherSet))
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hash = 0;
            foreach (var kvp in _dictionary)
            {
                int valueHash = 0;
                foreach (var value in kvp.Value)
                {
                    valueHash ^= value.GetHashCode();
                }
                hash ^= HashCode.Combine(kvp.Key, valueHash);
            }
            return hash;
        }
    }
}
