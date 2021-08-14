/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
  public sealed class ShardSet : ApplicationComponent<IShardedCrudDataStoreImplementation>, INamed, IOrdered, IEnumerable<IShard>
  {
    public const string CONFIG_SHARD_SECTION = "shard";

    public ShardSet(IShardedCrudDataStoreImplementation director, IConfigSectionNode conf) : base(director)
    {
      ConfigAttribute.Apply(this, conf.NonEmpty(nameof(conf)));
      m_Name.NonBlank("$name");

      m_Shards = new List<IShard>();
      //read shards
      foreach(var nsh in conf.ChildrenNamed(CONFIG_SHARD_SECTION))
      {
        var shard = director.MakeShard(this, nsh);
        if (m_Shards.Any( s => s.Name.EqualsIgnoreCase(shard.Name)))
        {
          throw new DataAccessException(StringConsts.DATA_SHARDING_DUPLICATE_SECTION_CONFIG_ERROR.Args(CONFIG_SHARD_SECTION, shard.Name));
        }
        m_Shards.Add(shard);
      }

      m_Shards.IsTrue( s => s.Count > 0, StringConsts.DATA_SHARDING_AT_LEAST_ONE_CLAUSE);
    }

    [Config]private string m_Name;
    [Config]private int m_Order;
    private List<IShard> m_Shards;

    public string Name => m_Name;
    public int Order => m_Order;
    public IEnumerable<IShard> Shards => m_Shards.ToArray();

    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;


    /// <summary>
    /// Gets <see cref="IShard"/> for the shard of this set routed to
    /// using the specified <see cref="ShardKey"/>
    /// </summary>
    public IShard GetOperationsFor(ShardKey key) => ComponentDirector.GetShardFor(this, key);

    public IEnumerator<IShard> GetEnumerator() => m_Shards.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => m_Shards.GetEnumerator();
  }
}
