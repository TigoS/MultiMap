namespace MultiMap.Interfaces
{
    /// <summary>
    /// Defines a collection that associates multiple values with each key, allowing retrieval, addition, and removal of key-value pairs.
    /// </summary>
    /// <remarks>
    /// A multimap enables storing multiple values for a single key, unlike a standard dictionary.
    /// Implementations may vary in how duplicate values are handled and in the ordering of values.
    /// The interface supports enumeration of all key-value pairs and provides methods for bulk operations and querying.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
    public interface IMultiMap<TKey, TValue> : IReadOnlyMultiMap<TKey, TValue>, ISimpleMultiMap<TKey, TValue>, IEquatable<IReadOnlyMultiMap<TKey, TValue>>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull, IEquatable<TValue>
    {
        /// <summary>
        /// Adds a collection of values to the specified key.
        /// </summary>
        /// <param name="key">The key to which the values will be added.</param>
        /// <param name="values">The collection of values to add to the key. Cannot be null.</param>
        /// <returns>The number of values that were actually added (excluding duplicates or already-existing values).</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> or <paramref name="values"/> is <see langword="null"/>.
        /// </exception>
        public int AddRange(TKey key, IEnumerable<TValue> values);

        /// <summary>
        /// Adds the elements of the specified collection to the current collection.
        /// </summary>
        /// <remarks>The order in which the elements are added is preserved. Duplicate key-value pairs that already exist are ignored.</remarks>
        /// <param name="items">The collection of key/value pairs to add.</param>
        /// <returns>The number of key/value pairs that were actually added (excluding duplicates or already-existing pairs).</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="items"/> is <see langword="null"/>.
        /// </exception>
        public int AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items);

        /// <summary>
        /// Removes the specified key/value pairs from the collection.
        /// </summary>
        /// <remarks>If a specified key/value pair does not exist in the collection, it is ignored.
        /// The method does not throw an exception if some or all pairs are not found.</remarks>
        /// <param name="items">The key/value pairs to remove from the collection. Each pair is matched by key and value; only pairs that exist in the collection are removed. Cannot be null.</param>
        /// <returns>The number of key/value pairs that were successfully removed from the collection.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="items"/> is <see langword="null"/>.
        /// </exception>
        public int RemoveRange(IEnumerable<KeyValuePair<TKey, TValue>> items);

        /// <summary>
        /// Removes all values associated with the specified key that match the given predicate.
        /// </summary>
        /// <remarks>If the key does not exist in the collection, no action is taken and the method returns 0. The predicate is applied only to values associated with the specified key.</remarks>
        /// <param name="key">The key whose associated values are to be tested and potentially removed.</param>
        /// <param name="predicate">A delegate that defines the conditions of the values to remove. Only values for which the predicate returns <see langword="true"/> are removed.</param>
        /// <returns>The number of values removed from the collection.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> or <paramref name="predicate"/> is <see langword="null"/>.
        /// </exception>
        public int RemoveWhere(TKey key, Predicate<TValue> predicate);
    }
}
