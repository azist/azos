/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Security.MinIdp
{
  /// <summary>
  /// Provides in-memory caching wrapper for target IMinIdpStore
  /// </summary>
  public sealed class CacheLayer : DaemonWithInstrumentation<IApplicationComponent>, IMinIdpStoreImplementation, IMinIdpStoreContainer
  {
    public const int DEFAULT_CACHE_AGE_SEC = 30;

    public CacheLayer(IApplicationComponent dir) : base(dir){  }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Store);
      base.Destructor();
    }

    private struct realmed : IEquatable<realmed>
    {
      public realmed(Atom r, string k){   Realm = r;   Key = k; }
      public readonly Atom Realm;
      public readonly string Key;

      public override bool Equals(object obj) => obj is realmed v ? this.Equals(v) : false;
      public override int GetHashCode() => Realm.GetHashCode() ^ (Key==null ? 0 : Key.GetHashCode());
      public bool Equals(realmed other) => this.Realm == other.Realm && this.Key.EqualsOrdSenseCase(other.Key);
    }


    private IMinIdpStoreImplementation m_Store;

    private object m_DataLock = new object();
    private Dictionary<realmed, (DateTime ts, MinIdpUserData d)> m_IdxSysToken = new();
    private Dictionary<realmed, (DateTime ts, MinIdpUserData d)> m_IdxId       = new();
    private Dictionary<realmed, (DateTime ts, MinIdpUserData d)> m_IdxUri      = new();


    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;


    public IMinIdpStore Store => m_Store;

    public ICryptoMessageAlgorithm MessageProtectionAlgorithm => Store.MessageProtectionAlgorithm;


    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    public override bool InstrumentationEnabled { get; set; }

    /// <summary>
    /// Cache age limit in seconds, set to 0 to disable caching
    /// </summary>
    [Config(Default = DEFAULT_CACHE_AGE_SEC), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_SECURITY, CoreConsts.EXT_PARAM_GROUP_CACHE)]
    public int MaxCacheAgeSec { get; set; } = DEFAULT_CACHE_AGE_SEC;


    public async Task<MinIdpUserData> GetByIdAsync(Atom realm, string id, AuthenticationRequestContext ctx = null)
    {
      if (!Running) return null;

      if (id.IsNullOrWhiteSpace()) return null;
      id = id.Trim();  // alex123, alex.123@something.com,  bearer@fbk::h2uIkosifds8Hw_83JIisqap

      if (MaxCacheAgeSec>0)
        lock(m_DataLock)
          if (m_IdxId.TryGetValue(new realmed(realm, id), out var existing)) return existing.d;

      var data = await m_Store.GetByIdAsync(realm, id, ctx).ConfigureAwait(false);
      if(data != null) data.EnteredLoginId = id; //G8 bug #269  NullRefException In MinIdp Bug #269

      updateIndexes(realm, data);
      return data;
    }

    public async Task<MinIdpUserData> GetBySysAsync(Atom realm, string sysToken, AuthenticationRequestContext ctx = null)
    {
      if (!Running) return null;

      if (sysToken.IsNullOrWhiteSpace()) return null;
      sysToken = sysToken.Trim();

      if (MaxCacheAgeSec > 0)
        lock (m_DataLock)
          if (m_IdxSysToken.TryGetValue(new realmed(realm, sysToken), out var existing)) return existing.d;

      var data = await m_Store.GetBySysAsync(realm, sysToken, ctx).ConfigureAwait(false);

      updateIndexes(realm, data);
      return data;
    }

    public async Task<MinIdpUserData> GetByUriAsync(Atom realm, string uri, AuthenticationRequestContext ctx = null)
    {
      if (!Running) return null;

      if (uri.IsNullOrWhiteSpace()) return null;
      uri = uri.Trim();

      if (MaxCacheAgeSec > 0)
        lock (m_DataLock)
          if (m_IdxUri.TryGetValue(new realmed(realm, uri), out var existing)) return existing.d;

      var data = await m_Store.GetByUriAsync(realm, uri, ctx).ConfigureAwait(false);
      if (data != null) data.EnteredUri = uri;

      updateIndexes(realm, data);
      return data;
    }

    private void updateIndexes(Atom realm, MinIdpUserData data)
    {
      if (data==null || !Running) return;

      var maxAge = MaxCacheAgeSec;
      if (maxAge < 1) return;

      var entry = (DateTime.UtcNow.AddSeconds(maxAge), data);
      lock (m_DataLock)
      {
        m_IdxSysToken[new realmed(realm, data.SysToken.Data)] = entry;

        if (data.EnteredLoginId.IsNotNullOrWhiteSpace())
        {
          m_IdxId[new realmed(realm, data.EnteredLoginId)] = entry;
        }

        if (data.EnteredUri.IsNotNullOrWhiteSpace())
        {
          m_IdxUri[new realmed(realm, data.EnteredUri)]    = entry;
        }
      }
    }

    private void scan()
    {
      const int RESCAN_MS = 2500;
      try
      {
        scanOnce();
      }
      catch(Exception error)
      {
        WriteLog(Log.MessageType.Error, nameof(scan), "Leaked: "+error.ToMessageWithType(), error);
      }

      if (Running)
        Task.Delay(RESCAN_MS).ContinueWith(_ => scan());
    }

    private void scanOnce()
    {
      if (!Running) return;

      void scanIndex(Dictionary<realmed, (DateTime ts, MinIdpUserData d)> idx, DateTime dod)
      {
        var toKill = new List<realmed>();
        foreach (var kvp in idx)
        {
          if (kvp.Value.ts > dod) continue;
          toKill.Add(kvp.Key);
        }

        toKill.ForEach(one => idx.Remove(one));
      }

      var now = DateTime.UtcNow;
      lock(m_DataLock)
      {
        scanIndex(m_IdxSysToken, now);
        scanIndex(m_IdxId, now);
        scanIndex(m_IdxUri, now);
      }
    }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      DisposeAndNull(ref m_Store);
      if (node==null) return;

      m_Store = FactoryUtils.MakeAndConfigureDirectedComponent<IMinIdpStoreImplementation>(
                                              this,
                                              node[MinIdpSecurityManager.CONFIG_STORE_SECTION]);
    }

    protected override void DoStart()
    {
      m_Store.NonNull($"{nameof(Store)} config")
             .Start();

      scan();
    }

    protected override void DoSignalStop()
    {
      m_Store.SignalStop();
    }

    protected override void DoWaitForCompleteStop()
    {
      m_Store.WaitForCompleteStop();
      lock(m_DataLock)
      {
        m_IdxId.Clear();
        m_IdxSysToken.Clear();
        m_IdxUri.Clear();
      }
    }

  }
}
