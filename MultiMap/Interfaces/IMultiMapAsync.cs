using MultiMap.Entities;

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
    /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
    public interface IMultiMapAsync<TKey, TValue> : IReadOnlyMultiMapAsync<TKey, TValue>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull, IEquatable<TValue>
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
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> or <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        public ValueTask<bool> AddAsync(TKey key, TValue value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously adds multiple values to the set associated with the specified key.
        /// Duplicate values are ignored.
        /// </summary>
        /// <param name="key">The key to associate the values with.</param>
        /// <param name="values">The values to add.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The result contains the number of values that were actually added.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> or <paramref name="values"/> is <see langword="null"/>.
        /// </exception>
        public ValueTask<int> AddRangeAsync(TKey key, IEnumerable<TValue> values, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously adds a collection of key/value pairs to the data store.
        /// </summary>
        /// <param name="items">The collection of key/value pairs to add. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The result contains the number of key/value pairs that were actually added.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="items"/> is <see langword="null"/>.
        /// </exception>
        public ValueTask<int> AddRangeAsync(IEnumerable<KeyValuePair<TKey, TValue>> items, CancellationToken cancellationToken = default);

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
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> or <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        public ValueTask<bool> RemoveAsync(TKey key, TValue value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously removes a range of key/value pairs from the collection.
        /// </summary>
        /// <param name="items">The collection of key/value pairs to remove from the collection.Each pair specifies a key and its associated value to be removed.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A ValueTask representing the asynchronous operation. The result contains the number of key/value pairs that were successfully removed.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="items"/> is <see langword="null"/>.
        /// </exception>
        public ValueTask<int> RemoveRangeAsync(IEnumerable<KeyValuePair<TKey, TValue>> items, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes all values associated with the specified key that match the given predicate.
        /// </summary>
        /// <param name="key">The key whose associated values are to be evaluated and potentially removed.</param>
        /// <param name="predicate">A delegate that defines the conditions of the values to remove. Only values for which the predicate returns <see langword="true"/> are removed.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A value task representing the asynchronous operation. The result contains the number of values removed.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> or <paramref name="predicate"/> is <see langword="null"/>.
        /// </exception>
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
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        public ValueTask<bool> RemoveKeyAsync(TKey key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously removes all keys and values from the multimap.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task ClearAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously determines whether the current instance and the specified object are equal by comparing their key-value mappings.
        /// </summary>
        /// <remarks>Equality is determined by comparing the set of keys and the associated sets of values in both instances. The comparison is thread-safe and takes a snapshot of the current state of both maps.
        /// If either instance has been disposed, an exception is thrown.</remarks>
        /// <param name="obj">The object to compare with the current instance. Typically, this should be another <see cref="MultiMapAsync{TKey, TValue}"/>.</param>
        /// <returns>A ValueTask that represents the asynchronous operation. The result is <see langword="true"/> if the specified object is a <see cref="MultiMapAsync{TKey, TValue}"/> and contains the same keys and associated values as the current instance; otherwise, <see langword="false"/>.</returns>
        public ValueTask<bool> EqualsAsync(object? obj);

        /// <summary>
        /// Asynchronously determines whether the current instance and the specified <see cref="IReadOnlyMultiMapAsync{TKey, TValue}"/> are equal by comparing their key-value mappings.
        /// </summary>
        /// <remarks>
        /// When <paramref name="other"/> is another <see cref="MultiMapAsync{TKey, TValue}"/>, both semaphores are acquired atomically (lock-ordering by identity hash code to prevent deadlock) and a pair of snapshots is taken before any comparison work is done outside the locks. For any other <see cref="IReadOnlyMultiMapAsync{TKey, TValue}"/> implementation, a snapshot of this instance is taken under its own semaphore, and the comparison is then performed asynchronously against the other instance using its public async API.
        /// </remarks>
        /// <param name="other">The map to compare with the current instance.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> whose result is <see langword="true"/> if both maps contain the same keys with the same associated value sets; otherwise <see langword="false"/>.
        /// </returns>
        public ValueTask<bool> EqualsAsync(IReadOnlyMultiMapAsync<TKey, TValue>? other, CancellationToken cancellationToken = default);
    }
}
