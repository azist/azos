using System;
using Azos.Data;

namespace Azos.Sky.Social.Graph.Server.Data
{
  [Table(name: "tbl_commentvol")]
  public sealed class CommentVolumeRow : BaseRow
  {
    /// <summary>Owner (target of comment such as PRODUCT NODE) GDID; Briefcase key for this row</summary>
    [Field(backendName: "G_OWN", required: true, key: true)]
    public GDID G_Owner { get; set; }

    /// <summary>Link for Comment Volume; briefcase key for COMMENT; obtained from NODE sequence</summary>
    [Field(backendName: "G_VOL", required: true, key: true)]
    public GDID G_CommentVolume { get; set; }

    /// <summary>
    /// Rating dimensional
    /// </summary>
    [Field(backendName: "DIM", required: true)]
    public string Dimension { get; set; }

    /// <summary>Count message in volume</summary>
    [Field(backendName: "CNT", required: true)]
    public int Count { get; set; }

    /// <summary>Create date volume</summary>
    [Field(backendName: "CDT", required: true)]
    public DateTime Create_Date { get; set; }
  }
}