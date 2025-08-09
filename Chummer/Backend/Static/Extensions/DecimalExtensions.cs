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

using System.Runtime.CompilerServices;

namespace Chummer
{
    public static class DecimalExtensions
    {
        /// <summary>
        /// Smallest positive number supported by the decimal type
        /// </summary>
        internal const decimal Epsilon = 0.000000000000000000000000001M;

        /// <summary>
        /// Smallest positive number supported by the double type expressed in the decimal type
        /// </summary>
        internal const decimal DoubleEpsilon = (decimal)double.Epsilon;

        /// <summary>
        /// Syntactic sugar for rounding a decimal away from zero and then converting it to an integer.
        /// </summary>
        /// <param name="decToRound">Decimal to round.</param>
        /// <returns>Rounded integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int StandardRound(this decimal decToRound)
        {
            return decimal.ToInt32(decToRound >= 0 ? decimal.Ceiling(decToRound) : decimal.Floor(decToRound));
        }

        /// <summary>
        /// Syntactic sugar for applying ToInt32 conversion to a decimal (rounds towards zero).
        /// </summary>
        /// <param name="decIn">Decimal to convert.</param>
        /// <returns>Rounded integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ToInt32(this decimal decIn)
        {
            return decimal.ToInt32(decIn);
        }
    }
}
