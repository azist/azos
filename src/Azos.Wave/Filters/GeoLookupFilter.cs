
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;


using Azos.Environment;
using Azos.Web.GeoLookup;


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
      public GeoLookupFilter(WorkDispatcher dispatcher, string name, int order) : base(dispatcher, name, order) {}
      public GeoLookupFilter(WorkDispatcher dispatcher, IConfigSectionNode confNode): base(dispatcher, confNode) { configureMatches(confNode); }
      public GeoLookupFilter(WorkHandler handler, string name, int order) : base(handler, name, order) {}
      public GeoLookupFilter(WorkHandler handler, IConfigSectionNode confNode): base(handler, confNode){ configureMatches(confNode); }

    #endregion

    #region Fields

      private OrderedRegistry<WorkMatch> m_LookupMatches = new OrderedRegistry<WorkMatch>();

    #endregion

    #region Properties

      /// <summary>
      /// Returns matches used by the filter to determine whether user's location should be looked-up
      /// </summary>
      public OrderedRegistry<WorkMatch> LookupMatches { get{ return m_LookupMatches;}}

    #endregion

    #region Protected

      protected override void DoFilterWork(WorkContext work, IList<WorkFilter> filters, int thisFilterIndex)
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
          var lookedUp = GeoLookupService.Instance.Lookup(address);
          work.GeoEntity = lookedUp;

          if (Server.m_InstrumentationEnabled)
          {
            Interlocked.Increment(ref Server.m_stat_GeoLookup);
            if (lookedUp!=null)
               Interlocked.Increment(ref Server.m_stat_GeoLookupHit);
          }
        }

        this.InvokeNextWorker(work, filters, thisFilterIndex);
      }

    #endregion

    #region .pvt
     private void configureMatches(IConfigSectionNode confNode)
      {
        foreach(var cn in confNode.Children.Where(cn=>cn.IsSameName(WorkMatch.CONFIG_MATCH_SECTION)))
          if(!m_LookupMatches.Register( FactoryUtils.Make<WorkMatch>(cn, typeof(WorkMatch), args: new object[]{ cn })) )
            throw new WaveException(StringConsts.CONFIG_OTHER_DUPLICATE_MATCH_NAME_ERROR.Args(cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value, "{0}".Args(GetType().FullName)));
      }

    #endregion
  }

}
