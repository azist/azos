using System;
using System.Collections.Generic;
using System.Linq;

using Azos;
using Azos.Data;
using Azos.Conf;
using Azos.Log;
using Azos.Serialization.Arow;

using Azos.Sky.Workers;
using Azos.Sky.Social.Graph.Server.Data;

namespace Azos.Sky.Social.Graph.Server.Workers
{

  [Arow]
  [TodoQueue(SocialConsts.TQ_EVT_SUB_DELIVER, "A1257BDA-B366-43CF-85FD-D323A11091BD")]
  public sealed class EventDeliverTodo : EventDeliveryBase
  {
    /// <summary>
    /// Смещение по итерациям
    /// </summary>
    [Field(backendName: "vol_vwo")] public int VolumeWorkerOffset { get; set; }
    /// <summary>
    /// Увеличивается на OffsetSize по завершении обработки конкретного volume.
    /// При следующей итерации индекс последующего volume вычисляется по формуле : VolumeIteration + OffsetSize
    /// </summary>
    [Field(backendName: "vol_idx")] public int VolumeIndex { get; set; }
    /// <summary>
    /// Это номер куска (чанка) работы производимой внутри данного volume
    /// </summary>
    [Field(backendName: "chk_idx")] public int ChunkIndex { get; set; }
    /// <summary>
    /// Current volume subscribers
    /// </summary>
    [Field(backendName: "g_vol")]   public GDID G_Volume { get; set; }

    protected override void DoPrepareForEnqueuePostValidate(string targetName)
    {
      base.DoPrepareForEnqueuePostValidate(targetName);
      var key = G_Volume.ToString();
      SysShardingKey = key;
      SysParallelKey = key;
    }

    protected override ExecuteState Execute(ITodoHost host, DateTime utcBatchNow)
    {
      try
      {
        if (G_Volume.IsZero) G_Volume = getVolume();
        var subscribers = readNextRows();
        var count = subscribers.Count();
        sendEventsChunk(subscribers);
        if (count < SocialConsts.SUBSCRIPTION_DELIVERY_CHUNK_SIZE)
        {
          G_Volume = getNextVolume();
          if (G_Volume.IsZero) return ExecuteState.Complete;
          ChunkIndex = 0;
        }
        else
        {
          ChunkIndex += count;
        }
        return ExecuteState.ReexecuteUpdated;
      }
      catch (Exception error)
      {
        host.Log(MessageType.Error, this, "Deliver()", error.ToMessageWithType(), error);
      }
      return ExecuteState.ReexecuteAfterError;
    }

    protected override int RetryAfterErrorInMs(DateTime utcBatchNow)
    {
#warning code repetition - refactor
      if (SysTries < 2)   return App.Random.NextScaledRandomInteger(1000, 3000);
      if (SysTries < 5)   return App.Random.NextScaledRandomInteger(20000, 60000);
      if (SysTries <= 10) return App.Random.NextScaledRandomInteger(80000, 160000);
      throw new GraphException("Event deliver after failure {0} retries".Args(10));
    }

    private GDID getNextVolume()
    {
      VolumeIndex = VolumeIndex + VolumeWorkerOffset;
      return getVolume();
    }

    private GDID getVolume()
    {
      var qry = Queries.GetNextVolume<SubscriberVolumeRow>(G_Emitter, VolumeIndex);
      var row = ForNode(G_Emitter).LoadDoc(qry);
      return row != null ?  row.G_SubscriberVolume : GDID.Zero;
    }

    private IEnumerable<SubscriberRow> readNextRows()
    {
      var qry = Queries.FindSubscribers<SubscriberRow>(G_Volume, ChunkIndex,
        SocialConsts.SUBSCRIPTION_DELIVERY_CHUNK_SIZE);
      return ForNode(G_Volume).LoadEnumerable(qry);
    }

    private void sendEventsChunk(IEnumerable<SubscriberRow> subscribersChunk)
    {
      var host = GraphOperationContext.Instance.GraphHost;
      IConfigSectionNode cfg = null;
      if (Event.Config.IsNotNullOrWhiteSpace()) cfg = Event.Config.AsLaconicConfig(handling: ConvertErrorHandling.ReturnDefault);

      var filtered = host.FilterEventsChunk(subscribersChunk, Event, cfg);
      var badSubs = host.DeliverEventsChunk(filtered, Event, cfg);
      if (badSubs != null && badSubs.Any())
      {
        var todo = Todo.MakeNew<EventRedeliverTodo>();
        todo.Event = Event;
        todo.ToRedilever = badSubs.ToArray();
        SocialGraphTodos.EnqueueDelivery(todo);
      }
    }
  }
}