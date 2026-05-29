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
    /// <typeparam name="TKey">The type of keys in the collection. Must not be null and must implement <see cref="IEquatable{TKey}"/>.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must not be null and must implement <see cref="IEquatable{TValue}"/>.</typeparam>
    public interface ISimpleMultiMap<TKey, TValue> : IReadOnlySimpleMultiMap<TKey, TValue>, IEquatable<IReadOnlySimpleMultiMap<TKey, TValue>>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull, IEquatable<TValue>
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
        /// <returns>
        /// <see langword="true"/> if the entry was successfully removed;
        /// otherwise, <see langword="false"/> if the entry does not exist.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> or <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        public bool Remove(TKey key, TValue value);

        /// <summary>
        /// Removes all values associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose associated values are to be removed. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        public void RemoveKey(TKey key);

        /// <summary>
        /// Removes all values associated with the specified key.
        /// </summary>
        /// <remarks>
        /// This method has been renamed to <see cref="RemoveKey"/> for API consistency with other multimap interfaces.
        /// Use <see cref="RemoveKey"/> instead.
        /// </remarks>
        /// <param name="key">The key whose associated values are to be removed. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        [Obsolete("Clear(key) has been renamed to RemoveKey(key) for API consistency. Use ISimpleMultiMap.RemoveKey(key) instead. This method will be removed in a future version.")]
        public void Clear(TKey key);

        /// <summary>
        /// Returns a flattened sequence of key-value pairs from the collection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>This method is deprecated.</b> <see cref="ISimpleMultiMap{TKey,TValue}"/> already implements
        /// <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/>, so iterating the map
        /// directly (e.g. <c>foreach</c>, <c>map.ToList()</c>, LINQ) produces the exact same sequence.
        /// Use direct enumeration instead.
        /// </para>
        /// The order of the pairs in the sequence may depend on the underlying collection implementation.
        /// </remarks>
        /// <returns>
        /// An enumerable collection of <see cref="KeyValuePair{TKey, TValue}"/> representing all key-value pairs contained in the collection.
        /// </returns>
        [Obsolete("Flatten() is redundant. ISimpleMultiMap<TKey,TValue> implements IEnumerable<KeyValuePair<TKey,TValue>> directly — enumerate the map instead (e.g. foreach, ToList(), or LINQ). This method will be removed in a future version.")]
        public IEnumerable<KeyValuePair<TKey, TValue>> Flatten();
    }
}
