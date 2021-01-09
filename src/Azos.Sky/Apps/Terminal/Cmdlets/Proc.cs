/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Text;

using Azos.Apps;
using Azos.Conf;
using Azos.Glue;
using Azos.Sky.Workers;

namespace Azos.Apps.Terminal.Cmdlets
{
  /// <summary>
  /// Process Manager
  /// </summary>
  public sealed class Proc : SkyAppCmdlet
  {
    public const string CONFIG_ALLOC_SECTION    = "alloc";
    public const string CONFIG_SPAWN_SECTION    = "spawn";
    public const string CONFIG_DISPATCH_SECTION = "dispatch";
    public const string CONFIG_ENQUEUE_SECTION  = "enqueue";
    public const string CONFIG_LIST_SECTION     = "list";

    public const string CONFIG_PROC_SECTION     = "proc";
    public const string CONFIG_SIGNAL_SECTION   = "signal";
    public const string CONFIG_TODO_SECTION     = "todo";

    public const string CONFIG_PID_ATTR  = "pid";
    public const string CONFIG_ZONE_ATTR = "zone";
    public const string CONFIG_MUTEX_ATTR = "mutex";
    public const string CONFIG_HOSTSET_ATTR = "hostset";
    public const string CONFIG_SVC_ATTR = "svc";

    public Proc(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args) { }

    public override string Execute()
    {
      var sb = new StringBuilder();

      var any = false;
      foreach (var arg in m_Args.Children)
      {
        sb.AppendLine(execute(arg));
        any = true;
      }
      if (!any) sb.AppendLine(list(null));
      return sb.ToString(0, sb.Length - Environment.NewLine.Length);
    }

    private string execute(IConfigSectionNode arg)
    {
      if (arg.IsSameName(CONFIG_ALLOC_SECTION))    return alloc(arg);
      if (arg.IsSameName(CONFIG_SPAWN_SECTION))    return spawn(arg);
      if (arg.IsSameName(CONFIG_DISPATCH_SECTION)) return dispatch(arg);
      if (arg.IsSameName(CONFIG_ENQUEUE_SECTION))  return enqueue(arg);
      if (arg.IsSameName(CONFIG_LIST_SECTION))     return list(arg);
      return string.Empty;
    }

    private string alloc(IConfigSectionNode arg)
    {
      var zone = arg.AttrByName(CONFIG_ZONE_ATTR).ValueAsString();
      var mutex = arg.AttrByName(CONFIG_MUTEX_ATTR).ValueAsString();

      var pid = mutex.IsNullOrWhiteSpace()
        ? App.ProcessManager.Allocate(zone)
        : App.ProcessManager.AllocateMutex(zone, mutex);

      return pid.ToString();
    }

    private string spawn(IConfigSectionNode arg)
    {
      var pid = PID.Parse(arg.AttrByName(CONFIG_PID_ATTR).ValueAsString());
      var proc = arg[CONFIG_PROC_SECTION];
      App.ProcessManager.Spawn(pid, proc);
      return "OK";
    }

    private string dispatch(IConfigSectionNode arg)
    {
      var pid = PID.Parse(arg.AttrByName(CONFIG_PID_ATTR).ValueAsString());
      var signal = arg[CONFIG_SIGNAL_SECTION];
      var result = App.ProcessManager.Dispatch(pid, signal);
      return result.ToString();
    }

    private string enqueue(IConfigSectionNode arg)
    {
      var hostSet= arg.AttrByName(CONFIG_HOSTSET_ATTR).ValueAsString();
      var svc = arg.AttrByName(CONFIG_SVC_ATTR).ValueAsString();

      var todo = arg[CONFIG_TODO_SECTION];
      App.ProcessManager.Enqueue(hostSet, svc, todo);
      return "OK";
    }

    private string list(IConfigSectionNode arg)
    {
      var sb = new StringBuilder();
      var zone = arg.AttrByName(CONFIG_ZONE_ATTR).ValueAsString();
      foreach(var process in App.ProcessManager.List(zone, arg))
        sb.AppendFormatLine("{0}", process.PID);
      return sb.ToString();
    }

    public override string GetHelp()
    {
      return @"Process Manager";
    }
  }
}