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

    private FileConfiguration m_Out;

    [Config("$appveyor|$appveyor-path")]
    public string AppveyorPath { get; set;}

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


    private string m_RunnableHeader;
    private bool m_HadRunnableMethods;
    private string m_PriorMethodName;
    private int m_PriorMethodCount;


    private ConfigSectionNode m_RunnableNode;

    public void BeginRunnable(Runner runner, FID id, object runnable)
    {
      m_TotalRunnables++;
      var t = runnable.GetType();
      m_RunnableHeader = "Starting {0}::{1}.{2} ...".Args(t.Assembly.GetName().Name, t.Namespace, t.DisplayNameWithExpandedGenericArgs());
      m_HadRunnableMethods = false;
      m_PriorMethodName = null;
      m_PriorMethodCount = 0;

      var o = m_Out?.Root;
      if (o!=null)
      {
        m_RunnableNode = o.AddChildNode("runnable", runnable.GetType().Name);
        m_RunnableNode.AddAttributeNode("id", id);
        m_RunnableNode.AddAttributeNode("type", runnable.GetType().AssemblyQualifiedName);
        m_RunnableNode.AddAttributeNode("now-loc", App.LocalizedTime);
        m_RunnableNode.AddAttributeNode("now-utc", App.TimeSource.UTCNow);
      }
    }

    public void EndRunnable(Runner runner, FID id, object runnable, Exception error)
    {
      if (m_RunnableNode!=null)
      {
         outError(m_RunnableNode, error);
      }

      if (error!=null)
      {
        m_TotalErrors++;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("EndRunnable caught: ");
        writeError(error);
        Console.ForegroundColor = ConsoleColor.Gray;
      }
      if (!m_HadRunnableMethods) return;
      Console.WriteLine("... done {0}".Args(runnable.GetType().DisplayNameWithExpandedGenericArgs()));
      Console.WriteLine();
      writeCurrentStats();
      Console.WriteLine();
    }


    public void BeforeMethodRun(Runner runner, FID id, MethodInfo method, RunAttribute attr)
    {
      if (m_RunnableHeader!=null)
      {
        Console.WriteLine(m_RunnableHeader);
        m_RunnableHeader = null;
      }
      m_HadRunnableMethods =true;
      m_TotalMethods++;
      Console.ForegroundColor = ConsoleColor.Gray;
      Console.Write("  - {0} ".Args(method.Name));
      if (attr.Name.IsNotNullOrWhiteSpace())
      {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("::");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("'{0}'".Args(attr.Name));
      }


      if (attr.ConfigContent.IsNotNullOrWhiteSpace())
      {

        try
        {
          Console.ForegroundColor = ConsoleColor.DarkCyan;
          Console.Write(" {0} ".Args( attr.Config.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.Compact)
                                               .Remove(0, 1)
                                               .TakeFirstChars(128, "...")));
        }
        catch
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine("<bad config>");
        }
      }

      if (method.Name == m_PriorMethodName)
      {
        m_PriorMethodCount++;
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("[{0}] ".Args(m_PriorMethodCount));
      }
      else
       m_PriorMethodCount = 0;

      m_PriorMethodName = method.Name;

      if (attr.Message.IsNotNullOrWhiteSpace())
      {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("Message:");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("  ");
        Console.WriteLine(attr.Message);
      }

      Console.ForegroundColor = ConsoleColor.DarkGray;
    }

    public void AfterMethodRun(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
    {
      Console.ForegroundColor = ConsoleColor.Gray;

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

      var o = m_RunnableNode;
      if (o != null)
      {
        var nrun = o.AddChildNode("run", method.Name);
        nrun.AddAttributeNode("id", id);
        nrun.AddAttributeNode("now-loc", App.LocalizedTime);
        nrun.AddAttributeNode("now-utc", App.TimeSource.UTCNow);
        nrun.AddAttributeNode("OK", error==null);

        if (runner.Emulate)
          nrun.AddAttributeNode("emulated", true);

        if (attr.Message.IsNotNullOrWhiteSpace())
          nrun.AddAttributeNode("message", attr.Message);

        nrun.AddAttributeNode("run-name", attr.Name);
        nrun.AddAttributeNode("run-explicit", attr.ExplicitName);
        nrun.AddAttributeNode("run-config", attr.ConfigContent);

        outError(nrun, error);
      }


      var wasF = Console.ForegroundColor;
      if (error==null)
      {
        m_TotalOKs++;
        if (runner.Emulate)
        {
          Console.ForegroundColor = ConsoleColor.Yellow;
          Console.Write("[Emulated]");
        }
        else
        {
          Console.ForegroundColor = ConsoleColor.Green;
          Console.Write("[OK]");
        }
      }
      else
      {
        m_TotalErrors++;
        writeError(error);
      }

      Console.ForegroundColor = wasF;
      Console.WriteLine();

      reportAppVeyor(method.Name, method.DeclaringType.FullName, error, 0, "", "");
    }



    public void Start(Runner runner)
    {
      m_Stopwatch = Stopwatch.StartNew();
      m_TotalRunnables = 0;
      m_TotalMethods = 0;
      m_TotalOKs =0;
      m_TotalErrors = 0;

      if (OutFileName.IsNotNullOrWhiteSpace())
      {
        m_Out = Configuration.MakeProviderForFile(OutFileName);
        m_Out.Create(this.GetType().FullName);
        m_Out.Root.AddAttributeNode("runtime", Platform.Abstraction.PlatformAbstractionLayer.PlatformName);
        m_Out.Root.AddAttributeNode("timestamp-local", App.LocalizedTime);
        m_Out.Root.AddAttributeNode("timestamp-utc", App.TimeSource.UTCNow);
        m_Out.Root.AddAttributeNode("user", System.Environment.UserName);
        m_Out.Root.AddAttributeNode("machine", System.Environment.MachineName);
        m_Out.Root.AddAttributeNode("os", Platform.Computer.OSFamily);
        m_Out.Root.AddAttributeNode("cmd", System.Environment.CommandLine);
        m_Out.Root.AddAttributeNode("app-name", App.Name);
        m_Out.Root.AddAttributeNode("app-instance", App.InstanceID);
        m_Out.SaveAs(OutFileName);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("Out file format: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("{0}".Args(m_Out.GetType()));

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("Out file name: ");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("'{0}'".Args(OutFileName));
      }


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

      if (OutFileName.IsNotNullOrWhiteSpace())
      {
        m_Out.SaveAs(OutFileName);
        Console.WriteLine();
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("Saved file: ");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("'{0}'".Args(OutFileName));
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
        Console.ForegroundColor = ConsoleColor.Red;
        if (nesting==0)
          Console.Write("[Error]");
        else
          Console.Write("[Error[{0}]]".Args(nesting));
        Console.ForegroundColor = error is ScriptingException ? ConsoleColor.Cyan : ConsoleColor.Magenta;
        Console.WriteLine(" "+error.ToMessageWithType());
        Console.WriteLine(error.StackTrace); //todo stack trace conditionally
        Console.WriteLine();

        error = error.InnerException;
        nesting++;
      }
    }

    private void outError(ConfigSectionNode node, Exception error)
    {
      var nesting = 0;
      while (error!=null)
      {
        node = node.AddChildNode("error", error.GetType().Name);
        node.AddAttributeNode("type", error.GetType().AssemblyQualifiedName);
        node.AddAttributeNode("nesting", nesting);
        node.AddAttributeNode("msg", error.Message);
        node.AddAttributeNode("stack", error.StackTrace);

        error = error.InnerException;
        nesting++;
      }
    }

    private void reportAppVeyor(string name, string file, Exception error, int durationMs, string stdOut, string stdError)
    {
      var appv = "./appveyor.exe"; //Path.Combine(this.AppveyorPath ?? "", "appveyor");
      var outcome = error == null ? "Passed" : "Failed";
      var cmd = $"{appv} AddTest {name} -Framework \"Azos\" -FileName \"{file}\" -Outcome \"{outcome}\""+
                $"-Duration \"{durationMs}\" -ErrorMessage \"error?.Message\" -ErrorStackTrace \"error?.StackTrace\""+
                $"-StdOut \"{stdOut}\" -StdError \"{stdError}\"";
      Console.WriteLine(cmd);
      ProcessRunner.Run(cmd);
    }

  }
}
