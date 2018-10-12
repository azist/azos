
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Glue;
using Azos.Security;

namespace Azos.Instrumentation.Telemetry
{
    /// <summary>
    /// Represents a contract for working with remote receiver of telemetry information
    /// </summary>
    [Glued]
    [LifeCycle(Mode = ServerInstanceMode.Singleton)]
    public interface ITelemetryReceiver
    {
        /// <summary>
        /// Sends data to remote telemetry receiver
        /// </summary>
        /// <param name="siteName">the name/identifier of the reporting site</param>
        /// <param name="data">Telemetry data</param>
        [OneWay] void Send(string siteName, Datum data);
    }
}
