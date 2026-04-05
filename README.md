# MultiMap

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![C# 14](https://img.shields.io/badge/C%23-14.0-blue)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![NUnit](https://img.shields.io/badge/tests-NUnit%204-green)](https://nunit.org/)
[![BenchmarkDotNet](https://img.shields.io/badge/BenchmarkDotNet-v0.15.0-blue)](https://benchmarkdotnet.org/)
[![NuGet](https://img.shields.io/nuget/v/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![Coverage](https://img.shields.io/badge/coverage-94.6%25-brightgreen)]()

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
- [Release Notes](#release-notes)
- [License](#license)

## Overview

A **multimap** is a collection that maps each key to one or more values — unlike a standard `Dictionary<TKey, TValue>`, which allows only one value per key. This library provides **7 ready-to-use implementations** behind **3 interfaces**, so you can choose the right trade-off between uniqueness, ordering, thread-safety, and async support for your scenario. It also ships with **set-like extension methods** (`Union`, `Intersect`, `ExceptWith`, `SymmetricExceptWith`) that work across all implementations.

## Features

- **7 multimap implementations** covering a wide range of use cases
- **3 interfaces** (`IMultiMap`, `IMultiMapAsync`, `ISimpleMultiMap`) for flexibility
- **Set-like extension methods**: `Union`, `Intersect`, `ExceptWith`, `SymmetricExceptWith`
- **Thread-safe variants**: per-key locked (`ConcurrentMultiMap`), reader-writer locked (`MultiMapLock`), and async-safe (`MultiMapAsync`)
- **Full XML documentation** for IntelliSense support
- **1023 unit tests** with NUnit 4
- **94.6% line coverage** via Coverlet (5 of 7 implementations at 100%)

## Project Structure

```
MultiMap/
├── MultiMap/                           # Core library (NuGet package)
│   ├── Interfaces/
│   │   ├── IReadOnlySimpleMultiMap.cs  # Base read-only interface
│   │   ├── IReadOnlyMultiMap.cs        # Extended read-only with TryGet, Contains, KeyCount
│   │   ├── IReadOnlyMultiMapAsync.cs   # Async read-only with cancellation support
│   │   ├── IMultiMap.cs                # Synchronous multimap (extends IReadOnlyMultiMap)
│   │   ├── IMultiMapAsync.cs           # Async multimap (extends IReadOnlyMultiMapAsync)
│   │   └── ISimpleMultiMap.cs          # Simplified interface (extends IReadOnlySimpleMultiMap)
│   ├── Entities/
│   │   ├── MultiMapList.cs             # List-based (allows duplicates)
│   │   ├── MultiMapSet.cs              # HashSet-based (unique values)
│   │   ├── SortedMultiMap.cs           # SortedDictionary + SortedSet
│   │   ├── ConcurrentMultiMap.cs       # ConcurrentDictionary + per-key locked HashSet
│   │   ├── MultiMapLock.cs             # ReaderWriterLockSlim-based
│   │   ├── MultiMapAsync.cs            # SemaphoreSlim-based async
│   │   └── SimpleMultiMap.cs           # Lightweight ISimpleMultiMap impl
│   └── Helpers/
│       ├── MultiMapHelper.cs           # Set-like extension methods
│       └── TestDataHelper.cs           # Sample data factory for demos
├── MultiMap.Tests/                     # Unit tests (NUnit 4, 1023 tests)
├── MultiMap.Demo/                      # Console demo application
└── BenchmarkSuite/                     # BenchmarkDotNet performance benchmarks
```

## Interfaces

### Interface Hierarchy

The library follows a hierarchical interface design with three parallel families:

**Read-Only Interfaces:**
- `IReadOnlySimpleMultiMap<TKey, TValue>` — Base read-only interface with `Get`, `GetOrDefault`
- `IReadOnlyMultiMap<TKey, TValue>` — Extends `IReadOnlySimpleMultiMap` with `TryGet`, `Contains`, `ContainsKey`, `KeyCount`
- `IReadOnlyMultiMapAsync<TKey, TValue>` — Async read-only with `GetAsync`, `TryGetAsync`, `ContainsAsync`, etc.

**Mutable Interfaces:**
- `ISimpleMultiMap<TKey, TValue>` — Extends `IReadOnlySimpleMultiMap` with `Add`, `Remove`, `Clear`
- `IMultiMap<TKey, TValue>` — Extends `IReadOnlyMultiMap` with `Add`, `AddRange`, `Remove`, `RemoveKey`, `Clear`
- `IMultiMapAsync<TKey, TValue>` — Extends `IReadOnlyMultiMapAsync` with async mutations and `CancellationToken` support

### `IReadOnlySimpleMultiMap<TKey, TValue>`

The base read-only interface. Extends `IEnumerable<KeyValuePair<TKey, TValue>>`.

| Method | Returns | Description |
|---|---|---|
| `Get(key)` | `IEnumerable<TValue>` | Returns values; throws `KeyNotFoundException` if not found |
| `GetOrDefault(key)` | `IEnumerable<TValue>` | Returns values or empty if not found |

### `IReadOnlyMultiMap<TKey, TValue>`

Extended read-only interface. Extends `IReadOnlySimpleMultiMap<TKey, TValue>`.

| Method | Returns | Description |
|---|---|---|
| `TryGet(key, out values)` | `bool` | Attempts to retrieve values; returns `true` if key exists |
| `ContainsKey(key)` | `bool` | Checks if a key exists |
| `Contains(key, value)` | `bool` | Checks if a specific key-value pair exists |
| `KeyCount` | `int` | Gets the number of keys (not total pairs) |

### `IMultiMap<TKey, TValue>`

The standard synchronous multimap interface. Extends `IReadOnlyMultiMap<TKey, TValue>`.

| Method | Returns | Description |
|---|---|---|
| `Add(key, value)` | `bool` | Adds a key-value pair; returns `false` if already present |
| `AddRange(key, values)` | `void` | Adds multiple values for a key |
| `AddRange(items)` | `void` | Adds multiple key-value pairs |
| `Remove(key, value)` | `bool` | Removes a specific key-value pair |
| `RemoveRange(items)` | `int` | Removes multiple key-value pairs; returns count removed |
| `RemoveWhere(key, predicate)` | `int` | Removes values matching predicate; returns count removed |
| `RemoveKey(key)` | `bool` | Removes a key and all its values |
| `Clear()` | `void` | Removes all entries |

**Inherited from `IReadOnlyMultiMap`:** `Get`, `GetOrDefault`, `TryGet`, `ContainsKey`, `Contains`, `KeyCount`

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

### `IMultiMapAsync<TKey, TValue>`

Asynchronous multimap interface. Extends `IReadOnlyMultiMapAsync<TKey, TValue>`. All methods support `CancellationToken` and return `ValueTask` or `Task`.

| Method | Returns | Description |
|---|---|---|
| `AddAsync(key, value)` | `ValueTask<bool>` | Asynchronously adds a key-value pair |
| `AddRangeAsync(key, values)` | `Task` | Asynchronously adds multiple values |
| `AddRangeAsync(items)` | `Task` | Asynchronously adds multiple key-value pairs |
| `RemoveAsync(key, value)` | `ValueTask<bool>` | Asynchronously removes a pair |
| `RemoveRangeAsync(items)` | `ValueTask<int>` | Asynchronously removes multiple pairs; returns count removed |
| `RemoveWhereAsync(key, predicate)` | `ValueTask<int>` | Asynchronously removes values matching predicate; returns count removed |
| `RemoveKeyAsync(key)` | `ValueTask<bool>` | Asynchronously removes a key |
| `ClearAsync()` | `Task` | Asynchronously clears all entries |

**Inherited from `IReadOnlyMultiMapAsync`:** `GetAsync`, `GetOrDefaultAsync`, `TryGetAsync`, `ContainsKeyAsync`, `ContainsAsync`, `GetCountAsync`, `GetKeyCountAsync`, `GetKeysAsync`, `GetValuesCountAsync`

### `ISimpleMultiMap<TKey, TValue>`

A simplified multimap interface. Extends `IReadOnlySimpleMultiMap<TKey, TValue>`.

| Method | Returns | Description |
|---|---|---|
| `Add(key, value)` | `bool` | Adds a key-value pair |
| `Remove(key, value)` | `void` | Removes a specific pair |
| `Clear(key)` | `void` | Removes all values for a key |
| `Flatten()` | `IEnumerable<KeyValuePair<TKey, TValue>>` | Returns all pairs as a flat sequence |

**Inherited from `IReadOnlySimpleMultiMap`:** `Get`, `GetOrDefault`

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
| **Interface Hierarchy** | Extends `IReadOnlyMultiMap` → `IReadOnlySimpleMultiMap` | Extends `IReadOnlyMultiMapAsync` | Extends `IReadOnlySimpleMultiMap` |
| **Get (missing key)** | `Get` throws `KeyNotFoundException`; `GetOrDefault` returns empty | `GetAsync` throws `KeyNotFoundException`; `GetOrDefaultAsync` returns empty | `Get` throws `KeyNotFoundException`; `GetOrDefault` returns empty |
| **TryGet (missing key)** | `TryGet` returns `false` with empty collection | `TryGetAsync` returns `(false, empty)` tuple | Not available |
| **KeyCount property** | ✅ `KeyCount` property (number of unique keys) | ✅ `GetKeyCountAsync()` method | ❌ Not available |
| **Add (duplicate)** | Returns `false` | Returns `false` (via `ValueTask<bool>`) | Returns `false` |
| **AddRange** | `AddRange(key, values)` and `AddRange(items)` | `AddRangeAsync(key, values)` and `AddRangeAsync(items)` | Not available |
| **Remove return type** | `bool` | `ValueTask<bool>` | `void` |
| **RemoveRange** | `RemoveRange(items)` returns `int` | `RemoveRangeAsync(items)` returns `ValueTask<int>` | Not available |
| **RemoveWhere** | `RemoveWhere(key, predicate)` returns `int` | `RemoveWhereAsync(key, predicate)` returns `ValueTask<int>` | Not available |
| **GetValuesCount** | Not available | `GetValuesCountAsync(key)` returns `ValueTask<int>` | Not available |
| **Enumeration** | `IEnumerable<KeyValuePair>` | `IAsyncEnumerable<KeyValuePair>` | `IEnumerable<KeyValuePair>` (+ `Flatten()`) |
| **Disposable** | Only `MultiMapLock` | ✅ Yes (`IAsyncDisposable` + `IDisposable`) | ❌ No |
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
| `MultiMapList` | 58,270 ns | 12,334 ns | 27 ns | < 1 ns | **1.0x** (baseline) |
| `MultiMapSet` | 128,527 ns | 14,103 ns | 36 ns | < 1 ns | 2.2x |
| `MultiMapAsync` | 198,333 ns | 13,804 ns | 28 ns | 28 ns | 3.4x |
| `MultiMapLock` | 202,264 ns | 19,906 ns | 25 ns | 16 ns | 3.5x |
| `ConcurrentMultiMap` | 247,160 ns | 21,862 ns | 143 ns | < 1 ns | 4.2x |
| `SortedMultiMap` | 1,539,825 ns | 66,734 ns | 23 ns | < 1 ns | 26.4x |

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
<PackageReference Include="MultiMap" Version="1.0.8" />
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

The library includes **1023 unit tests** written with **NUnit 4**, covering all implementations, interfaces, edge cases, and concurrent stress tests.

```shell
dotnet test
```

### Test Coverage by Implementation

| Test Class | Tests | Category |
|---|---|---|
| `MultiMapAsyncTests` | 148 | Async implementation |
| `ConcurrentMultiMapTests` | 119 | Per-key locked concurrent implementation |
| `MultiMapSetTests` | 119 | HashSet-based implementation |
| `SortedMultiMapTests` | 127 | Sorted implementation |
| `MultiMapListTests` | 121 | List-based implementation |
| `MultiMapLockTests` | 103 | RW Lock implementation |
| `SimpleMultiMapTests` | 33 | Lightweight implementation |
| **Entity subtotal** | **770** | |

### Test Coverage by Extension Methods

| Test Class | Tests | Category |
|---|---|---|
| `MultiMapHelperAsyncTests` | 95 | Async extension methods (`UnionAsync`, `IntersectAsync`, etc.) |
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
| **Helper subtotal** | **324** | |

| | |
|---|---|
| **Total** | **1023 tests** |

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
| `MultiMapAsyncTests` | 148 | 14.5% |
| `ConcurrentMultiMapTests` | 119 | 11.6% |
| `MultiMapSetTests` | 119 | 11.6% |
| `SortedMultiMapTests` | 127 | 12.4% |
| `MultiMapListTests` | 121 | 11.8% |
| `MultiMapLockTests` | 103 | 10.1% |
| `SimpleMultiMapTests` | 33 | 3.2% |
| **Entity subtotal** | **770** | **75.3%** |
| `MultiMapHelperAsyncTests` | 95 | 9.3% |
| `MultiMapHelperWithMultiMapSetTests` | 34 | 3.3% |
| `MultiMapHelperTests` | 28 | 2.7% |
| `SimpleMultiMapHelperTests` | 28 | 2.7% |
| `MultiMapHelperWithSortedMultiMapEdgeCaseTests` | 24 | 2.3% |
| `MultiMapHelperWithConcurrentMultiMapEdgeCaseTests` | 24 | 2.3% |
| `MultiMapHelperWithMultiMapLockEdgeCaseTests` | 24 | 2.3% |
| `MultiMapHelperWithMultiMapListEdgeCaseTests` | 23 | 2.2% |
| `MultiMapHelperWithMultiMapLockTests` | 12 | 1.2% |
| `MultiMapHelperWithConcurrentMultiMapTests` | 12 | 1.2% |
| `MultiMapHelperWithMultiMapListTests` | 10 | 1.0% |
| `MultiMapHelperWithSortedMultiMapTests` | 10 | 1.0% |
| **Helper subtotal** | **324** | **31.7%** |
| **Total** | **1023** | **100%** |

> **Coverage distribution:** ~75% of tests target the 7 core implementations (including new interface member tests, concurrent stress tests, and slow path contention tests), while ~32% cover the set-like extension methods across all interface families — including concurrent and sequential stress tests, edge cases, deep iteration tests that exercise helpers with all implementations, and comprehensive tests for async extension methods in MultiMapHelper.

### Code Coverage (Coverlet)

Code coverage is collected with **Coverlet** (`coverlet.collector`) during `dotnet test` and reported via **ReportGenerator**.

```shell
dotnet test --collect:"XPlat Code Coverage"
```

#### Summary

| Metric | Value |
|---|---|
| **Line coverage** | **94.6%** (MultiMap assembly) |
| **Branch coverage** | **89.1%** (MultiMap assembly) |
| **Method coverage** | **100%** (All public methods) |

#### Per-Class Breakdown

| Class | Line Coverage | Branch Coverage | Status |
|---|---|---|---|
| `ConcurrentMultiMap<TKey, TValue>` | 98.8% | 97.2% | ✅ Full |
| `MultiMapAsync<TKey, TValue>` | 89.3% | 90.0% | ✅ Full |
| `MultiMapList<TKey, TValue>` | 100% | 100% | ✅ Full |
| `MultiMapLock<TKey, TValue>` | 100% | 95.4% | ✅ Full |
| `MultiMapSet<TKey, TValue>` | 100% | 100% | ✅ Full |
| `SimpleMultiMap<TKey, TValue>` | 100% | 100% | ✅ Full |
| `SortedMultiMap<TKey, TValue>` | 100% | 100% | ✅ Full |
| `MultiMapHelper` | 100% | 100% | ✅ Full |
| `Program` | 0% | 0% | ➖ Entry point only |

> **Notes:**
> - **5 of 7 entity implementations** achieve **100% line coverage**.
> - `ConcurrentMultiMap` achieves **98.8% line coverage**. Uncovered lines are verify-after-lock race condition paths that are extremely difficult to trigger consistently.
> - `MultiMapAsync` achieves **89.3% line coverage** with async edge cases in slow paths under high concurrency.
> - `MultiMapHelper` achieves **100% line and branch coverage** for all extension methods, including async variants.
> - `MultiMapLock` has 95.4% branch coverage due to async/dispose pattern edge cases.
> - `Program` class (0% coverage) is the application entry point for the Demo project — excluded from quality targets.

## Benchmarks

Benchmarks are run with **BenchmarkDotNet v0.15.0** with `CPUUsageDiagnoser`.

**Environment:** .NET 10.0.5, 13th Gen Intel Core i9-13900H, 20 logical / 14 physical cores, RyuJIT AVX2

**Benchmark Parameters:** 100 keys × 50 values/key for bulk operations (5,000 pairs); 50 keys × 20 values/key for set operations (1,000 pairs).

### Core Operations

| Operation | MultiMapSet | MultiMapList | ConcurrentMultiMap | SortedMultiMap | MultiMapLock | MultiMapAsync |
|---|---|---|---|---|---|---|
| **Add** (5,000 pairs) | 128,527 ns | 58,270 ns | 247,160 ns | 1,539,825 ns | 202,264 ns | 198,333 ns |
| **AddRange** (key, values) | 82,139 ns | 7,999 ns | 97,471 ns | 230,223 ns | 83,230 ns | 49,518 ns |
| **Get** (100 keys) | 14,103 ns | 12,334 ns | 21,862 ns | 66,734 ns | 19,906 ns | 13,804 ns |
| **GetOrDefault** (100 keys) | 14,192 ns | 12,843 ns | 22,509 ns | 72,187 ns | 19,899 ns | 14,058 ns |
| **TryGet** | 63 ns | 54 ns | 262 ns | 42 ns | 109 ns | 30 ns |
| **Remove** (5,000 pairs) | 121,539 ns | 124,712 ns | 252,655 ns | 1,713,777 ns | 363,367 ns | 359,317 ns |
| **Clear** | 153,711 ns | 117,408 ns | 229,211 ns | 992,824 ns | 288,222 ns | 244,770 ns |
| **Contains** | 36 ns | 27 ns | 143 ns | 23 ns | 25 ns | 28 ns |
| **ContainsKey** | 31 ns | 26 ns | 139 ns | 22 ns | 23 ns | 28 ns |
| **Count** / **GetCount** | < 1 ns | < 1 ns | < 1 ns | < 1 ns | 16 ns | 28 ns |
| **GetKeys** | 60 ns | 48 ns | 544 ns | 43 ns | 336 ns | 194 ns |

### New Interface Members

Benchmarks for properties and methods introduced in v1.0.8+. Async equivalents are shown for `MultiMapAsync`.

| Operation | MultiMapSet | MultiMapList | ConcurrentMultiMap | SortedMultiMap | MultiMapLock | MultiMapAsync |
|---|---|---|---|---|---|---|
| **KeyCount** / **GetKeyCountAsync** | 0.36 ns | 0.33 ns | 987 ns | 0.12 ns | 327 ns | 28 ns |
| **Values** / **GetValuesAsync** | 12,069 ns | 11,535 ns | 16,435 ns | 37,411 ns | 10,586 ns | 15,007 ns |
| **GetValuesCount** / **GetValuesCountAsync** | 3.67 ns | 3.49 ns | 14.52 ns | 118 ns | 14 ns | 221 ns |
| **Indexer** (`this[key]`) | 3.69 ns | 3.63 ns | 59 ns | 113 ns | 67 ns | — |
| **AddRange(items)** / **AddRangeAsync(items)** | 244,890 ns | 197,502 ns | 308,306 ns | 983,963 ns | 285,335 ns | 241,920 ns |
| **RemoveRange** / **RemoveRangeAsync** | 274,189 ns | 244,911 ns | 368,309 ns | 1,412,023 ns | 331,751 ns | 371,435 ns |
| **RemoveWhere** / **RemoveWhereAsync** | 2,052 ns | 1,854 ns | 4,004 ns | 3,642 ns | 3,545 ns | 4,618 ns |

> **Notes:**
> - **KeyCount**: `ConcurrentMultiMap` (~987 ns) must enumerate all keys in the `ConcurrentDictionary`, while `MultiMapSet`/`MultiMapList`/`SortedMultiMap` expose a direct O(1) property (< 1 ns). `MultiMapLock` acquires a read lock (~327 ns). `MultiMapAsync` acquires a semaphore (~28 ns).
> - **Indexer**: Not available for `MultiMapAsync` (async API uses `GetAsync` instead).
> - **AddRange(items)**: The KVP overload is ~3x slower than `AddRange(key, values)` because it groups items by key and processes multiple keys across the map.
> - **RemoveWhere**: Very efficient (1.9–4.6 μs) compared to `RemoveRange` (245 μs–1,412 μs) because it operates on a single key's value set.

### Set Operations (via `MultiMapHelper`)

| Operation | MultiMapSet | MultiMapList | ConcurrentMultiMap | SortedMultiMap | MultiMapLock | MultiMapAsync |
|---|---|---|---|---|---|---|
| **Union** | 119,600 ns | 83,050 ns | 170,410 ns | 700,060 ns | 160,768 ns | 122,223 ns |
| **Intersect** | 108,690 ns | 98,090 ns | 180,960 ns | 676,260 ns | 154,151 ns | 122,718 ns |
| **ExceptWith** | 114,410 ns | 95,820 ns | 185,800 ns | 943,490 ns | 142,395 ns | 119,316 ns |
| **SymmetricExceptWith** | 148,150 ns | 122,280 ns | 225,560 ns | 1,013,870 ns | 155,785 ns | 125,698 ns |

### Microbenchmarks

Edge-case and diagnostic benchmarks for the four `IMultiMap` implementations in the primary benchmark suite:

| Operation | MultiMapSet | MultiMapList | ConcurrentMultiMap | SortedMultiMap |
|---|---|---|---|---|
| **Add (duplicate)** | 35 ns | 31 ns | 156 ns | 30 ns |
| **Remove (missing key)** | 5 ns | 6 ns | 89 ns | 8 ns |
| **ContainsKey (missing)** | 4 ns | 4 ns | 90 ns | 9 ns |
| **ContainsKey + Get** | 38 ns | 34 ns | 179 ns | 44 ns |
| **Count after Add** | 29 ns | 26 ns | 144 ns | 23 ns |
| **Count after Remove** | 39 ns | 30 ns | 171 ns | 31 ns |
| **Clear (empty)** | — | — | 279 ns | — |
| **Keys Enumeration** (100 keys) | 7,697 ns | 5,998 ns | 16,213 ns | 91,589 ns |

### Key Takeaways

- **AddRange vs Add**: `AddRange(key, values)` is significantly faster — `MultiMapList` **~7.3x**, `SortedMultiMap` **~6.7x**, `MultiMapAsync` **~4.0x**, `MultiMapLock` **~2.4x** faster than individual `Add` calls
- **Fastest adds**: `MultiMapList` (no uniqueness check) — **~2.2x faster** than `MultiMapSet`
- **Retrieval methods**: `Get()`, `GetOrDefault()`, and `TryGet()` offer comparable performance when keys exist; choose based on your error handling preference (exception, empty collection, or bool return)
- **KeyCount**: O(1) for `MultiMapSet`/`MultiMapList`/`SortedMultiMap` (< 1 ns). `ConcurrentMultiMap` enumerates keys (~987 ns). `MultiMapLock` acquires a read lock (~327 ns). `MultiMapAsync` acquires a semaphore (~28 ns)
- **GetValuesCount**: Ultra-fast for non-concurrent implementations (3–4 ns) vs `SortedMultiMap` (118 ns tree lookup) and `MultiMapAsync` (221 ns with semaphore overhead)
- **RemoveWhere vs RemoveRange**: `RemoveWhere` operates on a single key (1.9–4.6 μs) and is **~130x faster** than `RemoveRange` across multiple keys (245 μs–1,412 μs)
- **ConcurrentMultiMap Count**: O(1) via `Interlocked` counter — sub-nanosecond, on par with non-concurrent implementations
- **SortedMultiMap**: Slowest across all operations due to tree-based data structures, but provides sorted enumeration. Keys Enumeration is **~15x slower** (91.6 μs vs 6.0 μs for `MultiMapList`)
- **Thread-safe overhead**: `ConcurrentMultiMap` is ~1.9x slower than `MultiMapSet` for adds; `MultiMapLock` is ~1.6x slower
- **Async vs Lock**: `MultiMapAsync` and `MultiMapLock` have comparable add performance (~198 μs vs ~202 μs). `MultiMapAsync` is slightly faster for reads (13.8 μs vs 19.9 μs for `Get`)

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
| `AddRange(items)` | `void` method | Bulk insert from `IEnumerable<KeyValuePair>` |
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
| `AddRangeAsync(items)` | `Task` | Async bulk insert from `IEnumerable<KeyValuePair>` |
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
var allValues = map.Flatten().Select(kvp => kvp.Value);
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

## Release Notes

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
- Code coverage: **94.6% line coverage**, **89.1% branch coverage**, **100% method coverage**
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

## License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).

## Author

**TigoS** — [GitHub](https://github.com/TigoS/MultiMap)
