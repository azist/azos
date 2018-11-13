namespace Azos.Sky.Social.Graph
{
  /// <summary>
  /// Denotes the results of graph changing operations, such as: Create, Update, Delete
  /// </summary>
  public enum GraphChangeStatus
  {
    NotFound = -1,
    Unassigned = 0,
    Added,
    Updated,
    Deleted
  }

  /// <summary>
  /// Denotes friendship request direction - who requested the friendship (and who approved)
  /// </summary>
  public enum FriendshipRequestDirection
  {
    /// <summary>
    /// I requested = G_GraphNode
    /// </summary>
    I = 0,

    /// <summary>
    /// Friend requested = G_FriendNode
    /// </summary>
    F = 1, Friend = F
  }

  /// <summary>
  /// Denotes friend statuses
  /// </summary>
  public enum FriendStatusFilter
  {
    Approved = 0,
    PendingApproval,
    Banned,
    All
  }

  /// <summary>
  /// Friend approval status
  /// </summary>
  public enum FriendStatus
  {
    Pending = 0,
    Approved,
    Denied,
    Banned
  }

  public enum FriendVisibility
  {
    /// <summary>
    /// Anyone can see friends, even non-logged users/internet/google
    /// </summary>
    Anyone = 0,

    /// <summary>
    /// Only logged-in users may see friends
    /// </summary>
    Public,

    /// <summary>
    /// Only firends can see user's friends
    /// </summary>
    Friends,

    /// <summary>
    /// Only users can see their own friends
    /// </summary>
    Private
  }

  /// <summary>
  /// Defines rating grades
  /// </summary>
  public enum RatingValue
  {
    Undefined = 0,
    Star1 = 1,
    Star2 = 2,
    Star3 = 3,
    Star4 = 4,
    Star5 = 5
  }


  public enum PublicationState
  {
    //todo Нужны состояния публикации
    Private = 0,
    Public,
    Friend,
    Deleted
  }

  public enum CommentOrderType
  {
    ByDate,
    ByPositive,
    ByNegative,
    ByPopular,
    ByUsefull
  }

}
