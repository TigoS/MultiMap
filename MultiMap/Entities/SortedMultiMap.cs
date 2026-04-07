using MultiMap.Interfaces;
using System.Collections;

namespace MultiMap.Entities
{
    /// <summary>
    /// Represents a collection that associates each key with a sorted set of values,
    /// allowing multiple values per key and maintaining both keys and values in sorted order.
    /// </summary>
    /// <remarks>
    /// The keys and values are stored in sorted order according to their natural comparer or a specified comparer.
    /// This type is useful when you need to maintain multiple values per key and require predictable ordering for both keys and values.
    /// Thread safety is not guaranteed; external synchronization is required for concurrent access.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multi-map. Must be non-null and support sorting.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-null and support sorting.</typeparam>
    public class SortedMultiMap<TKey, TValue> : IMultiMap<TKey, TValue>
        where TKey : notnull, IEquatable<TKey>, IComparable<TKey>
        where TValue : notnull, IComparable<TValue>
    {
        private readonly SortedDictionary<TKey, SortedSet<TValue>> _dictionary;
        private int _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedMultiMap{TKey, TValue}"/> class.
        /// </summary>
        public SortedMultiMap()
        {
            _dictionary = new SortedDictionary<TKey, SortedSet<TValue>>();
        }

        /// <inheritdoc/>
        public bool Add(TKey key, TValue value)
        {
            if (!_dictionary.TryGetValue(key, out var sortedSet))
            {
                sortedSet = new SortedSet<TValue>();
                _dictionary[key] = sortedSet;
            }

            if (sortedSet.Add(value))
            {
                _count++;
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public void AddRange(TKey key, IEnumerable<TValue> values)
        {
            if (!_dictionary.TryGetValue(key, out var sortedSet))
            {
                sortedSet = new SortedSet<TValue>();
                _dictionary[key] = sortedSet;
            }

            foreach (var value in values)
            {
                if (sortedSet.Add(value))
                    _count++;
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
            if (_dictionary.TryGetValue(key, out var sortedSet))
                return sortedSet.ToArray();

            throw new KeyNotFoundException($"The key '{key}' was not found in the multimap.");
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> GetOrDefault(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var sortedSet))
                return sortedSet.ToArray();

            return [];
        }

        /// <inheritdoc/>
        public bool TryGet(TKey key, out IEnumerable<TValue> values)
        {
            bool result = _dictionary.TryGetValue(key, out var sortedSet);

            values = result ? sortedSet?.ToArray() ?? [] : [];

            return result;
        }

        /// <inheritdoc/>
        public bool Remove(TKey key, TValue value)
        {
            if (_dictionary.TryGetValue(key, out var sortedSet))
            {
                bool removed = sortedSet.Remove(value);

                if (removed)
                {
                    _count--;
                    if (sortedSet.Count == 0)
                        _dictionary.Remove(key);
                }

                return removed;
            }

            return false;
        }

        /// <inheritdoc/>
        public int RemoveRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            int removedCount = 0;
            foreach (var item in items)
            {
                if (Remove(item.Key, item.Value))
                {
                    removedCount++;
                }
            }

            return removedCount;
        }

        /// <inheritdoc/>
        public int RemoveWhere(TKey key, Predicate<TValue> predicate)
        {
            if (!_dictionary.TryGetValue(key, out var sortedSet))
                return 0;

            int removedCount = sortedSet.RemoveWhere(predicate);
            _count -= removedCount;

            if (sortedSet.Count == 0)
                _dictionary.Remove(key);

            return removedCount;
        }

        /// <inheritdoc/>
        public bool RemoveKey(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var sortedSet))
            {
                _count -= sortedSet.Count;
                return _dictionary.Remove(key);
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
            return _dictionary.TryGetValue(key, out var sortedSet) && sortedSet.Contains(value);
        }

        /// <inheritdoc/>
        public int Count => _count;

        /// <inheritdoc/>
        public IEnumerable<TKey> Keys => _dictionary.Keys;

        /// <inheritdoc/>
        public int KeyCount => _dictionary.Count;

        /// <inheritdoc/>
        public IEnumerable<TValue> Values => _dictionary.Values.SelectMany(sortedSet => sortedSet);

        /// <inheritdoc/>
        public int GetValuesCount(TKey key) => _dictionary.TryGetValue(key, out var sortedSet) ? sortedSet.Count : 0;

        /// <inheritdoc/>
        public IEnumerable<TValue> this[TKey key] => Get(key);

        /// <inheritdoc/>
        public void Clear()
        {
            _dictionary.Clear();
            _count = 0;
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var kvp in _dictionary)
            {
                foreach (var value in kvp.Value)
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
            if (obj is not SortedMultiMap<TKey, TValue> other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (_count != other._count || _dictionary.Count != other._dictionary.Count)
                return false;

            foreach (var kvp in _dictionary)
            {
                if (!other._dictionary.TryGetValue(kvp.Key, out var otherSet))
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
