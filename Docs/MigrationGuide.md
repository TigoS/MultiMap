# MultiMap

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0%20%7C%208.0%20%7C%20Standard%202.0-blue.svg)](https://dotnet.microsoft.com/)
[![C# 14](https://img.shields.io/badge/C%23-14.0-blue)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![NUnit](https://img.shields.io/badge/tests-NUnit%204-green)](https://nunit.org/)
[![BenchmarkDotNet](https://img.shields.io/badge/BenchmarkDotNet-v0.15.8-blue)](https://benchmarkdotnet.org/)
[![Test SDK](https://img.shields.io/badge/Microsoft.NET.Test.Sdk-v18.6.0-blue)](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk)
[![NuGet](https://img.shields.io/nuget/v/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MultiMap.svg)](https://www.nuget.org/packages/MultiMap/)
[![Coverage](https://img.shields.io/badge/coverage-95.86%25-brightgreen)]()

A **.NET** library providing various multimap implementations (set, list, sorted, concurrent, lock-based, async) that map generic keys to collections of generic values with set operations, benchmarks, and thread-safe variants, targeting **.NET 10**, **.NET 8**, and **.NET Standard 2.0**.

## Table of Contents

- [Migration Guide](#migration-guide)
  - [Upgrading to Version 2.1.0+](#upgrading-to-version-210)
  - [Upgrading to Version 2.0.1+](#upgrading-to-version-201)
  - [Upgrading to Version 1.0.12+](#upgrading-to-version-1012)
  - [Upgrading to Version 1.0.11+](#upgrading-to-version-1011)
  - [Upgrading to Version 1.0.8+](#upgrading-to-version-108)
  - [Upgrading to Version 1.0.7+](#upgrading-to-version-107)
  
## Migration Guide

### Upgrading to Version 2.1.0+

Version 2.1.0 adds read-only set query operations (`IsSubsetOf`, `IsSupersetOf`, `Overlaps`, `SetEquals`) and updates the interface surface.

#### Interface Additions (may affect custom implementations)

- `IReadOnlySimpleMultiMap<TKey, TValue>.TryGet(TKey key, out IEnumerable<TValue> values)` was added.
- `ISimpleMultiMap<TKey, TValue>.Clear()` was added.

If you implement these interfaces directly, add the new members to your implementation.

### Upgrading to Version 2.0.1+

Version 2.0.1 removes two `ISimpleMultiMap` members that were soft-deprecated in v1.0.12, completing their removal as a **source-breaking change**.

#### Breaking Changes

**`ISimpleMultiMap.Clear(TKey)` removed (v2.0.1):**

`Clear(TKey key)` was deprecated in v1.0.12 as an `[Obsolete]` alias for `RemoveKey(TKey key)`. It has now been removed. Update any remaining call sites:

```csharp
// Before (v1.0.x — emitted CS0618 warning since v1.0.12)
map.Clear("keyA");

// After (v2.0.1)
map.RemoveKey("keyA");
```

If you implement `ISimpleMultiMap` directly, remove your `Clear(TKey key)` override (it is no longer part of the contract).

---

**`ISimpleMultiMap.Flatten()` removed (v2.0.1):**

`Flatten()` was deprecated in v1.0.12 because `ISimpleMultiMap<TKey, TValue>` already implements `IEnumerable<KeyValuePair<TKey, TValue>>`. It has now been removed. Replace any remaining usages with direct enumeration:

```csharp
// Before (v1.0.x — emitted CS0618 warning since v1.0.12)
foreach (var kvp in map.Flatten()) { /* ... */ }
var pairs = map.Flatten().ToList();

// After (v2.0.1)
foreach (var kvp in map) { /* ... */ }
var pairs = map.ToList();
```

#### Recommended Upgrade Steps

1. **Update NuGet package:**
   ```bash
   dotnet add package MultiMap --version 2.0.1
   ```

2. **Replace `Clear(key)` with `RemoveKey(key)`** at every call site that was producing a CS0618 warning.

3. **Replace `Flatten()` with direct enumeration** at every call site that was producing a CS0618 warning.

4. **If you implement `ISimpleMultiMap` directly**, remove any `Clear(TKey)` and `Flatten()` override methods — they are no longer part of the interface contract.

#### Compatibility

All other APIs introduced in v1.0.12 (including `RemoveKey`, `SimpleMultiMap.Count`, `SimpleMultiMap.Equals`, the new constructors, and all `IMultiMap` / `IMultiMapAsync` members) remain **fully backward-compatible**.

### Upgrading to Version 1.0.12+

Version 1.0.12 is focused on **correctness, consistency, and performance**. There is one source-breaking API change, one backward-compatible rename alias, and one soft deprecation in `ISimpleMultiMap`.

#### Breaking Changes

**`ISimpleMultiMap.Remove` return type changed from `void` to `bool` (v1.0.12):**

`ISimpleMultiMap.Remove(TKey, TValue)` previously returned `void`, which was inconsistent with the `IMultiMap.Remove(TKey, TValue)` signature. The return type has been changed to `bool` so both interfaces are consistent.

**Before (v1.0.11):**
```csharp
map.Remove("A", 1);  // void — no way to know if the pair was actually removed
```

**After (v1.0.12):**
```csharp
bool removed = map.Remove("A", 1);  // true if the pair was found and removed
```

If you implement `ISimpleMultiMap` directly, update your `Remove` signature from `void` to `bool`. Callers that ignored the return value require no changes — the call still compiles.

---

**`ISimpleMultiMap.Clear(TKey)` renamed to `ISimpleMultiMap.RemoveKey(TKey)` (v1.0.12) — backward-compatible:**

`ISimpleMultiMap.Clear(TKey key)` has been renamed to `ISimpleMultiMap.RemoveKey(TKey key)` to align with `IMultiMap.RemoveKey(TKey key)` and make the API surface consistent across all multimap interfaces.

**Before (v1.0.11):**
```csharp
map.Clear("keyA");  // removes all values for "keyA"
```

**After (v1.0.12):**
```csharp
map.RemoveKey("keyA");  // removes all values for "keyA"
```

For backward compatibility, `Clear(TKey key)` is retained in `ISimpleMultiMap` as an `[Obsolete]` alias that forwards directly to `RemoveKey(TKey key)`. Existing call sites continue to compile and run; a compiler warning (`CS0618`) is emitted to guide migration. Migrate call sites to `map.RemoveKey(key)` before the next major version, when `Clear(key)` will be removed. Note: the parameterless `Clear()` on `IMultiMap` implementations is unaffected.

---

**`ISimpleMultiMap.Flatten()` deprecated (v1.0.12):**

`ISimpleMultiMap.Flatten()` is decorated with `[Obsolete]` and will be removed in a future version. The method was always equivalent to enumerating the map directly — `ISimpleMultiMap<TKey, TValue>` implements `IEnumerable<KeyValuePair<TKey, TValue>>`, so iterating the map with `foreach`, `ToList()`, or any LINQ method produces the exact same sequence.

**Before (v1.0.11):**
```csharp
foreach (var kvp in map.Flatten()) { /* ... */ }
var pairs = map.Flatten().ToList();
```

**After (v1.0.12):**
```csharp
foreach (var kvp in map) { /* ... */ }
var pairs = map.ToList();
```

This is a **soft deprecation** — existing call sites continue to compile and run without change; a compiler warning (`CS0618`) is emitted to guide migration. No immediate action is required, but callers should migrate before the next major version.

#### Non-Breaking Changes

- **`SimpleMultiMap.Count` property added:** `SimpleMultiMap<TKey, TValue>` now exposes a `Count` property (inherited via `IReadOnlyCollection<KeyValuePair<TKey, TValue>>`) that returns the total number of key-value pairs across all keys (**O(1)** — backed by `_count`).

- **`SimpleMultiMap.Equals(IReadOnlySimpleMultiMap<TKey, TValue>? other)` added:** Typed equality compares the total pair count first (fast exit), then verifies per-key value-set contents using set-equality semantics.

- **`SimpleMultiMap` equality bug fix:** `Equals(IReadOnlySimpleMultiMap<TKey, TValue>? other)` previously compared `_dictionary.Count` (key count) against `other.Count` (total pair count), producing incorrect results when maps had equal key counts but different numbers of values. Fixed to compare total pair counts on both sides.

- **`MultiMapAsync` typed equality:**

- **`ConcurrentMultiMap` is now fully lock-free:** The internal storage changed from `ConcurrentDictionary<TKey, HashSet<TValue>>` with per-key locking and an `Interlocked` counter to `ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, byte>>`. All per-key read and write operations are now lock-free. `Count` and `KeyCount` are **O(1)** — backed by `_count` and `_keyCount` fields updated via `Interlocked` in every mutating path.

- **`SymmetricExceptWith` optimization for `IMultiMap`:** The `IMultiMap` overload now uses a per-key lookup dictionary (same strategy as the `ISimpleMultiMap` overload) to avoid redundant lock acquisitions when multiple entries share the same key.

- **Zero-allocation `Values` property and `GetValuesAsync()`:** `MultiMapBase.Values`, `MultiMapLock.Values`, and `MultiMapAsync.GetValuesAsync()` now use a custom struct enumerator instead of `SelectMany` LINQ iterators, eliminating per-access heap allocations on hot read paths.

- **`MultiMapBase` partial classes:** The nested `ValuesCollection` and `ValuesEnumerator` types were extracted into separate `MultiMapBase.ValuesCollection.cs` and `MultiMapBase.ValuesEnumerator.cs` partial files for better code organization.

- **All concrete classes are now `sealed`:** Every concrete implementation (`MultiMapList`, `MultiMapSet`, `SortedMultiMap`, `ConcurrentMultiMap`, `MultiMapLock`, `MultiMapAsync`, `SimpleMultiMap`) is declared `sealed`, enabling JIT devirtualization on hot paths such as `Add` and `Remove`.

- **Null-value guard on `AddRange`:** A runtime guard was added to prevent `null` values from silently entering list-backed collections, preserving the `TValue: notnull` contract at runtime.

- **`MultiMapList` equality fix:** `MultiMapList.Equals(object?)` previously used `SequenceEqual`, which is order-dependent. The comparison now uses set-based equality, so two lists with the same content in a different insertion order compare equal.

- **`MultiMapSet(IEqualityComparer<TKey>?, IEqualityComparer<TValue>?)` constructor added:** A combined key-and-value comparer overload fills the gap between the separate key-only and value-only overloads, bringing `MultiMapSet` to a full 8-overload family.

- **`ConcurrentMultiMap` key-comparer constructors added:** `ConcurrentMultiMap(IEqualityComparer<TKey>?)` and `ConcurrentMultiMap(IEqualityComparer<TKey>?, IEqualityComparer<TValue>?)` overloads are now available, bringing `ConcurrentMultiMap` to a full 8-overload family on par with the other implementations.

- **`MultiMapList.AddRange(IEnumerable<KeyValuePair<TKey, TValue>>)` optimized on .NET 6+:** The KVP-sequence overload now overrides the base-class implementation and uses `CollectionsMarshal.GetValueRefOrAddDefault` on .NET 6 and later, eliminating the per-item virtual dispatch through the base class and matching the existing `Add` and `AddRange(key, values)` fast paths.

#### Recommended Upgrade Steps

1. **Update NuGet package:**
   ```bash
   dotnet add package MultiMap --version 2.0.1
   ```

2. **If you implement `ISimpleMultiMap` directly:**
   - Change your `Remove(TKey key, TValue value)` signature from `void` to `bool` and return `true` when the pair was removed.
   - Rename your `Clear(TKey key)` method to `RemoveKey(TKey key)`.

3. **Update `Clear(key)` call sites** (emits CS0618 warning; hard-removed in v2.0.1):
   ```csharp
   // Before
   map.Clear("keyA");

   // After
   map.RemoveKey("keyA");
   ```

4. **Migrate away from `Flatten()` (emits CS0618 warning; hard-removed in v2.0.1):**
   ```csharp
   // Before
   foreach (var kvp in map.Flatten()) { /* ... */ }
   var pairs = map.Flatten().ToList();

   // After — enumerate the map directly
   foreach (var kvp in map) { /* ... */ }
   var pairs = map.ToList();
   ```

5. **Adopt the `Remove` return value where useful:**
   ```csharp
   // Before: result was silently discarded
   map.Remove("key", value);

   // After: check whether anything was actually removed
   if (!map.Remove("key", value))
       Console.WriteLine("Pair not found");
   ```

#### Compatibility

All other APIs are **fully backward-compatible**. The one source-breaking change in v1.0.12 is the `ISimpleMultiMap.Remove` return type. `Clear(TKey)` and `Flatten()` emitted CS0618 deprecation warnings since v1.0.12 and have been **hard-removed in v2.0.1** — see the [Upgrading to 2.0.1+](#upgrading-to-version-201) section above.

### Upgrading to Version 1.0.11+

Version 1.0.11 changes the return types of `AddRange` and `AddRangeAsync` to report the number of pairs actually added. This is a **source-breaking change** if you relied on the previous `void`/`Task` signatures.

#### Breaking Changes

**`AddRange` return type changed from `void` to `int` (v1.0.11):**

**Before (v1.0.8–1.0.10):** `AddRange` returned `void`
```csharp
map.AddRange(items);  // No return value
```

**After (v1.0.11):** `AddRange` returns `int` — the count of successfully added pairs
```csharp
int added = map.AddRange(items);  // Returns number of pairs actually added
```

**`AddRangeAsync` return type changed from `Task` to `Task<int>` (v1.0.11):**

**Before (v1.0.8–1.0.10):** `AddRangeAsync` returned `Task`
```csharp
await map.AddRangeAsync(items);  // No return value
```

**After (v1.0.11):** `AddRangeAsync` returns `Task<int>` — the count of successfully added pairs
```csharp
int added = await map.AddRangeAsync(items);  // Returns number of pairs actually added
```

> **Note:** The `AddRange(key, values)` and `AddRangeAsync(key, values)` overloads already returned `int` and are unchanged.

#### Recommended Upgrade Steps

1. **Update NuGet package:**
   ```bash
   dotnet add package MultiMap --version 1.0.11
   ```

2. **Update call sites:**
   - If you ignored the return value, no changes are needed — the call still compiles
   - If you assigned the result or passed it to a method expecting `void`/`Task`, update to handle the `int`/`Task<int>` return type

3. **If you implement `IMultiMap` or `IMultiMapAsync` directly:**
   - Change your `AddRange(IEnumerable<KeyValuePair<TKey, TValue>>)` signature from `void` to `int`
   - Change your `AddRangeAsync(IEnumerable<KeyValuePair<TKey, TValue>>)` signature from `Task` to `Task<int>`

#### Compatibility

All other APIs remain unchanged. The only breaking changes are the `AddRange` and `AddRangeAsync` return types on the `IEnumerable<KeyValuePair>` overloads.

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
| `AddRange(items)` | `int` method | Bulk insert from `IEnumerable<KeyValuePair>`; returns count added |
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
| `AddRangeAsync(items)` | `Task<int>` | Async bulk insert from `IEnumerable<KeyValuePair>`; returns count added |
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

**Before:** Getting all values required for flattening
```csharp
var allValues = map.Select(kvp => kvp.Value);  // enumerate directly
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
// Remove all even numbers associated with the "numbers" key
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
// OLD: Accepts a mutable interface even though it doesn't modify
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

