using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azos.Data.Access;

namespace Azos.Data.Directory
{

  public static class DirectoryStandards
  {
    public const string ITEM_TYPE_PROFILE = "profile";
  }


  /// <summary>
  /// Represents a directory of string ID-keyed entities.
  /// Directories are used as a form of KEY-Value vector databases
  /// </summary>
  public interface IDirectory : IDataStore
  {
    /// <summary>
    /// Gets item directly by its primary key
    /// </summary>
    Task<Item> GetAsync(ItemId id, bool touch = false);

    /// <summary>
    /// Saves item to directory
    /// </summary>
    Task Save(Item item);

    /// <summary>
    /// Updates LastUseUtc timestamp for items that use sliding expiration
    /// </summary>
    Task<bool> Touch(ItemId id);

    /// <summary>
    /// Tries to delete an item, returning true if found and marked for deletion
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
