/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Apps.Terminal;

namespace Azos.Apps.Hosting
{
  /// <summary>
  /// Implements GovernorDaemon remote terminal
  /// </summary>
  public class GovernorDaemonRemoteTerminal : AppRemoteTerminal
  {
    public GovernorDaemonRemoteTerminal() : base() { }

    public override IEnumerable<Type> Cmdlets
    {
      get
      {
        var local = CmdletFinder.FindByNamespace(typeof(GovernorDaemonRemoteTerminal), "Azos.Apps.Hosting.Cmdlets");
        return base.Cmdlets.Concat(local);
      }
    }
  }
}
