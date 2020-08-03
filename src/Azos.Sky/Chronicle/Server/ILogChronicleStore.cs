using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Log;

namespace Azos.Sky.Chronicle.Server
{
  /// <summary>
  /// Abstracts the storage of Log Chronicle data
  /// </summary>
  public interface ILogChronicleStoreLogic : IApplicationComponent
  {
    Task WriteAsync(LogBatch data);
    Task<IEnumerable<Message>> GetAsync(LogChronicleFilter filter);
  }

  public interface ILogChronicleStoreLogicImplementation : ILogChronicleStoreLogic, IDaemon
  {
  }
}
