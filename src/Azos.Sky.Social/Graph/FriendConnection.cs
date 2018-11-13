using System;


namespace Azos.Sky.Social.Graph
{
  /// <summary>
  /// Returns data about a connected friend
  /// </summary>
  public struct FriendConnection
  {
    public FriendConnection(GraphNode friend,
                            DateTime requestDate,
                            DateTime? approveDate,
                            FriendshipRequestDirection dir,
                            FriendVisibility visibility,
                            string groups)
    {
      Friend = friend;
      RequestDate = requestDate;
      ApproveDate = approveDate;
      Direction = dir;
      Visibility = visibility;
      Groups = groups;
    }

    public readonly GraphNode Friend;
    public readonly DateTime  RequestDate;
    public readonly DateTime? ApproveDate;
    public readonly FriendshipRequestDirection  Direction;
    public readonly FriendVisibility            Visibility;

    /// <summary>A comma-delimited list of friend group ids</summary>
    public readonly string   Groups;

    public bool Approved { get { return ApproveDate.HasValue; } }
  }
}
