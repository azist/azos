/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Azos.Scripting;


using NFX;
using Azos.Apps;
using Azos.Conf;
using Azos.Glue.Native;
using Azos.Glue;
using Azos.IO;

namespace Azos.Tests.Unit.Glue
{
    [Runnable(TRUN.BASE, 7)]
    public class MpxTests
    {
   const string CONF_SRC_MPX =@"
 nfx
 {
  cs='mpx://127.0.0.1:5701'
  cs2='mpx://127.0.0.1:5702'
  

  log
  {
    destination{ type = 'Azos.Log.Destinations.ConsoleDestination, NFX'}
  }

  object-store
  {
    guid='B05D3038-A821-4BE0-96AA-E6D24DFA746F'
    provider {name='nop' type='Azos.Apps.Volatile.NOPObjectStoreProvider, NFX'}
  }

  glue
  {
     bindings
     {
        binding { name=mpx type='Azos.Glue.Native.MpxBinding, NFX' max-msg-size=900000}
     }

     servers
     {
        server { name='test1' node='mpx://*:5701' contract-servers='Azos.Tests.Unit.Glue.TestServerA, Azos.Tests.Unit;Azos.Tests.Unit.Glue.TestServerB_ThreadSafe, Azos.Tests.Unit'}
        server { name='test2' node='mpx://*:5702' contract-servers='Azos.Tests.Unit.Glue.TestServerB_NotThreadSafe, Azos.Tests.Unit'}
     }
  }
 }
 ";


        const string CONF_SRC_MPX_TRANSPORTS_A =@"
 nfx
 {
  cs='mpx://127.0.0.1:5701'
  cs2='mpx://127.0.0.1:5702'
  
  object-store
  {
    guid='B05D3038-A821-4BE0-96AA-E6D24DFA746F'
    provider {name='nop' type='Azos.Apps.Volatile.NOPObjectStoreProvider, NFX'}
  }

  glue
  {
     bindings
     {
        binding 
        {
          name=mpx type='Azos.Glue.Native.MpxBinding, NFX'
          max-msg-size=900000
          client-transport {  max-count=1 }
        }
     }

     servers
     {
        server { name='test1' node='mpx://*:5701' contract-servers='Azos.Tests.Unit.Glue.TestServerA, Azos.Tests.Unit;Azos.Tests.Unit.Glue.TestServerB_ThreadSafe, Azos.Tests.Unit'}
        server { name='test2' node='mpx://*:5702' contract-servers='Azos.Tests.Unit.Glue.TestServerB_NotThreadSafe, Azos.Tests.Unit'}
     }
  }
 }
 ";





        [Run(TRUN.BASE, null, 7)]
        public void Mpx_A_TwoWayCall()
        {
            TestLogic.TestContractA_TwoWayCall(CONF_SRC_MPX);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Mpx_TASK_A_TwoWayCall()
        {
            TestLogic.TASK_TestContractA_TwoWayCall(CONF_SRC_MPX);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Mpx_TASKReturning_A_TwoWayCall()
        {
            TestLogic.TASKReturning_TestContractA_TwoWayCall(CONF_SRC_MPX);
        }

        [Run]
        public void Mpx_A_TwoWayCall_Timeout()
        {
            TestLogic.TestContractA_TwoWayCall_Timeout(CONF_SRC_MPX);
        }

        [Run]
        public void Mpx_TASK_A_TwoWayCall_Timeout()
        {
            TestLogic.TASK_TestContractA_TwoWayCall_Timeout(CONF_SRC_MPX);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Mpx_A_OneWayCall()
        {
            TestLogic.TestContractA_OneWayCall(CONF_SRC_MPX);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Mpx_B_1()
        {
            TestLogic.TestContractB_1(CONF_SRC_MPX);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Mpx_B_1_Async()
        {
            TestLogic.TestContractB_1_Async(CONF_SRC_MPX);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Mpx_B_2()
        {
            TestLogic.TestContractB_2(CONF_SRC_MPX);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Mpx_B_3()
        {
            TestLogic.TestContractB_3(CONF_SRC_MPX);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Mpx_B_4()
        {
            TestLogic.TestContractB_4(CONF_SRC_MPX);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Mpx_B_4_Async()
        {
            TestLogic.TestContractB_4_Async(CONF_SRC_MPX);
        }

        [Run]
        public void Mpx_B_4_Async_TR_A()
        {
            TestLogic.TestContractB_4_Async(CONF_SRC_MPX_TRANSPORTS_A);
        }

        [Run]
        public void Mpx_B_4_AsyncReactor()
        {
            TestLogic.TestContractB_4_AsyncReactor(CONF_SRC_MPX);
        }

        [Run]
        public void Mpx_B_4_AsyncReactor_TR_A()
        {
            TestLogic.TestContractB_4_AsyncReactor(CONF_SRC_MPX_TRANSPORTS_A);
        }

        [Run]
        public void Mpx_B_4_Parallel_ThreadSafeServer()
        {
            TestLogic.TestContractB_4_Parallel(CONF_SRC_MPX, threadSafe: true);
        }

        [Run]
        public void Mpx_B_4_Parallel_Marshalling_ThreadSafeServer()
        {
            TestLogic.TestContractB_4_Marshalling_Parallel(CONF_SRC_MPX, threadSafe: true);
        }


        [Run]
        public void Mpx_TASK_B_4_Parallel_ThreadSafeServer()
        {
            TestLogic.TASK_TestContractB_4_Parallel(CONF_SRC_MPX, threadSafe: true);
        }

        [Run]
        public void Mpx_B_4_Parallel_ThreadSafeServer_TR_A()
        {
            TestLogic.TestContractB_4_Parallel(CONF_SRC_MPX_TRANSPORTS_A, threadSafe: true);
        }

        [Run]
        public void Mpx_B_4_Parallel_NotThreadSafeServer()
        {
            TestLogic.TestContractB_4_Parallel(CONF_SRC_MPX, threadSafe: false);
        }

        [Run]
        public void Mpx_B_4_Parallel_NotThreadSafeServer_TR_A()
        {
            TestLogic.TestContractB_4_Parallel(CONF_SRC_MPX_TRANSPORTS_A, threadSafe: false);
        }

        //-----

        [Run]
        public void Mpx_B_4_Parallel_ThreadSafeServer_ManyClients()
        {
            TestLogic.TestContractB_4_Parallel_ManyClients(CONF_SRC_MPX, threadSafe: true);
        }

        [Run]
        public void Mpx_B_4_Parallel_ThreadSafeServer_ManyClients_TR_A()
        {
            TestLogic.TestContractB_4_Parallel_ManyClients(CONF_SRC_MPX_TRANSPORTS_A, threadSafe: true);
        }

        [Run]
        public void Mpx_B_4_Parallel_NotThreadSafeServer_ManyClients()
        {
            TestLogic.TestContractB_4_Parallel_ManyClients(CONF_SRC_MPX, threadSafe: false);
        }

        [Run]
        public void Mpx_B_4_Parallel_NotThreadSafeServer_ManyClients_TR_A()
        {
            TestLogic.TestContractB_4_Parallel_ManyClients(CONF_SRC_MPX_TRANSPORTS_A, threadSafe: false);
        }


        [Run(TRUN.BASE, null, 7)]
        public void Mpx_B_5()
        {
            TestLogic.TestContractB_5(CONF_SRC_MPX);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Mpx_B_6()
        {
            TestLogic.TestContractB_6(CONF_SRC_MPX);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Mpx_B_7()
        {
            TestLogic.TestContractB_7(CONF_SRC_MPX);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Mpx_B_8()
        {
            TestLogic.TestContractB_8(CONF_SRC_MPX);
        }

        [Run(TRUN.BASE, null, 7)]
        public void Mpx_B_9()
        {
            TestLogic.TestContractB_9(CONF_SRC_MPX);
        }




    }

}
