using MultiMap.Entities;
using MultiMap.Helpers;

namespace MultiMap.Tests;

/// <summary>
/// Tests for MultiMapHelper extension async methods.
/// These are different from the instance methods on MultiMapAsync.
/// </summary>
[TestFixture]
public class MultiMapHelperExtensionAsyncTests
{
    private MultiMapAsync<string, int> _target;
    private MultiMapAsync<string, int> _other;

    [SetUp]
    public void SetUp()
    {
        _target = new MultiMapAsync<string, int>();
        _other = new MultiMapAsync<string, int>();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _target.DisposeAsync();
        await _other.DisposeAsync();
    }

    #region UnionAsync Extension Method Tests

    [Test]
    public async Task UnionAsync_Extension_AddsAllPairsFromOther()
    {
        await _target.AddAsync("a", 1);
        await _other.AddAsync("a", 2);
        await _other.AddAsync("b", 3);

        await MultiMapHelper.UnionAsync(_target, _other);

        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
        Assert.That(await _target.ContainsAsync("a", 2), Is.True);
        Assert.That(await _target.ContainsAsync("b", 3), Is.True);
        Assert.That(await _target.GetCountAsync(), Is.EqualTo(3));
    }

    [Test]
    public async Task UnionAsync_Extension_WithEmptyOther_DoesNotChangeTarget()
    {
        await _target.AddAsync("a", 1);

        await MultiMapHelper.UnionAsync(_target, _other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task UnionAsync_Extension_WithEmptyTarget_CopiesAllFromOther()
    {
        await _other.AddAsync("a", 1);
        await _other.AddAsync("b", 2);

        await MultiMapHelper.UnionAsync(_target, _other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(2));
        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
        Assert.That(await _target.ContainsAsync("b", 2), Is.True);
    }

    [Test]
    public async Task UnionAsync_Extension_OverlappingPairs_NoDuplicates()
    {
        await _target.AddAsync("a", 1);
        await _other.AddAsync("a", 1);

        await MultiMapHelper.UnionAsync(_target, _other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task UnionAsync_Extension_BothEmpty_RemainsEmpty()
    {
        await MultiMapHelper.UnionAsync(_target, _other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task UnionAsync_Extension_MultipleKeys_MergesCorrectly()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("b", 2);
        await _other.AddAsync("b", 3);
        await _other.AddAsync("c", 4);

        await MultiMapHelper.UnionAsync(_target, _other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(4));
        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
        Assert.That(await _target.ContainsAsync("b", 2), Is.True);
        Assert.That(await _target.ContainsAsync("b", 3), Is.True);
        Assert.That(await _target.ContainsAsync("c", 4), Is.True);
    }

    [Test]
    public async Task UnionAsync_Extension_WithCancellationToken_CanBeCancelled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        for (int i = 0; i < 100; i++)
        {
            await _other.AddAsync($"key{i}", i);
        }

        var ex = Assert.CatchAsync<OperationCanceledException>(async () =>
        {
            await MultiMapHelper.UnionAsync(_target, _other, cts.Token);
        });

        Assert.That(ex, Is.Not.Null);
    }

    #endregion

    #region IntersectAsync Extension Method Tests

    [Test]
    public async Task IntersectAsync_Extension_KeepsOnlyCommonPairs()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("a", 2);
        await _target.AddAsync("b", 3);
        await _other.AddAsync("a", 1);
        await _other.AddAsync("b", 3);

        await MultiMapHelper.IntersectAsync(_target, _other);

        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
        Assert.That(await _target.ContainsAsync("a", 2), Is.False);
        Assert.That(await _target.ContainsAsync("b", 3), Is.True);
        Assert.That(await _target.GetCountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task IntersectAsync_Extension_NoOverlap_ClearsTarget()
    {
        await _target.AddAsync("a", 1);
        await _other.AddAsync("b", 2);

        await MultiMapHelper.IntersectAsync(_target, _other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task IntersectAsync_Extension_WithEmptyOther_ClearsTarget()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("b", 2);

        await MultiMapHelper.IntersectAsync(_target, _other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task IntersectAsync_Extension_WithEmptyTarget_RemainsEmpty()
    {
        await _other.AddAsync("a", 1);

        await MultiMapHelper.IntersectAsync(_target, _other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task IntersectAsync_Extension_PartialOverlap_KeepsCommonOnly()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("a", 2);
        await _target.AddAsync("b", 3);
        await _target.AddAsync("c", 4);
        await _other.AddAsync("a", 1);
        await _other.AddAsync("c", 4);

        await MultiMapHelper.IntersectAsync(_target, _other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(2));
        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
        Assert.That(await _target.ContainsAsync("c", 4), Is.True);
    }

    [Test]
    public async Task IntersectAsync_Extension_WithCancellationToken_CanBeCancelled()
    {
        using var cts = new CancellationTokenSource();

        for (int i = 0; i < 100; i++)
        {
            await _target.AddAsync($"key{i}", i);
            await _other.AddAsync($"key{i}", i);
        }

        cts.Cancel();

        var ex = Assert.CatchAsync<OperationCanceledException>(async () =>
        {
            await MultiMapHelper.IntersectAsync(_target, _other, cts.Token);
        });

        Assert.That(ex, Is.Not.Null);
    }

    #endregion

    #region ExceptWithAsync Extension Method Tests

    [Test]
    public async Task ExceptWithAsync_Extension_RemovesCommonPairs()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("a", 2);
        await _target.AddAsync("b", 3);
        await _other.AddAsync("a", 1);
        await _other.AddAsync("b", 3);

        await MultiMapHelper.ExceptWithAsync(_target, _other);

        Assert.That(await _target.ContainsAsync("a", 1), Is.False);
        Assert.That(await _target.ContainsAsync("a", 2), Is.True);
        Assert.That(await _target.ContainsAsync("b", 3), Is.False);
        Assert.That(await _target.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task ExceptWithAsync_Extension_NoOverlap_NoChange()
    {
        await _target.AddAsync("a", 1);
        await _other.AddAsync("b", 2);

        await MultiMapHelper.ExceptWithAsync(_target, _other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(1));
        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
    }

    [Test]
    public async Task ExceptWithAsync_Extension_WithEmptyOther_NoChange()
    {
        await _target.AddAsync("a", 1);

        await MultiMapHelper.ExceptWithAsync(_target, _other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task ExceptWithAsync_Extension_WithEmptyTarget_RemainsEmpty()
    {
        await _other.AddAsync("a", 1);

        await MultiMapHelper.ExceptWithAsync(_target, _other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task ExceptWithAsync_Extension_AllPairsInOther_ClearsTarget()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("b", 2);
        await _other.AddAsync("a", 1);
        await _other.AddAsync("b", 2);

        await MultiMapHelper.ExceptWithAsync(_target, _other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task ExceptWithAsync_Extension_WithCancellationToken_CanBeCancelled()
    {
        using var cts = new CancellationTokenSource();

        for (int i = 0; i < 100; i++)
        {
            await _target.AddAsync($"key{i}", i);
            await _other.AddAsync($"key{i}", i);
        }

        cts.Cancel();

        var ex = Assert.CatchAsync<OperationCanceledException>(async () =>
        {
            await MultiMapHelper.ExceptWithAsync(_target, _other, cts.Token);
        });

        Assert.That(ex, Is.Not.Null);
    }

    #endregion

    #region SymmetricExceptWithAsync Extension Method Tests

    [Test]
    public async Task SymmetricExceptWithAsync_Extension_KeepsNonOverlappingPairs()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("b", 2);
        await _other.AddAsync("a", 1);
        await _other.AddAsync("c", 3);

        await MultiMapHelper.SymmetricExceptWithAsync(_target, _other);

        Assert.That(await _target.ContainsAsync("a", 1), Is.False);
        Assert.That(await _target.ContainsAsync("b", 2), Is.True);
        Assert.That(await _target.ContainsAsync("c", 3), Is.True);
        Assert.That(await _target.GetCountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task SymmetricExceptWithAsync_Extension_NoOverlap_MergesAll()
    {
        await _target.AddAsync("a", 1);
        await _other.AddAsync("b", 2);

        await MultiMapHelper.SymmetricExceptWithAsync(_target, _other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(2));
        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
        Assert.That(await _target.ContainsAsync("b", 2), Is.True);
    }

    [Test]
    public async Task SymmetricExceptWithAsync_Extension_WithEmptyOther_NoChange()
    {
        await _target.AddAsync("a", 1);

        await MultiMapHelper.SymmetricExceptWithAsync(_target, _other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task SymmetricExceptWithAsync_Extension_WithEmptyTarget_CopiesFromOther()
    {
        await _other.AddAsync("a", 1);
        await _other.AddAsync("b", 2);

        await MultiMapHelper.SymmetricExceptWithAsync(_target, _other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(2));
        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
        Assert.That(await _target.ContainsAsync("b", 2), Is.True);
    }

    [Test]
    public async Task SymmetricExceptWithAsync_Extension_IdenticalMaps_ClearsTarget()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("b", 2);
        await _other.AddAsync("a", 1);
        await _other.AddAsync("b", 2);

        await MultiMapHelper.SymmetricExceptWithAsync(_target, _other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task SymmetricExceptWithAsync_Extension_WithCancellationToken_CanBeCancelled()
    {
        using var cts = new CancellationTokenSource();

        for (int i = 0; i < 100; i++)
        {
            await _target.AddAsync($"key{i}", i);
            await _other.AddAsync($"key{i}", i + 50);
        }

        cts.Cancel();

        var ex = Assert.CatchAsync<OperationCanceledException>(async () =>
        {
            await MultiMapHelper.SymmetricExceptWithAsync(_target, _other, cts.Token);
        });

        Assert.That(ex, Is.Not.Null);
    }

    #endregion
}
