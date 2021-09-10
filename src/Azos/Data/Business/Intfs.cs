/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Data.Business
{
  /// <summary>
  /// Marker interface for business domain models.
  /// The derivation of IDataDoc imposes an architectural constraint
  /// as all models are to be data documents with rich metadata.
  /// Models exist on both client and server tiers, while remaining non-anemic OOP entities (classes with methods)
  /// they conveniently encapsulate and provide baseline data services such as: validation (including cross-field/conditional),
  /// dynamic picklist lookup, persistence/save; all in the same place in code, which
  /// significantly simplifies construction of complex business applications
  /// </summary>
  /// <remarks>
  /// <para>
  /// Business models are stateful, OOP objects with methods. This is a conscious design choice which
  /// results in significant complexity reduction in business-oriented applications which have many models
  /// with many fields.
  /// </para>
  /// <para>
  /// The unification of business model design allows for grouping of logically-related operations such these,
  /// many of which must be conditional (depend on data/other fields/context):
  ///  (a) validation, (b) dynamic picklist lookup, (c) conditional data serialization/formatting, (d) persistence/save
  /// Practice shows, that using OOP principles (vs anemic DTOs) significantly reduce the amount of needed code/data structures
  /// and help unit and integration testing, as the same dependency graph is used for uniform testing of ALL business entities
  /// in the whole application vs using circumstantial mocks for every test.
  /// </para>
  /// </remarks>
  public interface IBusinessModel : IDataDoc
  {
  }

  /// <summary>
  /// Marker interface for business domain logic modules.
  /// The derivation of IModule is done for architectural constraint,
  /// as logic should be implemented in application modules which get installed
  /// by app chassis. This also simplifies testing via uniform app chassis pattern
  /// using the same dependencies for all SUT (systems under test).
  /// </summary>
  public interface IBusinessLogic : Apps.IModule
  {
    /// <summary>
    /// Returns true when the implementor represents a server which is a final
    /// point of logic request handling. Returns false for implementations that act
    /// just as clients delegating model/work to some other logic servers.
    /// This feature allows for reuse of the same model logic which can test
    /// for presence of server-side capabilities, e.g. some rigorous validation
    /// may be bypassed in a client library altogether but mandated on the server
    /// </summary>
    bool IsServerImplementation { get; }
  }
}
