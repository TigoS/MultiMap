using MultiMap.Entities;

namespace MultiMap.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// MultiMapLock — CancellationToken overloads
//
// EnterWriteLockCancellable only checks the token INSIDE the retry loop, after
// a failed TryEnterWriteLock(20ms).  A pre-cancelled token on an uncontested
// lock still acquires immediately.  The "cancels while waiting" tests therefore
// HOLD the write lock from a background thread and then cancel, which forces
// the caller into the polling loop where the check fires.
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MultiMapLock_CancellationTokenTests
{
    private MultiMapLock<string, int> _map = null!;

    [SetUp]
    public void SetUp() => _map = new MultiMapLock<string, int>();

    [TearDown]
    public void TearDown() => _map.Dispose();

    // ── helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Occupies the write lock for ~500ms via a slow AddRange enumerable,
    /// signals when the lock is actually held, then runs <paramref name="action"/>
    /// with a token that cancels after 80ms — verifying OperationCanceledException.
    /// </summary>
    private void AssertCancelsWhileWaiting(Action<CancellationToken> action)
    {
        var lockHeld = new ManualResetEventSlim(false);
        var releaseLock = new ManualResetEventSlim(false);

        // Background thread: enter write lock via AddRange with a blocking enumerable.
        var bgTask = Task.Run(() =>
        {
            _map.AddRange(BlockingSequence(lockHeld, releaseLock));
        });

        // Wait until the background thread holds the write lock.
        Assert.That(lockHeld.Wait(TimeSpan.FromSeconds(5)), Is.True, "Background thread never acquired lock");

        try
        {
            using var cts = new CancellationTokenSource(80);
            Assert.Throws<OperationCanceledException>(() => action(cts.Token));
        }
        finally
        {
            releaseLock.Set();
            bgTask.Wait(TimeSpan.FromSeconds(5));
        }
    }

    private static IEnumerable<KeyValuePair<string, int>> BlockingSequence(
        ManualResetEventSlim lockHeld, ManualResetEventSlim releaseLock)
    {
        // Signal that we are now inside AddRange (i.e., write lock is held).
        lockHeld.Set();
        // Block until the test tells us to release.
        releaseLock.Wait(TimeSpan.FromSeconds(10));
        yield return new KeyValuePair<string, int>("__hold__", 0);
    }

    // ── Add(key, value, ct) ──────────────────────────────────────────────────

    [Test]
    public void Add_WithToken_AddsValue()
    {
        bool added = _map.Add("a", 1, CancellationToken.None);

        Assert.That(added, Is.True);
        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void Add_WithToken_DuplicateValue_ReturnsFalse()
    {
        _map.Add("a", 1);

        bool added = _map.Add("a", 1, CancellationToken.None);

        Assert.That(added, Is.False);
        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Add_WithToken_CancelsWhileWaiting_ThrowsOperationCanceledException()
    {
        AssertCancelsWhileWaiting(ct => _map.Add("z", 99, ct));
    }

    // ── AddRange(key, values, ct) ────────────────────────────────────────────

    [Test]
    public void AddRange_KeyValues_WithToken_AddsAllValues()
    {
        int count = _map.AddRange("a", new[] { 1, 2, 3 }, CancellationToken.None);

        Assert.That(count, Is.EqualTo(3));
        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void AddRange_KeyValues_WithToken_CancelsWhileWaiting_ThrowsOperationCanceledException()
    {
        AssertCancelsWhileWaiting(ct => _map.AddRange("z", new[] { 1, 2 }, ct));
    }

    // ── AddRange(items, ct) ──────────────────────────────────────────────────

    [Test]
    public void AddRange_Items_WithToken_AddsAllPairs()
    {
        var items = new[]
        {
            new KeyValuePair<string, int>("a", 1),
            new KeyValuePair<string, int>("b", 2),
        };

        int count = _map.AddRange(items, CancellationToken.None);

        Assert.That(count, Is.EqualTo(2));
        Assert.That(_map.Count, Is.EqualTo(2));
    }

    [Test]
    public void AddRange_Items_WithToken_CancelsWhileWaiting_ThrowsOperationCanceledException()
    {
        var items = new[] { new KeyValuePair<string, int>("z", 99) };
        AssertCancelsWhileWaiting(ct => _map.AddRange(items, ct));
    }

    // ── Remove(key, value, ct) ───────────────────────────────────────────────

    [Test]
    public void Remove_WithToken_ExistingPair_ReturnsTrue()
    {
        _map.Add("a", 1);

        bool removed = _map.Remove("a", 1, CancellationToken.None);

        Assert.That(removed, Is.True);
        Assert.That(_map.Count, Is.EqualTo(0));
    }

    [Test]
    public void Remove_WithToken_AbsentPair_ReturnsFalse()
    {
        bool removed = _map.Remove("z", 99, CancellationToken.None);

        Assert.That(removed, Is.False);
    }

    [Test]
    public void Remove_WithToken_CancelsWhileWaiting_ThrowsOperationCanceledException()
    {
        _map.Add("a", 1);
        AssertCancelsWhileWaiting(ct => _map.Remove("a", 1, ct));
    }

    // ── RemoveRange(items, ct) ───────────────────────────────────────────────

    [Test]
    public void RemoveRange_WithToken_RemovesMatchingPairs()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);

        var items = new[] { new KeyValuePair<string, int>("a", 1) };
        int removed = _map.RemoveRange(items, CancellationToken.None);

        Assert.That(removed, Is.EqualTo(1));
        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 2 }));
    }

    [Test]
    public void RemoveRange_WithToken_CancelsWhileWaiting_ThrowsOperationCanceledException()
    {
        _map.Add("a", 1);
        var items = new[] { new KeyValuePair<string, int>("a", 1) };
        AssertCancelsWhileWaiting(ct => _map.RemoveRange(items, ct));
    }

    // ── RemoveWhere(key, predicate, ct) ──────────────────────────────────────

    [Test]
    public void RemoveWhere_WithToken_RemovesMatching()
    {
        _map.AddRange("a", new[] { 1, 2, 3, 4 });

        int removed = _map.RemoveWhere("a", v => v % 2 == 0, CancellationToken.None);

        Assert.That(removed, Is.EqualTo(2));
        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1, 3 }));
    }

    [Test]
    public void RemoveWhere_WithToken_CancelsWhileWaiting_ThrowsOperationCanceledException()
    {
        _map.AddRange("a", new[] { 1, 2 });
        AssertCancelsWhileWaiting(ct => _map.RemoveWhere("a", _ => true, ct));
    }

    // ── RemoveKey(key, ct) ───────────────────────────────────────────────────

    [Test]
    public void RemoveKey_WithToken_ExistingKey_ReturnsTrue()
    {
        _map.Add("a", 1);

        bool removed = _map.RemoveKey("a", CancellationToken.None);

        Assert.That(removed, Is.True);
        Assert.That(_map.ContainsKey("a"), Is.False);
    }

    [Test]
    public void RemoveKey_WithToken_AbsentKey_ReturnsFalse()
    {
        bool removed = _map.RemoveKey("missing", CancellationToken.None);

        Assert.That(removed, Is.False);
    }

    [Test]
    public void RemoveKey_WithToken_CancelsWhileWaiting_ThrowsOperationCanceledException()
    {
        _map.Add("a", 1);
        AssertCancelsWhileWaiting(ct => _map.RemoveKey("a", ct));
    }

    // ── Clear(ct) ────────────────────────────────────────────────────────────

    [Test]
    public void Clear_WithToken_RemovesAll()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);

        _map.Clear(CancellationToken.None);

        Assert.That(_map.Count, Is.EqualTo(0));
        Assert.That(_map.KeyCount, Is.EqualTo(0));
    }

    [Test]
    public void Clear_WithToken_CancelsWhileWaiting_ThrowsOperationCanceledException()
    {
        _map.Add("a", 1);
        AssertCancelsWhileWaiting(ct => _map.Clear(ct));
    }

    // ── Union(other, ct) ─────────────────────────────────────────────────────

    [Test]
    public void Union_WithToken_MergesOther()
    {
        _map.Add("a", 1);

        using var other = new MultiMapLock<string, int>();
        other.Add("a", 2);
        other.Add("b", 3);

        _map.Union(other, CancellationToken.None);

        Assert.That(_map.Count, Is.EqualTo(3));
        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1, 2 }));
        Assert.That(_map.GetOrDefault("b"), Is.EquivalentTo(new[] { 3 }));
    }

    [Test]
    public void Union_WithToken_CancelsWhileWaiting_ThrowsOperationCanceledException()
    {
        using var other = new MultiMapLock<string, int>();
        other.Add("x", 10);
        AssertCancelsWhileWaiting(ct => _map.Union(other, ct));
    }

    // ── Intersect(other, ct) ─────────────────────────────────────────────────

    [Test]
    public void Intersect_WithToken_KeepsOnlyCommonPairs()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("b", 3);

        using var other = new MultiMapLock<string, int>();
        other.Add("a", 1);

        _map.Intersect(other, CancellationToken.None);

        Assert.That(_map.Count, Is.EqualTo(1));
        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(_map.ContainsKey("b"), Is.False);
    }

    [Test]
    public void Intersect_WithToken_CancelsWhileWaiting_ThrowsOperationCanceledException()
    {
        _map.Add("a", 1);
        using var other = new MultiMapLock<string, int>();
        other.Add("a", 1);
        AssertCancelsWhileWaiting(ct => _map.Intersect(other, ct));
    }

    // ── ExceptWith(other, ct) ────────────────────────────────────────────────

    [Test]
    public void ExceptWith_WithToken_RemovesCommonPairs()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);

        using var other = new MultiMapLock<string, int>();
        other.Add("a", 1);

        _map.ExceptWith(other, CancellationToken.None);

        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 2 }));
    }

    [Test]
    public void ExceptWith_WithToken_CancelsWhileWaiting_ThrowsOperationCanceledException()
    {
        _map.Add("a", 1);
        using var other = new MultiMapLock<string, int>();
        other.Add("a", 1);
        AssertCancelsWhileWaiting(ct => _map.ExceptWith(other, ct));
    }

    // ── SymmetricExceptWith(other, ct) ───────────────────────────────────────

    [Test]
    public void SymmetricExceptWith_WithToken_KeepsSymmetricDifference()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);

        using var other = new MultiMapLock<string, int>();
        other.Add("a", 2);
        other.Add("b", 3);

        _map.SymmetricExceptWith(other, CancellationToken.None);

        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(_map.GetOrDefault("b"), Is.EquivalentTo(new[] { 3 }));
    }

    [Test]
    public void SymmetricExceptWith_WithToken_CancelsWhileWaiting_ThrowsOperationCanceledException()
    {
        _map.Add("a", 1);
        using var other = new MultiMapLock<string, int>();
        other.Add("b", 2);
        AssertCancelsWhileWaiting(ct => _map.SymmetricExceptWith(other, ct));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Guard.NotNull — 3-parameter overload (with custom message) — null path
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class Guard_ThreeParamNotNullTests
{
    [Test]
    public void NotNull_ThreeParam_NullValue_ThrowsWithCustomMessage()
    {
        // AddRange(key, values) calls Guard.NotNull per element with a custom message.
        var map = new MultiMapSet<string, string>();

        var ex = Assert.Throws<ArgumentNullException>(
            () => map.AddRange("k", new string?[] { "a", null }!));

        Assert.That(ex!.Message, Does.Contain("Sequence contains a null value."));
    }

    [Test]
    public void NotNull_ThreeParam_NonNullValue_DoesNotThrow()
    {
        var map = new MultiMapSet<string, string>();

        Assert.DoesNotThrow(() => map.AddRange("k", new[] { "a", "b" }));
        Assert.That(map.Count, Is.EqualTo(2));
    }
}
