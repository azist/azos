using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;

using Azos.Sky.Social.Trending.Server;
using Azos.Sky.Coordination;

namespace Azos.Sky.Social.Trending
{
  public class TrendingManager : DisposableObject, IApplicationStarter, IApplicationFinishNotifiable
  {
    #region ctor

    private static object s_Lock = new object();
    private static volatile TrendingManager s_Instance;

    public static TrendingManager Instance
    {
      get
      {
        var instance = s_Instance;
        if (instance == null)
          throw new SocialException(StringConsts.TS_INSTANCE_DATA_LAYER_IS_NOT_ALLOCATED_ERROR.Args(typeof(TrendingSystemService).Name));
        return instance;
      }
    }

    private TrendingManager()
    {
      lock (s_Lock)
      {
        if (s_Instance != null)
          throw new SocialException(StringConsts.TS_INSTANCE_ALREADY_ALLOCATED_ERROR.Args(GetType().Name));
        s_Instance = this;
      }
    }

    protected override void Destructor()
    {
      lock (s_Lock)
      {
        base.Destructor();
        s_Instance = null;
      }
    }

    #endregion

    #region fields

    private IConfigSectionNode m_Config;
    private HostSet m_HostSet;

    #endregion

    #region properties

    public bool ApplicationStartBreakOnException {get { return true; } }
    public string Name { get { return GetType().Name; } }
    public HostSet HostSet { get { return m_HostSet; } }

    #endregion

    #region public

    public void Configure(IConfigSectionNode node)
    {
      m_Config = node;
    }

    public void ApplicationFinishAfterCleanup(IApplication application)
    {
    }

    public void ApplicationFinishBeforeCleanup(IApplication application)
    {
    }

    public void ApplicationStartAfterInit(IApplication application)
    {
      var trendingHostSetAttr = m_Config.AttrByName(SocialConsts.CONFIG_TRENDING_HOST_SET_ATTR).Value;
      m_HostSet = SkySystem.ProcessManager.HostSets[trendingHostSetAttr];
      if (m_HostSet == null) throw new SocialException(StringConsts.TS_HOST_SET_NOT_CONFIG_ERROR.Args(SocialConsts.CONFIG_TRENDING_HOST_SET_ATTR));
    }

    public void ApplicationStartBeforeInit(IApplication application)
    {
    }

    // ITrendingSystem

    public IEnumerable<TrendingEntity> GetTrending(TrendingQuery query)
    {
      var pair = HostSet.AssignHost(App.TimeSource.UTCNow.Ticks);
      return Contracts.ServiceClientHub.CallWithRetry<ITrendingSystemClient, IEnumerable<TrendingEntity>>(trending => trending.GetTrending(query), pair.Select(host => host.RegionPath));
    }

    #endregion
  }
}
