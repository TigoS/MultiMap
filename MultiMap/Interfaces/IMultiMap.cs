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
    /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-null.</typeparam>
    public interface IMultiMap<TKey, TValue> : IReadOnlyMultiMap<TKey, TValue>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull
    {
        /// <summary>
        /// Attempts to add the specified key and value to the collection.
        /// </summary>
        /// <remarks>
        /// If the collection already contains the specified key-value pair, the method does not modify the collection and returns false.
        /// The behavior regarding null keys or values depends on the specific implementation.
        /// </remarks>
        /// <param name="key">The key to add to the collection.
        /// Cannot be null if the collection does not support null keys.</param>
        /// <param name="value">The value associated with the key to add.
        /// May be subject to constraints depending on the collection implementation.</param>
        /// <returns>
        /// <see langword="true"/> if the key and value were added successfully;
        /// otherwise, <see langword="false"/> if the key-value pair already exists in the collection.
        /// </returns>
        public bool Add(TKey key, TValue value);

        /// <summary>
        /// Adds a collection of values to the specified key.
        /// </summary>
        /// <param name="key">The key to which the values will be added.</param>
        /// <param name="values">The collection of values to add to the key. Cannot be null.</param>
        public void AddRange(TKey key, IEnumerable<TValue> values);

        /// <summary>
        /// Adds the elements of the specified collection to the current collection.
        /// </summary>
        /// <remarks>If any key in the provided collection already exists in the current collection, an exception may be thrown depending on the implementation. The order in which the elements are added is preserved.</remarks>
        /// <param name="items">The collection of key/value pairs to add. Each key in the collection must be unique and not already present in the current collection.</param>
        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items);

        /// <summary>
        /// Removes the entry with the specified key and value from the collection.
        /// </summary>
        /// <remarks>This method removes the entry only if both the key and value match an existing element.
        /// If the collection does not contain the specified key-value pair, no action is taken.</remarks>
        /// <param name="key">The key of the element to remove. Cannot be null.</param>
        /// <param name="value">The value associated with the key to remove.</param>
        /// <returns>
        /// <see langword="true"/> if the element was found and removed;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool Remove(TKey key, TValue value);

        /// <summary>
        /// Removes the specified key/value pairs from the collection.
        /// </summary>
        /// <remarks>If a specified key/value pair does not exist in the collection, it is ignored.
        /// The method does not throw an exception if some or all pairs are not found.</remarks>
        /// <param name="items">The key/value pairs to remove from the collection. Each pair is matched by key and value; only pairs that exist in the collection are removed. Cannot be null.</param>
        /// <returns>The number of key/value pairs that were successfully removed from the collection.</returns>
        public int RemoveRange(IEnumerable<KeyValuePair<TKey, TValue>> items);

        /// <summary>
        /// Removes all values associated with the specified key that match the given predicate.
        /// </summary>
        /// <remarks>If the key does not exist in the collection, no action is taken and the method returns 0. The predicate is applied only to values associated with the specified key.</remarks>
        /// <param name="key">The key whose associated values are to be tested and potentially removed.</param>
        /// <param name="predicate">A delegate that defines the conditions of the values to remove. Only values for which the predicate returns <see langword="true"/> are removed.</param>
        /// <returns>The number of values removed from the collection.</returns>
        public int RemoveWhere(TKey key, Predicate<TValue> predicate);

        /// <summary>
        /// Removes all values associated with the specified key.
        /// </summary>
        /// <remarks>Use this method to remove all entries for a given key.
        /// If the key does not exist, no action is taken and the method returns false.</remarks>
        /// <param name="key">The key whose values are to be removed. Cannot be null.</param>
        /// <returns>
        /// <see langword="true"/> if the key was found and its values were removed;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool RemoveKey(TKey key);

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear();
    }
}
