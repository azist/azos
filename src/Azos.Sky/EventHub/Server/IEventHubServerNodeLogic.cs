/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;

namespace Azos.Sky.EventHub.Server
{
  /// <summary>
  /// Defines contract for event hub server node
  /// </summary>
  public interface IEventHubServerNodeLogic : IModule
  {
    /// <summary>Write event into nodes queue</summary>
    Task<ChangeResult> WriteAsync(Atom ns, Atom queue, Event evt);

    /// <summary> Fetches from nodes queue </summary>
    Task<IEnumerable<Event>> FetchAsync(Atom ns, Atom queue, ulong checkpoint, int skip, int count, bool onlyid);

    /// <summary> Gets checkpoint </summary>
    Task<ulong> GetCheckpointAsync(Atom ns, Atom queue, string consumer);

    /// <summary>
    /// Sets checkpoint for the specified caller
    /// </summary>
    Task SetCheckpointAsync(Atom ns, Atom queue, string consumer, ulong checkpoint);
  }
}
