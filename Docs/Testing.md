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

## Table of Contents

- [Testing](#testing)
  - [Test Coverage by Implementation](#test-coverage-by-implementation)
  - [Test Coverage by Base Class](#test-coverage-by-base-class)
  - [Coverage Gap Tests](#coverage-gap-tests)
  - [Test Coverage by Extension Methods](#test-coverage-by-extension-methods)
  - [Test Categories](#test-categories)
  - [Test Coverage Percentage](#test-coverage-percentage)
  - [Code Coverage (Coverlet)](#code-coverage-coverlet)

## Testing

The library includes **4,240 unit tests** written with **NUnit 4**, running on both **net10.0** and **net8.0** (**2,120 per framework** before fixtures), with comprehensive boundary-condition coverage across all implementations and interfaces, edge cases, concurrent stress tests, and exception-handling scenarios.

**Recent additions (v2.1.0 coverage expansion):**
- **46 new tests** in `AdditionalCoverage_UnitTests.cs` covering:
  - MultiMapSet duplicate value handling and partial range removal
  - MultiMapList duplicate value storage and ordering guarantees
  - SortedMultiMap value sorting across multiple keys
  - ConcurrentMultiMap concurrent add operations and thread-safety validation
  - MultiMapAsync async completion and concurrent operation safety
  - MultiMapLock atomic operations and set operations
  - SimpleMultiMap get snapshot behavior and duplicate prevention
  - Complex key scenarios (spaces, special characters, large keys)
  - Boundary conditions (zero counts, single items)
  - Enumeration consistency and count accuracy
  - Multi-operation sequences maintaining consistency
  - Predicate-based removal with all/partial/no condition matches

```shell
dotnet test
```

### Test Coverage by Implementation

| Test Class | Tests | Category |
|---|---|---|
| `ConcurrentMultiMapTests` | 161 | Lock-free concurrent implementation |
| `MultiMapAsyncTests` | 269 | Async implementation |
| `MultiMapAsync_GenericInterfaceEqualsTests` | 21 | Generic-interface async equality path |
| `MultiMapLockTests` | 230 | RW Lock implementation |
| `MultiMapListTests` | 149 | List-based implementation |
| `MultiMapSetTests` | 145 | HashSet-based implementation |
| `SortedMultiMapTests` | 137 | Sorted implementation |
| `SimpleMultiMapTests` | 76 | Lightweight implementation |
| `AdditionalCoverage_UnitTests` | 46 | Edge cases, complex scenarios, boundary conditions |
| **Entity subtotal** | **1,232** | |

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
| **Total** | **2,120 tests × 2 TFMs = 4,240 executions** |

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
| **Total** | **2,120 × 2 TFMs** | **4,240 executions** |

> **Coverage distribution:** tests target all core implementations, shared base contracts, dedicated branch-gap scenarios, and set-like extension methods (sync/async), including stress and edge-case coverage. All **2,120 unique tests** run on both **net10.0** and **net8.0**, validating `#if NET6_0_OR_GREATER` code paths on both target frameworks.

### Code Coverage (Coverlet)

Code coverage is collected with **Coverlet** (`coverlet.collector`) during `dotnet test` and reported via **ReportGenerator**.

```shell
dotnet test --collect:"XPlat Code Coverage"
```

#### Summary

| Metric | Value |
|---|---|
| **Method coverage** | **96.8%** |
| **Line coverage** | **98.35%** |
| **Branch coverage** | **93.20%** |

#### Per-Class Breakdown

| Class | Method Coverage | Line Coverage | Branch Coverage | Status |
|---|---|---|---|---|
| `SimpleMultiMap<TKey, TValue>` | 100% | 100% | 96.9% | ✅ Full |
| `MultiMapBase<TKey, TValue, TCollection>` | 100% | 100% | 98.0% | ✅ Full |
| `MultiMapBase.ValuesCollection<TKey, TValue, TCollection>` | 100% | 100% | 100% | ✅ Full |
| `MultiMapBase.ValuesEnumerator<TKey, TValue, TCollection>` | 100% | 95.7% | 100% | ✅ Near-full |
| `MultiMapList<TKey, TValue>` | 100% | 95.5% | 100% | ✅ Near-full |
| `MultiMapSet<TKey, TValue>` | 100% | 95.1% | 95.0% | ✅ Near-full |
| `SortedMultiMap<TKey, TValue>` | 100% | 94.1% | 86.4% | ✅ Near-full |
| `ConcurrentMultiMap<TKey, TValue>` | 100% | 95.3% | 100% | ✅ Near-full |
| `MultiMapLock<TKey, TValue>` | 100% | 99.2% | 96.7% | ✅ Full |
| `MultiMapAsync<TKey, TValue>` | 100% | 99.4% | 97.0% | ✅ Full |
| `MultiMapHelper` | 100% | 98.8% | 98.9% | ✅ Full |
| `Guard` (helpers) | 100% | 100% | 100% | ✅ Full |

> **Notes:**
> - Coverage is computed from the latest combined Coverlet reports for **net10.0** and **net8.0** using ReportGenerator.
> - **Latest Coverlet run (v2.1.0)**: Executed **4,240 total tests** (2,120 per framework) with **zero failures**.
> - **Overall assembly coverage**: **96.8% method coverage**, **98.3% line coverage**, **93.2% branch coverage** across all MultiMap implementations and helpers.
> - **Per-class highlights**:
>   - **SimpleMultiMap, MultiMapBase, MultiMapLock, Guard**: **100% line coverage** ✅
>   - **MultiMapAsync**: **99.4% line coverage** (strong async safety coverage)
>   - **MultiMapHelper**: **98.1% line coverage** (comprehensive set operation coverage)
>   - **ConcurrentMultiMap**: **95.4% line coverage** (lock-free concurrent implementation)
>   - **MultiMapSet, MultiMapList, SortedMultiMap**: **94-95% line coverage** (solid implementation coverage)
> - **Coverage improvements** from v2.1.0 additions:
>   - New **46 comprehensive edge-case and boundary-condition tests** in `AdditionalCoverage_UnitTests.cs`
>   - Added tests for concurrent safety validation, snapshot semantics, predicate-based removal, and complex key/value scenarios
>   - Maintains high coverage across all implementations while targeting previously untested edge cases and boundary conditions
