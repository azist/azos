/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Data.Heap
{
  /// <summary>
  /// Defines flags for heap write operations such as whether to Flush disk, use cache etc.
  /// </summary>
  [Flags]
  public enum WriteFlags
  {
    None = 0,

    /// <summary>
    /// Write one more copy into the mirrors/backup locations. This increases safety in case of
    /// the immediate node loss at the expense of extra time spent
    /// </summary>
    Backup = 1,

    /// <summary>
    /// Do not wait for operation to complete, e.g. post mutation to queue and return ASAP
    /// </summary>
    Async = 1 << 30,

    /// <summary>
    /// Flush write to disk if possible
    /// </summary>
    Flush = 1 << 31
  }
}
