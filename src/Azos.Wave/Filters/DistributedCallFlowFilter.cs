/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Linq;

using Azos.Apps;
using Azos.Conf;
using Azos.Serialization.JSON;

namespace Azos.Wave.Filters
{
  /// <summary>
  /// Establishes a distributed call flow scope for a Wave call flow
  /// </summary>
  public sealed class DistributedCallFlowFilter : WorkFilter
  {
    public DistributedCallFlowFilter(WorkDispatcher dispatcher, string name, int order) : base(dispatcher, name, order){ }
    public DistributedCallFlowFilter(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode){ }
    public DistributedCallFlowFilter(WorkHandler handler, string name, int order) : base(handler, name, order){ }
    public DistributedCallFlowFilter(WorkHandler handler, IConfigSectionNode confNode) : base(handler, confNode){ }


    /// <summary>
    /// When set, enables injection of DistributedCallFlow context
    /// </summary>
    [Config(Default = CoreConsts.HTTP_HDR_DEFAULT_CALL_FLOW)]
    public string DistributedCallFlowHeader { get; set; } = CoreConsts.HTTP_HDR_DEFAULT_CALL_FLOW;

    protected override void DoFilterWork(WorkContext work, IList<WorkFilter> filters, int thisFilterIndex)
    {
      var original = ExecutionContext.CallFlow;

      try
      {
        var hdrName = DistributedCallFlowHeader;
        if (hdrName.IsNotNullOrWhiteSpace() &&  !(original is DistributedCallFlow))
        {
          DistributedCallFlow flow = null;
          var hdrJson = work.Request.Headers[hdrName];

          if (hdrJson.IsNotNullOrWhiteSpace())
          {
            JsonDataMap existing;

            try   { existing = (hdrJson.JsonToDataObject() as JsonDataMap).IsTrue(v => v != null && v.Count > 0, nameof(existing)); }
            catch { throw HTTPStatusException.BadRequest_400("Bad distributed call flow header"); }

            flow = DistributedCallFlow.Continue(App, existing);
          }

          if (flow==null)
            flow = DistributedCallFlow.Start(App, App.Description);
        }

        this.InvokeNextWorker(work, filters, thisFilterIndex);
      }
      finally
      {
        ExecutionContext.__SetThreadLevelCallContext(original);
      }
    }
  }
}
