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

using Azos.Security;
using Azos.Serialization.JSON;
using Azos.Wave.Mvc;

namespace Azos.Apps.Hosting.Web
{
  /// <summary>
  /// Host Governor APIs
  /// </summary>
  [SystemAdministratorPermission(AccessLevel.ADVANCED)]
  public class HGov : ApiProtocolController
  {
    [ActionOnGet]
    public object Info(int level = 0)
    {
      var gov = App.Singletons.Get<GovernorDaemon>().NonNull(nameof(GovernorDaemon));
      var info = getHGovInfo(gov, level);
      var result = GetLogicResult(info);
      return new JsonResult(result, JsonWritingOptions.PrettyPrintRowsAsMap);//so it is easier for humans
    }

    [ActionOnPost]
    public object Stop()
    {
      ((IApplicationImplementation)App).Stop();
      return GetLogicResult("Stopped");
    }

    private JsonDataMap getHGovInfo(GovernorDaemon gov, int level)
    {
      var result = new JsonDataMap();

      result["cmp_sid"] = gov.ComponentSID;
      result["cmp_start"] = gov.ComponentStartTime;
      result["name"] = gov.Name;
      result["status"] = gov.Status;
      result["status_descr"] = gov.StatusDescription;
      result["service_descr"] = gov.ServiceDescription;

      if (level > 0)
      {
        result["cmp_effective_log"] = gov.ComponentEffectiveLogLevel;
        result["sipc_port"] = gov.AssignedSipcServerPort;
        result["sipc_start_port"] = gov.ServerStartPort;
        result["sipc_start_port"] = gov.ServerEndPort;
        result["instr"] = gov.InstrumentationEnabled;
      }

      if (level > 1)
      {
        result["app_start_time"] = gov.App.StartTime;
        result["app_utc_now"] = gov.App.TimeSource.UTCNow;
        result["app_id"] = gov.App.AppId;
        result["app_instance_id"] = gov.App.InstanceId;
        result["app_cloud_origin"] = gov.App.CloudOrigin;
        result["app_descr"] = gov.App.Description;
        result["app_environment"] = gov.App.EnvironmentName;
      }

      var apps = new List<JsonDataMap>();


      result["apps"] = apps;
      foreach(var one in gov.Applications)
      {
        var map = new JsonDataMap();
        map["name"] = one.Name;
        map["order"] = one.Order;
        map["sid"] = one.ComponentSID;
        map["status_descr"] = one.StatusDescription;
        map["service_descr"] = one.ServiceDescription;
        map["last_start"] = one.LastStartAttemptUtc;
        map["failed"] = one.Failed;
        map["fail_utc"] = one.FailUtc;
        map["fail_reason"] = one.FailReason;
        map["optional"] = one.Optional;

        if (level > 0 && one.Connection != null)
        {
          map["conn_state"] = one.Connection.State;
          map["conn_name"] = one.Connection.Name;
          map["conn_start_utc"] = one.Connection.StartUtc;
          map["conn_last_recv_utc"] = one.Connection.LastReceiveUtc;
          map["conn_last_send_utc"] = one.Connection.LastSendUtc;
        }

        if (level > 1)
        {
          map["activation_start_section"] = one.StartSection;
          map["activation_stop_section"] = one.StopSection;
        }

        apps.Add(map);
      }

      return result;
    }
  }
}
