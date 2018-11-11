using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Sky.Apps.Terminal;

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