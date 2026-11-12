# MultiMap

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-11.0%20%7C%2010.0%20%7C%209.0%20%7C%208.0-blue.svg)](https://dotnet.microsoft.com/)
[![C# 14](https://img.shields.io/badge/C%23-14.0-blue)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![BenchmarkDotNet](https://img.shields.io/badge/BenchmarkDotNet-v0.15.8-blue)](https://benchmarkdotnet.org/)
[![NuGet](https://img.shields.io/nuget/v/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![NUnit](https://img.shields.io/badge/tests-NUnit%205.0.0-green)](https://nunit.org/)
[![Test SDK](https://img.shields.io/badge/Microsoft.NET.Test.Sdk-v18.12.0-blue)](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk)
[![Coverage](https://img.shields.io/badge/coverage-99.45%25%20line%20%7C%2097.22%25%20branch%20%7C%20100%25%20method-brightgreen)](https://github.com/TigoS/MultiMap/blob/master/Docs/Testing.md#code-coverage-coverlet)
[![Build](https://img.shields.io/badge/tests-9620%2F9620%20passing-success)](https://github.com/TigoS/MultiMap/actions/workflows/ci.yml)

Frequent pitfalls and practical alternatives for safer MultiMap usage.

## Table of Contents

- [Common Mistakes](#common-mistakes)
  - [1. Iterating While Mutating](#1-iterating-while-mutating)
  - [2. Using SortedMultiMap with Incompatible Comparers](#2-using-sortedmultimap-with-incompatible-comparers)
  - [3. Relying on ConcurrentMultiMap Count for Control Flow](#3-relying-on-concurrentmultimap-count-for-control-flow)
  - [4. Not Disposing MultiMapLock or MultiMapAsync](#4-not-disposing-multimaplock-or-multimapasync)

## Common Mistakes

### 1. Iterating While Mutating

Wrong:

```csharp
foreach (var value in map.Get("key"))
{
    map.Add("key", 42); // may invalidate enumeration
}
```

Correct:

```csharp
var snapshot = map.Get("key").ToList();
foreach (var value in snapshot)
{
    map.Add("key", 42);
}
```

### 2. Using SortedMultiMap with Incompatible Comparers

Wrong:

```csharp
var map = new SortedMultiMap<string, int>(
    null,
    Comparer<int>.Create((a, b) => b.CompareTo(a))
);
// Throws ArgumentException
```

Correct:

```csharp
public sealed class DescendingIntComparer : IComparer<int>, IEqualityComparer<int>
{
    public int Compare(int x, int y) => y.CompareTo(x);
    public bool Equals(int x, int y) => x == y;
    public int GetHashCode(int obj) => obj.GetHashCode();
}

var map = new SortedMultiMap<string, int>(null, new DescendingIntComparer());
```

### 3. Relying on ConcurrentMultiMap Count for Control Flow

Wrong:

```csharp
if (map.Count > 0)
{
    ProcessValue(map.Values.First()); // race-prone
}
```

Correct:

```csharp
if (!map.IsEmpty)
{
    // proceed with a safer path
}
```

### 4. Not Disposing MultiMapLock or MultiMapAsync

Wrong:

```csharp
var map = new MultiMapAsync<string, int>();
await map.AddAsync("key", 42);
```

Correct:

```csharp
await using var map = new MultiMapAsync<string, int>();
await map.AddAsync("key", 42);
```
