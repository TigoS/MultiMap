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
        /// This method is not atomic. When used with concurrent implementations such as
        /// <see cref="Entities.ConcurrentMultiMap{TKey, TValue}"/> or <see cref="Entities.MultiMapLock{TKey, TValue}"/>,
        /// concurrent modifications to <paramref name="target"/> or <paramref name="other"/> between
        /// individual operations may cause the result to reflect a mix of states rather than a
        /// point-in-time union. No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to add pairs into.</param>
        /// <param name="other">The multimap whose pairs are added to <paramref name="target"/>.</param>
        public static void Union<TKey, TValue>(this IMultiMap<TKey, TValue> target, IMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            foreach (var key in other.Keys)
            {
                target.AddRange(key, other.GetOrDefault(key));
            }
        }

        /// <summary>
        /// Removes all key-value pairs from <paramref name="target"/> that do not exist in <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. It reads a snapshot of <paramref name="target"/> keys and values,
        /// then applies removals in a separate phase. When used with concurrent implementations,
        /// key-value pairs added to <paramref name="target"/> between the read and write phases will
        /// survive the intersect even if absent from <paramref name="other"/>. Pairs scheduled for
        /// <see cref="IMultiMap{TKey, TValue}.RemoveKey"/> removal will also remove any values added
        /// concurrently under the same key. No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to modify.</param>
        /// <param name="other">The multimap that defines the pairs to keep.</param>
        public static void Intersect<TKey, TValue>(this IMultiMap<TKey, TValue> target, IMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            var keysToRemove = new List<TKey>();
            var valuesToRemove = new List<KeyValuePair<TKey, TValue>>();
            var targetEnumerator = target.GetEnumerator();

            while (targetEnumerator.MoveNext())
            {
                var kvp = targetEnumerator.Current;

                if (!other.ContainsKey(kvp.Key))
                {
                    keysToRemove.Add(kvp.Key);
                    continue;
                }

                var otherValues = other.GetOrDefault(kvp.Key);
                var otherSet = otherValues as ISet<TValue> ?? new HashSet<TValue>(otherValues);
                if (!otherSet.Contains(kvp.Value))
                {
                    valuesToRemove.Add(new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value));
                }
            }

            foreach (var key in keysToRemove)
            {
                target.RemoveKey(key);
            }

            target.RemoveRange(valuesToRemove);
        }

        /// <summary>
        /// Removes all key-value pairs from <paramref name="target"/> that exist in <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. Each removal is individually atomic, but values added to
        /// <paramref name="target"/> between iterations that also exist in <paramref name="other"/>
        /// may not be removed. No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to remove pairs from.</param>
        /// <param name="other">The multimap whose pairs are removed from <paramref name="target"/>.</param>
        public static void ExceptWith<TKey, TValue>(this IMultiMap<TKey, TValue> target, IMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            var itemsToRemove = new List<KeyValuePair<TKey, TValue>>();
            var otherEnumerator = other.GetEnumerator();

            while (otherEnumerator.MoveNext())
            {
                var kvp = otherEnumerator.Current;
                itemsToRemove.Add(new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value));
            }

            target.RemoveRange(itemsToRemove);
        }

        /// <summary>
        /// Modifies <paramref name="target"/> to contain only pairs present in either
        /// <paramref name="target"/> or <paramref name="other"/>, but not both.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. It classifies pairs via <see cref="IReadOnlyMultiMap{TKey, TValue}.Contains"/>
        /// in a read phase, then applies additions and removals in separate write phases. When used with
        /// concurrent implementations, pairs added to <paramref name="target"/> between classification and
        /// mutation may be misclassified, leaving values that should have been removed or failing to add
        /// values that should have been included. No structural corruption or count drift will occur.
        /// </remarks>
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
            var otherEnumerator = other.GetEnumerator();

            while (otherEnumerator.MoveNext())
            {
                var kvp = otherEnumerator.Current;
                var targetValues = target.GetOrDefault(kvp.Key);
                var targetSet = targetValues as ISet<TValue> ?? new HashSet<TValue>(targetValues);
                
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
        }

        // ── ISimpleMultiMap overloads

        /// <summary>
        /// Adds all key-value pairs from <paramref name="other"/> into <paramref name="target"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to add pairs into.</param>
        /// <param name="other">The multimap whose pairs are added to <paramref name="target"/>.</param>
        public static ISimpleMultiMap<TKey, TValue> Union<TKey, TValue>(this ISimpleMultiMap<TKey, TValue> target, ISimpleMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            foreach (var kvp in other)
            {
                target.Add(kvp.Key, kvp.Value);
            }

            return target;
        }

        /// <summary>
        /// Removes all key-value pairs from <paramref name="target"/> that do not exist in <paramref name="other"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to modify.</param>
        /// <param name="other">The multimap that defines the pairs to keep.</param>
        public static ISimpleMultiMap<TKey, TValue> Intersect<TKey, TValue>(this ISimpleMultiMap<TKey, TValue> target, ISimpleMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            var toRemove = new List<KeyValuePair<TKey, TValue>>();
            var otherLookup = new Dictionary<TKey, ISet<TValue>>();

            foreach (var kvp in target.Flatten())
            {
                if (!otherLookup.TryGetValue(kvp.Key, out var otherSet))
                {
                    var raw = other.GetOrDefault(kvp.Key);
                    otherSet = raw as ISet<TValue> ?? new HashSet<TValue>(raw);
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
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to remove pairs from.</param>
        /// <param name="other">The multimap whose pairs are removed from <paramref name="target"/>.</param>
        public static ISimpleMultiMap<TKey, TValue> ExceptWith<TKey, TValue>(this ISimpleMultiMap<TKey, TValue> target, ISimpleMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
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
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to modify.</param>
        /// <param name="other">The multimap to compare against.</param>
        public static ISimpleMultiMap<TKey, TValue> SymmetricExceptWith<TKey, TValue>(this ISimpleMultiMap<TKey, TValue> target, ISimpleMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            var toRemove = new List<KeyValuePair<TKey, TValue>>();
            var toAdd = new List<KeyValuePair<TKey, TValue>>();

            var targetLookup = new Dictionary<TKey, ISet<TValue>>();

            foreach (var kvp in other.Flatten())
            {
                if (!targetLookup.TryGetValue(kvp.Key, out var targetSet))
                {
                    var raw = target.GetOrDefault(kvp.Key);
                    targetSet = raw as ISet<TValue> ?? new HashSet<TValue>(raw);
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

            foreach (var kvp in toRemove)
            {
                target.Remove(kvp.Key, kvp.Value);
            }

            foreach (var kvp in toAdd)
            {
                target.Add(kvp.Key, kvp.Value);
            }

            return target;
        }

        // ── IMultiMapAsync overloads ──────────────────────

        /// <summary>
        /// Asynchronously adds all key-value pairs from <paramref name="other"/> into <paramref name="target"/>.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. Each awaited operation releases the underlying lock, allowing
        /// concurrent callers to interleave between steps. The result may reflect a mix of states
        /// rather than a point-in-time union. No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to add pairs into.</param>
        /// <param name="other">The multimap whose pairs are added to <paramref name="target"/>.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        public static async Task UnionAsync<TKey, TValue>(this IMultiMapAsync<TKey, TValue> target, IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
            where TKey : notnull
            where TValue : notnull
        {
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
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to modify.</param>
        /// <param name="other">The multimap that defines the pairs to keep.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        public static async Task IntersectAsync<TKey, TValue>(this IMultiMapAsync<TKey, TValue> target, IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
            where TKey : notnull
            where TValue : notnull
        {
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

                var otherValues = await other.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false);
                var otherSet = otherValues as ISet<TValue> ?? new HashSet<TValue>(otherValues);
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
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to remove pairs from.</param>
        /// <param name="other">The multimap whose pairs are removed from <paramref name="target"/>.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        public static async Task ExceptWithAsync<TKey, TValue>(this IMultiMapAsync<TKey, TValue> target, IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
            where TKey : notnull
            where TValue : notnull
        {
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
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to modify.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        public static async Task SymmetricExceptWithAsync<TKey, TValue>(this IMultiMapAsync<TKey, TValue> target, IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
            where TKey : notnull
            where TValue : notnull
        {
            var toRemove = new List<KeyValuePair<TKey, TValue>>();
            var toAdd = new List<KeyValuePair<TKey, TValue>>();

            var otherKeys = await other.GetKeysAsync(cancellationToken).ConfigureAwait(false);
            foreach (var key in otherKeys)
            {
                var targetValues = await target.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false);
                var targetSet = targetValues as ISet<TValue> ?? new HashSet<TValue>(targetValues);
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
    }
}
