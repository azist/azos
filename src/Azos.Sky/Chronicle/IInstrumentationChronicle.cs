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
  /// Outlines a high-level contract for working with instrumentation data archives (chronicles).
  /// The chronicle archive operates in a type-agnostic way using instances of `DatumFrame` that may optionally
  /// inline the serialized version of full platform-specific Datum representation polymorphically.
  /// You can use `IInstrumentationChronicleClientLogic` to materialize DatumFrame into actual CLR types, given that
  /// type-containing assemblies are present and registered with the app chassis.
  /// </summary>
  /// <remarks>
  /// IInstrumentationChronicle servers supports instrumentation data streams produced by any environment/language, not
  /// necessarily CLR-enabled. You can attach platform-specific "raw" data as `DatumFrame.Content` property with
  /// `ContentType` flag which states the format of binary content, hence it is possible to use cross-platform
  /// serializers (such as Json) to read-in the telemetry produced on different systems.
  /// DatumFrame.Type sets a GUID/UUID per every gauge/instrument type which can be cross-mapped on various platforms, for example:
  /// you can create a "VoltageGauge" and assign it a GUID via C# class and Bix attribute, then map that guid
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
    /// Datum Frames encapsulate low-level raw data representation
    /// </summary>
    Task<IEnumerable<DatumFrame>> GetAsync(InstrumentationChronicleFilter filter);
  }


  /// <summary>
  /// Outlines a contract for implementing logic of IInstrumentationChronicle
  /// </summary>
  public interface IInstrumentationChronicleLogic : IInstrumentationChronicle, IDataBusinessLogic
  {
  }

  /// <summary>
  /// An interface for IInstrumentationChronicle client consumption, such as materializing DatumFrames
  /// </summary>
  public interface IInstrumentationChronicleClientLogic : IInstrumentationChronicleLogic
  {
    /// <summary>
    /// Maps Datum-derived instance into DatumFrame which is CLR/platform agnostic archive chronicle representation
    /// of the instrumentation measurement sample
    /// </summary>
    DatumFrame Map(Datum datum);

    /// <summary>
    /// Maps chronicle data frames into polymorphic data (datum instances).
    /// The null instance gets returned for DatumFrames that could not be mapped to actual CLR datum-derived
    /// types. This can happen, for example if not all assemblies have been registered with `azos-serialization-bix`
    /// or specified content type can not be deserialized
    /// </summary>
    Datum TryMaterialize(DatumFrame frame);

    /// <summary>
    /// Tries to map Bix type guid to CLR type or null if no mapping exists (e.g. when assemblies are not present/registered)
    /// </summary>
    Type MapInstrumentType(Guid id);

    /// <summary>
    /// Maps CLR type to type id Guid as supplied via [Bix(guid)] decoration
    /// </summary>
    Guid MapInstrumentType(Type tInstrument);
  }

}
