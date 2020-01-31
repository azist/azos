/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;

using Azos.Sky;

namespace Azos.Apps.Terminal
{
  /// <summary>
  /// Provides generalization for a "commandlet" - an application terminal command handler.
  /// This base class can execute in a common IApplication and does not require ISkyApplication
  /// </summary>
  public abstract class Cmdlet : DisposableObject
  {
    protected Cmdlet(AppRemoteTerminal terminal, IConfigSectionNode args)
    {
        m_Terminal = terminal;
        m_Args = args;
    }

    protected AppRemoteTerminal m_Terminal;
    protected IConfigSectionNode m_Args;

    public IApplication App => m_Terminal.App;

    /// <summary>
    /// Override to perform actual cmdlet work. Return string result
    /// </summary>
    public abstract string Execute();

    /// <summary>
    /// Override to produce help content
    /// </summary>
    public abstract string GetHelp();
  }

  /// <summary>
  /// Provides base for cmdlets which execute in SkyApplication chassis (e.g. require metabase)
  /// </summary>
  public abstract class SkyAppCmdlet : Cmdlet
  {
    protected SkyAppCmdlet(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args){ }

    public new ISkyApplication App => base.App.AsSky();
  }
}
