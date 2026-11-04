using System.Collections;
using System.Diagnostics;
using MultiMap.Helpers;
using MultiMap.Interfaces;

namespace MultiMap.Entities
{
    /// <summary>
    /// Provides a shared implementation of dictionary-backed multi-map operations for concrete multi-map types such as <see cref="MultiMapSet{TKey, TValue}"/>, <see cref="MultiMapList{TKey, TValue}"/>, and <see cref="SortedMultiMap{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the multi-map.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key.</typeparam>
    /// <typeparam name="TCollection">The collection type used to store values under each key.</typeparam>
    /// <remarks>
    /// Initializes a new instance of the MultiMapBase class using the specified dictionary as the underlying storage.
    /// </remarks>
    /// <remarks>The provided dictionary is used directly and is not copied. Changes to the dictionary after construction will affect the multimap, and vice versa. Callers are responsible for ensuring the dictionary is in a valid state and not modified concurrently.</remarks>
    /// <param name="dictionary">The dictionary to use as the underlying storage for the multimap. Must not be null.</param>
    [DebuggerDisplay("Keys={KeyCount}, Values={Count}")]
    public abstract partial class MultiMapBase<TKey, TValue, TCollection>(IDictionary<TKey, TCollection> dictionary) : IMultiMap<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
        where TCollection : ICollection<TValue>
    {
        /// <summary>
        /// The underlying dictionary that maps keys to collections of values. Each key is associated with a collection of values, allowing for multiple values per key.
        /// </summary>
        protected readonly IDictionary<TKey, TCollection> _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));

        /// <summary>Increments the value count by one.</summary>
        protected void IncrementCount() => _count++;

        /// <summary>Decrements the value count by <paramref name="by"/>.</summary>
        protected void DecrementCount(int by = 1) => _count -= by;

        /// <summary>Resets the value count to zero.</summary>
        protected void ResetCount() => _count = 0;

        /// <summary>
        /// The raw value counter. Accessible to <see cref="ConcurrentMultiMapBase{TKey,TValue,TCollection}"/>
        /// for atomic operations via <see cref="System.Threading.Interlocked"/> and
        /// <see cref="System.Threading.Volatile"/>; non-concurrent subclasses use
        /// <see cref="IncrementCount"/>, <see cref="DecrementCount"/>, and <see cref="ResetCount"/> instead.
        /// </summary>
        private protected int _count;

        /// <summary>
        /// Creates a new empty value collection for a key.
        /// </summary>
        protected abstract TCollection CreateCollection();

        /// <summary>
        /// Adds a single value to <paramref name="collection"/>, returning <see langword="true"/> when the value was actually inserted.
        /// </summary>
        protected abstract bool AddToCollection(TCollection collection, TValue value);

        /// <summary>
        /// Removes every value in <paramref name="collection"/> that satisfies
        /// <paramref name="predicate"/> and returns the number of values removed.
        /// </summary>
        protected abstract int RemoveWhereFromCollection(TCollection collection, Predicate<TValue> predicate);

        /// <summary>
        /// Returns a read-only view of the values in <paramref name="collection"/>.
        /// The default implementation creates a snapshot via <see cref="Enumerable.ToArray{TSource}"/>; subclasses backed by <see cref="List{T}"/> can override to return <see cref="List{T}.AsReadOnly"/> for a zero-copy wrapper.
        /// </summary>
        protected virtual IEnumerable<TValue> ToReadOnly(TCollection collection) => collection.ToArray();

        /// <summary>
        /// Attempts to retrieve the collection associated with a key from the underlying dictionary.
        /// The default implementation delegates to <see cref="IDictionary{TKey, TValue}.TryGetValue"/>. 
        /// Subclasses can override to add additional filtering (e.g., skipping empty collections in concurrent scenarios).
        /// </summary>
        protected virtual bool TryGetCollection(TKey key, out TCollection collection) => _dictionary.TryGetValue(key, out collection!);

        /// <inheritdoc/>
        public virtual bool Add(TKey key, TValue value)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            if (!_dictionary.TryGetValue(key, out var collection))
            {
                collection = CreateCollection();
                _dictionary[key] = collection;
            }

            if (AddToCollection(collection, value))
            {
                IncrementCount();
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        /// <remarks>Materialises the sequence upfront: Count is short-circuited on an empty
        /// input without allocating a new inner collection, and need to enumerate only
        /// once even if the caller passes a non-replayable IEnumerable{T}.</remarks>
        public virtual int AddRange(TKey key, IEnumerable<TValue> values)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));
            ArgumentNullException.ThrowIfNull(values, nameof(values));

            var materialised = values as ICollection<TValue> ?? [.. values];
            if (materialised.Count == 0)
            {
                return 0;
            }

            bool exists = _dictionary.TryGetValue(key, out var collection);
            if (!exists)
            {
                collection = CreateCollection();
            }

            int added = 0;
            foreach (var value in materialised)
            {
                if (AddToCollection(collection!, value))
                {
                    IncrementCount();
                    added++;
                }
            }

            if (!exists && added > 0)
            {
                _dictionary[key] = collection!;
            }

            return added;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// <para>The order in which the elements are added is preserved.</para>
        /// <para>Processes items grouped by key so each unique key pays at most one TryGetValue
        /// and one CreateCollection call, regardless of how many values it carries.
        /// It does not materialise a separate grouping structure: instead it maintains a
        /// small "current key" cursor and flushes into the dictionary when the key changes.
        /// This is O(N) in time and O(1) extra space for sequences already sorted by key,
        /// and still correct (though not minimal in lookups) for interleaved keys.</para>
        /// </remarks>
        public virtual int AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            ArgumentNullException.ThrowIfNull(items, nameof(items));

            int added = 0;

            TKey? currentKey = default;
            TCollection? currentCollection = default;
            bool currentExists = false;
            bool hasCurrent = false;

            foreach (var item in items)
            {
                Guard.NotNullMessage(item.Key, nameof(item.Key), "Sequence contains a null key.");
                Guard.NotNullMessage(item.Value, nameof(item.Value), "Sequence contains a null value.");

                // Switch to the collection for this key only when it differs from the last one seen.
                if (!hasCurrent || !EqualityComparer<TKey>.Default.Equals(item.Key, currentKey!))
                {
                    // Flush: if the previous collection was newly created, register it only if non-empty.
                    if (hasCurrent && !currentExists && currentCollection!.Count > 0)
                    {
                        _dictionary[currentKey!] = currentCollection;
                    }

                    currentKey = item.Key;
                    currentExists = _dictionary.TryGetValue(currentKey, out currentCollection!);
                    if (!currentExists)
                    {
                        currentCollection = CreateCollection();
                    }

                    hasCurrent = true;
                }

                if (AddToCollection(currentCollection!, item.Value))
                {
                    IncrementCount();
                    added++;
                }
            }

            // Final flush for the last key group.
            if (hasCurrent && !currentExists && currentCollection!.Count > 0)
            {
                _dictionary[currentKey!] = currentCollection;
            }

            return added;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TValue> Get(TKey key)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));

            return TryGetCollection(key, out var collection)
                ? ToReadOnly(collection)
                : throw new KeyNotFoundException($"The key '{key}' was not found in the multimap.");
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TValue> GetOrDefault(TKey key)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));

            return TryGetCollection(key, out var collection) ? ToReadOnly(collection) : [];
        }

        /// <inheritdoc/>
        public virtual bool TryGet(TKey key, out IEnumerable<TValue> values)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));

            bool result = TryGetCollection(key, out var collection);

            values = result ? ToReadOnly(collection!) : [];

            return result;
        }

        /// <inheritdoc/>
        public virtual bool Remove(TKey key, TValue value)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            if (_dictionary.TryGetValue(key, out var collection))
            {
                bool removed = collection.Remove(value);

                if (removed)
                {
                    DecrementCount();
                    if (collection.Count == 0)
                    {
                        _dictionary.Remove(key);
                    }
                }

                return removed;
            }

            return false;
        }

        /// <inheritdoc/>
        public int RemoveRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            ArgumentNullException.ThrowIfNull(items, nameof(items));

            int removedCount = 0;
            foreach (var item in items)
            {
                if (Remove(item.Key, item.Value))
                {
                    removedCount++;
                }
            }

            return removedCount;
        }

        /// <inheritdoc/>
        public virtual int RemoveWhere(TKey key, Predicate<TValue> predicate)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));
            ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

            if (!_dictionary.TryGetValue(key, out var collection))
            {
                return 0;
            }

            int removedCount = RemoveWhereFromCollection(collection, predicate);
            DecrementCount(removedCount);

            if (collection.Count == 0)
            {
                _dictionary.Remove(key);
            }

            return removedCount;
        }

        /// <inheritdoc/>
        public virtual bool RemoveKey(TKey key)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));

            if (_dictionary.Remove(key, out var collection))
            {
                DecrementCount(collection.Count);
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public virtual bool ContainsKey(TKey key)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));

            return TryGetCollection(key, out _);
        }

        /// <inheritdoc/>
        public virtual bool Contains(TKey key, TValue value)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            return _dictionary.TryGetValue(key, out var collection) && collection.Contains(value);
        }

        /// <inheritdoc/>
        public virtual int Count => _count;

        /// <inheritdoc/>
        public virtual IEnumerable<TKey> Keys => _dictionary.Keys;

        /// <inheritdoc/>
        public virtual int KeyCount => _dictionary.Count;

        /// <inheritdoc/>
        public virtual IEnumerable<TValue> Values => new ValuesCollection(_dictionary.Values);

        /// <inheritdoc/>
        public virtual int GetValuesCount(TKey key)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));

            return _dictionary.TryGetValue(key, out var collection) ? collection.Count : 0;
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> this[TKey key] => Get(key);

        /// <inheritdoc/>
        public virtual void Clear()
        {
            _dictionary.Clear();
            ResetCount();
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var kvp in _dictionary)
            {
                foreach (var value in kvp.Value)
                {
                    yield return new KeyValuePair<TKey, TValue>(kvp.Key, value);
                }
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public bool Equals(IReadOnlySimpleMultiMap<TKey, TValue>? other) => Equals(other as IReadOnlyMultiMap<TKey, TValue>);

        /// <inheritdoc/>
        public abstract bool Equals(IReadOnlyMultiMap<TKey, TValue>? other);
    }
}
