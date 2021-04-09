/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Data.Idgen
{
  /// <summary>
  /// Denotes entities that provide ULONG STABLE hash code for use in distributed (large scale) systems.
  /// This is needed primarily for cluster/large datasets to properly compute 64bit sharding addresses (shard keys) and to differentiate
  /// from GetHashCode() that returns 32 bits unstable hash for local object location in hashtables.
  /// DO not confuse with object.GetHashCode() which is unsuitable for long-term persistence routing.
  /// Warning: The returned hash is NOT intended to be used for cryptography, as the returned value
  /// does not have to comply with hash avalanche effect requirement
  /// </summary>
  public interface IDistributedStableHashProvider
  {
    /// <summary>
    /// Provides 64 bit STABLE hash suitable for distributed system application.
    /// This hash may NOT depend on platform/implementation as it is used for storage and
    /// always returns a stable predictable result for the same value regardless of platform.
    /// Warning!!! DO NOT CALL object.GetHashCode() as it may not be suitable for storage (e.g. string.GetHashCode() is different in CLR 32 vs 64 bit).
    /// Warning: The returned hash is NOT intended to be used for cryptography, as the returned value
    /// does not have the hash avalanche effect requirement
    /// </summary>
    /// <remarks>
    /// The obtained hash value must try to minimize collisions and must be stable - not depend on runtimes, so
    /// it can be used for permanent data routing (e.g. DHT applications), however: the returned value does not
    /// need to poses avalanche effect and it is not suitable for cryptography. The implementation should concentrate on collision minimization first.
    /// Use <see cref="ShardKey"/> struct to handle shard hash computations based on select system types and IDistributedStableHashProvider implementors.
    /// `ShardKey` handles hash distribution properly so this interface does not need to concentrate on avalanche properties, instead
    /// this implementation should concentrate on collision minimization first.
    /// For example, if an implementors' state may be expressed as an ulong, it is perfectly fine to return the state as its hash as-is, without
    /// any avalanche transforms and the hash value does NOT need to be secure.
    /// </remarks>
    ulong GetDistributedStableHash();
  }

}
