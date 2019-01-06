/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Sky.Apps.Terminal;

namespace Azos.Sky.Hosts.ahgov
{
  /// <summary>
  /// Implements Sky Host Governor remote terminal
  /// </summary>
  public class HGovRemoteTerminal : AppRemoteTerminal
  {
    public HGovRemoteTerminal() : base()
    {

    }

    public override IEnumerable<Type> Cmdlets
    {
      get
      {
        var local = CmdletFinder.FindByNamespace(typeof(HGovRemoteTerminal), "Azos.Sky.Hosts.ahgov.Cmdlets");
        return base.Cmdlets.Concat(CmdletFinder.HGov).Concat(local);
      }
    }
  }
}
