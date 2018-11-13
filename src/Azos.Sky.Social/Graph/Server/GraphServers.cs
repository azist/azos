using System;
using System.Collections.Generic;

using Azos.Data;

namespace Azos.Sky.Social.Graph.Server
{
  public sealed class GraphNodeSystemServer : IGraphNodeSystem
  {
    public IGraphNodeSystem Nodes { get { return GraphSystemService.Instance; } }

    public GraphChangeStatus SaveNode(GraphNode node)
    {
      return Nodes.SaveNode(node);
    }

    public GraphNode GetNode(GDID gNode)
    {
      return Nodes.GetNode(gNode);
    }

    public GraphChangeStatus DeleteNode(GDID gNode)
    {
      return Nodes.DeleteNode(gNode);
    }

    public GraphChangeStatus UndeleteNode(GDID gNode)
    {
      return Nodes.UndeleteNode(gNode);
    }

    public GraphChangeStatus RemoveNode(GDID gNode)
    {
      return Nodes.RemoveNode(gNode);
    }
  }

  public sealed class GraphCommentSystemServer : IGraphCommentSystem
  {
    public IGraphCommentSystem Comments { get { return GraphSystemService.Instance; } }

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
    /// <returns>Comment</returns>
    public Comment Create(GDID gAuthorNode,
      GDID gTargetNode,
      string dimension,
      string content,
      byte[] data,
      PublicationState publicationState,
      RatingValue value = RatingValue.Undefined,
      DateTime? timeStamp = null)
    {
      return Comments.Create(gAuthorNode, gTargetNode, dimension, content, data, publicationState, value, timeStamp);
    }

    /// <summary>
    /// Make response on target commentary
    /// </summary>
    /// <param name="gAuthorNode">Author</param>
    /// <param name="parent">Parent commentary</param>
    /// <param name="content">Content of commentary</param>
    /// <param name="data">Byte Array</param>
    /// <returns>New Comment</returns>
    public Comment Respond(GDID gAuthorNode, CommentID parent, string content, byte[] data)
    {
      return Comments.Respond(gAuthorNode, parent, content, data);
    }

    public GraphChangeStatus Update(CommentID commentId, RatingValue value, string content, byte[] data)
    {
      return Comments.Update(commentId, value, content, data);
    }

    /// <summary>
    /// Delete comment
    /// </summary>
    /// <param name="commentId">Existing Comment ID</param>
    public GraphChangeStatus DeleteComment(CommentID commentId)
    {
      return Comments.DeleteComment(commentId);
    }

    public GraphChangeStatus Like(CommentID commentId, int deltaLike, int deltaDislike)
    {
      return Comments.Like(commentId, deltaLike, deltaDislike);
    }

    public bool IsCommentedByAuthor(GDID gNode, GDID gAuthor, string dimension)
    {
      return Comments.IsCommentedByAuthor(gNode, gAuthor, dimension);
    }

    public IEnumerable<SummaryRating> GetNodeSummaries(GDID gNode)
    {
      return Comments.GetNodeSummaries(gNode);
    }

    public IEnumerable<Comment> Fetch(CommentQuery query)
    {
      return Comments.Fetch(query);
    }

    public IEnumerable<Comment> FetchResponses(CommentID commentId)
    {
      return Comments.FetchResponses(commentId);
    }

    public IEnumerable<Complaint> FetchComplaints(CommentID commentID)
    {
      return Comments.FetchComplaints(commentID);
    }

    public Comment GetComment(CommentID commentId)
    {
      return Comments.GetComment(commentId);
    }

    public GraphChangeStatus Complain(CommentID commentId, GDID gAuthorNode, string kind, string message)
    {
      return Comments.Complain(commentId, gAuthorNode, kind, message);
    }

    public GraphChangeStatus Justify(CommentID commentID)
    {
      return Comments.Justify(commentID);
    }
  }

  public sealed class GraphEventSystemServer : IGraphEventSystem
  {
    public IGraphEventSystem Events { get { return GraphSystemService.Instance; } }

    public void EmitEvent(Event evt)
    {
      Events.EmitEvent(evt);
    }

    public void Subscribe(GDID gRecipientNode, GDID gEmitterNode, byte[] parameters)
    {
      Events.Subscribe(gRecipientNode, gEmitterNode, parameters);
    }

    public void Unsubscribe(GDID gRecipientNode, GDID gEmitterNode)
    {
      Events.Unsubscribe(gRecipientNode, gEmitterNode);
    }

    public long EstimateSubscriberCount(GDID gEmitterNode)
    {
      return Events.EstimateSubscriberCount(gEmitterNode);
    }

    public IEnumerable<GraphNode> GetSubscribers(GDID gEmitterNode, long start, int count)
    {
      return Events.GetSubscribers(gEmitterNode, start, count);
    }
  }

  public sealed class GraphFriendSystemServer : IGraphFriendSystem
  {
    public IGraphFriendSystem Friends { get { return GraphSystemService.Instance; } }

    public IEnumerable<string> GetFriendLists(GDID gNode)
    {
      return Friends.GetFriendLists(gNode);
    }

    public GraphChangeStatus AddFriendList(GDID gNode, string list, string description)
    {
      return Friends.AddFriendList(gNode, list, description);
    }

    public GraphChangeStatus DeleteFriendList(GDID gNode, string list)
    {
      return Friends.DeleteFriendList(gNode, list);
    }

    public IEnumerable<FriendConnection> GetFriendConnections(FriendQuery query)
    {
      return Friends.GetFriendConnections(query);
    }

    public GraphChangeStatus AddFriend(GDID gNode, GDID gFriendNode, bool? approve)
    {
      return Friends.AddFriend(gNode, gFriendNode, approve);
    }

    public GraphChangeStatus AssignFriendLists(GDID gNode, GDID gFriendNode, string lists)
    {
      return Friends.AssignFriendLists(gNode, gFriendNode, lists);
    }

    public GraphChangeStatus DeleteFriend(GDID gNode, GDID gFriendNode)
    {
      return Friends.DeleteFriend(gNode, gFriendNode);
    }
  }
}