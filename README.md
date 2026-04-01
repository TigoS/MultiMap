# MultiMap

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![C# 14](https://img.shields.io/badge/C%23-14.0-blue)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![NUnit](https://img.shields.io/badge/tests-NUnit%204-green)](https://nunit.org/)
[![BenchmarkDotNet](https://img.shields.io/badge/BenchmarkDotNet-v0.15.0-blue)](https://benchmarkdotnet.org/)
[![NuGet](https://img.shields.io/nuget/v/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![Coverage](https://img.shields.io/badge/coverage-91.5%25-brightgreen)]()

A **.NET 10** library

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
- [License](#license)

## Overview

A **multimap** is a collection that maps each key to one or more values — unlike a standard `Dictionary<TKey, TValue>`, which allows only one value per key. This library provides **7 ready-to-use implementations** behind **3 interfaces**, so you can choose the right trade-off between uniqueness, ordering, thread-safety, and async support for your scenario. It also ships with **set-like extension methods** (`Union`, `Intersect`, `ExceptWith`, `SymmetricExceptWith`) that work across all implementations.

## Features

- **7 multimap implementations** covering a wide range of use cases
- **3 interfaces** (`IMultiMap`, `IMultiMapAsync`, `ISimpleMultiMap`) for flexibility
- **Set-like extension methods**: `Union`, `Intersect`, `ExceptWith`, `SymmetricExceptWith`
- **Thread-safe variants**: per-key locked (`ConcurrentMultiMap`), reader-writer locked (`MultiMapLock`), and async-safe (`MultiMapAsync`)
- **Full XML documentation** for IntelliSense support
- **714 unit tests** with NUnit 4
- **91.5% line coverage** via Coverlet (all 7 implementations at 100%)

## Project Structure

```
MultiMap/
├── MultiMap/                     # Core library (NuGet package)
│   ├── Interfaces/
│   │   ├── IMultiMap.cs          # Synchronous multimap interface
│   │   ├── IMultiMapAsync.cs     # Asynchronous multimap interface
│   │   └── ISimpleMultiMap.cs    # Simplified multimap interface
│   ├── Entities/
│   │   ├── MultiMapList.cs       # List-based (allows duplicates)
│   │   ├── MultiMapSet.cs        # HashSet-based (unique values)
│   │   ├── SortedMultiMap.cs     # SortedDictionary + SortedSet
│   │   ├── ConcurrentMultiMap.cs # ConcurrentDictionary + per-key locked HashSet
│   │   ├── MultiMapLock.cs       # ReaderWriterLockSlim-based
│   │   ├── MultiMapAsync.cs      # SemaphoreSlim-based async
│   │   └── SimpleMultiMap.cs     # Lightweight ISimpleMultiMap impl
│   └── Helpers/
│       ├── MultiMapHelper.cs     # Set-like extension methods
│       └── TestDataHelper.cs     # Sample data factory for demos
├── MultiMap.Tests/               # Unit tests (NUnit 4, 574 tests)
├── MultiMap.Demo/                # Console demo application
└── BenchmarkSuite/               # BenchmarkDotNet performance benchmarks
```

## Interfaces

### `IMultiMap<TKey, TValue>`

The standard synchronous multimap interface. Extends `IEnumerable<KeyValuePair<TKey, TValue>>`.

| Method | Returns | Description |
|---|---|---|
| `Add(key, value)` | `bool` | Adds a key-value pair; returns `false` if already present |
| `AddRange(key, values)` | `void` | Adds multiple values for a key |
| `Get(key)` | `IEnumerable<TValue>` | Returns values for a key (empty if not found) |
| `Remove(key, value)` | `bool` | Removes a specific key-value pair |
| `RemoveKey(key)` | `bool` | Removes a key and all its values |
| `ContainsKey(key)` | `bool` | Checks if a key exists |
| `Contains(key, value)` | `bool` | Checks if a specific key-value pair exists |
| `Clear()` | `void` | Removes all entries |
| `Count` | `int` | Total number of key-value pairs |
| `Keys` | `IEnumerable<TKey>` | All keys in the collection |

### `IMultiMapAsync<TKey, TValue>`

Asynchronous multimap interface. Extends `IAsyncEnumerable<KeyValuePair<TKey, TValue>>` and `IDisposable`. All methods support `CancellationToken` and return `ValueTask` or `Task`.

| Method | Returns | Description |
|---|---|---|
| `AddAsync(key, value)` | `ValueTask<bool>` | Asynchronously adds a key-value pair |
| `AddRangeAsync(key, values)` | `Task` | Asynchronously adds multiple values |
| `GetAsync(key)` | `ValueTask<IEnumerable<TValue>>` | Asynchronously retrieves values |
| `RemoveAsync(key, value)` | `ValueTask<bool>` | Asynchronously removes a pair |
| `RemoveKeyAsync(key)` | `ValueTask<bool>` | Asynchronously removes a key |
| `ContainsKeyAsync(key)` | `ValueTask<bool>` | Asynchronously checks for a key |
| `ContainsAsync(key, value)` | `ValueTask<bool>` | Asynchronously checks for a pair |
| `GetCountAsync()` | `ValueTask<int>` | Asynchronously gets total count |
| `ClearAsync()` | `Task` | Asynchronously clears all entries |
| `GetKeysAsync()` | `ValueTask<IEnumerable<TKey>>` | Asynchronously gets all keys |

### `ISimpleMultiMap<TKey, TValue>`

A simplified multimap interface. Extends `IEnumerable<KeyValuePair<TKey, TValue>>`.

| Method | Returns | Description |
|---|---|---|
| `Add(key, value)` | `bool` | Adds a key-value pair |
| `Get(key)` | `IEnumerable<TValue>` | Returns values; throws `KeyNotFoundException` if not found |
| `GetOrDefault(key)` | `IEnumerable<TValue>` | Returns values or empty if not found |
| `Remove(key, value)` | `void` | Removes a specific pair |
| `Clear(key)` | `void` | Removes all values for a key |
| `Flatten()` | `IEnumerable<KeyValuePair<TKey, TValue>>` | Returns all pairs as a flat sequence |

## Implementations

### `MultiMapList<TKey, TValue>` — List-Based

Implements `IMultiMap`. Uses `Dictionary<TKey, List<TValue>>` internally. **Allows duplicate values** per key. Fastest for add operations due to `List<T>.Add` being O(1) amortized.

### `MultiMapSet<TKey, TValue>` — HashSet-Based

Implements `IMultiMap`. Uses `Dictionary<TKey, HashSet<TValue>>` internally. **Ensures unique values** per key. Best for scenarios requiring fast lookups and unique value semantics.

### `SortedMultiMap<TKey, TValue>` — Sorted

Implements `IMultiMap`. Uses `SortedDictionary<TKey, SortedSet<TValue>>`. Keys and values are maintained in sorted order. Ideal for ordered enumeration and range queries.

### `ConcurrentMultiMap<TKey, TValue>` — Per-Key Locked Concurrent

Implements `IMultiMap`. Uses `ConcurrentDictionary<TKey, HashSet<TValue>>` with per-key `lock` for thread safety and an `Interlocked` counter for O(1) `Count`. A verify-after-lock pattern prevents count drift when concurrent `RemoveKey` invalidates a locked `HashSet`. Suitable for high-concurrency scenarios.

### `MultiMapLock<TKey, TValue>` — Reader-Writer Locked

Implements `IMultiMap` and `IDisposable`. Uses `ReaderWriterLockSlim` to allow concurrent reads with exclusive writes. Good for read-heavy workloads with occasional writes.

### `MultiMapAsync<TKey, TValue>` — Async-Safe

Implements `IMultiMapAsync` and `IAsyncEnumerable`. Uses `SemaphoreSlim` for async-compatible mutual exclusion. Designed for `async`/`await` patterns and I/O-bound scenarios.

### `SimpleMultiMap<TKey, TValue>` — Lightweight

Implements `ISimpleMultiMap`. A lightweight multimap with a simplified API. `Get` throws `KeyNotFoundException` if the key doesn't exist, while `GetOrDefault` returns an empty collection.

## Comparison Table

| Implementation | Interface | Thread-Safe | Duplicates | Ordered | Count Complexity |
|---|---|---|---|---|---|
| `MultiMapList` | `IMultiMap` | ❌ No | ✅ Yes | ❌ No | O(1) |
| `MultiMapSet` | `IMultiMap` | ❌ No | ❌ No | ❌ No | O(1) |
| `SortedMultiMap` | `IMultiMap` | ❌ No | ❌ No | ✅ Yes | O(1) |
| `ConcurrentMultiMap` | `IMultiMap` | Per-key lock | ❌ No | ❌ No | O(1) |
| `MultiMapLock` | `IMultiMap` | RW Lock | ❌ No | ❌ No | O(1) |
| `MultiMapAsync` | `IMultiMapAsync` | Semaphore | ❌ No | ❌ No | O(1) |
| `SimpleMultiMap` | `ISimpleMultiMap` | ❌ No | ❌ No | ❌ No | — |

### Internal Data Structures

| Implementation | Outer Structure | Inner Structure | Notes |
|---|---|---|---|
| `MultiMapList` | `Dictionary<TKey, List<TValue>>` | `List<TValue>` | O(1) amortized add; allows duplicate values |
| `MultiMapSet` | `Dictionary<TKey, HashSet<TValue>>` | `HashSet<TValue>` | O(1) add/contains; enforces unique values |
| `SortedMultiMap` | `SortedDictionary<TKey, SortedSet<TValue>>` | `SortedSet<TValue>` | O(log n) operations; keys & values sorted |
| `ConcurrentMultiMap` | `ConcurrentDictionary<TKey, HashSet<TValue>>` | `HashSet<TValue>` | Per-key `lock`; `Interlocked` counter for O(1) Count |
| `MultiMapLock` | `Dictionary<TKey, HashSet<TValue>>` | `HashSet<TValue>` | Protected by `ReaderWriterLockSlim` |
| `MultiMapAsync` | `Dictionary<TKey, HashSet<TValue>>` | `HashSet<TValue>` | Protected by `SemaphoreSlim(1,1)` |
| `SimpleMultiMap` | `Dictionary<TKey, HashSet<TValue>>` | `HashSet<TValue>` | Simplified API surface |

### API Behavior Differences

| Behavior | `IMultiMap` | `IMultiMapAsync` | `ISimpleMultiMap` |
|---|---|---|---|
| **Get (missing key)** | Returns empty `IEnumerable` | Returns empty `IEnumerable` | `Get` throws `KeyNotFoundException`; `GetOrDefault` returns empty |
| **Add (duplicate)** | Returns `false` | Returns `false` (via `ValueTask<bool>`) | Returns `false` |
| **Remove return type** | `bool` | `ValueTask<bool>` | `void` |
| **Enumeration** | `IEnumerable<KeyValuePair>` | `IAsyncEnumerable<KeyValuePair>` | `IEnumerable<KeyValuePair>` (+ `Flatten()`) |
| **Disposable** | Only `MultiMapLock` | ✅ Yes (`IDisposable`) | ❌ No |
| **CancellationToken** | ❌ No | ✅ Yes (all methods) | ❌ No |

### When to Use Which Implementation

| Use Case | Recommended Implementation | Reason |
|---|---|---|
| General purpose, unique values | `MultiMapSet` | Fast O(1) lookups with uniqueness guarantee |
| Duplicate values needed | `MultiMapList` | Only implementation allowing duplicate values per key |
| Sorted enumeration / range queries | `SortedMultiMap` | Maintains key and value ordering |
| High-concurrency, many threads | `ConcurrentMultiMap` | Per-key locking scales well with thread count |
| Read-heavy, occasional writes | `MultiMapLock` | RW lock allows concurrent readers |
| Async / I/O-bound code | `MultiMapAsync` | `SemaphoreSlim` works with `async`/`await` |
| Minimal API, quick prototyping | `SimpleMultiMap` | Simplified interface with `Flatten()` and `GetOrDefault` |

### Performance Comparison (5,000 pairs)

| Implementation | Add | Get (100 keys) | Contains | Count | Relative Add Speed |
|---|---|---|---|---|---|
| `MultiMapList` | 33,087 ns | 7,939 ns | 27 ns | 0.027 ns | **1.0x** (baseline) |
| `MultiMapSet` | 66,382 ns | 9,197 ns | 33 ns | 0.028 ns | 2.0x |
| `MultiMapLock` | 114,596 ns | 13,147 ns | 15 ns | 10 ns | 3.5x |
| `ConcurrentMultiMap` | 140,229 ns | 12,841 ns | 147 ns | 0.028 ns | 4.2x |
| `MultiMapAsync` | 190,839 ns | 14,267 ns | 32 ns | 30 ns | 5.8x |
| `SortedMultiMap` | 826,793 ns | 41,874 ns | 24 ns | 0.026 ns | 25.0x |

> **Note:** Performance data from BenchmarkDotNet. See [Benchmarks](#benchmarks) for full details.

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
<PackageReference Include="MultiMap" Version="1.0.4" />
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

### Concurrent Usage

```csharp
using MultiMap.Entities;

var concurrentMap = new ConcurrentMultiMap<string, int>();

// Safe to call from multiple threads
Parallel.For(0, 1000, i =>
{
    concurrentMap.Add("key", i);
});
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
var flat = map.Flatten();                 // all key-value pairs

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

The library includes **574 unit tests** written with **NUnit 4**, covering all implementations, interfaces, edge cases, and concurrent stress tests.

```shell
dotnet test
```

### Test Coverage by Implementation

| Test Class | Tests | Category |
|---|---|---|
| `MultiMapAsyncTests` | 92 | Async implementation |
| `MultiMapLockTests` | 67 | RW Lock implementation |
| `ConcurrentMultiMapTests` | 61 | Per-key locked concurrent implementation |
| `SortedMultiMapTests` | 58 | Sorted implementation |
| `MultiMapSetTests` | 55 | HashSet-based implementation |
| `MultiMapListTests` | 54 | List-based implementation |
| `SimpleMultiMapTests` | 33 | Lightweight implementation |
| **Entity subtotal** | **420** | |

### Test Coverage by Extension Methods

| Test Class | Tests | Category |
|---|---|---|
| `MultiMapHelperAsyncTests` | 65 | Async extension methods (`UnionAsync`, `IntersectAsync`, etc.) |
| `MultiMapHelperWithMultiMapSetTests` | 34 | Extensions with `MultiMapSet` + stress tests |
| `MultiMapHelperTests` | 28 | `IMultiMap` extensions (primary) |
| `SimpleMultiMapHelperTests` | 28 | `ISimpleMultiMap` extensions |
| `MultiMapHelperWithSortedMultiMapEdgeCaseTests` | 24 | Edge cases with `SortedMultiMap` |
| `MultiMapHelperWithConcurrentMultiMapEdgeCaseTests` | 24 | Edge cases with `ConcurrentMultiMap` |
| `MultiMapHelperWithMultiMapLockEdgeCaseTests` | 24 | Edge cases with `MultiMapLock` |
| `MultiMapHelperWithMultiMapListEdgeCaseTests` | 23 | Edge cases with `MultiMapList` |
| `MultiMapHelperWithMultiMapLockTests` | 12 | Extensions + concurrent stress tests |
| `MultiMapHelperWithConcurrentMultiMapTests` | 12 | Extensions + concurrent stress tests |
| `MultiMapHelperWithMultiMapListTests` | 10 | Extensions with `MultiMapList` + stress tests |
| `MultiMapHelperWithSortedMultiMapTests` | 10 | Extensions with `SortedMultiMap` + stress tests |
| **Helper subtotal** | **294** | |

| | |
|---|---|
| **Total** | **714 tests** |

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
| `MultiMapAsyncTests` | 92 | 12.9% |
| `MultiMapLockTests` | 67 | 9.4% |
| `ConcurrentMultiMapTests` | 61 | 8.5% |
| `SortedMultiMapTests` | 58 | 8.1% |
| `MultiMapSetTests` | 55 | 7.7% |
| `MultiMapListTests` | 54 | 7.6% |
| `SimpleMultiMapTests` | 33 | 4.6% |
| **Entity subtotal** | **420** | **58.8%** |
| `MultiMapHelperAsyncTests` | 65 | 9.1% |
| `MultiMapHelperWithMultiMapSetTests` | 34 | 4.8% |
| `MultiMapHelperTests` | 28 | 3.9% |
| `SimpleMultiMapHelperTests` | 28 | 3.9% |
| `MultiMapHelperWithSortedMultiMapEdgeCaseTests` | 24 | 3.4% |
| `MultiMapHelperWithConcurrentMultiMapEdgeCaseTests` | 24 | 3.4% |
| `MultiMapHelperWithMultiMapLockEdgeCaseTests` | 24 | 3.4% |
| `MultiMapHelperWithMultiMapListEdgeCaseTests` | 23 | 3.2% |
| `MultiMapHelperWithMultiMapLockTests` | 12 | 1.7% |
| `MultiMapHelperWithConcurrentMultiMapTests` | 12 | 1.7% |
| `MultiMapHelperWithMultiMapListTests` | 10 | 1.4% |
| `MultiMapHelperWithSortedMultiMapTests` | 10 | 1.4% |
| **Helper subtotal** | **294** | **41.2%** |
| **Total** | **714** | **100%** |

> **Coverage distribution:** ~59% of tests target the 7 core implementations, while ~41% cover the set-like extension methods across all interface families — including concurrent and sequential stress tests, edge cases, and deep iteration tests that exercise helpers with all implementations.

### Code Coverage (Coverlet)

Code coverage is collected with **Coverlet** (`coverlet.collector`) during `dotnet test` and reported via **ReportGenerator**.

```shell
dotnet test --collect:"XPlat Code Coverage"
```

#### Summary

| Metric | Value |
|---|---|
| **Line coverage** | **91.5%** (1,138 / 1,243 lines) |
| **Branch coverage** | **89.9%** (349 / 388 branches) |
| **Method coverage** | **95%** (135 / 142 methods) |

#### Per-Class Breakdown

| Class | Line Coverage | Branch Coverage | Status |
|---|---|---|---|
| `ConcurrentMultiMap<TKey, TValue>` | 100% | 100% | ✅ Full |
| `MultiMapAsync<TKey, TValue>` | 100% | 100% | ✅ Full |
| `MultiMapList<TKey, TValue>` | 100% | 100% | ✅ Full |
| `MultiMapLock<TKey, TValue>` | 100% | 100% | ✅ Full |
| `MultiMapSet<TKey, TValue>` | 100% | 100% | ✅ Full |
| `SimpleMultiMap<TKey, TValue>` | 100% | 100% | ✅ Full |
| `SortedMultiMap<TKey, TValue>` | 100% | 100% | ✅ Full |
| `MultiMapHelper` | 63.5% | — | ⚠️ Partial |
| `TestDataHelper` | 0% | — | ➖ Demo only |

> **Notes:**
> - **All 7 entity implementations** achieve **100% line and branch coverage**.
> - `MultiMapHelper` main class body (sync + async method signatures) is **100% covered**; the aggregate (63.5%) is lowered by compiler-generated async state-machine wrapper classes (`<UnionAsync>d__8`, `<IntersectAsync>d__9`, etc.) which duplicate the logic already exercised through `MultiMapAsync`'s own set-operation methods.
> - `TestDataHelper` is a demo-only data factory used by `MultiMap.Demo` — excluded from quality targets.

## Benchmarks

Benchmarks are run with **BenchmarkDotNet v0.15.0** using `DefaultJob` with `CPUUsageDiagnoser`.

**Environment:** .NET 10.0, 13th Gen Intel Core i9-13900H, 20 logical / 14 physical cores, RyuJIT AVX2

**Benchmark Parameters:** 100 keys x 50 values/key for bulk operations (5,000 pairs); 50 keys x 20 values/key for set operations (1,000 pairs).

### Core Operations

| Operation | MultiMapSet | MultiMapList | ConcurrentMultiMap | SortedMultiMap | MultiMapLock | MultiMapAsync |
|---|---|---|---|---|---|---|
| **Add** (5,000 pairs) | 66,382 ns | 33,087 ns | 140,229 ns | 826,793 ns | 114,596 ns | 190,839 ns |
| **AddRange** (5,000 pairs) | 45,696 ns | 4,618 ns | 60,926 ns | 141,084 ns | 44,983 ns | 44,209 ns |
| **Get** (100 keys) | 9,197 ns | 7,939 ns | 12,841 ns | 41,874 ns | 13,147 ns | 14,267 ns |
| **Remove** (5,000 pairs) | 126,430 ns | 121,723 ns | 262,223 ns | 1,509,576 ns | 210,443 ns | 351,489 ns |
| **Clear** | 159,103 ns | 120,527 ns | 249,991 ns | 935,291 ns | 197,853 ns | 248,076 ns |
| **Contains** | 33 ns | 27 ns | 147 ns | 24 ns | 15 ns | 32 ns |
| **ContainsKey** | 31 ns | 26 ns | 142 ns | 21 ns | 13 ns | 27 ns |
| **Count** | 0.028 ns | 0.027 ns | 0.028 ns | 0.026 ns | 10 ns | 30 ns |
| **GetKeys** | 32 ns | 29 ns | 336 ns | 24 ns | 164 ns | 187 ns |

### Set Operations (via `MultiMapHelper`)

| Operation | MultiMapSet | MultiMapList | ConcurrentMultiMap | SortedMultiMap | MultiMapLock | MultiMapAsync |
|---|---|---|---|---|---|---|
| **Union** | 83,740 ns | 57,700 ns | 116,170 ns | 402,100 ns | 98,520 ns | 120,980 ns |
| **Intersect** | 80,180 ns | 65,440 ns | 113,870 ns | 417,060 ns | 101,160 ns | 123,760 ns |
| **ExceptWith** | 82,610 ns | 64,520 ns | 115,920 ns | 522,300 ns | 94,700 ns | 115,540 ns |
| **SymmetricExceptWith** | 100,710 ns | 79,930 ns | 143,950 ns | 600,200 ns | 100,520 ns | 124,160 ns |

### Key Takeaways

- **AddRange vs Add**: `AddRange` is significantly faster — `MultiMapList` **~7x faster**, `SortedMultiMap` **~6x faster**, `MultiMapLock` **~2.5x faster**, `MultiMapAsync` **~4.3x faster** than individual `Add` calls
- **Fastest adds**: `MultiMapList` (no uniqueness check) — **~2x faster** than `MultiMapSet`
- **Fastest lookups**: `SortedMultiMap` Contains at 24 ns; `MultiMapLock` Contains at 15 ns
- **ConcurrentMultiMap Count**: Now O(1) via `Interlocked` counter — 0.028 ns, on par with non-concurrent implementations
- **SortedMultiMap**: Slowest across all operations due to tree-based data structures, but provides sorted enumeration
- **Thread-safe overhead**: `ConcurrentMultiMap` is ~2.1x slower than `MultiMapSet` for adds; `MultiMapLock` is ~1.7x slower
- **Async overhead**: `MultiMapAsync` is comparable to `MultiMapLock` for reads; ~1.7x slower for adds due to `SemaphoreSlim`

## License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).

## Author

**TigoS** — [GitHub](https://github.com/TigoS/MultiMap)
