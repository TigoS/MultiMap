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
        /// Atomically determines whether this multi-map is a subset of <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// The data from both multimaps is snapshotted via async interfaces before comparison.
        /// The entire read phase executes under a single semaphore hold on this instance, guaranteeing
        /// that no concurrent caller can observe partial data. When <paramref name="other"/> is also
        /// a <see cref="MultiMapAsync{TKey,TValue}"/>, both semaphores are acquired in a stable order
        /// to prevent deadlock.
        /// </remarks>
        /// <param name="other">The multi-map to compare against.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns><see langword="true"/> if every key-value pair in this instance exists in <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> IsSubsetOfAsync(IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
        {
            Guard.NotNull(other, nameof(other));

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

                    foreach (var value in kvp.Value)
                    {
                        if (!otherSet.Contains(value))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            // General path: snapshot this instance, then compare asynchronously via the interface API.
            Dictionary<TKey, HashSet<TValue>> snapshot;

            await EnterReadLockAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                snapshot = _dictionary.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TValue>(kvp.Value, _valueComparer));
            }
            finally
            {
                ExitReadLock();
            }

            foreach (var kvp in snapshot)
            {
                var otherValues = await other.GetOrDefaultAsync(kvp.Key, cancellationToken).ConfigureAwait(false);
                var otherSet = otherValues is HashSet<TValue> hs
                    ? hs
                    : new HashSet<TValue>(otherValues, _valueComparer);

                foreach (var value in kvp.Value)
                {
                    if (!otherSet.Contains(value))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Atomically determines whether this multi-map is a superset of <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// The data from both multimaps is snapshotted via async interfaces before comparison.
        /// This method delegates to <see cref="IsSubsetOfAsync"/> with reversed arguments.
        /// </remarks>
        /// <param name="other">The multi-map to compare against.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns><see langword="true"/> if every key-value pair in <paramref name="other"/> exists in this instance; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> IsSupersetOfAsync(IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            if (other is MultiMapAsync<TKey, TValue> concreteOther)
            {
                return await concreteOther.IsSubsetOfAsync(this, cancellationToken).ConfigureAwait(false);
            }

            return await other.IsSubsetOfAsync(this, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Atomically determines whether this multi-map and <paramref name="other"/> share at least one key-value pair.
        /// </summary>
        /// <remarks>
        /// The data from both multimaps is snapshotted via async interfaces before comparison.
        /// The entire read phase executes under a single semaphore hold on this instance, guaranteeing
        /// that no concurrent caller can observe partial data. When <paramref name="other"/> is also
        /// a <see cref="MultiMapAsync{TKey,TValue}"/>, both semaphores are acquired in a stable order
        /// to prevent deadlock.
        /// </remarks>
        /// <param name="other">The multi-map to compare against.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns><see langword="true"/> if at least one key-value pair exists in both multimaps; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> OverlapsAsync(IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
        {
            Guard.NotNull(other, nameof(other));

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
                    if (otherSnapshot.TryGetValue(kvp.Key, out var otherSet))
                    {
                        foreach (var value in kvp.Value)
                        {
                            if (otherSet.Contains(value))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            // General path: snapshot this instance, then compare asynchronously via the interface API.
            Dictionary<TKey, HashSet<TValue>> snapshot;

            await EnterReadLockAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                snapshot = _dictionary.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TValue>(kvp.Value, _valueComparer));
            }
            finally
            {
                ExitReadLock();
            }

            foreach (var kvp in snapshot)
            {
                var otherValues = await other.GetOrDefaultAsync(kvp.Key, cancellationToken).ConfigureAwait(false);
                var otherSet = otherValues is HashSet<TValue> hs
                    ? hs
                    : new HashSet<TValue>(otherValues, _valueComparer);

                foreach (var value in kvp.Value)
                {
                    if (otherSet.Contains(value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Atomically determines whether this multi-map and <paramref name="other"/> contain the same key-value pairs.
        /// </summary>
        /// <remarks>
        /// The data from both multimaps is snapshotted via async interfaces before comparison.
        /// The entire read phase executes under a single semaphore hold on this instance, guaranteeing
        /// that no concurrent caller can observe partial data. When <paramref name="other"/> is also
        /// a <see cref="MultiMapAsync{TKey,TValue}"/>, both semaphores are acquired in a stable order
        /// to prevent deadlock.
        /// </remarks>
        /// <param name="other">The multi-map to compare against.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns><see langword="true"/> if both multimaps contain exactly the same key-value pairs; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> SetEqualsAsync(IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
        {
            Guard.NotNull(other, nameof(other));

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

            // General path: snapshot this instance, then compare asynchronously via the interface API.
            Dictionary<TKey, HashSet<TValue>> snapshot;
            int thisCount;

            await EnterReadLockAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                thisCount = _count;
                snapshot = _dictionary.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TValue>(kvp.Value, _valueComparer));
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
    }
}
