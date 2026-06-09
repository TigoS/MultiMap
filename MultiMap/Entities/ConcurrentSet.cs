using System.Collections;
using System.Collections.Concurrent;
using MultiMap.Helpers;

namespace MultiMap.Entities
{
    /// <summary>
    /// A thread-safe set wrapper around <see cref="ConcurrentDictionary{TKey, TValue}"/> used as the value collection in <see cref="ConcurrentMultiMap{TKey, TValue}"/>.
    /// Implements <see cref="ICollection{T}"/> using the keys of the underlying dictionary; values are always <c>0</c> (ignored).
    /// Provides thread-safe <see cref="TryAdd"/>, <see cref="TryRemove"/>, and <see cref="IsEmpty"/> operations.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set. Must be non-nullable and implement <see cref="IEquatable{T}"/>.</typeparam>
    public sealed class ConcurrentSet<T> : ICollection<T>
        where T : notnull, IEquatable<T>
    {
        private readonly ConcurrentDictionary<T, byte> _inner;

        /// <summary>
        /// Initializes a new instance of <see cref="ConcurrentSet{T}"/> with an optional value comparer.
        /// </summary>
        internal ConcurrentSet(IEqualityComparer<T>? comparer = null)
        {
            _inner = new ConcurrentDictionary<T, byte>(comparer);
        }

        /// <summary>
        /// Gets the number of elements in the set.
        /// </summary>
        public int Count => _inner.Count;

        /// <summary>
        /// Gets a value indicating whether the collection is read-only (always <see langword="false"/>).
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets a value indicating whether the underlying dictionary is empty.
        /// </summary>
        internal bool IsEmpty => _inner.IsEmpty;

        /// <summary>
        /// Attempts to add an element to the set.
        /// </summary>
        internal bool TryAdd(T item) => _inner.TryAdd(item, 0);

        /// <summary>
        /// Attempts to remove an element from the set.
        /// </summary>
        internal bool TryRemove(T item, out byte _) => _inner.TryRemove(item, out _);

        /// <summary>
        /// Adds an element to the set (always succeeds for set semantics; returns silently if already present).
        /// </summary>
        public void Add(T item) => _inner.TryAdd(item, 0);

        /// <summary>
        /// Removes all elements from the set.
        /// </summary>
        public void Clear() => _inner.Clear();

        /// <summary>
        /// Determines whether the set contains a specific element.
        /// </summary>
        public bool Contains(T item) => _inner.ContainsKey(item);

        /// <summary>
        /// Copies the elements of the set to an array, starting at a particular array index.
        /// Under concurrent modification, the set size can change during enumeration.
        /// This method follows <see cref="ICollection{T}.CopyTo"/> semantics and throws if the destination array is not large enough.
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            Guard.NotNull(array, nameof(array));

            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            int index = arrayIndex;
            foreach (var item in _inner.Keys)
            {
                if (index >= array.Length)
                    throw new ArgumentException("Destination array was not long enough.");
                array[index++] = item;
            }
        }

        /// <summary>
        /// Removes an element from the set.
        /// </summary>
        public bool Remove(T item) => _inner.TryRemove(item, out _);

        /// <summary>
        /// Returns an enumerator that iterates over the keys (elements) of the set.
        /// </summary>
        public IEnumerator<T> GetEnumerator() => _inner.Keys.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates over the keys (elements) of the set.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
