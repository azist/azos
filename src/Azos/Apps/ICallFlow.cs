/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

namespace Azos.Apps
{
  /// <summary>
  /// Describes call flow - an entity which represents a logical call, such as service flow.
  /// This can be used as a correlation context
  /// </summary>
  public interface ICallFlow
  {
     /// <summary>
     /// Gets unique CallFlow identifier, you can use it for things like log correlation id
     /// </summary>
     Guid ID { get; }
  }

}
