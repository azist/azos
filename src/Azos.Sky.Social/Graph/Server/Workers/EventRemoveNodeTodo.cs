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
  [TodoQueue(SocialConsts.TQ_EVT_SUB_REMOVE, "A0BE51EB-E20B-4AC3-8615-86E6EDDB62BA")]
  public sealed class EventRemoveNodeTodo : Todo
  {
    /// <summary>
    /// Увеличивается на OffsetSize по завершении обработки конкретного volume.
    /// При селдующей итерации индекс последующего volume вычисляется по формуле : VolumeIteration + OffsetSize
    /// </summary>
    [Field(backendName: "vol_idx")] public int VolumeIndex { get; set; }
    /// <summary>
    /// Remove by Node
    /// </summary>
    [Field(backendName: "g_nod")] public GDID G_Node { get; set; }
    /// <summary>
    /// Current volume subscribers
    /// </summary>
    [Field(backendName: "g_vol")]   public GDID G_Volume { get; set; }


    protected override ExecuteState Execute(ITodoHost host, DateTime utcBatchNow)
    {
      try
      {
        if (G_Volume.IsZero) G_Volume = getVolume();
        removeSubscribers();
        G_Volume = getNextVolume();
        if (G_Volume.IsZero)
        {
          removeVolumes();
          removeNode();
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
#warning code repetition - refactor
      if (SysTries < 2)   return App.Random.NextScaledRandomInteger(1000, 3000);
      if (SysTries < 5)   return App.Random.NextScaledRandomInteger(20000, 60000);
      if (SysTries <= 10) return App.Random.NextScaledRandomInteger(80000, 160000);
      return App.Random.NextScaledRandomInteger(1600000, 320000);
      // throw new SocialException("Event redeliver after failure {0} retries".Args(10));
    }

    private GDID getNextVolume()
    {
      VolumeIndex = VolumeIndex + 1;
      return getVolume();
    }

    private GDID getVolume()
    {
      var qry = Queries.GetNextVolume<SubscriberVolumeRow>(G_Node, VolumeIndex);
      var row = ForNode(G_Node).LoadDoc(qry);
      return row != null ?  row.G_SubscriberVolume : GDID.Zero;
    }

    private void removeSubscribers()
    {
      var qry = Queries.RemoveSubscribers(G_Volume);
      ForNode(G_Node).ExecuteWithoutFetch(qry);
    }

    private void removeVolumes()
    {
      var qry = Queries.RemoveSubVol(G_Node);
      ForNode(G_Node).ExecuteWithoutFetch(qry);
    }

    private void removeNode()
    {
      var qry = Queries.RemoveNode(G_Node);
      ForNode(G_Node).ExecuteWithoutFetch(qry);
    }
  }
}