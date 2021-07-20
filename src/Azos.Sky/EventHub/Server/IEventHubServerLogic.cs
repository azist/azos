/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;

namespace Azos.Sky.EventHub.Server
{
  public interface IEventHubServerLogic : IModule
  {
    Task<ChangeResult> WriteAsync(Atom ns, Atom queue, Event evt);
  }
}
