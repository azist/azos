/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Collections;

namespace Azos.Sky.Fabric.Server
{
  /// <summary>
  /// Describes runspaces which are supported by the system.
  /// Runspaces partition fiber execution and storage zones
  /// </summary>
  public sealed class RunspaceMapping : IAtomNamed
  {
    /// <summary>
    /// Unique fiber namespace identifier
    /// </summary>
    public Atom Name => throw new NotImplementedException();


    /// <summary>
    /// Specifies relative weight of this runspace among others for processing
    /// </summary>
    public float ProcessingFactor { get; }


    /// <summary>
    /// Returns all shard mappings for this runspace
    /// </summary>
    public IAtomRegistry<ShardMapping> Shards => null;
  }
}
