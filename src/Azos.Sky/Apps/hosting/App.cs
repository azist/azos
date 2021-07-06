/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Net.Sockets;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.IO.Sipc;
using Azos.Log;

namespace Azos.Apps.Hosting
{
  /// <summary>
  /// Represents an instance of managed application process
  /// </summary>
  public class App : INamed, IOrdered
  {
    public App(IConfigSectionNode cfg)
    {

    }

    public string Name => throw new NotImplementedException();
    public int Order => throw new NotImplementedException();

    /// <summary>
    /// Assigned by activator
    /// </summary>
    public IAppActivatorContext ActivationContext { get; set; }

  }
}
