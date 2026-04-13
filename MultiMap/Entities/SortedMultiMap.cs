namespace MultiMap.Entities
{
    /// <summary>
    /// Represents a collection that associates each key with a sorted set of values, allowing multiple values per key and maintaining both keys and values in sorted order.
    /// </summary>
    /// <remarks>
    /// The keys and values are stored in sorted order according to their natural comparer or a specified comparer.
    /// This type is useful when you need to maintain multiple values per key and require predictable ordering for both keys and values.
    /// Thread safety is not guaranteed; external synchronization is required for concurrent access.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multi-map. Must be non-null and support sorting.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-null and support sorting.</typeparam>
    public class SortedMultiMap<TKey, TValue> : MultiMapBase<TKey, TValue, SortedSet<TValue>>
        where TKey : notnull, IComparable<TKey>
        where TValue : notnull, IComparable<TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SortedMultiMap{TKey, TValue}"/> class.
        /// </summary>
        public SortedMultiMap()
            : base(new SortedDictionary<TKey, SortedSet<TValue>>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedMultiMap{TKey, TValue}"/> class with the specified comparer for keys.
        /// </summary>
        /// <param name="keyComparer">The comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public SortedMultiMap(IComparer<TKey>? keyComparer)
            : base(new SortedDictionary<TKey, SortedSet<TValue>>(keyComparer))
        {
        }

        /// <inheritdoc/>
        protected override SortedSet<TValue> CreateCollection() => new SortedSet<TValue>();

        /// <inheritdoc/>
        protected override bool AddToCollection(SortedSet<TValue> collection, TValue value) => collection.Add(value);

        /// <inheritdoc/>
        protected override int RemoveWhereFromCollection(SortedSet<TValue> collection, Predicate<TValue> predicate) => collection.RemoveWhere(predicate);

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is not SortedMultiMap<TKey, TValue> other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (Count != other.Count || KeyCount != other.KeyCount)
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
            unchecked
            {
                int hash = 0;
                foreach (var kvp in _dictionary)
                {
                    int valueHash = 0;
                    foreach (var value in kvp.Value)
                    {
                        valueHash += Scramble(value.GetHashCode());
                    }
                    hash += Scramble(HashCode.Combine(kvp.Key, valueHash));
                }
                return hash;
            }

            static int Scramble(int h)
            {
                unchecked
                {
                    h ^= h >> 16;
                    h *= -2048144789;
                    h ^= h >> 13;
                    h *= -1028477387;
                    h ^= h >> 16;
                }
                return h;
            }
        }
    }
}
