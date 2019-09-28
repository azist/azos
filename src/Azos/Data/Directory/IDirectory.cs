using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azos.Data.Access;

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
  /// Directories are a form of KEY-Value vector databases
  /// </summary>
  public interface IDirectory : IDataStore
  {
    /// <summary>
    /// Gets an item directly by its primary key optionally updating it's last-use date
    /// </summary>
    Task<Item> GetAsync(ItemId id, bool touch = false);

    /// <summary>
    /// Saves an item to directory
    /// </summary>
    Task Save(Item item);

    /// <summary>
    /// Updates LastUseUtc timestamp for items that use sliding expiration. Returns true if an item was found and updated
    /// </summary>
    Task<bool> Touch(ItemId id);

    /// <summary>
    /// Tries to delete an item, returning true if an item found and marked for deletion
    /// </summary>
    Task<bool> Delete(ItemId id);

    /// <summary>
    /// Finds item/items that satisfy the queryExpression which is a JSON object of expression tree:
    /// `tbd`
    /// </summary>
    /// <param name="itemType">Type of item to query</param>
    /// <param name="queryExpression">JSON query object</param>
    /// <returns>Item enumeration</returns>
    Task<IEnumerable<Item>> Query(string itemType, string queryExpression);
  }

  public interface IDirectoryImplementation : IDirectory, IDataStoreImplementation
  {

  }

}
