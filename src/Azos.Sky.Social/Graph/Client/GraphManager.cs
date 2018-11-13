using System;

using Azos.Apps;
using Azos.Conf;
using Azos.Sky.Social.Graph.Server;

namespace Azos.Sky.Social.Graph
{
  /// <summary>
  /// Client facade for consuming IGraphNodeSystem, IGraphCommentSystem, IGraphEventSystem, IGraphFriendSystem
  /// </summary>
  public sealed class GraphManager : DisposableObject,  IApplicationStarter, IApplicationFinishNotifiable
  {
    #region ctor
    private static object s_Lock = new object();
    private static volatile GraphManager s_Instance;

    public static GraphManager Instance
    {
      get
      {
        var instance = s_Instance;
        if (instance == null)
          throw new GraphException(StringConsts.GS_INSTANCE_DATA_LAYER_IS_NOT_ALLOCATED_ERROR.Args(typeof(GraphSystemService).Name));
        return instance;
      }
    }

    private GraphManager()
    {
      lock (s_Lock)
      {
        if (s_Instance != null)
          throw new GraphException(StringConsts.GS_INSTANCE_ALREADY_ALLOCATED_ERROR.Args(GetType().Name));
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

    private GraphNodeManagerBase m_Nodes;
    private GraphCommentManagerBase m_Comments;
    private GraphEventManagerBase m_Events;
    private GraphFriendManagerBase m_Friends;

    #endregion

    #region properties

    public IGraphNodeSystem Nodes {get { return m_Nodes; }}
    public IGraphCommentSystem Comments {get { return m_Comments; }}
    public IGraphEventSystem Events { get { return m_Events; }}
    public IGraphFriendSystem Friends {get { return m_Friends; }}

    public bool ApplicationStartBreakOnException {get { return true; } }
    public string Name { get { return GetType().Name; } }


    #endregion

    public void Configure(IConfigSectionNode node)
    {
      m_Config = node;
    }

    public void ApplicationStartBeforeInit(IApplication application)
    {
    }

    public void ApplicationStartAfterInit(IApplication application)
    {
      var nodeHostSetAttr = m_Config.AttrByName(SocialConsts.CONFIG_GRAPH_NODE_HOST_SET_ATTR).Value;
      var commentHostSetAttr = m_Config.AttrByName(SocialConsts.CONFIG_GRAPH_COMMENT_HOST_SET_ATTR).Value;
      var eventHostSetAttr = m_Config.AttrByName(SocialConsts.CONFIG_GRAPH_EVENT_HOST_SET_ATTR).Value;
      var friendHostSetAttr = m_Config.AttrByName(SocialConsts.CONFIG_GRAPH_FRIEND_HOST_SET_ATTR).Value;


      m_Nodes = nodeHostSetAttr != null ? (GraphNodeManagerBase) new GraphNodeManager(SkySystem.ProcessManager.HostSets[nodeHostSetAttr]) : new NOPGraphNodeManager(null);
      m_Comments = commentHostSetAttr != null
        ? (GraphCommentManagerBase) new GraphCommentManager(SkySystem.ProcessManager.HostSets[commentHostSetAttr])
        : new NOPGraphCommentManager(null);
      m_Events = eventHostSetAttr != null ? (GraphEventManagerBase) new GraphEventManager(SkySystem.ProcessManager.HostSets[eventHostSetAttr]) : new NOPGraphEventManager(null);
      m_Friends = friendHostSetAttr != null ? (GraphFriendManagerBase) new GraphFriendManager(SkySystem.ProcessManager.HostSets[friendHostSetAttr]) : new NOPGraphFriendManager(null);

    }

    public void ApplicationFinishBeforeCleanup(IApplication application)
    {
      DisposableObject.DisposeAndNull(ref m_Nodes);
      DisposableObject.DisposeAndNull(ref m_Comments);
      DisposableObject.DisposeAndNull(ref m_Events);
      DisposableObject.DisposeAndNull(ref m_Friends);
    }

    public void ApplicationFinishAfterCleanup(IApplication application)
    {
    }
  }
}