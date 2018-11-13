using System.Collections.Generic;

using Azos.Glue;
using Azos.Data;

using Azos.Sky.Contracts;

namespace Azos.Sky.Social.Graph
{
  /// <summary>
  /// Handles the social graph functionality that deals with friend connection, friend list tagging and connection approval
  /// </summary>
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface IGraphFriendSystem : ISkyService
  {
    /// <summary>
    /// Returns an enumeration of friend list ids for the particular node
    /// </summary>
    IEnumerable<string> GetFriendLists(GDID gNode);

    /// <summary>
    /// Adds a new friend list id for the particular node. The list id may not contain commas
    /// </summary>
    GraphChangeStatus AddFriendList(GDID gNode, string list, string description);

    /// <summary>
    /// Removes friend list id for the particular node
    /// </summary>
    GraphChangeStatus DeleteFriendList(GDID gNode, string list);

    /// <summary>
    /// Returns an enumeration of FriendConnection{GraphNode, approve date, direction, groups}
    /// </summary>
    IEnumerable<FriendConnection> GetFriendConnections(FriendQuery query);

    /// <summary>
    /// Adds a bidirectional friend connection between gNode and gFriendNode
    /// If friend connection already exists updates the approve/ban stamp by the receiving party (otherwise approve is ignored)
    /// If approve==null then no stamps are set, if true connection is approved given that gNode is not the one who initiated the connection,
    /// false then connection is banned given that gNode is not the one who initiated the connection
    /// </summary>
    GraphChangeStatus AddFriend(GDID gNode, GDID gFriendNode, bool? approve);


    /// <summary>
    /// Assigns lists to the gNode (the operation is unidirectional - it only assigns the lists on the gNode).
    /// Lists is a comma-separated list of friend list ids
    /// </summary>
    GraphChangeStatus AssignFriendLists(GDID gNode, GDID gFriendNode, string lists);


    /// <summary>
    /// Deletes friend connections. The operation drops both connections from node and friend
    /// </summary>
    GraphChangeStatus DeleteFriend(GDID gNode, GDID gFriendNode);
  }


  /// <summary>
  /// Represents query parameters sent to IGraphFriendSystem.GetFriendConnections(query)
  /// </summary>
  public struct FriendQuery
  {
    public FriendQuery(GDID gNode, FriendStatusFilter status, string orgQry, string lists, int fetchStart, int fetchCount)
    {
      G_Node = gNode;
      Status = status;
      OriginQuery = orgQry;
      Lists = lists;
      FetchStart = fetchStart;
      FetchCount = fetchCount;
    }

    /// <summary>Node for which friends are returned</summary>
    public readonly GDID G_Node;

    public readonly FriendStatusFilter Status;

    /// <summary>Pass expression with * to search by name</summary>
    public readonly string OriginQuery;

    /// <summary>A comma-delimited list of friend list ids, null = all</summary>
    public readonly string Lists;

    /// <summary>From what position to start fetching</summary>
    public readonly int FetchStart;

    /// <summary>How many records to fetch</summary>
    public readonly int FetchCount;
  }

  /// <summary>
  /// Contract for client of IGraphFriendSystem svc
  /// </summary>
  public interface IGraphFriendSystemClient : ISkyServiceClient, IGraphFriendSystem
  {
    //todo Add async versions
  }
}
