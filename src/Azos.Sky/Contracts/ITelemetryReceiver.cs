using System;
using System.Reflection;

using Azos.Glue;
using Azos.Glue.Protocol;
using Azos.Instrumentation;

namespace Azos.Sky.Contracts
{
  /// <summary>
  /// Implemented by ITelemetryReceiver, receive datum data.
  /// This contract is singleton for efficiency
  /// </summary>
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface ITelemetryReceiver : ISkyService
  {
    [OneWay]
    [ArgsMarshalling(typeof(RequestMsg_ITelemetryReceiver_SendDatums))]
    void SendDatums(params Datum[] data);
  }

  /// <summary>
  /// Contract for client of ITelemetryReceiver svc
  /// </summary>
  public interface ITelemetryReceiverClient : ISkyServiceClient, ITelemetryReceiver
  {
    CallSlot Async_SendDatums(params Datum[] data);
  }

  public sealed class RequestMsg_ITelemetryReceiver_SendDatums : RequestMsg
  {
    public RequestMsg_ITelemetryReceiver_SendDatums(MethodInfo method, Guid? instance) : base(method, instance) { }
    public RequestMsg_ITelemetryReceiver_SendDatums(TypeSpec contract, MethodSpec method, bool oneWay, Guid? instance) : base(contract, method, oneWay, instance) { }

    public Datum[] MethodArg_0_data;
  }
}