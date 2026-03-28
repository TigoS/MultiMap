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
    public interface IMultiMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : notnull
        where TValue : notnull
    {
        /// <summary>
        /// Attempts to add the specified key and value to the collection.
        /// </summary>
        /// <remarks>
        /// If the collection already contains the specified key, the method does not modify the collection and returns false.
        /// The behavior regarding null keys or values depends on the specific implementation.
        /// </remarks>
        /// <param name="key">The key to add to the collection.
        /// Cannot be null if the collection does not support null keys.</param>
        /// <param name="value">The value associated with the key to add.
        /// May be subject to constraints depending on the collection implementation.</param>
        /// <returns>
        /// <see langword="true"/> if the key and value were added successfully;
        /// otherwise, <see langword="false"/> if the key already exists in the collection.
        /// </returns>
        public bool Add(TKey key, TValue value);

        /// <summary>
        /// Adds a collection of values to the specified key.
        /// </summary>
        /// <param name="key">The key to which the values will be added.</param>
        /// <param name="values">The collection of values to add to the key. Cannot be null.</param>
        public void AddRange(TKey key, IEnumerable<TValue> values);

        /// <summary>
        /// Retrieves all values associated with the specified key.
        /// </summary>
        /// <param name="key">The key for which to retrieve values. Cannot be null.
        /// If the key does not exist, an empty collection is returned.</param>
        /// <returns>
        /// An enumerable collection of values associated with the specified key.
        /// The collection is empty if the key is not found.
        /// </returns>
        public IEnumerable<TValue> Get(TKey key);

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
        /// Removes all items from the collection.
        /// </summary>
        public void Clear();

        /// <summary>
        /// Gets the total number of key-value pairs contained in the collection.
        /// </summary>
        public int Count { get; }
    }
}
