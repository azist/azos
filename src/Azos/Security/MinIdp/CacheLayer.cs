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

    }


    private IMinIdpStore m_Store;

    private object m_DataLock = new object();
    private Dictionary<string, MinIdpUserData> m_IdxId;
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
      MinIdpUserData data;

      lock(m_DataLock)
        if (m_IdxId.TryGetValue(id, out data)) return data;

      data = await m_Store.GetByIdAsync(id);

      updateIndexes(data);
      return data;
    }

    public async Task<MinIdpUserData> GetBySysAsync(SysAuthToken sysToken)
    {
      MinIdpUserData data;

      lock (m_DataLock)
        if (m_IdxSysToken.TryGetValue(sysToken, out data)) return data;

      data = await m_Store.GetBySysAsync(sysToken);

      updateIndexes(data);
      return data;
    }

    public async Task<MinIdpUserData> GetByUriAsync(string uri)
    {
      MinIdpUserData data;

      lock (m_DataLock)
        if (m_IdxUri.TryGetValue(uri, out data)) return data;

      data = await m_Store.GetByUriAsync(uri);

      updateIndexes(data);
      return data;
    }

    private void updateIndexes(MinIdpUserData data)
    {
      lock (m_DataLock)
      {
        m_IdxId[data.Id] = data;
        m_IdxSysToken[data.SysToken] = data;
        m_IdxUri[data.ScreenName] = data;
      }
    }
  }
}
