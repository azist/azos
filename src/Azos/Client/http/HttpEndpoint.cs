using System;
using System.Collections.Generic;
using System.Text;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Client
{
  /// <summary>
  /// Defines endpoints of Http/s service
  /// </summary>
  public class HttpEndpoint : EndpointBase<HttpService>, IEndpointImplementation
  {
    protected internal HttpEndpoint(HttpService service, IConfigSectionNode conf) : base(service, conf)
    {
     Uri.NonBlank("{0} is not configured".Args(nameof(Uri)));
    }

    [Config]private string m_Uri;

    /// <summary>
    /// Physical URI of the endpoint (the physical address of Http endpoint)
    /// </summary>
    public string Uri => m_Uri;
  }

}
