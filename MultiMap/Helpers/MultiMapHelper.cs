using System.Runtime.CompilerServices;
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
        /// <remarks>
        /// This method is not atomic. When used with concurrent implementations such as <see cref="Entities.ConcurrentMultiMap{TKey, TValue}"/> or <see cref="Entities.MultiMapLock{TKey, TValue}"/>, concurrent modifications to <paramref name="target"/> or <paramref name="other"/> between individual operations may cause the result to reflect a mix of states rather than a point-in-time union. No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to add pairs into.</param>
        /// <param name="other">The multimap whose pairs are added to <paramref name="target"/>.</param>
        public static IMultiMap<TKey, TValue> Union<TKey, TValue>(this IMultiMap<TKey, TValue> target, IMultiMap<TKey, TValue> other)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            target.AddRange(other);

            return target;
        }

        /// <summary>
        /// Removes all key-value pairs from <paramref name="target"/> that do not exist in <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. It reads a snapshot of <paramref name="target"/> keys and values, then applies removals in a separate phase. When used with concurrent implementations, key-value pairs added to <paramref name="target"/> between the read and write phases will survive the intersect even if absent from <paramref name="other"/>. Pairs scheduled for <see cref="ISimpleMultiMap{TKey, TValue}.RemoveKey"/> removal will also remove any values added concurrently under the same key. No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to modify.</param>
        /// <param name="other">The multimap that defines the pairs to keep.</param>
        public static IMultiMap<TKey, TValue> Intersect<TKey, TValue>(this IMultiMap<TKey, TValue> target, IMultiMap<TKey, TValue> other)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            var keysToRemove = new HashSet<TKey>();
            // Cache of other's value sets per key (materialised to avoid aliasing a live set).
            var otherLookup = new Dictionary<TKey, HashSet<TValue>>();
            // Values to remove grouped by key so we call RemoveWhere once per key
            // (one dictionary lookup) instead of once per value via RemoveRange.
            var removeByKey = new Dictionary<TKey, HashSet<TValue>>();

            foreach (var kvp in target)
            {
                if (!other.ContainsKey(kvp.Key))
                {
                    keysToRemove.Add(kvp.Key);
                    continue;
                }

                if (!otherLookup.TryGetValue(kvp.Key, out var otherSet))
                {
                    // Always materialise into a fresh HashSet to avoid aliasing a live
                    // mutable set that could be modified concurrently on the source.
                    otherSet = new HashSet<TValue>(other.GetOrDefault(kvp.Key));
                    otherLookup[kvp.Key] = otherSet;
                }

                if (!otherSet.Contains(kvp.Value))
                {
                    if (!removeByKey.TryGetValue(kvp.Key, out var toRemove))
                    {
                        toRemove = new HashSet<TValue>();
                        removeByKey[kvp.Key] = toRemove;
                    }
                    toRemove.Add(kvp.Value);
                }
            }

            foreach (var key in keysToRemove)
            {
                target.RemoveKey(key);
            }

            foreach (var kvp in removeByKey)
            {
                target.RemoveWhere(kvp.Key, v => kvp.Value.Contains(v));
            }

            return target;
        }

        /// <summary>
        /// Removes all key-value pairs from <paramref name="target"/> that exist in <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. Each removal is individually atomic, but values added to <paramref name="target"/> between iterations that also exist in <paramref name="other"/> may not be removed. No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to remove pairs from.</param>
        /// <param name="other">The multimap whose pairs are removed from <paramref name="target"/>.</param>
        public static IMultiMap<TKey, TValue> ExceptWith<TKey, TValue>(this IMultiMap<TKey, TValue> target, IMultiMap<TKey, TValue> other)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            if (ReferenceEquals(target, other))
            {
                target.Clear();
                return target;
            }

            foreach (var kvp in other)
            {
                target.Remove(kvp.Key, kvp.Value);
            }

            return target;
        }

        /// <summary>
        /// Modifies <paramref name="target"/> to contain only pairs present in either
        /// <paramref name="target"/> or <paramref name="other"/>, but not both.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. It classifies pairs via <see cref="ICollection{TValue}.Contains"/> in a read phase, then applies additions and removals in separate write phases. When used with concurrent implementations, pairs added to <paramref name="target"/> between classification and mutation may be misclassified, leaving values that should have been removed or failing to add values that should have been included. No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to modify.</param>
        /// <param name="other">The multimap to compare against.</param>
        public static IMultiMap<TKey, TValue> SymmetricExceptWith<TKey, TValue>(this IMultiMap<TKey, TValue> target, IMultiMap<TKey, TValue> other)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            var otherCount = other.Count;
            var toRemove = new List<KeyValuePair<TKey, TValue>>(otherCount);
            var toAdd = new List<KeyValuePair<TKey, TValue>>(otherCount);

            var targetLookup = new Dictionary<TKey, HashSet<TValue>>(other.KeyCount);

            foreach (var kvp in other)
            {
                if (!targetLookup.TryGetValue(kvp.Key, out var targetSet))
                {
                    targetSet = new HashSet<TValue>(target.GetOrDefault(kvp.Key));
                    targetLookup[kvp.Key] = targetSet;
                }

                if (targetSet.Contains(kvp.Value))
                {
                    toRemove.Add(new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value));
                }
                else
                {
                    toAdd.Add(new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value));
                }
            }

            target.RemoveRange(toRemove);
            target.AddRange(toAdd);

            return target;
        }

        /// <summary>
        /// Determines whether <paramref name="target"/> is a subset of <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. It reads <paramref name="target"/> keys and values in a read phase,
        /// then queries <paramref name="other"/> for membership. Concurrent modifications to either multimap
        /// may cause the result to reflect a mix of states. No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <returns><see langword="true"/> if every key-value pair in <paramref name="target"/> exists in <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public static bool IsSubsetOf<TKey, TValue>(this IMultiMap<TKey, TValue> target, IMultiMap<TKey, TValue> other)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            var otherLookup = new Dictionary<TKey, HashSet<TValue>>(other.KeyCount);

            foreach (var kvp in target)
            {
                if (!otherLookup.TryGetValue(kvp.Key, out var otherSet))
                {
                    otherSet = new HashSet<TValue>(other.GetOrDefault(kvp.Key));
                    otherLookup[kvp.Key] = otherSet;
                }

                if (!otherSet.Contains(kvp.Value))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether <paramref name="target"/> is a superset of <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. It reads <paramref name="other"/> keys and values in a read phase,
        /// then queries <paramref name="target"/> for membership. Concurrent modifications to either multimap
        /// may cause the result to reflect a mix of states. No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <returns><see langword="true"/> if every key-value pair in <paramref name="other"/> exists in <paramref name="target"/>; otherwise, <see langword="false"/>.</returns>
        public static bool IsSupersetOf<TKey, TValue>(this IMultiMap<TKey, TValue> target, IMultiMap<TKey, TValue> other)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            return other.IsSubsetOf(target);
        }

        /// <summary>
        /// Determines whether <paramref name="target"/> and <paramref name="other"/> share at least one key-value pair.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. It reads <paramref name="target"/> keys and values in a read phase,
        /// then queries <paramref name="other"/> for membership. Concurrent modifications to either multimap
        /// may cause the result to reflect a mix of states. No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <returns><see langword="true"/> if at least one key-value pair exists in both multimaps; otherwise, <see langword="false"/>.</returns>
        public static bool Overlaps<TKey, TValue>(this IMultiMap<TKey, TValue> target, IMultiMap<TKey, TValue> other)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            var otherLookup = new Dictionary<TKey, HashSet<TValue>>(other.KeyCount);

            foreach (var kvp in target)
            {
                if (!otherLookup.TryGetValue(kvp.Key, out var otherSet))
                {
                    otherSet = new HashSet<TValue>(other.GetOrDefault(kvp.Key));
                    otherLookup[kvp.Key] = otherSet;
                }

                if (otherSet.Contains(kvp.Value))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether <paramref name="target"/> and <paramref name="other"/> contain the same key-value pairs.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. It compares total pair counts,
        /// Concurrent modifications to either multimap may cause the result to reflect a mix of states.
        /// No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <returns><see langword="true"/> if both multimaps contain exactly the same key-value pairs; otherwise, <see langword="false"/>.</returns>
        public static bool SetEquals<TKey, TValue>(this IMultiMap<TKey, TValue> target, IMultiMap<TKey, TValue> other)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            if (target.Count != other.Count || target.KeyCount != other.KeyCount)
                return false;

            var otherLookup = new Dictionary<TKey, HashSet<TValue>>(other.KeyCount);

            foreach (var kvp in target)
            {
                if (!otherLookup.TryGetValue(kvp.Key, out var otherSet))
                {
                    otherSet = new HashSet<TValue>(other.GetOrDefault(kvp.Key));
                    otherLookup[kvp.Key] = otherSet;
                }

                if (!otherSet.Contains(kvp.Value))
                    return false;
            }

            return true;
        }

        // ── ISimpleMultiMap overloads

        /// <summary>
        /// Adds all key-value pairs from <paramref name="other"/> into <paramref name="target"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to add pairs into.</param>
        /// <param name="other">The multimap whose pairs are added to <paramref name="target"/>.</param>
        public static ISimpleMultiMap<TKey, TValue> Union<TKey, TValue>(this ISimpleMultiMap<TKey, TValue> target, ISimpleMultiMap<TKey, TValue> other)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            foreach (var kvp in other)
            {
                target.Add(kvp.Key, kvp.Value);
            }

            return target;
        }

        /// <summary>
        /// Removes all key-value pairs from <paramref name="target"/> that do not exist in <paramref name="other"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to modify.</param>
        /// <param name="other">The multimap that defines the pairs to keep.</param>
        public static ISimpleMultiMap<TKey, TValue> Intersect<TKey, TValue>(this ISimpleMultiMap<TKey, TValue> target, ISimpleMultiMap<TKey, TValue> other)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            var toRemove = new List<KeyValuePair<TKey, TValue>>(target.Count);
            var otherLookup = new Dictionary<TKey, HashSet<TValue>>();

            foreach (var kvp in target)
            {
                if (!otherLookup.TryGetValue(kvp.Key, out var otherSet))
                {
                    otherSet = new HashSet<TValue>(other.GetOrDefault(kvp.Key));
                    otherLookup[kvp.Key] = otherSet;
                }

                if (!otherSet.Contains(kvp.Value))
                {
                    toRemove.Add(kvp);
                }
            }

            foreach (var kvp in toRemove)
            {
                target.Remove(kvp.Key, kvp.Value);
            }

            return target;
        }

        /// <summary>
        /// Removes all key-value pairs from <paramref name="target"/> that exist in <paramref name="other"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to remove pairs from.</param>
        /// <param name="other">The multimap whose pairs are removed from <paramref name="target"/>.</param>
        public static ISimpleMultiMap<TKey, TValue> ExceptWith<TKey, TValue>(this ISimpleMultiMap<TKey, TValue> target, ISimpleMultiMap<TKey, TValue> other)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            if (ReferenceEquals(target, other))
            {
                target.Clear();
                return target;
            }

            foreach (var kvp in other)
            {
                target.Remove(kvp.Key, kvp.Value);
            }

            return target;
        }

        /// <summary>
        /// Modifies <paramref name="target"/> to contain only pairs present in either
        /// <paramref name="target"/> or <paramref name="other"/>, but not both.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to modify.</param>
        /// <param name="other">The multimap to compare against.</param>
        public static ISimpleMultiMap<TKey, TValue> SymmetricExceptWith<TKey, TValue>(this ISimpleMultiMap<TKey, TValue> target, ISimpleMultiMap<TKey, TValue> other)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            var otherCount = other.Count;
            var toRemove = new List<KeyValuePair<TKey, TValue>>(otherCount);
            var toAdd = new List<KeyValuePair<TKey, TValue>>(otherCount);

            var targetLookup = new Dictionary<TKey, HashSet<TValue>>();

            foreach (var kvp in other)
            {
                if (!targetLookup.TryGetValue(kvp.Key, out var targetSet))
                {
                    targetSet = new HashSet<TValue>(target.GetOrDefault(kvp.Key));
                    targetLookup[kvp.Key] = targetSet;
                }

                if (targetSet.Contains(kvp.Value))
                {
                    toRemove.Add(kvp);
                }
                else
                {
                    toAdd.Add(kvp);
                }
            }

            foreach (var kvp in toRemove) target.Remove(kvp.Key, kvp.Value);
            foreach (var kvp in toAdd) target.Add(kvp.Key, kvp.Value);

            return target;
        }

        /// <summary>
        /// Determines whether <paramref name="target"/> is a subset of <paramref name="other"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <returns><see langword="true"/> if every key-value pair in <paramref name="target"/> exists in <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public static bool IsSubsetOf<TKey, TValue>(this ISimpleMultiMap<TKey, TValue> target, ISimpleMultiMap<TKey, TValue> other)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            foreach (var kvp in target)
            {
                if (!new HashSet<TValue>(other.GetOrDefault(kvp.Key)).Contains(kvp.Value))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether <paramref name="target"/> is a superset of <paramref name="other"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <returns><see langword="true"/> if every key-value pair in <paramref name="other"/> exists in <paramref name="target"/>; otherwise, <see langword="false"/>.</returns>
        public static bool IsSupersetOf<TKey, TValue>(this ISimpleMultiMap<TKey, TValue> target, ISimpleMultiMap<TKey, TValue> other)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            return other.IsSubsetOf(target);
        }

        /// <summary>
        /// Determines whether <paramref name="target"/> and <paramref name="other"/> share at least one key-value pair.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <returns><see langword="true"/> if at least one key-value pair exists in both multimaps; otherwise, <see langword="false"/>.</returns>
        public static bool Overlaps<TKey, TValue>(this ISimpleMultiMap<TKey, TValue> target, ISimpleMultiMap<TKey, TValue> other)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            foreach (var kvp in target)
            {
                if (new HashSet<TValue>(other.GetOrDefault(kvp.Key)).Contains(kvp.Value))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether <paramref name="target"/> and <paramref name="other"/> contain the same key-value pairs.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <returns><see langword="true"/> if both multimaps contain exactly the same key-value pairs; otherwise, <see langword="false"/>.</returns>
        public static bool SetEquals<TKey, TValue>(this ISimpleMultiMap<TKey, TValue> target, ISimpleMultiMap<TKey, TValue> other)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            if (target.Count != other.Count)
                return false;

            foreach (var kvp in target)
            {
                if (!new HashSet<TValue>(other.GetOrDefault(kvp.Key)).Contains(kvp.Value))
                    return false;
            }

            return true;
        }

        // ── IMultiMapAsync overloads

        /// <summary>
        /// Asynchronously adds all key-value pairs from <paramref name="other"/> into <paramref name="target"/>.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. Each awaited operation releases the underlying lock, allowing
        /// concurrent callers to interleave between steps. The result may reflect a mix of states
        /// rather than a point-in-time union. No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to add pairs into.</param>
        /// <param name="other">The multimap whose pairs are added to <paramref name="target"/>.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        public static async Task UnionAsync<TKey, TValue>(this IMultiMapAsync<TKey, TValue> target, IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            var keys = await other.GetKeysAsync(cancellationToken).ConfigureAwait(false);

            foreach (var key in keys)
            {
                var values = await other.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false);
                await target.AddRangeAsync(key, values, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Asynchronously removes all key-value pairs from <paramref name="target"/> that do not exist in <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. It snapshots keys and values in a read phase, then removes
        /// non-matching pairs in a write phase. Between awaited operations, concurrent callers may
        /// add pairs that survive the intersect or remove pairs redundantly. Pairs scheduled for
        /// key-level removal will also remove any values added concurrently under the same key.
        /// No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to modify.</param>
        /// <param name="other">The multimap that defines the pairs to keep.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        public static async Task IntersectAsync<TKey, TValue>(this IMultiMapAsync<TKey, TValue> target, IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            var keysToRemove = new List<TKey>();
            var valuesToRemove = new List<KeyValuePair<TKey, TValue>>();

            var targetKeys = await target.GetKeysAsync(cancellationToken).ConfigureAwait(false);
            foreach (var key in targetKeys)
            {
                if (!await other.ContainsKeyAsync(key, cancellationToken).ConfigureAwait(false))
                {
                    keysToRemove.Add(key);
                    continue;
                }

                var otherSet = new HashSet<TValue>(await other.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false));
                var values = await target.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false);
                foreach (var value in values)
                {
                    if (!otherSet.Contains(value))
                    {
                        valuesToRemove.Add(new KeyValuePair<TKey, TValue>(key, value));
                    }
                }
            }

            foreach (var key in keysToRemove)
            {
                await target.RemoveKeyAsync(key, cancellationToken).ConfigureAwait(false);
            }

            await target.RemoveRangeAsync(valuesToRemove, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously removes all key-value pairs from <paramref name="target"/> that exist in <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. Each removal is individually atomic, but values added to
        /// <paramref name="target"/> between awaited iterations that also exist in <paramref name="other"/>
        /// may not be removed. No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to remove pairs from.</param>
        /// <param name="other">The multimap whose pairs are removed from <paramref name="target"/>.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        public static async Task ExceptWithAsync<TKey, TValue>(this IMultiMapAsync<TKey, TValue> target, IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            if (ReferenceEquals(target, other))
            {
                await target.ClearAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            var itemsToRemove = new List<KeyValuePair<TKey, TValue>>();

            var keys = await other.GetKeysAsync(cancellationToken).ConfigureAwait(false);
            foreach (var key in keys)
            {
                var values = await other.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false);
                foreach (var value in values)
                {
                    itemsToRemove.Add(new KeyValuePair<TKey, TValue>(key, value));
                }
            }

            await target.RemoveRangeAsync(itemsToRemove, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously modifies <paramref name="target"/> to contain only pairs present in either
        /// <paramref name="target"/> or <paramref name="other"/>, but not both.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. It classifies pairs in a read phase, then applies additions
        /// and removals in separate write phases. Between awaited operations, concurrent callers may
        /// cause pairs to be misclassified, leaving values that should have been removed or failing
        /// to add values that should have been included. No structural corruption or count drift
        /// will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to modify.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        public static async Task SymmetricExceptWithAsync<TKey, TValue>(this IMultiMapAsync<TKey, TValue> target, IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            var toRemove = new List<KeyValuePair<TKey, TValue>>();
            var toAdd = new List<KeyValuePair<TKey, TValue>>();

            var otherKeys = await other.GetKeysAsync(cancellationToken).ConfigureAwait(false);
            foreach (var key in otherKeys)
            {
                var targetSet = new HashSet<TValue>(await target.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false));
                var values = await other.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false);
                foreach (var value in values)
                {
                    if (targetSet.Contains(value))
                    {
                        toRemove.Add(new KeyValuePair<TKey, TValue>(key, value));
                    }
                    else
                    {
                        toAdd.Add(new KeyValuePair<TKey, TValue>(key, value));
                    }
                }
            }

            await target.RemoveRangeAsync(toRemove, cancellationToken).ConfigureAwait(false);
            await target.AddRangeAsync(toAdd, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously determines whether <paramref name="target"/> is a subset of <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. Each awaited operation releases the underlying lock, allowing
        /// concurrent callers to interleave between steps. The result may reflect a mix of states.
        /// No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns><see langword="true"/> if every key-value pair in <paramref name="target"/> exists in <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public static async Task<bool> IsSubsetOfAsync<TKey, TValue>(this IMultiMapAsync<TKey, TValue> target, IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            var targetKeys = await target.GetKeysAsync(cancellationToken).ConfigureAwait(false);
            foreach (var key in targetKeys)
            {
                var targetValues = await target.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false);
                var otherValues = await other.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false);

                var otherSet = new HashSet<TValue>(otherValues);

                foreach (var value in targetValues)
                {
                    if (!otherSet.Contains(value))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Asynchronously determines whether <paramref name="target"/> is a superset of <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. Each awaited operation releases the underlying lock, allowing
        /// concurrent callers to interleave between steps. The result may reflect a mix of states.
        /// No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns><see langword="true"/> if every key-value pair in <paramref name="other"/> exists in <paramref name="target"/>; otherwise, <see langword="false"/>.</returns>
        public static async Task<bool> IsSupersetOfAsync<TKey, TValue>(this IMultiMapAsync<TKey, TValue> target, IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            return await other.IsSubsetOfAsync(target, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously determines whether <paramref name="target"/> and <paramref name="other"/> share at least one key-value pair.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. Each awaited operation releases the underlying lock, allowing
        /// concurrent callers to interleave between steps. The result may reflect a mix of states.
        /// No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns><see langword="true"/> if at least one key-value pair exists in both multimaps; otherwise, <see langword="false"/>.</returns>
        public static async Task<bool> OverlapsAsync<TKey, TValue>(this IMultiMapAsync<TKey, TValue> target, IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            var targetKeys = await target.GetKeysAsync(cancellationToken).ConfigureAwait(false);
            foreach (var key in targetKeys)
            {
                var targetValues = await target.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false);
                var otherValues = await other.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false);

                var otherSet = new HashSet<TValue>(otherValues);

                foreach (var value in targetValues)
                {
                    if (otherSet.Contains(value))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Asynchronously determines whether <paramref name="target"/> and <paramref name="other"/> contain the same key-value pairs.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. Each awaited operation releases the underlying lock, allowing
        /// concurrent callers to interleave between steps. The result may reflect a mix of states.
        /// No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns><see langword="true"/> if both multimaps contain exactly the same key-value pairs; otherwise, <see langword="false"/>.</returns>
        public static async Task<bool> SetEqualsAsync<TKey, TValue>(this IMultiMapAsync<TKey, TValue> target, IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
            where TKey : notnull, IEquatable<TKey>
            where TValue : notnull, IEquatable<TValue>
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            var targetCount = await target.GetCountAsync(cancellationToken).ConfigureAwait(false);
            var otherCount = await other.GetCountAsync(cancellationToken).ConfigureAwait(false);
            var targetKeyCount = await target.GetKeyCountAsync(cancellationToken).ConfigureAwait(false);
            var otherKeyCount = await other.GetKeyCountAsync(cancellationToken).ConfigureAwait(false);

            if (targetCount != otherCount || targetKeyCount != otherKeyCount)
                return false;

            var targetKeys = await target.GetKeysAsync(cancellationToken).ConfigureAwait(false);
            foreach (var key in targetKeys)
            {
                var targetValues = await target.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false);
                var otherValues = await other.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false);

                var otherSet = new HashSet<TValue>(otherValues);

                foreach (var value in targetValues)
                {
                    if (!otherSet.Contains(value))
                        return false;
                }
            }

            return true;
        }

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
