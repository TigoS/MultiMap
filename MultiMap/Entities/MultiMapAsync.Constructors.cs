namespace MultiMap.Entities
{
    public sealed partial class MultiMapAsync<TKey, TValue>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull, IEquatable<TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapAsync{TKey, TValue}"/> class.
        /// </summary>
        public MultiMapAsync() : this(0, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapAsync{TKey, TValue}"/> class with the specified equality comparer for keys.
        /// </summary>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapAsync(IEqualityComparer<TKey>? keyComparer) : this(0, keyComparer, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapAsync{TKey, TValue}"/> class with the specified equality comparer for values.
        /// </summary>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapAsync(IEqualityComparer<TValue>? valueComparer) : this(0, null, valueComparer) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapAsync{TKey, TValue}"/> class with the specified initial capacity for keys.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        public MultiMapAsync(int capacity) : this(capacity, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapAsync{TKey, TValue}"/> class with the specified initial capacity for keys and equality comparer for keys.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapAsync(int capacity, IEqualityComparer<TKey>? keyComparer) : this(capacity, keyComparer, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapAsync{TKey, TValue}"/> class with the specified initial capacity for keys and equality comparer for values.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapAsync(int capacity, IEqualityComparer<TValue>? valueComparer) : this(capacity, null, valueComparer) { }
    }
}
