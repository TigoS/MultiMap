using System.Runtime.CompilerServices;
using MultiMap.Helpers;
using MultiMap.Interfaces;

namespace MultiMap.Entities
{
    public sealed partial class MultiMapAsync<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
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
            return obj is IReadOnlyMultiMapAsync<TKey, TValue> other && Equals(other);
        }

        /// <inheritdoc/>
        public bool Equals(IReadOnlyMultiMapAsync<TKey, TValue>? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            ThrowIfDisposed();

            // Fast path: both sides are MultiMapAsync — acquire both semaphores in a
            // consistent order to avoid deadlock, then compare under lock.
            if (other is MultiMapAsync<TKey, TValue> concreteOther)
            {
                concreteOther.ThrowIfDisposed();

                var first = RuntimeHelpers.GetHashCode(this) <= RuntimeHelpers.GetHashCode(concreteOther) ? this : concreteOther;
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
                        {
                            return false;
                        }

                        thisSnapshot = _dictionary.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TValue>(kvp.Value, _valueComparer));
                        otherSnapshot = concreteOther._dictionary.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TValue>(kvp.Value, concreteOther._valueComparer));
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
                    {
                        return false;
                    }

                    if (!kvp.Value.SetEquals(otherSet))
                    {
                        return false;
                    }
                }

                return true;
            }

            // General path: snapshot this instance under its semaphore, then query
            // the other side via the interface API. All foreign async calls are run
            // inside Task.Run so they execute on a thread-pool thread that has no
            // SynchronizationContext — eliminating any deadlock risk regardless of the
            // calling context (UI thread, ASP.NET classic, custom context, etc.).
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

            return Task.Run(async () =>
            {
                int otherCount = await other.GetCountAsync(CancellationToken.None).ConfigureAwait(false);
                int otherKeyCount = await other.GetKeyCountAsync(CancellationToken.None).ConfigureAwait(false);

                if (thisCount != otherCount || snapshot.Count != otherKeyCount)
                {
                    return false;
                }

                foreach (var kvp in snapshot)
                {
                    var (found, otherValues) = await other.TryGetAsync(kvp.Key, CancellationToken.None).ConfigureAwait(false);
                    if (!found)
                    {
                        return false;
                    }

                    var otherSet = otherValues is HashSet<TValue> hs
                        ? hs
                        : new HashSet<TValue>(otherValues, _valueComparer);

                    if (!kvp.Value.SetEquals(otherSet))
                    {
                        return false;
                    }
                }

                return true;
            }).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async ValueTask<bool> EqualsAsync(object? obj) => await EqualsAsync(obj as IReadOnlyMultiMapAsync<TKey, TValue>).ConfigureAwait(false);

        /// <inheritdoc/>
        public async ValueTask<bool> EqualsAsync(IReadOnlyMultiMapAsync<TKey, TValue>? other, CancellationToken cancellationToken = default)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

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
                        {
                            return false;
                        }

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
                    {
                        return false;
                    }

                    if (!kvp.Value.SetEquals(otherSet))
                    {
                        return false;
                    }
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
            {
                return false;
            }

            foreach (var kvp in snapshot)
            {
                var (found, otherValues) = await other.TryGetAsync(kvp.Key, cancellationToken).ConfigureAwait(false);
                if (!found)
                {
                    return false;
                }

                var otherSet = otherValues is HashSet<TValue> hs
                    ? hs
                    : new HashSet<TValue>(otherValues, _valueComparer);

                if (!kvp.Value.SetEquals(otherSet))
                {
                    return false;
                }
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
