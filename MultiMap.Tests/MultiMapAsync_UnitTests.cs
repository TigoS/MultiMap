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
        _map.DisposeAsync().GetAwaiter().GetResult();
    }

    [Test]
    public async Task AddAsync_SingleKeyValue_CanBeRetrieved()
    {
        await _map.AddAsync("a", 1);

        Assert.That(await _map.GetOrDefaultAsync("a"), Is.EquivalentTo(new[] { 1 }));
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

        Assert.That(await _map.GetOrDefaultAsync("a"), Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public async Task AddAsync_DifferentKeys_StoresIndependently()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("b", 2);

        Assert.That(await _map.GetOrDefaultAsync("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(await _map.GetOrDefaultAsync("b"), Is.EquivalentTo(new[] { 2 }));
    }

    [Test]
    public async Task AddRangeAsync_NewKey_StoresAllValues()
    {
        await _map.AddRangeAsync("a", new[] { 1, 2, 3 });

        Assert.That(await _map.GetOrDefaultAsync("a"), Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public async Task AddRangeAsync_ExistingKey_AppendsValues()
    {
        await _map.AddAsync("a", 1);
        await _map.AddRangeAsync("a", new[] { 2, 3 });

        Assert.That(await _map.GetOrDefaultAsync("a"), Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public async Task AddRangeAsync_EmptyCollection_DoesNotChangeState()
    {
        await _map.AddAsync("a", 1);
        await _map.AddRangeAsync("a", Enumerable.Empty<int>());

        Assert.That(await _map.GetOrDefaultAsync("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task AddRangeAsync_DuplicateValues_IgnoresDuplicates()
    {
        await _map.AddRangeAsync("a", new[] { 1, 1, 1 });

        Assert.That(await _map.GetOrDefaultAsync("a"), Is.EquivalentTo(new[] { 1 }));
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
    public async Task GetAsync_ExistingKey_ReturnsValues()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("a", 2);

        Assert.That(await _map.GetAsync("a"), Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public void GetAsync_NonExistentKey_ThrowsKeyNotFoundException()
    {
        Assert.ThrowsAsync<KeyNotFoundException>(async () => await _map.GetAsync("missing"));
    }

    [Test]
    public async Task GetAsync_NonExistentKey_ExceptionContainsKeyName()
    {
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => await _map.GetAsync("missing"));

        Assert.That(ex!.Message, Does.Contain("missing"));
    }

    [Test]
    public async Task GetOrDefaultAsync_NonExistentKey_ReturnsEmpty()
    {
        Assert.That(await _map.GetOrDefaultAsync("missing"), Is.Empty);
    }

    [Test]
    public async Task GetOrDefaultAsync_ReturnsSnapshot_NotLiveCollection()
    {
        await _map.AddAsync("a", 1);

        var snapshot = await _map.GetOrDefaultAsync("a");
        await _map.AddAsync("a", 2);

        Assert.That(snapshot, Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public async Task TryGetAsync_ExistingKey_ReturnsTrueWithValues()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("a", 2);

        var (found, values) = await _map.TryGetAsync("a");

        Assert.That(found, Is.True);
        Assert.That(values, Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public async Task TryGetAsync_NonExistentKey_ReturnsFalseWithEmpty()
    {
        var (found, values) = await _map.TryGetAsync("missing");

        Assert.That(found, Is.False);
        Assert.That(values, Is.Empty);
    }

    [Test]
    public async Task TryGetAsync_AfterRemovingLastValue_ReturnsFalseWithEmpty()
    {
        await _map.AddAsync("a", 1);
        await _map.RemoveAsync("a", 1);

        var (found, values) = await _map.TryGetAsync("a");

        Assert.That(found, Is.False);
        Assert.That(values, Is.Empty);
    }

    [Test]
    public async Task TryGetAsync_AfterRemoveKey_ReturnsFalseWithEmpty()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("a", 2);
        await _map.RemoveKeyAsync("a");

        var (found, values) = await _map.TryGetAsync("a");

        Assert.That(found, Is.False);
        Assert.That(values, Is.Empty);
    }

    [Test]
    public async Task TryGetAsync_AfterClear_ReturnsFalseWithEmpty()
    {
        await _map.AddAsync("a", 1);
        await _map.ClearAsync();

        var (found, values) = await _map.TryGetAsync("a");

        Assert.That(found, Is.False);
        Assert.That(values, Is.Empty);
    }

    [Test]
    public async Task TryGetAsync_MultipleValuesForSameKey_ReturnsAllValues()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("a", 2);
        await _map.AddAsync("a", 3);

        var (found, values) = await _map.TryGetAsync("a");

        Assert.That(found, Is.True);
        Assert.That(values, Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public async Task RemoveAsync_ExistingValue_ReturnsTrue()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("a", 2);

        Assert.That(await _map.RemoveAsync("a", 1), Is.True);
        Assert.That(await _map.GetOrDefaultAsync("a"), Is.EquivalentTo(new[] { 2 }));
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
        _map.DisposeAsync().GetAwaiter().GetResult();
        Assert.DoesNotThrow(() => _map.DisposeAsync().AsTask());
    }

    [Test]
    [Category("Stress")]
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
    [Category("Stress")]
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
    [Category("Stress")]
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
    [Category("Stress")]
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
    [Category("Stress")]
    public async Task ConcurrentReadsAndWrites_DoNotThrow()
    {
        const int count = 1000;

        var tasks = Enumerable.Range(0, count).Select(async i =>
        {
            if (i % 3 == 0)
                await _map.AddAsync("a", i);
            else if (i % 3 == 1)
                await _map.GetOrDefaultAsync("a");
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
    public async Task Equals_DifferentInstanceSameContent_ReturnsTrue()
    {
        var other = new MultiMapAsync<string, int>();
        await _map.AddAsync("a", 1);
        await other.AddAsync("a", 1);

        Assert.That(_map.Equals(other), Is.True);

        await other.DisposeAsync();
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
    public async Task GetHashCode_DifferentInstancesSameContent_ReturnsSameValue()
    {
        var other = new MultiMapAsync<string, int>();
        await _map.AddAsync("a", 1);
        await other.AddAsync("a", 1);

        Assert.That(_map.GetHashCode(), Is.EqualTo(other.GetHashCode()));

        await other.DisposeAsync();
    }

    [Test]
    public async Task Dispose_DisposesCleanly()
    {
        var map = new MultiMapAsync<string, int>();
        await map.AddAsync("a", 1);

        Assert.DoesNotThrowAsync(async () => await map.DisposeAsync());
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

        Assert.That(await map.GetOrDefaultAsync("x"), Is.EquivalentTo(new[] { 1, 2 }));
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
    [Category("Stress")]
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
    [Category("Stress")]
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
    [Category("Stress")]
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
                int beforeKeys = (await _map.GetOrDefaultAsync("a")).Count();
                await _map.RemoveKeyAsync("a");
                expected -= beforeKeys;
            }

            Assert.That(await _map.GetCountAsync(), Is.EqualTo(expected), $"Count mismatch at cycle {cycle}");
        }
    }

    [Test]
    [Category("Stress")]
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
            totalCount += (await _map.GetOrDefaultAsync(key)).Count();

        Assert.That(await _map.GetCountAsync(), Is.EqualTo(totalCount));
    }

    [Test]
    [Category("Stress")]
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
            verifyCount += (await _map.GetOrDefaultAsync(key)).Count();

        Assert.That(count, Is.EqualTo(verifyCount));
    }

    [Test]
    [Category("Stress")]
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
            verifyCount += (await _map.GetOrDefaultAsync(key)).Count();

        Assert.That(finalCount, Is.EqualTo(verifyCount),
            "Final Count does not match sum of per-key values");
    }

    // ── Helper ────────────────────────────────────────────────

    private SemaphoreSlim GetSemaphore()
    {
        return (SemaphoreSlim)typeof(MultiMapAsync<string, int>)
            .GetField("_semaphore", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(_map)!;
    }

    // ── Cancellation Token Tests ──────────────────────────────

    [Test]
    public void AddAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.That(async () => await _map.AddAsync("a", 1, cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void AddRangeAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.That(async () => await _map.AddRangeAsync("a", new[] { 1, 2 }, cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void GetOrDefaultAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.That(async () => await _map.GetOrDefaultAsync("a", cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void TryGetAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.That(async () => await _map.TryGetAsync("a", cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void RemoveAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.That(async () => await _map.RemoveAsync("a", 1, cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void RemoveKeyAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.That(async () => await _map.RemoveKeyAsync("a", cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void ContainsKeyAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.That(async () => await _map.ContainsKeyAsync("a", cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void ContainsAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.That(async () => await _map.ContainsAsync("a", 1, cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void GetCountAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.That(async () => await _map.GetCountAsync(cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void ClearAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.That(async () => await _map.ClearAsync(cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void GetKeysAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.That(async () => await _map.GetKeysAsync(cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    // ── Slow Path Tests (semaphore contention) ────────────────

    [Test]
    public async Task AddAsync_SlowPath_WhenSemaphoreIsHeld_CompletesAfterRelease()
    {
        var semaphore = GetSemaphore();
        await semaphore.WaitAsync();

        var addTask = _map.AddAsync("a", 1).AsTask();
        Assert.That(addTask.IsCompleted, Is.False);

        semaphore.Release();
        bool result = await addTask;

        Assert.That(result, Is.True);
        Assert.That(await _map.GetOrDefaultAsync("a"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public async Task AddRangeAsync_SlowPath_WhenSemaphoreIsHeld_CompletesAfterRelease()
    {
        var semaphore = GetSemaphore();
        await semaphore.WaitAsync();

        var addTask = _map.AddRangeAsync("a", new[] { 1, 2, 3 });
        Assert.That(addTask.IsCompleted, Is.False);

        semaphore.Release();
        await addTask;

        Assert.That(await _map.GetOrDefaultAsync("a"), Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public async Task GetOrDefaultAsync_SlowPath_WhenSemaphoreIsHeld_CompletesAfterRelease()
    {
        await _map.AddAsync("a", 1);

        var semaphore = GetSemaphore();
        await semaphore.WaitAsync();

        var getTask = _map.GetOrDefaultAsync("a").AsTask();
        Assert.That(getTask.IsCompleted, Is.False);

        semaphore.Release();
        var result = await getTask;

        Assert.That(result, Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public async Task TryGetAsync_SlowPath_WhenSemaphoreIsHeld_CompletesAfterRelease()
    {
        await _map.AddAsync("a", 1);

        var semaphore = GetSemaphore();
        await semaphore.WaitAsync();

        var tryGetTask = _map.TryGetAsync("a").AsTask();
        Assert.That(tryGetTask.IsCompleted, Is.False);

        semaphore.Release();
        var (found, values) = await tryGetTask;

        Assert.That(found, Is.True);
        Assert.That(values, Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public async Task RemoveAsync_SlowPath_WhenSemaphoreIsHeld_CompletesAfterRelease()
    {
        await _map.AddAsync("a", 1);

        var semaphore = GetSemaphore();
        await semaphore.WaitAsync();

        var removeTask = _map.RemoveAsync("a", 1).AsTask();
        Assert.That(removeTask.IsCompleted, Is.False);

        semaphore.Release();
        bool result = await removeTask;

        Assert.That(result, Is.True);
        Assert.That(await _map.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task RemoveKeyAsync_SlowPath_WhenSemaphoreIsHeld_CompletesAfterRelease()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("a", 2);

        var semaphore = GetSemaphore();
        await semaphore.WaitAsync();

        var removeTask = _map.RemoveKeyAsync("a").AsTask();
        Assert.That(removeTask.IsCompleted, Is.False);

        semaphore.Release();
        bool result = await removeTask;

        Assert.That(result, Is.True);
        Assert.That(await _map.ContainsKeyAsync("a"), Is.False);
    }

    [Test]
    public async Task ContainsKeyAsync_SlowPath_WhenSemaphoreIsHeld_CompletesAfterRelease()
    {
        await _map.AddAsync("a", 1);

        var semaphore = GetSemaphore();
        await semaphore.WaitAsync();

        var containsTask = _map.ContainsKeyAsync("a").AsTask();
        Assert.That(containsTask.IsCompleted, Is.False);

        semaphore.Release();
        bool result = await containsTask;

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ContainsAsync_SlowPath_WhenSemaphoreIsHeld_CompletesAfterRelease()
    {
        await _map.AddAsync("a", 1);

        var semaphore = GetSemaphore();
        await semaphore.WaitAsync();

        var containsTask = _map.ContainsAsync("a", 1).AsTask();
        Assert.That(containsTask.IsCompleted, Is.False);

        semaphore.Release();
        bool result = await containsTask;

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task GetCountAsync_SlowPath_WhenSemaphoreIsHeld_CompletesAfterRelease()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("b", 2);

        var semaphore = GetSemaphore();
        await semaphore.WaitAsync();

        var countTask = _map.GetCountAsync().AsTask();
        Assert.That(countTask.IsCompleted, Is.False);

        semaphore.Release();
        int result = await countTask;

        Assert.That(result, Is.EqualTo(2));
    }

    [Test]
    public async Task ClearAsync_SlowPath_WhenSemaphoreIsHeld_CompletesAfterRelease()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("b", 2);

        var semaphore = GetSemaphore();
        await semaphore.WaitAsync();

        var clearTask = _map.ClearAsync();
        Assert.That(clearTask.IsCompleted, Is.False);

        semaphore.Release();
        await clearTask;

        Assert.That(await _map.GetCountAsync(), Is.Zero);
        Assert.That(await _map.ContainsKeyAsync("a"), Is.False);
    }

    [Test]
    public async Task GetKeysAsync_SlowPath_WhenSemaphoreIsHeld_CompletesAfterRelease()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("b", 2);

        var semaphore = GetSemaphore();
        await semaphore.WaitAsync();

        var keysTask = _map.GetKeysAsync().AsTask();
        Assert.That(keysTask.IsCompleted, Is.False);

        semaphore.Release();
        var result = await keysTask;

        Assert.That(result, Is.EquivalentTo(new[] { "a", "b" }));
    }

    // ── Set Operation Direct Tests ────────────────────────────

    [Test]
    public async Task UnionAsync_AddsAllPairsFromOther()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("b", 2);

        await using var other = new MultiMapAsync<string, int>();
        await other.AddAsync("a", 3);
        await other.AddAsync("c", 4);

        await _map.UnionAsync(other);

        Assert.That(await _map.GetOrDefaultAsync("a"), Is.EquivalentTo(new[] { 1, 3 }));
        Assert.That(await _map.GetOrDefaultAsync("b"), Is.EquivalentTo(new[] { 2 }));
        Assert.That(await _map.GetOrDefaultAsync("c"), Is.EquivalentTo(new[] { 4 }));
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(4));
    }

    [Test]
    public async Task IntersectAsync_KeepsOnlyCommonPairs()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("a", 2);
        await _map.AddAsync("b", 3);

        await using var other = new MultiMapAsync<string, int>();
        await other.AddAsync("a", 1);
        await other.AddAsync("c", 4);

        await _map.IntersectAsync(other);

        Assert.That(await _map.GetOrDefaultAsync("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(await _map.ContainsKeyAsync("b"), Is.False);
        Assert.That(await _map.ContainsKeyAsync("c"), Is.False);
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task ExceptWithAsync_RemovesPairsPresentInOther()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("a", 2);
        await _map.AddAsync("b", 3);

        await using var other = new MultiMapAsync<string, int>();
        await other.AddAsync("a", 1);
        await other.AddAsync("b", 3);

        await _map.ExceptWithAsync(other);

        Assert.That(await _map.GetOrDefaultAsync("a"), Is.EquivalentTo(new[] { 2 }));
        Assert.That(await _map.ContainsKeyAsync("b"), Is.False);
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task SymmetricExceptWithAsync_KeepsPairsInOneButNotBoth()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("a", 2);
        await _map.AddAsync("b", 3);

        await using var other = new MultiMapAsync<string, int>();
        await other.AddAsync("a", 2);
        await other.AddAsync("a", 3);
        await other.AddAsync("c", 4);

        await _map.SymmetricExceptWithAsync(other);

        Assert.That(await _map.GetOrDefaultAsync("a"), Is.EquivalentTo(new[] { 1, 3 }));
        Assert.That(await _map.GetOrDefaultAsync("b"), Is.EquivalentTo(new[] { 3 }));
        Assert.That(await _map.GetOrDefaultAsync("c"), Is.EquivalentTo(new[] { 4 }));
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(4));
    }

    // ── Edge Case Tests ───────────────────────────────────────

    [Test]
    public async Task ClearAsync_OnEmptyMap_DoesNotThrow()
    {
        await _map.ClearAsync();

        Assert.That(await _map.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task AddAsync_AfterClear_WorksCorrectly()
    {
        await _map.AddAsync("a", 1);
        await _map.ClearAsync();
        await _map.AddAsync("a", 2);

        Assert.That(await _map.GetOrDefaultAsync("a"), Is.EquivalentTo(new[] { 2 }));
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task RemoveAsync_ValueExistsInDifferentKey_ReturnsFalse()
    {
        await _map.AddAsync("a", 1);
        await _map.AddAsync("b", 2);

        Assert.That(await _map.RemoveAsync("a", 2), Is.False);
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(2));
    }

    // ── GetKeyCountAsync Tests ────────────────────────────────

    [Test]
    public async Task GetKeyCountAsync_EmptyMap_ReturnsZero()
    {
        Assert.That(await _map.GetKeyCountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task GetKeyCountAsync_AfterAddingSingleKey_ReturnsOne()
    {
        await _map.AddAsync("key1", 1);
        Assert.That(await _map.GetKeyCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetKeyCountAsync_AfterAddingMultipleKeys_ReturnsCorrectCount()
    {
        await _map.AddAsync("key1", 1);
        await _map.AddAsync("key2", 2);
        await _map.AddAsync("key3", 3);
        Assert.That(await _map.GetKeyCountAsync(), Is.EqualTo(3));
    }

    [Test]
    public async Task GetKeyCountAsync_AfterAddingMultipleValuesToSameKey_ReturnsOne()
    {
        await _map.AddAsync("key1", 1);
        await _map.AddAsync("key1", 2);
        await _map.AddAsync("key1", 3);
        Assert.That(await _map.GetKeyCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetKeyCountAsync_AfterRemovingKey_DecreasesCorrectly()
    {
        await _map.AddAsync("key1", 1);
        await _map.AddAsync("key2", 2);
        await _map.RemoveKeyAsync("key1");
        Assert.That(await _map.GetKeyCountAsync(), Is.EqualTo(1));
    }

    // ── GetValuesAsync Tests ──────────────────────────────────

    [Test]
    public async Task GetValuesAsync_EmptyMap_ReturnsEmpty()
    {
        var values = await _map.GetValuesAsync();
        Assert.That(values, Is.Empty);
    }

    [Test]
    public async Task GetValuesAsync_WithSingleValue_ReturnsCorrectValue()
    {
        await _map.AddAsync("key1", 42);
        var values = await _map.GetValuesAsync();
        Assert.That(values, Is.EquivalentTo(new[] { 42 }));
    }

    [Test]
    public async Task GetValuesAsync_WithMultipleValuesAcrossKeys_ReturnsAllValues()
    {
        await _map.AddAsync("key1", 1);
        await _map.AddAsync("key1", 2);
        await _map.AddAsync("key2", 3);
        await _map.AddAsync("key2", 4);
        var values = await _map.GetValuesAsync();
        Assert.That(values, Is.EquivalentTo(new[] { 1, 2, 3, 4 }));
    }

    [Test]
    public async Task GetValuesAsync_AfterRemovingValue_ReturnsRemainingValues()
    {
        await _map.AddAsync("key1", 1);
        await _map.AddAsync("key1", 2);
        await _map.RemoveAsync("key1", 1);
        var values = await _map.GetValuesAsync();
        Assert.That(values, Is.EquivalentTo(new[] { 2 }));
    }

    // ── GetValuesCountAsync Tests ─────────────────────────────

    [Test]
    public async Task GetValuesCountAsync_NonExistentKey_ReturnsZero()
    {
        Assert.That(await _map.GetValuesCountAsync("missing"), Is.EqualTo(0));
    }

    [Test]
    public async Task GetValuesCountAsync_KeyWithSingleValue_ReturnsOne()
    {
        await _map.AddAsync("key1", 1);
        Assert.That(await _map.GetValuesCountAsync("key1"), Is.EqualTo(1));
    }

    [Test]
    public async Task GetValuesCountAsync_KeyWithMultipleValues_ReturnsCorrectCount()
    {
        await _map.AddAsync("key1", 1);
        await _map.AddAsync("key1", 2);
        await _map.AddAsync("key1", 3);
        Assert.That(await _map.GetValuesCountAsync("key1"), Is.EqualTo(3));
    }

    [Test]
    public async Task GetValuesCountAsync_AfterRemovingValue_DecreasesCorrectly()
    {
        await _map.AddAsync("key1", 1);
        await _map.AddAsync("key1", 2);
        await _map.RemoveAsync("key1", 1);
        Assert.That(await _map.GetValuesCountAsync("key1"), Is.EqualTo(1));
    }

    // ── AddRangeAsync(KeyValuePairs) Tests ────────────────────

    [Test]
    public async Task AddRangeAsyncKeyValuePairs_EmptyCollection_DoesNothing()
    {
        await _map.AddRangeAsync([]);
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task AddRangeAsyncKeyValuePairs_SinglePair_AddsCorrectly()
    {
        var pairs = new[] { new KeyValuePair<string, int>("key1", 1) };
        await _map.AddRangeAsync(pairs);
        Assert.That(await _map.GetOrDefaultAsync("key1"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public async Task AddRangeAsyncKeyValuePairs_MultiplePairsSameKey_AddsAllValues()
    {
        var pairs = new[]
        {
            new KeyValuePair<string, int>("key1", 1),
            new KeyValuePair<string, int>("key1", 2),
            new KeyValuePair<string, int>("key1", 3)
        };
        await _map.AddRangeAsync(pairs);
        Assert.That(await _map.GetOrDefaultAsync("key1"), Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public async Task AddRangeAsyncKeyValuePairs_MultiplePairsDifferentKeys_AddsCorrectly()
    {
        var pairs = new[]
        {
            new KeyValuePair<string, int>("key1", 1),
            new KeyValuePair<string, int>("key2", 2),
            new KeyValuePair<string, int>("key3", 3)
        };
        await _map.AddRangeAsync(pairs);
        Assert.That(await _map.GetOrDefaultAsync("key1"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(await _map.GetOrDefaultAsync("key2"), Is.EquivalentTo(new[] { 2 }));
        Assert.That(await _map.GetOrDefaultAsync("key3"), Is.EquivalentTo(new[] { 3 }));
    }

    [Test]
    public async Task AddRangeAsyncKeyValuePairs_DuplicatePairs_IgnoresDuplicates()
    {
        var pairs = new[]
        {
            new KeyValuePair<string, int>("key1", 1),
            new KeyValuePair<string, int>("key1", 1)
        };
        await _map.AddRangeAsync(pairs);
        Assert.That(await _map.GetOrDefaultAsync("key1"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(1));
    }

    // ── RemoveRangeAsync Tests ────────────────────────────────

    [Test]
    public async Task RemoveRangeAsync_EmptyCollection_ReturnsZero()
    {
        await _map.AddAsync("key1", 1);
        int removed = await _map.RemoveRangeAsync([]);
        Assert.That(removed, Is.EqualTo(0));
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task RemoveRangeAsync_SingleExistingPair_ReturnsOne()
    {
        await _map.AddAsync("key1", 1);
        var pairs = new[] { new KeyValuePair<string, int>("key1", 1) };
        int removed = await _map.RemoveRangeAsync(pairs);
        Assert.That(removed, Is.EqualTo(1));
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task RemoveRangeAsync_SingleNonExistentPair_ReturnsZero()
    {
        await _map.AddAsync("key1", 1);
        var pairs = new[] { new KeyValuePair<string, int>("key2", 2) };
        int removed = await _map.RemoveRangeAsync(pairs);
        Assert.That(removed, Is.EqualTo(0));
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task RemoveRangeAsync_MultiplePairs_RemovesCorrectCount()
    {
        await _map.AddAsync("key1", 1);
        await _map.AddAsync("key1", 2);
        await _map.AddAsync("key2", 3);
        var pairs = new[]
        {
            new KeyValuePair<string, int>("key1", 1),
            new KeyValuePair<string, int>("key2", 3)
        };
        int removed = await _map.RemoveRangeAsync(pairs);
        Assert.That(removed, Is.EqualTo(2));
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task RemoveRangeAsync_MixedExistingAndNonExisting_RemovesOnlyExisting()
    {
        await _map.AddAsync("key1", 1);
        await _map.AddAsync("key2", 2);
        var pairs = new[]
        {
            new KeyValuePair<string, int>("key1", 1),
            new KeyValuePair<string, int>("key3", 3)
        };
        int removed = await _map.RemoveRangeAsync(pairs);
        Assert.That(removed, Is.EqualTo(1));
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task RemoveRangeAsync_LastValueOfKey_RemovesKey()
    {
        await _map.AddAsync("key1", 1);
        var pairs = new[] { new KeyValuePair<string, int>("key1", 1) };
        await _map.RemoveRangeAsync(pairs);
        Assert.That(await _map.ContainsKeyAsync("key1"), Is.False);
    }

    // ── RemoveWhereAsync Tests ────────────────────────────────

    [Test]
    public async Task RemoveWhereAsync_NonExistentKey_ReturnsZero()
    {
        int removed = await _map.RemoveWhereAsync("missing", v => v > 0);
        Assert.That(removed, Is.EqualTo(0));
    }

    [Test]
    public async Task RemoveWhereAsync_NoMatchingValues_ReturnsZero()
    {
        await _map.AddAsync("key1", 1);
        await _map.AddAsync("key1", 2);
        int removed = await _map.RemoveWhereAsync("key1", v => v > 10);
        Assert.That(removed, Is.EqualTo(0));
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task RemoveWhereAsync_SingleMatchingValue_RemovesAndReturnsOne()
    {
        await _map.AddAsync("key1", 1);
        await _map.AddAsync("key1", 2);
        await _map.AddAsync("key1", 3);
        int removed = await _map.RemoveWhereAsync("key1", v => v == 2);
        Assert.That(removed, Is.EqualTo(1));
        Assert.That(await _map.GetOrDefaultAsync("key1"), Is.EquivalentTo(new[] { 1, 3 }));
    }

    [Test]
    public async Task RemoveWhereAsync_MultipleMatchingValues_RemovesAll()
    {
        await _map.AddAsync("key1", 1);
        await _map.AddAsync("key1", 2);
        await _map.AddAsync("key1", 3);
        await _map.AddAsync("key1", 4);
        int removed = await _map.RemoveWhereAsync("key1", v => v > 2);
        Assert.That(removed, Is.EqualTo(2));
        Assert.That(await _map.GetOrDefaultAsync("key1"), Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public async Task RemoveWhereAsync_AllValues_RemovesKeyAndReturnsCount()
    {
        await _map.AddAsync("key1", 1);
        await _map.AddAsync("key1", 2);
        int removed = await _map.RemoveWhereAsync("key1", v => true);
        Assert.That(removed, Is.EqualTo(2));
        Assert.That(await _map.ContainsKeyAsync("key1"), Is.False);
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task RemoveWhereAsync_ComplexPredicate_RemovesCorrectly()
    {
        await _map.AddAsync("key1", 1);
        await _map.AddAsync("key1", 2);
        await _map.AddAsync("key1", 3);
        await _map.AddAsync("key1", 4);
        await _map.AddAsync("key1", 5);
        int removed = await _map.RemoveWhereAsync("key1", v => v % 2 == 0);
        Assert.That(removed, Is.EqualTo(2));
        Assert.That(await _map.GetOrDefaultAsync("key1"), Is.EquivalentTo(new[] { 1, 3, 5 }));
    }

    [Test]
    public async Task RemoveWhereAsync_WithCancellationToken_CanBeCancelled()
    {
        await _map.AddAsync("key1", 1);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await _map.RemoveWhereAsync("key1", v => true, cts.Token));
    }

    // ── Slow Path Coverage Tests (Semaphore Contention) ──────────────────

    [Test]
    [Category("Concurrency")]
    public async Task GetAsync_SlowPath_UnderContention_ExecutesCorrectly()
    {
        await _map.AddAsync("key1", 1);
        await _map.AddAsync("key1", 2);

        var tasks = new List<Task<IEnumerable<int>>>();

        // Create contention by spawning many concurrent GetAsync calls
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(_map.GetAsync("key1").AsTask());
        }

        var results = await Task.WhenAll(tasks);

        // All should succeed and return the same values
        foreach (var result in results)
        {
            Assert.That(result, Is.EquivalentTo(new[] { 1, 2 }));
        }
    }

    [Test]
    [Category("Concurrency")]
    public async Task GetOrDefaultAsync_SlowPath_UnderContention_ExecutesCorrectly()
    {
        await _map.AddAsync("key1", 1);

        var tasks = new List<Task<IEnumerable<int>>>();

        for (int i = 0; i < 20; i++)
        {
            tasks.Add(_map.GetOrDefaultAsync("key1").AsTask());
        }

        var results = await Task.WhenAll(tasks);

        foreach (var result in results)
        {
            Assert.That(result, Is.EquivalentTo(new[] { 1 }));
        }
    }

    [Test]
    [Category("Concurrency")]
    public async Task TryGetAsync_SlowPath_UnderContention_ExecutesCorrectly()
    {
        await _map.AddAsync("key1", 1);

        var tasks = new List<Task<(bool found, IEnumerable<int> values)>>();

        for (int i = 0; i < 20; i++)
        {
            tasks.Add(_map.TryGetAsync("key1").AsTask());
        }

        var results = await Task.WhenAll(tasks);

        foreach (var (found, values) in results)
        {
            Assert.That(found, Is.True);
            Assert.That(values, Is.EquivalentTo(new[] { 1 }));
        }
    }

    [Test]
    [Category("Concurrency")]
    public async Task AddAsync_SlowPath_UnderContention_ExecutesCorrectly()
    {
        var tasks = new List<Task<bool>>();

        // Concurrent adds to different keys to create contention
        for (int i = 0; i < 50; i++)
        {
            int value = i;
            tasks.Add(_map.AddAsync($"key{i % 10}", value).AsTask());
        }

        var results = await Task.WhenAll(tasks);

        // Most adds should succeed (first occurrence of each key-value pair)
        Assert.That(results.Count(r => r), Is.GreaterThan(40));
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(50));
    }

    [Test]
    [Category("Concurrency")]
    public async Task AddRangeAsync_SlowPath_UnderContention_ExecutesCorrectly()
    {
        var tasks = new List<Task>();

        for (int i = 0; i < 10; i++)
        {
            int batch = i;
            var pairs = Enumerable.Range(batch * 10, 10)
                .Select(v => new KeyValuePair<string, int>($"key{v % 5}", v))
                .ToList();
            tasks.Add(_map.AddRangeAsync(pairs));
        }

        await Task.WhenAll(tasks);

        Assert.That(await _map.GetCountAsync(), Is.EqualTo(100));
    }

    [Test]
    [Category("Concurrency")]
    public async Task RemoveAsync_SlowPath_UnderContention_ExecutesCorrectly()
    {
        // Pre-populate
        for (int i = 0; i < 50; i++)
            await _map.AddAsync($"key{i % 10}", i);

        var tasks = new List<Task<bool>>();

        for (int i = 0; i < 50; i++)
        {
            int value = i;
            tasks.Add(_map.RemoveAsync($"key{value % 10}", value).AsTask());
        }

        var results = await Task.WhenAll(tasks);

        Assert.That(results.Count(r => r), Is.EqualTo(50));
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(0));
    }

    [Test]
    [Category("Concurrency")]
    public async Task RemoveRangeAsync_SlowPath_UnderContention_ExecutesCorrectly()
    {
        // Pre-populate
        for (int i = 0; i < 100; i++)
            await _map.AddAsync($"key{i % 10}", i);

        var tasks = new List<Task<int>>();

        for (int i = 0; i < 10; i++)
        {
            int batch = i;
            var pairs = Enumerable.Range(batch * 10, 10)
                .Select(v => new KeyValuePair<string, int>($"key{v % 10}", v))
                .ToList();
            tasks.Add(_map.RemoveRangeAsync(pairs).AsTask());
        }

        var results = await Task.WhenAll(tasks);

        Assert.That(results.Sum(), Is.EqualTo(100));
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(0));
    }

    [Test]
    [Category("Concurrency")]
    public async Task RemoveWhereAsync_SlowPath_UnderContention_ExecutesCorrectly()
    {
        // Pre-populate
        for (int i = 0; i < 100; i++)
            await _map.AddAsync($"key{i % 5}", i);

        var tasks = new List<Task<int>>();

        for (int i = 0; i < 5; i++)
        {
            string key = $"key{i}";
            tasks.Add(_map.RemoveWhereAsync(key, v => v % 2 == 0).AsTask());
        }

        var results = await Task.WhenAll(tasks);

        // Should have removed approximately half the values
        Assert.That(results.Sum(), Is.GreaterThan(40));
    }

    [Test]
    [Category("Concurrency")]
    public async Task RemoveKeyAsync_SlowPath_UnderContention_ExecutesCorrectly()
    {
        // Pre-populate
        for (int i = 0; i < 50; i++)
            await _map.AddAsync($"key{i}", i);

        var tasks = new List<Task<bool>>();

        for (int i = 0; i < 50; i++)
        {
            string key = $"key{i}";
            tasks.Add(_map.RemoveKeyAsync(key).AsTask());
        }

        var results = await Task.WhenAll(tasks);

        Assert.That(results.Count(r => r), Is.EqualTo(50));
        Assert.That(await _map.GetCountAsync(), Is.EqualTo(0));
    }

    [Test]
    [Category("Concurrency")]
    public async Task ContainsAsync_SlowPath_UnderContention_ExecutesCorrectly()
    {
        await _map.AddAsync("key1", 1);

        var tasks = new List<Task<bool>>();

        for (int i = 0; i < 20; i++)
        {
            tasks.Add(_map.ContainsAsync("key1", 1).AsTask());
        }

        var results = await Task.WhenAll(tasks);

        Assert.That(results, Has.All.True);
    }

    [Test]
    [Category("Concurrency")]
    public async Task ContainsKeyAsync_SlowPath_UnderContention_ExecutesCorrectly()
    {
        await _map.AddAsync("key1", 1);

        var tasks = new List<Task<bool>>();

        for (int i = 0; i < 20; i++)
        {
            tasks.Add(_map.ContainsKeyAsync("key1").AsTask());
        }

        var results = await Task.WhenAll(tasks);

        Assert.That(results, Has.All.True);
    }

    [Test]
    [Category("Concurrency")]
    public async Task ClearAsync_SlowPath_UnderContention_ExecutesCorrectly()
    {
        // Pre-populate
        for (int i = 0; i < 50; i++)
            await _map.AddAsync($"key{i}", i);

        var tasks = new List<Task>();

        // Multiple concurrent clears
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_map.ClearAsync());
        }

        await Task.WhenAll(tasks);

        Assert.That(await _map.GetCountAsync(), Is.EqualTo(0));
    }

    [Test]
    [Category("Stress")]
    public async Task MixedOperations_SlowPath_HighContention_MaintainsConsistency()
    {
        const int iterations = 100;
        var tasks = new List<Task>();

        for (int i = 0; i < iterations; i++)
        {
            int value = i;
            string key = $"key{value % 10}";

            // Mix of different operations to create contention
            if (value % 5 == 0)
                tasks.Add(_map.AddAsync(key, value).AsTask());
            else if (value % 5 == 1)
                tasks.Add(_map.GetOrDefaultAsync(key).AsTask());
            else if (value % 5 == 2)
                tasks.Add(_map.ContainsAsync(key, value).AsTask());
            else if (value % 5 == 3)
                tasks.Add(_map.RemoveAsync(key, value).AsTask());
            else
                tasks.Add(_map.GetCountAsync().AsTask());
        }

        // All operations should complete without exception
        await Task.WhenAll(tasks);

        Assert.Pass("All mixed operations completed successfully");
    }

    [Test]
    [Category("Stress")]
    public async Task GetKeysAsync_SlowPath_UnderContention_ExecutesCorrectly()
    {
        // Pre-populate
        for (int i = 0; i < 20; i++)
            await _map.AddAsync($"key{i}", i);

        var tasks = new List<Task<IEnumerable<string>>>();

        for (int i = 0; i < 20; i++)
        {
            tasks.Add(_map.GetKeysAsync().AsTask());
        }

        var results = await Task.WhenAll(tasks);

        foreach (var keys in results)
        {
            Assert.That(keys.Count(), Is.EqualTo(20));
        }
    }

    // ── Dispose Guard Tests ──────────────────────────────────

    [Test]
    public void AddAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(async () => await _map.AddAsync("a", 1));
    }

    [Test]
    public void AddRangeAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(async () => await _map.AddRangeAsync("a", new[] { 1, 2 }));
    }

    [Test]
    public void GetAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(async () => await _map.GetAsync("a"));
    }

    [Test]
    public void GetOrDefaultAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(async () => await _map.GetOrDefaultAsync("a"));
    }

    [Test]
    public void TryGetAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(async () => await _map.TryGetAsync("a"));
    }

    [Test]
    public void RemoveAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(async () => await _map.RemoveAsync("a", 1));
    }

    [Test]
    public void RemoveKeyAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(async () => await _map.RemoveKeyAsync("a"));
    }

    [Test]
    public void ContainsKeyAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(async () => await _map.ContainsKeyAsync("a"));
    }

    [Test]
    public void ContainsAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(async () => await _map.ContainsAsync("a", 1));
    }

    [Test]
    public void GetCountAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(async () => await _map.GetCountAsync());
    }

    [Test]
    public void GetKeysAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(async () => await _map.GetKeysAsync());
    }

    [Test]
    public void ClearAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        _map.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(async () => await _map.ClearAsync());
    }
}


