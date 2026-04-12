using MultiMap.Interfaces;
using System.Collections;

namespace MultiMap.Entities
{
    /// <summary>
    /// Provides a shared implementation of dictionary-backed multi-map operations for concrete multi-map types such as <see cref="MultiMapSet{TKey, TValue}"/>, <see cref="MultiMapList{TKey, TValue}"/>, and <see cref="SortedMultiMap{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the multi-map.</typeparam>
    /// <typeparam name="TValue">The type of values associated with each key.</typeparam>
    /// <typeparam name="TCollection">The collection type used to store values under each key.</typeparam>
    public abstract class MultiMapBase<TKey, TValue, TCollection> : IMultiMap<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
        where TCollection : ICollection<TValue>
    {
        /// <summary>
        /// The underlying dictionary that maps keys to collections of values. Each key is associated with a collection of values, allowing for multiple values per key.
        /// </summary>
        protected readonly IDictionary<TKey, TCollection> _dictionary;

        /// <summary>
        /// Represents the current count or number of items maintained by the containing type.
        /// </summary>
        protected int _count;

        /// <summary>
        /// Initializes a new instance of the MultiMapBase class using the specified dictionary as the underlying storage.
        /// </summary>
        /// <remarks>The provided dictionary is used directly and is not copied. Changes to the dictionary after construction will affect the multimap, and vice versa. Callers are responsible for ensuring the dictionary is in a valid state and not modified concurrently.</remarks>
        /// <param name="dictionary">The dictionary to use as the underlying storage for the multimap. Must not be null.</param>
        protected MultiMapBase(IDictionary<TKey, TCollection> dictionary)
        {
            _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        }

        /// <summary>
        /// Creates a new empty value collection for a key.x
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

        /// <inheritdoc/>
        public virtual bool Add(TKey key, TValue value)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));

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
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (values is null) throw new ArgumentNullException(nameof(values));

            if (!_dictionary.TryGetValue(key, out var collection))
            {
                collection = CreateCollection();
                _dictionary[key] = collection;
            }

            int added = 0;
            foreach (var value in values)
            {
                if (AddToCollection(collection, value))
                {
                    _count++;
                    added++;
                }
            }

            return added;
        }

        /// <inheritdoc/>
        public virtual int AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));

            int added = 0;
            foreach (var item in items)
            {
                if (Add(item.Key, item.Value))
                    added++;
            }

            return added;
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> Get(TKey key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            if (_dictionary.TryGetValue(key, out var collection))
                return ToReadOnly(collection);

            throw new KeyNotFoundException($"The key '{key}' was not found in the multimap.");
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> GetOrDefault(TKey key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            if (_dictionary.TryGetValue(key, out var collection))
                return ToReadOnly(collection);

            return [];
        }

        /// <inheritdoc/>
        public bool TryGet(TKey key, out IEnumerable<TValue> values)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            bool result = _dictionary.TryGetValue(key, out var collection);

            values = result ? ToReadOnly(collection!) : [];

            return result;
        }

        /// <inheritdoc/>
        public bool Remove(TKey key, TValue value)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));

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
            if (items is null) throw new ArgumentNullException(nameof(items));

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
        public int RemoveWhere(TKey key, Predicate<TValue> predicate)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            if (!_dictionary.TryGetValue(key, out var collection))
                return 0;

            int removedCount = RemoveWhereFromCollection(collection, predicate);
            _count -= removedCount;

            if (collection.Count == 0)
                _dictionary.Remove(key);

            return removedCount;
        }

        /// <inheritdoc/>
        public bool RemoveKey(TKey key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            if (_dictionary.TryGetValue(key, out var collection))
            {
                _count -= collection.Count;
                return _dictionary.Remove(key);
            }

            return false;
        }

        /// <inheritdoc/>
        public bool ContainsKey(TKey key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            return _dictionary.ContainsKey(key);
        }

        /// <inheritdoc/>
        public bool Contains(TKey key, TValue value)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));

            return _dictionary.TryGetValue(key, out var collection) && collection.Contains(value);
        }

        /// <inheritdoc/>
        public int Count => Volatile.Read(ref _count);

        /// <inheritdoc/>
        public IEnumerable<TKey> Keys => _dictionary.Keys;

        /// <inheritdoc/>
        public int KeyCount => _dictionary.Count;

        /// <inheritdoc/>
        public IEnumerable<TValue> Values => _dictionary.Values.SelectMany(c => c);

        /// <inheritdoc/>
        public int GetValuesCount(TKey key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            return _dictionary.TryGetValue(key, out var collection) ? collection.Count : 0;
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> this[TKey key] => Get(key);

        /// <inheritdoc/>
        public void Clear()
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
    }
}
