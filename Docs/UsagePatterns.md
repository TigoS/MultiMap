# MultiMap

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-11.0%20%7C%2010.0%20%7C%209.0%20%7C%208.0-blue.svg)](https://dotnet.microsoft.com/)
[![C# 14](https://img.shields.io/badge/C%23-14.0-blue)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![BenchmarkDotNet](https://img.shields.io/badge/BenchmarkDotNet-v0.15.8-blue)](https://benchmarkdotnet.org/)
[![NuGet](https://img.shields.io/nuget/v/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![NUnit](https://img.shields.io/badge/tests-NUnit%205.0.0-green)](https://nunit.org/)
[![Test SDK](https://img.shields.io/badge/Microsoft.NET.Test.Sdk-v18.12.0-blue)](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk)
[![Coverage](https://img.shields.io/badge/coverage-97.3%25%20line%20%7C%2095%25%20branch%20%7C%2098.8%25%20method-brightgreen)](https://github.com/TigoS/MultiMap/blob/master/Docs/Testing.md#code-coverage-coverlet)
[![Build](https://img.shields.io/badge/tests-9628%2F9628%20passing-success)](https://github.com/TigoS/MultiMap/actions/workflows/ci.yml)

Practical sync and async usage recipes for `IMultiMap` and `IMultiMapAsync`.

## Table of Contents

- [Usage Patterns](#usage-patterns)
  - [Basic Usage with `IMultiMap`](#basic-usage-with-imultimap)
  - [Advanced Usage](#advanced-usage)
    - [Working with KeyCount and Values](#working-with-keycount-and-values)
    - [Bulk Operations with AddRange and RemoveRange](#bulk-operations-with-addrange-and-removerange)
    - [Conditional Removal with RemoveWhere](#conditional-removal-with-removewhere)
    - [Retrieval Pattern Options](#retrieval-pattern-options)
    - [GetOrAdd and CopyTo](#getoradd-and-copyto)
  - [Concurrent Usage](#concurrent-usage)
  - [Async Usage](#async-usage)
    - [Advanced Async Operations](#advanced-async-operations)
  - [Set Operations](#set-operations)
  - [SimpleMultiMap with Demo](#simplemultimap-with-demo)

## Usage Patterns

### Basic Usage with `IMultiMap`

```csharp
using MultiMap.Entities;

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

### Advanced Usage

#### Working with KeyCount and Values

```csharp
using MultiMap.Entities;

var map = new MultiMapSet<string, int>();
map.Add("A", 1);
map.Add("A", 2);
map.Add("B", 3);

// KeyCount returns the number of unique keys (not total pairs)
int keyCount = map.KeyCount;        // 2 (keys: "A", "B")
int totalCount = map.Count;         // 3 (pairs: A→1, A→2, B→3)

// Values property returns all values across all keys
IEnumerable<int> allValues = map.Values;  // [1, 2, 3]

// GetValuesCount returns the count for a specific key
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

// RemoveRange returns the count of actually removed pairs
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

#### GetOrAdd and CopyTo

```csharp
using MultiMap.Entities;

var map = new MultiMapSet<string, int>();

// Get existing values or initialize with a default value
var tags = map.GetOrAdd("post_123", 1);

// Lazy initialization overload (factory evaluated only when key is missing)
var lazyValues = map.GetOrAdd("post_456", new Lazy<int>(() => 42));

// Copy all key-value pairs to an array
var buffer = new KeyValuePair<string, int>[map.Count];
map.CopyTo(buffer, 0);
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

map1 = map1.Union(map2);                // Union: adds all pairs from map2 into map1
map1 = map1.Intersect(map2);            // Intersect: keeps only pairs present in both
map1 = map1.ExceptWith(map2);           // ExceptWith: removes pairs that exist in map2
map1 = map1.SymmetricExceptWith(map2);  // SymmetricExceptWith: keeps only pairs in one but not both

// Read-only set query operations (extension methods):
bool isSubset = MultiMapHelper.IsSubsetOf(map1, map2);      // Check if map1 ⊆ map2
bool isSuperset = MultiMapHelper.IsSupersetOf(map1, map2);  // Check if map1 ⊇ map2
bool overlaps = MultiMapHelper.Overlaps(map1, map2);        // Check if they share any pairs
bool equals = MultiMapHelper.SetEquals(map1, map2);         // Check if they contain the same pairs

// Atomic set operations and set query methods for MultiMapLock:
var lockMap1 = new MultiMapLock<string, int>();
lockMap1.Add("A", 1);
lockMap1.Add("A", 2);
lockMap1.Add("B", 3);

var lockMap2 = new MultiMapLock<string, int>();
lockMap2.Add("A", 2);
lockMap2.Add("A", 3);
lockMap2.Add("C", 4);

lockMap1.Union(lockMap2);               // Union: adds all pairs from lockMap2 into lockMap1
lockMap1.Intersect(lockMap2);           // Intersect: keeps only pairs present in both
lockMap1.ExceptWith(lockMap2);          // ExceptWith: removes pairs that exist in lockMap2
lockMap1.SymmetricExceptWith(lockMap2); // SymmetricExceptWith: keeps only pairs in one but not both

bool isSubsetLock = lockMap1.IsSubsetOf(lockMap2);      // Check if lockMap1 ⊆ lockMap2
bool isSupersetLock = lockMap1.IsSupersetOf(lockMap2);  // Check if lockMap1 ⊇ lockMap2
bool overlapsLock = lockMap1.Overlaps(lockMap2);        // Check if they share any pairs
bool equalsLock = lockMap1.SetEquals(lockMap2);         // Check if they contain the same pairs

// Atomic set operations and set query methods for MultiMapAsync:
var asyncMap1 = new MultiMapAsync<string, int>();
await asyncMap1.AddAsync("A", 1);
await asyncMap1.AddAsync("A", 2);
await asyncMap1.AddAsync("B", 3);

var asyncMap2 = new MultiMapAsync<string, int>();
await asyncMap2.AddAsync("A", 2);
await asyncMap2.AddAsync("A", 3);
await asyncMap2.AddAsync("C", 4);

await asyncMap1.UnionAsync(asyncMap2);                  // Union: adds all pairs from asyncMap2 into asyncMap1
await asyncMap1.IntersectAsync(asyncMap2);              // Intersect: keeps only pairs present in both
await asyncMap1.ExceptWithAsync(asyncMap2);             // ExceptWith: removes pairs that exist in asyncMap2
await asyncMap1.SymmetricExceptWithAsync(asyncMap2);    // SymmetricExceptWith: keeps only pairs in one but not both

bool isSubsetAsync = await asyncMap1.IsSubsetOfAsync(asyncMap2);        // Check if asyncMap1 ⊆ asyncMap2
bool isSupersetAsync = await asyncMap1.IsSupersetOfAsync(asyncMap2);    // Check if asyncMap1 ⊇ asyncMap2
bool overlapsAsync = await asyncMap1.OverlapsAsync(asyncMap2);          // Check if they share any pairs
bool equalsAsync = await asyncMap1.SetEqualsAsync(asyncMap2);           // Check if they contain the same pairs
```

### SimpleMultiMap with Demo

```csharp
using MultiMap.Entities;
using MultiMap.Helpers;

ISimpleMultiMap<string, int> map = new SimpleMultiMap<string, int>();
map.Add("A", 1);
map.Add("A", 2);

var values = map.Get("A");                                      // [1, 2]
var safe = map.GetOrDefault("missing");                         // empty
bool tryGetA = map.TryGet("A", out var values);                 // true; values = [1, 2]
bool tryGetMissing = map.TryGet("missing", out var values);     // false; values = empty

// Enumerate directly — ISimpleMultiMap implements IEnumerable<KeyValuePair<TKey, TValue>>
foreach (var kvp in map) { /* ... */ }  // replaces map.Flatten()

// Set operations return the modified map
ISimpleMultiMap<string, int> union = map.Union(otherMap);
ISimpleMultiMap<string, int> intersection = map.Intersect(otherMap);
ISimpleMultiMap<string, int> exceptWith = map.ExceptWith(otherMap);
ISimpleMultiMap<string, int> symmetricExceptWith = map.SymmetricExceptWith(otherMap);

// Set query operations return bool
bool isSubsetOf = map.IsSubsetOf(otherMap);
bool isSupersetOf = map.IsSupersetOf(otherMap);
bool overlaps = map.Overlaps(otherMap);
bool setEquals = map.SetEquals(otherMap);
```
