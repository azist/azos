/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.Data
{

  /// <summary>
  /// Provides filter predicate for CRUD operations. Return true to include the specified field
  /// </summary>
  /// <param name="doc">Data document instance that filtering is performed on</param>
  /// <param name="key">If not null, the override key passed to Update() (if any)</param>
  /// <param name="fdef">A field that filtering is done for</param>
  public delegate bool FieldFilterFunc(Doc doc, Access.IDataStoreKey key, Schema.FieldDef fdef);

  /// <summary>
  /// Denotes an entity that supports validation
  /// </summary>
  public interface IValidatable
  {
    /// <summary>
    /// Validates entity state per particular named target, for performance reasons returns validation exception (instead of throwing)
    /// </summary>
    Exception Validate(string targetName);
  }

  /// <summary>
  /// Denotes an entity, which is typically a row-derivative, that has extra data fields that are not
  /// defined by particular schema and get represented as {name:value} map instead (schema-less data).
  /// This interface is usually implemented by rows that support version changing between releases, i.e. when
  /// structured storage (such as Mongo DB) stores more fields than are currently declared in the row the extra fields will be placed
  ///  in the AmorphousData collection. This interface also provides hook BeforeSave()/AfterLoad() that allow for transforms between
  ///  Amorphous and "hard-schema" data models
  /// </summary>
  public interface IAmorphousData
  {
    /// <summary>
    /// When true, enabled amorphous data behaviour, i.e. copying of amorphous data between rows.
    /// When false, the amorphous data is ignored as-if the type did not implement this interface
    /// This is needed for security, i.e. on the web returning false will prevent injection via posted forms
    /// </summary>
    bool AmorphousDataEnabled { get; }


    /// <summary>
    /// Returns data that does not comply with known schema (dynamic data).
    /// The field names are NOT case-sensitive
    /// </summary>
    IDictionary<string, object> AmorphousData { get; }

    /// <summary>
    /// Invoked to allow the entity (such as a row) to transform its state into AmorphousData bag.
    /// For example, this may be useful to store extra data that is not a part of established business schema.
    /// The operation is performed per particular targetName (name of physical backend). Simply put, this method allows
    ///  business code to "specify what to do before object gets saved in THE PARTICULAR TARGET backend store"
    /// </summary>
    void BeforeSave(string targetName);

    /// <summary>
    /// Invoked to allow the entity (such as a row) to hydrate its fields/state from AmorphousData bag.
    /// For example, this may be used to reconstruct some temporary object state that is not stored as a part of established business schema.
    /// The operation is performed per particular targetName (name of physical backend).
    /// Simply put, this method allows business code to "specify what to do after object gets loaded from THE PARTICULAR TARGET backend store".
    /// An example: suppose current MongoDB collection stores 3 fields for name, and we want to collapse First/Last/Middle name fields into one field.
    /// If we change rowschema then it will only contain 1 field which is not present in the database, however those 'older' fields will get populated
    /// into AmorphousData giving us an option to merge older 3 fields into 1 within AfterLoad() implementation
    /// </summary>
    void AfterLoad(string targetName);
  }



  /// <summary>
  /// Supplies caching params
  /// </summary>
  public interface ICacheParams
  {
    /// <summary>
    /// If greater than 0 then would allow reading a cached result for up-to the specified number of seconds.
    /// If =0 uses cache's default span.
    /// Less than 0 does not try to read from cache
    /// </summary>
    int ReadCacheMaxAgeSec { get; }

    /// <summary>
    /// If greater than 0 then writes to cache with the expiration.
    /// If =0 uses cache's default life span.
    /// Less than 0 does not write to cache
    /// </summary>
    int WriteCacheMaxAgeSec { get; }

    /// <summary>
    /// Relative cache priority which is used when WriteCacheMaxAgeSec>=0
    /// </summary>
    int WriteCachePriority { get; }

    /// <summary>
    /// When true would cache the instance of AbsentData to signify the absence of data in the backend for key
    /// </summary>
    bool CacheAbsentData { get; }
  }
}
