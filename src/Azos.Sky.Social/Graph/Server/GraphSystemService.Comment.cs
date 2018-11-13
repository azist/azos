using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Data;
using Azos.Log;

using Azos.Sky.Mdb;
using Azos.Sky.Social.Graph.Server.Data;
using Azos.Sky.Social.Graph.Server.Data.Schema;

namespace Azos.Sky.Social.Graph.Server
{
  public partial class GraphSystemService : IGraphCommentSystem
  {
    /// <summary>
    /// Create new comment with rating
    /// </summary>
    /// <param name="gAuthorNode">GDID of autor node</param>
    /// <param name="gTargetNode">GDID of target node</param>
    /// <param name="dimension">Scope for rating</param>
    /// <param name="content">Content</param>
    /// <param name="data">Byte array of data</param>
    /// <param name="publicationState">State of publication</param>
    /// <param name="value">star 1-2-3-4-5</param>
    /// <param name="timeStamp">Time of current action</param>
    /// <returns>ID</returns>
    Comment IGraphCommentSystem.Create(GDID gAuthorNode,
                                       GDID gTargetNode,
                                       string dimension,
                                       string content,
                                       byte[] data,
                                       PublicationState publicationState,
                                       RatingValue value,
                                       DateTime? timeStamp)
    {
      try
      {
        // 1. Get nodes for creation new comment
        var authorNode = DoGetNode(gAuthorNode);
        var targetNode = DoGetNode(gTargetNode);
        var currentDateTime = timeStamp ?? App.TimeSource.UTCNow;

        if (!GraphHost.CanCreateComment(authorNode, targetNode, dimension, currentDateTime, value))
            throw new GraphException(StringConsts.GS_CAN_NOT_CREATE_COMMENT_ERROR.Args(authorNode, targetNode, value));

        return DoCreate(targetNode,
                        authorNode,
                        null, dimension,
                        content,
                        data,
                        publicationState,
                        value,
                        currentDateTime);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "GraphCommentSystem.Create()", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_CREATE_COMMENT_ERROR.Args(gTargetNode, gAuthorNode), ex);
      }
    }

    /// <summary>
    /// Make response to target commentary
    /// </summary>
    /// <param name="gAuthorNode">Author</param>
    /// <param name="parentId">Parent commentary</param>
    /// <param name="content">Content of commentary</param>
    /// <param name="data">Byte Array</param>
    /// <returns>New CommentID</returns>
    public Comment Respond(GDID gAuthorNode, CommentID parentId, string content, byte[] data)
    {
      try
      {
        if (parentId.IsZero)
          throw new GraphException(StringConsts.GS_RESPONSE_BAD_PARENT_ID_ERROR);

        var currentDateTime = App.TimeSource.UTCNow;

        // Get parent comment
        var parent = getCommentRow(parentId);

        if (parent == null) throw new GraphException(StringConsts.GS_COMMENT_NOT_FOUND.Args(parentId.G_Comment));
        if (!parent.IsRoot) throw new GraphException(StringConsts.GS_PARENT_ID_NOT_ROOT.Args(parentId.G_Comment));

        var authorNode = GetNode(gAuthorNode);
        var parentComment = GraphCommentFetchStrategy.RowToComment(parent);

        if (!GraphHost.CanCreateCommentResponse(parentComment, authorNode, currentDateTime))
            throw new GraphException(StringConsts.GS_CAN_NOT_CREATE_RESPONSE_ERROR.Args(authorNode, parentComment.TargetNode));

        // Create new comment
        return DoCreate(parentComment.TargetNode,
                        authorNode,
                        parentId,
                        parent.Dimension,
                        content,
                        data,
                        GSPublicationState.ToPublicationState(parent.PublicationState),
                        RatingValue.Undefined,
                        App.TimeSource.UTCNow);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "GraphCommentSystem.Response", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_RESPONSE_COMMENT_ERROR.Args(parentId.G_Comment), ex);
      }
    }

    /// <summary>
    /// Updates existing rating by ID
    /// </summary>
    public GraphChangeStatus Update(CommentID commentId, RatingValue value, string content, byte[] data)
    {
      if(commentId.IsZero)
        return GraphChangeStatus.NotFound;

      try
      {
        // Taking comment row
        var currentDateTime = App.TimeSource.UTCNow;
        var ctxComment = ForComment(commentId.G_Volume); // CRUD context for comment
        var comment = getCommentRow(commentId, ctxComment);
        if (comment == null)
          return GraphChangeStatus.NotFound;

        var targetNode = DoGetNode(comment.G_TargetNode);
        if ((currentDateTime - comment.Create_Date) > GraphHost.EditCommentSpan(targetNode, comment.Dimension))
          return GraphChangeStatus.NotFound;

        var ctxNode = ForNode(comment.G_TargetNode); // CRUD context for target node

        // Updating fields
        var filter = "Message,Data";
        comment.Message = content;
        comment.Data = data;

        // if comment is root and rating value is not RatingValue.Undefined
        // Update rating
        if (comment.IsRoot && value != RatingValue.Undefined)
        {
          // Update NodeRating
          var nodeRating = getNodeRating(comment.G_TargetNode, comment.Dimension, ctxNode);
          nodeRating.UpdateRating((RatingValue) comment.Rating, -1);
          nodeRating.UpdateRating(value, 1);
          ctxNode.Update(nodeRating, filter: "Last_Change_Date,Cnt,Rating1,Rating2,Rating3,Rating4,Rating5".OnlyTheseFields());

          // Update rating value of comment
          comment.Rating = (byte) value;
          filter = "Rating," + filter;
        }
        var resComment = ctxComment.Update(comment, filter: filter.OnlyTheseFields());
        return resComment == 0 ? GraphChangeStatus.NotFound : GraphChangeStatus.Updated;
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "GraphCommentSystem.Update", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_UPDATE_RATING_ERROR.Args(commentId.G_Comment), ex);
      }
    }

    /// <summary>
    /// Delete comment
    /// </summary>
    /// <param name="commentId">Existing Comment ID</param>
    public GraphChangeStatus DeleteComment(CommentID commentId)
    {
      if(commentId.IsZero)
        return GraphChangeStatus.NotFound;

      try
      {
        //1. Try get comment row
        var ctxComment = ForComment(commentId.G_Volume); // CRUD context for comment
        var comment = getCommentRow(commentId);
        if (comment == null)
          return GraphChangeStatus.NotFound;

        var ctxNode = ForNode(comment.G_TargetNode); // CRUD context for target node

        //2. Delete comment
        comment.In_Use = false;

        if (comment.IsRoot) // Only Root comments has ratings and counts in comments of target node
        {
          //3. If comment has rating value, decrease value of that rating from target rating node
          var nodeRating = getNodeRating(comment.G_TargetNode, comment.Dimension, ctxNode);
          nodeRating.UpdateCount(-1); // Update count of comments
          nodeRating.UpdateRating((RatingValue)comment.Rating, -1); // Update ratings (if RatingValue.Undefined - nothing happens)
          nodeRating.Last_Change_Date = App.TimeSource.UTCNow;
          ctxNode.Update(nodeRating, filter: "Last_Change_Date,Cnt,Rating1,Rating2,Rating3,Rating4,Rating5".OnlyTheseFields());
        }
        // Update parent "ResponseCount" , if comment is not root
        else if (comment.G_Parent.HasValue)
        {
          var parentCommendID = new CommentID(comment.G_CommentVolume, comment.G_Parent.Value);
          var ctxParent = ForComment(parentCommendID.G_Volume);
          var parent = getCommentRow(parentCommendID, ctxParent);
          if (parent != null && parent.ResponseCount > 0)
          {
            parent.UpdateResponseCount(-1);
            ctxParent.Update(parent, filter: "ResponseCount".OnlyTheseFields());
          }
        }

        var res = ctxComment.Update(comment);
        return res > 0 ? GraphChangeStatus.Deleted : GraphChangeStatus.NotFound;
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "DeleteComment", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_DELETE_COMMENT_ERROR.Args(commentId.G_Comment), ex);
      }
    }

    /// <summary>
    /// Justify moderated comment
    /// </summary>
    /// <param name="commentID">Existing Comment ID</param>
    public GraphChangeStatus Justify(CommentID commentID)
    {
      if(commentID.IsZero)
        return GraphChangeStatus.NotFound;

      try
      {
        var ctxComment = ForComment(commentID.G_Volume);

        var comment = getCommentRow(commentID, ctxComment);
        if (comment == null)
          return GraphChangeStatus.NotFound;

        var qry = Queries.CancelComplaintsByComment(commentID.G_Comment);
        ctxComment.ExecuteWithoutFetch(qry);

        comment.ComplaintCount = 0;
        ctxComment.Update(comment, filter: "ComplaintCount".OnlyTheseFields());
        return GraphChangeStatus.Updated;
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "Justify", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_JUSTIFY_COMMENT_ERROR.Args(commentID.G_Comment), ex);
      }
    }

    /// <summary>
    /// Updates likes/dislikes
    /// </summary>
    public GraphChangeStatus Like(CommentID messageId, int deltaLike, int deltaDislike)
    {
      if(messageId.IsZero) return GraphChangeStatus.NotFound;

      try
      {
        var ctxRating = ForComment(messageId.G_Volume);
        var qry = Queries.UpdateLike(messageId.G_Volume, messageId.G_Comment, deltaLike, deltaDislike);
        var res = ctxRating.ExecuteWithoutFetch(qry);
        return res==0 ? GraphChangeStatus.NotFound : GraphChangeStatus.Updated;
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "Like", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_SET_LIKE_ERROR.Args(messageId.G_Comment), ex);
      }
    }

    /// <summary>
    /// Check if target node has existing comment, made by author
    /// </summary>
    /// <param name="gNode">Target node</param>
    /// <param name="gAuthor">Author</param>
    /// <param name="dimension">Scope of comments</param>
    public bool IsCommentedByAuthor(GDID gNode, GDID gAuthor, string dimension)
    {
      try
      {
        return DoIsCommentedByAuthor(gNode, gAuthor, dimension);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "DoIsCommentedByAuthor", ex.ToMessageWithType(), ex);
        throw new GraphException("IsCommentedByAuthor gNode={0} gAuthor={1} dimension={2}".Args(gNode, gAuthor, dimension), ex);
      }
    }

    public IEnumerable<SummaryRating> GetNodeSummaries(GDID gNode)
    {
      try
      {
        return DoGetNodeSummaries(gNode);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "GetNodeSummaries", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_GET_RATING_ERROR.Args(gNode), ex);
      }
    }

    public IEnumerable<Comment> Fetch(CommentQuery query)
    {
      try
      {
        return DoFetch(query);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "Fetch", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_FETCH_RATING_ERROR.Args(query.G_TargetNode), ex);
      }
    }

    public IEnumerable<Comment> FetchResponses(CommentID commentId)
    {
      try
      {
        return DoFetchResponses(commentId).ToArray();
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "DoFetchResponses", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_FETCH_RESPONSE_ERROR.Args(commentId.G_Comment), ex);
      }
    }

    public IEnumerable<Complaint> FetchComplaints(CommentID commentID)
    {
      try
      {
        return DoFetchComplaints(commentID).ToArray();
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "FetchComplaints", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_FETCH_COMPLAINTS_ERROR.Args(commentID.G_Comment), ex);
      }
    }

    public Comment GetComment(CommentID commentId)
    {
      try
      {
        return DoGetComment(commentId);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "GetComment", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_GET_COMMENT_ERROR.Args(commentId.G_Comment), ex);
      }
    }

    public GraphChangeStatus Complain(CommentID commentId, GDID gAuthorNode, string kind, string message)
    {
      if(commentId.IsZero) return GraphChangeStatus.NotFound;

      try
      {
        var comment = getCommentRow(commentId);
        if (comment == null) return GraphChangeStatus.NotFound;

        var ctx = ForComment(commentId.G_Volume);
        var complaintRow = new ComplaintRow(true)
        {
          G_Comment = commentId.G_Comment,
          G_AuthorNode = gAuthorNode,
          Kind = kind,
          Message = message,
          Create_Date = App.TimeSource.UTCNow,
          In_Use = true
        };
        ctx.Insert(complaintRow);

        comment.ComplaintCount++;
        ctx.Update(comment, filter: "ComplaintCount".OnlyTheseFields());

        return GraphChangeStatus.Updated;
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "Complain", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_COMPLAINT_ERROR.Args(commentId.G_Comment), ex);
      }
    }

    #region Protected

    /// <summary>
    /// Check if target node has existing comment, made by author
    /// </summary>
    /// <param name="gNode">Target node</param>
    /// <param name="gAuthor">Author</param>
    /// <param name="dimension">Scope of comments</param>
    protected virtual bool DoIsCommentedByAuthor(GDID gNode, GDID gAuthor, string dimension)
    {
      if (gNode.IsZero) throw new GraphException("gNode.IsZero=true");
      if (gAuthor.IsZero) throw new GraphException("gAuthor.IsZero=true");
      if (dimension.IsNullOrWhiteSpace()) throw new GraphException("dimension=null");

      var ctxNode = ForNode(gNode);
      var qryVolumes = Queries.FindCommentVolumes<CommentVolumeRow>(gNode, dimension, MaxScanTailCommentVolumes, App.TimeSource.UTCNow);
      var volumes = ctxNode.LoadEnumerable(qryVolumes);
      var hasComments = volumes.Any(v => hasAnyComments(v.G_CommentVolume, gNode, gAuthor, dimension));

      return hasComments;
    }

    /// <summary>
    /// Creating new comment
    /// </summary>
    /// <param name="targetNode">Target node</param>
    /// <param name="authorNode">Author node, who made comment</param>
    /// <param name="parentID">Parent comment id</param>
    /// <param name="dimension">Scope of comment</param>
    /// <param name="content">Content</param>
    /// <param name="data">Byte array of data</param>
    /// <param name="publicationState">State of publication</param>
    /// <param name="value">star 1-2-3-4-5</param>
    /// <param name="creationTime">Time of current action</param>
    /// <returns>ID</returns>
    protected virtual Comment DoCreate(GraphNode targetNode,
                                       GraphNode authorNode,
                                       CommentID? parentID,
                                       string dimension,
                                       string content,
                                       byte[] data,
                                       PublicationState publicationState,
                                       RatingValue value,
                                       DateTime? creationTime)
    {
      // Create comment and, if need, volume
      var ctxNode = ForNode(targetNode.GDID); // get shard context for target node
      var volume = parentID.HasValue // if we have parentID, we need to place response comment in same volume as parent
        ? getVolume(targetNode.GDID, parentID.Value.G_Volume, ctxNode)
        : getEmptyVolume(targetNode.GDID, dimension, ctxNode);

      if(volume == null)
        throw new GraphException(StringConsts.GS_RESPONSE_VOLUME_MISSING_ERROR);

      var comment = new CommentRow(true)
      {
        G_CommentVolume = volume.G_CommentVolume,
        G_Parent = parentID.HasValue ? parentID.Value.G_Comment : (GDID?)null,
        G_AuthorNode = authorNode.GDID,
        G_TargetNode = targetNode.GDID,
        Create_Date = creationTime ?? App.TimeSource.UTCNow,
        Dimension = dimension,

        Message = content,
        Data = data,
        PublicationState = GSPublicationState.ToDomainString(publicationState),
        Rating = (byte)value,

        Like = 0,
        Dislike = 0,
        ComplaintCount = 0,
        ResponseCount = 0,

        IsRoot = !parentID.HasValue,
        In_Use = true
      };
      ForComment(volume.G_CommentVolume).Insert(comment);

      // Update ratings , if comment is root
      if (comment.IsRoot)
      {
        var nodeRating = getNodeRating(targetNode.GDID, dimension, ctxNode);
        nodeRating.UpdateCount(1); // Update count of commentaries (only root commentaries counts)
        nodeRating.UpdateRating(value, 1); //Update ratings
        nodeRating.Last_Change_Date = App.TimeSource.UTCNow; // update change time
        ctxNode.Update(nodeRating, filter: "Last_Change_Date,Cnt,Rating1,Rating2,Rating3,Rating4,Rating5".OnlyTheseFields());
      }
      // Update parent ResponseCount, if comment is not root
      else if(parentID.HasValue)
      {
        var ctxParent = ForComment(parentID.Value.G_Volume);
        var parent = getCommentRow(parentID.Value, ctxParent);
        if (parent != null)
        {
          parent.UpdateResponseCount(1);
          ctxParent.Update(parent, filter: "ResponseCount".OnlyTheseFields());
        }
      }

      // Update count of commentaries in volume
      volume.Count++;
      ctxNode.Update(volume, filter: "Count".OnlyTheseFields());
      return Comment.MakeNew(new CommentID(comment.G_CommentVolume, comment.GDID),
                             parentID,
                             authorNode,
                             targetNode,
                             comment.Create_Date,
                             comment.Dimension,
                             GSPublicationState.ToPublicationState(comment.PublicationState),
                             (RatingValue)comment.Rating,
                             comment.Message,
                             comment.Data);
    }

    protected virtual IEnumerable<SummaryRating> DoGetNodeSummaries(GDID gNode)
    {
      var qry = Queries.FindNodeRatings<NodeRatingRow>(gNode, App.TimeSource.UTCNow);
      var rows = ForNode(gNode).LoadEnumerable(qry);
      return rows.Select(row => new SummaryRating(row.G_Node,
        row.Create_Date,
        row.Last_Change_Date,
        row.Dimension,
        row.Cnt,
        row.Rating1,
        row.Rating2,
        row.Rating3,
        row.Rating4,
        row.Rating5)).ToArray();
    }

    protected virtual IEnumerable<Comment> DoFetch(CommentQuery query)
    {
      return GraphCommentFetchStrategy.Fetch(query);
    }

    protected virtual IEnumerable<Comment> DoFetchResponses(CommentID commentId, int offset = 0, int limit = 1) // todo доделать
    {
      var qryResponses = Queries.FindResponses<CommentRow>(commentId);
      var responses = ForComment(commentId.G_Volume).LoadEnumerable(qryResponses);
      return responses.Where(r => r.In_Use || (!r.In_Use && r.ResponseCount > 0)).Select(GraphCommentFetchStrategy.RowToComment);
    }

    protected IEnumerable<Complaint> DoFetchComplaints(CommentID commentID)
    {
      var qryComplaints = Queries.FindComplaints<ComplaintRow>(commentID.G_Comment);
      var complaints = ForComment(commentID.G_Volume).LoadEnumerable(qryComplaints);
      return complaints.Select(c => rowToComplaint(commentID, c));
    }

    protected CRUDOperations ForComment(GDID gVolume)
    {
      return DataStore.PartitionedOperationsFor(SocialConsts.MDB_AREA_COMMENT, gVolume);
    }

    #endregion

    #region .pvt

    /// <summary>
    /// Get existing not full Comment Volume or create a new one
    /// </summary>
    /// <param name="ctxNode">TargetNode context</param>
    /// <param name="gTargetNode">TargetNode GDID</param>
    /// <param name="dimension">Scope of comment</param>
    /// <returns>Existing or new volume</returns>
    private CommentVolumeRow getEmptyVolume(GDID gTargetNode, string dimension, CRUDOperations? ctxNode = null)
    {
      var ctx = ctxNode ?? ForNode(gTargetNode);
      var qryVolume = Queries.FindEmptyCommentVolume<CommentVolumeRow>(gTargetNode, dimension, MaxSizeCommentVolume);
      var volume = ctxNode.LoadDoc(qryVolume);
      if (volume == null)
      {
        volume = new CommentVolumeRow
        {
          G_Owner = gTargetNode,
          G_CommentVolume = NodeRow.GenerateNewNodeRowGDID(),
          Dimension = dimension,
          Count = 0,
          Create_Date = App.TimeSource.UTCNow
        };
        ctx.Insert(volume);
      }
      return volume;
    }

    /// <summary>
    /// Get existing CommentVolume by TargetNode and Volume
    /// </summary>
    /// <param name="ctxNode">TargetNode context</param>
    /// <param name="gTargetNode">TargetNode GDID</param>
    /// <param name="gVolume">Volume GDID</param>
    /// <returns>Existing volume</returns>
    private CommentVolumeRow getVolume(GDID gTargetNode, GDID gVolume, CRUDOperations? ctxNode = null)
    {
      var ctx = ctxNode ?? ForNode(gTargetNode);
      var qryVolume = Queries.FindCommentVolume<CommentVolumeRow>(gTargetNode, gVolume);
      return ctx.LoadDoc(qryVolume);
    }

    /// <summary>
    /// Get existing NodeRating or create a new one
    /// </summary>
    /// <param name="ctxNode">TargetNode context</param>
    /// <param name="gTargetNode">TargetNode GDID</param>
    /// <param name="dimension">Scope of comment</param>
    /// <returns>Existing or new NodeRating</returns>
    private NodeRatingRow getNodeRating(GDID gTargetNode, string dimension, CRUDOperations? ctxNode = null)
    {
      var ctx = ctxNode ?? ForNode(gTargetNode);
      var qryNodeRating = Queries.FindNodeRating<NodeRatingRow>(gTargetNode, dimension);
      var nodeRating = ctxNode.LoadDoc(qryNodeRating);
      if(nodeRating == null)
      {
        nodeRating = new NodeRatingRow
        {
          G_Node = gTargetNode,
          Create_Date = App.TimeSource.UTCNow,
          Last_Change_Date = App.TimeSource.UTCNow,
          Dimension = dimension,
          Rating1 = 0,
          Rating2 = 0,
          Rating3 = 0,
          Rating4 = 0,
          Rating5 = 0,
          Cnt = 0
        };
        ctx.Insert(nodeRating);
      }
      return nodeRating;
    }

    private Comment DoGetComment(CommentID commentId)
    {
      var qry = Queries.FindCommentByGDID<CommentRow>(commentId.G_Comment);
      var row = ForComment(commentId.G_Volume).LoadDoc(qry);
      if (row == null) throw new GraphException("Comment not found, CommendID = {0}".Args(commentId));
      return GraphCommentFetchStrategy.RowToComment(row);
    }

    private CommentRow getCommentRow(CommentID commentId, CRUDOperations? ctxComment = null)
    {
      if (commentId.IsZero) return null;
      var ctx = ctxComment ?? ForComment(commentId.G_Volume);
      var qryComment = Queries.FindCommentByGDID<CommentRow>(commentId.G_Comment);
      return ctx.LoadDoc(qryComment);
    }

    private bool hasAnyComments(GDID gVolume, GDID gNode, GDID gAuthor, string dimension)
    {
      var qryHasComments = Queries.HasCommentsCreatedByAuthor<CommentRow>(gNode, gAuthor, dimension);
      var row = ForComment(gVolume).LoadOneDoc(qryHasComments);
      return row != null;
    }

    private Complaint rowToComplaint(CommentID commentID, ComplaintRow row)
    {
      if (row == null) return default(Complaint);

      return new Complaint(new CommentID(commentID.G_Volume, row.G_Comment),
                           row.GDID,
                           GetNode(row.G_AuthorNode),
                           row.Kind,
                           row.Message,
                           row.Create_Date,
                           row.In_Use);
    }

    #endregion
  }
}