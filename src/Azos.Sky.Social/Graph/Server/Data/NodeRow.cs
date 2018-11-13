using System;

using Azos.Data;

using Azos.Sky.Social.Graph.Server.Data.Schema;

namespace Azos.Sky.Social.Graph.Server.Data
{
  /// <summary>
  /// GraphNode data
  /// </summary>
  [Table(name: "tbl_node")]
  [UniqueSequence(SocialConsts.MDB_AREA_NODE, "node")]
  public sealed class NodeRow : BaseRowWithGDID
  {
    public static GDID GenerateNewNodeRowGDID()
    {
      return GenerateNewGDID(typeof(NodeRow));
    }

    public NodeRow() : base()
    {

    }

    public NodeRow(bool newGdid) : base(newGdid)
    {
    }

    [Field(backendName: "TYP", required: true, minLength: GSNodeType.MIN_LEN, maxLength: GSNodeType.MAX_LEN)]
    public string Node_Type { get; set; }

    /// <summary>
    /// Stores GDID of origin shard
    /// </summary>
    [Field(backendName: "G_OSH", required: true)]
    public GDID G_OriginShard { get; set; }

    /// <summary>
    /// Stores the GDID of the origin entity
    /// </summary>
    [Field(backendName: "G_ORI", required: true)]
    public GDID G_Origin { get; set; }

    /// <summary>
    /// Not required for subscriber link
    /// </summary>
    [Field(backendName: "ONM", required: true)]
    public string Origin_Name { get; set; }

    [Field(backendName: "ODT", required: false)]
    public byte[] Origin_Data { get; set; }

    [Field(backendName: "CDT", required: true)]
    public DateTime? Create_Date { get; set; }

    /// <summary>Default Friend Visibility</summary>
    [Field(backendName: "FVI", required: false, valueList: GSFriendVisibility.VALUE_LIST)]
    public string Friend_Visibility { get; set; }

    [Field(required: true)]
    public bool In_Use { get ; set; }
  }
}
