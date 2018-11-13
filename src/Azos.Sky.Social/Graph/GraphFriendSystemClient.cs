
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Azos.Glue;
using Azos.Glue.Protocol;


namespace Azos.Sky.Social.Graph
{
// This implementation needs @Azos.Sky.@Social.@Graph.@IGraphFriendSystemClient, so
// it can be used with ServiceClientHub class

  ///<summary>
  /// Client for glued contract Azos.Sky.Social.Graph.IGraphFriendSystem server.
  /// Each contract method has synchronous and asynchronous versions, the later denoted by 'Async_' prefix.
  /// May inject client-level inspectors here like so:
  ///   client.MsgInspectors.Register( new YOUR_CLIENT_INSPECTOR_TYPE());
  ///</summary>
  public class GraphFriendSystemClient : ClientEndPoint, @Azos.Sky.@Social.@Graph.@IGraphFriendSystemClient
  {

  #region Static Members

     private static TypeSpec s_ts_CONTRACT;
     private static MethodSpec @s_ms_GetFriendLists_0;
     private static MethodSpec @s_ms_AddFriendList_1;
     private static MethodSpec @s_ms_DeleteFriendList_2;
     private static MethodSpec @s_ms_GetFriendConnections_3;
     private static MethodSpec @s_ms_AddFriend_4;
     private static MethodSpec @s_ms_AssignFriendLists_5;
     private static MethodSpec @s_ms_DeleteFriend_6;

     //static .ctor
     static GraphFriendSystemClient()
     {
         var t = typeof(@Azos.Sky.@Social.@Graph.@IGraphFriendSystem);
         s_ts_CONTRACT = new TypeSpec(t);
         @s_ms_GetFriendLists_0 = new MethodSpec(t.GetMethod("GetFriendLists", new Type[]{ typeof(@Azos.@Data.@GDID) }));
         @s_ms_AddFriendList_1 = new MethodSpec(t.GetMethod("AddFriendList", new Type[]{ typeof(@Azos.@Data.@GDID), typeof(@System.@String), typeof(@System.@String) }));
         @s_ms_DeleteFriendList_2 = new MethodSpec(t.GetMethod("DeleteFriendList", new Type[]{ typeof(@Azos.@Data.@GDID), typeof(@System.@String) }));
         @s_ms_GetFriendConnections_3 = new MethodSpec(t.GetMethod("GetFriendConnections", new Type[]{ typeof(@Azos.Sky.@Social.@Graph.@FriendQuery) }));
         @s_ms_AddFriend_4 = new MethodSpec(t.GetMethod("AddFriend", new Type[]{ typeof(@Azos.@Data.@GDID), typeof(@Azos.@Data.@GDID), typeof(@System.@Nullable<@System.@Boolean>) }));
         @s_ms_AssignFriendLists_5 = new MethodSpec(t.GetMethod("AssignFriendLists", new Type[]{ typeof(@Azos.@Data.@GDID), typeof(@Azos.@Data.@GDID), typeof(@System.@String) }));
         @s_ms_DeleteFriend_6 = new MethodSpec(t.GetMethod("DeleteFriend", new Type[]{ typeof(@Azos.@Data.@GDID), typeof(@Azos.@Data.@GDID) }));
     }
  #endregion

  #region .ctor
     public GraphFriendSystemClient(string node, Binding binding = null) : base(node, binding) { ctor(); }
     public GraphFriendSystemClient(Node node, Binding binding = null) : base(node, binding) { ctor(); }
     public GraphFriendSystemClient(IGlue glue, string node, Binding binding = null) : base(glue, node, binding) { ctor(); }
     public GraphFriendSystemClient(IGlue glue, Node node, Binding binding = null) : base(glue, node, binding) { ctor(); }

     //common instance .ctor body
     private void ctor()
     {

     }

  #endregion

     public override Type Contract
     {
       get { return typeof(@Azos.Sky.@Social.@Graph.@IGraphFriendSystem); }
     }



  #region Contract Methods

         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphFriendSystem.GetFriendLists'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@Collections.@Generic.@IEnumerable<@System.@String>' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@Collections.@Generic.@IEnumerable<@System.@String> @GetFriendLists(@Azos.@Data.@GDID  @gNode)
         {
            var call = Async_GetFriendLists(@gNode);
            return call.GetValue<@System.@Collections.@Generic.@IEnumerable<@System.@String>>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphFriendSystem.GetFriendLists'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_GetFriendLists(@Azos.@Data.@GDID  @gNode)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_GetFriendLists_0, false, RemoteInstance, new object[]{@gNode});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphFriendSystem.AddFriendList'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@Azos.Sky.@Social.@Graph.@GraphChangeStatus' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @Azos.Sky.@Social.@Graph.@GraphChangeStatus @AddFriendList(@Azos.@Data.@GDID  @gNode, @System.@String  @list, @System.@String  @description)
         {
            var call = Async_AddFriendList(@gNode, @list, @description);
            return call.GetValue<@Azos.Sky.@Social.@Graph.@GraphChangeStatus>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphFriendSystem.AddFriendList'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_AddFriendList(@Azos.@Data.@GDID  @gNode, @System.@String  @list, @System.@String  @description)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_AddFriendList_1, false, RemoteInstance, new object[]{@gNode, @list, @description});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphFriendSystem.DeleteFriendList'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@Azos.Sky.@Social.@Graph.@GraphChangeStatus' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @Azos.Sky.@Social.@Graph.@GraphChangeStatus @DeleteFriendList(@Azos.@Data.@GDID  @gNode, @System.@String  @list)
         {
            var call = Async_DeleteFriendList(@gNode, @list);
            return call.GetValue<@Azos.Sky.@Social.@Graph.@GraphChangeStatus>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphFriendSystem.DeleteFriendList'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_DeleteFriendList(@Azos.@Data.@GDID  @gNode, @System.@String  @list)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_DeleteFriendList_2, false, RemoteInstance, new object[]{@gNode, @list});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphFriendSystem.GetFriendConnections'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Graph.@FriendConnection>' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Graph.@FriendConnection> @GetFriendConnections(@Azos.Sky.@Social.@Graph.@FriendQuery  @query)
         {
            var call = Async_GetFriendConnections(@query);
            return call.GetValue<@System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Graph.@FriendConnection>>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphFriendSystem.GetFriendConnections'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_GetFriendConnections(@Azos.Sky.@Social.@Graph.@FriendQuery  @query)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_GetFriendConnections_3, false, RemoteInstance, new object[]{@query});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphFriendSystem.AddFriend'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@Azos.Sky.@Social.@Graph.@GraphChangeStatus' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @Azos.Sky.@Social.@Graph.@GraphChangeStatus @AddFriend(@Azos.@Data.@GDID  @gNode, @Azos.@Data.@GDID  @gFriendNode, @System.@Nullable<@System.@Boolean>  @approve)
         {
            var call = Async_AddFriend(@gNode, @gFriendNode, @approve);
            return call.GetValue<@Azos.Sky.@Social.@Graph.@GraphChangeStatus>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphFriendSystem.AddFriend'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_AddFriend(@Azos.@Data.@GDID  @gNode, @Azos.@Data.@GDID  @gFriendNode, @System.@Nullable<@System.@Boolean>  @approve)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_AddFriend_4, false, RemoteInstance, new object[]{@gNode, @gFriendNode, @approve});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphFriendSystem.AssignFriendLists'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@Azos.Sky.@Social.@Graph.@GraphChangeStatus' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @Azos.Sky.@Social.@Graph.@GraphChangeStatus @AssignFriendLists(@Azos.@Data.@GDID  @gNode, @Azos.@Data.@GDID  @gFriendNode, @System.@String  @lists)
         {
            var call = Async_AssignFriendLists(@gNode, @gFriendNode, @lists);
            return call.GetValue<@Azos.Sky.@Social.@Graph.@GraphChangeStatus>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphFriendSystem.AssignFriendLists'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_AssignFriendLists(@Azos.@Data.@GDID  @gNode, @Azos.@Data.@GDID  @gFriendNode, @System.@String  @lists)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_AssignFriendLists_5, false, RemoteInstance, new object[]{@gNode, @gFriendNode, @lists});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphFriendSystem.DeleteFriend'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@Azos.Sky.@Social.@Graph.@GraphChangeStatus' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @Azos.Sky.@Social.@Graph.@GraphChangeStatus @DeleteFriend(@Azos.@Data.@GDID  @gNode, @Azos.@Data.@GDID  @gFriendNode)
         {
            var call = Async_DeleteFriend(@gNode, @gFriendNode);
            return call.GetValue<@Azos.Sky.@Social.@Graph.@GraphChangeStatus>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphFriendSystem.DeleteFriend'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_DeleteFriend(@Azos.@Data.@GDID  @gNode, @Azos.@Data.@GDID  @gFriendNode)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_DeleteFriend_6, false, RemoteInstance, new object[]{@gNode, @gFriendNode});
            return DispatchCall(request);
         }


  #endregion

  }//class
}//namespace
