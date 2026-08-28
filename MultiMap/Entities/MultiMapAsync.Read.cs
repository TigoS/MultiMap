using MultiMap.Helpers;

namespace MultiMap.Entities
{
    public sealed partial class MultiMapAsync<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        /// <inheritdoc/>
        public ValueTask<IEnumerable<TValue>> GetAsync(TKey key, CancellationToken cancellationToken = default)
        {
            Guard.NotNull(key, nameof(key));

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
            Guard.NotNull(key, nameof(key));

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
            Guard.NotNull(key, nameof(key));

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
        public ValueTask<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken = default)
        {
            Guard.NotNull(key, nameof(key));

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
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(value, nameof(value));

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
                    return new ValueTask<IEnumerable<TKey>>([.. _dictionary.Keys]);
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
            Guard.NotNull(key, nameof(key));

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
    }
}
