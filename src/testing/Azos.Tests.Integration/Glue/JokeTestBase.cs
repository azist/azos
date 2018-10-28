/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Azos.Scripting;
using Azos.Security;

namespace Azos.Tests.Integration.Glue
{
  public class JokeTestBase : IRunnableHook
  {
    public const string DEFAULT_PROCESS_FILENAME = "TestServer.exe";

    public const string DEFAULT_TEST_SERVER_HOST = "127.0.0.1";
    public const int DEFAULT_TEST_SERVER_SYNC_PORT = 8000;
    public const int DEFAULT_TEST_SERVER_MPX_PORT = 5701;

    public readonly string DEFAULT_TEST_SERVER_SYNC_NODE = "sync://" + DEFAULT_TEST_SERVER_HOST + ":" + DEFAULT_TEST_SERVER_SYNC_PORT;

    public readonly string DEFAULT_TEST_SERVER_MPX_NODE = "mpx://" + DEFAULT_TEST_SERVER_HOST + ":" + DEFAULT_TEST_SERVER_MPX_PORT;

    public readonly Credentials DEFAULT_TEST_CREDENTIALS = new IDPasswordCredentials("dima", "thejake");

    #region Pvt Fields

    private Process m_ServerProcess = new Process();

    #endregion

    #region Init/TearDown

    void IRunnableHook.Prologue(Runner runner, FID id)
    {
      ProcessStartInfo start;
      if (Platform.Abstraction.PlatformAbstractionLayer.IsNetCore)
        start = new ProcessStartInfo()
        {
          FileName = "dotnet",
          Arguments = "toy.dll -config toy-server.laconf",
          RedirectStandardInput = true,
          UseShellExecute = false
        };
      else
        start = new ProcessStartInfo()
        {
          FileName = "toy.exe",
          Arguments = "-config toy-server.laconf",
          RedirectStandardInput = true,
          UseShellExecute = false
        };

      m_ServerProcess = new Process() { StartInfo = start };

      m_ServerProcess.Start();

      System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
    }

    bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
    {
      m_ServerProcess.StandardInput.WriteLine(string.Empty);
      return false;
    }

    #endregion

    #region Protected

    protected virtual string ProcessFileName { get { return DEFAULT_PROCESS_FILENAME; } }

    protected virtual string TestServerHost { get { return DEFAULT_TEST_SERVER_HOST; } }
    protected virtual int TestServerSyncPort { get { return DEFAULT_TEST_SERVER_SYNC_PORT; } }
    protected virtual string TestServerSyncNode { get { return DEFAULT_TEST_SERVER_SYNC_NODE; } }
    protected virtual string TestServerMpxNode { get { return DEFAULT_TEST_SERVER_MPX_NODE; } }
    protected virtual Credentials TestCredentials { get { return DEFAULT_TEST_CREDENTIALS; } }

    #endregion
  }
}
