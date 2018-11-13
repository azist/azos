using System;

using Azos.Data;
using Azos.Sky.Social.Graph.Server.Data.Schema;

namespace Azos.Sky.Social.Graph.Server.Data
{

  /// <summary>
  /// Logical chunk of subscribers
  /// </summary>
  [Table(name: "tbl_subscribervol")]
  public sealed class SubscriberVolumeRow : BaseRow
  {
    /// <summary>Owner GDID; Briefcase key</summary>
    [Field(backendName: "G_OWN", required: true, key: true)]
    public GDID G_Owner { get; set; }

    /// <summary>
    /// Link for Subscriber Volume.
    /// G_VOL is generated of NodeRow GDID sequence
    /// </summary>
    [Field(backendName: "G_VOL", required: true, key: true)]
    public GDID G_SubscriberVolume { get; set; }

    /// <summary>Count subscriber in volume</summary>
    [Field(backendName: "CNT", required: true)]
    public int Count { get; set; }

    /// <summary>Create date volume</summary>
    [Field(backendName: "CDT", required: true)]
    public DateTime Create_Date { get; set; }

  }

  /// <summary>
  /// Lists all subscribers per volume; sharded in the graph area
  /// </summary>
  [Table(name: "tbl_subscriber")]
  public sealed class SubscriberRow : BaseRow
  {
    /// <summary>Emitter - briefcase key</summary>
    [Field(backendName: "G_VOL", required: true, key: true)]
    public GDID G_SubscriberVolume { get; set; }

    /// <summary>Subscriber</summary>
    [Field(backendName: "G_SUB", required: true, key: true)]
    public GDID G_Subscriber { get; set; }

    /// <summary> Denormalizes node type from G_Subscriber for faster search </summary>
    [Field(backendName: "STP", required: true, minLength: GSNodeType.MIN_LEN, maxLength: GSNodeType.MAX_LEN)]
    public string Subs_Type { get; set; }

    [Field(backendName: "CDT", required: true)]
    public DateTime Create_Date { get; set; }

    /// <summary>Subscription details</summary>
    [Field(backendName: "PAR", required: false)]
    public byte[] Parameters { get; set; }
  }
}
