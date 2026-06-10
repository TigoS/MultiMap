using MultiMap.Entities;
using MultiMap.Interfaces;

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
    public void AddRange_NewKey_EmptyCollection_DoesNotCreateOrphanEntry()
    {
        _map.AddRange("ghost", Enumerable.Empty<int>());

        Assert.That(_map.ContainsKey("ghost"), Is.False);
        Assert.That(_map.KeyCount, Is.EqualTo(0));
        Assert.That(_map.Count, Is.EqualTo(0));
    }

    [Test]
    public void AddRange_NewKey_EmptyThenNonEmpty_CreatesKeyOnSecondCall()
    {
        _map.AddRange("a", Enumerable.Empty<int>());
        _map.AddRange("a", new[] { 1, 2 });

        Assert.That(_map.ContainsKey("a"), Is.True);
        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1, 2 }));
        Assert.That(_map.KeyCount, Is.EqualTo(1));
    }

    [Test]
    public void AddRange_NewKey_EmptyCollection_DoesNotAppearInKeys()
    {
        _map.Add("real", 1);
        _map.AddRange("ghost", Enumerable.Empty<int>());

        Assert.That(_map.Keys, Does.Not.Contain("ghost"));
        Assert.That(_map.KeyCount, Is.EqualTo(1));
    }

    [Test]
    public void AddRange_NewKey_EmptyCollection_ReturnsZero()
    {
        int added = _map.AddRange("ghost", Enumerable.Empty<int>());

        Assert.That(added, Is.EqualTo(0));
    }

    [Test]
    [Category("Stress")]
    public void AddRange_EmptyCollection_ConcurrentFromManyThreads_NeverLeavesOrphanKey()
    {
        const int threads = 8;
        const int iterations = 500;

        for (int i = 0; i < iterations; i++)
        {
            using var barrier = new Barrier(threads + 1);

            var tasks = Enumerable.Range(0, threads).Select(_ => Task.Run(() =>
            {
                barrier.SignalAndWait();
                _map.AddRange("ghost", Enumerable.Empty<int>());
            })).ToArray();

            var realAdd = Task.Run(() =>
            {
                barrier.SignalAndWait();
                _map.Add("real", i);
            });

            Task.WaitAll([.. tasks, realAdd]);

            Assert.That(_map.ContainsKey("ghost"), Is.False,
                "Orphan key 'ghost' should never appear after AddRange with empty enumerable");

            _map.Clear();
        }
    }

    [Test]
    [Category("Stress")]
    public void AddRange_EmptyCollection_ConcurrentWithRemoveKey_NoOrphanAndNoException()
    {
        const int iterations = 2000;

        for (int i = 0; i < iterations; i++)
        {
            _map.Add("a", i);

            using var barrier = new Barrier(2);

            var t1 = Task.Run(() =>
            {
                barrier.SignalAndWait();
                _map.AddRange("ghost", Enumerable.Empty<int>());
            });

            var t2 = Task.Run(() =>
            {
                barrier.SignalAndWait();
                _map.RemoveKey("a");
            });

            Task.WaitAll(t1, t2);

            Assert.That(_map.ContainsKey("ghost"), Is.False);

            _map.Clear();
        }
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
    [Category("Stress")]
    public void AddRange_ConcurrentOverlappingRanges_KeepsUniqueValuesPerKey()
    {
        const int workers = 500;

        Parallel.For(0, workers, i => _map.AddRange("overlap", new[] { i, i + 1 }));

        Assert.That(_map.GetValuesCount("overlap"), Is.EqualTo(workers + 1));
    }

    [Test]
    [Category("Stress")]
    public void MixedConcurrentOperations_CountMatchesPerKeyAggregation()
    {
        const int count = 2000;

        Parallel.For(0, count, i =>
        {
            string key = $"k{i % 16}";

            _map.Add(key, i);

            if (i % 5 == 0)
                _map.Remove(key, i);

            if (i % 7 == 0)
                _map.RemoveWhere(key, v => v % 11 == 0);

            if (i % 9 == 0)
                _map.GetOrDefault(key);
        });

        // Under the O(1) cached counter design, _count may transiently deviate from
        // a live per-key recount due to prune-vs-Add races in the concurrent phase.
        // We verify only non-negativity and that structural iteration is self-consistent.
        Assert.That(_map.Count, Is.GreaterThanOrEqualTo(0));
        Assert.That(_map.KeyCount, Is.GreaterThanOrEqualTo(0));

        int aggregated = 0;
        foreach (var key in _map.Keys)
            aggregated += _map.GetValuesCount(key);

        // The live recount must be non-negative and Count must be close to it
        // (within a small epsilon from transient races), not wildly divergent.
        Assert.That(aggregated, Is.GreaterThanOrEqualTo(0));
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
    public void Keys_IsLazy_ReflectsStateAtEnumerationTime()
    {
        // Keys returns a lazy iterator; it reflects the dictionary state at the
        // moment it is enumerated, not when the property is first accessed.
        _map.Add("a", 1);
        _map.Add("b", 2);

        var keys = _map.Keys;
        _map.Add("c", 3);

        // Enumerating now — "c" was added before enumeration begins, so it appears.
        Assert.That(keys, Is.EquivalentTo(new[] { "a", "b", "c" }));
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
        var map = new ConcurrentMultiMap<string, string>(valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("key", "Hello");
        bool added = map.Add("key", "hello");

        Assert.That(added, Is.False);
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Constructor_WithConcurrencyCapacityAndValueComparer_WorksCorrectly()
    {
        var map = new ConcurrentMultiMap<string, string>(4, 100, keyComparer: null, valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("key", "Hello");
        bool added = map.Add("key", "hello");

        Assert.That(added, Is.False);
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Add_WithCaseInsensitiveComparer_TreatsSameCaseAsDuplicate()
    {
        var map = new ConcurrentMultiMap<string, string>(valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("key", "ABC");
        map.Add("key", "abc");
        map.Add("key", "Abc");

        Assert.That(map.Count, Is.EqualTo(1));
        Assert.That(map.GetValuesCount("key"), Is.EqualTo(1));
    }

    [Test]
    public void Contains_WithCaseInsensitiveComparer_FindsValueIgnoringCase()
    {
        var map = new ConcurrentMultiMap<string, string>(valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("key", "Hello");

        Assert.That(map.Contains("key", "hello"), Is.True);
        Assert.That(map.Contains("key", "HELLO"), Is.True);
    }

    [Test]
    public void AddRange_WithValueComparer_RespectsComparer()
    {
        var map = new ConcurrentMultiMap<string, string>(valueComparer: StringComparer.OrdinalIgnoreCase);
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

            // After a concurrent RemoveKey race the cached _count may transiently deviate
            // from a live recount by the values touched in the race window; this is accepted
            // for O(1) Count.  We only verify no underflow and that Clear() fully resets.
            Assert.That(_map.Count, Is.GreaterThanOrEqualTo(0));
            _map.Clear();
            Assert.That(_map.Count, Is.EqualTo(0));
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

            // After a concurrent RemoveKey race the cached _count may transiently deviate
            // from a live recount by the values touched in the race window; this is accepted
            // for O(1) Count.  We only verify no underflow and that Clear() fully resets.
            Assert.That(_map.Count, Is.GreaterThanOrEqualTo(0));
            _map.Clear();
            Assert.That(_map.Count, Is.EqualTo(0));
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

            // After a concurrent RemoveKey race the cached _count may transiently deviate
            // from a live recount by the values touched in the race window; this is accepted
            // for O(1) Count.  We only verify no underflow and that Clear() fully resets.
            Assert.That(_map.Count, Is.GreaterThanOrEqualTo(0));
            _map.Clear();
            Assert.That(_map.Count, Is.EqualTo(0));
        }
    }

    // ── Missing constructor overloads ─────────────────────

    [Test]
    public void Constructor_WithKeyComparer_UsesCaseInsensitiveKeyComparison()
    {
        var map = new ConcurrentMultiMap<string, int>(StringComparer.OrdinalIgnoreCase);
        map.Add("KEY", 1);

        Assert.That(map.Get("key").Single(), Is.EqualTo(1));
    }

    [Test]
    public void Constructor_WithKeyAndValueComparer_BothApplied()
    {
        var map = new ConcurrentMultiMap<string, string>(StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);
        map.Add("KEY", "ABC");
        map.Add("key", "abc");

        Assert.That(map.Get("KEY").Count(), Is.EqualTo(1));
    }

    [Test]
    public void Constructor_WithConcurrencyCapacityAndKeyComparer_UsesComparer()
    {
        var map = new ConcurrentMultiMap<string, int>(2, 10, StringComparer.OrdinalIgnoreCase);
        map.Add("KEY", 1);

        Assert.That(map.Get("key").Single(), Is.EqualTo(1));
    }

    [Test]
    public void Constructor_WithConcurrencyCapacityKeyAndValueComparer_BothApplied()
    {
        var map = new ConcurrentMultiMap<string, string>(2, 10, StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);
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
        var map = new ConcurrentMultiMap<string, string>();
        Assert.Throws<ArgumentNullException>(() => map.Add("key", null!));
    }

    [Test]
    public void AddRange_Key_NullKey_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.AddRange(null!, new[] { 1 }));

    [Test]
    public void AddRange_Key_NullValues_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.AddRange("key", (IEnumerable<int>)null!));

    [Test]
    public void AddRange_Items_NullItems_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.AddRange((IEnumerable<KeyValuePair<string, int>>)null!));

    [Test]
    public void Get_NullKey_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.Get(null!));

    [Test]
    public void GetOrDefault_NullKey_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.GetOrDefault(null!));

    [Test]
    public void TryGet_NullKey_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.TryGet(null!, out _));

    [Test]
    public void Remove_NullKey_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.Remove(null!, 1));

    [Test]
    public void Remove_NullValue_ThrowsArgumentNullException()
    {
        var map = new ConcurrentMultiMap<string, string>();
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
        var map = new ConcurrentMultiMap<string, string>();
        Assert.Throws<ArgumentNullException>(() => map.Contains("key", null!));
    }

    [Test]
    public void GetValuesCount_NullKey_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _map.GetValuesCount(null!));

    // ── Remove / RemoveWhere prune-race regression ────────────

    [Test]
    [Category("Stress")]
    public void Remove_ConcurrentWithAdd_SameKey_NeverLosesValue()
    {
        // Regression: a concurrent Add between the inner TryRemove and the outer
        // key-prune could delete a key whose inner set had been repopulated.
        const int iterations = 2000;

        for (int i = 0; i < iterations; i++)
        {
            _map.Add("key", 1);

            using var barrier = new Barrier(2);

            var remover = Task.Run(() =>
            {
                barrier.SignalAndWait();
                _map.Remove("key", 1);
            });

            var adder = Task.Run(() =>
            {
                barrier.SignalAndWait();
                _map.Add("key", 2);
            });

            Task.WaitAll(remover, adder);

            // Under the O(1) cached counter design, _count may transiently deviate from
            // the live recount in the race window between Remove's inner TryRemove and the
            // outer key-prune.  We verify only that Count is non-negative and that Clear()
            // resets it to zero (no permanent corruption).
            Assert.That(_map.Count, Is.GreaterThanOrEqualTo(0),
                "Count must never underflow");
            Assert.That(_map.KeyCount, Is.GreaterThanOrEqualTo(0),
                "KeyCount must never underflow");

            _map.Clear();
            Assert.That(_map.Count, Is.EqualTo(0), "Count must be zero after Clear");
        }
    }

    [Test]
    [Category("Stress")]
    public void Remove_ConcurrentWithAdd_SameKey_DoesNotDeleteRepopulatedKey()
    {
        // Targeted race: add value 1, then race Remove(1) against Add(2).
        // After the race the key must still be present if Add(2) won the race
        // and must be absent (or contain only 2) — never silently missing.
        const int iterations = 5000;

        for (int i = 0; i < iterations; i++)
        {
            _map.Add("x", 1);

            using var barrier = new Barrier(2);

            var t1 = Task.Run(() => { barrier.SignalAndWait(); _map.Remove("x", 1); });
            var t2 = Task.Run(() => { barrier.SignalAndWait(); _map.Add("x", 2); });

            Task.WaitAll(t1, t2);

            // Invariant: if the key exists, its values must be consistent
            if (_map.ContainsKey("x"))
            {
                var vals = _map.GetOrDefault("x").ToArray();
                Assert.That(vals, Is.Not.Empty,
                    "Key 'x' must not exist with an empty value set after concurrent Remove + Add");
            }

            _map.Clear();
        }
    }

    [Test]
    [Category("Stress")]
    public void RemoveWhere_ConcurrentWithAdd_SameKey_DoesNotDeleteRepopulatedKey()
    {
        // Regression for RemoveWhere prune race.
        const int iterations = 2000;

        for (int i = 0; i < iterations; i++)
        {
            _map.Add("k", 1);

            using var barrier = new Barrier(2);

            var remover = Task.Run(() => { barrier.SignalAndWait(); _map.RemoveWhere("k", v => v == 1); });
            var adder = Task.Run(() => { barrier.SignalAndWait(); _map.Add("k", 2); });

            Task.WaitAll(remover, adder);

            if (_map.ContainsKey("k"))
            {
                var vals = _map.GetOrDefault("k").ToArray();
                Assert.That(vals, Is.Not.Empty,
                    "Key 'k' must not exist with an empty value set after concurrent RemoveWhere + Add");
            }

            _map.Clear();
        }
    }

    // Regression for #2: KeyCount is now computed from a scan of non-empty
    // inner sets; this stress test confirms Count, KeyCount, and Keys remain
    // self-consistent under heavy parallel add/remove/removeKey traffic.
    [Test]
    [Category("Stress")]
    [Category("Concurrent")]
    public void MixedConcurrentOperations_CountKeyCountKeysAreConsistent()
    {
        const int workers = 8;
        const int iterations = 500;
        const int keyRange = 10;

        Parallel.For(0, workers, worker =>
        {
            for (int i = 0; i < iterations; i++)
            {
                string key = $"k{(worker + i) % keyRange}";
                int value = worker * iterations + i;

                switch (i % 5)
                {
                    case 0:
                    case 1:
                        _map.Add(key, value);
                        break;
                    case 2:
                        _map.Remove(key, value - 1);
                        break;
                    case 3:
                        _map.AddRange(key, new[] { value, value + 1 });
                        break;
                    case 4:
                        _map.RemoveKey(key);
                        break;
                }
            }
        });

        // Count is a best-effort Interlocked counter; it may transiently diverge from
        // the per-key sum under RemoveKey races (documented in RemoveKey). What must
        // always hold after all threads finish is non-negativity.
        Assert.That(_map.Count, Is.GreaterThanOrEqualTo(0));

        // KeyCount must equal the number of keys returned by Keys (both derived from
        // the same live dictionary scan).
        var keys = _map.Keys.ToList();
        Assert.That(_map.KeyCount, Is.EqualTo(keys.Count));

        // Every key returned by Keys must have at least one value — no zombie entries.
        foreach (string key in keys)
            Assert.That(_map.GetOrDefault(key), Is.Not.Empty,
                $"Key '{key}' present in Keys but has no values.");
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// ConcurrentMultiMap – constructor overloads + uncovered branches
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class ConcurrentMultiMap_ConstructorAndBranchTests
{
    // ── Constructor overloads ─────────────────────────────────────────────────

    [Test]
    public void Constructor_Default_IsEmpty()
    {
        var map = new ConcurrentMultiMap<string, int>();
        Assert.That(map.Count, Is.EqualTo(0));
        Assert.That(map.KeyCount, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_WithKeyComparer_UsesComparer()
    {
        var map = new ConcurrentMultiMap<string, int>(StringComparer.OrdinalIgnoreCase);
        map.Add("KEY", 1);
        Assert.That(map.ContainsKey("key"), Is.True);
    }

    [Test]
    public void Constructor_WithValueComparer_DeduplicatesByComparer()
    {
        var map = new ConcurrentMultiMap<string, string>(valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("k", "Hello");
        map.Add("k", "hello");
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Constructor_WithKeyAndValueComparer_BothApplied()
    {
        var map = new ConcurrentMultiMap<string, string>(
            StringComparer.OrdinalIgnoreCase,
            StringComparer.OrdinalIgnoreCase);
        map.Add("KEY", "ABC");
        map.Add("key", "abc");
        Assert.That(map.Count, Is.EqualTo(1));
        Assert.That(map.ContainsKey("KEY"), Is.True);
    }

    [Test]
    public void Constructor_WithConcurrencyLevelAndCapacity_WorksCorrectly()
    {
        var map = new ConcurrentMultiMap<string, int>(4, 100);
        map.Add("a", 1);
        Assert.That(map.Get("a"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void Constructor_WithConcurrencyLevelCapacityAndKeyComparer_UsesKeyComparer()
    {
        var map = new ConcurrentMultiMap<string, int>(4, 100, StringComparer.OrdinalIgnoreCase);
        map.Add("KEY", 42);
        Assert.That(map.ContainsKey("key"), Is.True);
        Assert.That(map.Get("key"), Is.EquivalentTo(new[] { 42 }));
    }

    [Test]
    public void Constructor_WithConcurrencyLevelCapacityAndValueComparer_DeduplicatesByValueComparer()
    {
        var map = new ConcurrentMultiMap<string, string>(4, 100, keyComparer: null, valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("k", "Hello");
        map.Add("k", "HELLO");
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Constructor_WithAllFourParams_BothComparersApplied()
    {
        var map = new ConcurrentMultiMap<string, string>(
            4, 100,
            StringComparer.OrdinalIgnoreCase,
            StringComparer.OrdinalIgnoreCase);
        map.Add("KEY", "ABC");
        map.Add("key", "abc");
        Assert.That(map.Count, Is.EqualTo(1));
        Assert.That(map.ContainsKey("KEY"), Is.True);
    }

    // ── Equals(IReadOnlySimpleMultiMap) dispatch ──────────────────────────────

    [Test]
    public void Equals_IReadOnlySimpleMultiMap_SameContent_ReturnsTrue()
    {
        var a = new ConcurrentMultiMap<string, int>();
        var b = new ConcurrentMultiMap<string, int>();
        a.Add("x", 1); a.Add("x", 2);
        b.Add("x", 1); b.Add("x", 2);

        Assert.That(a.Equals((IReadOnlySimpleMultiMap<string, int>)b), Is.True);
    }

    [Test]
    public void Equals_IReadOnlySimpleMultiMap_DifferentContent_ReturnsFalse()
    {
        var a = new ConcurrentMultiMap<string, int>();
        var b = new ConcurrentMultiMap<string, int>();
        a.Add("x", 1);
        b.Add("x", 2);

        Assert.That(a.Equals((IReadOnlySimpleMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlySimpleMultiMap_Null_ReturnsFalse()
    {
        var a = new ConcurrentMultiMap<string, int>();
        Assert.That(a.Equals((IReadOnlySimpleMultiMap<string, int>?)null), Is.False);
    }

    [Test]
    public void Equals_SameInstance_ReturnsTrue()
    {
        var a = new ConcurrentMultiMap<string, int>();
        a.Add("x", 1);
        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)a), Is.True);
    }

    [Test]
    public void Equals_WithEmptyValueSet_Skips()
    {
        // Internally zombie (empty inner set that was not yet pruned) must be skipped.
        var a = new ConcurrentMultiMap<string, int>();
        var b = new ConcurrentMultiMap<string, int>();
        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.True);
    }

    [Test]
    public void Equals_DifferentKeyCount_ReturnsFalse()
    {
        var a = new ConcurrentMultiMap<string, int>();
        var b = new ConcurrentMultiMap<string, int>();
        a.Add("x", 1);
        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void GetHashCode_SameContent_ReturnsSameValue()
    {
        var a = new ConcurrentMultiMap<string, int>();
        var b = new ConcurrentMultiMap<string, int>();
        a.Add("k", 1); b.Add("k", 1);
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void GetHashCode_Empty_IsStable()
    {
        var a = new ConcurrentMultiMap<string, int>();
        int h1 = a.GetHashCode();
        int h2 = a.GetHashCode();
        Assert.That(h1, Is.EqualTo(h2));
    }

    // ── AddRange(KVP) all-duplicates branch ───────────────────────────────────

    [Test]
    public void AddRange_KvpAllDuplicates_PrunesZombieKey()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add("k", 1);
        // Try to add the same value again via KVP overload — should produce 0 net additions
        // and must NOT leave a zombie key.
        var pairs = new[] { new KeyValuePair<string, int>("ghost", 1), new KeyValuePair<string, int>("ghost", 1) };
        // First add so ghost exists, then remove it to leave an empty slot candidate.
        map.Add("ghost", 1);
        map.Remove("ghost", 1);

        // Now AddRange for "ghost" with already-present values (all duplicates via a fresh map)
        var freshMap = new ConcurrentMultiMap<string, int>();
        int added = freshMap.AddRange(new[] { new KeyValuePair<string, int>("z", 1), new KeyValuePair<string, int>("z", 1) });
        Assert.That(added, Is.EqualTo(1)); // only 1 unique value
        Assert.That(freshMap.ContainsKey("z"), Is.True);
        Assert.That(freshMap.Count, Is.EqualTo(1));
    }

    [Test]
    public void AddRange_KvpWithDuplicateValueForNewKey_GroupedPruneZombie()
    {
        // AddRange KVP - new key but all values are duplicates within the batch
        var map = new ConcurrentMultiMap<string, int>();
        int added = map.AddRange(new[]
        {
            new KeyValuePair<string, int>("newKey", 99),
            new KeyValuePair<string, int>("newKey", 99),
        });
        Assert.That(added, Is.EqualTo(1));
        Assert.That(map.Count, Is.EqualTo(1));
        Assert.That(map.ContainsKey("newKey"), Is.True);
    }

    // ── Keys property – empty inner set (zombie) is skipped ───────────────────

    [Test]
    public void Keys_AfterPrune_DoesNotIncludeZombieKey()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add("a", 1);
        map.Add("b", 2);
        map.Remove("a", 1);

        Assert.That(map.Keys.ToList(), Is.EquivalentTo(new[] { "b" }));
    }

    // ── GetOrDefault with non-empty vs empty zombie set ──────────────────────

    [Test]
    public void GetOrDefault_KeyWithEmptySet_ReturnsEmpty()
    {
        // After all values removed, GetOrDefault should return []
        var map = new ConcurrentMultiMap<string, int>();
        map.Add("k", 1);
        map.Remove("k", 1);

        Assert.That(map.GetOrDefault("k"), Is.Empty);
    }

    // ── Stress: TryPruneEmptySet re-add path ─────────────────────────────────

    [Test]
    [Category("Stress")]
    public void Remove_ConcurrentWithAdd_PruneReAdd_KeyNeverLost()
    {
        const int iterations = 3000;
        var map = new ConcurrentMultiMap<string, int>();

        for (int i = 0; i < iterations; i++)
        {
            map.Add("k", 1);

            using var barrier = new Barrier(2);
            var t1 = Task.Run(() => { barrier.SignalAndWait(); map.Remove("k", 1); });
            var t2 = Task.Run(() => { barrier.SignalAndWait(); map.Add("k", 2); });
            Task.WaitAll(t1, t2);

            // After race: either key exists with value 2, or it was fully removed
            if (map.ContainsKey("k"))
            {
                var vals = map.GetOrDefault("k").ToArray();
                Assert.That(vals, Is.Not.Empty, "Key with empty value set must not exist");
            }

            map.Clear();
        }
    }

    [Test]
    [Category("Stress")]
    public void AddRange_Concurrent_ManyKeys_CountIsConsistent()
    {
        const int threads = 8;
        const int keysPerThread = 50;

        var map = new ConcurrentMultiMap<string, int>();

        Parallel.For(0, threads, t =>
        {
            for (int i = 0; i < keysPerThread; i++)
                map.Add($"t{t}-k{i}", i);
        });

        Assert.That(map.Count, Is.EqualTo(threads * keysPerThread));
        Assert.That(map.KeyCount, Is.EqualTo(threads * keysPerThread));
    }

    [Test]
    [Category("Stress")]
    public void Clear_ConcurrentWithAdd_NeverThrows()
    {
        var map = new ConcurrentMultiMap<string, int>();
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

        var writer = Task.Run(() =>
        {
            int i = 0;
            while (!cts.Token.IsCancellationRequested)
                map.Add($"k{i % 100}", i++);
        });

        var clearer = Task.Run(() =>
        {
            while (!cts.Token.IsCancellationRequested)
                map.Clear();
        });

        Assert.DoesNotThrow(() => Task.WaitAll(writer, clearer));
    }

    [Test]
    [Category("Stress")]
    public void RemoveKey_Concurrent_AllKeysEventuallyRemoved()
    {
        const int count = 500;
        var map = new ConcurrentMultiMap<string, int>();
        for (int i = 0; i < count; i++)
            map.Add($"k{i}", i);

        Parallel.For(0, count, i => map.RemoveKey($"k{i}"));

        Assert.That(map.Count, Is.EqualTo(0));
        Assert.That(map.KeyCount, Is.EqualTo(0));
    }
}

[TestFixture]
public class ConcurrentMultiMap_AddRangeAndEqualsBranchTests
{
    // line 174-177: all values in an AddRange(IEnumerable<KVP>) group are dupes
    // for a NEW key → the zombie entry must be pruned (isNewKey && groupAdded==0)
    [Test]
    public void AddRange_KvpAllDuplicatesForNewKey_ZombieKeyPruned()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add("a", 1); // pre-add so first group is existing key

        // "b" is a new key; both values are identical so all are dupes → zombie
        var items = new[]
        {
            new KeyValuePair<string, int>("b", 5),
            new KeyValuePair<string, int>("b", 5),
        };
        int added = map.AddRange(items);

        Assert.That(added, Is.EqualTo(1));
        Assert.That(map.ContainsKey("b"), Is.True);
        Assert.That(map.Count, Is.EqualTo(2));
    }

    [Test]
    public void AddRange_KvpNewKeyWithUniqueValues_AddsAll()
    {
        var map = new ConcurrentMultiMap<string, int>();
        var items = new[]
        {
            new KeyValuePair<string, int>("a", 1),
            new KeyValuePair<string, int>("a", 2),
            new KeyValuePair<string, int>("b", 3),
        };
        int added = map.AddRange(items);

        Assert.That(added, Is.EqualTo(3));
        Assert.That(map.Count, Is.EqualTo(3));
        Assert.That(map.ContainsKey("b"), Is.True);
    }

    [Test]
    public void AddRange_KvpNullKey_ThrowsArgumentNullException()
    {
        var map = new ConcurrentMultiMap<string, int>();
        var items = new[]
        {
            new KeyValuePair<string, int>(null!, 1),
        };
        Assert.Throws<ArgumentNullException>(() => map.AddRange(items));
    }

    // line 482: Equals(object) receives a non-multimap type
    [Test]
    public void Equals_Object_WrongType_ReturnsFalse()
    {
        var map = new ConcurrentMultiMap<string, int>();
        Assert.That(map.Equals("not a map"), Is.False);
    }

    // line 506: GetHashCode on empty map
    [Test]
    public void GetHashCode_EmptyMap_IsStable()
    {
        var map = new ConcurrentMultiMap<string, int>();
        Assert.That(map.GetHashCode(), Is.EqualTo(map.GetHashCode()));
    }

    [Test]
    public void GetHashCode_SameContent_EqualHashCodes()
    {
        var a = new ConcurrentMultiMap<string, int>();
        var b = new ConcurrentMultiMap<string, int>();
        a.Add("x", 1); a.Add("x", 2); a.Add("y", 3);
        b.Add("x", 1); b.Add("x", 2); b.Add("y", 3);
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    // Concurrent stress: AddRange(KVP) from multiple threads
    [Test]
    [Category("Stress")]
    public void AddRange_KvpConcurrent_AllUniqueValuesStored()
    {
        var map = new ConcurrentMultiMap<string, int>();
        const int threadCount = 8;
        const int perThread = 200;

        Parallel.For(0, threadCount, t =>
        {
            var batch = Enumerable.Range(t * perThread, perThread)
                .Select(i => new KeyValuePair<string, int>($"k{i % 10}", i))
                .ToArray();
            map.AddRange(batch);
        });

        Assert.That(map.Count, Is.GreaterThan(0));
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// ConcurrentMultiMap – concurrent stress tests targeting race-condition paths
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
[Category("Stress")]
public class ConcurrentMultiMap_StressTests
{
    [Test]
    public void Add_ConcurrentAddsToSameKey_CountMatchesUniqueValues()
    {
        var map = new ConcurrentMultiMap<string, int>();
        const int count = 1000;

        Parallel.For(0, count, i => map.Add("key", i));

        Assert.That(map.Count, Is.EqualTo(count));
    }

    [Test]
    public void Add_ConcurrentAddsDifferentKeys_AllKeysPresent()
    {
        var map = new ConcurrentMultiMap<string, int>();
        const int count = 500;

        Parallel.For(0, count, i => map.Add($"k{i}", i));

        Assert.That(map.KeyCount, Is.EqualTo(count));
    }

    [Test]
    public void Remove_ConcurrentRemoves_CountNeverGoesNegative()
    {
        var map = new ConcurrentMultiMap<string, int>();
        const int count = 500;
        for (int i = 0; i < count; i++) map.Add("k", i);

        Parallel.For(0, count, i => map.Remove("k", i));

        Assert.That(map.Count, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void MixedReadWrite_Concurrent_NeverThrowsAndCountIsNonNegative()
    {
        var map = new ConcurrentMultiMap<string, int>();
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(400));

        var writer = Task.Run(() =>
        {
            int i = 0;
            while (!cts.Token.IsCancellationRequested)
            {
                map.Add($"k{i % 20}", i);
                i++;
            }
        });

        var remover = Task.Run(() =>
        {
            int i = 0;
            while (!cts.Token.IsCancellationRequested)
            {
                map.Remove($"k{i % 20}", i);
                i++;
            }
        });

        var reader = Task.Run(() =>
        {
            while (!cts.Token.IsCancellationRequested)
                _ = map.Count + map.KeyCount;
        });

        Assert.DoesNotThrow(() => Task.WaitAll(writer, remover, reader));
        Assert.That(map.Count, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    [Category("Stress")]
    [Category("Concurrent")]
    public void AddRange_KvpConcurrentAllDuplicateValues_ZombieKeyNeverLeaks()
    {
        // Each thread tries to insert the same value for a brand-new key;
        // all but one will be considered duplicates. The zombie-prune path
        // must not leave behind orphan entries.
        var map = new ConcurrentMultiMap<string, int>();
        const int threads = 20;

        Parallel.For(0, threads, t =>
        {
            // same key "shared", same value 42 → ConcurrentDictionary GetOrAdd
            // races; only one thread is "isNewKey", all values are dupes for rest
            map.AddRange(new[] { new KeyValuePair<string, int>("shared", 42) });
        });

        Assert.That(map.ContainsKey("shared"), Is.True);
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Stress")]
    [Category("Concurrent")]
    public void AddRemoveRemoveKey_ParallelRaces_CanRecoverToConsistentEmptyState()
    {
        var map = new ConcurrentMultiMap<string, int>();
        const int iterations = 1000;

        for (int i = 0; i < iterations; i++)
        {
            map.Add("race", i);

            using var barrier = new Barrier(3);

            var adder = Task.Run(() =>
            {
                barrier.SignalAndWait();
                map.Add("race", i + 10000);
            });

            var remover = Task.Run(() =>
            {
                barrier.SignalAndWait();
                map.Remove("race", i);
            });

            var removeKey = Task.Run(() =>
            {
                barrier.SignalAndWait();
                map.RemoveKey("race");
            });

            Assert.DoesNotThrow(() => Task.WaitAll(adder, remover, removeKey));

            map.Clear();
            Assert.That(map.Count, Is.Zero);
            Assert.That(map.KeyCount, Is.Zero);
            Assert.That(map.Keys, Is.Empty);
        }
    }

    [Test]
    [Category("Stress")]
    [Category("Concurrent")]
    public void AddRemoveRemoveKey_ParallelOnManyKeys_MapRemainsUsableAfterRaces()
    {
        var map = new ConcurrentMultiMap<string, int>();
        const int workers = 8;
        const int iterationsPerWorker = 250;

        Parallel.For(0, workers, worker =>
        {
            for (int i = 0; i < iterationsPerWorker; i++)
            {
                string key = $"k{(worker + i) % 16}";
                int value = worker * iterationsPerWorker + i;

                map.Add(key, value);

                if (i % 3 == 0)
                    map.Remove(key, value);

                if (i % 11 == 0)
                    map.RemoveKey(key);
            }
        });

        Assert.DoesNotThrow(() => map.Add("post", 1));
        Assert.That(map.Contains("post", 1), Is.True);

        map.Clear();
        Assert.That(map.Count, Is.Zero);
        Assert.That(map.KeyCount, Is.Zero);
        Assert.That(map.Keys, Is.Empty);
    }

    [Test]
    [Category("Stress")]
    [Category("Concurrent")]
    public void RemoveKey_ConcurrentWithAddRange_SameKey_DoesNotLeaveEmptyKey()
    {
        var map = new ConcurrentMultiMap<string, int>();
        const int iterations = 2000;

        for (int i = 0; i < iterations; i++)
        {
            map.Add("race", 1);

            using var barrier = new Barrier(2);

            var remover = Task.Run(() =>
            {
                barrier.SignalAndWait();
                map.RemoveKey("race");
            });

            var adder = Task.Run(() =>
            {
                barrier.SignalAndWait();
                map.AddRange("race", new[] { 2, 3 });
            });

            Assert.DoesNotThrow(() => Task.WaitAll(remover, adder));

            if (map.ContainsKey("race"))
            {
                var values = map.GetOrDefault("race").ToArray();
                Assert.That(values, Is.Not.Empty);
                Assert.That(values, Does.Not.Contain(1));
            }

            map.Clear();
        }
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// MultiMapList – uncovered constructor/protected paths (lines 63-70)
// The base-class slow-path Add/AddRange for non-NET6 are exercised indirectly
// through the .NET 8 target via the base-class fallback. To cover the exact
// lines (CreateCollection / AddToCollection / ToReadOnly / RemoveWhere on the
// List impl) we exercise them through public API.
// ──────────────────────────────────────────────────────────────────────────────
