/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
  /// Provides base implementation of token rings which store tokens server-side
  /// </summary>
  public abstract class ServerTokenRingBase : DaemonWithInstrumentation<IOAuthManager>, ITokenRingImplementation
  {
    public const string CONFIG_PILE_SECTION = "pile";
    public const string CONFIG_CACHE_SECTION = "cache";
    public const int DEFAULT_CACHE_MAX_AGE_SEC = 37;

    protected ServerTokenRingBase(IApplication app) : base(app) => ctor();
    protected ServerTokenRingBase(IOAuthManagerImplementation director) : base(director) => ctor();

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

    private string m_IssuerName;

    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    public override bool InstrumentationEnabled { get; set;}

    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;

    /// <summary> Establishes the issuer name used for new token production </summary>
    [Config]
    public string IssuerName
    {
      get => m_IssuerName.IsNotNullOrWhiteSpace() ? m_IssuerName : Log.Message.DefaultHostName;
      set => m_IssuerName = value;
    }


    #region ITokenRing

    public abstract Task<TToken> GetAsync<TToken>(string token) where TToken : RingToken;

    public abstract Task<string> PutAsync(RingToken token);

    public abstract Task Blacklist(IConfigSectionNode selector);

    public virtual TToken GenerateNew<TToken>() where TToken : RingToken
    {
      var token = Activator.CreateInstance<TToken>();
      var len = token.TokenByteStrength;

      //1. Guid pad is used as a RNG source based on MAC addr/clock
      //https://en.wikipedia.org/w/index.php?title=Universally_unique_identifier&oldid=755882275#Random_UUID_probability_of_duplicates
      var guid = Guid.NewGuid();
      var guidpad = guid.ToNetworkByteOrder();//16 bytes

      ////2. Two independent RNGs are used to avoid library implementation errors affecting token entropy distribution,
      ////so shall an error happen in one (highly unlikely), the other one would still ensure crypto white noise spectrum distribution
      ////the Platform.RandomGenerator is periodically fed external entropy from system and network stack
      //var rnd = Platform.RandomGenerator.Instance.NextRandomBytes(len.min, len.max);
      //var rnd2 = new byte[rnd.Length];
      //m_CryptoRnd.GetBytes(rnd2);
      //for(var i=1; i<rnd.Length; i++) rnd[i] ^= rnd2[i];//both Random streams are combined using XOR

      //2. Random token body
      var rnd = App.SecurityManager.Cryptography.GenerateRandomBytes(len);

      //3. Concat GUid pad with key
      var btoken = guidpad.AppendToNew(rnd);
      token.ID = Convert.ToBase64String(btoken, Base64FormattingOptions.None);

      token.IssuedBy = this.IssuerName;
      token.IssueUtc = App.TimeSource.UTCNow;
      token.ExpireUtc = token.IssueUtc.Value.AddSeconds(token.TokenDefaultExpirationSeconds);

      return token;
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
