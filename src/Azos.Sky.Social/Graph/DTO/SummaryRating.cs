using System;

using Azos.Data;

namespace Azos.Sky.Social.Graph
{
  /// <summary>
  /// Contains summarized rating information per graph node
  /// </summary>
  [Serializable]
  public struct SummaryRating
  {
    internal SummaryRating(GDID gNode, DateTime createDate, DateTime lastDate, string dimension, ulong count, ulong rating1, ulong rating2, ulong rating3, ulong rating4, ulong rating5)
    {
      G_Node = gNode;
      CreateDate = createDate;
      LastChangeDate = lastDate;
      Dimension = dimension;
      Count = count;
      Rating1 = rating1;
      Rating2 = rating2;
      Rating3 = rating3;
      Rating4 = rating4;
      Rating5 = rating5;
      TotalRatings = Rating1 + Rating2 + Rating3 + Rating4 + Rating5;

      var tr = (float)TotalRatings;
      Rating = tr > 0f ? (Rating1 * 1 + Rating2 * 2 + Rating3 * 3 + Rating4 * 4 + Rating5 * 5) / tr : 0f;
    }

    public readonly GDID G_Node;
    public readonly DateTime CreateDate;
    public readonly DateTime LastChangeDate;
    public readonly string Dimension;
    public readonly ulong Count;
    public readonly ulong Rating1;
    public readonly ulong Rating2;
    public readonly ulong Rating3;
    public readonly ulong Rating4;
    public readonly ulong Rating5;
    public readonly ulong TotalRatings;
    public readonly float Rating;
  }
}
