using System;
using System.Collections.Generic;

using Azos.Data;
using Azos.Sky.Coordination;

namespace Azos.Sky.Social.Graph
{
  /// <summary>
  /// Provides general base for all client fixtures
  /// </summary>
  public class GraphManagerBase : DisposableObject
  {
    public GraphManagerBase(HostSet hostSet)
    {
      m_HostSet = hostSet;
    }

    protected HostSet m_HostSet;

    public HostSet HostSet
    {
      get
      {
        return m_HostSet;
      }
    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_HostSet);
      base.Destructor();
    }
  }

  public abstract class GraphNodeManagerBase : GraphManagerBase, IGraphNodeSystem
  {
    public GraphNodeManagerBase(HostSet hostSet) : base(hostSet)
    {
    }

    public abstract GraphChangeStatus SaveNode(GraphNode node);
    public abstract GraphNode GetNode(GDID gNode);
    public abstract GraphChangeStatus DeleteNode(GDID gNode);
    public abstract GraphChangeStatus UndeleteNode(GDID gNode);
    public abstract GraphChangeStatus RemoveNode(GDID gNode);
  }

  public abstract class GraphCommentManagerBase : GraphManagerBase, IGraphCommentSystem
  {
    public GraphCommentManagerBase(HostSet hostSet) : base(hostSet)
    {
    }

    public abstract Comment Create(GDID gAuthorNode, GDID gTargetNode, string dimension, string content, byte[] data,
      PublicationState publicationState, RatingValue rating = RatingValue.Undefined, DateTime? epoch = null);

    public abstract Comment Respond(GDID gAuthorNode, CommentID parent, string content, byte[] data);

    public abstract GraphChangeStatus Update(CommentID ratingId, RatingValue value, string content, byte[] data);

    public abstract GraphChangeStatus DeleteComment(CommentID commentId);

    public abstract GraphChangeStatus Like(CommentID commentId, int deltaLike, int deltaDislike);

    public abstract bool IsCommentedByAuthor(GDID gNode, GDID gAuthor, string dimension);

    public abstract IEnumerable<SummaryRating> GetNodeSummaries(GDID gNode);

    public abstract IEnumerable<Comment> Fetch(CommentQuery query);

    public abstract IEnumerable<Comment> FetchResponses(CommentID commentId);

    public abstract IEnumerable<Complaint> FetchComplaints(CommentID commentId);

    public abstract Comment GetComment(CommentID commentId);

    public abstract GraphChangeStatus Complain(CommentID commentId, GDID gAuthorNode, string kind, string message);

    public abstract GraphChangeStatus Justify(CommentID commentId);
  }

  public abstract class GraphEventManagerBase : GraphManagerBase, IGraphEventSystem
  {
    public GraphEventManagerBase(HostSet hostSet) : base(hostSet)
    {
    }

    public abstract void EmitEvent(Event evt);

    public abstract void Subscribe(GDID gRecipientNode, GDID gEmitterNode, byte[] parameters);

    public abstract void Unsubscribe(GDID gRecipientNode, GDID gEmitterNode);

    public abstract long EstimateSubscriberCount(GDID gEmitterNode);

    public abstract IEnumerable<GraphNode> GetSubscribers(GDID gEmitterNode, long start, int count);
  }

  public abstract class GraphFriendManagerBase : GraphManagerBase, IGraphFriendSystem
  {
    public GraphFriendManagerBase(HostSet hostSet) : base(hostSet)
    {
    }

    public abstract IEnumerable<string> GetFriendLists(GDID gNode);

    public abstract GraphChangeStatus AddFriendList(GDID gNode, string list, string description);

    public abstract GraphChangeStatus DeleteFriendList(GDID gNode, string list);

    public abstract IEnumerable<FriendConnection> GetFriendConnections(FriendQuery query);

    public abstract GraphChangeStatus AddFriend(GDID gNode, GDID gFriendNode, bool? approve);

    public abstract GraphChangeStatus AssignFriendLists(GDID gNode, GDID gFriendNode, string lists);

    public abstract GraphChangeStatus DeleteFriend(GDID gNode, GDID gFriendNode);
  }

}