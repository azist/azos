/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Apps
{
  /// <summary>
  /// Describe an entity that resolves type Guid ids into CLR type objects
  /// </summary>
  public interface IGuidTypeResolver
  {
    /// <summary>
    /// Tries to resolves the GUID into type or returns null
    /// </summary>
    Type TryResolve(Guid guid);

    /// <summary>
    /// Tries to resolves the GUID into type or returns null
    /// </summary>
    Type TryResolve(Fguid guid);

    /// <summary>
    /// Resolves the GUID into type or throws
    /// </summary>
    Type Resolve(Guid guid);

    /// <summary>
    /// Resolves the GUID into type or throws
    /// </summary>
    Type Resolve(Fguid guid);
  }

}
