using System;

namespace Azos.Sky.Social
{
  public static class SocialConsts
  {
    public const string TQ_EVT_SUB_MANAGE = "esub-manage";
    public const string TQ_EVT_SUB_DELIVER = "esub-deliver";
    public const string TQ_EVT_SUB_REMOVE = "esub-remove";

    public const string CONFIG_GRAPH_HOST_SECTION = "graph-host";
    public const string CONFIG_DATA_STORE_SECTION = "data-store";
    public const string CONFIG_GRAPH_COMMENT_FETCH_STRATEGY_SECTION = "graph-comment-fetch-strategy";
    public const string CONFIG_GRAPH_NODE_HOST_SET_ATTR = "node-hostset";
    public const string CONFIG_GRAPH_COMMENT_HOST_SET_ATTR = "comment-hostset";
    public const string CONFIG_GRAPH_FRIEND_HOST_SET_ATTR = "friend-hostset";
    public const string CONFIG_GRAPH_EVENT_HOST_SET_ATTR = "event-hostset";

    public const string CONFIG_TRENDING_HOST_SET_ATTR = "trending-hostset";

    //public const string GS_DS_TARGET = "MDB.GRAPH";
    //public const string GS_DS_MYSQL_TARGET = "MDB.GRAPH.MYSQL";

    public const string MDB_AREA_NODE = "node";
    public const string MDB_AREA_COMMENT = "comment";

    public const string GS_FRIEND_LIST_SEPARATOR = ",";

    public const string GS_NODE_TBL = "GraphSystemService.Node";
    public const string GS_COMMENT_BLOCK_TBL = "GraphSystemService.CommentBlock";

    public const int GS_MAX_FRIENDS_COUNT = 10000;
    public const int GS_MAX_SUBSCRIBERS_COUNT = 10000;

    public const string HOST_SET_SOCIAL_GRAPH = "socialgraphtodo";
    public const string SVC_SOCIAL_GRAPH_TODO = "socialgraphtodoqueue";
    public const string HOST_SET_SUBS_DELIVERY = "subsdeliverytodo";
    public const string SVC_SUBS_DELIBERY_TODO = SVC_SOCIAL_GRAPH_TODO; //"subsdeliverytodoqueue";
    public const string HOST_SET_SUBS_REMOVE = "removetodo";
    public const string SVC_SUBS_REMOVE_TODO = SVC_SOCIAL_GRAPH_TODO; //"removetodoqueue";

    public const int SUBSCRIPTION_DELIVERY_CHUNK_SIZE = 0xff;
    public const int SUBSCRIPTION_DELIVERY_MAX_CHUNK_SIZE = SUBSCRIPTION_DELIVERY_CHUNK_SIZE * 8;

    public static int GetVolumeMaxCountForPosition(int i)
    {
      if (i < 5) return SUBSCRIPTION_DELIVERY_CHUNK_SIZE - 1;
      if (i < 10) return SUBSCRIPTION_DELIVERY_CHUNK_SIZE * 2;
      if (i < 15) return SUBSCRIPTION_DELIVERY_CHUNK_SIZE * 4 ;
      return SUBSCRIPTION_DELIVERY_MAX_CHUNK_SIZE;
    }

    public const string GS_GENERAL_RATING_DIMENSION = "general";
  }
}
