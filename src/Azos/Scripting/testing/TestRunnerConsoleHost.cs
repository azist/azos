/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using Azos.Apps;
using Azos.Conf;
using Azos.IO;

namespace Azos.Scripting
{
  /// <summary>
  /// Hosts unit test runner in a console application. This host is NOT thread-safe
  /// </summary>
  public sealed class TestRunnerConsoleHost : ApplicationComponent, IRunnerHost
  {
    public const int FIRST_X_ERRORS = 5;

    public TestRunnerConsoleHost(IApplication app) : base(app) { }

    private Stopwatch  m_Stopwatch;
    private int m_TotalRunnables;
    private int m_TotalMethods;
    private int m_TotalOKs;
    private int m_TotalErrors;
    private IConsoleOut m_ConsoleOut = LocalConsoleOut.DEFAULT;
    private IConsoleOut m_ConsoleErrorOut = LocalConsoleOut.DEFAULT;

    private FileConfiguration m_Out;

    [Config("$out|$file|$out-file")]
    public string OutFileName { get; set;}

    public int TotalRunnables => m_TotalRunnables;
    public int TotalMethods   => m_TotalMethods;
    public int TotalOKs       => m_TotalOKs;
    public int TotalErrors    => m_TotalErrors;

    public override string ComponentLogTopic => CoreConsts.RUN_TOPIC;

    public void Configure(IConfigSectionNode node) => ConfigAttribute.Apply(this, node);

    public IConsoleOut ConsoleOut   => m_ConsoleOut;
    public IConsoleOut ConsoleError => m_ConsoleErrorOut;


    private string m_RunnableHeader;
    private bool   m_HadRunnableMethods;
    private string m_PriorMethodName;
    private int    m_PriorMethodCount;

    private ConfigSectionNode m_RunnableNode;

    private List<(FID id, object target, Exception error)> m_FirstXErrors;

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
        ConsoleOut.ForegroundColor = ConsoleColor.Yellow;
        ConsoleOut.WriteLine("EndRunnable caught: ");
        writeError(error);
        ConsoleOut.ForegroundColor = ConsoleColor.Gray;

        if (m_FirstXErrors.Count<FIRST_X_ERRORS)
        {
          m_FirstXErrors.Add( (id, runnable.GetType(), error) );
        }
      }
      if (!m_HadRunnableMethods) return;
      ConsoleOut.WriteLine("... done {0}".Args(runnable.GetType().DisplayNameWithExpandedGenericArgs()));
      ConsoleOut.WriteLine();
      writeCurrentStats();
      ConsoleOut.WriteLine();
    }


    public void BeforeMethodRun(Runner runner, FID id, MethodInfo method, RunAttribute attr)
    {
      if (m_RunnableHeader!=null)
      {
        ConsoleOut.WriteLine(m_RunnableHeader);
        m_RunnableHeader = null;
      }
      m_HadRunnableMethods =true;
      m_TotalMethods++;
      ConsoleOut.ForegroundColor = ConsoleColor.Gray;
      ConsoleOut.Write("  - {0} ".Args(method.Name));
      if (attr.Name.IsNotNullOrWhiteSpace())
      {
        ConsoleOut.ForegroundColor = ConsoleColor.DarkGray;
        ConsoleOut.Write("::");
        ConsoleOut.ForegroundColor = ConsoleColor.Blue;
        ConsoleOut.Write("'{0}'".Args(attr.Name));
      }


      if (attr.ConfigContent.IsNotNullOrWhiteSpace())
      {

        try
        {
          ConsoleOut.ForegroundColor = ConsoleColor.DarkCyan;
          ConsoleOut.Write(" {0} ".Args( attr.Config.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.Compact)
                                               .Remove(0, 1)
                                               .TakeFirstChars(128, "...")));
        }
        catch
        {
          ConsoleOut.ForegroundColor = ConsoleColor.Red;
          ConsoleOut.WriteLine("<bad config>");
        }
      }

      if (method.Name == m_PriorMethodName)
      {
        m_PriorMethodCount++;
        ConsoleOut.ForegroundColor = ConsoleColor.Blue;
        ConsoleOut.Write("[{0}] ".Args(m_PriorMethodCount));
      }
      else
       m_PriorMethodCount = 0;

      m_PriorMethodName = method.Name;

      if (attr.Message.IsNotNullOrWhiteSpace())
      {
        ConsoleOut.WriteLine();
        ConsoleOut.ForegroundColor = ConsoleColor.DarkYellow;
        ConsoleOut.WriteLine("Message:");
        ConsoleOut.ForegroundColor = ConsoleColor.Yellow;
        ConsoleOut.Write("  ");
        ConsoleOut.WriteLine(attr.Message);
      }

      ConsoleOut.ForegroundColor = ConsoleColor.DarkGray;
    }

    public void AfterMethodRun(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
    {
      ConsoleOut.ForegroundColor = ConsoleColor.Gray;

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


      var wasF = ConsoleOut.ForegroundColor;
      if (error==null)
      {
        m_TotalOKs++;
        if (runner.Emulate)
        {
          ConsoleOut.ForegroundColor = ConsoleColor.Yellow;
          ConsoleOut.Write("[Emulated]");
        }
        else
        {
          ConsoleOut.ForegroundColor = ConsoleColor.Green;
          ConsoleOut.Write("[OK]");
        }
      }
      else
      {
        m_TotalErrors++;
        writeError(error);
        if (m_FirstXErrors.Count < FIRST_X_ERRORS)
        {
          m_FirstXErrors.Add((id, method, error));
        }
      }

      ConsoleOut.ForegroundColor = wasF;
      ConsoleOut.WriteLine();


    }



    public void Start(Runner runner)
    {
      m_Stopwatch = Stopwatch.StartNew();
      m_TotalRunnables = 0;
      m_TotalMethods = 0;
      m_TotalOKs =0;
      m_TotalErrors = 0;
      m_FirstXErrors = new List<(FID id, object target, Exception error)>();

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
        m_Out.Root.AddAttributeNode("app-instance", App.InstanceId);
        m_Out.SaveAs(OutFileName);

        ConsoleOut.ForegroundColor = ConsoleColor.DarkGray;
        ConsoleOut.Write("Out file format: ");
        ConsoleOut.ForegroundColor = ConsoleColor.White;
        ConsoleOut.WriteLine("{0}".Args(m_Out.GetType()));

        ConsoleOut.ForegroundColor = ConsoleColor.DarkGray;
        ConsoleOut.Write("Out file name: ");
        ConsoleOut.ForegroundColor = ConsoleColor.Gray;
        ConsoleOut.WriteLine("'{0}'".Args(OutFileName));
      }


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

      if (m_FirstXErrors.Count>0)
      {
        ConsoleOut.WriteLine();
        ConsoleOut.WriteLine();
        ConsoleOut.ForegroundColor = ConsoleColor.Yellow;
        ConsoleOut.Write("  Dumping the first ");
        ConsoleOut.ForegroundColor = ConsoleColor.Red;
        ConsoleOut.WriteLine("{0} errors:".Args(m_FirstXErrors.Count));
        ConsoleOut.WriteLine();
        for (var i=0; i<m_FirstXErrors.Count; i++)
        {
          var eitem = m_FirstXErrors[i];
          ConsoleOut.ForegroundColor = ConsoleColor.DarkRed;
          ConsoleOut.Write("  Error #{0} from: ".Args(i + 1));
          ConsoleOut.ForegroundColor = ConsoleColor.Red;
          ConsoleOut.WriteLine(eitem.target is Type trunnable ? $"Runnable {trunnable.FullName}" : ((MethodInfo)eitem.target).ToDescription() );
          ConsoleOut.ForegroundColor = ConsoleColor.DarkRed;
          ConsoleOut.WriteLine("  =============================================");
          writeError(eitem.error);
        }

        writeCurrentStats();
      }

      if (runner.Emulate)
      {
        ConsoleOut.ForegroundColor = ConsoleColor.Yellow;
        ConsoleOut.WriteLine();
        ConsoleOut.WriteLine("    *** TEST RESULTS ARE EMULATED ***");
      }

      if (OutFileName.IsNotNullOrWhiteSpace())
      {
        m_Out.SaveAs(OutFileName);
        ConsoleOut.WriteLine();
        ConsoleOut.WriteLine();
        ConsoleOut.ForegroundColor = ConsoleColor.DarkGray;
        ConsoleOut.Write("Saved file: ");
        ConsoleOut.ForegroundColor = ConsoleColor.Gray;
        ConsoleOut.WriteLine("'{0}'".Args(OutFileName));
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
        ConsoleOut.ForegroundColor = ConsoleColor.Red;
        if (nesting==0)
          ConsoleOut.Write("[Error]");
        else
          ConsoleOut.Write("[Error[{0}]]".Args(nesting));
        ConsoleOut.ForegroundColor = error is ScriptingException ? ConsoleColor.Cyan : ConsoleColor.Magenta;
        ConsoleOut.WriteLine(" "+error.ToMessageWithType());
        ConsoleOut.WriteLine(error.StackTrace); //todo stack trace conditionally
        ConsoleOut.WriteLine();

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


  }
}
