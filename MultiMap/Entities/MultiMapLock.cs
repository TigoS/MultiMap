using MultiMap.Helpers;
using MultiMap.Interfaces;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
    /// <typeparam name="TKey">The type of keys in the multi-map.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key.</typeparam>
    /// <remarks>
    /// Initializes a new instance of the <see cref="MultiMapLock{TKey, TValue}"/> class with the specified initial capacity for keys and equality comparer for keys and values.
    /// </remarks>
    /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
    /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
    /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
    [DebuggerDisplay("Keys={KeyCount}, Values={Count}")]
    public sealed class MultiMapLock<TKey, TValue>(int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer) : IMultiMap<TKey, TValue>, IDisposable
        where TKey : notnull
        where TValue : notnull
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
            ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, GetType()?.FullName ?? string.Empty);
        }

        /// <summary>
        /// Polling interval (ms) used by <see cref="EnterWriteLockCancellable"/> between
        /// <see cref="ReaderWriterLockSlim.TryEnterWriteLock"/> attempts.
        /// 20 ms gives sub-20 ms cancellation latency with negligible CPU cost.
        /// </summary>
        private const int WriteLockPollIntervalMs = 20;

        /// <summary>
        /// Acquires the write lock, aborting if <paramref name="cancellationToken"/> is cancelled.
        /// </summary>
        /// <remarks>
        /// <see cref="ReaderWriterLockSlim"/> does not expose a cancellable entry point, so this
        /// helper polls with <see cref="ReaderWriterLockSlim.TryEnterWriteLock"/> at
        /// <see cref="WriteLockPollIntervalMs"/> intervals. Cancellation is observed within at most
        /// one polling interval (~20 ms). On cancellation, <see cref="OperationCanceledException"/>
        /// is thrown and the lock is <b>not</b> held.
        /// </remarks>
        /// <param name="cancellationToken">Token to observe for cancellation.</param>
        /// <exception cref="OperationCanceledException">
        /// Thrown when <paramref name="cancellationToken"/> is cancelled before the write lock is acquired.
        /// </exception>
        private void EnterWriteLockCancellable(CancellationToken cancellationToken)
        {
            while (!_lock.TryEnterWriteLock(WriteLockPollIntervalMs))
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
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
ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, key, out bool exists);
hashset ??= new HashSet<TValue>(_valueComparer);

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
                    Guard.NotNull(item.Key, nameof(item.Key), "Sequence contains a null key.");
                    Guard.NotNull(item.Value, nameof(item.Value), "Sequence contains a null value.");

ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, item.Key, out bool exists);
hashset ??= new HashSet<TValue>(_valueComparer);

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

        /// <summary>
        /// Adds the specified key-value pair, waiting for the write lock with cancellation support.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to associate with <paramref name="key"/>.</param>
        /// <param name="cancellationToken">
        /// A token to cancel waiting for the write lock.
        /// Checked at each polling interval (~20 ms); throws <see cref="OperationCanceledException"/>
        /// if cancelled before the lock is acquired. The map is not modified on cancellation.
        /// </param>
        /// <returns><see langword="true"/> if the pair was added; <see langword="false"/> if it already existed.</returns>
        public bool Add(TKey key, TValue value, CancellationToken cancellationToken)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(value, nameof(value));

            ThrowIfDisposed();

            EnterWriteLockCancellable(cancellationToken);
            try
            {
ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, key, out bool exists);
hashset ??= new HashSet<TValue>(_valueComparer);

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

        /// <summary>
        /// Adds all values in <paramref name="values"/> under <paramref name="key"/>,
        /// waiting for the write lock with cancellation support.
        /// </summary>
        /// <param name="key">The key to add values under.</param>
        /// <param name="values">The values to add.</param>
        /// <param name="cancellationToken">
        /// A token to cancel waiting for the write lock.
        /// Checked at each polling interval (~20 ms); throws <see cref="OperationCanceledException"/>
        /// if cancelled before the lock is acquired. The map is not modified on cancellation.
        /// </param>
        /// <returns>The number of values actually added (duplicates are ignored).</returns>
        public int AddRange(TKey key, IEnumerable<TValue> values, CancellationToken cancellationToken)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(values, nameof(values));

            ThrowIfDisposed();

            EnterWriteLockCancellable(cancellationToken);
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

        /// <summary>
        /// Adds all key-value pairs in <paramref name="items"/>,
        /// waiting for the write lock with cancellation support.
        /// </summary>
        /// <param name="items">The key-value pairs to add.</param>
        /// <param name="cancellationToken">
        /// A token to cancel waiting for the write lock.
        /// Checked at each polling interval (~20 ms); throws <see cref="OperationCanceledException"/>
        /// if cancelled before the lock is acquired. The map is not modified on cancellation.
        /// </param>
        /// <returns>The number of pairs actually added (duplicates are ignored).</returns>
        public int AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items, CancellationToken cancellationToken)
        {
            Guard.NotNull(items, nameof(items));

            ThrowIfDisposed();

            EnterWriteLockCancellable(cancellationToken);
            try
            {
                int added = 0;
                foreach (var item in items)
                {
                    Guard.NotNull(item.Key, nameof(item.Key), "Sequence contains a null key.");
                    Guard.NotNull(item.Value, nameof(item.Value), "Sequence contains a null value.");

ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, item.Key, out bool exists);
hashset ??= new HashSet<TValue>(_valueComparer);

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

        /// <summary>
        /// Removes the specified key-value pair, waiting for the write lock with cancellation support.
        /// </summary>
        /// <param name="key">The key to remove the value from.</param>
        /// <param name="value">The value to remove.</param>
        /// <param name="cancellationToken">
        /// A token to cancel waiting for the write lock.
        /// Checked at each polling interval (~20 ms); throws <see cref="OperationCanceledException"/>
        /// if cancelled before the lock is acquired. The map is not modified on cancellation.
        /// </param>
        /// <returns><see langword="true"/> if the pair was found and removed; otherwise, <see langword="false"/>.</returns>
        public bool Remove(TKey key, TValue value, CancellationToken cancellationToken)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(value, nameof(value));

            ThrowIfDisposed();

            EnterWriteLockCancellable(cancellationToken);
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

        /// <summary>
        /// Removes the specified key-value pairs, waiting for the write lock with cancellation support.
        /// </summary>
        /// <param name="items">The key-value pairs to remove.</param>
        /// <param name="cancellationToken">
        /// A token to cancel waiting for the write lock.
        /// Checked at each polling interval (~20 ms); throws <see cref="OperationCanceledException"/>
        /// if cancelled before the lock is acquired. The map is not modified on cancellation.
        /// </param>
        /// <returns>The number of pairs actually removed.</returns>
        public int RemoveRange(IEnumerable<KeyValuePair<TKey, TValue>> items, CancellationToken cancellationToken)
        {
            Guard.NotNull(items, nameof(items));

            ThrowIfDisposed();

            EnterWriteLockCancellable(cancellationToken);
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

        /// <summary>
        /// Removes all values for <paramref name="key"/> that match <paramref name="predicate"/>,
        /// waiting for the write lock with cancellation support.
        /// </summary>
        /// <param name="key">The key whose values are filtered.</param>
        /// <param name="predicate">A predicate that returns <see langword="true"/> for values to remove.</param>
        /// <param name="cancellationToken">
        /// A token to cancel waiting for the write lock.
        /// Checked at each polling interval (~20 ms); throws <see cref="OperationCanceledException"/>
        /// if cancelled before the lock is acquired. The map is not modified on cancellation.
        /// </param>
        /// <returns>The number of values removed.</returns>
        public int RemoveWhere(TKey key, Predicate<TValue> predicate, CancellationToken cancellationToken)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(predicate, nameof(predicate));

            ThrowIfDisposed();

            EnterWriteLockCancellable(cancellationToken);
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

        /// <summary>
        /// Removes all values associated with <paramref name="key"/>,
        /// waiting for the write lock with cancellation support.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <param name="cancellationToken">
        /// A token to cancel waiting for the write lock.
        /// Checked at each polling interval (~20 ms); throws <see cref="OperationCanceledException"/>
        /// if cancelled before the lock is acquired. The map is not modified on cancellation.
        /// </param>
        /// <returns><see langword="true"/> if the key existed and was removed; otherwise, <see langword="false"/>.</returns>
        public bool RemoveKey(TKey key, CancellationToken cancellationToken)
        {
            Guard.NotNull(key, nameof(key));

            ThrowIfDisposed();

            EnterWriteLockCancellable(cancellationToken);
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
        /// Removes all key-value pairs from this multi-map,
        /// waiting for the write lock with cancellation support.
        /// </summary>
        /// <param name="cancellationToken">
        /// A token to cancel waiting for the write lock.
        /// Checked at each polling interval (~20 ms); throws <see cref="OperationCanceledException"/>
        /// if cancelled before the lock is acquired. The map is not modified on cancellation.
        /// </param>
        public void Clear(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            EnterWriteLockCancellable(cancellationToken);
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
ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, key, out bool exists);
hashset ??= new HashSet<TValue>(_valueComparer);

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
        /// Adds all key-value pairs from <paramref name="other"/> into this multi-map,
        /// waiting for the write lock with cancellation support.
        /// </summary>
        /// <remarks>
        /// <paramref name="other"/> is snapshotted before the write lock is acquired, so it may be
        /// the same instance or another locked collection without risk of deadlock. The entire
        /// mutation phase executes under a single write lock.
        /// </remarks>
        /// <param name="other">The multi-map whose pairs are added to this instance.</param>
        /// <param name="cancellationToken">
        /// A token to cancel waiting for the write lock.
        /// Checked at each polling interval (~20 ms); throws <see cref="OperationCanceledException"/>
        /// if cancelled before the lock is acquired. The map is not modified on cancellation.
        /// </param>
        public void Union(IMultiMap<TKey, TValue> other, CancellationToken cancellationToken)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in other.Keys)
            {
                snapshot.Add((key, other.GetOrDefault(key).ToArray()));
            }

            EnterWriteLockCancellable(cancellationToken);
            try
            {
                foreach (var (key, values) in snapshot)
                {
ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, key, out bool exists);
hashset ??= new HashSet<TValue>(_valueComparer);

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
        /// Removes all key-value pairs from this multi-map that do not exist in <paramref name="other"/>,
        /// waiting for the write lock with cancellation support.
        /// </summary>
        /// <remarks>
        /// <paramref name="other"/> is snapshotted before the write lock is acquired. The entire
        /// read-and-remove phase executes under a single write lock.
        /// </remarks>
        /// <param name="other">The multi-map that defines the pairs to keep.</param>
        /// <param name="cancellationToken">
        /// A token to cancel waiting for the write lock.
        /// Checked at each polling interval (~20 ms); throws <see cref="OperationCanceledException"/>
        /// if cancelled before the lock is acquired. The map is not modified on cancellation.
        /// </param>
        public void Intersect(IMultiMap<TKey, TValue> other, CancellationToken cancellationToken)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            var otherIndex = new Dictionary<TKey, HashSet<TValue>>();
            foreach (var key in other.Keys)
            {
                otherIndex[key] = new HashSet<TValue>(other.GetOrDefault(key));
            }

            EnterWriteLockCancellable(cancellationToken);
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
        /// Removes all key-value pairs from this multi-map that exist in <paramref name="other"/>,
        /// waiting for the write lock with cancellation support.
        /// </summary>
        /// <remarks>
        /// <paramref name="other"/> is snapshotted before the write lock is acquired. The entire
        /// mutation phase executes under a single write lock.
        /// </remarks>
        /// <param name="other">The multi-map whose pairs are removed from this instance.</param>
        /// <param name="cancellationToken">
        /// A token to cancel waiting for the write lock.
        /// Checked at each polling interval (~20 ms); throws <see cref="OperationCanceledException"/>
        /// if cancelled before the lock is acquired. The map is not modified on cancellation.
        /// </param>
        public void ExceptWith(IMultiMap<TKey, TValue> other, CancellationToken cancellationToken)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in other.Keys)
            {
                snapshot.Add((key, other.GetOrDefault(key).ToArray()));
            }

            EnterWriteLockCancellable(cancellationToken);
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
ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, key, out bool exists);
hashset ??= new HashSet<TValue>(_valueComparer);

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
        /// Modifies this multi-map to contain only pairs present in either this instance or
        /// <paramref name="other"/>, but not both, waiting for the write lock with cancellation support.
        /// </summary>
        /// <remarks>
        /// <paramref name="other"/> is snapshotted before the write lock is acquired. Classification
        /// and all mutations execute under a single write lock.
        /// </remarks>
        /// <param name="other">The multi-map to compare against.</param>
        /// <param name="cancellationToken">
        /// A token to cancel waiting for the write lock.
        /// Checked at each polling interval (~20 ms); throws <see cref="OperationCanceledException"/>
        /// if cancelled before the lock is acquired. The map is not modified on cancellation.
        /// </param>
        public void SymmetricExceptWith(IMultiMap<TKey, TValue> other, CancellationToken cancellationToken)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in other.Keys)
            {
                snapshot.Add((key, other.GetOrDefault(key).ToArray()));
            }

            EnterWriteLockCancellable(cancellationToken);
            try
            {
                foreach (var (key, values) in snapshot)
                {
ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, key, out bool exists);
hashset ??= new HashSet<TValue>(_valueComparer);

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

            if (ReferenceEquals(this, other))
                return true;

            var otherIndex = new Dictionary<TKey, HashSet<TValue>>();
            foreach (var key in other.Keys)
            {
                var values = other.GetOrDefault(key).ToArray();
                otherIndex[key] = new HashSet<TValue>(values, _valueComparer);
            }

            _lock.EnterReadLock();
            try
            {
                foreach (var kvp in _dictionary)
                {
                    if (!otherIndex.TryGetValue(kvp.Key, out var otherSet) || otherSet.Count == 0)
                        return false;

                    foreach (var value in kvp.Value)
                    {
                        if (!otherSet.Contains(value))
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

            if (ReferenceEquals(this, other))
                return true;

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

            if (ReferenceEquals(this, other))
                return Count > 0;

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

            if (ReferenceEquals(this, other))
                return true;

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
