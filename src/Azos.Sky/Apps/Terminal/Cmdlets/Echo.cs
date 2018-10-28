
using Azos.Conf;

namespace Azos.Sky.Apps.Terminal.Cmdlets
{
  /// <summary>
  /// Echo text
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
      return @"Echo text";
    }
  }
}