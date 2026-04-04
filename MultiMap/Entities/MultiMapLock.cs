using MultiMap.Interfaces;
using System.Collections;
using System.Runtime.InteropServices;

namespace MultiMap.Entities
{
    /// <summary>
    /// Provides a thread-safe multi-map collection that associates each key with a set of values, allowing concurrent access and modification.
    /// </summary>
    /// <remarks>
    /// This class uses internal locking to ensure safe concurrent operations.
    /// It is suitable for scenarios where multiple threads need to add, remove, or query key-value associations without external synchronization.
    /// Dispose the instance when no longer needed to release resources.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multi-map. Must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-nullable.</typeparam>
    public class MultiMapLock<TKey, TValue> : IMultiMap<TKey, TValue>, IDisposable
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull
    {
        private readonly Dictionary<TKey, HashSet<TValue>> _dictionary;
        private readonly ReaderWriterLockSlim _lock;
        private int _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapLock{TKey, TValue}"/> class.
        /// </summary>
        public MultiMapLock()
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>();
            _lock = new ReaderWriterLockSlim();
        }

        /// <inheritdoc/>
        public bool Add(TKey key, TValue value)
        {
            _lock.EnterWriteLock();
            try
            {
                ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, key, out bool exists);
                hashset ??= new HashSet<TValue>();

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
        public void AddRange(TKey key, IEnumerable<TValue> values)
        {
            _lock.EnterWriteLock();
            try
            {
                ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, key, out bool exists);
                hashset ??= new HashSet<TValue>();

                foreach (var value in values)
                {
                    if (hashset.Add(value))
                        _count++;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> Get(TKey key)
        {
            _lock.EnterReadLock();
            try
            {
                if (_dictionary.TryGetValue(key, out var hashset))
                    return hashset.ToArray();

                throw new KeyNotFoundException($"The key '{key}' was not found in the multimap.");
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> GetOrDefault(TKey key)
        {
            _lock.EnterReadLock();
            try
            {
                if (_dictionary.TryGetValue(key, out var hashset))
                    return hashset.ToArray();

                return [];
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public bool TryGet(TKey key, out IEnumerable<TValue> values)
        {
            _lock.EnterReadLock();
            try
            {
                bool result = _dictionary.TryGetValue(key, out var hashset);

                values = result ? hashset ?? [] : [];

                return result;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public bool Remove(TKey key, TValue value)
        {
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
                            _dictionary.Remove(key);
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
        public bool RemoveKey(TKey key)
        {
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

        /// <inheritdoc/>
        public bool ContainsKey(TKey key)
        {
            _lock.EnterReadLock();
            try
            {
                return _dictionary.ContainsKey(key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public bool Contains(TKey key, TValue value)
        {
            _lock.EnterReadLock();
            try
            {
                return _dictionary.TryGetValue(key, out var hashset) && hashset.Contains(value);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<TKey> Keys
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _dictionary.Keys.ToArray();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public int KeyCount => throw new NotImplementedException();

        public IEnumerable<TValue> Values => throw new NotImplementedException();

        public IEnumerable<TValue> this[TKey key] => throw new NotImplementedException();

        /// <inheritdoc/>
        public void Clear()
        {
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
                    hashset ??= new HashSet<TValue>();

                    foreach (var value in values)
                    {
                        if (hashset.Add(value))
                            _count++;
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
        /// before the write lock is acquired, avoiding deadlock when <paramref name="other"/> is a
        /// locked collection. The entire read-and-remove phase executes under a single write lock,
        /// so concurrent operations cannot insert values that bypass the intersect filter.
        /// </remarks>
        /// <param name="other">The multi-map that defines the pairs to keep.</param>
        public void Intersect(IMultiMap<TKey, TValue> other)
        {
            var otherIndex = new Dictionary<TKey, HashSet<TValue>>();
            foreach (var key in other.Keys)
            {
                otherIndex[key] = new HashSet<TValue>(other.GetOrDefault(key));
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
                        keysToRemove.Add(kvp.Key);
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
                    hashset ??= new HashSet<TValue>();

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
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            List<KeyValuePair<TKey, TValue>> snapshot;

            _lock.EnterReadLock();
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
                _lock.ExitReadLock();
            }

            return snapshot.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is MultiMapLock<TKey, TValue> map &&
                   EqualityComparer<Dictionary<TKey, HashSet<TValue>>>.Default.Equals(_dictionary, map._dictionary);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(_dictionary);
        }

        /// <summary>
        /// Releases the resources used by the <see cref="MultiMapLock{TKey, TValue}"/> instance.
        /// </summary>
        public void Dispose()
        {
            _lock?.Dispose();
        }

        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            throw new NotImplementedException();
        }

        public int RemoveWhere(TKey key, Predicate<TValue> predicate)
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            throw new NotImplementedException();
        }

        public int GetValuesCount(TKey key)
        {
            throw new NotImplementedException();
        }
    }
}