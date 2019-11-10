using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

using Azos.Conf;

namespace Azos.Client
{
  /// <summary>
  /// Defines endpoints of Http/s service
  /// </summary>
  public class HttpEndpoint : EndpointBase<HttpService>, IEndpointImplementation
  {
    public const int DEFAULT_TIMEOUT_MS = 10_000;

    public HttpEndpoint(HttpService service, IConfigSectionNode conf) : base(service, conf)
    {
      Uri.NonNull("`{0}` is not configured".Args(nameof(Uri)));
      m_ClientHandler = new HttpClientHandler();
    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Client);
      DisposeAndNull(ref m_ClientHandler);
      base.Destructor();
    }

    private object m_Lock = new object();
    private HttpClientHandler m_ClientHandler;
    private HttpClient m_Client;

    /// <summary>
    /// Physical URI of the endpoint (the physical address of Http endpoint)
    /// </summary>
    [Config]
    public Uri Uri { get; private set; }

    /// <summary> If True, automatically follows HTTP redirect </summary>
    [Config(Default = true)]
    public bool AutoRedirect { get; private set; } = true;

    [Config(Default = "Basic")]
    public string AuthScheme { get; internal set; } = "Basic";

    [Config]
    public string AuthHeader { get; internal set; }

    [Config(Default = true)]
    public bool IsJson { get; internal set; } = true;

    /// <summary>
    /// Returns Http Client which is used to make calls to the remote http endpoint
    /// </summary>
    public HttpClient Client
    {
      get
      {
        EnsureObjectNotDisposed();
        lock (m_Lock)
        {
          if (m_Client == null)
          {
            m_ClientHandler = new HttpClientHandler();
            m_ClientHandler.AllowAutoRedirect = AutoRedirect;
            //configure client handler


            m_Client = new HttpClient(m_ClientHandler);
            m_Client.Timeout = TimeSpan.FromMilliseconds(TimeoutMs > 0 ? TimeoutMs : this.Service.DefaultTimeoutMs > 0 ? Service.DefaultTimeoutMs : DEFAULT_TIMEOUT_MS);
            m_Client.BaseAddress = this.Uri;

            //FINISH!!!
//            if (IsJson)
//              m_Client.DefaultRequestHeaders.Accept.ParseAdd(ContentType.JSON);

            if (AuthHeader.IsNotNullOrWhiteSpace())
              m_Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(AuthScheme, AuthHeader);
          }
          return m_Client;
        }
      }
    }


  }

}
