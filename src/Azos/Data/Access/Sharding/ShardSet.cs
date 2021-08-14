/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;

namespace Azos.Data.Access.Sharding
{
  /// <summary>
  /// A ShardSet is a set of <see cref="IShard"/> instances.
  /// Each ShardSet has a unique name and position in <see cref="IShardedCrudDataStore.ShardSets"/> registry.
  /// The <see cref="Order"/> defines the historical ordering of sets in a store, the smallest order (the first item in collection)
  /// being the current set, the subsequent orders represent the shard set generations/versions in history.
  /// Older shard sets may be needed to re-balance the sharded data
  /// </summary>
  public sealed class ShardSet : ApplicationComponent<IShardedCrudDataStore>, INamed, IOrdered, IEnumerable<IShard>
  {
    public ShardSet(IShardedCrudDataStoreImplementation director, IConfigSectionNode conf) : base(director)
    {

    }

    private string m_Name;
    private int m_Order;
    private IEnumerable<IShard> m_Shards;

    public string Name { get; private set; }
    public int Order { get; private set; }
    public IEnumerable<IShard> Shards => m_Shards;

    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;


    /// <summary>
    /// Gets <see cref="IShard"/> for the shard of this set routed to
    /// using the specified <see cref="ShardKey"/>
    /// </summary>
    public IShard GetOperationsFor(ShardKey key)
    {
       return null;
    }

    public IEnumerator<IShard> GetEnumerator() => m_Shards.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => m_Shards.GetEnumerator();
  }
}
