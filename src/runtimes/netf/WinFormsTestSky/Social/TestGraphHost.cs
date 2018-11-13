using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Conf;
using Azos.Data;
using Azos.Sky.Social.Graph;
using Azos.Sky.Social.Graph.Server.Data;
using Azos.Sky.Social.Graph.Server;

namespace WinFormsTestSky.Social
{
  class TestGraphHost : GraphHost
  {
    public override int MaxResponseForComment { get { return 1000; } }

    protected TestGraphHost(GraphOperationContext director, IConfigSectionNode config) : base(director, config)
    {
      ConfigAttribute.Apply(this, config);
    }

    public override bool CanBeFriends(string fromNodeType, string toNodeType)
    {
      return true;
      //throw new NotImplementedException();
    }

    public override bool CommentRatingRequired(string ratedNodeType, string dimension)
    {
      return true;
      //throw new NotImplementedException();
    }

    public override bool CanBeRatingActor(string ratingNodeType, string dimension)
    {
      return true;
      //throw new NotImplementedException();
    }

    public override bool CanBeSubscribed(string subscriberNodeType, string emitterNodeType)
    {
      return true;
      //throw new NotImplementedException();
    }

    public override void DeleteComment(GraphNode gAuthorNode, CommentID gComment)
    {
      //throw new NotImplementedException();
    }

    public override IEnumerable<SubscriberRow> DeliverEventsChunk(IEnumerable<SubscriberRow> filtered, Event evt, IConfigSectionNode cfg)
    {
      return filtered;
      //throw new NotImplementedException();
    }

    public override TimeSpan EditCommentSpan(GraphNode gTargetNode, string dimension)
    {
      return new TimeSpan(0, 0, 0);
      //throw new NotImplementedException();
    }

    public override IEnumerable<GraphNode> FilterByOriginQuery(IEnumerable<GraphNode> data, string orgQry)
    {
      return data;
      //throw new NotImplementedException();
    }

    public override IEnumerable<SubscriberRow> FilterEventsChunk(IEnumerable<SubscriberRow> subscribersChunk, Event evt, IConfigSectionNode cfg)
    {
      return subscribersChunk;
      //throw new NotImplementedException();
    }

    public override TypedDoc NodeBinaryDataToObject(string nodeType, byte[] data)
    {
      return null;
      //throw new NotImplementedException();
    }

    public override byte[] ObjectToNodeBinaryData(string nodeType, TypedDoc data)
    {
      return null;
      //throw new NotImplementedException();
    }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      //throw new NotImplementedException();
    }

    protected override bool DoDeliverEvent(GraphNode nodeRecipient, GraphNode nodeSender, Event evt, string subscriptionParameters)
    {
      return true;
    }

    protected override bool DoCanCreateComment(GraphNode authorNode, GraphNode targetNode, string dimension, DateTime commentDate, RatingValue rating)
    {
      throw new NotImplementedException();
    }

    protected override bool DoCanCreateCommentResponse(Comment parent, GraphNode authorNode, DateTime responseDate)
    {
      throw new NotImplementedException();
    }
  }
}
