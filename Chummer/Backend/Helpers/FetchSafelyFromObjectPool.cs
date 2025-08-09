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
using Microsoft.Extensions.ObjectPool;

namespace Chummer
{
    /// <summary>
    /// Syntactic Sugar for wrapping a ObjectPool{T}'s Get() and Return() methods into something that hooks into `using`
    /// and that guarantees that pooled objects will be returned
    /// </summary>
    public readonly struct FetchSafelyFromObjectPool<T> : IDisposable where T : class
    {
        private readonly ObjectPool<T> _objMyPool;
        private readonly T _objMyValue;

        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FetchSafelyFromObjectPool(ObjectPool<T> objMyPool, out T objReturn)
        {
            _objMyPool = objMyPool;
            objReturn = _objMyValue = objMyPool.Get();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _objMyPool?.Return(_objMyValue);
        }
    }
}
