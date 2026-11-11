# Choosing the Right MultiMap Implementation

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0%20%7C%209.0%20%7C%208.0-blue.svg)](https://dotnet.microsoft.com/)
[![C# 14](https://img.shields.io/badge/C%23-14.0-blue)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![BenchmarkDotNet](https://img.shields.io/badge/BenchmarkDotNet-v0.15.8-blue)](https://benchmarkdotnet.org/)
[![NuGet](https://img.shields.io/nuget/v/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![NUnit](https://img.shields.io/badge/tests-NUnit%205.0.0-green)](https://nunit.org/)
[![Test SDK](https://img.shields.io/badge/Microsoft.NET.Test.Sdk-v18.12.0-blue)](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk)
[![Coverage](https://img.shields.io/badge/coverage-99.45%25%20line%20%7C%2097.22%25%20branch%20%7C%20100%25%20method-brightgreen)](https://github.com/TigoS/MultiMap/blob/master/Docs/Testing.md#code-coverage-coverlet)
[![Build](https://img.shields.io/badge/tests-9496%2F9496%20passing-success)](https://github.com/TigoS/MultiMap/actions/workflows/ci.yml)

This guide helps you quickly pick the right MultiMap type for your scenario.

## Table of Contents

- [Quick Decision Tree](#quick-decision-tree)
- [Detailed Comparison](#detailed-comparison)
- [Best Use Cases](#best-use-cases)
- [Memory Considerations](#memory-considerations-relative)
- [Initialization with Capacity](#initialization-with-capacity)

## Quick Decision Tree

| Need | Choice | Why |
|------|--------|-----|
| Single-threaded, fastest writes, allow duplicates | **MultiMapList** | O(1) append and no uniqueness checks |
| Single-threaded, unique values | **MultiMapSet** | Hash-based uniqueness with solid write/read performance |
| Single-threaded, sorted values | **SortedMultiMap** | Keeps values ordered |
| Multi-threaded, lock-free access | **ConcurrentMultiMap** | Concurrent dictionary + concurrent set behavior |
| Multi-threaded, reader-heavy workload | **MultiMapLock** | ReaderWriterLockSlim favors readers |
| Async-first workload | **MultiMapAsync** | Async API surface with cancellation support |

## Detailed Comparison

### Add Performance (5,000 pairs)

- MultiMapList: 46,949 ns
- MultiMapSet: 81,379 ns
- MultiMapLock: 123,190 ns
- MultiMapAsync: 185,584 ns
- ConcurrentMultiMap: 202,077 ns
- SortedMultiMap: 845,301 ns

> See [Benchmarks](https://github.com/TigoS/MultiMap/blob/master/Docs/Benchmarks.md) for full benchmark details.

## Best Use Cases

### MultiMapList

Use when:
- duplicates are acceptable
- raw write speed is critical
- ordering beyond insertion is not required

Avoid when:
- uniqueness is required

### MultiMapSet

Use when:
- values must be unique
- hash-based equality is appropriate

Avoid when:
- duplicates are expected
- sorted iteration is required

### SortedMultiMap

Use when:
- values must remain sorted
- comparer-based ordering is needed

Avoid when:
- write throughput is the highest priority

### ConcurrentMultiMap

Use when:
- multiple threads add/remove concurrently
- lock-free style access is preferred

Avoid when:
- workload is mostly reads and lock-free overhead is unnecessary

### MultiMapLock

Use when:
- many readers and fewer writers access shared state
- reader throughput matters most

Avoid when:
- writer latency dominates your SLA

### MultiMapAsync

Use when:
- your pipeline is already async/await-first
- operation cancellation is required

Avoid when:
- your usage is purely synchronous

## Memory Considerations (relative)

| Implementation | Memory Overhead | Notes |
|---|---|---|
| MultiMapList | Lower | `List<TValue>` per key |
| MultiMapSet | Medium | `HashSet<TValue>` per key |
| SortedMultiMap | Medium-high | `SortedSet<TValue>` per key |
| ConcurrentMultiMap | High | concurrent structures add overhead |
| MultiMapLock | High | lock state + set-backed storage |
| MultiMapAsync | High | async coordination primitives + set-backed storage |

## Initialization with Capacity

Pre-sizing helps avoid rehash/resizing costs when key cardinality is known:

```csharp
var map = new MultiMapSet<string, int>(
    new MultiMapOptions<string, int>
    {
        Capacity = 10_000,
    });
```

Typical impact: 10-20% faster in large key-set scenarios.
