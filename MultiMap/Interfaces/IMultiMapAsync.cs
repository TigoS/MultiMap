namespace MultiMap.Interfaces
{
    /// <summary>
    /// Defines an asynchronous collection that associates multiple values with each key, allowing async retrieval, addition, and removal of key-value pairs.
    /// </summary>
    /// <remarks>
    /// An async multimap enables storing multiple values for a single key with all operations returning tasks for async/await usage.
    /// Implementations are expected to be thread-safe and suitable for concurrent scenarios.
    /// The interface supports asynchronous enumeration of all key-value pairs via <see cref="IAsyncEnumerable{T}"/>.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-null.</typeparam>
    public interface IMultiMapAsync<TKey, TValue> : IReadOnlyMultiMapAsync<TKey, TValue>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull
    {
        /// <summary>
        /// Asynchronously adds a value to the set associated with the specified key.
        /// </summary>
        /// <param name="key">The key to associate the value with.</param>
        /// <param name="value">The value to add.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>
        /// <see langword="true"/> if the value was added successfully;
        /// <see langword="false"/> if it already existed for the key.
        /// </returns>
        public ValueTask<bool> AddAsync(TKey key, TValue value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously adds multiple values to the set associated with the specified key.
        /// Duplicate values are ignored.
        /// </summary>
        /// <param name="key">The key to associate the values with.</param>
        /// <param name="values">The values to add.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task AddRangeAsync(TKey key, IEnumerable<TValue> values, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously adds a collection of key/value pairs to the data store.
        /// </summary>
        /// <param name="items">The collection of key/value pairs to add. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous add operation.</returns>
        public Task AddRangeAsync(IEnumerable<KeyValuePair<TKey, TValue>> items, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously removes a specific value from the set associated with the specified key.
        /// If the set becomes empty after removal, the key is also removed.
        /// </summary>
        /// <param name="key">The key from which to remove the value.</param>
        /// <param name="value">The value to remove.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>
        /// <see langword="true"/> if the value was successfully removed;
        /// <see langword="false"/> if the key or value was not found.
        /// </returns>
        public ValueTask<bool> RemoveAsync(TKey key, TValue value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously removes a range of key/value pairs from the collection.
        /// </summary>
        /// <param name="items">The collection of key/value pairs to remove from the collection.Each pair specifies a key and its associated value to be removed.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A ValueTask representing the asynchronous operation. The result contains the number of key/value pairs that were successfully removed.</returns>
        public ValueTask<int> RemoveRangeAsync(IEnumerable<KeyValuePair<TKey, TValue>> items, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes all values associated with the specified key that match the given predicate.
        /// </summary>
        /// <param name="key">The key whose associated values are to be evaluated and potentially removed.</param>
        /// <param name="predicate">A delegate that defines the conditions of the values to remove. Only values for which the predicate returns <see langword="true"/> are removed.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A value task representing the asynchronous operation. The result contains the number of values removed.</returns>
        public ValueTask<int> RemoveWhereAsync(TKey key, Predicate<TValue> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously removes the specified key and all its associated values.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>
        /// <see langword="true"/> if the key was found and removed;
        /// <see langword="false"/> if the key did not exist.
        /// </returns>
        public ValueTask<bool> RemoveKeyAsync(TKey key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously removes all keys and values from the multimap.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task ClearAsync(CancellationToken cancellationToken = default);
    }
}
