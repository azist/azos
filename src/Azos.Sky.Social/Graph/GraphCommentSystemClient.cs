
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Azos.Glue;
using Azos.Glue.Protocol;


namespace Azos.Sky.Social.Graph
{
// This implementation needs @Azos.Sky.@Social.@Graph.@IGraphCommentSystemClient, so
// it can be used with ServiceClientHub class

  ///<summary>
  /// Client for glued contract Azos.Sky.Social.Graph.IGraphCommentSystem server.
  /// Each contract method has synchronous and asynchronous versions, the later denoted by 'Async_' prefix.
  /// May inject client-level inspectors here like so:
  ///   client.MsgInspectors.Register( new YOUR_CLIENT_INSPECTOR_TYPE());
  ///</summary>
  public class GraphCommentSystemClient : ClientEndPoint, @Azos.Sky.@Social.@Graph.@IGraphCommentSystemClient
  {

  #region Static Members

     private static TypeSpec s_ts_CONTRACT;
     private static MethodSpec @s_ms_Create_0;
     private static MethodSpec s_ms_Respond_1;
     private static MethodSpec @s_ms_Update_2;
     private static MethodSpec @s_ms_DeleteComment_3;
     private static MethodSpec @s_ms_Like_4;
     private static MethodSpec @s_ms_IsCommentedByAuthor_5;
     private static MethodSpec @s_ms_GetNodeSummaries_6;
     private static MethodSpec @s_ms_Fetch_7;
     private static MethodSpec @s_ms_FetchResponses_8;
     private static MethodSpec @s_ms_FetchComplaints_9;
     private static MethodSpec @s_ms_GetComment_10;
     private static MethodSpec @s_ms_Complain_11;
     private static MethodSpec @s_ms_Justify_12;

     //static .ctor
     static GraphCommentSystemClient()
     {
         var t = typeof(@Azos.Sky.@Social.@Graph.@IGraphCommentSystem);
         s_ts_CONTRACT = new TypeSpec(t);
         @s_ms_Create_0 = new MethodSpec(t.GetMethod("Create", new Type[]{ typeof(@Azos.@Data.@GDID), typeof(@Azos.@Data.@GDID), typeof(@System.@String), typeof(@System.@String), typeof(@System.@Byte[]), typeof(@Azos.Sky.@Social.@Graph.@PublicationState), typeof(@Azos.Sky.@Social.@Graph.@RatingValue), typeof(@System.@Nullable<@System.@DateTime>) }));
         @s_ms_Respond_1 = new MethodSpec(t.GetMethod("Respond", new Type[]{ typeof(@Azos.@Data.@GDID), typeof(@Azos.Sky.@Social.@Graph.@CommentID), typeof(@System.@String), typeof(@System.@Byte[]) }));
         @s_ms_Update_2 = new MethodSpec(t.GetMethod("Update", new Type[]{ typeof(@Azos.Sky.@Social.@Graph.@CommentID), typeof(@Azos.Sky.@Social.@Graph.@RatingValue), typeof(@System.@String), typeof(@System.@Byte[]) }));
         @s_ms_DeleteComment_3 = new MethodSpec(t.GetMethod("DeleteComment", new Type[]{ typeof(@Azos.Sky.@Social.@Graph.@CommentID) }));
         @s_ms_Like_4 = new MethodSpec(t.GetMethod("Like", new Type[]{ typeof(@Azos.Sky.@Social.@Graph.@CommentID), typeof(@System.@Int32), typeof(@System.@Int32) }));
         @s_ms_IsCommentedByAuthor_5 = new MethodSpec(t.GetMethod("IsCommentedByAuthor", new Type[]{ typeof(@Azos.@Data.@GDID), typeof(@Azos.@Data.@GDID), typeof(@System.@String) }));
         @s_ms_GetNodeSummaries_6 = new MethodSpec(t.GetMethod("GetNodeSummaries", new Type[]{ typeof(@Azos.@Data.@GDID) }));
         @s_ms_Fetch_7 = new MethodSpec(t.GetMethod("Fetch", new Type[]{ typeof(@Azos.Sky.@Social.@Graph.@CommentQuery) }));
         @s_ms_FetchResponses_8 = new MethodSpec(t.GetMethod("FetchResponses", new Type[]{ typeof(@Azos.Sky.@Social.@Graph.@CommentID) }));
         @s_ms_FetchComplaints_9 = new MethodSpec(t.GetMethod("FetchComplaints", new Type[]{ typeof(@Azos.Sky.@Social.@Graph.@CommentID) }));
         @s_ms_GetComment_10 = new MethodSpec(t.GetMethod("GetComment", new Type[]{ typeof(@Azos.Sky.@Social.@Graph.@CommentID) }));
         @s_ms_Complain_11 = new MethodSpec(t.GetMethod("Complain", new Type[]{ typeof(@Azos.Sky.@Social.@Graph.@CommentID), typeof(@Azos.@Data.@GDID), typeof(@System.@String), typeof(@System.@String) }));
         @s_ms_Justify_12 = new MethodSpec(t.GetMethod("Justify", new Type[]{ typeof(@Azos.Sky.@Social.@Graph.@CommentID) }));
     }
  #endregion

  #region .ctor
     public GraphCommentSystemClient(string node, Binding binding = null) : base(node, binding) { ctor(); }
     public GraphCommentSystemClient(Node node, Binding binding = null) : base(node, binding) { ctor(); }
     public GraphCommentSystemClient(IGlue glue, string node, Binding binding = null) : base(glue, node, binding) { ctor(); }
     public GraphCommentSystemClient(IGlue glue, Node node, Binding binding = null) : base(glue, node, binding) { ctor(); }

     //common instance .ctor body
     private void ctor()
     {

     }

  #endregion

     public override Type Contract
     {
       get { return typeof(@Azos.Sky.@Social.@Graph.@IGraphCommentSystem); }
     }



  #region Contract Methods

         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.Create'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@Azos.Sky.@Social.@Graph.@Comment' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @Azos.Sky.@Social.@Graph.@Comment @Create(@Azos.@Data.@GDID  @gAuthorNode, @Azos.@Data.@GDID  @gTargetNode, @System.@String  @dimension, @System.@String  @content, @System.@Byte[]  @data, @Azos.Sky.@Social.@Graph.@PublicationState  @publicationState, @Azos.Sky.@Social.@Graph.@RatingValue  @value, @System.@Nullable<@System.@DateTime>  @timeStamp)
         {
            var call = Async_Create(@gAuthorNode, @gTargetNode, @dimension, @content, @data, @publicationState, @value, @timeStamp);
            return call.GetValue<@Azos.Sky.@Social.@Graph.@Comment>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.Create'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_Create(@Azos.@Data.@GDID  @gAuthorNode, @Azos.@Data.@GDID  @gTargetNode, @System.@String  @dimension, @System.@String  @content, @System.@Byte[]  @data, @Azos.Sky.@Social.@Graph.@PublicationState  @publicationState, @Azos.Sky.@Social.@Graph.@RatingValue  @value, @System.@Nullable<@System.@DateTime>  @timeStamp)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_Create_0, false, RemoteInstance, new object[]{@gAuthorNode, @gTargetNode, @dimension, @content, @data, @publicationState, @value, @timeStamp});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.Response'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@Azos.Sky.@Social.@Graph.@Comment' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @Azos.Sky.@Social.@Graph.@Comment Respond(@Azos.@Data.@GDID  @gAuthorNode, @Azos.Sky.@Social.@Graph.@CommentID  @parent, @System.@String  @content, @System.@Byte[]  @data)
         {
            var call = Async_Respond(@gAuthorNode, @parent, @content, @data);
            return call.GetValue<@Azos.Sky.@Social.@Graph.@Comment>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.Response'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_Respond(@Azos.@Data.@GDID  @gAuthorNode, @Azos.Sky.@Social.@Graph.@CommentID  @parent, @System.@String  @content, @System.@Byte[]  @data)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, s_ms_Respond_1, false, RemoteInstance, new object[]{@gAuthorNode, @parent, @content, @data});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.Update'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@Azos.Sky.@Social.@Graph.@GraphChangeStatus' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @Azos.Sky.@Social.@Graph.@GraphChangeStatus @Update(@Azos.Sky.@Social.@Graph.@CommentID  @ratingId, @Azos.Sky.@Social.@Graph.@RatingValue  @value, @System.@String  @content, @System.@Byte[]  @data)
         {
            var call = Async_Update(@ratingId, @value, @content, @data);
            return call.GetValue<@Azos.Sky.@Social.@Graph.@GraphChangeStatus>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.Update'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_Update(@Azos.Sky.@Social.@Graph.@CommentID  @ratingId, @Azos.Sky.@Social.@Graph.@RatingValue  @value, @System.@String  @content, @System.@Byte[]  @data)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_Update_2, false, RemoteInstance, new object[]{@ratingId, @value, @content, @data});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.DeleteComment'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@Azos.Sky.@Social.@Graph.@GraphChangeStatus' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @Azos.Sky.@Social.@Graph.@GraphChangeStatus @DeleteComment(@Azos.Sky.@Social.@Graph.@CommentID  @commentId)
         {
            var call = Async_DeleteComment(@commentId);
            return call.GetValue<@Azos.Sky.@Social.@Graph.@GraphChangeStatus>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.DeleteComment'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_DeleteComment(@Azos.Sky.@Social.@Graph.@CommentID  @commentId)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_DeleteComment_3, false, RemoteInstance, new object[]{@commentId});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.Like'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@Azos.Sky.@Social.@Graph.@GraphChangeStatus' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @Azos.Sky.@Social.@Graph.@GraphChangeStatus @Like(@Azos.Sky.@Social.@Graph.@CommentID  @commentId, @System.@Int32  @deltaLike, @System.@Int32  @deltaDislike)
         {
            var call = Async_Like(@commentId, @deltaLike, @deltaDislike);
            return call.GetValue<@Azos.Sky.@Social.@Graph.@GraphChangeStatus>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.Like'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_Like(@Azos.Sky.@Social.@Graph.@CommentID  @commentId, @System.@Int32  @deltaLike, @System.@Int32  @deltaDislike)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_Like_4, false, RemoteInstance, new object[]{@commentId, @deltaLike, @deltaDislike});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.IsCommentedByAuthor'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@Boolean' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@Boolean @IsCommentedByAuthor(@Azos.@Data.@GDID  @gNode, @Azos.@Data.@GDID  @gAuthor, @System.@String  @dimension)
         {
            var call = Async_IsCommentedByAuthor(@gNode, @gAuthor, @dimension);
            return call.GetValue<@System.@Boolean>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.IsCommentedByAuthor'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_IsCommentedByAuthor(@Azos.@Data.@GDID  @gNode, @Azos.@Data.@GDID  @gAuthor, @System.@String  @dimension)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_IsCommentedByAuthor_5, false, RemoteInstance, new object[]{@gNode, @gAuthor, @dimension});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.GetNodeSummaries'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Graph.@SummaryRating>' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Graph.@SummaryRating> @GetNodeSummaries(@Azos.@Data.@GDID  @gNode)
         {
            var call = Async_GetNodeSummaries(@gNode);
            return call.GetValue<@System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Graph.@SummaryRating>>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.GetNodeSummaries'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_GetNodeSummaries(@Azos.@Data.@GDID  @gNode)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_GetNodeSummaries_6, false, RemoteInstance, new object[]{@gNode});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.Fetch'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Graph.@Comment>' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Graph.@Comment> @Fetch(@Azos.Sky.@Social.@Graph.@CommentQuery  @query)
         {
            var call = Async_Fetch(@query);
            return call.GetValue<@System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Graph.@Comment>>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.Fetch'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_Fetch(@Azos.Sky.@Social.@Graph.@CommentQuery  @query)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_Fetch_7, false, RemoteInstance, new object[]{@query});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.FetchResponses'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Graph.@Comment>' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Graph.@Comment> @FetchResponses(@Azos.Sky.@Social.@Graph.@CommentID  @commentId)
         {
            var call = Async_FetchResponses(@commentId);
            return call.GetValue<@System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Graph.@Comment>>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.FetchResponses'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_FetchResponses(@Azos.Sky.@Social.@Graph.@CommentID  @commentId)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_FetchResponses_8, false, RemoteInstance, new object[]{@commentId});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.FetchComplaints'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Graph.@Complaint>' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Graph.@Complaint> @FetchComplaints(@Azos.Sky.@Social.@Graph.@CommentID  @commentId)
         {
            var call = Async_FetchComplaints(@commentId);
            return call.GetValue<@System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Graph.@Complaint>>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.FetchComplaints'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_FetchComplaints(@Azos.Sky.@Social.@Graph.@CommentID  @commentId)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_FetchComplaints_9, false, RemoteInstance, new object[]{@commentId});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.GetComment'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@Azos.Sky.@Social.@Graph.@Comment' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @Azos.Sky.@Social.@Graph.@Comment @GetComment(@Azos.Sky.@Social.@Graph.@CommentID  @commentId)
         {
            var call = Async_GetComment(@commentId);
            return call.GetValue<@Azos.Sky.@Social.@Graph.@Comment>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.GetComment'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_GetComment(@Azos.Sky.@Social.@Graph.@CommentID  @commentId)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_GetComment_10, false, RemoteInstance, new object[]{@commentId});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.Complain'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@Azos.Sky.@Social.@Graph.@GraphChangeStatus' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @Azos.Sky.@Social.@Graph.@GraphChangeStatus @Complain(@Azos.Sky.@Social.@Graph.@CommentID  @commentId, @Azos.@Data.@GDID  @gAuthorNode, @System.@String  @kind, @System.@String  @message)
         {
            var call = Async_Complain(@commentId, @gAuthorNode, @kind, @message);
            return call.GetValue<@Azos.Sky.@Social.@Graph.@GraphChangeStatus>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.Complain'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_Complain(@Azos.Sky.@Social.@Graph.@CommentID  @commentId, @Azos.@Data.@GDID  @gAuthorNode, @System.@String  @kind, @System.@String  @message)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_Complain_11, false, RemoteInstance, new object[]{@commentId, @gAuthorNode, @kind, @message});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.Justify'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@Azos.Sky.@Social.@Graph.@GraphChangeStatus' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @Azos.Sky.@Social.@Graph.@GraphChangeStatus @Justify(@Azos.Sky.@Social.@Graph.@CommentID  @commentID)
         {
            var call = Async_Justify(@commentID);
            return call.GetValue<@Azos.Sky.@Social.@Graph.@GraphChangeStatus>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphCommentSystem.Justify'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_Justify(@Azos.Sky.@Social.@Graph.@CommentID  @commentID)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_Justify_12, false, RemoteInstance, new object[]{@commentID});
            return DispatchCall(request);
         }


  #endregion

  }//class
}//namespace
