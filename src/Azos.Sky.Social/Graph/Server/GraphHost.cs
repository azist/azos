using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;
using Azos.Data;

using Azos.Sky.Social.Graph.Server.Data;

namespace Azos.Sky.Social.Graph.Server
{
  /// <summary>
  /// Provides hosting API for the graph system in the particular business system scope
  /// </summary>
  public abstract class GraphHost : ApplicationComponent, IConfigurable
  {

    protected GraphHost(GraphOperationContext director, IConfigSectionNode config) : base(director)
    {
      ConfigAttribute.Apply(this, config);
    }

    /// <summary>
    /// Limits the number of responses to comments
    /// </summary>
    public abstract int MaxResponseForComment { get; }

    /// <summary>
    /// Perform actual work of event delivery.
    /// Returns true if event was (scheduled-to be) delivered
    /// </summary>
    public bool DeliverEvent(GraphNode nodeRecipient, GraphNode nodeSender, Event evt, string subscriptionParameters)
    {
      return DoDeliverEvent(nodeRecipient, nodeSender, evt, subscriptionParameters);
    }

    //filter event batch start
    //filter event recipient
    //filter event batch end

    public void Configure(IConfigSectionNode node)
    {
      DoConfigure(node);
    }


    /// <summary>
    /// Convert binary data to Row Node
    /// </summary>
    public abstract TypedDoc NodeBinaryDataToObject(string nodeType, byte[] data);

    /// <summary>
    /// Convert Row Node to binary data
    /// </summary>
    public abstract byte[] ObjectToNodeBinaryData(string nodeType, TypedDoc data);

    /// <summary>
    /// Filter by query
    /// </summary>
    public abstract IEnumerable<GraphNode> FilterByOriginQuery(IEnumerable<GraphNode> data, string orgQry);

    /// <summary>
    ///
    /// </summary>
    public abstract IEnumerable<SubscriberRow> FilterEventsChunk(IEnumerable<SubscriberRow> subscribersChunk, Event evt, IConfigSectionNode cfg);

    /// <summary>
    /// This function must not leak exceptions
    /// Returns the subscriptions that could not be delivered now - the system will try to redeliver them later asynchronously
    /// </summary>
    public abstract IEnumerable<SubscriberRow> DeliverEventsChunk(IEnumerable<SubscriberRow> filtered, Event evt, IConfigSectionNode cfg);

    /// <summary>
    ///  Returns true if specified node types can be friends in the specified direction.
    ///  Keep in mind that friendship is bidirectional so this method only checks the initiating party
    /// </summary>
    public abstract bool CanBeFriends(string fromNodeType, string toNodeType);

    /// <summary>
    ///  Returns true if specified node types can be subscribed
    /// </summary>
    public abstract bool CanBeSubscribed(string subscriberNodeType, string emitterNodeType);

    #region Graph comment

    /// <summary>
    ///  Returns true if specified node type can rate other nodes
    /// </summary>
    public abstract bool CanBeRatingActor(string ratingNodeType, string dimension);

    /// <summary>
    ///  Returns true if the specified node type requires rating for the specified dimension
    /// </summary>
    public abstract bool CommentRatingRequired(string ratedNodeType, string dimension);

    /// <summary>
    ///  Returns a span of time within which a comment can be edited
    /// </summary>
    public abstract TimeSpan EditCommentSpan(GraphNode targetNode, string dimension);

    /// <summary>
    ///  Returns true if authorNode can create new comment of specified targetNode as of commentDate and
    ///  if rating value can be applied
    /// </summary>
    public virtual bool CanCreateComment(GraphNode authorNode,
                                         GraphNode targetNode,
                                         string dimension,
                                         DateTime commentDate,
                                         RatingValue rating)
    {
      var ratingRequired = CommentRatingRequired(targetNode.NodeType, dimension);

      if (ratingRequired && rating == RatingValue.Undefined)
        return false;

      if (ratingRequired && !CanBeRatingActor(authorNode.NodeType, dimension))
        return false;

      return DoCanCreateComment(authorNode, targetNode, dimension, commentDate, rating);
    }

    /// <summary>
    /// Returns true if authorNode can create a response to the specified comment as of commentDate and
    /// if rating value can be applied
    /// </summary>
    public virtual bool CanCreateCommentResponse(Comment parent, GraphNode authorNode, DateTime responseDate)
    {
      if (parent.ResponseCount > MaxResponseForComment) return false;

      return DoCanCreateCommentResponse(parent, authorNode, responseDate);
    }

    /// <summary>
    /// A hook invoked upon physical deletion of comment
    /// </summary>
    public abstract void DeleteComment(GraphNode authorNode, CommentID comment);

    #endregion

    protected abstract void DoConfigure(IConfigSectionNode node);

    protected abstract bool DoDeliverEvent(GraphNode nodeRecipient, GraphNode nodeSender, Event evt, string subscriptionParameters);

    protected abstract bool DoCanCreateComment(GraphNode authorNode,
                                               GraphNode targetNode,
                                               string dimension,
                                               DateTime commentDate,
                                               RatingValue rating);

    protected abstract bool DoCanCreateCommentResponse(Comment parent, GraphNode authorNode, DateTime responseDate);
  }
}
