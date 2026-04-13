using MultiMap.Entities;
using MultiMap.Interfaces;

namespace MultiMap.Tests;

[TestFixture]
public class SimpleMultiMapTests
{
    private ISimpleMultiMap<string, int> _map;

    [SetUp]
    public void SetUp()
    {
        _map = new SimpleMultiMap<string, int>();
    }

    // ── Add ────────────────────────────────────────────────

    [Test]
    public void Add_NewKeyAndValue_ReturnsTrue()
    {
        Assert.That(_map.Add("a", 1), Is.True);
    }

    [Test]
    public void Add_SingleKeyValue_CanBeRetrieved()
    {
        _map.Add("a", 1);

        Assert.That(_map.Get("a"), Is.EquivalentTo(new[] { 1 }));
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

        Assert.That(_map.Get("a"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void Add_MultipleValuesForSameKey_ReturnsAllValues()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("a", 3);

        Assert.That(_map.Get("a"), Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void Add_DifferentKeys_StoresIndependently()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);

        Assert.That(_map.Get("a"), Is.EquivalentTo(new[] { 1 }));
        Assert.That(_map.Get("b"), Is.EquivalentTo(new[] { 2 }));
    }

    // ── Get ────────────────────────────────────────────────

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

    // ── GetOrDefault ───────────────────────────────────────

    [Test]
    public void GetOrDefault_ExistingKey_ReturnsValues()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);

        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public void GetOrDefault_NonExistentKey_ReturnsEmpty()
    {
        Assert.That(_map.GetOrDefault("missing"), Is.Empty);
    }

    [Test]
    public void GetOrDefault_AfterClearingKey_ReturnsEmpty()
    {
        _map.Add("a", 1);
        _map.Clear("a");

        Assert.That(_map.GetOrDefault("a"), Is.Empty);
    }

    // ── Remove ─────────────────────────────────────────────

    [Test]
    public void Remove_ExistingValue_ValueIsRemoved()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);

        _map.Remove("a", 1);

        Assert.That(_map.GetOrDefault("a"), Is.EquivalentTo(new[] { 2 }));
    }

    [Test]
    public void Remove_LastValueForKey_RemovesKey()
    {
        _map.Add("a", 1);

        _map.Remove("a", 1);

        Assert.That(_map.GetOrDefault("a"), Is.Empty);
        Assert.Throws<KeyNotFoundException>(() => _map.Get("a"));
    }

    [Test]
    public void Remove_NonExistentValue_DoesNotThrow()
    {
        _map.Add("a", 1);

        Assert.DoesNotThrow(() => _map.Remove("a", 99));
        Assert.That(_map.Get("a"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void Remove_NonExistentKey_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _map.Remove("missing", 1));
    }

    [Test]
    public void Remove_DoesNotAffectOtherKeys()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);

        _map.Remove("a", 1);

        Assert.That(_map.Get("b"), Is.EquivalentTo(new[] { 2 }));
    }

    // ── Clear(key) ─────────────────────────────────────────

    [Test]
    public void Clear_ExistingKey_RemovesAllValues()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("a", 3);

        _map.Clear("a");

        Assert.That(_map.GetOrDefault("a"), Is.Empty);
    }

    [Test]
    public void Clear_ExistingKey_KeyNoLongerExists()
    {
        _map.Add("a", 1);

        _map.Clear("a");

        Assert.Throws<KeyNotFoundException>(() => _map.Get("a"));
    }

    [Test]
    public void Clear_NonExistentKey_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _map.Clear("missing"));
    }

    [Test]
    public void Clear_DoesNotAffectOtherKeys()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);

        _map.Clear("a");

        Assert.That(_map.Get("b"), Is.EquivalentTo(new[] { 2 }));
    }

    // ── Flatten ────────────────────────────────────────────

    [Test]
    public void Flatten_ReturnsAllKeyValuePairs()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("b", 3);

        var pairs = _map.Flatten().ToList();

        Assert.That(pairs, Has.Count.EqualTo(3));
        Assert.That(pairs, Does.Contain(new KeyValuePair<string, int>("a", 1)));
        Assert.That(pairs, Does.Contain(new KeyValuePair<string, int>("a", 2)));
        Assert.That(pairs, Does.Contain(new KeyValuePair<string, int>("b", 3)));
    }

    [Test]
    public void Flatten_EmptyMap_ReturnsEmpty()
    {
        Assert.That(_map.Flatten(), Is.Empty);
    }

    [Test]
    public void Flatten_AfterRemoval_ReflectsCurrentState()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Remove("a", 1);

        var pairs = _map.Flatten().ToList();

        Assert.That(pairs, Has.Count.EqualTo(1));
        Assert.That(pairs, Does.Contain(new KeyValuePair<string, int>("a", 2)));
    }

    [Test]
    public void Flatten_SingleKey_ReturnsAllValuesForKey()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);

        var pairs = _map.Flatten().ToList();

        Assert.That(pairs, Has.Count.EqualTo(2));
        Assert.That(pairs, Has.All.Matches<KeyValuePair<string, int>>(kvp => kvp.Key == "a"));
    }

    // ── GetEnumerator ──────────────────────────────────────

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
    public void GetEnumerator_AfterRemoval_ReflectsCurrentState()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Remove("a", 1);

        var pairs = _map.ToList();

        Assert.That(pairs, Has.Count.EqualTo(1));
        Assert.That(pairs, Does.Contain(new KeyValuePair<string, int>("a", 2)));
    }

    // ── Integration / Cross-method ─────────────────────────

    [Test]
    public void AddThenClearThenAdd_WorksCorrectly()
    {
        _map.Add("a", 1);
        _map.Clear("a");
        _map.Add("a", 2);

        Assert.That(_map.Get("a"), Is.EquivalentTo(new[] { 2 }));
    }

    [Test]
    public void AddThenRemoveAll_ThenGetOrDefault_ReturnsEmpty()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Remove("a", 1);
        _map.Remove("a", 2);

        Assert.That(_map.GetOrDefault("a"), Is.Empty);
    }

    [Test]
    public void Flatten_MatchesEnumerator()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);

        var fromFlatten = _map.Flatten().ToList();
        var fromEnumerator = _map.ToList();

        Assert.That(fromFlatten, Is.EquivalentTo(fromEnumerator));
    }

    // ── Equals / GetHashCode ──────────────────────────────

    [Test]
    public void Equals_SameInstance_ReturnsTrue()
    {
        Assert.That(_map.Equals(_map), Is.True);
    }

    [Test]
    public void Equals_DifferentInstanceSameContent_ReturnsTrue()
    {
        var other = new SimpleMultiMap<string, int>();
        _map.Add("a", 1);
        other.Add("a", 1);

        Assert.That(_map.Equals(other), Is.True);
    }

    [Test]
    public void Equals_DifferentContent_ReturnsFalse()
    {
        var other = new SimpleMultiMap<string, int>();
        _map.Add("a", 1);
        other.Add("a", 2);

        Assert.That(_map.Equals(other), Is.False);
    }

    [Test]
    public void Equals_DifferentKeys_ReturnsFalse()
    {
        var other = new SimpleMultiMap<string, int>();
        _map.Add("a", 1);
        other.Add("b", 1);

        Assert.That(_map.Equals(other), Is.False);
    }

    [Test]
    public void Equals_DifferentValueCount_SameKeys_ReturnsFalse()
    {
        var other = new SimpleMultiMap<string, int>();
        _map.Add("a", 1);
        _map.Add("a", 2);
        other.Add("a", 1);

        Assert.That(_map.Equals(other), Is.False);
    }

    [Test]
    public void Equals_EmptyMaps_ReturnsTrue()
    {
        var other = new SimpleMultiMap<string, int>();

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
        var other = new SimpleMultiMap<string, int>();
        _map.Add("a", 1);
        other.Add("a", 1);

        Assert.That(_map.GetHashCode(), Is.EqualTo(other.GetHashCode()));
    }

    [Test]
    public void GetHashCode_EmptyMaps_ReturnsSameValue()
    {
        var other = new SimpleMultiMap<string, int>();

        Assert.That(_map.GetHashCode(), Is.EqualTo(other.GetHashCode()));
    }

    [Test]
    public void Constructor_WithCapacity_WorksCorrectly()
    {
        var map = new SimpleMultiMap<string, int>(100);
        map.Add("a", 1);

        Assert.That(map.Get("a"), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void Constructor_WithValueComparer_UsesCaseInsensitiveComparison()
    {
        var map = new SimpleMultiMap<string, string>(StringComparer.OrdinalIgnoreCase);
        map.Add("key", "Hello");
        map.Add("key", "hello");

        Assert.That(map.Flatten().Count(), Is.EqualTo(1));
    }

    [Test]
    public void Constructor_WithCapacityAndValueComparer_WorksCorrectly()
    {
        var map = new SimpleMultiMap<string, string>(100, StringComparer.OrdinalIgnoreCase);
        map.Add("key", "Hello");
        map.Add("key", "hello");

        Assert.That(map.Flatten().Count(), Is.EqualTo(1));
    }

    [Test]
    public void Add_WithCaseInsensitiveComparer_TreatsSameCaseAsDuplicate()
    {
        var map = new SimpleMultiMap<string, string>(StringComparer.OrdinalIgnoreCase);
        map.Add("key", "ABC");
        map.Add("key", "abc");
        map.Add("key", "Abc");

        Assert.That(map.Flatten().Count(), Is.EqualTo(1));
    }
}
