/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using IDataBusinessLogic = Azos.Data.Business.IBusinessLogic;
using Azos.Instrumentation;

namespace Azos.Sky.Chronicle
{

  /// <summary>
  /// Outlines a high-level contract for working with instrumentation data archives (chronicles)
  /// </summary>
  public interface IInstrumentationChronicle
  {
    /// <summary>
    /// Writes an enumerable of datum instances into chronicle
    /// </summary>
    Task WriteAsync(IEnumerable<Datum> data);

    /// <summary>
    /// Gets chronicle (a list) of datum instances satisfying the supplied InstrumentationChronicleFilter object
    /// </summary>
    Task<IEnumerable<Datum>> GetAsync(InstrumentationChronicleFilter filter);
  }

  /// <summary>
  /// Outlines a contract for implementing logic of IInstrumentationChronicle
  /// </summary>
  public interface IInstrumentationChronicleLogic : IInstrumentationChronicle, IDataBusinessLogic
  {
  }

}
