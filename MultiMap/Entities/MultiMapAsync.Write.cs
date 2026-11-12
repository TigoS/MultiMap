using MultiMap.Interfaces;

namespace MultiMap.Entities
{
    public sealed partial class MultiMapAsync<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        /// <inheritdoc/>
        public ValueTask<bool> AddAsync(TKey key, TValue value, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            ThrowIfDisposed();

            Task waitTask = WaitWriteLockAsync(cancellationToken);
            if (waitTask.IsCompletedSuccessfully)
            {
                try
                {
                    return new ValueTask<bool>(AddCore(key, value));
                }
                finally
                {
                    ExitWriteLock();
                }
            }
            return AddSlowAsync(waitTask, key, value);
        }

        /// <inheritdoc/>
        public ValueTask<int> AddRangeAsync(TKey key, IEnumerable<TValue> values, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));
            ArgumentNullException.ThrowIfNull(values, nameof(values));

            ThrowIfDisposed();
            Task waitTask = WaitWriteLockAsync(cancellationToken);
            if (waitTask.IsCompletedSuccessfully)
            {
                try
                {
                    return new ValueTask<int>(AddRangeCore(key, values));
                }
                finally
                {
                    ExitWriteLock();
                }
            }
            return AddRangeSlowAsync(waitTask, key, values);
        }

        /// <inheritdoc/>
        public ValueTask<int> AddRangeAsync(IEnumerable<KeyValuePair<TKey, TValue>> items, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(items, nameof(items));

            ThrowIfDisposed();

            Task waitTask = WaitWriteLockAsync(cancellationToken);
            if (waitTask.IsCompletedSuccessfully)
            {
                try
                {
                    return new ValueTask<int>(AddRangeCore(items));
                }
                finally
                {
                    ExitWriteLock();
                }
            }
            return AddRangeSlowAsync(waitTask, items);
        }

        /// <inheritdoc/>
        public ValueTask<bool> RemoveAsync(TKey key, TValue value, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            ThrowIfDisposed();
            Task waitTask = WaitWriteLockAsync(cancellationToken);
            if (waitTask.IsCompletedSuccessfully)
            {
                try
                {
                    return new ValueTask<bool>(RemoveCore(key, value));
                }
                finally
                {
                    ExitWriteLock();
                }
            }
            return RemoveSlowAsync(waitTask, key, value);
        }

        /// <inheritdoc/>
        public ValueTask<int> RemoveRangeAsync(IEnumerable<KeyValuePair<TKey, TValue>> items, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(items, nameof(items));

            ThrowIfDisposed();

            Task waitTask = WaitWriteLockAsync(cancellationToken);
            if (waitTask.IsCompletedSuccessfully)
            {
                try
                {
                    return new ValueTask<int>(RemoveRangeCore(items));
                }
                finally
                {
                    ExitWriteLock();
                }
            }
            return RemoveRangeSlowAsync(waitTask, items);
        }

        /// <inheritdoc/>
        public ValueTask<int> RemoveWhereAsync(TKey key, Predicate<TValue> predicate, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));
            ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

            ThrowIfDisposed();
            Task waitTask = WaitWriteLockAsync(cancellationToken);
            if (waitTask.IsCompletedSuccessfully)
            {
                try
                {
                    return new ValueTask<int>(RemoveWhereCore(key, predicate));
                }
                finally
                {
                    ExitWriteLock();
                }
            }
            return RemoveWhereSlowAsync(waitTask, key, predicate);
        }

        /// <inheritdoc/>
        public ValueTask<bool> RemoveKeyAsync(TKey key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));

            ThrowIfDisposed();
            Task waitTask = WaitWriteLockAsync(cancellationToken);
            if (waitTask.IsCompletedSuccessfully)
            {
                try
                {
                    return new ValueTask<bool>(RemoveKeyCore(key));
                }
                finally
                {
                    ExitWriteLock();
                }
            }
            return RemoveKeySlowAsync(waitTask, key);
        }

        /// <inheritdoc/>
        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            Task waitTask = WaitWriteLockAsync(cancellationToken);
            if (waitTask.IsCompletedSuccessfully)
            {
                try
                {
                    _dictionary.Clear();
                    _count = 0;
                    return Task.CompletedTask;
                }
                finally
                {
                    ExitWriteLock();
                }
            }
            return ClearSlowAsync(waitTask);
        }

        /// <inheritdoc/>
        public ValueTask<IEnumerable<TValue>> GetOrAddAsync(TKey key, TValue defaultValue, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));
            ArgumentNullException.ThrowIfNull(defaultValue, nameof(defaultValue));

            ThrowIfDisposed();

            Task waitTask = WaitWriteLockAsync(cancellationToken);
            if (waitTask.IsCompletedSuccessfully)
            {
                try
                {
                    return new ValueTask<IEnumerable<TValue>>(GetOrAddCore(key, defaultValue));
                }
                finally
                {
                    ExitWriteLock();
                }
            }
            return GetOrAddSlowAsync(waitTask, key, defaultValue);
        }

        /// <inheritdoc/>
        public ValueTask<IEnumerable<TValue>> GetOrAddAsync(TKey key, Lazy<TValue> valueFactory, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));
            ArgumentNullException.ThrowIfNull(valueFactory, nameof(valueFactory));

            ThrowIfDisposed();

            Task waitTask = WaitWriteLockAsync(cancellationToken);
            if (waitTask.IsCompletedSuccessfully)
            {
                try
                {
                    return new ValueTask<IEnumerable<TValue>>(GetOrAddCore(key, valueFactory));
                }
                finally
                {
                    ExitWriteLock();
                }
            }
            return GetOrAddLazySlowAsync(waitTask, key, valueFactory);
        }

        /// <inheritdoc/>
        public ValueTask CopyToAsync(KeyValuePair<TKey, TValue>[] array, int arrayIndex, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(array, nameof(array));

            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            if (TryEnterReadLockSync())
            {
                try
                {
                    CopyToCore(array, arrayIndex);
                    return ValueTask.CompletedTask;
                }
                finally
                {
                    ExitReadLock();
                }
            }
            return CopyToSlowAsync(array, arrayIndex, cancellationToken);
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
            ArgumentNullException.ThrowIfNull(other, nameof(other));

            ThrowIfDisposed();

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in await other.GetKeysAsync(cancellationToken).ConfigureAwait(false))
            {
                snapshot.Add((key, (await other.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false)).ToArray()));
            }

            await WaitWriteLockAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                UnionCore(snapshot);
            }
            finally
            {
                ExitWriteLock();
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
            ArgumentNullException.ThrowIfNull(other, nameof(other));

            ThrowIfDisposed();

            var otherIndex = new Dictionary<TKey, HashSet<TValue>>();
            foreach (var key in await other.GetKeysAsync(cancellationToken).ConfigureAwait(false))
            {
                otherIndex[key] = new HashSet<TValue>([.. await other.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false)]);
            }

            await WaitWriteLockAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                IntersectCore(otherIndex);
            }
            finally
            {
                ExitWriteLock();
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
            ArgumentNullException.ThrowIfNull(other, nameof(other));

            ThrowIfDisposed();

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in await other.GetKeysAsync(cancellationToken).ConfigureAwait(false))
            {
                snapshot.Add((key, (await other.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false)).ToArray()));
            }

            await WaitWriteLockAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                ExceptWithCore(snapshot);
            }
            finally
            {
                ExitWriteLock();
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
            ArgumentNullException.ThrowIfNull(other, nameof(other));

            ThrowIfDisposed();

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in await other.GetKeysAsync(cancellationToken).ConfigureAwait(false))
            {
                snapshot.Add((key, (await other.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false)).ToArray()));
            }

            await WaitWriteLockAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                SymmetricExceptWithCore(snapshot);
            }
            finally
            {
                ExitWriteLock();
            }
        }
    }
}
