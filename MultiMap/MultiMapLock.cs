using System.Collections;

namespace MultiMap
{
    public class MultiMapLock<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable
        where TKey : notnull
        where TValue : notnull
    {
        private readonly Dictionary<TKey, HashSet<TValue>> _dictionary;
        private readonly ReaderWriterLockSlim _lock;

        public MultiMapLock()
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>();
            _lock = new ReaderWriterLockSlim();
        }

        // Add value (no duplicates)
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

        // Get values for a key (returns snapshot for safety)
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

        // Remove specific value
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

        // Remove entire key
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

        // Safe enumeration (snapshot)
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

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override bool Equals(object? obj)
        {
            return obj is MultiMapLock<TKey, TValue> map &&
                   EqualityComparer<Dictionary<TKey, HashSet<TValue>>>.Default.Equals(_dictionary, map._dictionary);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_dictionary);
        }

        public void Dispose()
        {
            _lock?.Dispose();
        }
    }
}