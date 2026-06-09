using System.Collections;

namespace MultiMap.Entities
{
    public abstract partial class MultiMapBase<TKey, TValue, TCollection>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull, IEquatable<TValue>
        where TCollection : ICollection<TValue>
    {
        /// <summary>
        /// A lightweight view over all values in a multi-map's inner dictionary.
        /// Enumerating with <c>foreach</c> on the concrete type uses <see cref="MultiMapBase{TKey, TValue, TCollection}.ValuesEnumerator"/> directly, avoiding boxing of the struct enumerator itself.
        /// Note: the enumerator struct still holds an <see cref="System.Collections.Generic.IEnumerator{T}"/> field that is a heap object; only the struct wrapper is stack-allocated.
        /// </summary>
        private sealed class ValuesCollection : IEnumerable<TValue>
        {
            private readonly ICollection<TCollection> _collections;

            internal ValuesCollection(ICollection<TCollection> collections) => _collections = collections;

            /// <summary>Returns a struct enumerator; <c>foreach</c> on the concrete type calls this overload directly, avoiding boxing of the enumerator struct itself.</summary>
            public ValuesEnumerator GetEnumerator() => new(_collections);

            /// <inheritdoc/>
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => GetEnumerator();

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
