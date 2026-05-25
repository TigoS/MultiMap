using MultiMap.Helpers;
using MultiMap.Interfaces;

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
    /// <typeparam name="TKey">The type of keys in the multi-map. Must be non-null and implement <see cref="IComparable{TKey}"/>.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-null and implement <see cref="IComparable{TValue}"/>.</typeparam>
    public sealed class SortedMultiMap<TKey, TValue> : MultiMapBase<TKey, TValue, SortedSet<TValue>>
        where TKey : notnull, IEquatable<TKey>, IComparable<TKey>
        where TValue : notnull, IEquatable<TValue>, IComparable<TValue>
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
        public override bool Equals(IReadOnlyMultiMap<TKey, TValue>? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (KeyCount != other.KeyCount)
            {
                return false;
            }

            foreach (var key in Keys)
            {
                if (!other.ContainsKey(key))
                {
                    return false;
                }

                var thisValues = this[key];
                var otherValues = other[key];

                if (thisValues.Count() != otherValues.Count())
                {
                    return false;
                }

                if (!thisValues.SequenceEqual(otherValues))
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
            => MultiMapHelper.ComputeUnorderedHash<TKey, TValue, SortedSet<TValue>>(_dictionary);
    }
}
