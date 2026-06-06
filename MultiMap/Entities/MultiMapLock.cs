using MultiMap.Helpers;
using MultiMap.Interfaces;
using System.Collections;
#if NET6_0_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace MultiMap.Entities
{
    /// <summary>
    /// Provides a thread-safe multi-map collection that associates each key with a set of values, allowing concurrent access and modification.
    /// </summary>
    /// <remarks>
    /// This class uses internal locking to ensure safe concurrent operations.
    /// It is suitable for scenarios where multiple threads need to add, remove, or query key-value associations without external synchronization.
    /// Dispose the instance when no longer needed to release resources.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multi-map. Must be non-nullable and implement <see cref="IEquatable{TKey}"/>.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-nullable and implement <see cref="IEquatable{TValue}"/>.</typeparam>
    /// <remarks>
    /// Initializes a new instance of the <see cref="MultiMapLock{TKey, TValue}"/> class with the specified initial capacity for keys and equality comparer for keys and values.
    /// </remarks>
    /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
    /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
    /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
    public sealed class MultiMapLock<TKey, TValue>(int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer) : IMultiMap<TKey, TValue>, IDisposable
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull, IEquatable<TValue>
    {
        private readonly Dictionary<TKey, HashSet<TValue>> _dictionary = capacity > 0
                ? new Dictionary<TKey, HashSet<TValue>>(capacity, keyComparer)
                : new Dictionary<TKey, HashSet<TValue>>(keyComparer);
        private readonly ReaderWriterLockSlim _lock = new();
        private readonly IEqualityComparer<TValue>? _valueComparer = valueComparer;
        private int _count;
        private int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapLock{TKey, TValue}"/> class.
        /// </summary>
        public MultiMapLock() : this(0, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapLock{TKey, TValue}"/> class with the specified equality comparer for keys.
        /// </summary>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapLock(IEqualityComparer<TKey>? keyComparer) : this(0, keyComparer, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapLock{TKey, TValue}"/> class with the specified equality comparer for values.
        /// </summary>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapLock(IEqualityComparer<TValue>? valueComparer) : this(0, null, valueComparer) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapLock{TKey, TValue}"/> class with the specified initial capacity for keys.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        public MultiMapLock(int capacity) : this(capacity, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapLock{TKey, TValue}"/> class with the specified initial capacity for keys and equality comparer for keys.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapLock(int capacity, IEqualityComparer<TKey>? keyComparer) : this(capacity, keyComparer, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapLock{TKey, TValue}"/> class with the specified initial capacity for keys and equality comparer for values.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapLock(int capacity, IEqualityComparer<TValue>? valueComparer) : this(capacity, null, valueComparer) { }

        private void ThrowIfDisposed()
        {
#if NET6_0_OR_GREATER
            ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, GetType()?.FullName ?? string.Empty);
#else
            if (Volatile.Read(ref _disposed) != 0)
                throw new ObjectDisposedException(GetType().FullName);
#endif
        }

        /// <inheritdoc/>
        public bool Add(TKey key, TValue value)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(value, nameof(value));

            ThrowIfDisposed();

            _lock.EnterWriteLock();
            try
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

                if (hashset.Add(value))
                {
                    _count++;
                    return true;
                }

                return false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public int AddRange(TKey key, IEnumerable<TValue> values)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(values, nameof(values));

            ThrowIfDisposed();

            _lock.EnterWriteLock();
            try
            {
                bool exists = _dictionary.TryGetValue(key, out var hashset);
                if (!exists)
                    hashset = new HashSet<TValue>(_valueComparer);

                int added = 0;
                foreach (var value in values)
                {
                    Guard.NotNull(value, nameof(values), "Sequence contains a null value.");

                    if (hashset!.Add(value))
                    {
                        _count++;
                        added++;
                    }
                }

                if (!exists && added > 0)
                    _dictionary[key] = hashset!;

                return added;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public int AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            Guard.NotNull(items, nameof(items));

            ThrowIfDisposed();

            _lock.EnterWriteLock();
            try
            {
                int added = 0;
                foreach (var item in items)
                {
                    Guard.NotNull(item, nameof(items), "Sequence contains a null item.");
                    Guard.NotNull(item.Key, nameof(items), "Sequence contains a null key.");
                    Guard.NotNull(item.Value, nameof(items), "Sequence contains a null value.");

#if NET6_0_OR_GREATER
                    ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, item.Key, out bool exists);
                    hashset ??= new HashSet<TValue>(_valueComparer);
#else
                    if (!_dictionary.TryGetValue(item.Key, out var hashset))
                    {
                        hashset = new HashSet<TValue>(_valueComparer);
                        _dictionary[item.Key] = hashset;
                    }
#endif

                    if (hashset.Add(item.Value))
                    {
                        _count++;
                        added++;
                    }
                }

                return added;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> Get(TKey key)
        {
            Guard.NotNull(key, nameof(key));

            ThrowIfDisposed();

            _lock.EnterReadLock();
            try
            {
                if (_dictionary.TryGetValue(key, out var hashset))
                    return hashset.ToArray();

                throw new KeyNotFoundException($"The key '{key}' was not found in the multimap.");
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> GetOrDefault(TKey key)
        {
            Guard.NotNull(key, nameof(key));

            ThrowIfDisposed();

            _lock.EnterReadLock();
            try
            {
                if (_dictionary.TryGetValue(key, out var hashset))
                    return hashset.ToArray();

                return [];
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public bool TryGet(TKey key, out IEnumerable<TValue> values)
        {
            Guard.NotNull(key, nameof(key));

            ThrowIfDisposed();

            _lock.EnterReadLock();
            try
            {
                bool result = _dictionary.TryGetValue(key, out var hashset);

                values = result ? hashset?.ToArray() ?? [] : [];

                return result;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public bool Remove(TKey key, TValue value)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(value, nameof(value));

            ThrowIfDisposed();

            _lock.EnterWriteLock();
            try
            {
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
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public int RemoveRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            Guard.NotNull(items, nameof(items));

            ThrowIfDisposed();

            _lock.EnterWriteLock();
            try
            {
                int removedCount = 0;
                foreach (var item in items)
                {
                    if (_dictionary.TryGetValue(item.Key, out var hashset))
                    {
                        if (hashset.Remove(item.Value))
                        {
                            _count--;
                            removedCount++;
                            if (hashset.Count == 0)
                                _dictionary.Remove(item.Key);
                        }
                    }
                }
                return removedCount;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public int RemoveWhere(TKey key, Predicate<TValue> predicate)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(predicate, nameof(predicate));

            ThrowIfDisposed();

            _lock.EnterWriteLock();
            try
            {
                if (!_dictionary.TryGetValue(key, out var hashset))
                    return 0;

                int removedCount = hashset.RemoveWhere(predicate);
                _count -= removedCount;

                if (hashset.Count == 0)
                    _dictionary.Remove(key);

                return removedCount;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public bool RemoveKey(TKey key)
        {
            Guard.NotNull(key, nameof(key));

            ThrowIfDisposed();

            _lock.EnterWriteLock();
            try
            {
                if (_dictionary.TryGetValue(key, out var hashset))
                {
                    _count -= hashset.Count;
                    return _dictionary.Remove(key);
                }

                return false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public bool ContainsKey(TKey key)
        {
            Guard.NotNull(key, nameof(key));

            ThrowIfDisposed();

            _lock.EnterReadLock();
            try
            {
                return _dictionary.ContainsKey(key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public bool Contains(TKey key, TValue value)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(value, nameof(value));

            ThrowIfDisposed();

            _lock.EnterReadLock();
            try
            {
                return _dictionary.TryGetValue(key, out var hashset) && hashset.Contains(value);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public int Count
        {
            get
            {
                ThrowIfDisposed();
                _lock.EnterReadLock();
                try
                {
                    return _count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<TKey> Keys
        {
            get
            {
                ThrowIfDisposed();
                _lock.EnterReadLock();
                try
                {
                    return _dictionary.Keys.ToArray();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc/>
        public int KeyCount
        {
            get
            {
                ThrowIfDisposed();
                _lock.EnterReadLock();
                try
                {
                    return _dictionary.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> Values
        {
            get
            {
                ThrowIfDisposed();
                _lock.EnterReadLock();
                try
                {
                    var result = new TValue[_count];
                    var index = 0;
                    foreach (var hashset in _dictionary.Values)
                    {
                        foreach (var value in hashset)
                        {
                            result[index++] = value;
                        }
                    }
                    return result;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc/>
        public int GetValuesCount(TKey key)
        {
            Guard.NotNull(key, nameof(key));

            ThrowIfDisposed();

            _lock.EnterReadLock();
            try
            {
                return _dictionary.TryGetValue(key, out var hashset) ? hashset.Count : 0;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> this[TKey key] => Get(key);

        /// <inheritdoc/>
        public void Clear()
        {
            ThrowIfDisposed();
            _lock.EnterWriteLock();
            try
            {
                _dictionary.Clear();
                _count = 0;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Atomically adds all key-value pairs from <paramref name="other"/> into this multi-map.
        /// </summary>
        /// <remarks>
        /// The data from <paramref name="other"/> is snapshotted before the write lock is acquired,
        /// so <paramref name="other"/> may be the same instance or another locked collection without
        /// risk of deadlock. The entire mutation phase executes under a single write lock, guaranteeing
        /// that no concurrent reader or writer can observe a partial union.
        /// </remarks>
        /// <param name="other">The multi-map whose pairs are added to this instance.</param>
        public void Union(IMultiMap<TKey, TValue> other)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in other.Keys)
            {
                snapshot.Add((key, other.GetOrDefault(key).ToArray()));
            }

            _lock.EnterWriteLock();
            try
            {
                foreach (var (key, values) in snapshot)
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

                    foreach (var value in values)
                    {
                        if (hashset.Add(value))
                            _count++;
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Atomically removes all key-value pairs from this multi-map that do not exist in <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// The membership of <paramref name="other"/> is snapshotted into a dictionary of hash sets
        /// before the write lock is acquired, avoiding deadlock when <paramref name="other"/> is a
        /// locked collection. The entire read-and-remove phase executes under a single write lock,
        /// so concurrent operations cannot insert values that bypass the intersect filter.
        /// </remarks>
        /// <param name="other">The multi-map that defines the pairs to keep.</param>
        public void Intersect(IMultiMap<TKey, TValue> other)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            var otherIndex = new Dictionary<TKey, HashSet<TValue>>();
            foreach (var key in other.Keys)
            {
                otherIndex[key] = new HashSet<TValue>(other.GetOrDefault(key));
            }

            _lock.EnterWriteLock();
            try
            {
                var keysToRemove = new List<TKey>();

                foreach (var kvp in _dictionary)
                {
                    if (!otherIndex.TryGetValue(kvp.Key, out var otherValues))
                    {
                        _count -= kvp.Value.Count;
                        keysToRemove.Add(kvp.Key);
                        continue;
                    }

                    int removed = kvp.Value.RemoveWhere(v => !otherValues.Contains(v));
                    _count -= removed;

                    if (kvp.Value.Count == 0)
                        keysToRemove.Add(kvp.Key);
                }

                foreach (var key in keysToRemove)
                {
                    _dictionary.Remove(key);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Atomically removes all key-value pairs from this multi-map that exist in <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// The data from <paramref name="other"/> is snapshotted before the write lock is acquired,
        /// so <paramref name="other"/> may be the same instance or another locked collection without
        /// risk of deadlock. The entire mutation phase executes under a single write lock, guaranteeing
        /// that no concurrent reader or writer can observe a partial removal.
        /// </remarks>
        /// <param name="other">The multi-map whose pairs are removed from this instance.</param>
        public void ExceptWith(IMultiMap<TKey, TValue> other)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in other.Keys)
            {
                snapshot.Add((key, other.GetOrDefault(key).ToArray()));
            }

            _lock.EnterWriteLock();
            try
            {
                foreach (var (key, values) in snapshot)
                {
                    if (!_dictionary.TryGetValue(key, out var hashset))
                        continue;

                    foreach (var value in values)
                    {
                        if (hashset.Remove(value))
                            _count--;
                    }

                    if (hashset.Count == 0)
                        _dictionary.Remove(key);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Atomically modifies this multi-map to contain only pairs present in either this instance
        /// or <paramref name="other"/>, but not both.
        /// </summary>
        /// <remarks>
        /// The data from <paramref name="other"/> is snapshotted before the write lock is acquired,
        /// so <paramref name="other"/> may be the same instance or another locked collection without
        /// risk of deadlock. Classification (common vs. unique) and all mutations execute under a
        /// single write lock, guaranteeing full atomicity.
        /// </remarks>
        /// <param name="other">The multi-map to compare against.</param>
        public void SymmetricExceptWith(IMultiMap<TKey, TValue> other)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in other.Keys)
            {
                snapshot.Add((key, other.GetOrDefault(key).ToArray()));
            }

            _lock.EnterWriteLock();
            try
            {
                foreach (var (key, values) in snapshot)
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

                    foreach (var value in values)
                    {
                        if (!hashset.Remove(value))
                        {
                            hashset.Add(value);
                            _count++;
                        }
                        else
                        {
                            _count--;
                        }
                    }

                    if (hashset.Count == 0)
                        _dictionary.Remove(key);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Atomically determines whether this multi-map is a subset of <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// The data from <paramref name="other"/> is snapshotted before the read lock is acquired,
        /// so <paramref name="other"/> may be the same instance or another locked collection without
        /// risk of deadlock. The entire comparison executes under a single read lock, guaranteeing
        /// consistent results.
        /// </remarks>
        /// <param name="other">The multi-map to compare against.</param>
        /// <returns><see langword="true"/> if every key-value pair in this instance exists in <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public bool IsSubsetOf(IMultiMap<TKey, TValue> other)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in other.Keys)
            {
                snapshot.Add((key, other.GetOrDefault(key).ToArray()));
            }

            _lock.EnterReadLock();
            try
            {
                foreach (var kvp in _dictionary)
                {
                    var otherSet = snapshot.FirstOrDefault(s => s.Key.Equals(kvp.Key)).Values;
                    if (otherSet == null || otherSet.Length == 0)
                        return false;

                    var otherHashSet = new HashSet<TValue>(otherSet, _valueComparer);
                    foreach (var value in kvp.Value)
                    {
                        if (!otherHashSet.Contains(value))
                            return false;
                    }
                }

                return true;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Atomically determines whether this multi-map is a superset of <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// The data from <paramref name="other"/> is snapshotted before the read lock is acquired,
        /// so <paramref name="other"/> may be the same instance or another locked collection without
        /// risk of deadlock. The entire comparison executes under a single read lock, guaranteeing
        /// consistent results.
        /// </remarks>
        /// <param name="other">The multi-map to compare against.</param>
        /// <returns><see langword="true"/> if every key-value pair in <paramref name="other"/> exists in this instance; otherwise, <see langword="false"/>.</returns>
        public bool IsSupersetOf(IMultiMap<TKey, TValue> other)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in other.Keys)
            {
                snapshot.Add((key, other.GetOrDefault(key).ToArray()));
            }

            _lock.EnterReadLock();
            try
            {
                foreach (var (key, values) in snapshot)
                {
                    if (!_dictionary.TryGetValue(key, out var thisSet))
                        return false;

                    foreach (var value in values)
                    {
                        if (!thisSet.Contains(value))
                            return false;
                    }
                }

                return true;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Atomically determines whether this multi-map and <paramref name="other"/> share at least one key-value pair.
        /// </summary>
        /// <remarks>
        /// The data from <paramref name="other"/> is snapshotted before the read lock is acquired,
        /// so <paramref name="other"/> may be the same instance or another locked collection without
        /// risk of deadlock. The entire comparison executes under a single read lock, guaranteeing
        /// consistent results.
        /// </remarks>
        /// <param name="other">The multi-map to compare against.</param>
        /// <returns><see langword="true"/> if at least one key-value pair exists in both multimaps; otherwise, <see langword="false"/>.</returns>
        public bool Overlaps(IMultiMap<TKey, TValue> other)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in other.Keys)
            {
                snapshot.Add((key, other.GetOrDefault(key).ToArray()));
            }

            _lock.EnterReadLock();
            try
            {
                foreach (var (key, values) in snapshot)
                {
                    if (_dictionary.TryGetValue(key, out var thisSet))
                    {
                        foreach (var value in values)
                        {
                            if (thisSet.Contains(value))
                                return true;
                        }
                    }
                }

                return false;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Atomically determines whether this multi-map and <paramref name="other"/> contain the same key-value pairs.
        /// </summary>
        /// <remarks>
        /// The data from <paramref name="other"/> is snapshotted before the read lock is acquired,
        /// so <paramref name="other"/> may be the same instance or another locked collection without
        /// risk of deadlock. The entire comparison executes under a single read lock, guaranteeing
        /// consistent results.
        /// </remarks>
        /// <param name="other">The multi-map to compare against.</param>
        /// <returns><see langword="true"/> if both multimaps contain exactly the same key-value pairs; otherwise, <see langword="false"/>.</returns>
        public bool SetEquals(IMultiMap<TKey, TValue> other)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            if (other.Count != Count || other.KeyCount != KeyCount)
                return false;

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in other.Keys)
            {
                snapshot.Add((key, other.GetOrDefault(key).ToArray()));
            }

            _lock.EnterReadLock();
            try
            {
                if (_dictionary.Count != snapshot.Count)
                    return false;

                foreach (var (key, values) in snapshot)
                {
                    if (!_dictionary.TryGetValue(key, out var thisSet))
                        return false;

                    if (thisSet.Count != values.Length)
                        return false;

                    var otherHashSet = new HashSet<TValue>(values, _valueComparer);
                    if (!thisSet.SetEquals(otherHashSet))
                        return false;
                }

                return true;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            ThrowIfDisposed();
            List<KeyValuePair<TKey, TValue>> snapshot;

            _lock.EnterReadLock();
            try
            {
                snapshot = new List<KeyValuePair<TKey, TValue>>(_count);
                foreach (var kvp in _dictionary)
                {
                    foreach (var value in kvp.Value)
                    {
                        snapshot.Add(new KeyValuePair<TKey, TValue>(kvp.Key, value));
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return snapshot.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as MultiMapLock<TKey, TValue>);

        /// <inheritdoc/>
        public bool Equals(IReadOnlySimpleMultiMap<TKey, TValue>? other) => Equals(other as IReadOnlyMultiMap<TKey, TValue>);

        /// <inheritdoc/>
        public bool Equals(IReadOnlyMultiMap<TKey, TValue>? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            ThrowIfDisposed();

            _lock.EnterReadLock();
            try
            {
                ThrowIfDisposed();

                if (_dictionary.Count != other.KeyCount || _count != other.Count)
                    return false;

                foreach (var key in _dictionary.Keys)
                {
                    var thisValues = _dictionary[key];

                    if (!other.ContainsKey(key) || thisValues.Count != other.GetValuesCount(key))
                        return false;

                    foreach (var value in thisValues)
                    {
                        if (!other.Contains(key, value))
                            return false;
                    }
                }

                return true;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            ThrowIfDisposed();
            _lock.EnterReadLock();
            try
            {
                return MultiMapHelper.ComputeUnorderedHash<TKey, TValue, HashSet<TValue>>(_dictionary, _dictionary.Comparer, _valueComparer);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Releases the resources used by the <see cref="MultiMapLock{TKey, TValue}"/> instance.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            try
            {
                _dictionary.Clear();
                _count = 0;
            }
            finally
            {
                _lock.Dispose();
            }
        }
    }
}
