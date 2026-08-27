using System.Collections;
using System.Reflection;
using MultiMap.Entities;
using MultiMap.Helpers;
using MultiMap.Interfaces;

namespace MultiMap.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// 1. ConcurrentMultiMap — uncovered paths
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class ConcurrentMultiMap_CoverageBoostTests
{
    // ── RemoveWhereFromCollection — predicate returns false for every element ──

    [Test]
    public void RemoveWhere_PredicateNeverMatches_RemovesNothing()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add("a", 1);
        map.Add("a", 2);
        map.Add("a", 3);

        int removed = map.RemoveWhere("a", _ => false);

        Assert.That(removed, Is.EqualTo(0));
        Assert.That(map.Count, Is.EqualTo(3));
    }

    [Test]
    public void RemoveWhere_PredicateMatchesSome_RemovesMatching()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add("a", 1);
        map.Add("a", 2);
        map.Add("a", 3);

        int removed = map.RemoveWhere("a", v => v % 2 == 0);

        Assert.That(removed, Is.EqualTo(1));
        Assert.That(map.Count, Is.EqualTo(2));
        Assert.That(map.GetOrDefault("a"), Does.Not.Contain(2));
    }

    // ── Equals(object?) — null, non-IReadOnlyMultiMap, and self ──

    [Test]
    public void Equals_Null_ReturnsFalse()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((object?)null), Is.False);
    }

    [Test]
    public void Equals_SameReference_ReturnsTrue()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>)map), Is.True);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_CountMismatch_ReturnsFalse()
    {
        var a = new ConcurrentMultiMap<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_ValueMismatch_ReturnsFalse()
    {
        var a = new ConcurrentMultiMap<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameContent_ReturnsTrue()
    {
        var a = new ConcurrentMultiMap<string, int>();
        a.Add("a", 1);
        a.Add("b", 2);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 1);
        b.Add("b", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.True);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_WithInjectedEmptyConcurrentSet_IsEmpty_BranchSkipped_ReturnsTrue()
    {
        // Cover the `kvp.Value.IsEmpty → continue` branch inside ConcurrentMultiMap.Equals.
        // We inject an empty ConcurrentSet directly into the underlying dictionary so that
        // the foreach loop visits an empty bucket. Because KeyCount and Count skip empty
        // buckets, both sides still agree on "1 key / 1 value", so the result is true.
        var a = new ConcurrentMultiMap<string, int>();
        a.Add("a", 1);

        var field = typeof(MultiMapBase<string, int, ConcurrentSet<int>>)
            .GetField("_dictionary", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var dict = (System.Collections.Concurrent.ConcurrentDictionary<string, ConcurrentSet<int>>)field.GetValue(a)!;

        var ctor = typeof(ConcurrentSet<int>)
            .GetConstructor(BindingFlags.Public | BindingFlags.Instance, null,
                            new[] { typeof(IEqualityComparer<int>) }, null)!;
        var emptySet = (ConcurrentSet<int>)ctor.Invoke(new object?[] { null });
        dict["ghost"] = emptySet;   // injected empty bucket — skipped by IsEmpty guard

        var b = new MultiMapSet<string, int>();
        b.Add("a", 1);

        // KeyCount / Count both agree (empty bucket is transparent), so Equals returns true.
        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.True);
    }

    // ── Concurrent stress: RemoveWhere under concurrent add ──

    [Test]
    [Category("Stress")]
    [Category("Concurrent")]
    public void RemoveWhere_ConcurrentWithAdd_NeverThrows()
    {
        var map = new ConcurrentMultiMap<string, int>();
        const int iterations = 500;

        Parallel.For(0, iterations, i =>
        {
            map.Add("k", i);
            map.RemoveWhere("k", v => v == i);
        });

        Assert.That(map.Count, Is.GreaterThanOrEqualTo(0));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// 2. MultiMapHelper — ExceptWith self-reference + async gap coverage
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MultiMapHelper_CoverageBoostTests
{
    // ── ExceptWith(ISimpleMultiMap, ISimpleMultiMap) — self-reference path ──

    [Test]
    public void ExceptWith_SelfReference_ClearsMap()
    {
        var map = new MultiMapSet<string, int>();
        map.Add("a", 1);
        map.Add("b", 2);

        // Cast to ISimpleMultiMap to call the extension method signature
        ISimpleMultiMap<string, int> imap = map;
        imap.ExceptWith((IReadOnlySimpleMultiMap<string, int>)map);

        Assert.That(map.Count, Is.EqualTo(0));
        Assert.That(map.KeyCount, Is.EqualTo(0));
    }

    // ── ExceptWithAsync — removes pairs that ARE in other; keeps pairs NOT in other ──

    [Test]
    public async Task ExceptWithAsync_KeyAbsentInOther_KeepsTargetPair()
    {
        await using var target = new MultiMapAsync<string, int>();
        await target.AddAsync("a", 1);
        await target.AddAsync("b", 2); // "b" is NOT in other — should be kept

        await using var other = new MultiMapAsync<string, int>();
        await other.AddAsync("a", 1); // only "a":1 is in other — should be removed from target

        await target.ExceptWithAsync(other);

        // "a":1 removed (it was in other); "b":2 kept (it was not in other)
        Assert.That(await target.ContainsKeyAsync("a"), Is.False);
        Assert.That(await target.ContainsKeyAsync("b"), Is.True);
        Assert.That(await target.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task ExceptWithAsync_SameInstance_ClearsMap()
    {
        await using var map = new MultiMapAsync<string, int>();
        await map.AddAsync("a", 1);
        await map.AddAsync("b", 2);

        await map.ExceptWithAsync(map);

        Assert.That(await map.GetCountAsync(), Is.EqualTo(0));
    }

    // ── SetEqualsAsync — value in target not in other ──

    [Test]
    public async Task SetEqualsAsync_ValueMismatch_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("k", 1);
        await a.AddAsync("k", 2);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("k", 1);
        await b.AddAsync("k", 99); // 2 != 99

        bool result = await a.SetEqualsAsync(b);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task SetEqualsAsync_EqualMaps_ReturnsTrue()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("k", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("k", 1);

        Assert.That(await a.SetEqualsAsync(b), Is.True);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// 3. MultiMapAsync — Equals(object?) null path + EqualsAsync(object?) null path
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MultiMapAsync_EqualsNullPathTests
{
    [Test]
    public void Equals_Object_Null_ReturnsFalse()
    {
        using var map = new MultiMapAsync<string, int>();

        Assert.That(map.Equals((object?)null), Is.False);
    }

    [Test]
    public void Equals_Object_NonMultiMapType_ReturnsFalse()
    {
        using var map = new MultiMapAsync<string, int>();

        Assert.That(map.Equals("not a multimap"), Is.False);
    }

    [Test]
    public async Task EqualsAsync_Object_Null_ReturnsFalse()
    {
        await using var map = new MultiMapAsync<string, int>();

        bool result = await map.EqualsAsync((object?)null);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task EqualsAsync_Object_NonMultiMapType_ReturnsFalse()
    {
        await using var map = new MultiMapAsync<string, int>();

        bool result = await map.EqualsAsync((object?)"not a multimap");

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task EqualsAsync_Interface_Null_ReturnsFalse()
    {
        await using var map = new MultiMapAsync<string, int>();

        bool result = await map.EqualsAsync((IReadOnlyMultiMapAsync<string, int>?)null);

        Assert.That(result, Is.False);
    }

    // ── General (foreign-implementation) comparison path ──

    [Test]
    public async Task Equals_ForeignIReadOnlyMultiMapAsync_SameContent_ReturnsTrue()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("x", 10);

        // Wrap 'a' behind a proxy that is NOT a MultiMapAsync<,> instance
        var proxy = new ReadOnlyMultiMapAsyncProxy<string, int>(a);

        Assert.That(a.Equals((IReadOnlyMultiMapAsync<string, int>)proxy), Is.True);
    }

    [Test]
    public async Task Equals_ForeignIReadOnlyMultiMapAsync_DifferentContent_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("x", 10);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("x", 99);

        var proxy = new ReadOnlyMultiMapAsyncProxy<string, int>(b);

        Assert.That(a.Equals((IReadOnlyMultiMapAsync<string, int>)proxy), Is.False);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// 4. MultiMapBase.ValuesEnumerator — IEnumerator.Current explicit implementation
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MultiMapBase_ValuesEnumerator_ExplicitCurrentTests
{
    [Test]
    public void ValuesEnumerator_ExplicitIEnumeratorCurrent_ReturnsCorrectValue()
    {
        var map = new MultiMapSet<string, int>();
        map.Add("a", 42);
        map.Add("a", 99);

        // Values is a ValuesCollection; GetEnumerator() returns a ValuesEnumerator struct.
        // Obtain the non-generic IEnumerator to hit the explicit IEnumerator.Current property.
        var valuesCollection = map.Values;
        IEnumerable nonGeneric = valuesCollection;
        var enumerator = nonGeneric.GetEnumerator();

        var results = new List<object?>();
        while (enumerator.MoveNext())
        {
            results.Add(enumerator.Current);
        }

        Assert.That(results, Is.EquivalentTo(new object[] { 42, 99 }));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// 5. MultiMapList — capacity+comparer constructor
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MultiMapList_ConstructorTests
{
    [Test]
    public void Constructor_CapacityAndComparer_Works()
    {
        var map = new MultiMapList<string, int>(16, StringComparer.OrdinalIgnoreCase);
        map.Add("Key", 1);
        map.Add("KEY", 2); // same key under OrdinalIgnoreCase

        Assert.That(map.Count, Is.EqualTo(2));
        Assert.That(map.KeyCount, Is.EqualTo(1));
        Assert.That(map.GetOrDefault("key"), Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public void Constructor_CapacityOnly_Works()
    {
        var map = new MultiMapList<string, int>(32);
        map.Add("a", 1);

        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Constructor_ValueComparer_Works()
    {
        var map = new MultiMapList<string, string>(StringComparer.Ordinal);
        map.Add("k", "hello");
        map.Add("k", "HELLO");

        Assert.That(map.Count, Is.EqualTo(2));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// 6. MultiMapSet — self-reference + value-mismatch Equals paths
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MultiMapSet_EqualsCoverageTests
{
    [Test]
    public void Equals_SameReference_ReturnsTrue()
    {
        var map = new MultiMapSet<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>)map), Is.True);
    }

    [Test]
    public void Equals_Object_SameReference_ReturnsTrue()
    {
        var map = new MultiMapSet<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((object)map), Is.True);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_ValueMismatch_ReturnsFalse()
    {
        var a = new MultiMapSet<string, int>();
        a.Add("k", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("k", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameContent_ReturnsTrue()
    {
        var a = new MultiMapSet<string, int>();
        a.Add("k", 1);
        a.Add("k", 2);

        var b = new MultiMapSet<string, int>();
        b.Add("k", 2);
        b.Add("k", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.True);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// 7. SortedMultiMap — self-reference + value-mismatch Equals paths
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class SortedMultiMap_EqualsCoverageTests
{
    [Test]
    public void Equals_SameReference_ReturnsTrue()
    {
        var map = new SortedMultiMap<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>)map), Is.True);
    }

    [Test]
    public void Equals_Object_SameReference_ReturnsTrue()
    {
        var map = new SortedMultiMap<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((object)map), Is.True);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_MissingKeyInOther_ReturnsFalse()
    {
        var a = new SortedMultiMap<string, int>();
        a.Add("a", 1);

        var b = new SortedMultiMap<string, int>();
        b.Add("b", 1); // different key

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// 8. MultiMapLock — SetOperations self-reference paths
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MultiMapLock_SetOperationsSelfReferenceTests
{
    [Test]
    public void IsSubsetOf_SelfReference_ReturnsTrue()
    {
        using var map = new MultiMapLock<string, int>();
        map.Add("a", 1);
        map.Add("b", 2);

        Assert.That(map.IsSubsetOf(map), Is.True);
    }

    [Test]
    public void IsSupersetOf_SelfReference_ReturnsTrue()
    {
        using var map = new MultiMapLock<string, int>();
        map.Add("a", 1);

        Assert.That(map.IsSupersetOf(map), Is.True);
    }

    [Test]
    public void SetEquals_SelfReference_ReturnsTrue()
    {
        using var map = new MultiMapLock<string, int>();
        map.Add("a", 1);
        map.Add("b", 2);

        Assert.That(map.SetEquals(map), Is.True);
    }

    [Test]
    public void Overlaps_SelfReference_NonEmpty_ReturnsTrue()
    {
        using var map = new MultiMapLock<string, int>();
        map.Add("a", 1);

        Assert.That(map.Overlaps(map), Is.True);
    }

    // ── SetEquals — value mismatch path ──

    [Test]
    public void SetEquals_IMultiMap_SameKeysDifferentValues_ReturnsFalse()
    {
        using var target = new MultiMapLock<string, int>();
        target.Add("a", 1);

        var other = new MultiMapSet<string, int>();
        other.Add("a", 99);

        Assert.That(target.SetEquals(other), Is.False);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// 9. Concurrent stress tests for MultiMapAsync helper paths
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
[Category("Stress")]
[Category("Concurrent")]
public class MultiMapAsync_ConcurrentHelperStressTests
{
    [Test]
    public async Task ExceptWithAsync_ConcurrentMutationOnTarget_NeverThrows()
    {
        await using var target = new MultiMapAsync<string, int>();
        await using var other = new MultiMapAsync<string, int>();

        // Seed both with shared content
        for (int i = 0; i < 50; i++)
        {
            await target.AddAsync($"k{i % 5}", i);
            await other.AddAsync($"k{i % 5}", i);
        }

        var mutator = Task.Run(async () =>
        {
            for (int i = 0; i < 100; i++)
            {
                await target.AddAsync($"k{i % 5}", i + 1000);
            }
        });

        var exceptor = Task.Run(() => target.ExceptWithAsync(other));

        Assert.DoesNotThrowAsync(async () => await Task.WhenAll(mutator, exceptor));
    }

    [Test]
    public async Task SetEqualsAsync_ConcurrentMutation_NeverThrows()
    {
        await using var a = new MultiMapAsync<string, int>();
        await using var b = new MultiMapAsync<string, int>();

        for (int i = 0; i < 20; i++)
        {
            await a.AddAsync("k", i);
            await b.AddAsync("k", i);
        }

        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        var mutator = Task.Run(async () =>
        {
            int i = 0;
            while (!cts.Token.IsCancellationRequested)
            {
                await a.AddAsync("k", i++ + 10000);
            }
        });

        var checker = Task.Run(async () =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                _ = await a.SetEqualsAsync(b);
            }
        });

        Assert.DoesNotThrowAsync(async () => await Task.WhenAll(mutator, checker));
    }

    [Test]
    public async Task ExceptWithAsync_EmptyOther_LeavesTargetUnchanged()
    {
        await using var target = new MultiMapAsync<string, int>();
        await target.AddAsync("a", 1);
        await target.AddAsync("b", 2);

        await using var other = new MultiMapAsync<string, int>(); // empty

        await target.ExceptWithAsync(other);

        Assert.That(await target.GetCountAsync(), Is.EqualTo(2));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// 10. MultiMapLock — concurrent stress: IsSubsetOf / IsSupersetOf racing writes
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
[Category("Stress")]
[Category("Concurrent")]
public class MultiMapLock_AdditionalStressTests
{
    [Test]
    public void SetEquals_UnderConcurrentMutation_NeverThrows()
    {
        using var a = new MultiMapLock<string, int>();
        using var b = new MultiMapLock<string, int>();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

        var writer = Task.Run(() =>
        {
            int i = 0;
            while (!cts.Token.IsCancellationRequested)
            {
                a.Add($"k{i % 10}", i % 50);
                b.Add($"k{i % 10}", i % 50);
                i++;
            }
        });

        var reader = Task.Run(() =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                _ = a.SetEquals(b);
            }
        });

        Assert.DoesNotThrow(() => Task.WaitAll(writer, reader));
    }

    [Test]
    public void Dispose_CalledTwice_IsIdempotent()
    {
        var map = new MultiMapLock<string, int>();
        map.Add("a", 1);

        map.Dispose();
        Assert.DoesNotThrow(() => map.Dispose());
    }

    [Test]
    public void GetHashCode_AfterMultipleAdds_IsConsistent()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("x", 1);
        a.Add("y", 2);

        int h1 = a.GetHashCode();
        int h2 = a.GetHashCode();

        Assert.That(h1, Is.EqualTo(h2));
    }

    [Test]
    public void Equals_WithIReadOnlySimpleMultiMap_SameContent_ReturnsTrue()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("k", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("k", 1);

        Assert.That(a.Equals((IReadOnlySimpleMultiMap<string, int>)b), Is.True);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// 11. MultiMapAsync — EqualsAsync general path (foreign implementation)
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MultiMapAsync_EqualsAsyncForeignTests
{
    [Test]
    public async Task EqualsAsync_ForeignImplementation_SameContent_ReturnsTrue()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("k", 1);

        var proxy = new ReadOnlyMultiMapAsyncProxy<string, int>(a);

        bool result = await a.EqualsAsync(proxy);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task EqualsAsync_ForeignImplementation_DifferentContent_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("k", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("k", 99);

        var proxy = new ReadOnlyMultiMapAsyncProxy<string, int>(b);

        bool result = await a.EqualsAsync(proxy);

        Assert.That(result, Is.False);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Helper: a thin IReadOnlyMultiMapAsync<,> proxy around a real MultiMapAsync<,>
// that is NOT itself an instance of MultiMapAsync<,> — forces the general path.
// ─────────────────────────────────────────────────────────────────────────────

internal sealed class ReadOnlyMultiMapAsyncProxy<TKey, TValue>(MultiMapAsync<TKey, TValue> inner)
    : IReadOnlyMultiMapAsync<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
{
    public ValueTask<IEnumerable<TValue>> GetAsync(TKey key, CancellationToken ct = default)
        => inner.GetAsync(key, ct);

    public ValueTask<IEnumerable<TValue>> GetOrDefaultAsync(TKey key, CancellationToken ct = default)
        => inner.GetOrDefaultAsync(key, ct);

    public ValueTask<(bool found, IEnumerable<TValue> values)> TryGetAsync(TKey key, CancellationToken ct = default)
        => inner.TryGetAsync(key, ct);

    public ValueTask<bool> ContainsKeyAsync(TKey key, CancellationToken ct = default)
        => inner.ContainsKeyAsync(key, ct);

    public ValueTask<bool> ContainsAsync(TKey key, TValue value, CancellationToken ct = default)
        => inner.ContainsAsync(key, value, ct);

    public ValueTask<int> GetKeyCountAsync(CancellationToken ct = default)
        => inner.GetKeyCountAsync(ct);

    public ValueTask<int> GetCountAsync(CancellationToken ct = default)
        => inner.GetCountAsync(ct);

    public ValueTask<IEnumerable<TKey>> GetKeysAsync(CancellationToken ct = default)
        => inner.GetKeysAsync(ct);

    public ValueTask<IEnumerable<TValue>> GetValuesAsync(CancellationToken ct = default)
        => inner.GetValuesAsync(ct);

    public ValueTask<int> GetValuesCountAsync(TKey key, CancellationToken ct = default)
        => inner.GetValuesCountAsync(key, ct);

    public IAsyncEnumerator<KeyValuePair<TKey, TValue>> GetAsyncEnumerator(CancellationToken ct = default)
        => inner.GetAsyncEnumerator(ct);

    public bool Equals(IReadOnlyMultiMapAsync<TKey, TValue>? other)
        => inner.Equals(other);

    public ValueTask<bool> EqualsAsync(object? obj)
        => inner.EqualsAsync(obj);

    public ValueTask<bool> EqualsAsync(IReadOnlyMultiMapAsync<TKey, TValue>? other, CancellationToken ct = default)
        => inner.EqualsAsync(other, ct);

    public void Dispose() { /* proxy does not own inner */ }

    public ValueTask DisposeAsync() => default;
}
