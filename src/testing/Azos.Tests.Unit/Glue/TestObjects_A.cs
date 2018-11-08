/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Azos.Glue;
using Azos.Glue.Protocol;

namespace Azos.Tests.Unit.Glue
{

        [Glued]
        public interface ITestContractA
        {
                     string Method1(int x);
            [OneWay] void Method2(int x);

                     int Sleeper(int sleepMs);
        }

        public class TestServerA : ITestContractA
        {
            internal static int s_Accumulator;


            public string Method1(int x)
            {
                s_Accumulator += x;
                return s_Accumulator.ToString();
            }

            public void Method2(int x)
            {
                s_Accumulator += x;
            }

            public int Sleeper(int sleepMs)
            {
               System.Threading.Thread.Sleep(sleepMs);
               return sleepMs;
            }
        }


  ///<summary>
  /// Client for glued contract Azos.Tests.Unit.Glue.ITestContractA server.
  /// Each contract method has synchronous and asynchronous versions, the later denoted by 'Async_' prefix.
  /// May inject client-level inspectors here like so:
  ///   client.MsgInspectors.Register( new YOUR_CLIENT_INSPECTOR_TYPE());
  ///</summary>
  public class TestContractAClient : ClientEndPoint, Azos.Tests.Unit.Glue.ITestContractA
  {

  #region Static Members

     private static TypeSpec s_ts_CONTRACT;
     private static MethodSpec @s_ms_Method1_0;
     private static MethodSpec @s_ms_Method2_1;
     private static MethodSpec @s_ms_Sleeper_2;

     //static .ctor
     static TestContractAClient()
     {
         var t = typeof(Azos.Tests.Unit.Glue.ITestContractA);
         s_ts_CONTRACT = new TypeSpec(t);
         @s_ms_Method1_0 = new MethodSpec(t.GetMethod("Method1", new Type[]{ typeof(@System.@Int32) }));
         @s_ms_Method2_1 = new MethodSpec(t.GetMethod("Method2", new Type[]{ typeof(@System.@Int32) }));
         @s_ms_Sleeper_2 = new MethodSpec(t.GetMethod("Sleeper", new Type[]{ typeof(@System.@Int32) }));
     }
  #endregion

  #region .ctor
     public TestContractAClient(string node, Binding binding = null) : base(node, binding) { ctor(); }
     public TestContractAClient(Node node, Binding binding = null) : base(node, binding) { ctor(); }
     public TestContractAClient(IGlue glue, string node, Binding binding = null) : base(glue, node, binding) { ctor(); }
     public TestContractAClient(IGlue glue, Node node, Binding binding = null) : base(glue, node, binding) { ctor(); }

     //common instance .ctor body
     private void ctor()
     {

     }

  #endregion

     public override Type Contract
     {
       get { return typeof(Azos.Tests.Unit.Glue.ITestContractA); }
     }



  #region Contract Methods

         ///<summary>
         /// Synchronous invoker for  'Azos.Tests.Unit.Glue.ITestContractA.Method1'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@String' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@String @Method1(@System.@Int32  @x)
         {
            var call = Async_Method1(@x);
            return call.GetValue<@System.@String>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Tests.Unit.Glue.ITestContractA.Method1'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_Method1(@System.@Int32  @x)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_Method1_0, false, RemoteInstance, new object[]{@x});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'Azos.Tests.Unit.Glue.ITestContractA.Method2'.
         /// This is a one-way call per contract specification, meaning - the server sends no acknowledgement of this call receipt and
         /// there is no result that server could return back to the caller.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         ///</summary>
         public void @Method2(@System.@Int32  @x)
         {
            var call = Async_Method2(@x);
            if (call.CallStatus != CallStatus.Dispatched)
                throw new ClientCallException(call.CallStatus, "Call failed: 'TestContractAClient.Method2'");
         }

         ///<summary>
         /// Asynchronous invoker for  'Azos.Tests.Unit.Glue.ITestContractA.Method2'.
         /// This is a one-way call per contract specification, meaning - the server sends no acknowledgement of this call receipt and
         /// there is no result that server could return back to the caller.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg.
         ///</summary>
         public CallSlot Async_Method2(@System.@Int32  @x)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_Method2_1, true, RemoteInstance, new object[]{@x});
            return DispatchCall(request);
         }


         public int @Sleeper(@System.@Int32  @sleepMs)
         {
            var call = Async_Sleeper(@sleepMs);
            return call.GetValue<@System.@Int32>();
         }

         public CallSlot Async_Sleeper(@System.@Int32  @sleepMs)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_Sleeper_2, false, RemoteInstance, new object[]{@sleepMs});
            return DispatchCall(request);
         }


  #endregion

  }//class


}
