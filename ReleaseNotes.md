# MultiMap

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0%20%7C%208.0%20%7C%20Standard%202.0-blue.svg)](https://dotnet.microsoft.com/)
[![C# 14](https://img.shields.io/badge/C%23-14.0-blue)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![NUnit](https://img.shields.io/badge/tests-NUnit%204-green)](https://nunit.org/)
[![BenchmarkDotNet](https://img.shields.io/badge/BenchmarkDotNet-v0.15.8-blue)](https://benchmarkdotnet.org/)
[![Test SDK](https://img.shields.io/badge/Microsoft.NET.Test.Sdk-v18.6.0-blue)](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk)
[![NuGet](https://img.shields.io/nuget/v/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![Coverage](https://img.shields.io/badge/coverage-95.4%25-brightgreen)]()

A **.NET** library targeting **.NET 10**, **.NET 8**, and **.NET Standard 2.0**

## Release Notes

### 2.1.0

**Added**

- **Read-only set query operations**: Added four new set algebra query methods to `MultiMapHelper` (extension methods), `MultiMapAsync` (atomic instance methods), and `MultiMapLock` (atomic instance methods):
  - `IsSubsetOf` / `IsSubsetOfAsync` — Check if the current multimap is a subset of another (all key-value pairs exist in the other)
  - `IsSupersetOf` / `IsSupersetOfAsync` — Check if the current multimap is a superset of another (contains all pairs from the other)
  - `Overlaps` / `OverlapsAsync` — Check if the current multimap shares any key-value pairs with another
  - `SetEquals` / `SetEqualsAsync` — Check if the current multimap contains exactly the same key-value pairs as another
- Comprehensive unit test coverage for all new set query operations (see **Tests** section below for per-class breakdown)
- Benchmark coverage for all new set query operations across `MultiMapBenchmarks`, `MultiMapAsyncBenchmarks`, and `MultiMapLockBenchmarks` (20 new benchmarks total)
- `SimpleMultiMap.Clear` benchmarks: **166,832 ns** for 5,000-pair map; **4.025 ns** for empty map

- **2,094 tests** per target framework — **4,188 total executions** on `net10.0` + `net8.0`.
- Coverlet: **95.4% line coverage**, **91.6% branch coverage**.
- Added **67 new sync set-query tests** in `MultiMapHelper_UnitTests.cs` for `IsSubsetOf`, `IsSupersetOf`, `Overlaps`, `SetEquals` (sync `IMultiMap`, `ISimpleMultiMap`, and `IMultiMap` overloads, including null guards and edge cases)
- Added **17 new async set-query tests** in `MultiMapHelperExtensionAsync_UnitTests.cs` for `IsSubsetOfAsync`, `IsSupersetOfAsync`, `OverlapsAsync`, `SetEqualsAsync`
- Added **46 new tests** in `MultiMapAsync_UnitTests.cs` for atomic `IsSubsetOfAsync`, `IsSupersetOfAsync`, `OverlapsAsync`, `SetEqualsAsync` (including concurrency and cancellation tests)
- Added **52 new tests** in `MultiMapLock_UnitTests.cs` for atomic `IsSubsetOf`, `IsSupersetOf`, `Overlaps`, `SetEquals` (including concurrency and lock-ordering tests)
- Added **43 new boundary condition and exception handling tests** in `MultiMapBoundaryConditions_UnitTests.cs` covering: empty collections, single-item operations, AddRange boundaries, MultiMapList duplicates, Clear operations, enumeration edges, capacity/resize scenarios, exception boundaries (null keys/values), ContainsKey/Contains edges, and count boundaries

**Implementation Details**

- `MultiMapHelper` extensions provide read-only query semantics for `IMultiMap<TKey, TValue>`, `ISimpleMultiMap<TKey, TValue>`, and `IMultiMapAsync<TKey, TValue>` interfaces
- `MultiMapAsync` instance methods implement atomic snapshot-based comparisons with ordered semaphore acquisition to prevent deadlocks when comparing two `MultiMapAsync` instances
- `MultiMapLock` instance methods implement atomic read-lock-based comparisons, snapshotting the other map before acquiring locks to avoid lock-ordering issues
- All query methods use `HashSet<TValue>` for efficient O(1) value lookups and short-circuit on first definitive result for optimal performance

### 2.0.1

**Breaking Changes**

- `ISimpleMultiMap.Clear(TKey key)` removed — deprecated in v1.0.12 as an `[Obsolete]` alias for `RemoveKey(TKey key)`. Update all remaining call sites to `map.RemoveKey(key)`.
- `ISimpleMultiMap.Flatten()` removed — deprecated in v1.0.12. Replace all remaining usages with direct enumeration: `foreach (var kvp in map)`, `map.ToList()`, or any LINQ expression.
- `AddRange(TKey key, IEnumerable<TValue> values)` on a new key with an empty enumerable now returns `0` instead of `void`. Callers must check the return value of `AddRange` to confirm whether any pairs were actually added.
- `AddRangeAsync(TKey key, IEnumerable<TValue> values)` on a new key with an empty enumerable now returns `0` instead of `Task`. Callers must `await` the result and check the returned count.

**Bug Fixes**

- `ConcurrentMultiMap.Remove` — eliminated a TOCTOU race condition using the `TryPruneEmptySet` helper.
- `ConcurrentMultiMap.RemoveWhere` — same TOCTOU race as `Remove`; fixed with the same `TryPruneEmptySet` helper.
- `ConcurrentMultiMap.GetOrDefault(TKey)` — now returns an empty sequence instead of a snapshot of a live-but-empty inner `ConcurrentDictionary` after concurrent removals. Behaviour is now consistent with `Get` and `TryGet`.
- `ConcurrentMultiMap.AddRange(TKey, IEnumerable<TValue>)` — short-circuits when the materialised collection is empty, preventing a transient orphan key registration via a no-op `GetOrAdd`.
- `MultiMapBase.AddRange(TKey, IEnumerable<TValue>)` — eliminated an unnecessary `CreateCollection()` allocation when the value sequence is empty. The method materialises the sequence first and short-circuits before allocating a collection; an empty sequence returns `0` and does not create an orphan key entry.
- `SimpleMultiMap.Get` and `SimpleMultiMap.GetOrDefault` — no longer call `.ToArray()` on the backing `HashSet<TValue>` on every read.

**Performance**

- `ConcurrentMultiMap.Count` and `KeyCount` are now **O(1)** — backed by dedicated `_count` and `_keyCount` fields updated via `Interlocked` in every mutating path (`Add`, `AddRange`, `Remove`, `RemoveWhere`, `RemoveKey`, `Clear`). BenchmarkDotNet reports both as ZeroMeasurement (< 1 ns) on .NET 10.0.8.
- `ConcurrentMultiMap.Keys` and `Values` are now **lazy iterators** (`yield return` over the live dictionary) instead of fully materialised `List<T>` snapshots. Call `.ToList()` / `.ToArray()` explicitly when a stable snapshot is needed.
- `MultiMapSet.AddRange(IEnumerable<KeyValuePair<TKey, TValue>>)` — on .NET 6+, now uses `CollectionsMarshal.GetValueRefOrAddDefault` for a single dictionary lookup per item, matching the fast path in `MultiMapSet.Add` and `MultiMapSet.AddRange(TKey, IEnumerable<TValue>)`. The `.NET Standard 2.0` path is unchanged.

**Added**

- Additional stress/concurrency coverage in `MultiMapAsyncTests`, `ConcurrentMultiMapTests`, and `MultiMapLockTests`, including mixed-operation invariants and overlapping concurrent `AddRange` scenarios.
- New benchmark coverage for previously unmeasured operations: `SimpleMultiMap_Contains`, `SimpleMultiMap_ContainsKey`, `MultiMapLock_TryGet`, and direct map enumeration (`MultiMapSet_Enumerate`, `MultiMapList_Enumerate`, `ConcurrentMultiMap_Enumerate`, `SortedMultiMap_Enumerate`).

**Changed**

- `SortedMultiMap<TKey, TValue>` type constraints documented explicitly: `IEquatable<TKey>` is a library-wide requirement propagated from `MultiMapBase`; `IComparable<TKey>` is the additional constraint enabling sorted key ordering.
- `ComputeUnorderedHash` now accepts optional `IEqualityComparer<TKey>?` and `IEqualityComparer<TValue>?` parameters, routing `GetHashCode` through the caller-supplied comparer. All concrete implementations forward their stored comparers to this helper.
- `Intersect` and `SymmetricExceptWith` (sync and async) build a per-key `HashSet<TValue>` lookup to avoid O(n²) inner-loop value scans; no allocation occurs when the source already implements `ISet<TValue>`.
- Benchmark suite rerun on .NET 10.0.8 / SDK 10.0.300; README benchmark tables updated to latest measured values.

**Documentation**

- `MultiMapAsync<TKey, TValue>` — added detailed XML doc comment explaining the custom readers-writer locking protocol: `_writeLock` (exclusive per write; held by the first reader for the duration of any concurrent read group) and `_readerLock` (guards the `_activeReaders` counter only). README updated with a matching **Locking protocol** callout.
- `ConcurrentMultiMap<TKey, TValue>` XML doc updated to note that the class does not implement `IDisposable` — intentional, as it owns no disposable resources.
- `MultiMapAsync<TKey, TValue>` XML doc corrected: removed a duplicate `<remarks>` block that was attached to the primary constructor declaration.
- README synchronised with current codebase: interface hierarchy, constructor matrices, `MultiMapHelper` signatures, testing/coverage metrics, and per-class coverage breakdown.

**Tests**

- **1,721 tests** per target framework — **3,442 total executions** on `net10.0` + `net8.0`.
- Coverlet: **98.6% line coverage** (2,400/2,435), **95.4% branch coverage** (1,013/1,062), **96.6% method coverage** (255/264).
- Added **3 new stress tests** in `ConcurrentMultiMapTests`:
  - `Remove_ConcurrentWithAdd_SameKey_NeverLosesValue`
  - `Remove_ConcurrentWithAdd_SameKey_DoesNotDeleteRepopulatedKey`
  - `RemoveWhere_ConcurrentWithAdd_SameKey_DoesNotDeleteRepopulatedKey`
- Added **2 new unit tests** in `MultiMapBaseTests` (shared across `MultiMapSet`, `MultiMapList`, `SortedMultiMap`):
  - `AddRange_NewKey_EmptyLazySequence_EnumeratedExactlyOnce`
  - `AddRange_ExistingKey_EmptySequence_DoesNotAlterExistingValues`
- Added **28 new unit tests** (×2 TFMs = **56 executions**) covering `AddRange`-empty-enumerable edge cases across all map types:
  - `AddRange_NewKey_EmptyCollection_DoesNotCreateOrphanEntry`
  - `AddRange_NewKey_EmptyThenNonEmpty_CreatesKeyOnSecondCall`
  - `AddRange_NewKey_EmptyCollection_DoesNotAppearInKeys`
  - `AddRange_NewKey_EmptyCollection_ReturnsZero`
  - `AddRange_EmptyCollection_ConcurrentFromManyThreads_NeverLeavesOrphanKey` (`ConcurrentMultiMap`, stress)
  - `AddRange_EmptyCollection_ConcurrentWithRemoveKey_NoOrphanAndNoException` (`ConcurrentMultiMap`, stress)
  - `AddRange_EmptyCollection_ConcurrentWithRealAdds_NeverLeavesOrphanKey` (`MultiMapLock`, stress)
  - `AddRangeAsync_EmptyCollection_ConcurrentFromManyTasks_NeverLeavesOrphanKey` (`MultiMapAsync`, stress)

### 1.0.12

**Added**

- `MultiMapBase.ValuesCollection.cs` and `MultiMapBase.ValuesEnumerator.cs` — partial class files that host the nested `ValuesCollection` and `ValuesEnumerator` struct, extracted from `MultiMapBase.cs` for better code organization
- `MultiMapAsync.Core.cs`
- `MultiMapSet(IEqualityComparer<TKey>?, IEqualityComparer<TValue>?)` constructor overload — fills the gap between the separate key-only and value-only overloads, completing `MultiMapSet` to a full 8-overload family
- `ConcurrentMultiMap(IEqualityComparer<TKey>?)` and `ConcurrentMultiMap(IEqualityComparer<TKey>?, IEqualityComparer<TValue>?)` constructor overloads — bring `ConcurrentMultiMap` to a full 8-overload family on par with the other implementations
- `SimpleMultiMap.Count` property — returns the total number of key–value pairs across all keys (sum of all per-key value set sizes)
- `SimpleMultiMap.Equals(IReadOnlySimpleMultiMap<TKey, TValue>? other)` — typed equality against the read-only interface; compares total pair count, then per-key value-set contents
- Additional benchmarks covering `Values`/`GetValuesAsync`, `KeyCount`/`GetKeyCountAsync`, `GetValuesCount`/`GetValuesCountAsync`, set operations, `SimpleMultiMap` operations, and the new `SimpleMultiMap.Count` / `SimpleMultiMap.Equals` members in `BenchmarkSuite`
- 75 `SimpleMultiMapTests` — full coverage of all `SimpleMultiMap` public members, including `Count`, typed equality, null-key guards, and `GetOrDefault` edge cases
- 217 new unit tests across all test files to close branch/line coverage gaps: null-guard paths for `MultiMapLock`, `MultiMapBase` (shared across all 3 subclass fixtures), `MultiMapHelper` (sync and async extension methods), generic-interface equality path for `MultiMapAsync`, and `ValuesEnumerator.Reset`/`Dispose` branches

**Changed**

- `ISimpleMultiMap.Flatten()` marked `[Obsolete]` — the method is redundant because `ISimpleMultiMap<TKey, TValue>` already implements `IEnumerable<KeyValuePair<TKey, TValue>>`; enumerating the map directly with `foreach`, `ToList()`, or LINQ produces the same sequence. Existing call sites continue to compile and run (soft deprecation, compiler warning `CS0618`). The method will be removed in a future version. Internal usages in `MultiMapHelper` and `MultiMap.Demo` have been updated to enumerate directly
- `ISimpleMultiMap.Clear(TKey)` renamed to `ISimpleMultiMap.RemoveKey(TKey)` — aligns naming with `IMultiMap.RemoveKey(TKey)` for API consistency. For backward compatibility, `Clear(TKey)` is retained as an `[Obsolete]` alias that forwards to `RemoveKey(TKey)` (soft deprecation, compiler warning `CS0618`). Migrate call sites to `RemoveKey(key)` before the next major version
- `ISimpleMultiMap.Remove(TKey, TValue)` now returns `bool` (previously `void`) — consistent with `IMultiMap.Remove(TKey, TValue)`.
- `ConcurrentMultiMap` is now fully lock-free: internal storage changed from `ConcurrentDictionary<TKey, HashSet<TValue>>` with per-key `lock`/`ReaderWriterLockSlim` and an `Interlocked` counter to `ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>`. All per-key operations are lock-free; `Count` is O(n) (sum of inner dictionary sizes)
- `MultiMapAsync.Equals(IReadOnlyMultiMapAsync<TKey, TValue>? other)` reworked with a deadlock-safe dual-semaphore strategy: when comparing two `MultiMapAsync` instances, both semaphores are acquired in a stable `RuntimeHelpers.GetHashCode`-ordered sequence; `Equals(object?)` additionally throws `InvalidOperationException` when called under a `SynchronizationContext` — callers in `async` contexts must use `EqualsAsync` instead
- `MultiMapList.AddRange(IEnumerable<KeyValuePair<TKey, TValue>>)` overrides the base-class implementation on .NET 6+ and uses `CollectionsMarshal.GetValueRefOrAddDefault`, eliminating per-item virtual dispatch and matching the existing `Add` / `AddRange(key, values)` fast paths
- `MultiMapBase.Values`, `MultiMapLock.Values`, and `MultiMapAsync.GetValuesAsync()` now use a custom zero-allocation struct enumerator instead of `SelectMany` LINQ iterators, eliminating per-access heap allocations on read-heavy paths
- All 7 concrete implementations (`MultiMapList`, `MultiMapSet`, `SortedMultiMap`, `ConcurrentMultiMap`, `MultiMapLock`, `MultiMapAsync`, `SimpleMultiMap`) are now declared `sealed`, enabling JIT devirtualization on hot paths such as `Add` and `Remove`
- `SymmetricExceptWith` for `IMultiMap` now caches per-key lookups in a local dictionary (matching the existing `ISimpleMultiMap` strategy), eliminating redundant dictionary lookups and, for locked implementations, redundant lock acquisitions when multiple values share the same key
- Test count increased from **1,354 tests** to **1,683 tests** × 2 target frameworks = **3,366 total test executions**
- Code coverage updated: **99.51% line coverage** (2,250/2,261 lines), **96.38% branch coverage** (906/940 branches)
- README updated with corrected API descriptions (`ConcurrentMultiMap` lock-free design and full constructor set, `MultiMapSet` full constructor set, `ISimpleMultiMap.Remove` return type, `ISimpleMultiMap.RemoveKey` rename with backward-compatible `Clear(key)` alias, `ISimpleMultiMap.Flatten` deprecation, `SimpleMultiMap.Count` and `SimpleMultiMap.Equals`, project structure including partial files, package installation version)

**Fixed**

- `SimpleMultiMap.Equals(IReadOnlySimpleMultiMap<TKey, TValue>? other)` — was incorrectly comparing `_dictionary.Count` (key count) against `other.Count` (total pair count); fixed to compare `Count` (total pairs) against `other.Count` for semantically correct equality
- `MultiMapList.Equals(object?)` used `SequenceEqual`, which is order-dependent; replaced with content-based set equality so two lists with the same values in a different insertion order compare equal
- Null-value guard added to `AddRange` on list-backed collections — prevents `null` values from silently entering a `List<TValue>` and violating the `TValue: notnull` constraint at runtime

### 1.0.11

**Added**

- `MultiMapBase<TKey, TValue, TCollection>` abstract base class — shared dictionary-backed implementation for `MultiMapSet`, `MultiMapList`, and `SortedMultiMap` with `Add`, `AddRange`, `Remove`, `RemoveKey`, `RemoveRange`, `RemoveWhere`, `Get`, `GetOrDefault`, `TryGet`, `ContainsKey`, `Contains`, `Count`, `KeyCount`, `Keys`, `Values`, `GetValuesCount`, indexer, `Clear`, `GetEnumerator`, `Equals`, and `GetHashCode`
- `IEqualityComparer<TKey>` (keyComparer) constructor overloads on `MultiMapSet` (3 new), `MultiMapList` (2 new), `SortedMultiMap` (1 new), `MultiMapLock` (3 new), and `MultiMapAsync` (3 new)
- `ArgumentNullException` guards on all public methods across 7 implementation files (76 total guards) — null keys, values, predicates, and enumerables are now rejected immediately
- `<exception cref="ArgumentNullException">` XML documentation tags on all 5 mutable interface files (27 tags total)
- `IEqualityComparer<TValue>` constructor overloads on `MultiMapSet`, `MultiMapLock`, `MultiMapAsync`, `ConcurrentMultiMap`, and `SimpleMultiMap` — enables custom value comparison (e.g., case-insensitive strings)
- Initial capacity constructor overloads on all 6 entity types (`MultiMapSet`, `MultiMapList`, `SortedMultiMap`, `ConcurrentMultiMap`, `MultiMapLock`, `MultiMapAsync`) for memory pre-allocation
- Combined capacity + comparer constructor overloads where applicable
- Dispose guards (`ObjectDisposedException`) on every public member of `MultiMapLock` and `MultiMapAsync` after disposal
- 198 `MultiMapBase` unit tests verifying the shared contract across all 3 subclass fixtures (`MultiMapSet`, `MultiMapList`, `SortedMultiMap`)
- 15 `EqualsAsync` unit tests for `MultiMapAsync`
- 16 coverage gap unit tests across 6 test files
- 35 new unit tests covering constructor overloads, comparer behavior, and case-insensitive scenarios
- 25 new dispose-guard unit tests (13 for `MultiMapLock`, 12 for `MultiMapAsync`)
- Multi-target support: the NuGet package now ships assemblies for `.NET 10`, `.NET 8`, and `.NET Standard 2.0`
- Conditional `Microsoft.Bcl.AsyncInterfaces` and `Microsoft.Bcl.HashCode` package references for `netstandard2.0`
- `#if NET6_0_OR_GREATER` guards for `CollectionsMarshal` fast-path optimizations across 5 entity files
- `#if NETSTANDARD2_0` polyfill for `Task.IsCompletedSuccessfully` in `MultiMapAsync`
- Test project now multi-targets `net10.0` and `net8.0` — validates `#if NET6_0_OR_GREATER` code paths (e.g., `CollectionsMarshal` optimizations, `IsCompletedSuccessfully` polyfill) on both target frameworks

**Changed**

- **Breaking:** `AddRange` return type changed from `void` to `int` across all implementations and interfaces — now returns the count of successfully added pairs
- **Breaking:** `AddRangeAsync` return type changed from `Task` to `Task<int>` across `IMultiMapAsync` and `MultiMapAsync`
- `MultiMapList.Get` now returns zero-copy `ReadOnlyCollection<TValue>` via `AsReadOnly()` instead of `.ToArray()` allocation
- `GetHashCode()` rewritten across all 7 implementations — replaced collision-prone XOR aggregation with MurmurHash3 finalizer scramble + unchecked addition for better hash distribution
- `MultiMapBase` code deduplication: `MultiMapSet` (−48%), `MultiMapList` (−52%), `SortedMultiMap` (−68%) code reduction
- `MultiMapHelper.Intersect`, `ExceptWith`, and `SymmetricExceptWith` (sync and async) now use batch `RemoveRange`/`AddRange` instead of individual `Remove`/`Add` calls for reduced method call overhead
- `ConcurrentMultiMap.Equals` now includes `_count` fast-exit comparison for consistency with other implementations
- `SortedMultiMap` variable names standardized: `hashset` → `sortedSet` across all methods for consistency with `SortedSet<T>` usage
- `MultiMapAsync.DisposeAsync` no longer allocates an unnecessary `async` state machine — returns `ValueTask` directly
- `BenchmarkSuite.csproj` now includes `ImplicitUsings` and `Nullable` properties for consistency with other projects
- Test count increased from 1,070 to **1,354 tests** across **2 target frameworks** (net10.0, net8.0) — **2,708 total test executions**
- Code coverage improved: **99.5% line coverage** (from 94.3%), **84.8% branch coverage** (from 82.9%), **98.2% method coverage** (from 95.3%)
- README and ReleaseNotes updated to reflect multi-targeting and dispose safety features

**Fixed**

- `ConcurrentMultiMap.Keys` now returns a `.ToArray()` snapshot instead of exposing the live `ConcurrentDictionary` key collection — prevents enumeration errors under concurrent modification
- `MultiMapLock.Equals` and `MultiMapAsync.Equals` now include a `_count` fast-exit comparison before iterating keys and values — short-circuits unequal maps early
- `ConcurrentMultiMap.Equals` could return `true` for maps with different total counts but same key count (missing `_count` comparison)
- `GetHashCode()` used collision-prone XOR aggregation that produced identical hashes for maps with different key orderings — replaced with MurmurHash3 scramble

### 1.0.10

**Fixed**

- Broken **ReleaseNotes** and **README** references for NuGet package

### 1.0.9

**Added**

- Value-based `Equals()`/`GetHashCode()` implementations across all 7 multimap entities — objects with identical key-value content are now structurally equal
- `IComparable<TKey>` and `IComparable<TValue>` generic constraints on `SortedMultiMap<TKey, TValue>` for compile-time safety
- SimpleMultiMap benchmark suite (9 benchmarks) in `SimpleMultiMapBenchmarks.cs`
- Defensive copy (snapshot) tests for `Get`, `GetOrDefault`, and `TryGet` across `MultiMapSet`, `MultiMapList`, `SortedMultiMap`, `ConcurrentMultiMap`, and `MultiMapLock` — ensures callers receive a point-in-time snapshot, not a live reference
- Equals edge-case tests (`DifferentContent`, `DifferentKeys`, `EmptyMaps`) for `MultiMapSet`, `MultiMapList`, `SortedMultiMap`, and `ConcurrentMultiMap`
- 10 new unit tests for `SimpleMultiMap` Equals/GetHashCode behavior

**Changed**

- Test count increased from 1,023 to **1,045 tests**
- Code coverage: **94.3% line coverage**, **92.6% branch coverage**, **100% method coverage**
- `Get`, `GetOrDefault`, and `TryGet` now return defensive `.ToArray()` copies across `MultiMapSet`, `MultiMapList`, `SortedMultiMap`, and `ConcurrentMultiMap` — prevents callers from observing mutations after retrieval
- `ConcurrentMultiMap.Equals` uses deterministic dual-lock ordering via `RuntimeHelpers.GetHashCode` to prevent deadlocks when comparing two instances
- `MultiMapAsync` now uses `ConfigureAwait(false)` on all `await` calls to avoid unnecessary synchronization context captures
- `MultiMapLock.AddRange` and `MultiMapLock.RemoveRange` now execute under a single write lock for atomicity
- `ConcurrentMultiMap.Clear()` now acquires a global lock to prevent count drift during concurrent clears
- `ConcurrentMultiMap.RemoveWhere` now locks the per-key set during predicate evaluation
- `MultiMapLock.RemoveWhere` now acquires a write lock for the entire operation
- `MultiMapLock.TryGet` now returns a snapshot copy instead of a live `HashSet` reference
- Optimized `RemoveWhere` across `MultiMapSet`, `MultiMapList`, and `SortedMultiMap` to use `RemoveAll`/`RemoveWhere` instead of manual iteration
- `ConcurrentMultiMap.KeyCount` simplified to use `ConcurrentDictionary.Count` directly
- `TestDataHelper` moved from `MultiMap/Helpers/` to `MultiMap.Demo/` — no longer part of the NuGet package
- Updated all benchmark data with fresh BenchmarkDotNet v0.15.0 results

**Fixed**

- `MultiMapAsync.GetValuesCountAsync` was ignoring the `key` parameter and always returning the total count instead of the per-key count
- Thread-safety issue in `MultiMapLock.TryGet` that returned a live mutable reference to the internal `HashSet`
- Thread-safety issue in `MultiMapLock.RemoveWhere` that reads without acquiring a lock
- Thread-safety issue in `ConcurrentMultiMap.RemoveWhere` that reads without per-key lock
- Race condition in `ConcurrentMultiMap.Clear()` under concurrent access causing count drift
- Misleading test name in `MultiMapAsync` dispose test

### 1.0.8

**Added**

- `KeyCount` property to `IReadOnlyMultiMap` and all synchronous implementations — O(1) unique key count
- `Values` property to `IReadOnlyMultiMap` and all synchronous implementations — all values across all keys
- `GetValuesCount(TKey key)` method to `IReadOnlyMultiMap` — returns 0 if key doesn't exist
- `this[TKey key]` indexer to `IReadOnlyMultiMap` — convenient dictionary-style access
- `AddRange(IEnumerable<KeyValuePair<TKey, TValue>>)` overload to `IMultiMap` — bulk insert of key-value pairs
- `RemoveRange(IEnumerable<KeyValuePair<TKey, TValue>>)` method to `IMultiMap` — bulk removal returning count of removed pairs
- `RemoveWhere(TKey key, Predicate<TValue>)` method to `IMultiMap` — conditional removal by predicate
- `GetKeyCountAsync()` method to `IReadOnlyMultiMapAsync` — async equivalent of `KeyCount`
- `GetValuesCountAsync(TKey key)` method to `IReadOnlyMultiMapAsync` — async equivalent of `GetValuesCount`
- `GetValuesAsync()` method to `IReadOnlyMultiMapAsync` — async equivalent of `Values`
- `AddRangeAsync(IEnumerable<KeyValuePair<TKey, TValue>>)` overload to `IMultiMapAsync` — async bulk insert
- `RemoveRangeAsync(IEnumerable<KeyValuePair<TKey, TValue>>)` method to `IMultiMapAsync` — async bulk removal
- `RemoveWhereAsync(TKey key, Predicate<TValue>)` method to `IMultiMapAsync` — async conditional removal
- BenchmarkDotNet benchmarks for all new members across all 6 implementations
- "New Interface Members" and "Microbenchmarks" benchmark tables in README
- 252 new unit tests covering all new interface members across all implementations

**Changed**

- Test count increased from 771 to **1,023 tests**
- Code coverage: **94.3% line coverage**, **92.6% branch coverage**, **100% method coverage**
- Updated all benchmark data with fresh BenchmarkDotNet v0.15.0 results
- Comprehensive README rewrite: expanded interface documentation, advanced usage examples, migration guide for v1.0.8+, performance comparison tables

### 1.0.7

**Added**

- `TryGet(TKey key, out IEnumerable<TValue> values)` method to `IReadOnlyMultiMap` interface and all synchronous implementations
- `TryGetAsync(TKey key)` method to `IReadOnlyMultiMapAsync` interface returning `ValueTask<(bool found, IEnumerable<TValue> values)>`
- `GetOrDefault(TKey key)` method to `IReadOnlySimpleMultiMap` interface  
- `GetOrDefaultAsync(TKey key)` method to `IReadOnlyMultiMapAsync` interface
- 57 new unit tests for `TryGet()` and `TryGetAsync()` methods across all implementations
- 18 new unit tests for `Get()` and `GetAsync()` to verify `KeyNotFoundException` behavior
- Benchmarks for `Get()`, `GetOrDefault()`, `TryGet()`, and their async variants (`GetAsync()`, `GetOrDefaultAsync()`, `TryGetAsync()`)
- Test coverage reporting now at **98.7% line coverage** and **98.4% branch coverage**

**Changed**

- Updated `Get()` and `GetAsync()` methods to throw `KeyNotFoundException` when key is not found (breaking change from previous behavior that returned empty)
- All implementations now support three retrieval patterns:
  - `Get(key)` / `GetAsync(key)` — throws exception if key not found
  - `GetOrDefault(key)` / `GetOrDefaultAsync(key)` — returns empty if key not found
  - `TryGet(key, out values)` / `TryGetAsync(key)` — returns bool indicating success
- Updated documentation to reflect interface hierarchy: `IMultiMap` extends `IReadOnlyMultiMap`, `IMultiMapAsync` extends `IReadOnlyMultiMapAsync`
- Test count increased from 714 to **771 tests**
- Updated README with complete interface documentation, including all retrieval method variants

**Fixed**

- Misleading test names that tested `GetOrDefaultAsync` but were named `GetAsync_NonExistentKey_ReturnsEmpty`

### 1.0.6

**Added**

- `Release Notes` section in README

### 1.0.5

**Changed**

- Refactored `ConcurrentMultiMap` internals from `ConcurrentDictionary<TValue, byte>` to `HashSet<TValue>` with per-key locking
- `ConcurrentMultiMap.Count` changed from O(k) to O(1) via `Interlocked` counter
- Added verify-after-lock pattern to prevent count drift under concurrent `RemoveKey`
- Updated all benchmark data in README

**Fixed**

- Race condition in `ConcurrentMultiMap` where `Add`/`Remove` could operate on orphaned hashsets after concurrent `RemoveKey`, causing `Count` to drift from actual data

### 1.0.4

**Changed**

- Updated NuGet package icon

### 1.0.3

**Added**

- `AddRange` benchmarks
- More async and stress tests (714 total)
- Code coverage reporting via Coverlet — 91.5% line coverage
- NuGet installation instructions in README

**Changed**

- Full README rewrite with comparison tables, usage examples, and benchmark results

### 1.0.2

**Added**

- `Keys` property on `IMultiMap` and all implementations
- `IMultiMapAsync` interface with full async/cancellation support
- `MultiMapAsync` implementation using `SemaphoreSlim`
- Async extension methods: `UnionAsync`, `IntersectAsync`, `ExceptWithAsync`, `SymmetricExceptWithAsync`
- BenchmarkDotNet benchmark suite
- Concurrent and sequential stress tests
- Atomic set-operations for thread-safe and async implementations
- Demo console application

**Changed**

- `Task` → `ValueTask` for async interface methods (performance)
- Optimized `MultiMapAsync`, `MultiMapHelper`, and extension methods
- Renamed set methods for consistency

**Fixed**

- Count vulnerability in `ConcurrentMultiMap`
- Count regression in `ConcurrentMultiMap` after optimization pass

### 1.0.1

**Added**

- Initial README with overview, features, and project structure

### 1.0.0

**Added**

- `IMultiMap<TKey, TValue>` synchronous interface
- `ISimpleMultiMap<TKey, TValue>` simplified interface
- `MultiMapList` — list-based, allows duplicate values
- `MultiMapSet` — HashSet-based, unique values per key
- `SortedMultiMap` — sorted keys and values
- `ConcurrentMultiMap` — thread-safe concurrent implementation
- `MultiMapLock` — `ReaderWriterLockSlim`-based
- `SimpleMultiMap` — lightweight simplified API
- Set extension methods: `Union`, `Intersect`, `ExceptWith`, `SymmetricExceptWith` for `IMultiMap` and `ISimpleMultiMap`
- Unit tests for all implementations and extension methods
- NuGet package
