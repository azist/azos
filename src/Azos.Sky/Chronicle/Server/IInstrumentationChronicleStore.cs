using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Log;
using Azos.Serialization.JSON;

namespace Azos.Sky.Chronicle.Server
{
  /// <summary>
  /// Abstracts the storage of Log Chronicle data
  /// </summary>
  public interface IInstrumentationChronicleStoreLogic : IApplicationComponent
  {
    Task WriteAsync(InstrumentationBatch data);
    Task<IEnumerable<JsonDataMap>> GetAsync(InstrumentationChronicleFilter filter);
  }

  public interface IInstrumentationChronicleStoreLogicImplementation : IInstrumentationChronicleStoreLogic, IDaemon
  {
  }
}
