using MultiMap.Entities;
using MultiMap.Interfaces;

namespace MultiMap.Tests;

[TestFixture]
public class MultiMapSetTests
{
    private MultiMapSet<string, int> _map;

    [SetUp]
    public void SetUp()
    {
        _map = new MultiMapSet<string, int>();
    }

    [Test]
    public void Add_SingleKeyValue_CanBeRetrieved()
    {
        _map.Add("a", 1);

        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void Add_NewValue_ReturnsTrue()
    {
        Assert.That(_map.Add("a", 1), Is.True);
    }

    [Test]
    public void Add_DuplicateValue_ReturnsFalse()
    {
        _map.Add("a", 1);

        Assert.That(_map.Add("a", 1), Is.False);
    }

    [Test]
    public void Add_DuplicateValue_DoesNotStoreSecondCopy()
    {
        _map.Add("a", 1);
        _map.Add("a", 1);

        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Add_MultipleValuesForSameKey_ReturnsAllValues()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("a", 3);

        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1, 2, 3 }));
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
    public void AddRange_DuplicateValues_IgnoresDuplicates()
    {
        _map.AddRange("a", new[] { 1, 1, 1 });

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
    public void GetOrDefault_NonExistentKey_ReturnsEmpty()
    {
        Assert.That(_map.GetOrDefault("missing"), Is.Empty);
    }

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
    public void TryGet_AfterRemoveKey_ReturnsFalseWithEmpty()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.RemoveKey("a");

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

    [Test]
    public void TryGet_MultipleValuesForSameKey_ReturnsAllValues()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("a", 3);

        bool found = _map.TryGet("a", out var values);

        Assert.That(found, Is.True);
        Assert.That(values, Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

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
    public void RemoveKey_ExistingKey_ReturnsTrueAndRemovesAll()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);

        Assert.That(_map.RemoveKey("a"), Is.True);
        Assert.That(_map.ContainsKey("a"), Is.False);
    }

    [Test]
    public void RemoveKey_NonExistentKey_ReturnsFalse()
    {
        Assert.That(_map.RemoveKey("missing"), Is.False);
    }

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
    public void Contains_ExistingKeyAndValue_ReturnsTrue()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);

        Assert.That(_map.Contains("a", 1), Is.True);
        Assert.That(_map.Contains("a", 2), Is.True);
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
    public void Count_DecreasesAfterRemove()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Remove("a", 1);

        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Count_DecreasesAfterRemoveKey()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("b", 3);

        _map.RemoveKey("a");

        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Count_UnchangedAfterFailedRemove_NonExistentValue()
    {
        _map.Add("a", 1);

        _map.Remove("a", 99);

        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Count_UnchangedAfterFailedRemove_NonExistentKey()
    {
        _map.Add("a", 1);

        _map.Remove("missing", 1);

        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Count_UnchangedAfterFailedRemoveKey()
    {
        _map.Add("a", 1);

        _map.RemoveKey("missing");

        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void AddRange_WithPartialDuplicates_CountsOnlyNewValues()
    {
        _map.Add("a", 1);
        _map.AddRange("a", new[] { 1, 2, 3 });

        Assert.That(_map.Count, Is.EqualTo(3));
    }

    [Test]
    public void Clear_RemovesAllEntries()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);

        _map.Clear();

        Assert.That(_map.Count, Is.Zero);
        Assert.That(_map.ContainsKey("a"), Is.False);
    }

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
    public void GetEnumerator_SingleKey_ReturnsAllValuesForKey()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);

        var pairs = _map.ToList();

        Assert.That(pairs, Has.Count.EqualTo(2));
        Assert.That(pairs, Has.All.Matches<KeyValuePair<string, int>>(kvp => kvp.Key == "a"));
    }

    [Test]
    public void GetEnumerator_AfterRemoval_ReflectsCurrentState()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Remove("a", 1);

        var pairs = _map.ToList();

        Assert.That(pairs, Has.Count.EqualTo(1));
        Assert.That(pairs, Does.Contain(new KeyValuePair<string, int>("a", 2)));
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

    [Test]
    public void GetEnumerator_ForEach_IteratesAllPairs()
    {
        _map.Add("x", 10);
        _map.Add("y", 20);

        var items = new List<KeyValuePair<string, int>>();
        foreach (var kvp in _map)
            items.Add(kvp);

        Assert.That(items, Has.Count.EqualTo(2));
        Assert.That(items, Does.Contain(new KeyValuePair<string, int>("x", 10)));
        Assert.That(items, Does.Contain(new KeyValuePair<string, int>("y", 20)));
    }

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
    public void Keys_MultipleValuesPerKey_ReturnsDistinctKeys()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("b", 3);

        Assert.That(_map.Keys, Is.EquivalentTo(new[] { "a", "b" }));
    }

    [Test]
    public void Keys_AfterRemovingLastValueForKey_DoesNotContainKey()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);
        _map.Remove("a", 1);

        Assert.That(_map.Keys, Is.EquivalentTo(new[] { "b" }));
    }

    [Test]
    public void Keys_AfterRemoveKey_DoesNotContainKey()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);
        _map.RemoveKey("a");

        Assert.That(_map.Keys, Is.EquivalentTo(new[] { "b" }));
    }

    [Test]
    public void Keys_AfterClear_ReturnsEmpty()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);
        _map.Clear();

        Assert.That(_map.Keys, Is.Empty);
    }

    [Test]
    public void Equals_SameInstance_ReturnsTrue()
    {
        Assert.That(_map.Equals(_map), Is.True);
    }

    [Test]
    public void Equals_DifferentInstanceSameContent_ReturnsTrue()
    {
        var other = new MultiMapSet<string, int>();
        _map.Add("a", 1);
        other.Add("a", 1);

        Assert.That(_map.Equals(other), Is.True);
    }

    [Test]
    public void Equals_Null_ReturnsFalse()
    {
        Assert.That(_map.Equals(null), Is.False);
    }

    [Test]
    public void Equals_DifferentType_ReturnsFalse()
    {
        Assert.That(_map.Equals("not a map"), Is.False);
    }

    [Test]
    public void GetHashCode_SameInstance_ReturnsSameValue()
    {
        _map.Add("a", 1);

        int hash1 = _map.GetHashCode();
        int hash2 = _map.GetHashCode();

        Assert.That(hash1, Is.EqualTo(hash2));
    }

    [Test]
    public void GetHashCode_DifferentInstancesSameContent_ReturnsSameValue()
    {
        var other = new MultiMapSet<string, int>();
        _map.Add("a", 1);
        other.Add("a", 1);

        Assert.That(_map.GetHashCode(), Is.EqualTo(other.GetHashCode()));
    }

    [Test]
    [Category("Stress")]
    public void Stress_RepeatedAddRemoveCycles_CountRemainsAccurate()
    {
        for (int cycle = 0; cycle < 50; cycle++)
        {
            for (int i = 0; i < 20; i++)
                _map.Add("key", i);

            Assert.That(_map.Count, Is.EqualTo(20), $"Count wrong after adds in cycle {cycle}");

            for (int i = 0; i < 20; i++)
                _map.Remove("key", i);

            Assert.That(_map.Count, Is.Zero, $"Count wrong after removes in cycle {cycle}");
        }
    }

    [Test]
    [Category("Stress")]
    public void Stress_ClearAndRebuild_CountResetsCorrectly()
    {
        for (int cycle = 0; cycle < 50; cycle++)
        {
            for (int i = 0; i < 10; i++)
                _map.Add($"k{i % 3}", cycle * 10 + i);

            Assert.That(_map.Count, Is.EqualTo(10), $"Count wrong before clear in cycle {cycle}");

            _map.Clear();

            Assert.That(_map.Count, Is.Zero, $"Count wrong after clear in cycle {cycle}");
            Assert.That(_map.Keys, Is.Empty);
        }
    }

    [Test]
    [Category("Stress")]
    public void Stress_MixedOperations_CountTracksCorrectly()
    {
        int expected = 0;

        for (int cycle = 0; cycle < 30; cycle++)
        {
            if (_map.Add("a", cycle))
                expected++;

            foreach (var v in new[] { cycle * 10, cycle * 10 + 1 })
            {
                if (_map.Add("b", v))
                    expected++;
            }

            if (cycle > 0 && cycle % 5 == 0)
            {
                _map.Clear();
                expected = 0;
            }

            if (cycle > 0 && cycle % 3 == 0 && _map.ContainsKey("a"))
            {
                int beforeKeys = _map.GetOrDefault("a").Count();
                _map.RemoveKey("a");
                expected -= beforeKeys;
            }

            Assert.That(_map.Count, Is.EqualTo(expected), $"Count mismatch at cycle {cycle}");
        }
    }

    [Test]
    [Category("Stress")]
    public void Stress_AddRangeAndRemoveKey_CountDecreasesCorrectly()
    {
        for (int cycle = 0; cycle < 40; cycle++)
        {
            string key = $"key{cycle % 5}";
            _map.RemoveKey(key);

            var values = Enumerable.Range(cycle * 10, 5);
            _map.AddRange(key, values);
        }

        int totalCount = 0;
        foreach (var key in _map.Keys)
            totalCount += _map.GetOrDefault(key).Count();

        Assert.That(_map.Count, Is.EqualTo(totalCount));
    }

    // ── KeyCount Property Tests ──────────────────────────────

    [Test]
    public void KeyCount_EmptyMap_ReturnsZero()
    {
        Assert.That(_map.KeyCount, Is.EqualTo(0));
    }

    [Test]
    public void KeyCount_AfterAddingSingleKey_ReturnsOne()
    {
        _map.Add("key1", 1);
        Assert.That(_map.KeyCount, Is.EqualTo(1));
    }

    [Test]
    public void KeyCount_AfterAddingMultipleKeys_ReturnsCorrectCount()
    {
        _map.Add("key1", 1);
        _map.Add("key2", 2);
        _map.Add("key3", 3);
        Assert.That(_map.KeyCount, Is.EqualTo(3));
    }

    [Test]
    public void KeyCount_AfterAddingMultipleValuesToSameKey_ReturnsOne()
    {
        _map.Add("key1", 1);
        _map.Add("key1", 2);
        _map.Add("key1", 3);
        Assert.That(_map.KeyCount, Is.EqualTo(1));
    }

    [Test]
    public void KeyCount_AfterRemovingKey_DecreasesCorrectly()
    {
        _map.Add("key1", 1);
        _map.Add("key2", 2);
        _map.RemoveKey("key1");
        Assert.That(_map.KeyCount, Is.EqualTo(1));
    }

    // ── Values Property Tests ────────────────────────────────

    [Test]
    public void Values_EmptyMap_ReturnsEmpty()
    {
        Assert.That(_map.Values, Is.Empty);
    }

    [Test]
    public void Values_WithSingleValue_ReturnsCorrectValue()
    {
        _map.Add("key1", 42);
        Assert.That(_map.Values, Is.EquivalentTo(new[] { 42 }));
    }

    [Test]
    public void Values_WithMultipleValuesAcrossKeys_ReturnsAllValues()
    {
        _map.Add("key1", 1);
        _map.Add("key1", 2);
        _map.Add("key2", 3);
        _map.Add("key2", 4);
        Assert.That(_map.Values, Is.EquivalentTo(new[] { 1, 2, 3, 4 }));
    }

    [Test]
    public void Values_AfterRemovingValue_ReturnsRemainingValues()
    {
        _map.Add("key1", 1);
        _map.Add("key1", 2);
        _map.Remove("key1", 1);
        Assert.That(_map.Values, Is.EquivalentTo(new[] { 2 }));
    }

    // ── GetValuesCount Method Tests ──────────────────────────

    [Test]
    public void GetValuesCount_NonExistentKey_ReturnsZero()
    {
        Assert.That(_map.GetValuesCount("missing"), Is.EqualTo(0));
    }

    [Test]
    public void GetValuesCount_KeyWithSingleValue_ReturnsOne()
    {
        _map.Add("key1", 1);
        Assert.That(_map.GetValuesCount("key1"), Is.EqualTo(1));
    }

    [Test]
    public void GetValuesCount_KeyWithMultipleValues_ReturnsCorrectCount()
    {
        _map.Add("key1", 1);
        _map.Add("key1", 2);
        _map.Add("key1", 3);
        Assert.That(_map.GetValuesCount("key1"), Is.EqualTo(3));
    }

    [Test]
    public void GetValuesCount_AfterRemovingValue_DecreasesCorrectly()
    {
        _map.Add("key1", 1);
        _map.Add("key1", 2);
        _map.Remove("key1", 1);
        Assert.That(_map.GetValuesCount("key1"), Is.EqualTo(1));
    }

    // ── Indexer Tests ─────────────────────────────────────────

    [Test]
    public void Indexer_ExistingKey_ReturnsValues()
    {
        _map.Add("key1", 1);
        _map.Add("key1", 2);
        Assert.That(_map["key1"], Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public void Indexer_NonExistentKey_ThrowsKeyNotFoundException()
    {
        Assert.Throws<KeyNotFoundException>(() => { var _ = _map["missing"].ToList(); });
    }

    [Test]
    public void Indexer_AfterAddingValue_ReturnsUpdatedValues()
    {
        _map.Add("key1", 1);
        Assert.That(_map["key1"], Is.EquivalentTo(new[] { 1 }));
        _map.Add("key1", 2);
        Assert.That(_map["key1"], Is.EquivalentTo(new[] { 1, 2 }));
    }

    // ── AddRange(KeyValuePairs) Tests ────────────────────────

    [Test]
    public void AddRangeKeyValuePairs_EmptyCollection_DoesNothing()
    {
        _map.AddRange([]);
        Assert.That(_map.Count, Is.EqualTo(0));
    }

    [Test]
    public void AddRangeKeyValuePairs_SinglePair_AddsCorrectly()
    {
        var pairs = new[] { new KeyValuePair<string, int>("key1", 1) };
        _map.AddRange(pairs);
        Assert.That(_map.Get("key1"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void AddRangeKeyValuePairs_MultiplePairsSameKey_AddsAllValues()
    {
        var pairs = new[]
        {
            new KeyValuePair<string, int>("key1", 1),
            new KeyValuePair<string, int>("key1", 2),
            new KeyValuePair<string, int>("key1", 3)
        };
        _map.AddRange(pairs);
        Assert.That(_map.Get("key1"), Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void AddRangeKeyValuePairs_MultiplePairsDifferentKeys_AddsCorrectly()
    {
        var pairs = new[]
        {
            new KeyValuePair<string, int>("key1", 1),
            new KeyValuePair<string, int>("key2", 2),
            new KeyValuePair<string, int>("key3", 3)
        };
        _map.AddRange(pairs);
        Assert.That(_map.Get("key1"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(_map.Get("key2"), Is.EquivalentTo(new[] { 2 }));
        Assert.That(_map.Get("key3"), Is.EquivalentTo(new[] { 3 }));
    }

    [Test]
    public void AddRangeKeyValuePairs_DuplicatePairs_IgnoresDuplicates()
    {
        var pairs = new[]
        {
            new KeyValuePair<string, int>("key1", 1),
            new KeyValuePair<string, int>("key1", 1)
        };
        _map.AddRange(pairs);
        Assert.That(_map.Get("key1"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(_map.Count, Is.EqualTo(1));
    }

    // ── RemoveRange Tests ─────────────────────────────────────

    [Test]
    public void RemoveRange_EmptyCollection_ReturnsZero()
    {
        _map.Add("key1", 1);
        int removed = _map.RemoveRange([]);
        Assert.That(removed, Is.EqualTo(0));
        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void RemoveRange_SingleExistingPair_ReturnsOne()
    {
        _map.Add("key1", 1);
        var pairs = new[] { new KeyValuePair<string, int>("key1", 1) };
        int removed = _map.RemoveRange(pairs);
        Assert.That(removed, Is.EqualTo(1));
        Assert.That(_map.Count, Is.EqualTo(0));
    }

    [Test]
    public void RemoveRange_SingleNonExistentPair_ReturnsZero()
    {
        _map.Add("key1", 1);
        var pairs = new[] { new KeyValuePair<string, int>("key2", 2) };
        int removed = _map.RemoveRange(pairs);
        Assert.That(removed, Is.EqualTo(0));
        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void RemoveRange_MultiplePairs_RemovesCorrectCount()
    {
        _map.Add("key1", 1);
        _map.Add("key1", 2);
        _map.Add("key2", 3);
        var pairs = new[]
        {
            new KeyValuePair<string, int>("key1", 1),
            new KeyValuePair<string, int>("key2", 3)
        };
        int removed = _map.RemoveRange(pairs);
        Assert.That(removed, Is.EqualTo(2));
        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void RemoveRange_MixedExistingAndNonExisting_RemovesOnlyExisting()
    {
        _map.Add("key1", 1);
        _map.Add("key2", 2);
        var pairs = new[]
        {
            new KeyValuePair<string, int>("key1", 1),
            new KeyValuePair<string, int>("key3", 3)
        };
        int removed = _map.RemoveRange(pairs);
        Assert.That(removed, Is.EqualTo(1));
        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void RemoveRange_LastValueOfKey_RemovesKey()
    {
        _map.Add("key1", 1);
        var pairs = new[] { new KeyValuePair<string, int>("key1", 1) };
        _map.RemoveRange(pairs);
        Assert.That(_map.ContainsKey("key1"), Is.False);
    }

    // ── RemoveWhere Tests ─────────────────────────────────────

    [Test]
    public void RemoveWhere_NonExistentKey_ReturnsZero()
    {
        int removed = _map.RemoveWhere("missing", v => v > 0);
        Assert.That(removed, Is.EqualTo(0));
    }

    [Test]
    public void RemoveWhere_NoMatchingValues_ReturnsZero()
    {
        _map.Add("key1", 1);
        _map.Add("key1", 2);
        int removed = _map.RemoveWhere("key1", v => v > 10);
        Assert.That(removed, Is.EqualTo(0));
        Assert.That(_map.Count, Is.EqualTo(2));
    }

    [Test]
    public void RemoveWhere_SingleMatchingValue_RemovesAndReturnsOne()
    {
        _map.Add("key1", 1);
        _map.Add("key1", 2);
        _map.Add("key1", 3);
        int removed = _map.RemoveWhere("key1", v => v == 2);
        Assert.That(removed, Is.EqualTo(1));
        Assert.That(_map.Get("key1"), Is.EquivalentTo(new[] { 1, 3 }));
    }

    [Test]
    public void RemoveWhere_MultipleMatchingValues_RemovesAll()
    {
        _map.Add("key1", 1);
        _map.Add("key1", 2);
        _map.Add("key1", 3);
        _map.Add("key1", 4);
        int removed = _map.RemoveWhere("key1", v => v > 2);
        Assert.That(removed, Is.EqualTo(2));
        Assert.That(_map.Get("key1"), Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public void RemoveWhere_AllValues_RemovesKeyAndReturnsCount()
    {
        _map.Add("key1", 1);
        _map.Add("key1", 2);
        int removed = _map.RemoveWhere("key1", v => true);
        Assert.That(removed, Is.EqualTo(2));
        Assert.That(_map.ContainsKey("key1"), Is.False);
        Assert.That(_map.Count, Is.EqualTo(0));
    }

    [Test]
    public void RemoveWhere_ComplexPredicate_RemovesCorrectly()
    {
        _map.Add("key1", 1);
        _map.Add("key1", 2);
        _map.Add("key1", 3);
        _map.Add("key1", 4);
        _map.Add("key1", 5);
        int removed = _map.RemoveWhere("key1", v => v % 2 == 0);
        Assert.That(removed, Is.EqualTo(2));
        Assert.That(_map.Get("key1"), Is.EquivalentTo(new[] { 1, 3, 5 }));
    }

    // ── Defensive Copy (Snapshot) Tests ───────────────────────

    [Test]
    public void Get_ReturnsSnapshot_NotLiveCollection()
    {
        _map.Add("a", 1);

        var snapshot = _map.Get("a").ToList();
        _map.Add("a", 2);

        Assert.That(snapshot, Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void GetOrDefault_ReturnsSnapshot_NotLiveCollection()
    {
        _map.Add("a", 1);

        var snapshot = _map.GetOrDefault("a").ToList();
        _map.Add("a", 2);

        Assert.That(snapshot, Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void TryGet_ReturnsSnapshot_NotLiveCollection()
    {
        _map.Add("a", 1);

        _map.TryGet("a", out var snapshot);
        _map.Add("a", 2);

        Assert.That(snapshot, Is.EquivalentTo(new[] { 1 }));
    }

    // ── Additional Equals Edge Cases ─────────────────────────

    [Test]
    public void Equals_DifferentContent_ReturnsFalse()
    {
        var other = new MultiMapSet<string, int>();
        _map.Add("a", 1);
        other.Add("a", 2);

        Assert.That(_map.Equals(other), Is.False);
    }

    [Test]
    public void Equals_DifferentKeys_ReturnsFalse()
    {
        var other = new MultiMapSet<string, int>();
        _map.Add("a", 1);
        other.Add("b", 1);

        Assert.That(_map.Equals(other), Is.False);
    }

    [Test]
    public void Equals_EmptyMaps_ReturnsTrue()
    {
        var other = new MultiMapSet<string, int>();

        Assert.That(_map.Equals(other), Is.True);
    }

    [Test]
    public void Equals_DifferentValueCount_SameKeys_ReturnsFalse()
    {
        var other = new MultiMapSet<string, int>();
        _map.Add("a", 1);
        _map.Add("a", 2);
        other.Add("a", 1);

        Assert.That(_map.Equals(other), Is.False);
    }

    [Test]
    public void Constructor_WithCapacity_WorksCorrectly()
    {
        var map = new MultiMapSet<string, int>(100);
        map.Add("a", 1);

        Assert.That(map.Get("a"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void Constructor_WithValueComparer_UsesCaseInsensitiveComparison()
    {
        var map = new MultiMapSet<string, string>(valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("key", "Hello");
        bool added = map.Add("key", "hello");

        Assert.That(added, Is.False);
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Constructor_WithCapacityAndValueComparer_WorksCorrectly()
    {
        var map = new MultiMapSet<string, string>(100, valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("key", "Hello");
        bool added = map.Add("key", "hello");

        Assert.That(added, Is.False);
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Add_WithCaseInsensitiveComparer_TreatsSameCaseAsDuplicate()
    {
        var map = new MultiMapSet<string, string>(valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("key", "ABC");
        map.Add("key", "abc");
        map.Add("key", "Abc");

        Assert.That(map.Count, Is.EqualTo(1));
        Assert.That(map.GetValuesCount("key"), Is.EqualTo(1));
    }

    [Test]
    public void Contains_WithCaseInsensitiveComparer_FindsValueIgnoringCase()
    {
        var map = new MultiMapSet<string, string>(valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("key", "Hello");

        Assert.That(map.Contains("key", "hello"), Is.True);
        Assert.That(map.Contains("key", "HELLO"), Is.True);
    }

    [Test]
    public void AddRange_WithValueComparer_RespectsComparer()
    {
        var map = new MultiMapSet<string, string>(valueComparer: StringComparer.OrdinalIgnoreCase);
        map.AddRange("key", new[] { "Hello", "hello", "HELLO" });

        Assert.That(map.Count, Is.EqualTo(1));
        Assert.That(map.GetValuesCount("key"), Is.EqualTo(1));
    }

    [Test]
    public void Remove_WithCaseInsensitiveComparer_RemovesIgnoringCase()
    {
        var map = new MultiMapSet<string, string>(valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("key", "Hello");
        bool removed = map.Remove("key", "hello");

        Assert.That(removed, Is.True);
        Assert.That(map.Count, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_WithKeyComparer_UsesCaseInsensitiveKeyComparison()
    {
        var map = new MultiMapSet<string, int>(keyComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("Key", 1);
        map.Add("key", 2);

        Assert.That(map.KeyCount, Is.EqualTo(1));
        Assert.That(map.Get("KEY"), Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public void Constructor_WithCapacityAndKeyComparer_UsesCaseInsensitiveKeyComparison()
    {
        var map = new MultiMapSet<string, int>(100, keyComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("Key", 1);
        map.Add("key", 2);

        Assert.That(map.KeyCount, Is.EqualTo(1));
        Assert.That(map.Get("KEY"), Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public void Constructor_WithCapacityKeyComparerAndValueComparer_WorksCorrectly()
    {
        var map = new MultiMapSet<string, string>(100, StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);
        map.Add("Key", "Hello");
        map.Add("key", "hello");

        Assert.That(map.KeyCount, Is.EqualTo(1));
        Assert.That(map.Count, Is.EqualTo(1));
        Assert.That(map.Get("KEY"), Is.EquivalentTo(new[] { "Hello" }));
    }

    // ── Equals(object?) self-reference ─────────────────────────────────────────

    [Test]
    public void Equals_Object_SameReference_ReturnsTrue()
    {
        _map.Add("a", 1);
        Assert.That(_map.Equals((object)_map), Is.True);
    }

    // ── Equals(IReadOnlyMultiMap<TKey,TValue>?) typed-interface overload ────────

    [Test]
    public void Equals_TypedInterface_SameReference_ReturnsTrue()
    {
        _map.Add("a", 1);
        Assert.That(_map.Equals((IReadOnlyMultiMap<string, int>)_map), Is.True);
    }

    [Test]
    public void Equals_TypedInterface_Null_ReturnsFalse()
    {
        Assert.That(_map.Equals((IReadOnlyMultiMap<string, int>?)null!), Is.False);
    }

    [Test]
    public void Equals_TypedInterface_SameContent_ReturnsTrue()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);

        var other = new MultiMapSet<string, int>();
        other.Add("a", 1);
        other.Add("b", 2);

        Assert.That(_map.Equals((IReadOnlyMultiMap<string, int>)other), Is.True);
    }

    [Test]
    public void Equals_TypedInterface_DifferentValues_ReturnsFalse()
    {
        _map.Add("a", 1);

        var other = new MultiMapSet<string, int>();
        other.Add("a", 2);

        Assert.That(_map.Equals((IReadOnlyMultiMap<string, int>)other), Is.False);
    }

    [Test]
    public void Equals_TypedInterface_MissingKey_ReturnsFalse()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);

        var other = new MultiMapSet<string, int>();
        other.Add("a", 1);

        Assert.That(_map.Equals((IReadOnlyMultiMap<string, int>)other), Is.False);
    }

    [Test]
    public void Equals_TypedInterface_DifferentValueCount_ReturnsFalse()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);

        var other = new MultiMapSet<string, int>();
        other.Add("a", 1);

        Assert.That(_map.Equals((IReadOnlyMultiMap<string, int>)other), Is.False);
    }

    [Test]
    public void Equals_TypedInterface_BothEmpty_ReturnsTrue()
    {
        Assert.That(_map.Equals((IReadOnlyMultiMap<string, int>)new MultiMapSet<string, int>()), Is.True);
    }

    [Test]
    public void Equals_TypedInterface_DifferentKeyCount_ReturnsFalse()
    {
        _map.Add("a", 1);

        var other = new MultiMapSet<string, int>();
        other.Add("a", 1);
        other.Add("b", 2);

        Assert.That(_map.Equals((IReadOnlyMultiMap<string, int>)other), Is.False);
    }

    [Test]
    public void Equals_TypedInterface_OtherKeyNotInThis_ReturnsFalse()
    {
        _map.Add("a", 1);

        var other = new MultiMapSet<string, int>();
        other.Add("z", 1);

        Assert.That(_map.Equals((IReadOnlyMultiMap<string, int>)other), Is.False);
    }

    // ── Missing constructor overloads ─────────────────────

    [Test]
    public void Constructor_WithKeyAndValueComparer_BothApplied()
    {
        var map = new MultiMapSet<string, string>(StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);
        map.Add("KEY", "ABC");
        map.Add("key", "abc");

        Assert.That(map.Get("KEY").Count(), Is.EqualTo(1));
    }

    // ── Null-guard branch coverage ─────────────────────────

    [Test]
    public void Add_NullKey_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.Add(null!, 1));

    [Test]
    public void Add_NullValue_ThrowsArgumentNullException()
    {
        var map = new MultiMapSet<string, string>();
        Assert.Throws<ArgumentNullException>(() => map.Add("key", null!));
    }

    [Test]
    public void AddRange_Key_NullKey_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.AddRange(null!, new[] { 1 }));

    [Test]
    public void AddRange_Key_NullValues_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.AddRange("key", (IEnumerable<int>)null!));

    [Test]
    public void AddRange_Key_NullElementInValues_ThrowsArgumentNullException()
    {
        var map = new MultiMapSet<string, string>();
        Assert.Throws<ArgumentNullException>(() => map.AddRange("key", new string?[] { "a", null }!));
    }

    [Test]
    public void AddRange_Items_NullItems_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.AddRange((IEnumerable<KeyValuePair<string, int>>)null!));

    [Test]
    public void AddRange_Items_NullKey_ThrowsArgumentNullException()
    {
        var items = new[] { new KeyValuePair<string, int>(null!, 1) };
        Assert.Throws<ArgumentNullException>(() => _map.AddRange(items));
    }

    [Test]
    public void AddRange_Items_NullValue_ThrowsArgumentNullException()
    {
        var map = new MultiMapSet<string, string>();
        var items = new[] { new KeyValuePair<string, string>("k", null!) };
        Assert.Throws<ArgumentNullException>(() => map.AddRange(items));
    }

    [Test]
    public void Get_NullKey_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.Get(null!));

    [Test]
    public void GetOrDefault_NullKey_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.GetOrDefault(null!));

    [Test]
    public void Remove_NullKey_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.Remove(null!, 1));

    [Test]
    public void Remove_NullValue_ThrowsArgumentNullException()
    {
        var map = new MultiMapSet<string, string>();
        Assert.Throws<ArgumentNullException>(() => map.Remove("key", null!));
    }

    [Test]
    public void RemoveRange_NullItems_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.RemoveRange(null!));

    [Test]
    public void RemoveWhere_NullKey_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.RemoveWhere(null!, _ => true));

    [Test]
    public void RemoveWhere_NullPredicate_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.RemoveWhere("key", null!));

    [Test]
    public void RemoveKey_NullKey_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.RemoveKey(null!));

    [Test]
    public void ContainsKey_NullKey_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.ContainsKey(null!));

    [Test]
    public void Contains_NullKey_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.Contains(null!, 1));

    [Test]
    public void Contains_NullValue_ThrowsArgumentNullException()
    {
        var map = new MultiMapSet<string, string>();
        Assert.Throws<ArgumentNullException>(() => map.Contains("key", null!));
    }

    [Test]
    public void GetValuesCount_NullKey_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.GetValuesCount(null!));
}


// ──────────────────────────────────────────────────────────────────────────────
// MultiMapSet – constructor overloads + GetHashCode coverage
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MultiMapSet_ConstructorAndHashTests
{
    [Test]
    public void Constructor_WithCapacity_WorksCorrectly()
    {
        var map = new MultiMapSet<string, int>(100);
        map.Add("a", 1);
        Assert.That(map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void Constructor_WithCapacityAndKeyComparer_UsesKeyComparer()
    {
        var map = new MultiMapSet<string, int>(10, StringComparer.OrdinalIgnoreCase);
        map.Add("KEY", 1);
        Assert.That(map.ContainsKey("key"), Is.True);
    }

    [Test]
    public void Constructor_WithCapacityAndValueComparer_DeduplicatesByValueComparer()
    {
        var map = new MultiMapSet<string, string>(10, valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("k", "Hello");
        map.Add("k", "HELLO");
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Constructor_WithCapacityKeyAndValueComparer_BothApplied()
    {
        var map = new MultiMapSet<string, string>(10, StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);
        map.Add("KEY", "ABC");
        map.Add("key", "abc");
        Assert.That(map.Count, Is.EqualTo(1));
        Assert.That(map.ContainsKey("key"), Is.True);
    }

    [Test]
    public void Constructor_WithValueComparer_DeduplicatesByValueComparer()
    {
        var map = new MultiMapSet<string, string>(valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("k", "Hello");
        map.Add("k", "hello");
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void GetHashCode_SameContent_EqualHashCodes()
    {
        var a = new MultiMapSet<string, int>();
        var b = new MultiMapSet<string, int>();
        a.Add("k", 1); a.Add("k", 2); a.Add("m", 3);
        b.Add("k", 2); b.Add("k", 1); b.Add("m", 3); // different insertion order

        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void GetHashCode_Empty_IsStable()
    {
        var map = new MultiMapSet<string, int>();
        Assert.That(map.GetHashCode(), Is.EqualTo(map.GetHashCode()));
    }

    [Test]
    public void Equals_Object_NullObj_ReturnsFalse()
    {
        var map = new MultiMapSet<string, int>();
        map.Add("a", 1);
        Assert.That(map.Equals((object?)null), Is.False);
    }

    [Test]
    public void Equals_Object_SameContent_ReturnsTrue()
    {
        var a = new MultiMapSet<string, int>();
        var b = new MultiMapSet<string, int>();
        a.Add("a", 1); b.Add("a", 1);
        Assert.That(a.Equals((object)b), Is.True);
    }

    [Test]
    public void Equals_Object_DifferentType_ReturnsFalse()
    {
        var map = new MultiMapSet<string, int>();
        Assert.That(map.Equals("not a map"), Is.False);
    }

    [Test]
    public void Equals_SameInstance_ReturnsTrue()
    {
        var map = new MultiMapSet<string, int>();
        map.Add("a", 1);
        Assert.That(map.Equals(map), Is.True);
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// MultiMapList – constructor overloads + GetHashCode / Equals coverage
// ──────────────────────────────────────────────────────────────────────────────


public class MultiMapSet_CapacityComparerConstructorTests
{
    // line 110-113: constructor(capacity, keyComparer, valueComparer)
    [Test]
    public void Constructor_CapacityKeyAndValueComparer_KeyComparerApplied()
    {
        var map = new MultiMapSet<string, string>(10, StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);
        map.Add("KEY", "Hello");
        Assert.That(map.ContainsKey("key"), Is.True);
    }

    [Test]
    public void Constructor_CapacityKeyAndValueComparer_ValueComparerApplied()
    {
        var map = new MultiMapSet<string, string>(10, StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);
        map.Add("a", "Hello");
        map.Add("a", "HELLO"); // duplicate under OrdinalIgnoreCase
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Constructor_CapacityKeyAndValueComparer_RemoveWhereUses_ValueComparer()
    {
        var map = new MultiMapSet<string, string>(10, null, StringComparer.OrdinalIgnoreCase);
        map.Add("k", "apple");
        map.Add("k", "banana");

        int removed = map.RemoveWhere("k", v => v == "APPLE");

        // RemoveWhere predicate uses object equality, not comparer; value "apple" != "APPLE" → 0 removed
        Assert.That(removed, Is.EqualTo(0));
        Assert.That(map.Count, Is.EqualTo(2));
    }

    [Test]
    public void AddRange_WithCapacityComparerMap_AllValuesAdded()
    {
        var map = new MultiMapSet<string, int>(5, null, null);
        int added = map.AddRange("x", new[] { 1, 2, 3 });
        Assert.That(added, Is.EqualTo(3));
    }
}
