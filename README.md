# MultiMap

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0%20%7C%208.0%20%7C%20Standard%202.0-blue.svg)](https://dotnet.microsoft.com/)
[![C# 14](https://img.shields.io/badge/C%23-14.0-blue)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![NUnit](https://img.shields.io/badge/tests-NUnit%204-green)](https://nunit.org/)
[![BenchmarkDotNet](https://img.shields.io/badge/BenchmarkDotNet-v0.15.8-blue)](https://benchmarkdotnet.org/)
[![Test SDK](https://img.shields.io/badge/Microsoft.NET.Test.Sdk-v18.6.0-blue)](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk)
[![NuGet](https://img.shields.io/nuget/v/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![Coverage](https://img.shields.io/badge/coverage-98.3%25-brightgreen)]()

A **.NET** library providing various generic multimap implementations (set, list, sorted, concurrent, lock-based, async) that map generic keys to collections of generic values with set operations, benchmarks, and thread-safe variants, targeting **.NET 10**, **.NET 8**, and **.NET Standard 2.0**.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Known Limitations](#known-limitations)
- [Project Structure](#project-structure)
- [Interfaces](#interfaces)
- [Implementations](#implementations)
- [Comparison Table](#comparison-table)
  - [Internal Data Structures](#internal-data-structures)
  - [API Behavior Differences](#api-behavior-differences)
  - [When to Use Which Implementation](#when-to-use-which-implementation)
  - [Performance Comparison](#performance-comparison-5000-pairs)
- [Extension Methods](#extension-methods)
- [Installation](#installation)
- [Usage](#usage)
  - [Demo Console Output](#demo-console-output)
- [Testing](#testing)
- [Benchmarks](#benchmarks)
- [Release Notes](#release-notes)
- [License](#license)

## Overview

A **multimap** is a collection that maps each key to one or more values — unlike a standard `Dictionary<TKey, TValue>`, which allows only one value per key. This library provides **7 ready-to-use implementations** behind **6 interfaces** (3 mutable + 3 read-only), so you can choose the right trade-off between uniqueness, ordering, thread-safety, and async support for your scenario. It also ships with **set-like extension methods** (`Union`, `Intersect`, `ExceptWith`, `SymmetricExceptWith`) and **set algebra query methods** (`IsSubsetOf`, `IsSupersetOf`, `Overlaps`, `SetEquals`) that work across all implementations.

## Features

- **7 multimap implementations** covering a wide range of use cases
- **6 interfaces** (3 read-only + 3 mutable): `IReadOnlySimpleMultiMap`, `IReadOnlyMultiMap`, `IReadOnlyMultiMapAsync`, `ISimpleMultiMap`, `IMultiMap`, `IMultiMapAsync` 
- **Multi-target**: .NET 10, .NET 8, and .NET Standard 2.0
- **Set-like extension methods**: `Union`, `Intersect`, `ExceptWith`, `SymmetricExceptWith`
- **Set algebra query methods**: `IsSubsetOf`, `IsSupersetOf`, `Overlaps`, `SetEquals`
- **Thread-safe variants**: fully lock-free (`ConcurrentMultiMap`), reader-writer locked (`MultiMapLock`), and async-safe (`MultiMapAsync`)
- **Dispose safety**: `MultiMapLock` and `MultiMapAsync` throw `ObjectDisposedException` after disposal
- **Custom value comparers**: `IEqualityComparer<TValue>` constructor overloads on all `HashSet`-based implementations
- **Value-based equality** (`Equals`/`GetHashCode`) across all 7 implementations
- **Initial capacity constructors**: Pre-size internal dictionaries to reduce re-allocations
- **Full XML documentation** for IntelliSense support
- **Comprehensive NUnit 4 test suite**: 
  - **4,240 total tests** (2,120 per framework) running on **net10.0** and **net8.0**
  - **98.3% line coverage**, **93.20% branch coverage**, **96.8% method coverage** (latest Coverlet metrics)
  - Covers all implementations, interfaces, edge cases, boundary conditions, concurrent stress tests, and exception handling scenarios
  - 46 new comprehensive tests in v2.1.0 targeting edge cases, complex scenarios, and boundary conditions
- **High code coverage** measured with Coverlet (see [Testing](#testing) section for latest report)

## Known Limitations

### SortedMultiMap Comparer Inconsistency

**Issue:** `SortedMultiMap<TKey, TValue>` accepts an `IComparer<TValue>` for sorting but not an explicit `IEqualityComparer<TValue>` for equality semantics.

**Impact:** When computing `GetHashCode()`, if the value comparer does not implement `IEqualityComparer<TValue>`, the code silently falls back to `EqualityComparer<TValue>.Default`. This can lead to:

- **Hash code inconsistency**: Two `SortedMultiMap` instances with identical content may have different hash codes if they use different custom comparers, violating the `Equals`/`GetHashCode` contract.
- **Collection lookup failures**: Placing such instances in hash-based collections (e.g., `HashSet<T>`, `Dictionary<TKey, TValue>`) can cause unexpected behavior, such as duplicates or lookup failures.

**Example of the Problem:**

```csharp
class CaseInsensitiveValueComparer : IComparer<string>
{
    public int Compare(string? x, string? y) 
        => StringComparer.OrdinalIgnoreCase.Compare(x, y);
}

var map1 = new SortedMultiMap<string, string>(null, new CaseInsensitiveValueComparer());
var map2 = new SortedMultiMap<string, string>();  // Uses default comparer

map1.Add("key", "Hello");
map2.Add("key", "hello");

// map1.Equals(map2) might return true (same content)
// but map1.GetHashCode() != map2.GetHashCode() (comparer mismatch)
// Violates: if Equals(x, y) then GetHashCode(x) == GetHashCode(y)
```

**Recommendation:**

- If you use a custom `IComparer<TValue>` on `SortedMultiMap`, ensure it also implements `IEqualityComparer<TValue>` with consistent semantics.
- Avoid relying on `GetHashCode()` and `Equals()` for hashing if you use a custom comparer that does not implement `IEqualityComparer<TValue>`.
- If you need guaranteed equality/hash consistency, use `MultiMapSet<TKey, TValue>` with a custom `IEqualityComparer<TValue>` instead.

## Project Structure

```
MultiMap/
├── MultiMap/                                 # Core library (NuGet package)
│   ├── Interfaces/
│   │   ├── IReadOnlySimpleMultiMap.cs        # Base read-only interface
│   │   ├── IReadOnlyMultiMap.cs              # Extended read-only interface
│   │   ├── IReadOnlyMultiMapAsync.cs         # Async read-only with cancellation support
│   │   ├── ISimpleMultiMap.cs                # Simplified (extends IReadOnlySimpleMultiMap)
│   │   ├── IMultiMap.cs                      # Synchronous multimap (extends IReadOnlyMultiMap)
│   │   └── IMultiMapAsync.cs                 # Async multimap (extends IReadOnlyMultiMapAsync)
│   ├── Entities/
│   │   ├── MultiMapBase.cs                   # Abstract dictionary-backed base for MultiMapList/Set/Sorted/Concurrent
│   │   ├── MultiMapBase.ValuesCollection.cs  # Nested ValuesCollection enumerator (partial)
│   │   ├── MultiMapBase.ValuesEnumerator.cs  # Nested ValuesEnumerator struct (partial)
│   │   ├── MultiMapList.cs                   # List-based (allows duplicates)
│   │   ├── MultiMapSet.cs                    # HashSet-based (unique values)
│   │   ├── SortedMultiMap.cs                 # SortedDictionary + SortedSet
│   │   ├── ConcurrentSet.cs                  # Thread-safe set value collection for ConcurrentMultiMap
│   │   ├── ConcurrentMultiMap.cs             # Nested ConcurrentDictionary, fully lock-free
│   │   ├── MultiMapLock.cs                   # ReaderWriterLockSlim-based
│   │   ├── MultiMapAsync.cs                  # SemaphoreSlim-based async (public API)
│   │   ├── MultiMapAsync.Core.cs             # SemaphoreSlim-based async (private helpers, partial)
│   │   └── SimpleMultiMap.cs                 # Lightweight ISimpleMultiMap implementation
│   └── Helpers/
│       └── MultiMapHelper.cs                 # Set-like extension methods
├── MultiMap.Tests/                           # Unit tests (NUnit 4, multi-targeted: net10.0 and net8.0)
├── MultiMap.Demo/                            # Console demo application
│   ├── Program.cs                            # Demo entry point
│   └── TestDataHelper.cs                     # Sample data factory for demos
└── BenchmarkSuite/                           # BenchmarkDotNet performance benchmarks
```

## Interfaces

### Interface Hierarchy

The library follows a hierarchical interface design with three parallel families:

**Read-Only Interfaces:**
- `IReadOnlySimpleMultiMap<TKey, TValue>` — Base read-only interface with `Get`, `GetOrDefault`, `TryGet`, `ContainsKey`, and `Contains`
- `IReadOnlyMultiMap<TKey, TValue>` — Extends `IReadOnlySimpleMultiMap` with `Count`, `KeyCount`, `Keys`, `Values`, `GetValuesCount`, `this[key]`, and typed enumeration
- `IReadOnlyMultiMapAsync<TKey, TValue>` — Async read-only with async `Get`-s, `Contains`-es, `Count`-s, `Keys`/`Values` accessors, and async enumeration

**Mutable Interfaces:**
- `ISimpleMultiMap<TKey, TValue>` — Extends `IReadOnlySimpleMultiMap` with `Add`, `Remove`, and `RemoveKey` (returns `bool`)
- `IMultiMap<TKey, TValue>` — Extends both `IReadOnlyMultiMap<TKey, TValue>` and `ISimpleMultiMap<TKey, TValue>` with `AddRange`, `RemoveRange`, `RemoveWhere`, and `Clear`
- `IMultiMapAsync<TKey, TValue>` — Extends `IReadOnlyMultiMapAsync` with async mutations (`Add`/`Remove`), `ClearAsync`, and async equality helpers (`EqualsAsync` overloads)

### `IReadOnlySimpleMultiMap<TKey, TValue>`

The base read-only interface. Extends `IEnumerable<KeyValuePair<TKey, TValue>>` and `IReadOnlyCollection<KeyValuePair<TKey,TValue>>`.

| Method | Returns | Description |
|---|---|---|
| `Get(key)` | `IEnumerable<TValue>` | Returns values; throws `KeyNotFoundException` if not found |
| `GetOrDefault(key)` | `IEnumerable<TValue>` | Returns values or empty if not found |
| `TryGet(key, out values)` | `bool` | Attempts to retrieve values; returns `true` if key exists |
| `ContainsKey(key)` | `bool` | Checks if a key exists |
| `Contains(key, value)` | `bool` | Checks if a specific key-value pair exists |
| `Count` | `int` | Gets the total number of key-value pairs (from `IReadOnlyCollection<KeyValuePair<TKey,TValue>>`) |

### `ISimpleMultiMap<TKey, TValue>`

A simplified multimap interface. Extends `IReadOnlySimpleMultiMap<TKey, TValue>`.

| Method | Returns | Description |
|---|---|---|
| `Add(key, value)` | `bool` | Adds a key-value pair; returns `false` if already present |
| `Remove(key, value)` | `bool` | Removes a specific pair; returns `true` if removed |
| `RemoveKey(key)` | `bool` | Removes all values for a key; returns `true` when the key existed |
| `Clear()` | `void` | Removes all entries |

> **Migration note:** legacy `ISimpleMultiMap.Clear(TKey)`/void patterns were removed; use `RemoveKey(TKey)` and consume its `bool` result.

**Inherited from `IReadOnlySimpleMultiMap`:** `Get`, `GetOrDefault`, `TryGet`, `ContainsKey`, `Contains`, `Count`

### `IReadOnlyMultiMap<TKey, TValue>`

Extended read-only interface. Extends `IReadOnlySimpleMultiMap<TKey, TValue>`.

| Member | Returns | Description |
|---|---|---|
| `KeyCount` | `int` | Gets the number of unique keys |
| `Keys` | `IEnumerable<TKey>` | Gets all keys |
| `Values` | `IEnumerable<TValue>` | Gets all values across all keys |
| `GetValuesCount(key)` | `int` | Gets count of values for a key (0 if missing) |
| `this[key]` | `IEnumerable<TValue>` | Indexer — convenient value access by key |
| `GetEnumerator()` | `IEnumerator<KeyValuePair>` | Enumerates all key-value pairs |

**Inherited from `IReadOnlySimpleMultiMap`:** `Get`, `GetOrDefault`, `TryGet`, `ContainsKey`, `Contains`, `Count`

### `IMultiMap<TKey, TValue>`

The standard synchronous multimap interface. Extends `IReadOnlyMultiMap<TKey, TValue>`.

| Method | Returns | Description |
|---|---|---|
| `Add(key, value)` | `bool` | Adds a key-value pair; returns `false` if already present |
| `AddRange(key, values)` | `int` | Adds multiple values for a key; returns count added |
| `AddRange(items)` | `int` | Adds multiple key-value pairs; returns count added |
| `Remove(key, value)` | `bool` | Removes a specific key-value pair |
| `RemoveRange(items)` | `int` | Removes multiple key-value pairs; returns count removed |
| `RemoveWhere(key, predicate)` | `int` | Removes values matching predicate; returns count removed |
| `RemoveKey(key)` | `bool` | Removes a key and all its values |
| `Clear()` | `void` | Removes all entries |

**Inherited from `IReadOnlyMultiMap`:** `Get`, `GetOrDefault`, `TryGet`, `ContainsKey`, `Contains`, `KeyCount`, `Count`, `Keys`, `Values`, `GetValuesCount`, `this[key]`

### `IReadOnlyMultiMapAsync<TKey, TValue>`

Asynchronous read-only multimap interface. Extends `IAsyncEnumerable<KeyValuePair<TKey, TValue>>`, `IDisposable`, and `IAsyncDisposable`. All methods support `CancellationToken`.

| Method | Returns | Description |
|---|---|---|
| `GetAsync(key)` | `ValueTask<IEnumerable<TValue>>` | Retrieves values; throws `KeyNotFoundException` if not found |
| `GetOrDefaultAsync(key)` | `ValueTask<IEnumerable<TValue>>` | Retrieves values or empty if not found |
| `TryGetAsync(key)` | `ValueTask<(bool, IEnumerable<TValue>)>` | Attempts to retrieve values; returns tuple with found status and values |
| `ContainsKeyAsync(key)` | `ValueTask<bool>` | Checks for a key |
| `ContainsAsync(key, value)` | `ValueTask<bool>` | Checks for a pair |
| `GetCountAsync()` | `ValueTask<int>` | Gets total count of key-value pairs |
| `GetKeyCountAsync()` | `ValueTask<int>` | Gets number of keys |
| `GetKeysAsync()` | `ValueTask<IEnumerable<TKey>>` | Gets all keys |
| `GetValuesCountAsync(key)` | `ValueTask<int>` | Gets count of values for a key |
| `GetValuesAsync()` | `ValueTask<IEnumerable<TValue>>` | Gets all values across all keys |

### `IMultiMapAsync<TKey, TValue>`

Asynchronous multimap interface. Extends `IReadOnlyMultiMapAsync<TKey, TValue>`. All methods support `CancellationToken` and return `ValueTask` or `Task`.

| Method | Returns | Description |
|---|---|---|
| `AddAsync(key, value)` | `ValueTask<bool>` | Asynchronously adds a key-value pair |
| `AddRangeAsync(key, values)` | `ValueTask<int>` | Asynchronously adds multiple values; returns count added |
| `AddRangeAsync(items)` | `ValueTask<int>` | Asynchronously adds multiple key-value pairs; returns count added |
| `RemoveAsync(key, value)` | `ValueTask<bool>` | Asynchronously removes a pair |
| `RemoveRangeAsync(items)` | `ValueTask<int>` | Asynchronously removes multiple pairs; returns count removed |
| `RemoveWhereAsync(key, predicate)` | `ValueTask<int>` | Asynchronously removes values matching predicate; returns count removed |
| `RemoveKeyAsync(key)` | `ValueTask<bool>` | Asynchronously removes a key |
| `ClearAsync()` | `Task` | Asynchronously clears all entries |

**Inherited from `IReadOnlyMultiMapAsync`:** `GetAsync`, `GetOrDefaultAsync`, `TryGetAsync`, `ContainsKeyAsync`, `ContainsAsync`, `GetCountAsync`, `GetKeyCountAsync`, `GetKeysAsync`, `GetValuesCountAsync`, `GetValuesAsync`

## Implementations

### `MultiMapBase<TKey, TValue, TCollection>` — Abstract Base Class

Provides the shared dictionary-backed implementation inherited by `MultiMapList`, `MultiMapSet`, `SortedMultiMap`, and `ConcurrentMultiMap`. Implements `IMultiMap<TKey, TValue>` with `Add`, `AddRange`, `Remove`, `RemoveKey`, `RemoveRange`, `RemoveWhere`, `Get`, `GetOrDefault`, `TryGet`, `ContainsKey`, `Contains`, `Count`, `KeyCount`, `Keys`, `Values`, `GetValuesCount`, indexer, `Clear`, and `GetEnumerator`. Subclasses override `protected` `CreateCollection()`, `TryGetCollection`, `AddToCollection()`, `ToReadOnly`, and `RemoveWhereFromCollection()` to plug in their specific collection type. On .NET 6+, subclasses may also override `Add`/`AddRange` to use `CollectionsMarshal.GetValueRefOrAddDefault` for a single dictionary lookup.

**Encapsulation:** The underlying `_dictionary` field and the `_count` field are `protected`, preventing external subclasses from bypassing the count-bookkeeping invariant.

### `MultiMapList<TKey, TValue>` — List-Based

Extends `MultiMapBase<TKey, TValue, List<TValue>>`. Uses `Dictionary<TKey, List<TValue>>` internally. **Allows duplicate values** per key. Fastest for add operations due to `List<T>.Add` being O(1) amortized. On .NET 6+, it uses `CollectionsMarshal` for optimized `Add`/`AddRange`. Returns a zero-copy `ReadOnlyCollection<TValue>` from `Get`.

**Constructors:** `()`, `(int capacity)`, `(IEqualityComparer<TKey>? keyComparer)`, `(int capacity, IEqualityComparer<TKey>? keyComparer)`

### `MultiMapSet<TKey, TValue>` — HashSet-Based

Extends `MultiMapBase<TKey, TValue, HashSet<TValue>>`. Uses `Dictionary<TKey, HashSet<TValue>>` internally. **Ensures unique values** per key. Best for scenarios that require fast lookups and unique-value semantics. On .NET 6+, it uses `CollectionsMarshal` for optimized `Add`/`AddRange`.

**Constructors:** `()`, `(IEqualityComparer<TKey>? keyComparer)`, `(IEqualityComparer<TValue>? valueComparer)`, `(int capacity)`, `(int capacity, IEqualityComparer<TKey>? keyComparer)`, `(int capacity, IEqualityComparer<TValue>? valueComparer)`, `(int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)`

### `SortedMultiMap<TKey, TValue>` — Sorted

Extends `MultiMapBase<TKey, TValue, SortedSet<TValue>>`. Uses `SortedDictionary<TKey, SortedSet<TValue>>`. Keys and values are maintained in sorted order. Ideal for ordered enumeration and range queries.

**Type constraints:** `TKey` must implement both `IEquatable<TKey>` (required by all multimap interfaces and `MultiMapBase`) and `IComparable<TKey>` (required by `SortedMultiMap` for sorted key ordering). The actual sorted operations rely on `IComparer<TKey>` — either the default comparer or a custom one supplied via a constructor overload. `TValue` similarly requires both `IEquatable<TValue>` (library-wide) and `IComparable<TValue>` (for `SortedSet<TValue>` ordering).

**Constructors:** `()`, `(IComparer<TKey>? keyComparer)`, `(IComparer<TValue>? valueComparer)`, `(IComparer<TKey>? keyComparer, IComparer<TValue>? valueComparer)`

### `ConcurrentMultiMap<TKey, TValue>` — Fully Lock-Free Concurrent

Extends `MultiMapBase<TKey, TValue, ConcurrentSet<TValue>>`. Uses `ConcurrentDictionary<TKey, ConcurrentSet<TValue>>` for fully lock-free concurrent access — no explicit locks are held for per-key operations. `Count` and `KeyCount` are **O(1)**, backed by `_count` and `_keyCount` fields maintained via `Interlocked` in every mutating path. `Keys` and `Values` are **lazy iterators** that yield directly from the live dictionary (call `.ToList()` / `.ToArray()` when a stable snapshot is needed). Suitable for high-concurrency scenarios.

> **Note:** `ConcurrentMultiMap` does **not** implement `IDisposable`. Unlike `MultiMapLock` (which disposes a `ReaderWriterLockSlim`) and `MultiMapAsync` (which disposes two `SemaphoreSlim` instances), `ConcurrentMultiMap` owns no disposable resources — the underlying `ConcurrentDictionary` requires no explicit cleanup.

**Constructors:** `()`, `(IEqualityComparer<TKey>? keyComparer)`, `(IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)`, `(IEqualityComparer<TValue>? valueComparer)`, `(int concurrencyLevel, int capacity)`, `(int concurrencyLevel, int capacity, IEqualityComparer<TKey>? keyComparer)`, `(int concurrencyLevel, int capacity, IEqualityComparer<TValue>? valueComparer)`, `(int concurrencyLevel, int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)`

### `MultiMapLock<TKey, TValue>` — Reader-Writer Locked

Implements `IMultiMap` and `IDisposable`. Uses `ReaderWriterLockSlim` to allow concurrent reads with exclusive writes. Good for read-heavy workloads with occasional writes.

**Constructors:** `()`, `(IEqualityComparer<TKey>? keyComparer)`, `(IEqualityComparer<TValue>? valueComparer)`, `(int capacity)`, `(int capacity, IEqualityComparer<TKey>? keyComparer)`, `(int capacity, IEqualityComparer<TValue>? valueComparer)`, `(int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)`

### `MultiMapAsync<TKey, TValue>` — Async-Safe

Implements `IMultiMapAsync`, `IDisposable`, and `IAsyncDisposable`. Uses `SemaphoreSlim` for async-compatible mutual exclusion. Designed for `async`/`await` patterns and I/O-bound scenarios. `Equals(IReadOnlyMultiMapAsync<TKey, TValue>?)` uses a deadlock-safe dual-semaphore acquisition when comparing two `MultiMapAsync` instances; `Equals(object?)` throws `InvalidOperationException` under a `SynchronizationContext` — use `EqualsAsync` in `async` contexts instead.

> **Locking protocol:** Two `SemaphoreSlim(1,1)` instances implement a custom readers-writer protocol. `_writeLock` is held exclusively by every mutating operation **and** by the *first* reader for as long as any reader is active — preventing writers from entering while readers hold it. `_readerLock` guards the `_activeReaders` counter and is held only for the brief increment/decrement critical section, allowing multiple concurrent readers once their count is registered. Key invariants: (1) a writer blocks until `_activeReaders` reaches 0 and `_writeLock` is released by the last reader; (2) the first reader blocks if a writer currently holds `_writeLock`; (3) every operation has a non-blocking fast path (`Wait(0)`) that avoids a heap-allocated continuation when there is no contention.

> **Writer starvation under heavy read traffic:** Because every read acquires the shared `_writeLock`, writers can be delayed indefinitely under sustained high-frequency concurrent reads. Prefer `MultiMapLock`, whose `ReaderWriterLockSlim` allows multiple concurrent readers while blocking only for exclusive writes.

> **Note:** The primary constructor is `MultiMapAsync(int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)`; all other constructor overloads delegate to it.

**Constructors:** `()`, `(IEqualityComparer<TKey>? keyComparer)`, `(IEqualityComparer<TValue>? valueComparer)`, `(int capacity)`, `(int capacity, IEqualityComparer<TKey>? keyComparer)`, `(int capacity, IEqualityComparer<TValue>? valueComparer)`, `(int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)`

### `SimpleMultiMap<TKey, TValue>` — Lightweight

Implements `ISimpleMultiMap`. A lightweight multimap with a simplified API. Provides typed `Equals(IReadOnlySimpleMultiMap<TKey, TValue>?)` comparing total pair count then per-key value-set contents.

**Constructors:** `()`, `(IEqualityComparer<TKey>? keyComparer)`, `(IEqualityComparer<TValue>? valueComparer)`, `(IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)`, `(int capacity)`, `(int capacity, IEqualityComparer<TKey>? keyComparer)`, `(int capacity, IEqualityComparer<TValue>? valueComparer)`, `(int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)`

## Comparison Table

| Implementation | Interface | Thread-Safe | Duplicates | Ordered | Count Complexity |
|---|---|---|---|---|---|
| `SimpleMultiMap` | `ISimpleMultiMap` | ❌ No | ❌ No | ❌ No | O(1) |
| `MultiMapList` | `IMultiMap` | ❌ No | ✅ Yes | ❌ No | O(1) |
| `MultiMapSet` | `IMultiMap` | ❌ No | ❌ No | ❌ No | O(1) |
| `SortedMultiMap` | `IMultiMap` | ❌ No | ❌ No | ✅ Yes | O(1) |
| `ConcurrentMultiMap` | `IMultiMap` | ✅ Lock-free | ❌ No | ❌ No | O(1) |
| `MultiMapLock` | `IMultiMap` | ✅ RW Lock | ❌ No | ❌ No | O(1) |
| `MultiMapAsync` | `IMultiMapAsync` | ✅ Semaphore | ❌ No | ❌ No | O(1) |

### Internal Data Structures

| Implementation | Outer Structure | Inner Structure | Notes |
|---|---|---|---|
| `SimpleMultiMap` | `Dictionary<TKey, HashSet<TValue>>` | `HashSet<TValue>` | Simplified API surface |
| `MultiMapList` | `Dictionary<TKey, List<TValue>>` | `List<TValue>` | O(1) amortized add; allows duplicate values |
| `MultiMapSet` | `Dictionary<TKey, HashSet<TValue>>` | `HashSet<TValue>` | O(1) add/contains; enforces unique values |
| `SortedMultiMap` | `SortedDictionary<TKey, SortedSet<TValue>>` | `SortedSet<TValue>` | O(log n) operations; keys & values sorted |
| `ConcurrentMultiMap` | `ConcurrentDictionary<TKey, ConcurrentSet<TValue>>` | `ConcurrentSet<TValue>` | Fully lock-free via nested `ConcurrentDictionary`; `Count` and `KeyCount` are O(1) via `Interlocked`-maintained counters |
| `MultiMapLock` | `Dictionary<TKey, HashSet<TValue>>` | `HashSet<TValue>` | Protected by `ReaderWriterLockSlim` |
| `MultiMapAsync` | `Dictionary<TKey, HashSet<TValue>>` | `HashSet<TValue>` | Protected by a custom readers-writer protocol over two `SemaphoreSlim(1,1)` instances (`_writeLock` + `_readerLock`) |

### API Behavior Differences

| Behavior | `IMultiMap` | `IMultiMapAsync` | `ISimpleMultiMap` |
|---|---|---|---|
| **Interface Hierarchy** | ✅ Extends `IReadOnlyMultiMap` → `IReadOnlySimpleMultiMap` | ✅ Extends `IReadOnlyMultiMapAsync` | ✅ Extends `IReadOnlySimpleMultiMap` |
| **Get (missing key)** | ✅ `Get` throws `KeyNotFoundException`; `GetOrDefault` returns empty | ✅ `GetAsync` throws `KeyNotFoundException`; `GetOrDefaultAsync` returns empty | ✅ `Get` throws `KeyNotFoundException`; `GetOrDefault` returns empty |
| **TryGet (missing key)** | ✅ `TryGet` returns `false` with empty collection | ✅ `TryGetAsync` returns `(false, empty)` tuple | ✅ `TryGet` returns `false` with empty collection |
| **KeyCount property** | ✅ `KeyCount` property (number of unique keys) | ✅ `GetKeyCountAsync()` method | ❌ Not available |
| **Add (duplicate)** | ✅ Returns `false` | ✅ Returns `false` (via `ValueTask<bool>`) | ✅ Returns `false` |
| **AddRange** | ✅ `AddRange(key, values)` and `AddRange(items)` | ✅ `AddRangeAsync(key, values)` and `AddRangeAsync(items)` | ❌ Not available |
| **Remove return type** | ✅ `bool` | ✅ `ValueTask<bool>` | ✅ `bool` |
| **RemoveRange** | ✅ `RemoveRange(items)` returns `int` | ✅ `RemoveRangeAsync(items)` returns `ValueTask<int>` | ❌ Not available |
| **RemoveWhere** | ✅ `RemoveWhere(key, predicate)` returns `int` | ✅ `RemoveWhereAsync(key, predicate)` returns `ValueTask<int>` | ❌ Not available |
| **GetValuesCount** | ✅ `GetValuesCount(key)` returns `int` | ✅ `GetValuesCountAsync(key)` returns `ValueTask<int>` | ❌ Not available |
| **Enumeration** | ✅ `IEnumerable<KeyValuePair>` | ✅ `IAsyncEnumerable<KeyValuePair>` | ✅ `IEnumerable<KeyValuePair>` |
| **Disposable** | ⚠️ Only `MultiMapLock` | ✅ Yes (`IAsyncDisposable` + `IDisposable`) | ❌ No |
| **CancellationToken** | ❌ No | ✅ Yes (all methods) | ❌ No |

### When to Use Which Implementation

| Use Case | Recommended Implementation | Reason |
|---|---|---|
| Minimal API, quick prototyping | `SimpleMultiMap` | Simplified interface with direct enumeration |
| General purpose, unique values | `MultiMapSet` | Fast O(1) lookups with uniqueness guarantee |
| Duplicate values needed | `MultiMapList` | Only implementation allowing duplicate values per key |
| Sorted enumeration / range queries | `SortedMultiMap` | Maintains key and value ordering |
| High-concurrency, many threads | `ConcurrentMultiMap` | Fully lock-free via nested `ConcurrentDictionary`; no contention under concurrent reads/writes |
| Read-heavy, occasional writes | `MultiMapLock` | RW lock allows concurrent readers |
| Async / I/O-bound code | `MultiMapAsync` | `SemaphoreSlim` works with `async`/`await` |

### Performance Comparison (5,000 pairs)

| Implementation | Add | Get (100 keys) | Contains | Count | Relative Add Speed |
|---|---|---|---|---|---|
| `MultiMapList` | 46,949 ns | 9,157 ns | 35 ns | < 1 ns | **1.0x** (baseline) |
| `SimpleMultiMap` | 81,557 ns | 12,013 ns | 5 ns | < 1 ns | 1.7x |
| `MultiMapSet` | 81,379 ns | 12,909 ns | 41 ns | < 1 ns | 1.7x |
| `MultiMapLock` | 123,190 ns | 12,439 ns | 15 ns | 11 ns | 2.6x |
| `MultiMapAsync` | 185,584 ns | 18,283 ns | 82 ns | 80 ns | 4.0x |
| `ConcurrentMultiMap` | 202,077 ns | 54,536 ns | 186 ns | < 1 ns | 4.3x |
| `SortedMultiMap` | 845,301 ns | 37,745 ns | 34 ns | < 1 ns | 18.0x |

> **Note:** Performance data from BenchmarkDotNet (latest run on .NET 10.0.8 / SDK 10.0.300). See [Benchmarks](#benchmarks) for full details.

## Extension Methods

For all 3 interface families, the `MultiMapHelper` provides:

1. **Set-like operations as extension methods:**

| Family | Methods | Current signatures |
|---|---|---|
| `ISimpleMultiMap<TKey, TValue>` | `Union`, `Intersect`, `ExceptWith`, `SymmetricExceptWith` | `this ISimpleMultiMap<TKey,TValue> target, ISimpleMultiMap<TKey,TValue> other` → returns `ISimpleMultiMap<TKey,TValue>` |
| `IMultiMap<TKey, TValue>` | `Union`, `Intersect`, `ExceptWith`, `SymmetricExceptWith` | `this IMultiMap<TKey,TValue> target, IMultiMap<TKey,TValue> other` → returns `IMultiMap<TKey,TValue>` |
| `IMultiMapAsync<TKey, TValue>` | `UnionAsync`, `IntersectAsync`, `ExceptWithAsync`, `SymmetricExceptWithAsync` | `this IMultiMapAsync<TKey,TValue> target, IMultiMapAsync<TKey,TValue> other, CancellationToken cancellationToken = default` → returns `Task` |

These signatures mutate and return the sync target map for fluent usage, while async extensions complete via `Task` and support `CancellationToken`.

2. **Set algebra query operations as extension methods:**

| Family | Methods | Current signatures |
|---|---|---|
| `ISimpleMultiMap<TKey, TValue>` | `IsSubsetOf`, `IsSupersetOf`, `Overlaps`, `SetEquals` | `this ISimpleMultiMap<TKey,TValue> target, ISimpleMultiMap<TKey,TValue> other` → returns `bool` |
| `IMultiMap<TKey, TValue>` | `IsSubsetOf`, `IsSupersetOf`, `Overlaps`, `SetEquals` | `this IMultiMap<TKey,TValue> target, IMultiMap<TKey,TValue> other` → returns `bool` |
| `IMultiMapAsync<TKey, TValue>` | `IsSubsetOfAsync`, `IsSupersetOfAsync`, `OverlapsAsync`, `SetEqualsAsync` | `this IMultiMapAsync<TKey,TValue> target, IMultiMapAsync<TKey,TValue> other, CancellationToken cancellationToken = default` → returns `Task<bool>` |

These signatures return `bool`, while async extensions complete via `Task<bool>` and support `CancellationToken`.

> **Note:** When used with concurrent implementations, these methods are **not atomic**. Individual operations are thread-safe, but the overall result may reflect interleaved concurrent modifications. No structural corruption or count drift will occur.

> **Performance notes for set-like helpers:** `Intersect` and `SymmetricExceptWith` build a per-key `HashSet<TValue>` lookup to avoid O(n²) inner-loop scans. When the per-key value collection already implements `ISet<TValue>` (for example, when the underlying map is a `MultiMapSet`), the existing set is used directly, and no allocation occurs. `ExceptWith` and `Union` iterate directly without any temporary collection. `GetHashCode()` on all concrete implementations uses the key and value equality comparers stored by that instance, so hash codes remain consistent with the equality semantics used by the map — custom comparers are fully respected.

## Installation

### NuGet

```shell
dotnet add package MultiMap
```

### Package Reference

```xml
<PackageReference Include="MultiMap" Version="2.1.0" />
```

## Usage

### Basic Usage with `IMultiMap`

```csharp
using MultiMap.Entities;

// HashSet-based (unique values per key)
var map = new MultiMapSet<string, int>();
map.Add("fruits", 1);
map.Add("fruits", 2);
map.Add("fruits", 1); // returns false — already exists
map.AddRange("vegetables", [10, 20, 30]);

IEnumerable<int> values = map.Get("fruits");   // [1, 2]
bool exists = map.Contains("fruits", 1);       // true
int count = map.Count;                         // 5

map.Remove("fruits", 1);
map.RemoveKey("vegetables");
```

### Advanced Usage — New Interface Members

#### Working with KeyCount and Values

```csharp
using MultiMap.Entities;

var map = new MultiMapSet<string, int>();
map.Add("A", 1);
map.Add("A", 2);
map.Add("B", 3);

// KeyCount returns the number of unique keys (not total pairs)
int keyCount = map.KeyCount;        // 2 (keys: "A", "B")
int totalCount = map.Count;         // 3 (pairs: A→1, A→2, B→3)

// Values property returns all values across all keys
IEnumerable<int> allValues = map.Values;  // [1, 2, 3]

// GetValuesCount returns the count for a specific key
int valuesForA = map.GetValuesCount("A");  // 2
int valuesForB = map.GetValuesCount("B");  // 1
int noKey = map.GetValuesCount("C");       // 0 (key doesn't exist)

// Indexer provides convenient access to values
IEnumerable<int> aValues = map["A"];  // [1, 2]
```

#### Bulk Operations with AddRange and RemoveRange

```csharp
using MultiMap.Entities;

var map = new MultiMapSet<string, int>();

// AddRange with key-value pairs collection
var items = new[]
{
    new KeyValuePair<string, int>("A", 1),
    new KeyValuePair<string, int>("A", 2),
    new KeyValuePair<string, int>("B", 3)
};
map.AddRange(items);

// RemoveRange returns the count of actually removed pairs
var toRemove = new[]
{
    new KeyValuePair<string, int>("A", 1),
    new KeyValuePair<string, int>("C", 99)  // doesn't exist
};
int removedCount = map.RemoveRange(toRemove);  // Returns 1 (only A→1 was removed)
```

#### Conditional Removal with RemoveWhere

```csharp
using MultiMap.Entities;

var map = new MultiMapSet<string, int>();
map.AddRange("numbers", [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);

// Remove all even numbers for the key "numbers"
int removed = map.RemoveWhere("numbers", n => n % 2 == 0);
Console.WriteLine($"Removed {removed} even numbers");  // "Removed 5 even numbers"

// map["numbers"] now contains: [1, 3, 5, 7, 9]
```

#### Retrieval Pattern Options

```csharp
using MultiMap.Entities;

var map = new MultiMapSet<string, int>();
map.Add("A", 1);

// Pattern 1: Get (throws on missing key)
try
{
    var values = map.Get("B");
}
catch (KeyNotFoundException)
{
    Console.WriteLine("Key not found!");
}

// Pattern 2: GetOrDefault (returns empty on missing key)
var safe = map.GetOrDefault("B");  // Returns empty collection

// Pattern 3: TryGet (boolean pattern)
if (map.TryGet("A", out var result))
{
    Console.WriteLine($"Found {result.Count()} values");
}
else
{
    Console.WriteLine("Key not found");
}
```

### Concurrent Usage

```csharp
using MultiMap.Entities;

var concurrentMap = new ConcurrentMultiMap<string, int>();

// Safe to call from multiple threads
Parallel.For(0, 1000, i =>
{
    concurrentMap.Add("key", i);
});

// KeyCount is O(1) with thread-safe reads
int keys = concurrentMap.KeyCount;
int total = concurrentMap.Count;
```

### Async Usage

```csharp
using MultiMap.Entities;

using var asyncMap = new MultiMapAsync<string, int>();
await asyncMap.AddAsync("key", 1);
await asyncMap.AddAsync("key", 2);

var values = await asyncMap.GetAsync("key");              // [1, 2]
var count = await asyncMap.GetCountAsync();               // 2
bool contains = await asyncMap.ContainsAsync("key", 1);   // true
```

#### Advanced Async Operations

```csharp
using MultiMap.Entities;

using var map = new MultiMapAsync<string, int>();

// Bulk add with AddRangeAsync
var items = new[]
{
    new KeyValuePair<string, int>("A", 1),
    new KeyValuePair<string, int>("A", 2),
    new KeyValuePair<string, int>("B", 3)
};
await map.AddRangeAsync(items);

// Get key and value counts
int keyCount = await map.GetKeyCountAsync();       // 2
int totalCount = await map.GetCountAsync();        // 3
int aValues = await map.GetValuesCountAsync("A");  // 2

// Bulk remove with RemoveRangeAsync
var toRemove = new[]
{
    new KeyValuePair<string, int>("A", 1),
    new KeyValuePair<string, int>("B", 3)
};
int removed = await map.RemoveRangeAsync(toRemove);  // Returns 2

// Conditional removal with RemoveWhereAsync
await map.AddRangeAsync("numbers", [1, 2, 3, 4, 5, 6]);
int removedCount = await map.RemoveWhereAsync("numbers", n => n > 3);
// Removed values: 4, 5, 6

// TryGetAsync pattern
var (found, values) = await map.TryGetAsync("A");
if (found)
{
    Console.WriteLine($"Found {values.Count()} values");
}

// All methods support CancellationToken
using var cts = new CancellationTokenSource();
await map.AddAsync("key", 100, cts.Token);
```

### Set Operations

```csharp
using MultiMap.Entities;
using MultiMap.Helpers;

var map1 = new MultiMapSet<string, int>();
map1.Add("A", 1);
map1.Add("A", 2);
map1.Add("B", 3);

var map2 = new MultiMapSet<string, int>();
map2.Add("A", 2);
map2.Add("A", 3);
map2.Add("C", 4);

map1 = map1.Union(map2);                // Union: adds all pairs from map2 into map1
map1 = map1.Intersect(map2);            // Intersect: keeps only pairs present in both
map1 = map1.ExceptWith(map2);           // ExceptWith: removes pairs that exist in map2
map1 = map1.SymmetricExceptWith(map2);  // SymmetricExceptWith: keeps only pairs in one but not both

// Read-only set query operations (extension methods):
bool isSubset = MultiMapHelper.IsSubsetOf(map1, map2);      // Check if map1 ⊆ map2
bool isSuperset = MultiMapHelper.IsSupersetOf(map1, map2);  // Check if map1 ⊇ map2
bool overlaps = MultiMapHelper.Overlaps(map1, map2);        // Check if they share any pairs
bool equals = MultiMapHelper.SetEquals(map1, map2);         // Check if they contain the same pairs

// Atomic set operations and set query methods for MultiMapLock:
var lockMap1 = new MultiMapLock<string, int>();
lockMap1.Add("A", 1);
lockMap1.Add("A", 2);
lockMap1.Add("B", 3);

var lockMap2 = new MultiMapLock<string, int>();
lockMap2.Add("A", 2);
lockMap2.Add("A", 3);
lockMap2.Add("C", 4);

lockMap1.Union(lockMap2);               // Union: adds all pairs from lockMap2 into lockMap1
lockMap1.Intersect(lockMap2);           // Intersect: keeps only pairs present in both
lockMap1.ExceptWith(lockMap2);          // ExceptWith: removes pairs that exist in lockMap2
lockMap1.SymmetricExceptWith(lockMap2); // SymmetricExceptWith: keeps only pairs in one but not both

bool isSubsetLock = lockMap1.IsSubsetOf(lockMap2);      // Check if lockMap1 ⊆ lockMap2
bool isSupersetLock = lockMap1.IsSupersetOf(lockMap2);  // Check if lockMap1 ⊇ lockMap2
bool overlapsLock = lockMap1.Overlaps(lockMap2);        // Check if they share any pairs
bool equalsLock = lockMap1.SetEquals(lockMap2);         // Check if they contain the same pairs

// Atomic set operations and set query methods for MultiMapAsync:
var asyncMap1 = new MultiMapAsync<string, int>();
await asyncMap1.AddAsync("A", 1);
await asyncMap1.AddAsync("A", 2);
await asyncMap1.AddAsync("B", 3);

var asyncMap2 = new MultiMapAsync<string, int>();
await asyncMap2.AddAsync("A", 2);
await asyncMap2.AddAsync("A", 3);
await asyncMap2.AddAsync("C", 4);

await asyncMap1.UnionAsync(asyncMap2);                  // Union: adds all pairs from asyncMap2 into asyncMap1
await asyncMap1.IntersectAsync(asyncMap2);              // Intersect: keeps only pairs present in both
await asyncMap1.ExceptWithAsync(asyncMap2);             // ExceptWith: removes pairs that exist in asyncMap2
await asyncMap1.SymmetricExceptWithAsync(asyncMap2);    // SymmetricExceptWith: keeps only pairs in one but not both

bool isSubsetAsync = await asyncMap1.IsSubsetOfAsync(asyncMap2);        // Check if asyncMap1 ⊆ asyncMap2
bool isSupersetAsync = await asyncMap1.IsSupersetOfAsync(asyncMap2);    // Check if asyncMap1 ⊇ asyncMap2
bool overlapsAsync = await asyncMap1.OverlapsAsync(asyncMap2);          // Check if they share any pairs
bool equalsAsync = await asyncMap1.SetEqualsAsync(asyncMap2);           // Check if they contain the same pairs
```

### SimpleMultiMap with Demo

```csharp
using MultiMap.Entities;
using MultiMap.Helpers;

ISimpleMultiMap<string, int> map = new SimpleMultiMap<string, int>();
map.Add("A", 1);
map.Add("A", 2);

var values = map.Get("A");                                      // [1, 2]
var safe = map.GetOrDefault("missing");                         // empty
bool tryGetA = map.TryGet("A", out var values);                 // true; values = [1, 2]
bool tryGetMissing = map.TryGet("missing", out var values);     // false; values = empty

// Enumerate directly — ISimpleMultiMap implements IEnumerable<KeyValuePair<TKey, TValue>>
foreach (var kvp in map) { /* ... */ }                  // replaces map.Flatten()

// Set operations return the modified map
ISimpleMultiMap<string, int> union = map.Union(otherMap);
ISimpleMultiMap<string, int> intersection = map.Intersect(otherMap);
ISimpleMultiMap<string, int> exceptWith = map.ExceptWith(otherMap);
ISimpleMultiMap<string, int> symmetricExceptWith = map.SymmetricExceptWith(otherMap);

// Set query operations return bool
bool isSubsetOf = map.IsSubsetOf(otherMap);
bool isSupersetOf = map.IsSupersetOf(otherMap);
bool overlaps = map.Overlaps(otherMap);
bool setEquals = map.SetEquals(otherMap);
```

## Testing

See [Testing](./Testing.md) for the full unit test details.

## Benchmarks

See [Benchmarks](./Benchmarks.md) for the full benchmark details.

## Migration Guide

See [MigrationGuide](./MigrationGuide.md) for the full migration guide.

## Release Notes

See [ReleaseNotes](./ReleaseNotes.md) for the full version history.

## License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).

See [LICENSE](https://github.com/TigoS/MultiMap?tab=MIT-1-ov-file) for details.

## Author

**TigoS** — [GitHub](https://github.com/TigoS/MultiMap)
