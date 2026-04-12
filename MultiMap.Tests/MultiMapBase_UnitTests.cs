using MultiMap.Entities;
using MultiMap.Interfaces;

namespace MultiMap.Tests;

/// <summary>
/// Provides factory methods and type name for each concrete MultiMapBase subclass.
/// Used by <see cref="MultiMapBaseTests{TMap}"/> via <see cref="TestFixtureSourceAttribute"/>.
/// </summary>
public static class MultiMapBaseTestSources
{
    public static IEnumerable<TestFixtureData> AllTypes()
    {
        yield return new TestFixtureData(typeof(MultiMapSet<string, int>))
            .SetArgDisplayNames("MultiMapSet");

        yield return new TestFixtureData(typeof(MultiMapList<string, int>))
            .SetArgDisplayNames("MultiMapList");

        yield return new TestFixtureData(typeof(SortedMultiMap<string, int>))
            .SetArgDisplayNames("SortedMultiMap");
    }
}

/// <summary>
/// Tests the <see cref="MultiMapBase{TKey,TValue,TCollection}"/> contract through all
/// concrete subclasses: MultiMapSet, MultiMapList, and SortedMultiMap.
/// </summary>
[TestFixtureSource(typeof(MultiMapBaseTestSources), nameof(MultiMapBaseTestSources.AllTypes))]
public class MultiMapBaseTests
{
    private readonly Type _mapType;
    private IMultiMap<string, int> _map = null!;

    public MultiMapBaseTests(Type mapType)
    {
        _mapType = mapType;
    }

    [SetUp]
    public void SetUp()
    {
        _map = (IMultiMap<string, int>)Activator.CreateInstance(_mapType)!;
    }

    // ── Add ───────────────────────────────────────────────────

    [Test]
    public void Add_NewKeyValue_ReturnsTrue()
    {
        Assert.That(_map.Add("a", 1), Is.True);
    }

    [Test]
    public void Add_SingleKeyValue_CanBeRetrieved()
    {
        _map.Add("a", 1);

        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void Add_DifferentKeys_StoresIndependently()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);

        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(_map.GetOrDefault("b"), Is.EquivalentTo(new[] { 2 }));
    }

    [Test]
    public void Add_MultipleValuesForSameKey_ReturnsAllValues()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("a", 3);

        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    // ── AddRange (key, values) ────────────────────────────────

    [Test]
    public void AddRange_NewKey_StoresAllValues()
    {
        _map.AddRange("a", new[] { 1, 2, 3 });

        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void AddRange_ExistingKey_AppendsValues()
    {
        _map.Add("a", 1);
        _map.AddRange("a", new[] { 2, 3 });

        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void AddRange_EmptyCollection_DoesNotChangeState()
    {
        _map.Add("a", 1);
        _map.AddRange("a", Enumerable.Empty<int>());

        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void AddRange_UpdatesCount()
    {
        _map.AddRange("a", new[] { 1, 2 });
        _map.AddRange("b", new[] { 3 });

        Assert.That(_map.Count, Is.EqualTo(3));
    }

    // ── AddRange (KeyValuePairs) ──────────────────────────────

    [Test]
    public void AddRangeKvp_EmptyCollection_DoesNothing()
    {
        _map.AddRange(Enumerable.Empty<KeyValuePair<string, int>>());

        Assert.That(_map.Count, Is.EqualTo(0));
    }

    [Test]
    public void AddRangeKvp_MultiplePairsDifferentKeys_AddsCorrectly()
    {
        var pairs = new[]
        {
            new KeyValuePair<string, int>("a", 1),
            new KeyValuePair<string, int>("b", 2),
            new KeyValuePair<string, int>("c", 3)
        };
        _map.AddRange(pairs);

        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(_map.GetOrDefault("b"), Is.EquivalentTo(new[] { 2 }));
        Assert.That(_map.GetOrDefault("c"), Is.EquivalentTo(new[] { 3 }));
        Assert.That(_map.Count, Is.EqualTo(3));
    }

    [Test]
    public void AddRangeKvp_SameKey_AddsAllValues()
    {
        var pairs = new[]
        {
            new KeyValuePair<string, int>("a", 1),
            new KeyValuePair<string, int>("a", 2),
            new KeyValuePair<string, int>("a", 3)
        };
        _map.AddRange(pairs);

        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    // ── Get ───────────────────────────────────────────────────

    [Test]
    public void Get_ExistingKey_ReturnsValues()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);

        Assert.That(_map.Get("a"), Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public void Get_NonExistentKey_ThrowsKeyNotFoundException()
    {
        Assert.Throws<KeyNotFoundException>(() => _map.Get("missing"));
    }

    [Test]
    public void Get_NonExistentKey_ExceptionContainsKeyName()
    {
        var ex = Assert.Throws<KeyNotFoundException>(() => _map.Get("missing"));

        Assert.That(ex!.Message, Does.Contain("missing"));
    }

    [Test]
    public void Get_ReturnsSnapshot_NotLiveCollection()
    {
        _map.Add("a", 1);
        var result = _map.Get("a");
        _map.Add("a", 2);

        Assert.That(result, Is.EquivalentTo(new[] { 1 }));
    }

    // ── GetOrDefault ──────────────────────────────────────────

    [Test]
    public void GetOrDefault_ExistingKey_ReturnsValues()
    {
        _map.Add("a", 1);

        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void GetOrDefault_NonExistentKey_ReturnsEmpty()
    {
        Assert.That(_map.GetOrDefault("missing"), Is.Empty);
    }

    // ── Indexer ───────────────────────────────────────────────

    [Test]
    public void Indexer_ExistingKey_ReturnsValues()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);

        Assert.That(_map["a"], Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public void Indexer_NonExistentKey_ThrowsKeyNotFoundException()
    {
        Assert.Throws<KeyNotFoundException>(() => { var _ = _map["missing"]; });
    }

    // ── TryGet ────────────────────────────────────────────────

    [Test]
    public void TryGet_ExistingKey_ReturnsTrueWithValues()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);

        bool found = _map.TryGet("a", out var values);

        Assert.That(found, Is.True);
        Assert.That(values, Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public void TryGet_NonExistentKey_ReturnsFalseWithEmpty()
    {
        bool found = _map.TryGet("missing", out var values);

        Assert.That(found, Is.False);
        Assert.That(values, Is.Empty);
    }

    [Test]
    public void TryGet_AfterRemovingLastValue_ReturnsFalseWithEmpty()
    {
        _map.Add("a", 1);
        _map.Remove("a", 1);

        bool found = _map.TryGet("a", out var values);

        Assert.That(found, Is.False);
        Assert.That(values, Is.Empty);
    }

    [Test]
    public void TryGet_AfterClear_ReturnsFalseWithEmpty()
    {
        _map.Add("a", 1);
        _map.Clear();

        bool found = _map.TryGet("a", out var values);

        Assert.That(found, Is.False);
        Assert.That(values, Is.Empty);
    }

    // ── Remove ────────────────────────────────────────────────

    [Test]
    public void Remove_ExistingValue_ReturnsTrue()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);

        Assert.That(_map.Remove("a", 1), Is.True);
        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 2 }));
    }

    [Test]
    public void Remove_LastValueForKey_RemovesKey()
    {
        _map.Add("a", 1);

        _map.Remove("a", 1);

        Assert.That(_map.ContainsKey("a"), Is.False);
    }

    [Test]
    public void Remove_NonExistentValue_ReturnsFalse()
    {
        _map.Add("a", 1);

        Assert.That(_map.Remove("a", 99), Is.False);
    }

    [Test]
    public void Remove_NonExistentKey_ReturnsFalse()
    {
        Assert.That(_map.Remove("missing", 1), Is.False);
    }

    [Test]
    public void Remove_DecreasesCount()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Remove("a", 1);

        Assert.That(_map.Count, Is.EqualTo(1));
    }

    // ── RemoveRange ───────────────────────────────────────────

    [Test]
    public void RemoveRange_EmptyCollection_ReturnsZero()
    {
        _map.Add("a", 1);
        int removed = _map.RemoveRange(Enumerable.Empty<KeyValuePair<string, int>>());

        Assert.That(removed, Is.EqualTo(0));
        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void RemoveRange_SingleExistingPair_ReturnsOne()
    {
        _map.Add("a", 1);
        var pairs = new[] { new KeyValuePair<string, int>("a", 1) };
        int removed = _map.RemoveRange(pairs);

        Assert.That(removed, Is.EqualTo(1));
        Assert.That(_map.Count, Is.EqualTo(0));
    }

    [Test]
    public void RemoveRange_MixedExistingAndNonExisting_RemovesOnlyExisting()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);
        var pairs = new[]
        {
            new KeyValuePair<string, int>("a", 1),
            new KeyValuePair<string, int>("c", 3)
        };
        int removed = _map.RemoveRange(pairs);

        Assert.That(removed, Is.EqualTo(1));
        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void RemoveRange_LastValueOfKey_RemovesKey()
    {
        _map.Add("a", 1);
        _map.RemoveRange(new[] { new KeyValuePair<string, int>("a", 1) });

        Assert.That(_map.ContainsKey("a"), Is.False);
    }

    // ── RemoveWhere ───────────────────────────────────────────

    [Test]
    public void RemoveWhere_NonExistentKey_ReturnsZero()
    {
        int removed = _map.RemoveWhere("missing", v => v > 0);

        Assert.That(removed, Is.EqualTo(0));
    }

    [Test]
    public void RemoveWhere_NoMatchingValues_ReturnsZero()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        int removed = _map.RemoveWhere("a", v => v > 10);

        Assert.That(removed, Is.EqualTo(0));
        Assert.That(_map.Count, Is.EqualTo(2));
    }

    [Test]
    public void RemoveWhere_MatchingValues_RemovesAndReturnsCount()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("a", 3);
        int removed = _map.RemoveWhere("a", v => v > 1);

        Assert.That(removed, Is.EqualTo(2));
        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void RemoveWhere_AllValues_RemovesKey()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        int removed = _map.RemoveWhere("a", _ => true);

        Assert.That(removed, Is.EqualTo(2));
        Assert.That(_map.ContainsKey("a"), Is.False);
        Assert.That(_map.Count, Is.EqualTo(0));
    }

    // ── RemoveKey ─────────────────────────────────────────────

    [Test]
    public void RemoveKey_ExistingKey_ReturnsTrueAndRemovesAll()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);

        Assert.That(_map.RemoveKey("a"), Is.True);
        Assert.That(_map.ContainsKey("a"), Is.False);
        Assert.That(_map.Count, Is.EqualTo(0));
    }

    [Test]
    public void RemoveKey_NonExistentKey_ReturnsFalse()
    {
        Assert.That(_map.RemoveKey("missing"), Is.False);
    }

    [Test]
    public void RemoveKey_DecreasesCount()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("b", 3);

        _map.RemoveKey("a");

        Assert.That(_map.Count, Is.EqualTo(1));
    }

    // ── ContainsKey ───────────────────────────────────────────

    [Test]
    public void ContainsKey_ExistingKey_ReturnsTrue()
    {
        _map.Add("a", 1);

        Assert.That(_map.ContainsKey("a"), Is.True);
    }

    [Test]
    public void ContainsKey_NonExistentKey_ReturnsFalse()
    {
        Assert.That(_map.ContainsKey("missing"), Is.False);
    }

    [Test]
    public void ContainsKey_AfterRemovingLastValue_ReturnsFalse()
    {
        _map.Add("a", 1);
        _map.Remove("a", 1);

        Assert.That(_map.ContainsKey("a"), Is.False);
    }

    // ── Contains ──────────────────────────────────────────────

    [Test]
    public void Contains_ExistingKeyAndValue_ReturnsTrue()
    {
        _map.Add("a", 1);

        Assert.That(_map.Contains("a", 1), Is.True);
    }

    [Test]
    public void Contains_ExistingKeyWrongValue_ReturnsFalse()
    {
        _map.Add("a", 1);

        Assert.That(_map.Contains("a", 99), Is.False);
    }

    [Test]
    public void Contains_NonExistentKey_ReturnsFalse()
    {
        Assert.That(_map.Contains("missing", 1), Is.False);
    }

    [Test]
    public void Contains_AfterRemovingValue_ReturnsFalse()
    {
        _map.Add("a", 1);
        _map.Remove("a", 1);

        Assert.That(_map.Contains("a", 1), Is.False);
    }

    // ── Count ─────────────────────────────────────────────────

    [Test]
    public void Count_EmptyMap_ReturnsZero()
    {
        Assert.That(_map.Count, Is.Zero);
    }

    [Test]
    public void Count_ReflectsTotalKeyValuePairs()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("b", 3);

        Assert.That(_map.Count, Is.EqualTo(3));
    }

    [Test]
    public void Count_UnchangedAfterFailedRemove()
    {
        _map.Add("a", 1);

        _map.Remove("a", 99);
        _map.Remove("missing", 1);
        _map.RemoveKey("missing");

        Assert.That(_map.Count, Is.EqualTo(1));
    }

    // ── Keys ──────────────────────────────────────────────────

    [Test]
    public void Keys_EmptyMap_ReturnsEmpty()
    {
        Assert.That(_map.Keys, Is.Empty);
    }

    [Test]
    public void Keys_MultipleKeys_ReturnsAllKeys()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);
        _map.Add("c", 3);

        Assert.That(_map.Keys, Is.EquivalentTo(new[] { "a", "b", "c" }));
    }

    [Test]
    public void Keys_AfterRemovingLastValueForKey_DoesNotContainKey()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);
        _map.Remove("a", 1);

        Assert.That(_map.Keys, Is.EquivalentTo(new[] { "b" }));
    }

    // ── KeyCount ──────────────────────────────────────────────

    [Test]
    public void KeyCount_EmptyMap_ReturnsZero()
    {
        Assert.That(_map.KeyCount, Is.EqualTo(0));
    }

    [Test]
    public void KeyCount_MultipleKeys_ReturnsCorrectCount()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);
        _map.Add("c", 3);

        Assert.That(_map.KeyCount, Is.EqualTo(3));
    }

    [Test]
    public void KeyCount_MultipleValuesPerKey_ReturnsOne()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("a", 3);

        Assert.That(_map.KeyCount, Is.EqualTo(1));
    }

    // ── Values ────────────────────────────────────────────────

    [Test]
    public void Values_EmptyMap_ReturnsEmpty()
    {
        Assert.That(_map.Values, Is.Empty);
    }

    [Test]
    public void Values_MultipleValuesAcrossKeys_ReturnsAll()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("b", 3);

        Assert.That(_map.Values, Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    // ── GetValuesCount ────────────────────────────────────────

    [Test]
    public void GetValuesCount_NonExistentKey_ReturnsZero()
    {
        Assert.That(_map.GetValuesCount("missing"), Is.EqualTo(0));
    }

    [Test]
    public void GetValuesCount_ExistingKey_ReturnsCorrectCount()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("a", 3);

        Assert.That(_map.GetValuesCount("a"), Is.EqualTo(3));
    }

    [Test]
    public void GetValuesCount_AfterRemovingValue_DecreasesCorrectly()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Remove("a", 1);

        Assert.That(_map.GetValuesCount("a"), Is.EqualTo(1));
    }

    // ── Clear ─────────────────────────────────────────────────

    [Test]
    public void Clear_RemovesAllEntries()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);

        _map.Clear();

        Assert.That(_map.Count, Is.Zero);
        Assert.That(_map.ContainsKey("a"), Is.False);
        Assert.That(_map.ContainsKey("b"), Is.False);
    }

    [Test]
    public void Clear_EmptyMap_DoesNotThrow()
    {
        _map.Clear();

        Assert.That(_map.Count, Is.Zero);
    }

    [Test]
    public void Clear_ThenAdd_WorksCorrectly()
    {
        _map.Add("a", 1);
        _map.Clear();
        _map.Add("a", 2);

        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 2 }));
        Assert.That(_map.Count, Is.EqualTo(1));
    }

    // ── GetEnumerator ─────────────────────────────────────────

    [Test]
    public void GetEnumerator_EnumeratesAllKeyValuePairs()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("b", 3);

        var pairs = _map.ToList();

        Assert.That(pairs, Has.Count.EqualTo(3));
        Assert.That(pairs, Does.Contain(new KeyValuePair<string, int>("a", 1)));
        Assert.That(pairs, Does.Contain(new KeyValuePair<string, int>("a", 2)));
        Assert.That(pairs, Does.Contain(new KeyValuePair<string, int>("b", 3)));
    }

    [Test]
    public void GetEnumerator_EmptyMap_YieldsNothing()
    {
        Assert.That(_map.ToList(), Is.Empty);
    }

    [Test]
    public void GetEnumerator_NonGeneric_EnumeratesAllPairs()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);

        var enumerable = (System.Collections.IEnumerable)_map;
        var items = new List<KeyValuePair<string, int>>();
        foreach (KeyValuePair<string, int> kvp in enumerable)
            items.Add(kvp);

        Assert.That(items, Has.Count.EqualTo(2));
        Assert.That(items, Does.Contain(new KeyValuePair<string, int>("a", 1)));
        Assert.That(items, Does.Contain(new KeyValuePair<string, int>("b", 2)));
    }
}
