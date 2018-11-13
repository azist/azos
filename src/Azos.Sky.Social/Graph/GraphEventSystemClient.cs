
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Azos.Glue;
using Azos.Glue.Protocol;


namespace Azos.Sky.Social.Graph
{
// This implementation needs @Azos.Sky.@Social.@Graph.@IGraphEventSystemClient, so
// it can be used with ServiceClientHub class

  ///<summary>
  /// Client for glued contract Azos.Sky.Social.Graph.IGraphEventSystem server.
  /// Each contract method has synchronous and asynchronous versions, the later denoted by 'Async_' prefix.
  /// May inject client-level inspectors here like so:
  ///   client.MsgInspectors.Register( new YOUR_CLIENT_INSPECTOR_TYPE());
  ///</summary>
  public class GraphEventSystemClient : ClientEndPoint, @Azos.Sky.@Social.@Graph.@IGraphEventSystemClient
  {

  #region Static Members

     private static TypeSpec s_ts_CONTRACT;
     private static MethodSpec @s_ms_EmitEvent_0;
     private static MethodSpec @s_ms_Subscribe_1;
     private static MethodSpec @s_ms_Unsubscribe_2;
     private static MethodSpec @s_ms_EstimateSubscriberCount_3;
     private static MethodSpec @s_ms_GetSubscribers_4;

     //static .ctor
     static GraphEventSystemClient()
     {
         var t = typeof(@Azos.Sky.@Social.@Graph.@IGraphEventSystem);
         s_ts_CONTRACT = new TypeSpec(t);
         @s_ms_EmitEvent_0 = new MethodSpec(t.GetMethod("EmitEvent", new Type[]{ typeof(@Azos.Sky.@Social.@Graph.@Event) }));
         @s_ms_Subscribe_1 = new MethodSpec(t.GetMethod("Subscribe", new Type[]{ typeof(@Azos.@Data.@GDID), typeof(@Azos.@Data.@GDID), typeof(@System.@Byte[]) }));
         @s_ms_Unsubscribe_2 = new MethodSpec(t.GetMethod("Unsubscribe", new Type[]{ typeof(@Azos.@Data.@GDID), typeof(@Azos.@Data.@GDID) }));
         @s_ms_EstimateSubscriberCount_3 = new MethodSpec(t.GetMethod("EstimateSubscriberCount", new Type[]{ typeof(@Azos.@Data.@GDID) }));
         @s_ms_GetSubscribers_4 = new MethodSpec(t.GetMethod("GetSubscribers", new Type[]{ typeof(@Azos.@Data.@GDID), typeof(@System.@Int64), typeof(@System.@Int32) }));
     }
  #endregion

  #region .ctor
     public GraphEventSystemClient(string node, Binding binding = null) : base(node, binding) { ctor(); }
     public GraphEventSystemClient(Node node, Binding binding = null) : base(node, binding) { ctor(); }
     public GraphEventSystemClient(IGlue glue, string node, Binding binding = null) : base(glue, node, binding) { ctor(); }
     public GraphEventSystemClient(IGlue glue, Node node, Binding binding = null) : base(glue, node, binding) { ctor(); }

     //common instance .ctor body
     private void ctor()
     {

     }

  #endregion

     public override Type Contract
     {
       get { return typeof(@Azos.Sky.@Social.@Graph.@IGraphEventSystem); }
     }



  #region Contract Methods

         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphEventSystem.EmitEvent'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public void @EmitEvent(@Azos.Sky.@Social.@Graph.@Event  @evt)
         {
            var call = Async_EmitEvent(@evt);
            call.CheckVoidValue();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphEventSystem.EmitEvent'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_EmitEvent(@Azos.Sky.@Social.@Graph.@Event  @evt)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_EmitEvent_0, false, RemoteInstance, new object[]{@evt});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphEventSystem.Subscribe'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public void @Subscribe(@Azos.@Data.@GDID  @gRecipientNode, @Azos.@Data.@GDID  @gEmitterNode, @System.@Byte[]  @parameters)
         {
            var call = Async_Subscribe(@gRecipientNode, @gEmitterNode, @parameters);
            call.CheckVoidValue();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphEventSystem.Subscribe'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_Subscribe(@Azos.@Data.@GDID  @gRecipientNode, @Azos.@Data.@GDID  @gEmitterNode, @System.@Byte[] @parameters)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_Subscribe_1, false, RemoteInstance, new object[]{@gRecipientNode, @gEmitterNode, @parameters});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphEventSystem.Unsubscribe'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public void @Unsubscribe(@Azos.@Data.@GDID  @gRecipientNode, @Azos.@Data.@GDID  @gEmitterNode)
         {
            var call = Async_Unsubscribe(@gRecipientNode, @gEmitterNode);
            call.CheckVoidValue();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphEventSystem.Unsubscribe'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_Unsubscribe(@Azos.@Data.@GDID  @gRecipientNode, @Azos.@Data.@GDID  @gEmitterNode)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_Unsubscribe_2, false, RemoteInstance, new object[]{@gRecipientNode, @gEmitterNode});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphEventSystem.EstimateSubscriberCount'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@Int64' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@Int64 @EstimateSubscriberCount(@Azos.@Data.@GDID  @gEmitterNode)
         {
            var call = Async_EstimateSubscriberCount(@gEmitterNode);
            return call.GetValue<@System.@Int64>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphEventSystem.EstimateSubscriberCount'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_EstimateSubscriberCount(@Azos.@Data.@GDID  @gEmitterNode)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_EstimateSubscriberCount_3, false, RemoteInstance, new object[]{@gEmitterNode});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Graph.IGraphEventSystem.GetSubscribers'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Graph.@GraphNode>' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Graph.@GraphNode> @GetSubscribers(@Azos.@Data.@GDID  @gEmitterNode, @System.@Int64  @start, @System.@Int32  @count)
         {
            var call = Async_GetSubscribers(@gEmitterNode, @start, @count);
            return call.GetValue<@System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Graph.@GraphNode>>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Graph.IGraphEventSystem.GetSubscribers'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_GetSubscribers(@Azos.@Data.@GDID  @gEmitterNode, @System.@Int64  @start, @System.@Int32  @count)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_GetSubscribers_4, false, RemoteInstance, new object[]{@gEmitterNode, @start, @count});
            return DispatchCall(request);
         }


  #endregion

  }//class
}//namespace
