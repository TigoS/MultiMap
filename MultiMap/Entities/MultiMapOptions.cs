namespace MultiMap.Entities
{
    /// <summary>
    /// Specifies configuration options for constructing a <see cref="MultiMapSet{TKey,TValue}"/>,
    /// <see cref="MultiMapList{TKey,TValue}"/>, <see cref="MultiMapLock{TKey,TValue}"/>, or
    /// <see cref="MultiMapAsync{TKey,TValue}"/> instance.
    /// </summary>
    /// <remarks>
    /// All properties are optional and default to <see langword="null"/> / zero, which selects the
    /// same defaults as the parameterless constructor for each type.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
    /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
    public sealed record MultiMapOptions<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        /// <summary>
        /// Gets or initializes the initial number of key slots the internal dictionary pre-allocates.
        /// Zero (the default) means no pre-allocation.
        /// </summary>
        public int Capacity { get; init; }

        /// <summary>
        /// Gets or initializes the equality comparer used to compare keys, or <see langword="null"/>
        /// to use <see cref="EqualityComparer{T}.Default"/>.
        /// </summary>
        public IEqualityComparer<TKey>? KeyComparer { get; init; }

        /// <summary>
        /// Gets or initializes the equality comparer used to compare values, or <see langword="null"/>
        /// to use <see cref="EqualityComparer{T}.Default"/>.
        /// </summary>
        public IEqualityComparer<TValue>? ValueComparer { get; init; }
    }

    /// <summary>
    /// Specifies configuration options for constructing a <see cref="ConcurrentMultiMap{TKey,TValue}"/> instance.
    /// </summary>
    /// <remarks>
    /// Extends <see cref="MultiMapOptions{TKey,TValue}"/> with the <see cref="ConcurrencyLevel"/> property,
    /// which maps to the <c>concurrencyLevel</c> parameter of <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey,TValue}"/>.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
    /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
    public sealed record ConcurrentMultiMapOptions<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        /// <summary>
        /// Gets or initializes the estimated number of threads that will update the multimap concurrently.
        /// Zero (the default) lets <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey,TValue}"/> choose automatically.
        /// </summary>
        public int ConcurrencyLevel { get; init; }

        /// <summary>
        /// Gets or initializes the initial number of key slots. Zero means no pre-allocation.
        /// </summary>
        public int Capacity { get; init; }

        /// <summary>
        /// Gets or initializes the equality comparer used to compare keys, or <see langword="null"/>
        /// to use <see cref="EqualityComparer{T}.Default"/>.
        /// </summary>
        public IEqualityComparer<TKey>? KeyComparer { get; init; }

        /// <summary>
        /// Gets or initializes the equality comparer used to compare values, or <see langword="null"/>
        /// to use <see cref="EqualityComparer{T}.Default"/>.
        /// </summary>
        public IEqualityComparer<TValue>? ValueComparer { get; init; }
    }

    /// <summary>
    /// Specifies configuration options for constructing a <see cref="SortedMultiMap{TKey,TValue}"/> instance.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="IComparer{T}"/> rather than <see cref="IEqualityComparer{T}"/> because
    /// <see cref="SortedMultiMap{TKey,TValue}"/> is backed by <see cref="System.Collections.Generic.SortedDictionary{TKey,TValue}"/>
    /// and <see cref="System.Collections.Generic.SortedSet{T}"/>, which require ordering comparers.
    /// See <see cref="SortedMultiMap{TKey,TValue}"/> class remarks for the requirement that
    /// <see cref="ValueComparer"/> also implement <see cref="IEqualityComparer{T}"/> when non-null.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys. Must implement <see cref="IComparable{TKey}"/>.</typeparam>
    /// <typeparam name="TValue">The type of values. Must implement <see cref="IComparable{TValue}"/>.</typeparam>
    public sealed record SortedMultiMapOptions<TKey, TValue>
        where TKey : notnull, IComparable<TKey>
        where TValue : notnull, IComparable<TValue>
    {
        /// <summary>
        /// Gets or initializes the comparer used to order keys, or <see langword="null"/> to use
        /// <see cref="Comparer{T}.Default"/>.
        /// </summary>
        public IComparer<TKey>? KeyComparer { get; init; }

        /// <summary>
        /// Gets or initializes the comparer used to order values, or <see langword="null"/> to use
        /// <see cref="Comparer{T}.Default"/>. When non-null, must also implement
        /// <see cref="IEqualityComparer{TValue}"/>; an <see cref="ArgumentException"/> is thrown
        /// at construction time otherwise.
        /// </summary>
        public IComparer<TValue>? ValueComparer { get; init; }
    }
}
