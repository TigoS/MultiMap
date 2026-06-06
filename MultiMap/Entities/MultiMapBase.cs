using System.Collections;
using MultiMap.Interfaces;
using MultiMap.Helpers;

namespace MultiMap.Entities
{
    /// <summary>
    /// Provides a shared implementation of dictionary-backed multi-map operations for concrete multi-map types such as <see cref="MultiMapSet{TKey, TValue}"/>, <see cref="MultiMapList{TKey, TValue}"/>, and <see cref="SortedMultiMap{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the multi-map. Must be non-null and implement <see cref="IEquatable{TKey}"/>.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key. Must be non-null and implement <see cref="IEquatable{TValue}"/>.</typeparam>
    /// <typeparam name="TCollection">The collection type used to store values under each key.</typeparam>
    /// <remarks>
    /// Initializes a new instance of the MultiMapBase class using the specified dictionary as the underlying storage.
    /// </remarks>
    /// <remarks>The provided dictionary is used directly and is not copied. Changes to the dictionary after construction will affect the multimap, and vice versa. Callers are responsible for ensuring the dictionary is in a valid state and not modified concurrently.</remarks>
    /// <param name="dictionary">The dictionary to use as the underlying storage for the multimap. Must not be null.</param>
    public abstract partial class MultiMapBase<TKey, TValue, TCollection>(IDictionary<TKey, TCollection> dictionary) : IMultiMap<TKey, TValue>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull, IEquatable<TValue>
        where TCollection : ICollection<TValue>
    {
        /// <summary>
        /// The underlying dictionary that maps keys to collections of values. Each key is associated with a collection of values, allowing for multiple values per key.
        /// </summary>
        protected readonly IDictionary<TKey, TCollection> _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));

        /// <summary>
        /// Represents the current count or number of items maintained by the containing type.
        /// </summary>
        protected int _count;

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
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(value, nameof(value));

            if (!_dictionary.TryGetValue(key, out var collection))
            {
                collection = CreateCollection();
                _dictionary[key] = collection;
            }

            if (AddToCollection(collection, value))
            {
                _count++;
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public virtual int AddRange(TKey key, IEnumerable<TValue> values)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(values, nameof(values));

            // Materialise the sequence upfront: we need Count to short-circuit on an empty
            // input without allocating a new inner collection, and we want to enumerate only
            // once even if the caller passes a non-replayable IEnumerable<T>.
            var materialised = values as ICollection<TValue> ?? values.ToArray();
            if (materialised.Count == 0)
                return 0;

            bool exists = _dictionary.TryGetValue(key, out var collection);
            if (!exists)
                collection = CreateCollection();

            int added = 0;
            foreach (var value in materialised)
            {
                if (AddToCollection(collection!, value))
                {
                    _count++;
                    added++;
                }
            }

            if (!exists && added > 0)
                _dictionary[key] = collection!;

            return added;
        }

        /// <inheritdoc/>
        public virtual int AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            Guard.NotNull(items, nameof(items));

            int added = 0;
            foreach (var item in items)
            {
                if (Add(item.Key, item.Value))
                    added++;
            }

            return added;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TValue> Get(TKey key)
        {
            Guard.NotNull(key, nameof(key));

            if (TryGetCollection(key, out var collection))
                return ToReadOnly(collection);

            throw new KeyNotFoundException($"The key '{key}' was not found in the multimap.");
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TValue> GetOrDefault(TKey key)
        {
            Guard.NotNull(key, nameof(key));

            if (TryGetCollection(key, out var collection))
                return ToReadOnly(collection);

            return [];
        }

        /// <inheritdoc/>
        public virtual bool TryGet(TKey key, out IEnumerable<TValue> values)
        {
            Guard.NotNull(key, nameof(key));

            bool result = TryGetCollection(key, out var collection);

            values = result ? ToReadOnly(collection!) : [];

            return result;
        }

        /// <inheritdoc/>
        public virtual bool Remove(TKey key, TValue value)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(value, nameof(value));

            if (_dictionary.TryGetValue(key, out var collection))
            {
                bool removed = collection.Remove(value);

                if (removed)
                {
                    _count--;
                    if (collection.Count == 0)
                        _dictionary.Remove(key);
                }

                return removed;
            }

            return false;
        }

        /// <inheritdoc/>
        public int RemoveRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            Guard.NotNull(items, nameof(items));

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
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(predicate, nameof(predicate));

            if (!_dictionary.TryGetValue(key, out var collection))
                return 0;

            int removedCount = RemoveWhereFromCollection(collection, predicate);
            _count -= removedCount;

            if (collection.Count == 0)
                _dictionary.Remove(key);

            return removedCount;
        }

        /// <inheritdoc/>
        public virtual bool RemoveKey(TKey key)
        {
            Guard.NotNull(key, nameof(key));

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            if (_dictionary.Remove(key, out var collection))
#else
            if (_dictionary.TryGetValue(key, out var collection) && _dictionary.Remove(key))
#endif
            {
                _count -= collection.Count;
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public virtual bool ContainsKey(TKey key)
        {
            Guard.NotNull(key, nameof(key));

            return TryGetCollection(key, out _);
        }

        /// <inheritdoc/>
        public virtual bool Contains(TKey key, TValue value)
        {
            Guard.NotNull(key, nameof(key));
            Guard.NotNull(value, nameof(value));

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
            Guard.NotNull(key, nameof(key));

            return _dictionary.TryGetValue(key, out var collection) ? collection.Count : 0;
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> this[TKey key] => Get(key);

        /// <inheritdoc/>
        public virtual void Clear()
        {
            _dictionary.Clear();
            _count = 0;
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
