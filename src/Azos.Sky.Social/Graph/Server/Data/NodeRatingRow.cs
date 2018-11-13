using System;

using Azos.Data;

namespace Azos.Sky.Social.Graph.Server.Data
{
  /// <summary>
  /// The table contains data for nodes that have rating. 1:M per dimension to Node extension
  /// </summary>
  [Table(name: "tbl_noderating")]
  public sealed class NodeRatingRow : BaseRow
  {
    /// <summary>
    /// Primary key GDID
    /// </summary>
    [Field(backendName: "G_NOD", required: true, key: true)]
    public GDID G_Node { get; set; }

    /// <summary>
    /// Primary key, Dimension of rating, e.g. if we rate cars, the dims are 'reliability', 'driving" etc.
    /// </summary>
    [Field(backendName: "DIM", required: true, key: true)]
    public string Dimension { get; set; }

    /// <summary>
    /// Also known as epoch
    /// </summary>
    [Field(backendName: "CDT", required: true)]
    public DateTime Create_Date { get; set; }

    /// <summary>
    /// When was the node's rating last changed
    /// </summary>
    [Field(backendName: "LCD", required: true)]
    public DateTime Last_Change_Date { get; set; }

    /// <summary>
    /// Count of comments
    /// </summary>
    [Field(backendName: "CNT", required: true)]
    public ulong Cnt { get; set; }

    /// <summary>
    /// Rating star 1 count
    /// </summary>
    [Field(backendName: "RTG1", required: true)]
    public ulong Rating1 { get; set; }

    /// <summary>
    /// Rating star 2 count
    /// </summary>
    [Field(backendName: "RTG2", required: true)]
    public ulong Rating2 { get; set; }

    /// <summary>
    /// Rating star 3 count
    /// </summary>
    [Field(backendName: "RTG3", required: true)]
    public ulong Rating3 { get; set; }

    /// <summary>
    /// Rating star 4 count
    /// </summary>
    [Field(backendName: "RTG4", required: true)]
    public ulong Rating4 { get; set; }

    /// <summary>
    /// Rating star 5 count
    /// </summary>
    [Field(backendName: "RTG5", required: true)]
    public ulong Rating5 { get; set; }

    /// <summary>
    /// Total amount of ratings
    /// </summary>
    public ulong TotalRatings
    {
      get { return Rating1 + Rating2 + Rating3 + Rating4 + Rating5; }
    }

    /// <summary>
    /// Average rating
    /// </summary>
    public float Rating
    {
      get
      {
        var tr = (float) TotalRatings;
        return tr > 0f
          ? ((Rating1*1) +
             (Rating2*2) +
             (Rating3*3) +
             (Rating4*4) +
             (Rating5*5)
            )/tr
          : 0f;
      }
    }

    /// <summary>
    /// Decrease or increase rating by delta
    /// </summary>
    /// <param name="value">Star 1-2-3-4-5</param>
    /// <param name="delta">+/- Delta</param>
    public void UpdateRating(RatingValue value, int delta)
    {
      var isNegative = delta < 0;
      var abs = (ulong) Math.Abs(delta);
      switch (value)
      {
        // if isNegative == true and rating counter == 0, decreasing ulong will make field MAX ulong.
        // so, need to check it
        case RatingValue.Star1:
          if (isNegative && Rating1 == 0)
            break;
          Rating1 = isNegative
            ? Rating1 - abs
            : Rating1 + abs;
          break;
        case RatingValue.Star2:
          if (isNegative && Rating2 == 0)
            break;
          Rating2 = isNegative
            ? Rating2 - abs
            : Rating2 + abs;
          break;
        case RatingValue.Star3:
          if (isNegative && Rating3 == 0)
            break;
          Rating3 = isNegative
            ? Rating3 - abs
            : Rating3 + abs;
          break;
        case RatingValue.Star4:
          if (isNegative && Rating4 == 0)
            break;
          Rating4 = isNegative
            ? Rating4 - abs
            : Rating4 + abs;
          break;
        case RatingValue.Star5:
          if (isNegative && Rating5 == 0)
            break;
          Rating5 = isNegative
            ? Rating5 - abs
            : Rating5 + abs;
          break;
      }
    }

    /// <summary>
    /// Update count of commentaries
    /// </summary>
    /// <param name="delta">+/- Delta</param>
    public void UpdateCount(int delta)
    {
      var isNegative = delta < 0;
      if (isNegative && Cnt == 0)
        return;

      var abs = (ulong) Math.Abs(delta);
      Cnt = isNegative
        ? Cnt - abs
        : Cnt + abs;
    }
  }
}