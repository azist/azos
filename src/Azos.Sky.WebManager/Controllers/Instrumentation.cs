using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Azos.Apps;
using Azos.Data;
using Azos.Collections;
using Azos.Instrumentation;
using Azos.Log;
using Azos.Serialization.JSON;
using Azos.Web;
using Azos.Wave.Mvc;

using Azos.Sky.Apps;
using Azos.Sky.Apps.ZoneGovernor;
using Azos.Sky.Metabase;

namespace Azos.Sky.WebManager.Controllers
{
  /// <summary>
  /// Instrumentation controller
  /// </summary>
  public class Instrumentation: WebManagerController
  {
    #region Consts

      private const int MAX_COUNT = 100 * 1024;
      private const int MIN_PROCESSING_INTERVAL_MS = 3000;

    #endregion

    #region Nested classes
      public class DatumRequest : TypedDoc
      {
        [Field] public NSDescr[] Namespaces { get; set; }
        [Field] public DateTime ToUTC { get; set; }
        public class NSDescr: TypedDoc   {   [Field] public string NS { get; set; }  [Field] public SRCDescr[] Sources { get; set; }      }
        public class SRCDescr : TypedDoc {   [Field] public string SRC{ get; set; }  [Field] public DateTime FromUTC { get; set; }        }
      }
    #endregion

    /// <summary>
    /// Navigates/redirects request to governor process (hgov/zgov) at host with metabase path "metabasePath"
    /// </summary>
    [Action]
    public object NavigateToHost(string metabasePath)
    {
      var host = SkySystem.Metabase.CatalogReg[metabasePath] as Metabank.SectionHost;
      if (host != null)
      {
        string hostURL;
        try
        {
          hostURL = SkySystem.Metabase.ResolveNetworkServiceToConnectString(
                                metabasePath,
                                SysConsts.NETWORK_INTERNOC,
                                host.IsZGov ? SysConsts.NETWORK_SVC_ZGOV_WEB_MANAGER : SysConsts.NETWORK_SVC_HGOV_WEB_MANAGER);
          return new Redirect(hostURL);
        }
        catch (Exception ex)
        {
          log(MessageType.Error, ".NavigateToHost(metabasePath='{0}')".Args(metabasePath), ex.ToMessageWithType(), ex);
        }
      }

      WorkContext.Response.StatusCode = WebConsts.STATUS_404;
      WorkContext.Response.StatusDescription = WebConsts.STATUS_404_DESCRIPTION;
      return "Couldn't navigate to host/zone '{0}'".Args(metabasePath);
    }

    [Action]
    public object LoadLogMessages(DateTime? from = null, int? maxCnt = null, MessageType? fromType = null, bool forZone = false)
    {
      //var buf = new Message[] {
      //  new Message() { Type = MessageType.Debug, Text="test111111111111111111", TimeStamp=DateTime.Now},
      //  new Message() { Type = MessageType.Trace, Text="test111111111111111111"},
      //  new Message() { Type = MessageType.Info, Text="test111111111111111111"},
      //  new Message() { Type = MessageType.Warning, Text="test111111111111111111"},
      //  new Message() { Type = MessageType.Error, Text="test111111111111111111"},
      //  new Message() { Type = MessageType.Emergency, Text="test111111111111111111"}
      //};

      var log = App.Log as LogDaemonBase;
      if (log == null) return Azos.Wave.SysConsts.JSON_RESULT_ERROR;

      from = (from == null) ? log.LocalizedTime.AddSeconds(-600) : log.UniversalTimeToLocalizedTime(from.Value.ToUniversalTime());

      IEnumerable<Message> buf;
      if (forZone &&
          SkySystem.SystemApplicationType == SystemApplicationType.ZoneGovernor &&
          ZoneGovernorService.IsZoneGovernor)
        buf = ZoneGovernorService.Instance.GetSubordinateInstrumentationLogBuffer(true);
      else
        buf = log.GetInstrumentationBuffer(true);

      if (fromType.HasValue) buf = buf.Where(m => m.Type >= fromType);

      buf = buf.Where(m => m.TimeStamp >= from);

      if (!maxCnt.HasValue || maxCnt > MAX_COUNT || maxCnt < 0) maxCnt = MAX_COUNT;

      buf = buf.Take(maxCnt.Value);

      var msgObjects = buf.Select(m => new
      {
        id = m.Guid.ToString(),
        rid = (m.RelatedTo != Guid.Empty) ? m.RelatedTo.ToString() : null,
        host = m.Host,
        ts = m.TimeStamp,
        tp = m.Type,
        topic = m.Topic,
        from = m.From,
        src = m.Source,
        txt = m.Text
      });

      var to = log.LocalizedTimeToUniversalTime(log.LocalizedTime);

      return new
      {
        OK = true,
        fromType = fromType,
        bufSize = log.InstrumentationBufferSize,
        to = to,
        buf = msgObjects.ToArray()
      };
    }

    [Action]
    public object SetComponentParameter(ulong sid, string key, string val)
    {
      var cmp = ApplicationComponent.GetAppComponentBySID(sid);
      if (cmp == null) return new { OK = false, err = "Component (SID={0}) couldn't be found".Args(sid)};

      var parameterized = cmp as IExternallyParameterized;
      if (parameterized == null) return new { OK = false, err = "Component (SID={0}) doesn't implement IExternallyParameterized".Args(sid) };

      if (!parameterized.ExternalSetParameter(key, val))
        return new
        {
          OK = false,
          err = "Parameter \"{0}\" of component (SID={1}) couldn't be set to value \"{2}\"".Args(key, sid, val)
        };

      object setVal;
      parameterized.ExternalGetParameter(key, out setVal);

      var valStr = setVal.AsString();
      if (valStr.IsNotNullOrWhiteSpace() && valStr.StartsWith(Azos.CoreConsts.EXT_PARAM_CONTENT_LACONIC))
      {
        valStr = valStr.Substring(Azos.CoreConsts.EXT_PARAM_CONTENT_LACONIC.Length);
        return new { OK = true, val = valStr.AsLaconicConfig().ToJSONDataMap()};
      }
      else
      {
        return new { OK = true, val = setVal};
      }
    }

    [Action]
    public object LoadComponentParamGroups()
    {
      var groups =
        ApplicationComponent.AllComponents
          .OfType<IExternallyParameterized>()
          .SelectMany(c => (c.ExternalParameters ?? Enumerable.Empty<KeyValuePair<string, Type>>())
                           .SelectMany(p =>
                             {
                              var tcomp = c.GetType();
                              var prop = tcomp.GetProperty(p.Key);
                              if (prop!=null)
                               return prop.GetCustomAttributes<ExternalParameterAttribute>();
                              else
                               return Enumerable.Empty<ExternalParameterAttribute>();
                             }
                            ))
          .SelectMany(a => a.Groups ?? Enumerable.Empty<string>())
          .Distinct()
          .OrderBy(g => g)
          .ToArray();

      return new { OK = true, groups = groups };
    }

    [Action]
    public object LoadComponentTree(string group = null)
    {
      var res = new JSONDataMap();

      var all = ApplicationComponent.AllComponents;

      var rootArr = new JSONDataArray();

      foreach(var cmp in all.Where(c => c.ComponentDirector==null))
        rootArr.Add(getComponentTreeMap(all, cmp, 0, group));

      res["root"] = rootArr;

      var otherArr = new JSONDataArray();

      foreach(var cmp in all.Where(c => c.ComponentDirector!=null && !(c is ApplicationComponent)))
        rootArr.Add(getComponentTreeMap(all, cmp, 0));

      res["other"] = otherArr;

      return new {OK=true, tree=res};
    }

    private JSONDataMap getComponentTreeMap(IEnumerable<ApplicationComponent> all, ApplicationComponent cmp, int level, string group = null)
    {
      var cmpTreeMap = new JSONDataMap();

      if (level>7) return cmpTreeMap;//cyclical ref

      cmpTreeMap = getComponentMap(cmp, group);

      var children = new JSONDataArray();

      foreach (var child in all.Where(c => object.ReferenceEquals(cmp, c.ComponentDirector)))
      {
        var childMap = getComponentTreeMap(all, child, level + 1, group);
        children.Add(childMap);
      }

      if (children.Count() > 0) cmpTreeMap["children"] = children;

      return cmpTreeMap;
    }

    private JSONDataMap getComponentMap(ApplicationComponent cmp, string group = null)
    {
      var cmpMap = new JSONDataMap();

      var parameterized = cmp as IExternallyParameterized;
      var instrumentable = cmp as IInstrumentable;

      cmpMap["instrumentable"] = instrumentable != null;
      cmpMap["instrumentationEnabled"] = instrumentable != null ? instrumentable.InstrumentationEnabled : false;
      cmpMap["SID"] = cmp.ComponentSID;
      cmpMap["startTime"] = Sky.Apps.Terminal.Cmdlets.Appl.DetailedComponentDateTime( cmp.ComponentStartTime );
      cmpMap["tp"] = cmp.GetType().FullName;
      if (cmp.ComponentCommonName.IsNotNullOrWhiteSpace()) cmpMap["commonName"] = cmp.ComponentCommonName;
      if (cmp is INamed) cmpMap["name"] = ((INamed)cmp).Name;
      if (cmp.ComponentDirector != null) cmpMap["director"] = cmp.ComponentDirector.GetType().FullName;

      if (parameterized == null) return cmpMap;

      var pars = group.IsNotNullOrWhiteSpace() ? parameterized.ExternalParametersForGroups(group) : parameterized.ExternalParameters;
      if (pars == null) return cmpMap;

      pars = pars.Where(p => p.Key != "InstrumentationEnabled").OrderBy(p => p.Key);
      if (pars.Count() == 0) return cmpMap;

      var parameters = new List<JSONDataMap>();

      foreach(var par in pars)
      {
        object val;
        if (!parameterized.ExternalGetParameter(par.Key, out val)) continue;

        var parameterMap = new JSONDataMap();

        string[] plist = null;
        var tp = par.Value;
        if (tp == typeof(bool)) plist = new string[] { "true", "false" };
        else if (tp.IsEnum) plist = Enum.GetNames(tp);

        parameterMap["key"] = par.Key;
        parameterMap["plist"] = plist;

        var valStr = val.AsString();
        if (valStr.IsNotNullOrWhiteSpace() && valStr.StartsWith(CoreConsts.EXT_PARAM_CONTENT_LACONIC))
        {
          valStr = valStr.Substring(Azos.CoreConsts.EXT_PARAM_CONTENT_LACONIC.Length);
          parameterMap["val"] = valStr.AsLaconicConfig().ToJSONDataMap();
//          parameterMap["val"] = @" {
//                                  'detailed-instrumentation': true,
//                                  tables:
//                                  {
//                                    master: { name: 'tfactory', 'fields-qty': 14},
//                                    slave: { name: 'tdoor', 'fields-qty': 20, important: true}
//                                  },
//                                  tables1:
//                                  {
//                                    master: { name: 'tfactory', 'fields-qty': 14},
//                                    slave: { name: 'tdoor', 'fields-qty': 20, important: true}
//                                  },
//                                  tables2:
//                                  {
//                                    master: { name: 'tfactory', 'fields-qty': 14},
//                                    slave: { name: 'tdoor', 'fields-qty': 20, important: true}
//                                  }
//                                }".JSONToDataObject();
          parameterMap["ct"] = "obj"; // content type is object (string in JSON format)
        }
        else
        {
          parameterMap["val"] = val;
          parameterMap["ct"] = "reg"; // content type is regular
        }

        parameters.Add(parameterMap);
      }

      cmpMap["params"] = parameters;

      return cmpMap;
    }


    [Action]
    public object LoadComponents(string group = null)
    {
      var components = ApplicationComponent.AllComponents.OrderBy(c => c.ComponentStartTime);

      var componentsList = new List<JSONDataMap>();

      foreach (var cmp in components)
      {
        var componentMap = new JSONDataMap();
        componentsList.Add(componentMap);

        var parameterized = cmp as IExternallyParameterized;
        var instrumentable = cmp as IInstrumentable;

        componentMap["instrumentable"] = instrumentable != null;
        componentMap["instrumentationEnabled"] = instrumentable != null ? instrumentable.InstrumentationEnabled : false;
        componentMap["SID"] = cmp.ComponentSID;
        componentMap["startTime"] = Apps.Terminal.Cmdlets.Appl.DetailedComponentDateTime( cmp.ComponentStartTime );
        componentMap["tp"] = cmp.GetType().FullName;
        if (cmp.ComponentCommonName.IsNotNullOrWhiteSpace()) componentMap["commonName"] = cmp.ComponentCommonName;
        if (cmp is INamed) componentMap["name"] = ((INamed)cmp).Name;
        if (cmp.ComponentDirector != null) componentMap["director"] = cmp.ComponentDirector.GetType().FullName;

        if (parameterized == null) continue;

        var pars = group.IsNotNullOrWhiteSpace() ? parameterized.ExternalParametersForGroups(group) : parameterized.ExternalParameters;
        if (pars == null) continue;

        pars = pars.Where(p => p.Key != "InstrumentationEnabled").OrderBy(p => p.Key);
        if (pars.Count() == 0) continue;

        var parameters = new List<JSONDataMap>();

        foreach(var par in pars)
        {
          object val;
          if (!parameterized.ExternalGetParameter(par.Key, out val)) continue;

          var parameterMap = new JSONDataMap();

          string[] plist = null;
          var tp = par.Value;
          if (tp == typeof(bool)) plist = new string[] { "true", "false" };
          else if (tp.IsEnum) plist = Enum.GetNames(tp);

          parameterMap["key"] = par.Key;
          parameterMap["plist"] = plist;
          parameterMap["val"] = val;

          parameters.Add(parameterMap);
        }

        componentMap["params"] = parameters;
      }

      return new { OK = true, components = componentsList };
    }

    public enum DataGrouping { NS = 0, Namespace = 0, Namespaces = 0, Name = 0, Names = 0,
                               Intf = 1, Interface = 1, Interfaces = 1, Class=1, Classes=1 }

    [Action]
    public object GetTree(bool forZone = false, DataGrouping grouping = DataGrouping.Namespace)
    {
      var instrumentation = App.Instrumentation;

      if (forZone &&
          SkySystem.SystemApplicationType == SystemApplicationType.ZoneGovernor &&
          ZoneGovernorService.IsZoneGovernor)
        instrumentation = ZoneGovernorService.Instance.SubordinateInstrumentation;

      var data = (grouping == DataGrouping.Namespace) ? getByNamespace(instrumentation) : getByInterface(instrumentation);

      var procInterval = instrumentation.ProcessingIntervalMS;
      if (procInterval < MIN_PROCESSING_INTERVAL_MS) procInterval = MIN_PROCESSING_INTERVAL_MS;

      return new {
        OK = true,
        grouping = grouping,
        enabled = instrumentation.Enabled,
        recordCount = instrumentation.RecordCount,
        maxRecordCount = instrumentation.MaxRecordCount,
        tree = data,
        processingIntervalMS = procInterval
      };
    }

    [Action("GetData", 0, "match{methods=POST}")]
    public object GetData(DatumRequest request, bool forZone = false)
    {
      var instrumentation = App.Instrumentation;

      if (forZone &&
          SkySystem.SystemApplicationType == SystemApplicationType.ZoneGovernor &&
          ZoneGovernorService.IsZoneGovernor)
        instrumentation = ZoneGovernorService.Instance.SubordinateInstrumentation;

      var nsMap = new JSONDataMap();
      var to = request.ToUTC.ToUniversalTime();

      var total = 0;
      foreach (var ns in request.Namespaces)
      {
        if (total>=MAX_COUNT) break;
        var srcMap = new JSONDataMap();

        foreach (var src in ns.Sources)
        {
          if (total>=MAX_COUNT) break;

          // TIME REVIEW!!! Log stores msg in local format, Instrumentation - in UTC
          //Console.WriteLine("from original: {0} {1}".Args(src.FromUTC, src.FromUTC.Kind));
          //Console.WriteLine("from to utc: {0} {1}".Args(src.FromUTC.ToUniversalTime(), src.FromUTC.ToUniversalTime().Kind));

          ////var from = App.UniversalTimeToLocalizedTime(src.FromUTC.ToUniversalTime());

          //Console.WriteLine("from UniversalTimeToLocalizedTime: {0} {1}".Args(from, from.Kind));
          //Console.WriteLine();

          var from = src.FromUTC.ToUniversalTime();

          var buf = instrumentation.GetBufferedResultsSince(from)
                                      .Where(d => d.GetType().FullName == ns.NS && d.Source == src.SRC)
                                      .Take(MAX_COUNT - total);
          var arr =  buf.ToArray();
          srcMap[src.SRC] = arr;
          total += arr.Length;
        }
        nsMap[ns.NS] = srcMap;
      }

      var procInterval = instrumentation.ProcessingIntervalMS;
      if (procInterval < MIN_PROCESSING_INTERVAL_MS) procInterval = MIN_PROCESSING_INTERVAL_MS;
      return new
      {
        OK = true,
        to = to,
        data = nsMap,
        recordCount = instrumentation.RecordCount,
        maxRecordCount = instrumentation.MaxRecordCount,
        total = total,
        truncated = total > MAX_COUNT,
        processingIntervalMS = procInterval
      };
    }

    private JSONDataMap getByNamespace(IInstrumentation instr)
    {
      var data = new JSONDataMap();

      IEnumerable<Type> typeKeys = instr.DataTypes.OrderBy(t => t.FullName);
      foreach (var tkey in typeKeys)
      {
        Datum datum = null;
        var sourceKeys = instr.GetDatumTypeSources(tkey, out datum).OrderBy(s => s);
        if (datum==null) continue;

        var tData = new JSONDataMap();
        tData["data"] = sourceKeys;

        tData["descr"] = datum.Description;
        tData["unit"] = datum.ValueUnitName;
        tData["error"] = datum is IErrorInstrument;
        tData["gauge"] = datum is Gauge;

        data.Add(tkey.FullName, tData);
      }

      return data;
    }

    private JSONDataMap getByInterface(IInstrumentation instr)
    {
      var data = new JSONDataMap();

      var sortedTypes = instr.DataTypes.OrderBy(t => t.FullName);

      IEnumerable<Type> intfKeys = instr.DataTypes.SelectMany( t=> Datum.GetViewGroupInterfaces(t) ).Distinct().OrderBy(ti => ti.FullName);
      foreach (var ikey in intfKeys)
      {

              var iData = new JSONDataMap();

              IEnumerable<Type> typeKeys = sortedTypes.Where(t => Datum.GetViewGroupInterfaces(t).Any(i => i == ikey));
              foreach (var tkey in typeKeys)
              {
                      Datum datum = null;
                      var sourceKeys = instr.GetDatumTypeSources(tkey, out datum);
                      if (datum==null) continue;

                      var tData = new JSONDataMap();
                      tData["descr"] = datum.Description;
                      tData["unit"] = datum.ValueUnitName;
                      tData["error"] = datum is IErrorInstrument;
                      tData["gauge"] = datum is Gauge;
                      tData["data"] = sourceKeys;

                      iData.Add(tkey.FullName, tData);
              }

        data.Add(ikey.Name, iData);
      }

      return data;
    }

    private static void log(MessageType tp, string from, string text, Exception error = null)
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
