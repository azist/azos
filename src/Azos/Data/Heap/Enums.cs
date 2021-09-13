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

  /// <summary>
  /// Defines flags describing general node capabilities
  /// </summary>
  [Flags]
  public enum NodeFlags
  {
    None = 0,

    /// <summary>
    /// Can not get by direct reference, e.g. if the storage engine is a file-based event log
    /// </summary>
    NoGet = 1 << 0,

    /// <summary>
    /// Can not execute query, e.g. if the storage engine is a file-based event log
    /// </summary>
    NoQuery = 1 << 1,

    /// <summary>
    /// Can not be written object changes to (no Set() or Delete())
    /// </summary>
    Readonly = 1 << 2,

    /// <summary>
    /// The node does not auto-sync from others
    /// </summary>
    NoSyncPull = 1 << 16,

    /// <summary>
    /// Others do not sync from this node
    /// </summary>
    NoSyncSource = 1 << 17
  }


  /// <summary>
  /// Node status: Online/Offline/Failed as observed from the calling peer
  /// </summary>
  public enum NodeStatus
  {
    /// <summary>
    /// The connection with the node has not been established yet
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// Node is connected and functioning as expected
    /// </summary>
    Online,

    /// <summary>
    /// Node is offline/inactive due to planned maintenance or inactivated manually
    /// </summary>
    Offline,

    /// <summary>
    /// The node is online but has a different schema definition, e.g. this prevents
    /// sync from this node
    /// </summary>
    SchemaMismatch,

    /// <summary>
    /// Communication failure - does not respond as expected or does not respond at all.
    /// This does not mean necessarily that node is down, it may be having network issues
    /// </summary>
    Failure
  }

}
