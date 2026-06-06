using MultiMap.Helpers;
using MultiMap.Interfaces;

namespace MultiMap.Entities
{
    /// <summary>
    /// Represents a collection that associates each key with a sorted set of values, allowing multiple values per key and maintaining both keys and values in sorted order.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The keys and values are stored in sorted order according to their natural comparer or a specified comparer.
    /// This type is useful when you need to maintain multiple values per key and require predictable ordering for both keys and values.
    /// Thread safety is not guaranteed; external synchronization is required for concurrent access.
    /// </para>
    /// <para>
    /// <strong>Important: Comparer Limitation</strong>
    /// </para>
    /// <para>
    /// This type accepts <see cref="IComparer{T}"/> (for sorting order) but not <see cref="IEqualityComparer{T}"/> (for equality semantics).
    /// When computing <see cref="GetHashCode"/>, the implementation attempts to cast the value comparer to <see cref="IEqualityComparer{TValue}"/>.
    /// If the cast fails (i.e., a custom <see cref="IComparer{TValue}"/> does not also implement <see cref="IEqualityComparer{TValue}"/>),
    /// the code silently falls back to <see cref="EqualityComparer{TValue}.Default"/>.
    /// </para>
    /// <para>
    /// This can lead to inconsistent behavior:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Two <see cref="SortedMultiMap{TKey, TValue}"/> instances with the same content may have different hash codes
    /// if one uses a custom <see cref="IComparer{TValue}"/> and the other does not, even though they compare equal via <see cref="Equals(IReadOnlyMultiMap{TKey, TValue})"/>.
    /// This violates the contract: if <c>a.Equals(b)</c>, then <c>a.GetHashCode() == b.GetHashCode()</c>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Placing such instances in hash-based collections (e.g., <see cref="HashSet{T}"/>, <see cref="Dictionary{TKey, TValue}"/>) can cause lookup failures or duplicates.
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// <strong>Best Practice:</strong> If you use a custom value comparer, ensure it implements both <see cref="IComparer{TValue}"/> and <see cref="IEqualityComparer{TValue}"/>
    /// with consistent semantics, or avoid relying on <see cref="GetHashCode"/> and <see cref="Equals(IReadOnlyMultiMap{TKey, TValue})"/> for hashing.
    /// </para>
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
        /// <remarks>
        /// The value comparer will be <see langword="null"/>, and values will be compared using their default ordering.
        /// See the class remarks for important information about comparer limitations.
        /// </remarks>
        /// <param name="keyComparer">The comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public SortedMultiMap(IComparer<TKey>? keyComparer)
            : base(new SortedDictionary<TKey, SortedSet<TValue>>(keyComparer))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedMultiMap{TKey, TValue}"/> class with the specified comparer for values.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The key comparer will be <see langword="null"/>, and keys will be compared using their default ordering.
        /// </para>
        /// <para>
        /// <strong>Important:</strong> If <paramref name="valueComparer"/> does not implement <see cref="IEqualityComparer{TValue}"/>,
        /// <see cref="GetHashCode"/> will fall back to <see cref="EqualityComparer{TValue}.Default"/>, potentially creating
        /// hash code inconsistencies. See the class remarks for details.
        /// </para>
        /// </remarks>
        /// <param name="valueComparer">The comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public SortedMultiMap(IComparer<TValue>? valueComparer)
            : base(new SortedDictionary<TKey, SortedSet<TValue>>())
        {
            _valueComparer = valueComparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedMultiMap{TKey, TValue}"/> class with the specified comparers for keys and values.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Important:</strong> If <paramref name="valueComparer"/> does not implement <see cref="IEqualityComparer{TValue}"/>,
        /// <see cref="GetHashCode"/> will fall back to <see cref="EqualityComparer{TValue}.Default"/>, potentially creating
        /// hash code inconsistencies. See the class remarks for details and recommended best practices.
        /// </para>
        /// </remarks>
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

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        /// <remarks>
        /// <para>
        /// SortedMultiMap uses <see cref="IComparer{TKey}"/> for ordering, not <see cref="IEqualityComparer{TKey}"/>;
        /// key hashing uses <see cref="EqualityComparer{TKey}.Default"/> (the <see cref="MultiMapHelper.ComputeUnorderedHash{TKey, TValue, TCollection}"/> default).
        /// </para>
        /// <para>
        /// For values: if <see cref="_valueComparer"/> implements <see cref="IEqualityComparer{TValue}"/>, use it.
        /// Otherwise, fall back to <see cref="EqualityComparer{TValue}.Default"/>.
        /// </para>
        /// <para>
        /// <strong>WARNING:</strong> This fallback can create hash code inconsistency if <see cref="_valueComparer"/> defines equality differently than the default comparer.
        /// </para>
        /// </remarks>
        public override int GetHashCode()
        {
            var valueEqualityComparer = _valueComparer is IEqualityComparer<TValue> ec ? ec : null;

            return MultiMapHelper.ComputeUnorderedHash<TKey, TValue, SortedSet<TValue>>(_dictionary, valueComparer: valueEqualityComparer);
        }
    }
}
