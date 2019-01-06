/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;

using Azos;
using Azos.Log;
using Azos.Wave.Mvc;
using Azos.Serialization.JSON;
using Azos.Web;

using Azos.Sky.Metabase;

namespace Azos.Sky.WebManager.Controllers
{
  /// <summary>
  /// Provides Metabase access
  /// </summary>
  public sealed class TheSystem: WebManagerController
  {
    /// <summary>
    /// Navigates/redirects request to process "appName" at host with metabase path "metabasePath"
    /// </summary>
    [Action]
    public object Navigate(string metabasePath, string appName)
    {
      var svc = SysConsts.NETWORK_SVC_WEB_MANAGER_PREFIX + appName;

      try
      {
        var url = Metabase.ResolveNetworkServiceToConnectString(metabasePath,
                  SysConsts.NETWORK_INTERNOC,
                  svc);

        return new Redirect(url);
      }
      catch
      {
        WorkContext.Response.StatusCode = WebConsts.STATUS_404;
        WorkContext.Response.StatusDescription = WebConsts.STATUS_404_DESCRIPTION;
        return "Web Manager service in application '{0}' on host '{1}' netsvc '{2}' is not available".Args(appName, metabasePath, svc);
      }
    }

    [Action]
    public object LoadLevel(string path, bool hosts = false)
    {
      return LoadLevelImpl(path, hosts);
    }

    internal object LoadLevelImpl(string path, bool hosts = false)
    {
      IEnumerable<Metabank.SectionRegionBase> children = null;

      Metabank.SectionRegionBase section = null;
      if (path.IsNotNullOrWhiteSpace() && path != "/" && path != "\\")
      {
        section = Metabase.CatalogReg[path];
        if (section == null) return Wave.SysConsts.JSON_RESULT_ERROR;

        if (section is Metabank.SectionRegion)
        {
          var region = (Metabank.SectionRegion)section;
          children = region.SubRegionNames.OrderBy(r => r).Select(r => (Metabank.SectionRegionBase)region.GetSubRegion(r))
            .Concat(region.NOCNames.OrderBy(n => n).Select(n => (Metabank.SectionRegionBase)region.GetNOC(n)));
        }
        else if (section is Metabank.SectionNOC)
        {
          var noc = (Metabank.SectionNOC)section;
          children = noc.ZoneNames.OrderBy(z => z).Select(z => noc.GetZone(z));
        }
        else if (section is Metabank.SectionZone)
        {
          var zone = (Metabank.SectionZone)section;
          children = zone.SubZoneNames.OrderBy(z => z).Select(z => (Metabank.SectionRegionBase)zone.GetSubZone(z));
          if (hosts)
            children = children.Concat(zone.HostNames.OrderBy(h => h).Select(h => (Metabank.SectionRegionBase)zone.GetHost(h)));
        }
        else
          return Azos.Wave.SysConsts.JSON_RESULT_ERROR;
      }
      else
      {
        children = Metabase.CatalogReg.Regions;
      }


      var shost = App.GetThisHostMetabaseSection();

      return new
      {
        OK=true,
        path=path,
        myPath=shost.RegionPath,
        myPathSegs=shost.SectionsOnPath.Select(s => s.Name).ToArray(),
        children=makeChildren(children)
      };
    }

    internal object makeChildren(IEnumerable<Metabank.SectionRegionBase> children)
    {
      var res = new List<JSONDataMap>();

      foreach (var child in children)
      {
        var d = new JSONDataMap();

        d["name"] = child.Name;
        d["path"] = child.RegionPath;
        d["me"] = child.IsLogicallyTheSame(App.GetThisHostMetabaseSection());
        d["tp"] = child.SectionMnemonicType;
        d["descr"] = child.Description;

        var host = child as Metabank.SectionHost;
        if (host != null)
        {
          var isZGov = host.IsZGov;
          d["role"] = host.RoleName;
          d["dynamic"] = host.Dynamic;
          d["os"] = host.OS;
          d["apps"] = host.Role.AppNames.OrderBy(a => a).ToArray();
          d["isZGov"] = isZGov;
          d["myZGov"] = child.IsLogicallyTheSame(host.ParentZoneGovernorPrimaryHost());

          string adminURL = null;
          try
          {
            adminURL = Metabase.ResolveNetworkServiceToConnectString(host.RegionPath,
              SysConsts.NETWORK_INTERNOC,
              isZGov ? SysConsts.NETWORK_SVC_ZGOV_WEB_MANAGER : SysConsts.NETWORK_SVC_HGOV_WEB_MANAGER);
          }
          catch(Exception ex)
          {
            log(MessageType.Error, "LoadLevel.makeLevel()", ex.ToMessageWithType(), ex);
          }

          d["adminURL"] = adminURL;
        }
        else
        {
          d["geo"] = new { lat = child.EffectiveGeoCenter.Lat, lng = child.EffectiveGeoCenter.Lng };
        }

        res.Add(d);
      }

      return res;
    }

    private void log(MessageType tp, string from, string text, Exception error = null)
    {
      App.Log.Write(new Message
      {
        Type = tp,
        Topic = SysConsts.LOG_TOPIC_APP_MANAGEMENT,
        From = "{0}.{1}".Args(typeof(TheSystem).FullName, from),
        Text = text,
        Exception = error
      });
    }

  }
}
