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
    public int TotalOKs        => m_TotalOKs;
    public int TotalErrors     => m_TotalErrors;

    public override string ComponentLogTopic => CoreConsts.RUN_TOPIC;

    public void Configure(IConfigSectionNode node) => ConfigAttribute.Apply(this, node);

    public TextWriter ConsoleOut   => Console.Out;
    public TextWriter ConsoleError => Console.Error;


    private ConfigSectionNode m_RunnableNode;

    public void BeginRunnable(Runner runner, FID id, object runnable)
    {
      m_TotalRunnables++;
      var t = runnable.GetType();
      Console.WriteLine( "Starting {0}::{1}.{2} ...".Args(t.Assembly.GetName().Name, t.Namespace, t.DisplayNameWithExpandedGenericArgs()) );
    }

    public void EndRunnable(Runner runner, FID id, object runnable, Exception error)
    {
      if (error!=null)
      {
        m_TotalErrors++;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("EndRunnable caught: ");
        writeError(error);
        Console.ForegroundColor = ConsoleColor.Gray;

        reportAppVeyor("Runnable failure:", runnable.GetType().FullName, error, 0, "", "");
      }
      else
        reportAppVeyor("{0}.*".Args(runnable.GetType().Name), runnable.GetType().FullName, null, 0, "", "");
    }


    public void BeforeMethodRun(Runner runner, FID id, MethodInfo method, RunAttribute attr)
    {
    }

    public void AfterMethodRun(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
    {
      m_TotalMethods++;

      Console.Write(method.ToDescription());

      //check for Aver.Throws()
      if (!runner.Emulate)
        try
        {
          var aversThrows = Aver.ThrowsAttribute.CheckMethodError(method, error);
          if (aversThrows) error =null;
        }
        catch(Exception err)
        {
          error = err;
        }

      if (error != null)
      {
        reportAppVeyor(method.Name, method.DeclaringType.FullName, error, 0, "", "");
        m_TotalErrors++;
        writeError(error);
      }
      else
      {
        m_TotalOKs++;
        Console.WriteLine(" [OK]");
      }
    }



    public void Start(Runner runner)
    {
      m_Stopwatch = Stopwatch.StartNew();
      m_TotalRunnables = 0;
      m_TotalMethods = 0;
      m_TotalOKs =0;
      m_TotalErrors = 0;

      Console.ForegroundColor = ConsoleColor.White;
      Console.Write("Started ");
      Console.ForegroundColor = ConsoleColor.Gray;
      Console.WriteLine("{0}".Args(App.TimeSource.Now));
    }

    public void Summarize(Runner runner)
    {
      Console.WriteLine();
      Console.ForegroundColor = ConsoleColor.Gray;
      Console.WriteLine("+------------------------------------------------");
      Console.ForegroundColor = ConsoleColor.DarkGray;
      Console.Write("|  Platform runtime: ");
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine(Azos.Platform.Abstraction.PlatformAbstractionLayer.PlatformName);

      Console.ForegroundColor = ConsoleColor.DarkGray;
      Console.Write("|  Total runnables: ");
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("{0}".Args(m_TotalRunnables));

      Console.ForegroundColor = ConsoleColor.DarkGray;
      Console.Write("|  Total methods: ");
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("{0}".Args(m_TotalMethods));

      Console.ForegroundColor = ConsoleColor.DarkGray;
      Console.Write("|  Finished: ");
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("{0}".Args(App.TimeSource.Now));

      Console.ForegroundColor = ConsoleColor.DarkGray;
      Console.Write("|  Running time: ");
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("{0}".Args(m_Stopwatch.Elapsed));

      Console.WriteLine("+------------------------------------------------");

      writeCurrentStats();

      if (runner.Emulate)
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine();
        Console.WriteLine("    *** TEST RESULTS ARE EMULATED ***");
      }

      Console.ForegroundColor = ConsoleColor.Gray;
    }

    private void writeCurrentStats()
    {
      Console.WriteLine();

      Console.ForegroundColor = m_TotalOKs >0 ? ConsoleColor.Green : ConsoleColor.DarkGreen;
      Console.Write("   OK: {0}   ".Args(m_TotalOKs));
      Console.ForegroundColor = m_TotalErrors>0? ConsoleColor.Red : ConsoleColor.DarkGray;
      Console.Write("ERROR: {0}   ".Args(m_TotalErrors));

      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine(" TOTAL: {0} ".Args(m_TotalOKs +  m_TotalErrors));
    }


    private void writeError(Exception error)
    {
      var nesting = 0;
      while (error!=null)
      {
        if (nesting==0)
          Console.Write("[Error]");
        else
          Console.Write("[Error[{0}]]".Args(nesting));
        Console.WriteLine(" "+error.ToMessageWithType());
        Console.WriteLine(error.StackTrace); //todo stack trace conditionally
        Console.WriteLine();

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
                $" -Duration \"{durationMs}\" -ErrorMessage \"{error?.Message}\" -ErrorStackTrace \"{error?.StackTrace}\""+
                $" -StdOut \"{stdOut}\" -StdError \"{stdError}\"";

      Console.WriteLine( ProcessRunner.Run(appv, args, out bool timeout) );
    }

  }
}
