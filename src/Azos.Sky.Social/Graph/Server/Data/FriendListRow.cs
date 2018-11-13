using System;

using Azos.Data;

using Azos.Sky.Social.Graph.Server.Data.Schema;

namespace Azos.Sky.Social.Graph.Server.Data
{
  /// <summary>
  /// A list of named friend lists - a named group of friends like "family", "coworkers" etc.
  /// </summary>
  [Table(name: "tbl_friendlist")]
  [UniqueSequence(SocialConsts.MDB_AREA_NODE, "flist")]
  public sealed class FriendListRow : BaseRowWithGDID
  {

    public FriendListRow() : base()
    {

    }

    public FriendListRow(bool gdid) : base(gdid)
    {

    }

    /// <summary>Node that this list belongs to - brief case key </summary>
    [Field(backendName: "G_OWN", required: true)]
    public GDID G_Owner { get; set; }

    /// <summary>List ID</summary>
    [Field(backendName: "LID", required: true, minLength: GSFriendListID.MIN_LEN, maxLength: GSFriendListID.MAX_LEN)]
    public string List_ID { get; set; }

    /// <summary>List description</summary>
    [Field(backendName: "LDR", required: false, minLength: GSFriendListID.MIN_LEN, maxLength: GSFriendListID.MAX_LEN)]
    public string List_Description { get; set; }

    /// <summary>When created </summary>
    [Field(backendName: "CDT", required: true)]
    public DateTime? Create_Date { get; set; }
  }
}
