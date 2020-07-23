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
    Task WriteAsync(InstrumentationBatch data);

    /// <summary>
    /// Gets chronicle (a list) of datum frames satisfying the supplied InstrumentationChronicleFilter object.
    /// Datum Frames encapsulate low-level raw data representation
    /// </summary>
    Task<IEnumerable<DatumFrame>> GetRawAsync(InstrumentationChronicleFilter filter);
  }

  /// <summary>
  /// Outlines a contract for implementing logic of IInstrumentationChronicle
  /// </summary>
  public interface IInstrumentationChronicleLogic : IInstrumentationChronicle, IDataBusinessLogic
  {
    ///// <summary>
    ///// Maps chronicle data frames into polymorphic datum instances.
    ///// The InvalidDatum instance gets returned for DatumFrames that could not be mapped
    ///// </summary>
    //Datum Materialize(DatumFrame frame);

    //Type MapInstrumentType(Guid id);
    //Guid MapInstrumentType(Type tInstrument);
  }

}
