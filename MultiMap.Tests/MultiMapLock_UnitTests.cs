using MultiMap.Entities;
using System.Reflection;

namespace MultiMap.Tests;

[TestFixture]
public class MultiMapLockTests
{
    private MultiMapLock<string, int> _map;

    [SetUp]
    public void SetUp()
    {
        _map = new MultiMapLock<string, int>();
    }

    [TearDown]
    public void TearDown()
    {
        _map.Dispose();
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
    public void GetOrDefault_ReturnsSnapshot_NotLiveCollection()
    {
        _map.Add("a", 1);

        var snapshot = _map.GetOrDefault("a");
        _map.Add("a", 2);

        Assert.That(snapshot, Is.EquivalentTo(new[] { 1 }));
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
    public void GetEnumerator_ReturnsSnapshot_NotAffectedByModification()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);

        var enumerator = _map.GetEnumerator();
        _map.Add("a", 3);

        var items = new List<KeyValuePair<string, int>>();
        while (enumerator.MoveNext())
            items.Add(enumerator.Current);

        Assert.That(items, Has.Count.EqualTo(2));
    }

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        _map.Dispose();
        Assert.DoesNotThrow(() => _map.Dispose());
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
    public void Keys_ReturnsSnapshot_NotLiveCollection()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);

        var keys = _map.Keys;
        _map.Add("c", 3);

        Assert.That(keys, Is.EquivalentTo(new[] { "a", "b" }));
    }

    [Test]
    public void Equals_SameInstance_ReturnsTrue()
    {
        Assert.That(_map.Equals(_map), Is.True);
    }

    [Test]
    public void Equals_DifferentInstanceSameContent_ReturnsTrue()
    {
        var other = new MultiMapLock<string, int>();
        _map.Add("a", 1);
        other.Add("a", 1);

        Assert.That(_map.Equals(other), Is.True);

        other.Dispose();
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
        var other = new MultiMapLock<string, int>();
        _map.Add("a", 1);
        other.Add("a", 1);

        Assert.That(_map.GetHashCode(), Is.EqualTo(other.GetHashCode()));

        other.Dispose();
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
    public void Dispose_DoesNotThrow()
    {
        var map = new MultiMapLock<string, int>();
        Assert.DoesNotThrow(() => map.Dispose());
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

    [Test]
    [Category("Stress")]
    public void Stress_EnumeratorSnapshot_CountConsistency()
    {
        const int snapshotCount = 30;
        const int mutationsPerCycle = 20;
        using var cts = new CancellationTokenSource();

        var mutationTask = Task.Run(() =>
        {
            int value = 0;
            while (!cts.IsCancellationRequested)
            {
                for (int i = 0; i < mutationsPerCycle; i++)
                    _map.Add($"key{i % 5}", value++);

                for (int i = 0; i < mutationsPerCycle; i++)
                    _map.Remove($"key{i % 5}", value - mutationsPerCycle + i);

                _map.AddRange("bulk", Enumerable.Range(value, 10));
                _map.RemoveKey("bulk");
            }
        });

        for (int snapshot = 0; snapshot < snapshotCount; snapshot++)
        {
            var items = _map.ToList();

            Assert.That(items.Count, Is.GreaterThanOrEqualTo(0),
                $"Snapshot {snapshot}: negative item count");

            var distinctPairs = items.Distinct().ToList();
            Assert.That(distinctPairs.Count, Is.EqualTo(items.Count),
                $"Snapshot {snapshot}: snapshot contains duplicate pairs");
        }

        cts.Cancel();

        try { mutationTask.Wait(); }
        catch (AggregateException) { }

        int finalCount = _map.Count;
        Assert.That(finalCount, Is.GreaterThanOrEqualTo(0));

        int verifyCount = 0;
        foreach (var key in _map.Keys)
            verifyCount += _map.GetOrDefault(key).Count();

        Assert.That(finalCount, Is.EqualTo(verifyCount),
            "Final Count does not match sum of per-key values");
    }

    // ────────────────────────────────────────────────────────────────────
    // Tests for newly added interface members
    // ────────────────────────────────────────────────────────────────────

    #region KeyCount Property Tests

    [Test]
    public void KeyCount_EmptyMap_ReturnsZero()
    {
        Assert.That(_map.KeyCount, Is.EqualTo(0));
    }

    [Test]
    public void KeyCount_AfterAddingSingleKey_ReturnsOne()
    {
        _map.Add("a", 1);

        Assert.That(_map.KeyCount, Is.EqualTo(1));
    }

    [Test]
    public void KeyCount_AfterAddingMultipleKeys_ReturnsCorrectCount()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);
        _map.Add("c", 3);

        Assert.That(_map.KeyCount, Is.EqualTo(3));
    }

    [Test]
    public void KeyCount_AfterAddingMultipleValuesToSameKey_ReturnsOne()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("a", 3);

        Assert.That(_map.KeyCount, Is.EqualTo(1));
    }

    [Test]
    public void KeyCount_AfterRemovingKey_DecreasesCorrectly()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);
        _map.Add("c", 3);

        _map.RemoveKey("b");

        Assert.That(_map.KeyCount, Is.EqualTo(2));
    }

    [Test]
    [Category("Concurrency")]
    public void KeyCount_ConcurrentAdditions_IsCorrect()
    {
        const int threadCount = 10;
        const int keysPerThread = 20;

        Parallel.For(0, threadCount, i =>
        {
            for (int k = 0; k < keysPerThread; k++)
            {
                _map.Add($"thread{i}_key{k}", i * 100 + k);
            }
        });

        Assert.That(_map.KeyCount, Is.EqualTo(threadCount * keysPerThread));
    }

    #endregion

    #region Values Property Tests

    [Test]
    public void Values_EmptyMap_ReturnsEmptyCollection()
    {
        Assert.That(_map.Values, Is.Empty);
    }

    [Test]
    public void Values_SingleKey_ReturnsAllValues()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("a", 3);

        var values = _map.Values.ToArray();

        Assert.That(values, Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void Values_MultipleKeys_ReturnsAllValuesFromAllKeys()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("b", 10);
        _map.Add("b", 20);

        var values = _map.Values.ToArray();

        Assert.That(values, Is.EquivalentTo(new[] { 1, 2, 10, 20 }));
    }

    [Test]
    public void Values_AfterRemoval_ReflectsChanges()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("b", 10);

        _map.Remove("a", 1);

        var values = _map.Values.ToArray();

        Assert.That(values, Is.EquivalentTo(new[] { 2, 10 }));
    }

    #endregion

    #region GetValuesCount Method Tests

    [Test]
    public void GetValuesCount_EmptyMap_ReturnsZero()
    {
        Assert.That(_map.GetValuesCount("a"), Is.EqualTo(0));
    }

    [Test]
    public void GetValuesCount_SingleValue_ReturnsOne()
    {
        _map.Add("a", 1);

        Assert.That(_map.GetValuesCount("a"), Is.EqualTo(1));
    }

    [Test]
    public void GetValuesCount_MultipleValues_ReturnsCorrectCount()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("a", 3);

        Assert.That(_map.GetValuesCount("a"), Is.EqualTo(3));
    }

    [Test]
    public void GetValuesCount_NonExistentKey_ReturnsZero()
    {
        _map.Add("a", 1);

        Assert.That(_map.GetValuesCount("b"), Is.EqualTo(0));
    }

    #endregion

    #region Indexer Tests

    [Test]
    public void Indexer_ExistingKey_ReturnsValues()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);

        var values = _map["a"];

        Assert.That(values, Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public void Indexer_NonExistentKey_ThrowsKeyNotFoundException()
    {
        Assert.Throws<KeyNotFoundException>(() =>
        {
            var _ = _map["nonexistent"];
        });
    }

    [Test]
    public void Indexer_AfterRemoval_ReflectsChanges()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("a", 3);

        _map.Remove("a", 2);

        var values = _map["a"];

        Assert.That(values, Is.EquivalentTo(new[] { 1, 3 }));
    }

    #endregion

    #region AddRange with KeyValuePair Tests

    [Test]
    public void AddRangeKeyValuePairs_EmptyCollection_DoesNotChangeMap()
    {
        var items = Enumerable.Empty<KeyValuePair<string, int>>();

        _map.AddRange(items);

        Assert.That(_map.Count, Is.EqualTo(0));
        Assert.That(_map.KeyCount, Is.EqualTo(0));
    }

    [Test]
    public void AddRangeKeyValuePairs_SinglePair_AddsSuccessfully()
    {
        var items = new[] { new KeyValuePair<string, int>("a", 1) };

        _map.AddRange(items);

        Assert.That(_map.Contains("a", 1), Is.True);
        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void AddRangeKeyValuePairs_MultiplePairs_AddsAll()
    {
        var items = new[]
        {
            new KeyValuePair<string, int>("a", 1),
            new KeyValuePair<string, int>("a", 2),
            new KeyValuePair<string, int>("b", 10)
        };

        _map.AddRange(items);

        Assert.That(_map.Count, Is.EqualTo(3));
        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1, 2 }));
        Assert.That(_map.GetOrDefault("b"), Is.EquivalentTo(new[] { 10 }));
    }

    [Test]
    public void AddRangeKeyValuePairs_DuplicatePairs_IgnoresDuplicates()
    {
        var items = new[]
        {
            new KeyValuePair<string, int>("a", 1),
            new KeyValuePair<string, int>("a", 1),
            new KeyValuePair<string, int>("a", 1)
        };

        _map.AddRange(items);

        Assert.That(_map.Count, Is.EqualTo(1));
        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void AddRangeKeyValuePairs_ToExistingMap_AppendsCorrectly()
    {
        _map.Add("a", 1);

        var items = new[]
        {
            new KeyValuePair<string, int>("a", 2),
            new KeyValuePair<string, int>("b", 10)
        };

        _map.AddRange(items);

        Assert.That(_map.Count, Is.EqualTo(3));
        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    [Category("Concurrency")]
    public void AddRangeKeyValuePairs_ConcurrentCalls_AllPairsAdded()
    {
        const int threadCount = 10;
        const int pairsPerThread = 20;

        Parallel.For(0, threadCount, i =>
        {
            var pairs = Enumerable.Range(0, pairsPerThread)
                .Select(k => new KeyValuePair<string, int>($"thread{i}_key{k}", i * 100 + k))
                .ToArray();

            _map.AddRange(pairs);
        });

        Assert.That(_map.Count, Is.EqualTo(threadCount * pairsPerThread));
    }

    #endregion

    #region RemoveRange Tests

    [Test]
    public void RemoveRange_EmptyCollection_DoesNotChangeMap()
    {
        _map.Add("a", 1);
        var items = Enumerable.Empty<KeyValuePair<string, int>>();

        int removed = _map.RemoveRange(items);

        Assert.That(removed, Is.EqualTo(0));
        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void RemoveRange_SinglePair_RemovesSuccessfully()
    {
        _map.Add("a", 1);
        var items = new[] { new KeyValuePair<string, int>("a", 1) };

        int removed = _map.RemoveRange(items);

        Assert.That(removed, Is.EqualTo(1));
        Assert.That(_map.Count, Is.EqualTo(0));
    }

    [Test]
    public void RemoveRange_MultiplePairs_RemovesAll()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("b", 10);

        var items = new[]
        {
            new KeyValuePair<string, int>("a", 1),
            new KeyValuePair<string, int>("b", 10)
        };

        int removed = _map.RemoveRange(items);

        Assert.That(removed, Is.EqualTo(2));
        Assert.That(_map.Count, Is.EqualTo(1));
        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 2 }));
    }

    [Test]
    public void RemoveRange_NonExistentPairs_ReturnsZero()
    {
        _map.Add("a", 1);

        var items = new[]
        {
            new KeyValuePair<string, int>("b", 10),
            new KeyValuePair<string, int>("c", 20)
        };

        int removed = _map.RemoveRange(items);

        Assert.That(removed, Is.EqualTo(0));
        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void RemoveRange_MixedExistentAndNonExistent_RemovesOnlyExistent()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);

        var items = new[]
        {
            new KeyValuePair<string, int>("a", 1),
            new KeyValuePair<string, int>("a", 999),
            new KeyValuePair<string, int>("b", 10)
        };

        int removed = _map.RemoveRange(items);

        Assert.That(removed, Is.EqualTo(1));
        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void RemoveRange_RemovesLastValueForKey_RemovesKey()
    {
        _map.Add("a", 1);

        var items = new[] { new KeyValuePair<string, int>("a", 1) };

        _map.RemoveRange(items);

        Assert.That(_map.ContainsKey("a"), Is.False);
        Assert.That(_map.KeyCount, Is.EqualTo(0));
    }

    #endregion

    #region RemoveWhere Tests

    [Test]
    public void RemoveWhere_EmptyMap_ReturnsZero()
    {
        int removed = _map.RemoveWhere("a", v => v > 5);

        Assert.That(removed, Is.EqualTo(0));
    }

    [Test]
    public void RemoveWhere_NoMatchingValues_ReturnsZero()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("a", 3);

        int removed = _map.RemoveWhere("a", v => v > 10);

        Assert.That(removed, Is.EqualTo(0));
        Assert.That(_map.Count, Is.EqualTo(3));
    }

    [Test]
    public void RemoveWhere_SomeMatchingValues_RemovesOnlyMatching()
    {
        _map.Add("a", 1);
        _map.Add("a", 5);
        _map.Add("a", 10);

        int removed = _map.RemoveWhere("a", v => v > 5);

        Assert.That(removed, Is.EqualTo(1));
        Assert.That(_map.Count, Is.EqualTo(2));
        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1, 5 }));
    }

    [Test]
    public void RemoveWhere_AllValuesMatch_RemovesAllAndKey()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("a", 3);

        int removed = _map.RemoveWhere("a", v => v > 0);

        Assert.That(removed, Is.EqualTo(3));
        Assert.That(_map.Count, Is.EqualTo(0));
        Assert.That(_map.ContainsKey("a"), Is.False);
    }

    [Test]
    public void RemoveWhere_NonExistentKey_ReturnsZero()
    {
        _map.Add("a", 1);

        int removed = _map.RemoveWhere("b", v => v > 0);

        Assert.That(removed, Is.EqualTo(0));
        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void RemoveWhere_ComplexPredicate_RemovesCorrectly()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("a", 3);
        _map.Add("a", 4);
        _map.Add("a", 5);

        int removed = _map.RemoveWhere("a", v => v % 2 == 0);

        Assert.That(removed, Is.EqualTo(2));
        Assert.That(_map.Count, Is.EqualTo(3));
        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1, 3, 5 }));
    }

    [Test]
    [Category("Concurrency")]
    public void RemoveWhere_ConcurrentCalls_ThreadSafe()
    {
        const int threadCount = 10;
        const int valuesPerThread = 20;

        for (int i = 0; i < threadCount; i++)
        {
            for (int k = 0; k < valuesPerThread; k++)
            {
                _map.Add($"key{i}", i * 100 + k);
            }
        }

        int totalRemoved = 0;

        Parallel.For(0, threadCount, i =>
        {
            int removed = _map.RemoveWhere($"key{i}", v => v % 2 == 0);
            Interlocked.Add(ref totalRemoved, removed);
        });

        Assert.That(totalRemoved, Is.EqualTo(threadCount * (valuesPerThread / 2)));
        Assert.That(_map.Count, Is.EqualTo(threadCount * (valuesPerThread / 2)));
    }

    #endregion

    #region Dispose Guard Tests

    [Test]
    public void Add_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _map.Add("a", 1));
    }

    [Test]
    public void AddRange_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _map.AddRange("a", new[] { 1, 2 }));
    }

    [Test]
    public void Get_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _map.Get("a"));
    }

    [Test]
    public void GetOrDefault_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _map.GetOrDefault("a"));
    }

    [Test]
    public void TryGet_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _map.TryGet("a", out _));
    }

    [Test]
    public void Remove_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _map.Remove("a", 1));
    }

    [Test]
    public void RemoveKey_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _map.RemoveKey("a"));
    }

    [Test]
    public void ContainsKey_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _map.ContainsKey("a"));
    }

    [Test]
    public void Contains_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _map.Contains("a", 1));
    }

    [Test]
    public void Count_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.Throws<ObjectDisposedException>(() => { var _ = _map.Count; });
    }

    [Test]
    public void Keys_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.Throws<ObjectDisposedException>(() => { var _ = _map.Keys; });
    }

    [Test]
    public void GetEnumerator_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _map.GetEnumerator());
    }

    [Test]
    public void Clear_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _map.Clear());
    }

    #endregion

    [Test]
    public void Constructor_WithCapacity_WorksCorrectly()
    {
        using var map = new MultiMapLock<string, int>(100);
        map.Add("a", 1);

        Assert.That(map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void Constructor_WithValueComparer_UsesCaseInsensitiveComparison()
    {
        using var map = new MultiMapLock<string, string>(valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("key", "Hello");
        bool added = map.Add("key", "hello");

        Assert.That(added, Is.False);
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Constructor_WithCapacityAndValueComparer_WorksCorrectly()
    {
        using var map = new MultiMapLock<string, string>(100, valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("key", "Hello");
        bool added = map.Add("key", "hello");

        Assert.That(added, Is.False);
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Equals_DifferentValueCount_SameKeys_ReturnsFalse()
    {
        var other = new MultiMapLock<string, int>();
        _map.Add("a", 1);
        _map.Add("a", 2);
        other.Add("a", 1);

        Assert.That(_map.Equals(other), Is.False);

        other.Dispose();
    }

    [Test]
    public void Equals_DifferentKeys_ReturnsFalse()
    {
        var other = new MultiMapLock<string, int>();
        _map.Add("a", 1);
        other.Add("b", 1);

        Assert.That(_map.Equals(other), Is.False);

        other.Dispose();
    }

    [Test]
    public void Equals_SameKeysDifferentValues_ReturnsFalse()
    {
        var other = new MultiMapLock<string, int>();
        _map.Add("a", 1);
        other.Add("a", 2);

        Assert.That(_map.Equals(other), Is.False);

        other.Dispose();
    }

    [Test]
    public void Add_WithCaseInsensitiveComparer_TreatsSameCaseAsDuplicate()
    {
        using var map = new MultiMapLock<string, string>(valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("key", "ABC");
        map.Add("key", "abc");
        map.Add("key", "Abc");

        Assert.That(map.Count, Is.EqualTo(1));
        Assert.That(map.GetValuesCount("key"), Is.EqualTo(1));
    }

    [Test]
    public void Contains_WithCaseInsensitiveComparer_FindsValueIgnoringCase()
    {
        using var map = new MultiMapLock<string, string>(valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("key", "Hello");

        Assert.That(map.Contains("key", "hello"), Is.True);
        Assert.That(map.Contains("key", "HELLO"), Is.True);
    }

    [Test]
    public void AddRange_WithValueComparer_RespectsComparer()
    {
        using var map = new MultiMapLock<string, string>(valueComparer: StringComparer.OrdinalIgnoreCase);
        map.AddRange("key", new[] { "Hello", "hello", "HELLO" });

        Assert.That(map.Count, Is.EqualTo(1));
        Assert.That(map.GetValuesCount("key"), Is.EqualTo(1));
    }

    [Test]
    public void Remove_WithCaseInsensitiveComparer_RemovesIgnoringCase()
    {
        using var map = new MultiMapLock<string, string>(valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("key", "Hello");
        bool removed = map.Remove("key", "hello");

        Assert.That(removed, Is.True);
        Assert.That(map.Count, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_WithKeyComparer_UsesCaseInsensitiveKeyComparison()
    {
        using var map = new MultiMapLock<string, int>(keyComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("Key", 1);
        map.Add("key", 2);

        Assert.That(map.KeyCount, Is.EqualTo(1));
        Assert.That(map.Get("KEY"), Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public void Constructor_WithCapacityAndKeyComparer_UsesCaseInsensitiveKeyComparison()
    {
        using var map = new MultiMapLock<string, int>(100, keyComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("Key", 1);
        map.Add("key", 2);

        Assert.That(map.KeyCount, Is.EqualTo(1));
        Assert.That(map.Get("KEY"), Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public void Constructor_WithCapacityKeyComparerAndValueComparer_WorksCorrectly()
    {
        using var map = new MultiMapLock<string, string>(100, StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);
        map.Add("Key", "Hello");
        map.Add("key", "hello");

        Assert.That(map.KeyCount, Is.EqualTo(1));
        Assert.That(map.Count, Is.EqualTo(1));
        Assert.That(map.Get("KEY"), Is.EquivalentTo(new[] { "Hello" }));
    }
}
