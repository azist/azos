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

using Azos.Sky;
using Azos.Sky.Contracts;
using Azos.Sky.Security.Permissions.Admin;


namespace Azos.Apps.Terminal
{
  /// <summary>
  /// Provides base for terminal application management capabilities. This is a Glue server
  /// </summary>
  [Serializable]
  public class AppRemoteTerminal : DisposableObject, IRemoteTerminal, INamed, IDeserializationCallback, IConfigurable
  {
    /// <summary>
    /// A string pragma which indicates that the returned content is markup (contains color and formatting information)
    /// </summary>
    public const string MARKUP_PRAGMA = "<!-- ###MARKUP### -->";

    public const string CONFIG_APP_REMOTE_TERMINAL_SECTION = "remote-terminal";

    public const string HELP_ARGS = "/?";

    /// <summary>
    /// Makes an instance of remote terminal which is configured under app/remote-terminal section.
    /// If section is not defined then makes AppRemoteTerminal instance
    /// </summary>
    public static AppRemoteTerminal MakeNewTerminal(IApplication app)
    {
      var result = FactoryUtils.MakeAndConfigure<AppRemoteTerminal>(app.ConfigRoot[CONFIG_APP_REMOTE_TERMINAL_SECTION], typeof(AppRemoteTerminal));
      app.InjectInto(result);
      return result;
    }

    public AppRemoteTerminal()
    {
      m_ID = AppRemoteTerminalRegistry.GenerateId();

      m_Name = new ELink((ulong)m_ID, null).Link;
      m_Vars = new Vars();
      m_ScriptRunner = new ScriptRunner();
    }

    protected override void Destructor()
    {
      AppRemoteTerminalRegistry.Unregister(this);
      base.Destructor();
    }

    public void OnDeserialization(object sender) => AppRemoteTerminalRegistry.Register(this);

#pragma warning disable 649
    [Inject] IApplication m_App;
#pragma warning restore 649

    private ulong m_ID;
    private string m_Name;
    private string m_Who;
    private DateTime m_WhenConnected;
    private DateTime m_WhenInteracted;
    private Vars m_Vars;
    private ScriptRunner m_ScriptRunner;


    /// <summary>
    /// Application chassis context for this terminal session
    /// </summary>
    public IApplication App => m_App.NonNull(nameof(m_App));

    /// <summary>
    /// Returns unique terminal session ID
    /// </summary>
    public ulong ID { get { return m_ID; } }

    /// <summary>
    /// Returns unique terminal session ID as an alpha link
    /// </summary>
    public string Name { get { return m_Name; } }

    /// <summary>
    /// Returns description about who is connected
    /// </summary>
    public string Who { get { return m_Who; } }

    /// <summary>
    /// Returns UTC timestamp of connection initiation
    /// </summary>
    public DateTime WhenConnected { get { return m_WhenConnected; } }


    /// <summary>
    /// Returns UTC timestamp of last interaction
    /// </summary>
    public DateTime WhenInteracted { get { return m_WhenInteracted; } }


    /// <summary>
    /// Provides cmdlets
    /// </summary>
    public virtual IEnumerable<Type> Cmdlets { get { return CmdletFinder.Common; } }

    /// <summary>
    /// Provides variable resolver
    /// </summary>
    public virtual Vars Vars { get { return m_Vars; } }

    public void Configure(IConfigSectionNode fromNode)
    {
      m_ScriptRunner.Configure(fromNode[ScriptRunner.CONFIG_SCRIPT_RUNNER_SECTION]);
      ConfigAttribute.Apply(this, fromNode);
      DoConfigure(fromNode);
    }

    public virtual RemoteTerminalInfo Connect(string who)
    {
      m_Who = who ?? Sky.SysConsts.UNKNOWN_ENTITY;
      var now = App.TimeSource.UTCNow;
      m_WhenConnected = now;
      m_WhenInteracted = now;

      AppRemoteTerminalRegistry.Register(this);

      return new RemoteTerminalInfo
      {
        TerminalName = Name,
        WelcomeMsg = "Connected to '[{0}]{1}'@'{2}' on {3:G} {4:T} UTC. Session '{5}'".Args(App.AppId.IsZero ? "#" : App.AppId.Value,
                                                                     App.Name,
                                                                     App.GetThisHostName(),
                                                                     App.TimeSource.Now,
                                                                     App.TimeSource.UTCNow,
                                                                     Name),
        Host = App.GetThisHostName(),
        AppName = App.Name,
        ServerLocalTime = App.TimeSource.Now,
        ServerUTCTime = App.TimeSource.UTCNow
      };
    }

    [AppRemoteTerminalPermission]
    public virtual string Execute(string command)
    {
      m_WhenInteracted = App.TimeSource.UTCNow;

      if (command == null) return string.Empty;

      command = command.Trim();

      if (command.IsNullOrWhiteSpace()) return string.Empty;

      if (!command.EndsWith("}") && command.Contains(HELP_ARGS))
      {
        return getHelp(command.Replace(HELP_ARGS,"").Trim());
      }

      if (!command.EndsWith("}")) command += "{}";

      var cmd = Sky.TerminalUtils.ParseCommand(command, m_Vars);
      var result = new MemoryConfiguration();
      result.EnvironmentVarResolver = cmd.EnvironmentVarResolver;
      m_ScriptRunner.Execute(cmd, result);

      return DoExecute(result.Root);
    }

    public string Execute(IConfigSectionNode command)
    {
      if(command==null || !command.Exists) return string.Empty;
      return DoExecute(command);
    }

    public virtual string Disconnect()
    {
      return "Good bye!";
    }

    protected virtual void DoConfigure(IConfigSectionNode fromNode) {}

    protected virtual string DoExecute(IConfigSectionNode command)
    {
      var cname = command.Name.ToLowerInvariant();

      var tp = Cmdlets.FirstOrDefault(cmd => cmd.Name.EqualsIgnoreCase(cname));

      if (tp == null)
        return Sky.StringConsts.RT_CMDLET_DONTKNOW_ERROR.Args(cname);

      //Check cmdlet security
      Permission.AuthorizeAndGuardAction(App, tp);
      Permission.AuthorizeAndGuardAction(App, tp.GetMethod(nameof(Execute)));

      var cmdlet = Activator.CreateInstance(tp, this, command) as Cmdlet;
      if (cmdlet == null)
        throw new SkyException(Sky.StringConsts.RT_CMDLET_ACTIVATION_ERROR.Args(cname));

      using (cmdlet)
      {
        App.DependencyInjector.InjectInto(cmdlet);
        return cmdlet.Execute();
      }
    }

    private string getHelp(string cmd)
    {
      if (cmd.IsNullOrEmpty()) return string.Empty;

      var target = Cmdlets.FirstOrDefault(c => c.Name.EqualsOrdIgnoreCase(cmd));
      if (target == null)  return string.Empty;

      var result = new StringBuilder(1024);
      result.AppendLine(AppRemoteTerminal.MARKUP_PRAGMA);
      result.AppendLine("<push>");

      string help;
      using (var inst = Activator.CreateInstance(target, this, null) as Cmdlet)
      {
        try
        {
          App.DependencyInjector.InjectInto(inst);
          help = inst.GetHelp();
        }
        catch (Exception error)
        {
          help = "Error getting help: " + error.ToMessageWithType();
        }
        result.AppendLine("<f color=white><j width=8 dir=right text=\"{0}\"><f color=gray> - {1}".Args(cmd, help));
      }
      result.AppendLine("<pop>");

      return result.ToString();
    }
  }
}
