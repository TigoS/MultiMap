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
    /// <typeparam name="TKey">The type of keys in the multi-map. Must be non-null and implement both <see cref="IEquatable{TKey}"/> (required by the base class and all multi-map interfaces) and <see cref="IComparable{TKey}"/> (required by this class for sorted key ordering). Note: only the comparer is used at runtime; <see cref="IEquatable{TKey}"/> is a library-wide constraint on all multi-map types.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-null and implement both <see cref="IEquatable{TValue}"/> and <see cref="IComparable{TValue}"/>.</typeparam>
    public sealed class SortedMultiMap<TKey, TValue> : MultiMapBase<TKey, TValue, SortedSet<TValue>>
        where TKey : notnull, IEquatable<TKey>, IComparable<TKey>
        where TValue : notnull, IEquatable<TValue>, IComparable<TValue>
    {
        private readonly IComparer<TValue>? _valueComparer;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedMultiMap{TKey, TValue}"/> class with the specified comparer for values.
        /// </summary>
        /// <param name="valueComparer">The comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public SortedMultiMap(IComparer<TValue>? valueComparer)
            : base(new SortedDictionary<TKey, SortedSet<TValue>>())
        {
            _valueComparer = valueComparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedMultiMap{TKey, TValue}"/> class with the specified comparers for keys and values.
        /// </summary>
        /// <param name="keyComparer">The comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        /// <param name="valueComparer">The comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public SortedMultiMap(IComparer<TKey>? keyComparer, IComparer<TValue>? valueComparer)
            : base(new SortedDictionary<TKey, SortedSet<TValue>>(keyComparer))
        {
            _valueComparer = valueComparer;
        }

        /// <inheritdoc/>
        protected override SortedSet<TValue> CreateCollection() => _valueComparer is null ? new SortedSet<TValue>() : new SortedSet<TValue>(_valueComparer);

        /// <inheritdoc/>
        protected override bool AddToCollection(SortedSet<TValue> collection, TValue value) => collection.Add(value);

        /// <inheritdoc/>
        protected override int RemoveWhereFromCollection(SortedSet<TValue> collection, Predicate<TValue> predicate) => collection.RemoveWhere(predicate);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as SortedMultiMap<TKey, TValue>);

        /// <inheritdoc/>
        public override bool Equals(IReadOnlyMultiMap<TKey, TValue>? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (KeyCount != other.KeyCount || Count != other.Count)
                return false;

            foreach (var key in Keys)
            {
                if (!other.TryGet(key, out var otherValues))
                    return false;

                var otherValuesSet = new SortedSet<TValue>(otherValues, _valueComparer);

                if (!_dictionary.TryGetValue(key, out var targetValuesSet))
                    return false;

                if (targetValuesSet.Count != otherValuesSet.Count)
                    return false;

                if (!targetValuesSet.SetEquals(otherValuesSet))
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
            // SortedMultiMap uses IComparer<TKey> for ordering, not IEqualityComparer<TKey>;
            // key hashing uses EqualityComparer<TKey>.Default (the ComputeUnorderedHash default).
            => MultiMapHelper.ComputeUnorderedHash<TKey, TValue, SortedSet<TValue>>(_dictionary, valueComparer: _valueComparer is IEqualityComparer<TValue> ec ? ec : null);
    }
}
