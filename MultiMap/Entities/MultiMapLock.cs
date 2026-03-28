using MultiMap.Interfaces;
using System.Collections;

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
        where TKey : notnull
        where TValue : notnull
    {
        private readonly Dictionary<TKey, HashSet<TValue>> _dictionary;
        private readonly ReaderWriterLockSlim _lock;

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
                if (!_dictionary.TryGetValue(key, out var hashset))
                {
                    hashset = new HashSet<TValue>();
                    _dictionary[key] = hashset;
                }

                return hashset.Add(value);
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
                if (!_dictionary.TryGetValue(key, out var hashset))
                {
                    hashset = new HashSet<TValue>();
                    _dictionary[key] = hashset;
                }

                foreach (var value in values)
                    hashset.Add(value);
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
                    return hashset.ToList(); // snapshot to avoid external modification issues

                return Enumerable.Empty<TValue>();
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

                    if (hashset.Count == 0)
                        _dictionary.Remove(key);

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
                return _dictionary.Remove(key);
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
                    return _dictionary.Sum(kvp => kvp.Value.Count);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _dictionary.Clear();
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
                snapshot = _dictionary
                    .SelectMany(kvp => kvp.Value.Select(v => new KeyValuePair<TKey, TValue>(kvp.Key, v)))
                    .ToList();
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
    }
}