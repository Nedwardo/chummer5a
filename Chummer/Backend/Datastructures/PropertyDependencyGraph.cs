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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    /// <summary>
    /// Special version of <see cref="DependencyGraph{string, T}"/> that is made explicitly for collecting property dependencies.
    /// The fact that all dependencies are stored as strings allows for a few extra optimizations to be used.
    /// </summary>
    public class PropertyDependencyGraph<T> : DependencyGraph<string, T>
    {
        /// <summary>
        /// Initializes a directed graph of dependent items based on a blueprint specified in the constructor.
        /// </summary>
        /// <param name="lstGraphNodes">Blueprints of nodes that should be followed when constructing the DependencyGraph. Make sure you use the correct DependencyGraphNode constructor!</param>
        public PropertyDependencyGraph(params DependencyGraphNode<string, T>[] lstGraphNodes) : base(lstGraphNodes)
        {
        }

        /// <summary>
        /// Returns a collection containing the current key's object and all objects that depend on the current key.
        /// Slower but idiot-proof compared to GetWithAllDependentsUnsafe().
        /// NOTE: If possible, use the override method that explicitly states whether to use the HashSet{string} pool for creating the return value or not.
        /// </summary>
        /// <param name="objParentInstance">Instance of the object whose dependencies are being processed, used for conditions.</param>
        /// <param name="objKey">Fetch the node associated with this object.</param>
        public override HashSet<string> GetWithAllDependents(T objParentInstance, string objKey)
        {
            return GetWithAllDependents(objParentInstance, objKey, false);
        }

        /// <summary>
        /// Returns a collection containing the current key's object and all objects that depend on the current key.
        /// Slower but idiot-proof compared to GetWithAllDependentsUnsafe().
        /// NOTE: If possible, use the override method that explicitly states whether to use the HashSet{string} pool for creating the return value or not.
        /// </summary>
        /// <param name="objParentInstance">Instance of the object whose dependencies are being processed, used for conditions.</param>
        /// <param name="objKey">Fetch the node associated with this object.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public override Task<HashSet<string>> GetWithAllDependentsAsync(T objParentInstance, string objKey, CancellationToken token = default)
        {
            return GetWithAllDependentsAsync(objParentInstance, objKey, false, token);
        }

        /// <summary>
        /// Returns a collection containing the current key's object and all objects that depend on the current key.
        /// Slower but idiot-proof compared to GetWithAllDependentsUnsafe().
        /// </summary>
        /// <param name="objParentInstance">Instance of the object whose dependencies are being processed, used for conditions.</param>
        /// <param name="objKey">Fetch the node associated with this object.</param>
        /// <param name="blnUsePool">Whether to fetch the returned HashSet{string} from a pool or to just create a new one on the stack.</param>
        public HashSet<string> GetWithAllDependents(T objParentInstance, string objKey, bool blnUsePool)
        {
            HashSet<string> setReturn = blnUsePool ? Utils.StringHashSetPool.Get() : new HashSet<string>();
            try
            {
                if (NodeDictionary.TryGetValue(objKey, out DependencyGraphNode<string, T> objLoopNode))
                {
                    if (setReturn.Add(objLoopNode.MyObject))
                    {
                        foreach (DependencyGraphNodeWithCondition<string, T> objNode in objLoopNode.UpStreamNodes)
                        {
                            if (objNode.DependencyCondition?.Invoke(objParentInstance) != false)
                            {
                                CollectDependents(objParentInstance, objNode.Node.MyObject, setReturn);
                            }
                        }
                    }
                }
                else
                    setReturn.Add(objKey);

                return setReturn;
            }
            catch
            {
                if (blnUsePool)
                    Utils.StringHashSetPool.Return(ref setReturn);
                throw;
            }
        }

        /// <summary>
        /// Returns a collection containing the current key's object and all objects that depend on the current key.
        /// Slower but idiot-proof compared to GetWithAllDependentsUnsafe().
        /// </summary>
        /// <param name="objParentInstance">Instance of the object whose dependencies are being processed, used for conditions.</param>
        /// <param name="objKey">Fetch the node associated with this object.</param>
        /// <param name="blnUsePool">Whether to fetch the returned HashSet{string} from a pool or to just create a new one on the stack.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task<HashSet<string>> GetWithAllDependentsAsync(T objParentInstance, string objKey, bool blnUsePool, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            HashSet<string> setReturn = blnUsePool ? Utils.StringHashSetPool.Get() : new HashSet<string>();
            try
            {
                if (NodeDictionary.TryGetValue(objKey, out DependencyGraphNode<string, T> objLoopNode))
                {
                    if (setReturn.Add(objLoopNode.MyObject))
                    {
                        foreach (DependencyGraphNodeWithCondition<string, T> objNode in objLoopNode.UpStreamNodes)
                        {
                            Func<T, CancellationToken, Task<bool>> objCondition = objNode.DependencyConditionAsync;
                            if (objCondition == null || await objCondition(objParentInstance, token).ConfigureAwait(false))
                            {
                                await CollectDependentsAsync(objParentInstance, objNode.Node.MyObject, setReturn, token).ConfigureAwait(false);
                            }
                        }
                    }
                }
                else
                    setReturn.Add(objKey);

                return setReturn;
            }
            catch
            {
                if (blnUsePool)
                    Utils.StringHashSetPool.Return(ref setReturn);
                throw;
            }
        }

        /// <summary>
        /// Returns a once-enumerable collection containing the current key's object and all objects that depend on the current key.
        /// Slower but idiot-proof compared to GetWithAllDependentsUnsafe().
        /// Automatically makes use of pooling.
        /// </summary>
        /// <param name="objParentInstance">Instance of the object whose dependencies are being processed, used for conditions.</param>
        /// <param name="objKey">Fetch the node associated with this object.</param>
        public IEnumerable<string> GetWithAllDependentsEnumerable(T objParentInstance, string objKey)
        {
            HashSet<string> setReturn = GetWithAllDependents(objParentInstance, objKey, true);
            try
            {
                foreach (string strReturn in setReturn)
                    yield return strReturn;
            }
            finally
            {
                Utils.StringHashSetPool.Return(ref setReturn);
            }
        }

        /// <summary>
        /// Returns a once-enumerable collection containing the current key's object and all objects that depend on the current key.
        /// Slower but idiot-proof compared to GetWithAllDependentsUnsafe().
        /// Automatically makes use of pooling.
        /// </summary>
        /// <param name="objParentInstance">Instance of the object whose dependencies are being processed, used for conditions.</param>
        /// <param name="objKey">Fetch the node associated with this object.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task<IEnumerable<string>> GetWithAllDependentsEnumerableAsync(T objParentInstance, string objKey, CancellationToken token = default)
        {
            HashSet<string> setReturn = await GetWithAllDependentsAsync(objParentInstance, objKey, true, token).ConfigureAwait(false);
            try
            {
                return setReturn.ToList();
            }
            finally
            {
                Utils.StringHashSetPool.Return(ref setReturn);
            }
        }
    }
}
