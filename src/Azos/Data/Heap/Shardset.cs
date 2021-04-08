/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Azos.Client;

namespace Azos.Data.Heap
{
  public sealed class Router
  {
    /// <summary>
    /// Represents shard node
    /// </summary>
    public sealed class Node
    {
      /// <summary>
      /// Logical unique ID of heap node in cluster
      /// </summary>
      public Atom NodeId { get; }

      /// <summary>
      /// The immutable set-unique ID assigned to a shard node.
      /// When hardware is upgraded you can re-use the ShardId on a different NodeId
      /// to route data into a new node. There may be no more than one node with any ShardId per Set.
      /// The ShardId value must span 8 bytes, so it has to be of at least 2 ^ 56 magnitude
      /// </summary>
      public ulong ShardId { get; }
      //"avalanched version" of ShardId, pre-computed for speed
      internal ulong ShardIdHash {  get;}

      /// <summary>
      /// 1 = the normal weight multiplier
      /// </summary>
      public double ShardWeight{ get; }

      /// <summary>
      /// Cluster host name, such as sky regional catalog path e.g. /world/us/east/cle/db/z1/lmed002.h
      /// </summary>
      string HostName { get; }
    }


    public sealed class Set : IEnumerable<Node>
    {
      private DateTime m_EffectiveUtc;
      private List<Node> m_Set;

      public DateTime EffectiveUtc => m_EffectiveUtc;

      /// <summary>
      /// Service client through which calls are made
      /// </summary>
      IService Service {  get; }

      public Node RendezvousRoute(ShardKey shardKey) //O(n)
      {
        //[W]eighted [R]endezvous [H]ashing Algorithm
        var keyHash = shardKey.Hash;//the hash is already "avalanched"

        Node best = null;
        double bestScore = double.MinValue;

        foreach(var node in m_Set)
        {
          var hash = node.ShardIdHash ^ keyHash;//both "avalanched"
          double norm = hash / (double)ulong.MaxValue; //0.0 .. 1.0
          if (norm == 0d) norm = 0.1d;
          var score = node.ShardWeight / -Math.Log(norm);//logarithm of real number is negative
          if (score > bestScore)
          {
            best = node;
            bestScore = score;
          }
        }

        return best;
      }


      public IEnumerator<Node> GetEnumerator() => m_Set.GetEnumerator();
      IEnumerator IEnumerable.GetEnumerator() => m_Set.GetEnumerator();
    }



    public Set this[int idx]
    {
      get{ return null; }
    }

    public Set this[DateTime asOfUtc]
    {
      get{ return null;}
    }
  }
}
