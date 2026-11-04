namespace MultiMap.Entities
{
    public sealed partial class MultiMapLock<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        /// <inheritdoc/>
        public IEnumerable<TValue> Get(TKey key)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));

            ThrowIfDisposed();

            _lock.EnterReadLock();
            try
            {
                return _dictionary.TryGetValue(key, out var hashset) ? hashset.ToArray() : throw new KeyNotFoundException($"The key '{key}' was not found in the multimap.");
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> GetOrDefault(TKey key)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));

            ThrowIfDisposed();

            _lock.EnterReadLock();
            try
            {
                return _dictionary.TryGetValue(key, out var hashset) ? [.. hashset] : [];
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public bool TryGet(TKey key, out IEnumerable<TValue> values)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));

            ThrowIfDisposed();

            _lock.EnterReadLock();
            try
            {
                bool result = _dictionary.TryGetValue(key, out var hashset);

                values = result ? hashset?.ToArray() ?? [] : [];

                return result;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public bool ContainsKey(TKey key)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));

            ThrowIfDisposed();

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
            ArgumentNullException.ThrowIfNull(key, nameof(key));
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            ThrowIfDisposed();

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
                ThrowIfDisposed();
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
                ThrowIfDisposed();
                _lock.EnterReadLock();
                try
                {
                    return [.. _dictionary.Keys];
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc/>
        public int KeyCount
        {
            get
            {
                ThrowIfDisposed();
                _lock.EnterReadLock();
                try
                {
                    return _dictionary.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> Values
        {
            get
            {
                ThrowIfDisposed();
                _lock.EnterReadLock();
                try
                {
                    var result = new TValue[_count];
                    var index = 0;
                    foreach (var hashset in _dictionary.Values)
                    {
                        foreach (var value in hashset)
                        {
                            result[index++] = value;
                        }
                    }
                    return result;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc/>
        public int GetValuesCount(TKey key)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));

            ThrowIfDisposed();

            _lock.EnterReadLock();
            try
            {
                return _dictionary.TryGetValue(key, out var hashset) ? hashset.Count : 0;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> this[TKey key] => Get(key);
    }
}
