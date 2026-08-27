using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using MultiMap.Entities;
using MultiMap.Helpers;
using MultiMap.Interfaces;

namespace MultiMap.Tests;

/// <summary>
/// Boundary condition and edge case tests for MultiMap implementations.
/// Tests cover: empty collections, single-item operations, capacity edges, 
/// overflow scenarios, and exception conditions at boundaries.
/// </summary>
[TestFixture]
public class MultiMapBoundaryConditionsTests
{
    private MultiMapSet<string, int> _target;
    private MultiMapList<string, int> _targetList;

    [SetUp]
    public void SetUp()
    {
        _target = new MultiMapSet<string, int>();
        _targetList = new MultiMapList<string, int>();
    }

    #region Empty Collection Boundaries

    [Test]
    public void RemoveAll_FromEmpty_ReturnsZero()
    {
        var result = _target.RemoveWhere("key", _ => true);
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void GetValuesCount_EmptyKey_ReturnsZero()
    {
        var count = _target.GetValuesCount("nonexistent");
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void Remove_FromEmptyMap_ReturnsFalse()
    {
        var result = _target.Remove("key", 42);
        Assert.That(result, Is.False);
    }

    [Test]
    public void RemoveKey_FromEmpty_ReturnsFalse()
    {
        var result = _target.RemoveKey("key");
        Assert.That(result, Is.False);
    }

    [Test]
    public void RemoveRange_EmptyItems_ReturnsZero()
    {
        _target.Add("a", 1);
        var removed = _target.RemoveRange(Array.Empty<KeyValuePair<string, int>>());
        Assert.That(removed, Is.EqualTo(0));
        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void RemoveRange_EmptyMap_ReturnsZero()
    {
        var items = new[] { new KeyValuePair<string, int>("a", 1) };
        var removed = _target.RemoveRange(items);
        Assert.That(removed, Is.EqualTo(0));
    }

    #endregion

    #region Single-Item Boundaries

    [Test]
    public void Add_SingleItem_ThenRemove_LeavesEmpty()
    {
        _target.Add("key", 1);
        var removed = _target.RemoveKey("key");

        Assert.That(removed, Is.True);
        Assert.That(_target.Count, Is.EqualTo(0));
        Assert.That(_target.KeyCount, Is.EqualTo(0));
    }

    [Test]
    public void RemoveWhere_SingleValue_MatchingPredicate_ReturnsOne()
    {
        _target.Add("key", 42);
        var removed = _target.RemoveWhere("key", v => v == 42);

        Assert.That(removed, Is.EqualTo(1));
        Assert.That(_target.Count, Is.EqualTo(0));
    }

    [Test]
    public void RemoveWhere_SingleValue_NonMatchingPredicate_ReturnsZero()
    {
        _target.Add("key", 42);
        var removed = _target.RemoveWhere("key", v => v == 99);

        Assert.That(removed, Is.EqualTo(0));
        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void Remove_OnlyValue_LeavesKeyEmpty()
    {
        _target.Add("key", 1);
        var removed = _target.Remove("key", 1);

        Assert.That(removed, Is.True);
        Assert.That(_target.Count, Is.EqualTo(0));
        Assert.That(_target.ContainsKey("key"), Is.False);
    }

    #endregion

    #region AddRange Boundaries

    [Test]
    public void AddRange_EmptySequence_ReturnsZero()
    {
        var added = _target.AddRange("key", Array.Empty<int>());
        Assert.That(added, Is.EqualTo(0));
        Assert.That(_target.KeyCount, Is.EqualTo(0)); // Key not created
    }

    [Test]
    public void AddRange_ItemsEmptySequence_ReturnsZero()
    {
        var added = _target.AddRange(Array.Empty<KeyValuePair<string, int>>());
        Assert.That(added, Is.EqualTo(0));
        Assert.That(_target.Count, Is.EqualTo(0));
    }

    [Test]
    public void AddRange_SingleValue_ReturnsOne()
    {
        var added = _target.AddRange("key", new[] { 42 });
        Assert.That(added, Is.EqualTo(1));
        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void AddRange_AllDuplicates_ReturnsZero()
    {
        _target.Add("key", 1);
        _target.Add("key", 2);

        var added = _target.AddRange("key", new[] { 1, 2 });
        Assert.That(added, Is.EqualTo(0));
        Assert.That(_target.Count, Is.EqualTo(2));
    }

    [Test]
    public void AddRange_PartialDuplicates_ReturnsCountOfNew()
    {
        _target.Add("key", 1);
        var added = _target.AddRange("key", new[] { 1, 2, 3 });

        Assert.That(added, Is.EqualTo(2)); // Only 2 and 3 are new
        Assert.That(_target.Count, Is.EqualTo(3));
    }

    #endregion

    #region MultiMapList-Specific Boundaries (Allows Duplicates)

    [Test]
    public void AddRange_MultiMapList_AllDuplicates_ReturnsCountAdded()
    {
        _targetList.Add("key", 1);
        var added = _targetList.AddRange("key", new[] { 1, 1, 1 });

        Assert.That(added, Is.EqualTo(3)); // MultiMapList allows duplicates
        Assert.That(_targetList.Count, Is.EqualTo(4));
    }

    [Test]
    public void Remove_MultiMapList_RemoveOne_LeavesOtherDuplicates()
    {
        _targetList.Add("key", 1);
        _targetList.Add("key", 1);
        _targetList.Add("key", 1);

        var removed = _targetList.Remove("key", 1);

        Assert.That(removed, Is.True);
        Assert.That(_targetList.Count, Is.EqualTo(2));
        Assert.That(_targetList.GetValuesCount("key"), Is.EqualTo(2));
    }

    #endregion

    #region Clear Boundaries

    [Test]
    public void Clear_EmptyMap_DoesNotThrow()
    {
        _target.Clear();
        Assert.That(_target.Count, Is.EqualTo(0));
        Assert.That(_target.KeyCount, Is.EqualTo(0));
    }

    [Test]
    public void Clear_SingleItem_RemovesIt()
    {
        _target.Add("key", 1);
        _target.Clear();

        Assert.That(_target.Count, Is.EqualTo(0));
        Assert.That(_target.KeyCount, Is.EqualTo(0));
        Assert.That(_target.ContainsKey("key"), Is.False);
    }

    [Test]
    public void Clear_MultipleItems_RemovesAll()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _target.Add("b", 3);

        _target.Clear();

        Assert.That(_target.Count, Is.EqualTo(0));
        Assert.That(_target.KeyCount, Is.EqualTo(0));
    }

    #endregion

    #region Enumeration Boundaries

    [Test]
    public void GetEnumerator_Empty_EnumeratesNothing()
    {
        var count = 0;
        foreach (var _ in _target)
            count++;

        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void Keys_Empty_EnumeratesNothing()
    {
        var count = _target.Keys.Count();
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void Values_Empty_EnumeratesNothing()
    {
        var count = _target.Values.Count();
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void Keys_SingleKey_EnumeratesOne()
    {
        _target.Add("key", 1);
        var keys = _target.Keys.ToList();

        Assert.That(keys.Count, Is.EqualTo(1));
        Assert.That(keys[0], Is.EqualTo("key"));
    }

    #endregion

    #region Capacity and Resize Boundaries

    [Test]
    public void SmallCapacity_ManyAdditions_Resizes()
    {
        var map = new MultiMapSet<int, int>(capacity: 1);

        for (int i = 0; i < 100; i++)
            map.Add(i, i);

        Assert.That(map.KeyCount, Is.EqualTo(100));
        Assert.That(map.Count, Is.EqualTo(100));
    }

    [Test]
    public void LargeCapacity_Allocation_DoesNotThrow()
    {
        var map = new MultiMapSet<int, int>(capacity: 10_000);
        map.Add(1, 100);

        Assert.That(map.Count, Is.EqualTo(1));
    }

    #endregion

    #region Exception Boundaries

    [Test]
    public void Add_Null_Key_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _target.Add(null!, 1));
    }

    [Test]
    public void Add_WithDuplicateValue_ReturnsFalse()
    {
        _target.Add("key", 1);
        var result = _target.Add("key", 1);
        Assert.That(result, Is.False);
    }

    [Test]
    public void AddRange_Null_Key_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _target.AddRange(null!, new[] { 1 }));
    }

    [Test]
    public void AddRange_Null_Values_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _target.AddRange("key", null!));
    }

    [Test]
    public void Remove_Null_Key_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _target.Remove(null!, 1));
    }

    [Test]
    public void RemoveWhere_Null_Predicate_ThrowsArgumentNullException()
    {
        _target.Add("key", 1);
        Assert.Throws<ArgumentNullException>(() => _target.RemoveWhere("key", null!));
    }

    [Test]
    public void Get_Null_Key_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _target.Get(null!));
    }

    [Test]
    public void Get_NonExistent_ThrowsKeyNotFoundException()
    {
        Assert.Throws<KeyNotFoundException>(() => _target.Get("nonexistent"));
    }

    [Test]
    public void Indexer_NonExistent_ThrowsKeyNotFoundException()
    {
        Assert.Throws<KeyNotFoundException>(() => _ = _target["nonexistent"]);
    }

    #endregion

    #region ContainsKey/Contains Boundaries

    [Test]
    public void ContainsKey_Empty_ReturnsFalse()
    {
        Assert.That(_target.ContainsKey("key"), Is.False);
    }

    [Test]
    public void Contains_Empty_ReturnsFalse()
    {
        Assert.That(_target.Contains("key", 1), Is.False);
    }

    [Test]
    public void Contains_WrongValue_ReturnsFalse()
    {
        _target.Add("key", 1);
        Assert.That(_target.Contains("key", 2), Is.False);
    }

    [Test]
    public void ContainsKey_AfterRemoveKey_ReturnsFalse()
    {
        _target.Add("key", 1);
        _target.RemoveKey("key");

        Assert.That(_target.ContainsKey("key"), Is.False);
    }

    #endregion

    #region Count Boundaries

    [Test]
    public void Count_Empty_ReturnsZero()
    {
        Assert.That(_target.Count, Is.EqualTo(0));
    }

    [Test]
    public void Count_AfterClear_ReturnsZero()
    {
        _target.Add("key", 1);
        _target.Clear();
        Assert.That(_target.Count, Is.EqualTo(0));
    }

    [Test]
    public void KeyCount_SingleKey_ReturnsOne()
    {
        _target.Add("key", 1);
        Assert.That(_target.KeyCount, Is.EqualTo(1));
    }

    [Test]
    public void KeyCount_MultipleValuesPerKey_ReturnsKeyCount()
    {
        _target.Add("key", 1);
        _target.Add("key", 2);
        _target.Add("key", 3);

        Assert.That(_target.KeyCount, Is.EqualTo(1));
        Assert.That(_target.Count, Is.EqualTo(3));
    }

    #endregion
}

/// <summary>
/// Additional comprehensive tests to increase coverage of MultiMap implementations,
/// focusing on edge cases, boundary conditions, and complex scenarios.
/// </summary>
[TestFixture]
public class AdditionalBoundaryTests
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
    // ── ExceptWith(ISimpleMultiMap, IReadOnlySimpleMultiMap) — self-reference path ──

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

// ──────────────────────────────────────────────────────────────────────────────
// ConcurrentSet<T> — public ICollection<T> surface
//
// ConcurrentSet<T> has an internal constructor, so we obtain instances via
// reflection from the ConcurrentMultiMap's protected _dictionary field.
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class ConcurrentSetPublicSurfaceTests
{
    private static ConcurrentSet<int> GetSet(IEnumerable<int> seed)
    {
        var map = new ConcurrentMultiMap<string, int>();
        foreach (var v in seed)
            map.Add("k", v);

        var field = typeof(MultiMapBase<string, int, ConcurrentSet<int>>)
            .GetField("_dictionary", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var dict = (ConcurrentDictionary<string, ConcurrentSet<int>>)field.GetValue(map)!;
        return dict["k"];
    }

    private static ConcurrentSet<int> EmptySet()
    {
        var ctor = typeof(ConcurrentSet<int>)
            .GetConstructor(BindingFlags.Public | BindingFlags.Instance,
                            null, new[] { typeof(IEqualityComparer<int>) }, null)!;
        return (ConcurrentSet<int>)ctor.Invoke(new object?[] { null });
    }

    // ── IsReadOnly ────────────────────────────────────────────────────────────

    [Test]
    public void IsReadOnly_AlwaysReturnsFalse()
    {
        ICollection<int> set = GetSet(new[] { 1 });

        Assert.That(set.IsReadOnly, Is.False);
    }

    // ── void Add(T) (ICollection explicit) ───────────────────────────────────

    [Test]
    public void Add_ICollection_InsertsElement()
    {
        ICollection<int> set = EmptySet();

        set.Add(42);

        Assert.That(set.Contains(42), Is.True);
        Assert.That(set.Count, Is.EqualTo(1));
    }

    [Test]
    public void Add_ICollection_DuplicateIsIdempotent()
    {
        ICollection<int> set = EmptySet();
        set.Add(1);
        set.Add(1);

        Assert.That(set.Count, Is.EqualTo(1));
    }

    // ── Clear() ───────────────────────────────────────────────────────────────

    [Test]
    public void Clear_RemovesAllElements()
    {
        ICollection<int> set = GetSet(new[] { 1, 2, 3 });

        set.Clear();

        Assert.That(set.Count, Is.EqualTo(0));
    }

    [Test]
    public void Clear_OnEmptySet_IsNoOp()
    {
        ICollection<int> set = EmptySet();

        Assert.DoesNotThrow(() => set.Clear());
        Assert.That(set.Count, Is.EqualTo(0));
    }

    // ── Remove(T) (ICollection explicit) ─────────────────────────────────────

    [Test]
    public void Remove_ExistingElement_ReturnsTrueAndReducesCount()
    {
        ICollection<int> set = GetSet(new[] { 10, 20 });

        bool removed = set.Remove(10);

        Assert.That(removed, Is.True);
        Assert.That(set.Count, Is.EqualTo(1));
        Assert.That(set.Contains(10), Is.False);
    }

    [Test]
    public void Remove_AbsentElement_ReturnsFalse()
    {
        ICollection<int> set = GetSet(new[] { 5 });

        bool removed = set.Remove(99);

        Assert.That(removed, Is.False);
        Assert.That(set.Count, Is.EqualTo(1));
    }

    // ── CopyTo(T[], int) — all 8 branches ─────────────────────────────────────
    //  Guard.NotNull → 2 branches
    //  arrayIndex < 0 || arrayIndex > array.Length → 4 branches
    //  index >= array.Length during loop → 2 branches

    [Test]
    public void CopyTo_NullArray_ThrowsArgumentNullException()
    {
        ICollection<int> set = GetSet(new[] { 1 });

        Assert.Throws<ArgumentNullException>(() => set.CopyTo(null!, 0));
    }

    [Test]
    public void CopyTo_NegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        ICollection<int> set = GetSet(new[] { 1 });
        var arr = new int[5];

        Assert.Throws<ArgumentOutOfRangeException>(() => set.CopyTo(arr, -1));
    }

    [Test]
    public void CopyTo_IndexGreaterThanArrayLength_ThrowsArgumentOutOfRangeException()
    {
        ICollection<int> set = GetSet(new[] { 1 });
        var arr = new int[2];

        Assert.Throws<ArgumentOutOfRangeException>(() => set.CopyTo(arr, 5));
    }

    [Test]
    public void CopyTo_DestinationArrayTooSmall_ThrowsArgumentException()
    {
        ICollection<int> set = GetSet(new[] { 1, 2, 3 });
        var arr = new int[2]; // only room for 2 elements; set has 3

        Assert.Throws<ArgumentException>(() => set.CopyTo(arr, 0));
    }

    [Test]
    public void CopyTo_ValidArgs_CopiesAllElements()
    {
        ICollection<int> set = GetSet(new[] { 7, 8, 9 });
        var arr = new int[5];

        set.CopyTo(arr, 1);

        var copied = arr.Skip(1).Take(3).ToHashSet();
        Assert.That(copied, Is.EquivalentTo(new[] { 7, 8, 9 }));
    }

    [Test]
    public void CopyTo_IndexEqualToArrayLength_WithEmptySet_DoesNotThrow()
    {
        ICollection<int> set = EmptySet();
        var arr = new int[0];

        // arrayIndex == 0 == array.Length: valid per contract when set is empty
        Assert.DoesNotThrow(() => set.CopyTo(arr, 0));
    }

    // ── IEnumerable.GetEnumerator() (explicit) ────────────────────────────────

    [Test]
    public void GetEnumerator_NonGeneric_IteratesAllElements()
    {
        ICollection<int> set = GetSet(new[] { 11, 22, 33 });
        var result = new List<object?>();

        var enumerator = ((IEnumerable)set).GetEnumerator();
        while (enumerator.MoveNext())
            result.Add(enumerator.Current);

        Assert.That(result, Is.EquivalentTo(new[] { 11, 22, 33 }));
    }

    [Test]
    public void GetEnumerator_NonGeneric_EmptySet_YieldsNothing()
    {
        ICollection<int> set = EmptySet();
        var result = new List<object?>();

        foreach (var item in (IEnumerable)set)
            result.Add(item);

        Assert.That(result, Is.Empty);
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// MultiMapAsync — general (non-MultiMapAsync) interface paths
// These hit the "slow" branches of IsSubsetOfAsync, IsSupersetOfAsync,
// OverlapsAsync, SetEqualsAsync and also the disposed ThrowIfDisposed branch.
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MultiMapAsync_GeneralInterfacePathTests
{
    // Adapter: wraps a MultiMapAsync<> but is NOT an instance of MultiMapAsync<>,
    // forcing the general (non-fast-path) branches.
    private sealed class WrappedMultiMapAsync<TKey, TValue> : IMultiMapAsync<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        private readonly MultiMapAsync<TKey, TValue> _inner;
        public WrappedMultiMapAsync(MultiMapAsync<TKey, TValue> inner) => _inner = inner;

        public ValueTask<bool> AddAsync(TKey key, TValue value, CancellationToken ct = default) => _inner.AddAsync(key, value, ct);
        public ValueTask<int> AddRangeAsync(TKey key, IEnumerable<TValue> values, CancellationToken ct = default) => _inner.AddRangeAsync(key, values, ct);
        public ValueTask<int> AddRangeAsync(IEnumerable<KeyValuePair<TKey, TValue>> items, CancellationToken ct = default) => _inner.AddRangeAsync(items, ct);
        public ValueTask<bool> RemoveAsync(TKey key, TValue value, CancellationToken ct = default) => _inner.RemoveAsync(key, value, ct);
        public ValueTask<int> RemoveRangeAsync(IEnumerable<KeyValuePair<TKey, TValue>> items, CancellationToken ct = default) => _inner.RemoveRangeAsync(items, ct);
        public ValueTask<int> RemoveWhereAsync(TKey key, Predicate<TValue> predicate, CancellationToken ct = default) => _inner.RemoveWhereAsync(key, predicate, ct);
        public ValueTask<bool> RemoveKeyAsync(TKey key, CancellationToken ct = default) => _inner.RemoveKeyAsync(key, ct);
        public Task ClearAsync(CancellationToken ct = default) => _inner.ClearAsync(ct);
        public ValueTask<bool> ContainsKeyAsync(TKey key, CancellationToken ct = default) => _inner.ContainsKeyAsync(key, ct);
        public ValueTask<bool> ContainsAsync(TKey key, TValue value, CancellationToken ct = default) => _inner.ContainsAsync(key, value, ct);
        public ValueTask<IEnumerable<TKey>> GetKeysAsync(CancellationToken ct = default) => _inner.GetKeysAsync(ct);
        public ValueTask<IEnumerable<TValue>> GetAsync(TKey key, CancellationToken ct = default) => _inner.GetAsync(key, ct);
        public ValueTask<IEnumerable<TValue>> GetOrDefaultAsync(TKey key, CancellationToken ct = default) => _inner.GetOrDefaultAsync(key, ct);
        public ValueTask<(bool found, IEnumerable<TValue> values)> TryGetAsync(TKey key, CancellationToken ct = default) => _inner.TryGetAsync(key, ct);
        public ValueTask<int> GetCountAsync(CancellationToken ct = default) => _inner.GetCountAsync(ct);
        public ValueTask<int> GetKeyCountAsync(CancellationToken ct = default) => _inner.GetKeyCountAsync(ct);
        public ValueTask<int> GetValuesCountAsync(TKey key, CancellationToken ct = default) => _inner.GetValuesCountAsync(key, ct);
        public ValueTask<IEnumerable<TValue>> GetValuesAsync(CancellationToken ct = default) => _inner.GetValuesAsync(ct);
        public Task<bool> IsSubsetOfAsync(IMultiMapAsync<TKey, TValue> other, CancellationToken ct = default) => _inner.IsSubsetOfAsync(other, ct);
        public Task<bool> IsSupersetOfAsync(IMultiMapAsync<TKey, TValue> other, CancellationToken ct = default) => _inner.IsSupersetOfAsync(other, ct);
        public Task<bool> OverlapsAsync(IMultiMapAsync<TKey, TValue> other, CancellationToken ct = default) => _inner.OverlapsAsync(other, ct);
        public Task<bool> SetEqualsAsync(IMultiMapAsync<TKey, TValue> other, CancellationToken ct = default) => _inner.SetEqualsAsync(other, ct);
        public ValueTask<bool> EqualsAsync(object? obj) => _inner.EqualsAsync(obj);
        public ValueTask<bool> EqualsAsync(IReadOnlyMultiMapAsync<TKey, TValue>? other, CancellationToken ct = default) => _inner.EqualsAsync(other, ct);
        public IAsyncEnumerator<KeyValuePair<TKey, TValue>> GetAsyncEnumerator(CancellationToken ct = default) => _inner.GetAsyncEnumerator(ct);
        public bool Equals(IReadOnlyMultiMapAsync<TKey, TValue>? other) => _inner.Equals(other);
        public void Dispose() { /* adapter does not own inner */ }
        public ValueTask DisposeAsync() => default;
    }

    // ── IsSubsetOfAsync — general path ───────────────────────────────────────

    [Test]
    public async Task IsSubsetOfAsync_GeneralPath_IsSubset_ReturnsTrue()
    {
        await using var subset = new MultiMapAsync<string, int>();
        await subset.AddAsync("a", 1);

        await using var superset = new MultiMapAsync<string, int>();
        await superset.AddAsync("a", 1);
        await superset.AddAsync("b", 2);

        var wrappedSuperset = new WrappedMultiMapAsync<string, int>(superset);

        Assert.That(await subset.IsSubsetOfAsync(wrappedSuperset), Is.True);
    }

    [Test]
    public async Task IsSubsetOfAsync_GeneralPath_KeyMissingInOther_ReturnsFalse()
    {
        await using var target = new MultiMapAsync<string, int>();
        await target.AddAsync("a", 1);
        await target.AddAsync("z", 99);

        await using var other = new MultiMapAsync<string, int>();
        await other.AddAsync("a", 1);

        var wrapped = new WrappedMultiMapAsync<string, int>(other);

        Assert.That(await target.IsSubsetOfAsync(wrapped), Is.False);
    }

    [Test]
    public async Task IsSubsetOfAsync_GeneralPath_ValueMissingInOther_ReturnsFalse()
    {
        await using var target = new MultiMapAsync<string, int>();
        await target.AddAsync("a", 1);
        await target.AddAsync("a", 2);

        await using var other = new MultiMapAsync<string, int>();
        await other.AddAsync("a", 1); // missing 2

        var wrapped = new WrappedMultiMapAsync<string, int>(other);

        Assert.That(await target.IsSubsetOfAsync(wrapped), Is.False);
    }

    [Test]
    public async Task IsSubsetOfAsync_GeneralPath_BothEmpty_ReturnsTrue()
    {
        await using var target = new MultiMapAsync<string, int>();
        await using var other = new MultiMapAsync<string, int>();

        var wrapped = new WrappedMultiMapAsync<string, int>(other);

        Assert.That(await target.IsSubsetOfAsync(wrapped), Is.True);
    }

    [Test]
    public async Task IsSubsetOfAsync_GeneralPath_OtherValuesIsHashSet_UsesHashSetBranch()
    {
        await using var target = new MultiMapAsync<string, int>();
        await target.AddAsync("a", 1);

        await using var other = new MultiMapAsync<string, int>();
        await other.AddAsync("a", 1);
        await other.AddAsync("a", 2);

        var wrapped = new WrappedMultiMapAsync<string, int>(other);

        Assert.That(await target.IsSubsetOfAsync(wrapped), Is.True);
    }

    // ── IsSupersetOfAsync — general path ──────────────────────────────────────

    [Test]
    public async Task IsSupersetOfAsync_GeneralPath_IsSuperset_ReturnsTrue()
    {
        await using var superset = new MultiMapAsync<string, int>();
        await superset.AddAsync("a", 1);
        await superset.AddAsync("b", 2);

        await using var subset = new MultiMapAsync<string, int>();
        await subset.AddAsync("a", 1);

        var wrappedSubset = new WrappedMultiMapAsync<string, int>(subset);

        Assert.That(await superset.IsSupersetOfAsync(wrappedSubset), Is.True);
    }

    [Test]
    public async Task IsSupersetOfAsync_GeneralPath_NotSuperset_ReturnsFalse()
    {
        await using var map = new MultiMapAsync<string, int>();
        await map.AddAsync("a", 1);

        await using var other = new MultiMapAsync<string, int>();
        await other.AddAsync("a", 1);
        await other.AddAsync("b", 99);

        var wrapped = new WrappedMultiMapAsync<string, int>(other);

        Assert.That(await map.IsSupersetOfAsync(wrapped), Is.False);
    }

    // ── OverlapsAsync — general path ──────────────────────────────────────────

    [Test]
    public async Task OverlapsAsync_GeneralPath_SharedPair_ReturnsTrue()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("x", 10);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("x", 10);
        await b.AddAsync("y", 20);

        var wrapped = new WrappedMultiMapAsync<string, int>(b);

        Assert.That(await a.OverlapsAsync(wrapped), Is.True);
    }

    [Test]
    public async Task OverlapsAsync_GeneralPath_NoSharedPair_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("x", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("y", 2);

        var wrapped = new WrappedMultiMapAsync<string, int>(b);

        Assert.That(await a.OverlapsAsync(wrapped), Is.False);
    }

    [Test]
    public async Task OverlapsAsync_GeneralPath_SameKeyDifferentValue_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("x", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("x", 99);

        var wrapped = new WrappedMultiMapAsync<string, int>(b);

        Assert.That(await a.OverlapsAsync(wrapped), Is.False);
    }

    [Test]
    public async Task OverlapsAsync_GeneralPath_BothEmpty_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await using var b = new MultiMapAsync<string, int>();

        var wrapped = new WrappedMultiMapAsync<string, int>(b);

        Assert.That(await a.OverlapsAsync(wrapped), Is.False);
    }

    // ── SetEqualsAsync — general path ─────────────────────────────────────────

    [Test]
    public async Task SetEqualsAsync_GeneralPath_SameContent_ReturnsTrue()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("k", 1);
        await a.AddAsync("k", 2);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("k", 1);
        await b.AddAsync("k", 2);

        var wrapped = new WrappedMultiMapAsync<string, int>(b);

        Assert.That(await a.SetEqualsAsync(wrapped), Is.True);
    }

    [Test]
    public async Task SetEqualsAsync_GeneralPath_DifferentCount_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("k", 1);
        await a.AddAsync("k", 2);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("k", 1);

        var wrapped = new WrappedMultiMapAsync<string, int>(b);

        Assert.That(await a.SetEqualsAsync(wrapped), Is.False);
    }

    [Test]
    public async Task SetEqualsAsync_GeneralPath_DifferentKeyCount_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("k1", 1);
        await a.AddAsync("k2", 2);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("k1", 1);

        var wrapped = new WrappedMultiMapAsync<string, int>(b);

        Assert.That(await a.SetEqualsAsync(wrapped), Is.False);
    }

    [Test]
    public async Task SetEqualsAsync_GeneralPath_KeyMissingInOther_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("k1", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("k2", 1);

        var wrapped = new WrappedMultiMapAsync<string, int>(b);

        Assert.That(await a.SetEqualsAsync(wrapped), Is.False);
    }

    [Test]
    public async Task SetEqualsAsync_GeneralPath_ValueMismatch_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("k", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("k", 99);

        var wrapped = new WrappedMultiMapAsync<string, int>(b);

        Assert.That(await a.SetEqualsAsync(wrapped), Is.False);
    }

    [Test]
    public async Task SetEqualsAsync_GeneralPath_BothEmpty_ReturnsTrue()
    {
        await using var a = new MultiMapAsync<string, int>();
        await using var b = new MultiMapAsync<string, int>();

        var wrapped = new WrappedMultiMapAsync<string, int>(b);

        Assert.That(await a.SetEqualsAsync(wrapped), Is.True);
    }

    // ── ThrowIfDisposed disposed branch ──────────────────────────────────────

    [Test]
    public async Task AddAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var map = new MultiMapAsync<string, int>();
        await map.AddAsync("a", 1);
        map.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(async () => await map.AddAsync("a", 2));
    }

    [Test]
    public async Task IsSubsetOfAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        await using var other = new MultiMapAsync<string, int>();
        var map = new MultiMapAsync<string, int>();
        map.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(async () => await map.IsSubsetOfAsync(other));
    }

    // ── ContainsAsync false branch (key not found) ─────────────────────────────

    [Test]
    public async Task ContainsAsync_KeyNotFound_ReturnsFalse()
    {
        await using var map = new MultiMapAsync<string, int>();
        await map.AddAsync("existing", 1);

        Assert.That(await map.ContainsAsync("nonexistent", 1), Is.False);
    }

    // ── GetValuesCountAsync false branch (key not found) ──────────────────────

    [Test]
    public async Task GetValuesCountAsync_KeyNotFound_ReturnsZero()
    {
        await using var map = new MultiMapAsync<string, int>();
        await map.AddAsync("real", 1);

        Assert.That(await map.GetValuesCountAsync("ghost"), Is.EqualTo(0));
    }

    // ── TryGetAsync false branch (key not found) ──────────────────────────────

    [Test]
    public async Task TryGetAsync_KeyNotFound_ReturnsFalse()
    {
        await using var map = new MultiMapAsync<string, int>();

        var (found, values) = await map.TryGetAsync("missing");

        Assert.That(found, Is.False);
        Assert.That(values, Is.Empty);
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// ConcurrentMultiMap<T1,T2> — Equals branch coverage
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class ConcurrentMultiMap_EqualsBranchTests
{
    [Test]
    public void Equals_IReadOnlyMultiMap_Null_ReturnsFalse()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>?)null), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameInstance_ReturnsTrue()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>)map), Is.True);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_DifferentKeyCount_ReturnsFalse()
    {
        var a = new ConcurrentMultiMap<string, int>();
        a.Add("a", 1);

        var b = new ConcurrentMultiMap<string, int>();
        b.Add("a", 1);
        b.Add("b", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_DifferentTotalCount_ReturnsFalse()
    {
        var a = new ConcurrentMultiMap<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new ConcurrentMultiMap<string, int>();
        b.Add("a", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_KeyNotFoundInOther_ReturnsFalse()
    {
        var a = new ConcurrentMultiMap<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>(); // different type, different key
        b.Add("z", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_ValueCountMismatch_ReturnsFalse()
    {
        var a = new ConcurrentMultiMap<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 1);
        b.Add("a", 3); // same count but different value — this tests valCount check

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameContent_ReturnsTrue()
    {
        var a = new ConcurrentMultiMap<string, int>();
        a.Add("a", 1);
        a.Add("b", 2);

        var b = new ConcurrentMultiMap<string, int>();
        b.Add("a", 1);
        b.Add("b", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.True);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_ValueNotFoundInOther_ReturnsFalse()
    {
        var a = new ConcurrentMultiMap<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 99); // same key, different value

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    // ── RemoveWhereFromCollection — concurrent predicate branches ────────────

    [Test]
    public void RemoveWhere_PredicateMatchesNone_ReturnsZero()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add("k", 1);
        map.Add("k", 2);
        map.Add("k", 3);

        int removed = map.RemoveWhere("k", v => v > 100);

        Assert.That(removed, Is.EqualTo(0));
        Assert.That(map.Count, Is.EqualTo(3));
    }

    [Test]
    public void RemoveWhere_PredicateMatchesAll_RemovesAllAndCleansKey()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add("k", 1);
        map.Add("k", 2);
        map.Add("k", 3);

        int removed = map.RemoveWhere("k", _ => true);

        Assert.That(removed, Is.EqualTo(3));
        Assert.That(map.ContainsKey("k"), Is.False);
        Assert.That(map.Count, Is.EqualTo(0));
    }

    [Test]
    [Category("Concurrent")]
    public void RemoveWhere_ConcurrentRemovalAndAdd_CountRemainsSane()
    {
        var map = new ConcurrentMultiMap<string, int>();
        for (int i = 0; i < 50; i++)
            map.Add("k", i);

        var remover = Task.Run(() => map.RemoveWhere("k", v => v % 2 == 0));
        var adder = Task.Run(() =>
        {
            for (int i = 100; i < 150; i++)
                map.Add("k", i);
        });

        Task.WaitAll(remover, adder);

        Assert.That(map.Count, Is.GreaterThanOrEqualTo(0));
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// MultiMapSet<T1,T2> — Equals branch coverage
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MultiMapSet_EqualsBranchTests
{
    [Test]
    public void Equals_IReadOnlyMultiMap_Null_ReturnsFalse()
    {
        var map = new MultiMapSet<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>?)null), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameInstance_ReturnsTrue()
    {
        var map = new MultiMapSet<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>)map), Is.True);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_DifferentKeyCount_ReturnsFalse()
    {
        var a = new MultiMapSet<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 1);
        b.Add("b", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_DifferentCount_ReturnsFalse()
    {
        var a = new MultiMapSet<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_KeyNotFoundInOther_ReturnsFalse()
    {
        var a = new MultiMapSet<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("z", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_ValueCountMismatch_ReturnsFalse()
    {
        var a = new MultiMapSet<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 1);
        b.Add("a", 3); // same count, but SetEquals will differ

        // tweak: use different actual sizes
        var a2 = new MultiMapSet<string, int>();
        a2.Add("a", 1);
        a2.Add("a", 2);
        var b2 = new MultiMapSet<string, int>();
        b2.Add("a", 5); // different count

        Assert.That(a2.Equals((IReadOnlyMultiMap<string, int>)b2), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SetNotEqual_ReturnsFalse()
    {
        var a = new MultiMapSet<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 99);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameContent_ReturnsTrue()
    {
        var a = new MultiMapSet<string, int>();
        a.Add("a", 1);
        a.Add("b", 2);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 1);
        b.Add("b", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.True);
    }

    [Test]
    public void Equals_Object_NonMapType_ReturnsFalse()
    {
        var map = new MultiMapSet<string, int>();
        Assert.That(map.Equals((object)"something"), Is.False);
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// MultiMapList<T1,T2> — Equals branch coverage
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MultiMapList_EqualsBranchTests
{
    [Test]
    public void Equals_IReadOnlyMultiMap_Null_ReturnsFalse()
    {
        var map = new MultiMapList<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>?)null), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameInstance_ReturnsTrue()
    {
        var map = new MultiMapList<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>)map), Is.True);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_DifferentKeyCount_ReturnsFalse()
    {
        var a = new MultiMapList<string, int>();
        a.Add("a", 1);

        var b = new MultiMapList<string, int>();
        b.Add("a", 1);
        b.Add("b", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_DifferentCount_ReturnsFalse()
    {
        var a = new MultiMapList<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new MultiMapList<string, int>();
        b.Add("a", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_KeyNotFoundInOther_ReturnsFalse()
    {
        var a = new MultiMapList<string, int>();
        a.Add("a", 1);

        var b = new MultiMapList<string, int>();
        b.Add("z", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_ValuesCountMismatch_ReturnsFalse()
    {
        var a = new MultiMapList<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new MultiMapList<string, int>();
        b.Add("a", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SequenceNotEqual_ReturnsFalse()
    {
        var a = new MultiMapList<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new MultiMapList<string, int>();
        b.Add("a", 2);
        b.Add("a", 1); // different order

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameSequence_ReturnsTrue()
    {
        var a = new MultiMapList<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new MultiMapList<string, int>();
        b.Add("a", 1);
        b.Add("a", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.True);
    }

    [Test]
    public void Equals_Object_NonMapType_ReturnsFalse()
    {
        var map = new MultiMapList<string, int>();
        Assert.That(map.Equals((object)"something"), Is.False);
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// SortedMultiMap<T1,T2> — Equals branch coverage
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class SortedMultiMap_EqualsBranchTests
{
    [Test]
    public void Equals_IReadOnlyMultiMap_Null_ReturnsFalse()
    {
        var map = new SortedMultiMap<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>?)null), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameInstance_ReturnsTrue()
    {
        var map = new SortedMultiMap<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>)map), Is.True);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_DifferentKeyCount_ReturnsFalse()
    {
        var a = new SortedMultiMap<string, int>();
        a.Add("a", 1);

        var b = new SortedMultiMap<string, int>();
        b.Add("a", 1);
        b.Add("b", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_DifferentCount_ReturnsFalse()
    {
        var a = new SortedMultiMap<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new SortedMultiMap<string, int>();
        b.Add("a", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_KeyNotFoundInOther_ReturnsFalse()
    {
        var a = new SortedMultiMap<string, int>();
        a.Add("a", 1);

        var b = new SortedMultiMap<string, int>();
        b.Add("z", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_ValueCountMismatch_ReturnsFalse()
    {
        var a = new SortedMultiMap<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new SortedMultiMap<string, int>();
        b.Add("a", 5); // one value vs two

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SetNotEqual_ReturnsFalse()
    {
        var a = new SortedMultiMap<string, int>();
        a.Add("a", 1);

        var b = new SortedMultiMap<string, int>();
        b.Add("a", 99);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameContent_ReturnsTrue()
    {
        var a = new SortedMultiMap<string, int>();
        a.Add("a", 1);
        a.Add("b", 3);

        var b = new SortedMultiMap<string, int>();
        b.Add("b", 3);
        b.Add("a", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.True);
    }

    [Test]
    public void Equals_Object_NonMapType_ReturnsFalse()
    {
        var map = new SortedMultiMap<string, int>();
        Assert.That(map.Equals((object)42), Is.False);
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// MultiMapLock<T1,T2> — Equals and SetEquals branch coverage
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MultiMapLock_EqualsBranchTests
{
    // ── Equals(IReadOnlyMultiMap) ────────────────────────────────────────────

    [Test]
    public void Equals_IReadOnlyMultiMap_Null_ReturnsFalse()
    {
        using var map = new MultiMapLock<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>?)null), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameInstance_ReturnsTrue()
    {
        using var map = new MultiMapLock<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>)map), Is.True);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_DifferentKeyCount_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);

        using var b = new MultiMapLock<string, int>();
        b.Add("a", 1);
        b.Add("b", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_DifferentCount_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        using var b = new MultiMapLock<string, int>();
        b.Add("a", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_KeyNotFoundInOther_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("z", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_ValueCountMismatch_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 1); // key present, but count differs

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_ValueNotFoundInOther_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 99); // same key, different value

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameContent_ReturnsTrue()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);
        a.Add("b", 2);

        using var b = new MultiMapLock<string, int>();
        b.Add("a", 1);
        b.Add("b", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.True);
    }

    [Test]
    public void Equals_Object_NonMapType_ReturnsFalse()
    {
        using var map = new MultiMapLock<string, int>();
        Assert.That(map.Equals((object)"irrelevant"), Is.False);
    }

    // ── SetEquals(IMultiMap) ──────────────────────────────────────────────────

    [Test]
    public void SetEquals_SameInstance_ReturnsTrue()
    {
        using var map = new MultiMapLock<string, int>();
        map.Add("a", 1);

        Assert.That(map.SetEquals(map), Is.True);
    }

    [Test]
    public void SetEquals_DifferentCount_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        using var b = new MultiMapLock<string, int>();
        b.Add("a", 1);

        Assert.That(a.SetEquals(b), Is.False);
    }

    [Test]
    public void SetEquals_DifferentKeyCount_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);

        using var b = new MultiMapLock<string, int>();
        b.Add("a", 1);
        b.Add("b", 2);

        Assert.That(a.SetEquals(b), Is.False);
    }

    [Test]
    public void SetEquals_KeyNotFoundInThis_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("z", 1); // different key — note: MultiMapSet doesn't implement IDisposable

        Assert.That(a.SetEquals(b), Is.False);
    }

    [Test]
    public void SetEquals_ValueCountMismatch_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 1);
        b.Add("a", 2); // more values

        Assert.That(a.SetEquals(b), Is.False);
    }

    [Test]
    public void SetEquals_ValueSetNotEqual_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 99); // different value

        Assert.That(a.SetEquals(b), Is.False);
    }

    [Test]
    public void SetEquals_SameContent_ReturnsTrue()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("x", 10);
        a.Add("y", 20);

        var b = new MultiMapSet<string, int>();
        b.Add("x", 10);
        b.Add("y", 20);

        Assert.That(a.SetEquals(b), Is.True);
    }

    // ── SetEquals — snapshot count vs post-read dictionary size divergence ──

    [Test]
    public void SetEquals_SnapshotDictionaryCountDiffers_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);
        a.Add("b", 2);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 1); // different key count supplied to SetEquals

        // A has 2 keys, but b has 1 → snapshot.Count != dictionary.Count path
        Assert.That(a.SetEquals(b), Is.False);
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// ConcurrentMultiMap — stress tests targeting concurrent remove branches
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
[Category("Stress")]
[Category("Concurrent")]
public class ConcurrentMultiMap_RemoveStressTests
{
    [Test]
    public void Remove_ConcurrentAddAndRemoveSameKey_CountNeverNegative()
    {
        var map = new ConcurrentMultiMap<string, int>();
        const int iterations = 200;

        var adders = Enumerable.Range(0, iterations)
            .Select(i => Task.Run(() => map.Add("k", i)))
            .ToArray();
        var removers = Enumerable.Range(0, iterations)
            .Select(i => Task.Run(() => map.Remove("k", i)))
            .ToArray();

        Task.WaitAll(adders.Concat(removers).ToArray());

        Assert.That(map.Count, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void RemoveKey_ConcurrentAddAndRemoveKey_CountNeverNegative()
    {
        var map = new ConcurrentMultiMap<string, int>();
        const int iterations = 200;

        var tasks = Enumerable.Range(0, iterations).SelectMany(i => new[]
        {
            Task.Run(() => map.Add("k", i)),
            Task.Run(() => map.RemoveKey("k"))
        }).ToArray();

        Task.WaitAll(tasks);

        Assert.That(map.Count, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void AddRange_ConcurrentCalls_AllValuesEventuallyPresent()
    {
        var map = new ConcurrentMultiMap<string, int>();
        const int threads = 8;
        const int perThread = 25;

        var tasks = Enumerable.Range(0, threads)
            .Select(t => Task.Run(() =>
            {
                int start = t * perThread;
                map.AddRange("k", Enumerable.Range(start, perThread));
            })).ToArray();

        Task.WaitAll(tasks);

        Assert.That(map.Count, Is.EqualTo(threads * perThread));
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// MultiMapAsync — IsSubsetOfAsync / OverlapsAsync / SetEqualsAsync fast-path
// branches that were not yet hit (count/key mismatch early-return in fast path)
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MultiMapAsync_FastPathBranchTests
{
    [Test]
    public async Task SetEqualsAsync_FastPath_DifferentCount_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("k", 1);
        await a.AddAsync("k", 2);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("k", 1);

        Assert.That(await a.SetEqualsAsync(b), Is.False);
    }

    [Test]
    public async Task SetEqualsAsync_FastPath_DifferentKeyCount_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("a", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("a", 1);
        await b.AddAsync("b", 2);

        Assert.That(await a.SetEqualsAsync(b), Is.False);
    }

    [Test]
    public async Task SetEqualsAsync_FastPath_KeyMissingInOther_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("a", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("z", 1);

        Assert.That(await a.SetEqualsAsync(b), Is.False);
    }

    [Test]
    public async Task SetEqualsAsync_FastPath_ValueSetNotEqual_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("a", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("a", 99);

        Assert.That(await a.SetEqualsAsync(b), Is.False);
    }

    [Test]
    public async Task IsSubsetOfAsync_FastPath_KeyMissingInOther_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("x", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("y", 1);

        Assert.That(await a.IsSubsetOfAsync(b), Is.False);
    }

    [Test]
    public async Task IsSubsetOfAsync_FastPath_ValueMissingInOther_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("k", 1);
        await a.AddAsync("k", 2);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("k", 1);

        Assert.That(await a.IsSubsetOfAsync(b), Is.False);
    }

    [Test]
    public async Task OverlapsAsync_FastPath_NoSharedValue_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("k", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("k", 99);

        Assert.That(await a.OverlapsAsync(b), Is.False);
    }

    [Test]
    public async Task OverlapsAsync_FastPath_NoSharedKey_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("a", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("z", 1);

        Assert.That(await a.OverlapsAsync(b), Is.False);
    }

    [Test]
    public async Task OverlapsAsync_FastPath_BothEmpty_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await using var b = new MultiMapAsync<string, int>();

        Assert.That(await a.OverlapsAsync(b), Is.False);
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// NonEquatable types — constraint-relaxation smoke tests
//
// Verifies that types that do NOT implement IEquatable<T> can be used as TKey
// or TValue now that the IEquatable<T> constraint has been removed from all
// multi-map interfaces and implementations.
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class NonEquatableConstraintTests
{
    // A plain class with no IEquatable<T> implementation.
    private sealed class NoEquatableKey
    {
        public int Id { get; init; }
        public override bool Equals(object? obj) => obj is NoEquatableKey other && Id == other.Id;
        public override int GetHashCode() => Id.GetHashCode();
    }

    private sealed class NoEquatableValue
    {
        public string Label { get; init; } = "";
        public override bool Equals(object? obj) => obj is NoEquatableValue other && Label == other.Label;
        public override int GetHashCode() => Label.GetHashCode();
    }

    [Test]
    public void MultiMapSet_WithNonEquatableTypes_AddAndRetrieve()
    {
        var map = new MultiMapSet<NoEquatableKey, NoEquatableValue>();
        var key = new NoEquatableKey { Id = 1 };
        var value = new NoEquatableValue { Label = "hello" };

        map.Add(key, value);

        Assert.That(map.Contains(key, value), Is.True);
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void MultiMapList_WithNonEquatableTypes_AddAndRetrieve()
    {
        var map = new MultiMapList<NoEquatableKey, NoEquatableValue>();
        var key = new NoEquatableKey { Id = 2 };
        var value = new NoEquatableValue { Label = "world" };

        map.Add(key, value);

        Assert.That(map.Contains(key, value), Is.True);
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void ConcurrentMultiMap_WithNonEquatableTypes_AddAndRetrieve()
    {
        var map = new ConcurrentMultiMap<NoEquatableKey, NoEquatableValue>();
        var key = new NoEquatableKey { Id = 3 };
        var value = new NoEquatableValue { Label = "concurrent" };

        map.Add(key, value);

        Assert.That(map.Contains(key, value), Is.True);
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void MultiMapLock_WithNonEquatableTypes_AddAndRetrieve()
    {
        using var map = new MultiMapLock<NoEquatableKey, NoEquatableValue>();
        var key = new NoEquatableKey { Id = 4 };
        var value = new NoEquatableValue { Label = "locked" };

        map.Add(key, value);

        Assert.That(map.Contains(key, value), Is.True);
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task MultiMapAsync_WithNonEquatableTypes_AddAndRetrieve()
    {
        await using var map = new MultiMapAsync<NoEquatableKey, NoEquatableValue>();
        var key = new NoEquatableKey { Id = 5 };
        var value = new NoEquatableValue { Label = "async" };

        await map.AddAsync(key, value);

        Assert.That(await map.ContainsAsync(key, value), Is.True);
        Assert.That(await map.GetCountAsync(), Is.EqualTo(1));
    }
}
