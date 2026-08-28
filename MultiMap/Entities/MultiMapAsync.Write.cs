using MultiMap.Helpers;
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
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(value, nameof(value));

            ThrowIfDisposed();

            Task waitTask = _writeLock.WaitAsync(cancellationToken);
            if (waitTask.IsCompletedSuccessfully)
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
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(values, nameof(values));

            ThrowIfDisposed();
            Task waitTask = _writeLock.WaitAsync(cancellationToken);
            if (waitTask.IsCompletedSuccessfully)
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
            Guard.NotNull(items, nameof(items));

            ThrowIfDisposed();

            Task waitTask = _writeLock.WaitAsync(cancellationToken);
            if (waitTask.IsCompletedSuccessfully)
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
        public ValueTask<bool> RemoveAsync(TKey key, TValue value, CancellationToken cancellationToken = default)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(value, nameof(value));

            ThrowIfDisposed();
            Task waitTask = _writeLock.WaitAsync(cancellationToken);
            if (waitTask.IsCompletedSuccessfully)
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
            Guard.NotNull(items, nameof(items));

            ThrowIfDisposed();

            Task waitTask = _writeLock.WaitAsync(cancellationToken);
            if (waitTask.IsCompletedSuccessfully)
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
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(predicate, nameof(predicate));

            ThrowIfDisposed();
            Task waitTask = _writeLock.WaitAsync(cancellationToken);
            if (waitTask.IsCompletedSuccessfully)
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
            Guard.NotNull(key, nameof(key));

            ThrowIfDisposed();
            Task waitTask = _writeLock.WaitAsync(cancellationToken);
            if (waitTask.IsCompletedSuccessfully)
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
        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            Task waitTask = _writeLock.WaitAsync(cancellationToken);
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
            Guard.NotNull(other, nameof(other));

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
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            var otherIndex = new Dictionary<TKey, HashSet<TValue>>();
            foreach (var key in await other.GetKeysAsync(cancellationToken).ConfigureAwait(false))
            {
                otherIndex[key] = new HashSet<TValue>([.. await other.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false)]);
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
            Guard.NotNull(other, nameof(other));

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
            Guard.NotNull(other, nameof(other));

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
    }
}
