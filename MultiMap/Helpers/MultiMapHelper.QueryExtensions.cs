using MultiMap.Interfaces;

namespace MultiMap.Helpers
{
    public static partial class MultiMapHelper
    {
        // ── IMultiMap<TKey, TValue> query operations ────────────────────────────

        /// <summary>
        /// Determines whether <paramref name="target"/> is a subset of <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// This method is not atomic. It reads <paramref name="target"/> keys and values in a read phase,
        /// then queries <paramref name="other"/> for membership. Concurrent modifications to either multimap
        /// may cause the result to reflect a mix of states. No structural corruption or count drift will occur.
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <returns><see langword="true"/> if every key-value pair in <paramref name="target"/> exists in <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public static bool IsSubsetOf<TKey, TValue>(this IMultiMap<TKey, TValue> target, IReadOnlyMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            var otherLookup = new Dictionary<TKey, HashSet<TValue>>(other.KeyCount);

            foreach (var kvp in target)
            {
                if (!otherLookup.TryGetValue(kvp.Key, out var otherSet))
                {
                    otherSet = [.. other.GetOrDefault(kvp.Key)];
                    otherLookup[kvp.Key] = otherSet;
                }

                if (!otherSet.Contains(kvp.Value))
                {
                    return false;
                }
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
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <returns><see langword="true"/> if every key-value pair in <paramref name="other"/> exists in <paramref name="target"/>; otherwise, <see langword="false"/>.</returns>
        public static bool IsSupersetOf<TKey, TValue>(this IMultiMap<TKey, TValue> target, IMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
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
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <returns><see langword="true"/> if at least one key-value pair exists in both multimaps; otherwise, <see langword="false"/>.</returns>
        public static bool Overlaps<TKey, TValue>(this IMultiMap<TKey, TValue> target, IReadOnlyMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            var otherLookup = new Dictionary<TKey, HashSet<TValue>>(other.KeyCount);

            foreach (var kvp in target)
            {
                if (!otherLookup.TryGetValue(kvp.Key, out var otherSet))
                {
                    otherSet = [.. other.GetOrDefault(kvp.Key)];
                    otherLookup[kvp.Key] = otherSet;
                }

                if (otherSet.Contains(kvp.Value))
                {
                    return true;
                }
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
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <returns><see langword="true"/> if both multimaps contain exactly the same key-value pairs; otherwise, <see langword="false"/>.</returns>
        public static bool SetEquals<TKey, TValue>(this IMultiMap<TKey, TValue> target, IReadOnlyMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            if (target.Count != other.Count || target.KeyCount != other.KeyCount)
            {
                return false;
            }

            var otherLookup = new Dictionary<TKey, HashSet<TValue>>(other.KeyCount);

            foreach (var kvp in target)
            {
                if (!otherLookup.TryGetValue(kvp.Key, out var otherSet))
                {
                    otherSet = [.. other.GetOrDefault(kvp.Key)];
                    otherLookup[kvp.Key] = otherSet;
                }

                if (!otherSet.Contains(kvp.Value))
                {
                    return false;
                }
            }

            return true;
        }

        // ── ISimpleMultiMap<TKey, TValue> query operations ──────────────────────

        /// <summary>
        /// Determines whether <paramref name="target"/> is a subset of <paramref name="other"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <returns><see langword="true"/> if every key-value pair in <paramref name="target"/> exists in <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public static bool IsSubsetOf<TKey, TValue>(this ISimpleMultiMap<TKey, TValue> target, IReadOnlySimpleMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            // Use default ctor because ISimpleMultiMap doesn't define KeyCount
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
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether <paramref name="target"/> is a superset of <paramref name="other"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <returns><see langword="true"/> if every key-value pair in <paramref name="other"/> exists in <paramref name="target"/>; otherwise, <see langword="false"/>.</returns>
        public static bool IsSupersetOf<TKey, TValue>(this ISimpleMultiMap<TKey, TValue> target, ISimpleMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            return other.IsSubsetOf(target);
        }

        /// <summary>
        /// Determines whether <paramref name="target"/> and <paramref name="other"/> share at least one key-value pair.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <returns><see langword="true"/> if at least one key-value pair exists in both multimaps; otherwise, <see langword="false"/>.</returns>
        public static bool Overlaps<TKey, TValue>(this ISimpleMultiMap<TKey, TValue> target, IReadOnlySimpleMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            foreach (var kvp in target)
            {
                if (other.Contains(kvp.Key, kvp.Value))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether <paramref name="target"/> and <paramref name="other"/> contain the same key-value pairs.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <returns><see langword="true"/> if both multimaps contain exactly the same key-value pairs; otherwise, <see langword="false"/>.</returns>
        public static bool SetEquals<TKey, TValue>(this ISimpleMultiMap<TKey, TValue> target, IReadOnlySimpleMultiMap<TKey, TValue> other)
            where TKey : notnull
            where TValue : notnull
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            if (target.Count != other.Count)
            {
                return false;
            }

            foreach (var kvp in target)
            {
                if (!new HashSet<TValue>(other.GetOrDefault(kvp.Key)).Contains(kvp.Value))
                {
                    return false;
                }
            }

            return true;
        }

        // ── IMultiMapAsync<TKey, TValue> query operations ───────────────────────

        /// <summary>
        /// Asynchronously determines whether <paramref name="target"/> is a subset of <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is not atomic. Each awaited operation releases the underlying lock, allowing
        /// concurrent callers to interleave between steps. The result may reflect a mix of states.
        /// No structural corruption or count drift will occur.
        /// </para>
        /// <para>
        /// <b>Cancellation:</b> The <paramref name="cancellationToken"/> is forwarded to every
        /// individual <see cref="Interfaces.IMultiMapAsync{TKey, TValue}"/> call
        /// (<c>GetKeysAsync</c>, <c>GetOrDefaultAsync</c>), so cancellation is observed at each
        /// await boundary. If the token is cancelled mid-check, <see cref="OperationCanceledException"/>
        /// is thrown. Because this is a read-only operation, no mutations need to be rolled back.
        /// </para>
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <param name="cancellationToken">A token to cancel the operation. Checked at every awaited <see cref="Interfaces.IMultiMapAsync{TKey, TValue}"/> call; throws <see cref="OperationCanceledException"/>. No rollback needed (read-only).</param>
        /// <returns><see langword="true"/> if every key-value pair in <paramref name="target"/> exists in <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public static async Task<bool> IsSubsetOfAsync<TKey, TValue>(this IMultiMapAsync<TKey, TValue> target, IReadOnlyMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
            where TKey : notnull
            where TValue : notnull
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
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Asynchronously determines whether <paramref name="target"/> is a superset of <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is not atomic. Each awaited operation releases the underlying lock, allowing
        /// concurrent callers to interleave between steps. The result may reflect a mix of states.
        /// No structural corruption or count drift will occur.
        /// </para>
        /// <para>
        /// <b>Cancellation:</b> The <paramref name="cancellationToken"/> is forwarded to every
        /// individual <see cref="Interfaces.IMultiMapAsync{TKey, TValue}"/> call
        /// (<c>GetKeysAsync</c>, <c>GetOrDefaultAsync</c>), so cancellation is observed at each
        /// await boundary. If the token is cancelled mid-check, <see cref="OperationCanceledException"/>
        /// is thrown. Because this is a read-only operation, no mutations need to be rolled back.
        /// </para>
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <param name="cancellationToken">A token to cancel the operation. Checked at every awaited <see cref="Interfaces.IMultiMapAsync{TKey, TValue}"/> call; throws <see cref="OperationCanceledException"/>. No rollback needed (read-only).</param>
        /// <returns><see langword="true"/> if every key-value pair in <paramref name="other"/> exists in <paramref name="target"/>; otherwise, <see langword="false"/>.</returns>
        public static async Task<bool> IsSupersetOfAsync<TKey, TValue>(this IMultiMapAsync<TKey, TValue> target, IMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
            where TKey : notnull
            where TValue : notnull
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            return await other.IsSubsetOfAsync(target, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously determines whether <paramref name="target"/> and <paramref name="other"/> share at least one key-value pair.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is not atomic. Each awaited operation releases the underlying lock, allowing
        /// concurrent callers to interleave between steps. The result may reflect a mix of states.
        /// No structural corruption or count drift will occur.
        /// </para>
        /// <para>
        /// <b>Cancellation:</b> The <paramref name="cancellationToken"/> is forwarded to every
        /// individual <see cref="Interfaces.IMultiMapAsync{TKey, TValue}"/> call
        /// (<c>GetKeysAsync</c>, <c>GetOrDefaultAsync</c>), so cancellation is observed at each
        /// await boundary. If the token is cancelled mid-check, <see cref="OperationCanceledException"/>
        /// is thrown. Because this is a read-only operation, no mutations need to be rolled back.
        /// </para>
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <param name="cancellationToken">A token to cancel the operation. Checked at every awaited <see cref="Interfaces.IMultiMapAsync{TKey, TValue}"/> call; throws <see cref="OperationCanceledException"/>. No rollback needed (read-only).</param>
        /// <returns><see langword="true"/> if at least one key-value pair exists in both multimaps; otherwise, <see langword="false"/>.</returns>
        public static async Task<bool> OverlapsAsync<TKey, TValue>(this IMultiMapAsync<TKey, TValue> target, IReadOnlyMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
            where TKey : notnull
            where TValue : notnull
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
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Asynchronously determines whether <paramref name="target"/> and <paramref name="other"/> contain the same key-value pairs.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is not atomic. Each awaited operation releases the underlying lock, allowing
        /// concurrent callers to interleave between steps. The result may reflect a mix of states.
        /// No structural corruption or count drift will occur.
        /// </para>
        /// <para>
        /// <b>Cancellation:</b> The <paramref name="cancellationToken"/> is forwarded to every
        /// individual <see cref="Interfaces.IMultiMapAsync{TKey, TValue}"/> call, so cancellation
        /// is observed at each await boundary. If the token is cancelled mid-check,
        /// <see cref="OperationCanceledException"/> is thrown. Because this is a read-only operation,
        /// no mutations need to be rolled back.
        /// </para>
        /// </remarks>
        /// <typeparam name="TKey">The type of keys in the multimap.</typeparam>
        /// <typeparam name="TValue">The type of values in the multimap.</typeparam>
        /// <param name="target">The multimap to check.</param>
        /// <param name="other">The multimap to compare against.</param>
        /// <param name="cancellationToken">A token to cancel the operation. Checked at every awaited <see cref="Interfaces.IMultiMapAsync{TKey, TValue}"/> call; throws <see cref="OperationCanceledException"/>. No rollback needed (read-only).</param>
        /// <returns><see langword="true"/> if both multimaps contain exactly the same key-value pairs; otherwise, <see langword="false"/>.</returns>
        public static async Task<bool> SetEqualsAsync<TKey, TValue>(this IMultiMapAsync<TKey, TValue> target, IReadOnlyMultiMapAsync<TKey, TValue> other, CancellationToken cancellationToken = default)
            where TKey : notnull
            where TValue : notnull
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(other, nameof(other));

            var targetCount = await target.GetCountAsync(cancellationToken).ConfigureAwait(false);
            var otherCount = await other.GetCountAsync(cancellationToken).ConfigureAwait(false);
            var targetKeyCount = await target.GetKeyCountAsync(cancellationToken).ConfigureAwait(false);
            var otherKeyCount = await other.GetKeyCountAsync(cancellationToken).ConfigureAwait(false);

            if (targetCount != otherCount || targetKeyCount != otherKeyCount)
            {
                return false;
            }

            var targetKeys = await target.GetKeysAsync(cancellationToken).ConfigureAwait(false);
            foreach (var key in targetKeys)
            {
                var targetValues = await target.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false);
                var otherValues = await other.GetOrDefaultAsync(key, cancellationToken).ConfigureAwait(false);

                var otherSet = new HashSet<TValue>(otherValues);

                foreach (var value in targetValues)
                {
                    if (!otherSet.Contains(value))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
