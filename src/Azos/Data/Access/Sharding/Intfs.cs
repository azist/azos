/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

namespace Azos.Data.Access.Sharding
{
  /// <summary>
  /// Represents data store which has a concept of data partitioning between shards.
  /// The shards are routed-into by <see cref="ShardKey"/>
  /// </summary>
  public interface IShardedCrudDataStore : IDataStore
  {
    /// <summary>
    /// Gets all shards in the current set
    /// </summary>
    IEnumerable<IShard> CurrentSet {  get; }

    /// <summary>
    /// Gets <see cref="CrudOperations"/> for the shard routed to
    /// using the specified <see cref="ShardKey"/>
    /// </summary>
    CrudOperations GetOperationsFor(ShardKey key);
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
  }

  /// <summary>
  /// Represents a shard (a partition) of the data in shard set
  /// </summary>
  public interface IShard
  {
    /// <summary>
    /// Data store which this shard is a part of
    /// </summary>
    IShardedCrudDataStoreImplementation Store { get; }

    /// <summary>
    /// The immutable unique ID assigned to a shard node within a set.
    /// The ID determines the consistent shard traffic routing, so be careful not to change
    /// the id once assigned. It is recommended to use all 8 bytes of the ID for better probability
    /// distribution
    /// </summary>
    ulong ShardId { get; }

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
    string RouteDatabaseName { get; set; }

    /// <summary>
    /// Operations for this shard
    /// </summary>
    CrudOperations Operations {  get; }
  }
}
