using Azos.Apps;
using Azos.Conf;

using Azos.Sky.Mdb;
using Azos.Sky.WebMessaging;


namespace Azos.Sky.Social.Graph.Server
{
  /// <summary>
  /// Provides context for graph operations: graph config, datastore, and host
  /// </summary>
  public sealed class GraphOperationContext : ApplicationComponent, IApplicationStarter, IApplicationFinishNotifiable
  {

    private static object s_Lock = new object();
    private static volatile GraphOperationContext s_Instance;

    public static GraphOperationContext Instance
    {
      get
      {
        var instance = s_Instance;
        if (instance == null)
          throw new GraphException(StringConsts.GS_INSTANCE_DATA_LAYER_IS_NOT_ALLOCATED_ERROR.Args(typeof(GraphSystemService).Name));
        return instance;
      }
    }

    private GraphOperationContext()
    {
      lock (s_Lock)
      {
        if (s_Instance != null)
          throw new WebMessagingException(StringConsts.GS_INSTANCE_ALREADY_ALLOCATED_ERROR.Args(GetType().Name));
        s_Instance = this;
      }
    }

    protected override void Destructor()
    {
      lock (s_Lock)
      {
        base.Destructor();

        DisposableObject.DisposeAndNull(ref m_GraphHost);
        DisposableObject.DisposeAndNull(ref m_DataStore);

        s_Instance = null;
      }
    }

    private IConfigSectionNode m_Config;
    private MdbDataStore m_DataStore;
    private GraphHost m_GraphHost;

    public string Name { get { return GetType().Name; } }
    public IMdbDataStore DataStore {get { return m_DataStore; }}
    public GraphHost GraphHost { get { return m_GraphHost; }}

    bool IApplicationStarter.ApplicationStartBreakOnException { get { return true; } }

    void IApplicationStarter.ApplicationStartBeforeInit(IApplication application) {}

    void IApplicationStarter.ApplicationStartAfterInit(IApplication application)
    {
      application.RegisterAppFinishNotifiable(this);

      var nDataSore = m_Config[SocialConsts.CONFIG_DATA_STORE_SECTION];
      if(!nDataSore.Exists) throw new SocialException(StringConsts.GS_INIT_NOT_CONF_ERRROR.Args(this.GetType().Name, SocialConsts.CONFIG_DATA_STORE_SECTION));
      m_DataStore = FactoryUtils.MakeAndConfigure<MdbDataStore>(nDataSore, args: new object[] { "GraphSystem", this });
      m_DataStore.Start();

      var nHost = m_Config[SocialConsts.CONFIG_GRAPH_HOST_SECTION];
      if (!nHost.Exists) throw new SocialException(StringConsts.GS_INIT_NOT_CONF_ERRROR.Args(this.GetType().Name, SocialConsts.CONFIG_GRAPH_HOST_SECTION));
      m_GraphHost = FactoryUtils.MakeAndConfigure<GraphHost>(nHost, args: new object[]{ this, nHost });
    }

    void IApplicationFinishNotifiable.ApplicationFinishBeforeCleanup(IApplication application)
    {
      Dispose();
    }

    void IApplicationFinishNotifiable.ApplicationFinishAfterCleanup(IApplication application) { }

    void IConfigurable.Configure(IConfigSectionNode node)
    {
      m_Config = node;

      ConfigAttribute.Apply(this, node);
    }
  }
}