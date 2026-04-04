namespace MultiMap.Interfaces
{
    /// <summary>
    /// Represents a read-only, asynchronous multimap that allows enumeration and retrieval of multiple values associated with a single key.
    /// </summary>
    /// <remarks>This interface provides asynchronous methods for querying the contents of a multimap, where each key can be associated with multiple values. Implementations are expected to support concurrent and asynchronous access patterns. The interface does not allow modification of the multimap's contents.
    /// Enumeration yields key-value pairs, and all retrieval operations are non-blocking and support cancellation via a token.</remarks>
    /// <typeparam name="TKey">The type of keys in the multimap. Must not be null.</typeparam>
    /// <typeparam name="TValue">The type of values in the multimap. Must not be null.</typeparam>
    public interface IReadOnlyMultiMapAsync<TKey, TValue> : IAsyncEnumerable<KeyValuePair<TKey, TValue>>, IDisposable, IAsyncDisposable
        where TKey : notnull
        where TValue : notnull
    {
        /// <summary>
        /// Asynchronously retrieves all values associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose values to retrieve.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="IEnumerable{TValue}"/> containing the values associated with the key,
        /// or an empty sequence if the key does not exist.
        /// </returns>
        public ValueTask<IEnumerable<TValue>> GetAsync(TKey key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to retrieve the values associated with the specified key asynchronously.
        /// </summary>
        /// <param name="key">The key whose associated values are to be retrieved.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A value task that represents the asynchronous operation. The result contains a tuple where <see langword="found"/> is <see langword="true"/> if values are found for the specified key; otherwise, <see langword="false"/>. <see langword="values"/> contains the associated values if found, or an empty collection if not.</returns>
        public ValueTask<(bool found, IEnumerable<TValue> values)> TryGetAsync(TKey key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously determines whether the multimap contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>
        /// <see langword="true"/> if the multimap contains the key; otherwise, <see langword="false"/>.
        /// </returns>
        public ValueTask<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously determines whether the multimap contains the specified value for the given key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <param name="value">The value to locate within the key's set.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>
        /// <see langword="true"/> if the key exists and contains the specified value; otherwise, <see langword="false"/>.
        /// </returns>
        public ValueTask<bool> ContainsAsync(TKey key, TValue value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously gets the total number of values across all keys in the multimap.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>The total count of all values stored in the multimap.</returns>
        public ValueTask<int> GetCountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously gets an enumerable collection of keys contained in the multimap.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>An enumerable collection of keys.</returns>
        public ValueTask<IEnumerable<TKey>> GetKeysAsync(CancellationToken cancellationToken = default);
    }
}
