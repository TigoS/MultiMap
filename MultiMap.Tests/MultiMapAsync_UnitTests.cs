using MultiMap.Entities;
using MultiMap.Interfaces;
using System.Reflection;

namespace MultiMap.Tests;

[TestFixture]
public class MultiMapAsyncTests
{
    private MultiMapAsync<string, int> _map;

    [SetUp]
    public void SetUp()
    {
        _map = new MultiMapAsync<string, int>();
    }

    [TearDown]
    public void TearDown()
    {
        _map.Dispose();
    }

    [Test]
    public async Task AddAsync_SingleKeyValue_CanBeRetrieved()
    {
        await _map.AddAsync("a", 1);

        Assert.That(await _map.GetAsync("a"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public async Task AddAsync_NewValue_ReturnsTrue()
    {
        Assert.That(await _map.AddAsync("a", 1), Is.True);
    }

    [Test]
    public async Task AddAsync_DuplicateValue_ReturnsFalse()
    {
        await _map.AddAsync("a", 1);

        Assert.That(await _map.AddAsync("a", 1), Is.False);
    }

    [Test]
    public async Task AddAsync_DuplicateValue_DoesNotStoreSecondCopy()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("a", 1);

        Assert.That(await _map.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task AddAsync_MultipleValuesForSameKey_ReturnsAllValues()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("a", 2);
        await _map.AddAsync("a", 3);

        Assert.That(await _map.GetAsync("a"), Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public async Task AddAsync_DifferentKeys_StoresIndependently()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("b", 2);

        Assert.That(await _map.GetAsync("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(await _map.GetAsync("b"), Is.EquivalentTo(new[] { 2 }));
    }

    [Test]
    public async Task AddRangeAsync_NewKey_StoresAllValues()
    {
        await _map.AddRangeAsync("a", new[] { 1, 2, 3 });

        Assert.That(await _map.GetAsync("a"), Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public async Task AddRangeAsync_ExistingKey_AppendsValues()
    {
        await _map.AddAsync("a", 1);
        await _map.AddRangeAsync("a", new[] { 2, 3 });

        Assert.That(await _map.GetAsync("a"), Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public async Task AddRangeAsync_EmptyCollection_DoesNotChangeState()
    {
        await _map.AddAsync("a", 1);
        await _map.AddRangeAsync("a", Enumerable.Empty<int>());

        Assert.That(await _map.GetAsync("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task AddRangeAsync_DuplicateValues_IgnoresDuplicates()
    {
        await _map.AddRangeAsync("a", new[] { 1, 1, 1 });

        Assert.That(await _map.GetAsync("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task AddRangeAsync_UpdatesCount()
    {
        await _map.AddRangeAsync("a", new[] { 1, 2 });
        await _map.AddRangeAsync("b", new[] { 3 });

        Assert.That(await _map.GetCountAsync(), Is.EqualTo(3));
    }

    [Test]
    public async Task GetAsync_NonExistentKey_ReturnsEmpty()
    {
        Assert.That(await _map.GetAsync("missing"), Is.Empty);
    }

    [Test]
    public async Task GetAsync_ReturnsSnapshot_NotLiveCollection()
    {
        await _map.AddAsync("a", 1);

        var snapshot = await _map.GetAsync("a");
        await _map.AddAsync("a", 2);

        Assert.That(snapshot, Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public async Task RemoveAsync_ExistingValue_ReturnsTrue()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("a", 2);

        Assert.That(await _map.RemoveAsync("a", 1), Is.True);
        Assert.That(await _map.GetAsync("a"), Is.EquivalentTo(new[] { 2 }));
    }

    [Test]
    public async Task RemoveAsync_LastValueForKey_RemovesKey()
    {
        await _map.AddAsync("a", 1);

        await _map.RemoveAsync("a", 1);

        Assert.That(await _map.ContainsKeyAsync("a"), Is.False);
    }

    [Test]
    public async Task RemoveAsync_NonExistentValue_ReturnsFalse()
    {
        await _map.AddAsync("a", 1);

        Assert.That(await _map.RemoveAsync("a", 99), Is.False);
    }

    [Test]
    public async Task RemoveAsync_NonExistentKey_ReturnsFalse()
    {
        Assert.That(await _map.RemoveAsync("missing", 1), Is.False);
    }

    [Test]
    public async Task RemoveKeyAsync_ExistingKey_ReturnsTrueAndRemovesAll()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("a", 2);

        Assert.That(await _map.RemoveKeyAsync("a"), Is.True);
        Assert.That(await _map.ContainsKeyAsync("a"), Is.False);
    }

    [Test]
    public async Task RemoveKeyAsync_NonExistentKey_ReturnsFalse()
    {
        Assert.That(await _map.RemoveKeyAsync("missing"), Is.False);
    }

    [Test]
    public async Task ContainsKeyAsync_ExistingKey_ReturnsTrue()
    {
        await _map.AddAsync("a", 1);

        Assert.That(await _map.ContainsKeyAsync("a"), Is.True);
    }

    [Test]
    public async Task ContainsKeyAsync_NonExistentKey_ReturnsFalse()
    {
        Assert.That(await _map.ContainsKeyAsync("missing"), Is.False);
    }

    [Test]
    public async Task ContainsAsync_ExistingKeyAndValue_ReturnsTrue()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("a", 2);

        Assert.That(await _map.ContainsAsync("a", 1), Is.True);
        Assert.That(await _map.ContainsAsync("a", 2), Is.True);
    }

    [Test]
    public async Task ContainsAsync_ExistingKeyWrongValue_ReturnsFalse()
    {
        await _map.AddAsync("a", 1);

        Assert.That(await _map.ContainsAsync("a", 99), Is.False);
    }

    [Test]
    public async Task ContainsAsync_NonExistentKey_ReturnsFalse()
    {
        Assert.That(await _map.ContainsAsync("missing", 1), Is.False);
    }

    [Test]
    public async Task ContainsAsync_AfterRemovingValue_ReturnsFalse()
    {
        await _map.AddAsync("a", 1);
        await _map.RemoveAsync("a", 1);

        Assert.That(await _map.ContainsAsync("a", 1), Is.False);
    }

    [Test]
    public async Task GetCountAsync_EmptyMap_ReturnsZero()
    {
        Assert.That(await _map.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task GetCountAsync_ReflectsTotalKeyValuePairs()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("a", 2);
        await _map.AddAsync("b", 3);

        Assert.That(await _map.GetCountAsync(), Is.EqualTo(3));
    }

    [Test]
    public async Task GetCountAsync_DecreasesAfterRemove()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("a", 2);
        await _map.RemoveAsync("a", 1);

        Assert.That(await _map.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetCountAsync_DecreasesAfterRemoveKey()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("a", 2);
        await _map.AddAsync("b", 3);

        await _map.RemoveKeyAsync("a");

        Assert.That(await _map.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetCountAsync_UnchangedAfterFailedRemove_NonExistentValue()
    {
        await _map.AddAsync("a", 1);

        await _map.RemoveAsync("a", 99);

        Assert.That(await _map.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetCountAsync_UnchangedAfterFailedRemove_NonExistentKey()
    {
        await _map.AddAsync("a", 1);

        await _map.RemoveAsync("missing", 1);

        Assert.That(await _map.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetCountAsync_UnchangedAfterFailedRemoveKey()
    {
        await _map.AddAsync("a", 1);

        await _map.RemoveKeyAsync("missing");

        Assert.That(await _map.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task AddRangeAsync_WithPartialDuplicates_CountsOnlyNewValues()
    {
        await _map.AddAsync("a", 1);
        await _map.AddRangeAsync("a", new[] { 1, 2, 3 });

        Assert.That(await _map.GetCountAsync(), Is.EqualTo(3));
    }

    [Test]
    public async Task ClearAsync_RemovesAllEntries()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("b", 2);

        await _map.ClearAsync();

        Assert.That(await _map.GetCountAsync(), Is.Zero);
        Assert.That(await _map.ContainsKeyAsync("a"), Is.False);
    }

    [Test]
    public async Task GetAsyncEnumerator_EnumeratesAllKeyValuePairs()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("a", 2);
        await _map.AddAsync("b", 3);

        var pairs = new List<KeyValuePair<string, int>>();
        await foreach (var kvp in _map)
            pairs.Add(kvp);

        Assert.That(pairs, Has.Count.EqualTo(3));
        Assert.That(pairs, Does.Contain(new KeyValuePair<string, int>("a", 1)));
        Assert.That(pairs, Does.Contain(new KeyValuePair<string, int>("a", 2)));
        Assert.That(pairs, Does.Contain(new KeyValuePair<string, int>("b", 3)));
    }

    [Test]
    public async Task GetAsyncEnumerator_EmptyMap_YieldsNothing()
    {
        var pairs = new List<KeyValuePair<string, int>>();
        await foreach (var kvp in _map)
            pairs.Add(kvp);

        Assert.That(pairs, Is.Empty);
    }

    [Test]
    public async Task GetAsyncEnumerator_ReturnsSnapshot_NotAffectedByModification()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("a", 2);

        var pairs = new List<KeyValuePair<string, int>>();
        await foreach (var kvp in _map)
        {
            pairs.Add(kvp);
            await _map.AddAsync("a", 99);
        }

        Assert.That(pairs, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetAsyncEnumerator_SupportsCancellation()
    {
        await _map.AddAsync("a", 1);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in _map.WithCancellation(cts.Token))
            {
            }
        });
    }

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        _map.Dispose();
        Assert.DoesNotThrow(() => _map.Dispose());
    }

    [Test]
    public async Task AddAsync_ConcurrentAdds_AllUniqueValuesStored()
    {
        const int count = 1000;

        var tasks = Enumerable.Range(0, count)
            .Select(i => _map.AddAsync("a", i).AsTask())
            .ToArray();

        await Task.WhenAll(tasks);

        Assert.That(await _map.GetCountAsync(), Is.EqualTo(count));
    }

    [Test]
    public async Task AddAsync_ConcurrentDuplicates_OnlyOneStored()
    {
        const int threads = 100;

        var tasks = Enumerable.Range(0, threads)
            .Select(_ => _map.AddAsync("a", 42).AsTask())
            .ToArray();

        await Task.WhenAll(tasks);

        Assert.That(await _map.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task RemoveAsync_ConcurrentRemoves_AllRemoved()
    {
        const int count = 1000;

        for (int i = 0; i < count; i++)
            await _map.AddAsync("a", i);

        var tasks = Enumerable.Range(0, count)
            .Select(i => _map.RemoveAsync("a", i).AsTask())
            .ToArray();

        await Task.WhenAll(tasks);

        Assert.That(await _map.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task AddAsync_ConcurrentAddsToDifferentKeys_AllStored()
    {
        const int count = 1000;

        var tasks = Enumerable.Range(0, count)
            .Select(i => _map.AddAsync($"key{i}", i).AsTask())
            .ToArray();

        await Task.WhenAll(tasks);

        Assert.That(await _map.GetCountAsync(), Is.EqualTo(count));
    }

    [Test]
    public async Task ConcurrentReadsAndWrites_DoNotThrow()
    {
        const int count = 1000;

        var tasks = Enumerable.Range(0, count).Select(async i =>
        {
            if (i % 3 == 0)
                await _map.AddAsync("a", i);
            else if (i % 3 == 1)
                await _map.GetAsync("a");
            else
                await _map.ContainsAsync("a", i);
        }).ToArray();

        Assert.DoesNotThrowAsync(() => Task.WhenAll(tasks));
    }

    [Test]
    public async Task Equals_SameInstance_ReturnsTrue()
    {
        Assert.That(_map.Equals(_map), Is.True);
    }

    [Test]
    public async Task Equals_DifferentInstanceSameContent_ReturnsFalse()
    {
        var other = new MultiMapAsync<string, int>();
        await _map.AddAsync("a", 1);
        await other.AddAsync("a", 1);

        Assert.That(_map.Equals(other), Is.False);

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
    public async Task GetHashCode_SameInstance_ReturnsSameValue()
    {
        await _map.AddAsync("a", 1);

        int hash1 = _map.GetHashCode();
        int hash2 = _map.GetHashCode();

        Assert.That(hash1, Is.EqualTo(hash2));
    }

    [Test]
    public async Task GetHashCode_DifferentInstances_MayDiffer()
    {
        var other = new MultiMapAsync<string, int>();
        await _map.AddAsync("a", 1);
        await other.AddAsync("a", 1);

        Assert.That(_map.GetHashCode(), Is.Not.EqualTo(other.GetHashCode()));

        other.Dispose();
    }

    [Test]
    public void Dispose_WhenSemaphoreIsNull_DoesNotThrow()
    {
        var map = new MultiMapAsync<string, int>();
        var field = typeof(MultiMapAsync<string, int>)
            .GetField("_semaphore", BindingFlags.NonPublic | BindingFlags.Instance)!;
        field.SetValue(map, null);

        Assert.DoesNotThrow(() => map.Dispose());
    }

    [Test]
    public async Task GetKeysAsync_EmptyMap_ReturnsEmpty()
    {
        Assert.That(await _map.GetKeysAsync(), Is.Empty);
    }

    [Test]
    public async Task GetKeysAsync_MultipleKeys_ReturnsAllKeys()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("b", 2);
        await _map.AddAsync("c", 3);

        Assert.That(await _map.GetKeysAsync(), Is.EquivalentTo(new[] { "a", "b", "c" }));
    }

    [Test]
    public async Task GetKeysAsync_MultipleValuesPerKey_ReturnsDistinctKeys()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("a", 2);
        await _map.AddAsync("b", 3);

        Assert.That(await _map.GetKeysAsync(), Is.EquivalentTo(new[] { "a", "b" }));
    }

    [Test]
    public async Task GetKeysAsync_AfterRemovingLastValueForKey_DoesNotContainKey()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("b", 2);
        await _map.RemoveAsync("a", 1);

        Assert.That(await _map.GetKeysAsync(), Is.EquivalentTo(new[] { "b" }));
    }

    [Test]
    public async Task GetKeysAsync_AfterRemoveKey_DoesNotContainKey()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("b", 2);
        await _map.RemoveKeyAsync("a");

        Assert.That(await _map.GetKeysAsync(), Is.EquivalentTo(new[] { "b" }));
    }

    [Test]
    public async Task GetKeysAsync_AfterClear_ReturnsEmpty()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("b", 2);
        await _map.ClearAsync();

        Assert.That(await _map.GetKeysAsync(), Is.Empty);
    }

    [Test]
    public async Task GetKeysAsync_ReturnsSnapshot_NotLiveCollection()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("b", 2);

        var keys = await _map.GetKeysAsync();
        await _map.AddAsync("c", 3);

        Assert.That(keys, Is.EquivalentTo(new[] { "a", "b" }));
    }

    [Test]
    public async Task IMultiMapAsync_CanBeUsedThroughInterface()
    {
        IMultiMapAsync<string, int> map = _map;

        Assert.That(await map.AddAsync("x", 1), Is.True);
        Assert.That(await map.AddAsync("x", 2), Is.True);
        await map.AddRangeAsync("y", new[] { 10, 20 });

        Assert.That(await map.GetAsync("x"), Is.EquivalentTo(new[] { 1, 2 }));
        Assert.That(await map.ContainsKeyAsync("x"), Is.True);
        Assert.That(await map.ContainsAsync("x", 1), Is.True);
        Assert.That(await map.GetCountAsync(), Is.EqualTo(4));
        Assert.That(await map.GetKeysAsync(), Is.EquivalentTo(new[] { "x", "y" }));

        Assert.That(await map.RemoveAsync("x", 1), Is.True);
        Assert.That(await map.RemoveKeyAsync("y"), Is.True);
        Assert.That(await map.GetCountAsync(), Is.EqualTo(1));

        await map.ClearAsync();
        Assert.That(await map.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task Stress_RepeatedAddRemoveCycles_CountRemainsAccurate()
    {
        for (int cycle = 0; cycle < 50; cycle++)
        {
            for (int i = 0; i < 20; i++)
                await _map.AddAsync("key", i);

            Assert.That(await _map.GetCountAsync(), Is.EqualTo(20), $"Count wrong after adds in cycle {cycle}");

            for (int i = 0; i < 20; i++)
                await _map.RemoveAsync("key", i);

            Assert.That(await _map.GetCountAsync(), Is.Zero, $"Count wrong after removes in cycle {cycle}");
        }
    }

    [Test]
    public async Task Stress_ClearAndRebuild_CountResetsCorrectly()
    {
        for (int cycle = 0; cycle < 50; cycle++)
        {
            for (int i = 0; i < 10; i++)
                await _map.AddAsync($"k{i % 3}", cycle * 10 + i);

            Assert.That(await _map.GetCountAsync(), Is.EqualTo(10), $"Count wrong before clear in cycle {cycle}");

            await _map.ClearAsync();

            Assert.That(await _map.GetCountAsync(), Is.Zero, $"Count wrong after clear in cycle {cycle}");
            Assert.That(await _map.GetKeysAsync(), Is.Empty);
        }
    }

    [Test]
    public async Task Stress_MixedOperations_CountTracksCorrectly()
    {
        int expected = 0;

        for (int cycle = 0; cycle < 30; cycle++)
        {
            if (await _map.AddAsync("a", cycle))
                expected++;

            foreach (var v in new[] { cycle * 10, cycle * 10 + 1 })
            {
                if (await _map.AddAsync("b", v))
                    expected++;
            }

            if (cycle > 0 && cycle % 5 == 0)
            {
                await _map.ClearAsync();
                expected = 0;
            }

            if (cycle > 0 && cycle % 3 == 0 && await _map.ContainsKeyAsync("a"))
            {
                int beforeKeys = (await _map.GetAsync("a")).Count();
                await _map.RemoveKeyAsync("a");
                expected -= beforeKeys;
            }

            Assert.That(await _map.GetCountAsync(), Is.EqualTo(expected), $"Count mismatch at cycle {cycle}");
        }
    }

    [Test]
    public async Task Stress_AddRangeAndRemoveKey_CountDecreasesCorrectly()
    {
        for (int cycle = 0; cycle < 40; cycle++)
        {
            string key = $"key{cycle % 5}";
            await _map.RemoveKeyAsync(key);

            var values = Enumerable.Range(cycle * 10, 5);
            await _map.AddRangeAsync(key, values);
        }

        int totalCount = 0;
        foreach (var key in await _map.GetKeysAsync())
            totalCount += (await _map.GetAsync(key)).Count();

        Assert.That(await _map.GetCountAsync(), Is.EqualTo(totalCount));
    }

    [Test]
    public async Task Stress_ConcurrentAddRemoveClear_CountNeverNegative()
    {
        const int iterations = 500;

        var tasks = Enumerable.Range(0, iterations).Select(async i =>
        {
            switch (i % 4)
            {
                case 0:
                    await _map.AddAsync($"key{i % 10}", i);
                    break;
                case 1:
                    await _map.RemoveAsync($"key{i % 10}", i - 1);
                    break;
                case 2:
                    await _map.AddRangeAsync($"key{i % 10}", new[] { i, i + 1000 });
                    break;
                case 3:
                    await _map.RemoveKeyAsync($"key{i % 10}");
                    break;
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        int count = await _map.GetCountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(0));

        int verifyCount = 0;
        foreach (var key in await _map.GetKeysAsync())
            verifyCount += (await _map.GetAsync(key)).Count();

        Assert.That(count, Is.EqualTo(verifyCount));
    }

    [Test]
    public async Task Stress_AsyncEnumeratorSnapshot_CountConsistency()
    {
        const int snapshotCount = 30;
        const int mutationsPerCycle = 20;
        using var cts = new CancellationTokenSource();

        var mutationTask = Task.Run(async () =>
        {
            int value = 0;
            while (!cts.IsCancellationRequested)
            {
                for (int i = 0; i < mutationsPerCycle; i++)
                    await _map.AddAsync($"key{i % 5}", value++);

                for (int i = 0; i < mutationsPerCycle; i++)
                    await _map.RemoveAsync($"key{i % 5}", value - mutationsPerCycle + i);

                await _map.AddRangeAsync("bulk", Enumerable.Range(value, 10));
                await _map.RemoveKeyAsync("bulk");
            }
        });

        for (int snapshot = 0; snapshot < snapshotCount; snapshot++)
        {
            var items = new List<KeyValuePair<string, int>>();
            await foreach (var kvp in _map)
                items.Add(kvp);

            Assert.That(items.Count, Is.GreaterThanOrEqualTo(0),
                $"Snapshot {snapshot}: negative item count");

            var distinctPairs = items.Distinct().ToList();
            Assert.That(distinctPairs.Count, Is.EqualTo(items.Count),
                $"Snapshot {snapshot}: snapshot contains duplicate pairs");
        }

        cts.Cancel();

        try { await mutationTask; }
        catch (OperationCanceledException) { }

        int finalCount = await _map.GetCountAsync();
        Assert.That(finalCount, Is.GreaterThanOrEqualTo(0));

        int verifyCount = 0;
        foreach (var key in await _map.GetKeysAsync())
            verifyCount += (await _map.GetAsync(key)).Count();

        Assert.That(finalCount, Is.EqualTo(verifyCount),
            "Final Count does not match sum of per-key values");
    }
}
