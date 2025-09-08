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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Chummer.Annotations;

namespace Chummer
{
    /// <summary>
    /// Expanded version of ObservableCollection that has an extra event for processing items before a Clear() command is executed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnhancedObservableCollection<T> : ObservableCollection<T>, INotifyMultiplePropertiesChangedAsync, IAsyncList<T>
    {
        /// <summary>
        /// CollectionChanged event subscription that will fire right before the collection is cleared.
        /// To make things easy, all the collections elements will be present in e.OldItems.
        /// </summary>
        [SuppressMessage("Design", "CA1070:Do not declare event fields as virtual", Justification = "We do want to override this, actually. Just make sure that any override has explicit adders and removers defined.")]
        public virtual event NotifyCollectionChangedEventHandler BeforeClearCollectionChanged;

        private readonly ConcurrentHashSet<AsyncNotifyCollectionChangedEventHandler> _setBeforeClearCollectionChangedAsync =
            new ConcurrentHashSet<AsyncNotifyCollectionChangedEventHandler>();

        /// <summary>
        /// CollectionChanged event subscription for async events that will fire right before the collection is cleared.
        /// To make things easy, all the collections elements will be present in e.OldItems.
        /// Use this event instead of BeforeClearCollectionChanged for tasks that will be awaited before completion.
        /// </summary>
        [SuppressMessage("Design", "CA1070:Do not declare event fields as virtual", Justification = "We do want to override this, actually. Just make sure that any override has explicit adders and removers defined.")]
        public virtual event AsyncNotifyCollectionChangedEventHandler BeforeClearCollectionChangedAsync
        {
            add => _setBeforeClearCollectionChangedAsync.TryAdd(value);
            remove => _setBeforeClearCollectionChangedAsync.Remove(value);
        }

        /// <inheritdoc />
        public EnhancedObservableCollection()
        {
        }

        /// <inheritdoc />
        public EnhancedObservableCollection(List<T> list) : base(list)
        {
        }

        /// <inheritdoc />
        public EnhancedObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        /// <inheritdoc />
        public EnhancedObservableCollection(AsyncFriendlyReaderWriterLock objCollectionChangedLock)
        {
            CollectionChangedLock = objCollectionChangedLock;
        }

        /// <inheritdoc />
        public EnhancedObservableCollection(AsyncFriendlyReaderWriterLock objCollectionChangedLock, List<T> list) : base(list)
        {
            CollectionChangedLock = objCollectionChangedLock;
        }

        /// <inheritdoc />
        public EnhancedObservableCollection(AsyncFriendlyReaderWriterLock objCollectionChangedLock, IEnumerable<T> collection) : base(collection)
        {
            CollectionChangedLock = objCollectionChangedLock;
        }

        protected override event PropertyChangedEventHandler PropertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                IDisposable objLocker = CollectionChangedLock?.EnterWriteLock();
                try
                {
                    base.PropertyChanged += value;
                }
                finally
                {
                    objLocker?.Dispose();
                }
            }
            remove
            {
                IDisposable objLocker = CollectionChangedLock?.EnterWriteLock();
                try
                {
                    base.PropertyChanged -= value;
                }
                finally
                {
                    objLocker?.Dispose();
                }
            }
        }

        private readonly ConcurrentHashSet<PropertyChangedAsyncEventHandler> _setPropertyChangedAsync =
            new ConcurrentHashSet<PropertyChangedAsyncEventHandler>();

        event PropertyChangedAsyncEventHandler INotifyPropertyChangedAsync.PropertyChangedAsync
        {
            add => _setPropertyChangedAsync.TryAdd(value);
            remove => _setPropertyChangedAsync.Remove(value);
        }

        protected virtual event MultiplePropertiesChangedEventHandler MultiplePropertiesChanged;

        event MultiplePropertiesChangedEventHandler INotifyMultiplePropertiesChanged.MultiplePropertiesChanged
        {
            add
            {
                IDisposable objLocker = CollectionChangedLock?.EnterWriteLock();
                try
                {
                    MultiplePropertiesChanged += value;
                }
                finally
                {
                    objLocker?.Dispose();
                }
            }
            remove
            {
                IDisposable objLocker = CollectionChangedLock?.EnterWriteLock();
                try
                {
                    MultiplePropertiesChanged -= value;
                }
                finally
                {
                    objLocker?.Dispose();
                }
            }
        }

        private readonly ConcurrentHashSet<MultiplePropertiesChangedAsyncEventHandler> _setMultiplePropertiesChangedAsync =
            new ConcurrentHashSet<MultiplePropertiesChangedAsyncEventHandler>();

        event MultiplePropertiesChangedAsyncEventHandler INotifyMultiplePropertiesChangedAsync.MultiplePropertiesChangedAsync
        {
            add => _setMultiplePropertiesChangedAsync.TryAdd(value);
            remove => _setMultiplePropertiesChangedAsync.Remove(value);
        }

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            this.OnMultiplePropertyChanged(strPropertyName);
        }

        public Task OnPropertyChangedAsync(string strPropertyName, CancellationToken token = default)
        {
            return this.OnMultiplePropertyChangedAsync(token, strPropertyName);
        }

        public void OnMultiplePropertiesChanged(IReadOnlyCollection<string> lstPropertyNames)
        {
            if (_setMultiplePropertiesChangedAsync.Count > 0)
            {
                MultiplePropertiesChangedEventArgs objArgs =
                    new MultiplePropertiesChangedEventArgs(lstPropertyNames);
                List<Func<Task>> lstFuncs = new List<Func<Task>>(_setMultiplePropertiesChangedAsync.Count);
                foreach (MultiplePropertiesChangedAsyncEventHandler objEvent in _setMultiplePropertiesChangedAsync)
                {
                    lstFuncs.Add(() => objEvent.Invoke(this, objArgs));
                }

                Utils.RunWithoutThreadLock(lstFuncs);
                if (MultiplePropertiesChanged != null)
                {
                    Utils.RunOnMainThread(() =>
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        MultiplePropertiesChanged?.Invoke(this, objArgs);
                    });
                }
            }
            else if (MultiplePropertiesChanged != null)
            {
                MultiplePropertiesChangedEventArgs objArgs =
                    new MultiplePropertiesChangedEventArgs(lstPropertyNames);
                Utils.RunOnMainThread(() =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    MultiplePropertiesChanged?.Invoke(this, objArgs);
                });
            }

            if (_setPropertyChangedAsync.Count > 0)
            {
                List<PropertyChangedEventArgs> lstArgsList = lstPropertyNames.Select(x => new PropertyChangedEventArgs(x)).ToList();
                List<Func<Task>> lstFuncs = new List<Func<Task>>(lstArgsList.Count * _setPropertyChangedAsync.Count);
                foreach (PropertyChangedAsyncEventHandler objEvent in _setPropertyChangedAsync)
                {
                    foreach (PropertyChangedEventArgs objArg in lstArgsList)
                        lstFuncs.Add(() => objEvent.Invoke(this, objArg));
                }

                Utils.RunWithoutThreadLock(lstFuncs);
                if (PropertyChanged != null)
                {
                    Utils.RunOnMainThread(() =>
                    {
                        if (PropertyChanged != null)
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            foreach (PropertyChangedEventArgs objArgs in lstArgsList)
                            {
                                base.OnPropertyChanged(objArgs);
                            }
                        }
                    });
                }
            }
            else if (PropertyChanged != null)
            {
                Utils.RunOnMainThread(() =>
                {
                    if (PropertyChanged != null)
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        foreach (string strPropertyToChange in lstPropertyNames)
                        {
                            base.OnPropertyChanged(new PropertyChangedEventArgs(strPropertyToChange));
                        }
                    }
                });
            }
        }

        public async Task OnMultiplePropertiesChangedAsync(IReadOnlyCollection<string> lstPropertyNames, CancellationToken token = default)
        {
            if (_setMultiplePropertiesChangedAsync.Count > 0)
            {
                MultiplePropertiesChangedEventArgs objArgs =
                    new MultiplePropertiesChangedEventArgs(lstPropertyNames);
                await ParallelExtensions.ForEachAsync(_setMultiplePropertiesChangedAsync, objEvent => objEvent.Invoke(this, objArgs, token), token).ConfigureAwait(false);
                if (MultiplePropertiesChanged != null)
                {
                    await Utils.RunOnMainThreadAsync(() =>
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        MultiplePropertiesChanged?.Invoke(this, objArgs);
                    }, token: token).ConfigureAwait(false);
                }
            }
            else if (MultiplePropertiesChanged != null)
            {
                MultiplePropertiesChangedEventArgs objArgs =
                    new MultiplePropertiesChangedEventArgs(lstPropertyNames);
                await Utils.RunOnMainThreadAsync(() =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    MultiplePropertiesChanged?.Invoke(this, objArgs);
                }, token: token).ConfigureAwait(false);
            }

            if (_setPropertyChangedAsync.Count > 0)
            {
                List<PropertyChangedEventArgs> lstArgsList = lstPropertyNames
                    .Select(x => new PropertyChangedEventArgs(x)).ToList();
                List<Tuple<PropertyChangedAsyncEventHandler, PropertyChangedEventArgs>> lstAsyncEventsList
                            = new List<Tuple<PropertyChangedAsyncEventHandler, PropertyChangedEventArgs>>(lstArgsList.Count * _setPropertyChangedAsync.Count);
                foreach (PropertyChangedAsyncEventHandler objEvent in _setPropertyChangedAsync)
                {
                    foreach (PropertyChangedEventArgs objArg in lstArgsList)
                    {
                        lstAsyncEventsList.Add(new Tuple<PropertyChangedAsyncEventHandler, PropertyChangedEventArgs>(objEvent, objArg));
                    }
                }
                await ParallelExtensions.ForEachAsync(lstAsyncEventsList, tupEvent => tupEvent.Item1.Invoke(this, tupEvent.Item2, token), token).ConfigureAwait(false);

                if (PropertyChanged != null)
                {
                    await Utils.RunOnMainThreadAsync(() =>
                    {
                        if (PropertyChanged != null)
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            foreach (PropertyChangedEventArgs objArgs in lstArgsList)
                            {
                                base.OnPropertyChanged(objArgs);
                            }
                        }
                    }, token).ConfigureAwait(false);
                }
            }
            else if (PropertyChanged != null)
            {
                await Utils.RunOnMainThreadAsync(() =>
                {
                    if (PropertyChanged != null)
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        foreach (string strPropertyToChange in lstPropertyNames)
                        {
                            base.OnPropertyChanged(new PropertyChangedEventArgs(strPropertyToChange));
                        }
                    }
                }, token: token).ConfigureAwait(false);
            }
        }

        private Task OnCollectionChangedAsync(NotifyCollectionChangedAction action, object item, int index, CancellationToken token = default)
        {
            return OnCollectionChangedAsync(new NotifyCollectionChangedEventArgs(action, item, index), token);
        }

        private Task OnCollectionChangedAsync(NotifyCollectionChangedAction action, object item, int index, int oldIndex, CancellationToken token = default)
        {
            return OnCollectionChangedAsync(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex), token);
        }

        private Task OnCollectionChangedAsync(NotifyCollectionChangedAction action, object oldItem, object newItem, int index, CancellationToken token = default)
        {
            return OnCollectionChangedAsync(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index), token);
        }

        private Task OnCollectionResetAsync(CancellationToken token = default)
        {
            return OnCollectionChangedAsync(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset), token);
        }

        public Task MoveAsync(int oldIndex, int newIndex, CancellationToken token = default) => MoveItemAsync(oldIndex, newIndex, token);

        /// <inheritdoc />
        protected override void ClearItems()
        {
            IDisposable objLocker = CollectionChangedLock?.EnterReadLockWithUpgradeableParent();
            try
            {
                using (BlockReentrancy())
                {
                    if (_setBeforeClearCollectionChangedAsync.Count != 0)
                    {
                        NotifyCollectionChangedEventArgs objArgs =
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                                (IList)Items);
                        List<Func<Task>> lstFuncs =
                            new List<Func<Task>>(_setBeforeClearCollectionChangedAsync.Count);
                        foreach (AsyncNotifyCollectionChangedEventHandler objEvent in
                                 _setBeforeClearCollectionChangedAsync)
                            lstFuncs.Add(() => objEvent.Invoke(this, objArgs));
                        Utils.RunWithoutThreadLock(lstFuncs);
                        BeforeClearCollectionChanged?.Invoke(this, objArgs);
                    }
                    else
                    {
                        BeforeClearCollectionChanged?.Invoke(this,
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)Items));
                    }
                }
            }
            finally
            {
                objLocker?.Dispose();
            }

            base.ClearItems();
        }

        public virtual async Task ClearItemsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CheckReentrancy();
            IAsyncDisposable objLocker = CollectionChangedLock != null
                ? await CollectionChangedLock.EnterReadLockWithUpgradeableParentAsync(token)
                    .ConfigureAwait(false)
                : null;
            try
            {
                token.ThrowIfCancellationRequested();
                using (BlockReentrancy())
                {
                    if (_setBeforeClearCollectionChangedAsync.Count != 0)
                    {
                        NotifyCollectionChangedEventArgs objArgs =
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                                    (IList)Items);
                        await ParallelExtensions.ForEachAsync(
                                _setBeforeClearCollectionChangedAsync, x => x.Invoke(this, objArgs, token), token)
                            .ConfigureAwait(false);
                        BeforeClearCollectionChanged?.Invoke(this, objArgs);
                    }
                    else
                    {
                        BeforeClearCollectionChanged?.Invoke(this,
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)Items));
                    }
                }
            }
            finally
            {
                if (objLocker != null)
                    await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            Items.Clear();
            await this.OnMultiplePropertyChangedAsync(token, "Count", "Item[]").ConfigureAwait(false);
            await OnCollectionResetAsync(token).ConfigureAwait(false);
        }

        public virtual async Task RemoveItemAsync(int index, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CheckReentrancy();
            T obj = this[index];
            Items.RemoveAt(index);
            await this.OnMultiplePropertyChangedAsync(token, "Count", "Item[]").ConfigureAwait(false);
            await OnCollectionChangedAsync(NotifyCollectionChangedAction.Remove, obj, index, token).ConfigureAwait(false);
        }

        public virtual async Task InsertItemAsync(int index, T item, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CheckReentrancy();
            Items.Insert(index, item);
            await this.OnMultiplePropertyChangedAsync(token, "Count", "Item[]").ConfigureAwait(false);
            await OnCollectionChangedAsync(NotifyCollectionChangedAction.Add, item, index, token).ConfigureAwait(false);
        }

        public virtual async Task SetItemAsync(int index, T item, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CheckReentrancy();
            T oldItem = Items[index];
            Items[index] = item;
            await OnPropertyChangedAsync("Item[]", token).ConfigureAwait(false);
            await OnCollectionChangedAsync(NotifyCollectionChangedAction.Replace, oldItem, item, index, token).ConfigureAwait(false);
        }

        public virtual async Task MoveItemAsync(int oldIndex, int newIndex, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CheckReentrancy();
            T obj = Items[oldIndex];
            Items.RemoveAt(oldIndex);
            Items.Insert(newIndex, obj);
            await OnPropertyChangedAsync("Item[]", token).ConfigureAwait(false);
            await OnCollectionChangedAsync(NotifyCollectionChangedAction.Move, obj, newIndex, oldIndex, token).ConfigureAwait(false);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            IDisposable objLocker = CollectionChangedLock?.EnterReadLockWithUpgradeableParent();
            try
            {
                if (_setCollectionChangedAsync.Count != 0)
                {
                    List<Func<Task>> lstFuncs = new List<Func<Task>>(_setCollectionChangedAsync.Count);
                    foreach (AsyncNotifyCollectionChangedEventHandler objEvent in _setCollectionChangedAsync)
                        lstFuncs.Add(() => objEvent.Invoke(this, e));
                    Utils.RunWithoutThreadLock(lstFuncs);
                }
                base.OnCollectionChanged(e);
            }
            finally
            {
                objLocker?.Dispose();
            }
        }

        protected virtual async Task OnCollectionChangedAsync(NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = CollectionChangedLock != null
                ? await CollectionChangedLock.EnterReadLockWithUpgradeableParentAsync(token).ConfigureAwait(false)
                : null;
            try
            {
                token.ThrowIfCancellationRequested();
                if (_setCollectionChangedAsync.Count != 0)
                {
                    await ParallelExtensions.ForEachAsync(_setCollectionChangedAsync, x => x.Invoke(this, e, token), token)
                        .ConfigureAwait(false);
                }
                base.OnCollectionChanged(e);
            }
            finally
            {
                if (objLocker != null)
                    await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public AsyncFriendlyReaderWriterLock CollectionChangedLock { get; }

        private readonly ConcurrentHashSet<AsyncNotifyCollectionChangedEventHandler> _setCollectionChangedAsync =
            new ConcurrentHashSet<AsyncNotifyCollectionChangedEventHandler>();

        /// <summary>
        /// Like CollectionChanged, occurs when an item is added, removed, changed, moved, or the entire list is refreshed.
        /// Use this event instead of CollectionChanged for tasks that will be awaited before completion.
        /// </summary>
        [SuppressMessage("Design", "CA1070:Do not declare event fields as virtual", Justification = "We do want to override this, actually. Just make sure that any override has explicit adders and removers defined.")]
        public virtual event AsyncNotifyCollectionChangedEventHandler CollectionChangedAsync
        {
            add => _setCollectionChangedAsync.TryAdd(value);
            remove => _setCollectionChangedAsync.Remove(value);
        }

        public Task<IEnumerator<T>> GetEnumeratorAsync(CancellationToken token = default)
        {
            return token.IsCancellationRequested ? Task.FromCanceled<IEnumerator<T>>(token) : Task.FromResult(GetEnumerator());
        }

        public Task<int> GetCountAsync(CancellationToken token = default)
        {
            return token.IsCancellationRequested ? Task.FromCanceled<int>(token) : Task.FromResult(Count);
        }

        public async Task AddAsync(T item, CancellationToken token = default)
        {
            await InsertItemAsync(await GetCountAsync(token).ConfigureAwait(false), item, token).ConfigureAwait(false);
        }

        public Task ClearAsync(CancellationToken token = default)
        {
            return ClearItemsAsync(token);
        }

        public Task<bool> ContainsAsync(T item, CancellationToken token = default)
        {
            return token.IsCancellationRequested ? Task.FromCanceled<bool>(token) : Task.FromResult(Contains(item));
        }

        public Task CopyToAsync(T[] array, int index, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            CopyTo(array, index);
            return Task.CompletedTask;
        }

        public async Task<bool> RemoveAsync(T item, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int index = await IndexOfAsync(item, token).ConfigureAwait(false);
            if (index < 0)
                return false;
            await RemoveItemAsync(index, token).ConfigureAwait(false);
            return true;
        }

        public Task<T> GetValueAtAsync(int index, CancellationToken token = default)
        {
            return token.IsCancellationRequested ? Task.FromCanceled<T>(token) : Task.FromResult(this[index]);
        }

        public Task SetValueAtAsync(int index, T value, CancellationToken token = default)
        {
            return SetItemAsync(index, value, token);
        }

        public Task<int> IndexOfAsync(T item, CancellationToken token = default)
        {
            return token.IsCancellationRequested ? Task.FromCanceled<int>(token) : Task.FromResult(IndexOf(item));
        }

        public Task InsertAsync(int index, T item, CancellationToken token = default)
        {
            return InsertItemAsync(index, item, token);
        }

        public Task RemoveAtAsync(int index, CancellationToken token = default)
        {
            return RemoveItemAsync(index, token);
        }
    }

    public delegate Task AsyncNotifyCollectionChangedEventHandler(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default);
}
