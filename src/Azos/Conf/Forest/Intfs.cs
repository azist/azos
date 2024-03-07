/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Data.Business;

namespace Azos.Conf.Forest
{
  /// <summary>
  /// Defines logic for consuming Forest services.
  /// A forest is a system of named trees; each tree is a hierarchy (DAG) or tree nodes containing configuration
  /// content. The strength of the system lies in inheritance of configuration vectors which cascade down from tree root level down
  /// to individual node level respecting structured config override rules.
  /// All data is a). version-controlled  b). has a temporal component - a point in time when a node becomes active, therefore the system
  /// allows for back and forward dating node presence in a tree
  /// </summary>
  public interface IForestLogic : IBusinessLogic
  {
    /// <summary>
    /// Defaults Utc timestamp value from app time source when the supplied one is null, then
    /// aligns the supplied or defaulted timestamp on the configured value for the optionally specified tree
    /// if provided or the <see cref="Constraints.DEFAULT_POLICY_REFRESH_WINDOW_MINUTES"/>
    /// </summary>
    /// <param name="v">UTC timestamp, or null to default the app-current time</param>
    /// <param name="id">The optional EntityId of the tree targeted in the configuration policy</param>
    /// <returns>Aligned dateTime</returns>
    DateTime DefaultAndAlignOnPolicyBoundary(DateTime? v, EntityId? id = null);

    /// <summary>
    /// Retrieves a list trees in the specified forest
    /// </summary>
    /// <param name="idForest">Id of a forest to get tree ids from. Forest id is encoded as `EntityId.System` field</param>
    /// <returns>Enumerable of <see cref="Atom"/> objects identifying trees of the selected forest</returns>
    Task<IEnumerable<Atom>> GetTreeListAsync(Atom idForest);

    /// <summary>
    /// Retrieves a list of child nodes headers (node info without config content) for the specified tree node
    /// </summary>
    /// <param name="idParent">EntityId of a parent node</param>
    /// <param name="asOfUtc">As of which point in time to retrieve the state, if null passed then current timestamp assumed</param>
    /// <param name="cache">Controls cache options used by the call, such as bypass cache etc.</param>
    /// <returns>Enumerable of <see cref="TreeNodeHeader"/> objects</returns>
    Task<IEnumerable<TreeNodeHeader>> GetChildNodeListAsync(EntityId idParent, DateTime? asOfUtc = null, ICacheParams cache = null);

    /// <summary>
    /// Retrieves a node of TreeNodeInfo by its id as of certain point in time
    /// </summary>
    /// <param name="id">Node id</param>
    /// <param name="asOfUtc">As of which point in time to retrieve the state, if null passed then current timestamp assumed</param>
    /// <param name="cache">Controls cache options used by the call, such as bypass cache etc.</param>
    /// <returns>TreeNodeInfo object or null if such item is not found</returns>
    Task<TreeNodeInfo> GetNodeInfoAsync(EntityId id, DateTime? asOfUtc = null, ICacheParams cache = null);

    //Task<IEnumerable<TreeNodeHeader>> GetNodesByTagGeoAddressAsync(Atom idForest, Atom idTree, Atom tag, string tagValue, DateTime? asOfUtc = null, ICacheParams cache = null);
    // Task<IEnumerable<TreeNodeInfo>> GetNodesByGeoAddressAsync(Atom idForest, Atom idTree, AddressSpec address, int radius, DateTime? asOfUtc = null, ICacheParams cache = null);
    // Task<IEnumerable<TreeNodeInfo>> GetNodesByGeoLocationAsync(Atom idForest, Atom idTree, LatLng location, int radius, DateTime? asOfUtc = null, ICacheParams cache = null);
  }

  /// <summary>
  /// Defines setup operations for forest tree structures: Saving and Deleting tree nodes.
  /// This interface provide writing functionality on top of reading functionality provided by `IForestLogic`
  /// and the caller is required to have a additional permission grants to perform these operations
  /// </summary>
  public interface IForestSetupLogic : IForestLogic
  {
    /// <summary>
    /// Retrieves all versions of the specified tree node identified by GDID.
    /// You must use <see cref="Constraints.SCH_GNODE"/> address schema
    /// </summary>
    /// <param name="id">Node Gdid is the only address schema supported</param>
    /// <returns>List of versions of the specified path or null if such id does not exist</returns>
    Task<IEnumerable<VersionInfo>> GetNodeVersionListAsync(EntityId id);

    /// <summary>
    /// Retrieves tree node information of the specified version.
    /// Note: you must use <see cref="Constraints.SCH_GVER"/> address schema:  `region.gver@geo::0:0:345`
    /// </summary>
    /// <param name="idVersion">EntityId of version containing (system, type, gver: gVersion)</param>
    /// <returns>NodeInfo or null if not found</returns>
    Task<TreeNodeInfo> GetNodeInfoVersionAsync(EntityId idVersion);

    /// <summary>
    /// Performs complex validation steps on the supplied node before it gets saved.
    /// This method is called by the `TreeNode` instance as a part of save activity.
    /// For example, child nodes check that they are saved under the proper parent entity type etc.
    /// </summary>
    /// <param name="node">Tree node to validate</param>
    /// <param name="state">Existing validState</param>
    /// <returns><see cref="ValidState"/> representative of validation outcome</returns>
    Task<ValidState> ValidateNodeAsync(TreeNode node, ValidState state);

    /// <summary>
    /// Persists the representation of data supplied as `TreeNode` in the version-controlled
    /// store. The deletion works via `node.FormMode = Delete` which effectively inactivates the record
    /// </summary>
    /// <param name="node">Persisted data model</param>
    /// <returns>Data change operation result containing GDID for newly inserted entities</returns>
    Task<ChangeResult> SaveNodeAsync(TreeNode node);

    /// <summary>
    /// Logically deletes node by its ID returning <see cref="ChangeResult"/>.
    /// Deletion of node is logical(not-physical) and logically inactivates all child structures under it, for example
    /// deletion of `policies` node logically deletes all individual `policy` child nodes and their sub-nodes effectively making
    /// those entities not found
    /// </summary>
    /// <param name="id">Node id</param>
    /// <param name="startUtc">Timestamp as of which the node becomes logically deleted</param>
    /// <returns>ChangeResult</returns>
    Task<ChangeResult> DeleteNodeAsync(EntityId id, DateTime? startUtc = null);

    /// <summary>
    /// Physically deletes all data from the specified forest tree.
    /// The tree is returned to the state as-if just created anew.
    /// The data loss is unrecoverable.
    /// Requires additional <see cref="Security.Config.TreePurgePermission"/> granted to the caller
    /// </summary>
    Task PurgeAsync(Atom idForest, Atom idTree);
  }
}

