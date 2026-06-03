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
    public void GetOrDefault_AfterRemovingKey_ReturnsEmpty()
    {
        _map.Add("a", 1);
        _map.RemoveKey("a");

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

    // ── RemoveKey ──────────────────────────────────────────

    [Test]
    public void RemoveKey_ExistingKey_RemovesAllValues()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("a", 3);

        _map.RemoveKey("a");

        Assert.That(_map.GetOrDefault("a"), Is.Empty);
    }

    [Test]
    public void RemoveKey_ExistingKey_KeyNoLongerExists()
    {
        _map.Add("a", 1);

        _map.RemoveKey("a");

        Assert.Throws<KeyNotFoundException>(() => _map.Get("a"));
    }

    [Test]
    public void RemoveKey_NonExistentKey_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _map.RemoveKey("missing"));
    }

    [Test]
    public void RemoveKey_DoesNotAffectOtherKeys()
    {
        _map.Add("a", 1);
        _map.Add("b", 2);

        _map.RemoveKey("a");

        Assert.That(_map.Get("b"), Is.EquivalentTo(new[] { 2 }));
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
    public void AddThenRemoveKeyThenAdd_WorksCorrectly()
    {
        _map.Add("a", 1);
        _map.RemoveKey("a");
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
        var map = new SimpleMultiMap<string, string>(valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("key", "Hello");
        map.Add("key", "hello");

        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Constructor_WithCapacityAndValueComparer_WorksCorrectly()
    {
        var map = new SimpleMultiMap<string, string>(100, valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("key", "Hello");
        map.Add("key", "hello");

        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Add_WithCaseInsensitiveComparer_TreatsSameCaseAsDuplicate()
    {
        var map = new SimpleMultiMap<string, string>(valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("key", "ABC");
        map.Add("key", "abc");
        map.Add("key", "Abc");

        Assert.That(map.Count, Is.EqualTo(1));
    }

    // ── Count ──────────────────────────────────────────────

    [Test]
    public void Count_EmptyMap_IsZero()
    {
        Assert.That(_map.Count, Is.EqualTo(0));
    }

    [Test]
    public void Count_SingleEntry_IsOne()
    {
        _map.Add("a", 1);

        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Count_MultipleValuesForSameKey_ReflectsTotal()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("a", 3);

        Assert.That(_map.Count, Is.EqualTo(3));
    }

    [Test]
    public void Count_MultipleKeys_SumsAllValues()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("b", 10);

        Assert.That(_map.Count, Is.EqualTo(3));
    }

    [Test]
    public void Count_AfterRemovingOneValue_Decrements()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);

        _map.Remove("a", 1);

        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Count_AfterRemovingKey_Decrements()
    {
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("b", 3);

        _map.RemoveKey("a");

        Assert.That(_map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Count_DuplicateValue_NotCounted()
    {
        _map.Add("a", 1);
        _map.Add("a", 1);

        Assert.That(_map.Count, Is.EqualTo(1));
    }

    // ── Typed Equals(IReadOnlySimpleMultiMap<TKey, TValue>?) ───

    [Test]
    public void Equals_Typed_SameInstance_ReturnsTrue()
    {
        _map.Add("a", 1);

        Assert.That(_map.Equals((IReadOnlySimpleMultiMap<string, int>)_map), Is.True);
    }

    [Test]
    public void Equals_Typed_NullOther_ReturnsFalse()
    {
        Assert.That(_map.Equals((IReadOnlySimpleMultiMap<string, int>?)null), Is.False);
    }

    [Test]
    public void Equals_Typed_EmptyMaps_ReturnsTrue()
    {
        var other = new SimpleMultiMap<string, int>();

        Assert.That(_map.Equals((IReadOnlySimpleMultiMap<string, int>)other), Is.True);
    }

    [Test]
    public void Equals_Typed_SameContent_ReturnsTrue()
    {
        var other = new SimpleMultiMap<string, int>();
        _map.Add("a", 1);
        _map.Add("a", 2);
        _map.Add("b", 3);
        other.Add("a", 1);
        other.Add("a", 2);
        other.Add("b", 3);

        Assert.That(_map.Equals((IReadOnlySimpleMultiMap<string, int>)other), Is.True);
    }

    [Test]
    public void Equals_Typed_DifferentValues_ReturnsFalse()
    {
        var other = new SimpleMultiMap<string, int>();
        _map.Add("a", 1);
        other.Add("a", 2);

        Assert.That(_map.Equals((IReadOnlySimpleMultiMap<string, int>)other), Is.False);
    }

    [Test]
    public void Equals_Typed_DifferentKeyCount_ReturnsFalse()
    {
        var other = new SimpleMultiMap<string, int>();
        _map.Add("a", 1);
        other.Add("a", 1);
        other.Add("b", 2);

        Assert.That(_map.Equals((IReadOnlySimpleMultiMap<string, int>)other), Is.False);
    }

    [Test]
    public void Equals_Typed_DifferentKeys_ReturnsFalse()
    {
        var other = new SimpleMultiMap<string, int>();
        _map.Add("a", 1);
        other.Add("b", 1);

        Assert.That(_map.Equals((IReadOnlySimpleMultiMap<string, int>)other), Is.False);
    }

    [Test]
    public void Equals_Typed_DifferentValueCount_SameKey_ReturnsFalse()
    {
        var other = new SimpleMultiMap<string, int>();
        _map.Add("a", 1);
        _map.Add("a", 2);
        other.Add("a", 1);

        Assert.That(_map.Equals((IReadOnlySimpleMultiMap<string, int>)other), Is.False);
    }

    [Test]
    public void Equals_Typed_ValuesInDifferentInsertionOrder_ReturnsTrue()
    {
        var other = new SimpleMultiMap<string, int>();
        _map.Add("a", 1);
        _map.Add("a", 2);
        other.Add("a", 2);
        other.Add("a", 1);

        Assert.That(_map.Equals((IReadOnlySimpleMultiMap<string, int>)other), Is.True);
    }

    [Test]
    public void GetOrDefault_NullKey_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _map.GetOrDefault(null!));
    }

    // ── Constructor overloads ─────────────────────────────

    [Test]
    public void Constructor_WithKeyComparer_UsesComparer()
    {
        var map = new SimpleMultiMap<string, int>(StringComparer.OrdinalIgnoreCase);
        map.Add("KEY", 1);

        Assert.That(map.Get("key").Single(), Is.EqualTo(1));
    }

    [Test]
    public void Constructor_WithCapacityAndKeyComparer_UsesComparer()
    {
        var map = new SimpleMultiMap<string, int>(10, StringComparer.OrdinalIgnoreCase);
        map.Add("KEY", 1);

        Assert.That(map.Get("key").Single(), Is.EqualTo(1));
    }

    [Test]
    public void Constructor_WithCapacityKeyAndValueComparer_BothApplied()
    {
        var map = new SimpleMultiMap<string, string>(10, StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);
        map.Add("KEY", "ABC");
        map.Add("key", "abc");

        Assert.That(map.Get("KEY").Count(), Is.EqualTo(1));
    }

    [Test]
    public void Constructor_WithKeyAndValueComparer_BothApplied()
    {
        var map = new SimpleMultiMap<string, string>(StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);
        map.Add("KEY", "ABC");
        map.Add("key", "abc");

        Assert.That(map.Get("KEY").Count(), Is.EqualTo(1));
    }

    // ── Null-guard coverage ───────────────────────────────

    [Test]
    public void Add_NullKey_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _map.Add(null!, 1));
    }

    [Test]
    public void Add_NullValue_ThrowsArgumentNullException()
    {
        var map = new SimpleMultiMap<string, string>();
        Assert.Throws<ArgumentNullException>(() => map.Add("key", null!));
    }

    [Test]
    public void Get_NullKey_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _map.Get(null!));
    }

    [Test]
    public void Remove_NullKey_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _map.Remove(null!, 1));
    }

    [Test]
    public void Remove_NullValue_ThrowsArgumentNullException()
    {
        var map = new SimpleMultiMap<string, string>();
        Assert.Throws<ArgumentNullException>(() => map.Remove("key", null!));
    }

    [Test]
    public void RemoveKey_NullKey_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _map.RemoveKey(null!));
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// SimpleMultiMap – remaining constructor overloads
// ──────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class SimpleMultiMap_ConstructorCoverageTests
{
    [Test]
    public void Constructor_Default_IsEmpty()
    {
        var map = new SimpleMultiMap<string, int>();
        Assert.That(map.Count, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_WithKeyComparer_UsesComparer()
    {
        var map = new SimpleMultiMap<string, int>(StringComparer.OrdinalIgnoreCase);
        map.Add("KEY", 1);
        Assert.That(map.ContainsKey("key"), Is.True);
    }

    [Test]
    public void Constructor_WithCapacity_AcceptsEntries()
    {
        var map = new SimpleMultiMap<string, int>(50);
        map.Add("a", 1);
        Assert.That(map.Get("a").Single(), Is.EqualTo(1));
    }

    [Test]
    public void Constructor_WithCapacityAndKeyComparer_BothApplied()
    {
        var map = new SimpleMultiMap<string, int>(10, StringComparer.OrdinalIgnoreCase);
        map.Add("KEY", 7);
        Assert.That(map.Get("key").Single(), Is.EqualTo(7));
    }

    [Test]
    public void Constructor_WithCapacityAndValueComparer_DeduplicatesByValueComparer()
    {
        var map = new SimpleMultiMap<string, string>(10, valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("k", "Hello");
        map.Add("k", "HELLO");
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Constructor_WithCapacityKeyAndValueComparer_BothApplied()
    {
        var map = new SimpleMultiMap<string, string>(10, StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);
        map.Add("KEY", "ABC");
        map.Add("key", "abc");
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Constructor_WithKeyAndValueComparer_BothApplied()
    {
        var map = new SimpleMultiMap<string, string>(StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);
        map.Add("KEY", "ABC");
        map.Add("key", "abc");
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Constructor_WithValueComparer_DeduplicatesByValueComparer()
    {
        var map = new SimpleMultiMap<string, string>(valueComparer: StringComparer.OrdinalIgnoreCase);
        map.Add("k", "Hello");
        map.Add("k", "hello");
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void Contains_ExistingKeyAndValue_ReturnsTrue()
    {
        var map = new SimpleMultiMap<string, int>();
        map.Add("a", 1);
        Assert.That(map.Contains("a", 1), Is.True);
    }

    [Test]
    public void Contains_WrongValue_ReturnsFalse()
    {
        var map = new SimpleMultiMap<string, int>();
        map.Add("a", 1);
        Assert.That(map.Contains("a", 99), Is.False);
    }

    [Test]
    public void Contains_NonExistentKey_ReturnsFalse()
    {
        var map = new SimpleMultiMap<string, int>();
        Assert.That(map.Contains("missing", 1), Is.False);
    }

    [Test]
    public void ContainsKey_ExistingKey_ReturnsTrue()
    {
        var map = new SimpleMultiMap<string, int>();
        map.Add("a", 1);
        Assert.That(map.ContainsKey("a"), Is.True);
    }

    [Test]
    public void ContainsKey_NonExistentKey_ReturnsFalse()
    {
        var map = new SimpleMultiMap<string, int>();
        Assert.That(map.ContainsKey("missing"), Is.False);
    }

    [Test]
    public void Equals_ObjectOverload_WrongType_ReturnsFalse()
    {
        var map = new SimpleMultiMap<string, int>();
        Assert.That(map.Equals(42), Is.False);
    }

    [Test]
    public void GetHashCode_SameContent_EqualHashCodes()
    {
        var a = new SimpleMultiMap<string, int>();
        var b = new SimpleMultiMap<string, int>();
        a.Add("k", 1); b.Add("k", 1);
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// MultiMapLock – additional concurrent stress tests
// ──────────────────────────────────────────────────────────────────────────────

