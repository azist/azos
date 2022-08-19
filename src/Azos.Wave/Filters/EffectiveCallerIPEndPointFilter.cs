/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Net;
using System.Threading.Tasks;

using Azos.Conf;
using Azos.Data;

namespace Azos.Wave.Filters
{
  /// <summary>
  /// Upon match, injects real caller WorkContext.EffectiveCallerIPEndPoint analyzing the specified named header, or "X-Forwarded-For"
  /// </summary>
  public class EffectiveCallerIPEndPointFilter : WorkFilter
  {
    #region .ctor
    public EffectiveCallerIPEndPointFilter(WorkHandler handler, string name, int order) : base(handler, name, order) {}
    public EffectiveCallerIPEndPointFilter(WorkHandler handler, IConfigSectionNode confNode): base(handler, confNode){ ConfigAttribute.Apply(this, confNode); }
    #endregion

    #region Properties
    [Config] public string  RealIpHdr  {  get; set; }
    [Config] public string  RealPortHdr{  get; set; }
    #endregion

    #region Protected
    protected override async Task DoFilterWorkAsync(WorkContext work, CallChain callChain)
    {
      var ipep  = work.m_EffectiveCallerIPEndPoint;

      if (ipep==null)
      {
        var hip = RealIpHdr;
        var hprt = RealPortHdr;

        hip = hip.IsNotNullOrWhiteSpace() ? hip : "Real-IP";
        hprt  = hprt.IsNotNullOrWhiteSpace() ? hprt : "Real-Port";


        var rIP = work.Request.HeaderAsString(hip);
        var rPort = work.Request.HeaderAsString(hprt);

        if (hip.EqualsOrdIgnoreCase(WebConsts.HTTP_HDR_X_FORWARDED_FOR) && rIP.IsNotNullOrWhiteSpace())
        {
          var ic = rIP.LastIndexOf(',');
          if (ic>0 && ic < rIP.Length-1)
          {
            rIP = rIP.Substring(ic + 1);//take the last IP address in the header list to prevent spoofing
            // see: https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Forwarded-For
          }
        }

        if (rIP.IsNotNullOrWhiteSpace())
        {
          IPAddress address = null;
          if (IPAddress.TryParse(rIP, out address))
          {
            work.m_EffectiveCallerIPEndPoint = new IPEndPoint(address, rPort.AsInt(0));
          }
        }
      }

      await this.InvokeNextWorkerAsync(work, callChain).ConfigureAwait(false);
    }
    #endregion
  }
}
