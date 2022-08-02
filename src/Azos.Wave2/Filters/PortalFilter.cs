/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Linq;

using Azos.Conf;
using Azos.Collections;
using Azos.Data;
using System.Threading.Tasks;

namespace Azos.Wave.Filters
{
  /// <summary>
  /// Manages injection of portal into the work context
  /// </summary>
  public class PortalFilter : WorkFilter
  {
    #region CONSTS
    public const string VAR_PORTAL_NAME = "portal-name";
    public const string CONF_THEME_COOKIE_NAME_ATTR = "theme-cookie-name";
    public const string CONF_USE_THEME_COOKIE_ATTR = "use-theme-cookie";
    public const string DEFAULT_THEME_COOKIE_NAME = "UIT";
    #endregion

    #region .ctor
    public PortalFilter(WorkHandler handler, string name, int order) : base(handler, name, order) {}
    public PortalFilter(WorkHandler handler, IConfigSectionNode confNode): base(handler, confNode) {ctor(confNode);}

    private void ctor(IConfigSectionNode confNode)
    {
      m_UseThemeCookie = confNode.AttrByName(CONF_USE_THEME_COOKIE_ATTR).ValueAsBool(true);
      m_ThemeCookieName = confNode.AttrByName(CONF_THEME_COOKIE_NAME_ATTR).ValueAsString(DEFAULT_THEME_COOKIE_NAME);
      WorkMatch.MakeAndRegisterFromConfig(m_PortalMatches, confNode, GetType().Name);
    }

    #endregion

    #region Fields
    private OrderedRegistry<WorkMatch> m_PortalMatches = new OrderedRegistry<WorkMatch>();
    private bool m_UseThemeCookie;
    private string m_ThemeCookieName = DEFAULT_THEME_COOKIE_NAME;
    #endregion

    #region Properties

    /// <summary>
    /// OrderedRegistry of matches used by the filter to determine whether work should match a portal
    /// </summary>
    public OrderedRegistry<WorkMatch> PortalMatches { get{ return m_PortalMatches;}}


    /// <summary>
    /// Specifies true to interpret ThemeCookieName
    /// </summary>
    public bool UseThemeCookie
    {
      get { return m_UseThemeCookie;}
      set { m_UseThemeCookie = value; }
    }

    /// <summary>
    /// Specifies theme cookie name
    /// </summary>
    public string ThemeCookieName
    {
      get { return m_ThemeCookieName ?? DEFAULT_THEME_COOKIE_NAME;}
      set { m_ThemeCookieName = value; }
    }
    #endregion

    #region Protected

    protected sealed override async Task DoFilterWorkAsync(WorkContext work, CallChain callChain)
    {
      var hub = App.GetPortalHub();

      if (work.m_PortalFilter==null)
      {
        try
        {
          work.m_PortalFilter = this;

          foreach(var match in m_PortalMatches.OrderedValues)
          {
            var matched = match.Make(work);
            if (matched!=null)
            {
              var portalName = matched[VAR_PORTAL_NAME].AsString();
              if (portalName.IsNotNullOrWhiteSpace())
              {
                var portal = hub.Portals[portalName];
                if (portal!=null && !portal.Offline)
                {
                  work.m_Portal = portal;
                  work.m_PortalMatch = match;
                  work.m_PortalMatchedVars = matched;
                }
                break;
              }
            }
          }

          if (work.m_Portal==null)
          {
            var defaultPortal = hub.DefaultOnline;
            if (defaultPortal!=null)
            {
                work.m_Portal = defaultPortal;
            }
          }

          if (m_UseThemeCookie && work.m_Portal!=null)
          {
            //Use regular cookies so client JS can set it up
            var tcv = work.Request.Cookies[m_ThemeCookieName];
            if (tcv.IsNotNullOrWhiteSpace())
            {
              var theme = work.m_Portal.Themes[tcv];
              if (theme != null)
                work.m_PortalTheme = theme;
            }
          }

          if (Server.m_InstrumentationEnabled &&
              work.m_Portal!=null &&
              work.m_Portal.InstrumentationEnabled)
          {
            Server.m_stat_PortalRequest.IncrementLong(work.m_Portal.Name);
          }

          await this.InvokeNextWorkerAsync(work, callChain).ConfigureAwait(false);
        }
        finally
        {
          work.m_PortalFilter = null;
          work.m_Portal = null;
          work.m_PortalMatch = null;
          work.m_PortalMatchedVars = null;
          work.PortalTheme = null;
        }
      }
      else await this.InvokeNextWorkerAsync(work, callChain).ConfigureAwait(false);
    }

    #endregion
  }
}
