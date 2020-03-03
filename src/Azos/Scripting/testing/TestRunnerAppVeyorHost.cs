/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using Azos.Apps;
using Azos.Conf;
using Azos.IO.Console;
using Azos.Platform;

namespace Azos.Scripting
{
  /// <summary>
  /// Hosts unit test runner on a AppVeyor Server. This host is NOT thread-safe
  /// </summary>
  /// <see cref="https://www.appveyor.com/docs/build-worker-api/#add-tests"/>
  public sealed class TestRunnerAppVeyorHost : ApplicationComponent, IRunnerHost
  {

    public TestRunnerAppVeyorHost(IApplication app) : base(app) { }

    private Stopwatch  m_Stopwatch;
    private int m_TotalRunnables;
    private int m_TotalMethods;
    private int m_TotalOKs;
    private int m_TotalErrors;

    [Config("$out|$file|$out-file")]
    public string OutFileName { get; set; }

    public int TotalRunnables => m_TotalRunnables;
    public int TotalMethods   => m_TotalMethods;
    public int TotalOKs       => m_TotalOKs;
    public int TotalErrors    => m_TotalErrors;

    public override string ComponentLogTopic => CoreConsts.RUN_TOPIC;

    public void Configure(IConfigSectionNode node) => ConfigAttribute.Apply(this, node);

    public IConsolePort ConsolePort => Ambient.AppConsolePort;
    public IConsoleOut ConsoleOut => ConsolePort.DefaultConsole;

    private Stopwatch m_RunnableStopwatch;

    public void BeginRunnable(Runner runner, FID id, object runnable)
    {
      m_TotalRunnables++;
      var t = runnable.GetType();
      ConsoleOut.WriteLine( "Starting {0}::{1}.{2} ...".Args(t.Assembly.GetName().Name, t.Namespace, t.DisplayNameWithExpandedGenericArgs()) );
      m_RunnableStopwatch = Stopwatch.StartNew();
    }

    public void EndRunnable(Runner runner, FID id, object runnable, Exception error)
    {
      var duration = (int)m_RunnableStopwatch.ElapsedMilliseconds;
      if (error!=null)
      {
        m_TotalErrors++;
        ConsoleOut.ForegroundColor = ConsoleColor.Yellow;
        ConsoleOut.WriteLine("EndRunnable caught: ");
        writeError(error);
        ConsoleOut.ForegroundColor = ConsoleColor.Gray;

        reportAppVeyor("Runnable failure:", runnable.GetType().FullName, error, duration, "", "");
      }
      else
        reportAppVeyor("{0}.*".Args(runnable.GetType().Name), runnable.GetType().FullName, null, duration, "", "");
    }

    private Stopwatch m_MethodStopwatch;
    public void BeforeMethodRun(Runner runner, FID id, MethodInfo method, RunAttribute attr)
    {
      m_MethodStopwatch = Stopwatch.StartNew();
    }

    public void AfterMethodRun(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
    {
      var duration = (int)m_MethodStopwatch.ElapsedMilliseconds;
      m_TotalMethods++;

      ConsoleOut.Write(method.ToDescription());

      //////////////////check for Aver.Throws()
      ////////////////if (!runner.Emulate)
      ////////////////  try
      ////////////////  {
      ////////////////    var aversThrows = Aver.ThrowsAttribute.CheckMethodError(method, error);
      ////////////////    if (aversThrows) error =null;
      ////////////////  }
      ////////////////  catch(Exception err)
      ////////////////  {
      ////////////////    error = err;
      ////////////////  }

      if (error != null)
      {
        reportAppVeyor(method.Name, method.DeclaringType.FullName, error, duration, "", "");
        m_TotalErrors++;
        writeError(error);
      }
      else
      {
        m_TotalOKs++;
        ConsoleOut.WriteLine(" [OK]");
      }
    }



    public void Start(Runner runner)
    {
      m_Stopwatch = Stopwatch.StartNew();
      m_TotalRunnables = 0;
      m_TotalMethods = 0;
      m_TotalOKs =0;
      m_TotalErrors = 0;

      ConsoleOut.ForegroundColor = ConsoleColor.White;
      ConsoleOut.Write("Started ");
      ConsoleOut.ForegroundColor = ConsoleColor.Gray;
      ConsoleOut.WriteLine("{0}".Args(App.TimeSource.Now));
    }

    public void Summarize(Runner runner)
    {
      ConsoleOut.WriteLine();
      ConsoleOut.ForegroundColor = ConsoleColor.Gray;
      ConsoleOut.WriteLine("+------------------------------------------------");
      ConsoleOut.ForegroundColor = ConsoleColor.DarkGray;
      ConsoleOut.Write("|  Platform runtime: ");
      ConsoleOut.ForegroundColor = ConsoleColor.Yellow;
      ConsoleOut.WriteLine(Azos.Platform.Abstraction.PlatformAbstractionLayer.PlatformName);

      ConsoleOut.ForegroundColor = ConsoleColor.DarkGray;
      ConsoleOut.Write("|  Total runnables: ");
      ConsoleOut.ForegroundColor = ConsoleColor.White;
      ConsoleOut.WriteLine("{0}".Args(m_TotalRunnables));

      ConsoleOut.ForegroundColor = ConsoleColor.DarkGray;
      ConsoleOut.Write("|  Total methods: ");
      ConsoleOut.ForegroundColor = ConsoleColor.White;
      ConsoleOut.WriteLine("{0}".Args(m_TotalMethods));

      ConsoleOut.ForegroundColor = ConsoleColor.DarkGray;
      ConsoleOut.Write("|  Finished: ");
      ConsoleOut.ForegroundColor = ConsoleColor.White;
      ConsoleOut.WriteLine("{0}".Args(App.TimeSource.Now));

      ConsoleOut.ForegroundColor = ConsoleColor.DarkGray;
      ConsoleOut.Write("|  Running time: ");
      ConsoleOut.ForegroundColor = ConsoleColor.White;
      ConsoleOut.WriteLine("{0}".Args(m_Stopwatch.Elapsed));

      ConsoleOut.WriteLine("+------------------------------------------------");

      writeCurrentStats();

      if (runner.Emulate)
      {
        ConsoleOut.ForegroundColor = ConsoleColor.Yellow;
        ConsoleOut.WriteLine();
        ConsoleOut.WriteLine("    *** TEST RESULTS ARE EMULATED ***");
      }

      ConsoleOut.ForegroundColor = ConsoleColor.Gray;
    }

    private void writeCurrentStats()
    {
      ConsoleOut.WriteLine();

      ConsoleOut.ForegroundColor = m_TotalOKs >0 ? ConsoleColor.Green : ConsoleColor.DarkGreen;
      ConsoleOut.Write("   OK: {0}   ".Args(m_TotalOKs));
      ConsoleOut.ForegroundColor = m_TotalErrors>0? ConsoleColor.Red : ConsoleColor.DarkGray;
      ConsoleOut.Write("ERROR: {0}   ".Args(m_TotalErrors));

      ConsoleOut.ForegroundColor = ConsoleColor.White;
      ConsoleOut.WriteLine(" TOTAL: {0} ".Args(m_TotalOKs +  m_TotalErrors));
    }


    private void writeError(Exception error)
    {
      var nesting = 0;
      while (error!=null)
      {
        if (nesting==0)
          ConsoleOut.Write("[Error]");
        else
          ConsoleOut.Write("[Error[{0}]]".Args(nesting));
        ConsoleOut.WriteLine(" "+error.ToMessageWithType());
        ConsoleOut.WriteLine(error.StackTrace); //todo stack trace conditionally
        ConsoleOut.WriteLine();

        error = error.InnerException;
        nesting++;
      }
    }


    //https://www.appveyor.com/docs/build-worker-api/#add-tests
    private void reportAppVeyor(string name, string file, Exception error, int durationMs, string stdOut, string stdError)
    {
      var appv = "appveyor.exe";
      var outcome = error == null ? "Passed" : "Failed";
      var args = $"AddTest \"{name}\" -Framework \"Azos\" -FileName \"{file}\" -Outcome \"{outcome}\""+
                $" -Duration \"{durationMs}\" -ErrorMessage \"{error?.Message.Replace('"', ' ').TrimAll('\n','\r')}\" -ErrorStackTrace \"{error?.StackTrace.Replace('"', ' ').TrimAll('\n', '\r')}\""+
                $" -StdOut \"{stdOut}\" -StdError \"{stdError}\"";

      ConsoleOut.WriteLine( ProcessRunner.Run(appv, args, out bool timeout) );
    }

  }
}
