using System;

namespace Azos.Sky.Social.Graph
{
  /// <summary>
  /// Contains social comment data
  /// </summary>
  [Serializable]
  public struct Comment
  {
    /// <summary>
    /// Comment
    /// </summary>
    /// <param name="id">ID</param>
    /// <param name="parentId">Parent comment ID</param>
    /// <param name="authorNode">Author node</param>
    /// <param name="targetNode">Target node</param>
    /// <param name="createDate">Creation Date</param>
    /// <param name="dimension">Scope of comments</param>
    /// <param name="publicationState">Publication state</param>
    /// <param name="rating">Rating (0,1,2,3,4,5)</param>
    /// <param name="message">Message</param>
    /// <param name="data">Data</param>
    /// <param name="likes">Likes count</param>
    /// <param name="dislikes">Dislikes count</param>
    /// <param name="complaintCount">Complaint count</param>
    /// <param name="responseCount">Response count</param>
    /// <param name="inUse">In use (InUse = false - comment has been deleted)</param>
    /// <param name="editable">Can be edited</param>
    public Comment(CommentID id,
                   CommentID? parentId,
                   GraphNode authorNode,
                   GraphNode targetNode,
                   DateTime createDate,
                   string dimension,

                   PublicationState publicationState,
                   RatingValue rating,
                   string message,
                   byte[] data,

                   uint likes,
                   uint dislikes,
                   uint complaintCount,
                   uint responseCount,

                   bool inUse,
                   bool editable)
    {
      ID = id;
      ParentID = parentId;
      AuthorNode = authorNode;
      TargetNode = targetNode;
      Create_Date = createDate;
      Dimension = dimension;

      PublicationState = publicationState;
      Rating = rating;
      Message = message;
      Data = data;

      Likes = likes;
      Dislikes = dislikes;
      ComplaintCount = complaintCount;
      ResponseCount = responseCount;

      IsRoot = !parentId.HasValue;
      In_Use = inUse;
      Editable = editable;
    }

    public readonly CommentID ID;
    public readonly CommentID? ParentID;
    public readonly GraphNode AuthorNode;
    public readonly GraphNode TargetNode;
    public readonly DateTime Create_Date;

    public readonly bool IsRoot;
    public readonly RatingValue Rating;
    public readonly string Message;
    public readonly byte[] Data;

    public readonly uint Likes;
    public readonly uint Dislikes;
    public readonly uint ComplaintCount;
    public readonly PublicationState PublicationState;
    public readonly bool In_Use;
    public readonly string Dimension;
    public readonly bool Editable;
    public readonly uint ResponseCount;

    public override string ToString()
    {
      return "[{0}-{1}]; [{2}] - {3}; {4}; {5}; ({6}) {7}; Like - {8}; Dislike - {9}".Args(ID.G_Volume, ID.G_Comment, AuthorNode.GDID, AuthorNode.OriginName, TargetNode.GDID, Create_Date, Rating, Message, Likes, Dislikes);
    }

    /// <summary>
    /// Make new Comment
    /// </summary>
    internal static Comment MakeNew(CommentID commentID, CommentID? parentID, GraphNode authorNode, GraphNode targetNode, DateTime create_Date, string dimension, PublicationState publicationState, RatingValue rating, string message, byte[] data)
    {
      return new Comment(commentID, parentID, authorNode, targetNode, create_Date, dimension, publicationState, rating, message, data, 0, 0, 0, 0, true, true);
    }
  }
}
