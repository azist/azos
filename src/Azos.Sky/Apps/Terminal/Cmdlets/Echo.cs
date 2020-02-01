/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;

namespace Azos.Apps.Terminal.Cmdlets
{
  /// <summary>
  /// Echoes text
  /// </summary>
  public class Echo: Cmdlet
  {
    public Echo(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args) { }

    public override string Execute()
    {
      return m_Args.ValueAsString();
    }

    public override string GetHelp()
    {
      return @"Echoes text";
    }
  }
}