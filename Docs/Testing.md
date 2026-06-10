# MultiMap

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0%20%7C%208.0%20%7C%20Standard%202.0-blue.svg)](https://dotnet.microsoft.com/)
[![C# 14](https://img.shields.io/badge/C%23-14.0-blue)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![NUnit](https://img.shields.io/badge/tests-NUnit%204-green)](https://nunit.org/)
[![BenchmarkDotNet](https://img.shields.io/badge/BenchmarkDotNet-v0.15.8-blue)](https://benchmarkdotnet.org/)
[![Test SDK](https://img.shields.io/badge/Microsoft.NET.Test.Sdk-v18.6.0-blue)](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk)
[![NuGet](https://img.shields.io/nuget/v/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![Coverage](https://img.shields.io/badge/coverage-98.7%25-brightgreen)]()

A **.NET** library providing various multimap implementations — collections that associate each generic key with one or more generic values.
Includes _**list-based**_, _**set-based**_, _**sorted**_, _**concurrent**_, _**reader-writer locked**_, and _**async**_ variants with set-like extension methods.
Targets **.NET 10**, **.NET 8**, and **.NET Standard 2.0**.

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

The library includes **4,452 unit tests** written with **NUnit 4**, running on both **net10.0** and **net8.0** (**2,226 per framework** before fixtures), with comprehensive boundary-condition coverage across all implementations and interfaces, edge cases, concurrent stress tests, and exception-handling scenarios.

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
- **101 new tests** in `GapCoverage_UnitTests.cs` targeting precise coverage gaps:
  - `ConcurrentSet<T>` full `ICollection<T>` surface: `IsReadOnly`, `void Add(T)`, `Clear`, `Remove`, all 8 `CopyTo` branches (null array, negative index, index > length, destination too small, valid copy, empty-set edge), and `IEnumerable.GetEnumerator`
  - `MultiMapAsync` general-interface paths via `WrappedMultiMapAsync<>` adapter: `IsSubsetOfAsync`, `IsSupersetOfAsync`, `OverlapsAsync`, `SetEqualsAsync` slow paths for all true/false branches
  - `MultiMapAsync` disposed-state guard (`ObjectDisposedException` after `Dispose()`)
  - `MultiMapAsync` fast-path false branches: count mismatch, key not found, value not found, empty maps
  - `ContainsAsync` / `TryGetAsync` / `GetValuesCountAsync` key-not-found branches
  - `Equals(IReadOnlyMultiMap<>)` all false-path branches for `ConcurrentMultiMap`, `MultiMapSet`, `MultiMapList`, `SortedMultiMap`, `MultiMapLock`
  - `MultiMapLock.SetEquals` all false-path branches (count, key count, key not found, value count mismatch, value set not equal)
  - `ConcurrentMultiMap.RemoveWhere` predicate-matches-none, matches-all, and concurrent stress scenarios
  - `ConcurrentMultiMap` concurrent `Remove`/`RemoveKey`/`AddRange` stress tests (`Category="Stress"/"Concurrent"`)

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
| `ConcurrentSetPublicSurfaceTests` | 15 | `ConcurrentSet<T>` full `ICollection<T>` surface and enumerator |
| `MultiMapAsync_GeneralInterfacePathTests` | 30 | `MultiMapAsync` general-interface slow paths and disposed-state guards |
| `MultiMapAsync_FastPathBranchTests` | 9 | `MultiMapAsync` fast-path false branches (count, key, value mismatch) |
| `ConcurrentMultiMap_EqualsBranchTests` | 10 | `ConcurrentMultiMap.Equals` all false-path branches + `RemoveWhere` predicate stress |
| `MultiMapSet_EqualsBranchTests` | 8 | `MultiMapSet.Equals` all false-path branches |
| `MultiMapList_EqualsBranchTests` | 8 | `MultiMapList.Equals` all false-path branches |
| `SortedMultiMap_EqualsBranchTests` | 8 | `SortedMultiMap.Equals` all false-path branches |
| `MultiMapLock_EqualsBranchTests` | 16 | `MultiMapLock.Equals` and `SetEquals` all false-path branches |
| `ConcurrentMultiMap_RemoveStressTests` | 3 | Concurrent `Remove`/`RemoveKey`/`AddRange` stress |
| **Gap subtotal** | **248** | |

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
| **Total** | **2,226 tests × 2 TFMs = 4,452 executions** |

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
| `SimpleMultiMapTests` | 76 | 3.4% |
| **Entity subtotal** | **1,232** | **55.3%** |
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
| `MultiMapHelper_IMultiMapOverloadsTests` | 26 | 1.2% |
| `ConcurrentSetPublicSurfaceTests` | 15 | 0.7% |
| `MultiMapAsync_GeneralInterfacePathTests` | 30 | 1.3% |
| `MultiMapAsync_FastPathBranchTests` | 9 | 0.4% |
| `ConcurrentMultiMap_EqualsBranchTests` | 10 | 0.4% |
| `MultiMapSet_EqualsBranchTests` | 8 | 0.4% |
| `MultiMapList_EqualsBranchTests` | 8 | 0.4% |
| `SortedMultiMap_EqualsBranchTests` | 8 | 0.4% |
| `MultiMapLock_EqualsBranchTests` | 16 | 0.7% |
| `ConcurrentMultiMap_RemoveStressTests` | 3 | 0.1% |
| **Gap subtotal** | **248** | **11.1%** |
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
| `MultiMapHelperWithSortedMultiMapTests` | 14 | 0.6% |
| **Helper subtotal** | **408** | **18.3%** |
| **Total** | **2,226 × 2 TFMs** | **4,452 executions** |

> **Coverage distribution:** tests target all core implementations, shared base contracts, dedicated branch-gap scenarios, and set-like extension methods (sync/async), including stress and edge-case coverage. All **2,226 unique tests** run on both **net10.0** and **net8.0**, validating `#if NET6_0_OR_GREATER` code paths on both target frameworks.

### Code Coverage (Coverlet)

Code coverage is collected with **Coverlet** (`coverlet.collector`) during `dotnet test` and reported via **ReportGenerator**.

```shell
dotnet test --collect:"XPlat Code Coverage"
```

#### Summary

| Metric | Value |
|---|---|
| **Method coverage** | **97.2%** (285 / 293) |
| **Line coverage** | **98.7%** (2,829 / 2,865) |
| **Branch coverage** | **96.3%** (921 / 956) |

#### Per-Class Breakdown

| Class | Methods (covered/total) | Line Coverage | Branch Coverage | Status |
|---|---|---|---|---|
| `ConcurrentMultiMap<T1, T2>` | 28 / 29 | 94.3% (218/231) | 93.0% (93/100) | ✅ Near-full |
| `ConcurrentSet<T>` | 13 / 13 | **100%** (27/27) | **100%** (8/8) | ✅ Full |
| `MultiMapAsync<T1, T2>` | 81 / 81 | 99.8% (1045/1047) | 96.2% (304/316) | ✅ Full |
| `MultiMapBase<T1, T2, T3>` | 35 / 35 | 99.4% (181/182) | 98.4% (65/66) | ✅ Full |
| `MultiMapList<T1, T2>` | 14 / 14 | 94.6% (88/93) | **100%** (34/34) | ✅ Near-full |
| `MultiMapLock<T1, T2>` | 42 / 42 | 99.6% (578/580) | 97.3% (183/188) | ✅ Full |
| `MultiMapSet<T1, T2>` | 18 / 18 | 96.2% (103/107) | 95.0% (38/40) | ✅ Near-full |
| `SimpleMultiMap<T1, T2>` | 23 / 23 | **100%** (134/134) | 97.2% (35/36) | ✅ Full |
| `SortedMultiMap<T1, T2>` | 10 / 10 | 95.3% (41/43) | 86.3% (19/22) | ✅ Near-full |
| `Guard` | 2 / 2 | **100%** (7/7) | **100%** (2/2) | ✅ Full |
| `MultiMapHelper` | 26 / 26 | 98.3% (407/414) | 97.2% (140/144) | ✅ Full |

> **Notes:**
> - Coverage is computed from the latest combined Coverlet reports for **net10.0** and **net8.0** using ReportGenerator (run: 2026-06-10).
> - **Latest Coverlet run (v2.1.0)**: Executed **4,452 total tests** (2,226 per framework) with **zero failures**.
> - **Overall assembly coverage**: **96.8% method coverage**, **98.7% line coverage**, **96.3% branch coverage** across all MultiMap implementations and helpers.
> - **Per-class highlights**:
>   - **ConcurrentSet\<T\>, SimpleMultiMap, Guard**: **100% line coverage** ✅
>   - **`ConcurrentSet<T>`**: raised from 39.1% line / 0% branch to **100% / 100%** via 15 new targeted tests
>   - **MultiMapAsync**: 99.8% line coverage with 96.2% branch coverage (up from 92.6% line / 87.1% branch)
>   - **MultiMapLock**: 99.6% line coverage with 97.3% branch coverage
>   - **MultiMapHelper**: 98.3% line coverage with 97.2% branch coverage
> - **Coverage improvements** from v2.2.0 gap-coverage additions:
>   - +101 tests in `GapCoverage_UnitTests.cs` targeting exact uncovered branches identified via Coverlet HTML reports
>   - `ConcurrentSet<T>` brought to 100% line + branch coverage
>   - `MultiMapAsync` general-interface slow paths (IsSubsetOf, IsSuperset, Overlaps, SetEquals via non-concrete adapter) fully covered
>   - All `Equals(IReadOnlyMultiMap<>)` false-path branches covered for every implementation
>   - `MultiMapLock.SetEquals` all false-path branches covered
>   - Disposed-state (`ObjectDisposedException`) branches for `MultiMapAsync` covered
