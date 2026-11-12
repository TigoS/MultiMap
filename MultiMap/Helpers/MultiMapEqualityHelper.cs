using MultiMap.Interfaces;

namespace MultiMap.Helpers
{
    /// <summary>
    /// Provides shared key-based equality logic for read-only multi-map implementations.
    /// </summary>
    /// <remarks>Performs fast checks for null, reference equality, and collection size before validating each
    /// key and its associated values through a caller-provided comparison delegate.</remarks>
    internal static class MultiMapEqualityHelper
    {
        /// <summary>
        /// Determines whether two multi-maps are equal by key using a supplied key sequence and per-key value
        /// comparison function.
        /// </summary>
        /// <remarks>Comparison uses the provided key sequence and delegates per-key value equality to the
        /// supplied function.</remarks>
        /// <typeparam name="TKey">The type of the keys in the multi-maps.</typeparam>
        /// <typeparam name="TValue">The type of the values in the multi-maps.</typeparam>
        /// <param name="left">The multi-map used as the baseline for key and value comparison.</param>
        /// <param name="right">The multi-map to compare against the baseline multi-map.</param>
        /// <param name="leftKeys">The keys from the baseline multi-map to evaluate for equality.</param>
        /// <param name="valuesEqualForKey">A function that returns whether the values for a specified key are equal.</param>
        /// <returns><see langword="true"/> when the compared multi-map is equal by key and values for the evaluated keys;
        /// otherwise, <see langword="false"/>.</returns>
        internal static bool EqualsByKey<TKey, TValue>(
            IReadOnlyMultiMap<TKey, TValue> left,
            IReadOnlyMultiMap<TKey, TValue>? right,
            IEnumerable<TKey> leftKeys,
            Func<TKey, IEnumerable<TValue>, bool> valuesEqualForKey)
            where TKey : notnull
            where TValue : notnull
            => EqualsByKey(left, right, left.KeyCount, left.Count, leftKeys, valuesEqualForKey);

        /// <summary>
        /// Determines whether two read-only multimaps contain the same keys and equivalent values for each key.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="left">The left multimap used as the comparison source.</param>
        /// <param name="right">The right multimap to compare with the left multimap.</param>
        /// <param name="leftKeyCount">The number of distinct keys in the left multimap.</param>
        /// <param name="leftCount">The total number of values in the left multimap.</param>
        /// <param name="leftKeys">The keys from the left multimap to validate against the right multimap.</param>
        /// <param name="valuesEqualForKey">Compares the left values for a key with the values retrieved from the right multimap for the same key.</param>
        /// <returns><see langword="true"/> when both multimaps are the same instance, or when the right multimap is not <see
        /// langword="null"/>, counts match, every key exists in the right multimap, and values are equivalent for each
        /// key; otherwise, <see langword="false"/>.</returns>
        internal static bool EqualsByKey<TKey, TValue>(
            IReadOnlyMultiMap<TKey, TValue> left,
            IReadOnlyMultiMap<TKey, TValue>? right,
            int leftKeyCount,
            int leftCount,
            IEnumerable<TKey> leftKeys,
            Func<TKey, IEnumerable<TValue>, bool> valuesEqualForKey)
            where TKey : notnull
            where TValue : notnull
        {
            if (right is null)
            {
                return false;
            }

            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (leftKeyCount != right.KeyCount || leftCount != right.Count)
            {
                return false;
            }

            foreach (var key in leftKeys)
            {
                if (!right.TryGet(key, out var rightValues))
                {
                    return false;
                }

                if (!valuesEqualForKey(key, rightValues))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether two read-only multimaps are equal by comparing key presence and values for each key in a
        /// specified key sequence.
        /// </summary>
        /// <remarks>Returns true immediately when both parameters reference the same multimap
        /// instance.</remarks>
        /// <typeparam name="TKey">The type of keys in the multimaps.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimaps.</typeparam>
        /// <param name="left">The left multimap to compare.</param>
        /// <param name="right">The right multimap to compare.</param>
        /// <param name="leftKeys">The keys from the left multimap to evaluate against the right multimap.</param>
        /// <param name="valuesEqualForKey">A delegate that compares values for a key using the values from the right multimap.</param>
        /// <returns>true if the right multimap is not null, both multimaps have the same count, every key is present in the
        /// right multimap, and values are equal for each key; otherwise, false.</returns>
        internal static bool EqualsByKey<TKey, TValue>(
            IReadOnlySimpleMultiMap<TKey, TValue> left,
            IReadOnlySimpleMultiMap<TKey, TValue>? right,
            IEnumerable<TKey> leftKeys,
            Func<TKey, IEnumerable<TValue>, bool> valuesEqualForKey)
            where TKey : notnull
            where TValue : notnull
        {
            if (right is null)
            {
                return false;
            }

            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left.Count != right.Count)
            {
                return false;
            }

            foreach (var key in leftKeys)
            {
                if (!right.TryGet(key, out var rightValues))
                {
                    return false;
                }

                if (!valuesEqualForKey(key, rightValues))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
