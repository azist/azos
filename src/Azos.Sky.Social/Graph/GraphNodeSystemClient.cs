
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Azos.Glue;
using Azos.Glue.Protocol;


namespace Azos.Sky.Social.Graph
{
// This implementation needs @Azos.Sky.@Social.@Graph.@IGraphNodeSystemClient, so
// it can be used with ServiceClientHub class

  ///<summary>
  /// Client for glued contract Azos.Sky.Social.Graph.IGraphNodeSystem server.
  /// Each contract method has synchronous and asynchronous versions, the later denoted by 'Async_' prefix.
  /// May inject client-level inspectors here like so:
  ///   client.MsgInspectors.Register( new YOUR_CLIENT_INSPECTOR_TYPE());
  ///</summary>
  public class GraphNodeSystemClient : ClientEndPoint, @Azos.Sky.@Social.@Graph.@IGraphNodeSystemClient
  {

  #region Static Members

     private static TypeSpec s_ts_CONTRACT;
     private static MethodSpec @s_ms_SaveNode_0;
     private static MethodSpec @s_ms_GetNode_1;
     private static MethodSpec @s_ms_DeleteNode_2;
     private static MethodSpec @s_ms_UndeleteNode_3;
     private static MethodSpec @s_ms_RemoveNode_4;

     //static .ctor
     static GraphNodeSystemClient()
     {
         var t = typeof(@Azos.Sky.@Social.@Graph.@IGraphNodeSystem);
         s_ts_CONTRACT = new TypeSpec(t);
         @s_ms_SaveNode_0 = new MethodSpec(t.GetMethod("SaveNode", new Type[]{ typeof(@Azos.Sky.@Social.@Graph.@GraphNode) }));
         @s_ms_GetNode_1 = new MethodSpec(t.GetMethod("GetNode", new Type[]{ typeof(@Azos.@Data.@GDID) }));
         @s_ms_DeleteNode_2 = new MethodSpec(t.GetMethod("DeleteNode", new Type[]{ typeof(@Azos.@Data.@GDID) }));
         @s_ms_UndeleteNode_3 = new MethodSpec(t.GetMethod("UndeleteNode", new Type[]{ typeof(@Azos.@Data.@GDID) }));
         @s_ms_RemoveNode_4 = new MethodSpec(t.GetMethod("RemoveNode", new Type[]{ typeof(@Azos.@Data.@GDID) }));
     }
  #endregion

  #region .ctor
     public GraphNodeSystemClient(string node, Binding binding = null) : base(node, binding) { ctor(); }
     public GraphNodeSystemClient(Node node, Binding binding = null) : base(node, binding) { ctor(); }
     public GraphNodeSystemClient(IGlue glue, string node, Binding binding = null) : base(glue, node, binding) { ctor(); }
     public GraphNodeSystemClient(IGlue glue, Node node, Binding binding = null) : base(glue, node, binding) { ctor(); }

     //common instance .ctor body
     private void ctor()
     {

     }

  #endregion

     public override Type Contract
     {
       get { return typeof(@Azos.Sky.@Social.@Graph.@IGraphNodeSystem); }
     }



  #region Contract Methods

         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphNodeSystem.SaveNode'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@Azos.Sky.@Social.@Graph.@GraphChangeStatus' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @Azos.Sky.@Social.@Graph.@GraphChangeStatus @SaveNode(@Azos.Sky.@Social.@Graph.@GraphNode  @node)
         {
            var call = Async_SaveNode(@node);
            return call.GetValue<@Azos.Sky.@Social.@Graph.@GraphChangeStatus>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphNodeSystem.SaveNode'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_SaveNode(@Azos.Sky.@Social.@Graph.@GraphNode  @node)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_SaveNode_0, false, RemoteInstance, new object[]{@node});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphNodeSystem.GetNode'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@Azos.Sky.@Social.@Graph.@GraphNode' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @Azos.Sky.@Social.@Graph.@GraphNode @GetNode(@Azos.@Data.@GDID  @gNode)
         {
            var call = Async_GetNode(@gNode);
            return call.GetValue<@Azos.Sky.@Social.@Graph.@GraphNode>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphNodeSystem.GetNode'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_GetNode(@Azos.@Data.@GDID  @gNode)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_GetNode_1, false, RemoteInstance, new object[]{@gNode});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphNodeSystem.DeleteNode'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@Azos.Sky.@Social.@Graph.@GraphChangeStatus' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @Azos.Sky.@Social.@Graph.@GraphChangeStatus @DeleteNode(@Azos.@Data.@GDID @gNode)
         {
            var call = Async_DeleteNode(@gNode);
            return call.GetValue<@Azos.Sky.@Social.@Graph.@GraphChangeStatus>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphNodeSystem.DeleteNode'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_DeleteNode(@Azos.@Data.@GDID @gNode)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_DeleteNode_2, false, RemoteInstance, new object[]{@gNode});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphNodeSystem.UndeleteNode'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@Azos.Sky.@Social.@Graph.@GraphChangeStatus' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @Azos.Sky.@Social.@Graph.@GraphChangeStatus @UndeleteNode(@Azos.@Data.@GDID @gNode)
         {
            var call = Async_UndeleteNode(@gNode);
            return call.GetValue<@Azos.Sky.@Social.@Graph.@GraphChangeStatus>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphNodeSystem.UndeleteNode'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_UndeleteNode(@Azos.@Data.@GDID  @gNode)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_UndeleteNode_3, false, RemoteInstance, new object[]{@gNode});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphNodeSystem.RemoveNode'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@Azos.Sky.@Social.@Graph.@GraphChangeStatus' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @Azos.Sky.@Social.@Graph.@GraphChangeStatus @RemoveNode(@Azos.@Data.@GDID  @gNode)
         {
            var call = Async_RemoveNode(@gNode);
            return call.GetValue<@Azos.Sky.@Social.@Graph.@GraphChangeStatus>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphNodeSystem.RemoveNode'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_RemoveNode(@Azos.@Data.@GDID  @gNode)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_RemoveNode_4, false, RemoteInstance, new object[]{@gNode});
            return DispatchCall(request);
         }


  #endregion

  }//class
}//namespace
