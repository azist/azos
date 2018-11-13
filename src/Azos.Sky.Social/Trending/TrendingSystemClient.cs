

using System;

using Azos.Glue;
using Azos.Glue.Protocol;


namespace Azos.Sky.Social.Trending
{
// This implementation needs @Azos.Sky.@Social.@Trending.@ITrendingSystemClient, so
// it can be used with ServiceClientHub class

  ///<summary>
  /// Client for glued contract Azos.Sky.Social.Trending.ITrendingSystem server.
  /// Each contract method has synchronous and asynchronous versions, the later denoted by 'Async_' prefix.
  /// May inject client-level inspectors here like so:
  ///   client.MsgInspectors.Register( new YOUR_CLIENT_INSPECTOR_TYPE());
  ///</summary>
  public class TrendingSystemClient : ClientEndPoint, @Azos.Sky.@Social.@Trending.@ITrendingSystemClient
  {

  #region Static Members

     private static TypeSpec s_ts_CONTRACT;
     private static MethodSpec @s_ms_Send_0;
     private static MethodSpec @s_ms_GetTrending_1;

     //static .ctor
     static TrendingSystemClient()
     {
         var t = typeof(@Azos.Sky.@Social.@Trending.@ITrendingSystem);
         s_ts_CONTRACT = new TypeSpec(t);
         @s_ms_Send_0 = new MethodSpec(t.GetMethod("Send", new Type[]{ typeof(@Azos.Sky.@Social.@Trending.@SocialTrendingGauge[]) }));
         @s_ms_GetTrending_1 = new MethodSpec(t.GetMethod("GetTrending", new Type[]{ typeof(@Azos.Sky.@Social.@Trending.@TrendingQuery) }));
     }
  #endregion

  #region .ctor
     public TrendingSystemClient(string node, Binding binding = null) : base(node, binding) { ctor(); }
     public TrendingSystemClient(Node node, Binding binding = null) : base(node, binding) { ctor(); }
     public TrendingSystemClient(IGlue glue, string node, Binding binding = null) : base(glue, node, binding) { ctor(); }
     public TrendingSystemClient(IGlue glue, Node node, Binding binding = null) : base(glue, node, binding) { ctor(); }

     //common instance .ctor body
     private void ctor()
     {

     }

  #endregion

     public override Type Contract
     {
       get { return typeof(@Azos.Sky.@Social.@Trending.@ITrendingSystem); }
     }



  #region Contract Methods

         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Trending.ITrendingSystem.Send'.
         /// This is a one-way call per contract specification, meaning - the server sends no acknowledgment of this call receipt and
         /// there is no result that server could return back to the caller.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         ///</summary>
         public void @Send(@Azos.Sky.@Social.@Trending.@SocialTrendingGauge[]  @gauges)
         {
            var call = Async_Send(@gauges);
            if (call.CallStatus != CallStatus.Dispatched)
                throw new ClientCallException(call.CallStatus, "Call failed: 'TrendingSystemClient.Send'");
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Trending.ITrendingSystem.Send'.
         /// This is a one-way call per contract specification, meaning - the server sends no acknowledgment of this call receipt and
         /// there is no result that server could return back to the caller.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg.
         ///</summary>
         public CallSlot Async_Send(@Azos.Sky.@Social.@Trending.@SocialTrendingGauge[]  @gauges)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_Send_0, true, RemoteInstance, new object[]{@gauges});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Sky.Social.Trending.ITrendingSystem.GetTrending'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Trending.@TrendingEntity>' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Trending.@TrendingEntity> @GetTrending(@Azos.Sky.@Social.@Trending.@TrendingQuery  @query)
         {
            var call = Async_GetTrending(@query);
            return call.GetValue<@System.@Collections.@Generic.@IEnumerable<@Azos.Sky.@Social.@Trending.@TrendingEntity>>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Sky.Social.Trending.ITrendingSystem.GetTrending'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_GetTrending(@Azos.Sky.@Social.@Trending.@TrendingQuery  @query)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_GetTrending_1, false, RemoteInstance, new object[]{@query});
            return DispatchCall(request);
         }


  #endregion

  }//class
}//namespace
