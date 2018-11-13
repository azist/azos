using System;
using System.Collections.Generic;

using Azos;
using Azos.Data;
using Azos.Log;

using Azos.Sky.Social.Graph.Server.Data;
using Azos.Sky.Social.Graph.Server.Data.Schema;


namespace Azos.Sky.Social.Graph.Server
{
  public partial class GraphSystemService
  {
    /// <summary>
    /// Returns an enumeration of friend list ids for the particular node
    /// </summary>
    IEnumerable<string> IGraphFriendSystem.GetFriendLists(GDID gNode)
    {
      try
      {
        return DoGetFriendLists(gNode);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "GetFriendLists", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_GET_FRIEND_LISTS_ERROR.Args(gNode.ToString()), ex);
      }
    }

    /// <summary>
    /// Adds a new friend list id for the particular node. The list id may not contain commas
    /// </summary>
    GraphChangeStatus IGraphFriendSystem.AddFriendList(GDID gNode, string list, string description)
    {
      try
      {
        return DoAddFriendList(gNode, list, description);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "AddFriendList", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_ADD_FRIEND_LIST_ERROR.Args(gNode.ToString()), ex);
      }
    }

    /// <summary>
    /// Removes friend list id for the particular node
    /// </summary>
    GraphChangeStatus IGraphFriendSystem.DeleteFriendList(GDID gNode, string list)
    {
      try
      {
        return DoDeleteFriendList(gNode, list);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "DeleteFriendList", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_DELETE_FRIEND_LIST_ERROR.Args(gNode.ToString()), ex);
      }
    }

    /// <summary>
    /// Returns an enumeration of FriendConnection{GraphNode, approve date, direction, groups}
    /// </summary>
    IEnumerable<FriendConnection> IGraphFriendSystem.GetFriendConnections(FriendQuery query)
    {
      try
      {
        return DoGetFriendConnections(query);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "GetFriendConnections", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_GET_FRIEND_CONNECTIONS_ERROR.Args(query.ToString()), ex);
      }
    }

    /// <summary>
    /// Adds a bidirectional friend connection between gNode and gFriendNode
    /// If friend connection already exists updates the approve/ban stamp by the receiving party (otherwise approve is ignored)
    /// If approve==null then no stamps are set, if true connection is approved given that gNode is not the one who initiated the connection,
    /// false then connection is banned given that gNode is not the one who initiated the connection
    /// </summary>
    GraphChangeStatus IGraphFriendSystem.AddFriend(GDID gNode, GDID gFriendNode, bool? approve)
    {
      try
      {
        return DoAddFriend(gNode, gFriendNode, approve);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "AddFriend", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_ADD_FRIEND_ERROR.Args(gNode.ToString(), gFriendNode.ToString()), ex);
      }
    }

    /// <summary>
    /// Assigns lists to the gNode (the operation is unidirectional - it only assigns the lists on the gNode).
    /// Lists is a comma-separated list of friend list ids
    /// </summary>
    GraphChangeStatus IGraphFriendSystem.AssignFriendLists(GDID gNode, GDID gFriendNode, string lists)
    {
      try
      {
        return DoAssignFriendLists(gNode, gFriendNode, lists);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "AssignFriendLists", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_ASSIGN_FRIEND_LISTS_ERROR.Args(gNode.ToString(), gFriendNode.ToString()), ex);
      }
    }

    /// <summary>
    /// Deletes friend connections. The operation drops both connections from node and friend
    /// </summary>
    GraphChangeStatus IGraphFriendSystem.DeleteFriend(GDID gNode, GDID gFriendNode)
    {
      try
      {
        return DoDeleteFriend(gNode, gFriendNode);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "DeleteFriend", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_DELETE_FRIEND_ERROR.Args(gNode.ToString(), gFriendNode.ToString()), ex);
      }
    }

    protected virtual IEnumerable<string> DoGetFriendLists(GDID gNode)
    {
      DoGetNode(gNode);
      var rows = ForNode(gNode).LoadEnumerable(Queries.FindFriendListByNode<FriendListRow>(gNode));
      foreach (var row in rows)
      {
        yield return row.List_ID;
      }
    }

    protected virtual GraphChangeStatus DoAddFriendList(GDID gNode, string list, string description)
    {
      DoGetNode(gNode);
      var row = new FriendListRow(true)
      {
        G_Owner = gNode,
        List_ID = list,
        List_Description = description,
        Create_Date = App.TimeSource.UTCNow
      };
      ForNode(gNode).Insert(row);
      return GraphChangeStatus.Added;
    }

    protected virtual GraphChangeStatus DoDeleteFriendList(GDID gNode, string list)
    {
      ForNode(gNode).ExecuteWithoutFetch(Queries.DeleteFriendListByListId(gNode, list));
      return GraphChangeStatus.Deleted;
    }

    protected virtual IEnumerable<FriendConnection> DoGetFriendConnections(FriendQuery query, ICacheParams cacheParams = null)
    {
      var rows = ForNode(query.G_Node).LoadEnumerable(Queries.FindFriends<FriendRow>(query));
      foreach (var row in rows)
      {
        var friendNode = DoGetNode(row.G_Friend, cacheParams);
        foreach (var graphNode in GraphHost.FilterByOriginQuery(new[] {friendNode}, query.OriginQuery))
        {
          yield return new FriendConnection(graphNode,
            row.Request_Date,
            FriendStatus.Approved.Equals(GSFriendStatus.ToFriendStatus(row.Status))
              ? (DateTime?) row.Status_Date
              : null,
            GSFriendshipRequestDirection.ToFriendshipRequestDirection(row.Direction),
            GSFriendVisibility.ToFriendVisibility(row.Visibility),
            row.Lists);
        }
      }
    }

    protected virtual GraphChangeStatus DoAddFriend(GDID gNode, GDID gFriendNode, bool? approve)
    {
      var node = DoGetNode(gNode);
      var friendNode = DoGetNode(gFriendNode);

      if(!GraphHost.CanBeFriends(node.NodeType, friendNode.NodeType)) throw new GraphException(StringConsts.GS_FRIEND_DRIECTION_ERROR.Args(node.NodeType, friendNode.NodeType));

      int countMe = countFriends(gNode);
      int countFriend = countFriends(gFriendNode);

      if (countMe > SocialConsts.GS_MAX_FRIENDS_COUNT) throw new GraphException(StringConsts.GS_MAX_FRIENDS_IN_NODE.Args(gNode));
      if (countFriend > SocialConsts.GS_MAX_FRIENDS_COUNT) throw new GraphException(StringConsts.GS_MAX_FRIENDS_IN_NODE.Args(gFriendNode));


      GraphChangeStatus resultMe = addFriend(gNode, gFriendNode, approve, FriendshipRequestDirection.I);
      GraphChangeStatus resultFriend = addFriend(gFriendNode, gNode, approve, FriendshipRequestDirection.Friend);

      return resultMe == GraphChangeStatus.Added && resultFriend == GraphChangeStatus.Added ? GraphChangeStatus.Added : GraphChangeStatus.Updated;
    }

    protected virtual GraphChangeStatus DoAssignFriendLists(GDID gNode, GDID gFriendNode, string lists)
    {
      DoGetNode(gNode);
      DoGetNode(gFriendNode);

      var ctx = ForNode(gNode);
      var row = ctx.LoadDoc(Queries.FindOneFriendByNodeAndFriend<FriendRow>(gNode, gFriendNode));
      if (row == null) return GraphChangeStatus.NotFound;
      row.Lists = addToList(row.Lists, lists);
      ctx.Update(row);
      return GraphChangeStatus.Updated;
    }

    protected virtual GraphChangeStatus DoDeleteFriend(GDID gNode, GDID gFriendNode)
    {
      ForNode(gNode).ExecuteWithoutFetch(Queries.DeleteFriendByNodeAndFriend(gNode, gFriendNode));
      ForNode(gFriendNode).ExecuteWithoutFetch(Queries.DeleteFriendByNodeAndFriend(gFriendNode, gNode));
      return GraphChangeStatus.Deleted;
    }


    private GraphChangeStatus addFriend(GDID gNode, GDID gFriendNode, bool? approve, FriendshipRequestDirection direction)
    {
      GraphChangeStatus result;
      var ctx = ForNode(gNode);
      var row = ctx.LoadDoc(Queries.FindOneFriendByNodeAndFriend<FriendRow>(gNode, gFriendNode));
      if (row != null)
      {
        if (approve == null) return GraphChangeStatus.Updated;
        row.Status = approve.Value ? GSFriendStatus.APPROVED : GSFriendStatus.DENIED ;
        row.Status_Date = App.TimeSource.UTCNow;
        ctx.Update(row);
        result = GraphChangeStatus.Updated;
      }
      else
      {
        row = new FriendRow(true)
        {
          G_Owner = gNode,
          G_Friend = gFriendNode,
          Status_Date = App.TimeSource.UTCNow,
          Status = approve!= null && approve.Value ? GSFriendStatus.APPROVED: GSFriendStatus.PENDING,
          Visibility = GSFriendVisibility.FRIENDS,
          Request_Date = App.TimeSource.UTCNow,
          Direction = GSFriendshipRequestDirection.ToDomainString(direction)
        };
        ctx.Insert(row);
        result = GraphChangeStatus.Added;
      }
      return result;
    }

    private string addToList(string rowLists, string lists)
    {
      HashSet<String> oldList = new HashSet<string>(rowLists.Split(SocialConsts.GS_FRIEND_LIST_SEPARATOR.ToCharArray()));
      lists.Split(SocialConsts.GS_FRIEND_LIST_SEPARATOR.ToCharArray()).ForEach(s => oldList.Add(s));
      return string.Join(SocialConsts.GS_FRIEND_LIST_SEPARATOR, oldList);
    }

    private int countFriends(GDID gNode)
    {
      var count = ForNode(gNode).LoadOneDoc(Queries.CountFriends<DynamicDoc>(gNode));
      return count[0].AsInt();
    }
  }
}