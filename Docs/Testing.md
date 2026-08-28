# MultiMap

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0%20%7C%209.0%20%7C%208.0-blue.svg)](https://dotnet.microsoft.com/)
[![C# 14](https://img.shields.io/badge/C%23-14.0-blue)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![BenchmarkDotNet](https://img.shields.io/badge/BenchmarkDotNet-v0.15.8-blue)](https://benchmarkdotnet.org/)
[![NuGet](https://img.shields.io/nuget/v/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![NUnit](https://img.shields.io/badge/tests-NUnit%204.6.1-green)](https://nunit.org/)
[![Test SDK](https://img.shields.io/badge/Microsoft.NET.Test.Sdk-v18.6.0-blue)](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk)
[![Coverage](https://img.shields.io/badge/coverage-98.4%25%20line%20%7C%2096.1%25%20branch%20%7C%2098%25%20method-brightgreen)](https://github.com/TigoS/MultiMap/blob/master/Docs/Testing.md#code-coverage-coverlet)
[![Build](https://img.shields.io/badge/tests-6924%2F6924%20passing-success)](https://github.com/TigoS/MultiMap/actions/workflows/ci.yml)

A **.NET** library providing various multimap implementations — collections that associate each generic key with one or more generic values.
Includes _**list-based**_, _**set-based**_, _**sorted**_, _**concurrent**_, _**reader-writer locked**_, and _**async**_ variants with set-like extension methods.
Targets **.NET 10**, **.NET 9**, and **.NET 8**.

## Table of Contents

- [Testing](#testing)
  - [Test Files and Fixtures](#test-files-and-fixtures)
  - [Code Coverage (Coverlet)](#code-coverage-coverlet)

## Testing

The library includes **6,924 unit-test executions** written with **NUnit 4**, running on **net10.0**, **net9.0**, and **net8.0** (**2,308 per framework**), with comprehensive coverage across all implementations and interfaces, including boundary conditions, concurrent stress tests, cancellation-token scenarios, and exception-handling.

```shell
dotnet test
```

### Test Files and Fixtures

| File | Fixture Classes | Notes |
|---|---|---|
| `ConcurrentMultiMap_UnitTests.cs` | `ConcurrentMultiMapTests`, `ConcurrentMultiMap_ConstructorAndBranchTests`, `ConcurrentMultiMap_AddRangeAndEqualsBranchTests`, `ConcurrentMultiMap_StressTests` | Lock-free concurrent implementation, constructor/branch gaps, stress |
| `MultiMapAsync_UnitTests.cs` | `MultiMapAsyncTests`, `MultiMapAsync_GenericInterfaceEqualsTests`, `MultiMapAsync_StressTests` | Async implementation, generic-interface equality paths, stress |
| `MultiMapBase_UnitTests.cs` | `MultiMapSetBaseTests`, `MultiMapListBaseTests`, `SortedMultiMapBaseTests`, `MultiMapBase_ExtraContractTests` | Shared base-class contract for all `MultiMapBase`-derived types |
| `MultiMapBoundaryConditions_UnitTests.cs` | `MultiMapBoundaryConditionsTests`, `AdditionalBoundaryTests`, `ConcurrentMultiMap_CoverageBoostTests`, `MultiMapHelper_CoverageBoostTests`, `MultiMapAsync_EqualsNullPathTests`, `MultiMapBase_ValuesEnumerator_ExplicitCurrentTests`, `MultiMapList_ConstructorTests`, `MultiMapSet_EqualsCoverageTests`, `SortedMultiMap_EqualsCoverageTests`, `MultiMapLock_SetOperationsSelfReferenceTests`, `MultiMapAsync_StressTests`, `MultiMapAsync_EqualsAsyncForeignTests`, `ConcurrentSetPublicSurfaceTests`, `MultiMapAsync_GeneralInterfacePathTests`, `ConcurrentMultiMap_EqualsBranchTests`, `MultiMapSet_EqualsBranchTests`, `MultiMapList_EqualsBranchTests`, `SortedMultiMap_EqualsBranchTests`, `MultiMapLock_EqualsBranchTests`, `MultiMapAsync_FastPathBranchTests`, `NonEquatableConstraintTests` | Boundary conditions, coverage-gap fills, equals-branch coverage, constraint tests |
| `MultiMapHelper_UnitTests.cs` | `MultiMapHelperTests`, `MultiMapHelperWithMultiMapSetTests`, `MultiMapHelperWithSortedMultiMapTests`, `MultiMapHelperWithConcurrentMultiMapTests`, `MultiMapHelperWithMultiMapListTests`, `MultiMapHelperWithMultiMapLockTests`, `MultiMapHelperAsyncTests`, `MultiMapHelperWithSortedMultiMapEdgeCaseTests`, `MultiMapHelperWithConcurrentMultiMapEdgeCaseTests`, `MultiMapHelperWithMultiMapListEdgeCaseTests`, `MultiMapHelperWithMultiMapLockEdgeCaseTests`, `MultiMapHelper_IMultiMapOverloadsTests`, `MultiMapHelperExtensionAsyncTests` | `MultiMapHelper` sync/async extension methods across all implementations |
| `MultiMapList_UnitTest.cs` | `MultiMapListTests`, `MultiMapList_ConstructorAndHashTests`, `MultiMapList_CoverageTests` | List-based implementation |
| `MultiMapLock_UnitTests.cs` | `MultiMapLockTests`, `MultiMapLock_ExtraStressTests`, `MultiMapLock_StressTests`, `MultiMapLock_CancellationTokenTests`, `Guard_ThreeParamNotNullTests` | RW-lock implementation, cancellation-token paths, stress |
| `MultiMapSet_UnitTests.cs` | `MultiMapSetTests`, `MultiMapSet_ConstructorAndHashTests` | HashSet-based implementation |
| `SimpleMultiMap_UnitTests.cs` | `SimpleMultiMapTests` ⚠️ | Deprecated (`[Obsolete]`). Tests retained for regression coverage of `SimpleMultiMap<TKey,TValue>` |
| `SortedMultiMap_UnitTests.cs` | `SortedMultiMapTests`, `SortedMultiMap_ConstructorAndHashTests` | Sorted implementation |

> **Note:** `SimpleMultiMapTests` is marked `[Obsolete]` alongside `SimpleMultiMap<TKey, TValue>`, which was deprecated in v3.0.0. The fixture and its 165 tests remain in the suite to maintain regression coverage while the class is still present.

### Code Coverage (Coverlet)

Code coverage is collected with **Coverlet** (`coverlet.collector`) during `dotnet test`.

```shell
dotnet test --collect:"XPlat Code Coverage"
```

#### Summary (net10.0, 2026-08-28)

| Metric | Value |
|---|---|
| **Line coverage** | **98.4%** (3,333 / 3,387) |
| **Branch coverage** | **96.1%** (1,012 / 1,052) |
| **Method coverage** | **98%** (309 / 315) |

#### Per-Class Breakdown

| Class | Methods | Line Coverage | Branch Coverage | Status |
|---|---|---|---|---|
| `ConcurrentMultiMap<TKey, TValue>` | 27 / 29 | 93.5% (258/276) | 91.2% | ✅ Near-full |
| `ConcurrentSet<T>` | 13 / 13 | **100%** (27/27) | **100%** | ✅ Full |
| `MultiMapAsync<TKey, TValue>` | 93 / 93 | 99.8% (1105/1107) | 96.7% | ✅ Full |
| `MultiMapBase<TKey, TValue, TCollection>` | 42 / 42 | **100%** (200/200) | 98.5% | ✅ Full |
| `MultiMapList<TKey, TValue>` | 12 / 14 | 95.0% (95/100) | **100%** | ✅ Near-full |
| `MultiMapLock<TKey, TValue>` | 55 / 55 | 97.7% (893/914) | 95.3% | ✅ Full |
| `MultiMapSet<TKey, TValue>` | 16 / 18 | 98.2% (111/113) | 95.0% | ✅ Near-full |
| `SimpleMultiMap<TKey, TValue>` ⚠️ | 23 / 23 | **100%** (139/139) | **100%** | ✅ Full |
| `SortedMultiMap<TKey, TValue>` | 10 / 10 | **100%** (60/60) | 90.0% | ✅ Full |
| `Guard` | 3 / 3 | 90.0% (9/10) | **100%** | ✅ Near-full |
| `MultiMapHelper` | 26 / 26 | 98.9% (433/438) | 98.8% | ✅ Full |
| `Polyfills` | 1 / 1 | **100%** (3/3) | **100%** | ✅ Full |

> **Notes:**
> - Coverage from fresh Coverlet run for **net10.0** (2026-08-28). All **2,308 tests** (6,924 total across 3 TFMs) passed with **zero failures**.
> - `SimpleMultiMap<TKey, TValue>` ⚠️ is deprecated (`[Obsolete]` since v3.0.0). Tests are retained for regression coverage.
> - `MultiMapList` and `MultiMapSet` have 2 uncovered methods each — protected abstract `CreateCollection()` factory overloads not reachable via the public API.
> - `Guard` has 1 uncovered line — a dead-code branch in the three-argument overload not reachable in practice.
> - `ConcurrentMultiMap` has 2 uncovered methods — internal helper paths unreachable via the public API without specific concurrent timing.

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
| **Total** | **2,308 tests × 3 TFMs = 6,924 executions** |

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
| **Total** | **2,308 × 3 TFMs** | **6,924 executions** |

> **Coverage distribution:** tests target all core implementations, shared base contracts, dedicated branch-gap scenarios, and set-like extension methods (sync/async), including stress and edge-case coverage. All **2,308 unique tests** run on **net10.0**, **net9.0**, and **net8.0**, validating `#if NET6_0_OR_GREATER` paths across three runtime targets.

> **Historical context:**
> - `ConcurrentSet<T>` was raised from 39.1% line / 0% branch to **100% / 100%** via targeted tests.
> - `MultiMapAsync` general-interface slow paths (IsSubsetOf, IsSupersetOf, Overlaps, SetEquals via non-concrete adapter) are fully covered.
> - All `Equals(IReadOnlyMultiMap<>)` false-path branches are covered for every implementation.
> - Disposed-state (`ObjectDisposedException`) branches for `MultiMapAsync` are covered.
