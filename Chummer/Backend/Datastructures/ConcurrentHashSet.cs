/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Chummer
{
    public class ConcurrentHashSet<T> : ISet<T>, IReadOnlyCollection<T>, IProducerConsumerCollection<T>
    {
        protected ConcurrentDictionary<T, bool> DicInternal { get; }

        public ConcurrentHashSet()
        {
            DicInternal = new ConcurrentDictionary<T, bool>();
        }

        public ConcurrentHashSet(IEnumerable<T> collection)
        {
            DicInternal = new ConcurrentDictionary<T, bool>(collection.Select(x => new KeyValuePair<T, bool>(x, false)));
        }

        public ConcurrentHashSet(IEqualityComparer<T> comparer)
        {
            DicInternal = new ConcurrentDictionary<T, bool>(comparer);
        }

        public ConcurrentHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            DicInternal = new ConcurrentDictionary<T, bool>(collection.Select(x => new KeyValuePair<T, bool>(x, false)), comparer);
        }

        public ConcurrentHashSet(int concurrencyLevel, IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            DicInternal = new ConcurrentDictionary<T, bool>(concurrencyLevel, collection.Select(x => new KeyValuePair<T, bool>(x, false)), comparer);
        }

        public ConcurrentHashSet(
            int concurrencyLevel,
            int capacity,
            IEqualityComparer<T> comparer)
        {
            DicInternal = new ConcurrentDictionary<T, bool>(concurrencyLevel, capacity, comparer);
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return new ConcurrentHashSetEnumerator(DicInternal.GetEnumerator());
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private sealed class ConcurrentHashSetEnumerator : IEnumerator<T>
        {
            private readonly IEnumerator<KeyValuePair<T, bool>> _objInternalEnumerator;

            public ConcurrentHashSetEnumerator(IEnumerator<KeyValuePair<T, bool>> objInternalEnumerator)
            {
                _objInternalEnumerator = objInternalEnumerator;
            }

            public void Dispose()
            {
                _objInternalEnumerator.Dispose();
            }

            public bool MoveNext()
            {
                return _objInternalEnumerator.MoveNext();
            }

            public void Reset()
            {
                _objInternalEnumerator.Reset();
            }

            public T Current => _objInternalEnumerator.Current.Key;

            object IEnumerator.Current => Current;
        }

        /// <inheritdoc />
        public bool TryAdd(T item)
        {
            return item != null && DicInternal.TryAdd(item, false);
        }

        /// <inheritdoc />
        public bool TryTake(out T item)
        {
            while (!DicInternal.IsEmpty)
            {
                // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                item = DicInternal.First().Key;
                if (DicInternal.TryRemove(item, out bool _))
                    return true;
            }
            item = default;
            return false;
        }

        /// <inheritdoc />
        public T[] ToArray()
        {
            // Don't use Keys collection directly because iterating over it does not lock the main dictionary against changes
            return DicInternal.GetKeysToListSafe().ToArray();
        }

        /// <inheritdoc />
        void ICollection<T>.Add(T item)
        {
            if (item != null)
                DicInternal.TryAdd(item, false);
        }

        /// <inheritdoc />
        public void UnionWith(IEnumerable<T> other)
        {
            foreach (T item in other)
            {
                DicInternal.TryAdd(item, false);
            }
        }

        /// <inheritdoc />
        public virtual void IntersectWith(IEnumerable<T> other)
        {
            HashSet<T> setOther = new HashSet<T>(other);
            bool blnRemovalHappened;
            List<T> lstKeysToDelete = new List<T>(Count);
            do
            {
                blnRemovalHappened = false;
                lstKeysToDelete.Clear();
                foreach (KeyValuePair<T, bool> kvpItem in DicInternal) // Set up this way because working with Keys directly does not lock the dictionary
                {
                    if (!setOther.Contains(kvpItem.Key))
                        lstKeysToDelete.Add(kvpItem.Key);
                }
                foreach (T item in lstKeysToDelete)
                {
                    if (!setOther.Contains(item)) // Double check
                        blnRemovalHappened = DicInternal.TryRemove(item, out _) || blnRemovalHappened;
                }
            }
            while (blnRemovalHappened);
        }

        /// <inheritdoc />
        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (T item in other)
            {
                DicInternal.TryRemove(item, out bool _);
            }
        }

        /// <inheritdoc />
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            foreach (T item in other)
            {
                while (!DicInternal.TryAdd(item, false))
                {
                    if (DicInternal.TryRemove(item, out bool _))
                        break;
                }
            }
        }

        /// <inheritdoc />
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return other.Count(item => DicInternal.ContainsKey(item)) == DicInternal.Count;
        }

        /// <inheritdoc />
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return other.All(item => DicInternal.ContainsKey(item));
        }

        /// <inheritdoc />
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            int count = 0;
            foreach (T item in other)
            {
                if (!DicInternal.ContainsKey(item))
                    return false;
                ++count;
            }
            return count < DicInternal.Count;
        }

        /// <inheritdoc />
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            int count = 0;
            bool equals = true;
            foreach (T item in other)
            {
                if (DicInternal.ContainsKey(item))
                    ++count;
                else
                    equals = false;
            }
            return count == DicInternal.Count && !equals;
        }

        /// <inheritdoc />
        public bool Overlaps(IEnumerable<T> other)
        {
            return other.Any(item => DicInternal.ContainsKey(item));
        }

        /// <inheritdoc />
        public bool SetEquals(IEnumerable<T> other)
        {
            int count = 0;
            foreach (T item in other)
            {
                if (!DicInternal.ContainsKey(item))
                    return false;
                ++count;
            }
            return count == DicInternal.Count;
        }

        /// <inheritdoc />
        bool ISet<T>.Add(T item)
        {
            return DicInternal.TryAdd(item, false);
        }

        /// <inheritdoc />
        public void Clear()
        {
            DicInternal.Clear();
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            return item != null && DicInternal.ContainsKey(item);
        }

        /// <inheritdoc cref="ISet{T}.CopyTo" />
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (arrayIndex + DicInternal.Count > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            foreach (KeyValuePair<T, bool> kvpLoop in DicInternal)
            {
                array[arrayIndex] = kvpLoop.Key;
                ++arrayIndex;
            }
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            return item != null && DicInternal.TryRemove(item, out bool _);
        }

        /// <inheritdoc />
        public void CopyTo(Array array, int index)
        {
            if (index + DicInternal.Count > array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            foreach (KeyValuePair<T, bool> kvpLoop in DicInternal)
            {
                array.SetValue(kvpLoop.Key, index);
                ++index;
            }
        }

        /// <inheritdoc cref="ISet{T}.Count" />
        public int Count => DicInternal.Count;

        /// <summary>Gets a value that indicates whether the ConcurrentHashSet is empty.</summary>
        /// <returns>
        /// <see langword="true" /> if the ConcurrentHashSet is empty; otherwise, <see langword="false" />.</returns>
        public bool IsEmpty => DicInternal.IsEmpty;

        /// <inheritdoc />
        public object SyncRoot => throw new NotSupportedException();

        /// <inheritdoc />
        public bool IsSynchronized => false;

        /// <inheritdoc />
        public bool IsReadOnly => false;
    }
}
