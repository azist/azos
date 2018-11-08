/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Threading.Tasks;
using Azos.Scripting;

namespace Azos.Tests.Unit.Glue
{
    [Runnable(TRUN.BASE, 7)]
    public class SyncTests
    {
   const string CONF_SRC_SYNC = @"
 app
 {
  cs='sync://127.0.0.1:5432'
  cs2='sync://127.0.0.1:5433'
  
  object-store
  {
    guid='B05D3038-A821-4BE0-96AA-E6D24DFA746F'
    provider {name='nop' type='Azos.Apps.Volatile.NOPObjectStoreProvider, Azos'}
  }

  glue
  {
     bindings
     {
        binding { name=sync type='Azos.Glue.Native.SyncBinding, Azos' max-msg-size=900000}
     }

     servers
     {
        server { name='test1' node='sync://*:5432' contract-servers='Azos.Tests.Unit.Glue.TestServerA, Azos.Tests.Unit;Azos.Tests.Unit.Glue.TestServerB_ThreadSafe, Azos.Tests.Unit'}
        server { name='test2' node='sync://*:5433' contract-servers='Azos.Tests.Unit.Glue.TestServerB_NotThreadSafe, Azos.Tests.Unit'}
     }
  }
 }
 ";


        const string CONF_SRC_SYNC_TRANSPORTS_A = @"
 app
 {
  cs='sync://127.0.0.1:5432'
  cs2='sync://127.0.0.1:5433'
  
  object-store
  {
    guid='B05D3038-A821-4BE0-96AA-E6D24DFA746F'
    provider {name='nop' type='Azos.Apps.Volatile.NOPObjectStoreProvider, Azos'}
  }

  glue
  {
     bindings
     {
        binding 
        {
          name=sync type='Azos.Glue.Native.SyncBinding, Azos'
          max-msg-size=900000
          client-transport {  max-count=1 }
        }
     }

     servers
     {
        server { name='test1' node='sync://*:5432' contract-servers='Azos.Tests.Unit.Glue.TestServerA, Azos.Tests.Unit;Azos.Tests.Unit.Glue.TestServerB_ThreadSafe, Azos.Tests.Unit'}
        server { name='test2' node='sync://*:5433' contract-servers='Azos.Tests.Unit.Glue.TestServerB_NotThreadSafe, Azos.Tests.Unit'}
     }
  }
 }
 ";





        [Run(TRUN.BASE, null, 7)]
        public void Sync_A_TwoWayCall()
        {
            TestLogic.TestContractA_TwoWayCall(CONF_SRC_SYNC);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Sync_TASK_A_TwoWayCall()
        {
            TestLogic.TASK_TestContractA_TwoWayCall(CONF_SRC_SYNC);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Sync_TASKReturning_A_TwoWayCall()
        {
            TestLogic.TASKReturning_TestContractA_TwoWayCall(CONF_SRC_SYNC);
        }


        [Run(TRUN.BASE, null, 7)]
        public async Task Sync_ASYNC_AWAIT_CALL_TestContractA_TwoWayCall()
        {
            await TestLogic.ASYNC_AWAIT_CALL_TestContractA_TwoWayCall(CONF_SRC_SYNC);
        }

        [Run(TRUN.BASE, null, 7)]
        public async Task Sync_ASYNC_MANY_AWAITS_TestContractA_TwoWayCall()
        {
            await TestLogic.ASYNC_MANY_AWAITS_TestContractA_TwoWayCall(CONF_SRC_SYNC);
        }


        [Run]
        public void Sync_A_TwoWayCall_Timeout()
        {
            TestLogic.TestContractA_TwoWayCall_Timeout(CONF_SRC_SYNC);
        }

        [Run]
        public void Sync_TASK_A_TwoWayCall_Timeout()
        {
            TestLogic.TASK_TestContractA_TwoWayCall_Timeout(CONF_SRC_SYNC);
        }

        [Run]
        public async Task Sync_ASYNC_TestContractA_TwoWayCall_Timeout()
        {
          await TestLogic.ASYNC_TestContractA_TwoWayCall_Timeout(CONF_SRC_SYNC);
        }


        [Run(TRUN.BASE, null, 7, null)]
        public void Sync_A_OneWayCall()
        {
            TestLogic.TestContractA_OneWayCall(CONF_SRC_SYNC);
        }

        [Run(TRUN.BASE, null, 7, null)]
        public async Task Sync_ASYNC_TestContractA_OneWayCall()
        {
          await TestLogic.ASYNC_TestContractA_OneWayCall(CONF_SRC_SYNC);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Sync_B_1()
        {
            TestLogic.TestContractB_1(CONF_SRC_SYNC);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Sync_B_1_Async()
        {
            TestLogic.TestContractB_1_Async(CONF_SRC_SYNC);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Sync_B_2()
        {
            TestLogic.TestContractB_2(CONF_SRC_SYNC);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Sync_B_3()
        {
            TestLogic.TestContractB_3(CONF_SRC_SYNC);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Sync_B_4()
        {
            TestLogic.TestContractB_4(CONF_SRC_SYNC);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Sync_B_4_Async()
        {
            TestLogic.TestContractB_4_Async(CONF_SRC_SYNC);
        }

        [Run]
        public void Sync_B_4_Async_TR_A()
        {
            TestLogic.TestContractB_4_Async(CONF_SRC_SYNC_TRANSPORTS_A);
        }

        [Run]
        public void Sync_B_4_AsyncReactor()
        {
            TestLogic.TestContractB_4_AsyncReactor(CONF_SRC_SYNC);
        }

        [Run]
        public void Sync_B_4_AsyncReactor_TR_A()
        {
            TestLogic.TestContractB_4_AsyncReactor(CONF_SRC_SYNC_TRANSPORTS_A);
        }

        [Run]
        public void Sync_B_4_Parallel_ThreadSafeServer()
        {
            TestLogic.TestContractB_4_Parallel(CONF_SRC_SYNC, threadSafe: true);
        }

        [Run]
        public void Sync_B_4_Parallel_Marshalling_ThreadSafeServer()
        {
            TestLogic.TestContractB_4_Marshalling_Parallel(CONF_SRC_SYNC, threadSafe: true);
        }

        [Run]
        public void Sync_TASK_B_4_Parallel_ThreadSafeServer()
        {
            TestLogic.TASK_TestContractB_4_Parallel(CONF_SRC_SYNC, threadSafe: true);
        }

        [Run]
        public void Sync_B_4_Parallel_ThreadSafeServer_TR_A()
        {
            TestLogic.TestContractB_4_Parallel(CONF_SRC_SYNC_TRANSPORTS_A, threadSafe: true);
        }

        [Run]
        public void Sync_B_4_Parallel_NotThreadSafeServer()
        {
            TestLogic.TestContractB_4_Parallel(CONF_SRC_SYNC, threadSafe: false);
        }

        [Run]
        public void Sync_B_4_Parallel_NotThreadSafeServer_TR_A()
        {
            TestLogic.TestContractB_4_Parallel(CONF_SRC_SYNC_TRANSPORTS_A, threadSafe: false);
        }

        //-----

        [Run]
        public void Sync_B_4_Parallel_ThreadSafeServer_ManyClients()
        {
            TestLogic.TestContractB_4_Parallel_ManyClients(CONF_SRC_SYNC, threadSafe: true);
        }

        [Run]
        public void Sync_B_4_Parallel_ThreadSafeServer_ManyClients_TR_A()
        {
            TestLogic.TestContractB_4_Parallel_ManyClients(CONF_SRC_SYNC_TRANSPORTS_A, threadSafe: true);
        }

        [Run]
        public void Sync_B_4_Parallel_NotThreadSafeServer_ManyClients()
        {
            TestLogic.TestContractB_4_Parallel_ManyClients(CONF_SRC_SYNC, threadSafe: false);
        }

        [Run]
        public void Sync_B_4_Parallel_NotThreadSafeServer_ManyClients_TR_A()
        {
            TestLogic.TestContractB_4_Parallel_ManyClients(CONF_SRC_SYNC_TRANSPORTS_A, threadSafe: false);
        }


        [Run(TRUN.BASE, null, 7)]
        public void Sync_B_5()
        {
            TestLogic.TestContractB_5(CONF_SRC_SYNC);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Sync_B_6()
        {
            TestLogic.TestContractB_6(CONF_SRC_SYNC);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Sync_B_7()
        {
            TestLogic.TestContractB_7(CONF_SRC_SYNC);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Sync_B_8()
        {
            TestLogic.TestContractB_8(CONF_SRC_SYNC);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Sync_B_9()
        {
            TestLogic.TestContractB_9(CONF_SRC_SYNC);
        }




    }

}
