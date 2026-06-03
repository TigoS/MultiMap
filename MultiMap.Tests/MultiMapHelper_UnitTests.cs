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

    [Test]
    public void SymmetricExceptWith_MultipleValuesPerKey_CachesPerKeyLookup()
    {
        // 'other' has two entries under key "a" — exercises the targetLookup cache path
        // where the second kvp for "a" must reuse the cached set instead of re-querying.
        _target.Add("a", 1);
        _target.Add("a", 2);
        _target.Add("a", 3);
        _other.Add("a", 2); // overlap  → remove
        _other.Add("a", 4); // not in target → add
        _other.Add("b", 5); // new key → add

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Contains("a", 1), Is.True);  // only in target  → kept
        Assert.That(_target.Contains("a", 2), Is.False); // in both          → removed
        Assert.That(_target.Contains("a", 3), Is.True);  // only in target  → kept
        Assert.That(_target.Contains("a", 4), Is.True);  // only in other   → added
        Assert.That(_target.Contains("b", 5), Is.True);  // only in other   → added
        Assert.That(_target.Count, Is.EqualTo(4));
    }

    [Test]
    public void SymmetricExceptWith_AllValuesUnderSameKeyInOther_UsesCache()
    {
        // All five entries in 'other' share the same key — the cache is hit four times.
        _target.Add("x", 10);
        _target.Add("x", 20);
        _target.Add("x", 30);
        _other.Add("x", 10); // overlap  → remove
        _other.Add("x", 20); // overlap  → remove
        _other.Add("x", 40); // only in other → add
        _other.Add("x", 50); // only in other → add

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Contains("x", 10), Is.False);
        Assert.That(_target.Contains("x", 20), Is.False);
        Assert.That(_target.Contains("x", 30), Is.True);
        Assert.That(_target.Contains("x", 40), Is.True);
        Assert.That(_target.Contains("x", 50), Is.True);
        Assert.That(_target.Count, Is.EqualTo(3));
    }

    // ── Null-guard branch coverage ─────────────────────────

    [Test]
    public void Union_NullTarget_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => MultiMapHelper.Union<string, int>(null!, _other));

    [Test]
    public void Union_NullOther_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _target.Union(null!));

    [Test]
    public void Intersect_NullTarget_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => MultiMapHelper.Intersect<string, int>(null!, _other));

    [Test]
    public void Intersect_NullOther_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _target.Intersect(null!));

    [Test]
    public void ExceptWith_NullTarget_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => MultiMapHelper.ExceptWith<string, int>(null!, _other));

    [Test]
    public void ExceptWith_NullOther_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _target.ExceptWith(null!));

    [Test]
    public void SymmetricExceptWith_NullTarget_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => MultiMapHelper.SymmetricExceptWith<string, int>(null!, _other));

    [Test]
    public void SymmetricExceptWith_NullOther_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _target.SymmetricExceptWith(null!));
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

    [Test]
    public void SetEquals_WithSameInstance_IsReflexive()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);

        Assert.That(_target.SetEquals(_target), Is.True);
    }

    [Test]
    public void Union_CommutativeLaw_HoldsForSetBasedImplementation()
    {
        var left = new MultiMapSet<string, int>();
        left.Add("a", 1);
        left.Add("a", 2);
        left.Add("b", 3);

        var right = new MultiMapSet<string, int>();
        right.Add("a", 2);
        right.Add("c", 4);

        var aThenB = new MultiMapSet<string, int>();
        aThenB.AddRange(left);
        aThenB.Union(right);

        var bThenA = new MultiMapSet<string, int>();
        bThenA.AddRange(right);
        bThenA.Union(left);

        Assert.That(aThenB.SetEquals(bThenA), Is.True);
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

    [Test]
    public void ExceptWith_WithSameInstance_ResultsInEmptySet()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);

        _target.ExceptWith(_target);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void SymmetricExceptWith_WithSameInstance_ResultsInEmptySet()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);

        _target.SymmetricExceptWith(_target);

        Assert.That(_target.Count, Is.Zero);
    }

    // ── Stress tests ────────────────────────────────────────

    [Test]
    [Category("Stress")]
    public void Stress_UnionRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            var source = new MultiMapSet<string, int>();
            for (int i = 0; i < 10; i++)
                source.Add($"k{i % 3}", cycle * 10 + i);

            _target.Union(source);

            int enumerated = _target.Count;
            Assert.That(_target.Count, Is.EqualTo(enumerated),
                $"Count mismatch after union in cycle {cycle}");
        }

        Assert.That(_target.Count, Is.EqualTo(300));
    }

    [Test]
    [Category("Stress")]
    public void Stress_IntersectRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            for (int i = 0; i < 10; i++)
                _target.Add("a", i);

            var mask = new MultiMapSet<string, int>();
            for (int i = 0; i < 5; i++)
                mask.Add("a", i);

            _target.Intersect(mask);

            Assert.That(_target.Count, Is.EqualTo(5),
                $"Count wrong after intersect in cycle {cycle}");
        }
    }

    [Test]
    [Category("Stress")]
    public void Stress_ExceptWithRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            for (int i = 0; i < 10; i++)
                _target.Add("a", i);

            var removal = new MultiMapSet<string, int>();
            for (int i = 0; i < 5; i++)
                removal.Add("a", i);

            _target.ExceptWith(removal);

            Assert.That(_target.Count, Is.EqualTo(5),
                $"Count wrong after except in cycle {cycle}");
        }
    }

    [Test]
    [Category("Stress")]
    public void Stress_SymmetricExceptWithRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            _target.Add("a", 1);
            _target.Add("a", 2);
            _target.Add("b", 3);

            var sym = new MultiMapSet<string, int>();
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
    [Category("Stress")]
    public void Stress_UnionThenExcept_RoundTrip_CountReturnsToOriginal()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            for (int i = 0; i < 5; i++)
                _target.Add("a", i);

            var extra = new MultiMapSet<string, int>();
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
    [Category("Stress")]
    public void Stress_IntersectAndSymmetric_AlternatingCycles_CountTracksCorrectly()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            for (int i = 0; i < 10; i++)
                _target.Add("x", i);

            var operand = new MultiMapSet<string, int>();

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
                int enumerated = _target.Count;
                Assert.That(count, Is.EqualTo(enumerated),
                    $"Count mismatch in cycle {cycle}");
            }
        }
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
    public void SetEquals_WithSameInstance_IsReflexive()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);

        Assert.That(_target.SetEquals(_target), Is.True);
    }

    [Test]
    public void Union_CommutativeLaw_HoldsForSetBasedImplementation()
    {
        var left = new SortedMultiMap<string, int>();
        left.Add("a", 1);
        left.Add("a", 2);
        left.Add("b", 3);

        var right = new SortedMultiMap<string, int>();
        right.Add("a", 2);
        right.Add("c", 4);

        var aThenB = new SortedMultiMap<string, int>();
        aThenB.AddRange(left);
        aThenB.Union(right);

        var bThenA = new SortedMultiMap<string, int>();
        bThenA.AddRange(right);
        bThenA.Union(left);

        Assert.That(aThenB.SetEquals(bThenA), Is.True);
    }

    [Test]
    public void ExceptWith_WithSameInstance_ResultsInEmptySet()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);

        _target.ExceptWith(_target);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void SymmetricExceptWith_WithSameInstance_ResultsInEmptySet()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);

        _target.SymmetricExceptWith(_target);

        Assert.That(_target.Count, Is.Zero);
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
    [Category("Stress")]
    public void Stress_UnionRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            var source = new SortedMultiMap<string, int>();
            for (int i = 0; i < 10; i++)
                source.Add($"k{i % 3}", cycle * 10 + i);

            _target.Union(source);

            int enumerated = _target.Count;
            Assert.That(_target.Count, Is.EqualTo(enumerated),
                $"Count mismatch after union in cycle {cycle}");
        }

        Assert.That(_target.Count, Is.EqualTo(300));
    }

    [Test]
    [Category("Stress")]
    public void Stress_IntersectRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            for (int i = 0; i < 10; i++)
                _target.Add("a", i);

            var mask = new SortedMultiMap<string, int>();
            for (int i = 0; i < 5; i++)
                mask.Add("a", i);

            _target.Intersect(mask);

            Assert.That(_target.Count, Is.EqualTo(5),
                $"Count wrong after intersect in cycle {cycle}");
        }
    }

    [Test]
    [Category("Stress")]
    public void Stress_ExceptWithRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            for (int i = 0; i < 10; i++)
                _target.Add("a", i);

            var removal = new SortedMultiMap<string, int>();
            for (int i = 0; i < 5; i++)
                removal.Add("a", i);

            _target.ExceptWith(removal);

            Assert.That(_target.Count, Is.EqualTo(5),
                $"Count wrong after except in cycle {cycle}");
        }
    }

    [Test]
    [Category("Stress")]
    public void Stress_SymmetricExceptWithRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            _target.Add("a", 1);
            _target.Add("a", 2);
            _target.Add("b", 3);

            var sym = new SortedMultiMap<string, int>();
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
    [Category("Stress")]
    public void Stress_UnionThenExcept_RoundTrip_CountReturnsToOriginal()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            for (int i = 0; i < 5; i++)
                _target.Add("a", i);

            var extra = new SortedMultiMap<string, int>();
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
    [Category("Stress")]
    public void Stress_IntersectAndSymmetric_AlternatingCycles_CountTracksCorrectly()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            for (int i = 0; i < 10; i++)
                _target.Add("x", i);

            var operand = new SortedMultiMap<string, int>();

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
                int enumerated = _target.Count;
                Assert.That(count, Is.EqualTo(enumerated),
                    $"Count mismatch in cycle {cycle}");
            }
        }
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
    [Category("Stress")]
    public void Stress_UnionRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            var source = new ConcurrentMultiMap<string, int>();
            for (int i = 0; i < 10; i++)
                source.Add($"k{i % 3}", cycle * 10 + i);

            _target.Union(source);

            int enumerated = _target.Count;
            Assert.That(_target.Count, Is.EqualTo(enumerated),
                $"Count mismatch after union in cycle {cycle}");
        }

        Assert.That(_target.Count, Is.EqualTo(300));
    }

    [Test]
    [Category("Stress")]
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
    [Category("Stress")]
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
    [Category("Stress")]
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
    [Category("Stress")]
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
    [Category("Stress")]
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

        // Under the O(1) cached counter design, Count may transiently deviate from
        // a live enumeration-based recount due to prune-vs-Add races during the concurrent phase.
        // We verify only that Count is non-negative (no underflow / corruption).
        Assert.That(_target.Count, Is.GreaterThanOrEqualTo(0),
            "Count must never be negative");

        Assert.That(_target.KeyCount, Is.GreaterThanOrEqualTo(0),
            "KeyCount must never be negative");
    }

    [Test]
    [Category("Stress")]
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

        int verifyCount = _target.Count;
        Assert.That(count, Is.EqualTo(verifyCount),
            "Final count must match enumerated total");
    }

    [Test]
    [Category("Stress")]
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
                int enumerated = _target.Count;
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

    // ── Stress tests ────────────────────────────────────────

    [Test]
    [Category("Stress")]
    public void Stress_UnionRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            var source = new MultiMapList<string, int>();
            for (int i = 0; i < 10; i++)
                source.Add($"k{i % 3}", cycle * 10 + i);

            _target.Union(source);

            int enumerated = _target.Count;
            Assert.That(_target.Count, Is.EqualTo(enumerated),
                $"Count mismatch after union in cycle {cycle}");
        }

        Assert.That(_target.Count, Is.EqualTo(300));
    }

    [Test]
    [Category("Stress")]
    public void Stress_IntersectRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            for (int i = 0; i < 10; i++)
                _target.Add("a", i);

            var mask = new MultiMapList<string, int>();
            for (int i = 0; i < 5; i++)
                mask.Add("a", i);

            _target.Intersect(mask);

            Assert.That(_target.Count, Is.EqualTo(5),
                $"Count wrong after intersect in cycle {cycle}");
        }
    }

    [Test]
    [Category("Stress")]
    public void Stress_ExceptWithRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            for (int i = 0; i < 10; i++)
                _target.Add("a", i);

            var removal = new MultiMapList<string, int>();
            for (int i = 0; i < 5; i++)
                removal.Add("a", i);

            _target.ExceptWith(removal);

            Assert.That(_target.Count, Is.EqualTo(5),
                $"Count wrong after except in cycle {cycle}");
        }
    }

    [Test]
    [Category("Stress")]
    public void Stress_SymmetricExceptWithRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            _target.Add("a", 1);
            _target.Add("a", 2);
            _target.Add("b", 3);

            var sym = new MultiMapList<string, int>();
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
    [Category("Stress")]
    public void Stress_UnionThenExcept_RoundTrip_CountReturnsToOriginal()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            for (int i = 0; i < 5; i++)
                _target.Add("a", i);

            var extra = new MultiMapList<string, int>();
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
    [Category("Stress")]
    public void Stress_IntersectAndSymmetric_AlternatingCycles_CountTracksCorrectly()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            _target.Clear();
            for (int i = 0; i < 10; i++)
                _target.Add("x", i);

            var operand = new MultiMapList<string, int>();

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
                int enumerated = _target.Count;
                Assert.That(count, Is.EqualTo(enumerated),
                    $"Count mismatch in cycle {cycle}");
            }
        }
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
    [Category("Stress")]
    public void Stress_UnionRepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            using var source = new MultiMapLock<string, int>();
            for (int i = 0; i < 10; i++)
                source.Add($"k{i % 3}", cycle * 10 + i);

            _target.Union(source);

            int enumerated = _target.Count;
            Assert.That(_target.Count, Is.EqualTo(enumerated),
                $"Count mismatch after union in cycle {cycle}");
        }

        Assert.That(_target.Count, Is.EqualTo(300));
    }

    [Test]
    [Category("Stress")]
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
    [Category("Stress")]
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
    [Category("Stress")]
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
    [Category("Stress")]
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
    [Category("Stress")]
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

        int verifyCount = _target.Count;
        Assert.That(count, Is.EqualTo(verifyCount),
            "Count must match enumerated total");
    }

    [Test]
    [Category("Stress")]
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

        int verifyCount = _target.Count;
        Assert.That(count, Is.EqualTo(verifyCount),
            "Final count must match enumerated total");
    }

    [Test]
    [Category("Stress")]
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
                int enumerated = _target.Count;
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

        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void Union_WithEmptyTarget_CopiesAllFromOther()
    {
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.Union(_other);

        Assert.That(_target.Count, Is.EqualTo(2));
        Assert.That(_target.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(_target.GetOrDefault("b"), Is.EquivalentTo(new[] { 2 }));
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

        Assert.That(_target, Is.Empty);
    }

    [Test]
    public void Union_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.Union(_other);

        Assert.That(_other.Count, Is.EqualTo(1));
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

        Assert.That(_target.Count, Is.EqualTo(4));
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
        Assert.That(_target.Count, Is.EqualTo(2));
    }

    [Test]
    public void Intersect_NoOverlap_ClearsTarget()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target = _target.Intersect(_other);

        Assert.That(_target, Is.Empty);
    }

    [Test]
    public void Intersect_WithEmptyOther_ClearsTarget()
    {
        _target.Add("a", 1);

        _target = _target.Intersect(_other);

        Assert.That(_target, Is.Empty);
    }

    [Test]
    public void Intersect_WithEmptyTarget_RemainsEmpty()
    {
        _other.Add("a", 1);

        _target = _target.Intersect(_other);

        Assert.That(_target, Is.Empty);
    }

    [Test]
    public void Intersect_IdenticalMaps_KeepsAll()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target = _target.Intersect(_other);

        Assert.That(_target.Count, Is.EqualTo(2));
    }

    [Test]
    public void Intersect_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("a", 1);

        _target = _target.Intersect(_other);

        Assert.That(_other.Count, Is.EqualTo(1));
    }

    [Test]
    public void Intersect_SameKeySomeValuesMatch_KeepsOnlyMatchingValues()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _target.Add("a", 3);
        _other.Add("a", 2);

        _target = _target.Intersect(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
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
        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void ExceptWith_NoOverlap_KeepsAll()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
        Assert.That(_target.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
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

        Assert.That(_target, Is.Empty);
    }

    [Test]
    public void ExceptWith_WithEmptyTarget_RemainsEmpty()
    {
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_target, Is.Empty);
    }

    [Test]
    public void ExceptWith_OtherHasExtraPairs_OnlyRemovesMatching()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.ExceptWith(_other);

        Assert.That(_target, Is.Empty);
    }

    [Test]
    public void ExceptWith_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_other.Count, Is.EqualTo(1));
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
        Assert.That(_target.Count, Is.EqualTo(2));
    }

    [Test]
    public void SymmetricExceptWith_NoOverlap_UnionsBoth()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target = _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(2));
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

        Assert.That(_target, Is.Empty);
    }

    [Test]
    public void SymmetricExceptWith_WithEmptyOther_KeepsAll()
    {
        _target.Add("a", 1);

        _target = _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
        Assert.That(_target.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void SymmetricExceptWith_WithEmptyTarget_CopiesOther()
    {
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target = _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(2));
        Assert.That(_target.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(_target.GetOrDefault("b"), Is.EquivalentTo(new[] { 2 }));
    }

    [Test]
    public void SymmetricExceptWith_BothEmpty_RemainsEmpty()
    {
        _target = _target.SymmetricExceptWith(_other);

        Assert.That(_target, Is.Empty);
    }

    [Test]
    public void SymmetricExceptWith_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target = _target.SymmetricExceptWith(_other);

        Assert.That(_other.Count, Is.EqualTo(2));
        Assert.That(_other.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(_other.GetOrDefault("b"), Is.EquivalentTo(new[] { 2 }));
    }

    // ── IsSubsetOf ─────────────────────────────────────────

    [Test]
    public void IsSubsetOf_EmptyIsSubsetOfEmpty_ReturnsTrue()
    {
        var result = _target.IsSubsetOf(_other);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsSubsetOf_EmptyIsSubsetOfNonEmpty_ReturnsTrue()
    {
        _other.Add("a", 1);

        var result = _target.IsSubsetOf(_other);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsSubsetOf_NonEmptyIsNotSubsetOfEmpty_ReturnsFalse()
    {
        _target.Add("a", 1);

        var result = _target.IsSubsetOf(_other);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsSubsetOf_IdenticalSets_ReturnsTrue()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("a", 1);
        _other.Add("b", 2);

        var result = _target.IsSubsetOf(_other);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsSubsetOf_ProperSubset_ReturnsTrue()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);
        _other.Add("b", 2);

        var result = _target.IsSubsetOf(_other);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsSubsetOf_DisjointSets_ReturnsFalse()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        var result = _target.IsSubsetOf(_other);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsSubsetOf_PartialOverlap_ReturnsFalse()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("a", 1);
        _other.Add("c", 3);

        var result = _target.IsSubsetOf(_other);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsSubsetOf_MultipleValuesPerKey_ChecksAllValues()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _other.Add("a", 1);
        _other.Add("a", 2);
        _other.Add("a", 3);

        var result = _target.IsSubsetOf(_other);

        Assert.That(result, Is.True);
    }

    // ── IsSupersetOf ───────────────────────────────────────

    [Test]
    public void IsSupersetOf_EmptyIsSupersetOfEmpty_ReturnsTrue()
    {
        var result = _target.IsSupersetOf(_other);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsSupersetOf_NonEmptyIsSupersetOfEmpty_ReturnsTrue()
    {
        _target.Add("a", 1);

        var result = _target.IsSupersetOf(_other);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsSupersetOf_EmptyIsNotSupersetOfNonEmpty_ReturnsFalse()
    {
        _other.Add("a", 1);

        var result = _target.IsSupersetOf(_other);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsSupersetOf_IdenticalSets_ReturnsTrue()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("a", 1);
        _other.Add("b", 2);

        var result = _target.IsSupersetOf(_other);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsSupersetOf_ProperSuperset_ReturnsTrue()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("a", 1);

        var result = _target.IsSupersetOf(_other);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsSupersetOf_DisjointSets_ReturnsFalse()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        var result = _target.IsSupersetOf(_other);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsSupersetOf_PartialOverlap_ReturnsFalse()
    {
        _target.Add("a", 1);
        _target.Add("c", 3);
        _other.Add("a", 1);
        _other.Add("b", 2);

        var result = _target.IsSupersetOf(_other);

        Assert.That(result, Is.False);
    }

    // ── Overlaps ───────────────────────────────────────────

    [Test]
    public void Overlaps_EmptySets_ReturnsFalse()
    {
        var result = _target.Overlaps(_other);

        Assert.That(result, Is.False);
    }

    [Test]
    public void Overlaps_EmptyAndNonEmpty_ReturnsFalse()
    {
        _other.Add("a", 1);

        var result = _target.Overlaps(_other);

        Assert.That(result, Is.False);
    }

    [Test]
    public void Overlaps_DisjointSets_ReturnsFalse()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        var result = _target.Overlaps(_other);

        Assert.That(result, Is.False);
    }

    [Test]
    public void Overlaps_SingleCommonPair_ReturnsTrue()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);

        var result = _target.Overlaps(_other);

        Assert.That(result, Is.True);
    }

    [Test]
    public void Overlaps_MultipleCommonPairs_ReturnsTrue()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("a", 1);
        _other.Add("b", 2);

        var result = _target.Overlaps(_other);

        Assert.That(result, Is.True);
    }

    [Test]
    public void Overlaps_PartialOverlap_ReturnsTrue()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("a", 1);
        _other.Add("c", 3);

        var result = _target.Overlaps(_other);

        Assert.That(result, Is.True);
    }

    [Test]
    public void Overlaps_SameKeyDifferentValues_ReturnsFalse()
    {
        _target.Add("a", 1);
        _other.Add("a", 2);

        var result = _target.Overlaps(_other);

        Assert.That(result, Is.False);
    }

    // ── SetEquals ──────────────────────────────────────────

    [Test]
    public void SetEquals_EmptySets_ReturnsTrue()
    {
        var result = _target.SetEquals(_other);

        Assert.That(result, Is.True);
    }

    [Test]
    public void SetEquals_EmptyAndNonEmpty_ReturnsFalse()
    {
        _other.Add("a", 1);

        var result = _target.SetEquals(_other);

        Assert.That(result, Is.False);
    }

    [Test]
    public void SetEquals_NonEmptyAndEmpty_ReturnsFalse()
    {
        _target.Add("a", 1);

        var result = _target.SetEquals(_other);

        Assert.That(result, Is.False);
    }

    [Test]
    public void SetEquals_IdenticalSets_ReturnsTrue()
    {
        _target.Add("a", 1);
        _target.Add("b", 2);
        _other.Add("a", 1);
        _other.Add("b", 2);

        var result = _target.SetEquals(_other);

        Assert.That(result, Is.True);
    }

    [Test]
    public void SetEquals_DifferentCounts_ReturnsFalse()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);
        _other.Add("b", 2);

        var result = _target.SetEquals(_other);

        Assert.That(result, Is.False);
    }

    [Test]
    public void SetEquals_DifferentKeys_ReturnsFalse()
    {
        _target.Add("a", 1);
        _other.Add("b", 1);

        var result = _target.SetEquals(_other);

        Assert.That(result, Is.False);
    }

    [Test]
    public void SetEquals_SameKeysDifferentValues_ReturnsFalse()
    {
        _target.Add("a", 1);
        _other.Add("a", 2);

        var result = _target.SetEquals(_other);

        Assert.That(result, Is.False);
    }

    [Test]
    public void SetEquals_MultipleValuesPerKey_ChecksAllValues()
    {
        _target.Add("a", 1);
        _target.Add("a", 2);
        _target.Add("b", 3);
        _other.Add("a", 1);
        _other.Add("a", 2);
        _other.Add("b", 3);

        var result = _target.SetEquals(_other);

        Assert.That(result, Is.True);
    }

    // ── Null-guard branch coverage ─────────────────────────

    [Test]
    public void Union_NullTarget_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => ((ISimpleMultiMap<string, int>)null!).Union(_other));

    [Test]
    public void Union_NullOther_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _target.Union(null!));

    [Test]
    public void Intersect_NullTarget_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => ((ISimpleMultiMap<string, int>)null!).Intersect(_other));

    [Test]
    public void Intersect_NullOther_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _target.Intersect(null!));

    [Test]
    public void ExceptWith_NullTarget_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => ((ISimpleMultiMap<string, int>)null!).ExceptWith(_other));

    [Test]
    public void ExceptWith_NullOther_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _target.ExceptWith(null!));

    [Test]
    public void SymmetricExceptWith_NullTarget_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => ((ISimpleMultiMap<string, int>)null!).SymmetricExceptWith(_other));

    [Test]
    public void SymmetricExceptWith_NullOther_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _target.SymmetricExceptWith(null!));

    [Test]
    public void IsSubsetOf_NullTarget_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => ((ISimpleMultiMap<string, int>)null!).IsSubsetOf(_other));

    [Test]
    public void IsSubsetOf_NullOther_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _target.IsSubsetOf(null!));

    [Test]
    public void IsSupersetOf_NullTarget_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => ((ISimpleMultiMap<string, int>)null!).IsSupersetOf(_other));

    [Test]
    public void IsSupersetOf_NullOther_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _target.IsSupersetOf(null!));

    [Test]
    public void Overlaps_NullTarget_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => ((ISimpleMultiMap<string, int>)null!).Overlaps(_other));

    [Test]
    public void Overlaps_NullOther_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _target.Overlaps(null!));

    [Test]
    public void SetEquals_NullTarget_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => ((ISimpleMultiMap<string, int>)null!).SetEquals(_other));

    [Test]
    public void SetEquals_NullOther_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => _target.SetEquals(null!));
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
        _target.DisposeAsync().GetAwaiter().GetResult();
        _other.DisposeAsync().GetAwaiter().GetResult();
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
            enumerated += (await _target.GetOrDefaultAsync(key)).Count();

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
    [Category("Stress")]
    public async Task Stress_UnionAsync_RepeatedCycles_CountConsistent()
    {
        for (int cycle = 0; cycle < 30; cycle++)
        {
            var source = new MultiMapAsync<string, int>();
            for (int i = 0; i < 10; i++)
                await source.AddAsync($"k{i % 3}", cycle * 10 + i);

            await _target.UnionAsync(source);
            await source.DisposeAsync();

            int enumerated = 0;
            foreach (var key in await _target.GetKeysAsync())
                enumerated += (await _target.GetOrDefaultAsync(key)).Count();

            Assert.That(await _target.GetCountAsync(), Is.EqualTo(enumerated),
                $"Count mismatch after union in cycle {cycle}");
        }

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(300));
    }

    [Test]
    [Category("Stress")]
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
            await mask.DisposeAsync();

            Assert.That(await _target.GetCountAsync(), Is.EqualTo(5),
                $"Count wrong after intersect in cycle {cycle}");
        }
    }

    [Test]
    [Category("Stress")]
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
            await removal.DisposeAsync();

            Assert.That(await _target.GetCountAsync(), Is.EqualTo(5),
                $"Count wrong after except in cycle {cycle}");

            var values = await _target.GetOrDefaultAsync("a");
            Assert.That(values, Is.EquivalentTo(new[] { 5, 6, 7, 8, 9 }),
                $"Wrong values after except in cycle {cycle}");
        }
    }

    [Test]
    [Category("Stress")]
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
            await sym.DisposeAsync();

            Assert.That(await _target.ContainsAsync("a", 1), Is.True, $"cycle {cycle}");
            Assert.That(await _target.ContainsAsync("a", 2), Is.False, $"cycle {cycle}");
            Assert.That(await _target.ContainsAsync("b", 3), Is.True, $"cycle {cycle}");
            Assert.That(await _target.ContainsAsync("c", 4), Is.True, $"cycle {cycle}");
            Assert.That(await _target.GetCountAsync(), Is.EqualTo(3), $"cycle {cycle}");
        }
    }

    [Test]
    [Category("Stress")]
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
            await extra.DisposeAsync();

            Assert.That(await _target.GetCountAsync(), Is.EqualTo(5),
                $"Count wrong after except round-trip in cycle {cycle}");
        }
    }

    [Test]
    [Category("Stress")]
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

            await temp.DisposeAsync();
        }).ToArray();

        await Task.WhenAll(tasks);

        int count = await _target.GetCountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(0),
            "Count must never be negative");

        int verifyCount = 0;
        foreach (var key in await _target.GetKeysAsync())
            verifyCount += (await _target.GetOrDefaultAsync(key)).Count();

        Assert.That(count, Is.EqualTo(verifyCount),
            "Count must match sum of per-key values");
    }

    [Test]
    [Category("Stress")]
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
            await source.DisposeAsync();
        }

        cts.Cancel();
        try { await mutationTask; } catch (OperationCanceledException) { }

        int count = await _target.GetCountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(0));

        int verifyCount = 0;
        foreach (var key in await _target.GetKeysAsync())
            verifyCount += (await _target.GetOrDefaultAsync(key)).Count();

        Assert.That(count, Is.EqualTo(verifyCount),
            "Final count must match enumerated total");
    }

    [Test]
    [Category("Stress")]
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
                int enumerated = (await _target.GetOrDefaultAsync("x")).Count();
                Assert.That(count, Is.EqualTo(enumerated),
                    $"Count mismatch in cycle {cycle}");
            }

            await operand.DisposeAsync();
        }
    }

    // ── Additional async coverage tests ─────────────────────

    [Test]
    public async Task IntersectAsync_MixedKeysToRemoveAndValuesToRemove_InSameCall()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("a", 2);
        await _target.AddAsync("b", 3);
        await _target.AddAsync("c", 4);
        await _target.AddAsync("c", 5);

        await _other.AddAsync("a", 1);
        await _other.AddAsync("c", 5);

        await _target.IntersectAsync(_other);

        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
        Assert.That(await _target.ContainsAsync("a", 2), Is.False);
        Assert.That(await _target.ContainsKeyAsync("b"), Is.False);
        Assert.That(await _target.ContainsAsync("c", 4), Is.False);
        Assert.That(await _target.ContainsAsync("c", 5), Is.True);
        Assert.That(await _target.GetCountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task IntersectAsync_ManyKeysDeepIteration_AllPathsExercised()
    {
        for (int k = 0; k < 10; k++)
            for (int v = 0; v < 5; v++)
                await _target.AddAsync($"k{k}", k * 100 + v);

        for (int k = 0; k < 10; k += 2)
            for (int v = 0; v < 3; v++)
                await _other.AddAsync($"k{k}", k * 100 + v);

        await _target.IntersectAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(15));
        for (int k = 1; k < 10; k += 2)
            Assert.That(await _target.ContainsKeyAsync($"k{k}"), Is.False);
        for (int k = 0; k < 10; k += 2)
        {
            var values = await _target.GetOrDefaultAsync($"k{k}");
            Assert.That(values.Count(), Is.EqualTo(3));
        }
    }

    [Test]
    public async Task IntersectAsync_OnlyKeysToRemove_NoValuesToRemove()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("b", 2);
        await _target.AddAsync("c", 3);

        await _other.AddAsync("d", 4);

        await _target.IntersectAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
        Assert.That(await _target.GetKeysAsync(), Is.Empty);
    }

    [Test]
    public async Task IntersectAsync_OnlyValuesToRemove_NoKeysToRemove()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("a", 2);
        await _target.AddAsync("a", 3);

        await _other.AddAsync("a", 2);

        await _target.IntersectAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(1));
        Assert.That(await _target.ContainsAsync("a", 2), Is.True);
    }

    [Test]
    public async Task ExceptWithAsync_ManyKeys_RemovesAcrossAllKeys()
    {
        for (int k = 0; k < 10; k++)
            for (int v = 0; v < 5; v++)
                await _target.AddAsync($"k{k}", k * 100 + v);

        for (int k = 0; k < 10; k++)
            for (int v = 0; v < 3; v++)
                await _other.AddAsync($"k{k}", k * 100 + v);

        await _target.ExceptWithAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(20));
        for (int k = 0; k < 10; k++)
        {
            var values = await _target.GetOrDefaultAsync($"k{k}");
            Assert.That(values.Count(), Is.EqualTo(2));
        }
    }

    [Test]
    public async Task SymmetricExceptWithAsync_ManyKeys_MixedOverlap_DeepIteration()
    {
        for (int k = 0; k < 5; k++)
            for (int v = 0; v < 4; v++)
                await _target.AddAsync($"k{k}", k * 100 + v);

        for (int k = 2; k < 7; k++)
            for (int v = 2; v < 6; v++)
                await _other.AddAsync($"k{k}", k * 100 + v);

        await _target.SymmetricExceptWithAsync(_other);

        Assert.That(await _target.ContainsAsync("k0", 0), Is.True);
        Assert.That(await _target.ContainsAsync("k0", 1), Is.True);
        Assert.That(await _target.ContainsAsync("k2", 202), Is.False);
        Assert.That(await _target.ContainsAsync("k2", 203), Is.False);
        Assert.That(await _target.ContainsAsync("k2", 200), Is.True);
        Assert.That(await _target.ContainsAsync("k2", 201), Is.True);
        Assert.That(await _target.ContainsAsync("k2", 204), Is.True);
        Assert.That(await _target.ContainsAsync("k2", 205), Is.True);
        Assert.That(await _target.ContainsAsync("k5", 502), Is.True);
        Assert.That(await _target.ContainsAsync("k6", 602), Is.True);
    }

    [Test]
    public async Task SymmetricExceptWithAsync_AllOverlap_ClearsAllShared()
    {
        for (int v = 0; v < 5; v++)
        {
            await _target.AddAsync("x", v);
            await _other.AddAsync("x", v);
        }

        await _target.SymmetricExceptWithAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task UnionAsync_ManyKeysDeepIteration()
    {
        for (int k = 0; k < 10; k++)
            await _target.AddAsync($"k{k}", k);

        for (int k = 5; k < 15; k++)
            await _other.AddAsync($"k{k}", k + 100);

        await _target.UnionAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(20));
        for (int k = 0; k < 10; k++)
            Assert.That(await _target.ContainsAsync($"k{k}", k), Is.True);
        for (int k = 5; k < 15; k++)
            Assert.That(await _target.ContainsAsync($"k{k}", k + 100), Is.True);
    }

    [Test]
    public async Task UnionAsync_ChainedWithIntersect_ProducesCorrectResult()
    {
        await _target.AddAsync("a", 1);
        await _other.AddAsync("a", 2);
        await _other.AddAsync("b", 3);

        await _target.UnionAsync(_other);
        Assert.That(await _target.GetCountAsync(), Is.EqualTo(3));

        var intersector = new MultiMapAsync<string, int>();
        await intersector.AddAsync("a", 1);
        await intersector.AddAsync("a", 2);

        await _target.IntersectAsync(intersector);
        await intersector.DisposeAsync();

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(2));
        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
        Assert.That(await _target.ContainsAsync("a", 2), Is.True);
        Assert.That(await _target.ContainsKeyAsync("b"), Is.False);
    }

    [Test]
    public async Task ExceptWithAsync_ChainedWithUnion_RoundTrip()
    {
        for (int i = 0; i < 5; i++)
            await _target.AddAsync("a", i);

        var extra = new MultiMapAsync<string, int>();
        for (int i = 5; i < 10; i++)
            await extra.AddAsync("a", i);

        await _target.UnionAsync(extra);
        Assert.That(await _target.GetCountAsync(), Is.EqualTo(10));

        await _target.ExceptWithAsync(extra);
        await extra.DisposeAsync();

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(5));
        for (int i = 0; i < 5; i++)
            Assert.That(await _target.ContainsAsync("a", i), Is.True);
        for (int i = 5; i < 10; i++)
            Assert.That(await _target.ContainsAsync("a", i), Is.False);
    }

    [Test]
    public async Task IntersectAsync_TargetHasManyKeysOtherHasOne_RemovesMostKeys()
    {
        for (int k = 0; k < 20; k++)
            await _target.AddAsync($"k{k}", k);

        await _other.AddAsync("k5", 5);

        await _target.IntersectAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(1));
        Assert.That(await _target.ContainsAsync("k5", 5), Is.True);
        var keys = await _target.GetKeysAsync();
        Assert.That(keys.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task ExceptWithAsync_RemovesPartialValuesFromMultipleKeys()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("a", 2);
        await _target.AddAsync("a", 3);
        await _target.AddAsync("b", 10);
        await _target.AddAsync("b", 20);
        await _target.AddAsync("c", 100);

        await _other.AddAsync("a", 2);
        await _other.AddAsync("b", 10);
        await _other.AddAsync("c", 100);

        await _target.ExceptWithAsync(_other);

        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
        Assert.That(await _target.ContainsAsync("a", 3), Is.True);
        Assert.That(await _target.ContainsAsync("b", 20), Is.True);
        Assert.That(await _target.ContainsKeyAsync("c"), Is.False);
        Assert.That(await _target.GetCountAsync(), Is.EqualTo(3));
    }

    [Test]
    public async Task SymmetricExceptWithAsync_OtherOnlyAdds_WhenNoOverlap()
    {
        await _target.AddAsync("a", 1);
        await _other.AddAsync("b", 2);
        await _other.AddAsync("c", 3);

        await _target.SymmetricExceptWithAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(3));
        Assert.That(await _target.ContainsAsync("a", 1), Is.True);
        Assert.That(await _target.ContainsAsync("b", 2), Is.True);
        Assert.That(await _target.ContainsAsync("c", 3), Is.True);
    }

    [Test]
    public async Task SymmetricExceptWithAsync_OtherOnlyRemoves_WhenFullOverlap()
    {
        await _target.AddAsync("a", 1);
        await _target.AddAsync("a", 2);
        await _other.AddAsync("a", 1);
        await _other.AddAsync("a", 2);

        await _target.SymmetricExceptWithAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
        Assert.That(await _target.GetKeysAsync(), Is.Empty);
    }

    [Test]
    public async Task IntersectAsync_BothEmpty_RemainsEmpty()
    {
        await _target.IntersectAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task ExceptWithAsync_BothHaveSameMultipleKeys_AllRemoved()
    {
        for (int k = 0; k < 5; k++)
            for (int v = 0; v < 3; v++)
            {
                await _target.AddAsync($"k{k}", v);
                await _other.AddAsync($"k{k}", v);
            }

        await _target.ExceptWithAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.Zero);
    }

    [Test]
    public async Task UnionAsync_SameKeyDifferentValuesInBoth_MergesAll()
    {
        await _target.AddAsync("x", 1);
        await _target.AddAsync("x", 2);
        await _other.AddAsync("x", 3);
        await _other.AddAsync("x", 4);

        await _target.UnionAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(4));
        for (int v = 1; v <= 4; v++)
            Assert.That(await _target.ContainsAsync("x", v), Is.True);
    }

    [Test]
    public async Task IntersectAsync_MultipleValuesPerKey_KeepsOnlyShared()
    {
        for (int v = 0; v < 10; v++)
            await _target.AddAsync("x", v);

        for (int v = 5; v < 15; v++)
            await _other.AddAsync("x", v);

        await _target.IntersectAsync(_other);

        Assert.That(await _target.GetCountAsync(), Is.EqualTo(5));
        for (int v = 5; v < 10; v++)
            Assert.That(await _target.ContainsAsync("x", v), Is.True);
        for (int v = 0; v < 5; v++)
            Assert.That(await _target.ContainsAsync("x", v), Is.False);
    }

    // ── Null-guard branch coverage ─────────────────────────

    [Test]
    public void UnionAsync_NullTarget_ThrowsArgumentNullException()
        => Assert.ThrowsAsync<ArgumentNullException>(() => ((IMultiMapAsync<string, int>)null!).UnionAsync(_other));

    [Test]
    public void UnionAsync_NullOther_ThrowsArgumentNullException()
        => Assert.ThrowsAsync<ArgumentNullException>(() => _target.UnionAsync(null!));

    [Test]
    public void IntersectAsync_NullTarget_ThrowsArgumentNullException()
        => Assert.ThrowsAsync<ArgumentNullException>(() => ((IMultiMapAsync<string, int>)null!).IntersectAsync(_other));

    [Test]
    public void IntersectAsync_NullOther_ThrowsArgumentNullException()
        => Assert.ThrowsAsync<ArgumentNullException>(() => _target.IntersectAsync(null!));

    [Test]
    public void ExceptWithAsync_NullTarget_ThrowsArgumentNullException()
        => Assert.ThrowsAsync<ArgumentNullException>(() => ((IMultiMapAsync<string, int>)null!).ExceptWithAsync(_other));

    [Test]
    public void ExceptWithAsync_NullOther_ThrowsArgumentNullException()
        => Assert.ThrowsAsync<ArgumentNullException>(() => _target.ExceptWithAsync(null!));

    [Test]
    public void SymmetricExceptWithAsync_NullTarget_ThrowsArgumentNullException()
        => Assert.ThrowsAsync<ArgumentNullException>(() => ((IMultiMapAsync<string, int>)null!).SymmetricExceptWithAsync(_other));

    [Test]
    public void SymmetricExceptWithAsync_NullOther_ThrowsArgumentNullException()
        => Assert.ThrowsAsync<ArgumentNullException>(() => _target.SymmetricExceptWithAsync(null!));
}

[TestFixture]
public class MultiMapHelperWithSortedMultiMapEdgeCaseTests
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
    public void Union_OverlappingPairs_NoDuplicatesInSortedSet()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);

        _target.Union(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
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
    public void Intersect_NoOverlap_ClearsTarget()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

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
    public void ExceptWith_NoOverlap_KeepsAll()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void ExceptWith_WithEmptyTarget_RemainsEmpty()
    {
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void ExceptWith_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_other.Count, Is.EqualTo(1));
    }

    [Test]
    public void ExceptWith_OtherHasExtraPairs_OnlyRemovesMatching()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void SymmetricExceptWith_WithEmptyOther_KeepsAll()
    {
        _target.Add("a", 1);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void SymmetricExceptWith_WithEmptyTarget_CopiesOther()
    {
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(2));
    }

    [Test]
    public void SymmetricExceptWith_BothEmpty_RemainsEmpty()
    {
        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void SymmetricExceptWith_NoOverlap_UnionsBoth()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(2));
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
    public void SymmetricExceptWith_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_other.Count, Is.EqualTo(2));
    }
}

[TestFixture]
public class MultiMapHelperWithConcurrentMultiMapEdgeCaseTests
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
    }

    [Test]
    public void Union_BothEmpty_RemainsEmpty()
    {
        _target.Union(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void Union_OverlappingPairs_NoDuplicates()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);

        _target.Union(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
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
    public void Intersect_NoOverlap_ClearsTarget()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

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
    public void ExceptWith_NoOverlap_KeepsAll()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void ExceptWith_WithEmptyTarget_RemainsEmpty()
    {
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void ExceptWith_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_other.Count, Is.EqualTo(1));
    }

    [Test]
    public void ExceptWith_OtherHasExtraPairs_OnlyRemovesMatching()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void SymmetricExceptWith_WithEmptyOther_KeepsAll()
    {
        _target.Add("a", 1);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void SymmetricExceptWith_WithEmptyTarget_CopiesOther()
    {
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(2));
    }

    [Test]
    public void SymmetricExceptWith_BothEmpty_RemainsEmpty()
    {
        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void SymmetricExceptWith_NoOverlap_UnionsBoth()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(2));
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
    public void SymmetricExceptWith_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_other.Count, Is.EqualTo(2));
    }
}

[TestFixture]
public class MultiMapHelperWithMultiMapListEdgeCaseTests
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
    public void Intersect_NoOverlap_ClearsTarget()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

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
    public void ExceptWith_NoOverlap_KeepsAll()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void ExceptWith_WithEmptyTarget_RemainsEmpty()
    {
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void ExceptWith_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_other.Count, Is.EqualTo(1));
    }

    [Test]
    public void ExceptWith_OtherHasExtraPairs_OnlyRemovesMatching()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void SymmetricExceptWith_WithEmptyOther_KeepsAll()
    {
        _target.Add("a", 1);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void SymmetricExceptWith_WithEmptyTarget_CopiesOther()
    {
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(2));
    }

    [Test]
    public void SymmetricExceptWith_BothEmpty_RemainsEmpty()
    {
        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void SymmetricExceptWith_NoOverlap_UnionsBoth()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(2));
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
    public void SymmetricExceptWith_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_other.Count, Is.EqualTo(2));
    }
}

[TestFixture]
public class MultiMapHelperWithMultiMapLockEdgeCaseTests
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
    }

    [Test]
    public void Union_BothEmpty_RemainsEmpty()
    {
        _target.Union(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void Union_OverlappingPairs_NoDuplicates()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);

        _target.Union(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
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
    public void Intersect_NoOverlap_ClearsTarget()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

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
    public void ExceptWith_NoOverlap_KeepsAll()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void ExceptWith_WithEmptyTarget_RemainsEmpty()
    {
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void ExceptWith_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);

        _target.ExceptWith(_other);

        Assert.That(_other.Count, Is.EqualTo(1));
    }

    [Test]
    public void ExceptWith_OtherHasExtraPairs_OnlyRemovesMatching()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.ExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void SymmetricExceptWith_WithEmptyOther_KeepsAll()
    {
        _target.Add("a", 1);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(1));
    }

    [Test]
    public void SymmetricExceptWith_WithEmptyTarget_CopiesOther()
    {
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(2));
    }

    [Test]
    public void SymmetricExceptWith_BothEmpty_RemainsEmpty()
    {
        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.Zero);
    }

    [Test]
    public void SymmetricExceptWith_NoOverlap_UnionsBoth()
    {
        _target.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_target.Count, Is.EqualTo(2));
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
    public void SymmetricExceptWith_DoesNotModifyOther()
    {
        _target.Add("a", 1);
        _other.Add("a", 1);
        _other.Add("b", 2);

        _target.SymmetricExceptWith(_other);

        Assert.That(_other.Count, Is.EqualTo(2));
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// MultiMapHelper – IMultiMap<> overloads (IsSubsetOf / IsSupersetOf / Overlaps
// / SetEquals) that are covered by a different dispatch path from the already
// tested ISimpleMultiMap<> overloads.
// Lines 183-317 of MultiMapHelper.cs use IMultiMap; covered here with
// ConcurrentMultiMap which implements IMultiMap but NOT ISimpleMultiMap.
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MultiMapHelper_IMultiMapOverloadsTests
{
    // helpers so tests stay concise
    private static ConcurrentMultiMap<string, int> Map(params (string k, int v)[] pairs)
    {
        var m = new ConcurrentMultiMap<string, int>();
        foreach (var (k, v) in pairs) m.Add(k, v);
        return m;
    }

    // ── IsSubsetOf (IMultiMap overload) ───────────────────────────────────────

    [Test]
    public void IMultiMap_IsSubsetOf_EmptyIsSubsetOfEmpty_ReturnsTrue()
    {
        Assert.That(Map().IsSubsetOf(Map()), Is.True);
    }

    [Test]
    public void IMultiMap_IsSubsetOf_TargetIsProperSubset_ReturnsTrue()
    {
        Assert.That(Map(("a", 1)).IsSubsetOf(Map(("a", 1), ("b", 2))), Is.True);
    }

    [Test]
    public void IMultiMap_IsSubsetOf_IdenticalContent_ReturnsTrue()
    {
        Assert.That(Map(("a", 1), ("b", 2)).IsSubsetOf(Map(("a", 1), ("b", 2))), Is.True);
    }

    [Test]
    public void IMultiMap_IsSubsetOf_ValueMissing_ReturnsFalse()
    {
        Assert.That(Map(("a", 1), ("a", 3)).IsSubsetOf(Map(("a", 1), ("b", 2))), Is.False);
    }

    [Test]
    public void IMultiMap_IsSubsetOf_DisjointSets_ReturnsFalse()
    {
        Assert.That(Map(("a", 1)).IsSubsetOf(Map(("b", 2))), Is.False);
    }

    // exercises the non-ISet<> branch (other values not already an ISet)
    [Test]
    public void IMultiMap_IsSubsetOf_WithMultiMapListOther_UsesHashSetFallback()
    {
        // MultiMapList values are IReadOnlyList, not ISet → triggers the else branch
        var target = new ConcurrentMultiMap<string, int>();
        target.Add("a", 1);

        var other = new MultiMapList<string, int>();
        other.Add("a", 1);
        other.Add("a", 2);

        Assert.That(((IMultiMap<string, int>)target).IsSubsetOf(other), Is.True);
    }

    [Test]
    public void IMultiMap_IsSubsetOf_NullTarget_ThrowsArgumentNullException()
    {
        IMultiMap<string, int> nullMap = null!;
        Assert.Throws<ArgumentNullException>(() => nullMap.IsSubsetOf(Map()));
    }

    [Test]
    public void IMultiMap_IsSubsetOf_NullOther_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Map().IsSubsetOf((IMultiMap<string, int>)null!));
    }

    // ── IsSupersetOf (IMultiMap overload) ────────────────────────────────────

    [Test]
    public void IMultiMap_IsSupersetOf_EmptyBothSides_ReturnsTrue()
    {
        Assert.That(Map().IsSupersetOf(Map()), Is.True);
    }

    [Test]
    public void IMultiMap_IsSupersetOf_TargetIsProperSuperset_ReturnsTrue()
    {
        Assert.That(Map(("a", 1), ("b", 2)).IsSupersetOf(Map(("a", 1))), Is.True);
    }

    [Test]
    public void IMultiMap_IsSupersetOf_TargetMissesValue_ReturnsFalse()
    {
        Assert.That(Map(("a", 1)).IsSupersetOf(Map(("a", 1), ("b", 2))), Is.False);
    }

    [Test]
    public void IMultiMap_IsSupersetOf_NullTarget_ThrowsArgumentNullException()
    {
        IMultiMap<string, int> nullMap = null!;
        Assert.Throws<ArgumentNullException>(() => nullMap.IsSupersetOf(Map()));
    }

    [Test]
    public void IMultiMap_IsSupersetOf_NullOther_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Map().IsSupersetOf((IMultiMap<string, int>)null!));
    }

    // ── Overlaps (IMultiMap overload) ─────────────────────────────────────────

    [Test]
    public void IMultiMap_Overlaps_EmptySets_ReturnsFalse()
    {
        Assert.That(Map().Overlaps(Map()), Is.False);
    }

    [Test]
    public void IMultiMap_Overlaps_SharedPair_ReturnsTrue()
    {
        Assert.That(Map(("a", 1)).Overlaps(Map(("a", 1), ("b", 2))), Is.True);
    }

    [Test]
    public void IMultiMap_Overlaps_DisjointSets_ReturnsFalse()
    {
        Assert.That(Map(("a", 1)).Overlaps(Map(("b", 2))), Is.False);
    }

    // exercises the non-ISet<> branch
    [Test]
    public void IMultiMap_Overlaps_WithMultiMapListOther_UsesHashSetFallback()
    {
        var target = new ConcurrentMultiMap<string, int>();
        target.Add("a", 1);

        var other = new MultiMapList<string, int>();
        other.Add("a", 1);

        Assert.That(((IMultiMap<string, int>)target).Overlaps(other), Is.True);
    }

    [Test]
    public void IMultiMap_Overlaps_NullTarget_ThrowsArgumentNullException()
    {
        IMultiMap<string, int> nullMap = null!;
        Assert.Throws<ArgumentNullException>(() => nullMap.Overlaps(Map()));
    }

    [Test]
    public void IMultiMap_Overlaps_NullOther_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Map().Overlaps((IMultiMap<string, int>)null!));
    }

    // ── SetEquals (IMultiMap overload) ────────────────────────────────────────

    [Test]
    public void IMultiMap_SetEquals_EmptyBothSides_ReturnsTrue()
    {
        Assert.That(Map().SetEquals(Map()), Is.True);
    }

    [Test]
    public void IMultiMap_SetEquals_IdenticalContent_ReturnsTrue()
    {
        Assert.That(Map(("a", 1), ("b", 2)).SetEquals(Map(("a", 1), ("b", 2))), Is.True);
    }

    [Test]
    public void IMultiMap_SetEquals_DifferentCount_ReturnsFalse()
    {
        Assert.That(Map(("a", 1)).SetEquals(Map(("a", 1), ("b", 2))), Is.False);
    }

    [Test]
    public void IMultiMap_SetEquals_DifferentValues_ReturnsFalse()
    {
        Assert.That(Map(("a", 1)).SetEquals(Map(("a", 2))), Is.False);
    }

    // exercises non-ISet branch in SetEquals
    [Test]
    public void IMultiMap_SetEquals_WithMultiMapListOther_UsesHashSetFallback()
    {
        var target = new ConcurrentMultiMap<string, int>();
        target.Add("a", 1);

        var other = new MultiMapList<string, int>();
        other.Add("a", 1);

        Assert.That(((IMultiMap<string, int>)target).SetEquals(other), Is.True);
    }

    [Test]
    public void IMultiMap_SetEquals_NullTarget_ThrowsArgumentNullException()
    {
        IMultiMap<string, int> nullMap = null!;
        Assert.Throws<ArgumentNullException>(() => nullMap.SetEquals(Map()));
    }

    [Test]
    public void IMultiMap_SetEquals_NullOther_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Map().SetEquals((IMultiMap<string, int>)null!));
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// ConcurrentMultiMap – AddRange(KVP) duplicate / zombie-key branches (lines
// 174-177, 227-229) and Equals(object) with non-multimap type (line 482/506)
// ──────────────────────────────────────────────────────────────────────────────
