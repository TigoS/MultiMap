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
    public interface IMultiMapAsync<TKey, TValue> : IAsyncEnumerable<KeyValuePair<TKey, TValue>>, IDisposable
        where TKey : notnull
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
        Task<bool> AddAsync(TKey key, TValue value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously adds multiple values to the set associated with the specified key.
        /// Duplicate values are ignored.
        /// </summary>
        /// <param name="key">The key to associate the values with.</param>
        /// <param name="values">The values to add.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task AddRangeAsync(TKey key, IEnumerable<TValue> values, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously retrieves all values associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose values to retrieve.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="IEnumerable{TValue}"/> containing the values associated with the key,
        /// or an empty sequence if the key does not exist.
        /// </returns>
        Task<IEnumerable<TValue>> GetAsync(TKey key, CancellationToken cancellationToken = default);

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
        Task<bool> RemoveAsync(TKey key, TValue value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously removes the specified key and all its associated values.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>
        /// <see langword="true"/> if the key was found and removed;
        /// <see langword="false"/> if the key did not exist.
        /// </returns>
        Task<bool> RemoveKeyAsync(TKey key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously determines whether the multimap contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>
        /// <see langword="true"/> if the multimap contains the key; otherwise, <see langword="false"/>.
        /// </returns>
        Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously determines whether the multimap contains the specified value for the given key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <param name="value">The value to locate within the key's set.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>
        /// <see langword="true"/> if the key exists and contains the specified value; otherwise, <see langword="false"/>.
        /// </returns>
        Task<bool> ContainsAsync(TKey key, TValue value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously gets the total number of values across all keys in the multimap.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>The total count of all values stored in the multimap.</returns>
        Task<int> GetCountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously removes all keys and values from the multimap.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task ClearAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously gets an enumerable collection of keys contained in the multimap.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>An enumerable collection of keys.</returns>
        Task<IEnumerable<TKey>> GetKeysAsync(CancellationToken cancellationToken = default);
    }
}
