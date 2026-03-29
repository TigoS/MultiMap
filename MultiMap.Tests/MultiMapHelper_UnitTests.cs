using MultiMap.Entities;
using MultiMap.Helpers;
using MultiMap.Interfaces;

namespace MultiMap.Tests;

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

    // ── Stress tests ────────────────────────────────────────

    [Test]
    public void Stress_UnionRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            var source = new ConcurrentMultiMap<string, int>();
            for (int i = 0; i < 10; i++)
                source.Add($"k{i % 3}", cycle * 10 + i);

            _target.Union(source);

            int enumerated = _target.Count();
            Assert.That(_target.Count, Is.EqualTo(enumerated),
                $"Count mismatch after union in cycle {cycle}");
        }

        Assert.That(_target.Count, Is.EqualTo(300));
    }

    [Test]
    public void Stress_IntersectRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            for (int i = 0; i < 10; i++)
                _target.Add("a", i);

            var mask = new ConcurrentMultiMap<string, int>();
            for (int i = 0; i < 5; i++)
                mask.Add("a", i);

            _target.Intersect(mask);

            Assert.That(_target.Count, Is.EqualTo(5),
                $"Count wrong after intersect in cycle {cycle}");
        }
    }

    [Test]
    public void Stress_ExceptWithRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            for (int i = 0; i < 10; i++)
                _target.Add("a", i);

            var removal = new ConcurrentMultiMap<string, int>();
            for (int i = 0; i < 5; i++)
                removal.Add("a", i);

            _target.ExceptWith(removal);

            Assert.That(_target.Count, Is.EqualTo(5),
                $"Count wrong after except in cycle {cycle}");
        }
    }

    [Test]
    public void Stress_SymmetricExceptWithRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            _target.Add("a", 1);
            _target.Add("a", 2);
            _target.Add("b", 3);

            var sym = new ConcurrentMultiMap<string, int>();
            sym.Add("a", 2);
            sym.Add("c", 4);

            _target.SymmetricExceptWith(sym);

            Assert.That(_target.Contains("a", 1), Is.True, $"cycle {cycle}");
            Assert.That(_target.Contains("a", 2), Is.False, $"cycle {cycle}");
            Assert.That(_target.Contains("b", 3), Is.True, $"cycle {cycle}");
            Assert.That(_target.Contains("c", 4), Is.True, $"cycle {cycle}");
            Assert.That(_target.Count, Is.EqualTo(3), $"cycle {cycle}");
        }
    }

    [Test]
    public void Stress_UnionThenExcept_RoundTrip_CountReturnsToOriginal()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            for (int i = 0; i < 5; i++)
                _target.Add("a", i);

            var extra = new ConcurrentMultiMap<string, int>();
            for (int i = 5; i < 10; i++)
                extra.Add("a", i);

            _target.Union(extra);
            Assert.That(_target.Count, Is.EqualTo(10),
                $"Count wrong after union in cycle {cycle}");

            _target.ExceptWith(extra);
            Assert.That(_target.Count, Is.EqualTo(5),
                $"Count wrong after except round-trip in cycle {cycle}");
        }
    }

    [Test]
    public void Stress_ConcurrentHelperOperations_CountNeverNegative()
    {
        for (int i = 0; i < 20; i++)
            _target.Add($"k{i % 4}", i);

        const int iterations = 50;

        var tasks = Enumerable.Range(0, iterations).Select(i => Task.Run(() =>
        {
            var temp = new ConcurrentMultiMap<string, int>();
            temp.Add($"k{i % 4}", i);
            temp.Add($"k{(i + 1) % 4}", i + 1000);

            switch (i % 4)
            {
                case 0: _target.Union(temp); break;
                case 1: _target.ExceptWith(temp); break;
                case 2: _target.Intersect(temp); break;
                case 3: _target.SymmetricExceptWith(temp); break;
            }
        })).ToArray();

        Task.WaitAll(tasks);

        int count = _target.Count;
        Assert.That(count, Is.GreaterThanOrEqualTo(0),
            "Count must never be negative");

        int verifyCount = _target.Count();
        Assert.That(count, Is.EqualTo(verifyCount),
            "Count must match enumerated total");
    }

    [Test]
    public void Stress_UnionUnderConcurrentMutation_NoCorruption()
    {
        using var cts = new CancellationTokenSource();

        var mutationTask = Task.Run(() =>
        {
            int v = 0;
            while (!cts.IsCancellationRequested)
            {
                _target.Add($"m{v % 5}", v);
                v++;
                if (v % 10 == 0)
                    _target.RemoveKey($"m{v % 5}");
            }
        });

        for (int round = 0; round < 20; round++)
        {
            var source = new ConcurrentMultiMap<string, int>();
            for (int i = 0; i < 5; i++)
                source.Add($"u{i}", round * 100 + i);

            _target.Union(source);
        }

        cts.Cancel();
        try { mutationTask.Wait(); } catch (AggregateException) { }

        int count = _target.Count;
        Assert.That(count, Is.GreaterThanOrEqualTo(0));

        int verifyCount = _target.Count();
        Assert.That(count, Is.EqualTo(verifyCount),
            "Final count must match enumerated total");
    }

    [Test]
    public void Stress_IntersectAndSymmetric_AlternatingCycles_CountTracksCorrectly()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            for (int i = 0; i < 10; i++)
                _target.Add("x", i);

            var operand = new ConcurrentMultiMap<string, int>();

            if (cycle % 2 == 0)
            {
                for (int i = 0; i < 5; i++)
                    operand.Add("x", i);

                _target.Intersect(operand);

                Assert.That(_target.Count, Is.EqualTo(5),
                    $"Count wrong after intersect in cycle {cycle}");
            }
            else
            {
                for (int i = 5; i < 15; i++)
                    operand.Add("x", i);

                _target.SymmetricExceptWith(operand);

                int count = _target.Count;
                int enumerated = _target.Count();
                Assert.That(count, Is.EqualTo(enumerated),
                    $"Count mismatch in cycle {cycle}");
            }
        }
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

    // ── Stress tests ────────────────────────────────────────

    [Test]
    public void Stress_UnionRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            using var source = new MultiMapLock<string, int>();
            for (int i = 0; i < 10; i++)
                source.Add($"k{i % 3}", cycle * 10 + i);

            _target.Union(source);

            int enumerated = _target.Count();
            Assert.That(_target.Count, Is.EqualTo(enumerated),
                $"Count mismatch after union in cycle {cycle}");
        }

        Assert.That(_target.Count, Is.EqualTo(300));
    }

    [Test]
    public void Stress_IntersectRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            for (int i = 0; i < 10; i++)
                _target.Add("a", i);

            using var mask = new MultiMapLock<string, int>();
            for (int i = 0; i < 5; i++)
                mask.Add("a", i);

            _target.Intersect(mask);

            Assert.That(_target.Count, Is.EqualTo(5),
                $"Count wrong after intersect in cycle {cycle}");
        }
    }

    [Test]
    public void Stress_ExceptWithRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            for (int i = 0; i < 10; i++)
                _target.Add("a", i);

            using var removal = new MultiMapLock<string, int>();
            for (int i = 0; i < 5; i++)
                removal.Add("a", i);

            _target.ExceptWith(removal);

            Assert.That(_target.Count, Is.EqualTo(5),
                $"Count wrong after except in cycle {cycle}");
        }
    }

    [Test]
    public void Stress_SymmetricExceptWithRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            _target.Add("a", 1);
            _target.Add("a", 2);
            _target.Add("b", 3);

            using var sym = new MultiMapLock<string, int>();
            sym.Add("a", 2);
            sym.Add("c", 4);

            _target.SymmetricExceptWith(sym);

            Assert.That(_target.Contains("a", 1), Is.True, $"cycle {cycle}");
            Assert.That(_target.Contains("a", 2), Is.False, $"cycle {cycle}");
            Assert.That(_target.Contains("b", 3), Is.True, $"cycle {cycle}");
            Assert.That(_target.Contains("c", 4), Is.True, $"cycle {cycle}");
            Assert.That(_target.Count, Is.EqualTo(3), $"cycle {cycle}");
        }
    }

    [Test]
    public void Stress_UnionThenExcept_RoundTrip_CountReturnsToOriginal()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            for (int i = 0; i < 5; i++)
                _target.Add("a", i);

            using var extra = new MultiMapLock<string, int>();
            for (int i = 5; i < 10; i++)
                extra.Add("a", i);

            _target.Union(extra);
            Assert.That(_target.Count, Is.EqualTo(10),
                $"Count wrong after union in cycle {cycle}");

            _target.ExceptWith(extra);
            Assert.That(_target.Count, Is.EqualTo(5),
                $"Count wrong after except round-trip in cycle {cycle}");
        }
    }

    [Test]
    public void Stress_ConcurrentHelperOperations_CountNeverNegative()
    {
        for (int i = 0; i < 20; i++)
            _target.Add($"k{i % 4}", i);

        const int iterations = 50;

        var tasks = Enumerable.Range(0, iterations).Select(i => Task.Run(() =>
        {
            using var temp = new MultiMapLock<string, int>();
            temp.Add($"k{i % 4}", i);
            temp.Add($"k{(i + 1) % 4}", i + 1000);

            switch (i % 4)
            {
                case 0: _target.Union(temp); break;
                case 1: _target.ExceptWith(temp); break;
                case 2: _target.Intersect(temp); break;
                case 3: _target.SymmetricExceptWith(temp); break;
            }
        })).ToArray();

        Task.WaitAll(tasks);

        int count = _target.Count;
        Assert.That(count, Is.GreaterThanOrEqualTo(0),
            "Count must never be negative");

        int verifyCount = _target.Count();
        Assert.That(count, Is.EqualTo(verifyCount),
            "Count must match enumerated total");
    }

    [Test]
    public void Stress_UnionUnderConcurrentMutation_NoCorruption()
    {
        using var cts = new CancellationTokenSource();

        var mutationTask = Task.Run(() =>
        {
            int v = 0;
            while (!cts.IsCancellationRequested)
            {
                _target.Add($"m{v % 5}", v);
                v++;
                if (v % 10 == 0)
                    _target.RemoveKey($"m{v % 5}");
            }
        });

        for (int round = 0; round < 20; round++)
        {
            using var source = new MultiMapLock<string, int>();
            for (int i = 0; i < 5; i++)
                source.Add($"u{i}", round * 100 + i);

            _target.Union(source);
        }

        cts.Cancel();
        try { mutationTask.Wait(); } catch (AggregateException) { }

        int count = _target.Count;
        Assert.That(count, Is.GreaterThanOrEqualTo(0));

        int verifyCount = _target.Count();
        Assert.That(count, Is.EqualTo(verifyCount),
            "Final count must match enumerated total");
    }

    [Test]
    public void Stress_IntersectAndSymmetric_AlternatingCycles_CountTracksCorrectly()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            for (int i = 0; i < 10; i++)
                _target.Add("x", i);

            if (cycle % 2 == 0)
            {
                using var operand = new MultiMapLock<string, int>();
                for (int i = 0; i < 5; i++)
                    operand.Add("x", i);

                _target.Intersect(operand);

                Assert.That(_target.Count, Is.EqualTo(5),
                    $"Count wrong after intersect in cycle {cycle}");
            }
            else
            {
                using var operand = new MultiMapLock<string, int>();
                for (int i = 5; i < 15; i++)
                    operand.Add("x", i);

                _target.SymmetricExceptWith(operand);

                int count = _target.Count;
                int enumerated = _target.Count();
                Assert.That(count, Is.EqualTo(enumerated),
                    $"Count mismatch in cycle {cycle}");
            }
        }
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

[TestFixture]
public class MultiMapHelperAsyncTests
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
    public void TearDown()
    {
        _target.Dispose();
        _other.Dispose();
    }

    // ── UnionAsync ──────────────────────────────────────────

    [Test]
    public async Task UnionAsync_AddsAllPairsFromOther()
    {
        await _target.AddAsync("a", 1);
        await _other.AddAsync("a", 2);
        await _other.AddAsync("b", 3);

        await _target.UnionAsync(_other);

        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
        Assert.That(await _target.ContainsAsync("a", 2), Is.True);
        Assert.That(await _target.ContainsAsync("b", 3), Is.True);
        Assert.That(await _target.GetCountAsync(), Is.EqualTo(3));
    }

    [Test]
    public async Task UnionAsync_WithEmptyOther_DoesNotChangeTarget()
    {
        await _target.AddAsync("a", 1);

        await _target.UnionAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task UnionAsync_WithEmptyTarget_CopiesAllFromOther()
    {
        await _other.AddAsync("a", 1);
        await _other.AddAsync("b", 2);

        await _target.UnionAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(2));
        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
        Assert.That(await _target.ContainsAsync("b", 2), Is.True);
    }

    [Test]
    public async Task UnionAsync_OverlappingPairs_NoDuplicates()
    {
        await _target.AddAsync("a", 1);
        await _other.AddAsync("a", 1);

        await _target.UnionAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task UnionAsync_BothEmpty_RemainsEmpty()
    {
        await _target.UnionAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task UnionAsync_DoesNotModifyOther()
    {
        await _target.AddAsync("a", 1);
        await _other.AddAsync("b", 2);

        await _target.UnionAsync(_other);

        Assert.That(await _other.GetCountAsync(), Is.EqualTo(1));
        Assert.That(await _other.ContainsAsync("a", 1), Is.False);
    }

    [Test]
    public async Task UnionAsync_MultipleKeys_MergesCorrectly()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("b", 2);
        await _other.AddAsync("b", 3);
        await _other.AddAsync("c", 4);

        await _target.UnionAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(4));
        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
        Assert.That(await _target.ContainsAsync("b", 2), Is.True);
        Assert.That(await _target.ContainsAsync("b", 3), Is.True);
        Assert.That(await _target.ContainsAsync("c", 4), Is.True);
    }

    // ── IntersectAsync ──────────────────────────────────────

    [Test]
    public async Task IntersectAsync_KeepsOnlyCommonPairs()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("a", 2);
        await _target.AddAsync("b", 3);
        await _other.AddAsync("a", 1);
        await _other.AddAsync("b", 3);

        await _target.IntersectAsync(_other);

        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
        Assert.That(await _target.ContainsAsync("a", 2), Is.False);
        Assert.That(await _target.ContainsAsync("b", 3), Is.True);
        Assert.That(await _target.GetCountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task IntersectAsync_NoOverlap_ClearsTarget()
    {
        await _target.AddAsync("a", 1);
        await _other.AddAsync("b", 2);

        await _target.IntersectAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task IntersectAsync_WithEmptyOther_ClearsTarget()
    {
        await _target.AddAsync("a", 1);

        await _target.IntersectAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task IntersectAsync_WithEmptyTarget_RemainsEmpty()
    {
        await _other.AddAsync("a", 1);

        await _target.IntersectAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task IntersectAsync_IdenticalMaps_KeepsAll()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("b", 2);
        await _other.AddAsync("a", 1);
        await _other.AddAsync("b", 2);

        await _target.IntersectAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task IntersectAsync_DoesNotModifyOther()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("b", 2);
        await _other.AddAsync("a", 1);

        await _target.IntersectAsync(_other);

        Assert.That(await _other.GetCountAsync(), Is.EqualTo(1));
        Assert.That(await _other.ContainsAsync("b", 2), Is.False);
    }

    [Test]
    public async Task IntersectAsync_SameKeySomeValuesMatch_KeepsOnlyMatchingValues()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("a", 2);
        await _target.AddAsync("a", 3);
        await _other.AddAsync("a", 2);

        await _target.IntersectAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(1));
        Assert.That(await _target.ContainsAsync("a", 2), Is.True);
    }

    // ── ExceptWithAsync ─────────────────────────────────────

    [Test]
    public async Task ExceptWithAsync_RemovesPairsFoundInOther()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("a", 2);
        await _target.AddAsync("b", 3);
        await _other.AddAsync("a", 1);
        await _other.AddAsync("b", 3);

        await _target.ExceptWithAsync(_other);

        Assert.That(await _target.ContainsAsync("a", 2), Is.True);
        Assert.That(await _target.ContainsAsync("a", 1), Is.False);
        Assert.That(await _target.ContainsAsync("b", 3), Is.False);
        Assert.That(await _target.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task ExceptWithAsync_WithEmptyOther_DoesNotChangeTarget()
    {
        await _target.AddAsync("a", 1);

        await _target.ExceptWithAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task ExceptWithAsync_WithEmptyTarget_RemainsEmpty()
    {
        await _other.AddAsync("a", 1);

        await _target.ExceptWithAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task ExceptWithAsync_CompleteOverlap_ClearsTarget()
    {
        await _target.AddAsync("a", 1);
        await _other.AddAsync("a", 1);

        await _target.ExceptWithAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task ExceptWithAsync_NoOverlap_TargetUnchanged()
    {
        await _target.AddAsync("a", 1);
        await _other.AddAsync("b", 2);

        await _target.ExceptWithAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(1));
        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
    }

    [Test]
    public async Task ExceptWithAsync_DoesNotModifyOther()
    {
        await _target.AddAsync("a", 1);
        await _other.AddAsync("a", 1);
        await _other.AddAsync("b", 2);

        await _target.ExceptWithAsync(_other);

        Assert.That(await _other.GetCountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task ExceptWithAsync_BothEmpty_RemainsEmpty()
    {
        await _target.ExceptWithAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
    }

    // ── SymmetricExceptWithAsync ────────────────────────────

    [Test]
    public async Task SymmetricExceptWithAsync_KeepsPairsInOneButNotBoth()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("a", 2);
        await _other.AddAsync("a", 2);
        await _other.AddAsync("b", 3);

        await _target.SymmetricExceptWithAsync(_other);

        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
        Assert.That(await _target.ContainsAsync("a", 2), Is.False);
        Assert.That(await _target.ContainsAsync("b", 3), Is.True);
        Assert.That(await _target.GetCountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task SymmetricExceptWithAsync_IdenticalMaps_ClearsTarget()
    {
        await _target.AddAsync("a", 1);
        await _other.AddAsync("a", 1);

        await _target.SymmetricExceptWithAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task SymmetricExceptWithAsync_NoOverlap_CombinesAll()
    {
        await _target.AddAsync("a", 1);
        await _other.AddAsync("b", 2);

        await _target.SymmetricExceptWithAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(2));
        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
        Assert.That(await _target.ContainsAsync("b", 2), Is.True);
    }

    [Test]
    public async Task SymmetricExceptWithAsync_WithEmptyOther_TargetUnchanged()
    {
        await _target.AddAsync("a", 1);

        await _target.SymmetricExceptWithAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task SymmetricExceptWithAsync_WithEmptyTarget_CopiesOther()
    {
        await _other.AddAsync("a", 1);
        await _other.AddAsync("b", 2);

        await _target.SymmetricExceptWithAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(2));
        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
        Assert.That(await _target.ContainsAsync("b", 2), Is.True);
    }

    [Test]
    public async Task SymmetricExceptWithAsync_BothEmpty_RemainsEmpty()
    {
        await _target.SymmetricExceptWithAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task SymmetricExceptWithAsync_DoesNotModifyOther()
    {
        await _target.AddAsync("a", 1);
        await _other.AddAsync("a", 1);
        await _other.AddAsync("b", 2);

        await _target.SymmetricExceptWithAsync(_other);

        Assert.That(await _other.GetCountAsync(), Is.EqualTo(2));
        Assert.That(await _other.ContainsAsync("a", 1), Is.True);
        Assert.That(await _other.ContainsAsync("b", 2), Is.True);
    }

    // ── Edge-case gaps ──────────────────────────────────────

    [Test]
    public async Task ExceptWithAsync_OtherHasExtraPairs_OnlyRemovesMatching()
    {
        await _target.AddAsync("a", 1);
        await _other.AddAsync("a", 1);
        await _other.AddAsync("b", 2);

        await _target.ExceptWithAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
        Assert.That(await _target.ContainsAsync("b", 2), Is.False);
    }

    [Test]
    public async Task IntersectAsync_SameKeyNoValueOverlap_RemovesKey()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("a", 2);
        await _other.AddAsync("a", 99);

        await _target.IntersectAsync(_other);

        Assert.That(await _target.ContainsKeyAsync("a"), Is.False);
        Assert.That(await _target.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task IntersectAsync_OtherIsSubsetOfTarget_TargetShrinksToSubset()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("a", 2);
        await _target.AddAsync("b", 3);
        await _target.AddAsync("c", 4);
        await _other.AddAsync("a", 1);
        await _other.AddAsync("c", 4);

        await _target.IntersectAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(2));
        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
        Assert.That(await _target.ContainsAsync("c", 4), Is.True);
        Assert.That(await _target.ContainsKeyAsync("b"), Is.False);
        Assert.That(await _target.ContainsAsync("a", 2), Is.False);
    }

    [Test]
    public async Task ExceptWithAsync_RemovesAllValuesForKey_RemovesKey()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("a", 2);
        await _other.AddAsync("a", 1);
        await _other.AddAsync("a", 2);

        await _target.ExceptWithAsync(_other);

        Assert.That(await _target.ContainsKeyAsync("a"), Is.False);
        Assert.That(await _target.GetCountAsync(), Is.Zero);
        Assert.That(await _target.GetKeysAsync(), Is.Empty);
    }

    [Test]
    public async Task SymmetricExceptWithAsync_SameKeyDifferentValues_AddsAll()
    {
        await _target.AddAsync("a", 1);
        await _other.AddAsync("a", 2);

        await _target.SymmetricExceptWithAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(2));
        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
        Assert.That(await _target.ContainsAsync("a", 2), Is.True);
    }

    [Test]
    public async Task SymmetricExceptWithAsync_MultipleKeys_MixedOverlap()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("b", 2);
        await _target.AddAsync("c", 3);
        await _other.AddAsync("a", 1);
        await _other.AddAsync("b", 4);
        await _other.AddAsync("d", 5);

        await _target.SymmetricExceptWithAsync(_other);

        Assert.That(await _target.ContainsAsync("a", 1), Is.False);
        Assert.That(await _target.ContainsAsync("b", 2), Is.True);
        Assert.That(await _target.ContainsAsync("b", 4), Is.True);
        Assert.That(await _target.ContainsAsync("c", 3), Is.True);
        Assert.That(await _target.ContainsAsync("d", 5), Is.True);
        Assert.That(await _target.GetCountAsync(), Is.EqualTo(4));
    }

    [Test]
    public async Task UnionAsync_CountAndKeysConsistency_AfterLargeOverlap()
    {
        for (int i = 0; i < 20; i++)
            await _target.AddAsync($"k{i % 5}", i);

        for (int i = 10; i < 30; i++)
            await _other.AddAsync($"k{i % 5}", i);

        await _target.UnionAsync(_other);

        int enumerated = 0;
        foreach (var key in await _target.GetKeysAsync())
            enumerated += (await _target.GetAsync(key)).Count();

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(enumerated));
        Assert.That(await _target.GetCountAsync(), Is.EqualTo(30));
    }

    [Test]
    public async Task UnionAsync_PreCanceledToken_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await _target.UnionAsync(_other, cts.Token));
    }

    [Test]
    public async Task IntersectAsync_PreCanceledToken_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await _target.IntersectAsync(_other, cts.Token));
    }

    [Test]
    public async Task ExceptWithAsync_PreCanceledToken_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await _target.ExceptWithAsync(_other, cts.Token));
    }

    [Test]
    public async Task SymmetricExceptWithAsync_PreCanceledToken_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await _target.SymmetricExceptWithAsync(_other, cts.Token));
    }

    // ── Stress tests ────────────────────────────────────────

    [Test]
    public async Task Stress_UnionAsync_RepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            var source = new MultiMapAsync<string, int>();
            for (int i = 0; i < 10; i++)
                await source.AddAsync($"k{i % 3}", cycle * 10 + i);

            await _target.UnionAsync(source);
            source.Dispose();

            int enumerated = 0;
            foreach (var key in await _target.GetKeysAsync())
                enumerated += (await _target.GetAsync(key)).Count();

            Assert.That(await _target.GetCountAsync(), Is.EqualTo(enumerated),
                $"Count mismatch after union in cycle {cycle}");
        }

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(300));
    }

    [Test]
    public async Task Stress_IntersectAsync_RepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            await _target.ClearAsync();
            for (int i = 0; i < 10; i++)
                await _target.AddAsync("a", i);

            var mask = new MultiMapAsync<string, int>();
            for (int i = 0; i < 5; i++)
                await mask.AddAsync("a", i);

            await _target.IntersectAsync(mask);
            mask.Dispose();

            Assert.That(await _target.GetCountAsync(), Is.EqualTo(5),
                $"Count wrong after intersect in cycle {cycle}");
        }
    }

    [Test]
    public async Task Stress_ExceptWithAsync_RepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            await _target.ClearAsync();
            for (int i = 0; i < 10; i++)
                await _target.AddAsync("a", i);

            var removal = new MultiMapAsync<string, int>();
            for (int i = 0; i < 5; i++)
                await removal.AddAsync("a", i);

            await _target.ExceptWithAsync(removal);
            removal.Dispose();

            Assert.That(await _target.GetCountAsync(), Is.EqualTo(5),
                $"Count wrong after except in cycle {cycle}");

            var values = await _target.GetAsync("a");
            Assert.That(values, Is.EquivalentTo(new[] { 5, 6, 7, 8, 9 }),
                $"Wrong values after except in cycle {cycle}");
        }
    }

    [Test]
    public async Task Stress_SymmetricExceptWithAsync_RepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            await _target.ClearAsync();
            await _target.AddAsync("a", 1);
            await _target.AddAsync("a", 2);
            await _target.AddAsync("b", 3);

            var sym = new MultiMapAsync<string, int>();
            await sym.AddAsync("a", 2);
            await sym.AddAsync("c", 4);

            await _target.SymmetricExceptWithAsync(sym);
            sym.Dispose();

            Assert.That(await _target.ContainsAsync("a", 1), Is.True, $"cycle {cycle}");
            Assert.That(await _target.ContainsAsync("a", 2), Is.False, $"cycle {cycle}");
            Assert.That(await _target.ContainsAsync("b", 3), Is.True, $"cycle {cycle}");
            Assert.That(await _target.ContainsAsync("c", 4), Is.True, $"cycle {cycle}");
            Assert.That(await _target.GetCountAsync(), Is.EqualTo(3), $"cycle {cycle}");
        }
    }

    [Test]
    public async Task Stress_UnionThenExcept_RoundTrip_CountReturnsToOriginal()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            await _target.ClearAsync();
            for (int i = 0; i < 5; i++)
                await _target.AddAsync("a", i);

            var extra = new MultiMapAsync<string, int>();
            for (int i = 5; i < 10; i++)
                await extra.AddAsync("a", i);

            await _target.UnionAsync(extra);
            Assert.That(await _target.GetCountAsync(), Is.EqualTo(10),
                $"Count wrong after union in cycle {cycle}");

            await _target.ExceptWithAsync(extra);
            extra.Dispose();

            Assert.That(await _target.GetCountAsync(), Is.EqualTo(5),
                $"Count wrong after except round-trip in cycle {cycle}");
        }
    }

    [Test]
    public async Task Stress_ConcurrentHelperOperations_CountNeverNegative()
    {
        for (int i = 0; i < 20; i++)
            await _target.AddAsync($"k{i % 4}", i);

        const int iterations = 50;

        var tasks = Enumerable.Range(0, iterations).Select(async i =>
        {
            var temp = new MultiMapAsync<string, int>();
            await temp.AddAsync($"k{i % 4}", i);
            await temp.AddAsync($"k{(i + 1) % 4}", i + 1000);

            switch (i % 4)
            {
                case 0:
                    await _target.UnionAsync(temp);
                    break;
                case 1:
                    await _target.ExceptWithAsync(temp);
                    break;
                case 2:
                    await _target.IntersectAsync(temp);
                    break;
                case 3:
                    await _target.SymmetricExceptWithAsync(temp);
                    break;
            }

            temp.Dispose();
        }).ToArray();

        await Task.WhenAll(tasks);

        int count = await _target.GetCountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(0),
            "Count must never be negative");

        int verifyCount = 0;
        foreach (var key in await _target.GetKeysAsync())
            verifyCount += (await _target.GetAsync(key)).Count();

        Assert.That(count, Is.EqualTo(verifyCount),
            "Count must match sum of per-key values");
    }

    [Test]
    public async Task Stress_UnionAsync_UnderConcurrentMutation_NoCorruption()
    {
        using var cts = new CancellationTokenSource();

        var mutationTask = Task.Run(async () =>
        {
            int v = 0;
            while (!cts.IsCancellationRequested)
            {
                await _target.AddAsync($"m{v % 5}", v);
                v++;
                if (v % 10 == 0)
                    await _target.RemoveKeyAsync($"m{v % 5}");
            }
        });

        for (int round = 0; round < 20; round++)
        {
            var source = new MultiMapAsync<string, int>();
            for (int i = 0; i < 5; i++)
                await source.AddAsync($"u{i}", round * 100 + i);

            await _target.UnionAsync(source);
            source.Dispose();
        }

        cts.Cancel();
        try { await mutationTask; } catch (OperationCanceledException) { }

        int count = await _target.GetCountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(0));

        int verifyCount = 0;
        foreach (var key in await _target.GetKeysAsync())
            verifyCount += (await _target.GetAsync(key)).Count();

        Assert.That(count, Is.EqualTo(verifyCount),
            "Final count must match enumerated total");
    }

    [Test]
    public async Task Stress_IntersectAndSymmetric_AlternatingCycles_CountTracksCorrectly()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            await _target.ClearAsync();
            for (int i = 0; i < 10; i++)
                await _target.AddAsync("x", i);

            var operand = new MultiMapAsync<string, int>();

            if (cycle % 2 == 0)
            {
                for (int i = 0; i < 5; i++)
                    await operand.AddAsync("x", i);

                await _target.IntersectAsync(operand);

                Assert.That(await _target.GetCountAsync(), Is.EqualTo(5),
                    $"Count wrong after intersect in cycle {cycle}");
            }
            else
            {
                for (int i = 5; i < 15; i++)
                    await operand.AddAsync("x", i);

                await _target.SymmetricExceptWithAsync(operand);

                int count = await _target.GetCountAsync();
                int enumerated = (await _target.GetAsync("x")).Count();
                Assert.That(count, Is.EqualTo(enumerated),
                    $"Count mismatch in cycle {cycle}");
            }

            operand.Dispose();
        }
    }
}
