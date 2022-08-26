/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Apps.Terminal;

namespace Azos.Apps.Terminal
{
  /// <summary>
  /// Implements base for sky-specific app terminals
  /// </summary>
  public abstract class SkyAppRemoteTerminal : AppRemoteTerminal
  {
    public SkyAppRemoteTerminal() : base()
    {
    }

    public override IEnumerable<Type> Cmdlets => CmdletFinder.Common;
  }
}
