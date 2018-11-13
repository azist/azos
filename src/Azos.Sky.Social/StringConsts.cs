
namespace Azos.Sky.Social
{
  internal static class StringConsts
  {

    public const string ARGUMENT_ERROR = "Argument error: ";

    public const string TS_SERVICE_NO_VOLUMES_ERROR = "{0} service start error - no volumes configured";
    public const string TS_SERVICE_NO_TRENDING_HOST_ERROR = "{0} service start error - no trending host configured";
    public const string TS_SERVICE_DUPLICATE_VOLUMES_ERROR = "{0} service config error - duplicate volume name: '{1}'";
    public const string TS_INSTANCE_NOT_ALLOCATED_ERROR = "{0} is not allocated";
    public const string TS_INSTANCE_ALREADY_ALLOCATED_ERROR = "{0} is already allocated";
    public const string TS_INSTANCE_DATA_LAYER_IS_NOT_ALLOCATED_ERROR = "{0} Instance data Layer is not allocated";
    public const string TS_HOST_SET_NOT_CONFIG_ERROR = "Error config {0}";
    public const string TS_VOLUME_UNKNOWN_DETALIZATION_ERROR = "Unknown volume detalization level: '{0}'";

    public const string GS_INSTANCE_NOT_ALLOCATED_ERROR = "{0} is not allocated";
    public const string GS_INSTANCE_ALREADY_ALLOCATED_ERROR = "{0} is already allocated";
    public const string GS_SERVICE_NO_GRAPH_HOST_ERROR = "{0} service start error - no graph host configured";
    public const string GS_SAVE_NODE_ERROR = "Error save graph node {0}";
    public const string GS_GET_NODE_ERROR = "Error get graph node {0}";
    public const string GS_DELETE_NODE_ERROR = "Error delete graph node {0}";
    public const string GS_EMIT_EVENT_ERROR = "Error emit event {0}";
    public const string GS_SUBSCRIBE_ERROR = "Error subscribe {0} to the {1}";
    public const string GS_UNSUBSCRIBE_ERROR = "{0} unsubscription error on {1}";
    public const string GS_ESTIMATE_SUBSCRIPTION_COUNT_ERROR = "Estimate subscription count error {0}";
    public const string GS_GET_FRIEND_LISTS_ERROR = "Error get friend lists {0}";
    public const string GS_ADD_FRIEND_LIST_ERROR = "Error add friend list {0}";
    public const string GS_DELETE_FRIEND_LIST_ERROR = "Error delete friend list {0}";
    public const string GS_GET_FRIEND_CONNECTIONS_ERROR = "Error get friend connections {0}";
    public const string GS_ADD_FRIEND_ERROR = "Error add friend {1} to {0}";
    public const string GS_ASSIGN_FRIEND_LISTS_ERROR = "Error assign friend lists from {1} to {0}";
    public const string GS_DELETE_FRIEND_ERROR = "Error delete friend {1} from {1}";
    public const string GS_MAX_FRIENDS_IN_NODE = "The node {0} exceeds the maximum  allowed number of friends";
    public const string GS_INSTANCE_DATA_LAYER_IS_NOT_ALLOCATED_ERROR = "{0} Instance data Layer is not allocated";
    public const string GS_FRIEND_DRIECTION_ERROR = "Friendship not supported from {0} to {1}";
    public const string GS_CAN_NOT_BE_SUSBCRIBED_ERROR = "{0} can not be subscribed {1}";
    public const string GS_GET_SUBSCRIBER_ERROR = "Error get subscriber for node {0}";
    public const string GS_CAN_NOT_CREATE_COMMENT_ERROR = "Author '{0}' can not create comment for target '{1}' with rating '{2}'";
    public const string GS_CAN_NOT_CREATE_RESPONSE_ERROR = "Author '{0}' can not create response for target '{1}'";

    public const string GS_INIT_NOT_CONF_ERRROR = "{0} init error: {1} not configured";

    public const string GS_GET_RATING_ERROR = "Error read rating for node {0}";
    public const string GS_DELETE_RATING_ERROR = "Error delete rating {0}";
    public const string GS_CREATE_RATING_ERROR = "Error create rating in comment {0}.";
    public const string GS_UPDATE_RATING_ERROR = "Error update rating {0}.";
    public const string GS_GET_RATING_DETAILS_ERROR = "Error get details rating for node {0}, dimensional {1}";
    public const string GS_SET_LIKE_ERROR = "Error change like and dislike for {0} rating message";
    public const string GS_CREATE_RATING_ACCESS_DENIED_ERROR = "Access denied create rating [{1}, {2}] for [{0}]";
    public const string GS_CREATE_UPDATE_RATING_ERROR = "Error create response for the message [{0}] from user [1]";
    public const string GS_RATING_NOT_FOUND = "Rating {0} not found";
    public const string GS_CAN_NOT_BE_RATE_ERROR = " {0} can not be rate {1}";
    public const string GS_MAX_MESSAGE_IN_RATING = "The rating {0} exceeds the maximum  allowed number of message";
    public const string GS_FETCH_RATING_ERROR = "Error fetching rating for node {0}";
    public const string GS_CREATE_COMMENT_ERROR = "Error add comment for node '{0}' by author {1}";
    public const string GS_RESPONSE_BAD_PARENT_ID_ERROR = "Bad Parent CommentID (CommentID.IsZero == true)";
    public const string GS_RESPONSE_VOLUME_MISSING_ERROR = "Missing volume for create response comment";
    public const string GS_RESPONSE_COMMENT_ERROR = "Error create response for comment '{0}'";
    public const string GS_COMMENT_NOT_FOUND = "Comment '{0}' not found";
    public const string GS_PARENT_ID_NOT_ROOT = "Parent comment '{0}' is not root";
    public const string GS_NODE_RATING_NOT_FOUND = "Summary rating not found for node '{0}'";
    public const string GS_DELETE_COMMENT_ERROR = "Error delete comment '{0}'";
    public const string GS_FETCH_RESPONSE_ERROR = "Error fetching response for comment '{0}'";
    public const string GS_FETCH_COMPLAINTS_ERROR = "Error fetching complaints for comment '{0}'";
    public const string GS_GET_COMMENT_ERROR = "Comment not found '{0}'";
    public const string GS_COMPLAINT_ERROR = "Error while creating a complaint about comment '{0}'";
    public const string GS_JUSTIFY_COMMENT_ERROR = "Error while justifying comment '{0}'";

    public const string GS_HOST_SET_NOT_FOUND = "Graph system hostset '{0}' not found";
  }
}
