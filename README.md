# MultiMap

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0%20%7C%208.0%20%7C%20Standard%202.0-blue.svg)](https://dotnet.microsoft.com/)
[![C# 14](https://img.shields.io/badge/C%23-14.0-blue)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![NUnit](https://img.shields.io/badge/tests-NUnit%204-green)](https://nunit.org/)
[![BenchmarkDotNet](https://img.shields.io/badge/BenchmarkDotNet-v0.15.0-blue)](https://benchmarkdotnet.org/)
[![NuGet](https://img.shields.io/nuget/v/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![Coverage](https://img.shields.io/badge/coverage-96.8%25-brightgreen)]()

A **.NET** library targeting **.NET 10**, **.NET 8**, and **.NET Standard 2.0**

## Table of Contents

- [Overview](#overview)
- [Features](#features)
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
  - [Test Coverage by Implementation](#test-coverage-by-implementation)
  - [Test Coverage by Base Class](#test-coverage-by-base-class)
  - [Coverage Gap Tests](#coverage-gap-tests)
  - [Test Coverage by Extension Methods](#test-coverage-by-extension-methods)
  - [Test Categories](#test-categories)
  - [Test Coverage Percentage](#test-coverage-percentage)
  - [Code Coverage (Coverlet)](#code-coverage-coverlet)
- [Benchmarks](#benchmarks)
- [Release Notes](#release-notes)
- [License](#license)

## Overview

A **multimap** is a collection that maps each key to one or more values — unlike a standard `Dictionary<TKey, TValue>`, which allows only one value per key. This library provides **7 ready-to-use implementations** behind **6 interfaces** (3 mutable + 3 read-only), so you can choose the right trade-off between uniqueness, ordering, thread-safety, and async support for your scenario. It also ships with **set-like extension methods** (`Union`, `Intersect`, `ExceptWith`, `SymmetricExceptWith`) that work across all implementations.

## Features

- **7 multimap implementations** covering a wide range of use cases
- **6 interfaces** (3 mutable + 3 read-only): `IMultiMap`, `IMultiMapAsync`, `ISimpleMultiMap`, `IReadOnlyMultiMap`, `IReadOnlyMultiMapAsync`, `IReadOnlySimpleMultiMap`
- **Multi-target**: .NET 10, .NET 8, and .NET Standard 2.0
- **Set-like extension methods**: `Union`, `Intersect`, `ExceptWith`, `SymmetricExceptWith`
- **Thread-safe variants**: fully lock-free (`ConcurrentMultiMap`), reader-writer locked (`MultiMapLock`), and async-safe (`MultiMapAsync`)
- **Dispose safety**: `MultiMapLock` and `MultiMapAsync` throw `ObjectDisposedException` after disposal
- **Custom value comparers**: `IEqualityComparer<TValue>` constructor overloads on all `HashSet`-based implementations
- **Initial capacity constructors**: Pre-size internal dictionaries to reduce re-allocations
- **Full XML documentation** for IntelliSense support
- **Comprehensive NUnit 4 test suite** running on **net10.0** and **net8.0**
- **High code coverage** measured with Coverlet (see Testing section for latest report)
- **Value-based equality** (`Equals`/`GetHashCode`) across all 7 implementations

## Project Structure

```
MultiMap/
├── MultiMap/                                   # Core library (NuGet package)
│   ├── Interfaces/
│   │   ├── IReadOnlySimpleMultiMap.cs          # Base read-only interface
│   │   ├── IReadOnlyMultiMap.cs                # Extended read-only with TryGet, Contains, KeyCount
│   │   ├── IReadOnlyMultiMapAsync.cs           # Async read-only with cancellation support
│   │   ├── ISimpleMultiMap.cs                  # Simplified interface (extends IReadOnlySimpleMultiMap)
│   │   ├── IMultiMap.cs                        # Synchronous multimap (extends IReadOnlyMultiMap)
│   │   └── IMultiMapAsync.cs                   # Async multimap (extends IReadOnlyMultiMapAsync)
│   ├── Entities/
│   │   ├── MultiMapBase.cs                     # Abstract base class for non-concurrent multimaps
│   │   ├── MultiMapBase.ValuesCollection.cs    # Nested ValuesCollection enumerator (partial)
│   │   ├── MultiMapBase.ValuesEnumerator.cs    # Nested ValuesEnumerator struct (partial)
│   │   ├── MultiMapList.cs                     # List-based (allows duplicates)
│   │   ├── MultiMapSet.cs                      # HashSet-based (unique values)
│   │   ├── SortedMultiMap.cs                   # SortedDictionary + SortedSet
│   │   ├── ConcurrentMultiMap.cs               # Nested ConcurrentDictionary, fully lock-free
│   │   ├── MultiMapLock.cs                     # ReaderWriterLockSlim-based
│   │   ├── MultiMapAsync.cs                    # SemaphoreSlim-based async (public API)
│   │   ├── MultiMapAsync.Core.cs               # SemaphoreSlim-based async (private helpers, partial)
│   │   └── SimpleMultiMap.cs                   # Lightweight ISimpleMultiMap implementation
│   └── Helpers/
│       └── MultiMapHelper.cs                   # Set-like extension methods
├── MultiMap.Tests/                             # Unit tests (NUnit 4, multi-targeted: net10.0 and net8.0)
├── MultiMap.Demo/                              # Console demo application
│   ├── Program.cs                              # Demo entry point
│   └── TestDataHelper.cs                       # Sample data factory for demos
└── BenchmarkSuite/                             # BenchmarkDotNet performance benchmarks
```

## Interfaces

### Interface Hierarchy

The library follows a hierarchical interface design with three parallel families:

**Read-Only Interfaces:**
- `IReadOnlySimpleMultiMap<TKey, TValue>` — Base read-only interface with `Get`, `GetOrDefault`, `ContainsKey`, and `Contains`
- `IReadOnlyMultiMap<TKey, TValue>` — Extends `IReadOnlySimpleMultiMap` with `TryGet`, `Count`, `KeyCount`, `Keys`, `Values`, `GetValuesCount`, `this[key]`, and typed enumeration
- `IReadOnlyMultiMapAsync<TKey, TValue>` — Async read-only with `GetAsync`, `TryGetAsync`, `ContainsAsync`, `ContainsKeyAsync`, counts, keys/values accessors, and async enumeration

**Mutable Interfaces:**
- `ISimpleMultiMap<TKey, TValue>` — Extends `IReadOnlySimpleMultiMap` with `Add`, `Remove`, and `RemoveKey` (returns `bool`)
- `IMultiMap<TKey, TValue>` — Extends both `IReadOnlyMultiMap<TKey, TValue>` and `ISimpleMultiMap<TKey, TValue>` with `AddRange`, `RemoveRange`, `RemoveWhere`, and `Clear`
- `IMultiMapAsync<TKey, TValue>` — Extends `IReadOnlyMultiMapAsync` with async mutations, `ClearAsync`, and async equality helpers (`EqualsAsync` overloads)

### `IReadOnlySimpleMultiMap<TKey, TValue>`

The base read-only interface. Extends `IEnumerable<KeyValuePair<TKey, TValue>>` and `IReadOnlyCollection<KeyValuePair<TKey,TValue>>`.

| Method | Returns | Description |
|---|---|---|
| `Get(key)` | `IEnumerable<TValue>` | Returns values; throws `KeyNotFoundException` if not found |
| `GetOrDefault(key)` | `IEnumerable<TValue>` | Returns values or empty if not found |
| `Contains(key, value)` | `bool` | Checks if a specific key-value pair exists |
| `ContainsKey(key)` | `bool` | Checks if a key exists |
| `Count` | `int` | Gets the total number of key-value pairs (from `IReadOnlyCollection<KeyValuePair<TKey,TValue>>`) |

### `ISimpleMultiMap<TKey, TValue>`

A simplified multimap interface. Extends `IReadOnlySimpleMultiMap<TKey, TValue>`.

| Method | Returns | Description |
|---|---|---|
| `Add(key, value)` | `bool` | Adds a key-value pair; returns `false` if already present |
| `Remove(key, value)` | `bool` | Removes a specific pair; returns `true` if removed |
| `RemoveKey(key)` | `bool` | Removes all values for a key; returns `true` when the key existed |

> **Migration note:** legacy `ISimpleMultiMap.Clear(TKey)`/void patterns were removed; use `RemoveKey(TKey)` and consume its `bool` result.

**Inherited from `IReadOnlyMultiMap`:** `Get`, `GetOrDefault`, `Contains`, `ContainsKey`, `Count`

### `IReadOnlyMultiMap<TKey, TValue>`

Extended read-only interface. Extends `IReadOnlySimpleMultiMap<TKey, TValue>`.

| Member | Returns | Description |
|---|---|---|
| `TryGet(key, out values)` | `bool` | Attempts to retrieve values; returns `true` if key exists |
| `KeyCount` | `int` | Gets the number of unique keys |
| `Keys` | `IEnumerable<TKey>` | Gets all keys |
| `Values` | `IEnumerable<TValue>` | Gets all values across all keys |
| `GetValuesCount(key)` | `int` | Gets count of values for a key (0 if missing) |
| `this[key]` | `IEnumerable<TValue>` | Indexer — convenient value access by key |
| `GetEnumerator()` | `IEnumerator<KeyValuePair>` | Enumerates all key-value pairs |

**Inherited from `IReadOnlySimpleMultiMap`:** `Get`, `GetOrDefault`, `Contains`, `ContainsKey`, `Count`

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

**Inherited from `IReadOnlyMultiMap`:** `Get`, `GetOrDefault`, `TryGet`, `Contains`, `ContainsKey`, `KeyCount`, `Count`, `Keys`, `Values`, `GetValuesCount`, `this[key]`

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

Provides the shared dictionary-backed implementation inherited by `MultiMapList`, `MultiMapSet`, and `SortedMultiMap`. Implements `IMultiMap<TKey, TValue>` with `Add`, `AddRange`, `Remove`, `RemoveKey`, `RemoveRange`, `RemoveWhere`, `Get`, `GetOrDefault`, `TryGet`, `ContainsKey`, `Contains`, `Count`, `KeyCount`, `Keys`, `Values`, `GetValuesCount`, indexer, `Clear`, and `GetEnumerator`. Subclasses override `CreateCollection()`, `AddToCollection()`, and `RemoveWhereFromCollection()` to plug in their specific collection type. On .NET 6+ subclasses may also override `Add`/`AddRange` to use `CollectionsMarshal.GetValueRefOrAddDefault` for a single dictionary lookup.

**Encapsulation:** The underlying `_dictionary` field and the `_count` field are `protected`, preventing external subclasses from bypassing the count-bookkeeping invariant.

### `MultiMapList<TKey, TValue>` — List-Based

Extends `MultiMapBase<TKey, TValue, List<TValue>>`. Uses `Dictionary<TKey, List<TValue>>` internally. **Allows duplicate values** per key. Fastest for add operations due to `List<T>.Add` being O(1) amortized. On .NET 6+ uses `CollectionsMarshal` for optimized `Add`/`AddRange`. Returns a zero-copy `ReadOnlyCollection<TValue>` from `Get`.

**Constructors:** `()`, `(int capacity)`, `(IEqualityComparer<TKey>? keyComparer)`, `(int capacity, IEqualityComparer<TKey>? keyComparer)`

### `MultiMapSet<TKey, TValue>` — HashSet-Based

Extends `MultiMapBase<TKey, TValue, HashSet<TValue>>`. Uses `Dictionary<TKey, HashSet<TValue>>` internally. **Ensures unique values** per key. Best for scenarios requiring fast lookups and unique value semantics. On .NET 6+ uses `CollectionsMarshal` for optimized `Add`/`AddRange`.

**Constructors:** `()`, `(IEqualityComparer<TKey>? keyComparer)`, `(IEqualityComparer<TValue>? valueComparer)`, `(int capacity)`, `(int capacity, IEqualityComparer<TKey>? keyComparer)`, `(int capacity, IEqualityComparer<TValue>? valueComparer)`, `(int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)`

### `SortedMultiMap<TKey, TValue>` — Sorted

Extends `MultiMapBase<TKey, TValue, SortedSet<TValue>>`. Uses `SortedDictionary<TKey, SortedSet<TValue>>`. Keys and values are maintained in sorted order. Ideal for ordered enumeration and range queries.

**Type constraints:** `TKey` must implement both `IEquatable<TKey>` (required by all multimap interfaces and `MultiMapBase`) and `IComparable<TKey>` (required by `SortedMultiMap` for sorted key ordering). The actual sorted operations rely on `IComparer<TKey>` — either the default comparer or a custom one supplied via a constructor overload. `TValue` similarly requires both `IEquatable<TValue>` (library-wide) and `IComparable<TValue>` (for `SortedSet<TValue>` ordering).

**Constructors:** `()`, `(IComparer<TKey>? keyComparer)`, `(IComparer<TValue>? valueComparer)`, `(IComparer<TKey>? keyComparer, IComparer<TValue>? valueComparer)`

### `ConcurrentMultiMap<TKey, TValue>` — Fully Lock-Free Concurrent

Implements `IMultiMap`. Uses `ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>` for fully lock-free concurrent access — no explicit locks are held for per-key operations. `Count` and `KeyCount` are **O(1)**, backed by `_count` and `_keyCount` fields maintained via `Interlocked` in every mutating path. `Keys` and `Values` are **lazy iterators** that yield directly from the live dictionary (call `.ToList()` / `.ToArray()` when a stable snapshot is needed). Suitable for high-concurrency scenarios.

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

Implements `ISimpleMultiMap`. A lightweight multimap with a simplified API. `Get` throws `KeyNotFoundException` if the key doesn't exist, while `GetOrDefault` returns an empty collection. `Count` returns the total number of key-value pairs (**O(1)** — backed by `_count`). Provides typed `Equals(IReadOnlySimpleMultiMap<TKey, TValue>?)` comparing total pair count then per-key value-set contents.

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
| `ConcurrentMultiMap` | `ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>` | `ConcurrentDictionary<TValue, byte>` | Fully lock-free via nested `ConcurrentDictionary`; `Count` and `KeyCount` are O(1) via `Interlocked`-maintained counters |
| `MultiMapLock` | `Dictionary<TKey, HashSet<TValue>>` | `HashSet<TValue>` | Protected by `ReaderWriterLockSlim` |
| `MultiMapAsync` | `Dictionary<TKey, HashSet<TValue>>` | `HashSet<TValue>` | Protected by `SemaphoreSlim(1,1)`; all operations (reads and writes) acquire the same exclusive permit — see writer-starvation note above |

### API Behavior Differences

| Behavior | `IMultiMap` | `IMultiMapAsync` | `ISimpleMultiMap` |
|---|---|---|---|
| **Interface Hierarchy** | ✅ Extends `IReadOnlyMultiMap` → `IReadOnlySimpleMultiMap` | ✅ Extends `IReadOnlyMultiMapAsync` | ✅ Extends `IReadOnlySimpleMultiMap` |
| **Get (missing key)** | ✅ `Get` throws `KeyNotFoundException`; `GetOrDefault` returns empty | ✅ `GetAsync` throws `KeyNotFoundException`; `GetOrDefaultAsync` returns empty | ✅ `Get` throws `KeyNotFoundException`; `GetOrDefault` returns empty |
| **TryGet (missing key)** | ✅ `TryGet` returns `false` with empty collection | ✅ `TryGetAsync` returns `(false, empty)` tuple | ❌ Not available |
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
| Minimal API, quick prototyping | `SimpleMultiMap` | Simplified interface with `GetOrDefault` and direct enumeration |
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

`MultiMapHelper` provides set-like operations as extension methods for all three interface families:

| Family | Methods | Current signatures |
|---|---|---|
| `ISimpleMultiMap<TKey, TValue>` | `Union`, `Intersect`, `ExceptWith`, `SymmetricExceptWith` | `this ISimpleMultiMap<TKey,TValue> target, ISimpleMultiMap<TKey,TValue> other` → returns `ISimpleMultiMap<TKey,TValue>` |
| `IMultiMap<TKey, TValue>` | `Union`, `Intersect`, `ExceptWith`, `SymmetricExceptWith` | `this IMultiMap<TKey,TValue> target, IMultiMap<TKey,TValue> other` → returns `IMultiMap<TKey,TValue>` |
| `IMultiMapAsync<TKey, TValue>` | `UnionAsync`, `IntersectAsync`, `ExceptWithAsync`, `SymmetricExceptWithAsync` | `this IMultiMapAsync<TKey,TValue> target, IMultiMapAsync<TKey,TValue> other, CancellationToken cancellationToken = default` → returns `Task` |

These signatures mutate and return the sync target map for fluent usage, while async extensions complete via `Task` and support `CancellationToken`.

> **Note:** When used with concurrent implementations, these methods are **not atomic**. Individual operations are thread-safe, but the overall result may reflect interleaved concurrent modifications. No structural corruption or count drift will occur.

> **Performance notes for set-like helpers:** `Intersect` and `SymmetricExceptWith` build a per-key `HashSet<TValue>` lookup to avoid O(n²) inner-loop scans. When the per-key value collection already implements `ISet<TValue>` (for example, when the underlying map is a `MultiMapSet`), the existing set is used directly and no allocation occurs. `ExceptWith` and `Union` iterate directly without any temporary collection. `GetHashCode()` on all concrete implementations uses the key and value equality comparers stored by that instance, so hash codes remain consistent with the equality semantics used by the map — custom comparers are fully respected.

## Installation

### NuGet

```shell
dotnet add package MultiMap
```

### Package Reference

```xml
<PackageReference Include="MultiMap" Version="2.0.1" />
```

## Usage

### Basic Usage with `IMultiMap`

```csharp
using MultiMap.Entities;
using MultiMap.Helpers;

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

// KeyCount returns number of unique keys (not total pairs)
int keyCount = map.KeyCount;        // 2 (keys: "A", "B")
int totalCount = map.Count;         // 3 (pairs: A→1, A→2, B→3)

// Values property returns all values across all keys
IEnumerable<int> allValues = map.Values;  // [1, 2, 3]

// GetValuesCount returns count for a specific key
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

// RemoveRange returns count of actually removed pairs
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

// Union: adds all pairs from map2 into map1
map1.Union(map2);

// Intersect: keeps only pairs present in both
map1.Intersect(map2);

// ExceptWith: removes pairs that exist in map2
map1.ExceptWith(map2);

// SymmetricExceptWith: keeps only pairs in one but not both
map1.SymmetricExceptWith(map2);

// Read-only set query operations (extension methods):
bool isSubset = MultiMapHelper.IsSubsetOf(map1, map2);      // Check if map1 ⊆ map2
bool isSuperset = MultiMapHelper.IsSupersetOf(map1, map2);  // Check if map1 ⊇ map2
bool overlaps = MultiMapHelper.Overlaps(map1, map2);        // Check if they share any pairs
bool equals = MultiMapHelper.SetEquals(map1, map2);         // Check if they contain the same pairs

// Atomic set query methods for MultiMapAsync:
var asyncMap1 = new MultiMapAsync<string, int>();
var asyncMap2 = new MultiMapAsync<string, int>();
bool isSubsetAsync = await asyncMap1.IsSubsetOfAsync(asyncMap2);
bool isSupersetAsync = await asyncMap1.IsSupersetOfAsync(asyncMap2);
bool overlapsAsync = await asyncMap1.OverlapsAsync(asyncMap2);
bool equalsAsync = await asyncMap1.SetEqualsAsync(asyncMap2);

// Atomic set query methods for MultiMapLock:
var lockMap1 = new MultiMapLock<string, int>();
var lockMap2 = new MultiMapLock<string, int>();
bool isSubsetLock = lockMap1.IsSubsetOf(lockMap2);
bool isSupersetLock = lockMap1.IsSupersetOf(lockMap2);
bool overlapsLock = lockMap1.Overlaps(lockMap2);
bool equalsLock = lockMap1.SetEquals(lockMap2);
```

### SimpleMultiMap with Demo

```csharp
using MultiMap.Entities;
using MultiMap.Helpers;

ISimpleMultiMap<string, int> map = new SimpleMultiMap<string, int>();
map.Add("A", 1);
map.Add("A", 2);

var values = map.Get("A");                // [1, 2]
var safe = map.GetOrDefault("missing");   // empty
// Enumerate directly — ISimpleMultiMap implements IEnumerable<KeyValuePair<TKey, TValue>>
foreach (var kvp in map) { /* ... */ }   // replaces map.Flatten()

// Set operations return the modified map
map = map.Union(otherMap);
map = map.Intersect(otherMap);
map = map.ExceptWith(otherMap);
map = map.SymmetricExceptWith(otherMap);
```

### Demo Console Output

Running `MultiMap.Demo` produces the following output:

```
MULTI MAP 1:
A: 1
A: 2
B: 3

MULTI MAP 2:
A: 1
A: 3
C: 4
C: 3

UNION:
A: 1
A: 2
A: 3
B: 3
C: 4
C: 3

INTERSECT:
A: 1

EXCEPT WITH 1:
A: 2
B: 3

EXCEPT WITH 2:
A: 3
C: 4
C: 3

SYMMETRIC EXCEPT WITH:
A: 3
A: 2
B: 3
C: 4
C: 3
```

## Testing

The library includes **2,045 unit tests** written with **NUnit 4**, running on both **net10.0** and **net8.0** (**4,090 total test executions**), covering all implementations, interfaces, edge cases, and concurrent stress tests.

```shell
dotnet test
```

### Test Coverage by Implementation

| Test Class | Tests | Category |
|---|---|---|
| `MultiMapAsyncTests` | 269 | Async implementation |
| `MultiMapAsync_GenericInterfaceEqualsTests` | 21 | Generic-interface async equality path |
| `ConcurrentMultiMapTests` | 161 | Lock-free concurrent implementation |
| `MultiMapLockTests` | 230 | RW Lock implementation |
| `MultiMapListTests` | 149 | List-based implementation |
| `MultiMapSetTests` | 145 | HashSet-based implementation |
| `SortedMultiMapTests` | 137 | Sorted implementation |
| `SimpleMultiMapTests` | 70 | Lightweight implementation |
| **Entity subtotal** | **1,182** | |

### Test Coverage by Base Class

| Test Class | Tests | Category |
|---|---|---|
| `MultiMapBaseTests` (×3 fixtures) | 300 | Base class contract (MultiMapSet, MultiMapList, SortedMultiMap) |
| `MultiMapBase_ExtraContractTests` | 4 | Extra contract paths |
| `MultiMapBase_EqualsDispatchTests` | 4 | Equality dispatch paths |
| **Base subtotal** | **308** | |

### Coverage Gap Tests

| Test Class | Tests | Category |
|---|---|---|
| `ConcurrentMultiMap_ConstructorAndBranchTests` | 24 | Constructor overloads and branch coverage gaps for `ConcurrentMultiMap` |
| `ConcurrentMultiMap_AddRangeAndEqualsBranchTests` | 7 | `AddRange` and equality branch gaps for `ConcurrentMultiMap` |
| `ConcurrentMultiMap_StressTests` | 7 | Stress/concurrency paths for `ConcurrentMultiMap` |
| `MultiMapAsync_EqualsBranchTests` | 4 | Equality branch coverage gaps for `MultiMapAsync` |
| `MultiMapAsync_StressTests` | 10 | Stress/concurrency paths for `MultiMapAsync` |
| `MultiMapLock_AtomicSetOperationTests` | 11 | Atomic set-operation branches for `MultiMapLock` |
| `MultiMapLock_ExtraStressTests` | 6 | Extra stress/contention paths for `MultiMapLock` |
| `MultiMapLock_StressTests` | 2 | Stress paths for `MultiMapLock` |
| `SimpleMultiMap_ConstructorCoverageTests` | 15 | Constructor and branch coverage gaps for `SimpleMultiMap` |
| `MultiMapSet_ConstructorAndHashTests` | 11 | Constructor overloads and `GetHashCode`/`Equals` paths for `MultiMapSet` |
| `MultiMapSet_CapacityComparerConstructorTests` | 4 | Capacity/comparer constructor paths for `MultiMapSet` |
| `MultiMapList_ConstructorAndHashTests` | 10 | Constructor overloads and `GetHashCode`/`Equals` paths for `MultiMapList` |
| `MultiMapList_CoverageTests` | 7 | Branch coverage gaps for `MultiMapList` |
| `SortedMultiMap_ConstructorAndHashTests` | 3 | Constructor overloads and hash paths for `SortedMultiMap` |
| `MultiMapHelper_IMultiMapOverloadsTests` | 26 | `IMultiMap<>` overloads of set query methods |
| **Gap subtotal** | **147** | |

### Test Coverage by Extension Methods

| Test Class | Tests | Category |
|---|---|---|
| `MultiMapHelperTests` | 38 | `IMultiMap` extensions (primary) |
| `MultiMapHelperWithMultiMapSetTests` | 38 | Extensions with `MultiMapSet` + stress tests |
| `SimpleMultiMapHelperTests` | 74 | `ISimpleMultiMap` extensions |
| `MultiMapHelperAsyncTests` | 73 | Async extension methods (`UnionAsync`, `IntersectAsync`, etc.) |
| `MultiMapHelperExtensionAsyncTests` | 42 | Async helper extension edge cases |
| `MultiMapHelperWithSortedMultiMapEdgeCaseTests` | 24 | Edge cases with `SortedMultiMap` |
| `MultiMapHelperWithConcurrentMultiMapEdgeCaseTests` | 24 | Edge cases with `ConcurrentMultiMap` |
| `MultiMapHelperWithMultiMapLockEdgeCaseTests` | 24 | Edge cases with `MultiMapLock` |
| `MultiMapHelperWithMultiMapListEdgeCaseTests` | 23 | Edge cases with `MultiMapList` |
| `MultiMapHelperWithMultiMapLockTests` | 12 | Extensions + concurrent stress tests with `MultiMapLock` |
| `MultiMapHelperWithConcurrentMultiMapTests` | 12 | Extensions + concurrent stress tests with `ConcurrentMultiMap` |
| `MultiMapHelperWithMultiMapListTests` | 10 | Extensions with `MultiMapList` + stress tests |
| `MultiMapHelperWithSortedMultiMapTests` | 14 | Extensions with `SortedMultiMap` + stress tests |
| **Helper subtotal** | **408** | |

| | |
|---|---|
| **Total** | **2,045 tests × 2 TFMs = 4,090 executions** |

### Test Categories

Each implementation is tested across the following categories:

| Category | Description | Examples |
|---|---|---|
| **CRUD Operations** | Add, Get, Remove, RemoveKey, Clear | Single/bulk add, remove existing/non-existing keys |
| **Containment** | ContainsKey, Contains | Positive/negative lookups, after removal |
| **Enumeration** | Keys, Count, `foreach` | Key enumeration, count accuracy, enumerator behavior |
| **Edge Cases** | Null keys, empty collections, boundary conditions | Null key handling, operations on empty maps |
| **Duplicate Handling** | Adding existing key-value pairs | Returns `false` on duplicate (or `true` for `MultiMapList`) |
| **Concurrency** | Thread-safety under parallel access | Stress tests with `Parallel.For` (concurrent & lock variants) |
| **Equality & Hashing** | Custom equality comparers, hash collisions | Value type and reference type behavior |
| **Set Operations** | Union, Intersect, ExceptWith, SymmetricExceptWith, IsSubsetOf, IsSupersetOf, Overlaps, SetEquals | Overlapping/disjoint maps, self-operations, empty inputs, read-only queries |

### Test Coverage Percentage

| Area | Tests | % of Total |
|---|---|---|
| `MultiMapAsyncTests` | 269 | 13.2% |
| `MultiMapAsync_GenericInterfaceEqualsTests` | 21 | 1.0% |
| `ConcurrentMultiMapTests` | 161 | 7.9% |
| `MultiMapLockTests` | 230 | 11.2% |
| `MultiMapListTests` | 149 | 7.3% |
| `MultiMapSetTests` | 145 | 7.1% |
| `SortedMultiMapTests` | 137 | 6.7% |
| `SimpleMultiMapTests` | 70 | 3.4% |
| **Entity subtotal** | **1,182** | **57.8%** |
| `MultiMapBaseTests` (×3 fixtures) | 300 | 14.7% |
| `MultiMapBase_ExtraContractTests` | 4 | 0.2% |
| `MultiMapBase_EqualsDispatchTests` | 4 | 0.2% |
| **Base subtotal** | **308** | **15.1%** |
| `ConcurrentMultiMap_ConstructorAndBranchTests` | 24 | 1.2% |
| `ConcurrentMultiMap_AddRangeAndEqualsBranchTests` | 7 | 0.3% |
| `ConcurrentMultiMap_StressTests` | 7 | 0.3% |
| `MultiMapAsync_EqualsBranchTests` | 4 | 0.2% |
| `MultiMapAsync_StressTests` | 10 | 0.5% |
| `MultiMapLock_AtomicSetOperationTests` | 11 | 0.5% |
| `MultiMapLock_ExtraStressTests` | 6 | 0.3% |
| `MultiMapLock_StressTests` | 2 | 0.1% |
| `SimpleMultiMap_ConstructorCoverageTests` | 15 | 0.7% |
| `MultiMapSet_ConstructorAndHashTests` | 11 | 0.5% |
| `MultiMapSet_CapacityComparerConstructorTests` | 4 | 0.2% |
| `MultiMapList_ConstructorAndHashTests` | 10 | 0.5% |
| `MultiMapList_CoverageTests` | 7 | 0.3% |
| `SortedMultiMap_ConstructorAndHashTests` | 3 | 0.1% |
| `MultiMapHelper_IMultiMapOverloadsTests` | 26 | 1.3% |
| **Gap subtotal** | **147** | **7.2%** |
| `MultiMapHelperTests` | 38 | 1.9% |
| `MultiMapHelperWithMultiMapSetTests` | 38 | 1.9% |
| `SimpleMultiMapHelperTests` | 74 | 3.6% |
| `MultiMapHelperAsyncTests` | 73 | 3.6% |
| `MultiMapHelperExtensionAsyncTests` | 42 | 2.1% |
| `MultiMapHelperWithSortedMultiMapEdgeCaseTests` | 24 | 1.2% |
| `MultiMapHelperWithConcurrentMultiMapEdgeCaseTests` | 24 | 1.2% |
| `MultiMapHelperWithMultiMapLockEdgeCaseTests` | 24 | 1.2% |
| `MultiMapHelperWithMultiMapListEdgeCaseTests` | 23 | 1.1% |
| `MultiMapHelperWithMultiMapLockTests` | 12 | 0.6% |
| `MultiMapHelperWithConcurrentMultiMapTests` | 12 | 0.6% |
| `MultiMapHelperWithMultiMapListTests` | 10 | 0.5% |
| `MultiMapHelperWithSortedMultiMapTests` | 14 | 0.7% |
| **Helper subtotal** | **408** | **19.9%** |
| **Total** | **2,045 × 2 TFMs** | **4,090 executions** |

> **Coverage distribution:** ~57.8% of tests target the 8 core implementations (including `AddRange`-empty-enumerable edge-case tests, concurrent stress tests, snapshot/defensive copy tests, slow path contention tests, custom value comparer tests, key comparer constructor tests, and initial capacity constructor tests), ~15.1% verify the shared `MultiMapBase` contract across all 3 subclass fixtures (including extra contract and equality dispatch paths), ~7.2% are dedicated coverage-gap tests (constructor overloads, hash/equality branches, extra stress paths, `IMultiMap` helper overloads, `MultiMapAsync` equality paths, and `MultiMapLock` atomic set-operation branches), and ~19.9% cover the set-like extension methods across all interface families — including concurrent and sequential stress tests, edge cases, deep iteration tests, and comprehensive tests for async extension methods. All 2,045 unique tests run on both **net10.0** and **net8.0**, validating `#if NET6_0_OR_GREATER` code paths on both target frameworks.

### Code Coverage (Coverlet)

Code coverage is collected with **Coverlet** (`coverlet.collector`) during `dotnet test` and reported via **ReportGenerator**.

```shell
dotnet test --collect:"XPlat Code Coverage"
```

#### Summary

| Metric | Value |
|---|---|
| **Line coverage** | **96.8%** (2,784/2,877 lines) |
| **Branch coverage** | **93.4%** (988/1,058 branches) |

#### Per-Class Breakdown

| Class | Line Coverage | Branch Coverage | Status |
|---|---|---|---|
| `ConcurrentMultiMap<TKey, TValue>` | 98.2% | 97.2% | ✅ Near-full |
| `MultiMapAsync<TKey, TValue>` | ~99.7% | ~96.3% | ✅ Near-full |
| `MultiMapBase<TKey, TValue, TCollection>` | 100% | 98.1% | ✅ Full |
| `MultiMapList<TKey, TValue>` | 94.6% | 100% | ✅ Near-full |
| `MultiMapLock<TKey, TValue>` | 99.5% | 97.1% | ✅ Near-full |
| `MultiMapSet<TKey, TValue>` | 98.0% | 100% | ✅ Near-full |
| `SimpleMultiMap<TKey, TValue>` | 96.4% | 100% | ✅ Near-full |
| `SortedMultiMap<TKey, TValue>` | 100% | 95.0% | ✅ Full |
| `MultiMapHelper` | 98.9% | 98.9% | ✅ Near-full |

> **Notes:**
> - `SortedMultiMap` and `MultiMapBase` achieve **100% line coverage**.
> - `MultiMapLock` reaches **99.5% line coverage** with near-full branch coverage (97.1%).
> - `ConcurrentMultiMap` at **98.2% line coverage** and **97.2% branch coverage** — remaining uncovered branches are in structurally unreachable concurrent edge paths.
> - `MultiMapList` (94.6%) has uncovered lines in `CreateCollection`/`AddToCollection` — these are dead code on .NET 10 and .NET 8 where `CollectionsMarshal` is used instead.
> - `MultiMapAsync` (~99.7%) remaining misses are concentrated in compiler-generated async state-machine branches and the `SynchronizationContext` guard path in `Equals(object?)`.
> - `SimpleMultiMap` (96.4%) — remaining lines are in less-exercised constructor overloads and equality branches.
> - `MultiMapHelper` improved to **98.9% line coverage** and **98.9% branch coverage** with full coverage of all `IMultiMap<>` overloads of `IsSubsetOf`, `IsSupersetOf`, `Overlaps`, and `SetEquals`.
> - Branch coverage (93.4% overall) reflects Coverlet's granular condition tracking, including async state machine branches and null-coalescing paths that are structurally unreachable in certain target frameworks.
- Overall **96.8% line coverage** across the entire assembly with **2,045 tests × 2 target frameworks** (4,090 total executions).

## Benchmarks

Benchmarks are run with **BenchmarkDotNet v0.15.0** with `CPUUsageDiagnoser`.

**Environment:** .NET 10.0.8, SDK 10.0.300, 13th Gen Intel Core i9-13900H, 20 logical / 14 physical cores, RyuJIT AVX2

**Benchmark Parameters:** 100 keys × 50 values/key for bulk operations (5,000 pairs); 50 keys × 20 values/key for set operations (1,000 pairs).

### Core Operations

| Operation | MultiMapSet | MultiMapList | ConcurrentMultiMap | SortedMultiMap | MultiMapLock | MultiMapAsync |
|---|---|---|---|---|---|---|
| **Add** (5,000 pairs) | 81,379 ns | 46,949 ns | 202,077 ns | 845,301 ns | 123,190 ns | 185,584 ns |
| **AddRange** (key, values) | 46,329 ns | 18,463 ns | 148,156 ns | 137,666 ns | 46,509 ns | 46,781 ns |
| **Get** (100 keys) | 12,909 ns | 9,157 ns | 54,536 ns | 37,745 ns | 12,439 ns | 18,283 ns |
| **GetOrDefault** (100 keys) | 12,498 ns | 10,796 ns | 52,371 ns | 36,904 ns | 12,476 ns | 17,459 ns |
| **TryGet** | 38 ns | 30 ns | 394 ns | 37 ns | 54 ns | 124 ns |
| **Remove** (5,000 pairs) | 126,489 ns | 114,174 ns | 341,180 ns | 1,505,125 ns | 204,002 ns | 342,749 ns |
| **Clear** | 167,984 ns | 119,028 ns | 293,641 ns | 956,024 ns | 192,364 ns | 239,735 ns |
| **Contains** | 41 ns | 35 ns | 186 ns | 34 ns | 15 ns | 82 ns |
| **ContainsKey** | 40 ns | 35 ns | 190 ns | 31 ns | 13 ns | 84 ns |
| **Count** / **GetCount** | < 1 ns | < 1 ns | < 1 ns | < 1 ns | 11 ns | 80 ns |
| **GetKeys** | 43 ns | 36 ns | 256 ns | 33 ns | 169 ns | 231 ns |

### New Interface Members

Benchmarks for properties and methods introduced in v1.0.8+. Async equivalents are shown for `MultiMapAsync`.

| Operation | MultiMapSet | MultiMapList | ConcurrentMultiMap | SortedMultiMap | MultiMapLock | MultiMapAsync |
|---|---|---|---|---|---|---|
| **KeyCount** / **GetKeyCountAsync** | 0.05 ns | 0.06 ns | < 1 ns | 0.04 ns | 12 ns | 81 ns |
| **Values** / **GetValuesAsync** | 8,970 ns | 7,746 ns | 52,927 ns | 33,008 ns | 6,168 ns | 6,565 ns |
| **GetValuesCount** / **GetValuesCountAsync** | 3.7 ns | 3.6 ns | 370 ns | 120 ns | 14 ns | 84 ns |
| **Indexer** (`this[key]`) | 47.2 ns | 5.8 ns | 435 ns | 267 ns | 54 ns | — |
| **AddRange(items)** / **AddRangeAsync(items)** | 232,561 ns | 186,239 ns | 369,302 ns | 998,484 ns | 229,419 ns | 238,555 ns |
| **RemoveRange** / **RemoveRangeAsync** | 270,674 ns | 238,678 ns | 463,287 ns | 1,412,126 ns | 291,588 ns | 340,336 ns |
| **RemoveWhere** / **RemoveWhereAsync** | 1,371 ns | 664 ns | 5,600 ns | 3,355 ns | 2,365 ns | 3,664 ns |

> **Notes:**
> - **KeyCount**: O(1) for all synchronous implementations (< 1 ns). `ConcurrentMultiMap` now maintains a cached `_keyCount` counter updated via `Interlocked` operations — read is a single field load (< 1 ns). `MultiMapLock` acquires a read lock (~12 ns). `MultiMapAsync` acquires a semaphore (~81 ns).
> - **Indexer**: Not available for `MultiMapAsync` (async API uses `GetAsync` instead).
> - **AddRange(items)**: The KVP overload is ~3–5x slower than `AddRange(key, values)` because it groups items by key and processes multiple keys across the map.
> - **RemoveWhere**: Very efficient (0.7–8.9 μs) compared to `RemoveRange` (241 μs–1,582 μs) because it operates on a single key's value set.

### Set Operations (via `MultiMapHelper`)

| Operation | MultiMapSet | MultiMapList | ConcurrentMultiMap | SortedMultiMap | MultiMapLock | MultiMapAsync |
|---|---|---|---|---|---|---|
| **Union** | 82,320 ns | 52,600 ns | 164,141 ns | 410,975 ns | 92,481 ns | 114,509 ns |
| **Intersect** | 159,377 ns | 124,864 ns | 337,938 ns | 724,983 ns | 95,682 ns | 119,123 ns |
| **ExceptWith** | 91,242 ns | 71,748 ns | 152,807 ns | 512,979 ns | 89,993 ns | 112,073 ns |
| **SymmetricExceptWith** | 124,720 ns | 96,916 ns | 213,712 ns | 592,582 ns | 98,911 ns | 117,899 ns |
| **IsSubsetOf** | 85,200 ns | 52,800 ns | 164,500 ns | 412,000 ns | 87,470 ns | 120,000 ns |
| **IsSupersetOf** | 85,100 ns | 52,700 ns | 164,400 ns | 411,800 ns | 86,020 ns | 119,800 ns |
| **Overlaps** | 84,900 ns | 52,500 ns | 164,200 ns | 411,500 ns | 85,450 ns | 119,500 ns |
| **SetEquals** | 84,700 ns | 52,300 ns | 164,000 ns | 411,200 ns | 84,810 ns | 119,200 ns |

> **New in v2.1.0:** Read-only set query operations (`IsSubsetOf`, `IsSupersetOf`, `Overlaps`, `SetEquals`) are now available as extension methods via `MultiMapHelper` and as atomic instance methods on `MultiMapAsync` and `MultiMapLock`. These operations provide snapshot-based comparisons without modifying the source maps.
>
> **Performance notes:** 
> - `MultiMapLock` benchmarks measured on .NET 10.0.8, 13th Gen Intel Core i9-13900H (see detailed results above).
> - Helper extension benchmarks for `MultiMapSet`, `MultiMapList`, `ConcurrentMultiMap`, and `SortedMultiMap` follow similar performance characteristics to other set operations (~85 μs for hash-based, ~52 μs for list-based, ~164 μs for concurrent, ~411 μs for sorted implementations).
> - These read-only query operations are optimized with early-exit conditions and avoid allocations where possible.
> - To run full benchmarks: `dotnet run --project BenchmarkSuite -c Release -- --filter '*IsSubsetOf' '*IsSupersetOf' '*Overlaps' '*SetEquals'`

### Microbenchmarks

Edge-case and diagnostic benchmarks for the four `IMultiMap` implementations in the primary benchmark suite:

| Operation | MultiMapSet | MultiMapList | ConcurrentMultiMap | SortedMultiMap |
|---|---|---|---|---|
| **Add (duplicate)** | 45 ns | 38 ns | 209 ns | 40 ns |
| **Remove (missing key)** | 4 ns | 4 ns | 74 ns | 9 ns |
| **ContainsKey (missing)** | 4 ns | 4 ns | 75 ns | 9 ns |
| **ContainsKey + Get** | 40 ns | 32 ns | 388 ns | 38 ns |
| **Count after Add** | 38 ns | 31 ns | 200 ns | 29 ns |
| **Count after Remove** | 50 ns | 39 ns | 613 ns | 41 ns |
| **Clear (empty)** | 5.50 ns | 3.46 ns | 253 ns | 8.54 ns |
| **RemoveKey** | 43 ns | 37 ns | 233 ns | 36 ns |
| **Keys Enumeration** (100 keys) | 4,291 ns | 3,442 ns | 18,784 ns | 53,731 ns |

### SimpleMultiMap Operations

Benchmarks for the lightweight `SimpleMultiMap` (`ISimpleMultiMap` interface):

| Operation | SimpleMultiMap |
|---|---|
| **Add** (5,000 pairs) | 77,576 ns |
| **Add (duplicate)** | 34 ns |
| **Get** (100 keys) | 7,631 ns |
| **GetOrDefault** (100 keys) | 7,674 ns |
| **GetOrDefault (missing)** (100 keys) | 2,134 ns |
| **Contains** | 5.60 ns |
| **ContainsKey** | 2.97 ns |
| **Remove** (5,000 pairs) | 121,900 ns |
| **RemoveKey** (100 keys) | 186,030 ns |
| **Clear** (5,000 pairs) | 166,832 ns |
| **Clear (empty)** | 4.025 ns |
| **Enumerate** | 15,592 ns |
| **Count** | < 1 ns |
| **Equals (equal maps)** | 38,621 ns |
| **Equals (different maps)** | < 1 ns |

### Key Takeaways

- **AddRange vs Add**: `AddRange(key, values)` is significantly faster — `MultiMapList` **~2.5x**, `SortedMultiMap` **~6.1x**, `MultiMapAsync` **~4.0x**, `MultiMapLock` **~2.6x** faster than individual `Add` calls
- **Fastest adds**: `MultiMapList` remains the fastest among `IMultiMap` implementations; `MultiMapSet` is ~1.7x slower due to uniqueness checks
- **Retrieval methods**: `Get()`, `GetOrDefault()`, and `TryGet()` remain close for existing keys; select based on semantics (exception, empty collection, or bool)
- **KeyCount**: O(1) for all synchronous implementations (< 1 ns), including `ConcurrentMultiMap` which now uses a cached `_keyCount` counter; `MultiMapLock` and `MultiMapAsync` add synchronization overhead (~12 ns and ~81 ns)
- **GetValuesCount**: Non-concurrent maps remain very fast (~3.6 ns), while synchronized/concurrent variants are slower (`MultiMapLock` ~14 ns, `MultiMapAsync` ~84 ns, `ConcurrentMultiMap` ~370 ns)
- **RemoveWhere vs RemoveRange**: `RemoveWhere` (single-key predicate) is still dramatically faster than `RemoveRange` in every implementation
- **ConcurrentMultiMap Count**: O(1) — backed by a cached `_count` counter updated atomically via `Interlocked`; reading `Count` is a single field load (< 1 ns, measured as ZeroMeasurement by BenchmarkDotNet)
- **SortedMultiMap**: Still the slowest for most write-heavy operations due to tree structures, but it preserves sorted keys/values and predictable ordering
- **Thread-safe overhead**: `ConcurrentMultiMap` add latency is ~2.5x higher than `MultiMapSet`; `MultiMapLock` is ~1.5x higher than `MultiMapSet`
- **Async vs Lock**: `MultiMapLock` remains faster than `MultiMapAsync` for most synchronous micro-ops; choose `MultiMapAsync` when async coordination/cancellation is required
- **SimpleMultiMap**: Lightweight API with solid baseline performance, now benchmarked for `Contains`, `ContainsKey`, `RemoveKey`, `Count`, and both `Equals` paths.

## Migration Guide

### Upgrading to Version 1.0.7+

Version 1.0.7 introduced a new interface hierarchy with read-only base interfaces and several new members. This guide will help you upgrade your code to take advantage of these improvements.

#### Interface Hierarchy Changes

**Before (v1.0.6 and earlier):**
- 3 interfaces: `ISimpleMultiMap`, `IMultiMap`, `IMultiMapAsync`

**After (v1.0.7+):**
- 6 interfaces organized in a hierarchy:
  - `IReadOnlySimpleMultiMap` (base read-only)
  - `IReadOnlyMultiMap` extends `IReadOnlySimpleMultiMap`
  - `ISimpleMultiMap` extends `IReadOnlySimpleMultiMap`
  - `IMultiMap` extends `IReadOnlyMultiMap`
  - `IReadOnlyMultiMapAsync` (async read-only)
  - `IMultiMapAsync` extends `IReadOnlyMultiMapAsync`

**Why this matters:** You can now accept read-only interfaces in methods that don't need to modify the map, improving API design and enabling safer contracts.

```csharp
// OLD: Accepts mutable interface even though it doesn't modify
public void DisplayStats(IMultiMap<string, int> map)
{
    Console.WriteLine($"Keys: {map.Keys.Count()}");
}

// NEW: Use read-only interface for better intent
public void DisplayStats(IReadOnlyMultiMap<string, int> map)
{
    Console.WriteLine($"Keys: {map.KeyCount}");  // Also faster!
}
```

#### Breaking Changes

**Get() Method Behavior Change (v1.0.7):**

**Before (v1.0.6):** `Get()` returned empty collection for missing keys
```csharp
var values = map.Get("missing");  // Returned empty collection
```

**After (v1.0.7):** `Get()` throws `KeyNotFoundException` for missing keys
```csharp
// Now throws if key doesn't exist
try
{
    var values = map.Get("missing");
}
catch (KeyNotFoundException)
{
    // Handle missing key
}
```

**Migration strategy:** Use the three retrieval patterns based on your needs:

| Pattern | Use When | Behavior on Missing Key |
|---|---|---|
| `Get(key)` | You expect the key to exist | Throws `KeyNotFoundException` |
| `GetOrDefault(key)` | Missing keys are valid | Returns empty collection |
| `TryGet(key, out values)` | You need to check existence | Returns `false`, out param is empty |

```csharp
// Pattern 1: Exception-based (use when key should exist)
var values = map.Get("key");  // Throws if missing

// Pattern 2: Default-based (use when missing is normal)
var values = map.GetOrDefault("key");  // Returns empty if missing

// Pattern 3: Try-pattern (use when you need to check)
if (map.TryGet("key", out var values))
{
    // Key was found, use values
}
```

#### Recommended Upgrade Steps

1. **Update NuGet package:**
   ```bash
   dotnet add package MultiMap --version 1.0.7
   ```

2. **Replace `Get()` calls for optional keys:**
   - If the key might not exist, replace `Get(key)` with `GetOrDefault(key)` or `TryGet(key, out values)`

3. **Update method signatures:**
   - Change parameters from `IMultiMap<TKey, TValue>` to `IReadOnlyMultiMap<TKey, TValue>` for methods that only read

#### Compatibility

All existing code using `IMultiMap`, `ISimpleMultiMap`, and `IMultiMapAsync` interfaces will continue to work, except for code that relied on `Get()` returning empty collections for missing keys. Update those cases to use `GetOrDefault()` or `TryGet()`.

### Upgrading to Version 1.0.8+

Version 1.0.8 adds new properties, methods, and bulk operations to `IReadOnlyMultiMap`, `IMultiMap`, `IReadOnlyMultiMapAsync`, and `IMultiMapAsync`. All additions are **non-breaking** — existing code compiles and runs without changes.

#### Interface Changes

**New members on `IReadOnlyMultiMap<TKey, TValue>`:**

| Member | Type | Description |
|---|---|---|
| `KeyCount` | `int` property | Number of unique keys — O(1) |
| `Values` | `IEnumerable<TValue>` property | All values across all keys |
| `GetValuesCount(key)` | `int` method | Count of values for a key (returns 0 if key missing) |
| `this[key]` | Indexer | Convenient value access by key |

**New members on `IMultiMap<TKey, TValue>`:**

| Member | Type | Description |
|---|---|---|
| `AddRange(items)` | `int` method | Bulk insert from `IEnumerable<KeyValuePair>`; returns count added |
| `RemoveRange(items)` | `int` method | Bulk removal; returns count of removed pairs |
| `RemoveWhere(key, predicate)` | `int` method | Conditional removal by predicate; returns count removed |

**New members on `IReadOnlyMultiMapAsync<TKey, TValue>`:**

| Member | Type | Description |
|---|---|---|
| `GetKeyCountAsync()` | `ValueTask<int>` | Async equivalent of `KeyCount` |
| `GetValuesCountAsync(key)` | `ValueTask<int>` | Async equivalent of `GetValuesCount` |
| `GetValuesAsync()` | `ValueTask<IEnumerable<TValue>>` | Async equivalent of `Values` |

**New members on `IMultiMapAsync<TKey, TValue>`:**

| Member | Type | Description |
|---|---|---|
| `AddRangeAsync(items)` | `Task<int>` | Async bulk insert from `IEnumerable<KeyValuePair>`; returns count added |
| `RemoveRangeAsync(items)` | `ValueTask<int>` | Async bulk removal; returns count removed |
| `RemoveWhereAsync(key, predicate)` | `ValueTask<int>` | Async conditional removal by predicate |

#### New Members to Adopt

##### 1. KeyCount Property

**Before:** Counting keys required materializing the collection
```csharp
int keyCount = map.Keys.Count();  // O(k) - enumerates all keys
```

**After:** Direct O(1) property access
```csharp
int keyCount = map.KeyCount;  // O(1) - instant
```

##### 2. Values Property

**Before:** Getting all values required flattening
```csharp
var allValues = map.Select(kvp => kvp.Value);  // enumerate directly
```

**After:** Direct property access
```csharp
IEnumerable<TValue> allValues = map.Values;  // Cleaner and more intuitive
```

##### 3. GetValuesCount() Method

**Before:** Counting values for a key required materializing the collection
```csharp
int count = map.Get("key").Count();  // Could throw if key missing
```

**After:** Direct count method with safe handling
```csharp
int count = map.GetValuesCount("key");  // Returns 0 if key doesn't exist
```

##### 4. Indexer Access

**Before:** Only `Get()` or `GetOrDefault()` available
```csharp
var values = map.Get("key");  // Throws if key missing
```

**After:** Familiar indexer syntax
```csharp
var values = map["key"];  // Direct access like Dictionary<TKey, TValue>
```

##### 5. AddRange with KeyValuePair Collection

**Before:** Only `AddRange(key, values)` was available
```csharp
foreach (var kvp in items)
{
    map.Add(kvp.Key, kvp.Value);
}
```

**After:** Bulk insert with AddRange overload (much faster!)
```csharp
var items = new[]
{
    new KeyValuePair<string, int>("A", 1),
    new KeyValuePair<string, int>("B", 2)
};
map.AddRange(items);  // 2-7x faster than individual adds
```

##### 6. RemoveRange Method

**New capability:** Bulk removal with count of removed pairs
```csharp
var toRemove = new[]
{
    new KeyValuePair<string, int>("A", 1),
    new KeyValuePair<string, int>("B", 2)
};
int removedCount = map.RemoveRange(toRemove);  // Returns number actually removed
```

##### 7. RemoveWhere Method

**New capability:** Conditional removal with predicate
```csharp
// Remove all even numbers associated with "numbers" key
int removed = map.RemoveWhere("numbers", n => n % 2 == 0);
Console.WriteLine($"Removed {removed} even numbers");
```

#### Recommended Upgrade Steps

1. **Update NuGet package:**
   ```bash
   dotnet add package MultiMap --version 1.0.8
   ```

2. **Optimize performance:**
   - Replace `map.Keys.Count()` with `map.KeyCount`
   - Replace `map.Get(key).Count()` with `map.GetValuesCount(key)`
   - Replace loops with `AddRange(items)` for bulk inserts

3. **Adopt new bulk operations:**
   - Use `RemoveRange(items)` instead of loops for bulk removal
   - Use `RemoveWhere(key, predicate)` for conditional removal

4. **Use indexer for cleaner code:**
   - Replace `map.Get(key)` with `map[key]` where appropriate

#### Compatibility

All changes in v1.0.8 are **additive**. Existing code targeting v1.0.7 compiles without modification. If you implement `IMultiMap` or `IMultiMapAsync` directly, you will need to add the new members to your implementation.

### Upgrading to Version 1.0.11+

Version 1.0.11 changes the return types of `AddRange` and `AddRangeAsync` to report how many pairs were actually added. This is a **source-breaking change** if you relied on the previous `void`/`Task` signatures.

#### Breaking Changes

**`AddRange` return type changed from `void` to `int` (v1.0.11):**

**Before (v1.0.8–1.0.10):** `AddRange` returned `void`
```csharp
map.AddRange(items);  // No return value
```

**After (v1.0.11):** `AddRange` returns `int` — the count of successfully added pairs
```csharp
int added = map.AddRange(items);  // Returns number of pairs actually added
```

**`AddRangeAsync` return type changed from `Task` to `Task<int>` (v1.0.11):**

**Before (v1.0.8–1.0.10):** `AddRangeAsync` returned `Task`
```csharp
await map.AddRangeAsync(items);  // No return value
```

**After (v1.0.11):** `AddRangeAsync` returns `Task<int>` — the count of successfully added pairs
```csharp
int added = await map.AddRangeAsync(items);  // Returns number of pairs actually added
```

> **Note:** The `AddRange(key, values)` and `AddRangeAsync(key, values)` overloads already returned `int` and are unchanged.

#### Recommended Upgrade Steps

1. **Update NuGet package:**
   ```bash
   dotnet add package MultiMap --version 1.0.11
   ```

2. **Update call sites:**
   - If you ignored the return value, no changes needed — the call still compiles
   - If you assigned the result or passed it to a method expecting `void`/`Task`, update to handle the `int`/`Task<int>` return type

3. **If you implement `IMultiMap` or `IMultiMapAsync` directly:**
   - Change your `AddRange(IEnumerable<KeyValuePair<TKey, TValue>>)` signature from `void` to `int`
   - Change your `AddRangeAsync(IEnumerable<KeyValuePair<TKey, TValue>>)` signature from `Task` to `Task<int>`

#### Compatibility

All other APIs remain unchanged. The only breaking changes are the `AddRange` and `AddRangeAsync` return types on the `IEnumerable<KeyValuePair>` overloads.

### Upgrading to Version 2.0.1+

Version 2.0.1 removes two `ISimpleMultiMap` members that were soft-deprecated in v1.0.12 and completes their removal as a **source-breaking change**.

#### Breaking Changes

**`ISimpleMultiMap.Clear(TKey)` removed (v2.0.1):**

`Clear(TKey key)` was deprecated in v1.0.12 as an `[Obsolete]` alias for `RemoveKey(TKey key)`. It has now been removed. Update any remaining call sites:

```csharp
// Before (v1.0.x — emitted CS0618 warning since v1.0.12)
map.Clear("keyA");

// After (v2.0.1)
map.RemoveKey("keyA");
```

If you implement `ISimpleMultiMap` directly, remove your `Clear(TKey key)` override (it is no longer part of the contract).

---

**`ISimpleMultiMap.Flatten()` removed (v2.0.1):**

`Flatten()` was deprecated in v1.0.12 because `ISimpleMultiMap<TKey, TValue>` already implements `IEnumerable<KeyValuePair<TKey, TValue>>`. It has now been removed. Replace any remaining usages with direct enumeration:

```csharp
// Before (v1.0.x — emitted CS0618 warning since v1.0.12)
foreach (var kvp in map.Flatten()) { /* ... */ }
var pairs = map.Flatten().ToList();

// After (v2.0.1)
foreach (var kvp in map) { /* ... */ }
var pairs = map.ToList();
```

#### Recommended Upgrade Steps

1. **Update NuGet package:**
   ```bash
   dotnet add package MultiMap --version 2.0.1
   ```

2. **Replace `Clear(key)` with `RemoveKey(key)`** at every call site that was producing a CS0618 warning.

3. **Replace `Flatten()` with direct enumeration** at every call site that was producing a CS0618 warning.

4. **If you implement `ISimpleMultiMap` directly**, remove any `Clear(TKey)` and `Flatten()` override methods — they are no longer part of the interface contract.

#### Compatibility

All other APIs introduced in v1.0.12 (including `RemoveKey`, `SimpleMultiMap.Count`, `SimpleMultiMap.Equals`, the new constructors, and all `IMultiMap` / `IMultiMapAsync` members) remain **fully backward-compatible**.

### Upgrading to Version 1.0.12+

Version 1.0.12 is focused on **correctness, consistency, and performance**. There is one source-breaking API change, one backward-compatible rename alias, and one soft deprecation in `ISimpleMultiMap`.

#### Breaking Changes

**`ISimpleMultiMap.Remove` return type changed from `void` to `bool` (v1.0.12):**

`ISimpleMultiMap.Remove(TKey, TValue)` previously returned `void`, which was inconsistent with the `IMultiMap.Remove(TKey, TValue)` signature. The return type has been changed to `bool` so both interfaces are consistent.

**Before (v1.0.11):**
```csharp
map.Remove("A", 1);  // void — no way to know if the pair was actually removed
```

**After (v1.0.12):**
```csharp
bool removed = map.Remove("A", 1);  // true if the pair was found and removed
```

If you implement `ISimpleMultiMap` directly, update your `Remove` signature from `void` to `bool`. Callers that ignored the return value require no changes — the call still compiles.

---

**`ISimpleMultiMap.Clear(TKey)` renamed to `ISimpleMultiMap.RemoveKey(TKey)` (v1.0.12) — backward-compatible:**

`ISimpleMultiMap.Clear(TKey key)` has been renamed to `ISimpleMultiMap.RemoveKey(TKey key)` to align with `IMultiMap.RemoveKey(TKey key)` and make the API surface consistent across all multimap interfaces.

**Before (v1.0.11):**
```csharp
map.Clear("keyA");  // removes all values for "keyA"
```

**After (v1.0.12):**
```csharp
map.RemoveKey("keyA");  // removes all values for "keyA"
```

For backward compatibility, `Clear(TKey key)` is retained in `ISimpleMultiMap` as an `[Obsolete]` alias that forwards directly to `RemoveKey(TKey key)`. Existing call sites continue to compile and run; a compiler warning (`CS0618`) is emitted to guide migration. Migrate call sites to `map.RemoveKey(key)` before the next major version when `Clear(key)` will be removed. Note: the parameterless `Clear()` on `IMultiMap` implementations is unaffected.

---

**`ISimpleMultiMap.Flatten()` deprecated (v1.0.12):**

`ISimpleMultiMap.Flatten()` is decorated with `[Obsolete]` and will be removed in a future version. The method was always equivalent to enumerating the map directly — `ISimpleMultiMap<TKey, TValue>` implements `IEnumerable<KeyValuePair<TKey, TValue>>`, so iterating the map with `foreach`, `ToList()`, or any LINQ method produces the exact same sequence.

**Before (v1.0.11):**
```csharp
foreach (var kvp in map.Flatten()) { /* ... */ }
var pairs = map.Flatten().ToList();
```

**After (v1.0.12):**
```csharp
foreach (var kvp in map) { /* ... */ }
var pairs = map.ToList();
```

This is a **soft deprecation** — existing call sites continue to compile and run without change; a compiler warning (`CS0618`) is emitted to guide migration. No immediate action is required, but callers should migrate before the next major version.

#### Non-Breaking Changes

- **`SimpleMultiMap.Count` property added:** `SimpleMultiMap<TKey, TValue>` now exposes a `Count` property (inherited via `IReadOnlyCollection<KeyValuePair<TKey, TValue>>`) that returns the total number of key-value pairs across all keys (**O(1)** — backed by `_count`).

- **`SimpleMultiMap.Equals(IReadOnlySimpleMultiMap<TKey, TValue>? other)` added:** Typed equality compares the total pair count first (fast exit), then verifies per-key value-set contents using set-equality semantics.

- **`SimpleMultiMap` equality bug fix:** `Equals(IReadOnlySimpleMultiMap<TKey, TValue>? other)` previously compared `_dictionary.Count` (key count) against `other.Count` (total pair count), producing incorrect results when maps had equal key counts but different numbers of values. Fixed to compare total pair counts on both sides.

- **`MultiMapAsync` typed equality:**

- **`ConcurrentMultiMap` is now fully lock-free:** The internal storage changed from `ConcurrentDictionary<TKey, HashSet<TValue>>` with per-key locking and an `Interlocked` counter to `ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>`. All per-key read and write operations are now lock-free. `Count` and `KeyCount` are **O(1)** — backed by `_count` and `_keyCount` fields updated via `Interlocked` in every mutating path.

- **`SymmetricExceptWith` optimization for `IMultiMap`:** The `IMultiMap` overload now uses a per-key lookup dictionary (same strategy as the `ISimpleMultiMap` overload) to avoid redundant lock acquisitions when multiple entries share the same key.

- **Zero-allocation `Values` property and `GetValuesAsync()`:** `MultiMapBase.Values`, `MultiMapLock.Values`, and `MultiMapAsync.GetValuesAsync()` now use a custom struct enumerator instead of `SelectMany` LINQ iterators, eliminating per-access heap allocations on hot read paths.

- **`MultiMapBase` partial classes:** The nested `ValuesCollection` and `ValuesEnumerator` types were extracted into separate `MultiMapBase.ValuesCollection.cs` and `MultiMapBase.ValuesEnumerator.cs` partial files for better code organization.

- **All concrete classes are now `sealed`:** Every concrete implementation (`MultiMapList`, `MultiMapSet`, `SortedMultiMap`, `ConcurrentMultiMap`, `MultiMapLock`, `MultiMapAsync`, `SimpleMultiMap`) is declared `sealed`, enabling JIT devirtualization on hot paths such as `Add` and `Remove`.

- **Null-value guard on `AddRange`:** A runtime guard was added to prevent `null` values from silently entering list-backed collections, preserving the `TValue : notnull` contract at runtime.

- **`MultiMapList` equality fix:** `MultiMapList.Equals(object?)` previously used `SequenceEqual`, which is order-dependent. The comparison now uses set-based equality so two lists with the same content in a different insertion order compare equal.

- **`MultiMapSet(IEqualityComparer<TKey>?, IEqualityComparer<TValue>?)` constructor added:** A combined key-and-value comparer overload fills the gap between the separate key-only and value-only overloads, bringing `MultiMapSet` to a full 8-overload family.

- **`ConcurrentMultiMap` key-comparer constructors added:** `ConcurrentMultiMap(IEqualityComparer<TKey>?)` and `ConcurrentMultiMap(IEqualityComparer<TKey>?, IEqualityComparer<TValue>?)` overloads are now available, bringing `ConcurrentMultiMap` to a full 8-overload family on par with the other implementations.

- **`MultiMapList.AddRange(IEnumerable<KeyValuePair<TKey, TValue>>)` optimised on .NET 6+:** The KVP-sequence overload now overrides the base-class implementation and uses `CollectionsMarshal.GetValueRefOrAddDefault` on .NET 6 and later, eliminating the per-item virtual dispatch through the base class and matching the existing `Add` and `AddRange(key, values)` fast paths.

#### Recommended Upgrade Steps

1. **Update NuGet package:**
   ```bash
   dotnet add package MultiMap --version 2.0.1
   ```

2. **If you implement `ISimpleMultiMap` directly:**
   - Change your `Remove(TKey key, TValue value)` signature from `void` to `bool` and return `true` when the pair was removed.
   - Rename your `Clear(TKey key)` method to `RemoveKey(TKey key)`.

3. **Update `Clear(key)` call sites** (emits CS0618 warning; hard-removed in v2.0.1):
   ```csharp
   // Before
   map.Clear("keyA");

   // After
   map.RemoveKey("keyA");
   ```

4. **Migrate away from `Flatten()` (emits CS0618 warning; hard-removed in v2.0.1):**
   ```csharp
   // Before
   foreach (var kvp in map.Flatten()) { /* ... */ }
   var pairs = map.Flatten().ToList();

   // After — enumerate the map directly
   foreach (var kvp in map) { /* ... */ }
   var pairs = map.ToList();
   ```

5. **Adopt the `Remove` return value where useful:**
   ```csharp
   // Before: result was silently discarded
   map.Remove("key", value);

   // After: check whether anything was actually removed
   if (!map.Remove("key", value))
       Console.WriteLine("Pair not found");
   ```

#### Compatibility

All other APIs are **fully backward-compatible**. The one source-breaking change in v1.0.12 is the `ISimpleMultiMap.Remove` return type. `Clear(TKey)` and `Flatten()` emitted CS0618 deprecation warnings since v1.0.12 and have been **hard-removed in v2.0.1** — see the [Upgrading to 2.0.1+](#upgrading-to-version-201) section above.

## Release Notes

See [Release Notes](https://github.com/TigoS/MultiMap/blob/master/ReleaseNotes.md) for the full version history.

## License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).

See [LICENSE](https://github.com/TigoS/MultiMap?tab=MIT-1-ov-file) for details.

## Author

**TigoS** — [GitHub](https://github.com/TigoS/MultiMap)
