using System;

using Azos.Data;

namespace Azos.Sky.Social.Graph
{
  /// <summary>
  /// Represents and event that is emitted into the GraphEventSystem
  /// </summary>
  [Serializable]
  public struct Event
  {
    /// <summary>
    /// Creates a new GraphNode with new GDID
    /// </summary>
    public static Event MakeNew(GDID gEmitterNode, string eType, GDID gTargetShard, GDID gTarget, string config)
    {
#warning rewrite to use named Gdid
      var gdid = GDID.Zero;//SkySystem.GDIDProvider.GenerateOneGDID(SysConsts.GDID_NS_SOCIAL, SysConsts.GDID_NAME_SOCIAL_EVENT);
      var utcNow  = App.TimeSource.UTCNow;
      return new Event(gdid, utcNow, gEmitterNode, eType, gTargetShard, gTarget, config);
    }

    internal Event(GDID gdid, DateTime utcTimestamp, GDID gEmitterNode, string eType, GDID gTargetShard, GDID gTarget, string config)
    {
      GDID            = gdid;
      TimestampUTC    = utcTimestamp;
      G_EmitterNode   = gEmitterNode;
      EventType       = eType;
      G_TargetShard   = gTargetShard;
      G_Target        = gTarget;
      Config          = config;
    }

    /// <summary>Unique ID of the event itself</summary>
    public readonly GDID     GDID;

    /// <summary>Event creation UTC timestamp</summary>
    public readonly DateTime TimestampUTC;

    /// <summary>GDID of the emitter node</summary>
    public readonly GDID     G_EmitterNode;

    /// <summary>Event type per particular business system</summary>
    public readonly string   EventType;

    /// <summary>The sharding gdid of the target (such as a GDID of the user where post is stored)</summary>
    public readonly GDID     G_TargetShard;

    /// <summary>The GDID of the target that this event represents, (such as the GDID of the post on user's wall)</summary>
    public readonly GDID     G_Target;

    /// <summary>Arbitrary parameters in Laconic format</summary>
    public readonly string   Config;
  }
}
