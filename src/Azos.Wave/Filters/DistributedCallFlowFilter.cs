/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Serialization.JSON;
using Azos.Web;

namespace Azos.Wave.Filters
{
  /// <summary>
  /// Establishes a <see cref="DistributedCallFlow"/> (DCF) scope for a Wave call flow if the request header is sent with call flow json
  /// or starts a new call flow if <see cref="Establish"/> is true even when header is not sent.
  /// If DCF is already set, then does nothing
  /// </summary>
  public sealed class DistributedCallFlowFilter : WorkFilter
  {
    public DistributedCallFlowFilter(WorkHandler handler, string name, int order) : base(handler, name, order){ }
    public DistributedCallFlowFilter(WorkHandler handler, IConfigSectionNode confNode) : base(handler, confNode){ }


    /// <summary>
    /// When set, enables injection of DistributedCallFlow context as continuation of incoming call flow.
    /// When not set (null/empty) the does not continue DCF from the caller, however if <see cref="Establish"/>
    /// is true the DCF can still start even when this header is turned off
    /// </summary>
    [Config(Default = CoreConsts.HTTP_HDR_DEFAULT_CALL_FLOW)]
    public string DistributedCallFlowHeader { get; set; } = CoreConsts.HTTP_HDR_DEFAULT_CALL_FLOW;

    /// <summary>
    /// When true starts DCF even if the header is not passed or turned off.
    /// If false (default), sets DCF only if the header is turned on, header value is passed and contains a DCF json, otherwise does nothing
    /// </summary>
    [Config(Default = false)]
    public bool Establish { get; set; } = false;

    protected override async Task DoFilterWorkAsync(WorkContext work, CallChain callChain)
    {
      var original = ExecutionContext.CallFlow;

      try
      {
        if (original is not DistributedCallFlow)
        {
          var hdrName = DistributedCallFlowHeader;

          DistributedCallFlow dcflow = null;

          if (hdrName.IsNotNullOrWhiteSpace())
          {
            var hdrJson = work.Request.HeaderAsString(hdrName);

            if (hdrJson.IsNotNullOrWhiteSpace())
            {
              JsonDataMap existing;

              try   { existing = (hdrJson.JsonToDataObject() as JsonDataMap).IsTrue(v => v != null && v.Count > 0, nameof(existing)); }
              catch { throw HTTPStatusException.BadRequest_400("Bad distributed call flow header"); }

              dcflow = DistributedCallFlow.Continue(App, existing);
            }
          }

          if (dcflow == null && Establish)
          {
            dcflow = DistributedCallFlow.Start(App, $"{App.AppId}/{App.Description}");
          }
        }

        await this.InvokeNextWorkerAsync(work, callChain).ConfigureAwait(false);
      }
      finally
      {
        ExecutionContext.__SetThreadLevelCallContext(original);
      }
    }
  }
}
