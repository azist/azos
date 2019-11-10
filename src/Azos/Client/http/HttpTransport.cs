using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Client
{
  //https://medium.com/@nuno.caneco/c-httpclient-should-not-be-disposed-or-should-it-45d2a8f568bc
  public class HttpTransport : DisposableObject, ITransportImplementation
  {
    protected internal HttpTransport(EndpointAssignment<HttpEndpoint> assignment)
    {

    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Client);
      base.Destructor();
    }

    protected readonly EndpointAssignment<HttpEndpoint> m_Assignment;
    protected HttpClient m_Client;

    EndpointAssignment ITransport.Assignment => m_Assignment.Upcast();
    public EndpointAssignment<HttpEndpoint> Assignment => m_Assignment;

    public HttpClient Client => m_Client;

  }
}
