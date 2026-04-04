namespace MultiMap.Interfaces
{
    /// <summary>
    /// Represents a read-only collection of keys, each mapped to one or more values. Provides methods to retrieve values associated with a key and to query the presence of keys or key-value pairs.
    /// </summary>
    /// <remarks>This interface allows enumeration of all key-value pairs and provides efficient lookup of values by key.
    /// Implementations are read-only and do not support modification of the collection.
    /// The interface guarantees that methods never return null collections; if a key is not present, an empty collection is returned instead. Thread safety and ordering of keys or values depend on the specific implementation.</remarks>
    /// <typeparam name="TKey">The type of keys in the multi-map. Must not be null.</typeparam>
    /// <typeparam name="TValue">The type of values in the multi-map. Must not be null.</typeparam>
    public interface IReadOnlyMultiMap<TKey, TValue> : IReadOnlySimpleMultiMap<TKey, TValue>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull
    {
        /// <summary>
        /// Attempts to retrieve the collection of values associated with the specified key.
        /// </summary>
        /// <remarks>The method does not throw an exception if the key is not found.
        /// The returned collection in the out parameter is empty if the key does not exist.</remarks>
        /// <param name="key">The key whose associated values are to be retrieved.</param>
        /// <param name="values">When this method returns, contains the collection of values associated with the specified key, if the key is found; otherwise, an empty collection.</param>
        /// <returns>true if the key was found and values were retrieved; otherwise, false.</returns>
        public bool TryGet(TKey key, out IEnumerable<TValue> values);

        /// <summary>
        /// Determines whether the collection contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the collection.
        /// Cannot be null if the collection does not accept null keys.</param>
        /// <returns>
        /// <see langword="true"/> if the collection contains an element with the specified key;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool ContainsKey(TKey key);

        /// <summary>
        /// Determines whether the collection contains an element with the specified key and value.
        /// </summary>
        /// <param name="key">The key to locate in the collection.</param>
        /// <param name="value">The value to locate in the collection associated with the specified key.</param>
        /// <returns>
        /// <see langword="true"/> if an element with the specified key and value exists in the collection;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(TKey key, TValue value);

        /// <summary>
        /// Gets the total number of key-value pairs contained in the collection.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Gets an enumerable collection of keys contained in the collection.
        /// </summary>
        public IEnumerable<TKey> Keys { get; }
    }
}
