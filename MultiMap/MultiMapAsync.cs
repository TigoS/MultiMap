using System.Runtime.CompilerServices;

namespace MultiMap
{
    public class MultiMapAsync<TKey, TValue> : IAsyncEnumerable<KeyValuePair<TKey, TValue>>, IDisposable
        where TKey : notnull
        where TValue : notnull
    {
        private readonly Dictionary<TKey, HashSet<TValue>> _dictionary;
        private readonly SemaphoreSlim _semaphore;

        public MultiMapAsync()
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>();
            _semaphore = new SemaphoreSlim(1, 1);
        }

        // Add value (no duplicates)
        public async Task<bool> AddAsync(TKey key, TValue value, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
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
                _semaphore.Release();
            }
        }

        public async Task AddRangeAsync(TKey key, IEnumerable<TValue> values, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
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
                _semaphore.Release();
            }
        }

        // Get values for a key (returns snapshot for safety)
        public async Task<IEnumerable<TValue>> GetAsync(TKey key, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_dictionary.TryGetValue(key, out var hashset))
                    return hashset.ToList();

                return Enumerable.Empty<TValue>();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // Remove specific value
        public async Task<bool> RemoveAsync(TKey key, TValue value, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
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
                _semaphore.Release();
            }
        }

        // Remove entire key
        public async Task<bool> RemoveKeyAsync(TKey key, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                return _dictionary.Remove(key);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                return _dictionary.ContainsKey(key);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> ContainsAsync(TKey key, TValue value, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                return _dictionary.TryGetValue(key, out var hashset) && hashset.Contains(value);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                return _dictionary.Sum(kvp => kvp.Value.Count);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                _dictionary.Clear();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // Safe async enumeration (snapshot)
        public async IAsyncEnumerator<KeyValuePair<TKey, TValue>> GetAsyncEnumerator(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            List<KeyValuePair<TKey, TValue>> snapshot;

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                snapshot = _dictionary
                    .SelectMany(kvp => kvp.Value.Select(v => new KeyValuePair<TKey, TValue>(kvp.Key, v)))
                    .ToList();
            }
            finally
            {
                _semaphore.Release();
            }

            foreach (var pair in snapshot)
            {
                yield return pair;
            }
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
        }

        public override bool Equals(object? obj)
        {
            return obj is MultiMapAsync<TKey, TValue> map &&
                   EqualityComparer<Dictionary<TKey, HashSet<TValue>>>.Default.Equals(_dictionary, map._dictionary);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_dictionary);
        }
    }
}
