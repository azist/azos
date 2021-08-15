/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Azos.Data.Access.Sharding
{
  /// <summary>
  /// Provides various extensions methods related to sharding
  /// </summary>
  public static class Extensions
  {
    /// <summary>
    /// Shortcut which gets <see cref="CrudOperations"/> for the routed shard for the specified
    /// <see cref="ShardKey"/> from the current shard set
    /// </summary>
    /// <param name="store">Store instance</param>
    /// <param name="key">
    /// Key based on which shard routing is performed. The system maps (routes) the request
    /// based on this key to a most appropriate shard of a shard set
    /// </param>
    /// <returns>CrudOperations for the mapped shard</returns>
    public static CrudOperations GetOperationsFor(this IShardedCrudDataStore store, ShardKey key)
     => store.NonNull(nameof(store)).CurrentShardSet.GetShardFor(key).Operations;

    /// <summary>
    /// Provides O(n) linear weighted rendezvous routing algorithm
    /// </summary>
    /// <param name="shards">Shard set out of which one shard is selected</param>
    /// <param name="shardKey">Sharding key - argument of shard mapping</param>
    /// <returns>IShard chosen from the shard set</returns>
    public static IShard RendezvouzRoute(this IEnumerable<IShard> shards, ShardKey shardKey)
    {
      //[W]eighted [R]endezvous [H]ashing Algorithm
      //https://tools.ietf.org/html/draft-mohanty-bess-weighted-hrw-01
      var keyHash = shardKey.Hash;//the hash is already "avalanched"

      shards.IsTrue( s => s!=null && s.Any(), "shards!=null && !empty");

      IShard best = null;
      double bestScore = double.MinValue;

      foreach (var shard in shards) // O(n)
      {
        var hash = shard.NameHash ^ keyHash;//both "avalanched"
        double norm = hash / (double)ulong.MaxValue; //[0.0 .. 1.0]
        var score = shard.ShardWeight / -Math.Log(norm);//logarithm of real number is negative; log (0) = - infinity ; 1 / -log(0) = 0
        if (score > bestScore)
        {
          best = shard;
          bestScore = score;
        }
      }

      return best;
    }
  }
}
