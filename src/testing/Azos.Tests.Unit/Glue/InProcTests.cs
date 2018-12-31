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



using Azos.Apps;
using Azos.Conf;
using Azos.Glue.Native;
using Azos.Glue;
using Azos.IO;

namespace Azos.Tests.Unit.Glue
{
    [Runnable(TRUN.BASE)]
    public class InProcTests
    {
   const string CONF_SRC_INPROC =@"
 app
 {
  cs='inproc://'

  object-store
  {
    guid='B05D3038-A821-4BE0-96AA-E6D24DFA746F'
    provider {name='nop' type='Azos.Apps.Volatile.NOPObjectStoreProvider, Azos'}
  }

  glue
  {
     bindings
     {
        binding { name=inproc type='Azos.Glue.Native.InProcBinding, Azos'}
     }

     servers
     {
        server { name='test' node='inproc://' contract-servers='Azos.Tests.Unit.Glue.TestServerA, Azos.Tests.Unit;Azos.Tests.Unit.Glue.TestServerB_ThreadSafe, Azos.Tests.Unit'}
     }
  }
 }
 ";
        [Run]
        public void Inproc_A_TwoWayCall()
        {
            TestLogic.TestContractA_TwoWayCall(CONF_SRC_INPROC);
        }

        [Run]
        public void Inproc_TASK_A_TwoWayCall()
        {
            TestLogic.TASK_TestContractA_TwoWayCall(CONF_SRC_INPROC);
        }

        [Run]
        public void Inproc_TASKReturning_A_TwoWayCall()
        {
            TestLogic.TASKReturning_TestContractA_TwoWayCall(CONF_SRC_INPROC);
        }

        ////////this test will always fail for inproc, as there is no timeout for inproc because DipsatchRequest() blocks
        //////[Run]
        //////public void Inproc_TASK_A_TwoWayCall_Timeout()
        //////{
        //////    TestLogic.TASK_TestContractA_TwoWayCall_Timeout(CONF_SRC_INPROC);
        //////}

        [Run]
        public void Inproc_A_OneWayCall()
        {
            TestLogic.TestContractA_OneWayCall(CONF_SRC_INPROC);
        }

        [Run]
        public void Inproc_B_1()
        {
            TestLogic.TestContractB_1(CONF_SRC_INPROC);
        }

        [Run]
        public void Inproc_B_1_Async()
        {
            TestLogic.TestContractB_1_Async(CONF_SRC_INPROC);
        }

        [Run]
        public void Inproc_B_2()
        {
            TestLogic.TestContractB_2(CONF_SRC_INPROC);
        }

        [Run]
        public void Inproc_B_3()
        {
            TestLogic.TestContractB_3(CONF_SRC_INPROC);
        }


        [Run]
        public void Inproc_B_4()
        {
            TestLogic.TestContractB_4(CONF_SRC_INPROC);
        }

        [Run]
        public void Inproc_B_4_Async()
        {
            TestLogic.TestContractB_4_Async(CONF_SRC_INPROC);
        }


        [Run]
        public void Inproc_B_4_Parallel()
        {
            TestLogic.TestContractB_4_Parallel(CONF_SRC_INPROC, threadSafe: true);
        }

        [Run]
        public void Inproc_B_4_Marshalling_Parallel()
        {
            TestLogic.TestContractB_4_Marshalling_Parallel(CONF_SRC_INPROC, threadSafe: true);
        }



        [Run]
        public void Inproc_TASK_B_4_Parallel()
        {
            TestLogic.TASK_TestContractB_4_Parallel(CONF_SRC_INPROC, threadSafe: true);
        }

        [Run]
        public void Inproc_B_4_Parallel_ManyClients()
        {
            TestLogic.TestContractB_4_Parallel_ManyClients(CONF_SRC_INPROC, threadSafe: true);
        }

        [Run]
        public void Inproc_B_5()
        {
            TestLogic.TestContractB_5(CONF_SRC_INPROC);
        }


        [Run]
        public void Inproc_B_6()
        {
            TestLogic.TestContractB_6(CONF_SRC_INPROC);
        }

        [Run]
        public void Inproc_B_7()
        {
            TestLogic.TestContractB_7(CONF_SRC_INPROC);
        }

        [Run]
        public void Inproc_B_8()
        {
            TestLogic.TestContractB_8(CONF_SRC_INPROC);
        }


    }

}
