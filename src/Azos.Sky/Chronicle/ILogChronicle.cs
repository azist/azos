/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Log;

namespace Azos.Sky.Chronicle
{

  /// <summary>
  /// Outlines a high-level contract for working with log message archives (chronicles)
  /// </summary>
  public interface ILogChronicle
  {
    /// <summary>
    /// Writes an enumerable of messages into chronicle
    /// </summary>
    Task WriteAsync(LogBatch data);

    /// <summary>
    /// Gets chronicle (a list) of messages satisfying the supplied LogChronicleFilter object
    /// </summary>
    Task<IEnumerable<Message>> GetAsync(LogChronicleFilter filter);

    //20230515 DKh #861
    /// <summary>
    /// Gets chronicle (a list) of facts extracted from log messages on the server satisfying
    /// the supplied LogChronicleFilter object
    /// </summary>
    Task<IEnumerable<Fact>> GetFactsAsync(LogChronicleFactFilter filter);
  }

  /// <summary>
  /// Outlines a contract for implementing logic of ILogChronicle
  /// </summary>
  public interface ILogChronicleLogic : ILogChronicle, IModule
  {
  }

}
