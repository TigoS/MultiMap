using MultiMap.Helpers;
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
    /// <typeparam name="TKey">The type of keys in the multi-map. Must be non-nullable and implement <see cref="IEquatable{TKey}"/>.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-nullable and implement <see cref="IEquatable{TValue}"/>.</typeparam>
    public sealed class ConcurrentMultiMap<TKey, TValue> : IMultiMap<TKey, TValue>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull, IEquatable<TValue>
    {
        private readonly ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>> _dictionary;
        private readonly IEqualityComparer<TValue>? _valueComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class.
        /// </summary>
        public ConcurrentMultiMap()
        {
            _dictionary = new ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class with the specified equality comparer for keys.
        /// </summary>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public ConcurrentMultiMap(IEqualityComparer<TKey>? keyComparer)
        {
            _dictionary = new ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>(keyComparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class with the specified equality comparers for keys and values.
        /// </summary>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public ConcurrentMultiMap(IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)
        {
            _dictionary = new ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>(keyComparer);
            _valueComparer = valueComparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class with the specified initial capacity for keys.
        /// </summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the multimap concurrently.</param>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        public ConcurrentMultiMap(int concurrencyLevel, int capacity)
        {
            _dictionary = new ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>(concurrencyLevel, capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class with the specified concurrency level, initial capacity, and equality comparer for keys.
        /// </summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the multimap concurrently.</param>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public ConcurrentMultiMap(int concurrencyLevel, int capacity, IEqualityComparer<TKey>? keyComparer)
        {
            _dictionary = new ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>(concurrencyLevel, capacity, keyComparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class with the specified initial capacity for keys and equality comparer for values.
        /// </summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the multimap concurrently.</param>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public ConcurrentMultiMap(int concurrencyLevel, int capacity, IEqualityComparer<TValue>? valueComparer)
        {
            _dictionary = new ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>(concurrencyLevel, capacity);
            _valueComparer = valueComparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class with the specified concurrency level, initial capacity, and equality comparers for keys and values.
        /// </summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the multimap concurrently.</param>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public ConcurrentMultiMap(int concurrencyLevel, int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)
        {
            _dictionary = new ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>(concurrencyLevel, capacity, keyComparer);
            _valueComparer = valueComparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class with the specified equality comparer for values.
        /// </summary>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public ConcurrentMultiMap(IEqualityComparer<TValue>? valueComparer)
        {
            _dictionary = new ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>();
            _valueComparer = valueComparer;
        }

        private ConcurrentDictionary<TValue, byte> CreateValueSet() => new(_valueComparer);

        /// <inheritdoc/>
        public bool Add(TKey key, TValue value)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));

            var valuesSet = _dictionary.GetOrAdd(key, _ => CreateValueSet());
            return valuesSet.TryAdd(value, 0);
        }

        /// <inheritdoc/>
        public int AddRange(TKey key, IEnumerable<TValue> values)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (values is null) throw new ArgumentNullException(nameof(values));

            var items = values as ICollection<TValue> ?? values.ToArray();
            var valuesSet = _dictionary.GetOrAdd(key, _ => CreateValueSet());

            int added = 0;
            foreach (var value in items)
                if (valuesSet.TryAdd(value, 0))
                    added++;

            return added;
        }

        /// <inheritdoc/>
        public int AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));

            int added = 0;
            foreach (var item in items)
            {
                if (Add(item.Key, item.Value))
                    added++;
            }

            return added;
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> Get(TKey key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            if (_dictionary.TryGetValue(key, out var valuesSet) && !valuesSet.IsEmpty)
                return valuesSet.Keys.ToArray();

            throw new KeyNotFoundException($"The key '{key}' was not found in the multimap.");
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> GetOrDefault(TKey key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            if (_dictionary.TryGetValue(key, out var valuesSet))
                return valuesSet.Keys.ToArray();

            return [];
        }

        /// <inheritdoc/>
        public bool TryGet(TKey key, out IEnumerable<TValue> values)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            if (_dictionary.TryGetValue(key, out var valuesSet) && !valuesSet.IsEmpty)
            {
                values = valuesSet.Keys.ToArray();
                return true;
            }

            values = [];
            return false;
        }

        /// <inheritdoc/>
        public bool Remove(TKey key, TValue value)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));

            return _dictionary.TryGetValue(key, out var valuesSet) && valuesSet.TryRemove(value, out _);
        }

        /// <inheritdoc/>
        public int RemoveRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));

            int removedCount = 0;

            foreach (var item in items)
            {
                if (Remove(item.Key, item.Value))
                    removedCount++;
            }

            return removedCount;
        }

        /// <inheritdoc/>
        public int RemoveWhere(TKey key, Predicate<TValue> predicate)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            if (!_dictionary.TryGetValue(key, out var valuesSet))
                return 0;

            int removedCount = 0;
            foreach (var value in valuesSet.Keys)
                if (predicate(value) && valuesSet.TryRemove(value, out _))
                    removedCount++;

            return removedCount;
        }

        /// <inheritdoc/>
        public bool RemoveKey(TKey key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            return _dictionary.TryRemove(key, out _);
        }

        /// <inheritdoc/>
        public bool ContainsKey(TKey key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            return _dictionary.TryGetValue(key, out var valuesSet) && !valuesSet.IsEmpty;
        }

        /// <inheritdoc/>
        public bool Contains(TKey key, TValue value)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));

            return _dictionary.TryGetValue(key, out var valuesSet) && valuesSet.ContainsKey(value);
        }

        /// <inheritdoc/>
        public int Count
        {
            get
            {
                int n = 0;
                foreach (var kvp in _dictionary)
                    n += kvp.Value.Count;
                return n;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<TKey> Keys
        {
            get
            {
                var result = new List<TKey>();
                foreach (var kvp in _dictionary)
                    if (!kvp.Value.IsEmpty)
                        result.Add(kvp.Key);
                return result;
            }
        }

        /// <inheritdoc/>
        public int KeyCount
        {
            get
            {
                int n = 0;
                foreach (var kvp in _dictionary)
                    if (!kvp.Value.IsEmpty)
                        n++;
                return n;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> Values
        {
            get
            {
                var result = new List<TValue>();
                foreach (var kvp in _dictionary)
                    result.AddRange(kvp.Value.Keys);
                return result;
            }
        }

        /// <inheritdoc/>
        public int GetValuesCount(TKey key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            return _dictionary.TryGetValue(key, out var valuesSet) ? valuesSet.Count : 0;
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> this[TKey key] => Get(key);

        /// <inheritdoc/>
        public void Clear()
        {
            _dictionary.Clear();
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            var snapshot = new List<KeyValuePair<TKey, TValue>>();
            foreach (var kvp in _dictionary)
                foreach (var value in kvp.Value.Keys)
                    snapshot.Add(new KeyValuePair<TKey, TValue>(kvp.Key, value));
            return snapshot.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is not ConcurrentMultiMap<TKey, TValue> other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (Count != other.Count)
                return false;

            if (KeyCount != other.KeyCount)
                return false;

            foreach (var kvp in _dictionary)
            {
                if (kvp.Value.IsEmpty)
                    continue;

                if (!other._dictionary.TryGetValue(kvp.Key, out var otherSet))
                    return false;

                var thisSet = new HashSet<TValue>(kvp.Value.Keys, _valueComparer);
                if (!thisSet.SetEquals(otherSet.Keys))
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public bool Equals(IReadOnlyMultiMap<TKey, TValue>? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (KeyCount != other.KeyCount)
                return false;

            foreach (var kvp in _dictionary)
            {
                if (kvp.Value.IsEmpty)
                    continue;

                if (!other.ContainsKey(kvp.Key))
                    return false;

                var thisSet = new HashSet<TValue>(kvp.Value.Keys, _valueComparer);
                var otherSet = new HashSet<TValue>(other[kvp.Key], _valueComparer);

                if (!thisSet.SetEquals(otherSet))
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 0;
                foreach (var kvp in _dictionary)
                {
                    if (kvp.Value.IsEmpty)
                        continue;

                    int valueHash = 0;
                    foreach (var value in kvp.Value.Keys)
                        valueHash += MultiMapHelper.Scramble(value.GetHashCode());
                    hash += MultiMapHelper.Scramble(HashCode.Combine(kvp.Key, valueHash));
                }
                return hash;
            }
        }
    }
}
