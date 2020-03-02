/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Syscon=System.Console;
using System.Threading;

using Azos.Conf;

namespace Azos.IO.Console
{
  /// <summary>
  /// Provides helper methods for working with/implementing IRemoteTerminal
  /// </summary>
  public static class TerminalUtils
  {
    /// <summary>
    /// Parses command string into config node
    /// </summary>
    public static Configuration ParseCommand(string command, IEnvironmentVariableResolver resolver = null)
    {
      var cfg = LaconicConfiguration.CreateFromString(command);
      cfg.EnvironmentVarResolver = resolver;
      return cfg;
    }

    public static void ShowRemoteException(Glue.RemoteException exception)
    {
      var remote = exception.Remote;

      ConsoleUtils.Error("Remote error " + exception.ToMessageWithType());
      ShowRemoteExceptionData(remote, 2);
    }

    public static void ShowRemoteExceptionData(WrappedExceptionData data, int lvl)
    {
      var ind = "".PadLeft(lvl);

      Syscon.WriteLine(ind + "Application: " + data.ApplicationName);
      Syscon.WriteLine(ind + "Code: {0}  Source: {1}".Args(data.Code, data.Source));
      Syscon.WriteLine(ind + "Type: " + data.TypeName);
      Syscon.WriteLine(ind + "Message: " + data.Message);
      Syscon.WriteLine(ind + "Stack: " + data.StackTrace);
      if (data.InnerException != null)
      {
        Syscon.WriteLine(ind + "Inner exception: ");
        ShowRemoteExceptionData(data.InnerException, lvl + 2);
      }
    }


    /// <summary>
    /// Allows to execute an otherwise-blocking Console.ReadLine() call with the ability to abort the call gracefully.
    /// This class can be constructed only once per process
    /// </summary>
    public sealed class AbortableLineReader
    {
      private static object s_Lock = new object();
      private static volatile AbortableLineReader s_Instance;

      /// <summary>
      /// This .ctor can be called only once per process
      /// </summary>
      public AbortableLineReader()
      {
        lock (s_Lock)
        {
          if (s_Instance != null)
            throw new AzosException("{0}.ctor(already called)".Args(GetType().FullName));
          s_Instance = this;


          m_Thread = new Thread(spin, 16 * 1024);//stack size enough to call Console.ReadLine() with all of its dependencies
          m_Thread.IsBackground = true;
          m_Thread.Name = GetType().FullName;
          m_Thread.Start();

          Syscon.CancelKeyPress += (_, e) => { Abort("CTRL+C\r\n"); e.Cancel = true;};
        }
      }

      private bool m_Aborted;
      private string m_Line;
      private Thread m_Thread;

      public bool Aborted => m_Aborted;

      /// <summary>
      /// Returns non-null when stdin supplied newline string
      /// </summary>
      public string Line => m_Line;

      public void Abort(string line = null)
      {
        if (m_Aborted) return;
        m_Line = line;
        m_Thread.Abort();
        m_Aborted = true;
      }

      private void spin()
      {
        try
        {
          m_Line = Syscon.ReadLine();
        }
        catch (ThreadAbortException)
        {
        }
      }
    }
  }

}
