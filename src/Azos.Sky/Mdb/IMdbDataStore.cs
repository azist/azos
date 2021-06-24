/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Collections;
using Azos.Data;
using Azos.Data.Access;
using Azos.Data.Idgen;
using Azos.Pile;


namespace Azos.Sky.Mdb
{
  /// <summary>
  /// Stipulates a contract for MDBDataStore
  /// </summary>
  public interface IMdbDataStore  : IDataStore
  {
    string SchemaName{ get;}
    string BankName{ get;}

    IGdidProvider GdidGenerator{ get;}

    MDBCentralArea CentralArea{ get;}

    IRegistry<MdbArea> Areas{ get;}

    /// <summary>
    /// Pile big memory cache
    /// </summary>
    ICache Cache { get;}

    /// <summary>
    /// Returns CRUDOperations facade connected to the appropriate database server within the named area
    ///  which services the shard computed from the briefcase GDID
    /// </summary>
    CRUDOperations PartitionedOperationsFor(string areaName, GDID idBriefcase);

    /// <summary>
    /// Returns CRUDOperations facade connected to the appropriate shard within the central area as
    /// determined by the the shardingID
    /// </summary>
    CRUDOperations CentralOperationsFor(ShardKey shardingID);
  }
}
