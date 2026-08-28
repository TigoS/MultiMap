using System.Runtime.InteropServices;
using MultiMap.Helpers;
using MultiMap.Interfaces;

namespace MultiMap.Entities
{
    public sealed partial class MultiMapLock<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        /// <summary>
        /// Atomically adds all key-value pairs from <paramref name="other"/> into this multi-map.
        /// </summary>
        /// <remarks>
        /// The data from <paramref name="other"/> is snapshotted before the write lock is acquired,
        /// so <paramref name="other"/> may be the same instance or another locked collection without
        /// risk of deadlock. The entire mutation phase executes under a single write lock, guaranteeing
        /// that no concurrent reader or writer can observe a partial union.
        /// </remarks>
        /// <param name="other">The multi-map whose pairs are added to this instance.</param>
        public void Union(IMultiMap<TKey, TValue> other)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in other.Keys)
            {
                snapshot.Add((key, other.GetOrDefault(key).ToArray()));
            }

            _lock.EnterWriteLock();
            try
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
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Adds all key-value pairs from <paramref name="other"/> into this multi-map,
        /// waiting for the write lock with cancellation support.
        /// </summary>
        /// <remarks>
        /// <paramref name="other"/> is snapshotted before the write lock is acquired, so it may be
        /// the same instance or another locked collection without risk of deadlock. The entire
        /// mutation phase executes under a single write lock.
        /// </remarks>
        /// <param name="other">The multi-map whose pairs are added to this instance.</param>
        /// <param name="cancellationToken">
        /// A token to cancel waiting for the write lock.
        /// Checked at each polling interval (~20 ms); throws <see cref="OperationCanceledException"/>
        /// if cancelled before the lock is acquired. The map is not modified on cancellation.
        /// </param>
        public void Union(IMultiMap<TKey, TValue> other, CancellationToken cancellationToken)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in other.Keys)
            {
                snapshot.Add((key, other.GetOrDefault(key).ToArray()));
            }

            EnterWriteLockCancellable(cancellationToken);
            try
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
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Atomically removes all key-value pairs from this multi-map that do not exist in <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// The membership of <paramref name="other"/> is snapshotted into a dictionary of hash sets
        /// before the write lock is acquired, so <paramref name="other"/> may be the same instance or
        /// another locked collection without risk of deadlock. The entire read-and-remove phase executes
        /// under a single write lock, so concurrent operations cannot insert values that bypass the
        /// intersect filter.
        /// </remarks>
        /// <param name="other">The multi-map that defines the pairs to keep.</param>
        public void Intersect(IMultiMap<TKey, TValue> other)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            var otherIndex = new Dictionary<TKey, HashSet<TValue>>();
            foreach (var key in other.Keys)
            {
                otherIndex[key] = new HashSet<TValue>([.. other.GetOrDefault(key)]);
            }

            _lock.EnterWriteLock();
            try
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
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes all key-value pairs from this multi-map that do not exist in <paramref name="other"/>,
        /// waiting for the write lock with cancellation support.
        /// </summary>
        /// <remarks>
        /// <paramref name="other"/> is snapshotted before the write lock is acquired. The entire
        /// read-and-remove phase executes under a single write lock.
        /// </remarks>
        /// <param name="other">The multi-map that defines the pairs to keep.</param>
        /// <param name="cancellationToken">
        /// A token to cancel waiting for the write lock.
        /// Checked at each polling interval (~20 ms); throws <see cref="OperationCanceledException"/>
        /// if cancelled before the lock is acquired. The map is not modified on cancellation.
        /// </param>
        public void Intersect(IMultiMap<TKey, TValue> other, CancellationToken cancellationToken)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            var otherIndex = new Dictionary<TKey, HashSet<TValue>>();
            foreach (var key in other.Keys)
            {
                otherIndex[key] = new HashSet<TValue>([.. other.GetOrDefault(key)]);
            }

            EnterWriteLockCancellable(cancellationToken);
            try
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
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Atomically removes all key-value pairs from this multi-map that exist in <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// The data from <paramref name="other"/> is snapshotted before the write lock is acquired,
        /// so <paramref name="other"/> may be the same instance or another locked collection without
        /// risk of deadlock. The entire mutation phase executes under a single write lock, guaranteeing
        /// that no concurrent reader or writer can observe a partial removal.
        /// </remarks>
        /// <param name="other">The multi-map whose pairs are removed from this instance.</param>
        public void ExceptWith(IMultiMap<TKey, TValue> other)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in other.Keys)
            {
                snapshot.Add((key, other.GetOrDefault(key).ToArray()));
            }

            _lock.EnterWriteLock();
            try
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
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes all key-value pairs from this multi-map that exist in <paramref name="other"/>,
        /// waiting for the write lock with cancellation support.
        /// </summary>
        /// <remarks>
        /// <paramref name="other"/> is snapshotted before the write lock is acquired. The entire
        /// mutation phase executes under a single write lock.
        /// </remarks>
        /// <param name="other">The multi-map whose pairs are removed from this instance.</param>
        /// <param name="cancellationToken">
        /// A token to cancel waiting for the write lock.
        /// Checked at each polling interval (~20 ms); throws <see cref="OperationCanceledException"/>
        /// if cancelled before the lock is acquired. The map is not modified on cancellation.
        /// </param>
        public void ExceptWith(IMultiMap<TKey, TValue> other, CancellationToken cancellationToken)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in other.Keys)
            {
                snapshot.Add((key, other.GetOrDefault(key).ToArray()));
            }

            EnterWriteLockCancellable(cancellationToken);
            try
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
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Atomically modifies this multi-map to contain only pairs present in either this instance
        /// or <paramref name="other"/>, but not both.
        /// </summary>
        /// <remarks>
        /// The data from <paramref name="other"/> is snapshotted before the write lock is acquired,
        /// so <paramref name="other"/> may be the same instance or another locked collection without
        /// risk of deadlock. Classification (common vs. unique) and all mutations execute under a
        /// single write lock, guaranteeing full atomicity.
        /// </remarks>
        /// <param name="other">The multi-map to compare against.</param>
        public void SymmetricExceptWith(IMultiMap<TKey, TValue> other)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in other.Keys)
            {
                snapshot.Add((key, other.GetOrDefault(key).ToArray()));
            }

            _lock.EnterWriteLock();
            try
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
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Modifies this multi-map to contain only pairs present in either this instance or
        /// <paramref name="other"/>, but not both, waiting for the write lock with cancellation support.
        /// </summary>
        /// <remarks>
        /// <paramref name="other"/> is snapshotted before the write lock is acquired. Classification
        /// and all mutations execute under a single write lock.
        /// </remarks>
        /// <param name="other">The multi-map to compare against.</param>
        /// <param name="cancellationToken">
        /// A token to cancel waiting for the write lock.
        /// Checked at each polling interval (~20 ms); throws <see cref="OperationCanceledException"/>
        /// if cancelled before the lock is acquired. The map is not modified on cancellation.
        /// </param>
        public void SymmetricExceptWith(IMultiMap<TKey, TValue> other, CancellationToken cancellationToken)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in other.Keys)
            {
                snapshot.Add((key, other.GetOrDefault(key).ToArray()));
            }

            EnterWriteLockCancellable(cancellationToken);
            try
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
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Atomically determines whether this multi-map is a subset of <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// The data from <paramref name="other"/> is snapshotted before the read lock is acquired,
        /// so <paramref name="other"/> may be the same instance or another locked collection without
        /// risk of deadlock. The entire comparison executes under a single read lock, guaranteeing
        /// consistent results.
        /// </remarks>
        /// <param name="other">The multi-map to compare against.</param>
        /// <returns><see langword="true"/> if every key-value pair in this instance exists in <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public bool IsSubsetOf(IMultiMap<TKey, TValue> other)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var otherIndex = new Dictionary<TKey, HashSet<TValue>>();
            foreach (var key in other.Keys)
            {
                var values = other.GetOrDefault(key).ToArray();
                otherIndex[key] = new HashSet<TValue>(values, _valueComparer);
            }

            _lock.EnterReadLock();
            try
            {
                foreach (var kvp in _dictionary)
                {
                    if (!otherIndex.TryGetValue(kvp.Key, out var otherSet) || otherSet.Count == 0)
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
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Atomically determines whether this multi-map is a superset of <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// The data from <paramref name="other"/> is snapshotted before the read lock is acquired,
        /// so <paramref name="other"/> may be the same instance or another locked collection without
        /// risk of deadlock. The entire comparison executes under a single read lock, guaranteeing
        /// consistent results.
        /// </remarks>
        /// <param name="other">The multi-map to compare against.</param>
        /// <returns><see langword="true"/> if every key-value pair in <paramref name="other"/> exists in this instance; otherwise, <see langword="false"/>.</returns>
        public bool IsSupersetOf(IMultiMap<TKey, TValue> other)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in other.Keys)
            {
                snapshot.Add((key, other.GetOrDefault(key).ToArray()));
            }

            _lock.EnterReadLock();
            try
            {
                foreach (var (key, values) in snapshot)
                {
                    if (!_dictionary.TryGetValue(key, out var thisSet))
                    {
                        return false;
                    }

                    foreach (var value in values)
                    {
                        if (!thisSet.Contains(value))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Atomically determines whether this multi-map and <paramref name="other"/> share at least one key-value pair.
        /// </summary>
        /// <remarks>
        /// The data from <paramref name="other"/> is snapshotted before the read lock is acquired,
        /// so <paramref name="other"/> may be the same instance or another locked collection without
        /// risk of deadlock. The entire comparison executes under a single read lock, guaranteeing
        /// consistent results.
        /// </remarks>
        /// <param name="other">The multi-map to compare against.</param>
        /// <returns><see langword="true"/> if at least one key-value pair exists in both multimaps; otherwise, <see langword="false"/>.</returns>
        public bool Overlaps(IMultiMap<TKey, TValue> other)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            if (ReferenceEquals(this, other))
            {
                return Count > 0;
            }

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in other.Keys)
            {
                snapshot.Add((key, other.GetOrDefault(key).ToArray()));
            }

            _lock.EnterReadLock();
            try
            {
                foreach (var (key, values) in snapshot)
                {
                    if (_dictionary.TryGetValue(key, out var thisSet))
                    {
                        foreach (var value in values)
                        {
                            if (thisSet.Contains(value))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Atomically determines whether this multi-map and <paramref name="other"/> contain the same key-value pairs.
        /// </summary>
        /// <remarks>
        /// The data from <paramref name="other"/> is snapshotted before the read lock is acquired,
        /// so <paramref name="other"/> may be the same instance or another locked collection without
        /// risk of deadlock. The entire comparison executes under a single read lock, guaranteeing
        /// consistent results.
        /// </remarks>
        /// <param name="other">The multi-map to compare against.</param>
        /// <returns><see langword="true"/> if both multimaps contain exactly the same key-value pairs; otherwise, <see langword="false"/>.</returns>
        public bool SetEquals(IMultiMap<TKey, TValue> other)
        {
            Guard.NotNull(other, nameof(other));

            ThrowIfDisposed();

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other.Count != Count || other.KeyCount != KeyCount)
            {
                return false;
            }

            var snapshot = new List<(TKey Key, TValue[] Values)>();
            foreach (var key in other.Keys)
            {
                snapshot.Add((key, other.GetOrDefault(key).ToArray()));
            }

            _lock.EnterReadLock();
            try
            {
                if (_dictionary.Count != snapshot.Count)
                {
                    return false;
                }

                foreach (var (key, values) in snapshot)
                {
                    if (!_dictionary.TryGetValue(key, out var thisSet) ||
                        thisSet.Count != values.Length)
                    {
                        return false;
                    }

                    var otherHashSet = new HashSet<TValue>(values, _valueComparer);
                    if (!thisSet.SetEquals(otherHashSet))
                    {
                        return false;
                    }
                }

                return true;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}
