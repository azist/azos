/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.Apps
{
  /// <summary>
  /// Describes an entity that can request some hosting container to end its lifetime by calling End() method
  /// </summary>
  public interface IEndableInstance
  {
    /// <summary>
    /// Indicates whether this instance was requested to be ended and will get destoyed by the hosting container
    /// </summary>
    bool IsEnded { get; }

    /// <summary>
    /// Requests container that hosts.runs this entity to end its instance
    /// </summary>
    void End();
  }
}
