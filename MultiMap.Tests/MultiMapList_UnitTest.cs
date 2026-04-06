using MultiMap.Entities;

namespace MultiMap.Tests;

[TestFixture]
public class MultiMapListTests
{
    private MultiMapList<string, int> _map;

    [SetUp]
    public void SetUp()
    {
        _map = new MultiMapList<string, int>();
    }

    [Test]
    public void Add_SingleKeyValue_CanBeRetrieved()
    {
        _map.Add("a", 1);

        Assert.That(_map.GetOrDefault("a"), Is.EqualTo(new[] { 1 }));
    }

    [Test]
    public void Add_NewValue_ReturnsTrue()
    {
        Assert.That(_map.Add("a", 1), Is.True);
    }

    [Test]
    public void Add_DuplicateValue_AlsoReturnsTrue()
    {
        _map.Add("a", 1);

        Assert.That(_map.Add("a", 1), Is.True);
    }

    [Test]
    public void Add_MultipleValuesForSameKey_ReturnsAllValues()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("a", 3);

        Assert.That(_map.GetOrDefault("a"), Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void Add_DifferentKeys_StoresIndependently()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);

        Assert.That(_map.GetOrDefault("a"), Is.EqualTo(new[] { 1 }));
        Assert.That(_map.GetOrDefault("b"), Is.EqualTo(new[] { 2 }));
    }

    [Test]
    public void AddRange_NewKey_StoresAllValues()
    {
        _map.AddRange("a", new[] { 1, 2, 3 });

        Assert.That(_map.GetOrDefault("a"), Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void AddRange_ExistingKey_AppendsValues()
    {
        _map.Add("a", 1);
        _map.AddRange("a", new[] { 2, 3 });

        Assert.That(_map.GetOrDefault("a"), Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void AddRange_EmptyCollection_DoesNotChangeState()
    {
        _map.Add("a", 1);
        _map.AddRange("a", Enumerable.Empty<int>());

        Assert.That(_map.GetOrDefault("a"), Is.EqualTo(new[] { 1 }));
        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void AddRange_DuplicateValues_AllStored()
    {
        _map.AddRange("a", new[] { 1, 1, 1 });

        Assert.That(_map.GetOrDefault("a"), Is.EqualTo(new[] { 1, 1, 1 }));
        Assert.That(_map.Count, Is.EqualTo(3));
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

        Assert.That(_map.Get("a"), Is.EqualTo(new[] { 1, 2 }));
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
        Assert.That(values, Is.EqualTo(new[] { 1, 2 }));
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
        Assert.That(values, Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void TryGet_WithDuplicates_ReturnsAllInstances()
    {
        _map.Add("a", 1);
        _map.Add("a", 1);

        bool found = _map.TryGet("a", out var values);

        Assert.That(found, Is.True);
        Assert.That(values, Is.EqualTo(new[] { 1, 1 }));
    }

    [Test]
    public void Remove_ExistingValue_ReturnsTrue()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);

        Assert.That(_map.Remove("a", 1), Is.True);
        Assert.That(_map.GetOrDefault("a"), Is.EqualTo(new[] { 2 }));
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
    public void Add_DuplicateValues_AreBothStored()
    {
        _map.Add("a", 1);
        _map.Add("a", 1);

        Assert.That(_map.GetOrDefault("a"), Is.EqualTo(new[] { 1, 1 }));
        Assert.That(_map.Count, Is.EqualTo(2));
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
    public void Clear_RemovesAllEntries()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);

        _map.Clear();

        Assert.That(_map.Count, Is.Zero);
        Assert.That(_map.ContainsKey("a"), Is.False);
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
        var other = new MultiMapList<string, int>();
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
        var other = new MultiMapList<string, int>();
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
            int expectedCount = (cycle + 1) * 5;
            for (int i = 0; i < expectedCount; i++)
                _map.Add($"k{i % 3}", i);

            Assert.That(_map.Count, Is.EqualTo(expectedCount), $"Count wrong before clear in cycle {cycle}");

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
            _map.Add("a", cycle);
            expected++;

            _map.AddRange("b", new[] { cycle * 10, cycle * 10 + 1 });
            expected += 2;

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
        int expected = 0;

        for (int cycle = 0; cycle < 40; cycle++)
        {
            string key = $"key{cycle % 5}";

            if (_map.ContainsKey(key))
            {
                int keyCount = _map.GetOrDefault(key).Count();
                _map.RemoveKey(key);
                expected -= keyCount;
            }

            var values = Enumerable.Range(cycle * 10, 5).ToArray();
            _map.AddRange(key, values);
            expected += values.Length;

            Assert.That(_map.Count, Is.EqualTo(expected), $"Count mismatch at cycle {cycle}");
        }
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
    public void KeyCount_WithDuplicateValuesForSameKey_ReturnsOne()
    {
        _map.Add("key1", 1);
        _map.Add("key1", 1);
        _map.Add("key1", 1);
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
    public void Values_WithDuplicateValues_ReturnsAllIncludingDuplicates()
    {
        _map.Add("key1", 1);
        _map.Add("key1", 1);
        _map.Add("key1", 2);
        Assert.That(_map.Values, Is.EquivalentTo(new[] { 1, 1, 2 }));
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
    public void GetValuesCount_WithDuplicateValues_CountsAllDuplicates()
    {
        _map.Add("key1", 1);
        _map.Add("key1", 1);
        _map.Add("key1", 1);
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
    public void Indexer_WithDuplicateValues_ReturnsAllDuplicates()
    {
        _map.Add("key1", 1);
        _map.Add("key1", 1);
        Assert.That(_map["key1"], Is.EquivalentTo(new[] { 1, 1 }));
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
    public void AddRangeKeyValuePairs_DuplicatePairs_AddsDuplicates()
    {
        var pairs = new[]
        {
            new KeyValuePair<string, int>("key1", 1),
            new KeyValuePair<string, int>("key1", 1)
        };
        _map.AddRange(pairs);
        Assert.That(_map.Get("key1"), Is.EquivalentTo(new[] { 1, 1 }));
        Assert.That(_map.Count, Is.EqualTo(2));
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
    public void RemoveRange_DuplicateValues_RemovesOnlyOneInstance()
    {
        _map.Add("key1", 1);
        _map.Add("key1", 1);
        var pairs = new[] { new KeyValuePair<string, int>("key1", 1) };
        int removed = _map.RemoveRange(pairs);
        Assert.That(removed, Is.EqualTo(1));
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
    public void RemoveWhere_DuplicateValues_RemovesAllMatchingDuplicates()
    {
        _map.Add("key1", 1);
        _map.Add("key1", 1);
        _map.Add("key1", 2);
        int removed = _map.RemoveWhere("key1", v => v == 1);
        Assert.That(removed, Is.EqualTo(2));
        Assert.That(_map.Get("key1"), Is.EquivalentTo(new[] { 2 }));
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

        Assert.That(snapshot, Is.EqualTo(new[] { 1 }));
    }

    [Test]
    public void GetOrDefault_ReturnsSnapshot_NotLiveCollection()
    {
        _map.Add("a", 1);

        var snapshot = _map.GetOrDefault("a").ToList();
        _map.Add("a", 2);

        Assert.That(snapshot, Is.EqualTo(new[] { 1 }));
    }

    [Test]
    public void TryGet_ReturnsSnapshot_NotLiveCollection()
    {
        _map.Add("a", 1);

        _map.TryGet("a", out var snapshot);
        _map.Add("a", 2);

        Assert.That(snapshot, Is.EqualTo(new[] { 1 }));
    }

    // ── Additional Equals Edge Cases ─────────────────────────

    [Test]
    public void Equals_DifferentContent_ReturnsFalse()
    {
        var other = new MultiMapList<string, int>();
        _map.Add("a", 1);
        other.Add("a", 2);

        Assert.That(_map.Equals(other), Is.False);
    }

    [Test]
    public void Equals_DifferentKeys_ReturnsFalse()
    {
        var other = new MultiMapList<string, int>();
        _map.Add("a", 1);
        other.Add("b", 1);

        Assert.That(_map.Equals(other), Is.False);
    }

    [Test]
    public void Equals_EmptyMaps_ReturnsTrue()
    {
        var other = new MultiMapList<string, int>();

        Assert.That(_map.Equals(other), Is.True);
    }
}

