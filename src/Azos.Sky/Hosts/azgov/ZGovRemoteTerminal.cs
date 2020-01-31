/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Apps.Terminal;

namespace Azos.Sky.Hosts.azgov
{
  /// <summary>
  /// Implements Sky Zone Governor remote terminal
  /// </summary>
  public class ZGovRemoteTerminal : AppRemoteTerminal
  {
    public ZGovRemoteTerminal() : base() { }

    public override IEnumerable<Type> Cmdlets
    {
      get
      {
        var local = CmdletFinder.FindByNamespace(typeof(ZGovRemoteTerminal), "Azos.Sky.Hosts.azgov.Cmdlets");
        return base.Cmdlets.Concat(CmdletFinder.ZGov).Concat(local);
      }
    }
  }
}