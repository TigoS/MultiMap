using System.Runtime.CompilerServices;

namespace MultiMap.Helpers
{
    /// <summary>
    /// Provides set-like extension methods for multimap instances.
    /// </summary>
    /// <remarks>
    /// Methods are organized into two partial-class files by concern:
    /// <list type="bullet">
    ///   <item><description><see cref="MultiMapHelper"/> (this file) — internal hash utilities used by entity implementations.</description></item>
    ///   <item><description><c>MultiMapHelper.SetOperations.cs</c> — mutating set operations: <c>Union</c>, <c>Intersect</c>, <c>ExceptWith</c>, <c>SymmetricExceptWith</c> for <see cref="Interfaces.IMultiMap{TKey,TValue}"/>, <see cref="Interfaces.ISimpleMultiMap{TKey,TValue}"/>, and <see cref="Interfaces.IMultiMapAsync{TKey,TValue}"/>.</description></item>
    ///   <item><description><c>MultiMapHelper.QueryExtensions.cs</c> — read-only set algebra queries: <c>IsSubsetOf</c>, <c>IsSupersetOf</c>, <c>Overlaps</c>, <c>SetEquals</c> for all three families.</description></item>
    /// </list>
    /// </remarks>
    public static partial class MultiMapHelper
    {
        /// <summary>
        /// Scrambles a hash code
        /// </summary>
        /// <param name="h">The hash code to scramble.</param>
        /// <returns>The scrambled hash code.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Scramble(int h)
        {
            unchecked
            {
                h ^= h >> 16;
                h *= -2048144789;
                h ^= h >> 13;
                h *= -1028477387;
                h ^= h >> 16;
            }
            return h;
        }

        /// <summary>
        /// Computes an order-independent hash code for a dictionary of key-to-collection entries.
        /// When a custom comparer is supplied the hash is derived from <see cref="IEqualityComparer{T}.GetHashCode(T)"/> so that the result is consistent with the equality semantics used by the owning map instance.
        /// </summary>
        /// <typeparam name="TKey">The type of keys.</typeparam>
        /// <typeparam name="TValue">The type of values in each collection.</typeparam>
        /// <typeparam name="TCollection">The collection type that implements <see cref="IEnumerable{TValue}"/>.</typeparam>
        /// <param name="entries">The key-collection pairs to compute the hash for.</param>
        /// <param name="keyComparer">
        /// The equality comparer used for keys, or <see langword="null"/> to use <see cref="EqualityComparer{T}.Default"/>.
        /// </param>
        /// <param name="valueComparer">
        /// The equality comparer used for values, or <see langword="null"/> to use <see cref="EqualityComparer{T}.Default"/>.
        /// </param>
        /// <returns>A hash code that is independent of the enumeration order of keys and values.</returns>
        internal static int ComputeUnorderedHash<TKey, TValue, TCollection>(
            IEnumerable<KeyValuePair<TKey, TCollection>> entries,
            IEqualityComparer<TKey>? keyComparer = null,
            IEqualityComparer<TValue>? valueComparer = null)
            where TKey : notnull
            where TValue : notnull
            where TCollection : IEnumerable<TValue>
        {
            var kc = keyComparer ?? EqualityComparer<TKey>.Default;
            var vc = valueComparer ?? EqualityComparer<TValue>.Default;

            unchecked
            {
                int hash = 0;

                foreach (var kvp in entries)
                {
                    int valueHash = 0;
                    foreach (var value in kvp.Value)
                    {
                        valueHash += Scramble(vc.GetHashCode(value));
                    }
                    hash += Scramble(HashCode.Combine(kc.GetHashCode(kvp.Key), valueHash));
                }

                return hash;
            }
        }
    }
}
