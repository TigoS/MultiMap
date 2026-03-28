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
        where TKey : notnull
        where TValue : notnull
    {
        private SortedDictionary<TKey, SortedSet<TValue>> _dictionary;
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
            if (!_dictionary.TryGetValue(key, out var set))
            {
                set = new SortedSet<TValue>();
                _dictionary[key] = set;
            }

            if (set.Add(value))
            {
                _count++;
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public void AddRange(TKey key, IEnumerable<TValue> values)
        {
            if (!_dictionary.TryGetValue(key, out var set))
            {
                set = new SortedSet<TValue>();
                _dictionary[key] = set;
            }

            foreach (var value in values)
            {
                if (set.Add(value))
                    _count++;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> Get(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var set))
                return set;

            return [];
        }

        /// <inheritdoc/>
        public bool Remove(TKey key, TValue value)
        {
            if (_dictionary.TryGetValue(key, out var set))
            {
                bool removed = set.Remove(value);

                if (removed)
                {
                    _count--;
                    if (set.Count == 0)
                        _dictionary.Remove(key);
                }

                return removed;
            }

            return false;
        }

        /// <inheritdoc/>
        public bool RemoveKey(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var set))
            {
                _count -= set.Count;
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
            return _dictionary.TryGetValue(key, out var set) && set.Contains(value);
        }

        /// <inheritdoc/>
        public int Count => _count;

        /// <inheritdoc/>
        public IEnumerable<TKey> Keys => _dictionary.Keys;

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
            return obj is SortedMultiMap<TKey, TValue> map &&
                   EqualityComparer<SortedDictionary<TKey, SortedSet<TValue>>>.Default.Equals(_dictionary, map._dictionary);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(_dictionary);
        }
    }
}
