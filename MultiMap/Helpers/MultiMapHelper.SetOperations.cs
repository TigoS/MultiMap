using MultiMap.Interfaces;

namespace MultiMap.Helpers
{
    public static partial class MultiMapHelper
    {
        // ── IMultiMap<TKey, TValue> set operations ──────────────────────────────

        /// <summary>
        /// Adds all key-value pairs from <paramref name="other"/> into <paramref name="target"/>.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. When used with concurrent implementations such as <see cref="Entities.ConcurrentMultiMap{TKey, TValue}"/> or <see cref="Entities.MultiMapLock{TKey, TValue}"/>, concurrent modifications to <paramref name="target"/> or <paramref name="other"/> between individual operations may cause the result to reflect a mix of states rather than a point-in-time union. No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to add pairs into.</param>
        /// <param name="other">The multimap whose pairs are added to <paramref name="target"/>.</param>
        public static IMultiMap<TKey, TValue> Union<TKey, TValue>(this IMultiMap<TKey, TValue> target, IReadOnlyMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(target, nameof(target));
            ArgumentNullException.ThrowIfNull(other, nameof(other));

            target.AddRange(other);

            return target;
        }

        /// <summary>
        /// Removes all key-value pairs from <paramref name="target"/> that do not exist in <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. It reads a snapshot of <paramref name="target"/> keys and values, then applies removals in a separate phase. When used with concurrent implementations, key-value pairs added to <paramref name="target"/> between the read and write phases will survive the intersect even if absent from <paramref name="other"/>. Pairs scheduled for <see cref="ISimpleMultiMap{TKey, TValue}.RemoveKey"/> removal will also remove any values added concurrently under the same key. No structural corruption or count drift will occur.
        /// <para>
        /// The intermediate collections used during classification are pre-sized using <see cref="IReadOnlyMultiMap{TKey, TValue}.KeyCount"/>
        /// to minimise rehashing: <c>keysToRemove</c> is pre-sized to <c>target.KeyCount</c>; <c>otherLookup</c> and
        /// <c>removeByKey</c> are pre-sized to <c>other.KeyCount</c>.
        /// </para>
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to modify.</param>
        /// <param name="other">The multimap that defines the pairs to keep.</param>
        public static IMultiMap<TKey, TValue> Intersect<TKey, TValue>(this IMultiMap<TKey, TValue> target, IReadOnlyMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(target, nameof(target));
            ArgumentNullException.ThrowIfNull(other, nameof(other));

            // Pre-sized to the number of keys in target: worst case every key is absent from other.
            var keysToRemove = new HashSet<TKey>(target.KeyCount);
            // Pre-sized to other.KeyCount: at most one entry per key in other will be cached.
            var otherLookup = new Dictionary<TKey, HashSet<TValue>>(other.KeyCount);
            // Values to remove grouped by key so we call RemoveWhere once per key
            // (one dictionary lookup) instead of once per value via RemoveRange.
            // Pre-sized to other.KeyCount: at most one entry per key in other can have values to remove.
            var removeByKey = new Dictionary<TKey, HashSet<TValue>>(other.KeyCount);

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
                    otherSet = new HashSet<TValue>([.. other.GetOrDefault(kvp.Key)]);
                    otherLookup[kvp.Key] = otherSet;
                }

                if (!otherSet.Contains(kvp.Value))
                {
                    if (!removeByKey.TryGetValue(kvp.Key, out var toRemove))
                    {
                        toRemove = [];
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
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to remove pairs from.</param>
        /// <param name="other">The multimap whose pairs are removed from <paramref name="target"/>.</param>
        public static IMultiMap<TKey, TValue> ExceptWith<TKey, TValue>(this IMultiMap<TKey, TValue> target, IReadOnlyMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(target, nameof(target));
            ArgumentNullException.ThrowIfNull(other, nameof(other));

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
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to modify.</param>
        /// <param name="other">The multimap to compare against.</param>
        public static IMultiMap<TKey, TValue> SymmetricExceptWith<TKey, TValue>(this IMultiMap<TKey, TValue> target, IReadOnlyMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(target, nameof(target));
            ArgumentNullException.ThrowIfNull(other, nameof(other));

            var otherCount = other.Count;
            var toRemove = new List<KeyValuePair<TKey, TValue>>(otherCount);
            var toAdd = new List<KeyValuePair<TKey, TValue>>(otherCount);

            var targetLookup = new Dictionary<TKey, HashSet<TValue>>(other.KeyCount);

            foreach (var kvp in other)
            {
                if (!targetLookup.TryGetValue(kvp.Key, out var targetSet))
                {
                    targetSet = [.. target.GetOrDefault(kvp.Key)];
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

        // ── ISimpleMultiMap<TKey, TValue> set operations ────────────────────────

        /// <summary>
        /// Adds all key-value pairs from <paramref name="other"/> into <paramref name="target"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to add pairs into.</param>
        /// <param name="other">The multimap whose pairs are added to <paramref name="target"/>.</param>
        public static ISimpleMultiMap<TKey, TValue> Union<TKey, TValue>(this ISimpleMultiMap<TKey, TValue> target, IReadOnlySimpleMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(target, nameof(target));
            ArgumentNullException.ThrowIfNull(other, nameof(other));

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
        public static ISimpleMultiMap<TKey, TValue> Intersect<TKey, TValue>(this ISimpleMultiMap<TKey, TValue> target, IReadOnlySimpleMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(target, nameof(target));
            ArgumentNullException.ThrowIfNull(other, nameof(other));

            var toRemove = new List<KeyValuePair<TKey, TValue>>(target.Count);
            var otherLookup = new Dictionary<TKey, HashSet<TValue>>();

            foreach (var kvp in target)
            {
                if (!otherLookup.TryGetValue(kvp.Key, out var otherSet))
                {
                    otherSet = [.. other.GetOrDefault(kvp.Key)];
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
        public static ISimpleMultiMap<TKey, TValue> ExceptWith<TKey, TValue>(this ISimpleMultiMap<TKey, TValue> target, IReadOnlySimpleMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(target, nameof(target));
            ArgumentNullException.ThrowIfNull(other, nameof(other));

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
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to modify.</param>
        /// <param name="other">The multimap to compare against.</param>
        public static ISimpleMultiMap<TKey, TValue> SymmetricExceptWith<TKey, TValue>(this ISimpleMultiMap<TKey, TValue> target, IReadOnlySimpleMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(target, nameof(target));
            ArgumentNullException.ThrowIfNull(other, nameof(other));

            var otherCount = other.Count;
            var toRemove = new List<KeyValuePair<TKey, TValue>>(otherCount);
            var toAdd = new List<KeyValuePair<TKey, TValue>>(otherCount);

            var targetLookup = new Dictionary<TKey, HashSet<TValue>>();

            foreach (var kvp in other)
            {
                if (!targetLookup.TryGetValue(kvp.Key, out var targetSet))
                {
                    targetSet = [.. target.GetOrDefault(kvp.Key)];
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

        // ── IMultiMapAsync<TKey, TValue> set operations ─────────────────────────

        /// <summary>
        /// Asynchronously adds all key-value pairs from <paramref name="other"/> into <paramref name="target"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is not atomic. Each awaited operation releases the underlying lock, allowing
        /// concurrent callers to interleave between steps. The result may reflect a mix of states
        /// rather than a point-in-time union. No structural corruption or count drift will occur.
        /// </para>
        /// <para>
        /// <b>Cancellation:</b> The <paramref name="cancellationToken"/> is forwarded to every
        /// individual <see cref="Interfaces.IMultiMapAsync{TKey, TValue}"/> call
        /// (<c>GetKeysAsync</c>, <c>GetOrDefaultAsync</c>, <c>AddRangeAsync</c>), so cancellation
        /// is observed at each await boundary. If the token is cancelled mid-operation,
        /// <see cref="OperationCanceledException"/> is thrown and already-added pairs are
        /// <b>not</b> rolled back.
        /// </para>
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to add pairs into.</param>
        /// <param name="other">The multimap whose pairs are added to <paramref name="target"/>.</param>
        /// <param name="cancellationToken">A token to cancel the operation. Checked at every awaited <see cref="Interfaces.IMultiMapAsync{TKey, TValue}"/> call; throws <see cref="OperationCanceledException"/> without rolling back already-applied changes.</param>
        public static async Task UnionAsync<TKey, TValue>(this IMultiMapAsync<TKey, TValue> target, IReadOnlyMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
            where TKey : notnull
            where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(target, nameof(target));
            ArgumentNullException.ThrowIfNull(other, nameof(other));

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
        /// <para>
        /// This method is not atomic. It snapshots keys and values in a read phase, then removes
        /// non-matching pairs in a write phase. Between awaited operations, concurrent callers may
        /// add pairs that survive the intersect or remove pairs redundantly. Pairs scheduled for
        /// key-level removal will also remove any values added concurrently under the same key.
        /// No structural corruption or count drift will occur.
        /// </para>
        /// <para>
        /// <b>Cancellation:</b> The <paramref name="cancellationToken"/> is forwarded to every
        /// individual <see cref="Interfaces.IMultiMapAsync{TKey, TValue}"/> call across both the
        /// read and write phases, so cancellation is observed at each await boundary. If the token
        /// is cancelled during the write phase, <see cref="OperationCanceledException"/> is thrown
        /// and already-removed pairs are <b>not</b> restored.
        /// </para>
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to modify.</param>
        /// <param name="other">The multimap that defines the pairs to keep.</param>
        /// <param name="cancellationToken">A token to cancel the operation. Checked at every awaited <see cref="Interfaces.IMultiMapAsync{TKey, TValue}"/> call across both read and write phases; throws <see cref="OperationCanceledException"/> without rolling back already-applied removals.</param>
        public static async Task IntersectAsync<TKey, TValue>(this IMultiMapAsync<TKey, TValue> target, IReadOnlyMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
            where TKey : notnull
            where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(target, nameof(target));
            ArgumentNullException.ThrowIfNull(other, nameof(other));

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
        /// <para>
        /// This method is not atomic. Each removal is individually atomic, but values added to
        /// <paramref name="target"/> between awaited iterations that also exist in <paramref name="other"/>
        /// may not be removed. No structural corruption or count drift will occur.
        /// </para>
        /// <para>
        /// <b>Cancellation:</b> The <paramref name="cancellationToken"/> is forwarded to every
        /// individual <see cref="Interfaces.IMultiMapAsync{TKey, TValue}"/> call
        /// (<c>GetKeysAsync</c>, <c>GetOrDefaultAsync</c>, <c>RemoveRangeAsync</c>), so
        /// cancellation is observed at each await boundary. If the token is cancelled
        /// mid-operation, <see cref="OperationCanceledException"/> is thrown and
        /// already-removed pairs are <b>not</b> restored. When
        /// <paramref name="target"/> and <paramref name="other"/> are the same instance,
        /// <c>ClearAsync</c> is called directly and the token is checked on that single call.
        /// </para>
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to remove pairs from.</param>
        /// <param name="other">The multimap whose pairs are removed from <paramref name="target"/>.</param>
        /// <param name="cancellationToken">A token to cancel the operation. Checked at every awaited <see cref="Interfaces.IMultiMapAsync{TKey, TValue}"/> call; throws <see cref="OperationCanceledException"/> without rolling back already-applied removals.</param>
        public static async Task ExceptWithAsync<TKey, TValue>(this IMultiMapAsync<TKey, TValue> target, IReadOnlyMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
            where TKey : notnull
            where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(target, nameof(target));
            ArgumentNullException.ThrowIfNull(other, nameof(other));

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
        /// <para>
        /// This method is not atomic. It classifies pairs in a read phase, then applies additions
        /// and removals in separate write phases. Between awaited operations, concurrent callers may
        /// cause pairs to be misclassified, leaving values that should have been removed or failing
        /// to add values that should have been included. No structural corruption or count drift
        /// will occur.
        /// </para>
        /// <para>
        /// <b>Cancellation:</b> The <paramref name="cancellationToken"/> is forwarded to every
        /// individual <see cref="Interfaces.IMultiMapAsync{TKey, TValue}"/> call across the read,
        /// remove, and add phases, so cancellation is observed at each await boundary. If the
        /// token is cancelled during the write phase, <see cref="OperationCanceledException"/> is
        /// thrown; removals already applied in <c>RemoveRangeAsync</c> are <b>not</b> reversed,
        /// and the subsequent <c>AddRangeAsync</c> call may not have started.
        /// </para>
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to modify.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <param name="cancellationToken">A token to cancel the operation. Checked at every awaited <see cref="Interfaces.IMultiMapAsync{TKey, TValue}"/> call across all three phases; throws <see cref="OperationCanceledException"/> without rolling back already-applied changes.</param>
        public static async Task SymmetricExceptWithAsync<TKey, TValue>(this IMultiMapAsync<TKey, TValue> target, IReadOnlyMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
            where TKey : notnull
            where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(target, nameof(target));
            ArgumentNullException.ThrowIfNull(other, nameof(other));

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
    }
}
