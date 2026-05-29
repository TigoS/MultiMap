using System.Collections;

namespace MultiMap.Entities
{
    public abstract partial class MultiMapBase<TKey, TValue, TCollection>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull, IEquatable<TValue>
        where TCollection : ICollection<TValue>
    {
        /// <summary>
        /// A lightweight, allocation-free view over all values in a multi-map's inner dictionary.
        /// Enumerating with <c>foreach</c> on the concrete type uses <see cref="MultiMapBase{TKey, TValue, TCollection}.ValuesEnumerator{TEnumerator}"/> directly, avoiding boxing of the struct enumerator.
        /// </summary>
        private sealed class ValuesCollection : IEnumerable<TValue>
        {
            private readonly ICollection<TCollection> _collections;

            internal ValuesCollection(ICollection<TCollection> collections) => _collections = collections;

            /// <summary>Returns a struct enumerator; <c>foreach</c> on the concrete type calls this without boxing.</summary>
            public ValuesEnumerator<IEnumerator<TValue>> GetEnumerator() => new(_collections);

            /// <inheritdoc/>
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => GetEnumerator();

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
