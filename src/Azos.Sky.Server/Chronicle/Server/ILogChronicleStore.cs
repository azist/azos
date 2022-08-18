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
  public interface ILogChronicleStore : IDaemonView
  {
    Task WriteAsync(LogBatch data);
    Task<IEnumerable<Message>> GetAsync(LogChronicleFilter filter);
  }

  public interface ILogChronicleStoreImplementation : ILogChronicleStore, IDaemon
  {
  }
}
