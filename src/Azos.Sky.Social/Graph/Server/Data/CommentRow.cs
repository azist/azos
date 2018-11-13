using System;

using Azos.Data;
using Azos.Sky.Social.Graph.Server.Data.Schema;

namespace Azos.Sky.Social.Graph.Server.Data
{
  /// <summary>
  /// Comment data stored in COMMENT area. Every comment has a unique ID, but is briefcased by G_COMMENTVOLUME
  /// </summary>
  [Table(name: "tbl_comment")]
  [UniqueSequence(SocialConsts.MDB_AREA_COMMENT, "comment")]
  public sealed class CommentRow : BaseRowWithGDID
  {
    public CommentRow() : base()
    {
    }

    public CommentRow(bool newGdid) : base(newGdid)
    {
    }

    /// <summary>
    /// Briefcase key
    /// </summary>
    [Field(backendName: "G_VOL", required: true)]
    public GDID G_CommentVolume { get; set; }

    [Field(backendName: "DIM", required: true)]
    public string Dimension { get; set; }

    [Field(backendName: "G_ATR", required: true)]
    public GDID G_AuthorNode { get; set; }

    [Field(backendName: "G_TRG", required: true)]
    public GDID G_TargetNode { get; set; }

    [Field(backendName: "ROOT", required: true)]
    public bool IsRoot { get; set; }

    [Field(backendName: "G_PAR")]
    public GDID? G_Parent { get; set; }

    [Field(backendName: "MSG",
           minLength: GSCommentMessage.MIN_LEN,
           maxLength: GSCommentMessage.MAX_LEN,
           required: true)]
    public string Message { get; set; }

    [Field(backendName: "DAT", required: true)]
    public byte[] Data { get; set; }

    [Field(backendName: "CDT", required: true)]
    public DateTime Create_Date { get; set; }

    [Field(backendName: "LKE", required: true)]
    public uint Like { get;set; }

    [Field(backendName: "DIS", required: true)]
    public uint Dislike { get; set; }

    [Field(backendName: "CMP", required: true)]
    public uint ComplaintCount { get; set; }

    [Field(backendName: "PST", required: true)]
    public string PublicationState { get; set; }

    [Field(backendName: "RTG", required: true)]
    public byte Rating { get; set; }

    [Field(backendName: "RCNT", required: true)]
    public uint ResponseCount { get; set; }

    [Field(required: true)]
    public bool In_Use { get ; set; }

    /// <summary>
    /// Update count of commentaries
    /// </summary>
    /// <param name="delta">+/- Delta</param>
    public void UpdateResponseCount(int delta)
    {
      var isNegative = delta < 0;
      if (isNegative && ResponseCount == 0)
        return;

      var abs = (uint)Math.Abs(delta);
      ResponseCount = isNegative
        ? ResponseCount - abs
        : ResponseCount + abs;
    }
  }
}