/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Instrumentation
{
  /// <summary>
  /// Denotes entities that handle external calls - a form of RPC used for application component administration.
  /// An implementer of this interface returns an implementation of IExternalCallHandler which processes the call requests
  /// </summary>
  /// <remarks>
  /// This interface is a form of inversion of control used in Sky cmdlets: instead of implementing different cmdlets
  /// each coupled to specific component types, the general purpose `cman` cmdlet can delegate component-specific admin
  /// behavior into that component type
  /// Note: this interface is designed for system administration needs and should not be used as an implementation technique of
  /// business domain logic. Consequently, the model is constrained to console-like interaction.
  /// </remarks>
  public interface IExternallyCallable
  {
    /// <summary>
    /// Returns an implementation of IExternalCallHandler initialized in the context of this implementing entity
    /// </summary>
    IExternalCallHandler GetExternalCallHandler();
  }
}
