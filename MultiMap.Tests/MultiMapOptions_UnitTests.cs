using MultiMap.Entities;

namespace MultiMap.Tests;

[TestFixture]
public class MultiMapOptionsTests
{
    // ─── MultiMapOptions ────────────────────────────────────────────────────────

    [Test]
    public void MultiMapOptions_DefaultValues_AreZeroAndNull()
    {
        var opts = new MultiMapOptions<string, int>();

        Assert.That(opts.Capacity, Is.EqualTo(0));
        Assert.That(opts.KeyComparer, Is.Null);
        Assert.That(opts.ValueComparer, Is.Null);
    }

    [Test]
    public void MultiMapOptions_WithValues_StoresCorrectly()
    {
        var keyComp = StringComparer.OrdinalIgnoreCase;
        var valComp = EqualityComparer<int>.Default;

        var opts = new MultiMapOptions<string, int>
        {
            Capacity = 64,
            KeyComparer = keyComp,
            ValueComparer = valComp
        };

        Assert.That(opts.Capacity, Is.EqualTo(64));
        Assert.That(opts.KeyComparer, Is.SameAs(keyComp));
        Assert.That(opts.ValueComparer, Is.SameAs(valComp));
    }

    // ─── MultiMapSet ─────────────────────────────────────────────────────────────

    [Test]
    public void MultiMapSet_OptionsConstructor_NullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _ = new MultiMapSet<string, int>((MultiMapOptions<string, int>)null!));
    }

    [Test]
    public void MultiMapSet_OptionsConstructor_CreatesInstance()
    {
        var opts = new MultiMapOptions<string, int> { Capacity = 16 };
        var map = new MultiMapSet<string, int>(opts);

        Assert.That(map, Is.Not.Null);
        Assert.That(map.Count, Is.EqualTo(0));
    }

    [Test]
    public void MultiMapSet_OptionsConstructor_KeyComparerIsUsed()
    {
        var opts = new MultiMapOptions<string, int>
        {
            KeyComparer = StringComparer.OrdinalIgnoreCase
        };
        var map = new MultiMapSet<string, int>(opts);

        map.Add("KEY", 1);

        Assert.That(map.ContainsKey("key"), Is.True);
    }

    [Test]
    public void MultiMapSet_OptionsConstructor_ValueComparerIsUsed()
    {
        var opts = new MultiMapOptions<string, int>();
        var map = new MultiMapSet<string, int>(opts);

        map.Add("a", 1);
        var added = map.Add("a", 1); // duplicate — should be rejected

        Assert.That(added, Is.False);
        Assert.That(map.Count, Is.EqualTo(1));
    }

    // ─── MultiMapList ────────────────────────────────────────────────────────────

    [Test]
    public void MultiMapList_OptionsConstructor_NullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _ = new MultiMapList<string, int>((MultiMapOptions<string, int>)null!));
    }

    [Test]
    public void MultiMapList_OptionsConstructor_CreatesInstance()
    {
        var opts = new MultiMapOptions<string, int> { Capacity = 8 };
        var map = new MultiMapList<string, int>(opts);

        Assert.That(map, Is.Not.Null);
        Assert.That(map.Count, Is.EqualTo(0));
    }

    [Test]
    public void MultiMapList_OptionsConstructor_KeyComparerIsUsed()
    {
        var opts = new MultiMapOptions<string, int>
        {
            KeyComparer = StringComparer.OrdinalIgnoreCase
        };
        var map = new MultiMapList<string, int>(opts);

        map.Add("KEY", 1);

        Assert.That(map.ContainsKey("key"), Is.True);
    }

    [Test]
    public void MultiMapList_OptionsConstructor_AllowsDuplicateValues()
    {
        var opts = new MultiMapOptions<string, int>();
        var map = new MultiMapList<string, int>(opts);

        map.Add("a", 1);
        map.Add("a", 1);

        Assert.That(map.Count, Is.EqualTo(2));
    }

    // ─── SortedMultiMap ──────────────────────────────────────────────────────────

    [Test]
    public void SortedMultiMap_OptionsConstructor_NullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _ = new SortedMultiMap<string, int>((SortedMultiMapOptions<string, int>)null!));
    }

    [Test]
    public void SortedMultiMap_OptionsConstructor_CreatesInstance()
    {
        var opts = new SortedMultiMapOptions<string, int>();
        var map = new SortedMultiMap<string, int>(opts);

        Assert.That(map, Is.Not.Null);
        Assert.That(map.Count, Is.EqualTo(0));
    }

    [Test]
    public void SortedMultiMap_OptionsConstructor_KeyComparerIsUsed()
    {
        var opts = new SortedMultiMapOptions<string, int>
        {
            KeyComparer = StringComparer.OrdinalIgnoreCase
        };
        var map = new SortedMultiMap<string, int>(opts);

        map.Add("KEY", 1);

        Assert.That(map.ContainsKey("key"), Is.True);
    }

    [Test]
    public void SortedMultiMap_OptionsConstructor_InvalidValueComparer_ThrowsArgumentException()
    {
        // A bare IComparer without IEqualityComparer should be rejected
        var opts = new SortedMultiMapOptions<string, int>
        {
            ValueComparer = Comparer<int>.Default // does NOT implement IEqualityComparer<int>
        };

        Assert.Throws<ArgumentException>(() => _ = new SortedMultiMap<string, int>(opts));
    }

    // ─── ConcurrentMultiMap ──────────────────────────────────────────────────────

    [Test]
    public void ConcurrentMultiMap_OptionsConstructor_NullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _ = new ConcurrentMultiMap<string, int>((ConcurrentMultiMapOptions<string, int>)null!));
    }

    [Test]
    public void ConcurrentMultiMap_OptionsConstructor_CreatesInstance()
    {
        var opts = new ConcurrentMultiMapOptions<string, int> { Capacity = 16 };
        var map = new ConcurrentMultiMap<string, int>(opts);

        Assert.That(map, Is.Not.Null);
        Assert.That(map.Count, Is.EqualTo(0));
    }

    [Test]
    public void ConcurrentMultiMap_OptionsConstructor_KeyComparerIsUsed()
    {
        var opts = new ConcurrentMultiMapOptions<string, int>
        {
            KeyComparer = StringComparer.OrdinalIgnoreCase
        };
        var map = new ConcurrentMultiMap<string, int>(opts);

        map.Add("KEY", 1);

        Assert.That(map.ContainsKey("key"), Is.True);
    }

    [Test]
    public void ConcurrentMultiMap_OptionsConstructor_ConcurrencyLevelAndCapacity_CreatesInstance()
    {
        var opts = new ConcurrentMultiMapOptions<string, int>
        {
            ConcurrencyLevel = 4,
            Capacity = 32
        };
        var map = new ConcurrentMultiMap<string, int>(opts);

        Assert.That(map, Is.Not.Null);
        Assert.That(map.Count, Is.EqualTo(0));
    }

    [Test]
    public void ConcurrentMultiMap_OptionsConstructor_CapacityOnly_CreatesInstance()
    {
        var opts = new ConcurrentMultiMapOptions<string, int>
        {
            Capacity = 32
        };
        var map = new ConcurrentMultiMap<string, int>(opts);

        Assert.That(map, Is.Not.Null);
    }

    // ─── MultiMapLock ────────────────────────────────────────────────────────────

    [Test]
    public void MultiMapLock_OptionsConstructor_NullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _ = new MultiMapLock<string, int>((MultiMapOptions<string, int>)null!));
    }

    [Test]
    public void MultiMapLock_OptionsConstructor_CreatesInstance()
    {
        var opts = new MultiMapOptions<string, int> { Capacity = 8 };
        var map = new MultiMapLock<string, int>(opts);

        Assert.That(map, Is.Not.Null);
        Assert.That(map.Count, Is.EqualTo(0));
    }

    [Test]
    public void MultiMapLock_OptionsConstructor_KeyComparerIsUsed()
    {
        var opts = new MultiMapOptions<string, int>
        {
            KeyComparer = StringComparer.OrdinalIgnoreCase
        };
        var map = new MultiMapLock<string, int>(opts);

        map.Add("KEY", 1);

        Assert.That(map.ContainsKey("key"), Is.True);
    }

    // ─── MultiMapAsync ───────────────────────────────────────────────────────────

    [Test]
    public void MultiMapAsync_OptionsConstructor_NullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _ = new MultiMapAsync<string, int>((MultiMapOptions<string, int>)null!));
    }

    [Test]
    public async Task MultiMapAsync_OptionsConstructor_CreatesInstance()
    {
        var opts = new MultiMapOptions<string, int> { Capacity = 8 };
        var map = new MultiMapAsync<string, int>(opts);

        Assert.That(map, Is.Not.Null);
        Assert.That(await map.GetCountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task MultiMapAsync_OptionsConstructor_KeyComparerIsUsed()
    {
        var opts = new MultiMapOptions<string, int>
        {
            KeyComparer = StringComparer.OrdinalIgnoreCase
        };
        var map = new MultiMapAsync<string, int>(opts);

        await map.AddAsync("KEY", 1);

        Assert.That(await map.ContainsKeyAsync("key"), Is.True);
    }

    // ─── ConcurrentMultiMapOptions ───────────────────────────────────────────────

    [Test]
    public void ConcurrentMultiMapOptions_DefaultValues_AreZeroAndNull()
    {
        var opts = new ConcurrentMultiMapOptions<string, int>();

        Assert.That(opts.ConcurrencyLevel, Is.EqualTo(0));
        Assert.That(opts.Capacity, Is.EqualTo(0));
        Assert.That(opts.KeyComparer, Is.Null);
        Assert.That(opts.ValueComparer, Is.Null);
    }

    // ─── SortedMultiMapOptions ───────────────────────────────────────────────────

    [Test]
    public void SortedMultiMapOptions_DefaultValues_AreNull()
    {
        var opts = new SortedMultiMapOptions<string, int>();

        Assert.That(opts.KeyComparer, Is.Null);
        Assert.That(opts.ValueComparer, Is.Null);
    }
}
