using MultiMap.Interfaces;
using System.Collections;
#if NET6_0_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace MultiMap.Entities
{
    /// <summary>
    /// Represents a collection that maps keys to sets of values, allowing multiple values to be associated with each key.
    /// Provides set semantics for values, ensuring that each value is unique per key.
    /// </summary>
    /// <remarks>
    /// This implementation uses a dictionary of hash sets to store values for each key, providing efficient lookup and uniqueness enforcement.
    /// Values associated with a key are unordered and duplicates are not allowed.
    /// The class is not thread-safe; external synchronization is required for concurrent access.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multimap. Must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The type of values in the multimap. Must be non-nullable.</typeparam>
    public class MultiMapSet<TKey, TValue> : IMultiMap<TKey, TValue>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull
    {
        private readonly Dictionary<TKey, HashSet<TValue>> _dictionary;
        private int _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapSet{TKey, TValue}"/> class.
        /// </summary>
        public MultiMapSet()
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>();
        }

        /// <inheritdoc/>
        public bool Add(TKey key, TValue value)
        {
#if NET6_0_OR_GREATER
            ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, key, out bool exists);
            hashset ??= new HashSet<TValue>();
#else
            if (!_dictionary.TryGetValue(key, out var hashset))
            {
                hashset = new HashSet<TValue>();
                _dictionary[key] = hashset;
            }
#endif

            if (hashset.Add(value))
            {
                _count++;
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public void AddRange(TKey key, IEnumerable<TValue> values)
        {
#if NET6_0_OR_GREATER
            ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, key, out bool exists);
            hashset ??= new HashSet<TValue>();
#else
            if (!_dictionary.TryGetValue(key, out var hashset))
            {
                hashset = new HashSet<TValue>();
                _dictionary[key] = hashset;
            }
#endif

            foreach (var value in values)
            {
                if (hashset.Add(value))
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
            if (_dictionary.TryGetValue(key, out var hashset))
                return hashset.ToArray();

            throw new KeyNotFoundException($"The key '{key}' was not found in the multimap.");
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> GetOrDefault(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
                return hashset.ToArray();

            return [];
        }

        /// <inheritdoc/>
        public bool TryGet(TKey key, out IEnumerable<TValue> values)
        {
            bool result = _dictionary.TryGetValue(key, out var hashset);

            values = result ? hashset?.ToArray() ?? [] : [];

            return result;
        }

        /// <inheritdoc/>
        public bool Remove(TKey key, TValue value)
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
            if (!_dictionary.TryGetValue(key, out var hashset))
                return 0;

            int removedCount = hashset.RemoveWhere(predicate);
            _count -= removedCount;

            if (hashset.Count == 0)
                _dictionary.Remove(key);

            return removedCount;
        }

        /// <inheritdoc/>
        public bool RemoveKey(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var hashset))
            {
                _count -= hashset.Count;
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
            return _dictionary.TryGetValue(key, out var hashset) && hashset.Contains(value);
        }

        /// <inheritdoc/>
        public int Count => _count;

        /// <inheritdoc/>
        public IEnumerable<TKey> Keys => _dictionary.Keys;

        /// <inheritdoc/>
        public int KeyCount => _dictionary.Count;

        /// <inheritdoc/>
        public IEnumerable<TValue> Values => _dictionary.Values.SelectMany(hashset => hashset);

        /// <inheritdoc/>
        public int GetValuesCount(TKey key) => _dictionary.TryGetValue(key, out var hashset) ? hashset.Count : 0;

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
            if (obj is not MultiMapSet<TKey, TValue> other)
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
