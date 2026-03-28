# MultiMap

A .NET 10 library providing multiple multimap implementations — collections that associate each key with one or more values. The library offers a range of implementations from simple, non-thread-safe collections to fully concurrent and asynchronous variants.

[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![C# 14](https://img.shields.io/badge/C%23-14.0-blue)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![NUnit](https://img.shields.io/badge/tests-NUnit%204-green)](https://nunit.org/)
[![NuGet](https://img.shields.io/nuget/v/MultiMap)](https://www.nuget.org/packages/MultiMap)

---

## Table of Contents

- [Overview](#overview)
- [Project Structure](#project-structure)
- [Interfaces](#interfaces)
  - [IMultiMap\<TKey, TValue\>](#imultimaptkey-tvalue)
  - [IMultiMapAsync\<TKey, TValue\>](#imultimapasynctkey-tvalue)
  - [ISimpleMultiMap\<TKey, TValue\>](#isimplemultimaptkey-tvalue)
- [Implementations](#implementations)
  - [MultiMapList](#multimaplist)
  - [MultiMapSet](#multimapset)
  - [SortedMultiMap](#sortedmultimap)
  - [ConcurrentMultiMap](#concurrentmultimap)
  - [MultiMapLock](#multimaplock)
  - [MultiMapAsync](#multimapasync)
  - [SimpleMultiMap](#simplemultimap)
- [Comparison Tables](#comparison-tables)
  - [Implementation Comparison](#implementation-comparison)
  - [Interface Comparison](#interface-comparison)
  - [Internal Storage Comparison](#internal-storage-comparison)
  - [Thread Safety Comparison](#thread-safety-comparison)
- [Extension Methods (MultiMapHelper)](#extension-methods-multimaphelper)
- [NuGet Package](#nuget-package)
- [Getting Started](#getting-started)
- [Demo Project (MultiMap.Demo)](#demo-project-multimapdemo)
- [Usage Examples](#usage-examples)
- [Testing](#testing)

---

## Overview

A **multimap** is a generalization of a dictionary where each key can be associated with multiple values. Unlike `Dictionary<TKey, TValue>`, which enforces a one-to-one mapping, a multimap supports one-to-many relationships.

This library provides:

- **7 concrete implementations** covering different use cases (lists, sets, sorted, concurrent, locked, async, simple)
- **3 interfaces** (`IMultiMap<TKey, TValue>`, `IMultiMapAsync<TKey, TValue>`, and `ISimpleMultiMap<TKey, TValue>`) for polymorphic usage
- **Set-like extension methods** (Union, Intersect, ExceptWith, SymmetricExceptWith) for `IMultiMap`, `IMultiMapAsync`, and `ISimpleMultiMap`
- **Full thread-safety options** via `ConcurrentDictionary`, `ReaderWriterLockSlim`, and `SemaphoreSlim`

---

## Project Structure

```
MultiMap/                            # Class library (NuGet package)
├── Entities/
│   ├── MultiMapList.cs              # List-based multimap (allows duplicates per key)
│   ├── MultiMapSet.cs               # HashSet-based multimap (unique values per key)
│   ├── SortedMultiMap.cs            # Sorted keys and sorted values
│   ├── ConcurrentMultiMap.cs        # Lock-free concurrent multimap
│   ├── MultiMapLock.cs              # ReaderWriterLockSlim-based thread-safe multimap
│   ├── MultiMapAsync.cs             # Async/await multimap with SemaphoreSlim
│   └── SimpleMultiMap.cs            # Simplified multimap with ISimpleMultiMap interface
├── Interfaces/
│   ├── IMultiMap.cs                 # Full-featured multimap interface
│   ├── IMultiMapAsync.cs            # Async multimap interface
│   └── ISimpleMultiMap.cs           # Simplified multimap interface
├── Helpers/
│   ├── MultiMapHelper.cs            # Set-like extension methods
│   └── TestDataHelper.cs            # Sample data factory for demos
└── MultiMap.csproj                  # Library project with NuGet package metadata

MultiMap.Demo/                       # Console app demonstrating library usage
├── Program.cs                       # Demonstrates set operations on SimpleMultiMap
└── MultiMap.Demo.csproj             # Console project referencing MultiMap

MultiMap.Tests/                      # Unit test project
├── MultiMapList_UnitTest.cs
├── MultiMapSet_UnitTests.cs
├── SortedMultiMap_UnitTests.cs
├── ConcurrentMultiMap_UnitTests.cs
├── MultiMapLock_UnitTests.cs
├── MultiMapAsync_UnitTests.cs
├── SimpleMultiMap_UnitTests.cs
├── MultiMapHelper_UnitTests.cs
└── MultiMap.Tests.csproj
```

---

## Interfaces

### IMultiMap\<TKey, TValue\>

The full-featured multimap interface, extending `IEnumerable<KeyValuePair<TKey, TValue>>`.

| Method | Return Type | Description |
|---|---|---|
| `Add(TKey, TValue)` | `bool` | Adds a key-value pair. Returns `false` if duplicate (set-based) or always `true` (list-based). |
| `AddRange(TKey, IEnumerable<TValue>)` | `void` | Adds multiple values to a key. |
| `Get(TKey)` | `IEnumerable<TValue>` | Retrieves all values for a key; returns empty if key not found. |
| `Remove(TKey, TValue)` | `bool` | Removes a specific value from a key. Returns `true` if removed. |
| `RemoveKey(TKey)` | `bool` | Removes a key and all its values. Returns `true` if key existed. |
| `ContainsKey(TKey)` | `bool` | Checks if a key exists. |
| `Contains(TKey, TValue)` | `bool` | Checks if a specific key-value pair exists. |
| `Clear()` | `void` | Removes all entries. |
| `Count` | `int` | Total number of key-value pairs across all keys. |
| `Keys` | `IEnumerable<TKey>` | Gets the collection of keys in the multimap. |

### IMultiMapAsync\<TKey, TValue\>

An asynchronous multimap interface extending `IAsyncEnumerable<KeyValuePair<TKey, TValue>>` and `IDisposable`. All operations return `Task` for async/await usage.

| Method | Return Type | Description |
|---|---|---|
| `AddAsync(TKey, TValue, CancellationToken)` | `Task<bool>` | Adds a key-value pair asynchronously. Returns `false` if duplicate. |
| `AddRangeAsync(TKey, IEnumerable<TValue>, CancellationToken)` | `Task` | Adds multiple values to a key asynchronously. |
| `GetAsync(TKey, CancellationToken)` | `Task<IEnumerable<TValue>>` | Retrieves all values for a key; returns empty if not found. |
| `RemoveAsync(TKey, TValue, CancellationToken)` | `Task<bool>` | Removes a specific value from a key. Returns `true` if removed. |
| `RemoveKeyAsync(TKey, CancellationToken)` | `Task<bool>` | Removes a key and all its values. Returns `true` if key existed. |
| `ContainsKeyAsync(TKey, CancellationToken)` | `Task<bool>` | Checks if a key exists asynchronously. |
| `ContainsAsync(TKey, TValue, CancellationToken)` | `Task<bool>` | Checks if a specific key-value pair exists asynchronously. |
| `GetCountAsync(CancellationToken)` | `Task<int>` | Gets the total number of key-value pairs across all keys. |
| `ClearAsync(CancellationToken)` | `Task` | Removes all entries asynchronously. |
| `GetKeysAsync(CancellationToken)` | `Task<IEnumerable<TKey>>` | Gets the collection of keys asynchronously. |

### ISimpleMultiMap\<TKey, TValue\>

A simplified multimap interface with a different API surface, extending `IEnumerable<KeyValuePair<TKey, TValue>>`.

| Method | Return Type | Description |
|---|---|---|
| `Add(TKey, TValue)` | `bool` | Adds a key-value pair. Returns `false` if duplicate. |
| `Get(TKey)` | `IEnumerable<TValue>` | Retrieves values for a key. Throws `KeyNotFoundException` if not found. |
| `GetOrDefault(TKey)` | `IEnumerable<TValue>` | Retrieves values or empty collection if key not found. |
| `Remove(TKey, TValue)` | `void` | Removes a specific value (no return value). |
| `Clear(TKey)` | `void` | Removes all values for a key. |
| `Flatten()` | `IEnumerable<KeyValuePair<TKey, TValue>>` | Returns all key-value pairs as a flat sequence. |

---

## Implementations

### MultiMapList

A multimap backed by `Dictionary<TKey, List<TValue>>`. **Allows duplicate values** per key and preserves insertion order.

- **Interface:** `IMultiMap<TKey, TValue>`
- **Duplicates:** Allowed — `Add` always returns `true`
- **Ordering:** Insertion order preserved
- **Thread Safety:** None

### MultiMapSet

A multimap backed by `Dictionary<TKey, HashSet<TValue>>`. **Enforces unique values** per key with O(1) lookup.

- **Interface:** `IMultiMap<TKey, TValue>`
- **Duplicates:** Not allowed — `Add` returns `false` for duplicates
- **Ordering:** Unordered
- **Thread Safety:** None

### SortedMultiMap

A multimap backed by `SortedDictionary<TKey, SortedSet<TValue>>`. Both keys and values are maintained in **sorted order**.

- **Interface:** `IMultiMap<TKey, TValue>`
- **Duplicates:** Not allowed — `Add` returns `false` for duplicates
- **Ordering:** Keys and values sorted by natural comparer
- **Thread Safety:** None

### ConcurrentMultiMap

A **lock-free** thread-safe multimap using `ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>`. Safe for concurrent reads and writes without explicit locking.

- **Interface:** `IMultiMap<TKey, TValue>`
- **Duplicates:** Not allowed
- **Ordering:** Unordered
- **Thread Safety:** Full (lock-free)

### MultiMapLock

A thread-safe multimap using `ReaderWriterLockSlim` for fine-grained locking. Allows concurrent readers with exclusive writers. Implements `IDisposable`.

- **Interface:** `IMultiMap<TKey, TValue>`, `IDisposable`
- **Duplicates:** Not allowed
- **Ordering:** Unordered
- **Thread Safety:** Full (reader-writer lock)
- **Enumeration:** Returns snapshot to avoid lock contention

### MultiMapAsync

An **asynchronous** thread-safe multimap using `SemaphoreSlim`. All operations are `async`/`await`-based. Implements `IMultiMapAsync<TKey, TValue>` (which extends `IAsyncEnumerable<KeyValuePair<TKey, TValue>>` and `IDisposable`).

- **Interface:** `IMultiMapAsync<TKey, TValue>`
- **Duplicates:** Not allowed
- **Ordering:** Unordered
- **Thread Safety:** Full (semaphore-based)
- **Enumeration:** Async snapshot via `await foreach`

### SimpleMultiMap

A lightweight multimap implementing the simplified `ISimpleMultiMap` interface. Uses `Dictionary<TKey, HashSet<TValue>>` internally.

- **Interface:** `ISimpleMultiMap<TKey, TValue>`
- **Duplicates:** Not allowed
- **Ordering:** Unordered
- **Thread Safety:** None

---

## Comparison Tables

### Implementation Comparison

| Class | Interface | Allows Duplicates | Sorted | Thread-Safe | Disposable | Async |
|---|---|---|---|---|---|---|
| `MultiMapList` | `IMultiMap` | ✅ Yes | ❌ | ❌ | ❌ | ❌ |
| `MultiMapSet` | `IMultiMap` | ❌ No | ❌ | ❌ | ❌ | ❌ |
| `SortedMultiMap` | `IMultiMap` | ❌ No | ✅ | ❌ | ❌ | ❌ |
| `ConcurrentMultiMap` | `IMultiMap` | ❌ No | ❌ | ✅ Lock-free | ❌ | ❌ |
| `MultiMapLock` | `IMultiMap` | ❌ No | ❌ | ✅ RW Lock | ✅ | ❌ |
| `MultiMapAsync` | `IMultiMapAsync` | ❌ No | ❌ | ✅ Semaphore | ✅ | ✅ |
| `SimpleMultiMap` | `ISimpleMultiMap` | ❌ No | ❌ | ❌ | ❌ | ❌ |

### Interface Comparison

| Feature | `IMultiMap<TKey, TValue>` | `ISimpleMultiMap<TKey, TValue>` | `IMultiMapAsync<TKey, TValue>` |
|---|---|---|---|
| `Add` | `bool Add(key, value)` | `bool Add(key, value)` | `Task<bool> AddAsync(key, value)` |
| `AddRange` | ✅ `void AddRange(key, values)` | ❌ Not available | ✅ `Task AddRangeAsync(key, values)` |
| `Get` (key not found) | Returns empty collection | Throws `KeyNotFoundException` | Returns empty collection |
| `GetOrDefault` | ❌ Not available | ✅ Returns empty collection | ❌ Not available |
| `Remove(key, value)` | Returns `bool` | Returns `void` | Returns `Task<bool>` |
| `RemoveKey` / `Clear(key)` | `bool RemoveKey(key)` | `void Clear(key)` | `Task<bool> RemoveKeyAsync(key)` |
| `Clear` (all) | ✅ `void Clear()` | ❌ Not available | ✅ `Task ClearAsync()` |
| `ContainsKey` | ✅ | ❌ Not available | ✅ `Task<bool> ContainsKeyAsync(key)` |
| `Contains(key, value)` | ✅ | ❌ Not available | ✅ `Task<bool> ContainsAsync(key, value)` |
| `Count` / `GetCount` | ✅ `int Count` | ❌ Not available | ✅ `Task<int> GetCountAsync()` |
| `Keys` / `GetKeys` | ✅ `IEnumerable<TKey> Keys` | ❌ Not available | ✅ `Task<IEnumerable<TKey>> GetKeysAsync()` |
| `Flatten` | ❌ (use enumeration) | ✅ `IEnumerable<KVP> Flatten()` | ❌ (use async enumeration) |
| Enumeration | `IEnumerable<KVP>` | `IEnumerable<KVP>` | `IAsyncEnumerable<KVP>` |
| `IDisposable` | ❌ | ❌ | ✅ |

### Internal Storage Comparison

| Class | Key Storage | Value Storage | Lookup Complexity |
|---|---|---|---|
| `MultiMapList` | `Dictionary<TKey, ...>` | `List<TValue>` | O(n) per value |
| `MultiMapSet` | `Dictionary<TKey, ...>` | `HashSet<TValue>` | O(1) per value |
| `SortedMultiMap` | `SortedDictionary<TKey, ...>` | `SortedSet<TValue>` | O(log n) per value |
| `ConcurrentMultiMap` | `ConcurrentDictionary<TKey, ...>` | `ConcurrentDictionary<TValue, byte>` | O(1) per value |
| `MultiMapLock` | `Dictionary<TKey, ...>` | `HashSet<TValue>` | O(1) per value |
| `MultiMapAsync` | `Dictionary<TKey, ...>` | `HashSet<TValue>` | O(1) per value |
| `SimpleMultiMap` | `Dictionary<TKey, ...>` | `HashSet<TValue>` | O(1) per value |

### Thread Safety Comparison

| Class | Mechanism | Read Concurrency | Write Concurrency | Best For |
|---|---|---|---|---|
| `MultiMapList` | None | ❌ | ❌ | Single-threaded, ordered values |
| `MultiMapSet` | None | ❌ | ❌ | Single-threaded, unique values |
| `SortedMultiMap` | None | ❌ | ❌ | Single-threaded, sorted access |
| `ConcurrentMultiMap` | `ConcurrentDictionary` | ✅ | ✅ | High-throughput concurrent access |
| `MultiMapLock` | `ReaderWriterLockSlim` | ✅ Concurrent reads | ❌ Exclusive writes | Read-heavy workloads |
| `MultiMapAsync` | `SemaphoreSlim` | ❌ Exclusive | ❌ Exclusive | Async/await workflows |
| `SimpleMultiMap` | None | ❌ | ❌ | Simple one-to-many mapping |

---

## Extension Methods (MultiMapHelper)

The `MultiMapHelper` static class provides set-like operations as extension methods for `IMultiMap`, `ISimpleMultiMap`, and `IMultiMapAsync`.

### IMultiMap Extensions

| Method | Description |
|---|---|
| `Union(target, other)` | Adds all key-value pairs from `other` into `target`. |
| `Intersect(target, other)` | Removes pairs from `target` that do not exist in `other`. |
| `ExceptWith(target, other)` | Removes pairs from `target` that exist in `other`. |
| `SymmetricExceptWith(target, other)` | Keeps only pairs present in `target` or `other`, but not both. |

### ISimpleMultiMap Extensions

| Method | Returns | Description |
|---|---|---|
| `Union(target, other)` | `ISimpleMultiMap` | Adds all pairs from `other` into `target`. |
| `Intersect(target, other)` | `ISimpleMultiMap` | Keeps only pairs present in both. |
| `ExceptWith(target, other)` | `ISimpleMultiMap` | Removes pairs found in `other`. |
| `SymmetricExceptWith(target, other)` | `ISimpleMultiMap` | Keeps only pairs in one but not both. |

### IMultiMapAsync Extensions

| Method | Returns | Description |
|---|---|---|
| `UnionAsync(target, other, cancellationToken)` | `Task` | Asynchronously adds all key-value pairs from `other` into `target`. |
| `IntersectAsync(target, other, cancellationToken)` | `Task` | Asynchronously removes pairs from `target` that do not exist in `other`. |
| `ExceptWithAsync(target, other, cancellationToken)` | `Task` | Asynchronously removes pairs from `target` that exist in `other`. |
| `SymmetricExceptWithAsync(target, other, cancellationToken)` | `Task` | Asynchronously keeps only pairs present in `target` or `other`, but not both. |

---

## NuGet Package

The library is published as the **MultiMap** NuGet package.

| Property | Value |
|---|---|
| **Package ID** | `MultiMap` |
| **Version** | `1.0.0` |
| **Target Framework** | `net10.0` |
| **License** | MIT |
| **Tags** | `multimap` `dictionary` `collection` `concurrent` `async` `sorted` `data-structures` |

### Install

#### .NET CLI

```bash
dotnet add package MultiMap
```

#### Package Manager Console

```powershell
Install-Package MultiMap
```

#### PackageReference

```xml
<PackageReference Include="MultiMap" Version="1.0.0" />
```

### Build the Package Locally

```bash
dotnet pack MultiMap/MultiMap.csproj --configuration Release
```

The `.nupkg` file is output to `MultiMap/bin/Release/`.

### Publish to NuGet.org

```bash
dotnet nuget push MultiMap/bin/Release/MultiMap.1.0.0.nupkg --api-key <YOUR_API_KEY> --source https://api.nuget.org/v3/index.json
```

### Package Contents

The NuGet package includes:

- Compiled library assembly (`MultiMap.dll`)
- XML documentation file for IntelliSense support in consuming projects
- This `README.md` as the package readme

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Build

```bash
dotnet build
```

---

## Demo Project (MultiMap.Demo)

The `MultiMap.Demo` console application demonstrates the library's set-like extension methods using `SimpleMultiMap` and `ISimpleMultiMap`. The demo project consumes the library as a **NuGet package** (via `PackageReference`) rather than a project reference.

### Run the Demo

```bash
dotnet run --project MultiMap.Demo
```

### What It Demonstrates

The demo creates two sample multimaps and runs all four set operations, printing the results:

| Operation | Description |
|---|---|
| **Union** | Combines all key-value pairs from both maps |
| **Intersect** | Keeps only pairs present in both maps |
| **ExceptWith** | Removes pairs found in the second map from the first |
| **SymmetricExceptWith** | Keeps only pairs present in one map but not both |

### Sample Output

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

...
```

---

## Usage Examples

### Basic Usage with MultiMapSet

```csharp
using MultiMap.Entities;

var map = new MultiMapSet<string, int>();

map.Add("fruits", 1);
map.Add("fruits", 2);
map.Add("fruits", 2); // returns false — duplicate
map.Add("vegetables", 3);

var fruits = map.Get("fruits"); // [1, 2]
bool hasFruits = map.ContainsKey("fruits"); // true
int total = map.Count; // 3

map.Remove("fruits", 1); // removes value 1 from "fruits"
map.RemoveKey("vegetables"); // removes "vegetables" and all values
```

### Thread-Safe with ConcurrentMultiMap

```csharp
using MultiMap.Entities;

var map = new ConcurrentMultiMap<string, int>();

// Safe to call from multiple threads
Parallel.For(0, 100, i =>
{
    map.Add("key", i);
});

foreach (var kvp in map)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}
```

### Async Usage with MultiMapAsync

```csharp
using MultiMap.Entities;
using MultiMap.Interfaces;

await using var map = new MultiMapAsync<string, int>();

await map.AddAsync("colors", 1);
await map.AddAsync("colors", 2);
await map.AddRangeAsync("sizes", new[] { 10, 20, 30 });

var colors = await map.GetAsync("colors"); // [1, 2]
int count = await map.GetCountAsync(); // 5
var keys = await map.GetKeysAsync(); // ["colors", "sizes"]

await foreach (var kvp in map)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}

// Use through the IMultiMapAsync interface
IMultiMapAsync<string, int> asyncMap = new MultiMapAsync<string, int>();
await asyncMap.AddAsync("key", 42);
var values = await asyncMap.GetAsync("key");
```

### Set Operations with Extension Methods

```csharp
using MultiMap.Entities;
using MultiMap.Helpers;

var mapA = new MultiMapSet<string, int>();
mapA.Add("x", 1);
mapA.Add("x", 2);
mapA.Add("y", 3);

var mapB = new MultiMapSet<string, int>();
mapB.Add("x", 2);
mapB.Add("x", 4);
mapB.Add("z", 5);

// Union: mapA now contains all pairs from both
mapA.Union(mapB);

// Intersect: keep only common pairs
mapA.Intersect(mapB);

// ExceptWith: remove pairs found in mapB
mapA.ExceptWith(mapB);

// SymmetricExceptWith: keep only pairs in one but not both
mapA.SymmetricExceptWith(mapB);
```

### SimpleMultiMap with Fluent Helpers

```csharp
using MultiMap.Entities;
using MultiMap.Helpers;

var map1 = new SimpleMultiMap<string, int>();
map1.Add("A", 1);
map1.Add("A", 2);

var map2 = new SimpleMultiMap<string, int>();
map2.Add("A", 2);
map2.Add("B", 3);

var result = map1.Union(map2); // A:[1,2], B:[3]

foreach (var kvp in result.Flatten())
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}
```

---

## Testing

The test suite uses **NUnit 4** and covers all implementations with **460 tests**

### Run Tests

```bash
dotnet test
```

### Run with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test Fixtures

| Test File | Target Class | Test Count |
|---|---|---|
| `MultiMapList_UnitTest.cs` | `MultiMapList` | 46 |
| `MultiMapSet_UnitTests.cs` | `MultiMapSet` | 46 |
| `SortedMultiMap_UnitTests.cs` | `SortedMultiMap` | 49 |
| `ConcurrentMultiMap_UnitTests.cs` | `ConcurrentMultiMap` | 46 |
| `MultiMapLock_UnitTests.cs` | `MultiMapLock` | 59 |
| `MultiMapAsync_UnitTests.cs` | `MultiMapAsync` | 53 |
| `SimpleMultiMap_UnitTests.cs` | `SimpleMultiMap` | 33 |
| `MultiMapHelper_UnitTests.cs` | `MultiMapHelper` | 128 |

### Coverage Details

| Class | Line Coverage | Branch Coverage |
|---|---|---|
| `MultiMapHelper` | 100% | 100% |
| `ConcurrentMultiMap` | 100% | 100% |
| `MultiMapAsync` | 100% | 100% |
| `MultiMapList` | 100% | 100% |
| `MultiMapLock` | 100% | 100% |
| `MultiMapSet` | 100% | 100% |
| `SimpleMultiMap` | 100% | 100% |
| `SortedMultiMap` | 100% | 100% |
| `TestDataHelper` | 0% (demo-only) | 0% (demo-only) |

---

## License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).
