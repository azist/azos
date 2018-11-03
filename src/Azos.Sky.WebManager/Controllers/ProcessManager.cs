using System;
using System.Linq;

using Azos;
using Azos.Data;
using Azos.Serialization.JSON;
using Azos.Wave.Mvc;
using Azos.Sky.Workers;

namespace Azos.Sky.WebManager.Controllers
{

  /// <summary>
  /// Proce management controller
  /// </summary>
  public sealed class ProcessManager : WebManagerController
  {

    [Action]
    public object List(string zone, string signal, DateTime? startDate, DateTime? endDate, string description)
    {
      try
      {
        ProcessStatus? status = null;
        if ("Created".EqualsOrdIgnoreCase(signal)) status = ProcessStatus.Created;
        else if ("Started".EqualsOrdIgnoreCase(signal)) status = ProcessStatus.Started;
        else if ("Finished".EqualsOrdIgnoreCase(signal)) status = ProcessStatus.Finished;
        else if ("Canceled".EqualsOrdIgnoreCase(signal)) status = ProcessStatus.Canceled;
        else if ("Terminated".EqualsOrdIgnoreCase(signal)) status = ProcessStatus.Terminated;
        else if ("Failed".EqualsOrdIgnoreCase(signal)) status = ProcessStatus.Failed;

        var sd = startDate != null ?  new DateTime(startDate.Value.Year, startDate.Value.Month, startDate.Value.Day, 0, 0, 0) : DateTime.MinValue;
        var ed = endDate != null ?  new DateTime(endDate.Value.Year, endDate.Value.Month, endDate.Value.Day, 23, 59, 59) : DateTime.MaxValue;

        var result = SkySystem.ProcessManager
          .List(zone)
          .Where(descriptor => (signal.IsNullOrEmpty() || signal.EqualsOrdIgnoreCase("ALL") || descriptor.Status.Equals(status) ) &&
                               ( sd <= descriptor.Timestamp && descriptor.Timestamp <= ed ) &&
                               (description.IsNullOrWhiteSpace() || descriptor.Description.ToUpper().Contains(description.ToUpper()) || descriptor.StatusDescription.ToUpper().Contains(description.ToUpper()))
                               )
          .ToJSON();
        return new {Status = "OK", Result = result};
      }
      catch (Exception ex)
      {
        return new {Status = "Error", Exception = ex.Message};
      }
    }

    [Action("SendCancel", 0, "match { methods=POST accept-json=true}")]
    public object SendCancel(JSONDataMap map)
    {
      var pid = new PID(map);
      var cancel = Signal.MakeNew<CancelSignal>(pid);
      var result = SkySystem.ProcessManager.Dispatch(cancel);
      return new {Status = result.GetType().Name};
    }

    [Action("SendTerminate", 0, "match { methods=POST accept-json=true}")]
    public object SendTerminate(JSONDataMap map)
    {
      var pid = new PID(map);
      var terminate = Signal.MakeNew<TerminateSignal>(pid);
      var result = SkySystem.ProcessManager.Dispatch(terminate);
      return new {Status = result.GetType().Name};
    }

    [Action("SendSignal", 0, "match { methods=POST accept-json=true}")]
    public object SendSignal(JSONDataMap map)
    {
      try
      {
        var _pid = map["pid"] as JSONDataMap;
        var _signal = map["signal"];
        var pid = new PID(_pid);
        var conf = _signal.AsLaconicConfig();
        var result = SkySystem.ProcessManager.Dispatch(pid, conf);
        return new {Status = result.GetType().Name};
      }
      catch (Exception ex)
      {
        return new {Status = "Error", Exception = ex.Message};
      }
    }

    [Action("GetDetails", 0, "match { methods=POST accept-json=true}")]
    public object GetDetails(JSONDataMap pidMap)
    {
      var _pid = pidMap["pid"] as JSONDataMap;
      var pid = new PID(_pid);
      var result = SkySystem.ProcessManager.Get<Process>(pid);
      return result;
    }

  }


}