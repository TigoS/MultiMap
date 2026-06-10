using MultiMap.Helpers;
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
    /// <see cref="SimpleMultiMap{TKey, TValue}"/> is an intentionally limited, standalone set-based multimap intended for initial design and drafting scenarios where only the <see cref="ISimpleMultiMap{TKey, TValue}"/> contract is needed.
    /// It duplicates the constructor overloads already present on <see cref="MultiMapSet{TKey, TValue}"/> because it does not inherit from <see cref="MultiMapBase{TKey, TValue, TCollection}"/>; the duplication is deliberate, not an oversight.
    /// For production use, prefer <see cref="MultiMapSet{TKey, TValue}"/> or another full <see cref="Interfaces.IMultiMap{TKey, TValue}"/> implementation.
    /// Duplicate values per key are not allowed.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the map. Must be non-nullable and implement <see cref="IEquatable{TKey}"/>.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-nullable and implement <see cref="IEquatable{TValue}"/>.</typeparam>
    public sealed class SimpleMultiMap<TKey, TValue> : ISimpleMultiMap<TKey, TValue>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull, IEquatable<TValue>
    {
        private readonly Dictionary<TKey, HashSet<TValue>> _dictionary;
        private readonly IEqualityComparer<TValue>? _valueComparer;
        private int _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleMultiMap{TKey, TValue}"/> class.
        /// </summary>
        public SimpleMultiMap()
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleMultiMap{TKey, TValue}"/> class with the specified equality comparer for keys.
        /// </summary>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public SimpleMultiMap(IEqualityComparer<TKey>? keyComparer)
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>(keyComparer);
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
        /// with the specified initial capacity for keys and equality comparer for keys.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public SimpleMultiMap(int capacity, IEqualityComparer<TKey>? keyComparer)
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>(capacity, keyComparer);
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
        /// with the specified initial capacity for keys and equality comparers for keys and values.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public SimpleMultiMap(int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>(capacity, keyComparer);
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

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleMultiMap{TKey, TValue}"/> class
        /// with the specified equality comparers for keys and values.
        /// </summary>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public SimpleMultiMap(IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>(keyComparer);
            _valueComparer = valueComparer;
        }

        /// <inheritdoc />
        public int Count => _count;

        /// <inheritdoc />
        public bool Add(TKey key, TValue value)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(value, nameof(value));

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

            bool added = hashset.Add(value);
            if (added) _count++;
            return added;
        }

        /// <inheritdoc />
        public IEnumerable<TValue> Get(TKey key)
        {
            Guard.NotNull(key, nameof(key));

            if (_dictionary.TryGetValue(key, out var hashset))
#if NET9_0_OR_GREATER
                return hashset.AsReadOnly();
#else
                return hashset.ToArray();
#endif

            throw new KeyNotFoundException($"The key '{key}' was not found in the multimap.");
        }

        /// <inheritdoc />
        public IEnumerable<TValue> GetOrDefault(TKey key)
        {
            Guard.NotNull(key, nameof(key));

            if (_dictionary.TryGetValue(key, out var hashset))
#if NET9_0_OR_GREATER
                return hashset.AsReadOnly();
#else
                return hashset.ToArray();
#endif

            return Array.Empty<TValue>();
        }

        /// <inheritdoc/>
        public bool TryGet(TKey key, out IEnumerable<TValue> values)
        {
            Guard.NotNull(key, nameof(key));

            bool result = _dictionary.TryGetValue(key, out var hashset);

            if (!result || hashset is null)
            {
                values = Array.Empty<TValue>();
            }
            else
            {
                values =
#if NET9_0_OR_GREATER
                hashset.AsReadOnly();
#else
                hashset.ToArray();
#endif
            }

            return result;
        }

        /// <inheritdoc />
        public bool Remove(TKey key, TValue value)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(value, nameof(value));

            if (_dictionary.TryGetValue(key, out var hashset))
            {
                bool removed = hashset.Remove(value);

                if (removed)
                {
                    _count--;
                    if (hashset.Count == 0)
                        _dictionary.Remove(key);
                }

                return removed;
            }

            return false;
        }

        /// <inheritdoc />
        public bool RemoveKey(TKey key)
        {
            Guard.NotNull(key, nameof(key));

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            if (_dictionary.Remove(key, out var collection))
#else
            if (_dictionary.TryGetValue(key, out var collection) && _dictionary.Remove(key))
#endif
            {
                _count -= collection.Count;
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _dictionary.Clear();
            _count = 0;
        }

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

        /// <inheritdoc />
        public bool ContainsKey(TKey key)
        {
            Guard.NotNull(key, nameof(key));

            return _dictionary.ContainsKey(key);
        }

        /// <inheritdoc />
        public bool Contains(TKey key, TValue value)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(value, nameof(value));

            return _dictionary.TryGetValue(key, out var hashset) && hashset.Contains(value);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as IReadOnlySimpleMultiMap<TKey, TValue>);

        /// <inheritdoc/>
        public bool Equals(IReadOnlySimpleMultiMap<TKey, TValue>? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (Count != other.Count)
                return false;

            foreach (var kvp in _dictionary)
            {
                if (!kvp.Value.SetEquals(other.GetOrDefault(kvp.Key)))
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
            => MultiMapHelper.ComputeUnorderedHash<TKey, TValue, HashSet<TValue>>(_dictionary, _dictionary.Comparer, _valueComparer);
    }
}
