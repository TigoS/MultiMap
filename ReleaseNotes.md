# MultiMap

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0%20%7C%208.0%20%7C%20Standard%202.0-blue.svg)](https://dotnet.microsoft.com/)
[![C# 14](https://img.shields.io/badge/C%23-14.0-blue)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![NUnit](https://img.shields.io/badge/tests-NUnit%204-green)](https://nunit.org/)
[![BenchmarkDotNet](https://img.shields.io/badge/BenchmarkDotNet-v0.15.0-blue)](https://benchmarkdotnet.org/)
[![NuGet](https://img.shields.io/nuget/v/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![Coverage](https://img.shields.io/badge/coverage-94.3%25-brightgreen)]()

A **.NET** library targeting **.NET 10**, **.NET 8**, and **.NET Standard 2.0**

## Release Notes

### 1.0.11

**Added**

- `IEqualityComparer<TValue>` constructor overloads on `MultiMapSet`, `MultiMapLock`, `MultiMapAsync`, `ConcurrentMultiMap`, and `SimpleMultiMap` — enables custom value comparison (e.g., case-insensitive strings)
- Initial capacity constructor overloads on all 6 entity types (`MultiMapSet`, `MultiMapList`, `SortedMultiMap`, `ConcurrentMultiMap`, `MultiMapLock`, `MultiMapAsync`) for memory pre-allocation
- Combined capacity + comparer constructor overloads where applicable
- 35 new unit tests covering constructor overloads, comparer behavior, and case-insensitive scenarios
- Multi-target support: the NuGet package now ships assemblies for `.NET 10`, `.NET 8`, and `.NET Standard 2.0`
- Dispose guards (`ObjectDisposedException`) on every public member of `MultiMapLock` and `MultiMapAsync` after disposal
- 25 new dispose-guard unit tests (13 for `MultiMapLock`, 12 for `MultiMapAsync`)
- Conditional `Microsoft.Bcl.AsyncInterfaces` and `Microsoft.Bcl.HashCode` package references for `netstandard2.0`
- `#if NET6_0_OR_GREATER` guards for `CollectionsMarshal` fast-path optimizations across 5 entity files
- `#if NETSTANDARD2_0` polyfill for `Task.IsCompletedSuccessfully` in `MultiMapAsync`

**Changed**

- Test count increased from 1,070 to **1,105 tests**
- `MultiMapHelper.Intersect`, `ExceptWith`, and `SymmetricExceptWith` (sync and async) now use batch `RemoveRange`/`AddRange` instead of individual `Remove`/`Add` calls for reduced method call overhead
- Test count increased from 1,045 to **1,070 tests**
- `ConcurrentMultiMap.Equals` now includes `_count` fast-exit comparison for consistency with other implementations
- `SortedMultiMap` variable names standardized: `hashset` → `sortedSet` across all methods for consistency with `SortedSet<T>` usage
- `MultiMapAsync.DisposeAsync` no longer allocates an unnecessary `async` state machine — returns `ValueTask` directly
- `BenchmarkSuite.csproj` now includes `ImplicitUsings` and `Nullable` properties for consistency with other projects
- README and ReleaseNotes updated to reflect multi-targeting and dispose safety features

**Fixed**

- `ConcurrentMultiMap.Keys` now returns a `.ToArray()` snapshot instead of exposing the live `ConcurrentDictionary` key collection — prevents enumeration errors under concurrent modification
- `MultiMapLock.Equals` and `MultiMapAsync.Equals` now include a `_count` fast-exit comparison before iterating keys and values — short-circuits unequal maps early
- `ConcurrentMultiMap.Equals` could return `true` for maps with different total counts but same key count (missing `_count` comparison)

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
- Updated README with complete interface documentation including all retrieval method variants

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
