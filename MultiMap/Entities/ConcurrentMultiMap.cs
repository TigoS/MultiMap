using MultiMap.Interfaces;
using System.Collections;
using System.Collections.Concurrent;

namespace MultiMap.Entities
{
    /// <summary>
    /// Represents a thread-safe multi-map collection that associates each key with a set of values.
    /// Supports concurrent operations for adding, removing, and querying key-value pairs.
    /// </summary>
    /// <remarks>
    /// ConcurrentMultiMap is designed for scenarios where multiple threads may add or remove key-value pairs simultaneously.
    /// Each key maps to a set of unique values, and all operations are safe for concurrent access.
    /// This class is useful for managing collections where keys can have multiple associated values and thread safety is required.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multi-map. Must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-nullable.</typeparam>
    public class ConcurrentMultiMap<TKey, TValue> : IMultiMap<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        private readonly ConcurrentDictionary<TKey, HashSet<TValue>> _dictionary;
        private int _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class.
        /// </summary>
        public ConcurrentMultiMap()
        {
            _dictionary = new ConcurrentDictionary<TKey, HashSet<TValue>>();
        }

        /// <inheritdoc/>
        public bool Add(TKey key, TValue value)
        {
            while (true)
            {
                if (!_dictionary.TryGetValue(key, out var hashset))
                    hashset = _dictionary.GetOrAdd(key, static _ => new HashSet<TValue>());

                lock (hashset)
                {
                    if (!_dictionary.TryGetValue(key, out var current) || !ReferenceEquals(current, hashset))
                        continue;

                    if (hashset.Add(value))
                    {
                        Interlocked.Increment(ref _count);
                        return true;
                    }

                    return false;
                }
            }
        }

        /// <inheritdoc/>
        public void AddRange(TKey key, IEnumerable<TValue> values)
        {
            var items = values as ICollection<TValue> ?? [.. values];

            while (true)
            {
                if (!_dictionary.TryGetValue(key, out var hashset))
                    hashset = _dictionary.GetOrAdd(key, static _ => new HashSet<TValue>());

                lock (hashset)
                {
                    if (!_dictionary.TryGetValue(key, out var current) || !ReferenceEquals(current, hashset))
                        continue;

                    foreach (var value in items)
                    {
                        if (hashset.Add(value))
                            Interlocked.Increment(ref _count);
                    }

                    return;
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> Get(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
            {
                lock (hashset)
                {
                    return hashset.ToArray();
                }
            }

            return [];
        }

        /// <inheritdoc/>
        public bool TryGet(TKey key, out IEnumerable<TValue> values)
        {
            bool result = _dictionary.TryGetValue(key, out var hashset);

            var tmpValues = result ? hashset ?? [] : [];
            lock (tmpValues)
            {
                values = tmpValues.ToArray();
            }

            return result;
        }

        /// <inheritdoc/>
        public bool Remove(TKey key, TValue value)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
            {
                lock (hashset)
                {
                    if (!_dictionary.TryGetValue(key, out var current) || !ReferenceEquals(current, hashset))
                        return false;

                    if (hashset.Remove(value))
                    {
                        Interlocked.Decrement(ref _count);

                        if (hashset.Count == 0)
                            _dictionary.TryRemove(new KeyValuePair<TKey, HashSet<TValue>>(key, hashset));

                        return true;
                    }
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public bool RemoveKey(TKey key)
        {
            if (_dictionary.TryRemove(key, out var hashset))
            {
                lock (hashset)
                {
                    Interlocked.Add(ref _count, -hashset.Count);
                }
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <inheritdoc/>
        public bool Contains(TKey key, TValue value)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
            {
                lock (hashset)
                {
                    return hashset.Contains(value);
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public int Count => Volatile.Read(ref _count);

        /// <inheritdoc/>
        public IEnumerable<TKey> Keys => _dictionary.Keys;

        /// <inheritdoc/>
        public void Clear()
        {
            _dictionary.Clear();
            Volatile.Write(ref _count, 0);
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var kvp in _dictionary)
            {
                TValue[] snapshot;
                lock (kvp.Value)
                {
                    snapshot = [.. kvp.Value];
                }

                foreach (var value in snapshot)
                {
                    yield return new KeyValuePair<TKey, TValue>(kvp.Key, value);
                }
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is ConcurrentMultiMap<TKey, TValue> map &&
                   EqualityComparer<ConcurrentDictionary<TKey, HashSet<TValue>>>.Default.Equals(_dictionary, map._dictionary);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(_dictionary);
        }
    }
}
