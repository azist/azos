using System;

using Azos.Data;

using Azos.Sky.Mdb;
using Azos.Sky.Workers;

namespace Azos.Sky.Social.Graph.Server.Workers
{
  public abstract class EventDeliveryBase : Todo
  {
    [Field(backendName: "g_emt")]   public GDID     G_Emitter          { get; set; }
    [Field(backendName: "evt_gdd")] public GDID     Evt_GDID           { get; set; }
    [Field(backendName: "evt_utc")] public DateTime Evt_TimestampUTC   { get; set; }
    [Field(backendName: "evt_typ")] public string   Evt_EventType      { get; set; }
    [Field(backendName: "evt_shr")] public GDID     Evt_G_TargetShard  { get; set; }
    [Field(backendName: "evt_tar")] public GDID     Evt_G_Target       { get; set; }
    [Field(backendName: "evt_cfg")] public string   Evt_Config         { get; set; }


    private Event? m_Event;

    public Event Event
    {
      get
      {
        if (m_Event == null)
          m_Event = new Event(Evt_GDID,
            Evt_TimestampUTC,
            G_Emitter,
            Evt_EventType,
            Evt_G_TargetShard,
            Evt_G_Target,
            Evt_Config);
        return m_Event.Value;
      }
      set
      {
        G_Emitter = value.G_EmitterNode;
        Evt_GDID = value.GDID;
        Evt_TimestampUTC = value.TimestampUTC;
        Evt_EventType = value.EventType;
        Evt_G_TargetShard = value.G_TargetShard;
        Evt_G_Target = value.G_Target;
        Evt_Config = value.Config;
        m_Event = null;
      }
    }

    public GraphHost GraphHost { get { return GraphOperationContext.Instance.GraphHost; } }

    public CRUDOperations ForNode(GDID gNode)
    {
      return GraphOperationContext.Instance.DataStore.PartitionedOperationsFor(SocialConsts.MDB_AREA_NODE, gNode);
    }
  }
}