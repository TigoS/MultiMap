using System.Diagnostics;
using MultiMap.Interfaces;

namespace MultiMap.Entities
{
    /// <summary>
    /// Provides a thread-safe multi-map collection that associates each key with a set of values, allowing concurrent access and modification.
    /// </summary>
    /// <remarks>
    /// This class uses internal locking to ensure safe concurrent operations.
    /// It is suitable for scenarios where multiple threads need to add, remove, or query key-value associations without external synchronization.
    /// Dispose the instance when no longer needed to release resources.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multi-map.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key.</typeparam>
    /// <remarks>
    /// Initializes a new instance of the <see cref="MultiMapLock{TKey, TValue}"/> class with the specified initial capacity for keys and equality comparer for keys and values.
    /// </remarks>
    /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
    /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
    /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
    [DebuggerDisplay("Keys={KeyCount}, Values={Count}")]
    public sealed partial class MultiMapLock<TKey, TValue>(int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer) : IMultiMap<TKey, TValue>, IDisposable
        where TKey : notnull
        where TValue : notnull
    {
        private readonly Dictionary<TKey, HashSet<TValue>> _dictionary = capacity > 0
                ? new Dictionary<TKey, HashSet<TValue>>(capacity, keyComparer)
                : new Dictionary<TKey, HashSet<TValue>>(keyComparer);
        private readonly ReaderWriterLockSlim _lock = new();
        private readonly IEqualityComparer<TValue>? _valueComparer = valueComparer;
        private int _count;
        private int _disposed;

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, GetType()?.FullName ?? string.Empty);
        }

        /// <summary>
        /// Polling interval (ms) used by <see cref="EnterWriteLockCancellable"/> between
        /// <see cref="ReaderWriterLockSlim.TryEnterWriteLock(int)"/> attempts.
        /// 20 ms gives sub-20 ms cancellation latency with negligible CPU cost.
        /// </summary>
        private const int WriteLockPollIntervalMs = 20;

        /// <summary>
        /// Acquires the write lock, aborting if <paramref name="cancellationToken"/> is cancelled.
        /// </summary>
        /// <remarks>
        /// <see cref="ReaderWriterLockSlim"/> does not expose a cancellable entry point, so this
        /// helper polls with <see cref="ReaderWriterLockSlim.TryEnterWriteLock(int)"/> at
        /// <see cref="WriteLockPollIntervalMs"/> intervals. Cancellation is observed within at most
        /// one polling interval (~20 ms). On cancellation, <see cref="OperationCanceledException"/>
        /// is thrown and the lock is <b>not</b> held.
        /// </remarks>
        /// <param name="cancellationToken">Token to observe for cancellation.</param>
        /// <exception cref="OperationCanceledException">
        /// Thrown when <paramref name="cancellationToken"/> is cancelled before the write lock is acquired.
        /// </exception>
        private void EnterWriteLockCancellable(CancellationToken cancellationToken)
        {
            while (!_lock.TryEnterWriteLock(WriteLockPollIntervalMs))
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
