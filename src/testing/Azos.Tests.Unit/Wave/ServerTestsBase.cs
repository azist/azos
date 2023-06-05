/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Net;
using System.Net.Http;

using Azos.Apps;
using Azos.Scripting;
using Azos.Platform;
using Azos.Wave;

namespace Azos.Tests.Unit.Wave
{
  [Runnable(TRUN.BASE)]
  public class ServerTestsBase : IRunnableHook
  {
    protected static readonly Uri BASE_URI = new Uri("http://localhost:9871/");

    private AzosApplication m_App;
    private WaveServer m_Server;
    private HttpClient m_Client;

    protected HttpClient Client => m_Client;

    public void Prologue(Runner runner, FID id)
    {
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine("-------------------------------------------");
      Console.WriteLine("Depending on listener, ");
      Console.WriteLine("might need Run as Administrator or grant all users rights to http://localhost:9871");
      Console.WriteLine("-------------------------------------------");
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine();

      m_Client = new HttpClient();
      var config = typeof(ServerTestsBase).GetText("tests.laconf").AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);
      m_App = new AzosApplication(null, config);
      m_Server = new WaveServer(m_App);
      m_Server.Configure(null);
      m_Server.Start();
      DoPrologue(runner, id);
    }

    protected virtual void DoPrologue(Runner runner, FID id)
    {

    }

    public bool Epilogue(Runner runner, FID id, Exception error)
    {
      DisposableObject.DisposeAndNull(ref m_Server);
      DisposableObject.DisposeAndNull(ref m_App);
      m_Client.Dispose();
      return false;
    }

  }

}
