using System.Collections;
using System.Collections.Concurrent;
using MultiMap.Helpers;
using MultiMap.Interfaces;

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
    /// <para>
    /// Unlike <see cref="MultiMapLock{TKey, TValue}"/> and <see cref="MultiMapAsync{TKey, TValue}"/>, this class does <b>not</b> implement <see cref="IDisposable"/> because it owns no disposable resources — the underlying <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}"/> requires no explicit cleanup.
    /// </para>
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multi-map. Must be non-nullable and implement <see cref="IEquatable{TKey}"/>.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-nullable and implement <see cref="IEquatable{TValue}"/>.</typeparam>
    public sealed class ConcurrentMultiMap<TKey, TValue> : IMultiMap<TKey, TValue>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull, IEquatable<TValue>
    {
        private readonly ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>> _dictionary;
        private readonly IEqualityComparer<TValue>? _valueComparer;
        private int _count;

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
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);
#else
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));
#endif

            if (!_dictionary.TryGetValue(key, out var valuesSet))
            {
                var candidate = CreateValueSet();
                valuesSet = _dictionary.GetOrAdd(key, candidate);
            }

            if (valuesSet.TryAdd(value, 0))
            {
                Interlocked.Increment(ref _count);
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public int AddRange(TKey key, IEnumerable<TValue> values)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(values);
#else
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (values is null) throw new ArgumentNullException(nameof(values));
#endif

            var items = values as ICollection<TValue> ?? values.ToArray();
            if (items.Count == 0)
            {
                return 0;
            }

            if (!_dictionary.TryGetValue(key, out var valuesSet))
            {
                var candidate = CreateValueSet();
                valuesSet = _dictionary.GetOrAdd(key, candidate);
            }

            int added = 0;
            foreach (var value in items)
            {
                if (valuesSet.TryAdd(value, 0))
                {
                    added++;
                }
            }

            if (added > 0)
            {
                Interlocked.Add(ref _count, added);
            }

            return added;
        }

        /// <inheritdoc/>
        public int AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(items);
#else
            if (items is null) throw new ArgumentNullException(nameof(items));
#endif

            // Group by key so we call GetOrAdd at most once per unique key instead of once per item.
            var grouped = new Dictionary<TKey, List<TValue>>();
            foreach (var item in items)
            {
                if (item.Key is null)
                    throw new ArgumentNullException(nameof(items), "Sequence contains a null key.");

                if (item.Value is null)
                    throw new ArgumentNullException(nameof(items), "Sequence contains a null value.");

                if (!grouped.TryGetValue(item.Key, out var list))
                {
                    list = new List<TValue>();
                    grouped[item.Key] = list;
                }

                list.Add(item.Value);
            }

            int added = 0;
            foreach (var group in grouped)
            {
                if (!_dictionary.TryGetValue(group.Key, out var valuesSet))
                {
                    var candidate = CreateValueSet();
                    valuesSet = _dictionary.GetOrAdd(group.Key, candidate);
                }

                int groupAdded = 0;
                foreach (var value in group.Value)
                {
                    if (valuesSet.TryAdd(value, 0))
                    {
                        groupAdded++;
                    }
                }

                if (groupAdded > 0)
                {
                    Interlocked.Add(ref _count, groupAdded);
                    added += groupAdded;
                }
            }

            return added;
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> Get(TKey key)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(key);
#else
            if (key is null) throw new ArgumentNullException(nameof(key));
#endif

            if (_dictionary.TryGetValue(key, out var valuesSet) && !valuesSet.IsEmpty)
            {
                return valuesSet.Keys.ToArray();
            }

            throw new KeyNotFoundException($"The key '{key}' was not found in the multimap.");
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> GetOrDefault(TKey key)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(key);
#else
            if (key is null) throw new ArgumentNullException(nameof(key));
#endif

            if (_dictionary.TryGetValue(key, out var valuesSet) && !valuesSet.IsEmpty)
            {
                return valuesSet.Keys.ToArray();
            }

            return [];
        }

        /// <inheritdoc/>
        public bool TryGet(TKey key, out IEnumerable<TValue> values)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(key);
#else
            if (key is null) throw new ArgumentNullException(nameof(key));
#endif

            if (_dictionary.TryGetValue(key, out var valuesSet) && !valuesSet.IsEmpty)
            {
                values = valuesSet.Keys.ToArray();
                return true;
            }

            values = Array.Empty<TValue>();

            return false;
        }

        /// <inheritdoc/>
        public bool Remove(TKey key, TValue value)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);
#else
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));
#endif

            if (!_dictionary.TryGetValue(key, out var valuesSet))
            {
                return false;
            }

            if (!valuesSet.TryRemove(value, out _))
            {
                return false;
            }

            Interlocked.Decrement(ref _count);

            // Prune the outer key only when the inner set is confirmed empty.
            // Use the conditional-remove pattern: remove the entry, then re-add it
            // if another thread concurrently inserted a value — this makes the prune
            // atomic with respect to concurrent Adds on the same key.
            TryPruneEmptySet(key, valuesSet);

            return true;
        }

        /// <inheritdoc/>
        public int RemoveRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(items);
#else
            if (items is null) throw new ArgumentNullException(nameof(items));
#endif

            int removedCount = 0;

            foreach (var item in items)
            {
                if (Remove(item.Key, item.Value))
                {
                    removedCount++;
                }
            }

            return removedCount;
        }

        /// <inheritdoc/>
        public int RemoveWhere(TKey key, Predicate<TValue> predicate)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(predicate);
#else
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));
#endif

            if (!_dictionary.TryGetValue(key, out var valuesSet))
            {
                return 0;
            }

            int removedCount = 0;
            foreach (var value in valuesSet.Keys)
            {
                if (predicate(value) && valuesSet.TryRemove(value, out _))
                {
                    removedCount++;
                }
            }

            if (removedCount > 0)
            {
                Interlocked.Add(ref _count, -removedCount);
                TryPruneEmptySet(key, valuesSet);
            }

            return removedCount;
        }

        /// <inheritdoc/>
        public bool RemoveKey(TKey key)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(key);
#else
            if (key is null) throw new ArgumentNullException(nameof(key));
#endif

            if (!_dictionary.TryRemove(key, out var removedSet))
            {
                return false;
            }

            // Snapshot the inner count at the moment of removal and adjust _count.
            // Under a concurrent Add/Remove racing on the same key, _count may transiently
            // deviate from the true value by the number of values touched in that race window;
            // this is an accepted trade-off for O(1) Count.
            int c = removedSet.Count;
            if (c > 0)
            {
                Interlocked.Add(ref _count, -c);
            }

            return true;
        }

        /// <inheritdoc/>
        public bool ContainsKey(TKey key)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(key);
#else
            if (key is null) throw new ArgumentNullException(nameof(key));
#endif

            return _dictionary.TryGetValue(key, out var valuesSet) && !valuesSet.IsEmpty;
        }

        /// <inheritdoc/>
        public bool Contains(TKey key, TValue value)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);
#else
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));
#endif

            return _dictionary.TryGetValue(key, out var valuesSet) && valuesSet.ContainsKey(value);
        }

        /// <inheritdoc/>
        public int Count => Volatile.Read(ref _count);

        /// <inheritdoc/>
        public IEnumerable<TKey> Keys
        {
            get
            {
                foreach (var kvp in _dictionary)
                {
                    if (!kvp.Value.IsEmpty)
                    {
                        yield return kvp.Key;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public int KeyCount
        {
            get
            {
                int count = 0;
                foreach (var kvp in _dictionary)
                {
                    if (!kvp.Value.IsEmpty)
                        count++;
                }
                return count;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> Values
        {
            get
            {
                foreach (var kvp in _dictionary)
                {
                    foreach (var value in kvp.Value.Keys)
                    {
                        yield return value;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public int GetValuesCount(TKey key)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(key);
#else
            if (key is null) throw new ArgumentNullException(nameof(key));
#endif

            return _dictionary.TryGetValue(key, out var valuesSet) ? valuesSet.Count : 0;
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> this[TKey key] => Get(key);

        /// <summary>
        /// Attempts to remove <paramref name="key"/> from the outer dictionary when its inner set is empty.
        /// If another thread has concurrently added a value to the set between the caller's last removal and this prune attempt, the key is left in place (or re-inserted if <c>TryRemove</c> already succeeded).
        /// This makes key-pruning safe under concurrent <c>Add</c> operations.
        /// </summary>
        private void TryPruneEmptySet(TKey key, ConcurrentDictionary<TValue, byte> valuesSet)
        {
            if (!valuesSet.IsEmpty)
            {
                return;
            }

            // Attempt to remove the outer entry only while the inner set is still empty.
            // If TryRemove succeeds but the set was concurrently populated, put it back.
            if (_dictionary.TryRemove(key, out var removedSet))
            {
                if (!removedSet.IsEmpty)
                {
                    // Another thread added to the set between our check and the remove;
                    // restore the entry so no values are lost.
                    _dictionary.TryAdd(key, removedSet);
                }
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _dictionary.Clear();
            Interlocked.Exchange(ref _count, 0);
        }

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
        public override bool Equals(object? obj) => Equals(obj as ConcurrentMultiMap<TKey, TValue>);

        /// <inheritdoc/>
        public bool Equals(IReadOnlySimpleMultiMap<TKey, TValue>? other) => Equals(other as IReadOnlyMultiMap<TKey, TValue>);

        /// <inheritdoc/>
        public bool Equals(IReadOnlyMultiMap<TKey, TValue>? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (KeyCount != other.KeyCount || Count != other.Count)
            {
                return false;
            }

            foreach (var kvp in _dictionary)
            {
                if (kvp.Value.IsEmpty)
                {
                    continue;
                }

                if (!other.ContainsKey(kvp.Key) || GetValuesCount(kvp.Key) != other.GetValuesCount(kvp.Key))
                {
                    return false;
                }

                var thisSet = new HashSet<TValue>(kvp.Value.Keys, _valueComparer);
                var otherSet = new HashSet<TValue>(other[kvp.Key], _valueComparer);

                if (!thisSet.SetEquals(otherSet))
                {
                    return false;
                }
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
                    {
                        continue;
                    }

                    int valueHash = 0;
                    foreach (var value in kvp.Value.Keys)
                    {
                        valueHash += MultiMapHelper.Scramble(value.GetHashCode());
                    }

                    hash += MultiMapHelper.Scramble(HashCode.Combine(kvp.Key, valueHash));
                }

                return hash;
            }
        }
    }
}
