#if NET6_0_OR_GREATER
using System.Runtime.InteropServices;
#endif

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
    /// <typeparam name="TKey">The type of keys in the multimap. Must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The type of values in the multimap. Must be non-nullable.</typeparam>
    public class MultiMapSet<TKey, TValue> : MultiMapBase<TKey, TValue, HashSet<TValue>>
        where TKey : notnull
        where TValue : notnull
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
        protected override HashSet<TValue> CreateCollection() => new HashSet<TValue>(_valueComparer);

        /// <inheritdoc/>
        protected override bool AddToCollection(HashSet<TValue> collection, TValue value) => collection.Add(value);

        /// <inheritdoc/>
        protected override int RemoveWhereFromCollection(HashSet<TValue> collection, Predicate<TValue> predicate) => collection.RemoveWhere(predicate);

#if NET6_0_OR_GREATER
        /// <inheritdoc/>
        public override bool Add(TKey key, TValue value)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));

            ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault((Dictionary<TKey, HashSet<TValue>>)_dictionary, key, out bool exists);
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
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (values is null) throw new ArgumentNullException(nameof(values));

            ref var hashset = ref CollectionsMarshal.GetValueRefOrAddDefault((Dictionary<TKey, HashSet<TValue>>)_dictionary, key, out bool exists);
            hashset ??= new HashSet<TValue>(_valueComparer);

            int added = 0;
            foreach (var value in values)
            {
                if (hashset.Add(value))
                {
                    _count++;
                    added++;
                }
            }

            return added;
        }
#endif

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is not MultiMapSet<TKey, TValue> other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (Count != other.Count || KeyCount != other.KeyCount)
                return false;

            foreach (var kvp in _dictionary)
            {
                if (!other._dictionary.TryGetValue(kvp.Key, out var otherSet))
                    return false;

                if (!kvp.Value.SetEquals(otherSet))
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hash = 0;
            foreach (var kvp in _dictionary)
            {
                int valueHash = 0;
                foreach (var value in kvp.Value)
                {
                    valueHash ^= value.GetHashCode();
                }
                hash ^= HashCode.Combine(kvp.Key, valueHash);
            }
            return hash;
        }
    }
}
