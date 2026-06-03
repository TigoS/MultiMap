#if NET6_0_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace MultiMap.Entities
{
    public sealed partial class MultiMapAsync<TKey, TValue>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull, IEquatable<TValue>
    {
        // ── Guards ────────────────────────────────────────────

        private void ThrowIfDisposed()
        {
#if NET6_0_OR_GREATER
            ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, GetType()?.FullName ?? string.Empty);
#else
            if (Volatile.Read(ref _disposed) != 0)
                throw new ObjectDisposedException(GetType().FullName);
#endif
        }

        private static bool IsCompletedSuccessfully(Task task)
        {
#if NETSTANDARD2_0
            return task.Status == TaskStatus.RanToCompletion;
#else
            return task.IsCompletedSuccessfully;
#endif
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
                return false;

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
                    _writeLock.Wait();

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
                    await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);

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
                    _writeLock.Release();
            }
            finally
            {
                _readerLock.Release();
            }
        }

        // ── Add ───────────────────────────────────────────────

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
                _writeLock.Release();
            }
        }

        // ── AddRange ──────────────────────────────────────────

        private int AddRangeCore(TKey key, IEnumerable<TValue> values)
        {
            bool exists = _dictionary.TryGetValue(key, out var hashset);
            if (!exists)
                hashset = new HashSet<TValue>(_valueComparer);

            int added = 0;
            foreach (var value in values)
            {
                if (value is null) throw new ArgumentNullException(nameof(values), "Sequence contains a null value.");

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
                if (item.Key is null) throw new ArgumentNullException(nameof(items), "Sequence contains a null key.");
                if (item.Value is null) throw new ArgumentNullException(nameof(items), "Sequence contains a null value.");

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
                return hashset.ToArray();

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
                return hashset.ToArray();

            return Array.Empty<TValue>();
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
                return _dictionary.Keys.ToArray();
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

        // ── Dispose ───────────────────────────────────────────

        /// <summary>
        /// Releases resources used by the current instance.
        /// </summary>
        /// <remarks>This method is called by the public Dispose and DisposeAsync pattern implementations to perform actual cleanup of managed or unmanaged resources.</remarks>
        private void DisposeCore()
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
                _writeLock.Dispose();
                _readerLock.Dispose();
            }
        }
    }
}
