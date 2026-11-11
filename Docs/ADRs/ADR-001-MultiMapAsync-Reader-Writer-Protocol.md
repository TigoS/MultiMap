# ADR-001: Custom Reader-Writer Protocol for MultiMapAsync

## Status

Accepted (v2.0.0)

## Context

`MultiMapAsync<TKey, TValue>` requires async-friendly synchronization for concurrent reads and writes.

`ReaderWriterLockSlim` is not awaitable and would force blocking calls or `Task.Run` wrappers, which increases overhead and weakens async flow.

## Decision

Use a custom semaphore-based readers-writer protocol built from:

- `_writeLock` (`SemaphoreSlim(1,1)`) for exclusive writes
- `_readerLock` (`SemaphoreSlim(1,1)`) for reader-count updates
- `_activeReaders` counter

Protocol summary:

- First reader acquires `_writeLock`
- Additional readers increment `_activeReaders`
- Last reader releases `_writeLock`
- Writers acquire `_writeLock` exclusively

## Rationale

- Native `async`/`await` support across all operations
- Avoids blocking thread-pool threads for read/write coordination
- Preserves concurrent-reader, exclusive-writer behavior
- Matches current implementation architecture and performance expectations

## Consequences

Positive:

- Async-safe synchronization without external adapters
- Clear separation of read and write critical sections

Negative:

- More implementation complexity than `ReaderWriterLockSlim`
- Writer starvation remains possible under sustained reader pressure

## Alternatives Considered

1. `ReaderWriterLockSlim` + `Task.Run` wrappers
   - Rejected: blocking + additional scheduler overhead

2. Single async mutex (`SemaphoreSlim`) without reader grouping
   - Rejected: poor read concurrency

3. Third-party async reader-writer lock package
   - Rejected: additional dependency and maintenance overhead
