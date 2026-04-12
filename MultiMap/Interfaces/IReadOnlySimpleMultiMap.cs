namespace MultiMap.Interfaces
{
    /// <summary>
    /// Represents a read-only collection that maps keys to one or more values, allowing retrieval of all values associated with a given key.
    /// </summary>
    /// <remarks>This interface provides read-only access to a multi-value mapping, where each key can be associated with multiple values. It does not allow modification of the collection. Implementations must ensure that both keys and values are non-null. Enumeration yields key-value pairs for all associations in the collection.</remarks>
    /// <typeparam name="TKey">The type of keys in the multi-map. Must not be null.</typeparam>
    /// <typeparam name="TValue">The type of values in the multi-map. Must not be null.</typeparam>
    public interface IReadOnlySimpleMultiMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : notnull
        where TValue : notnull
    {
        /// <summary>
        /// Retrieves all values associated with the specified key.
        /// </summary>
        /// <param name="key">The key for which to retrieve the associated values. Cannot be null.</param>
        /// <returns>
        /// An enumerable collection of values associated with the specified key.
        /// Throws a <see cref="KeyNotFoundException"/> if the key is not found.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified key does not exist in the collection.
        /// </exception>
        public IEnumerable<TValue> Get(TKey key);

        /// <summary>
        /// Retrieves the collection of values associated with the specified key, or an empty collection if the key does not exist.
        /// </summary>
        /// <param name="key">The key whose associated values are to be returned. Cannot be null.</param>
        /// <returns>
        /// An enumerable collection of values associated with the specified key.
        /// If the key is not found, returns an empty collection.
        /// </returns>
        public IEnumerable<TValue> GetOrDefault(TKey key);
    }
}
