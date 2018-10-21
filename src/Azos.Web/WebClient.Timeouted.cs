
using System;
using System.Net;

namespace Azos.Web
{
  public static partial class WebClient
  {
    /// <summary>
    /// Provides WebClient functionality with timeout
    /// </summary>
    public class WebClientTimeouted : System.Net.WebClient
    {
      public WebClientTimeouted(IWebClientCaller caller)
      {
        WebSettings.RequireInitializedSettings();
        m_Caller = caller;
      }

      private IWebClientCaller m_Caller;

      protected override WebRequest GetWebRequest(Uri uri)
      {
        var w = base.GetWebRequest(uri) as HttpWebRequest;
        w.Timeout = m_Caller.WebServiceCallTimeoutMs;
        w.KeepAlive = m_Caller.KeepAlive;
        w.Pipelined = m_Caller.Pipelined;
        return w;
      }
    }
  }

}
