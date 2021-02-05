/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Application
{
  [Runnable]
  public class DistributedCallFlowTests
  {
    [Run]
    public void Test001()
    {
      //---- 1st host
      var start = Guid.NewGuid();
      ExecutionContext.__SetThreadLevelCallContext(new CodeCallFlow(start, callerPort: "my-entrypoint" ));


      var flow = DistributedCallFlow.Start(NOPApplication.Instance, "My call flow");
      var jsonHeader = flow.ToJson(JsonWritingOptions.CompactASCII);
      //SEND OVER HTTP -------------------------
      ExecutionContext.__SetThreadLevelCallContext(null);
      //--------------------------------------------------

      //---- 2nd host
      flow = DistributedCallFlow.Continue(NOPApplication.Instance, jsonHeader.JsonToDataObject() as JsonDataMap );


      jsonHeader = flow.ToJson(JsonWritingOptions.CompactASCII);
      //SEND OVER HTTP -------------------------
      ExecutionContext.__SetThreadLevelCallContext(null);
      //--------------------------------------------------

      //---- 3nd host
      flow = DistributedCallFlow.Continue(NOPApplication.Instance, jsonHeader.JsonToDataObject() as JsonDataMap);

      var json = flow.ToJson(JsonWritingOptions.PrettyPrint);

      "start is: {0} \n {1} is {2} chars".SeeArgs(start, json, json.Length);
    }
  }
}
