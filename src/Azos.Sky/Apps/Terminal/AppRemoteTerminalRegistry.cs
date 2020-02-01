/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using Azos.Apps.Injection;
using Azos.Conf;
using Azos.Collections;
using Azos.Data;
using Azos.Security;

using Azos.Sky.Contracts;
using Azos.Sky.Security.Permissions.Admin;


namespace Azos.Apps.Terminal
{
  /// <summary>
  /// Internal framework registry of AppRemoteTerminal
  /// </summary>
  internal sealed class AppRemoteTerminalRegistry
  {
    private AppRemoteTerminalRegistry() { }

    private Registry<AppRemoteTerminal> m_Registry = new Registry<AppRemoteTerminal>();

    /// <summary>
    /// Returns singleton instance of AppRemoteTerminalRegistry per application
    /// </summary>
    public static AppRemoteTerminalRegistry Instance(IApplication app)
      => app.NonNull(nameof(app))
            .Singletons
            .GetOrCreate( () => new AppRemoteTerminalRegistry() )
            .instance;

    public IEnumerable<AppRemoteTerminal> All => m_Registry;

    public static ulong GenerateId() => FID.Generate().ID;


    public bool Register(AppRemoteTerminal term) => m_Registry.Register(term);
    public bool Unregister(AppRemoteTerminal term) => m_Registry.Unregister(term);
  }

}
