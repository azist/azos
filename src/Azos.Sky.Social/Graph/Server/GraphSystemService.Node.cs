using System;

using Azos.Pile;
using Azos.Data;
using Azos.Log;
using Azos.Serialization.JSON;

using Azos.Sky.Social.Graph.Server.Data;
using Azos.Sky.Social.Graph.Server.Data.Schema;
using Azos.Sky.Social.Graph.Server.Workers;

namespace Azos.Sky.Social.Graph.Server
{
  public partial class GraphSystemService
  {
    /// <summary>
    /// Saves the GraphNode instances into the system.
    /// If a node with such ID already exists, updates it, otherwise creates new node
    /// </summary>
    public GraphChangeStatus SaveNode(GraphNode node)
    {
      try
      {
        return DoSaveNode(node);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "SaveNode", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_SAVE_NODE_ERROR.Args(node.ToJSON()), ex);
      }
    }

    /// <summary>
    /// Fetches the GraphNode by its unique GDID or null if not found
    /// </summary>
    public GraphNode GetNode(GDID gNode)
    {
      try
      {
        return DoGetNode(gNode);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "GetNode", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_GET_NODE_ERROR.Args(gNode.ToString()), ex);
      }
    }

    /// <summary>
    /// Deletes node by GDID
    /// </summary>
    public GraphChangeStatus DeleteNode(GDID gNode)
    {
      try
      {
        return DoDeleteNode(gNode);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "DeleteNode", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_DELETE_NODE_ERROR.Args(gNode.ToString()), ex);
      }
    }

    /// <summary>
    /// Undeletes node by GDID
    /// </summary>
    public GraphChangeStatus UndeleteNode(GDID gNode)
    {
      try
      {
        return DoUndeleteNode(gNode);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "UndeleteNode", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_DELETE_NODE_ERROR.Args(gNode.ToString()), ex);
      }
    }

    /// <summary>
    /// Remove node by GDID from Databases
    /// </summary>
    public GraphChangeStatus RemoveNode(GDID gNode)
    {
      try
      {
        return DoRemoveNode(gNode);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "RemoveNode", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_DELETE_NODE_ERROR.Args(gNode.ToString()), ex);
      }
    }


    protected virtual GraphChangeStatus DoRemoveNode(GDID gNode)
    {
      var removeFriendsTodo = new EventRemoveFriendsTodo()
      {
        G_Node = gNode,
        FriendIndex = 0,
        G_Friend = GDID.Zero
      };
      SocialGraphTodos.EnqueueRemove(removeFriendsTodo);

      var removeNodeTodo = new EventRemoveNodeTodo()
      {
        G_Node = gNode,
        VolumeIndex = 0,
        G_Volume = GDID.Zero
      };
      SocialGraphTodos.EnqueueRemove(removeNodeTodo);

      return GraphChangeStatus.Unassigned;
    }

    private GraphChangeStatus DoUndeleteNode(GDID gNode)
    {
      var qry = Queries.ChangeInUseNodeByGDID<NodeRow>(gNode, isDel:false);
      var affected = ForNode(gNode).ExecuteWithoutFetch(qry);
      return (affected>0) ? GraphChangeStatus.Updated : GraphChangeStatus.NotFound;
    }

    protected virtual GraphChangeStatus DoSaveNode(GraphNode node)
    {
      GraphChangeStatus result;

      var row = loadNodeRow(node.GDID);
      if (row == null)
      {
        row = new NodeRow(false)
        {
          GDID = node.GDID,
          In_Use = true,
          Node_Type = node.NodeType,
          G_OriginShard = node.G_OriginShard,
          G_Origin = node.G_Origin,
          Create_Date = node.TimestampUTC
        };
        result = GraphChangeStatus.Added;
      }
      else
        result = GraphChangeStatus.Updated;

      row.Origin_Name = node.OriginName;
      row.Friend_Visibility = GSFriendVisibility.ToDomainString(node.DefaultFriendVisibility);

      ForNode(node.GDID).Upsert(row);

      return result;
    }

    protected virtual GraphNode DoGetNode(GDID gNode,  ICacheParams cacheParams = null)
    {
      var row = Cache.FetchThrough(gNode,
        SocialConsts.GS_NODE_TBL,
        cacheParams,
        gdid => loadNodeRow(gNode));

      if (row == null) return new GraphNode();

      return new GraphNode(row.Node_Type,
                           row.GDID,
                           row.G_OriginShard,
                           row.G_Origin,
                           row.Origin_Name,
                           row.Origin_Data,
                           row.Create_Date.Value,
                           GSFriendVisibility.ToFriendVisibility(row.Friend_Visibility));
    }

    protected virtual GraphChangeStatus DoDeleteNode(GDID gNode)
    {
      var qry = Queries.ChangeInUseNodeByGDID<NodeRow>(gNode, true);
      var affected = ForNode(gNode).ExecuteWithoutFetch(qry);
      return (affected>0) ? GraphChangeStatus.Deleted : GraphChangeStatus.NotFound;
    }



    private NodeRow loadNodeRow(GDID gNode)
    {
      return ForNode(gNode).LoadDoc(Queries.FindOneNodeByGDID<NodeRow>(gNode));
    }

  }
}