using MultiMap.Entities;
using MultiMap.Helpers;
using MultiMap.Interfaces;

namespace MultyMap.Tests;

[TestFixture]
public class MultiMapHelperTests
{
    private MultiMapSet<string, int> _target;
    private MultiMapSet<string, int> _other;

    [SetUp]
    public void SetUp()
    {
        _target = new MultiMapSet<string, int>();
        _other = new MultiMapSet<string, int>();
    }

    // ── Union ──────────────────────────────────────────────

    [Test]
    public void Union_AddsAllPairsFromOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 2);
        _other.Add("b", 3);

        _target.Union(_other);

        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("a", 2), Is.True);
        Assert.That(_target.Contains("b", 3), Is.True);
        Assert.That(_target.Count, Is.EqualTo(3));
    }

    [Test]
    public void Union_WithEmptyOther_DoesNotChangeTarget()
    {
        _target.Add("a", 1);

        _target.Union(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void Union_WithEmptyTarget_CopiesAllFromOther()
    {
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.Union(_other);

        Assert.That(_target.Count, Is.EqualTo(2));
        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("b", 2), Is.True);
    }

    [Test]
    public void Union_OverlappingPairs_NoDuplicatesInSet()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);

        _target.Union(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void Union_BothEmpty_RemainsEmpty()
    {
        _target.Union(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void Union_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.Union(_other);

        Assert.That(_other.Count, Is.EqualTo(1));
        Assert.That(_other.Contains("a", 1), Is.False);
    }

    [Test]
    public void Union_MultipleKeys_MergesCorrectly()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("b", 3);
        _other.Add("c", 4);

        _target.Union(_other);

        Assert.That(_target.Count, Is.EqualTo(4));
        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("b", 2), Is.True);
        Assert.That(_target.Contains("b", 3), Is.True);
        Assert.That(_target.Contains("c", 4), Is.True);
    }

    // ── Intersect ──────────────────────────────────────────

    [Test]
    public void Intersect_KeepsOnlyCommonPairs()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _target.Add("b", 3);
        _other.Add("a", 1);
        _other.Add("b", 3);

        _target.Intersect(_other);

        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("a", 2), Is.False);
        Assert.That(_target.Contains("b", 3), Is.True);
        Assert.That(_target.Count, Is.EqualTo(2));
    }

    [Test]
    public void Intersect_NoOverlap_ClearsTarget()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.Intersect(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void Intersect_WithEmptyOther_ClearsTarget()
    {
        _target.Add("a", 1);

        _target.Intersect(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void Intersect_WithEmptyTarget_RemainsEmpty()
    {
        _other.Add("a", 1);

        _target.Intersect(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void Intersect_IdenticalMaps_KeepsAll()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.Intersect(_other);

        Assert.That(_target.Count, Is.EqualTo(2));
    }

    [Test]
    public void Intersect_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("a", 1);

        _target.Intersect(_other);

        Assert.That(_other.Count, Is.EqualTo(1));
        Assert.That(_other.Contains("b", 2), Is.False);
    }

    [Test]
    public void Intersect_SameKeySomeValuesMatch_KeepsOnlyMatchingValues()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _target.Add("a", 3);
        _other.Add("a", 2);

        _target.Intersect(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
        Assert.That(_target.Contains("a", 2), Is.True);
    }

    // ── ExceptWith ─────────────────────────────────────────

    [Test]
    public void ExceptWith_RemovesMatchingPairs()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _target.Add("b", 3);
        _other.Add("a", 1);
        _other.Add("b", 3);

        _target.ExceptWith(_other);

        Assert.That(_target.Contains("a", 1), Is.False);
        Assert.That(_target.Contains("a", 2), Is.True);
        Assert.That(_target.Contains("b", 3), Is.False);
        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void ExceptWith_NoOverlap_KeepsAll()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
        Assert.That(_target.Contains("a", 1), Is.True);
    }

    [Test]
    public void ExceptWith_WithEmptyOther_KeepsAll()
    {
        _target.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void ExceptWith_IdenticalMaps_ClearsTarget()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void ExceptWith_WithEmptyTarget_RemainsEmpty()
    {
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void ExceptWith_OtherHasExtraPairs_OnlyRemovesMatching()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
        Assert.That(_target.Contains("b", 2), Is.False);
    }

    [Test]
    public void ExceptWith_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_other.Count, Is.EqualTo(1));
        Assert.That(_other.Contains("a", 1), Is.True);
    }

    // ── SymmetricExceptWith ────────────────────────────────

    [Test]
    public void SymmetricExceptWith_KeepsOnlyUniqueToEach()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _other.Add("a", 2);
        _other.Add("b", 3);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("a", 2), Is.False);
        Assert.That(_target.Contains("b", 3), Is.True);
        Assert.That(_target.Count, Is.EqualTo(2));
    }

    [Test]
    public void SymmetricExceptWith_NoOverlap_UnionsBoth()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(2));
        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("b", 2), Is.True);
    }

    [Test]
    public void SymmetricExceptWith_IdenticalMaps_ClearsTarget()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void SymmetricExceptWith_WithEmptyOther_KeepsAll()
    {
        _target.Add("a", 1);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
        Assert.That(_target.Contains("a", 1), Is.True);
    }

    [Test]
    public void SymmetricExceptWith_WithEmptyTarget_CopiesOther()
    {
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(2));
        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("b", 2), Is.True);
    }

    [Test]
    public void SymmetricExceptWith_BothEmpty_RemainsEmpty()
    {
        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void SymmetricExceptWith_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_other.Count, Is.EqualTo(2));
        Assert.That(_other.Contains("a", 1), Is.True);
        Assert.That(_other.Contains("b", 2), Is.True);
    }
}

[TestFixture]
public class MultiMapHelperWithMultiMapSetTests
{
    private MultiMapSet<string, int> _target;
    private MultiMapSet<string, int> _other;

    [SetUp]
    public void SetUp()
    {
        _target = new MultiMapSet<string, int>();
        _other = new MultiMapSet<string, int>();
    }

    // ── Union ──────────────────────────────────────────────

    [Test]
    public void Union_AddsAllPairsFromOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 2);
        _other.Add("b", 3);

        _target.Union(_other);

        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("a", 2), Is.True);
        Assert.That(_target.Contains("b", 3), Is.True);
        Assert.That(_target.Count, Is.EqualTo(3));
    }

    [Test]
    public void Union_WithEmptyOther_DoesNotChangeTarget()
    {
        _target.Add("a", 1);

        _target.Union(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void Union_WithEmptyTarget_CopiesAllFromOther()
    {
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.Union(_other);

        Assert.That(_target.Count, Is.EqualTo(2));
        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("b", 2), Is.True);
    }

    [Test]
    public void Union_OverlappingPairs_NoDuplicatesInSet()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);

        _target.Union(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void Union_BothEmpty_RemainsEmpty()
    {
        _target.Union(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void Union_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.Union(_other);

        Assert.That(_other.Count, Is.EqualTo(1));
        Assert.That(_other.Contains("a", 1), Is.False);
    }

    [Test]
    public void Union_MultipleKeys_MergesCorrectly()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("b", 3);
        _other.Add("c", 4);

        _target.Union(_other);

        Assert.That(_target.Count, Is.EqualTo(4));
        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("b", 2), Is.True);
        Assert.That(_target.Contains("b", 3), Is.True);
        Assert.That(_target.Contains("c", 4), Is.True);
    }

    // ── Intersect ──────────────────────────────────────────

    [Test]
    public void Intersect_KeepsOnlyCommonPairs()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _target.Add("b", 3);
        _other.Add("a", 1);
        _other.Add("b", 3);

        _target.Intersect(_other);

        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("a", 2), Is.False);
        Assert.That(_target.Contains("b", 3), Is.True);
        Assert.That(_target.Count, Is.EqualTo(2));
    }

    [Test]
    public void Intersect_NoOverlap_ClearsTarget()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.Intersect(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void Intersect_WithEmptyOther_ClearsTarget()
    {
        _target.Add("a", 1);

        _target.Intersect(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void Intersect_WithEmptyTarget_RemainsEmpty()
    {
        _other.Add("a", 1);

        _target.Intersect(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void Intersect_IdenticalMaps_KeepsAll()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.Intersect(_other);

        Assert.That(_target.Count, Is.EqualTo(2));
    }

    [Test]
    public void Intersect_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("a", 1);

        _target.Intersect(_other);

        Assert.That(_other.Count, Is.EqualTo(1));
        Assert.That(_other.Contains("b", 2), Is.False);
    }

    [Test]
    public void Intersect_SameKeySomeValuesMatch_KeepsOnlyMatchingValues()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _target.Add("a", 3);
        _other.Add("a", 2);

        _target.Intersect(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
        Assert.That(_target.Contains("a", 2), Is.True);
    }

    // ── ExceptWith ─────────────────────────────────────────

    [Test]
    public void ExceptWith_RemovesMatchingPairs()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _target.Add("b", 3);
        _other.Add("a", 1);
        _other.Add("b", 3);

        _target.ExceptWith(_other);

        Assert.That(_target.Contains("a", 1), Is.False);
        Assert.That(_target.Contains("a", 2), Is.True);
        Assert.That(_target.Contains("b", 3), Is.False);
        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void ExceptWith_NoOverlap_KeepsAll()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
        Assert.That(_target.Contains("a", 1), Is.True);
    }

    [Test]
    public void ExceptWith_WithEmptyOther_KeepsAll()
    {
        _target.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void ExceptWith_IdenticalMaps_ClearsTarget()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void ExceptWith_WithEmptyTarget_RemainsEmpty()
    {
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void ExceptWith_OtherHasExtraPairs_OnlyRemovesMatching()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
        Assert.That(_target.Contains("b", 2), Is.False);
    }

    [Test]
    public void ExceptWith_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_other.Count, Is.EqualTo(1));
        Assert.That(_other.Contains("a", 1), Is.True);
    }

    // ── SymmetricExceptWith ────────────────────────────────

    [Test]
    public void SymmetricExceptWith_KeepsOnlyUniqueToEach()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _other.Add("a", 2);
        _other.Add("b", 3);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("a", 2), Is.False);
        Assert.That(_target.Contains("b", 3), Is.True);
        Assert.That(_target.Count, Is.EqualTo(2));
    }

    [Test]
    public void SymmetricExceptWith_NoOverlap_UnionsBoth()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(2));
        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("b", 2), Is.True);
    }

    [Test]
    public void SymmetricExceptWith_IdenticalMaps_ClearsTarget()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void SymmetricExceptWith_WithEmptyOther_KeepsAll()
    {
        _target.Add("a", 1);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
        Assert.That(_target.Contains("a", 1), Is.True);
    }

    [Test]
    public void SymmetricExceptWith_WithEmptyTarget_CopiesOther()
    {
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(2));
        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("b", 2), Is.True);
    }

    [Test]
    public void SymmetricExceptWith_BothEmpty_RemainsEmpty()
    {
        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void SymmetricExceptWith_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_other.Count, Is.EqualTo(2));
        Assert.That(_other.Contains("a", 1), Is.True);
        Assert.That(_other.Contains("b", 2), Is.True);
    }
}

[TestFixture]
public class MultiMapHelperWithSortedMultiMapTests
{
    private SortedMultiMap<string, int> _target;
    private SortedMultiMap<string, int> _other;

    [SetUp]
    public void SetUp()
    {
        _target = new SortedMultiMap<string, int>();
        _other = new SortedMultiMap<string, int>();
    }

    [Test]
    public void Union_AddsAllPairsFromOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 2);
        _other.Add("b", 3);

        _target.Union(_other);

        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("a", 2), Is.True);
        Assert.That(_target.Contains("b", 3), Is.True);
        Assert.That(_target.Count, Is.EqualTo(3));
    }

    [Test]
    public void Intersect_KeepsOnlyCommonPairs()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _target.Add("b", 3);
        _other.Add("a", 1);
        _other.Add("b", 3);

        _target.Intersect(_other);

        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("a", 2), Is.False);
        Assert.That(_target.Count, Is.EqualTo(2));
    }

    [Test]
    public void ExceptWith_RemovesMatchingPairs()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_target.Contains("a", 1), Is.False);
        Assert.That(_target.Contains("a", 2), Is.True);
        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void SymmetricExceptWith_KeepsOnlyUniqueToEach()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _other.Add("a", 2);
        _other.Add("b", 3);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("a", 2), Is.False);
        Assert.That(_target.Contains("b", 3), Is.True);
        Assert.That(_target.Count, Is.EqualTo(2));
    }
}

[TestFixture]
public class MultiMapHelperWithConcurrentMultiMapTests
{
    private ConcurrentMultiMap<string, int> _target;
    private ConcurrentMultiMap<string, int> _other;

    [SetUp]
    public void SetUp()
    {
        _target = new ConcurrentMultiMap<string, int>();
        _other = new ConcurrentMultiMap<string, int>();
    }

    [Test]
    public void Union_AddsAllPairsFromOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 2);
        _other.Add("b", 3);

        _target.Union(_other);

        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("a", 2), Is.True);
        Assert.That(_target.Contains("b", 3), Is.True);
        Assert.That(_target.Count, Is.EqualTo(3));
    }

    [Test]
    public void Intersect_KeepsOnlyCommonPairs()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _target.Add("b", 3);
        _other.Add("a", 1);
        _other.Add("b", 3);

        _target.Intersect(_other);

        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("a", 2), Is.False);
        Assert.That(_target.Count, Is.EqualTo(2));
    }

    [Test]
    public void ExceptWith_RemovesMatchingPairs()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_target.Contains("a", 1), Is.False);
        Assert.That(_target.Contains("a", 2), Is.True);
        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void SymmetricExceptWith_KeepsOnlyUniqueToEach()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _other.Add("a", 2);
        _other.Add("b", 3);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("a", 2), Is.False);
        Assert.That(_target.Contains("b", 3), Is.True);
        Assert.That(_target.Count, Is.EqualTo(2));
    }
}

[TestFixture]
public class MultiMapHelperWithMultiMapListTests
{
    private MultiMapList<string, int> _target;
    private MultiMapList<string, int> _other;

    [SetUp]
    public void SetUp()
    {
        _target = new MultiMapList<string, int>();
        _other = new MultiMapList<string, int>();
    }

    [Test]
    public void Union_AddsAllPairsFromOther()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.Union(_other);

        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("b", 2), Is.True);
        Assert.That(_target.Count, Is.EqualTo(2));
    }

    [Test]
    public void Intersect_KeepsOnlyCommonPairs()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _target.Add("b", 3);
        _other.Add("a", 1);

        _target.Intersect(_other);

        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("a", 2), Is.False);
        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void ExceptWith_RemovesMatchingPairs()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_target.Contains("a", 1), Is.False);
        Assert.That(_target.Contains("b", 2), Is.True);
        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void SymmetricExceptWith_KeepsOnlyUniqueToEach()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Contains("a", 1), Is.False);
        Assert.That(_target.Contains("b", 2), Is.True);
        Assert.That(_target.Count, Is.EqualTo(1));
    }
}

[TestFixture]
public class MultiMapHelperWithMultiMapLockTests
{
    private MultiMapLock<string, int> _target;
    private MultiMapLock<string, int> _other;

    [SetUp]
    public void SetUp()
    {
        _target = new MultiMapLock<string, int>();
        _other = new MultiMapLock<string, int>();
    }

    [TearDown]
    public void TearDown()
    {
        _target.Dispose();
        _other.Dispose();
    }

    [Test]
    public void Union_AddsAllPairsFromOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 2);
        _other.Add("b", 3);

        _target.Union(_other);

        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("a", 2), Is.True);
        Assert.That(_target.Contains("b", 3), Is.True);
        Assert.That(_target.Count, Is.EqualTo(3));
    }

    [Test]
    public void Intersect_KeepsOnlyCommonPairs()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _other.Add("a", 1);

        _target.Intersect(_other);

        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("a", 2), Is.False);
        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void ExceptWith_RemovesMatchingPairs()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_target.Contains("a", 1), Is.False);
        Assert.That(_target.Contains("a", 2), Is.True);
        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void SymmetricExceptWith_KeepsOnlyUniqueToEach()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _other.Add("a", 2);
        _other.Add("b", 3);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Contains("a", 1), Is.True);
        Assert.That(_target.Contains("a", 2), Is.False);
        Assert.That(_target.Contains("b", 3), Is.True);
        Assert.That(_target.Count, Is.EqualTo(2));
    }
}

[TestFixture]
public class SimpleMultiMapHelperTests
{
    private ISimpleMultiMap<string, int> _target;
    private ISimpleMultiMap<string, int> _other;

    [SetUp]
    public void SetUp()
    {
        _target = new SimpleMultiMap<string, int>();
        _other = new SimpleMultiMap<string, int>();
    }

    // ── Union ──────────────────────────────────────────────

    [Test]
    public void Union_AddsAllPairsFromOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 2);
        _other.Add("b", 3);

        _target = _target.Union(_other);

        Assert.That(_target.GetOrDefault("a"), Is.EquivalentTo(new[] { 1, 2 }));
        Assert.That(_target.GetOrDefault("b"), Is.EquivalentTo(new[] { 3 }));
    }

    [Test]
    public void Union_WithEmptyOther_DoesNotChangeTarget()
    {
        _target.Add("a", 1);

        _target.Union(_other);

        Assert.That(_target.Flatten().Count(), Is.EqualTo(1));
    }

    [Test]
    public void Union_WithEmptyTarget_CopiesAllFromOther()
    {
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.Union(_other);

        Assert.That(_target.Flatten().Count(), Is.EqualTo(2));
        Assert.That(_target.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(_target.GetOrDefault("b"), Is.EquivalentTo(new[] { 2 }));
    }

    [Test]
    public void Union_OverlappingPairs_NoDuplicatesInSet()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);

        _target.Union(_other);

        Assert.That(_target.Flatten().Count(), Is.EqualTo(1));
    }

    [Test]
    public void Union_BothEmpty_RemainsEmpty()
    {
        _target.Union(_other);

        Assert.That(_target.Flatten(), Is.Empty);
    }

    [Test]
    public void Union_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.Union(_other);

        Assert.That(_other.Flatten().Count(), Is.EqualTo(1));
        Assert.That(_other.GetOrDefault("a"), Is.Empty);
    }

    [Test]
    public void Union_MultipleKeys_MergesCorrectly()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("b", 3);
        _other.Add("c", 4);

        _target.Union(_other);

        Assert.That(_target.Flatten().Count(), Is.EqualTo(4));
        Assert.That(_target.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(_target.GetOrDefault("b"), Is.EquivalentTo(new[] { 2, 3 }));
        Assert.That(_target.GetOrDefault("c"), Is.EquivalentTo(new[] { 4 }));
    }

    // ── Intersect ──────────────────────────────────────────

    [Test]
    public void Intersect_KeepsOnlyCommonPairs()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _target.Add("b", 3);
        _other.Add("a", 1);
        _other.Add("b", 3);

        _target = _target.Intersect(_other);

        Assert.That(_target.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(_target.GetOrDefault("b"), Is.EquivalentTo(new[] { 3 }));
        Assert.That(_target.Flatten().Count(), Is.EqualTo(2));
    }

    [Test]
    public void Intersect_NoOverlap_ClearsTarget()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target = _target.Intersect(_other);

        Assert.That(_target.Flatten(), Is.Empty);
    }

    [Test]
    public void Intersect_WithEmptyOther_ClearsTarget()
    {
        _target.Add("a", 1);

        _target = _target.Intersect(_other);

        Assert.That(_target.Flatten(), Is.Empty);
    }

    [Test]
    public void Intersect_WithEmptyTarget_RemainsEmpty()
    {
        _other.Add("a", 1);

        _target = _target.Intersect(_other);

        Assert.That(_target.Flatten(), Is.Empty);
    }

    [Test]
    public void Intersect_IdenticalMaps_KeepsAll()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target = _target.Intersect(_other);

        Assert.That(_target.Flatten().Count(), Is.EqualTo(2));
    }

    [Test]
    public void Intersect_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("a", 1);

        _target = _target.Intersect(_other);

        Assert.That(_other.Flatten().Count(), Is.EqualTo(1));
    }

    [Test]
    public void Intersect_SameKeySomeValuesMatch_KeepsOnlyMatchingValues()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _target.Add("a", 3);
        _other.Add("a", 2);

        _target = _target.Intersect(_other);

        Assert.That(_target.Flatten().Count(), Is.EqualTo(1));
        Assert.That(_target.GetOrDefault("a"), Is.EquivalentTo(new[] { 2 }));
    }

    // ── ExceptWith ─────────────────────────────────────────

    [Test]
    public void ExceptWith_RemovesMatchingPairs()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _target.Add("b", 3);
        _other.Add("a", 1);
        _other.Add("b", 3);

        _target.ExceptWith(_other);

        Assert.That(_target.GetOrDefault("a"), Is.EquivalentTo(new[] { 2 }));
        Assert.That(_target.GetOrDefault("b"), Is.Empty);
        Assert.That(_target.Flatten().Count(), Is.EqualTo(1));
    }

    [Test]
    public void ExceptWith_NoOverlap_KeepsAll()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.ExceptWith(_other);

        Assert.That(_target.Flatten().Count(), Is.EqualTo(1));
        Assert.That(_target.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void ExceptWith_WithEmptyOther_KeepsAll()
    {
        _target.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_target.Flatten().Count(), Is.EqualTo(1));
    }

    [Test]
    public void ExceptWith_IdenticalMaps_ClearsTarget()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_target.Flatten(), Is.Empty);
    }

    [Test]
    public void ExceptWith_WithEmptyTarget_RemainsEmpty()
    {
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_target.Flatten(), Is.Empty);
    }

    [Test]
    public void ExceptWith_OtherHasExtraPairs_OnlyRemovesMatching()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.ExceptWith(_other);

        Assert.That(_target.Flatten(), Is.Empty);
    }

    [Test]
    public void ExceptWith_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_other.Flatten().Count(), Is.EqualTo(1));
        Assert.That(_other.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
    }

    // ── SymmetricExceptWith ────────────────────────────────

    [Test]
    public void SymmetricExceptWith_KeepsOnlyUniqueToEach()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _other.Add("a", 2);
        _other.Add("b", 3);

        _target = _target.SymmetricExceptWith(_other);

        Assert.That(_target.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(_target.GetOrDefault("b"), Is.EquivalentTo(new[] { 3 }));
        Assert.That(_target.Flatten().Count(), Is.EqualTo(2));
    }

    [Test]
    public void SymmetricExceptWith_NoOverlap_UnionsBoth()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target = _target.SymmetricExceptWith(_other);

        Assert.That(_target.Flatten().Count(), Is.EqualTo(2));
        Assert.That(_target.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(_target.GetOrDefault("b"), Is.EquivalentTo(new[] { 2 }));
    }

    [Test]
    public void SymmetricExceptWith_IdenticalMaps_ClearsTarget()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target = _target.SymmetricExceptWith(_other);

        Assert.That(_target.Flatten(), Is.Empty);
    }

    [Test]
    public void SymmetricExceptWith_WithEmptyOther_KeepsAll()
    {
        _target.Add("a", 1);

        _target = _target.SymmetricExceptWith(_other);

        Assert.That(_target.Flatten().Count(), Is.EqualTo(1));
        Assert.That(_target.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void SymmetricExceptWith_WithEmptyTarget_CopiesOther()
    {
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target = _target.SymmetricExceptWith(_other);

        Assert.That(_target.Flatten().Count(), Is.EqualTo(2));
        Assert.That(_target.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(_target.GetOrDefault("b"), Is.EquivalentTo(new[] { 2 }));
    }

    [Test]
    public void SymmetricExceptWith_BothEmpty_RemainsEmpty()
    {
        _target = _target.SymmetricExceptWith(_other);

        Assert.That(_target.Flatten(), Is.Empty);
    }

    [Test]
    public void SymmetricExceptWith_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target = _target.SymmetricExceptWith(_other);

        Assert.That(_other.Flatten().Count(), Is.EqualTo(2));
        Assert.That(_other.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(_other.GetOrDefault("b"), Is.EquivalentTo(new[] { 2 }));
    }
}
