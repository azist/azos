/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Security;

namespace Azos.Apps.Terminal.Cmdlets
{
  [SystemAdministratorPermission(AccessLevel.ADVANCED)]
  public sealed class Conf : Cmdlet
  {
    public const string CONFIG_PATH_ATTR = "path";
    public const string CONFIG_EVAL_ATTR = "eval";
    public const string CONFIG_SAFE_SCOPE_ATTR = "safe-scope";

    public Conf(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
    {
    }

    public override string Execute()
    {
      var cfg = App.ConfigRoot;

      var path = m_Args.ValOf(CONFIG_PATH_ATTR);
      var eval = m_Args.Of(CONFIG_EVAL_ATTR).VerbatimValue;
      var safeScope = m_Args.Of(CONFIG_SAFE_SCOPE_ATTR).ValueAsBool(false);
      SecurityFlowScope scope = null;
      if (safeScope)
      {
        scope = new SecurityFlowScope(TheSafe.SAFE_ACCESS_FLAG);
      }
      try
      {
        if (path.IsNotNullOrWhiteSpace())
        {
          cfg = cfg.NavigateSection(path);
        }

        if (eval.IsNotNullOrWhiteSpace())
        {
          var evalResult = cfg.EvaluateValueVariables(eval, true);
          return evalResult;
        }

        var cfgResult = cfg.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.PrettyPrint);
        return cfgResult;
      }
      finally
      {
        DisposeAndNull(ref scope);
      }
    }

    public override string GetHelp()
    {
      return
@"Fetches current process configuration tree.
      Parameters:
      <f color=yellow>path=string<f color=gray> - optional section path within config tree
      <f color=yellow>eval=string<f color=gray> - optional variable expression evaluated as of path or root
      <f color=yellow>safe-scope=bool<f color=gray> - pass true to set SecurityFlowScope(TheSafe.SAFE_ACCESS_FLAG) for TheSafe access
 <f color=magenta>Example:
   <f color=green> conf{ path='/log/' }
   <f color=green> conf{ path='/machine/test' eval='$(::decipher value=""base64:hhu78sgdDvz6WgfowGW-9MioPf"" algo=""mykey"")'}

   <f color=cyan> NOTE:
    Requires  <f color=red>SystemAdministratorPermission(AccessLevel.ADVANCED)<f color=cyan> grant
";
    }
  }
}
