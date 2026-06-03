#if NET6_0_OR_GREATER
using System.Runtime.InteropServices;
#endif
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif
using MultiMap.Helpers;
using MultiMap.Interfaces;

namespace MultiMap.Entities
{
    /// <summary>
    /// Represents a collection that maps keys to sets of values, allowing multiple values to be associated with each key.
    /// Provides set semantics for values, ensuring that each value is unique per key.
    /// </summary>
    /// <remarks>
    /// This implementation uses a dictionary of hash sets to store values for each key, providing efficient lookup and uniqueness enforcement.
    /// Values associated with a key are unordered and duplicates are not allowed.
    /// The class is not thread-safe; external synchronization is required for concurrent access.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multimap. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
    /// <typeparam name="TValue">The type of values in the multimap. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
    public sealed class MultiMapSet<TKey, TValue> : MultiMapBase<TKey, TValue, HashSet<TValue>>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull, IEquatable<TValue>
    {
        private readonly IEqualityComparer<TValue>? _valueComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapSet{TKey, TValue}"/> class.
        /// </summary>
        public MultiMapSet()
            : base(new Dictionary<TKey, HashSet<TValue>>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapSet{TKey, TValue}"/> class with the specified equality comparer for keys.
        /// </summary>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapSet(IEqualityComparer<TKey>? keyComparer)
            : base(new Dictionary<TKey, HashSet<TValue>>(keyComparer))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapSet{TKey, TValue}"/> class with the specified equality comparer for values.
        /// </summary>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapSet(IEqualityComparer<TValue>? valueComparer)
            : base(new Dictionary<TKey, HashSet<TValue>>())
        {
            _valueComparer = valueComparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapSet{TKey, TValue}"/> class with the specified equality comparers for keys and values.
        /// </summary>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapSet(IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)
            : base(new Dictionary<TKey, HashSet<TValue>>(keyComparer))
        {
            _valueComparer = valueComparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapSet{TKey, TValue}"/> class with the specified initial capacity for keys.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        public MultiMapSet(int capacity)
            : base(new Dictionary<TKey, HashSet<TValue>>(capacity))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapSet{TKey, TValue}"/> class with the specified initial capacity for keys.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapSet(int capacity, IEqualityComparer<TKey>? keyComparer)
            : base(new Dictionary<TKey, HashSet<TValue>>(capacity, keyComparer))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapSet{TKey, TValue}"/> class with the specified initial capacity for keys and equality comparer for values.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapSet(int capacity, IEqualityComparer<TValue>? valueComparer)
            : base(new Dictionary<TKey, HashSet<TValue>>(capacity))
        {
            _valueComparer = valueComparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapSet{TKey, TValue}"/> class with the specified initial capacity for keys and equality comparer for keys and values.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        /// <param name="valueComparer">The equality comparer to use for comparing values, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapSet(int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)
            : base(new Dictionary<TKey, HashSet<TValue>>(capacity, keyComparer))
        {
            _valueComparer = valueComparer;
        }

        /// <inheritdoc/>
        protected override HashSet<TValue> CreateCollection() => new(_valueComparer);

        /// <inheritdoc/>
        protected override bool AddToCollection(HashSet<TValue> collection, TValue value) => collection.Add(value);

        /// <inheritdoc/>
        protected override int RemoveWhereFromCollection(HashSet<TValue> collection, Predicate<TValue> predicate) => collection.RemoveWhere(predicate);

#if NET8_0_OR_GREATER
        /// <inheritdoc/>
        protected override IEnumerable<TValue> ToReadOnly(HashSet<TValue> collection)
            => collection.ToFrozenSet(_valueComparer);
#endif

#if NET6_0_OR_GREATER
        /// <inheritdoc/>
        public override bool Add(TKey key, TValue value)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);

            ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault((Dictionary<TKey, HashSet<TValue>>)_dictionary, key, out _);
            hashset ??= new HashSet<TValue>(_valueComparer);

            if (hashset.Add(value))
            {
                _count++;
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public override int AddRange(TKey key, IEnumerable<TValue> values)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(values);

            ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault((Dictionary<TKey, HashSet<TValue>>)_dictionary, key, out bool exists);
            hashset ??= new HashSet<TValue>(_valueComparer);

            int added = 0;
            foreach (var value in values)
            {
                if (value is null) throw new ArgumentNullException(nameof(values), "Sequence contains a null value.");

                if (hashset.Add(value))
                {
                    _count++;
                    added++;
                }
            }

            if (!exists && added == 0)
                ((Dictionary<TKey, HashSet<TValue>>)_dictionary).Remove(key);

            return added;
        }

        /// <inheritdoc/>
        public override int AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            int added = 0;
            var dict = (Dictionary<TKey, HashSet<TValue>>)_dictionary;

            foreach (var item in items)
            {
                if (item.Key is null) throw new ArgumentNullException(nameof(items), "Sequence contains a null key.");
                if (item.Value is null) throw new ArgumentNullException(nameof(items), "Sequence contains a null value.");

                ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, item.Key, out bool exists);
                hashset ??= new HashSet<TValue>(_valueComparer);

                if (hashset.Add(item.Value))
                {
                    _count++;
                    added++;
                }
            }

            return added;
        }
#endif

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as MultiMapSet<TKey, TValue>);

        /// <inheritdoc/>
        public override bool Equals(IReadOnlyMultiMap<TKey, TValue>? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (KeyCount != other.KeyCount || Count != other.Count)
                return false;

            foreach (var key in Keys)
            {
                if (!other.ContainsKey(key) || GetValuesCount(key) != other.GetValuesCount(key))
                    return false;

                // Compare values as sets
                var otherValuesSet = new HashSet<TValue>(other[key], _valueComparer);
                foreach (var value in this[key])
                {
                    if (!otherValuesSet.Contains(value))
                        return false;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
            => MultiMapHelper.ComputeUnorderedHash<TKey, TValue, HashSet<TValue>>(_dictionary,
                ((Dictionary<TKey, HashSet<TValue>>)_dictionary).Comparer, _valueComparer);
    }
}
