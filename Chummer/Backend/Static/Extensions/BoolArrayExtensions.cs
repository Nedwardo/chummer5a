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
using System.Runtime.CompilerServices;

namespace Chummer
{
    public static class BoolArrayExtensions
    {
        /// <summary>
        /// Get the first element in a bool array that matches <paramref name="blnValue"/>.
        /// </summary>
        /// <param name="ablnArray">Array to search.</param>
        /// <param name="blnValue">Value for which to look.</param>
        /// <param name="intFrom">Index from which to start search (inclusive).</param>
        /// <param name="intTo">Index at which to end search (exclusive).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FirstMatching(this bool[] ablnArray, bool blnValue, int intFrom = 0, int intTo = int.MaxValue)
        {
            if (ablnArray == null)
                throw new ArgumentNullException(nameof(ablnArray));
            if (intTo > ablnArray.Length)
                intTo = ablnArray.Length;
            if (intFrom < 0)
                intFrom = 0;
            for (; intFrom < intTo; ++intFrom)
            {
                if (ablnArray[intFrom] == blnValue)
                    return intFrom;
            }
            return -1;
        }
    }
}
