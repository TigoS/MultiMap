using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using MultiMap.Entities;
using MultiMap.Interfaces;

namespace MultiMap.Tests;

// ──────────────────────────────────────────────────────────────────────────────
// ConcurrentSet<T> — public ICollection<T> surface
//
// ConcurrentSet<T> has an internal constructor, so we obtain instances via
// reflection from the ConcurrentMultiMap's protected _dictionary field.
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class ConcurrentSetPublicSurfaceTests
{
    private static ConcurrentSet<int> GetSet(IEnumerable<int> seed)
    {
        var map = new ConcurrentMultiMap<string, int>();
        foreach (var v in seed)
            map.Add("k", v);

        var field = typeof(MultiMapBase<string, int, ConcurrentSet<int>>)
            .GetField("_dictionary", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var dict = (ConcurrentDictionary<string, ConcurrentSet<int>>)field.GetValue(map)!;
        return dict["k"];
    }

    private static ConcurrentSet<int> EmptySet()
    {
        var ctor = typeof(ConcurrentSet<int>)
            .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
                            null, new[] { typeof(IEqualityComparer<int>) }, null)!;
        return (ConcurrentSet<int>)ctor.Invoke(new object?[] { null });
    }

    // ── IsReadOnly ────────────────────────────────────────────────────────────

    [Test]
    public void IsReadOnly_AlwaysReturnsFalse()
    {
        ICollection<int> set = GetSet(new[] { 1 });

        Assert.That(set.IsReadOnly, Is.False);
    }

    // ── void Add(T) (ICollection explicit) ───────────────────────────────────

    [Test]
    public void Add_ICollection_InsertsElement()
    {
        ICollection<int> set = EmptySet();

        set.Add(42);

        Assert.That(set.Contains(42), Is.True);
        Assert.That(set.Count, Is.EqualTo(1));
    }

    [Test]
    public void Add_ICollection_DuplicateIsIdempotent()
    {
        ICollection<int> set = EmptySet();
        set.Add(1);
        set.Add(1);

        Assert.That(set.Count, Is.EqualTo(1));
    }

    // ── Clear() ───────────────────────────────────────────────────────────────

    [Test]
    public void Clear_RemovesAllElements()
    {
        ICollection<int> set = GetSet(new[] { 1, 2, 3 });

        set.Clear();

        Assert.That(set.Count, Is.EqualTo(0));
    }

    [Test]
    public void Clear_OnEmptySet_IsNoOp()
    {
        ICollection<int> set = EmptySet();

        Assert.DoesNotThrow(() => set.Clear());
        Assert.That(set.Count, Is.EqualTo(0));
    }

    // ── Remove(T) (ICollection explicit) ─────────────────────────────────────

    [Test]
    public void Remove_ExistingElement_ReturnsTrueAndReducesCount()
    {
        ICollection<int> set = GetSet(new[] { 10, 20 });

        bool removed = set.Remove(10);

        Assert.That(removed, Is.True);
        Assert.That(set.Count, Is.EqualTo(1));
        Assert.That(set.Contains(10), Is.False);
    }

    [Test]
    public void Remove_AbsentElement_ReturnsFalse()
    {
        ICollection<int> set = GetSet(new[] { 5 });

        bool removed = set.Remove(99);

        Assert.That(removed, Is.False);
        Assert.That(set.Count, Is.EqualTo(1));
    }

    // ── CopyTo(T[], int) — all 8 branches ─────────────────────────────────────
    //  Guard.NotNull → 2 branches
    //  arrayIndex < 0 || arrayIndex > array.Length → 4 branches
    //  index >= array.Length during loop → 2 branches

    [Test]
    public void CopyTo_NullArray_ThrowsArgumentNullException()
    {
        ICollection<int> set = GetSet(new[] { 1 });

        Assert.Throws<ArgumentNullException>(() => set.CopyTo(null!, 0));
    }

    [Test]
    public void CopyTo_NegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        ICollection<int> set = GetSet(new[] { 1 });
        var arr = new int[5];

        Assert.Throws<ArgumentOutOfRangeException>(() => set.CopyTo(arr, -1));
    }

    [Test]
    public void CopyTo_IndexGreaterThanArrayLength_ThrowsArgumentOutOfRangeException()
    {
        ICollection<int> set = GetSet(new[] { 1 });
        var arr = new int[2];

        Assert.Throws<ArgumentOutOfRangeException>(() => set.CopyTo(arr, 5));
    }

    [Test]
    public void CopyTo_DestinationArrayTooSmall_ThrowsArgumentException()
    {
        ICollection<int> set = GetSet(new[] { 1, 2, 3 });
        var arr = new int[2]; // only room for 2 elements; set has 3

        Assert.Throws<ArgumentException>(() => set.CopyTo(arr, 0));
    }

    [Test]
    public void CopyTo_ValidArgs_CopiesAllElements()
    {
        ICollection<int> set = GetSet(new[] { 7, 8, 9 });
        var arr = new int[5];

        set.CopyTo(arr, 1);

        var copied = arr.Skip(1).Take(3).ToHashSet();
        Assert.That(copied, Is.EquivalentTo(new[] { 7, 8, 9 }));
    }

    [Test]
    public void CopyTo_IndexEqualToArrayLength_WithEmptySet_DoesNotThrow()
    {
        ICollection<int> set = EmptySet();
        var arr = new int[0];

        // arrayIndex == 0 == array.Length: valid per contract when set is empty
        Assert.DoesNotThrow(() => set.CopyTo(arr, 0));
    }

    // ── IEnumerable.GetEnumerator() (explicit) ────────────────────────────────

    [Test]
    public void GetEnumerator_NonGeneric_IteratesAllElements()
    {
        ICollection<int> set = GetSet(new[] { 11, 22, 33 });
        var result = new List<object?>();

        var enumerator = ((IEnumerable)set).GetEnumerator();
        while (enumerator.MoveNext())
            result.Add(enumerator.Current);

        Assert.That(result, Is.EquivalentTo(new[] { 11, 22, 33 }));
    }

    [Test]
    public void GetEnumerator_NonGeneric_EmptySet_YieldsNothing()
    {
        ICollection<int> set = EmptySet();
        var result = new List<object?>();

        foreach (var item in (IEnumerable)set)
            result.Add(item);

        Assert.That(result, Is.Empty);
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// MultiMapAsync — general (non-MultiMapAsync) interface paths
// These hit the "slow" branches of IsSubsetOfAsync, IsSupersetOfAsync,
// OverlapsAsync, SetEqualsAsync and also the disposed ThrowIfDisposed branch.
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MultiMapAsync_GeneralInterfacePathTests
{
    // Adapter: wraps a MultiMapAsync<> but is NOT an instance of MultiMapAsync<>,
    // forcing the general (non-fast-path) branches.
    private sealed class WrappedMultiMapAsync<TKey, TValue> : IMultiMapAsync<TKey, TValue>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull, IEquatable<TValue>
    {
        private readonly MultiMapAsync<TKey, TValue> _inner;
        public WrappedMultiMapAsync(MultiMapAsync<TKey, TValue> inner) => _inner = inner;

        public ValueTask<bool> AddAsync(TKey key, TValue value, CancellationToken ct = default) => _inner.AddAsync(key, value, ct);
        public ValueTask<int> AddRangeAsync(TKey key, IEnumerable<TValue> values, CancellationToken ct = default) => _inner.AddRangeAsync(key, values, ct);
        public ValueTask<int> AddRangeAsync(IEnumerable<KeyValuePair<TKey, TValue>> items, CancellationToken ct = default) => _inner.AddRangeAsync(items, ct);
        public ValueTask<bool> RemoveAsync(TKey key, TValue value, CancellationToken ct = default) => _inner.RemoveAsync(key, value, ct);
        public ValueTask<int> RemoveRangeAsync(IEnumerable<KeyValuePair<TKey, TValue>> items, CancellationToken ct = default) => _inner.RemoveRangeAsync(items, ct);
        public ValueTask<int> RemoveWhereAsync(TKey key, Predicate<TValue> predicate, CancellationToken ct = default) => _inner.RemoveWhereAsync(key, predicate, ct);
        public ValueTask<bool> RemoveKeyAsync(TKey key, CancellationToken ct = default) => _inner.RemoveKeyAsync(key, ct);
        public Task ClearAsync(CancellationToken ct = default) => _inner.ClearAsync(ct);
        public ValueTask<bool> ContainsKeyAsync(TKey key, CancellationToken ct = default) => _inner.ContainsKeyAsync(key, ct);
        public ValueTask<bool> ContainsAsync(TKey key, TValue value, CancellationToken ct = default) => _inner.ContainsAsync(key, value, ct);
        public ValueTask<IEnumerable<TKey>> GetKeysAsync(CancellationToken ct = default) => _inner.GetKeysAsync(ct);
        public ValueTask<IEnumerable<TValue>> GetAsync(TKey key, CancellationToken ct = default) => _inner.GetAsync(key, ct);
        public ValueTask<IEnumerable<TValue>> GetOrDefaultAsync(TKey key, CancellationToken ct = default) => _inner.GetOrDefaultAsync(key, ct);
        public ValueTask<(bool found, IEnumerable<TValue> values)> TryGetAsync(TKey key, CancellationToken ct = default) => _inner.TryGetAsync(key, ct);
        public ValueTask<int> GetCountAsync(CancellationToken ct = default) => _inner.GetCountAsync(ct);
        public ValueTask<int> GetKeyCountAsync(CancellationToken ct = default) => _inner.GetKeyCountAsync(ct);
        public ValueTask<int> GetValuesCountAsync(TKey key, CancellationToken ct = default) => _inner.GetValuesCountAsync(key, ct);
        public ValueTask<IEnumerable<TValue>> GetValuesAsync(CancellationToken ct = default) => _inner.GetValuesAsync(ct);
        public Task<bool> IsSubsetOfAsync(IMultiMapAsync<TKey, TValue> other, CancellationToken ct = default) => _inner.IsSubsetOfAsync(other, ct);
        public Task<bool> IsSupersetOfAsync(IMultiMapAsync<TKey, TValue> other, CancellationToken ct = default) => _inner.IsSupersetOfAsync(other, ct);
        public Task<bool> OverlapsAsync(IMultiMapAsync<TKey, TValue> other, CancellationToken ct = default) => _inner.OverlapsAsync(other, ct);
        public Task<bool> SetEqualsAsync(IMultiMapAsync<TKey, TValue> other, CancellationToken ct = default) => _inner.SetEqualsAsync(other, ct);
        public ValueTask<bool> EqualsAsync(object? obj) => _inner.EqualsAsync(obj);
        public ValueTask<bool> EqualsAsync(IReadOnlyMultiMapAsync<TKey, TValue>? other, CancellationToken ct = default) => _inner.EqualsAsync(other, ct);
        public IAsyncEnumerator<KeyValuePair<TKey, TValue>> GetAsyncEnumerator(CancellationToken ct = default) => _inner.GetAsyncEnumerator(ct);
        public bool Equals(IReadOnlyMultiMapAsync<TKey, TValue>? other) => _inner.Equals(other);
        public void Dispose() { /* adapter does not own inner */ }
        public ValueTask DisposeAsync() => default;
    }

    // ── IsSubsetOfAsync — general path ───────────────────────────────────────

    [Test]
    public async Task IsSubsetOfAsync_GeneralPath_IsSubset_ReturnsTrue()
    {
        await using var subset = new MultiMapAsync<string, int>();
        await subset.AddAsync("a", 1);

        await using var superset = new MultiMapAsync<string, int>();
        await superset.AddAsync("a", 1);
        await superset.AddAsync("b", 2);

        var wrappedSuperset = new WrappedMultiMapAsync<string, int>(superset);

        Assert.That(await subset.IsSubsetOfAsync(wrappedSuperset), Is.True);
    }

    [Test]
    public async Task IsSubsetOfAsync_GeneralPath_KeyMissingInOther_ReturnsFalse()
    {
        await using var target = new MultiMapAsync<string, int>();
        await target.AddAsync("a", 1);
        await target.AddAsync("z", 99);

        await using var other = new MultiMapAsync<string, int>();
        await other.AddAsync("a", 1);

        var wrapped = new WrappedMultiMapAsync<string, int>(other);

        Assert.That(await target.IsSubsetOfAsync(wrapped), Is.False);
    }

    [Test]
    public async Task IsSubsetOfAsync_GeneralPath_ValueMissingInOther_ReturnsFalse()
    {
        await using var target = new MultiMapAsync<string, int>();
        await target.AddAsync("a", 1);
        await target.AddAsync("a", 2);

        await using var other = new MultiMapAsync<string, int>();
        await other.AddAsync("a", 1); // missing 2

        var wrapped = new WrappedMultiMapAsync<string, int>(other);

        Assert.That(await target.IsSubsetOfAsync(wrapped), Is.False);
    }

    [Test]
    public async Task IsSubsetOfAsync_GeneralPath_BothEmpty_ReturnsTrue()
    {
        await using var target = new MultiMapAsync<string, int>();
        await using var other = new MultiMapAsync<string, int>();

        var wrapped = new WrappedMultiMapAsync<string, int>(other);

        Assert.That(await target.IsSubsetOfAsync(wrapped), Is.True);
    }

    [Test]
    public async Task IsSubsetOfAsync_GeneralPath_OtherValuesIsHashSet_UsesHashSetBranch()
    {
        await using var target = new MultiMapAsync<string, int>();
        await target.AddAsync("a", 1);

        await using var other = new MultiMapAsync<string, int>();
        await other.AddAsync("a", 1);
        await other.AddAsync("a", 2);

        var wrapped = new WrappedMultiMapAsync<string, int>(other);

        Assert.That(await target.IsSubsetOfAsync(wrapped), Is.True);
    }

    // ── IsSupersetOfAsync — general path ──────────────────────────────────────

    [Test]
    public async Task IsSupersetOfAsync_GeneralPath_IsSuperset_ReturnsTrue()
    {
        await using var superset = new MultiMapAsync<string, int>();
        await superset.AddAsync("a", 1);
        await superset.AddAsync("b", 2);

        await using var subset = new MultiMapAsync<string, int>();
        await subset.AddAsync("a", 1);

        var wrappedSubset = new WrappedMultiMapAsync<string, int>(subset);

        Assert.That(await superset.IsSupersetOfAsync(wrappedSubset), Is.True);
    }

    [Test]
    public async Task IsSupersetOfAsync_GeneralPath_NotSuperset_ReturnsFalse()
    {
        await using var map = new MultiMapAsync<string, int>();
        await map.AddAsync("a", 1);

        await using var other = new MultiMapAsync<string, int>();
        await other.AddAsync("a", 1);
        await other.AddAsync("b", 99);

        var wrapped = new WrappedMultiMapAsync<string, int>(other);

        Assert.That(await map.IsSupersetOfAsync(wrapped), Is.False);
    }

    // ── OverlapsAsync — general path ──────────────────────────────────────────

    [Test]
    public async Task OverlapsAsync_GeneralPath_SharedPair_ReturnsTrue()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("x", 10);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("x", 10);
        await b.AddAsync("y", 20);

        var wrapped = new WrappedMultiMapAsync<string, int>(b);

        Assert.That(await a.OverlapsAsync(wrapped), Is.True);
    }

    [Test]
    public async Task OverlapsAsync_GeneralPath_NoSharedPair_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("x", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("y", 2);

        var wrapped = new WrappedMultiMapAsync<string, int>(b);

        Assert.That(await a.OverlapsAsync(wrapped), Is.False);
    }

    [Test]
    public async Task OverlapsAsync_GeneralPath_SameKeyDifferentValue_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("x", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("x", 99);

        var wrapped = new WrappedMultiMapAsync<string, int>(b);

        Assert.That(await a.OverlapsAsync(wrapped), Is.False);
    }

    [Test]
    public async Task OverlapsAsync_GeneralPath_BothEmpty_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await using var b = new MultiMapAsync<string, int>();

        var wrapped = new WrappedMultiMapAsync<string, int>(b);

        Assert.That(await a.OverlapsAsync(wrapped), Is.False);
    }

    // ── SetEqualsAsync — general path ─────────────────────────────────────────

    [Test]
    public async Task SetEqualsAsync_GeneralPath_SameContent_ReturnsTrue()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("k", 1);
        await a.AddAsync("k", 2);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("k", 1);
        await b.AddAsync("k", 2);

        var wrapped = new WrappedMultiMapAsync<string, int>(b);

        Assert.That(await a.SetEqualsAsync(wrapped), Is.True);
    }

    [Test]
    public async Task SetEqualsAsync_GeneralPath_DifferentCount_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("k", 1);
        await a.AddAsync("k", 2);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("k", 1);

        var wrapped = new WrappedMultiMapAsync<string, int>(b);

        Assert.That(await a.SetEqualsAsync(wrapped), Is.False);
    }

    [Test]
    public async Task SetEqualsAsync_GeneralPath_DifferentKeyCount_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("k1", 1);
        await a.AddAsync("k2", 2);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("k1", 1);

        var wrapped = new WrappedMultiMapAsync<string, int>(b);

        Assert.That(await a.SetEqualsAsync(wrapped), Is.False);
    }

    [Test]
    public async Task SetEqualsAsync_GeneralPath_KeyMissingInOther_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("k1", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("k2", 1);

        var wrapped = new WrappedMultiMapAsync<string, int>(b);

        Assert.That(await a.SetEqualsAsync(wrapped), Is.False);
    }

    [Test]
    public async Task SetEqualsAsync_GeneralPath_ValueMismatch_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("k", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("k", 99);

        var wrapped = new WrappedMultiMapAsync<string, int>(b);

        Assert.That(await a.SetEqualsAsync(wrapped), Is.False);
    }

    [Test]
    public async Task SetEqualsAsync_GeneralPath_BothEmpty_ReturnsTrue()
    {
        await using var a = new MultiMapAsync<string, int>();
        await using var b = new MultiMapAsync<string, int>();

        var wrapped = new WrappedMultiMapAsync<string, int>(b);

        Assert.That(await a.SetEqualsAsync(wrapped), Is.True);
    }

    // ── ThrowIfDisposed disposed branch ──────────────────────────────────────

    [Test]
    public async Task AddAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var map = new MultiMapAsync<string, int>();
        await map.AddAsync("a", 1);
        map.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(async () => await map.AddAsync("a", 2));
    }

    [Test]
    public async Task IsSubsetOfAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        await using var other = new MultiMapAsync<string, int>();
        var map = new MultiMapAsync<string, int>();
        map.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(async () => await map.IsSubsetOfAsync(other));
    }

    // ── ContainsAsync false branch (key not found) ─────────────────────────────

    [Test]
    public async Task ContainsAsync_KeyNotFound_ReturnsFalse()
    {
        await using var map = new MultiMapAsync<string, int>();
        await map.AddAsync("existing", 1);

        Assert.That(await map.ContainsAsync("nonexistent", 1), Is.False);
    }

    // ── GetValuesCountAsync false branch (key not found) ──────────────────────

    [Test]
    public async Task GetValuesCountAsync_KeyNotFound_ReturnsZero()
    {
        await using var map = new MultiMapAsync<string, int>();
        await map.AddAsync("real", 1);

        Assert.That(await map.GetValuesCountAsync("ghost"), Is.EqualTo(0));
    }

    // ── TryGetAsync false branch (key not found) ──────────────────────────────

    [Test]
    public async Task TryGetAsync_KeyNotFound_ReturnsFalse()
    {
        await using var map = new MultiMapAsync<string, int>();

        var (found, values) = await map.TryGetAsync("missing");

        Assert.That(found, Is.False);
        Assert.That(values, Is.Empty);
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// ConcurrentMultiMap<T1,T2> — Equals branch coverage
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class ConcurrentMultiMap_EqualsBranchTests
{
    [Test]
    public void Equals_IReadOnlyMultiMap_Null_ReturnsFalse()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>?)null), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameInstance_ReturnsTrue()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>)map), Is.True);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_DifferentKeyCount_ReturnsFalse()
    {
        var a = new ConcurrentMultiMap<string, int>();
        a.Add("a", 1);

        var b = new ConcurrentMultiMap<string, int>();
        b.Add("a", 1);
        b.Add("b", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_DifferentTotalCount_ReturnsFalse()
    {
        var a = new ConcurrentMultiMap<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new ConcurrentMultiMap<string, int>();
        b.Add("a", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_KeyNotFoundInOther_ReturnsFalse()
    {
        var a = new ConcurrentMultiMap<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>(); // different type, different key
        b.Add("z", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_ValueCountMismatch_ReturnsFalse()
    {
        var a = new ConcurrentMultiMap<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 1);
        b.Add("a", 3); // same count but different value — this tests valCount check

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameContent_ReturnsTrue()
    {
        var a = new ConcurrentMultiMap<string, int>();
        a.Add("a", 1);
        a.Add("b", 2);

        var b = new ConcurrentMultiMap<string, int>();
        b.Add("a", 1);
        b.Add("b", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.True);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_ValueNotFoundInOther_ReturnsFalse()
    {
        var a = new ConcurrentMultiMap<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 99); // same key, different value

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    // ── RemoveWhereFromCollection — concurrent predicate branches ────────────

    [Test]
    public void RemoveWhere_PredicateMatchesNone_ReturnsZero()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add("k", 1);
        map.Add("k", 2);
        map.Add("k", 3);

        int removed = map.RemoveWhere("k", v => v > 100);

        Assert.That(removed, Is.EqualTo(0));
        Assert.That(map.Count, Is.EqualTo(3));
    }

    [Test]
    public void RemoveWhere_PredicateMatchesAll_RemovesAllAndCleansKey()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add("k", 1);
        map.Add("k", 2);
        map.Add("k", 3);

        int removed = map.RemoveWhere("k", _ => true);

        Assert.That(removed, Is.EqualTo(3));
        Assert.That(map.ContainsKey("k"), Is.False);
        Assert.That(map.Count, Is.EqualTo(0));
    }

    [Test]
    [Category("Concurrent")]
    public void RemoveWhere_ConcurrentRemovalAndAdd_CountRemainsSane()
    {
        var map = new ConcurrentMultiMap<string, int>();
        for (int i = 0; i < 50; i++)
            map.Add("k", i);

        var remover = Task.Run(() => map.RemoveWhere("k", v => v % 2 == 0));
        var adder = Task.Run(() =>
        {
            for (int i = 100; i < 150; i++)
                map.Add("k", i);
        });

        Task.WaitAll(remover, adder);

        Assert.That(map.Count, Is.GreaterThanOrEqualTo(0));
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// MultiMapSet<T1,T2> — Equals branch coverage
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MultiMapSet_EqualsBranchTests
{
    [Test]
    public void Equals_IReadOnlyMultiMap_Null_ReturnsFalse()
    {
        var map = new MultiMapSet<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>?)null), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameInstance_ReturnsTrue()
    {
        var map = new MultiMapSet<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>)map), Is.True);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_DifferentKeyCount_ReturnsFalse()
    {
        var a = new MultiMapSet<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 1);
        b.Add("b", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_DifferentCount_ReturnsFalse()
    {
        var a = new MultiMapSet<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_KeyNotFoundInOther_ReturnsFalse()
    {
        var a = new MultiMapSet<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("z", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_ValueCountMismatch_ReturnsFalse()
    {
        var a = new MultiMapSet<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 1);
        b.Add("a", 3); // same count, but SetEquals will differ

        // tweak: use different actual sizes
        var a2 = new MultiMapSet<string, int>();
        a2.Add("a", 1);
        a2.Add("a", 2);
        var b2 = new MultiMapSet<string, int>();
        b2.Add("a", 5); // different count

        Assert.That(a2.Equals((IReadOnlyMultiMap<string, int>)b2), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SetNotEqual_ReturnsFalse()
    {
        var a = new MultiMapSet<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 99);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameContent_ReturnsTrue()
    {
        var a = new MultiMapSet<string, int>();
        a.Add("a", 1);
        a.Add("b", 2);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 1);
        b.Add("b", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.True);
    }

    [Test]
    public void Equals_Object_NonMapType_ReturnsFalse()
    {
        var map = new MultiMapSet<string, int>();
        Assert.That(map.Equals((object)"something"), Is.False);
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// MultiMapList<T1,T2> — Equals branch coverage
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MultiMapList_EqualsBranchTests
{
    [Test]
    public void Equals_IReadOnlyMultiMap_Null_ReturnsFalse()
    {
        var map = new MultiMapList<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>?)null), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameInstance_ReturnsTrue()
    {
        var map = new MultiMapList<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>)map), Is.True);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_DifferentKeyCount_ReturnsFalse()
    {
        var a = new MultiMapList<string, int>();
        a.Add("a", 1);

        var b = new MultiMapList<string, int>();
        b.Add("a", 1);
        b.Add("b", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_DifferentCount_ReturnsFalse()
    {
        var a = new MultiMapList<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new MultiMapList<string, int>();
        b.Add("a", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_KeyNotFoundInOther_ReturnsFalse()
    {
        var a = new MultiMapList<string, int>();
        a.Add("a", 1);

        var b = new MultiMapList<string, int>();
        b.Add("z", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_ValuesCountMismatch_ReturnsFalse()
    {
        var a = new MultiMapList<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new MultiMapList<string, int>();
        b.Add("a", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SequenceNotEqual_ReturnsFalse()
    {
        var a = new MultiMapList<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new MultiMapList<string, int>();
        b.Add("a", 2);
        b.Add("a", 1); // different order

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameSequence_ReturnsTrue()
    {
        var a = new MultiMapList<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new MultiMapList<string, int>();
        b.Add("a", 1);
        b.Add("a", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.True);
    }

    [Test]
    public void Equals_Object_NonMapType_ReturnsFalse()
    {
        var map = new MultiMapList<string, int>();
        Assert.That(map.Equals((object)"something"), Is.False);
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// SortedMultiMap<T1,T2> — Equals branch coverage
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class SortedMultiMap_EqualsBranchTests
{
    [Test]
    public void Equals_IReadOnlyMultiMap_Null_ReturnsFalse()
    {
        var map = new SortedMultiMap<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>?)null), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameInstance_ReturnsTrue()
    {
        var map = new SortedMultiMap<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>)map), Is.True);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_DifferentKeyCount_ReturnsFalse()
    {
        var a = new SortedMultiMap<string, int>();
        a.Add("a", 1);

        var b = new SortedMultiMap<string, int>();
        b.Add("a", 1);
        b.Add("b", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_DifferentCount_ReturnsFalse()
    {
        var a = new SortedMultiMap<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new SortedMultiMap<string, int>();
        b.Add("a", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_KeyNotFoundInOther_ReturnsFalse()
    {
        var a = new SortedMultiMap<string, int>();
        a.Add("a", 1);

        var b = new SortedMultiMap<string, int>();
        b.Add("z", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_ValueCountMismatch_ReturnsFalse()
    {
        var a = new SortedMultiMap<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new SortedMultiMap<string, int>();
        b.Add("a", 5); // one value vs two

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SetNotEqual_ReturnsFalse()
    {
        var a = new SortedMultiMap<string, int>();
        a.Add("a", 1);

        var b = new SortedMultiMap<string, int>();
        b.Add("a", 99);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameContent_ReturnsTrue()
    {
        var a = new SortedMultiMap<string, int>();
        a.Add("a", 1);
        a.Add("b", 3);

        var b = new SortedMultiMap<string, int>();
        b.Add("b", 3);
        b.Add("a", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.True);
    }

    [Test]
    public void Equals_Object_NonMapType_ReturnsFalse()
    {
        var map = new SortedMultiMap<string, int>();
        Assert.That(map.Equals((object)42), Is.False);
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// MultiMapLock<T1,T2> — Equals and SetEquals branch coverage
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MultiMapLock_EqualsBranchTests
{
    // ── Equals(IReadOnlyMultiMap) ────────────────────────────────────────────

    [Test]
    public void Equals_IReadOnlyMultiMap_Null_ReturnsFalse()
    {
        using var map = new MultiMapLock<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>?)null), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameInstance_ReturnsTrue()
    {
        using var map = new MultiMapLock<string, int>();
        map.Add("a", 1);

        Assert.That(map.Equals((IReadOnlyMultiMap<string, int>)map), Is.True);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_DifferentKeyCount_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);

        using var b = new MultiMapLock<string, int>();
        b.Add("a", 1);
        b.Add("b", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_DifferentCount_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        using var b = new MultiMapLock<string, int>();
        b.Add("a", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_KeyNotFoundInOther_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("z", 1);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_ValueCountMismatch_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 1); // key present, but count differs

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_ValueNotFoundInOther_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 99); // same key, different value

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.False);
    }

    [Test]
    public void Equals_IReadOnlyMultiMap_SameContent_ReturnsTrue()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);
        a.Add("b", 2);

        using var b = new MultiMapLock<string, int>();
        b.Add("a", 1);
        b.Add("b", 2);

        Assert.That(a.Equals((IReadOnlyMultiMap<string, int>)b), Is.True);
    }

    [Test]
    public void Equals_Object_NonMapType_ReturnsFalse()
    {
        using var map = new MultiMapLock<string, int>();
        Assert.That(map.Equals((object)"irrelevant"), Is.False);
    }

    // ── SetEquals(IMultiMap) ──────────────────────────────────────────────────

    [Test]
    public void SetEquals_SameInstance_ReturnsTrue()
    {
        using var map = new MultiMapLock<string, int>();
        map.Add("a", 1);

        Assert.That(map.SetEquals(map), Is.True);
    }

    [Test]
    public void SetEquals_DifferentCount_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);
        a.Add("a", 2);

        using var b = new MultiMapLock<string, int>();
        b.Add("a", 1);

        Assert.That(a.SetEquals(b), Is.False);
    }

    [Test]
    public void SetEquals_DifferentKeyCount_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);

        using var b = new MultiMapLock<string, int>();
        b.Add("a", 1);
        b.Add("b", 2);

        Assert.That(a.SetEquals(b), Is.False);
    }

    [Test]
    public void SetEquals_KeyNotFoundInThis_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("z", 1); // different key — note: MultiMapSet doesn't implement IDisposable

        Assert.That(a.SetEquals(b), Is.False);
    }

    [Test]
    public void SetEquals_ValueCountMismatch_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 1);
        b.Add("a", 2); // more values

        Assert.That(a.SetEquals(b), Is.False);
    }

    [Test]
    public void SetEquals_ValueSetNotEqual_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 99); // different value

        Assert.That(a.SetEquals(b), Is.False);
    }

    [Test]
    public void SetEquals_SameContent_ReturnsTrue()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("x", 10);
        a.Add("y", 20);

        var b = new MultiMapSet<string, int>();
        b.Add("x", 10);
        b.Add("y", 20);

        Assert.That(a.SetEquals(b), Is.True);
    }

    // ── SetEquals — snapshot count vs post-read dictionary size divergence ──

    [Test]
    public void SetEquals_SnapshotDictionaryCountDiffers_ReturnsFalse()
    {
        using var a = new MultiMapLock<string, int>();
        a.Add("a", 1);
        a.Add("b", 2);

        var b = new MultiMapSet<string, int>();
        b.Add("a", 1); // different key count supplied to SetEquals

        // A has 2 keys, but b has 1 → snapshot.Count != dictionary.Count path
        Assert.That(a.SetEquals(b), Is.False);
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// ConcurrentMultiMap — stress tests targeting concurrent remove branches
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
[Category("Stress")]
[Category("Concurrent")]
public class ConcurrentMultiMap_RemoveStressTests
{
    [Test]
    public void Remove_ConcurrentAddAndRemoveSameKey_CountNeverNegative()
    {
        var map = new ConcurrentMultiMap<string, int>();
        const int iterations = 200;

        var adders = Enumerable.Range(0, iterations)
            .Select(i => Task.Run(() => map.Add("k", i)))
            .ToArray();
        var removers = Enumerable.Range(0, iterations)
            .Select(i => Task.Run(() => map.Remove("k", i)))
            .ToArray();

        Task.WaitAll(adders.Concat(removers).ToArray());

        Assert.That(map.Count, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void RemoveKey_ConcurrentAddAndRemoveKey_CountNeverNegative()
    {
        var map = new ConcurrentMultiMap<string, int>();
        const int iterations = 200;

        var tasks = Enumerable.Range(0, iterations).SelectMany(i => new[]
        {
            Task.Run(() => map.Add("k", i)),
            Task.Run(() => map.RemoveKey("k"))
        }).ToArray();

        Task.WaitAll(tasks);

        Assert.That(map.Count, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void AddRange_ConcurrentCalls_AllValuesEventuallyPresent()
    {
        var map = new ConcurrentMultiMap<string, int>();
        const int threads = 8;
        const int perThread = 25;

        var tasks = Enumerable.Range(0, threads)
            .Select(t => Task.Run(() =>
            {
                int start = t * perThread;
                map.AddRange("k", Enumerable.Range(start, perThread));
            })).ToArray();

        Task.WaitAll(tasks);

        Assert.That(map.Count, Is.EqualTo(threads * perThread));
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// MultiMapAsync — IsSubsetOfAsync / OverlapsAsync / SetEqualsAsync fast-path
// branches that were not yet hit (count/key mismatch early-return in fast path)
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MultiMapAsync_FastPathBranchTests
{
    [Test]
    public async Task SetEqualsAsync_FastPath_DifferentCount_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("k", 1);
        await a.AddAsync("k", 2);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("k", 1);

        Assert.That(await a.SetEqualsAsync(b), Is.False);
    }

    [Test]
    public async Task SetEqualsAsync_FastPath_DifferentKeyCount_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("a", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("a", 1);
        await b.AddAsync("b", 2);

        Assert.That(await a.SetEqualsAsync(b), Is.False);
    }

    [Test]
    public async Task SetEqualsAsync_FastPath_KeyMissingInOther_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("a", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("z", 1);

        Assert.That(await a.SetEqualsAsync(b), Is.False);
    }

    [Test]
    public async Task SetEqualsAsync_FastPath_ValueSetNotEqual_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("a", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("a", 99);

        Assert.That(await a.SetEqualsAsync(b), Is.False);
    }

    [Test]
    public async Task IsSubsetOfAsync_FastPath_KeyMissingInOther_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("x", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("y", 1);

        Assert.That(await a.IsSubsetOfAsync(b), Is.False);
    }

    [Test]
    public async Task IsSubsetOfAsync_FastPath_ValueMissingInOther_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("k", 1);
        await a.AddAsync("k", 2);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("k", 1);

        Assert.That(await a.IsSubsetOfAsync(b), Is.False);
    }

    [Test]
    public async Task OverlapsAsync_FastPath_NoSharedValue_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("k", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("k", 99);

        Assert.That(await a.OverlapsAsync(b), Is.False);
    }

    [Test]
    public async Task OverlapsAsync_FastPath_NoSharedKey_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await a.AddAsync("a", 1);

        await using var b = new MultiMapAsync<string, int>();
        await b.AddAsync("z", 1);

        Assert.That(await a.OverlapsAsync(b), Is.False);
    }

    [Test]
    public async Task OverlapsAsync_FastPath_BothEmpty_ReturnsFalse()
    {
        await using var a = new MultiMapAsync<string, int>();
        await using var b = new MultiMapAsync<string, int>();

        Assert.That(await a.OverlapsAsync(b), Is.False);
    }
}
