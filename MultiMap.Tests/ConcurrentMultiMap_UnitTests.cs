using MultiMap.Entities;

namespace MultiMap.Tests;

[TestFixture]
public class ConcurrentMultiMapTests
{
    private ConcurrentMultiMap<string, int> _map;

    [SetUp]
    public void SetUp()
    {
        _map = new ConcurrentMultiMap<string, int>();
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
    [Category("Stress")]
    public void Add_ConcurrentAdds_AllUniqueValuesStored()
    {
        const int count = 1000;

        Parallel.For(0, count, i =>
        {
            _map.Add("a", i);
        });

        Assert.That(_map.Count, Is.EqualTo(count));
    }

    [Test]
    [Category("Stress")]
    public void Add_ConcurrentDuplicates_OnlyOneStored()
    {
        const int threads = 100;

        Parallel.For(0, threads, _ =>
        {
            _map.Add("a", 42);
        });

        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Stress")]
    public void Remove_ConcurrentRemoves_AllRemoved()
    {
        const int count = 1000;

        for (int i = 0; i < count; i++)
            _map.Add("a", i);

        Parallel.For(0, count, i =>
        {
            _map.Remove("a", i);
        });

        Assert.That(_map.Count, Is.Zero);
    }

    [Test]
    [Category("Stress")]
    public void Add_ConcurrentAddsToDifferentKeys_AllStored()
    {
        const int count = 1000;

        Parallel.For(0, count, i =>
        {
            _map.Add($"key{i}", i);
        });

        Assert.That(_map.Count, Is.EqualTo(count));
    }

    [Test]
    [Category("Stress")]
    public void ConcurrentReadsAndWrites_DoNotThrow()
    {
        const int count = 1000;

        Assert.DoesNotThrow(() =>
        {
            Parallel.For(0, count, i =>
            {
                if (i % 3 == 0)
                    _map.Add("a", i);
                else if (i % 3 == 1)
                    _map.GetOrDefault("a");
                else
                    _map.Contains("a", i);
            });
        });
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
        var other = new ConcurrentMultiMap<string, int>();
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
        var other = new ConcurrentMultiMap<string, int>();
        _map.Add("a", 1);
        other.Add("a", 1);

        Assert.That(_map.GetHashCode(), Is.EqualTo(other.GetHashCode()));
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

    [Test]
    [Category("Stress")]
    public void Stress_ConcurrentAddRemoveClear_CountNeverNegative()
    {
        const int iterations = 500;

        Parallel.For(0, iterations, i =>
        {
            switch (i % 4)
            {
                case 0:
                    _map.Add($"key{i % 10}", i);
                    break;
                case 1:
                    _map.Remove($"key{i % 10}", i - 1);
                    break;
                case 2:
                    _map.AddRange($"key{i % 10}", new[] { i, i + 1000 });
                    break;
                case 3:
                    _map.RemoveKey($"key{i % 10}");
                    break;
            }
        });

        Assert.That(_map.Count, Is.GreaterThanOrEqualTo(0));

        int verifyCount = 0;
        foreach (var key in _map.Keys)
            verifyCount += _map.GetOrDefault(key).Count();

        Assert.That(_map.Count, Is.EqualTo(verifyCount));
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

    [Test]
    [Category("Concurrency")]
    public void KeyCount_ConcurrentAdditions_IsCorrect()
    {
        Parallel.For(0, 100, i => _map.Add($"key{i}", i));
        Assert.That(_map.KeyCount, Is.EqualTo(100));
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

    [Test]
    [Category("Concurrency")]
    public void AddRangeKeyValuePairs_ConcurrentCalls_AllPairsAdded()
    {
        Parallel.For(0, 50, i =>
        {
            var pairs = new[]
            {
                new KeyValuePair<string, int>($"key{i}", i * 10),
                new KeyValuePair<string, int>($"key{i}", i * 10 + 1)
            };
            _map.AddRange(pairs);
        });

        Assert.That(_map.KeyCount, Is.EqualTo(50));
        Assert.That(_map.Count, Is.EqualTo(100));
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

    [Test]
    [Category("Concurrency")]
    public void RemoveWhere_ConcurrentCalls_ThreadSafe()
    {
        for (int i = 0; i < 100; i++)
        {
            _map.Add("key1", i);
        }

        int totalRemoved = 0;
        Parallel.For(0, 10, i =>
        {
            int removed = _map.RemoveWhere("key1", v => v % 10 == i);
            Interlocked.Add(ref totalRemoved, removed);
        });

        Assert.That(totalRemoved, Is.EqualTo(100));
        Assert.That(_map.Count, Is.EqualTo(0));
    }

    // ── Additional Coverage Tests for Values Property and Edge Cases ──

    [Test]
    [Category("Concurrency")]
    public void Values_ConcurrentEnumeration_DoesNotThrow()
    {
        // Add values across multiple keys
        for (int i = 0; i < 20; i++)
            _map.Add($"key{i % 5}", i);

        Assert.DoesNotThrow(() =>
        {
            Parallel.For(0, 10, _ =>
            {
                var values = _map.Values.ToList();
                Assert.That(values, Is.Not.Empty);
            });
        });
    }

    [Test]
    [Category("Stress")]
    public void Values_WithConcurrentModifications_ReturnsSnapshot()
    {
        // Add initial data
        for (int i = 0; i < 50; i++)
            _map.Add($"key{i % 10}", i);

        var enumerationComplete = false;
        var values = new List<int>();

        var enumerationTask = Task.Run(() =>
        {
            values.AddRange(_map.Values);
            enumerationComplete = true;
        });

        var modificationTask = Task.Run(() =>
        {
            for (int i = 0; i < 20; i++)
            {
                _map.Add($"newkey{i}", i + 1000);
                Thread.Sleep(1);
            }
        });

        Task.WaitAll(enumerationTask, modificationTask);
        Assert.That(enumerationComplete, Is.True);
        Assert.That(values, Is.Not.Empty);
    }

    [Test]
    [Category("Concurrency")]
    public void GetEnumerator_ConcurrentEnumerations_DoNotInterfere()
    {
        for (int i = 0; i < 20; i++)
            _map.Add($"key{i % 5}", i);

        var lists = new System.Collections.Concurrent.ConcurrentBag<List<KeyValuePair<string, int>>>();

        Parallel.For(0, 10, _ =>
        {
            var list = _map.ToList();
            lists.Add(list);
        });

        Assert.That(lists.Count, Is.EqualTo(10));
        foreach (var list in lists)
        {
            Assert.That(list.Count, Is.EqualTo(20));
        }
    }

    [Test]
    [Category("Stress")]
    public void Values_MultipleKeysWithMultipleValues_ReturnsAllValues()
    {
        const int keyCount = 50;
        const int valuesPerKey = 20;

        for (int k = 0; k < keyCount; k++)
            for (int v = 0; v < valuesPerKey; v++)
                _map.Add($"key{k}", k * 100 + v);

        var allValues = _map.Values.ToList();
        Assert.That(allValues.Count, Is.EqualTo(keyCount * valuesPerKey));
    }

    [Test]
    [Category("Concurrency")]
    public void GetEnumerator_WithConcurrentRemoval_DoesNotThrow()
    {
        for (int i = 0; i < 100; i++)
            _map.Add($"key{i % 10}", i);

        Assert.DoesNotThrow(() =>
        {
            var enumerateTask = Task.Run(() =>
            {
                foreach (var _ in _map)
                {
                    Thread.Sleep(1);
                }
            });

            var removeTask = Task.Run(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    _map.RemoveKey($"key{i}");
                    Thread.Sleep(2);
                }
            });

            Task.WaitAll(enumerateTask, removeTask);
        });
    }

    [Test]
    [Category("Stress")]
    public void Values_RepeatedEnumeration_ConsistentResults()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("b", 3);

        var first = _map.Values.ToList();
        var second = _map.Values.ToList();

        Assert.That(first, Is.EquivalentTo(second));
    }

    [Test]
    public void Keys_ReturnsSnapshot_NotLiveCollection()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);

        var keys = _map.Keys;
        _map.Add("c", 3);

        Assert.That(keys, Is.EquivalentTo(new[] { "a", "b" }));
    }

    [Test]
    public void Constructor_WithConcurrencyAndCapacity_WorksCorrectly()
    {
        var map = new ConcurrentMultiMap<string, int>(4, 100);
        map.Add("a", 1);

        Assert.That(map.Get("a"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void Constructor_WithValueComparer_UsesCaseInsensitiveComparison()
    {
        var map = new ConcurrentMultiMap<string, string>(StringComparer.OrdinalIgnoreCase);
        map.Add("key", "Hello");
        bool added = map.Add("key", "hello");

        Assert.That(added, Is.False);
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Constructor_WithConcurrencyCapacityAndValueComparer_WorksCorrectly()
    {
        var map = new ConcurrentMultiMap<string, string>(4, 100, StringComparer.OrdinalIgnoreCase);
        map.Add("key", "Hello");
        bool added = map.Add("key", "hello");

        Assert.That(added, Is.False);
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Add_WithCaseInsensitiveComparer_TreatsSameCaseAsDuplicate()
    {
        var map = new ConcurrentMultiMap<string, string>(StringComparer.OrdinalIgnoreCase);
        map.Add("key", "ABC");
        map.Add("key", "abc");
        map.Add("key", "Abc");

        Assert.That(map.Count, Is.EqualTo(1));
        Assert.That(map.GetValuesCount("key"), Is.EqualTo(1));
    }

    [Test]
    public void Contains_WithCaseInsensitiveComparer_FindsValueIgnoringCase()
    {
        var map = new ConcurrentMultiMap<string, string>(StringComparer.OrdinalIgnoreCase);
        map.Add("key", "Hello");

        Assert.That(map.Contains("key", "hello"), Is.True);
        Assert.That(map.Contains("key", "HELLO"), Is.True);
    }

    [Test]
    public void AddRange_WithValueComparer_RespectsComparer()
    {
        var map = new ConcurrentMultiMap<string, string>(StringComparer.OrdinalIgnoreCase);
        map.AddRange("key", new[] { "Hello", "hello", "HELLO" });

        Assert.That(map.Count, Is.EqualTo(1));
        Assert.That(map.GetValuesCount("key"), Is.EqualTo(1));
    }

    // ── AddRange ICollection branch coverage ─────────────────

    [Test]
    public void AddRange_WithListValues_UsesICollectionPath()
    {
        var values = new List<int> { 1, 2, 3 };
        _map.AddRange("a", values);

        Assert.That(_map.Get("a"), Is.EquivalentTo(new[] { 1, 2, 3 }));
        Assert.That(_map.Count, Is.EqualTo(3));
    }

    [Test]
    public void AddRange_WithNonCollectionEnumerable_UsesToArrayFallback()
    {
        var values = Enumerable.Range(1, 3).Select(x => x);
        _map.AddRange("a", values);

        Assert.That(_map.Get("a"), Is.EquivalentTo(new[] { 1, 2, 3 }));
        Assert.That(_map.Count, Is.EqualTo(3));
    }

    // ── Equals branch coverage ───────────────────────────────

    [Test]
    public void Equals_DifferentKeyCount_ReturnsFalse()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);

        var other = new ConcurrentMultiMap<string, int>();
        other.Add("a", 1);

        Assert.That(_map.Equals(other), Is.False);
    }

    [Test]
    public void Equals_SameKeyCountDifferentKeys_ReturnsFalse()
    {
        _map.Add("a", 1);

        var other = new ConcurrentMultiMap<string, int>();
        other.Add("b", 1);

        Assert.That(_map.Equals(other), Is.False);
    }

    [Test]
    public void Equals_SameKeysDifferentValues_ReturnsFalse()
    {
        _map.Add("a", 1);

        var other = new ConcurrentMultiMap<string, int>();
        other.Add("a", 2);

        Assert.That(_map.Equals(other), Is.False);
    }

    // ── Verify-after-lock stale hashset coverage ─────────────

    [Test]
    [Category("Stress")]
    public void Remove_ConcurrentWithRemoveKeyAndReAdd_HandlesStaleHashset()
    {
        const int iterations = 5000;

        for (int i = 0; i < iterations; i++)
        {
            _map.Add("a", 1);
            _map.Add("a", 2);

            using var barrier = new Barrier(2);

            var t1 = Task.Run(() =>
            {
                barrier.SignalAndWait();
                _map.Remove("a", 1);
            });

            var t2 = Task.Run(() =>
            {
                barrier.SignalAndWait();
                _map.RemoveKey("a");
                _map.Add("a", 3);
            });

            Task.WaitAll(t1, t2);

            Assert.That(_map.Count, Is.GreaterThanOrEqualTo(0));

            int verifyCount = 0;
            foreach (var key in _map.Keys)
                verifyCount += _map.GetOrDefault(key).Count();

            Assert.That(_map.Count, Is.EqualTo(verifyCount));
            _map.Clear();
        }
    }

    [Test]
    [Category("Stress")]
    public void RemoveWhere_ConcurrentWithRemoveKeyAndReAdd_HandlesStaleHashset()
    {
        const int iterations = 5000;

        for (int i = 0; i < iterations; i++)
        {
            _map.Add("a", 1);
            _map.Add("a", 2);

            using var barrier = new Barrier(2);

            var t1 = Task.Run(() =>
            {
                barrier.SignalAndWait();
                _map.RemoveWhere("a", v => v == 1);
            });

            var t2 = Task.Run(() =>
            {
                barrier.SignalAndWait();
                _map.RemoveKey("a");
                _map.Add("a", 3);
            });

            Task.WaitAll(t1, t2);

            Assert.That(_map.Count, Is.GreaterThanOrEqualTo(0));

            int verifyCount = 0;
            foreach (var key in _map.Keys)
                verifyCount += _map.GetOrDefault(key).Count();

            Assert.That(_map.Count, Is.EqualTo(verifyCount));
            _map.Clear();
        }
    }

    [Test]
    [Category("Stress")]
    public void AddRange_ConcurrentWithRemoveKey_HandlesStaleHashset()
    {
        const int iterations = 5000;

        for (int i = 0; i < iterations; i++)
        {
            _map.Add("a", 100);

            using var barrier = new Barrier(2);

            var t1 = Task.Run(() =>
            {
                barrier.SignalAndWait();
                _map.AddRange("a", new[] { i * 2, i * 2 + 1 });
            });

            var t2 = Task.Run(() =>
            {
                barrier.SignalAndWait();
                _map.RemoveKey("a");
            });

            Task.WaitAll(t1, t2);

            Assert.That(_map.Count, Is.GreaterThanOrEqualTo(0));

            int verifyCount = 0;
            foreach (var key in _map.Keys)
                verifyCount += _map.GetOrDefault(key).Count();

            Assert.That(_map.Count, Is.EqualTo(verifyCount));
            _map.Clear();
        }
    }
}

