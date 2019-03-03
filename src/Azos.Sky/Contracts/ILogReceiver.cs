/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Reflection;
using System.Collections.Generic;

using Azos.Glue;
using Azos.Glue.Protocol;
using Azos.Log;

namespace Azos.Sky.Contracts
{

  /// <summary>
  /// Implemented by ILogReceiver, receive log data.
  /// This contract is singleton for efficiency
  /// </summary>
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface ILogReceiver : ISkyService
  {
    [OneWay]
    [ArgsMarshalling(typeof(RequestMsg_ILogReceiver_SendLog))]
    void SendLog(Message data);

    Message GetByID(Guid id, Atom channel);

    IEnumerable<Message> List(Atom channel, string archiveDimensionsFilter, DateTime startDate, DateTime endDate, MessageType? type = null,
      string host = null, string topic = null,
      Guid? relatedTo = null,
      int skipCount = 0);
  }

  /// <summary>
  /// Contract for client of ILogReceiver svc
  /// </summary>
  public interface ILogReceiverClient : ISkyServiceClient, ILogReceiver
  {
    CallSlot Async_SendLog(Message data);
    CallSlot Async_GetByID(Guid id, Atom channel);
    CallSlot Async_List(Atom channel, string archiveDimensionsFilter, DateTime startDate, DateTime endDate, MessageType? type = null,
      string host = null, string topic = null,
      Guid? relatedTo = null,
      int skipCount = 0);
  }

  public sealed class RequestMsg_ILogReceiver_SendLog : RequestMsg
  {
    public RequestMsg_ILogReceiver_SendLog(MethodInfo method, Guid? instance) : base(method, instance) { }
    public RequestMsg_ILogReceiver_SendLog(TypeSpec contract, MethodSpec method, bool oneWay, Guid? instance) : base(contract, method, oneWay, instance) { }

    public Message MethodArg_0_data;
  }
}