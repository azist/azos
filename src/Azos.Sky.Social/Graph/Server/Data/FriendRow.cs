using System;

using Azos.Data;

using Azos.Sky.Social.Graph.Server.Data.Schema;

namespace Azos.Sky.Social.Graph.Server.Data
{
  /// <summary>
  /// GraphNode data
  /// </summary>
  [Table(name: "tbl_friend")]
  [UniqueSequence(SocialConsts.MDB_AREA_NODE, "friend")]
  public sealed class FriendRow : BaseRowWithGDID
  {

    public FriendRow() : base()
    {

    }

    public FriendRow(bool newGdid) : base(newGdid)
    {

    }

    /// <summary>Owner GDID; Briefcase key</summary>
    [Field(backendName: "G_OWN", required: false)]
    public GDID G_Owner { get; set; }

    /// <summary>Pointer to a friend</summary>
    [Field(backendName: "G_FND", required: true)]
    public GDID G_Friend { get; set; }

    /// <summary>When connection formed</summary>
    [Field(backendName: "G_RDT", required: true)]
    public DateTime Request_Date { get; set; }

    /// <summary>If set, then approved</summary>
    [Field(backendName: "G_SDT", required: true)]
    public DateTime Status_Date { get; set; }

    /// <summary>Approval status</summary>
    [Field(backendName: "STS", required: true, valueList: GSFriendStatus.VALUE_LIST)]
    public string Status { get; set; }

    /// <summary>Who requested friendship</summary>
    [Field(backendName: "DIR", required: true, valueList: GSFriendshipRequestDirection.VALUE_LIST)]
    public string Direction { get; set; }

    /// <summary>Who can see this connection - cascades from user profile</summary>
    [Field(backendName: "VIS", required: true, valueList: GSFriendVisibility.VALUE_LIST)]
    public string Visibility { get; set; }

    /// <summary>Comma-separated list of friend list Ids</summary>
    [Field(backendName: "LST", required: false)]
    public string Lists { get; set; }

  }
}
