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
    /// MultiMapAsync is designed for concurrent scenarios where asynchronous access and modification of the collection are required.
    /// All operations are thread-safe and use internal locking to ensure consistency.
    /// Enumerating the collection produces a snapshot of the current state, so changes made during enumeration are not reflected.
    /// This class is useful for managing associations where each key can have multiple distinct values, such as grouping or indexing tasks in asynchronous workflows.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multi-map. Must be non-nullable and implement <see cref="IEquatable{TKey}"/>.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-nullable and implement <see cref="IEquatable{TValue}"/>.</typeparam>
    public sealed partial class MultiMapAsync<TKey, TValue> : IMultiMapAsync<TKey, TValue>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull, IEquatable<TValue>
    {
        private readonly Dictionary<TKey, HashSet<TValue>> _dictionary;
        private readonly SemaphoreSlim _semaphore;
        private readonly IEqualityComparer<TValue>? _valueComparer;
        private int _count;
        private int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapAsync{TKey, TValue}"/> class.
        /// </summary>
        public MultiMapAsync()
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>();
            _semaphore = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapAsync{TKey, TValue}"/> class with the specified equality comparer for keys.
        /// </summary>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapAsync(IEqualityComparer<TKey>? keyComparer)
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>(keyComparer);
            _semaphore = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapAsync{TKey, TValue}"/> class with the specified equality comparer for values.
        /// </summary>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapAsync(IEqualityComparer<TValue>? valueComparer)
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>();
            _semaphore = new SemaphoreSlim(1, 1);
            _valueComparer = valueComparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapAsync{TKey, TValue}"/> class with the specified initial capacity for keys.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        public MultiMapAsync(int capacity)
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>(capacity);
            _semaphore = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapAsync{TKey, TValue}"/> class with the specified initial capacity for keys and equality comparer for keys.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapAsync(int capacity, IEqualityComparer<TKey>? keyComparer)
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>(capacity, keyComparer);
            _semaphore = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapAsync{TKey, TValue}"/> class with the specified initial capacity for keys and equality comparer for values.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapAsync(int capacity, IEqualityComparer<TValue>? valueComparer)
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>(capacity);
            _semaphore = new SemaphoreSlim(1, 1);
            _valueComparer = valueComparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapAsync{TKey, TValue}"/> class with the specified initial capacity for keys and equality comparer for keys and values.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapAsync(int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>(capacity, keyComparer);
            _semaphore = new SemaphoreSlim(1, 1);
            _valueComparer = valueComparer;
        }

        // ── AddAsync ──────────────────────────────────────────

        /// <inheritdoc/>
        public ValueTask<bool> AddAsync(TKey key, TValue value, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));

            ThrowIfDisposed();
            Task waitTask = _semaphore.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<bool>(AddCore(key, value));
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return AddSlowAsync(waitTask, key, value);
        }
        // ── AddRangeAsync ─────────────────────────────────────

        /// <inheritdoc/>
        public ValueTask<int> AddRangeAsync(TKey key, IEnumerable<TValue> values, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (values is null) throw new ArgumentNullException(nameof(values));

            ThrowIfDisposed();
            Task waitTask = _semaphore.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<int>(AddRangeCore(key, values));
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return AddRangeSlowAsync(waitTask, key, values);
        }

        // ── AddRangeAsync (pairs) ──────────────────────────────

        /// <inheritdoc/>
        public ValueTask<int> AddRangeAsync(IEnumerable<KeyValuePair<TKey, TValue>> items, CancellationToken cancellationToken = default)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));

            ThrowIfDisposed();
            Task waitTask = _semaphore.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<int>(AddRangeCore(items));
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return AddRangeSlowAsync(waitTask, items);
        }

        // ── GetAsync ──────────────────────────────────────────

        /// <inheritdoc/>
        public ValueTask<IEnumerable<TValue>> GetAsync(TKey key, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            ThrowIfDisposed();
            Task waitTask = _semaphore.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<IEnumerable<TValue>>(GetCore(key));
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return GetSlowAsync(waitTask, key);
        }
        // ── GetOrDefaultAsync ─────────────────────────────────

        /// <inheritdoc/>
        public ValueTask<IEnumerable<TValue>> GetOrDefaultAsync(TKey key, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            ThrowIfDisposed();
            Task waitTask = _semaphore.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<IEnumerable<TValue>>(GetOrDefaultCore(key));
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return GetOrDefaultSlowAsync(waitTask, key);
        }

        // ── TryGetAsync ────────────────────────────────────────

        /// <inheritdoc/>
        public ValueTask<(bool found, IEnumerable<TValue> values)> TryGetAsync(TKey key, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            ThrowIfDisposed();
            Task waitTask = _semaphore.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<(bool found, IEnumerable<TValue> values)>(TryGetCore(key));
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return TryGetSlowAsync(waitTask, key);
        }

        // ── RemoveAsync ───────────────────────────────────────

        /// <inheritdoc/>
        public ValueTask<bool> RemoveAsync(TKey key, TValue value, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));

            ThrowIfDisposed();
            Task waitTask = _semaphore.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<bool>(RemoveCore(key, value));
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return RemoveSlowAsync(waitTask, key, value);
        }
        // ── RemoveRangeAsync ──────────────────────────────────

        /// <inheritdoc/>
        public ValueTask<int> RemoveRangeAsync(IEnumerable<KeyValuePair<TKey, TValue>> items, CancellationToken cancellationToken = default)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));

            ThrowIfDisposed();
            Task waitTask = _semaphore.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<int>(RemoveRangeCore(items));
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return RemoveRangeSlowAsync(waitTask, items);
        }

        // ── RemoveWhereAsync ──────────────────────────────────

        /// <inheritdoc/>
        public ValueTask<int> RemoveWhereAsync(TKey key, Predicate<TValue> predicate, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            ThrowIfDisposed();
            Task waitTask = _semaphore.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<int>(RemoveWhereCore(key, predicate));
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return RemoveWhereSlowAsync(waitTask, key, predicate);
        }

        // ── RemoveKeyAsync ────────────────────────────────────

        /// <inheritdoc/>
        public ValueTask<bool> RemoveKeyAsync(TKey key, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            ThrowIfDisposed();
            Task waitTask = _semaphore.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<bool>(RemoveKeyCore(key));
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return RemoveKeySlowAsync(waitTask, key);
        }

        // ── ContainsKeyAsync ──────────────────────────────────

        /// <inheritdoc/>
        public ValueTask<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            ThrowIfDisposed();
            Task waitTask = _semaphore.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<bool>(_dictionary.ContainsKey(key));
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return ContainsKeySlowAsync(waitTask, key);
        }

        // ── ContainsAsync ─────────────────────────────────────

        /// <inheritdoc/>
        public ValueTask<bool> ContainsAsync(TKey key, TValue value, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));

            ThrowIfDisposed();
            Task waitTask = _semaphore.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<bool>(
                        _dictionary.TryGetValue(key, out var hashset) && hashset.Contains(value));
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return ContainsSlowAsync(waitTask, key, value);
        }

        // ── GetCountAsync ─────────────────────────────────────

        /// <inheritdoc/>
        public ValueTask<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            Task waitTask = _semaphore.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<int>(_count);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return GetCountSlowAsync(waitTask);
        }

        // ── GetKeysAsync ──────────────────────────────────────

        /// <inheritdoc/>
        public ValueTask<IEnumerable<TKey>> GetKeysAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            Task waitTask = _semaphore.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<IEnumerable<TKey>>(_dictionary.Keys.ToArray());
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return GetKeysSlowAsync(waitTask);
        }

        // ── GetKeyCountAsync ──────────────────────────────────

        /// <inheritdoc/>
        public ValueTask<int> GetKeyCountAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            Task waitTask = _semaphore.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<int>(_dictionary.Count);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return GetKeysCountSlowAsync(waitTask);
        }

        // ── GetValuesAsync ────────────────────────────────────

        /// <inheritdoc/>
        public ValueTask<IEnumerable<TValue>> GetValuesAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            Task waitTask = _semaphore.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<IEnumerable<TValue>>(GetValuesCore());
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return GetValuesSlowAsync(waitTask);
        }

        // ── GetValuesCountAsync ───────────────────────────────

        /// <inheritdoc/>
        public ValueTask<int> GetValuesCountAsync(TKey key, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            ThrowIfDisposed();
            Task waitTask = _semaphore.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return new ValueTask<int>(_dictionary.TryGetValue(key, out var hashset) ? hashset.Count : 0);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return GetValuesCountSlowAsync(waitTask, key);
        }

        // ── ClearAsync ────────────────────────────────────────

        /// <inheritdoc/>
        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            Task waitTask = _semaphore.WaitAsync(cancellationToken);
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
                    _semaphore.Release();
                }
            }
            return ClearSlowAsync(waitTask);
        }

        // ── UnionAsync ────────────────────────────────────────

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

            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                UnionCore(snapshot);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // ── IntersectAsync ────────────────────────────────────

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

            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                IntersectCore(otherIndex);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // ── ExceptWithAsync ───────────────────────────────────

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

            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                ExceptWithCore(snapshot);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // ── SymmetricExceptWithAsync ──────────────────────────

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

            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                SymmetricExceptWithCore(snapshot);
            }
            finally
            {
                _semaphore.Release();
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
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            List<KeyValuePair<TKey, TValue>> snapshot;

            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
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
                _semaphore.Release();
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
            if (obj is not MultiMapAsync<TKey, TValue> other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (SynchronizationContext.Current != null)
                throw new InvalidOperationException(
                    $"Calling {nameof(Equals)} on {nameof(MultiMapAsync<TKey, TValue>)} from a thread with a " +
                    $"{nameof(SynchronizationContext)} (e.g. a UI thread) can deadlock. " +
                    $"Use {nameof(EqualsAsync)} instead.");

            ThrowIfDisposed();
            other.ThrowIfDisposed();

            var first = RuntimeHelpers.GetHashCode(this) <= RuntimeHelpers.GetHashCode(other) ? this : other;
            var second = ReferenceEquals(first, this) ? other : this;

            Dictionary<TKey, HashSet<TValue>> thisSnapshot;
            Dictionary<TKey, HashSet<TValue>> otherSnapshot;

            first._semaphore.Wait();
            try
            {
                second._semaphore.Wait();
                try
                {
                    if (Volatile.Read(ref _count) != Volatile.Read(ref other._count) || _dictionary.Count != other._dictionary.Count)
                        return false;

                    thisSnapshot = _dictionary.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TValue>(kvp.Value));
                    otherSnapshot = other._dictionary.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TValue>(kvp.Value));
                }
                finally
                {
                    second._semaphore.Release();
                }
            }
            finally
            {
                first._semaphore.Release();
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

        /// <inheritdoc/>
        public bool Equals(IReadOnlyMultiMapAsync<TKey, TValue>? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            ThrowIfDisposed();

            // Wait for both semaphores to compare safely
            _semaphore.Wait();
            try
            {
                // Get counts synchronously from this instance
                int thisKeyCount = _dictionary.Count;
                int thisCount = _count;

                // Get counts from other instance
                int otherKeyCount = other.GetKeyCountAsync(CancellationToken.None).GetAwaiter().GetResult();
                int otherCount = other.GetCountAsync(CancellationToken.None).GetAwaiter().GetResult();

                // Quick check: if counts differ, they're not equal
                if (thisKeyCount != otherKeyCount || thisCount != otherCount)
                    return false;

                // Compare each key and its values
                foreach (var kvp in _dictionary)
                {
                    var key = kvp.Key;
                    var thisValues = kvp.Value;

                    // Check if other contains the key
                    var (found, otherValues) = other.TryGetAsync(key, CancellationToken.None).GetAwaiter().GetResult();
                    if (!found)
                        return false;

                    // Convert otherValues to HashSet for comparison
                    var otherValuesSet = otherValues is HashSet<TValue> hs
                        ? hs
                        : new HashSet<TValue>(otherValues, _valueComparer);

                    // Check if value counts match
                    if (thisValues.Count != otherValuesSet.Count)
                        return false;

                    // Check if all values in thisValues are in otherValuesSet
                    foreach (var value in thisValues)
                    {
                        if (!otherValuesSet.Contains(value))
                            return false;
                    }
                }

                return true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Asynchronously determines whether the current instance and the specified object are equal by comparing their key-value mappings.
        /// </summary>
        /// <remarks>Equality is determined by comparing the set of keys and the associated sets of values in both instances. The comparison is thread-safe and takes a snapshot of the current state of both maps.
        /// If either instance has been disposed, an exception is thrown.</remarks>
        /// <param name="obj">The object to compare with the current instance. Typically, this should be another <see cref="MultiMapAsync{TKey, TValue}"/>.</param>
        /// <returns>A ValueTask that represents the asynchronous operation. The result is <see langword="true"/> if the specified object is a <see cref="MultiMapAsync{TKey, TValue}"/> and contains the same keys and associated values as the current instance; otherwise, <see langword="false"/>.</returns>
        public async ValueTask<bool> EqualsAsync(object? obj)
        {
            if (obj is not MultiMapAsync<TKey, TValue> other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            ThrowIfDisposed();
            other.ThrowIfDisposed();

            var first = RuntimeHelpers.GetHashCode(this) <= RuntimeHelpers.GetHashCode(other) ? this : other;
            var second = ReferenceEquals(first, this) ? other : this;

            Dictionary<TKey, HashSet<TValue>> thisSnapshot;
            Dictionary<TKey, HashSet<TValue>> otherSnapshot;

            await first._semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                await second._semaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    if (Volatile.Read(ref _count) != Volatile.Read(ref other._count) || _dictionary.Count != other._dictionary.Count)
                        return false;

                    thisSnapshot = _dictionary.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TValue>(kvp.Value));
                    otherSnapshot = other._dictionary.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TValue>(kvp.Value));
                }
                finally
                {
                    second._semaphore.Release();
                }
            }
            finally
            {
                first._semaphore.Release();
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

        /// <summary>
        /// Asynchronously determines whether the current instance and the specified <see cref="IReadOnlyMultiMapAsync{TKey, TValue}"/> are equal by comparing their key-value mappings.
        /// </summary>
        /// <remarks>
        /// When <paramref name="other"/> is another <see cref="MultiMapAsync{TKey, TValue}"/>, both semaphores are acquired atomically (lock-ordering by identity hash code to prevent deadlock) and a pair of snapshots is taken before any comparison work is done outside the locks. For any other <see cref="IReadOnlyMultiMapAsync{TKey, TValue}"/> implementation, a snapshot of this instance is taken under its own semaphore, and the comparison is then performed asynchronously against the other instance using its public async API.
        /// </remarks>
        /// <param name="other">The map to compare with the current instance.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> whose result is <see langword="true"/> if both maps contain the same keys with the same associated value sets; otherwise <see langword="false"/>.
        /// </returns>
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

                await first._semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    await second._semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
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
                        second._semaphore.Release();
                    }
                }
                finally
                {
                    first._semaphore.Release();
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

            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                thisCount = _count;
                snapshot = _dictionary.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TValue>(kvp.Value));
            }
            finally
            {
                _semaphore.Release();
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
            _semaphore.Wait();
            try
            {
                return MultiMapHelper.ComputeUnorderedHash<TKey, TValue, HashSet<TValue>>(_dictionary);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}