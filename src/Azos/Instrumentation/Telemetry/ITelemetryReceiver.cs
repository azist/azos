/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Glue;

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
