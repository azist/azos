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
    private Dictionary<realmed, (DateTime ts, MinIdpUserData d)> m_IdxId = new Dictionary<realmed, (DateTime ts, MinIdpUserData d)>();
    private Dictionary<realmed, MinIdpUserData> m_IdxSysToken = new Dictionary<realmed, MinIdpUserData>();
    private Dictionary<realmed, MinIdpUserData> m_IdxUri = new Dictionary<realmed, MinIdpUserData>();


    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;


    public IMinIdpStore Store => m_Store;


    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    public override bool InstrumentationEnabled { get; set; }

    /// <summary>
    /// Cache age limit in seconds, set to 0 to disable caching
    /// </summary>
    [Config(Default = DEFAULT_CACHE_AGE_SEC), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_SECURITY, CoreConsts.EXT_PARAM_GROUP_CACHE)]
    public int MaxCacheAgeSec { get; set; } = DEFAULT_CACHE_AGE_SEC;


    public async Task<MinIdpUserData> GetByIdAsync(Atom realm, string id)
    {
      if (!Running) return null;

      if (id.IsNullOrWhiteSpace()) return null;
      id = id.ToLowerInvariant();

      if (MaxCacheAgeSec>0)
        lock(m_DataLock)
          if (m_IdxId.TryGetValue(new realmed(realm, id), out var existing)) return existing.d;

      var data = await m_Store.GetByIdAsync(realm, id);

      updateIndexes(realm, data);
      return data;
    }

    public async Task<MinIdpUserData> GetBySysAsync(Atom realm, string sysToken)
    {
      if (!Running) return null;

      if (sysToken.IsNullOrWhiteSpace()) return null;

      if (MaxCacheAgeSec > 0)
        lock (m_DataLock)
          if (m_IdxSysToken.TryGetValue(new realmed(realm, sysToken), out var existing)) return existing;

      var data = await m_Store.GetBySysAsync(realm, sysToken);

      updateIndexes(realm, data);
      return data;
    }

    public async Task<MinIdpUserData> GetByUriAsync(Atom realm, string uri)
    {
      if (!Running) return null;

      if (uri.IsNullOrWhiteSpace()) return null;

      if (MaxCacheAgeSec > 0)
        lock (m_DataLock)
          if (m_IdxUri.TryGetValue(new realmed(realm, uri), out var existing)) return existing;

      var data = await m_Store.GetByUriAsync(realm, uri);

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
        m_IdxId      [new realmed(realm, data.LoginId)]       = entry;
        m_IdxSysToken[new realmed(realm, data.SysToken.Data)] = data;
        m_IdxUri     [new realmed(realm, data.ScreenName)]    = data;
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

      var toKill = new List<MinIdpUserData>();
      var now = DateTime.UtcNow;
      lock(m_DataLock)
      {
        foreach(var kvp in m_IdxId)
        {
          if (kvp.Value.ts > now) continue;
          toKill.Add(kvp.Value.d);
        }

        if (toKill.Count>0)
        {
          foreach(var item in toKill)
          {
            m_IdxId.Remove(new realmed(item.Realm, item.LoginId));
            m_IdxSysToken.Remove(new realmed(item.Realm, item.SysToken.Data));
            m_IdxUri.Remove(new realmed(item.Realm, item.ScreenName));
          }
        }
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
