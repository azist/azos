/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Diagnostics;
using System.IO;
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
  public interface IAppActivatorContext { }

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
    public const string CONFIG_START_WORKING_DIRECTORY_ATTR = "working-directory";
    public const string CONFIG_START_EXECUTABLE_ATTR = "executable";
    public const string CONFIG_START_EXECUTABLE_ARGS_ATTR = "args";
    public const string CONFIG_START_HGOV_ARGS_PRAGMA = "hgov-args-pragma";
    public const string PRAGMA_HGOV_DEFAULT = "{{gov}}";

    internal sealed class ProcessContext : IAppActivatorContext
    {
      //https://developers.redhat.com/blog/2019/10/29/the-net-process-class-on-linux#processname
      public ProcessContext(Process process) => Process = process;
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
      var ctx = app.ActivationContext as ProcessContext;
      if (ctx != null) return false;//already running

      WriteLogFromHere(MessageType.Trace, "Initiating app `{0}` start".Args(app.Name), related: rel);


      var workingDirectory = app.StartSection.ValOf(CONFIG_START_WORKING_DIRECTORY_ATTR).Default(string.Empty);
      var exeFile = app.StartSection.ValOf(CONFIG_START_EXECUTABLE_ATTR);
      var exeArgs = app.StartSection.ValOf(CONFIG_START_EXECUTABLE_ARGS_ATTR);

      var govPragma = app.StartSection.ValOf(CONFIG_START_HGOV_ARGS_PRAGMA).Default(PRAGMA_HGOV_DEFAULT);

      if (exeArgs.IsNotNullOrWhiteSpace() && exeArgs.IndexOf(govPragma, StringComparison.Ordinal) >=0)
      {
        var govDirective = "{0}://{1}:{2}".Args(BootArgs.GOV_BINDING, ComponentDirector.AssignedSipcServerPort, app.Name);
        exeArgs = exeArgs.Replace(govPragma, govDirective, StringComparison.Ordinal);
      }

      if (exeFile.IsNullOrWhiteSpace())
      {
        var reason = "App failure: `{0}` process exe image attribute `${1}` is missing ".Args(app.Name, CONFIG_START_EXECUTABLE_ATTR);
        WriteLogFromHere(MessageType.CatastrophicError, text: reason, related: rel);
        app.Fail(reason);
        return false;
      }

      if (workingDirectory.IsNotNullOrWhiteSpace() && !Directory.Exists(workingDirectory))
      {
        var reason = "App failure: `{0}` process working directory `{1}` does not exist".Args(app.Name, workingDirectory);
        WriteLogFromHere(MessageType.CatastrophicError, text: reason, related: rel);
        app.Fail(reason);
        return false;
      }

      WriteLogFromHere(MessageType.Trace, "Set directories".Args(app.Name), related: rel, pars: (new { workingDirectory, exeFile, exeArgs }).ToJson());

      var process = new Process();
      app.ActivationContext = new ProcessContext(process);//link context
      app.LastStartAttemptUtc = App.TimeSource.UTCNow;

      process.StartInfo.FileName = exeFile;
      process.StartInfo.WorkingDirectory = workingDirectory;
      process.StartInfo.Arguments = exeArgs;//todo: In .Net 5+ use ArgumentList instead which properly handles platform specific escapes

      //Unix shell scripts are considered real executables by the operating system (OS). This means it is not required
      //to set UseShellExecute. This approach is unlike Windows .bat files, which need the Windows shell to find the interpreter.
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

      return true;
    }

    public bool StopApplication(App app)
    {
      var rel = Guid.NewGuid();
      var ctx = app.ActivationContext as ProcessContext;
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
        WriteLogFromHere(MessageType.Error, "sipc.Send(STOP) for `{0}` leaked: {1}".Args(app.Name, error.ToMessageWithType()), error, related: rel);
      }

      WriteLogFromHere(MessageType.Trace, "Will wait for subordinate process to stop for {0} sec".Args(app.StopTimeoutSec), related: rel);

      var startUtc = App.TimeSource.UTCNow;
      while(true)
      {
        var exited = process.WaitForExit(250);
        if (exited) break;

        var utc = App.TimeSource.UTCNow;
        if ((utc - startUtc).TotalSeconds > app.StopTimeoutSec)
        {
          WriteLogFromHere(MessageType.WarningExpectation, "Subordinate process `{0}` Stop timeout of {1} sec exceeded. Killing now".Args(app.Name, app.StopTimeoutSec), related: rel);

          try
          {
            process.Kill();
            WriteLogFromHere(MessageType.WarningExpectation, "Killed(`{0}`)".Args(app.Name), related: rel);
          }
          catch(Exception error)
          {
            WriteLogFromHere(MessageType.Critical, "Kill(`{0}`) leaked: {1}".Args(app.Name, error.ToMessageWithType()), error, related: rel);
          }

          break;
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
