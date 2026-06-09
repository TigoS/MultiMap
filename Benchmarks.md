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

A **.NET** library targeting **.NET 10**, **.NET 8**, and **.NET Standard 2.0**

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
| **TryGet** | 39.30 ns |
| **TryGet (missing)** | 34.29 ns |
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
