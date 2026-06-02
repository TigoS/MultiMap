using MultiMap.Helpers;
using System.Runtime.CompilerServices;
using MultiMap.Interfaces;

namespace MultiMap.Entities
{
    /// <summary>
    /// Represents an asynchronous multi-map collection that associates each key with a set of unique values.
    /// Provides thread-safe operations for adding, removing, and retrieving values by key, as well as asynchronous enumeration of all key-value pairs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// MultiMapAsync is designed for concurrent scenarios where asynchronous access and modification of the collection are required.
    /// All operations are thread-safe and use internal locking to ensure consistency.
    /// Enumerating the collection produces a snapshot of the current state, so changes made during enumeration are not reflected.
    /// This class is useful for managing associations where each key can have multiple distinct values, such as grouping or indexing tasks in asynchronous workflows.
    /// </para>
    /// <para><b>Locking protocol (internal readers-writer protocol)</b></para>
    /// <para>
    /// Two <see cref="SemaphoreSlim"/> instances implement a custom readers-writer protocol:
    /// </para>
    /// <list type="bullet">
    ///   <item><term><c>_writeLock</c> (1, 1)</term>
    ///     <description>
    ///       Exclusive permit held for the entire duration of every mutating operation
    ///       (Add, AddRange, Remove*, Clear).  The first reader to enter also acquires
    ///       this permit and holds it until the last concurrent reader exits, which
    ///       prevents any writer from entering while readers are active.
    ///     </description>
    ///   </item>
    ///   <item><term><c>_readerLock</c> (1, 1)</term>
    ///     <description>
    ///       Guards the <c>_activeReaders</c> counter.  It is held only for the brief
    ///       critical section of incrementing or decrementing the counter, so many
    ///       readers can proceed concurrently once their count is registered.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// <b>Invariants:</b>
    /// </para>
    /// <list type="number">
    ///   <item>A writer must acquire <c>_writeLock</c> exclusively; it will block until all active readers have exited (i.e. <c>_activeReaders</c> drops to 0 and <c>_writeLock</c> is released by the last reader).</item>
    ///   <item>Readers must wait for any active writer: when <c>_activeReaders == 0</c> the first reader acquires <c>_writeLock</c>; if a writer currently holds it the reader blocks until the writer releases it.</item>
    ///   <item>While at least one reader is active (<c>_activeReaders &gt; 0</c>), <c>_writeLock</c> remains held, so writers queue behind all current readers.</item>
    ///   <item>Each operation has a fast path (non-blocking <c>Wait(0)</c>) that avoids allocating a <c>Task</c>/continuation; falling back to the <c>SlowAsync</c> variant only when contention is detected.</item>
    /// </list>
    /// <para>
    /// Because every read acquires the shared <c>_writeLock</c>, writers can be starved under sustained high-frequency concurrent reads.
    /// Prefer <see cref="MultiMapLock{TKey,TValue}"/> (which uses <see cref="System.Threading.ReaderWriterLockSlim"/>) for read-heavy workloads with latency-sensitive writers.
    /// </para>
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multi-map. Must be non-nullable and implement <see cref="IEquatable{TKey}"/>.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-nullable and implement <see cref="IEquatable{TValue}"/>.</typeparam>
    /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
    /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
    /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
    public sealed partial class MultiMapAsync<TKey, TValue>(int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer) : IMultiMapAsync<TKey, TValue>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull, IEquatable<TValue>
    {
        private readonly Dictionary<TKey, HashSet<TValue>> _dictionary = capacity > 0
                ? new Dictionary<TKey, HashSet<TValue>>(capacity, keyComparer)
                : new Dictionary<TKey, HashSet<TValue>>(keyComparer);
        /// <summary>Exclusive write lock – held by the single active writer.</summary>
        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);
        /// <summary>Guards the <see cref="_activeReaders"/> counter; held only for the duration of an increment/decrement.</summary>
        private readonly SemaphoreSlim _readerLock = new SemaphoreSlim(1, 1);
        private readonly IEqualityComparer<TValue>? _valueComparer = valueComparer;
        private int _activeReaders;
        private int _count;
        private int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapAsync{TKey, TValue}"/> class.
        /// </summary>
        public MultiMapAsync() : this(0, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapAsync{TKey, TValue}"/> class with the specified equality comparer for keys.
        /// </summary>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapAsync(IEqualityComparer<TKey>? keyComparer) : this(0, keyComparer, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapAsync{TKey, TValue}"/> class with the specified equality comparer for values.
        /// </summary>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapAsync(IEqualityComparer<TValue>? valueComparer) : this(0, null, valueComparer) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapAsync{TKey, TValue}"/> class with the specified initial capacity for keys.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        public MultiMapAsync(int capacity) : this(capacity, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapAsync{TKey, TValue}"/> class with the specified initial capacity for keys and equality comparer for keys.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapAsync(int capacity, IEqualityComparer<TKey>? keyComparer) : this(capacity, keyComparer, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapAsync{TKey, TValue}"/> class with the specified initial capacity for keys and equality comparer for values.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapAsync(int capacity, IEqualityComparer<TValue>? valueComparer) : this(capacity, null, valueComparer) { }

        /// <inheritdoc/>
        public ValueTask<bool> AddAsync(TKey key, TValue value, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));

            ThrowIfDisposed();
            Task waitTask = _writeLock.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<bool>(AddCore(key, value));
                }
                finally
                {
                    _writeLock.Release();
                }
            }
            return AddSlowAsync(waitTask, key, value);
        }

        /// <inheritdoc/>
        public ValueTask<int> AddRangeAsync(TKey key, IEnumerable<TValue> values, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (values is null) throw new ArgumentNullException(nameof(values));

            ThrowIfDisposed();
            Task waitTask = _writeLock.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<int>(AddRangeCore(key, values));
                }
                finally
                {
                    _writeLock.Release();
                }
            }
            return AddRangeSlowAsync(waitTask, key, values);
        }

        /// <inheritdoc/>
        public ValueTask<int> AddRangeAsync(IEnumerable<KeyValuePair<TKey, TValue>> items, CancellationToken cancellationToken = default)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));

            ThrowIfDisposed();
            Task waitTask = _writeLock.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<int>(AddRangeCore(items));
                }
                finally
                {
                    _writeLock.Release();
                }
            }
            return AddRangeSlowAsync(waitTask, items);
        }

        /// <inheritdoc/>
        public ValueTask<IEnumerable<TValue>> GetAsync(TKey key, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (TryEnterReadLockSync())
            {
                try
                {
                    return new ValueTask<IEnumerable<TValue>>(GetCore(key));
                }
                finally
                {
                    ExitReadLock();
                }
            }
            return GetSlowAsync(key, cancellationToken);
        }

        /// <inheritdoc/>
        public ValueTask<IEnumerable<TValue>> GetOrDefaultAsync(TKey key, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (TryEnterReadLockSync())
            {
                try
                {
                    return new ValueTask<IEnumerable<TValue>>(GetOrDefaultCore(key));
                }
                finally
                {
                    ExitReadLock();
                }
            }
            return GetOrDefaultSlowAsync(key, cancellationToken);
        }

        /// <inheritdoc/>
        public ValueTask<(bool found, IEnumerable<TValue> values)> TryGetAsync(TKey key, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (TryEnterReadLockSync())
            {
                try
                {
                    return new ValueTask<(bool found, IEnumerable<TValue> values)>(TryGetCore(key));
                }
                finally
                {
                    ExitReadLock();
                }
            }
            return TryGetSlowAsync(key, cancellationToken);
        }

        /// <inheritdoc/>
        public ValueTask<bool> RemoveAsync(TKey key, TValue value, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));

            ThrowIfDisposed();
            Task waitTask = _writeLock.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<bool>(RemoveCore(key, value));
                }
                finally
                {
                    _writeLock.Release();
                }
            }
            return RemoveSlowAsync(waitTask, key, value);
        }

        /// <inheritdoc/>
        public ValueTask<int> RemoveRangeAsync(IEnumerable<KeyValuePair<TKey, TValue>> items, CancellationToken cancellationToken = default)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));

            ThrowIfDisposed();
            Task waitTask = _writeLock.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<int>(RemoveRangeCore(items));
                }
                finally
                {
                    _writeLock.Release();
                }
            }
            return RemoveRangeSlowAsync(waitTask, items);
        }

        /// <inheritdoc/>
        public ValueTask<int> RemoveWhereAsync(TKey key, Predicate<TValue> predicate, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            ThrowIfDisposed();
            Task waitTask = _writeLock.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<int>(RemoveWhereCore(key, predicate));
                }
                finally
                {
                    _writeLock.Release();
                }
            }
            return RemoveWhereSlowAsync(waitTask, key, predicate);
        }

        /// <inheritdoc/>
        public ValueTask<bool> RemoveKeyAsync(TKey key, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            ThrowIfDisposed();
            Task waitTask = _writeLock.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<bool>(RemoveKeyCore(key));
                }
                finally
                {
                    _writeLock.Release();
                }
            }
            return RemoveKeySlowAsync(waitTask, key);
        }

        /// <inheritdoc/>
        public ValueTask<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (TryEnterReadLockSync())
            {
                try
                {
                    return new ValueTask<bool>(_dictionary.ContainsKey(key));
                }
                finally
                {
                    ExitReadLock();
                }
            }
            return ContainsKeySlowAsync(key, cancellationToken);
        }

        /// <inheritdoc/>
        public ValueTask<bool> ContainsAsync(TKey key, TValue value, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));

            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (TryEnterReadLockSync())
            {
                try
                {
                    return new ValueTask<bool>(
                        _dictionary.TryGetValue(key, out var hashset) && hashset.Contains(value));
                }
                finally
                {
                    ExitReadLock();
                }
            }
            return ContainsSlowAsync(key, value, cancellationToken);
        }

        /// <inheritdoc/>
        public ValueTask<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (TryEnterReadLockSync())
            {
                try
                {
                    return new ValueTask<int>(_count);
                }
                finally
                {
                    ExitReadLock();
                }
            }
            return GetCountSlowAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public ValueTask<IEnumerable<TKey>> GetKeysAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (TryEnterReadLockSync())
            {
                try
                {
                    return new ValueTask<IEnumerable<TKey>>(_dictionary.Keys.ToArray());
                }
                finally
                {
                    ExitReadLock();
                }
            }
            return GetKeysSlowAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public ValueTask<int> GetKeyCountAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (TryEnterReadLockSync())
            {
                try
                {
                    return new ValueTask<int>(_dictionary.Count);
                }
                finally
                {
                    ExitReadLock();
                }
            }
            return GetKeyCountSlowAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public ValueTask<IEnumerable<TValue>> GetValuesAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (TryEnterReadLockSync())
            {
                try
                {
                    return new ValueTask<IEnumerable<TValue>>(GetValuesCore());
                }
                finally
                {
                    ExitReadLock();
                }
            }
            return GetValuesSlowAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public ValueTask<int> GetValuesCountAsync(TKey key, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (TryEnterReadLockSync())
            {
                try
                {
                    return new ValueTask<int>(_dictionary.TryGetValue(key, out var hashset) ? hashset.Count : 0);
                }
                finally
                {
                    ExitReadLock();
                }
            }
            return GetValuesCountSlowAsync(key, cancellationToken);
        }

        /// <inheritdoc/>
        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            Task waitTask = _writeLock.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    _dictionary.Clear();
                    _count = 0;
                    return Task.CompletedTask;
                }
                finally
                {
                    _writeLock.Release();
                }
            }
            return ClearSlowAsync(waitTask);
        }

        /// <summary>
        /// Atomically adds all key-value pairs from <paramref name="other"/> into this multi-map.
        /// </summary>
        /// <remarks>
        /// The data from <paramref name="other"/> is snapshotted via its async interface before the
        /// semaphore is acquired, so <paramref name="other"/> may be the same instance or another
        /// locked collection without risk of deadlock. The entire mutation phase executes under a
        /// single semaphore hold, guaranteeing that no concurrent caller can observe a partial union.
        /// </remarks>
        /// <param name="other">The multi-map whose pairs are added to this instance.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        public async Task UnionAsync(IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
        {
            if (other is null) throw new ArgumentNullException(nameof(other));

            ThrowIfDisposed();
            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in await other.GetKeysAsync(cancellationToken).ConfigureAwait(false))
            {
                snapshot.Add((key, (await other.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false)).ToArray()));
            }

            await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                UnionCore(snapshot);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        /// <summary>
        /// Atomically removes all key-value pairs from this multi-map that do not exist in <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// The membership of <paramref name="other"/> is snapshotted into a dictionary of hash sets
        /// via its async interface before the semaphore is acquired, avoiding deadlock when
        /// <paramref name="other"/> is a locked collection. The entire read-and-remove phase executes
        /// under a single semaphore hold, so concurrent operations cannot insert values that bypass
        /// the intersect filter.
        /// </remarks>
        /// <param name="other">The multi-map that defines the pairs to keep.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        public async Task IntersectAsync(IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
        {
            if (other is null) throw new ArgumentNullException(nameof(other));

            ThrowIfDisposed();
            var otherIndex = new Dictionary<TKey, HashSet<TValue>>();
            foreach (var key in await other.GetKeysAsync(cancellationToken).ConfigureAwait(false))
            {
                otherIndex[key] = new HashSet<TValue>(await other.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false));
            }

            await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                IntersectCore(otherIndex);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        /// <summary>
        /// Atomically removes all key-value pairs from this multi-map that exist in <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// The data from <paramref name="other"/> is snapshotted via its async interface before the
        /// semaphore is acquired, so <paramref name="other"/> may be the same instance or another
        /// locked collection without risk of deadlock. The entire mutation phase executes under a
        /// single semaphore hold, guaranteeing that no concurrent caller can observe a partial removal.
        /// </remarks>
        /// <param name="other">The multi-map whose pairs are removed from this instance.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        public async Task ExceptWithAsync(IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
        {
            if (other is null) throw new ArgumentNullException(nameof(other));

            ThrowIfDisposed();
            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in await other.GetKeysAsync(cancellationToken).ConfigureAwait(false))
            {
                snapshot.Add((key, (await other.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false)).ToArray()));
            }

            await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                ExceptWithCore(snapshot);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        /// <summary>
        /// Atomically modifies this multi-map to contain only pairs present in either this instance
        /// or <paramref name="other"/>, but not both.
        /// </summary>
        /// <remarks>
        /// The data from <paramref name="other"/> is snapshotted via its async interface before the
        /// semaphore is acquired, so <paramref name="other"/> may be the same instance or another
        /// locked collection without risk of deadlock. Classification and all mutations execute under
        /// a single semaphore hold, guaranteeing full atomicity.
        /// </remarks>
        /// <param name="other">The multi-map to compare against.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        public async Task SymmetricExceptWithAsync(IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
        {
            if (other is null) throw new ArgumentNullException(nameof(other));

            ThrowIfDisposed();
            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in await other.GetKeysAsync(cancellationToken).ConfigureAwait(false))
            {
                snapshot.Add((key, (await other.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false)).ToArray()));
            }

            await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                SymmetricExceptWithCore(snapshot);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        /// <summary>
        /// Returns an asynchronous enumerator that iterates over a snapshot of all key-value pairs in the multi-map.
        /// Changes made to the collection during enumeration are not reflected.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous enumeration.</param>
        /// <returns>
        /// An <see cref="IAsyncEnumerator{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/> representing all entries in the multi-map.
        /// </returns>
        public async IAsyncEnumerator<KeyValuePair<TKey, TValue>> GetAsyncEnumerator(
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            List<KeyValuePair<TKey, TValue>> snapshot;

            await EnterReadLockAsync(cancellationToken).ConfigureAwait(false);
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
                ExitReadLock();
            }

            foreach (var pair in snapshot)
            {
                yield return pair;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            DisposeCore();
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            DisposeCore();

            return default;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is not IReadOnlyMultiMapAsync<TKey, TValue> other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (SynchronizationContext.Current != null)
                throw new InvalidOperationException(
                    "Equals cannot be called synchronously when a SynchronizationContext is present. " +
                    "Use EqualsAsync instead to avoid deadlocks.");

            return Equals(other);
}

        /// <inheritdoc/>
        public bool Equals(IReadOnlyMultiMapAsync<TKey, TValue>? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            ThrowIfDisposed();

            // Fast path: both sides are MultiMapAsync — acquire both semaphores in a
            // consistent order to avoid deadlock, then compare under lock.
            if (other is MultiMapAsync<TKey, TValue> concreteOther)
            {
                concreteOther.ThrowIfDisposed();

                var first  = RuntimeHelpers.GetHashCode(this) <= RuntimeHelpers.GetHashCode(concreteOther) ? this : concreteOther;
                var second = ReferenceEquals(first, this) ? concreteOther : this;

                Dictionary<TKey, HashSet<TValue>> thisSnapshot;
                Dictionary<TKey, HashSet<TValue>> otherSnapshot;

                first.EnterReadLockSync();
                try
                {
                    second.EnterReadLockSync();
                    try
                    {
                        if (Volatile.Read(ref _count) != Volatile.Read(ref concreteOther._count) ||
                            _dictionary.Count != concreteOther._dictionary.Count)
                            return false;

                        thisSnapshot  = _dictionary.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TValue>(kvp.Value));
                        otherSnapshot = concreteOther._dictionary.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TValue>(kvp.Value));
                    }
                    finally
                    {
                        second.ExitReadLock();
                    }
                }
                finally
                {
                    first.ExitReadLock();
                }

                foreach (var kvp in thisSnapshot)
                {
                    if (!otherSnapshot.TryGetValue(kvp.Key, out var otherSet))
                        return false;

                    if (!kvp.Value.SetEquals(otherSet))
                        return false;
                }

                return true;
            }

            // General path: snapshot this instance under its semaphore, then query
            // the other side via the interface API (no sync-context risk here because
            // the caller already opted into the synchronous overload).
            Dictionary<TKey, HashSet<TValue>> snapshot;
            int thisCount;

            EnterReadLockSync();
            try
            {
                thisCount = _count;
                snapshot = _dictionary.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TValue>(kvp.Value));
            }
            finally
            {
                ExitReadLock();
            }

            int otherCount = other.GetCountAsync(CancellationToken.None).AsTask().GetAwaiter().GetResult();
            int otherKeyCount = other.GetKeyCountAsync(CancellationToken.None).AsTask().GetAwaiter().GetResult();

            if (thisCount != otherCount || snapshot.Count != otherKeyCount)
                return false;

            foreach (var kvp in snapshot)
            {
                var (found, otherValues) = other.TryGetAsync(kvp.Key, CancellationToken.None).AsTask().GetAwaiter().GetResult();
                if (!found)
                    return false;

                var otherSet = otherValues is HashSet<TValue> hs
                    ? hs
                    : new HashSet<TValue>(otherValues, _valueComparer);

                if (!kvp.Value.SetEquals(otherSet))
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public async ValueTask<bool> EqualsAsync(object? obj) => await EqualsAsync(obj as IReadOnlyMultiMapAsync<TKey, TValue>).ConfigureAwait(false);

        /// <inheritdoc/>
        public async ValueTask<bool> EqualsAsync(IReadOnlyMultiMapAsync<TKey, TValue>? other, CancellationToken cancellationToken = default)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            ThrowIfDisposed();

            // Fast path: both sides are MultiMapAsync — acquire both semaphores atomically.
            if (other is MultiMapAsync<TKey, TValue> concreteOther)
            {
                concreteOther.ThrowIfDisposed();

                var first = RuntimeHelpers.GetHashCode(this) <= RuntimeHelpers.GetHashCode(concreteOther) ? this : concreteOther;
                var second = ReferenceEquals(first, this) ? concreteOther : this;

                Dictionary<TKey, HashSet<TValue>> thisSnapshot;
                Dictionary<TKey, HashSet<TValue>> otherSnapshot;

                await first.EnterReadLockAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    await second.EnterReadLockAsync(cancellationToken).ConfigureAwait(false);
                    try
                    {
                        if (Volatile.Read(ref _count) != Volatile.Read(ref concreteOther._count) ||
                            _dictionary.Count != concreteOther._dictionary.Count)
                            return false;

                        thisSnapshot = _dictionary.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TValue>(kvp.Value));
                        otherSnapshot = concreteOther._dictionary.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TValue>(kvp.Value));
                    }
                    finally
                    {
                        second.ExitReadLock();
                    }
                }
                finally
                {
                    first.ExitReadLock();
                }

                foreach (var kvp in thisSnapshot)
                {
                    if (!otherSnapshot.TryGetValue(kvp.Key, out var otherSet))
                        return false;

                    if (!kvp.Value.SetEquals(otherSet))
                        return false;
                }

                return true;
            }

            // General path: snapshot this instance, then compare asynchronously via the interface API.
            Dictionary<TKey, HashSet<TValue>> snapshot;
            int thisCount;

            await EnterReadLockAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                thisCount = _count;
                snapshot = _dictionary.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TValue>(kvp.Value));
            }
            finally
            {
                ExitReadLock();
            }

            int otherCount = await other.GetCountAsync(cancellationToken).ConfigureAwait(false);
            int otherKeyCount = await other.GetKeyCountAsync(cancellationToken).ConfigureAwait(false);

            if (thisCount != otherCount || snapshot.Count != otherKeyCount)
                return false;

            foreach (var kvp in snapshot)
            {
                var (found, otherValues) = await other.TryGetAsync(kvp.Key, cancellationToken).ConfigureAwait(false);
                if (!found)
                    return false;

                var otherSet = otherValues is HashSet<TValue> hs
                    ? hs
                    : new HashSet<TValue>(otherValues, _valueComparer);

                if (!kvp.Value.SetEquals(otherSet))
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            ThrowIfDisposed();
            EnterReadLockSync();
            try
            {
                return MultiMapHelper.ComputeUnorderedHash<TKey, TValue, HashSet<TValue>>(_dictionary, _dictionary.Comparer, _valueComparer);
            }
            finally
            {
                ExitReadLock();
            }
        }
    }
}
