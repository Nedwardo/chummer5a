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
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer.UI.Table
{
    /// <summary>
    /// Class containing information on how to visually represent
    /// a item in a certain table column.
    /// </summary>
    /// <typeparam name="T">the table item type</typeparam>
    public class TableColumn<T> : IDisposable where T : INotifyPropertyChanged
    {
        private Func<TableCell> _cellFactory;
        private Func<Task<object>, Task<object>, CancellationToken, Task<int>> _sorter;
        private bool _blnLive;
        private string _strText;
        private string _strTag;
        private int _intMinWidth;
        private int _intPrefWidth;
        private Func<T, CancellationToken, Task<object>> _funcExtractor;
        private Func<T, T, CancellationToken, Task<int>> _itemSorter;
        private HashSet<string> _setDependencies = Utils.StringHashSetPool.Get();

        public TableColumn(Func<TableCell> cellFactory)
        {
            _cellFactory = cellFactory ?? throw new ArgumentNullException(nameof(cellFactory));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // to help the GC
                _cellFactory = null;
                _sorter = null;
                _funcExtractor = null;
                _itemSorter = null;
                ToolTipExtractor = null;
                if (_setDependencies != null)
                    Utils.StringHashSetPool.Return(ref _setDependencies);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Add an additional dependency to the dependencies
        /// of this column.
        /// </summary>
        /// <param name="strDependency">the dependency to add</param>
        public void AddDependency(string strDependency)
        {
            CheckLive();
            _setDependencies.Add(strDependency);
        }

        /// <summary>
        /// Throw an InvalidOperationException in case the column is in
        /// live state.
        /// </summary>
        protected void CheckLive(bool blnExpectedValue = false)
        {
            if (_blnLive != blnExpectedValue)
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// create a cell instance
        /// </summary>
        /// <returns>a new instance of the cell</returns>
        internal TableCell CreateCell() => _cellFactory();

        internal Func<T, T, CancellationToken, Task<int>> CreateSorter()
        {
            if (_itemSorter == null && _sorter != null)
            {
                if (_funcExtractor == null)
                {
                    _itemSorter = (i1, i2, t) => _sorter(Task.FromResult<object>(i1), Task.FromResult<object>(i2), t);
                }
                else
                {
                    _itemSorter = (i1, i2, t) => _sorter(_funcExtractor(i1, t), _funcExtractor(i2, t), t);
                }
            }
            return _itemSorter;
        }

        #region Properties

        /// <summary>
        /// The dependencies as enumerable.
        /// </summary>
        internal IReadOnlyCollection<string> Dependencies => _setDependencies;

        /// <summary>
        /// Method for extracting the value for the cell from
        /// the item
        /// </summary>
        public Func<T, CancellationToken, Task<object>> Extractor
        {
            get => _funcExtractor;
            set
            {
                CheckLive();
                _funcExtractor = value;
            }
        }

        /// <summary>
        /// Property indicating whether this column is live or not.
        /// </summary>
        protected bool Live => _blnLive;

        /// <summary>
        /// The minimal width for this column.
        /// </summary>
        public int MinWidth
        {
            get => _intMinWidth;
            set
            {
                CheckLive();
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(MinWidth));
                }
                _intMinWidth = value;
            }
        }

        /// <summary>
        /// sorter for this column
        /// </summary>
        public Func<Task<object>, Task<object>, CancellationToken, Task<int>> Sorter
        {
            get => _sorter;
            set
            {
                CheckLive();
                _sorter = value;
            }
        }

        /// <summary>
        /// The text that should be displayed in the header.
        /// </summary>
        public string Text
        {
            get => _strText;
            set
            {
                CheckLive();
                _strText = value;
            }
        }

        /// <summary>
        /// The tag for the header.
        /// </summary>
        public string Tag
        {
            get => _strTag;
            set
            {
                CheckLive();
                _strTag = value;
            }
        }

        /// <summary>
        /// the preferred width of the column
        /// </summary>
        public int PrefWidth
        {
            get => _intPrefWidth;
            set
            {
                if (value >= _intMinWidth)
                {
                    _intPrefWidth = value;
                }
            }
        }

        /// <summary>
        /// Extractor for tooltip text on cell
        /// </summary>
        public Func<T, CancellationToken, Task<string>> ToolTipExtractor { get; set; }

        /// <summary>
        /// transfer the column to the live state
        /// </summary>
        internal void MakeLive()
        {
            CheckLive();
            _blnLive = true;
        }

        #endregion Properties
    }
}
