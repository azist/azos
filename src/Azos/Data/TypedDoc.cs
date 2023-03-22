/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

namespace Azos.Data
{
  /// <summary>
  /// Represents a type-safe data document with schema known at compile-time.
  /// Typed docs store data in instance fields, providing better performance and schema definition compile-time checking than DynamicDocs
  /// at the expense of inability to define schema at runtime
  /// </summary>
  [Serializable]
  public abstract class TypedDoc : Doc
  {
    #region .ctor

    protected TypedDoc()
    {
    }

    #endregion

    #region Fields

    [NonSerialized]
    private Schema m_CachedSchema;

    #endregion

    #region Properties

    /// <summary>
    /// References a schema of this data document
    /// </summary>
    public sealed override Schema Schema
    {
      get
      {
        if (m_CachedSchema == null)
          m_CachedSchema = Schema.GetForTypedDoc(this);

        return m_CachedSchema;
      }
    }

    #endregion

    #region Public

    public override object GetFieldValue(Schema.FieldDef fdef)
    {
      var pinf = fdef.MemberInfo;
      var result = pinf.GetValue(this, null);

      if (result == DBNull.Value) result = null;

      return result;
    }

    public override void SetFieldValue(Schema.FieldDef fdef, object value)
    {
      value = ConvertFieldValueToDef(fdef, value);
      fdef.SetPropertyValue(this, value);
    }

    #endregion
  }


  /// <summary>
  /// Represents a type-safe data document when schema is known at compile-time that also implements IAmorphousData
  /// interface that allows this row to store "extra" data that does not comply with the current schema.
  /// Typed docs store data in instance fields, providing better performance and schema definition compile-time checking than DynamicDocs
  /// at the expense of inability to define schema at runtime
  /// </summary>
  [Serializable]
  public abstract class AmorphousTypedDoc : TypedDoc, IAmorphousData
  {
    protected AmorphousTypedDoc()
    {
    }

    private Dictionary<string, object> m_AmorphousData;

    /// <summary>
    /// True by default for rows
    /// </summary>
    public virtual bool AmorphousDataEnabled => true;

    /// <summary>
    /// Returns data that does not comply with known schema (dynamic data). The field names are NOT case-sensitive
    /// </summary>
    public IDictionary<string, object> AmorphousData
    {
      get
      {
        if (m_AmorphousData == null)
          m_AmorphousData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        return m_AmorphousData;
      }
    }

    void IAmorphousData.BeforeSave(string targetName) => DoAmorphousDataBeforeSave(targetName);
    void IAmorphousData.AfterLoad(string targetName) => DoAmorphousDataAfterLoad(targetName);

    /// <summary>
    /// Invoked to allow the doc to transform its state into AmorphousData bag.
    /// For example, this may be useful to store extra data that is not a part of established business schema.
    /// The operation is performed per particular targetName (name of physical backend). Simply put, this method allows
    ///  business code to "specify what to do with amorphous data before object gets saved in THE PARTICULAR TARGET backend store"
    /// </summary>
    protected virtual void DoAmorphousDataBeforeSave(string targetName)
    {

    }

    /// <summary>
    /// Invoked to allow the doc to hydrate its fields/state from AmorphousData bag.
    /// For example, this may be used to reconstruct some temporary object state that is not stored as a part of established business schema.
    /// The operation is performed per particular targetName (name of physical backend).
    /// Simply put, this method allows business code to "specify what to do with amorphous data after object gets loaded from THE PARTICULAR TARGET backend store".
    /// An example: suppose current MongoDB collection stores 3 fields for name, and we want to collapse First/Last/Middle name fields into one field.
    /// If we change schema then it will only contain 1 field which is not present in the database, however those 'older' fields will get populated
    /// into AmorphousData giving us an option to merge older 3 fields into 1 within AfterLoad() implementation
    /// </summary>
    protected virtual void DoAmorphousDataAfterLoad(string targetName)
    {

    }

  }
}
