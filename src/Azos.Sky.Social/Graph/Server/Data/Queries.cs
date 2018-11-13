using System;
using Azos.Sky.Social.Graph.Server.Data.Schema;

using Azos.Data;
using Azos.Data.Access;


namespace Azos.Sky.Social.Graph.Server.Data
{
  internal static class Queries
  {

    #region Nodes and Subscribers

    public static Query<TRow> FindOneNodeByGDID<TRow>(GDID gNode) where TRow : Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Node.FindOneNodeByGDID")
      {
        new Query.Param("pgnode", gNode)
      };
    }

    public static Query<TRow> ChangeInUseNodeByGDID<TRow>(GDID gNode, bool isDel) where TRow : Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Node.DeleteOneNodeByGDID")
      {
        new Query.Param("pgnode", gNode),
        new Query.Param("pInUse", isDel ? "F" : "T")
      };
    }

    public static Query<TRow> CountSubscribers<TRow>(GDID gNode) where TRow : Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Node.CountSubscribers")
      {
        new Query.Param("pNode",gNode)
      };
    }

    public static Query<TRow> FindSubscriber<TRow>(SubscriberVolumeRow volume, GDID gSubscriber) where TRow:Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Node.FindSubscriber")
      {
        new Query.Param("pVol",volume),
        new Query.Param("pSub",gSubscriber)
      };
    }

    public static Query<TRow> FindSubscriberVolumes<TRow>(GDID gNode) where TRow : Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Node.FindSubscriberVolumes")
      {
        new Query.Param("pNode",gNode)
      };
    }

    public static Query<TRow> CountSubscriberVolumes<TRow>(GDID gNode) where TRow : Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Node.CountSubscriberVolumes")
      {
        new Query.Param("pNode",gNode)
      };
    }

    public static Query<TRow> FindSubscribers<TRow>(GDID gVolume, long start, int count) where TRow : Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Node.FindSubscribers")
      {
        new Query.Param("pVol",gVolume),
        new Query.Param("pStart",start),
        new Query.Param("pCount",count)
      };
    }

    public static Query<TRow> GetNextVolume<TRow>(GDID gNode, int start) where TRow : Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Node.GetNextVolume")
      {
        new Query.Param("pNode", gNode),
        new Query.Param("pStart", start)
      };
    }

    public static Query RemoveNode(GDID gNode)
    {
      return new Query("Graph.Server.Data.Scripts.Node.RemoveNode")
      {
        new Query.Param("pNode", gNode)
      };
    }

    public static Query RemoveSubVol(GDID gNode)
    {
      return new Query("Graph.Server.Data.Scripts.Node.RemoveSubVol")
      {
        new Query.Param("pNode", gNode)
      };
    }

    public static Query RemoveSubscribers(GDID gVol)
    {
      return new Query("Graph.Server.Data.Scripts.Node.RemoveSubscribers")
      {
        new Query.Param("pVol", gVol)
      };
    }

    #endregion

    #region Friend

    public static Query<TRow> FindOneFriendByNodeAndFriend<TRow>(GDID gNode, GDID gFriendNode) where TRow : Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Friend.FindOneFriendByNodeAndFriend")
      {
        new Query.Param("pown", gNode),
        new Query.Param("pfnd", gFriendNode)
      };
    }

    public static Query<TRow> FindFriends<TRow>(FriendQuery query) where TRow : Doc
    {
      var status = "%";
      switch (query.Status)
      {
        case FriendStatusFilter.Approved:
          status = GSFriendStatus.APPROVED;
          break;
        case FriendStatusFilter.Banned:
          status = GSFriendStatus.BANNED;
          break;
        case FriendStatusFilter.PendingApproval:
          status = GSFriendStatus.PENDING;
          break;
      }

      return new Query<TRow>("Graph.Server.Data.Scripts.Friend.FindFriends")
      {
        new Query.Param("pNode", query.G_Node),
        new Query.Param("pList", query.Lists),
        new Query.Param("pStatus", status),
        new Query.Param("pFetchStart", query.FetchStart),
        new Query.Param("pFetchCount", query.FetchCount)
      };
    }

    public static Query<TRow> CountFriends<TRow>(GDID gNode) where TRow:Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Friend.CountFriends")
      {
        new Query.Param("pG_Node",gNode)
      };
    }

    public static Query<TRow> FindFriendListByNode<TRow>(GDID gNode) where TRow : Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Friend.FindFriendListByNode")
      {
        new Query.Param("pgnode", gNode)
      };
    }

    public static Query<TRow> FindAllFriends<TRow>(GDID gNode) where TRow : Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.FindAllFriends")
      {
        new Query.Param("pNode", gNode)
      };
    }

    public static Query DeleteFriendByNode(GDID gNode)
    {
      return new Query("Graph.Server.Data.Scripts.Friend.DeleteFriendByNode")
      {
        new Query.Param("gG_Node", gNode)
      };
    }

    public static Query DeleteFriendByNodeAndFriend(GDID gNode, GDID gFriendNode)
    {
      return new Query("Graph.Server.Data.Scripts.Friend.DeleteFriendByNodeAndFriend")
      {
        new Query.Param("pown", gNode),
        new Query.Param("pfnd", gFriendNode),
        new Query.Param("pdt", App.TimeSource.UTCNow)
      };
    }

    public static Query DeleteFriendListByListId(GDID gNode, string list)
    {
      return new Query("Graph.Server.Data.Scripts.Friend.DeleteFriendListByListId")
      {
        new Query.Param("pgnode", gNode),
        new Query.Param("plistid", list)
      };
    }

    public static Query RemoveFriendByNode(GDID gNode)
    {
      return new Query("Graph.Server.Data.Scripts.Friend.RemoveFriendByNode")
      {
        new Query.Param("pNode", gNode)
      };
    }

    public static Query RemoveFriendListByNode(GDID gNode)
    {
      return new Query("Graph.Server.Data.Scripts.Friend.RemoveFriendListByNode")
      {
        new Query.Param("pNode", gNode)
      };
    }

    public static Query<TRow> GetNextFriend<TRow>(GDID gNode, int start) where TRow : Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Friend.GetNextFriend")
      {
        new Query.Param("pNode", gNode),
        new Query.Param("pStart", start)
      };
    }

    #endregion

    #region Comments and ratings

    public static Query<TRow> FindNodeRating<TRow>(GDID gNode, string dimension) where TRow : Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Rating.FindNodeRating")
      {
        new Query.Param("pNode", gNode),
        new Query.Param("pDim", dimension)
      };
    }

    public static Query<TRow> FindNodeRatings<TRow>(GDID gNode, DateTime dt) where TRow :  Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Rating.FindNodeRatings")
      {
        new Query.Param("pNode", gNode),
        new Query.Param("pDT", dt)
      };
    }

    public static Query ClearNodeRating(GDID gNode, string dimension, DateTime utc)
    {
      return new Query("Graph.Server.Data.Scripts.Rating.ClearNodeRating")
      {
        new Query.Param("pNode", gNode),
        new Query.Param("pDim", dimension),
        new Query.Param("pCDT", utc)
      };
    }

    public static Query ClearNodeRatings(GDID gNode, DateTime utc)
    {
      return new Query("Graph.Server.Data.Scripts.Rating.ClearNodeRatings")
      {
        new Query.Param("pNode", gNode),
        new Query.Param("pCDT", utc)
      };
    }

    public static Query UpdateLike(GDID gVolume, GDID gComment, int deltaLike, int deltaDislike)
    {
      return new Query("Graph.Server.Data.Scripts.Rating.UpdateLike")
      {
        new Query.Param("pVolume", gVolume),
        new Query.Param("pComment", gComment),
        new Query.Param("pdtLike", deltaLike),
        new Query.Param("pdtDislike", deltaDislike)
      };
    }

    public static Query CancelComplaintsByComment(GDID gComment)
    {
      return new Query("Graph.Server.Data.Scripts.Rating.CancelComplaintsByComment")
      {
        new Query.Param("pG_Comment", gComment)
      };
    }

    public static Query<TRow> FindCommentVolume<TRow>(GDID gNode, GDID gVolume) where TRow : Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Rating.FindCommentVolume")
      {
        new Query.Param("pNode", gNode),
        new Query.Param("pVol", gVolume)
      };
    }

    public static Query<TRow> FindEmptyCommentVolume<TRow>(GDID gNode, string dimension, int maxCount) where TRow : Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Rating.FindEmptyCommentVolume")
      {
        new Query.Param("pNode", gNode),
        new Query.Param("pDim", dimension),
        new Query.Param("pMaxCount", maxCount)
      };
    }

    public static Query<TRow> FindCommentByGDID<TRow>(GDID gComment) where TRow : Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Rating.FindCommentByGDID")
      {
        new Query.Param("pGDID", gComment)
      };
    }

    public static Query<TRow> FindComments<TRow>(GDID gNode, string dimension, bool isRoot) where TRow : Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Rating.FindComments")
      {
        new Query.Param("pNode", gNode),
        new Query.Param("pDim", dimension),
        new Query.Param("pRoot", isRoot)
      };
    }

    public static Query<TRow> HasCommentsCreatedByAuthor<TRow>(GDID gNode, GDID gAuthor, string dimension) where TRow : Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Rating.HasCommentsCreatedByAuthor")
      {
        new Query.Param("pNode", gNode),
        new Query.Param("pAuthor", gAuthor),
        new Query.Param("pDim", dimension)
      };
    }

    public static Query DeleteResponses(CommentID commentId)
    {
      return new Query("Graph.Server.Data.Scripts.Rating.DeleteResponses")
      {
        new Query.Param("pParent", commentId.G_Comment),
        new Query.Param("pVolume", commentId.G_Volume)
      };
    }

    public static Query<TRow> CountResponses<TRow>(CommentID commentId) where TRow: Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Rating.CountResponses")
      {
        new Query.Param("pComment", commentId.G_Comment),
        new Query.Param("pVolume", commentId.G_Volume)
      };
    }

    public static Query<TRow> FindResponses<TRow>(CommentID commentId) where TRow:Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Rating.FindResponses")
      {
        new Query.Param("pComment", commentId.G_Comment),
        new Query.Param("pVolume", commentId.G_Volume)
      };
    }

    public static Query<TRow> FindComplaints<TRow>(GDID gComment) where TRow : Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Rating.FindComplaints")
      {
        new Query.Param("pComment", gComment)
      };
    }

    public static Query<TRow> FindCommentVolumes<TRow>(GDID gNode, string dimension,int count, DateTime cdt) where TRow: Doc
    {
      return new Query<TRow>("Graph.Server.Data.Scripts.Rating.FindCommentVolumes")
      {
        new Query.Param("pNode", gNode),
        new Query.Param("pDIM", dimension),
        new Query.Param("pCDT", cdt),
        new Query.Param("cnt", count)
      };
    }

    #endregion

  }
}