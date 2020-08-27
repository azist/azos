/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data.AST;
using Azos.Data.Business;

namespace Azos.Data.Directory
{

  public static class DirectoryStandards
  {
    /// <summary>
    /// Standard collection name used for user profiles
    /// </summary>
    public const string COLLECTION_USER_PROFILE = "usrp";
  }


  /// <summary>
  /// Represents a directory of string ID-keyed entities.
  /// Directories are a form of KEY-Value tuple databases
  /// </summary>
  public interface IDirectory : IApplicationComponent
  {
    /// <summary>
    /// Gets an item directly by its primary key optionally updating it's last-use date
    /// </summary>
    Task<Item> GetAsync(EntityId id, bool touch = false);

    /// <summary>
    /// Saves an item to directory
    /// </summary>
    Task<ChangeResult> SaveAsync(Item item);

    /// <summary>
    /// Updates LastUseUtc timestamp for items that use sliding expiration. Returns true if an item was found and updated
    /// </summary>
    Task<ChangeResult> TouchAsync(IEnumerable<EntityId> ids);

    /// <summary>
    /// Tries to delete an item, returning true if an item found and marked for deletion
    /// </summary>
    Task<ChangeResult> DeleteAsync(EntityId id);

    /// <summary>
    /// Finds item/items that satisfy the queryExpression which is an object graph representing an expression tree
    /// </summary>
    /// <param name="entity">Entity designator uses the (System:Type) tuple to identify sub-directory to route query into</param>
    /// <param name="queryExpression">Query expression graph</param>
    /// <returns>Item enumeration</returns>
    Task<IEnumerable<Item>> QueryAsync(EntityId entity, Expression queryExpression);
  }


  /// <summary>
  /// Outlines a contract for implementing logic of IDirectory
  /// </summary>
  public interface IDirectoryLogic : IDirectory, IModule
  {
  }

}
