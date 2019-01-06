/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;


using Azos.Conf;


namespace Azos.Sky.Mdb
{
  /// <summary>
  /// Represents a single central area that has one central partition
  /// </summary>
  public sealed class MDBCentralArea : MdbArea
  {

    internal MDBCentralArea(MdbDataStore store, IConfigSectionNode node) : base(store, node)
    {
      m_CentralPartition = new Partition(this, node);
    }

    //Central has only one partition
    private Partition m_CentralPartition;


    public override string Name{ get{ return CENTRAL_AREA_NAME;}}

    /// <summary>
    /// Returns a single partition of MDBCentralArea
    /// </summary>
    public Partition CentralPartition{ get{ return m_CentralPartition;}}

    public override IEnumerable<Partition> AllPartitions { get { yield return m_CentralPartition;} }

    public override IEnumerable<Partition.Shard> AllShards { get { return m_CentralPartition.Shards;}}


    /// <summary>
    /// Returns CRUDOperations facade connected to the appropriate database server within the CENTRAL area's partition
    ///  which services the shard computed from sharding id
    /// </summary>
    public CRUDOperations ShardedOperationsFor(object idSharding)
    {
      return m_CentralPartition.ShardedOperationsFor(idSharding);
    }

  }

}
