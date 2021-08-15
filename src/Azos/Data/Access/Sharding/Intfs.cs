/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Collections;
using Azos.Conf;

namespace Azos.Data.Access.Sharding
{
  /// <summary>
  /// Represents data store which has a concept of data partitioning between shards.
  /// The shards are routed-into by <see cref="ShardKey"/>
  /// </summary>
  public interface IShardedCrudDataStore : IDataStore
  {
    /// <summary>
    /// Current <see cref="ShardSet"/> is the first instance from the list
    /// </summary>
    ShardSet CurrentShardSet {  get; }

    /// <summary>
    /// Ordered registry of <see cref="ShardSet"/> instances, the first one being the current,
    /// subsequent ones go back in time history. Older shard set generations may be needed to re-balance the sharded
    /// dataset, whilst in 99% of cases the system routes data requests to the current shardset
    /// </summary>
    IOrderedRegistry<ShardSet> ShardSets { get; }
  }

  public interface IShardedCrudDataStoreImplementation : IShardedCrudDataStore, IDataStoreImplementation
  {
    /// <summary>
    /// Physical store which services the shard set
    /// </summary>
    ICrudDataStore PhysicalStore { get; }

    /// <summary>
    /// Factory method which makes appropriate instance of <see cref="CrudOperationCallContext"/>
    /// or its derivative class, suitable for passing into <see cref="PhysicalStore"/> for the
    /// specified shard instance
    /// </summary>
    CrudOperationCallContext MakeCallContext(IShard shard);

    IShard MakeShard(ShardSet set, IConfigSectionNode conf);

    /// <summary>
    /// Determines what shard should the provided key be routed to
    /// </summary>
    IShard GetShardFor(ShardSet set, ShardKey key);
  }

  /// <summary>
  /// Represents a shard (a partition) of the data in shard set
  /// </summary>
  public interface IShard : INamed
  {
    /// <summary>
    /// Shard set which this shard is a part of
    /// </summary>
    ShardSet Set { get; }


    /// <summary>
    /// Name hashed with stable function with avalanche effect
    /// </summary>
    ulong NameHash {  get; }

    /// <summary>
    /// Shard weight controls relative weighting of this shard in a set.
    /// Value of 1 represents the normal (default) weight multiplier.
    /// Changing this parameters changes data distribution
    /// </summary>
    double ShardWeight { get; }

    /// <summary>
    /// Returns Route for the destination, such as database connect string, node config etc..
    /// This is where the traffic gets routed into.
    /// Note: depending on physical store, this can represent logical connection name (such as cluster db instance name)
    /// which gets translated to physical database connection address later
    /// </summary>
    string RouteConnectString { get; }

    /// <summary>
    /// Used to override store's default database name - used by some stores, others take db name from the connect string
    /// </summary>
    string RouteDatabaseName { get; }

    /// <summary>
    /// Operations for this shard
    /// </summary>
    CrudOperations Operations {  get; }
  }
}
