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
  public sealed class CacheLayer : ApplicationComponent, IMinIdpStore
  {
    public CacheLayer(IApplicationComponent dir, IConfigSectionNode cfg) : base(dir)
    {
      scan();
    }


    private IMinIdpStore m_Store;

    private object m_DataLock = new object();
    private Dictionary<string, (DateTime ts, MinIdpUserData d)> m_IdxId;
    private Dictionary<SysAuthToken, MinIdpUserData> m_IdxSysToken;
    private Dictionary<string, MinIdpUserData> m_IdxUri;


    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;

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
        m_IdxId[data.Id] = entry;
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

      if (!Disposed || App.Active)
        Task.Delay(RESCAN_MS).ContinueWith(_ => scan());
    }

    private void scanOnce()
    {
      if (Disposed || !App.Active) return;

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
            m_IdxId.Remove(item.Id);
            m_IdxSysToken.Remove(item.SysToken);
            m_IdxUri.Remove(item.ScreenName);
          }
        }
      }

    }

  }
}
