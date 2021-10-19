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
  /// Defines logic for consuming Forest services
  /// </summary>
  public interface IForestLogic : IBusinessLogic
  {
    /// <summary>
    /// Retrieves all versions of the specified tree node identified by GDID.
    /// You must use <see cref="TreePath.SCH_GNODE"/> address schema
    /// </summary>
    /// <param name="id">Node Gdid is the only address schema supported</param>
    /// <returns>List of versions of the specified path or null if such id does not exist</returns>
    Task<IEnumerable<VersionInfo>> GetNodeVersionListAsync(EntityId id);

    /// <summary>
    /// Retrieves tree node information of the specified version.
    /// Note: you must use <see cref="TreePath.SCH_GVER"/> address schema:  `region.gver@geo::0:0:345`
    /// </summary>
    /// <param name="idVersion">EntityId of version containing (system, type, gver: gVersion)</param>
    /// <returns>NodeInfo or null if not found</returns>
    Task<TreeNodeInfo> GetNodeInfoVersionAsync(EntityId idVersion);

//Delete this????
/*
    /// <summary>
    /// Retrieves a list of enterprise nodes which are top-level nodes of the corporate hierarchy
    /// </summary>
    /// <param name="asOfUtc">As of which point in time to retrieve the state, if null passed then current timestamp assumed</param>
    /// <param name="cache">Controls cache options used by the call, such as bypass cache etc.</param>
    /// <returns>Enumerable of <see cref="ListItem"/> objects</returns>
    Task<IEnumerable<(Atom, Atom)>> GetTreeRootListAsync(DateTime? asOfUtc = null, ICacheParams cache = null);
*/

    /// <summary>
    /// Retrieves a list of child nodes for the specified corporate hierarchy entity
    /// </summary>
    /// <param name="idParent">EntityId of a parent, e.g. an enterprise, company or division node</param>
    /// <param name="asOfUtc">As of which point in time to retrive the state, if null passed then current timestamp assumed</param>
    /// <param name="cache">Controls cache options used by the call, such as bypass cache etc.</param>
    /// <returns>Enumerable of <see cref="ListItem"/> objects</returns>
//    Task<IEnumerable<ListItem>> GetChildNodeListAsync(EntityId idParent, DateTime? asOfUtc = null, ICacheParams cache = null);

    /// <summary>
    /// Retrieves a node of corporate hierarchy by its id as of certain point in time
    /// </summary>
    /// <param name="id">Node id</param>
    /// <param name="asOfUtc">As of which point in time to retrieve the state, if null passed then current timestamp assumed</param>
    /// <param name="cache">Controls cache options used by the call, such as bypass cache etc.</param>
    /// <returns>CorporateHierarchyNodeInfo-derived object for the requested level, null if such item is not found</returns>
    Task<TreeNodeInfo> GetNodeInfoAsync(EntityId id, DateTime? asOfUtc = null, ICacheParams cache = null);


    /// <summary>
    /// Performs complex validation steps on the supplied node before it gets saved.
    /// This method is called by the CorporateNode instance as a part of save activity.
    /// For example, child nodes check that they are saved under the proper parent entity type etc.
    /// </summary>
    /// <param name="node">Model node to validate</param>
    /// <param name="state">Existing validState</param>
    /// <returns><see cref="ValidState"/> representative of validation outcome</returns>
    Task<ValidState> ValidateNodeAsync(TreeNode node, ValidState state);

    /// <summary>
    /// Persists the representation of data supplied as `CorporateHierarchyNode` in the version-controlled
    /// store. The deletion works via `node.FormMode = Delete` which effectively inactivates the record
    /// </summary>
    /// <param name="node">Persisted data model</param>
    /// <returns>Data change operation result containing GDID for newly inserted entities</returns>
    Task<ChangeResult> SaveNodeAsync(TreeNode node);

    /// <summary>
    /// Logically deletes node by its ID returning <see cref="ChangeResult"/>.
    /// Deletion of node is logical(not-physical) and logically inactivates all child structures under it, for example
    /// deletion of a company logically deletes all divisions and units under that company effectively making
    /// those entities not found
    /// </summary>
    /// <param name="id">Node id</param>
    /// <param name="startUtc">Timestamp as of which the node becomes logically deleted</param>
    /// <returns>ChangeResult</returns>
    Task<ChangeResult> DeleteNodeAsync(EntityId id, DateTime? startUtc = null);
  }
}

