/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azos.Platform;
using Azos.Security;
using Azos.Serialization.JSON;
using Azos.Wave.Mvc;

namespace Azos.Apps.Hosting.Skyod.Web
{
  /// <summary>
  /// Skyod host APIs provided by SkyodDaemon
  /// </summary>
  [SystemAdministratorPermission(AccessLevel.ADVANCED)]
  public sealed class Host : ApiProtocolController
  {

    private SkyodDaemon SkyodDaemon => App.Singletons.Get<SkyodDaemon>().NonNull(nameof(SkyodDaemon));


    [ActionOnGet]
    public object Info(int level = 0)
    {
      var info = getSkyodInfo(SkyodDaemon, level);
      var result = GetLogicResult(info);
      return new JsonResult(result, JsonWritingOptions.PrettyPrintRowsAsMap);//so it is easier for humans
    }

    [ActionOnPost]
    public object Stop()
    {
      ((IApplicationImplementation)App).Stop();
      return GetLogicResult("Stopped");
    }

    [ActionOnPost(Name = "exec")]
    public async Task<object> ExecAsync(string soft, string component, Adapters.AdapterRequest request)
    {
      var cmp = SkyodDaemon.Sets[soft.NonBlank(nameof(soft))].NonNull(nameof(soft))
                           .Components[component.NonBlank(nameof(component))].NonNull(nameof(component));
      var got = await cmp.ExecAdapterRequestAsync(request);
      return GetLogicResult(got);
    }


    private JsonDataMap getSkyodInfo(SkyodDaemon sd, int level)
    {
      var result = new JsonDataMap();

      result["host"] = Computer.HostName;
      result["os"] = Computer.OSFamily;
      result["net-signature"] = Computer.UniqueNetworkSignature;
      result["cmp_sid"] = sd.ComponentSID;
      result["cmp_start"] = sd.ComponentStartTime;
      result["name"] = sd.Name;
      result["status"] = sd.Status;
      result["status_descr"] = sd.StatusDescription;
      result["service_descr"] = sd.ServiceDescription;

      if (level > 0)
      {
        result["cmp_effective_log"] = sd.ComponentEffectiveLogLevel;
        result["software_root_dir"] = sd.SoftwareRootDirectory;
        result["instr"] = sd.InstrumentationEnabled;
        result["app_start_time"] = sd.App.StartTime;
        result["app_utc_now"] = sd.App.TimeSource.UTCNow;
        result["app_id"] = sd.App.AppId;
        result["app_instance_id"] = sd.App.InstanceId;
        result["app_cloud_origin"] = sd.App.CloudOrigin;
        result["app_descr"] = sd.App.Description;
        result["app_environment"] = sd.App.EnvironmentName;
      }

      if (level < 2) return result;
      //============================

      var software = new List<JsonDataMap>();
      result["software"] = software;
      foreach(var soft in sd.Sets)
      {
        var mapSoft = new JsonDataMap();
        software.Add(mapSoft);
        mapSoft["name"] = soft.Name;
        var components = new List<JsonDataMap>();
        mapSoft["components"] = components;
        foreach (var cmp in soft.Components)
        {
          var mapCmp = new JsonDataMap();
          components.Add(mapCmp);
          mapCmp["name"] = cmp.Name;
          mapCmp["order"] = cmp.Order;
          mapCmp["is_local"] = cmp.IsLocal;

          var subordinates = new List<JsonDataMap>();
          mapCmp["subordinate_hosts"] = subordinates;
          foreach(var host in cmp.SubordinateHosts)
          {
            var mapHost = new JsonDataMap();
            subordinates.Add(mapHost);
            mapHost["name"] = host.Name;
            mapHost["order"] = host.Order;
            mapHost["causes_panic"] = host.CausesPanic;
            mapHost["skyod_root_uri"] = host.SkyodRootUri;
            mapHost["ping_interval_sec"] = host.PingIntervalSec;
          }
        }
      }

      return result;
    }
  }
}
