/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Net;

using Azos.Conf;
using Azos.Data;
using Azos.Web.GeoLookup;


namespace Azos.Wave.Filters
{
  /// <summary>
  /// Upon match, injects real caller WorkContext.EffectiveCallerIPEndPoint
  /// </summary>
  public class EffectiveCallerIPEndPointFilter : WorkFilter
  {
    #region .ctor
      public EffectiveCallerIPEndPointFilter(WorkDispatcher dispatcher, string name, int order) : base(dispatcher, name, order) {}
      public EffectiveCallerIPEndPointFilter(WorkDispatcher dispatcher, IConfigSectionNode confNode): base(dispatcher, confNode) { ConfigAttribute.Apply(this, confNode); }
      public EffectiveCallerIPEndPointFilter(WorkHandler handler, string name, int order) : base(handler, name, order) {}
      public EffectiveCallerIPEndPointFilter(WorkHandler handler, IConfigSectionNode confNode): base(handler, confNode){ ConfigAttribute.Apply(this, confNode); }

    #endregion

    #region Properties

      [Config] public string  RealIpHdr  {  get; set; }
      [Config] public string  RealPortHdr{  get; set; }

    #endregion

    #region Protected

      protected override void DoFilterWork(WorkContext work, IList<WorkFilter> filters, int thisFilterIndex)
      {
        var ipep  = work.m_EffectiveCallerIPEndPoint;

        if (ipep==null)
        {
          var hip = RealIpHdr;
          var hprt = RealPortHdr;

          hip = hip.IsNotNullOrWhiteSpace() ? hip : "Real-IP";
          hprt  = hprt.IsNotNullOrWhiteSpace() ? hprt : "Real-Port";

          var rIP = work.Request.Headers[hip];
          var rPort = work.Request.Headers[hprt];

          if (rIP.IsNotNullOrWhiteSpace())
          {
            IPAddress address = null;
            if (IPAddress.TryParse(rIP, out address))
            {
              work.m_EffectiveCallerIPEndPoint = new IPEndPoint(address, rPort.AsInt(0));
            }
          }
        }

        this.InvokeNextWorker(work, filters, thisFilterIndex);
      }

    #endregion

  }

}
