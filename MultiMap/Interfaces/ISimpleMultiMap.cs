namespace MultiMap.Interfaces
{
    /// <summary>
    /// Defines a collection that associates multiple values with each key, supporting retrieval and removal of key-value pairs.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface allow storing multiple values for a single key and provide methods to add, retrieve, and remove key-value pairs.
    /// The interface does not specify whether keys or values can be null; this depends on the specific implementation.
    /// The collection supports enumeration of all key-value pairs.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the collection. Must not be null.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must not be null.</typeparam>
    public interface ISimpleMultiMap<TKey, TValue> : IReadOnlySimpleMultiMap<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        /// <summary>
        /// Attempts to add the specified key and value to the collection.
        /// </summary>
        /// <remarks>
        /// If the key already exists in the collection, the method does not modify the existing entry and returns false.
        /// The behavior regarding null keys or values depends on the implementation of the collection.
        /// </remarks>
        /// <param name="key">The key to add to the collection.
        /// Cannot be null if the collection does not support null keys.</param>
        /// <param name="value">The value associated with the key to add.
        /// Cannot be null if the collection does not support null values.</param>
        /// <returns>
        /// <see langword="true"/> if the key and value were added successfully;
        /// otherwise, <see langword="false"/> if the key already exists in the collection.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> or <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        public bool Add(TKey key, TValue value);

        /// <summary>
        /// Removes the entry with the specified key and value from the collection, if it exists.
        /// </summary>
        /// <param name="key">The key of the entry to remove. Cannot be null.</param>
        /// <param name="value">The value associated with the key to remove.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> or <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        public void Remove(TKey key, TValue value);

        /// <summary>
        /// Removes all values associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose associated values are to be cleared. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        public void Clear(TKey key);

        /// <summary>
        /// Returns a flattened sequence of key-value pairs from the collection.
        /// </summary>
        /// <remarks>
        /// The returned sequence includes all key-value pairs, regardless of their original nesting or structure within the collection.
        /// The order of the pairs in the sequence may depend on the underlying collection implementation.
        /// </remarks>
        /// <returns>
        /// An enumerable collection of <see cref="KeyValuePair{TKey, TValue}"/> representing all key-value pairs contained in the collection.
        /// </returns>
        public IEnumerable<KeyValuePair<TKey, TValue>> Flatten();
    }
}
