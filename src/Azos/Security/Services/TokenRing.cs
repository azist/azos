using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Data;
using Azos.Instrumentation;
using Azos.Pile;

namespace Azos.Security.Services
{
  /// <summary>
  /// Provides base implementation of token management services
  /// </summary>
  public abstract class TokenRing : DaemonWithInstrumentation<IOAuthManager>, ITokenRingImplementation
  {
    public const string CONFIG_PILE_SECTION = "pile";
    public const string CONFIG_CACHE_SECTION = "cache";
    public const int DEFAULT_CACHE_MAX_AGE_SEC = 37;

    public TokenRing(IApplication app) : base(app) => ctor();
    public TokenRing(IOAuthManagerImplementation director) : base(director) => ctor();

    private void ctor()
    {
      m_Pile = new DefaultPile(this, "token-ring-pile"){ SegmentSize = 64 * 1024 * 1024};
      m_Cache = new LocalCache(this, "token-ring-cache", m_Pile);
      m_Cache.DefaultTableOptions.DefaultMaxAgeSec = DEFAULT_CACHE_MAX_AGE_SEC;
    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Cache);
      DisposeAndNull(ref m_Pile);
      base.Destructor();
    }

    private DefaultPile m_Pile;
    private LocalCache m_Cache;

    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    public override bool InstrumentationEnabled { get; set;}

    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;


    #region ITokenRing
    public abstract TToken GenerateNewToken<TToken>() where TToken : RingToken;

    public abstract Task InvalidateAccessToken(string accessToken);

    public abstract Task InvalidateClient(string clientID);

    public abstract Task InvalidateSubject(AuthenticationToken token);

    public abstract Task<AccessToken> IssueAccessToken(User userClient, User targetUser);

    public abstract Task<ClientAccessCodeToken> LookupClientAccessCodeAsync(string accessCode);

    public AuthenticationToken? MapAccessToken(string accessToken)
    {
      if (!Running) return null;
      var tbl = GetCacheTableOf(typeof(AccessToken));

      var utcNow = App.TimeSource.UTCNow;//important to use accurate time source
      var cached = tbl.Get(accessToken.NonBlank(nameof(accessToken)));
      if (cached is AbsentValue) return null;
      var access  = cached as AccessToken;
      if (access==null)
      {
        access = DoFetchAccessToken(accessToken, utcNow);
        tbl.Put(accessToken, (object)access ?? AbsentValue.Instance);//does not exist in the store
        if (access==null)
          return null;//does not exist, AbsentData
      }

      if ((access.ExpireUtc ?? DateTime.MinValue) < utcNow) return null;//expired

      var content = access.SubjectAuthenticationToken;
      if (content.IsNullOrWhiteSpace()) return null;
      var result = MapSubjectAuthenticationTokenFromContent(content);
      return result;
    }

    public abstract AuthenticationToken MapSubjectAuthenticationTokenFromContent(string content);

    public abstract string  MapSubjectAuthenticationTokenToContent(AuthenticationToken token);

    public string TargetAuthenticationTokenToContent(AuthenticationToken token)
    {
      throw new NotImplementedException();
    }
    #endregion

    #region Protected

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      if (node == null) return;

      m_Pile.Configure(node[CONFIG_PILE_SECTION]);
      m_Cache.Configure(node[CONFIG_CACHE_SECTION]);
    }

    protected override void DoStart()
    {
      base.DoStart();
      m_Pile.Start();
      m_Cache.Start();
    }

    protected override void DoSignalStop()
    {
      base.DoSignalStop();
      m_Cache.SignalStop();
      m_Pile.SignalStop();
    }

    protected override void DoWaitForCompleteStop()
    {
      m_Cache.WaitForCompleteStop();
      m_Pile.WaitForCompleteStop();
      base.DoWaitForCompleteStop();

    }

    private static volatile Dictionary<Type, string> s_TableNames = new Dictionary<Type, string>();

    protected ICacheTable<string> GetCacheTableOf(Type ttoken)
    {
      if (!s_TableNames.TryGetValue(ttoken, out var name))
      {
        var schema = Schema.GetForTypedDoc(ttoken);
        name = schema.GetTableAttrForTarget(null).Name;
        var dict = new Dictionary<Type, string>(s_TableNames);
        dict[ttoken] = name;
        s_TableNames = dict;
      }
      return m_Cache.GetOrCreateTable<string>(name, StringComparer.Ordinal);
    }


    /// <summary>
    /// Performs physical fetch of access token from the store.
    /// The tokens that have expired already or marked as deleted shall not be fetched (return null as if they don't exist)
    /// </summary>
    protected abstract AccessToken DoFetchAccessToken(string accessToken, DateTime utcNow);


    #endregion

  }
}
