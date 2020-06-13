/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Data.Idgen
{

  /// <summary>
  /// Denotes entities that provide ULONG STABLE hash code for use in a distributed (large scale) system.
  /// This is needed primarily for cluster/large datasets to properly compute 64bit sharding addresses and to differentiate
  /// from GetHashCode() that returns 32 bits unstable hash for local object location in hashtables.
  /// DO not confuse with object.GetHashCode() which is un-suitable for long-term persistence
  /// </summary>
  public interface IDistributedStableHashProvider
  {
    /// <summary>
    /// Provides 64 bit STABLE hash suitable for distributed system application.
    /// This hash may NOT depend on platform as it is used for storage.
    /// Warning! DO NOT CALL object.GetHashCode() as it may not be suitable for storage
    /// </summary>
    ulong GetDistributedStableHash();
  }

}
