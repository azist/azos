using System;

using Azos.Data;
using Azos.Log;
using Azos.Serialization.Arow;
using Azos.Sky.Social.Graph.Server.Data;
using Azos.Sky.Workers;
using Azos.Sky.Mdb;

namespace Azos.Sky.Social.Graph.Server.Workers
{
  [Arow]
  [TodoQueue(SocialConsts.TQ_EVT_SUB_REMOVE, "AF60C7EF-350D-4241-8FF7-F486024C3A14")]
  public sealed class EventRemoveFriendsTodo : Todo
  {
    /// <summary>
    /// Индекс текущего друга
    /// </summary>
    [Field(backendName: "frd_idx")] public int FriendIndex { get; set; }
    /// <summary>
    /// Remove by Node
    /// </summary>
    [Field(backendName: "g_nod")] public GDID G_Node { get; set; }
    /// <summary>
    /// Current friend
    /// </summary>
    [Field(backendName: "g_frd")]   public GDID G_Friend { get; set; }

    protected override ExecuteState Execute(ITodoHost host, DateTime utcBatchNow)
    {
      try
      {
        if (G_Friend.IsZero) G_Friend = getFriend();
        removeFriends();
        G_Friend = getNextFriend();
        if (G_Friend.IsZero)
        {
          removeFriendList();
          removeFriend();
          return ExecuteState.Complete;
        }
        return ExecuteState.ReexecuteUpdated;
      }
      catch (Exception error)
      {
        host.Log(MessageType.Error, this, "RemoveNode()", error.ToMessageWithType(), error);
      }
      return ExecuteState.ReexecuteAfterError;
    }

    public CRUDOperations ForNode(GDID gNode)
    {
      return GraphOperationContext.Instance.DataStore.PartitionedOperationsFor(SocialConsts.MDB_AREA_NODE, gNode);
    }

    protected override int RetryAfterErrorInMs(DateTime utcBatchNow)
    {
#warning Why does this code repeat?
      if (SysTries < 2)   return App.Random.NextScaledRandomInteger(1000, 3000);
      if (SysTries < 5)   return App.Random.NextScaledRandomInteger(20000, 60000);
      if (SysTries <= 10) return App.Random.NextScaledRandomInteger(80000, 160000);
      return App.Random.NextScaledRandomInteger(1600000, 320000);
      // throw new SocialException("Event redeliver after failure {0} retries".Args(10));
    }

    private GDID getNextFriend()
    {
      FriendIndex = FriendIndex + 1;
      return getFriend();
    }

    private GDID getFriend()
    {
      var qry = Queries.GetNextFriend<FriendRow>(G_Node, FriendIndex);
      var row = ForNode(G_Node).LoadDoc(qry);
      return row != null ?  row.G_Friend : GDID.Zero;
    }

    private void removeFriends()
    {
      var qry = Queries.DeleteFriendByNode(G_Node);
      ForNode(G_Friend).ExecuteWithoutFetch(qry);
    }

    private void removeFriendList()
    {
      var qry = Queries.RemoveFriendListByNode(G_Node);
      ForNode(G_Node).ExecuteWithoutFetch(qry);
    }

    private void removeFriend()
    {
      var qry = Queries.RemoveFriendByNode(G_Node);
      ForNode(G_Node).ExecuteWithoutFetch(qry);
    }

  }
}