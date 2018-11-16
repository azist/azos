using System;

using Azos.Apps;
using Azos.Pile;
using Azos.Data;
using Azos.Conf;
using Azos.Log;
using Azos.Instrumentation;

using Azos.Sky.Mdb;
using Azos.Sky.WebMessaging;

namespace Azos.Sky.Social.Graph.Server
{
  public partial class GraphSystemService : DaemonWithInstrumentation<object>
    , IGraphEventSystem
    , IGraphFriendSystem
    , IGraphNodeSystem
    , IGraphCommentSystem
  {
    #region CONSTS

    public const int DEFAULT_EVENT_DELIVERY_COHORT_SIZE = 32;
    public const int MAX_SIZE_COMMENT_VOLUME = 1000;
    public const int MAX_SCAN_TAIL_COMMENT_VOLUMES = 2;

    #endregion

    #region STATIC/.ctor
    private static object s_Lock = new object();
    private static volatile GraphSystemService s_Instance;


    internal static GraphSystemService Instance
    {
      get
      {
        var instance = s_Instance;
        if (instance == null)
          throw new GraphException(StringConsts.GS_INSTANCE_NOT_ALLOCATED_ERROR.Args(typeof(GraphSystemService).Name));
        return instance;
      }
    }

    public GraphSystemService(object director) : base(director)
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
        s_Instance = null;
      }
    }

    #endregion

    #region fields

    private GraphCommentFetchDefaultStrategy m_GraphCommentFetchStrategy;

    #endregion

    #region properties

    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled { get; set; }

    internal GraphCommentFetchDefaultStrategy GraphCommentFetchStrategy { get { return m_GraphCommentFetchStrategy; } }

    internal GraphHost         GraphHost    { get { return GraphOperationContext.Instance.GraphHost; }}
    internal IMdbDataStore     DataStore    { get { return GraphOperationContext.Instance.DataStore; }}
    internal ICache            Cache        { get { return DataStore.Cache;}}

    /// <summary>
    /// Defines the number of EventDeliveryTodo instances executing in parallel
    /// </summary>
    internal int EventDeliveryCohortSize   { get { return DEFAULT_EVENT_DELIVERY_COHORT_SIZE;} }

    /// <summary>
    /// Sets the limit of the number of comments per comment volume
    /// </summary>
    internal int MaxSizeCommentVolume  { get { return MAX_SIZE_COMMENT_VOLUME; }}

    /// <summary>
    /// Specifies how many tail comment volumes should be scanned for detection of duplicates
    /// </summary>
    internal int MaxScanTailCommentVolumes { get { return MAX_SCAN_TAIL_COMMENT_VOLUMES; }}

    #endregion

    #region public

    public CRUDOperations ForNode(GDID gNode)
    {
      return DataStore.PartitionedOperationsFor(SocialConsts.MDB_AREA_NODE, gNode);
    }

    #endregion

    #region protected

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      if (node == null || !node.Exists) return;

      var nGraphComment = node[SocialConsts.CONFIG_GRAPH_COMMENT_FETCH_STRATEGY_SECTION];
      if (!nGraphComment.Exists) throw new SocialException(StringConsts.GS_INIT_NOT_CONF_ERRROR.Args(this.GetType().Name, SocialConsts.CONFIG_GRAPH_COMMENT_FETCH_STRATEGY_SECTION));
      m_GraphCommentFetchStrategy = FactoryUtils.MakeAndConfigure<GraphCommentFetchDefaultStrategy>(nGraphComment, args: new object[] { this, nGraphComment });

    }

    protected override void DoStart()
    {
      base.DoStart();
    }

    protected override void DoSignalStop()
    {
      base.DoSignalStop();
    }

    protected override void DoWaitForCompleteStop()
    {
      base.DoWaitForCompleteStop();
    }

    protected void Log(MessageType type, string from, string text, Exception error = null, Guid? related = null)
    {
      var msg = new Message
      {
        Type = type,
        Topic = SysConsts.LOG_TOPIC_APP_MANAGEMENT,
        From = "{0}.{1}".Args(GetType().FullName, from),
        Text = text,
        Exception = error
      };

      if (related.HasValue) msg.RelatedTo = related.Value;

      App.Log.Write( msg );
    }

    #endregion
  }
}