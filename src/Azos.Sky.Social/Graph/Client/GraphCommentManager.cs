using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Data;
using Azos.Sky.Coordination;

namespace Azos.Sky.Social.Graph
{
  public sealed class GraphCommentManager : GraphCommentManagerBase
  {
    public GraphCommentManager(HostSet hostSet) : base(hostSet)
    {
    }

    public override Comment Create(GDID gAuthorNode, GDID gTargetNode, string dimension, string content, byte[] data,
      PublicationState publicationState, RatingValue rating = RatingValue.Undefined, DateTime? epoch = null)
    {
      var pair = HostSet.AssignHost(gTargetNode);
      return Contracts.ServiceClientHub
        .CallWithRetry<IGraphCommentSystemClient, Comment>(
          commentSystem => commentSystem.Create(gAuthorNode, gTargetNode, dimension, content, data, publicationState, rating, epoch),
          pair.Select(host => host.RegionPath)
          );
    }

    public override Comment Respond(GDID gAuthorNode, CommentID parent, string content, byte[] data)
    {
      var pair = HostSet.AssignHost(parent.G_Volume);
      return Contracts.ServiceClientHub
        .CallWithRetry<IGraphCommentSystemClient, Comment>(
          commentSystem => commentSystem.Respond(gAuthorNode, parent, content, data),
          pair.Select(host => host.RegionPath)
        );
    }

    public override GraphChangeStatus Update(CommentID ratingId, RatingValue value, string content, byte[] data)
    {
      var pair = HostSet.AssignHost(ratingId.G_Volume);
      return Contracts.ServiceClientHub
        .CallWithRetry<IGraphCommentSystemClient, GraphChangeStatus>(
          commentSystem => commentSystem.Update(ratingId, value, content, data),
          pair.Select(host => host.RegionPath)
        );
    }

    public override GraphChangeStatus DeleteComment(CommentID commentId)
    {
      var pair = HostSet.AssignHost(commentId.G_Volume);
      return Contracts.ServiceClientHub
        .CallWithRetry<IGraphCommentSystemClient, GraphChangeStatus>(
          commentSystem => commentSystem.DeleteComment(commentId),
          pair.Select(host => host.RegionPath)
        );
    }

    public override GraphChangeStatus Like(CommentID commentId, int deltaLike, int deltaDislike)
    {
      var pair = HostSet.AssignHost(commentId.G_Volume);
      return Contracts.ServiceClientHub
        .CallWithRetry<IGraphCommentSystemClient, GraphChangeStatus>(
          commentSystem => commentSystem.Like(commentId, deltaLike, deltaDislike),
          pair.Select(host => host.RegionPath)
        );
    }

    public override bool IsCommentedByAuthor(GDID gNode, GDID gAuthor, string dimension)
    {
      var pair = HostSet.AssignHost(gNode);
      return Contracts.ServiceClientHub
        .CallWithRetry<IGraphCommentSystemClient, bool>(
          commentSystem => commentSystem.IsCommentedByAuthor(gNode, gAuthor, dimension),
          pair.Select(host => host.RegionPath)
        );
    }

    public override IEnumerable<SummaryRating> GetNodeSummaries(GDID gNode)
    {
      var pair = HostSet.AssignHost(gNode);
      return Contracts.ServiceClientHub
        .CallWithRetry<IGraphCommentSystemClient, IEnumerable<SummaryRating>>(
          commentSystem => commentSystem.GetNodeSummaries(gNode),
          pair.Select(host => host.RegionPath)
        );
    }

    public override IEnumerable<Comment> Fetch(CommentQuery query)
    {
      var pair = HostSet.AssignHost(query.G_TargetNode);
      return Contracts.ServiceClientHub
        .CallWithRetry<IGraphCommentSystemClient, IEnumerable<Comment>>(
          commentSystem => commentSystem.Fetch(query),
          pair.Select(host => host.RegionPath)
        );
    }

    public override IEnumerable<Comment> FetchResponses(CommentID commentId)
    {
      var pair = HostSet.AssignHost(commentId.G_Volume);
      return Contracts.ServiceClientHub
        .CallWithRetry<IGraphCommentSystemClient, IEnumerable<Comment>>(
          commentSystem => commentSystem.FetchResponses(commentId),
          pair.Select(host => host.RegionPath)
        );
    }

    public override IEnumerable<Complaint> FetchComplaints(CommentID commentId)
    {
      var pair = HostSet.AssignHost(commentId.G_Volume);
      return Contracts.ServiceClientHub
        .CallWithRetry<IGraphCommentSystemClient, IEnumerable<Complaint>>(
          commentSystem => commentSystem.FetchComplaints(commentId),
          pair.Select(host => host.RegionPath)
        );
    }

    public override Comment GetComment(CommentID commentId)
    {
      var pair = HostSet.AssignHost(commentId.G_Volume);
      return Contracts.ServiceClientHub
        .CallWithRetry<IGraphCommentSystemClient, Comment>(
          commentSystem => commentSystem.GetComment(commentId),
          pair.Select(host => host.RegionPath)
        );
    }

    public override GraphChangeStatus Complain(CommentID commentId, GDID gAuthorNode, string kind, string message)
    {
      var pair = HostSet.AssignHost(commentId.G_Volume);
      return Contracts.ServiceClientHub
            .CallWithRetry<IGraphCommentSystemClient, GraphChangeStatus>(
               commentSystem => commentSystem.Complain(commentId, gAuthorNode, kind, message),
               pair.Select(host => host.RegionPath)
             );
    }

    public override GraphChangeStatus Justify(CommentID commentID)
    {
      var pair = HostSet.AssignHost(commentID.G_Volume);
      return Contracts.ServiceClientHub
            .CallWithRetry<IGraphCommentSystemClient, GraphChangeStatus>(
               commentSystem => commentSystem.Justify(commentID),
               pair.Select(host => host.RegionPath)
             );
    }
  }
  /// <summary>
  /// Заглушка для интерфейса IGraphCommentSystem на клиенте
  /// </summary>
  public sealed class NOPGraphCommentManager : GraphCommentManagerBase
  {
    public NOPGraphCommentManager(HostSet hostSet) : base(hostSet)
    {
    }

    public override Comment Create(GDID gAuthorNode, GDID gTargetNode, string dimension, string content, byte[] data,
      PublicationState publicationState, RatingValue rating = RatingValue.Undefined, DateTime? epoch = null)
    {
      return default(Comment);
    }

    public override Comment Respond(GDID gAuthorNode, CommentID parent, string content, byte[] data)
    {
      return default(Comment);
    }

    public override GraphChangeStatus Update(CommentID ratingId, RatingValue value, string content, byte[] data)
    {
      return GraphChangeStatus.NotFound;
    }

    public override GraphChangeStatus DeleteComment(CommentID commentId)
    {
      return GraphChangeStatus.NotFound;
    }

    public override GraphChangeStatus Like(CommentID commentId, int deltaLike, int deltaDislike)
    {
      return GraphChangeStatus.NotFound;
    }

    public override bool IsCommentedByAuthor(GDID gNode, GDID gAuthor, string dimension)
    {
      return false;
    }

    public override IEnumerable<SummaryRating> GetNodeSummaries(GDID gNode)
    {
      yield break;
    }

    public override IEnumerable<Comment> Fetch(CommentQuery query)
    {
      yield break;
    }

    public override IEnumerable<Comment> FetchResponses(CommentID commentId)
    {
      yield break;
    }

    public override IEnumerable<Complaint> FetchComplaints(CommentID commentId)
    {
      yield break;
    }

    public override Comment GetComment(CommentID commentId)
    {
      return default(Comment);
    }

    public override GraphChangeStatus Complain(CommentID commentId, GDID gAuthorNode, string kind, string message)
    {
      return GraphChangeStatus.NotFound;
    }

    public override GraphChangeStatus Justify(CommentID commentID)
    {
      return GraphChangeStatus.NotFound;
    }
  }
}