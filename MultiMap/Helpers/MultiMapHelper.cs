using MultiMap.Interfaces;

namespace MultiMap.Helpers
{
    /// <summary>
    /// Provides set-like extension methods for <see cref="IMultiMap{TKey, TValue}"/> instances.
    /// </summary>
    public static class MultiMapHelper
    {
        /// <summary>
        /// Adds all key-value pairs from <paramref name="other"/> into <paramref name="target"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to add pairs into.</param>
        /// <param name="other">The multimap whose pairs are added to <paramref name="target"/>.</param>
        public static void Union<TKey, TValue>(this IMultiMap<TKey, TValue> target, IMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            foreach (var kvp in other)
            {
                target.Add(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Removes all key-value pairs from <paramref name="target"/> that do not exist in <paramref name="other"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to modify.</param>
        /// <param name="other">The multimap that defines the pairs to keep.</param>
        public static void Intersect<TKey, TValue>(this IMultiMap<TKey, TValue> target, IMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            var toRemove = new List<KeyValuePair<TKey, TValue>>();

            foreach (var kvp in target)
            {
                if (!other.Contains(kvp.Key, kvp.Value))
                {
                    toRemove.Add(kvp);
                }
            }

            foreach (var kvp in toRemove)
            {
                target.Remove(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Removes all key-value pairs from <paramref name="target"/> that exist in <paramref name="other"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to remove pairs from.</param>
        /// <param name="other">The multimap whose pairs are removed from <paramref name="target"/>.</param>
        public static void ExceptWith<TKey, TValue>(this IMultiMap<TKey, TValue> target, IMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            foreach (var kvp in other)
            {
                target.Remove(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Modifies <paramref name="target"/> to contain only pairs present in either
        /// <paramref name="target"/> or <paramref name="other"/>, but not both.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to modify.</param>
        /// <param name="other">The multimap to compare against.</param>
        public static void SymmetricExceptWith<TKey, TValue>(this IMultiMap<TKey, TValue> target, IMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            var toRemove = new List<KeyValuePair<TKey, TValue>>();
            var toAdd = new List<KeyValuePair<TKey, TValue>>();

            foreach (var kvp in other)
            {
                if (target.Contains(kvp.Key, kvp.Value))
                {
                    toRemove.Add(kvp);
                }
                else
                {
                    toAdd.Add(kvp);
                }
            }

            foreach (var kvp in toRemove)
            {
                target.Remove(kvp.Key, kvp.Value);
            }

            foreach (var kvp in toAdd)
            {
                target.Add(kvp.Key, kvp.Value);
            }
        }
    }
}
