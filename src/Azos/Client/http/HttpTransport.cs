using System;
using System.Net.Http;


namespace Azos.Client
{
  //https://medium.com/@nuno.caneco/c-httpclient-should-not-be-disposed-or-should-it-45d2a8f568bc
  public class HttpTransport : DisposableObject, ITransportImplementation
  {
    protected internal HttpTransport(EndpointAssignment assignment)
    {
      m_Assignment = assignment;
      Endpoint.NonNull("assignment is invalid - unassigned or endpoint is not HttpEndpoint");
    }
    protected readonly EndpointAssignment m_Assignment;

    public EndpointAssignment Assignment => m_Assignment;
    public HttpEndpoint Endpoint => Assignment.Endpoint as HttpEndpoint;
    public HttpClient Client => Endpoint.Client;

  }
}
