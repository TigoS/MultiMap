using System.Diagnostics;
using System.Runtime.InteropServices;
using MultiMap.Helpers;
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
    /// <typeparam name="TKey">The type of keys in the multi-map.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key.</typeparam>
    /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
    /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
    /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
    [DebuggerDisplay("Keys={_dictionary.Count}, Values={_count}")]
    public sealed partial class MultiMapAsync<TKey, TValue>(int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer) : IMultiMapAsync<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        private readonly Dictionary<TKey, HashSet<TValue>> _dictionary = capacity > 0
                ? new Dictionary<TKey, HashSet<TValue>>(capacity, keyComparer)
                : new Dictionary<TKey, HashSet<TValue>>(keyComparer);

        /// <summary>Exclusive write lock – held by the single active writer.</summary>
        private readonly SemaphoreSlim _writeLock = new(1, 1);

        /// <summary>Guards the <see cref="_activeReaders"/> counter; held only for the duration of an increment/decrement.</summary>
        private readonly SemaphoreSlim _readerLock = new(1, 1);

        private readonly IEqualityComparer<TValue>? _valueComparer = valueComparer;
        private int _activeReaders;
        private int _count;
        private int _disposed;

        // ── Guards ────────────────────────────────────────────

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, GetType().FullName!);
        }

        // ── Async reader-writer lock helpers ──────────────────
        //
        // Design: _readerLock guards the _activeReaders counter (held only for
        // the duration of an increment/decrement).  The first reader also acquires
        // _writeLock so that writers are blocked while any reader is active; the
        // last reader releases _writeLock.  Writers acquire _writeLock directly.

        /// <summary>
        /// Tries to enter a read lock without blocking.
        /// Returns <see langword="true"/> and increments the reader count when successful.
        /// </summary>
        private bool TryEnterReadLockSync()
        {
            if (!_readerLock.Wait(0))
            {
                return false;
            }

            try
            {
                if (_activeReaders == 0)
                {
                    if (!_writeLock.Wait(0))
                    {
                        // Writer holds the lock – do not enter read lock.
                        return false;
                    }
                }

                _activeReaders++;
                return true;
            }
            finally
            {
                _readerLock.Release();
            }
        }

        /// <summary>Enters a read lock synchronously (blocking).</summary>
        private void EnterReadLockSync()
        {
            _readerLock.Wait();
            try
            {
                if (_activeReaders == 0)
                {
                    _writeLock.Wait();
                }

                _activeReaders++;
            }
            finally
            {
                _readerLock.Release();
            }
        }

        /// <summary>Enters a read lock asynchronously.</summary>
        private async Task EnterReadLockAsync(CancellationToken cancellationToken)
        {
            await _readerLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_activeReaders == 0)
                {
                    await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                }

                _activeReaders++;
            }
            finally
            {
                _readerLock.Release();
            }
        }

        /// <summary>Exits a previously entered read lock.</summary>
        private void ExitReadLock()
        {
            _readerLock.Wait();
            try
            {
                if (--_activeReaders == 0)
                {
                    _writeLock.Release();
                }
            }
            finally
            {
                _readerLock.Release();
            }
        }

        // ── Add ───────────────────────────────────────────────

        private bool AddCore(TKey key, TValue value)
        {
            ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, key, out _);
            hashset ??= new HashSet<TValue>(_valueComparer);

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
                _writeLock.Release();
            }
        }

        // ── AddRange ──────────────────────────────────────────

        private int AddRangeCore(TKey key, IEnumerable<TValue> values)
        {
            bool exists = _dictionary.TryGetValue(key, out var hashset);
            if (!exists)
            {
                hashset = new HashSet<TValue>(_valueComparer);
            }

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
            {
                _dictionary[key] = hashset!;
            }

            return added;
        }

        private async ValueTask<int> AddRangeSlowAsync(Task waitTask, TKey key, IEnumerable<TValue> values)
        {
            await waitTask.ConfigureAwait(false);
            try
            {
                return AddRangeCore(key, values);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        private int AddRangeCore(IEnumerable<KeyValuePair<TKey, TValue>> items)
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

        private async ValueTask<int> AddRangeSlowAsync(Task waitTask, IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            await waitTask.ConfigureAwait(false);
            try
            {
                return AddRangeCore(items);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        // ── Get ───────────────────────────────────────────────

        private TValue[] GetCore(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
            {
                return [.. hashset];
            }

            throw new KeyNotFoundException($"The key '{key}' was not found in the multimap.");
        }

        private async ValueTask<IEnumerable<TValue>> GetSlowAsync(TKey key, CancellationToken cancellationToken)
        {
            await EnterReadLockAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return GetCore(key);
            }
            finally
            {
                ExitReadLock();
            }
        }

        // ── GetOrDefault ──────────────────────────────────────

        private TValue[] GetOrDefaultCore(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
            {
                return [.. hashset];
            }

            return [];
        }

        private async ValueTask<IEnumerable<TValue>> GetOrDefaultSlowAsync(TKey key, CancellationToken cancellationToken)
        {
            await EnterReadLockAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return GetOrDefaultCore(key);
            }
            finally
            {
                ExitReadLock();
            }
        }

        // ── TryGet ────────────────────────────────────────────

        private (bool found, IEnumerable<TValue> values) TryGetCore(TKey key)
        {
            (bool found, IEnumerable<TValue> values) result;
            result.found = _dictionary.TryGetValue(key, out var hashset);
            result.values = result.found ? hashset?.ToArray() ?? [] : [];

            return result;
        }

        private async ValueTask<(bool found, IEnumerable<TValue> values)> TryGetSlowAsync(TKey key, CancellationToken cancellationToken)
        {
            await EnterReadLockAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return TryGetCore(key);
            }
            finally
            {
                ExitReadLock();
            }
        }

        // ── Remove ────────────────────────────────────────────

        private bool RemoveCore(TKey key, TValue value)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
            {
                bool removed = hashset.Remove(value);

                if (removed)
                {
                    _count--;
                    if (hashset.Count == 0)
                    {
                        _dictionary.Remove(key);
                    }
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
                _writeLock.Release();
            }
        }

        // ── RemoveRange ───────────────────────────────────────

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
                _writeLock.Release();
            }
        }

        // ── RemoveWhere ───────────────────────────────────────

        private int RemoveWhereCore(TKey key, Predicate<TValue> predicate)
        {
            if (!_dictionary.TryGetValue(key, out var hashset))
            {
                return 0;
            }

            int removedCount = hashset.RemoveWhere(predicate);
            _count -= removedCount;

            if (hashset.Count == 0)
            {
                _dictionary.Remove(key);
            }

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
                _writeLock.Release();
            }
        }

        // ── RemoveKey ─────────────────────────────────────────

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
                _writeLock.Release();
            }
        }

        // ── ContainsKey ───────────────────────────────────────

        private async ValueTask<bool> ContainsKeySlowAsync(TKey key, CancellationToken cancellationToken)
        {
            await EnterReadLockAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return _dictionary.ContainsKey(key);
            }
            finally
            {
                ExitReadLock();
            }
        }

        // ── Contains ──────────────────────────────────────────

        private async ValueTask<bool> ContainsSlowAsync(TKey key, TValue value, CancellationToken cancellationToken)
        {
            await EnterReadLockAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return _dictionary.TryGetValue(key, out var hashset) && hashset.Contains(value);
            }
            finally
            {
                ExitReadLock();
            }
        }

        // ── GetCount ──────────────────────────────────────────

        private async ValueTask<int> GetCountSlowAsync(CancellationToken cancellationToken)
        {
            await EnterReadLockAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return _count;
            }
            finally
            {
                ExitReadLock();
            }
        }

        // ── GetKeys ───────────────────────────────────────────

        private async ValueTask<IEnumerable<TKey>> GetKeysSlowAsync(CancellationToken cancellationToken)
        {
            await EnterReadLockAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return [.. _dictionary.Keys];
            }
            finally
            {
                ExitReadLock();
            }
        }

        // ── GetKeyCount ───────────────────────────────────────

        private async ValueTask<int> GetKeyCountSlowAsync(CancellationToken cancellationToken)
        {
            await EnterReadLockAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return _dictionary.Count;
            }
            finally
            {
                ExitReadLock();
            }
        }

        // ── GetValues ─────────────────────────────────────────

        private TValue[] GetValuesCore()
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

        private async ValueTask<IEnumerable<TValue>> GetValuesSlowAsync(CancellationToken cancellationToken)
        {
            await EnterReadLockAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return GetValuesCore();
            }
            finally
            {
                ExitReadLock();
            }
        }

        // ── GetValuesCount ────────────────────────────────────

        private async ValueTask<int> GetValuesCountSlowAsync(TKey key, CancellationToken cancellationToken)
        {
            await EnterReadLockAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return _dictionary.TryGetValue(key, out var hashset) ? hashset.Count : 0;
            }
            finally
            {
                ExitReadLock();
            }
        }

        // ── Clear ─────────────────────────────────────────────

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
                _writeLock.Release();
            }
        }

        // ── Set operations ────────────────────────────────────

        private void UnionCore(List<(TKey Key, TValue[] Values)> snapshot)
        {
            foreach (var (key, values) in snapshot)
            {
                ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, key, out bool exists);
                hashset ??= new HashSet<TValue>(_valueComparer);

                foreach (var value in values)
                {
                    if (hashset.Add(value))
                    {
                        _count++;
                    }
                }
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
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _dictionary.Remove(key);
            }
        }

        private void ExceptWithCore(List<(TKey Key, TValue[] Values)> snapshot)
        {
            foreach (var (key, values) in snapshot)
            {
                if (!_dictionary.TryGetValue(key, out var hashset))
                {
                    continue;
                }

                foreach (var value in values)
                {
                    if (hashset.Remove(value))
                    {
                        _count--;
                    }
                }

                if (hashset.Count == 0)
                {
                    _dictionary.Remove(key);
                }
            }
        }

        private void SymmetricExceptWithCore(List<(TKey Key, TValue[] Values)> snapshot)
        {
            foreach (var (key, values) in snapshot)
            {
                ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, key, out bool _);
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
                {
                    _dictionary.Remove(key);
                }
            }
        }

        // ── Dispose ───────────────────────────────────────────

        /// <summary>
        /// Releases resources used by the current instance.
        /// </summary>
        /// <remarks>This method is called by the public Dispose and DisposeAsync pattern implementations to perform actual cleanup of managed or unmanaged resources.</remarks>
        private void DisposeCore()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            try
            {
                _dictionary.Clear();
                _count = 0;
            }
            finally
            {
                _writeLock.Dispose();
                _readerLock.Dispose();
            }
        }
    }
}
