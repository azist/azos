using System;

using Azos.Data;
using Azos.Sky.Social.Graph.Server.Data.Schema;

namespace Azos.Sky.Social.Graph.Server.Data
{
  /// <summary>
  /// Comment complaint data stored in COMMENT area
  /// </summary>
  [Table(name: "tbl_complaint")]
  [UniqueSequence(SocialConsts.MDB_AREA_COMMENT, "complaint")]
  public sealed class ComplaintRow : BaseRowWithGDID
  {
    public ComplaintRow() : base() {}
    public ComplaintRow(bool newGdid) : base(newGdid) {}

    [Field(backendName: "G_CMT", required: true)]
    public GDID G_Comment { get; set; }

    [Field(backendName: "G_ATH", required: true)]
    public GDID G_AuthorNode { get; set; }

    [Field(backendName: "KND",
           required: true,
           min: GSMessageType.MIN_LEN,
           max: GSMessageType.MAX_LEN)]
    public string Kind { get; set; }

    [Field(backendName: "MSG",
           minLength: GSCommentMessage.MIN_LEN,
           maxLength: GSCommentMessage.MAX_LEN,
           required: false)]
    public string Message { get; set; }

    [Field(backendName: "CDT", required: true)]
    public DateTime Create_Date { get; set; }

    [Field(required: true)]
    public bool In_Use { get ; set; }
  }
}