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
    }
  }

}
