/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Glue;
using Azos.Instrumentation;

namespace Azos.Sky.Contracts
{

    /// <summary>
    /// Implemented by ZoneGovernors, receive telemetry data from subordinate nodes (another zone governors or other hosts).
    /// This contract is singleton for efficiency
    /// </summary>
    [Glued]
    [LifeCycle(ServerInstanceMode.Singleton)]
    public interface IZoneTelemetryReceiver : ISkyService
  {
       /// <summary>
       /// Sends telemetry batch from named subordinate host.
       /// Returns the receiver condition - a number of expected Datum instances in the next call.
       /// Keep in mind that a large number may be returned and Glue buffer limit may not be sufficient for large send, so
       /// impose a limit on the caller side (i.e. 200 max datum instances per call)
       /// The busier the receiver gets, the lower is the number. This is a form of throttling/flow control
       /// </summary>
       int SendTelemetry(string host, Datum[] data);
    }


    /// <summary>
    /// Contract for client of IZoneTelemetryReceiver svc
    /// </summary>
    public interface IZoneTelemetryReceiverClient : ISkyServiceClient, IZoneTelemetryReceiver {  }


}
