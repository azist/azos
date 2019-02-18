/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Apps;
using Azos.Scripting;
using Azos.Platform;
using Azos.Wave;
using System.Net;

namespace Azos.Tests.Unit.Wave
{
  [Runnable(TRUN.BASE)]
  public class ServerTestsBase : IRunnableHook
  {
    protected static readonly Uri BASE_URI = new Uri("http://localhost:9871/");

    AzosApplication m_App;
    WaveServer m_Server;

    public void Prologue(Runner runner, FID id)
    {
      var config = typeof(ServerTestsBase).GetText("tests.laconf").AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);
      m_App = new AzosApplication(null, config);
      m_Server = new WaveServer(m_App);
      m_Server.Configure(null);
      m_Server.Start();
    }

    public bool Epilogue(Runner runner, FID id, Exception error)
    {
      DisposableObject.DisposeAndNull(ref m_Server);
      DisposableObject.DisposeAndNull(ref m_App);
      return false;
    }

    protected WebClientCookied CreateWebClient()
    {
      var wc = new WebClientCookied();
      wc.CookieContainer.Add(BASE_URI, new Cookie("Secret", "Hello"));
      return wc;
    }

  }

  //todo: THis needs to be refactored into Azos.Web
  public class WebClientCookied : WebClient
  {
    private CookieContainer m_CookieContainer = new CookieContainer();

    public CookieContainer CookieContainer { get { return m_CookieContainer; } }

    protected override WebRequest GetWebRequest(Uri address)
    {
      var request = base.GetWebRequest(address);

      var httpRequest = request as HttpWebRequest;
      if (httpRequest != null)
      {
        httpRequest.CookieContainer = CookieContainer;
        httpRequest.AllowAutoRedirect = true;
      }

      return request;
    }
  }
}
