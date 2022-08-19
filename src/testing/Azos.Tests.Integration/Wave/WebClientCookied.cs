/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Azos.Tests.Integration.Wave
{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
  public class WebClientCookied: WebClient
  {
    private CookieContainer m_CookieContainer = new CookieContainer();

    public CookieContainer CookieContainer { get { return m_CookieContainer; }}

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
#pragma warning restore SYSLIB0014 // Type or member is obsolete
}
