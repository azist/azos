using System;
using System.Linq;
using System.Reflection;

using Azos.Apps;

namespace Azos.Tools.Crun.Logic
{
  /// <summary>
  /// The base command class used to provide command mapping and invocation for inheritors.
  /// </summary>
  public abstract class CmdBase
  {
    /// <summary>Attempts to locate a specific command by command name attribute value.</summary>
    public virtual bool TryGetCommand(string cmdOption, out Cmd cmd)
    {
      cmd = default;
      if (cmdOption.IsNullOrWhiteSpace()) return false;
      cmd = CmdAttribute.GetCommandDictionary(GetType()).FirstOrDefault(x => x.Key.Equals(cmdOption.Trim(), StringComparison.OrdinalIgnoreCase)).Value;
      return cmd != default(Cmd);
    }

    /// <summary>Invokes the matched method from TryGetCommand method</summary>
    public virtual object InvokeCommand(AzosApplication app, MethodInfo method)
      => method.Invoke(this, new[] { app });
  }
}
