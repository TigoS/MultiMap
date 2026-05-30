# MultiMap

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0%20%7C%208.0%20%7C%20Standard%202.0-blue.svg)](https://dotnet.microsoft.com/)
[![C# 14](https://img.shields.io/badge/C%23-14.0-blue)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![NUnit](https://img.shields.io/badge/tests-NUnit%204-green)](https://nunit.org/)
[![BenchmarkDotNet](https://img.shields.io/badge/BenchmarkDotNet-v0.15.0-blue)](https://benchmarkdotnet.org/)
[![NuGet](https://img.shields.io/nuget/v/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![Coverage](https://img.shields.io/badge/coverage-99.51%25-brightgreen)]()

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
  - [Test Coverage by Extension Methods](#test-coverage-by-extension-methods)
  - [Test Categories](#test-categories)
  - [Test Coverage Percentage](#test-coverage-percentage)
  - [Code Coverage (Coverlet)](#code-coverage-coverlet)
- [Benchmarks](#benchmarks)
- [Release Notes](#release-notes)
- [License](#license)

## Overview

A **multimap** is a collection that maps each key to one or more values — unlike a standard `Dictionary<TKey, TValue>`, which allows only one value per key. This library provides **7 ready-to-use implementations** behind **3 interfaces**, so you can choose the right trade-off between uniqueness, ordering, thread-safety, and async support for your scenario. It also ships with **set-like extension methods** (`Union`, `Intersect`, `ExceptWith`, `SymmetricExceptWith`) that work across all implementations.

## Features

- **7 multimap implementations** covering a wide range of use cases
- **3 interfaces** (`IMultiMap`, `IMultiMapAsync`, `ISimpleMultiMap`) for flexibility
- **Multi-target**: .NET 10, .NET 8, and .NET Standard 2.0
- **Set-like extension methods**: `Union`, `Intersect`, `ExceptWith`, `SymmetricExceptWith`
- **Thread-safe variants**: fully lock-free (`ConcurrentMultiMap`), reader-writer locked (`MultiMapLock`), and async-safe (`MultiMapAsync`)
- **Dispose safety**: `MultiMapLock` and `MultiMapAsync` throw `ObjectDisposedException` after disposal
- **Custom value comparers**: `IEqualityComparer<TValue>` constructor overloads on all `HashSet`-based implementations
- **Initial capacity constructors**: Pre-size internal dictionaries to reduce re-allocations
- **Full XML documentation** for IntelliSense support
- **3,366 test executions** (1,683 tests × 2 target frameworks) with NUnit 4
- **99.51% line coverage, 96.38% branch coverage** via Coverlet
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
├── MultiMap.Tests/                             # Unit tests (NUnit 4, 1,466 tests × 2 TFMs)
├── MultiMap.Demo/                              # Console demo application
│   ├── Program.cs                              # Demo entry point
│   └── TestDataHelper.cs                       # Sample data factory for demos
└── BenchmarkSuite/                             # BenchmarkDotNet performance benchmarks
```

## Interfaces

### Interface Hierarchy

The library follows a hierarchical interface design with three parallel families:

**Read-Only Interfaces:**
- `IReadOnlySimpleMultiMap<TKey, TValue>` — Base read-only interface with `Get`, `GetOrDefault`
- `IReadOnlyMultiMap<TKey, TValue>` — Extends `IReadOnlySimpleMultiMap` with `TryGet`, `Contains`, `ContainsKey`, `Count`, `KeyCount`, `Keys`, `Values`, `GetValuesCount`, `this[key]`
- `IReadOnlyMultiMapAsync<TKey, TValue>` — Async read-only with `GetAsync`, `TryGetAsync`, `ContainsAsync`, etc.

**Mutable Interfaces:**
- `ISimpleMultiMap<TKey, TValue>` — Extends `IReadOnlySimpleMultiMap` with `Add`, `Remove`, `Clear`
- `IMultiMap<TKey, TValue>` — Extends `IReadOnlyMultiMap` with `Add`, `AddRange`, `Remove`, `RemoveRange`, `RemoveWhere`, `RemoveKey`, `Clear`
- `IMultiMapAsync<TKey, TValue>` — Extends `IReadOnlyMultiMapAsync` with async mutations and `CancellationToken` support

### `IReadOnlySimpleMultiMap<TKey, TValue>`

The base read-only interface. Extends `IEnumerable<KeyValuePair<TKey, TValue>>`.

| Method | Returns | Description |
|---|---|---|
| `Get(key)` | `IEnumerable<TValue>` | Returns values; throws `KeyNotFoundException` if not found |
| `GetOrDefault(key)` | `IEnumerable<TValue>` | Returns values or empty if not found |
| `Count` | `int` | Total number of key-value pairs (from `IReadOnlyCollection<KeyValuePair<TKey,TValue>>`) |

### `IReadOnlyMultiMap<TKey, TValue>`

Extended read-only interface. Extends `IReadOnlySimpleMultiMap<TKey, TValue>`.

| Member | Returns | Description |
|---|---|---|
| `TryGet(key, out values)` | `bool` | Attempts to retrieve values; returns `true` if key exists |
| `ContainsKey(key)` | `bool` | Checks if a key exists |
| `Contains(key, value)` | `bool` | Checks if a specific key-value pair exists |
| `KeyCount` | `int` | Gets the number of unique keys |
| `Count` | `int` | Gets the total number of key-value pairs |
| `Keys` | `IEnumerable<TKey>` | Gets all keys |
| `Values` | `IEnumerable<TValue>` | Gets all values across all keys |
| `GetValuesCount(key)` | `int` | Gets count of values for a key (0 if missing) |
| `this[key]` | `IEnumerable<TValue>` | Indexer — convenient value access by key |
| `GetEnumerator()` | `IEnumerator<KeyValuePair>` | Enumerates all key-value pairs |

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
| `AddRangeAsync(key, values)` | `Task<int>` | Asynchronously adds multiple values; returns count added |
| `AddRangeAsync(items)` | `Task<int>` | Asynchronously adds multiple key-value pairs; returns count added |
| `RemoveAsync(key, value)` | `ValueTask<bool>` | Asynchronously removes a pair |
| `RemoveRangeAsync(items)` | `ValueTask<int>` | Asynchronously removes multiple pairs; returns count removed |
| `RemoveWhereAsync(key, predicate)` | `ValueTask<int>` | Asynchronously removes values matching predicate; returns count removed |
| `RemoveKeyAsync(key)` | `ValueTask<bool>` | Asynchronously removes a key |
| `ClearAsync()` | `Task` | Asynchronously clears all entries |

**Inherited from `IReadOnlyMultiMapAsync`:** `GetAsync`, `GetOrDefaultAsync`, `TryGetAsync`, `ContainsKeyAsync`, `ContainsAsync`, `GetCountAsync`, `GetKeyCountAsync`, `GetKeysAsync`, `GetValuesCountAsync`, `GetValuesAsync`

### `ISimpleMultiMap<TKey, TValue>`

A simplified multimap interface. Extends `IReadOnlySimpleMultiMap<TKey, TValue>`.

| Method | Returns | Description |
|---|---|---|
| `Add(key, value)` | `bool` | Adds a key-value pair; returns `false` if already present |
| `Remove(key, value)` | `bool` | Removes a specific pair; returns `true` if removed |
| `RemoveKey(key)` | `void` | Removes all values for a key |
| ~~`Flatten()`~~ | `IEnumerable<KeyValuePair<TKey, TValue>>` | **Deprecated.** Enumerate the map directly instead (`foreach`, `ToList()`, LINQ) |

**Inherited from `IReadOnlySimpleMultiMap`:** `Get`, `GetOrDefault`

## Implementations

### `MultiMapBase<TKey, TValue, TCollection>` — Abstract Base Class

Provides the shared dictionary-backed implementation inherited by `MultiMapList`, `MultiMapSet`, and `SortedMultiMap`. Implements `IMultiMap<TKey, TValue>` with `Add`, `AddRange`, `Remove`, `RemoveKey`, `RemoveRange`, `RemoveWhere`, `Get`, `GetOrDefault`, `TryGet`, `ContainsKey`, `Contains`, `Count`, `KeyCount`, `Keys`, `Values`, `GetValuesCount`, indexer, `Clear`, and `GetEnumerator`. Subclasses override `CreateCollection()`, `AddToCollection()`, and `RemoveWhereFromCollection()` to plug in their specific collection type. On .NET 6+ subclasses may also override `Add`/`AddRange` to use `CollectionsMarshal.GetValueRefOrAddDefault` for a single dictionary lookup.

### `MultiMapList<TKey, TValue>` — List-Based

Extends `MultiMapBase<TKey, TValue, List<TValue>>`. Uses `Dictionary<TKey, List<TValue>>` internally. **Allows duplicate values** per key. Fastest for add operations due to `List<T>.Add` being O(1) amortized. On .NET 6+ uses `CollectionsMarshal` for optimized `Add`/`AddRange`. Returns a zero-copy `ReadOnlyCollection<TValue>` from `Get`.

**Constructors:** `()`, `(int capacity)`, `(IEqualityComparer<TKey>? keyComparer)`, `(int capacity, IEqualityComparer<TKey>? keyComparer)`

### `MultiMapSet<TKey, TValue>` — HashSet-Based

Extends `MultiMapBase<TKey, TValue, HashSet<TValue>>`. Uses `Dictionary<TKey, HashSet<TValue>>` internally. **Ensures unique values** per key. Best for scenarios requiring fast lookups and unique value semantics. On .NET 6+ uses `CollectionsMarshal` for optimized `Add`/`AddRange`.

**Constructors:** `()`, `(IEqualityComparer<TKey>? keyComparer)`, `(IEqualityComparer<TValue>? valueComparer)`, `(int capacity)`, `(int capacity, IEqualityComparer<TKey>? keyComparer)`, `(int capacity, IEqualityComparer<TValue>? valueComparer)`, `(int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)`

### `SortedMultiMap<TKey, TValue>` — Sorted

Extends `MultiMapBase<TKey, TValue, SortedSet<TValue>>`. Uses `SortedDictionary<TKey, SortedSet<TValue>>`. Keys and values are maintained in sorted order. Ideal for ordered enumeration and range queries. Requires `TKey : IComparable<TKey>` and `TValue : IComparable<TValue>`.

**Constructors:** `()`, `(IComparer<TKey>? keyComparer)`

### `ConcurrentMultiMap<TKey, TValue>` — Fully Lock-Free Concurrent

Implements `IMultiMap`. Uses `ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>` for fully lock-free concurrent access — no explicit locks are held for per-key operations. `Count` is O(n), computed by summing the sizes of all inner dictionaries. `KeyCount` iterates the outer dictionary filtering empty inner sets. `Keys` returns a snapshot (`ToArray()`) for safe concurrent enumeration. Suitable for high-concurrency scenarios.

**Constructors:** `()`, `(IEqualityComparer<TKey>? keyComparer)`, `(IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)`, `(IEqualityComparer<TValue>? valueComparer)`, `(int concurrencyLevel, int capacity)`, `(int concurrencyLevel, int capacity, IEqualityComparer<TKey>? keyComparer)`, `(int concurrencyLevel, int capacity, IEqualityComparer<TValue>? valueComparer)`, `(int concurrencyLevel, int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)`

### `MultiMapLock<TKey, TValue>` — Reader-Writer Locked

Implements `IMultiMap` and `IDisposable`. Uses `ReaderWriterLockSlim` to allow concurrent reads with exclusive writes. Good for read-heavy workloads with occasional writes.

**Constructors:** `()`, `(IEqualityComparer<TKey>? keyComparer)`, `(IEqualityComparer<TValue>? valueComparer)`, `(int capacity)`, `(int capacity, IEqualityComparer<TKey>? keyComparer)`, `(int capacity, IEqualityComparer<TValue>? valueComparer)`, `(int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)`

### `MultiMapAsync<TKey, TValue>` — Async-Safe

Implements `IMultiMapAsync`, `IDisposable`, and `IAsyncDisposable`. Uses `SemaphoreSlim` for async-compatible mutual exclusion. Designed for `async`/`await` patterns and I/O-bound scenarios. `Equals(IReadOnlyMultiMapAsync<TKey, TValue>?)` uses a deadlock-safe dual-semaphore acquisition when comparing two `MultiMapAsync` instances; `Equals(object?)` throws `InvalidOperationException` under a `SynchronizationContext` — use `EqualsAsync` in `async` contexts instead.

**Constructors:** `()`, `(IEqualityComparer<TKey>? keyComparer)`, `(IEqualityComparer<TValue>? valueComparer)`, `(int capacity)`, `(int capacity, IEqualityComparer<TKey>? keyComparer)`, `(int capacity, IEqualityComparer<TValue>? valueComparer)`, `(int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)`

### `SimpleMultiMap<TKey, TValue>` — Lightweight

Implements `ISimpleMultiMap`. A lightweight multimap with a simplified API. `Get` throws `KeyNotFoundException` if the key doesn't exist, while `GetOrDefault` returns an empty collection. `Count` returns the total number of key-value pairs (O(n) over keys, summing per-key set sizes). Provides typed `Equals(IReadOnlySimpleMultiMap<TKey, TValue>?)` comparing total pair count then per-key value-set contents.

**Constructors:** `()`, `(int capacity)`, `(IEqualityComparer<TValue>? valueComparer)`, `(int capacity, IEqualityComparer<TValue>? valueComparer)`

## Comparison Table

| Implementation | Interface | Thread-Safe | Duplicates | Ordered | Count Complexity |
|---|---|---|---|---|---|
| `MultiMapList` | `IMultiMap` | ❌ No | ✅ Yes | ❌ No | O(1) |
| `MultiMapSet` | `IMultiMap` | ❌ No | ❌ No | ❌ No | O(1) |
| `SortedMultiMap` | `IMultiMap` | ❌ No | ❌ No | ✅ Yes | O(1) |
| `ConcurrentMultiMap` | `IMultiMap` | ✅ Lock-free | ❌ No | ❌ No | O(n) |
| `MultiMapLock` | `IMultiMap` | ✅ RW Lock | ❌ No | ❌ No | O(1) |
| `MultiMapAsync` | `IMultiMapAsync` | ✅ Semaphore | ❌ No | ❌ No | O(1) |
| `SimpleMultiMap` | `ISimpleMultiMap` | ❌ No | ❌ No | ❌ No | ➖ |

### Internal Data Structures

| Implementation | Outer Structure | Inner Structure | Notes |
|---|---|---|---|
| `MultiMapList` | `Dictionary<TKey, List<TValue>>` | `List<TValue>` | O(1) amortized add; allows duplicate values |
| `MultiMapSet` | `Dictionary<TKey, HashSet<TValue>>` | `HashSet<TValue>` | O(1) add/contains; enforces unique values |
| `SortedMultiMap` | `SortedDictionary<TKey, SortedSet<TValue>>` | `SortedSet<TValue>` | O(log n) operations; keys & values sorted |
| `ConcurrentMultiMap` | `ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>` | `ConcurrentDictionary<TValue, byte>` | Fully lock-free via nested `ConcurrentDictionary`; `Count` is O(n) by summing inner sizes |
| `MultiMapLock` | `Dictionary<TKey, HashSet<TValue>>` | `HashSet<TValue>` | Protected by `ReaderWriterLockSlim` |
| `MultiMapAsync` | `Dictionary<TKey, HashSet<TValue>>` | `HashSet<TValue>` | Protected by `SemaphoreSlim(1,1)` |
| `SimpleMultiMap` | `Dictionary<TKey, HashSet<TValue>>` | `HashSet<TValue>` | Simplified API surface |

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
| **Enumeration** | ✅ `IEnumerable<KeyValuePair>` | ✅ `IAsyncEnumerable<KeyValuePair>` | ✅ `IEnumerable<KeyValuePair>` (`Flatten()` deprecated) |
| **Disposable** | ⚠️ Only `MultiMapLock` | ✅ Yes (`IAsyncDisposable` + `IDisposable`) | ❌ No |
| **CancellationToken** | ❌ No | ✅ Yes (all methods) | ❌ No |

### When to Use Which Implementation

| Use Case | Recommended Implementation | Reason |
|---|---|---|
| General purpose, unique values | `MultiMapSet` | Fast O(1) lookups with uniqueness guarantee |
| Duplicate values needed | `MultiMapList` | Only implementation allowing duplicate values per key |
| Sorted enumeration / range queries | `SortedMultiMap` | Maintains key and value ordering |
| High-concurrency, many threads | `ConcurrentMultiMap` | Fully lock-free via nested `ConcurrentDictionary`; no contention under concurrent reads/writes |
| Read-heavy, occasional writes | `MultiMapLock` | RW lock allows concurrent readers |
| Async / I/O-bound code | `MultiMapAsync` | `SemaphoreSlim` works with `async`/`await` |
| Minimal API, quick prototyping | `SimpleMultiMap` | Simplified interface with `GetOrDefault` and direct enumeration |

### Performance Comparison (5,000 pairs)

| Implementation | Add | Get (100 keys) | Contains | Count | Relative Add Speed |
|---|---|---|---|---|---|
| `MultiMapList` | 34,239 ns | 8,031 ns | 28 ns | < 1 ns | **1.0x** (baseline) |
| `MultiMapSet` | 72,331 ns | 8,845 ns | 34 ns | < 1 ns | 2.1x |
| `SimpleMultiMap` | 71,966 ns | 11,327 ns | — | ~190 ns ¹ | 2.1x |
| `ConcurrentMultiMap` | 347,000 ns | 85,976 ns | 294 ns | ~59,000 ns | 10.1x |
| `MultiMapLock` | 203,122 ns | 13,985 ns | 25 ns | 16 ns | 5.9x |
| `MultiMapAsync` | 290,558 ns | 21,917 ns | 40 ns | 33 ns | 8.5x |
| `SortedMultiMap` | 829,766 ns | 40,506 ns | 24 ns | < 1 ns | 24.2x |

> **Note:** Performance data from BenchmarkDotNet. See [Benchmarks](#benchmarks) for full details. ¹ `SimpleMultiMap.Count` sums per-key set sizes (O(n) over keys); benchmark pending next run.

## Extension Methods

`MultiMapHelper` provides set-like operations as extension methods for all three interface families:

| Method | `IMultiMap` | `ISimpleMultiMap` | `IMultiMapAsync` |
|---|---|---|---|
| **Union** | `Union()` | `Union()` | `UnionAsync()` |
| **Intersect** | `Intersect()` | `Intersect()` | `IntersectAsync()` |
| **ExceptWith** | `ExceptWith()` | `ExceptWith()` | `ExceptWithAsync()` |
| **SymmetricExceptWith** | `SymmetricExceptWith()` | `SymmetricExceptWith()` | `SymmetricExceptWithAsync()` |

> **Note:** When used with concurrent implementations, these methods are **not atomic**. Individual operations are thread-safe, but the overall result may reflect interleaved concurrent modifications. No structural corruption or count drift will occur.

## Installation

### NuGet

```shell
dotnet add package MultiMap
```

### Package Reference

```xml
<PackageReference Include="MultiMap" Version="1.0.12" />
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

The library includes **1,683 unit tests** written with **NUnit 4**, running on both **net10.0** and **net8.0** (**3,366 total test executions**), covering all implementations, interfaces, edge cases, and concurrent stress tests.

```shell
dotnet test
```

### Test Coverage by Implementation

| Test Class | Tests | Category |
|---|---|---|
| `MultiMapAsyncTests` | 189 | Async implementation |
| `MultiMapAsync_GenericInterfaceEqualsTests` | 21 | Generic-interface async equality path |
| `ConcurrentMultiMapTests` | 127 | Lock-free concurrent implementation |
| `MultiMapLockTests` | 164 | RW Lock implementation |
| `MultiMapListTests` | 145 | List-based implementation |
| `MultiMapSetTests` | 140 | HashSet-based implementation |
| `SortedMultiMapTests` | 133 | Sorted implementation |
| `SimpleMultiMapTests` | 75 | Lightweight implementation |
| **Entity subtotal** | **994** | |

### Test Coverage by Base Class

| Test Class | Tests | Category |
|---|---|---|
| `MultiMapBaseTests` (×3 fixtures) | 270 | Base class contract (MultiMapSet, MultiMapList, SortedMultiMap) |
| **Base subtotal** | **270** | |

### Test Coverage by Extension Methods

| Test Class | Tests | Category |
|---|---|---|
| `MultiMapHelperAsyncTests` | 65 | Async extension methods (`UnionAsync`, `IntersectAsync`, etc.) |
| `MultiMapHelperWithMultiMapSetTests` | 28 | Extensions with `MultiMapSet` + stress tests |
| `MultiMapHelperTests` | 38 | `IMultiMap` extensions (primary) |
| `SimpleMultiMapHelperTests` | 36 | `ISimpleMultiMap` extensions |
| `MultiMapHelperExtensionAsyncTests` | 25 | Async helper extension edge cases |
| `MultiMapHelperWithSortedMultiMapEdgeCaseTests` | 24 | Edge cases with `SortedMultiMap` |
| `MultiMapHelperWithConcurrentMultiMapEdgeCaseTests` | 24 | Edge cases with `ConcurrentMultiMap` |
| `MultiMapHelperWithMultiMapLockEdgeCaseTests` | 24 | Edge cases with `MultiMapLock` |
| `MultiMapHelperWithMultiMapListEdgeCaseTests` | 23 | Edge cases with `MultiMapList` |
| `MultiMapHelperWithMultiMapLockTests` | 4 | Extensions + concurrent stress tests |
| `MultiMapHelperWithConcurrentMultiMapTests` | 4 | Extensions + concurrent stress tests |
| `MultiMapHelperWithMultiMapListTests` | 4 | Extensions with `MultiMapList` + stress tests |
| `MultiMapHelperWithSortedMultiMapTests` | 4 | Extensions with `SortedMultiMap` + stress tests |
| **Helper subtotal** | **303** | |

| | |
|---|---|
| **Total** | **1,683 tests × 2 TFMs = 3,366 executions** |

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
| **Set Operations** | Union, Intersect, ExceptWith, SymmetricExceptWith | Overlapping/disjoint maps, self-operations, empty inputs |

### Test Coverage Percentage

| Area | Tests | % of Total |
|---|---|---|
| `MultiMapAsyncTests` | 189 | 11.2% |
| `MultiMapAsync_GenericInterfaceEqualsTests` | 21 | 1.2% |
| `ConcurrentMultiMapTests` | 127 | 7.5% |
| `MultiMapLockTests` | 164 | 9.7% |
| `MultiMapListTests` | 145 | 8.6% |
| `MultiMapSetTests` | 140 | 8.3% |
| `SortedMultiMapTests` | 133 | 7.9% |
| `SimpleMultiMapTests` | 75 | 4.5% |
| **Entity subtotal** | **994** | **59.1%** |
| `MultiMapBaseTests` (×3 fixtures) | 270 | 16.0% |
| **Base subtotal** | **270** | **16.0%** |
| `MultiMapHelperAsyncTests` | 65 | 3.9% |
| `MultiMapHelperWithMultiMapSetTests` | 28 | 1.7% |
| `MultiMapHelperTests` | 38 | 2.3% |
| `SimpleMultiMapHelperTests` | 36 | 2.1% |
| `MultiMapHelperExtensionAsyncTests` | 25 | 1.5% |
| `MultiMapHelperWithSortedMultiMapEdgeCaseTests` | 24 | 1.4% |
| `MultiMapHelperWithConcurrentMultiMapEdgeCaseTests` | 24 | 1.4% |
| `MultiMapHelperWithMultiMapLockEdgeCaseTests` | 24 | 1.4% |
| `MultiMapHelperWithMultiMapListEdgeCaseTests` | 23 | 1.4% |
| `MultiMapHelperWithMultiMapLockTests` | 4 | 0.2% |
| `MultiMapHelperWithConcurrentMultiMapTests` | 4 | 0.2% |
| `MultiMapHelperWithMultiMapListTests` | 4 | 0.2% |
| `MultiMapHelperWithSortedMultiMapTests` | 4 | 0.2% |
| **Helper subtotal** | **303** | **18.0%** |
| **Total** | **1,683 × 2 TFMs** | **3,366 executions** |

> **Coverage distribution:** ~59% of tests target the 8 core implementations (including new interface member tests, concurrent stress tests, snapshot/defensive copy tests, slow path contention tests, custom value comparer tests, key comparer constructor tests, and initial capacity constructor tests), ~16% verify the shared `MultiMapBase` contract across all 3 subclass fixtures, and ~18% cover the set-like extension methods across all interface families — including concurrent and sequential stress tests, edge cases, deep iteration tests that exercise helpers with all implementations, and comprehensive tests for async extension methods in MultiMapHelper. All 1,683 unique tests run on both **net10.0** and **net8.0**, validating `#if NET6_0_OR_GREATER` code paths on both target frameworks.

### Code Coverage (Coverlet)

Code coverage is collected with **Coverlet** (`coverlet.collector`) during `dotnet test` and reported via **ReportGenerator**.

```shell
dotnet test --collect:"XPlat Code Coverage"
```

#### Summary

| Metric | Value |
|---|---|
| **Line coverage** | **99.51%** (2,250/2,261 lines) |
| **Branch coverage** | **96.38%** (906/940 branches)
| **Method coverage** | **97.9%** (236/241 methods) |

#### Per-Class Breakdown

| Class | Line Coverage | Branch Coverage | Status |
|---|---|---|---|
| `ConcurrentMultiMap<TKey, TValue>` | 99.02% | 98.43% | ✅ Near-full |
| `MultiMapAsync<TKey, TValue>` | 99.45% | 84.12% | ✅ Near-full |
| `MultiMapBase<TKey, TValue, TCollection>` | 100% | 98.71% | ✅ Full |
| `MultiMapBase/ValuesCollection` | 100% | 100% | ✅ Full |
| `MultiMapBase/ValuesEnumerator` | 100% | 100% | ✅ Full |
| `MultiMapList<TKey, TValue>` | 94.5% | 100% | ✅ Near-full |
| `MultiMapLock<TKey, TValue>` | 100% | 98.8% | ✅ Full |
| `MultiMapSet<TKey, TValue>` | 97.5% | 100% | ✅ Near-full |
| `SimpleMultiMap<TKey, TValue>` | 100% | 100% | ✅ Full |
| `SortedMultiMap<TKey, TValue>` | 100% | 100% | ✅ Full |
| `MultiMapHelper` | 100% | 100% | ✅ Full |

> **Notes:**
> - **7 of 11 tracked classes achieve 100% line coverage**: `SimpleMultiMap`, `MultiMapBase`, `MultiMapLock`, `SortedMultiMap`, and `MultiMapHelper`; `ValuesCollection` and `ValuesEnumerator` (nested) also reach 100% line coverage.
> - `SimpleMultiMap`, `SortedMultiMap`, `ValuesCollection`, `ValuesEnumerator`, and `MultiMapHelper` achieve **100% line and 100% branch coverage**.
> - `MultiMapLock` reaches **100% line coverage** with **98.8% branch coverage**.
> - `ConcurrentMultiMap` at **99.02% line coverage** — remaining uncovered lines are `continue` guards in race-condition retry loops that are structurally unreachable in single-threaded test scenarios.
> - `MultiMapList` (94.5%) and `MultiMapSet` (97.5%) have uncovered lines in `CreateCollection`/`AddToCollection` methods — these are dead code on .NET 10 and .NET 8 where `CollectionsMarshal` is used instead.
> - `MultiMapAsync` remaining branch misses are concentrated in compiler-generated async state-machine branches and the `SynchronizationContext` guard path, which are not exercisable from the test host.
> - Branch coverage numbers reflect Coverlet's granular condition tracking, including async state machine branches and null-coalescing paths that are structurally unreachable in certain target frameworks.
> - Overall **99.51% line coverage** across the entire assembly with **1,683 tests × 2 target frameworks** (3,366 total executions).

## Benchmarks

Benchmarks are run with **BenchmarkDotNet v0.15.0** with `CPUUsageDiagnoser`.

**Environment:** .NET 10.0.5 (MultiMapSet/List/Sorted/Lock/Async/Simple) · .NET 10.0.8 (ConcurrentMultiMap), 13th Gen Intel Core i9-13900H, 20 logical / 14 physical cores, RyuJIT AVX2

**Benchmark Parameters:** 100 keys × 50 values/key for bulk operations (5,000 pairs); 50 keys × 20 values/key for set operations (1,000 pairs).

### Core Operations

| Operation | MultiMapSet | MultiMapList | ConcurrentMultiMap | SortedMultiMap | MultiMapLock | MultiMapAsync |
|---|---|---|---|---|---|---|
| **Add** (5,000 pairs) | 72,331 ns | 34,239 ns | 347,000 ns | 829,766 ns | 203,122 ns | 290,558 ns |
| **AddRange** (key, values) | 44,503 ns | 4,677 ns | 239,474 ns | 135,734 ns | 83,066 ns | 82,674 ns |
| **Get** (100 keys) | 8,845 ns | 8,031 ns | 85,976 ns | 40,506 ns | 13,985 ns | 21,917 ns |
| **GetOrDefault** (100 keys) | 8,701 ns | 8,434 ns | 85,057 ns | 43,170 ns | 19,995 ns | 21,840 ns |
| **TryGet** | 36 ns | 29 ns | 625 ns | 24 ns | 68 ns | 122 ns |
| **Remove** (5,000 pairs) | 123,370 ns | 112,941 ns | 472,842 ns | 1,713,694 ns | 379,655 ns | 515,439 ns |
| **Clear** | 157,012 ns | 119,122 ns | 434,721 ns | 949,403 ns | 291,700 ns | 365,615 ns |
| **Contains** | 34 ns | 28 ns | 294 ns | 24 ns | 25 ns | 40 ns |
| **ContainsKey** | 34 ns | 27 ns | 295 ns | 23 ns | 23 ns | 37 ns |
| **Count** / **GetCount** | < 1 ns | < 1 ns | ~59,000 ns | < 1 ns | 16 ns | 33 ns |
| **GetKeys** | 31 ns | 27 ns | 383 ns | 24 ns | 321 ns | 335 ns |

### New Interface Members

Benchmarks for properties and methods introduced in v1.0.8+. Async equivalents are shown for `MultiMapAsync`.

| Operation | MultiMapSet | MultiMapList | ConcurrentMultiMap | SortedMultiMap | MultiMapLock | MultiMapAsync |
|---|---|---|---|---|---|---|
| **KeyCount** / **GetKeyCountAsync** | 0.25 ns | 0.03 ns | 564 ns | 0.05 ns | 17 ns | 35 ns |
| **Values** / **GetValuesAsync** | 11,741 ns | 11,283 ns | 80,816 ns | 35,865 ns | 18,195 ns | 18,229 ns |
| **GetValuesCount** / **GetValuesCountAsync** | 3.5 ns | 3.6 ns | 589 ns | 114 ns | 25 ns | 39 ns |
| **Indexer** (`this[key]`) | 4.0 ns | 3.8 ns | 720 ns | 117 ns | 106 ns | — |
| **AddRange(items)** / **AddRangeAsync(items)** | 241,677 ns | 191,969 ns | 538,760 ns | 1,129,768 ns | 353,910 ns | 264,718 ns |
| **RemoveRange** / **RemoveRangeAsync** | 269,777 ns | 241,100 ns | 621,871 ns | 1,581,737 ns | 456,169 ns | 383,420 ns |
| **RemoveWhere** / **RemoveWhereAsync** | 1,441 ns | 663 ns | 8,851 ns | 3,374 ns | 4,287 ns | 4,048 ns |

> **Notes:**
> - **KeyCount**: `ConcurrentMultiMap` (~564 ns) enumerates all entries in the outer `ConcurrentDictionary` filtering empty inner sets, while `MultiMapSet`/`MultiMapList`/`SortedMultiMap` expose a direct O(1) property (< 1 ns). `MultiMapLock` acquires a read lock (~17 ns). `MultiMapAsync` acquires a semaphore (~35 ns).
> - **Indexer**: Not available for `MultiMapAsync` (async API uses `GetAsync` instead).
> - **AddRange(items)**: The KVP overload is ~3–5x slower than `AddRange(key, values)` because it groups items by key and processes multiple keys across the map.
> - **RemoveWhere**: Very efficient (0.7–8.9 μs) compared to `RemoveRange` (241 μs–1,582 μs) because it operates on a single key's value set.

### Set Operations (via `MultiMapHelper`)

| Operation | MultiMapSet | MultiMapList | ConcurrentMultiMap | SortedMultiMap | MultiMapLock | MultiMapAsync |
|---|---|---|---|---|---|---|
| **Union** | 79,720 ns | 57,160 ns | 232,924 ns | 438,700 ns | 150,521 ns | 189,794 ns |
| **Intersect** | 78,230 ns | 64,400 ns | 495,255 ns | 437,430 ns | 152,191 ns | 192,079 ns |
| **ExceptWith** | 80,460 ns | 63,790 ns | 224,808 ns | 572,490 ns | 142,321 ns | 183,657 ns |
| **SymmetricExceptWith** | 94,070 ns | 78,500 ns | 556,905 ns | 685,100 ns | 149,055 ns | 189,629 ns |

### Microbenchmarks

Edge-case and diagnostic benchmarks for the four `IMultiMap` implementations in the primary benchmark suite:

| Operation | MultiMapSet | MultiMapList | ConcurrentMultiMap | SortedMultiMap |
|---|---|---|---|---|
| **Add (duplicate)** | 36 ns | 32 ns | 303 ns | 29 ns |
| **Remove (missing key)** | 5 ns | 5 ns | 118 ns | 8 ns |
| **ContainsKey (missing)** | 4 ns | 5 ns | 118 ns | 8 ns |
| **ContainsKey + Get** | 34 ns | 28 ns | 622 ns | 23 ns |
| **Count after Add** | 30 ns | 23 ns | 664 ns | 19 ns |
| **Count after Remove** | 39 ns | 29 ns | 688 ns | 29 ns |
| **Clear (empty)** | 3.77 ns | 3.55 ns | 398 ns | 8.44 ns |
| **RemoveKey** | 36 ns | 27 ns | 307 ns | 24 ns |
| **Keys Enumeration** (100 keys) | 4,198 ns | 3,576 ns | 28,009 ns | 53,122 ns |

### SimpleMultiMap Operations

Benchmarks for the lightweight `SimpleMultiMap` (`ISimpleMultiMap` interface):

| Operation | SimpleMultiMap |
|---|---|
| **Add** (5,000 pairs) | 71,966 ns |
| **Add (duplicate)** | 34 ns |
| **Get** (100 keys) | 11,327 ns |
| **GetOrDefault** (100 keys) | 10,967 ns |
| **GetOrDefault (missing)** (100 keys) | 2,143 ns |
| **Remove** (5,000 pairs) | 117,249 ns |
| **Clear** (100 keys) | 155,339 ns |
| ~~**Flatten**~~ (deprecated) | 14,799 ns |
| **Enumerate** | 14,995 ns |
| **Count** | — ¹ |
| **Equals (equal maps)** | — ¹ |
| **Equals (different maps)** | — ¹ |

### Key Takeaways

- **AddRange vs Add**: `AddRange(key, values)` is significantly faster — `MultiMapList` **~7.3x**, `SortedMultiMap` **~6.1x**, `MultiMapAsync` **~3.5x**, `MultiMapLock` **~2.4x** faster than individual `Add` calls
- **Fastest adds**: `MultiMapList` (no uniqueness check) — **~2.1x faster** than `MultiMapSet`
- **Retrieval methods**: `Get()`, `GetOrDefault()`, and `TryGet()` offer comparable performance when keys exist; choose based on your error handling preference (exception, empty collection, or bool return)
- **KeyCount**: O(1) for `MultiMapSet`/`MultiMapList`/`SortedMultiMap` (< 1 ns). `ConcurrentMultiMap` enumerates entries filtering empty inner sets (~564 ns). `MultiMapLock` acquires a read lock (~17 ns). `MultiMapAsync` acquires a semaphore (~35 ns)
- **GetValuesCount**: Ultra-fast for non-concurrent implementations (3–4 ns) vs `SortedMultiMap` (114 ns tree lookup) and `MultiMapAsync` (39 ns with semaphore overhead)
- **RemoveWhere vs RemoveRange**: `RemoveWhere` operates on a single key (0.7–8.9 μs) and is **~70–469x faster** than `RemoveRange` across multiple keys (241 μs–1,582 μs)
- **ConcurrentMultiMap Count**: O(n) by summing inner `ConcurrentDictionary` sizes — ~59 µs for 100 keys × 50 values; no `Interlocked` counter needed in the lock-free design
- **SortedMultiMap**: Slowest across all operations due to tree-based data structures, but provides sorted enumeration. Keys Enumeration is **~15x slower** (53.1 μs vs 3.6 μs for `MultiMapList`)
- **Thread-safe overhead**: `ConcurrentMultiMap` is ~4.8x slower than `MultiMapSet` for adds (lock-free but higher allocation); `MultiMapLock` is ~2.8x slower
- **Async vs Lock**: `MultiMapLock` is faster than `MultiMapAsync` for adds (~203 μs vs ~291 μs) and reads (~14 μs vs ~22 μs for `Get`). Choose `MultiMapAsync` when you need `async`/`await` compatibility
- **SimpleMultiMap**: Lightweight alternative with performance between `MultiMapSet` and `MultiMapLock` — `Add` at 72 μs, `Get` at 11.3 μs. Benchmarks for the new `Count` and `Equals` members are included in the benchmark suite (see `SimpleMultiMapBenchmarks.cs`; marked ¹ above pending next benchmark run).

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

- **`SimpleMultiMap.Count` property added:** `SimpleMultiMap<TKey, TValue>` now exposes a `Count` property (inherited via `IReadOnlyCollection<KeyValuePair<TKey, TValue>>`) that returns the total number of key-value pairs across all keys (O(n) over keys, summing per-key `HashSet` sizes).

- **`SimpleMultiMap.Equals(IReadOnlySimpleMultiMap<TKey, TValue>? other)` added:** Typed equality compares the total pair count first (fast exit), then verifies per-key value-set contents using set-equality semantics.

- **`SimpleMultiMap` equality bug fix:** `Equals(IReadOnlySimpleMultiMap<TKey, TValue>? other)` previously compared `_dictionary.Count` (key count) against `other.Count` (total pair count), producing incorrect results when maps had equal key counts but different numbers of values. Fixed to compare total pair counts on both sides.

- **`MultiMapAsync` typed equality:**

- **`ConcurrentMultiMap` is now fully lock-free:** The internal storage changed from `ConcurrentDictionary<TKey, HashSet<TValue>>` with per-key locking and an `Interlocked` counter to `ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>`. All per-key read and write operations are now lock-free. `Count` is O(n) (sum of inner dictionary sizes) — no `Interlocked` counter is needed.

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
   dotnet add package MultiMap --version 1.0.12
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
