/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;
using Azos.Collections;

namespace Azos.Client
{
  /// <summary>
  /// Extensions address cross-cutting concerns (e.g. logging) and provide extensions for various activities
  /// </summary>
  public interface IAspect : IApplicationComponent, INamed, IOrdered
  {
  }
}
