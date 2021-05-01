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
  /// as all models are to be data documents with rich metadata
  /// </summary>
  public interface IBusinessModel : IDataDoc
  {
  }

  /// <summary>
  /// Marker interface for business domain logic modules.
  /// The derivation of IModule is done for architectural constraint,
  /// as logic should be implemented in application modules which get installed
  /// by app chassis
  /// </summary>
  public interface IBusinessLogic : Apps.IModule
  {
  }

}
