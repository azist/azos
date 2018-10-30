


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Glue;
using NFX.Log;


namespace Azos.Sky.Contracts
{

    /// <summary>
    /// Implemented by ZoneGovernors, receive log data from subordinate nodes (another zone governors or other hosts).
    /// This contract is singleton for efficiency
    /// </summary>
    [Glued]
    [LifeCycle(ServerInstanceMode.Singleton)]
    public interface IZoneLogReceiver : ISkyService
  {
       /// <summary>
       /// Sends log batch from a named subordinate host.
       /// Returns the receiver condition - a number of expected Message instances in the next call.
       /// Keep in mind that a large number may be returned and Glue buffer limit may not be sufficient for large send, so
       /// impose a limit on the caller side (i.e. 200 max message instances per call)
       /// The busier the receiver gets, the lower is the number. This is a form of throttling/flow control
       /// </summary>
       int SendLog(string host, string appName, Message[] data);
    }


    /// <summary>
    /// Contract for client of IZoneLogReceiver svc
    /// </summary>
    public interface IZoneLogReceiverClient : ISkyServiceClient, IZoneLogReceiver {  }


}
