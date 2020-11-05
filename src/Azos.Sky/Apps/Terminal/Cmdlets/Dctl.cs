/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Linq;
using System.Text;

using Azos.Apps;
using Azos.Conf;
using Azos.Security;

namespace Azos.Apps.Terminal.Cmdlets
{
  /// <summary>
  /// Daemon control
  /// </summary>
  [SystemAdministratorPermission(AccessLevel.ADVANCED)]
  public class Dctl : Cmdlet
  {
    public enum Action
    {
      Undefined = 0,
      Start = 1 ,
      SignalStop = 2,
      Stop = 3,
      Wait = Stop,
      Config = 4,
      Cfg = Config,
      Configure = Config
    };

    public const string CONFIG_ACTION_ATTR = "action";
    public const string CONFIG_ROOT_SECT = "root";

    public Dctl(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
    {
    }

    public override string Execute()
    {
      var daemon = CMan.GetApplicationComponentBySIDorName(App, m_Args) as IDaemon;

      if (daemon==null)
        return "The specified component is not a IDaemon";

      var action = m_Args.AttrByName(CONFIG_ACTION_ATTR).ValueAsEnum<Action>();

      switch (action)
      {
        case Action.Start:      { daemon.Start();               return "`{0}` started".Args(daemon); }
        case Action.SignalStop: { daemon.SignalStop();          return "`{0}` signaled to stop".Args(daemon); }
        case Action.Stop:       { daemon.WaitForCompleteStop(); return "`{0}` stopped".Args(daemon); }
        case Action.Config:
        {
          var cdaemon = daemon as IConfigurable;
          if (cdaemon == null) return "This daemon is not IConfigurable";

          var croot = m_Args[CONFIG_ROOT_SECT];

          cdaemon.Configure(croot);

          return "`{0}` configured".Args(daemon); ;
        }
      }

      return "Requested daemon action `{0}` is unsupported".Args(action);
    }

    public override string GetHelp()
    {
return
@"Controls an IDaemon component by applying ctrl actions such as Start/Stop/Config
  Parameters:
      <f color=yellow>sid=id<f color=gray> - IDaemon component SID
        or
      <f color=yellow>name=name<f color=gray> - IDaemon component common name


      <f color=yellow>act=action<f color=gray> - one of: Start | SignalStop | Stop | Config
      <f color=yellow>root<f color=gray> - optional config section root
 <f color=magenta>Example:
  <f color=green> dctl{ sid=123 action=config  root{ a=1 b=2 }}

   <f color=cyan> NOTE:
    Requires  <f color=red>SystemAdministratorPermission(AccessLevel.ADVANCED)<f color=cyan> grant 
";
    }
  }

}
