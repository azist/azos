using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;

using Azos.IAM.Protocol;
using Azos.Web;

namespace Azos.IAM.Client
{
  public sealed partial class IAMClient : ModuleBase, IAdminLogic, IAuthLogic
  {
    /// <summary>
    /// Provides information for connection to a remote IAM server
    /// </summary>
    public sealed class ServerLocation : DisposableObject, INamed, IOrdered
    {
      public const string CONFIG_SERVER_SECTION = "server";
      public const int MIN_TIMEOUT_SEC = 1;
      public const int DFLT_TIMEOUT_SEC = 5;


      internal ServerLocation(IAMClient owner, IConfigSectionNode cfg)
      {
        Owner = owner;
        ConfigAttribute.Apply(this, cfg.NonEmpty(nameof(cfg)));
        if (Uri==null) throw new IAMException(StringConsts.ARGUMENT_ERROR+"ServerLocation.ctor(uri==null)");
        if (Name.IsNullOrWhiteSpace()) Name = Uri.AbsolutePath;
      }

      protected override void Destructor()
      {
        DisposeAndNull(ref m_Client);
        DisposeAndNull(ref m_ClientHandler);
        base.Destructor();
      }

      public IAMClient Owner { get; private set; }

      private int m_TimeoutSec = DFLT_TIMEOUT_SEC;

      /// <summary> Provides a name that uniquely identifies this server location, if not configured, defaults to Uri, e.g. "Primary" </summary>
      [Config]
      public string Name   { get; private set; }

      /// <summary> Defines relative order/precedence of connection </summary>
      [Config]
      public int    Order  { get; private set; }

      /// <summary> Uri of the IAM server to connect to, e.g. "https://contoso.me/iam" </summary>
      [Config]
      public Uri    Uri    { get; private set; }

      /// <summary> If True, automatically follows HTTP redirect </summary>
      [Config(Default = true)]
      public bool  AutoRedirect{  get; private set; } = true;

      /// <summary> Call timeout in seconds </summary>
      [Config(Default = DFLT_TIMEOUT_SEC)]
      public int TimeoutSec
      {
        get => m_TimeoutSec;
        set => m_TimeoutSec = value < MIN_TIMEOUT_SEC ? MIN_TIMEOUT_SEC : value;
      }



      private object m_Lock = new object();
      private HttpClientHandler m_ClientHandler;
      private HttpClient m_Client;

      internal HttpClient GetClient()
      {
        EnsureObjectNotDisposed();
        lock(m_Lock)
        {
          if (m_Client==null)
          {
            m_ClientHandler = new HttpClientHandler();
            m_ClientHandler.AllowAutoRedirect = AutoRedirect;
            //configure client handler
            m_Client = new HttpClient(m_ClientHandler);
            m_Client.Timeout = TimeSpan.FromSeconds(TimeoutSec);
            m_Client.BaseAddress = this.Uri;
            m_Client.DefaultRequestHeaders.Accept.ParseAdd(ContentType.JSON);
            m_Client.DefaultRequestHeaders.Authorization =
              new AuthenticationHeaderValue(Owner.AuthScheme, Owner.AuthHeader);
          }
          return m_Client;
        }
      }
    }

    public IAMClient(IApplication app) : base(app) { }
    public IAMClient(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      m_ServerLocations.ForEach( l => l.Dispose() );
      base.Destructor();
    }

    private OrderedRegistry<ServerLocation> m_ServerLocations = new OrderedRegistry<ServerLocation>();


    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;

    /// <summary>
    /// Registry of server locations tried in sequence if service becomes unavailable
    /// </summary>
    public IOrderedRegistry<ServerLocation> ServerLocations => m_ServerLocations;

    [Config(Default ="Basic")]
    public string AuthScheme {  get; internal set; } = "Basic";

    [Config]
    public string AuthHeader { get; internal set; }


    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      foreach(var snode in node.Children.Where(c => c.IsSameName(ServerLocation.CONFIG_SERVER_SECTION)))
      {
        var loc = new ServerLocation(this, snode);
        if (!m_ServerLocations.Register(loc))
          throw new IAMException("Duplicate server  `{0}`".Args(loc.Name));
      }
    }

    private async Task<TResult> callWithRetry<TResult>(Func<ServerLocation, Task<TResult>> body)
    {
      foreach(var server in ServerLocations.OrderedValues)
      {
        try
        {
          var request = await body(server);
          return request;
        }
        catch (Exception error)
        {
          if (error is TaskCanceledException cerr) continue;
        }
      }
      throw new IAMException("Call eventually failed");  //todo: log/instrument?
    }


  }
}
