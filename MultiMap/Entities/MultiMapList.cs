#if NET6_0_OR_GREATER
using System.Runtime.InteropServices;
#endif
using MultiMap.Helpers;
using MultiMap.Interfaces;

namespace MultiMap.Entities
{
    /// <summary>
    /// Represents a collection that associates multiple values with each key, allowing efficient storage and retrieval of key-value pairs where keys may map to zero or more values.
    /// </summary>
    /// <remarks>
    /// This class is useful for scenarios where a key can have multiple associated values, such as grouping or indexing.
    /// The collection maintains insertion order for values under each key.
    /// Keys and values must be non-null. Thread safety is not guaranteed;
    /// external synchronization is required for concurrent access.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the multi-map. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
    public sealed class MultiMapList<TKey, TValue> : MultiMapBase<TKey, TValue, List<TValue>>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull, IEquatable<TValue>
    {
        /// <summary>
        /// Initializes a new instance of the MultiMapList class with an empty mapping.
        /// </summary>
        /// <remarks>Use this constructor to create a MultiMapList that starts with no key-value associations.
        /// The internal dictionary is initialized and ready for adding keys and values.</remarks>
        public MultiMapList()
            : base(new Dictionary<TKey, List<TValue>>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the MultiMapList class with the specified initial capacity for keys.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        public MultiMapList(int capacity)
            : base(new Dictionary<TKey, List<TValue>>(capacity))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapList{TKey, TValue}"/> class with the specified equality comparer for keys.
        /// </summary>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapList(IEqualityComparer<TKey>? keyComparer)
            : base(new Dictionary<TKey, List<TValue>>(keyComparer))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiMapList{TKey, TValue}"/> class with the specified initial capacity for keys.
        /// </summary>
        /// <param name="capacity">The initial number of keys that the multimap can contain without resizing.</param>
        /// <param name="keyComparer">The equality comparer to use for comparing keys, or <see langword="null"/> to use the default comparer.</param>
        public MultiMapList(int capacity, IEqualityComparer<TKey>? keyComparer)
            : base(new Dictionary<TKey, List<TValue>>(capacity, keyComparer))
        {
        }

        /// <inheritdoc/>
        protected override List<TValue> CreateCollection() => new();

        /// <inheritdoc/>
        protected override bool AddToCollection(List<TValue> collection, TValue value)
        {
            collection.Add(value);
            return true;
        }

        /// <inheritdoc/>
        protected override int RemoveWhereFromCollection(List<TValue> collection, Predicate<TValue> predicate) => collection.RemoveAll(predicate);

        /// <inheritdoc/>
        protected override IEnumerable<TValue> ToReadOnly(List<TValue> collection) => collection.AsReadOnly();

#if NET6_0_OR_GREATER
        /// <inheritdoc/>
        public override bool Add(TKey key, TValue value)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);
            ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault((Dictionary<TKey, List<TValue>>)_dictionary, key, out _);
            list ??= new List<TValue>();

            list.Add(value);
            _count++;

            return true;
        }
#endif

        /// <inheritdoc/>
        public override int AddRange(TKey key, IEnumerable<TValue> values)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(values);
#else
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (values is null) throw new ArgumentNullException(nameof(values));
#endif

#if NET6_0_OR_GREATER
            ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault((Dictionary<TKey, List<TValue>>)_dictionary, key, out bool exists);
            list ??= new List<TValue>();
#else
            bool exists = _dictionary.TryGetValue(key, out var list);
            if (!exists)
                list = new List<TValue>();
#endif

            // Prevent null values in the enumerable silently enter the list,
            // violating the TValue : notnull constraint at runtime.
            int added = 0;
            foreach (var value in values)
            {
                if (value is null) throw new ArgumentNullException(nameof(values), "Sequence contains a null value.");

                list!.Add(value);
                _count++;
                added++;
            }

#if NET6_0_OR_GREATER
            if (!exists && added == 0)
                ((Dictionary<TKey, List<TValue>>)_dictionary).Remove(key);
#else
            if (!exists && added > 0)
                _dictionary[key] = list!;
#endif

            return added;
        }

#if NET6_0_OR_GREATER
        /// <inheritdoc/>
        public override int AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            int added = 0;
            var dict = (Dictionary<TKey, List<TValue>>)_dictionary;

            foreach (var item in items)
            {
                if (item.Key is null) throw new ArgumentNullException(nameof(items), "Sequence contains a null key.");
                if (item.Value is null) throw new ArgumentNullException(nameof(items), "Sequence contains a null value.");

                ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, item.Key, out _);
                list ??= new List<TValue>();

                list.Add(item.Value);
                _count++;
                added++;
            }

            return added;
        }
#endif

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as MultiMapList<TKey, TValue>);

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

                if (!this[key].SequenceEqual(other[key]))
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 0;
                foreach (var kvp in _dictionary)
                {
                    var entryHash = new HashCode();
                    entryHash.Add(kvp.Key);
                    foreach (var value in kvp.Value)
                    {
                        entryHash.Add(value);
                    }
                    hash += MultiMapHelper.Scramble(entryHash.ToHashCode());
                }
                return hash;
            }
        }
    }
}
