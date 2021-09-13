/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;

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
  /// Implemented by entities that check whether their state has the required data or not, possibly conditionally
  /// depending on a targetName.
  /// This interface is typically implemented by structs, which may be assigned, but not representing a logical assignment.
  /// For example, a GDID.Zero being assigned as a value, does not represent a required data value (Zero is invalid).
  /// You can also implement this on classes, so even if an object instance of implementing class is not null, that object may not represent
  /// a required value per given target(system/use-case).
  /// </summary>
  public interface IRequiredCheck
  {
    /// <summary>
    /// Checks required state for the supplied target. Returns true if an instance state has the required value
    /// </summary>
    /// <param name="targetName">
    /// The target scope under which  the state check is done.
    /// Most implementations do not depend on targets and just ignore the value</param>
    /// <returns>
    /// True if this instance state contains the required value.
    /// False when this instance does not have required data for the specified target, for example a struct is in a default state
    /// and is effectively not logically assigned
    /// </returns>
    bool CheckRequired(string targetName);
  }


  /// <summary>
  /// Implemented by entities that check their state/value length expressed in logical units (e.g. characters), possibly conditionally
  /// depending on a targetName
  /// </summary>
  public interface ILengthCheck
  {
    /// <summary>
    /// Checks instance adherence to logical length limits for the supplied target.
    /// Returns true if an instance state represents the logically minimum length
    /// </summary>
    /// <param name="targetName">The target scope under which  the state check is done.
    /// Most implementations do not depend on targets and just ignore the value</param>
    /// <param name="minLength">
    /// The minimum required length. This method is not called with values less than one
    /// </param>
    /// <returns>
    /// True if this instance state contains a value of at least the minimum required length
    /// </returns>
    bool CheckMinLength(string targetName, int minLength);

    /// <summary>
    /// Checks instance adherence to logical length limits for the supplied target.
    /// Returns true if an instance state does not exceed the logically maximum length
    /// </summary>
    /// <param name="targetName">The target scope under which  the state check is done.
    /// Most implementations do not depend on targets and just ignore the value</param>
    /// <param name="maxLength">
    /// The maximum allowed length. This method is not called with values less than one
    /// </param>
    /// <returns>
    /// True if this instance state contains a value of at most the maximum allowed length
    /// </returns>
    bool CheckMaxLength(string targetName, int maxLength);
  }


  /// <summary>
  /// Defines entities that support custom validation logic
  /// </summary>
  public interface IValidatable
  {
    /// <summary>
    /// Validates entity state per particular validation state context, for performance reasons returns
    /// validation state as a struct initialized with a single or a of batch exception (instead of throwing).
    /// </summary>
    /// <param name="state">The validation state that should be used and returned back</param>
    /// <param name="scope">
    /// An optional name of the scope which calls/causes this validation.
    /// For example, this may be a field name when a custom field value type (e.g. a struct or a nested Doc) get validated
    /// as a part of larger data structure. It is used to report field/collection subscript of value location
    /// e.g. "Products[2]" typically in nested data types. An implementation may use the scope name to include it in the error
    /// text/field to point to the error location more accurately
    /// </param>
    /// <remarks>
    /// This design is done for allocation-free validation of very many entities (e.g. multiple 100K/sec data documents).
    /// When entities pass validation (in a most common case), no heap allocations are necessary.
    /// An optional scope name is passed as a separate parameter from ValidState because in practice it is needed mostly for
    /// nested IValidatable such as custom data types so they can report a name of the field that contains them
    /// </remarks>
    ValidState Validate(ValidState state, string scope = null);
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
    /// When true, enabled amorphous data behavior, i.e. copying of amorphous data between rows.
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
  /// Supplies caching parameters
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


  /// <summary>
  /// Denotes entities that have schemas, such as rowsets and data Docs
  /// </summary>
  public interface IOfSchema
  {
    /// <summary>
    /// Defines data schema for this instance
    /// </summary>
    Schema Schema{  get; }
  }
}
