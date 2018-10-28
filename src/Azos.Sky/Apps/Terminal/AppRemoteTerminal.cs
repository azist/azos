/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;

using Azos.Apps;
using Azos.Security;
using Azos.Conf;

using Azos.Contracts;
using Azos.Security.Permissions.Admin;


namespace Azos.Sky.Apps.Terminal
{

  /// <summary>
  /// Provides basic app-management capabilities
  /// </summary>
  [Serializable]
  public class AppRemoteTerminal : ApplicationComponent, IRemoteTerminal, INamed, IDeserializationCallback, IConfigurable
  {
    public const string MARKUP_PRAGMA = "<!-- ###MARKUP### -->";

    public const string CONFIG_APP_REMOTE_TERMINAL_SECTION = "remote-terminal";

    public const string HELP_ARGS = "/?";


    private static object s_IDLock = new Object();
    private static int s_ID;
    internal static Registry<AppRemoteTerminal> s_Registry = new Registry<AppRemoteTerminal>();


    /// <summary>
    /// Makes an instance of remote terminal which is configured under app/remote-terminal section.
    /// If section is not defined then makes AppRemoteTerminal instance
    /// </summary>
    public static AppRemoteTerminal MakeNewTerminal()
    {
      return FactoryUtils.MakeAndConfigure<AppRemoteTerminal>(App.ConfigRoot[CONFIG_APP_REMOTE_TERMINAL_SECTION], typeof(AppRemoteTerminal));
    }


    public AppRemoteTerminal() : base()
    {
      lock (s_IDLock)
      {
        s_ID++;
        m_ID = s_ID;
      }
      m_Name = new ELink((ulong)m_ID, null).Link;
      m_Vars = new Vars();
      m_ScriptRunner = new ScriptRunner();
    }



    protected override void Destructor()
    {
      s_Registry.Unregister(this);
      base.Destructor();
    }

    public void OnDeserialization(object sender)
    {
      lock (s_IDLock)
      {
        if (this.m_ID > s_ID) s_ID = m_ID;
      }
      s_Registry.Register(this);
    }

    private int m_ID;
    private string m_Name;
    private string m_Who;
    private DateTime m_WhenConnected;
    private DateTime m_WhenInteracted;
    private Vars m_Vars;
    private ScriptRunner m_ScriptRunner;

    public override string ComponentCommonName { get { return "term-" + m_Name; } }

    /// <summary>
    /// Returns unique terminal session ID
    /// </summary>
    public int ID { get { return m_ID; } }

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
      m_Who = who ?? SysConsts.UNKNOWN_ENTITY;
      m_WhenConnected = App.TimeSource.UTCNow;
      m_WhenInteracted = App.TimeSource.UTCNow;

      s_Registry.Register(this);

      return new RemoteTerminalInfo
      {
        TerminalName = Name,
        WelcomeMsg = "Connected to '[{0}]{1}'@'{2}' on {3:G} {4:T} UTC. Session '{5}'".Args(SkySystem.MetabaseApplicationName,
                                                                     App.Name,
                                                                     SkySystem.HostName,
                                                                     App.TimeSource.Now,
                                                                     App.TimeSource.UTCNow,
                                                                     Name),
        Host = SkySystem.HostName,
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

      var cmd = TerminalUtils.ParseCommand(command, m_Vars);
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
        return StringConsts.RT_CMDLET_DONTKNOW_ERROR.Args(cname);

      //Check cmdlet security
      Permission.AuthorizeAndGuardAction(tp);
      Permission.AuthorizeAndGuardAction(tp.GetMethod(nameof(Execute)));

      var cmdlet = Activator.CreateInstance(tp, this, command) as Cmdlet;
      if (cmdlet == null)
        throw new SkyException(StringConsts.RT_CMDLET_ACTIVATION_ERROR.Args(cname));

      using (cmdlet)
      {
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
