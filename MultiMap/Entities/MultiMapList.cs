#if NET6_0_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace MultiMap.Entities
{
    /// <summary>
    /// Represents a collection that associates multiple values with each key, allowing efficient storage and retrieval of key-value pairs where keys may map to zero or more values.
    /// </summary>
    /// <remarks>
    /// This class is useful for scenarios where a key can have multiple associated values, such as grouping or indexing.
    /// The collection maintains insertion order for values under each key.
    /// Keys and values must be non-null. Thread safety is not guaranteed;
    /// external synchronization is required for concurrent access.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multi-map. Must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-nullable.</typeparam>
    public class MultiMapList<TKey, TValue> : MultiMapBase<TKey, TValue, List<TValue>>
        where TKey : notnull
        where TValue : notnull
    {
        /// <summary>
        /// Initializes a new instance of the MultiMapList class with an empty mapping.
        /// </summary>
        /// <remarks>Use this constructor to create a MultiMapList that starts with no key-value associations.
        /// The internal dictionary is initialized and ready for adding keys and values.</remarks>
        public MultiMapList()
            : base(new Dictionary<TKey, List<TValue>>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the MultiMapList class with the specified initial capacity for keys.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        public MultiMapList(int capacity)
            : base(new Dictionary<TKey, List<TValue>>(capacity))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapList{TKey, TValue}"/> class with the specified equality comparer for keys.
        /// </summary>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapList(IEqualityComparer<TKey>? keyComparer)
            : base(new Dictionary<TKey, List<TValue>>(keyComparer))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapList{TKey, TValue}"/> class with the specified initial capacity for keys.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapList(int capacity, IEqualityComparer<TKey>? keyComparer)
            : base(new Dictionary<TKey, List<TValue>>(capacity, keyComparer))
        {
        }

        /// <inheritdoc/>
        protected override List<TValue> CreateCollection() => new List<TValue>();

        /// <inheritdoc/>
        protected override bool AddToCollection(List<TValue> collection, TValue value)
        {
            if (collection is null) throw new ArgumentNullException(nameof(collection));
            if (value is null) throw new ArgumentNullException(nameof(value));

            collection.Add(value);
            return true;
        }

        /// <inheritdoc/>
        protected override int RemoveWhereFromCollection(List<TValue> collection, Predicate<TValue> predicate)
        {
            if (collection is null) throw new ArgumentNullException(nameof(collection));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            return collection.RemoveAll(predicate);
        }

        /// <inheritdoc/>
        protected override IEnumerable<TValue> ToReadOnly(List<TValue> collection) => collection.AsReadOnly();

#if NET6_0_OR_GREATER
        /// <inheritdoc/>
        public override bool Add(TKey key, TValue value)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));

            ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault((Dictionary<TKey, List<TValue>>)_dictionary, key, out bool exists);
            list ??= new List<TValue>();

            list.Add(value);
            _count++;

            return true;
        }
#endif

        /// <inheritdoc/>
        public override int AddRange(TKey key, IEnumerable<TValue> values)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (values is null) throw new ArgumentNullException(nameof(values));

#if NET6_0_OR_GREATER
            ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault((Dictionary<TKey, List<TValue>>)_dictionary, key, out bool exists);
            list ??= new List<TValue>();
#else
            if (!_dictionary.TryGetValue(key, out var list))
            {
                list = new List<TValue>();
                _dictionary[key] = list;
            }
#endif

            int before = list.Count;
            list.AddRange(values);
            int added = list.Count - before;
            _count += added;

            return added;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is not MultiMapList<TKey, TValue> other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (Count != other.Count || KeyCount != other.KeyCount)
                return false;

            foreach (var kvp in _dictionary)
            {
                if (!other._dictionary.TryGetValue(kvp.Key, out var otherList))
                    return false;

                if (!kvp.Value.SequenceEqual(otherList))
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
                var entryHash = new HashCode();
                entryHash.Add(kvp.Key);
                foreach (var value in kvp.Value)
                {
                    entryHash.Add(value);
                }
                hash ^= entryHash.ToHashCode();
            }
            return hash;
        }
    }
}
