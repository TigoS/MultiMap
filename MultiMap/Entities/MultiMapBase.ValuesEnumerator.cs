using System.Collections;

namespace MultiMap.Entities
{
    public abstract partial class MultiMapBase<TKey, TValue, TCollection>
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull, IEquatable<TValue>
        where TCollection : ICollection<TValue>
    {
        /// <summary>
        /// A struct enumerator that walks every value in every inner collection without allocating a heap iterator object.
        /// </summary>
        internal struct ValuesEnumerator : IEnumerator<TValue>
        {
            private IEnumerator<TCollection> _outerEnumerator;
            private IEnumerator<TValue>? _innerEnumerator;
            private TValue _current;

            internal ValuesEnumerator(ICollection<TCollection> collections)
            {
                _outerEnumerator = collections.GetEnumerator();
                _innerEnumerator = null;
                _current = default!;
            }

            /// <inheritdoc/>
            public readonly TValue Current => _current;

            object IEnumerator.Current => _current;

            /// <inheritdoc/>
            public bool MoveNext()
            {
                while (true)
                {
                    if (_innerEnumerator != null && _innerEnumerator.MoveNext())
                    {
                        _current = _innerEnumerator.Current;
                        return true;
                    }

                    _innerEnumerator?.Dispose();
                    _innerEnumerator = null;

                    if (!_outerEnumerator.MoveNext())
                        return false;

                    _innerEnumerator = _outerEnumerator.Current.GetEnumerator();
                }
            }

            /// <inheritdoc/>
            public void Reset()
            {
                _outerEnumerator.Reset();
                _innerEnumerator?.Dispose();
                _innerEnumerator = null;
                _current = default!;
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                _innerEnumerator?.Dispose();
                _outerEnumerator.Dispose();
            }
        }
    }
}
