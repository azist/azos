using System;

using Azos;
using Azos.Data;
using Azos.Data.Access;
using Azos.Log;
using Azos.Serialization.Arow;

using Azos.Sky.Social.Graph.Server.Data;
using Azos.Sky.Workers;

namespace Azos.Sky.Social.Graph.Server.Workers
{

  [Arow]
  [TodoQueue(SocialConsts.TQ_EVT_SUB_MANAGE, "A00C4F64-DBE0-4B86-8132-6FD77B684243")]
  public sealed class EventSubscribeTodo : EventSubscriptionBase
  {
    [Field(backendName: "stp")]   public string Subs_Type { get; set; }
    [Field(backendName: "par")]   public byte[] Parameters { get; set; }

    protected override void DoPrepareForEnqueuePostValidate(string targetName)
    {
      base.DoPrepareForEnqueuePostValidate(targetName);
      var key = G_Owner.ToString();
      SysShardingKey = key;
      SysParallelKey = key;
    }

    protected override ExecuteState Execute(ITodoHost host, DateTime utcBatchNow)
    {
      try
      {
        SubscriberVolumeRow minCntVol;
        var existing = FindSubscriber(out minCntVol);
        if (existing != null) return ExecuteState.Complete;
        if (minCntVol == null) minCntVol = createVolume();

        var row = new SubscriberRow()
        {
          G_SubscriberVolume = minCntVol.G_SubscriberVolume,
          G_Subscriber  = G_Subscriber,
          Subs_Type = Subs_Type,
          Create_Date = App.TimeSource.UTCNow,
          Parameters = Parameters
        };

        try
        {
          ForNode(row.G_SubscriberVolume).Insert(row);
        }
        catch (DataAccessException dae)
        {
          if(dae.KeyViolation.IsNotNullOrWhiteSpace() ) return ExecuteState.Complete;
          throw;
        }

        try
        {
          minCntVol.Count++;
          ForNode(G_Owner).Update(minCntVol, filter: "Count".OnlyTheseFields() );
        }
        catch (Exception error)
        {
          host.Log(MessageType.Error, this, "Subscribe()", error.ToMessageWithType(), error);
        }


        return ExecuteState.Complete;
      }
      catch (Exception error)
      {
        host.Log(MessageType.Error, this, "Subscribe()", error.ToMessageWithType(), error);
        return ExecuteState.ReexecuteAfterError;
      }
    }

    protected override int RetryAfterErrorInMs(DateTime utcBatchNow)
    {
#warning code repetition - refactor
      if (SysTries < 2)   return App.Random.NextScaledRandomInteger(1000, 3000);
      if (SysTries < 5)   return App.Random.NextScaledRandomInteger(20000, 60000);
      if (SysTries <= 10) return App.Random.NextScaledRandomInteger(80000, 160000);
      throw new GraphException("Event subscription after failure {0} retries".Args(10));
    }

    private SubscriberVolumeRow createVolume()
    {
      var gVol = NodeRow.GenerateNewNodeRowGDID(); // NOTICE: Volume uses Node Row GDID (Briefcase key)
      var result = new SubscriberVolumeRow()
      {
        G_Owner =  G_Owner,
        G_SubscriberVolume = gVol,
        Create_Date = App.TimeSource.UTCNow,
        Count = 0
      };
      ForNode(G_Owner).Insert(result);
      return result;
    }
  }
}