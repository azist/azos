using System;
using System.Collections.Generic;
using System.Text;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Client
{
  public class HttpService : ServiceBase<HttpEndpoint, HttpTransport>
  {
    public HttpService(IApplicationComponent director, IConfigSectionNode conf) : base(director, conf)
    {
    }

    protected override HttpTransport DoAcquireTransport(HttpEndpoint endpoint)
    {
      throw new NotImplementedException();
    }

    protected override IEnumerable<IEndpoint> DoGetEndpointsForCall(string contract, object shardKey, string network, string binding)
    {
      throw new NotImplementedException();
    }

    protected override HttpTransport DoReleaseTransport(HttpTransport endpoint)
    {
      throw new NotImplementedException();
    }
  }

  public class HttpEndpoint : IEndpointImplementation
  {
    public IService Service => throw new NotImplementedException();

    public string RemoteAddress => throw new NotImplementedException();

    public string Network => throw new NotImplementedException();

    public string Binding => throw new NotImplementedException();

    public string Contract => throw new NotImplementedException();

    public int Shard => throw new NotImplementedException();

    public int ShardOrder => throw new NotImplementedException();

    public int TimeoutMs => throw new NotImplementedException();

    public DateTime? CircuitBreakerTimeStampUtc => throw new NotImplementedException();

    public DateTime? OfflineTimeStampUtc => throw new NotImplementedException();

    public bool IsOnline => throw new NotImplementedException();

    public string StatusMsg => throw new NotImplementedException();

    public void Dispose()
    {
      throw new NotImplementedException();
    }

    public void PutOffline(string statusMsg)
    {
      throw new NotImplementedException();
    }

    public void PutOnline(string statusMsg)
    {
      throw new NotImplementedException();
    }

    public bool TryResetCircuitBreaker(string statusMessage)
    {
      throw new NotImplementedException();
    }
  }

  //https://medium.com/@nuno.caneco/c-httpclient-should-not-be-disposed-or-should-it-45d2a8f568bc
  public class HttpTransport : ITransportImplementation
  {
    public IEndpoint Endpoint => throw new NotImplementedException();

    public void Dispose()
    {
      throw new NotImplementedException();
    }
  }
}
