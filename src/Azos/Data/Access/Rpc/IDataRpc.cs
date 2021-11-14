/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;

namespace Azos.Data.Access.Rpc
{
  /// <summary>
  /// Implements a data access Remote Procedure Call handler module
  /// </summary>
  public interface IRpcHandler : IModule
  {
    /// <summary>
    /// Executes a single data fetch RPC request against the `Session.DataContext` yielding a Doc resultset
    /// </summary>
    Task<IEnumerable<Doc>> ReadAsync(ReadRequest request);

    /// <summary>
    /// Executes a transactional RPC request against the `Session.DataContext`
    /// </summary>
    Task<IEnumerable<ChangeResult>> TransactAsync(TransactRequest request);
  }
}
