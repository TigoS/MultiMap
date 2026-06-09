using MultiMap.Entities;
using MultiMap.Interfaces;

namespace MultiMap.Tests;

/// <summary>
/// Additional comprehensive tests to increase coverage of MultiMap implementations,
/// focusing on edge cases, boundary conditions, and complex scenarios.
/// </summary>
[TestFixture]
public class AdditionalCoverageTests
{
    // ── MultiMapSet additional edge cases ───────────────────

    [Test]
    public void MultiMapSet_Add_DuplicateValue_ReturnsFalseAndDoesNotIncrement()
    {
        var map = new MultiMapSet<string, int>();

        bool first = map.Add("key", 1);
        bool second = map.Add("key", 1);

        Assert.That(first, Is.True);
        Assert.That(second, Is.False);
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void MultiMapSet_RemoveRange_PartialMatch_RemovesOnlyExisting()
    {
        var map = new MultiMapSet<string, int>();
        map.Add("a", 1);
        map.Add("a", 2);
        map.Add("b", 3);

        var removed = map.RemoveRange(new[]
        {
            new KeyValuePair<string, int>("a", 1),
            new KeyValuePair<string, int>("a", 99),
            new KeyValuePair<string, int>("c", 3)
        });

        Assert.That(removed, Is.EqualTo(1));
        Assert.That(map.Count, Is.EqualTo(2));
    }

    // ── MultiMapList additional edge cases ──────────────────

    [Test]
    public void MultiMapList_Add_DuplicateValue_ReturnsTrue()
    {
        var map = new MultiMapList<string, int>();

        bool first = map.Add("key", 1);
        bool second = map.Add("key", 1);

        Assert.That(first, Is.True);
        Assert.That(second, Is.True);
        Assert.That(map.Count, Is.EqualTo(2));
    }

    [Test]
    public void MultiMapList_GetOrDefault_ReturnsValuesInOrder()
    {
        var map = new MultiMapList<string, int>();
        map.Add("key", 3);
        map.Add("key", 1);
        map.Add("key", 2);

        var values = map.GetOrDefault("key").ToList();

        Assert.That(values, Is.EquivalentTo(new[] { 3, 1, 2 }));
    }

    // ── SortedMultiMap additional edge cases ───────────────

    [Test]
    public void SortedMultiMap_ValuesAreSorted()
    {
        var map = new SortedMultiMap<string, int>();
        map.Add("key", 3);
        map.Add("key", 1);
        map.Add("key", 2);

        var values = map.GetOrDefault("key").ToList();

        Assert.That(values, Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void SortedMultiMap_MultipleKeys_EachSorted()
    {
        var map = new SortedMultiMap<string, int>();
        map.Add("a", 3);
        map.Add("a", 1);
        map.Add("b", 5);
        map.Add("b", 2);

        var aValues = map.Get("a").ToList();
        var bValues = map.Get("b").ToList();

        Assert.That(aValues, Is.EquivalentTo(new[] { 1, 3 }));
        Assert.That(bValues, Is.EquivalentTo(new[] { 2, 5 }));
    }

    // ── ConcurrentMultiMap specific scenarios ──────────────

    [Test]
    public void ConcurrentMultiMap_ConcurrentAdd_NoLoss()
    {
        var map = new ConcurrentMultiMap<string, int>();
        var tasks = Enumerable.Range(0, 100)
            .Select(i => Task.Run(() => map.Add($"key{i % 10}", i)))
            .ToArray();

        Task.WaitAll(tasks);

        Assert.That(map.Count, Is.EqualTo(100));
    }

    [Test]
    public void ConcurrentMultiMap_GetOrDefault_ThreadSafe()
    {
        var map = new ConcurrentMultiMap<string, int>();
        for (int i = 0; i < 100; i++)
            map.Add("key", i);

        var results = new System.Collections.Concurrent.ConcurrentBag<int>();
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() =>
            {
                var values = map.GetOrDefault("key").ToList();
                foreach (var v in values)
                    results.Add(v);
            }))
            .ToArray();

        Task.WaitAll(tasks);

        Assert.That(results.Count, Is.EqualTo(1000)); // 100 values × 10 reads
    }

    // ── MultiMapAsync specific scenarios ──────────────────

    [Test]
    public async Task MultiMapAsync_AddAsync_Completes()
    {
        var map = new MultiMapAsync<string, int>();

        bool result = await map.AddAsync("key", 1);

        Assert.That(result, Is.True);
        var values = await map.GetOrDefaultAsync("key");
        Assert.That(values.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task MultiMapAsync_GetAsync_Completes()
    {
        var map = new MultiMapAsync<string, int>();
        await map.AddAsync("key", 1);

        var values = await map.GetAsync("key");

        Assert.That(values, Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public async Task MultiMapAsync_ConcurrentOperations_NoRaceCondition()
    {
        var map = new MultiMapAsync<string, int>();

        var addTasks = Enumerable.Range(0, 50)
            .Select(i => map.AddAsync("key", i).AsTask())
            .ToArray();

        await Task.WhenAll(addTasks);

        var values = await map.GetAsync("key");

        Assert.That(values.Count, Is.EqualTo(50));
    }

    // ── MultiMapLock specific scenarios ────────────────────

    [Test]
    public void MultiMapLock_UnionAsync_Completes()
    {
        var map1 = new MultiMapLock<string, int>();
        var map2 = new MultiMapLock<string, int>();
        map1.Add("a", 1);
        map2.Add("a", 2);

        map1.Union(map2);

        Assert.That(map1.Count, Is.EqualTo(2));
    }

    [Test]
    public void MultiMapLock_ExceptWith_RemovesCorrectly()
    {
        var map1 = new MultiMapLock<string, int>();
        var map2 = new MultiMapLock<string, int>();

        map1.Add("a", 1);
        map1.Add("a", 2);
        map1.Add("b", 3);

        map2.Add("a", 1);

        map1.ExceptWith(map2);

        // After except: "a"->2, "b"->3
        Assert.That(map1.Count, Is.EqualTo(2));
        Assert.That(map1.Contains("a", 2), Is.True);
        Assert.That(map1.Contains("a", 1), Is.False);
    }

    // ── SimpleMultiMap specific scenarios ──────────────────

    [Test]
    public void SimpleMultiMap_Add_DuplicateNotStored()
    {
        var map = new SimpleMultiMap<string, int>();

        map.Add("key", 1);
        map.Add("key", 1);

        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void SimpleMultiMap_Get_SnapshotNotLive()
    {
        var map = new SimpleMultiMap<string, int>();
        map.Add("key", 1);

        var snapshot = map.Get("key").ToList();
        map.Add("key", 2);

        Assert.That(snapshot.Count, Is.EqualTo(1));
        Assert.That(map.Get("key").Count(), Is.EqualTo(2));
    }

    // ── Complex key/value scenarios ────────────────────────

    [Test]
    public void MultiMap_WithComplexStringKeys_Works()
    {
        var map = new MultiMapSet<string, int>();
        var complexKey1 = "key with spaces";
        var complexKey2 = "key@#$%with&special()chars";

        map.Add(complexKey1, 1);
        map.Add(complexKey2, 2);

        Assert.That(map.Get(complexKey1), Is.EquivalentTo(new[] { 1 }));
        Assert.That(map.Get(complexKey2), Is.EquivalentTo(new[] { 2 }));
    }

    [Test]
    public void MultiMap_WithLargeStringKeys_Works()
    {
        var map = new MultiMapSet<string, int>();
        var largeKey = new string('x', 10000);

        map.Add(largeKey, 1);

        Assert.That(map.Contains(largeKey, 1), Is.True);
    }

    // ── Boundary condition tests ──────────────────────────

    [Test]
    public void MultiMap_KeyCount_BoundaryAtZero()
    {
        var map = new MultiMapSet<string, int>();

        Assert.That(map.KeyCount, Is.EqualTo(0));

        map.Add("key", 1);
        Assert.That(map.KeyCount, Is.EqualTo(1));

        map.RemoveKey("key");
        Assert.That(map.KeyCount, Is.EqualTo(0));
    }

    [Test]
    public void MultiMap_Count_BoundaryAtZero()
    {
        var map = new MultiMapSet<string, int>();

        Assert.That(map.Count, Is.EqualTo(0));

        map.Add("key", 1);
        Assert.That(map.Count, Is.EqualTo(1));

        map.Remove("key", 1);
        Assert.That(map.Count, Is.EqualTo(0));
    }

    [Test]
    public void MultiMap_AddRange_BoundaryWithSingleItem()
    {
        var map = new MultiMapSet<string, int>();

        int added = map.AddRange("key", new[] { 42 });

        Assert.That(added, Is.EqualTo(1));
        Assert.That(map.Count, Is.EqualTo(1));
    }

    // ── Enumeration scenarios ──────────────────────────────

    [Test]
    public void MultiMap_Keys_NoModificationDuringEnumeration()
    {
        var map = new MultiMapSet<string, int>();
        map.Add("a", 1);
        map.Add("b", 2);
        map.Add("c", 3);

        var keys = map.Keys.ToList();

        Assert.That(keys.Count, Is.EqualTo(3));
        Assert.That(keys, Contains.Item("a"));
        Assert.That(keys, Contains.Item("b"));
        Assert.That(keys, Contains.Item("c"));
    }

    [Test]
    public void MultiMap_Values_NoModificationDuringEnumeration()
    {
        var map = new MultiMapSet<string, int>();
        map.Add("a", 1);
        map.Add("a", 2);
        map.Add("b", 3);

        var values = map.Values.ToList();

        Assert.That(values.Count, Is.EqualTo(3));
    }

    // ── Multiple operations sequence ───────────────────────

    [Test]
    public void MultiMap_ComplexSequenceOfOperations_MaintainsConsistency()
    {
        var map = new MultiMapSet<string, int>();

        // Add phase
        map.AddRange("a", new[] { 1, 2, 3 });
        map.AddRange("b", new[] { 4, 5 });
        Assert.That(map.Count, Is.EqualTo(5));

        // Mixed phase
        map.Add("a", 6);
        map.Remove("b", 4);
        Assert.That(map.Count, Is.EqualTo(5));

        // Remove phase
        map.RemoveKey("a");
        Assert.That(map.Count, Is.EqualTo(1));
        Assert.That(map.KeyCount, Is.EqualTo(1));

        // Final state
        map.Clear();
        Assert.That(map.Count, Is.EqualTo(0));
        Assert.That(map.KeyCount, Is.EqualTo(0));
    }

    // ── Predicate-based operations ─────────────────────────

    [Test]
    public void MultiMap_RemoveWhere_AllConditions()
    {
        var map = new MultiMapSet<string, int>();
        map.AddRange("key", new[] { 1, 2, 3, 4, 5 });

        int removed = map.RemoveWhere("key", v => v % 2 == 0);

        Assert.That(removed, Is.EqualTo(2)); // 2, 4
        Assert.That(map.Get("key"), Is.EquivalentTo(new[] { 1, 3, 5 }));
    }

    [Test]
    public void MultiMap_RemoveWhere_NoConditionsMatch()
    {
        var map = new MultiMapSet<string, int>();
        map.AddRange("key", new[] { 1, 3, 5 });

        int removed = map.RemoveWhere("key", v => v > 100);

        Assert.That(removed, Is.EqualTo(0));
        Assert.That(map.Count, Is.EqualTo(3));
    }

    [Test]
    public void MultiMap_RemoveWhere_AllConditionsMatch()
    {
        var map = new MultiMapSet<string, int>();
        map.AddRange("key", new[] { 1, 2, 3 });

        int removed = map.RemoveWhere("key", v => v > 0);

        Assert.That(removed, Is.EqualTo(3));
        Assert.That(map.ContainsKey("key"), Is.False);
    }
}
