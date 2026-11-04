namespace MultiMap.Entities
{
    /// <summary>
    /// Provides an intermediate abstract base class for concurrent multimap implementations
    /// that need atomic counter operations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class sits between <see cref="MultiMapBase{TKey,TValue,TCollection}"/> and
    /// <see cref="ConcurrentMultiMap{TKey,TValue}"/> in the inheritance hierarchy. It exists
    /// solely to confine <see cref="System.Threading.Interlocked"/> and
    /// <see cref="System.Threading.Volatile"/> counter helpers to the types that actually
    /// require them, rather than exposing them on the non-concurrent base class.
    /// </para>
    /// <para>
    /// <see cref="MultiMapBase{TKey,TValue,TCollection}"/> retains the non-atomic helpers
    /// (<c>IncrementCount</c>, <c>DecrementCount</c>, <c>ResetCount</c>) used by
    /// <see cref="MultiMapSet{TKey,TValue}"/>, <see cref="MultiMapList{TKey,TValue}"/>, and
    /// <see cref="SortedMultiMap{TKey,TValue}"/>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key.</typeparam>
    /// <typeparam name="TCollection">The thread-safe collection type used to store values under each key.</typeparam>
    public abstract class ConcurrentMultiMapBase<TKey, TValue, TCollection>(IDictionary<TKey, TCollection> dictionary)
        : MultiMapBase<TKey, TValue, TCollection>(dictionary)
        where TKey : notnull
        where TValue : notnull
        where TCollection : ICollection<TValue>
    {
        /// <summary>Atomically increments the value count by one and returns the new value.</summary>
        protected int InterlockedIncrementCount() => Interlocked.Increment(ref _count);

        /// <summary>Atomically adds <paramref name="by"/> to the value count and returns the new value.</summary>
        protected int InterlockedAddCount(int by) => Interlocked.Add(ref _count, by);

        /// <summary>Atomically sets the value count to <paramref name="value"/> and returns the original value.</summary>
        protected int InterlockedExchangeCount(int value) => Interlocked.Exchange(ref _count, value);

        /// <summary>Reads the value count with acquire semantics.</summary>
        protected int VolatileReadCount() => Volatile.Read(ref _count);
    }
}
