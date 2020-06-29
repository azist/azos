using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Data.Business;
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
    Task WriteAsync(IEnumerable<Message> data);

    /// <summary>
    /// Gets chronicle (a list) of messages satisfying the supplied LogChronicleFilter object
    /// </summary>
    Task<IEnumerable<Message>> GetAsync(LogChronicleFilter filter);
  }

  /// <summary>
  /// Outlines a contract for implementing logic of ILogChronicle
  /// </summary>
  public interface ILogChronicleLogic : ILogChronicle, IBusinessLogic
  {
  }

}
