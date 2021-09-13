/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Sky.EventHub
{
  /// <summary>
  /// Defines the data loss possibilities.
  /// The less loss is possible the more resources system needs to make extra copies/broadcast event
  /// </summary>
  public enum DataLossMode
  {
    /// <summary> (Default) Writes to/read from 3 nodes = prevents temporal failures between the two nodes via the 3rd node</summary>
    Default = 0,

    /// <summary> Fastest - only one node gets written to/read from </summary>
    High = 1,

    /// <summary> Writes to/read from 2 nodes = 1/2 failure chance </summary>
    Mid = 2,

    /// <summary> (Default) Writes to/read from 3 nodes = prevents temporal failures between the two nodes via the 3rd node </summary>
    Low = Default,

    /// <summary>
    /// Writes to/reads from as many nodes as possible right now (some may be offline)
    /// </summary>
    MinimumPossible = 4,

    /// <summary>
    /// (Slowest) Write to/reads from ALL nodes in cohort.
    /// If some fail
    /// </summary>
    Minimum = 0xffff
  }
}
