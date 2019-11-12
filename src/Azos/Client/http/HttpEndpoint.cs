using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using Azos.Conf;
using Azos.Web;

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
    /// Physical URI of the endpoint (the physical address of Http endpoint) base address.
    /// The Contract gets appended to this address when making actual calls
    /// </summary>
    [Config]
    public Uri Uri { get; private set; }

    /// <summary> If True, automatically follows HTTP redirect </summary>
    [Config(Default = true)]
    public bool AutoRedirect { get; private set; } = true;

    /// <summary> When set imposes maximum on the redirect count </summary>
    [Config]
    public int? AutoRedirectMax { get; private set; }

    /// <summary> If True, automatically decompresses traffic </summary>
    [Config(Default = true)]
    public bool AutoDecompress { get; private set; } = true;

    [Config(Default = "Basic")]
    public string AuthScheme { get; internal set; } = "Basic";

    [Config]
    public string AuthHeader { get; internal set; }


    /// <summary>
    /// When set to true, attaches Authorization header with sysAuthTOken content, overriding
    /// the AuthHeader value (if any)
    /// </summary>
    [Config]
    public bool AuthImpersonate { get; internal set; }


    [Config(Default = true)]
    public bool AcceptJson { get; internal set; } = true;

    [Config]
    public bool UseCookies { get; internal set; }

    /// <summary> When set imposes maximum content buffer size limit in bytes </summary>
    [Config]
    public int? MaxRequestContentBufferSize { get; private set; }

    /// <summary> When set imposes maximum on the response headers length sent back from server </summary>
    [Config]
    public int? MaxResponseHeadersLength { get; private set; }

    /// <summary> When set imposes maximum on connection count </summary>
    [Config]
    public int? MaxConnections { get; private set; }

    /// <summary>
    /// Returns Http Client which is used to make calls to the remote http endpoint
    /// </summary>
    public HttpClient Client
    {
      get
      {
        EnsureObjectNotDisposed();
        Uri.NonNull("`{0}` is not configured".Args(nameof(Uri)));
        lock (m_Lock)
        {
          if (m_Client == null)
          {
            m_ClientHandler = MakeHttpClientHandler();
            m_Client = MakeHttpClient();
          }
          return m_Client;
        }
      }
    }

    /// <summary>
    /// Override factory to make and configure/build HttpClientHandler instance.
    /// The default implementation allocates HttpClientHandler and sets AllowAutoRedirect.
    /// Attention: This method is called under lock and must not create any blocking conditions
    /// </summary>
    protected virtual HttpClientHandler MakeHttpClientHandler()
    {
      var result = new HttpClientHandler();
      result.AllowAutoRedirect = AutoRedirect;

      if (AutoRedirectMax.HasValue) result.MaxAutomaticRedirections = AutoRedirectMax.Value;
      if (MaxRequestContentBufferSize.HasValue) result.MaxRequestContentBufferSize = MaxRequestContentBufferSize.Value;
      if (MaxResponseHeadersLength.HasValue) result.MaxResponseHeadersLength = MaxResponseHeadersLength.Value;
      if (MaxConnections.HasValue) result.MaxConnectionsPerServer = MaxConnections.Value;

      result.UseCookies = UseCookies;

      result.AutomaticDecompression = AutoDecompress ?
               result.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate :
               result.AutomaticDecompression = DecompressionMethods.None;


      return result;
    }

    /// <summary>
    /// Override factory to make and configure/build HttpClient instance.
    /// The default implementation allocates HttpClient(m_ClientHandler) and conditionally sets Accept: json.; and Authorization header if they are set.
    /// The HttpClient.BaseAddress is set to Uri property.
    /// Attention: This method is called under lock and must not create any blocking conditions
    /// </summary>
    protected virtual HttpClient MakeHttpClient()
    {
      var result = new HttpClient(m_ClientHandler);
      result.Timeout = TimeSpan.FromMilliseconds(TimeoutMs > 0 ? TimeoutMs : this.Service.DefaultTimeoutMs > 0 ? Service.DefaultTimeoutMs : DEFAULT_TIMEOUT_MS);
      result.BaseAddress = this.Uri.NonNull("`{0}` is not configured".Args(nameof(Uri)));

      if (AcceptJson)
        result.DefaultRequestHeaders.Accept.ParseAdd(ContentType.JSON);

      //If impersonation is used, it attaches headers per call obtained from Ambient security context
      if (!AuthImpersonate && AuthHeader.IsNotNullOrWhiteSpace())
        result.DefaultRequestHeaders.Authorization =
          new AuthenticationHeaderValue(AuthScheme, AuthHeader);

      return result;
    }


  }

}
