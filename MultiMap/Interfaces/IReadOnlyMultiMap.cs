namespace MultiMap.Interfaces
{
    /// <summary>
    /// Represents a read-only collection of keys, each mapped to one or more values. Provides methods to retrieve values associated with a key and to query the presence of keys or key-value pairs.
    /// </summary>
    /// <remarks>This interface allows enumeration of all key-value pairs and provides efficient lookup of values by key.
    /// Implementations are read-only and do not support modification of the collection.
    /// The <see cref="IReadOnlySimpleMultiMap{TKey, TValue}.Get"/> method throws <see cref="KeyNotFoundException"/> if the key is not present;
    /// use <see cref="IReadOnlySimpleMultiMap{TKey, TValue}.GetOrDefault"/> for safe retrieval that returns an empty collection.
    /// Thread safety and ordering of keys or values depend on the specific implementation.</remarks>
    /// <typeparam name="TKey">The type of keys in the multi-map. Must not be null and must implement <see cref="IEquatable{TKey}"/>.</typeparam>
    /// <typeparam name="TValue">The type of values in the multi-map. Must not be null and must implement <see cref="IEquatable{TValue}"/>.</typeparam>
    public interface IReadOnlyMultiMap<TKey, TValue> : IReadOnlySimpleMultiMap<TKey, TValue>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull, IEquatable<TValue>
    {
        /// <summary>
        /// Attempts to retrieve the collection of values associated with the specified key.
        /// </summary>
        /// <remarks>The method does not throw an exception if the key is not found.
        /// The returned collection in the out parameter is empty if the key does not exist.</remarks>
        /// <param name="key">The key whose associated values are to be retrieved.</param>
        /// <param name="values">When this method returns, contains the collection of values associated with the specified key, if the key is found; otherwise, an empty collection.</param>
        /// <returns>true if the key was found and values were retrieved; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        public bool TryGet(TKey key, out IEnumerable<TValue> values);

        /// <summary>
        /// Gets the number of keys contained in the collection.
        /// </summary>
        public int KeyCount { get; }

        /// <summary>
        /// Gets an enumerable collection of keys contained in the collection.
        /// </summary>
        public IEnumerable<TKey> Keys { get; }

        /// <summary>
        /// Gets a collection containing the values in the dictionary.
        /// </summary>
        public IEnumerable<TValue> Values { get; }

        /// <summary>
        /// Gets the number of values associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose associated value count is to be retrieved. Cannot be null.</param>
        /// <returns>The number of values associated with the specified key. Returns 0 if the key does not exist.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        public int GetValuesCount(TKey key);

        /// <summary>
        /// Gets the collection of values associated with the specified key.
        /// </summary>
        /// <remarks>This indexer delegates to <see cref="IReadOnlySimpleMultiMap{TKey, TValue}.Get"/>
        /// and throws <see cref="KeyNotFoundException"/> if the key is not found.</remarks>
        /// <param name="key">The key whose associated values to retrieve.</param>
        /// <returns>An enumerable collection of values associated with the specified key.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="KeyNotFoundException">Thrown when the specified key does not exist in the collection.</exception>
        public IEnumerable<TValue> this[TKey key] { get; }
    }
}
