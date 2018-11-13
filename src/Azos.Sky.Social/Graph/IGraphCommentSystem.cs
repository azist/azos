using System;
using System.Collections.Generic;


using Azos.Data;
using Azos.Glue;

using Azos.Sky.Contracts;

namespace Azos.Sky.Social.Graph
{
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface IGraphCommentSystem : ISkyService
  {
    /// <summary>
    /// Create new comment with rating
    /// </summary>
    /// <param name="gAuthorNode">GDID of author node</param>
    /// <param name="gTargetNode">GDID of target node</param>
    /// <param name="dimension">Scope for rating</param>
    /// <param name="content">Content</param>
    /// <param name="data">Byte array of data</param>
    /// <param name="publicationState">State of publication</param>
    /// <param name="value">star 1-2-3-4-5</param>
    /// <param name="timeStamp">Time of current action</param>
    /// <returns>Comment</returns>
    Comment Create(GDID gAuthorNode,
                   GDID gTargetNode,
                   string dimension,
                   string content,
                   byte[] data,
                   PublicationState publicationState,
                   RatingValue value = RatingValue.Undefined,
                   DateTime? timeStamp = null);

    /// <summary>
    /// Make response to target commentary
    /// </summary>
    /// <param name="gAuthorNode">Author</param>
    /// <param name="parent">Parent commentary</param>
    /// <param name="content">Content of commentary</param>
    /// <param name="data">Byte Array</param>
    /// <returns>New CommentID</returns>
    Comment Respond(GDID gAuthorNode, CommentID parent, string content, byte[] data);

    /// <summary>
    /// Updates existing rating by ID
    /// </summary>
    GraphChangeStatus Update(CommentID ratingId, RatingValue value, string content, byte[] data);

    /// <summary>
    /// Delete comment
    /// </summary>
    /// <param name="commentId">Existing Comment ID</param>
    GraphChangeStatus DeleteComment(CommentID commentId);

    /// <summary>
    /// Updates likes/dislikes
    /// </summary>
    GraphChangeStatus Like(CommentID commentId, int deltaLike, int deltaDislike);

    /// <summary>
    /// Check if target node has existing comment, made by author
    /// </summary>
    /// <param name="gNode">Target node</param>
    /// <param name="gAuthor">Author</param>
    /// <param name="dimension">Scope of comments</param>
    bool IsCommentedByAuthor(GDID gNode, GDID gAuthor, string dimension);

    /// <summary>
    /// Returns summary ratings per node, dimensions and create date (rating epochs)
    /// </summary>
    IEnumerable<SummaryRating> GetNodeSummaries(GDID gNode);

    /// <summary>
    /// Returns comments for Target Node
    /// </summary>
    IEnumerable<Comment> Fetch(CommentQuery query);

    /// <summary>
    /// Returns comments for comment by Comment ID
    /// </summary>
    IEnumerable<Comment> FetchResponses(CommentID commentId);

    /// <summary>
    /// Returns comment complaints by comment ID
    /// </summary>
    /// <param name="commentId"></param>
    /// <returns></returns>
    IEnumerable<Complaint> FetchComplaints(CommentID commentId);

    /// <summary>
    /// Return comment by comment ID
    /// </summary>
    Comment GetComment(CommentID commentId);

    /// <summary>
    /// Complain about comment
    /// </summary>
    GraphChangeStatus Complain(CommentID commentId, GDID gAuthorNode, string kind, string message);

    /// <summary>
    /// Justify moderated comment
    /// </summary>
    GraphChangeStatus Justify(CommentID commentID);
  }

  public interface IGraphCommentSystemClient : IGraphCommentSystem, ISkyServiceClient
  {
    //todo Add async versions
  }

  public struct CommentQuery : IEquatable<CommentQuery>
  {
    //todo design
    public const int COMMENT_BLOCK_SIZE = 32;

    public static CommentQuery MakeNew(GDID gTargetNode, string dimension, CommentOrderType orderType, bool asc, DateTime asOfDate, int blockIndex)
    {
      return new CommentQuery(gTargetNode, dimension, orderType, asc, asOfDate, blockIndex);
    }

    internal CommentQuery(GDID gTargetNode, string dimension, CommentOrderType orderType, bool asc, DateTime asOfDate, int blockIndex)
    {
      G_TargetNode = gTargetNode;
      Dimension = dimension;
      OrderType = orderType;
      Ascending = asc;
      AsOfDate = asOfDate;
      BlockIndex = blockIndex;
    }

    public readonly GDID G_TargetNode;
    public readonly string Dimension;
    public readonly CommentOrderType OrderType;
    public readonly bool Ascending;

    public readonly DateTime AsOfDate;
    public readonly int BlockIndex;


    public bool Equals(CommentQuery other)
    {
      return this.G_TargetNode == other.G_TargetNode &&
        this.Dimension == other.Dimension &&
        this.OrderType == other.OrderType &&
        this.Ascending == other.Ascending &&
        this.BlockIndex == other.BlockIndex
      ;
    }

    public override int GetHashCode()
    {
      return G_TargetNode.GetHashCode() ^ BlockIndex;
    }

    public override bool Equals(object obj)
    {
      if (! (obj is CommentQuery)) return false;
      return Equals((CommentQuery)obj);
    }

  }
}
