namespace MultiMap.Helpers
{
    /// <summary>
    /// Coordinates synchronous and asynchronous reader-writer locking with concurrent readers and exclusive writers.
    /// </summary>
    /// <remarks>Allows multiple readers at the same time while enforcing single-writer exclusivity. The first
    /// reader acquires the writer gate and the last reader releases it. Disposes the underlying semaphores only when it
    /// owns them.</remarks>
    internal sealed class AsyncReaderWriterLock : IDisposable
    {
        private readonly SemaphoreSlim _writeLock;
        private readonly SemaphoreSlim _readerLock;
        private readonly bool _ownsSemaphores;
        private int _activeReaders;

        /// <summary>
        /// Initializes a new instance of the AsyncReaderWriterLock class using internally created semaphores.
        /// </summary>
        /// <remarks>Creates two SemaphoreSlim instances with an initial and maximum count of 1 and
        /// configures the lock to own and dispose them.</remarks>
        public AsyncReaderWriterLock()
            : this(new SemaphoreSlim(1, 1), new SemaphoreSlim(1, 1), ownsSemaphores: true)
        {
        }

        /// <summary>
        /// Initializes a new instance using externally managed semaphores for writer and reader coordination.
        /// </summary>
        /// <remarks>The provided semaphores are not owned by this instance and are not disposed by
        /// it.</remarks>
        /// <param name="writeLock">Semaphore that controls exclusive writer access.</param>
        /// <param name="readerLock">Semaphore that coordinates reader admission.</param>
        public AsyncReaderWriterLock(SemaphoreSlim writeLock, SemaphoreSlim readerLock)
            : this(writeLock, readerLock, ownsSemaphores: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the AsyncReaderWriterLock class with the specified synchronization semaphores
        /// and ownership behavior.
        /// </summary>
        /// <param name="writeLock">Semaphore that coordinates write access.</param>
        /// <param name="readerLock">Semaphore that coordinates reader admission.</param>
        /// <param name="ownsSemaphores">Indicates whether the instance is responsible for disposing the provided semaphores.</param>
        private AsyncReaderWriterLock(SemaphoreSlim writeLock, SemaphoreSlim readerLock, bool ownsSemaphores)
        {
            _writeLock = writeLock;
            _readerLock = readerLock;
            _ownsSemaphores = ownsSemaphores;
        }

        /// <summary>
        /// Asynchronously waits to acquire the write lock.
        /// </summary>
        /// <remarks>The returned task is canceled if cancellation is requested before the write lock is
        /// acquired.</remarks>
        /// <param name="cancellationToken">A token to monitor for cancellation while waiting to acquire the write lock.</param>
        /// <returns>A task that completes when the write lock has been acquired.</returns>
        public Task WaitWriteLockAsync(CancellationToken cancellationToken = default)
            => _writeLock.WaitAsync(cancellationToken);

        /// <summary>
        /// Releases the write lock.
        /// </summary>
        /// <remarks>Call after acquiring the write lock to allow other operations to proceed.</remarks>
        public void ExitWriteLock()
            => _writeLock.Release();

        /// <summary>
        /// Attempts to enter the lock in read mode without waiting.
        /// </summary>
        /// <remarks>When the first reader enters, write mode is also acquired to prevent writers from
        /// entering until all readers have exited.</remarks>
        /// <returns><see langword="true"/> if read mode was entered; otherwise, <see langword="false"/>.</returns>
        public bool TryEnterReadLockSync()
        {
            if (!_readerLock.Wait(0))
            {
                return false;
            }

            try
            {
                if (_activeReaders == 0)
                {
                    if (!_writeLock.Wait(0))
                    {
                        return false;
                    }
                }

                _activeReaders++;
                return true;
            }
            finally
            {
                _readerLock.Release();
            }
        }
        
        /// <summary>
        /// Synchronously waits to enter the lock in read mode.
        /// </summary>
        public void EnterReadLockSync()
        {
            _readerLock.Wait();
            try
            {
                if (_activeReaders == 0)
                {
                    _writeLock.Wait();
                }

                _activeReaders++;
            }
            finally
            {
                _readerLock.Release();
            }
        }

        /// <summary>
        /// Asynchronously enters the reader lock, allowing concurrent readers while preventing writers from entering.
        /// </summary>
        /// <remarks>The first active reader also acquires the writer gate so writers are blocked until
        /// all readers have exited.</remarks>
        /// <param name="cancellationToken">A token that can be used to cancel waiting to enter the lock.</param>
        /// <returns>A task that completes when the reader lock has been entered.</returns>
        public async Task EnterReadLockAsync(CancellationToken cancellationToken)
        {
            await _readerLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_activeReaders == 0)
                {
                    await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                }

                _activeReaders++;
            }
            finally
            {
                _readerLock.Release();
            }
        }

        /// <summary>
        /// Exits the read lock and releases the write lock when the last active reader exits.
        /// </summary>
        /// <remarks>Call after a successful read-lock acquisition to keep reader tracking and lock state
        /// consistent.</remarks>
        public void ExitReadLock()
        {
            _readerLock.Wait();
            try
            {
                if (--_activeReaders == 0)
                {
                    _writeLock.Release();
                }
            }
            finally
            {
                _readerLock.Release();
            }
        }

        /// <summary>
        /// Releases the reader and writer lock semaphores owned by the instance.
        /// </summary>
        /// <remarks>Returns without releasing resources when semaphore ownership is not held by the
        /// instance.</remarks>
        public void Dispose()
        {
            if (!_ownsSemaphores)
            {
                return;
            }

            _writeLock.Dispose();
            _readerLock.Dispose();
        }
    }
}
