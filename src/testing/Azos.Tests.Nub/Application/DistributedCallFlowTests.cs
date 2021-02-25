/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Conf;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Application
{
  [Runnable]
  public class DistributedCallFlowTests : IRunHook
  {
    public bool Prologue(Runner runner, FID id, MethodInfo method, RunAttribute attr, ref object[] args)
    {
      ExecutionContext.__SetThreadLevelCallContext(null);
      return false;
    }

    public bool Epilogue(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
    {
      ExecutionContext.__SetThreadLevelCallContext(null);
      return false;
    }


    [Run]
    public void Test001()
    {
      //---- 1st host
      var start = Guid.NewGuid();
      ExecutionContext.__SetThreadLevelCallContext(new CodeCallFlow(start, callerPort: "my-entrypoint" ));


      var flow = DistributedCallFlow.Start(NOPApplication.Instance, "My call flow");

      Aver.AreEqual(start, flow.ID);
      Aver.AreEqual(1, flow.Count);

      var jsonHeader = flow.ToHeaderValue();
      //SEND OVER HTTP -------------------------
      ExecutionContext.__SetThreadLevelCallContext(null);
      //--------------------------------------------------

      //---- 2nd host
      flow = DistributedCallFlow.Continue(NOPApplication.Instance, jsonHeader);

      Aver.AreEqual(start, flow.ID);
      Aver.AreEqual(2, flow.Count);

      jsonHeader = flow.ToHeaderValue();
      //SEND OVER HTTP -------------------------
      ExecutionContext.__SetThreadLevelCallContext(null);
      //--------------------------------------------------

      //---- 3nd host
      flow = DistributedCallFlow.Continue(NOPApplication.Instance, jsonHeader);

      Aver.AreEqual(start, flow.ID);
      Aver.AreEqual(3, flow.Count);

      var json = flow.ToJson(JsonWritingOptions.PrettyPrint);

      "start is: {0} \n {1} is {2} chars".SeeArgs(start, json, json.Length);
    }

    [Run]
    public void Test002()
    {
      //---- 1st host
      var start = Guid.NewGuid();
      ExecutionContext.__SetThreadLevelCallContext(new CodeCallFlow(start, callerPort: "my-entrypoint-A"));


      var flow = DistributedCallFlow.Start(NOPApplication.Instance, "My call flow");
      var jsonHeader = flow.ToHeaderValue();

      flow.See();
      Aver.AreEqual(start, flow.ID);
      Aver.AreEqual("my-entrypoint-A", flow.CallerPort);
    }


    [Run]
    public async Task Test003_ExecuteBlockAsync()
    {
      //---- 1st host
      var start = Guid.NewGuid();
      var flow = DistributedCallFlow.Start(NOPApplication.Instance, "Flow A", start, "Ryazanov", callerPort: "TV");

      //later in flow....
      await DistributedCallFlow.ExecuteBlockAsync(NOPApplication.Instance, (innerFlow) => {
        innerFlow.See();

        innerFlow.ToHeaderValue().See();

        Aver.AreSameRef(innerFlow, ExecutionContext.CallFlow);

        Aver.AreEqual(start, flow.ID);
        Aver.AreEqual(start, innerFlow.ID);
        Aver.AreEqual("TV", flow.CallerPort);
        Aver.AreEqual("TV", innerFlow.CallerPort);

        Aver.AreEqual("Ryazanov", flow.DirectorName);
        Aver.AreEqual("Ryazanov", innerFlow.DirectorName);

        Aver.AreEqual(2, innerFlow.Count);

        Aver.AreEqual("Ryazanov", innerFlow[0].DirectorName);
        Aver.AreEqual("Dovzhenko", innerFlow[1].DirectorName);

        Aver.AreSameRef(innerFlow[0], innerFlow.EntryPoint);
        Aver.AreSameRef(innerFlow[1], innerFlow.Current);

        Aver.AreEqual(start, innerFlow.EntryPoint.ID);
        Aver.AreNotEqual(start, innerFlow.Current.ID);

        return Task.FromResult(0);
      }, "Continuation", directorName: "Dovzhenko", callerAgent: "mytezt003");//, callerPort: "VCR");

     Aver.AreSameRef(flow, ExecutionContext.CallFlow);
    }
  }
}
