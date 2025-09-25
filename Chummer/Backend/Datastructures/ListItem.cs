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
using System.Diagnostics;
using System.Windows.Forms;

namespace Chummer
{
    /// <summary>
    /// ListItem class to make populating a DropDownList from a DataSource easier.
    /// </summary>
    [DebuggerDisplay("{Name} {Value?.ToString() ?? \"\"}")]
    public readonly struct ListItem : IEquatable<ListItem>, IComparable, IComparable<ListItem>
    {
        public static readonly ListItem Blank = new ListItem(string.Empty, string.Empty);

        public ListItem(object objValue, string strName)
        {
            Value = objValue;
            Name = strName;
        }

        /// <summary>
        /// Value.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; }

        public bool Equals(ListItem other)
        {
            return Name == other.Name && Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            if (obj is ListItem objItem)
                return CompareTo(objItem);
            return string.CompareOrdinal(ToString(), obj?.ToString() ?? string.Empty);
        }

        public int CompareTo(ListItem other)
        {
            return CompareListItems.CompareNames(this, other);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(ListItem x, ListItem y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(ListItem x, ListItem y)
        {
            return !x.Equals(y);
        }

        public static bool operator ==(ListItem x, object y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(ListItem x, object y)
        {
            return !x.Equals(y);
        }

        public static bool operator ==(object x, ListItem y)
        {
            return x?.Equals(y) ?? false;
        }

        public static bool operator !=(object x, ListItem y)
        {
            return !(x?.Equals(y) ?? false);
        }

        public static bool operator <(ListItem left, ListItem right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(ListItem left, ListItem right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(ListItem left, ListItem right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(ListItem left, ListItem right)
        {
            return left.CompareTo(right) >= 0;
        }
    }

    #region Sorting Classes

    public static class CompareTreeNodes
    {
        /// <summary>
        /// Sort TreeNodes in alphabetical order, ignoring [].
        /// </summary>
        public static int CompareText(TreeNode tx, TreeNode ty)
        {
            if (tx == null)
            {
                if (ty == null)
                    return 0;
                return -1;
            }
            return ty == null ? 1 : string.Compare(tx.Text.FastEscape('[', ']'), ty.Text.FastEscape('[', ']'), false, GlobalSettings.CultureInfo);
        }
    }

    public static class CompareListViewItems
    {
        /// <summary>
        /// Sort ListViewItems in reverse chronological order.
        /// </summary>
        public static int CompareTextAsDates(ListViewItem lx, ListViewItem ly)
        {
            if (lx == null || !DateTime.TryParse(lx.Text, GlobalSettings.CultureInfo, System.Globalization.DateTimeStyles.None, out DateTime datX))
            {
                if (ly == null || !DateTime.TryParse(ly.Text, GlobalSettings.CultureInfo, System.Globalization.DateTimeStyles.None, out _))
                    return 0;
                return -1;
            }

            if (ly == null || !DateTime.TryParse(ly.Text, GlobalSettings.CultureInfo, System.Globalization.DateTimeStyles.None, out DateTime datY))
                return 1;

            return DateTime.Compare(datY, datX);
        }
    }

    public static class CompareListItems
    {
        /// <summary>
        /// Sort ListItems by Name in alphabetical order.
        /// </summary>
        public static int CompareNames(ListItem objX, ListItem objY)
        {
            return string.Compare(objX.Name, objY.Name, false, GlobalSettings.CultureInfo);
        }
    }

    /// <summary>
    /// Sort ListViewColumns.
    /// </summary>
    public class ListViewColumnSorter : IComparer
    {
        private int _intColumnToSort;
        private SortOrder _eOrderOfSort;

        private readonly char[] _achrCachedNuyenSymbols = LanguageManager.GetString("String_NuyenSymbol").ToCharArray();

        public int Compare(object x, object y)
        {
            if (_eOrderOfSort == SortOrder.None)
                return 0;

            int intCompareResult;

            // Cast the objects to be compared to ListViewItem objects
            ListViewItem objListViewX = (ListViewItem)x;
            ListViewItem objListViewY = (ListViewItem)y;

            // Compare the two items
            if (_intColumnToSort == 0)
            {
                if (objListViewX is ListViewItemWithValue objListViewItemWithValueX &&
                    objListViewY is ListViewItemWithValue objListViewItemWithValueY &&
                    (objListViewItemWithValueX.Value is IComparable ||
                     objListViewItemWithValueY.Value is IComparable))
                {
                    if (objListViewItemWithValueX.Value is IComparable objXValue)
                        intCompareResult = objXValue.CompareTo(objListViewItemWithValueY.Value);
                    else
                        intCompareResult = -(objListViewItemWithValueY.Value as IComparable)?.CompareTo(objListViewItemWithValueX.Value) ?? 0;
                }
                else
                    intCompareResult = CompareListViewItems.CompareTextAsDates(objListViewX, objListViewY);
            }
            else
            {
                ListViewItem.ListViewSubItem objListViewSubItemX = objListViewX?.SubItems[_intColumnToSort];
                ListViewItem.ListViewSubItem objListViewSubItemY = objListViewY?.SubItems[_intColumnToSort];
                if (objListViewSubItemX is ListViewItemWithValue.ListViewSubItemWithValue objListViewSubItemWithValueX &&
                    objListViewSubItemY is ListViewItemWithValue.ListViewSubItemWithValue objListViewSubItemWithValueY &&
                    (objListViewSubItemWithValueX.Value is IComparable ||
                     objListViewSubItemWithValueY.Value is IComparable))
                {
                    if (objListViewSubItemWithValueX.Value is IComparable objXValue)
                        intCompareResult = objXValue.CompareTo(objListViewSubItemWithValueY.Value);
                    else
                        intCompareResult = -(objListViewSubItemWithValueY.Value as IComparable)?.CompareTo(objListViewSubItemWithValueX.Value) ?? 0;
                }
                else
                {
                    string strX = objListViewX?.SubItems[_intColumnToSort].Text.FastEscape(_achrCachedNuyenSymbols);
                    string strY = objListViewY?.SubItems[_intColumnToSort].Text.FastEscape(_achrCachedNuyenSymbols);
                    if (decimal.TryParse(strX, System.Globalization.NumberStyles.Any, GlobalSettings.CultureInfo,
                            out decimal decX) &&
                        decimal.TryParse(strY, System.Globalization.NumberStyles.Any, GlobalSettings.CultureInfo,
                            out decimal decY))
                        intCompareResult = decimal.Compare(decX, decY);
                    else
                        intCompareResult = string.Compare(strX, strY, true, GlobalSettings.CultureInfo);
                }
            }

            // Calculate correct return value based on object comparison
            if (_eOrderOfSort == SortOrder.Ascending)
                return intCompareResult;
            return -intCompareResult;
        }

        /// <summary>
        /// Column number to sort on.
        /// </summary>
        public int SortColumn
        {
            get => _intColumnToSort;
            set => _intColumnToSort = value;
        }

        /// <summary>
        /// SortOrder to be used.
        /// </summary>
        public SortOrder Order
        {
            get => _eOrderOfSort;
            set => _eOrderOfSort = value;
        }
    }

    /// <summary>
    /// Sort DataGridView Columns.
    /// </summary>
    public class DataGridViewColumnSorter : IComparer
    {
        private int _intColumnToSort;
        private SortOrder _objOrderOfSort;

        private readonly char[] _achrCachedNuyenSymbols = LanguageManager.GetString("String_NuyenSymbol").ToCharArray();
        private readonly string _strCachedRestrictedSymbol = LanguageManager.GetString("String_AvailRestricted");
        private readonly string _strCachedForbiddenSymbol = LanguageManager.GetString("String_AvailForbidden");

        public int Compare(object x, object y)
        {
            if (_objOrderOfSort == SortOrder.None)
                return 0;

            int intCompareResult;

            // Cast the objects to be compared to ListViewItem objects
            DataGridViewRow datagridviewrowX = (DataGridViewRow)x;
            DataGridViewRow datagridviewrowY = (DataGridViewRow)y;

            // Compare the two items
            string strX = datagridviewrowX?.Cells[_intColumnToSort].Value.ToString();
            string strY = datagridviewrowY?.Cells[_intColumnToSort].Value.ToString();
            string strNumberX = strX?.TrimEnd(_achrCachedNuyenSymbols)
                                    .TrimEnd('+')
                                    .TrimEndOnce(_strCachedRestrictedSymbol)
                                    .TrimEndOnce(_strCachedForbiddenSymbol);
            string strNumberY = strY?.TrimEnd(_achrCachedNuyenSymbols)
                                    .TrimEnd('+')
                                    .TrimEndOnce(_strCachedRestrictedSymbol)
                                    .TrimEndOnce(_strCachedForbiddenSymbol);
            if (decimal.TryParse(strNumberX, System.Globalization.NumberStyles.Any, GlobalSettings.CultureInfo, out decimal decX))
            {
                if (decimal.TryParse(strNumberY, System.Globalization.NumberStyles.Any, GlobalSettings.CultureInfo, out decimal decY))
                    intCompareResult = decimal.Compare(decX, decY);
                else
                    intCompareResult = -1;
            }
            else if (decimal.TryParse(strNumberY, System.Globalization.NumberStyles.Any, GlobalSettings.CultureInfo, out decimal _))
            {
                intCompareResult = 1;
            }
            else
                intCompareResult = string.Compare(strX, strY, true, GlobalSettings.CultureInfo);

            // Calculate correct return value based on object comparison
            if (_objOrderOfSort == SortOrder.Ascending)
                return intCompareResult;
            return -intCompareResult;
        }

        /// <summary>
        /// Column number to sort on.
        /// </summary>
        public int SortColumn
        {
            get => _intColumnToSort;
            set => _intColumnToSort = value;
        }

        /// <summary>
        /// SortOrder to be used.
        /// </summary>
        public SortOrder Order
        {
            get => _objOrderOfSort;
            set => _objOrderOfSort = value;
        }
    }

    #endregion Sorting Classes
}
