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
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull
    {
        private readonly ConcurrentDictionary<TKey, HashSet<TValue>> _dictionary;
        private readonly object _globalLock = new();
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
            lock (_globalLock)
            {
                var hashset = _dictionary.GetOrAdd(key, static _ => new HashSet<TValue>());

                lock (hashset)
                {
                    if (hashset.Add(value))
                    {
                        _count++;
                        return true;
                    }

                    return false;
                }
            }
        }

        /// <inheritdoc/>
        public void AddRange(TKey key, IEnumerable<TValue> values)
        {
            lock (_globalLock)
            {
                var hashset = _dictionary.GetOrAdd(key, static _ => new HashSet<TValue>());

                lock (hashset)
                {
                    foreach (var value in values)
                    {
                        if (hashset.Add(value))
                            _count++;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            lock (_globalLock)
            {
                foreach (var item in items)
                {
                    var hashset = _dictionary.GetOrAdd(item.Key, static _ => new HashSet<TValue>());

                    lock (hashset)
                    {
                        if (hashset.Add(item.Value))
                            _count++;
                    }
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
            lock (_globalLock)
            {
                if (_dictionary.TryGetValue(key, out var hashset))
                {
                    lock (hashset)
                    {
                        if (hashset.Remove(value))
                        {
                            _count--;

                            if (hashset.Count == 0)
                                _dictionary.TryRemove(key, out _);

                            return true;
                        }
                    }
                }

                return false;
            }
        }

        /// <inheritdoc/>
        public int RemoveRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            lock (_globalLock)
            {
                int removedCount = 0;
                foreach (var item in items)
                {
                    if (_dictionary.TryGetValue(item.Key, out var hashset))
                    {
                        lock (hashset)
                        {
                            if (hashset.Remove(item.Value))
                            {
                                _count--;
                                removedCount++;

                                if (hashset.Count == 0)
                                    _dictionary.TryRemove(item.Key, out _);
                            }
                        }
                    }
                }

                return removedCount;
            }
        }

        /// <inheritdoc/>
        public int RemoveWhere(TKey key, Predicate<TValue> predicate)
        {
            lock (_globalLock)
            {
                if (!_dictionary.TryGetValue(key, out var hashset))
                    return 0;

                lock (hashset)
                {
                    int removedCount = hashset.RemoveWhere(predicate);
                    _count -= removedCount;

                    if (hashset.Count == 0)
                        _dictionary.TryRemove(key, out _);

                    return removedCount;
                }
            }
        }

        /// <inheritdoc/>
        public bool RemoveKey(TKey key)
        {
            lock (_globalLock)
            {
                if (_dictionary.TryRemove(key, out var hashset))
                {
                    lock (hashset)
                    {
                        _count -= hashset.Count;
                    }
                    return true;
                }

                return false;
            }
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
                _count = 0;
            }
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
            if (obj is not ConcurrentMultiMap<TKey, TValue> other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            var first = RuntimeHelpers.GetHashCode(this) <= RuntimeHelpers.GetHashCode(other) ? this : other;
            var second = ReferenceEquals(first, this) ? other : this;

            Dictionary<TKey, HashSet<TValue>> thisSnapshot;
            Dictionary<TKey, HashSet<TValue>> otherSnapshot;

            lock (first._globalLock)
            {
                lock (second._globalLock)
                {
                    if (_dictionary.Count != other._dictionary.Count)
                        return false;

                    thisSnapshot = _dictionary.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TValue>(kvp.Value));
                    otherSnapshot = other._dictionary.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TValue>(kvp.Value));
                }
            }

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
            lock (_globalLock)
            {
                int hash = 0;
                foreach (var kvp in _dictionary)
                {
                    int valueHash = 0;
                    foreach (var value in kvp.Value)
                    {
                        valueHash ^= value.GetHashCode();
                    }
                    hash ^= HashCode.Combine(kvp.Key, valueHash);
                }
                return hash;
            }
        }
    }
}
