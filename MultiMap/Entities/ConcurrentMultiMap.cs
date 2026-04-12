using MultiMap.Interfaces;
using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

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
        private readonly object _globalLock = new();
        private readonly IEqualityComparer<TValue>? _valueComparer;
        private int _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class.
        /// </summary>
        public ConcurrentMultiMap()
        {
            _dictionary = new ConcurrentDictionary<TKey, HashSet<TValue>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class
        /// with the specified initial capacity for keys.
        /// </summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the multimap concurrently.</param>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        public ConcurrentMultiMap(int concurrencyLevel, int capacity)
        {
            _dictionary = new ConcurrentDictionary<TKey, HashSet<TValue>>(concurrencyLevel, capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class
        /// with the specified initial capacity for keys and equality comparer for values.
        /// </summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the multimap concurrently.</param>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public ConcurrentMultiMap(int concurrencyLevel, int capacity, IEqualityComparer<TValue>? valueComparer)
        {
            _dictionary = new ConcurrentDictionary<TKey, HashSet<TValue>>(concurrencyLevel, capacity);
            _valueComparer = valueComparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMultiMap{TKey, TValue}"/> class
        /// with the specified equality comparer for values.
        /// </summary>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public ConcurrentMultiMap(IEqualityComparer<TValue>? valueComparer)
        {
            _dictionary = new ConcurrentDictionary<TKey, HashSet<TValue>>();
            _valueComparer = valueComparer;
        }

        private HashSet<TValue> CreateValueSet() => new(_valueComparer);

        /// <inheritdoc/>
        public bool Add(TKey key, TValue value)
        {
            while (true)
            {
                var hashset = _dictionary.GetOrAdd(key, _ => CreateValueSet());

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
            var items = values as ICollection<TValue> ?? values.ToArray();

            while (true)
            {
                var hashset = _dictionary.GetOrAdd(key, _ => CreateValueSet());

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
        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            foreach (var item in items)
            {
                Add(item.Key, item.Value);
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

            throw new KeyNotFoundException($"The key '{key}' was not found in the multimap.");
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> GetOrDefault(TKey key)
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
            if (_dictionary.TryGetValue(key, out var hashset))
            {
                lock (hashset)
                {
                    values = hashset.ToArray();
                }
                return true;
            }

            values = [];
            return false;
        }

        /// <inheritdoc/>
        public bool Remove(TKey key, TValue value)
        {
            if (!_dictionary.TryGetValue(key, out var hashset))
                return false;

            lock (hashset)
            {
                if (!_dictionary.TryGetValue(key, out var current) || !ReferenceEquals(current, hashset))
                    return false;

                if (!hashset.Remove(value))
                    return false;

                Interlocked.Decrement(ref _count);

                if (hashset.Count == 0)
                {
                    ((ICollection<KeyValuePair<TKey, HashSet<TValue>>>)_dictionary)
                        .Remove(new KeyValuePair<TKey, HashSet<TValue>>(key, hashset));
                }

                return true;
            }
        }

        /// <inheritdoc/>
        public int RemoveRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            int removedCount = 0;

            foreach (var item in items)
            {
                if (Remove(item.Key, item.Value))
                    removedCount++;
            }

            return removedCount;
        }

        /// <inheritdoc/>
        public int RemoveWhere(TKey key, Predicate<TValue> predicate)
        {
            if (!_dictionary.TryGetValue(key, out var hashset))
                return 0;

            lock (hashset)
            {
                if (!_dictionary.TryGetValue(key, out var current) || !ReferenceEquals(current, hashset))
                    return 0;

                int removedCount = hashset.RemoveWhere(predicate);
                Interlocked.Add(ref _count, -removedCount);

                if (hashset.Count == 0)
                {
                    ((ICollection<KeyValuePair<TKey, HashSet<TValue>>>)_dictionary)
                        .Remove(new KeyValuePair<TKey, HashSet<TValue>>(key, hashset));
                }

                return removedCount;
            }
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
        public IEnumerable<TKey> Keys => _dictionary.Keys.ToArray();

        /// <inheritdoc/>
        public int KeyCount => _dictionary.Count;

        /// <inheritdoc/>
        public IEnumerable<TValue> Values
        {
            get
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
                        yield return value;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public int GetValuesCount(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
            {
                lock (hashset)
                {
                    return hashset.Count;
                }
            }

            return 0;
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> this[TKey key] => Get(key);

        /// <inheritdoc/>
        public void Clear()
        {
            lock (_globalLock)
            {
                _dictionary.Clear();
                Interlocked.Exchange(ref _count, 0);
            }
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (_globalLock)
            {
                foreach (var kvp in _dictionary)
                {
                    foreach (var value in kvp.Value.ToArray())
                    {
                        yield return new KeyValuePair<TKey, TValue>(kvp.Key, value);
                    }
                }
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is not ConcurrentMultiMap<TKey, TValue> other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            var thisSnapshot = new Dictionary<TKey, HashSet<TValue>>();
            foreach (var kvp in _dictionary)
            {
                lock (kvp.Value)
                {
                    thisSnapshot[kvp.Key] = new HashSet<TValue>(kvp.Value, _valueComparer);
                }
            }

            var otherSnapshot = new Dictionary<TKey, HashSet<TValue>>();
            foreach (var kvp in other._dictionary)
            {
                lock (kvp.Value)
                {
                    otherSnapshot[kvp.Key] = new HashSet<TValue>(kvp.Value, other._valueComparer);
                }
            }

            if (Count != other.Count || KeyCount != other.KeyCount)
                return false;

            foreach (var kvp in thisSnapshot)
            {
                if (!otherSnapshot.TryGetValue(kvp.Key, out var otherSet))
                    return false;

                if (!kvp.Value.SetEquals(otherSet))
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hash = 0;
            lock (_globalLock)
            {
                foreach (var kvp in _dictionary)
                {
                    int valueHash = 0;
                    foreach (var value in kvp.Value)
                    {
                        valueHash ^= value.GetHashCode();
                    }
                    hash ^= HashCode.Combine(kvp.Key, valueHash);
                }
            }
            return hash;
        }
    }
}
