/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Diagnostics;
using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.IO.Sipc;
using Azos.Log;
using Azos.Serialization.JSON;

namespace Azos.Apps.Hosting
{
  /// <summary>
  /// Marker interface for objects which keep activator-specific state (such as process, or container instance handle)
  /// per application
  /// </summary>
  public interface IAppActivatorContext
  {

  }

  public interface IAppActivator : IApplicationComponent
  {
    /// <summary>
    /// Returns true if application was started, false if it is already running
    /// </summary>
    bool StartApplication(App app);

    /// <summary>
    /// Returns true if application was stopped, false if it was already stopped
    /// </summary>
    bool StopApplication(App app);
  }



  /// <summary>
  /// Provides an abstraction for activation/deactivation of application process.
  /// The default implementation uses System.Process to run/terminate applications
  /// You can create another activator which spawns apps in their on container runtime, such as Docker
  /// </summary>
  public sealed class ProcessAppActivator : ApplicationComponent<GovernorDaemon>, IAppActivator
  {
    public const string CONFIG_START_EXE_PATH_ATTR = "exe-path";
    public const string CONFIG_START_EXE_NAME_ATTR = "exe-name";
    public const string CONFIG_START_EXE_ARGS_ATTR = "exe-args";


    //https://developers.redhat.com/blog/2019/10/29/the-net-process-class-on-linux#processname
    public sealed class Context : IAppActivatorContext
    {
      public Context(Process process) => Process = process;
      public readonly Process Process;
    }

    public ProcessAppActivator(GovernorDaemon gov, IConfigSectionNode cfg) : base(gov)
    {
      ConfigAttribute.Apply(this, cfg);
    }

    public override string ComponentLogTopic => Sky.SysConsts.LOG_TOPIC_HOST_GOV;

    public bool StartApplication(App app)
    {
      var rel = Guid.NewGuid();
      var ctx = app.ActivationContext as Context;
      if (ctx != null) return false;//already running

      WriteLogFromHere(MessageType.Trace, "Initiating app `{0}` start".Args(app.Name), related: rel);

      var rootExeDir = app.StartSection.ValOf(CONFIG_START_EXE_PATH_ATTR);
      var exeFile = app.StartSection.ValOf(CONFIG_START_EXE_NAME_ATTR);
      var exeArgs = app.StartSection.ValOf(CONFIG_START_EXE_ARGS_ATTR);

      var exeFullPath = System.IO.Path.Combine(rootExeDir, exeFile);

      WriteLogFromHere(MessageType.Trace, "Set directories".Args(app.Name), related: rel, pars: (new {rootExeDir, exeFile, exeArgs }).ToJson());

      var process = new Process();
      process.StartInfo.FileName = exeFullPath;
      process.StartInfo.WorkingDirectory = rootExeDir;
      process.StartInfo.Arguments = exeArgs;//todo: In .Net 5+ use ArgumentList instead which properly handles platform specific escapes
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.RedirectStandardInput = false;
      process.StartInfo.RedirectStandardOutput = false;
      process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

      WriteLogFromHere(MessageType.Trace, "Attemp process start".Args(app.Name), related: rel);
      try
      {
        process.Start();
        WriteLogFromHere(MessageType.Trace, "Started");
      }
      catch(Exception error)
      {
        WriteLogFromHere(MessageType.Critical, "process.Start() leaked: " + error.ToMessageWithType(), error, related: rel);
      }

      //poll so it has not crashed
      app.ActivationContext = new Context(process);//link context
      return true;
    }

    public bool StopApplication(App app)
    {
      var rel = Guid.NewGuid();
      var ctx = app.ActivationContext as Context;
      if (ctx == null) return false;//already stopped
      var process = ctx.Process;

      app.ActivationContext = null;

      WriteLogFromHere(MessageType.Trace, "Initiating app `{0}` stop".Args(app.Name), related: rel);

      try
      {
        var sipc = app.Connection;
        if (sipc != null)
        {
          app.Connection.Send(Protocol.CMD_STOP);
        }
      }
      catch(Exception error)
      {
        WriteLogFromHere(MessageType.Error, "sipc.Send(STOP) leaked: " + error.ToMessageWithType(), error, related: rel);
      }

      WriteLogFromHere(MessageType.Trace, "Will wait for subordinate process to stop for {0} sec".Args(app.StopTimeoutSec), related: rel);

      var startUtc = App.TimeSource.UTCNow;
      var exited = false;
      while(ComponentDirector.Running)
      {
        exited = process.WaitForExit(250);
        if (exited) break;

        var utc = App.TimeSource.UTCNow;
        if ((utc - startUtc).TotalSeconds > app.StopTimeoutSec)
        {
          WriteLogFromHere(MessageType.WarningExpectation, "Subordinate process Stop timeout of {0} sec exceeded. Killing now".Args(app.StopTimeoutSec), related: rel);
          try
          {
            process.Kill();
            WriteLogFromHere(MessageType.WarningExpectation, "Killed");
          }
          catch(Exception error)
          {
            WriteLogFromHere(MessageType.Critical, "Kill() leaked: " + error.ToMessageWithType(), error, related: rel);
          }
        }
      }

      try
      {
        process.Close();
      }
      catch(Exception error)
      {
        WriteLogFromHere(MessageType.Critical, "process.close() leaked: " + error.ToMessageWithType(), error, related: rel);
      }

      return true;
    }
  }
}
