/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Instrumentation;
using Azos.Serialization.JSON;

namespace Azos.Sky.Chronicle
{
  /// <summary>
  /// Outlines a high-level contract for working with instrumentation data archives (chronicles).
  /// The chronicle archive operates in a type-agnostic way passing data as array of JsonDataMap objects acting like raw datum frames.
  /// Each frame contains bix type discriminator allowing for optional de-serialization into original Datum-derived type
  /// if assemblies containing these types are present and registered with the app container
  /// </summary>
  /// <remarks>
  /// IInstrumentationChronicle servers supports instrumentation data streams produced by any environment/language, not
  /// necessarily CLR-enabled. Bix type discriminator sets a GUID/UUID per every gauge/instrument type which can be cross-mapped on
  /// various platforms, for example: you can create a "VoltageGauge" and assign it a GUID via C# class and Bix attribute, then map that guid
  /// to a "VoltageGauge" name in Node.js or Python system, and vice-versa.
  /// </remarks>
  public interface IInstrumentationChronicle
  {
    /// <summary>
    /// Writes an enumerable of datum instances into chronicle
    /// </summary>
    Task WriteAsync(InstrumentationBatch data);

    /// <summary>
    /// Gets chronicle (a list) of datum frames satisfying the supplied InstrumentationChronicleFilter object.
    /// Datum frames (JsonDataMap) contain raw data for every individual measurement sample
    /// </summary>
    Task<IEnumerable<JsonDataMap>> GetAsync(InstrumentationChronicleFilter filter);
  }


  /// <summary>
  /// Outlines a contract for implementing logic of IInstrumentationChronicle
  /// </summary>
  public interface IInstrumentationChronicleLogic : IInstrumentationChronicle, IModule
  {
  }

}
