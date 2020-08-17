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
  public sealed class CacheLayer : DaemonWithInstrumentation<MinIdpSecurityManager>, IMinIdpStoreImplementation
  {
    public CacheLayer(MinIdpSecurityManager dir) : base(dir)
    {
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_Store);
    }


    private IMinIdpStoreImplementation m_Store;

    private object m_DataLock = new object();
    private Dictionary<string, (DateTime ts, MinIdpUserData d)> m_IdxId;
    private Dictionary<SysAuthToken, MinIdpUserData> m_IdxSysToken;
    private Dictionary<string, MinIdpUserData> m_IdxUri;


    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;


    public IMinIdpStore Store => m_Store;


    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    public override bool InstrumentationEnabled { get; set; }

    /// <summary>
    /// Cache age limit in seconds, set to 0 to disable caching
    /// </summary>
    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_SECURITY, CoreConsts.EXT_PARAM_GROUP_CACHE)]
    public int MaxCacheAgeSec { get; set; }


    public async Task<MinIdpUserData> GetByIdAsync(string id)
    {
      lock(m_DataLock)
        if (m_IdxId.TryGetValue(id, out var existing)) return existing.d;

      var data = await m_Store.GetByIdAsync(id);

      updateIndexes(data);
      return data;
    }

    public async Task<MinIdpUserData> GetBySysAsync(SysAuthToken sysToken)
    {
      lock (m_DataLock)
        if (m_IdxSysToken.TryGetValue(sysToken, out var existing)) return existing;

      var data = await m_Store.GetBySysAsync(sysToken);

      updateIndexes(data);
      return data;
    }

    public async Task<MinIdpUserData> GetByUriAsync(string uri)
    {
      lock (m_DataLock)
        if (m_IdxUri.TryGetValue(uri, out var existing)) return existing;

      var data = await m_Store.GetByUriAsync(uri);

      updateIndexes(data);
      return data;
    }

    private void updateIndexes(MinIdpUserData data)
    {
      if (data==null) return;

      var maxAge = MaxCacheAgeSec;
      if (maxAge < 1) return;

      var entry = (DateTime.UtcNow.AddSeconds(maxAge), data);
      lock (m_DataLock)
      {
        m_IdxId[data.LoginId] = entry;
        m_IdxSysToken[data.SysToken] = data;
        m_IdxUri[data.ScreenName] = data;
      }
    }

    private void scan()
    {
      const int RESCAN_MS = 1500;
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
            m_IdxId.Remove(item.LoginId);
            m_IdxSysToken.Remove(item.SysToken);
            m_IdxUri.Remove(item.ScreenName);
          }
        }
      }

    }


    protected override void DoStart()
    {
      m_Store.NonNull($"{nameof(Store)} config").Start();
      scan();
    }

    protected override void DoSignalStop()
    {
      m_Store.SignalStop();
    }

    protected override void DoWaitForCompleteStop()
    {
      m_Store.WaitForCompleteStop();
    }

  }
}
