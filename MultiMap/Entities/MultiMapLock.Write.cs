using System.Runtime.InteropServices;
using MultiMap.Helpers;
using MultiMap.Interfaces;

namespace MultiMap.Entities
{
    public sealed partial class MultiMapLock<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
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
                        {
                            _dictionary.Remove(key);
                        }
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
                        {
                            _dictionary.Remove(key);
                        }
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
                            {
                                _dictionary.Remove(item.Key);
                            }
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
                            {
                                _dictionary.Remove(item.Key);
                            }
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
    }
}
