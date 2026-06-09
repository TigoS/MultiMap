using MultiMap.Entities;

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
