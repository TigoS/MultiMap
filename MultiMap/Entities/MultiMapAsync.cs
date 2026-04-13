using MultiMap.Helpers;
using System.Runtime.CompilerServices;
#if NET6_0_OR_GREATER
using System.Runtime.InteropServices;
#endif
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
    /// <typeparam name="TKey">The type of keys in the multi-map. Must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-nullable.</typeparam>
    public class MultiMapAsync<TKey, TValue> : IMultiMapAsync<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
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

        private void ThrowIfDisposed()
        {
            if (Volatile.Read(ref _disposed) != 0)
                throw new ObjectDisposedException(GetType().FullName);
        }

        private static bool IsCompletedSuccessfully(Task task)
        {
#if NETSTANDARD2_0
            return task.Status == TaskStatus.RanToCompletion;
#else
            return task.IsCompletedSuccessfully;
#endif
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

        private bool AddCore(TKey key, TValue value)
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

        private async ValueTask<bool> AddSlowAsync(Task waitTask, TKey key, TValue value)
        {
            await waitTask.ConfigureAwait(false);
            try
            {
                return AddCore(key, value);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // ── AddRangeAsync ─────────────────────────────────────

        /// <inheritdoc/>
        public Task<int> AddRangeAsync(TKey key, IEnumerable<TValue> values, CancellationToken cancellationToken = default)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (values is null) throw new ArgumentNullException(nameof(values));

            ThrowIfDisposed();
            Task waitTask = _semaphore.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return Task.FromResult(AddRangeCore(key, values));
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return AddRangeSlowAsync(waitTask, key, values);
        }

        private int AddRangeCore(TKey key, IEnumerable<TValue> values)
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

            int added = 0;
            foreach (var value in values)
            {
                if (hashset.Add(value))
                {
                    _count++;
                    added++;
                }
            }

            return added;
        }

        private async Task<int> AddRangeSlowAsync(Task waitTask, TKey key, IEnumerable<TValue> values)
        {
            await waitTask.ConfigureAwait(false);
            try
            {
                return AddRangeCore(key, values);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // ── AddRangeAsync (overload for params array) ─────────

        /// <inheritdoc/>
        public Task<int> AddRangeAsync(IEnumerable<KeyValuePair<TKey, TValue>> items, CancellationToken cancellationToken = default)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));

            ThrowIfDisposed();
            Task waitTask = _semaphore.WaitAsync(cancellationToken);
            if (IsCompletedSuccessfully(waitTask))
            {
                try
                {
                    return Task.FromResult(AddRangeCore(items));
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return AddRangeSlowAsync(waitTask, items);
        }

        private int AddRangeCore(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            int added = 0;
            foreach (var item in items)
            {
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

        private async Task<int> AddRangeSlowAsync(Task waitTask, IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            await waitTask.ConfigureAwait(false);
            try
            {
                return AddRangeCore(items);
            }
            finally
            {
                _semaphore.Release();
            }
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

        private IEnumerable<TValue> GetCore(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
                return hashset.ToArray();

            throw new KeyNotFoundException($"The key '{key}' was not found in the multimap.");
        }

        private async ValueTask<IEnumerable<TValue>> GetSlowAsync(Task waitTask, TKey key)
        {
            await waitTask.ConfigureAwait(false);
            try
            {
                return GetCore(key);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // ── GetOrDefaultAsync ──────────────────────────────────────────

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

        private IEnumerable<TValue> GetOrDefaultCore(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
                return hashset.ToArray();

            return [];
        }

        private async ValueTask<IEnumerable<TValue>> GetOrDefaultSlowAsync(Task waitTask, TKey key)
        {
            await waitTask.ConfigureAwait(false);
            try
            {
                return GetOrDefaultCore(key);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // ── TryGetAsync ──────────────────────────────────────────

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

        private (bool found, IEnumerable<TValue> values) TryGetCore(TKey key)
        {
            (bool found, IEnumerable<TValue> values) result;
            result.found = _dictionary.TryGetValue(key, out var hashset);
            result.values = result.found ? hashset?.ToArray() ?? [] : [];

            return result;
        }

        private async ValueTask<(bool found, IEnumerable<TValue> values)> TryGetSlowAsync(Task waitTask, TKey key)
        {
            await waitTask.ConfigureAwait(false);
            try
            {
                return TryGetCore(key);
            }
            finally
            {
                _semaphore.Release();
            }
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

        private bool RemoveCore(TKey key, TValue value)
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

        private async ValueTask<bool> RemoveSlowAsync(Task waitTask, TKey key, TValue value)
        {
            await waitTask.ConfigureAwait(false);
            try
            {
                return RemoveCore(key, value);
            }
            finally
            {
                _semaphore.Release();
            }
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

        private int RemoveRangeCore(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            int removedCount = 0;
            foreach (var item in items)
            {
                if (RemoveCore(item.Key, item.Value))
                {
                    removedCount++;
                }
            }

            return removedCount;
        }

        private async ValueTask<int> RemoveRangeSlowAsync(Task waitTask, IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            await waitTask.ConfigureAwait(false);
            try
            {
                return RemoveRangeCore(items);
            }
            finally
            {
                _semaphore.Release();
            }
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

        private int RemoveWhereCore(TKey key, Predicate<TValue> predicate)
        {
            if (!_dictionary.TryGetValue(key, out var hashset))
                return 0;

            int removedCount = hashset.RemoveWhere(predicate);
            _count -= removedCount;

            if (hashset.Count == 0)
                _dictionary.Remove(key);

            return removedCount;
        }

        private async ValueTask<int> RemoveWhereSlowAsync(Task waitTask, TKey key, Predicate<TValue> predicate)
        {
            await waitTask.ConfigureAwait(false);
            try
            {
                return RemoveWhereCore(key, predicate);
            }
            finally
            {
                _semaphore.Release();
            }
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

        private bool RemoveKeyCore(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
            {
                _count -= hashset.Count;
                return _dictionary.Remove(key);
            }

            return false;
        }

        private async ValueTask<bool> RemoveKeySlowAsync(Task waitTask, TKey key)
        {
            await waitTask.ConfigureAwait(false);
            try
            {
                return RemoveKeyCore(key);
            }
            finally
            {
                _semaphore.Release();
            }
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

        private async ValueTask<bool> ContainsKeySlowAsync(Task waitTask, TKey key)
        {
            await waitTask.ConfigureAwait(false);
            try
            {
                return _dictionary.ContainsKey(key);
            }
            finally
            {
                _semaphore.Release();
            }
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

        private async ValueTask<bool> ContainsSlowAsync(Task waitTask, TKey key, TValue value)
        {
            await waitTask.ConfigureAwait(false);
            try
            {
                return _dictionary.TryGetValue(key, out var hashset) && hashset.Contains(value);
            }
            finally
            {
                _semaphore.Release();
            }
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

        private async ValueTask<int> GetCountSlowAsync(Task waitTask)
        {
            await waitTask.ConfigureAwait(false);
            try
            {
                return _count;
            }
            finally
            {
                _semaphore.Release();
            }
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

        private async ValueTask<IEnumerable<TKey>> GetKeysSlowAsync(Task waitTask)
        {
            await waitTask.ConfigureAwait(false);
            try
            {
                return _dictionary.Keys.ToArray();
            }
            finally
            {
                _semaphore.Release();
            }
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

        private async ValueTask<int> GetKeysCountSlowAsync(Task waitTask)
        {
            await waitTask.ConfigureAwait(false);
            try
            {
                return _dictionary.Count;
            }
            finally
            {
                _semaphore.Release();
            }
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
                    return new ValueTask<IEnumerable<TValue>>(_dictionary.Values.SelectMany(v => v).ToArray());
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return GetValuesSlowAsync(waitTask);
        }

        private async ValueTask<IEnumerable<TValue>> GetValuesSlowAsync(Task waitTask)
        {
            await waitTask.ConfigureAwait(false);
            try
            {
                return _dictionary.Values.SelectMany(v => v).ToArray();
            }
            finally
            {
                _semaphore.Release();
            }
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

        private async ValueTask<int> GetValuesCountSlowAsync(Task waitTask, TKey key)
        {
            await waitTask.ConfigureAwait(false);
            try
            {
                return _dictionary.TryGetValue(key, out var hashset) ? hashset.Count : 0;
            }
            finally
            {
                _semaphore.Release();
            }
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

        private async Task ClearSlowAsync(Task waitTask)
        {
            await waitTask.ConfigureAwait(false);
            try
            {
                _dictionary.Clear();
                _count = 0;
            }
            finally
            {
                _semaphore.Release();
            }
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

        private void UnionCore(List<(TKey Key, TValue[] Values)> snapshot)
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

        private void IntersectCore(Dictionary<TKey, HashSet<TValue>> otherIndex)
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

        private void ExceptWithCore(List<(TKey Key, TValue[] Values)> snapshot)
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

        private void SymmetricExceptWithCore(List<(TKey Key, TValue[] Values)> snapshot)
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
            return DisposeAsyncCore();
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
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

        /// <summary>
        /// Releases resources used by the current instance.
        /// </summary>
        /// <remarks>Override this method in a derived class to release additional resources. This method is called by the public Dispose pattern implementation to perform actual cleanup of managed or unmanaged resources.</remarks>
        protected virtual void DisposeCore()
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
                _semaphore.Dispose();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <remarks>Override this method to release additional resources in a derived class when disposing asynchronously. This method is called by DisposeAsync and should not be called directly.</remarks>
        /// <returns>A ValueTask that represents the asynchronous dispose operation.</returns>
        protected virtual ValueTask DisposeAsyncCore()
        {
            DisposeCore();
            return default;
        }
    }
}
