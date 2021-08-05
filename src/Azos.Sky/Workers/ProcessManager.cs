/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;

using Azos.Sky.Coordination;

namespace Azos.Sky.Workers
{
  public sealed class ProcessManager : ProcessManagerBase
  {
    public ProcessManager(IApplication app) : base(app)
    {
    }

    protected override PID DoAllocate(string zonePath, string id, bool isUnique)
    {
      var zone = App.Metabase.CatalogReg.NavigateZone(zonePath);
      var processorID = zone.MapShardingKeyToProcessorID(id);

      return new PID(zone.RegionPath, processorID, id, isUnique);
    }

    protected override void DoSpawn<TProcess>(TProcess process)
    {
      var pid = process.SysPID;
      var zone = App.Metabase.CatalogReg.NavigateZone(pid.Zone);
      var hosts = zone.GetProcessorHostsByID(pid.ProcessorID);

      App.GetServiceClientHub().CallWithRetry<Contracts.IProcessControllerClient>
      (
        (controller) => controller.Spawn(new ProcessFrame(process)),
        hosts.Select(h => h.RegionPath)
      );
    }

    protected override Task Async_DoSpawn<TProcess>(TProcess process)
    {
      var pid = process.SysPID;
      var zone = App.Metabase.CatalogReg.NavigateZone(pid.Zone);
      var hosts = zone.GetProcessorHostsByID(pid.ProcessorID);

      return App.GetServiceClientHub().CallWithRetryAsync<Contracts.IProcessControllerClient>
      (
        (controller) => controller.Async_Spawn(new ProcessFrame(process)).AsTaskReturningVoid(),
        hosts.Select(h => h.RegionPath)
      );
    }

    protected override ResultSignal DoDispatch<TSignal>(TSignal signal)
    {
      var pid = signal.SysPID;
      var zone = App.Metabase.CatalogReg.NavigateZone(pid.Zone);
      var hosts = zone.GetProcessorHostsByID(pid.ProcessorID);

      return App.GetServiceClientHub().CallWithRetry<Contracts.IProcessControllerClient, SignalFrame>
      (
        (controller) => controller.Dispatch(new SignalFrame(signal)),
        hosts.Select(h => h.RegionPath)
      ).Materialize(SignalTypeResolver) as ResultSignal;
    }

    protected override Task<ResultSignal> Async_DoDispatch<TSignal>(TSignal signal)
    {
      var pid = signal.SysPID;
      var zone = App.Metabase.CatalogReg.NavigateZone(pid.Zone);
      var hosts = zone.GetProcessorHostsByID(pid.ProcessorID);

      return App.GetServiceClientHub().CallWithRetryAsync<Contracts.IProcessControllerClient, SignalFrame>
      (
        (controller) => controller.Async_Dispatch(new SignalFrame(signal)).AsTaskReturning<SignalFrame>(),
        hosts.Select(h => h.RegionPath)
      ).ContinueWith((antecedent) => antecedent.Result.Materialize(SignalTypeResolver) as ResultSignal);
    }

    protected override int DoEnqueue<TTodo>(IEnumerable<TTodo> todos, HostSet hs, string svcName)
    {
      var hostPair = hs.AssignHost(new ShardKey(todos.First().SysShardingKey));
      return App.GetServiceClientHub().CallWithRetry<Contracts.ITodoQueueClient, int>
      (
        (client) => client.Enqueue(todos.Select(t => new TodoFrame(t)).ToArray()),
        hostPair.Select(host => host.RegionPath),
        svcName: svcName
      );
    }

    protected override Task<int> Async_DoEnqueue<TTodo>(IEnumerable<TTodo> todos, HostSet hs, string svcName)
    {
      var hostPair = hs.AssignHost(new ShardKey(todos.First().SysShardingKey));
      return App.GetServiceClientHub().CallWithRetryAsync<Contracts.ITodoQueueClient, int>
      (
        (client) => client.Async_Enqueue(todos.Select(t => new TodoFrame(t)).ToArray()).AsTaskReturning<int>(),
        hostPair.Select(host => host.RegionPath),
        svcName: svcName
      );
    }

    protected override TProcess DoGet<TProcess>(PID pid)
    {
      var zone = App.Metabase.CatalogReg.NavigateZone(pid.Zone);
      var hosts = zone.GetProcessorHostsByID(pid.ProcessorID);

      var processFrame = App.GetServiceClientHub().CallWithRetry<Contracts.IProcessControllerClient, ProcessFrame>
      (
        (controller) => controller.Get(pid),
        hosts.Select(h => h.RegionPath)
      );

      // TODO Check type
      return processFrame.Materialize(ProcessTypeResolver) as TProcess;
    }

    protected override ProcessDescriptor DoGetDescriptor(PID pid)
    {
      var zone = App.Metabase.CatalogReg.NavigateZone(pid.Zone);
      var hosts = zone.GetProcessorHostsByID(pid.ProcessorID);

      return App.GetServiceClientHub().CallWithRetry<Contracts.IProcessControllerClient, ProcessDescriptor>
      (
        (controller) => controller.GetDescriptor(pid),
        hosts.Select(h => h.RegionPath)
      );
    }

    protected override IEnumerable<ProcessDescriptor> DoList(string zonePath, IConfigSectionNode filter)
    {
      var tasks = new List<Task<IEnumerable<ProcessDescriptor>>>();

      var zone = App.Metabase.CatalogReg.NavigateZone(zonePath);
      foreach (var processorID in zone.ProcessorMap.Keys)
      {
        var hosts = zone.GetProcessorHostsByID(processorID);
        var descriptors = App.GetServiceClientHub().CallWithRetry<Contracts.IProcessControllerClient, IEnumerable<ProcessDescriptor>>
        (
          (controller) => controller.List(processorID),
          hosts.Select(h => h.RegionPath)
        );

        foreach (var descriptor in descriptors)
          yield return descriptor;
      }
    }
  }
}
