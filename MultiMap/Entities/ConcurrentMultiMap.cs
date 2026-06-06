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
    public sealed class ConcurrentMultiMap<TKey, TValue> : MultiMapBase<TKey, TValue, ConcurrentSet<TValue>>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull, IEquatable<TValue>
    {
        private readonly IEqualityComparer<TValue>? _valueComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class.
        /// </summary>
        public ConcurrentMultiMap()
            : base(new ConcurrentDictionary<TKey, ConcurrentSet<TValue>>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class with the specified equality comparer for keys.
        /// </summary>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public ConcurrentMultiMap(IEqualityComparer<TKey>? keyComparer)
            : base(new ConcurrentDictionary<TKey, ConcurrentSet<TValue>>(keyComparer))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class with the specified equality comparers for keys and values.
        /// </summary>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public ConcurrentMultiMap(IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)
            : base(new ConcurrentDictionary<TKey, ConcurrentSet<TValue>>(keyComparer))
        {
            _valueComparer = valueComparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class with the specified concurrency level and initial capacity for keys.
        /// </summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the multimap concurrently.</param>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        public ConcurrentMultiMap(int concurrencyLevel, int capacity)
            : base(new ConcurrentDictionary<TKey, ConcurrentSet<TValue>>(concurrencyLevel, capacity))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class with the specified concurrency level, initial capacity, and equality comparer for keys.
        /// </summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the multimap concurrently.</param>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public ConcurrentMultiMap(int concurrencyLevel, int capacity, IEqualityComparer<TKey>? keyComparer)
            : base(new ConcurrentDictionary<TKey, ConcurrentSet<TValue>>(concurrencyLevel, capacity, keyComparer))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class with the specified concurrency level, initial capacity, and equality comparers for keys and values.
        /// </summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the multimap concurrently.</param>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public ConcurrentMultiMap(int concurrencyLevel, int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)
            : base(new ConcurrentDictionary<TKey, ConcurrentSet<TValue>>(concurrencyLevel, capacity, keyComparer))
        {
            _valueComparer = valueComparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class with the specified concurrency level, initial capacity, and equality comparer for values.
        /// </summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the multimap concurrently.</param>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public ConcurrentMultiMap(int concurrencyLevel, int capacity, IEqualityComparer<TValue>? valueComparer)
            : base(new ConcurrentDictionary<TKey, ConcurrentSet<TValue>>(concurrencyLevel, capacity))
        {
            _valueComparer = valueComparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class with the specified equality comparer for values.
        /// </summary>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public ConcurrentMultiMap(IEqualityComparer<TValue>? valueComparer)
            : base(new ConcurrentDictionary<TKey, ConcurrentSet<TValue>>())
        {
            _valueComparer = valueComparer;
        }

        /// <summary>
        /// Gets a typed reference to the underlying concurrent dictionary for efficient access.
        /// </summary>
        private ConcurrentDictionary<TKey, ConcurrentSet<TValue>> _concurrentDictionary => (ConcurrentDictionary<TKey, ConcurrentSet<TValue>>)_dictionary;

        /// <inheritdoc/>
        protected override ConcurrentSet<TValue> CreateCollection() => new(_valueComparer);

        /// <inheritdoc/>
        protected override bool AddToCollection(ConcurrentSet<TValue> collection, TValue value) => collection.TryAdd(value);

        /// <inheritdoc/>
        protected override int RemoveWhereFromCollection(ConcurrentSet<TValue> collection, Predicate<TValue> predicate)
        {
            int removed = 0;
            foreach (var value in collection)
            {
                if (predicate(value) && collection.TryRemove(value, out _))
                    removed++;
            }
            return removed;
        }

        /// <summary>
        /// Returns a snapshot of values from a concurrent set without relying on Count, which can change during enumeration.
        /// This avoids "Destination array was not long enough" errors under concurrent modification.
        /// </summary>
        protected override IEnumerable<TValue> ToReadOnly(ConcurrentSet<TValue> collection)
        {
            var result = new List<TValue>();
            foreach (var item in collection)
            {
                result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// Attempts to retrieve the collection associated with a key, filtering out empty sets.
        /// Empty sets can transiently exist in the dictionary due to concurrent remove operations; this override hides them.
        /// </summary>
        protected override bool TryGetCollection(TKey key, out ConcurrentSet<TValue> collection)
        {
            if (_dictionary.TryGetValue(key, out var valueSet) && !valueSet.IsEmpty)
            {
                collection = valueSet;
                return true;
            }

            collection = null!;

            return false;
        }

        /// <inheritdoc/>
        public override bool Add(TKey key, TValue value)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(value, nameof(value));

            var valueSet = _concurrentDictionary.GetOrAdd(key, _ => CreateCollection());

            if (valueSet.TryAdd(value))
            {
                Interlocked.Increment(ref _count);
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public override int AddRange(TKey key, IEnumerable<TValue> values)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(values, nameof(values));

            var items = values as ICollection<TValue> ?? values.ToArray();
            if (items.Count == 0)
                return 0;

            var valueSet = _concurrentDictionary.GetOrAdd(key, _ => CreateCollection());

            int added = 0;
            foreach (var value in items)
            {
                if (valueSet.TryAdd(value))
                    added++;
            }

            if (added > 0)
                Interlocked.Add(ref _count, added);

            return added;
        }

        /// <inheritdoc/>
        public override int AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            Guard.NotNull(items, nameof(items));

            // Group by key so we call GetOrAdd at most once per unique key instead of once per item.
            var grouped = new Dictionary<TKey, List<TValue>>();
            foreach (var item in items)
            {
                Guard.NotNull(item, nameof(items), "Sequence contains a null item.");
                Guard.NotNull(item.Key, nameof(items), "Sequence contains a null key.");
                Guard.NotNull(item.Value, nameof(items), "Sequence contains a null value.");

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
                var valueSet = _concurrentDictionary.GetOrAdd(group.Key, _ => CreateCollection());

                int groupAdded = 0;
                foreach (var value in group.Value)
                {
                    if (valueSet.TryAdd(value))
                        groupAdded++;
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
        public override bool Remove(TKey key, TValue value)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(value, nameof(value));

            if (!_dictionary.TryGetValue(key, out var valueSet))
                return false;

            if (!valueSet.TryRemove(value, out _))
                return false;

            Interlocked.Decrement(ref _count);

            // Prune the outer key only when the inner set is confirmed empty.
            // Use the conditional-remove pattern: remove the entry, then re-add it
            // if another thread concurrently inserted a value — this makes the prune
            // atomic with respect to concurrent Adds on the same key.
            TryPruneEmptySet(key, valueSet);

            return true;
        }

        /// <inheritdoc/>
        public override int RemoveWhere(TKey key, Predicate<TValue> predicate)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(predicate, nameof(predicate));

            if (!_dictionary.TryGetValue(key, out var valueSet))
                return 0;

            int removedCount = 0;
            foreach (var value in valueSet)
            {
                if (predicate(value) && valueSet.TryRemove(value, out _))
                    removedCount++;
            }

            if (removedCount > 0)
            {
                Interlocked.Add(ref _count, -removedCount);
                TryPruneEmptySet(key, valueSet);
            }

            return removedCount;
        }

        /// <inheritdoc/>
        public override bool RemoveKey(TKey key)
        {
            Guard.NotNull(key, nameof(key));

            if (!_concurrentDictionary.TryRemove(key, out var removedSet))
                return false;

            // Snapshot the inner count at the moment of removal and adjust _count.
            // Under a concurrent Add/Remove racing on the same key, _count may transiently
            // deviate from the true value by the number of values touched in that race window;
            // this is an accepted trade-off for O(1) Count.
            int c = removedSet.Count;
            if (c > 0)
                Interlocked.Add(ref _count, -c);

            return true;
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            _concurrentDictionary.Clear();
            Interlocked.Exchange(ref _count, 0);
        }

        /// <inheritdoc/>
        public override int Count => Volatile.Read(ref _count);

        /// <inheritdoc/>
        public override IEnumerable<TKey> Keys
        {
            get
            {
                foreach (var kvp in _concurrentDictionary)
                {
                    if (!kvp.Value.IsEmpty)
                        yield return kvp.Key;
                }
            }
        }

        /// <inheritdoc/>
        public override int KeyCount
        {
            get
            {
                int count = 0;
                foreach (var kvp in _concurrentDictionary)
                {
                    if (!kvp.Value.IsEmpty)
                        count++;
                }
                return count;
            }
        }

        /// <inheritdoc/>
        public override bool Contains(TKey key, TValue value)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(value, nameof(value));

            return _dictionary.TryGetValue(key, out var collection) && collection.Contains(value);
        }

        /// <inheritdoc/>
        public override int GetValuesCount(TKey key)
        {
            Guard.NotNull(key, nameof(key));

            if (!TryGetCollection(key, out var collection))
                return 0;

            return collection.Count;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as ConcurrentMultiMap<TKey, TValue>);

        /// <inheritdoc/>
        public override bool Equals(IReadOnlyMultiMap<TKey, TValue>? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (KeyCount != other.KeyCount || Count != other.Count)
                return false;

            foreach (var kvp in _concurrentDictionary)
            {
                if (kvp.Value.IsEmpty)
                    continue;

                if (!other.ContainsKey(kvp.Key) || GetValuesCount(kvp.Key) != other.GetValuesCount(kvp.Key))
                    return false;

                var otherSet = new HashSet<TValue>(other[kvp.Key], _valueComparer);
                if (kvp.Value.Count != otherSet.Count)
                    return false;

                foreach (var value in kvp.Value)
                    if (!otherSet.Contains(value))
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
                foreach (var kvp in _concurrentDictionary)
                {
                    if (kvp.Value.IsEmpty)
                        continue;

                    int valueHash = 0;
                    foreach (var value in kvp.Value)
                    {
                        valueHash += MultiMapHelper.Scramble(value.GetHashCode());
                    }

                    hash += MultiMapHelper.Scramble(HashCode.Combine(kvp.Key, valueHash));
                }

                return hash;
            }
        }

        /// <summary>
        /// Attempts to remove <paramref name="key"/> from the outer dictionary when its inner set is empty.
        /// If another thread has concurrently added a value to the set between the caller's last removal and this prune attempt, the key is left in place (or re-inserted if <c>TryRemove</c> already succeeded).
        /// This makes key-pruning safe under concurrent <c>Add</c> operations.
        /// </summary>
        private void TryPruneEmptySet(TKey key, ConcurrentSet<TValue> valueSet)
        {
            if (!valueSet.IsEmpty)
                return;

            // Attempt to remove the outer entry only while the inner set is still empty.
            // If TryRemove succeeds but the set was concurrently populated, put it back.
            if (_concurrentDictionary.TryRemove(key, out var removedSet))
            {
                if (!removedSet.IsEmpty)
                {
                    // Another thread added to the set between our check and the remove;
                    // restore the entry so no values are lost.
                    _concurrentDictionary.TryAdd(key, removedSet);
                }
            }
        }
    }
}
