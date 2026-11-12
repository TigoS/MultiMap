using System.Collections;
using MultiMap.Helpers;
using MultiMap.Interfaces;

namespace MultiMap.Entities
{
    public sealed partial class MultiMapLock<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            ThrowIfDisposed();
            List<KeyValuePair<TKey, TValue>> snapshot;

            _lock.EnterReadLock();
            try
            {
                snapshot = new List<KeyValuePair<TKey, TValue>>(_count);
                foreach (var kvp in _dictionary)
                {
                    foreach (var value in kvp.Value)
                    {
                        snapshot.Add(new KeyValuePair<TKey, TValue>(kvp.Key, value));
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return snapshot.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ArgumentNullException.ThrowIfNull(array, nameof(array));

            ThrowIfDisposed();

            _lock.EnterReadLock();
            try
            {
                if (arrayIndex < 0 || arrayIndex > array.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(arrayIndex));
                }

                if (array.Length - arrayIndex < _count)
                {
                    throw new ArgumentException("The destination array has insufficient space for all elements.", nameof(array));
                }

                int index = arrayIndex;
                foreach (var kvp in _dictionary)
                {
                    foreach (var value in kvp.Value)
                    {
                        array[index++] = new KeyValuePair<TKey, TValue>(kvp.Key, value);
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as MultiMapLock<TKey, TValue>);

        /// <inheritdoc/>
        public bool Equals(IReadOnlySimpleMultiMap<TKey, TValue>? other) => Equals(other as IReadOnlyMultiMap<TKey, TValue>);

        /// <inheritdoc/>
        public bool Equals(IReadOnlyMultiMap<TKey, TValue>? other)
        {
            if (other is null)
            {
                return false;
            }

            ThrowIfDisposed();

            _lock.EnterReadLock();
            try
            {
                return MultiMapEqualityHelper.EqualsByKey(this, other, _dictionary.Count, _count, _dictionary.Keys, (key, _) =>
                {
                    var thisValues = _dictionary[key];

                    if (thisValues.Count != other.GetValuesCount(key))
                    {
                        return false;
                    }

                    foreach (var value in thisValues)
                    {
                        if (!other.Contains(key, value))
                        {
                            return false;
                        }
                    }

                    return true;
                });
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            ThrowIfDisposed();
            _lock.EnterReadLock();

            try
            {
                return MultiMapHelper.ComputeUnorderedHash<TKey, TValue, HashSet<TValue>>(_dictionary, _dictionary.Comparer, _valueComparer);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Releases the resources used by the <see cref="MultiMapLock{TKey, TValue}"/> instance.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            try
            {
                _dictionary.Clear();
                _count = 0;
            }
            finally
            {
                _lock.Dispose();
            }
        }
    }
}
