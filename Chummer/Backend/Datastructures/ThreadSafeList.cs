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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public class ThreadSafeList<T> : IAsyncList<T>, IAsyncReadOnlyList<T>, IList, IAsyncProducerConsumerCollection<T>, IAsyncEnumerableWithSideEffects<T>, IHasLockObject
    {
        private readonly List<T> _lstData;
        public AsyncFriendlyReaderWriterLock LockObject { get; }

        public ThreadSafeList(AsyncFriendlyReaderWriterLock objParentLock = null, bool blnLockReadOnlyForParent = false)
        {
            LockObject = new AsyncFriendlyReaderWriterLock(objParentLock, blnLockReadOnlyForParent);
            _lstData = new List<T>();
        }

        public ThreadSafeList(int capacity, AsyncFriendlyReaderWriterLock objParentLock = null, bool blnLockReadOnlyForParent = false)
        {
            LockObject = new AsyncFriendlyReaderWriterLock(objParentLock, blnLockReadOnlyForParent);
            _lstData = new List<T>(capacity);
        }

        public ThreadSafeList(IEnumerable<T> collection, AsyncFriendlyReaderWriterLock objParentLock = null, bool blnLockReadOnlyForParent = false)
        {
            LockObject = new AsyncFriendlyReaderWriterLock(objParentLock, blnLockReadOnlyForParent);
            _lstData = new List<T>(collection);
        }

        /// <inheritdoc cref="List{T}.Capacity" />
        public int Capacity
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstData.Capacity;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_lstData.Capacity == value)
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_lstData.Capacity == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _lstData.Capacity = value;
                }
            }
        }

        public void CopyTo(Array array, int index)
        {
            using (LockObject.EnterReadLock())
            {
                foreach (T objItem in _lstData)
                {
                    array.SetValue(objItem, index);
                    ++index;
                }
            }
        }

        public async Task CopyToAsync(Array array, int index, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                foreach (T objItem in _lstData)
                {
                    array.SetValue(objItem, index);
                    ++index;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Count" />
        public int Count
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstData.Count;
            }
        }

        public async Task<int> GetCountAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.Count;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        bool IList.IsFixedSize => false;

        /// <inheritdoc />
        bool ICollection<T>.IsReadOnly => false;

        /// <inheritdoc />
        bool IList.IsReadOnly => false;

        /// <inheritdoc />
        bool ICollection.IsSynchronized => true;

        /// <inheritdoc />
        object ICollection.SyncRoot => LockObject;

        public T this[int index]
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstData[index];
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_lstData[index].Equals(value))
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_lstData[index].Equals(value))
                        return;
                    using (LockObject.EnterWriteLock())
                        _lstData[index] = value;
                }
            }
        }

        public async Task<T> GetValueAtAsync(int index, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData[index];
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetValueAtAsync(int index, T value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_lstData[index].Equals(value))
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_lstData[index].Equals(value))
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _lstData[index] = value;
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        object IList.this[int index]
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstData[index];
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_lstData[index].Equals(value))
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_lstData[index].Equals(value))
                        return;
                    using (LockObject.EnterWriteLock())
                        _lstData[index] = (T)value;
                }
            }
        }

        public bool SequenceEqual(ThreadSafeList<T> other)
        {
            if (other == null)
                return false;
            using (other.LockObject.EnterReadLock())
            using (LockObject.EnterReadLock())
            {
                return _lstData.Count == other._lstData.Count
                       && _lstData.SequenceEqual(other._lstData);
            }
        }

        public async Task<bool> SequenceEqualAsync(ThreadSafeList<T> other, CancellationToken token = default)
        {
            if (other == null)
                return false;
            IAsyncDisposable objLocker = await other.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                IAsyncDisposable objLocker2 = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    return _lstData.Count == other._lstData.Count
                           && _lstData.SequenceEqual(other._lstData);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public void Add(T item)
        {
            using (LockObject.EnterWriteLock())
                _lstData.Add(item);
        }

        public async Task AddAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstData.Add(item);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.AddRange" />
        public void AddRange(IEnumerable<T> collection)
        {
            using (LockObject.EnterWriteLock())
                _lstData.AddRange(collection);
        }

        public async Task AddRangeAsync(IEnumerable<T> collection, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstData.AddRange(collection);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.AsReadOnly" />
        public ReadOnlyCollection<T> AsReadOnly()
        {
            using (LockObject.EnterReadLock())
                return _lstData.AsReadOnly();
        }

        /// <inheritdoc cref="List{T}.AsReadOnly" />
        public async Task<ReadOnlyCollection<T>> AsReadOnlyAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.AsReadOnly();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.BinarySearch(int, int, T, IComparer{T})" />
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            using (LockObject.EnterReadLock())
                return _lstData.BinarySearch(index, count, item, comparer);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T)" />
        public int BinarySearch(T item)
        {
            using (LockObject.EnterReadLock())
                return _lstData.BinarySearch(item);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T, IComparer{T})" />
        public int BinarySearch(T item, IComparer<T> comparer)
        {
            using (LockObject.EnterReadLock())
                return _lstData.BinarySearch(item, comparer);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(int, int, T, IComparer{T})" />
        public async Task<int> BinarySearchAsync(int index, int count, T item, IComparer<T> comparer, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.BinarySearch(index, count, item, comparer);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T)" />
        public async Task<int> BinarySearchAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.BinarySearch(item);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T, IComparer{T})" />
        public async Task<int> BinarySearchAsync(T item, IComparer<T> comparer, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.BinarySearch(item, comparer);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public int Add(object value)
        {
            if (!(value is T objValue))
                return -1;
            using (LockObject.EnterUpgradeableReadLock())
            {
                Add(objValue);
                return _lstData.Count - 1;
            }
        }

        public bool Contains(object value)
        {
            if (!(value is T objValue))
                return false;
            return Contains(objValue);
        }

        /// <inheritdoc cref="List{T}.Clear" />
        public void Clear()
        {
            using (LockObject.EnterWriteLock())
                _lstData.Clear();
        }

        public int IndexOf(object value)
        {
            if (!(value is T objValue))
                return -1;
            return IndexOf(objValue);
        }

        public void Insert(int index, object value)
        {
            if (!(value is T objValue))
                return;
            Insert(index, objValue);
        }

        public void Remove(object value)
        {
            if (!(value is T objValue))
                return;
            Remove(objValue);
        }

        /// <inheritdoc cref="List{T}.Clear" />
        public async Task ClearAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstData.Clear();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Contains" />
        public bool Contains(T item)
        {
            using (LockObject.EnterReadLock())
                return _lstData.Contains(item);
        }

        /// <inheritdoc cref="List{T}.Contains" />
        public async Task<bool> ContainsAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.Contains(item);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.ConvertAll{TOutput}" />
        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            using (LockObject.EnterReadLock())
                return _lstData.ConvertAll(converter);
        }

        /// <inheritdoc cref="List{T}.ConvertAll{TOutput}" />
        public async Task<List<TOutput>> ConvertAllAsync<TOutput>(Converter<T, TOutput> converter, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.ConvertAll(converter);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.CopyTo(T[])" />
        public void CopyTo(T[] array)
        {
            using (LockObject.EnterReadLock())
                _lstData.CopyTo(array);
        }

        /// <inheritdoc cref="List{T}.CopyTo(int, T[], int, int)" />
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            using (LockObject.EnterReadLock())
                _lstData.CopyTo(index, array, arrayIndex, count);
        }

        /// <inheritdoc cref="List{T}.CopyTo(T[], int)" />
        public void CopyTo(T[] array, int arrayIndex)
        {
            using (LockObject.EnterReadLock())
                _lstData.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc cref="List{T}.CopyTo(T[])" />
        public async Task CopyToAsync(T[] array, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstData.CopyTo(array);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.CopyTo(int, T[], int, int)" />
        public async Task CopyToAsync(int index, T[] array, int arrayIndex, int count, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstData.CopyTo(index, array, arrayIndex, count);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.CopyTo(T[], int)" />
        public async Task CopyToAsync(T[] array, int index, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstData.CopyTo(array, index);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public bool TryAdd(T item)
        {
            Add(item);
            return true;
        }

        public async Task<bool> TryAddAsync(T item, CancellationToken token = default)
        {
            await AddAsync(item, token).ConfigureAwait(false);
            return true;
        }

        /// <inheritdoc />
        public async Task<ValueTuple<bool, T>> TryTakeAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_lstData.Count == 0)
                    return new ValueTuple<bool, T>(false, default);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_lstData.Count > 0)
                {
                    // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                    T objReturn = _lstData[0];
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _lstData.RemoveAt(0);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }

                    return new ValueTuple<bool, T>(true, objReturn);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return new ValueTuple<bool, T>(false, default);
        }

        /// <inheritdoc />
        public bool TryTake(out T item)
        {
            using (LockObject.EnterReadLock())
            {
                if (_lstData.Count == 0)
                {
                    item = default;
                    return false;
                }
            }
            using (LockObject.EnterUpgradeableReadLock())
            {
                if (_lstData.Count > 0)
                {
                    // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                    item = _lstData[0];
                    using (LockObject.EnterWriteLock())
                        _lstData.RemoveAt(0);
                    return true;
                }
            }

            item = default;
            return false;
        }

        /// <inheritdoc cref="List{T}.Exists" />
        public bool Exists(Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.Exists(match);
        }

        /// <inheritdoc cref="List{T}.Exists" />
        public async Task<bool> ExistsAsync(Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.Exists(match);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Find" />
        public T Find(Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.Find(match);
        }

        /// <inheritdoc cref="List{T}.Find" />
        public async Task<T> FindAsync(Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.Find(match);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.FindAll" />
        public List<T> FindAll(Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.FindAll(match);
        }

        /// <inheritdoc cref="List{T}.FindAll" />
        public async Task<List<T>> FindAllAsync(Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.FindAll(match);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.FindIndex(Predicate{T})" />
        public int FindIndex(Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.FindIndex(match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, Predicate{T})" />
        public int FindIndex(int startIndex, Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.FindIndex(startIndex, match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, int, Predicate{T})" />
        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.FindIndex(startIndex, count, match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(Predicate{T})" />
        public async Task<int> FindIndexAsync(Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.FindIndex(match);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, Predicate{T})" />
        public async Task<int> FindIndexAsync(int startIndex, Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.FindIndex(startIndex, match);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, int, Predicate{T})" />
        public async Task<int> FindIndexAsync(int startIndex, int count, Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.FindIndex(startIndex, count, match);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.FindLast" />
        public T FindLast(Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.FindLast(match);
        }

        /// <inheritdoc cref="List{T}.FindLast" />
        public async Task<T> FindLastAsync(Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.FindLast(match);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(Predicate{T})" />
        public int FindLastIndex(Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.FindIndex(match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, Predicate{T})" />
        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.FindIndex(startIndex, match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, int, Predicate{T})" />
        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.FindLastIndex(startIndex, count, match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(Predicate{T})" />
        public async Task<int> FindLastIndexAsync(Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.FindLastIndex(match);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, Predicate{T})" />
        public async Task<int> FindLastIndexAsync(int startIndex, Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.FindLastIndex(startIndex, match);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, int, Predicate{T})" />
        public async Task<int> FindLastIndexAsync(int startIndex, int count, Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.FindLastIndex(startIndex, count, match);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.ForEach" />
        public void ForEach(Action<T> action)
        {
            using (LockObject.EnterReadLock())
                _lstData.ForEach(action);
        }

        /// <inheritdoc cref="List{T}.ForEach" />
        public async Task ForEachAsync(Action<T> action, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstData.ForEach(x =>
                {
                    token.ThrowIfCancellationRequested();
                    action.Invoke(x);
                });
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.ForEach" />
        public Task ForEachAsync(Func<T, Task> action, CancellationToken token = default)
        {
            return AsyncEnumerableExtensions.ForEachAsync(this, action, token);
        }

        /// <inheritdoc cref="List{T}.ForEach" />
        public async Task ForEachAsync(Task<Action<T>> action, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Action<T> funcAction = await action.ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                _lstData.ForEach(x =>
                {
                    token.ThrowIfCancellationRequested();
                    funcAction.Invoke(x);
                });
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.GetEnumerator" />
        public IEnumerator<T> GetEnumerator()
        {
            LockingEnumerator<T> objReturn = LockingEnumerator<T>.Get(this);
            objReturn.SetEnumerator(_lstData.GetEnumerator());
            return objReturn;
        }

        public Task<IEnumerator<T>> GetEnumeratorAsync(CancellationToken token = default)
        {
            // Needs to be like this (using async inner function) to make sure AsyncLocals for parents are set in proper location
            Task<LockingEnumerator<T>> tskReturn = LockingEnumerator<T>.GetAsync(this, token);
            return Inner(tskReturn);
            async Task<IEnumerator<T>> Inner(Task<LockingEnumerator<T>> tskInner)
            {
                LockingEnumerator<T> objResult = await tskInner.ConfigureAwait(false);
                objResult.SetEnumerator(_lstData.GetEnumerator());
                return objResult;
            }
        }

        public IEnumerator<T> EnumerateWithSideEffects()
        {
            LockingEnumerator<T> objReturn = LockingEnumerator<T>.GetWithSideEffects(this);
            objReturn.SetEnumerator(_lstData.GetEnumerator());
            return objReturn;
        }

        public Task<IEnumerator<T>> EnumerateWithSideEffectsAsync(CancellationToken token = default)
        {
            // Needs to be like this (using async inner function) to make sure AsyncLocals for parents are set in proper location
            Task<LockingEnumerator<T>> tskReturn = LockingEnumerator<T>.GetWithSideEffectsAsync(this, token);
            return Inner(tskReturn);
            async Task<IEnumerator<T>> Inner(Task<LockingEnumerator<T>> tskInner)
            {
                LockingEnumerator<T> objResult = await tskInner.ConfigureAwait(false);
                objResult.SetEnumerator(_lstData.GetEnumerator());
                return objResult;
            }
        }

        /// <inheritdoc cref="List{T}.GetRange" />
        public List<T> GetRange(int index, int count)
        {
            using (LockObject.EnterReadLock())
                return _lstData.GetRange(index, count);
        }

        /// <inheritdoc cref="List{T}.GetRange" />
        public async Task<List<T>> GetRangeAsync(int index, int count, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.GetRange(index, count);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.IndexOf(T)" />
        public int IndexOf(T item)
        {
            using (LockObject.EnterReadLock())
                return _lstData.IndexOf(item);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T, int)" />
        public int IndexOf(T item, int index)
        {
            using (LockObject.EnterReadLock())
                return _lstData.IndexOf(item, index);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T, int, int)" />
        public int IndexOf(T item, int index, int count)
        {
            using (LockObject.EnterReadLock())
                return _lstData.IndexOf(item, index, count);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T)" />
        public async Task<int> IndexOfAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.IndexOf(item);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.IndexOf(T, int)" />
        public async Task<int> IndexOfAsync(T item, int index, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.IndexOf(item, index);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.IndexOf(T, int, int)" />
        public async Task<int> IndexOfAsync(T item, int index, int count, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.IndexOf(item, index, count);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Insert" />
        public void Insert(int index, T item)
        {
            using (LockObject.EnterWriteLock())
                _lstData.Insert(index, item);
        }

        /// <inheritdoc cref="List{T}.Insert" />
        public async Task InsertAsync(int index, T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstData.Insert(index, item);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.InsertRange" />
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            using (LockObject.EnterWriteLock())
                _lstData.InsertRange(index, collection);
        }

        /// <inheritdoc cref="List{T}.InsertRange" />
        public async Task InsertRangeAsync(int index, IEnumerable<T> collection, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstData.InsertRange(index, collection);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T)" />
        public int LastIndexOf(T item)
        {
            using (LockObject.EnterReadLock())
                return _lstData.LastIndexOf(item);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int)" />
        public int LastIndexOf(T item, int index)
        {
            using (LockObject.EnterReadLock())
                return _lstData.LastIndexOf(item, index);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int, int)" />
        public int LastIndexOf(T item, int index, int count)
        {
            using (LockObject.EnterReadLock())
                return _lstData.LastIndexOf(item, index, count);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T)" />
        public async Task<int> LastIndexOfAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.LastIndexOf(item);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int)" />
        public async Task<int> LastIndexOfAsync(T item, int index, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.LastIndexOf(item, index);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int, int)" />
        public async Task<int> LastIndexOfAsync(T item, int index, int count, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.LastIndexOf(item, index, count);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            using (LockObject.EnterWriteLock())
                return _lstData.Remove(item);
        }

        /// <inheritdoc cref="List{T}.Remove(T)" />
        public async Task<bool> RemoveAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.Remove(item);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.RemoveAll" />
        public int RemoveAll(Predicate<T> match)
        {
            using (LockObject.EnterWriteLock())
                return _lstData.RemoveAll(match);
        }

        /// <inheritdoc cref="List{T}.RemoveAll" />
        public async Task<int> RemoveAllAsync(Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.RemoveAll(match);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.RemoveAll" />
        public async Task<int> RemoveAllAsync(Task<Predicate<T>> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.RemoveAll(await match.ConfigureAwait(false));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.RemoveAt" />
        public void RemoveAt(int index)
        {
            using (LockObject.EnterWriteLock())
                _lstData.RemoveAt(index);
        }

        /// <inheritdoc cref="List{T}.RemoveAt" />
        public async Task RemoveAtAsync(int index, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstData.RemoveAt(index);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.RemoveRange" />
        public void RemoveRange(int index, int count)
        {
            using (LockObject.EnterWriteLock())
                _lstData.RemoveRange(index, count);
        }

        /// <inheritdoc cref="List{T}.RemoveRange" />
        public async Task RemoveRangeAsync(int index, int count, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstData.RemoveRange(index, count);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Reverse()" />
        public void Reverse()
        {
            using (LockObject.EnterWriteLock())
                _lstData.Reverse();
        }

        /// <inheritdoc cref="List{T}.Reverse(int, int)" />
        public void Reverse(int index, int count)
        {
            using (LockObject.EnterWriteLock())
                _lstData.Reverse(index, count);
        }

        /// <inheritdoc cref="List{T}.Reverse()" />
        public async Task ReverseAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstData.Reverse();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Reverse(int, int)" />
        public async Task ReverseAsync(int index, int count, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstData.Reverse(index, count);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Sort()" />
        public void Sort()
        {
            using (LockObject.EnterWriteLock())
                _lstData.Sort();
        }

        /// <inheritdoc cref="List{T}.Sort(IComparer{T})" />
        public void Sort(IComparer<T> comparer)
        {
            using (LockObject.EnterWriteLock())
                _lstData.Sort(comparer);
        }

        /// <inheritdoc cref="List{T}.Sort(int, int, IComparer{T})" />
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            using (LockObject.EnterWriteLock())
                _lstData.Sort(index, count, comparer);
        }

        /// <inheritdoc cref="List{T}.Sort(Comparison{T})" />
        public void Sort(Comparison<T> comparison)
        {
            using (LockObject.EnterWriteLock())
                _lstData.Sort(comparison);
        }

        /// <inheritdoc cref="List{T}.Sort()" />
        public async Task SortAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstData.Sort();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Sort(IComparer{T})" />
        public async Task SortAsync(IComparer<T> comparer, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstData.Sort(comparer);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Sort(int, int, IComparer{T})" />
        public async Task SortAsync(int index, int count, IComparer<T> comparer, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstData.Sort(index, count, comparer);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Sort(Comparison{T})" />
        public async Task SortAsync(Comparison<T> comparison, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstData.Sort(comparison);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public T[] ToArray()
        {
            using (LockObject.EnterReadLock())
                return _lstData.ToArray();
        }

        public async Task<T[]> ToArrayAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.ToArray();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.TrimExcess" />
        public void TrimExcess()
        {
            using (LockObject.EnterWriteLock())
                _lstData.TrimExcess();
        }

        /// <inheritdoc cref="List{T}.TrimExcess" />
        public async Task TrimExcessAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstData.TrimExcess();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.TrueForAll" />
        public bool TrueForAll(Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.TrueForAll(match);
        }

        /// <inheritdoc cref="List{T}.TrueForAll" />
        public async Task<bool> TrueForAllAsync(Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.TrueForAll(match);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.TrueForAll" />
        public async Task<bool> TrueForAllAsync(Task<Predicate<T>> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.TrueForAll(await match.ConfigureAwait(false));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intIsDisposed;

        public bool IsDisposed => _intIsDisposed > 0;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) > 0)
                    return;
                LockObject.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                if (Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) > 0)
                    return;
                await LockObject.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true).ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
