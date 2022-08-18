/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Threading.Tasks;

using Azos.Conf;
using Azos.Collections;
using Azos.Sky.GeoLookup;

namespace Azos.Wave.Filters
{
  /// <summary>
  /// Upon match, looks up user's geo location based on a IP addresses
  /// </summary>
  public class GeoLookupFilter : WorkFilter
  {
    #region CONSTS
    /// <summary>
    /// Allows to override user real address with this one
    /// </summary>
    public const string VAR_USE_ADDR = "use-addr";
    #endregion

    #region .ctor
    public GeoLookupFilter(WorkHandler handler, string name, int order) : base(handler, name, order) {}
    public GeoLookupFilter(WorkHandler handler, IConfigSectionNode confNode): base(handler, confNode){ ctor(confNode); }

    private void ctor(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);

      WorkMatch.MakeAndRegisterFromConfig(m_LookupMatches, node, GetType().Name);
    }
    #endregion

    #region Fields
    private OrderedRegistry<WorkMatch> m_LookupMatches = new OrderedRegistry<WorkMatch>();
    [Config]private string m_GeoLookupModuleName;
    #endregion

    #region Properties
    /// <summary>
    /// Returns matches used by the filter to determine whether user's location should be looked-up
    /// </summary>
    public OrderedRegistry<WorkMatch> LookupMatches => m_LookupMatches;

    /// <summary>
    /// Sets name of the geo lookup module
    /// </summary>
    public string GeoLookupModuleName
    {
      get {  return m_GeoLookupModuleName.IsNullOrWhiteSpace() ? nameof(GeoLookupModule) : m_GeoLookupModuleName;}
      set { m_GeoLookupModuleName = value; }
    }
    #endregion

    #region Protected

    protected override async Task DoFilterWorkAsync(WorkContext work, CallChain callChain)
    {
      var needLookup = true;
      var address = work.EffectiveCallerIPEndPoint.Address;

      if (work.GeoEntity!=null)
        needLookup = !string.Equals(work.GeoEntity.Query, address);


      if (needLookup && m_LookupMatches.Count>0)
      {
        foreach(var match in m_LookupMatches.OrderedValues)
        {
          var matched = match.Make(work);
          needLookup = matched!=null;

          if (needLookup)
          {
            var useAddr = matched[VAR_USE_ADDR];
            if (useAddr!=null)
            {
              IPAddress ip;
              if (IPAddress.TryParse(useAddr.ToString(), out ip))
                address = ip;
            }
            break;
          }
        }
      }

      if (needLookup)
      {
        var svc = App.ModuleRoot.Get<GeoLookupModule>(GeoLookupModuleName);//throws if module not found
        var lookedUp = svc.Lookup(address);
        work.GeoEntity = lookedUp;

        if (Server.m_InstrumentationEnabled)
        {
          Interlocked.Increment(ref Server.m_stat_GeoLookup);
          if (lookedUp!=null)
              Interlocked.Increment(ref Server.m_stat_GeoLookupHit);
        }
      }

      await this.InvokeNextWorkerAsync(work, callChain).ConfigureAwait(false);
    }

    #endregion

  }

}
