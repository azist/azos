/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;

using Azos.Conf;

using Azos.Serialization.JSON;

namespace Azos.Data
{
  /// <summary>
  /// Injects function that tries to set document field value.
  /// May elect to skip the set and return false to indicate failure(instead of throwing exception)
  /// </summary>
  public delegate bool SetFieldFunc(Doc doc, Schema.FieldDef fdef, object val);

  /// <summary>
  /// Marker interface - abstraction for data documents, interface used as constraint on other interfaces
  /// typically used in domain entity design. Doc class is the only one implementing this interface
  /// </summary>
  public interface IDataDoc : IOfSchema, IEquatable<Doc>, IEnumerable<Object>, IValidatable, IConfigurable, IConfigurationPersistent
  {
    /// <summary>
    /// Gets/sets field values by name
    /// </summary>
    object this[string fieldName] {  get; set; }

    /// <summary>
    /// Gets/sets field values by positional index(Order)
    /// </summary>
    object this[int fieldIdx]{  get; set; }
  }

  /// <summary>
  /// Base class for any data document. This class has two direct subtypes - DynamicDoc and TypedDoc.
  /// Documents are NOT THREAD SAFE by definition
  /// </summary>
  [Serializable, CustomMetadata(typeof(DocCustomMetadataProvider))]
  public abstract partial class Doc : IDataDoc, IJsonWritable, IJsonReadable, Apps.Injection.IApplicationInjection
  {

    #region Static

    /// <summary>
    /// Factory method that makes an appropriate data document type. For performance purposes,
    ///  this method does not check passed type for Doc-derivation and returns null instead if type was invalid
    /// </summary>
    /// <param name="schema">Schema, which is used for creation of DynamicDocs and their derivatives</param>
    /// <param name="tDoc">
    /// A type of doc to create, if the type is TypedDoc-descending then a parameterless .ctor is called,
    /// otherwise a type must have a .ctor that takes schema as a sole argument
    /// </param>
    /// <returns>
    /// Row instance or null if wrong type was passed. For performance purposes,
    ///  this method does not check passed type for Row-derivation and returns null instead if type was invalid
    /// </returns>
    public static Doc MakeDoc(Schema schema, Type tDoc = null)
    {
      if (tDoc != null && tDoc != typeof(Doc) && tDoc != typeof(DynamicDoc))
      {
          if (typeof(TypedDoc).IsAssignableFrom(tDoc))
              return Activator.CreateInstance(tDoc) as Doc;
          else                                         //todo Compile do dynamic functors for speed
              return Activator.CreateInstance(tDoc, schema) as Doc;
      }

      return new DynamicDoc(schema);
    }

    /// <summary>
    /// Tries to fill the document with data returning true if field count matched
    /// </summary>
    public static bool TryFillFromJSON(Doc doc, IJsonDataObject jsonData, SetFieldFunc setFieldFunc = null)
    {
      if (doc==null || jsonData==null) return false;

      var allMatch = true;
      var map = jsonData as JsonDataMap;
      if (map!=null)
      {
        foreach(var kvp in map)
        {
          var fdef = doc.Schema[kvp.Key];
          if (fdef==null)
          {
            var ad = doc as IAmorphousData;
            if (ad!=null && ad.AmorphousDataEnabled)
              ad.AmorphousData[kvp.Key] = kvp.Value;

            allMatch = false;
            continue;
          }

          if (setFieldFunc==null)
            doc.SetFieldValue(fdef, kvp.Value);
          else
          {
            var ok = setFieldFunc(doc, fdef, kvp.Value);
            if (!ok) allMatch = false;
          }
        }
        if (map.Count!=doc.Schema.FieldCount) allMatch = false;
      }
      else
      {
        var arr = jsonData as JsonDataArray;
        if (arr==null) return false;

        for(var i=0; i<doc.Schema.FieldCount; i++)
        {
            if (i==arr.Count) break;
            var fdef = doc.Schema[i];

            if (setFieldFunc==null)
              doc.SetFieldValue(fdef, arr[i]);
            else
            {
              var ok = setFieldFunc(doc, fdef, arr[i]);
              if (!ok) allMatch = false;
            }
        }
        if (arr.Count!=doc.Schema.FieldCount) allMatch = false;
      }

      return allMatch;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Returns schema object that contains doc field definitions
    /// </summary>
    public abstract Schema Schema { get; }

    /// <summary>
    /// Gets/sets field values by name
    /// </summary>
    public object this[string fieldName]
    {
      get
      {
        try
        {
          return GetFieldValue( Schema.GetFieldDefByName(fieldName) );
        }
        catch(Exception error)
        {
          throw new DataException(StringConsts.CRUD_FIELD_VALUE_GET_ERROR.Args(fieldName, error.ToMessageWithType()), error);
        }
      }
      set
      {
        try
        {
          SetFieldValue( Schema.GetFieldDefByName(fieldName), value);
        }
        catch(Exception error)
        {
          throw new DataException(StringConsts.CRUD_FIELD_VALUE_SET_ERROR.Args(fieldName, error.ToMessageWithType()), error);
        }
      }
    }

    /// <summary>
    /// Gets/sets field values by positional index(Order)
    /// </summary>
    public object this[int fieldIdx]
    {
      get
      {
        try
        {
            return GetFieldValue( Schema.GetFieldDefByIndex( fieldIdx ) );
        }
        catch(Exception error)
        {
            throw new DataException(StringConsts.CRUD_FIELD_VALUE_GET_ERROR.Args("["+fieldIdx+"]", error.ToMessageWithType()), error);
        }
      }
      set
      {
        try
        {
            SetFieldValue(Schema.GetFieldDefByIndex(fieldIdx), value);
        }
        catch(Exception error)
        {
            throw new DataException(StringConsts.CRUD_FIELD_VALUE_SET_ERROR.Args("["+fieldIdx+"]", error.ToMessageWithType()), error);
        }
      }
    }


    /// <summary>
    /// Returns values for fields that represent document/row primary key
    /// </summary>
    public Access.IDataStoreKey GetDataStoreKey(string targetName = null)
    {
      var result = new Access.NameValueDataStoreKey();
      foreach(var kdef in Schema.GetKeyFieldDefsForTarget(targetName))
          result.Add(kdef.GetBackendNameForTarget(targetName), this[kdef.Order]);

      return result;
    }

    #endregion

    #region Public

    /// <summary>
    /// In base class applies Config attribute. Useful for typed rows
    /// </summary>
    public virtual void Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
    }

    /// <summary>
    /// The base class does not implement this method. Override to persist row fields into config node
    /// </summary>
    public virtual void PersistConfiguration(ConfigSectionNode node)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Override to perform custom row equality comparison.
    /// Default implementation equates rows using their key fields
    /// </summary>
    public virtual bool Equals(Doc other)
    {
        if (other==null) return false;
        if (!this.Schema.IsEquivalentTo(other.Schema)) return false;

        foreach(var fdef in Schema.AnyTargetKeyFieldDefs)
        {
              var obj1 = this[fdef.Order];
              var obj2 = other[fdef.Order];
              if (obj1==null && obj2==null) continue;
              if (! obj1.Equals(obj2) ) return false;
        }

        return true;
    }

    /// <summary>
    /// Object override - sealed. Override Equals(doc) instead
    /// </summary>
    public sealed override bool Equals(object obj)
    {
        return this.Equals(obj as Doc);
    }

    /// <summary>
    /// Object override - gets hash code from key fields
    /// </summary>
    public override int GetHashCode()
    {
        var result = 0;
        foreach(var fdef in Schema.AnyTargetKeyFieldDefs)
        {
              var val = this[fdef.Order];
              if (val!=null) result+= val.GetHashCode();
        }
        return result;
    }


    /// <summary>
    /// Returns true if this row satisfies simple filter - it contains the supplied filter string.
    /// The filter pattern may start or end with "*" char that denotes a wildcard. A wildcard is permitted on both sides of the filter value
    /// </summary>
    public bool SimpleFilterPredicate(string filter, bool caseSensitive = false)
    {
      if (filter==null) return false;

      if (!caseSensitive)
        filter = filter.ToUpperInvariant();

      var sany = false;
      var eany = false;

      if (filter.StartsWith("*"))
      {
        sany = true;
        filter = filter.Remove(0, 1);
      }

      if (filter.EndsWith("*"))
      {
        eany = true;
        filter = filter.Remove(filter.Length-1, 1);
      }

      foreach(var val in this)
      {
          if (val==null) continue;

          var sval = val.ToString();

          if (!caseSensitive)
            sval = sval.ToUpperInvariant();

          if (sany && eany && sval.Contains(filter)) return true;

          if (!sany && eany && sval.StartsWith(filter)) return true;

          if (sany && !eany && sval.EndsWith(filter)) return true;

          if (!sany && !eany && sval==filter) return true;
      }

      return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new docFieldValueEnumerator(this);
    }

    public IEnumerator<Object> GetEnumerator()
    {
        return new docFieldValueEnumerator(this);
    }


    /// <summary>
    /// Gets value of the field, for typeddocs it accesses property using reflection; for dynamic rows it reads data from
    ///  row buffer array using field index(order)
    /// </summary>
    public abstract object GetFieldValue(Schema.FieldDef fdef);

    /// <summary>
    /// Sets value of the field, for typeddocs it accesses property using reflection/lambda; for dynamic rows it sets data into
    ///  row buffer array using field index(order)
    /// </summary>
    public abstract void SetFieldValue(Schema.FieldDef fdef, object value);

#warning This needs to be refactored without hard-coded types
    /// <summary>
    /// Converts field value to the type specified by Schema.FieldDef. For example converts GDID->ulong or ulong->GDID.
    /// This method can be overridden to perform custom handling of types,
    ///  for example one can assign bool field as "Si" that would convert to TRUE.
    /// This method is called by SetFieldValue(...) before assigning actual field buffer
    /// </summary>
    /// <param name="fdef">Field being converted</param>
    /// <param name="value">Value to convert</param>
    /// <returns>Converted value before assignment to field buffer</returns>
    public virtual object ConvertFieldValueToDef(Schema.FieldDef fdef, object value)
    {
      if (value==DBNull.Value) value = null;

      if (value == null) return null;

      var tv = value.GetType();

      if (tv != fdef.NonNullableType && !fdef.NonNullableType.IsAssignableFrom(tv))
      {

          if (value is ObjectValueConversion.TriStateBool)
          {
            var tsb = (ObjectValueConversion.TriStateBool)value;
            if (tsb==ObjectValueConversion.TriStateBool.Unspecified)
              value = null;
            else
              value = tsb==ObjectValueConversion.TriStateBool.True;

            return value;
          }

          if (fdef.NonNullableType==typeof(ObjectValueConversion.TriStateBool))
          {
            var nb = value.AsNullableBool();
            if (!nb.HasValue)
              value = ObjectValueConversion.TriStateBool.Unspecified;
            else
              value = nb.Value ? ObjectValueConversion.TriStateBool.True : ObjectValueConversion.TriStateBool.False;

            return value;
          }


          // 20150224 DKh, addedEra to GDID. Only GDIDS with ERA=0 can be converted to/from INT64
          if (fdef.NonNullableType==typeof(GDID))
          {
              if (tv==typeof(byte[]))//20151103 DKh GDID support for byte[]
                value = new GDID((byte[])value);
              else if (tv==typeof(string))//20160504 Spol GDID support for string
              {
                var sv = (string)value;
                if (sv.IsNotNullOrWhiteSpace())
                  value = GDID.Parse((string)value);
                else
                  value = fdef.Type == typeof(GDID?) ? (GDID?)null : GDID.ZERO;
              }
              else
                value = new GDID(0, (UInt64)Convert.ChangeType(value, typeof(UInt64)));

              return value;
          }

          if (tv==typeof(GDID))
          {
              if (fdef.NonNullableType==typeof(byte[]))
              {
                value = ((GDID)value).Bytes;
              }
              else if (fdef.NonNullableType==typeof(string))
              {
                value = value.ToString();
              }
              else
              {
                var gdid = (GDID)value;
                if (gdid.Era!=0)
                  throw new DataException(StringConsts.CRUD_GDID_ERA_CONVERSION_ERROR.Args(fdef.Name, fdef.NonNullableType.Name));
                value = gdid.ID;
              }

              return value;
          }

          // 20161026 Serge: handle values of enumerated field types
          if (fdef.NonNullableType.IsEnum)
          {
              value = value.AsString().AsType(fdef.NonNullableType);
              return value;
          }

          value = Convert.ChangeType(value, fdef.NonNullableType);

      }//Types Differ

      return value;
    }

    /// <summary>
    /// Writes default values specified in schema into fields.
    /// Pass overwrite=true to force defaults over non-null existing values (false by default)
    /// </summary>
    public void ApplyDefaultFieldValues(string targetName = null, bool overwrite = false)
    {
      foreach(var fdef in  Schema)
      {
        var attr = fdef[targetName];
        if (attr==null) continue;
        if (attr.Default!=null)
          if (overwrite || this.GetFieldValue(fdef)==null)
          this.SetFieldValue(fdef, attr.Default);
      }
    }


    /// <summary>
    /// Copies fields from this doc into another doc/form.
    /// Note: this is  shallow copy, as field values for complex types are just copied over
    /// </summary>
    public void CopyFields(Doc other,
                           string target = null,
                           bool includeAmorphousData = true,
                           bool invokeAmorphousAfterLoad = true,
                           Func<string, Schema.FieldDef, bool> fieldFilter = null,
                           Func<string, string, bool> amorphousFieldFilter = null)
    {
      if (other==null || object.ReferenceEquals(this, other)) return;

      if (target.IsNullOrWhiteSpace())
      {
        if (other is Form frm) target = frm.DataStoreTargetName;
        if (target==null) target = string.Empty;
      }

      var oad = includeAmorphousData ? other as IAmorphousData : null;

      if (oad!=null && this is IAmorphousData)
      {
        var athis = (IAmorphousData)this;
        if (oad.AmorphousDataEnabled && athis.AmorphousDataEnabled)//only copy IF amorphous behavior is enabled here and in the destination
        {
          foreach(var kvp in athis.AmorphousData)
          {
            if (amorphousFieldFilter!=null && !amorphousFieldFilter(target, kvp.Key)) continue;

            var ofd = other.Schema[kvp.Key];
            if (ofd!=null)
            {
              try
              {
                if (!isFieldDefLoaded(target, ofd)) continue;

                other.SetFieldValue(ofd, kvp.Value);
                continue;
              }catch{} //this may be impossible, then we assign to amorphous in other
            }
            oad.AmorphousData[kvp.Key] = kvp.Value;
          }
        }
      }

      foreach(var fdef in this.Schema)
      {
        if (fieldFilter!=null && !fieldFilter(target, fdef)) continue;

        if (!isFieldDefLoaded(target, fdef)) continue;

        var ofd = other.Schema[fdef.Name];
        if (ofd==null)
        {
          if (oad!=null && oad.AmorphousDataEnabled)// IF amorphous behavior is enabled in destination
          {
            oad.AmorphousData[fdef.Name] = GetFieldValue(fdef);
          }
          continue;
        }

        other.SetFieldValue(ofd, GetFieldValue(fdef));
      }//foreach

      if (oad!=null && oad.AmorphousDataEnabled && invokeAmorphousAfterLoad)
        oad.AfterLoad( target );
    }

    private bool isFieldDefLoaded(string target, Schema.FieldDef def)
    {
      var atr = def[target];
      return (atr==null) ? true : (atr.StoreFlag == StoreFlag.LoadAndStore || atr.StoreFlag == StoreFlag.OnlyLoad);
    }


    /// <summary>
    /// For fields with ValueList returns value's description per specified targeted schema
    /// </summary>
    public string GetFieldValueDescription(string fieldName, string targetName=null, bool caseSensitiveKeys=false)
    {
      var def = Schema[fieldName];
      if (def==null)
        throw new DataException(StringConsts.CRUD_FIELD_NOT_FOUND_ERROR.Args(fieldName, Schema));

      return def.ValueDescription( GetFieldValue(def), targetName, caseSensitiveKeys);
    }

    /// <summary>
    /// For fields with ValueList returns value's description per specified targeted schema
    /// </summary>
    public string GetFieldValueDescription(int fieldIndex, string targetName=null, bool caseSensitiveKeys=false)
    {
      var def = Schema[fieldIndex];
      if (def==null)
        throw new DataException(StringConsts.CRUD_FIELD_NOT_FOUND_ERROR.Args("[{0}]".Args(fieldIndex), Schema));

      return def.ValueDescription( GetFieldValue(def), targetName, caseSensitiveKeys);
    }


    /// <summary>
    /// Returns field value as string formatted per target DisplayFormat attribute
    /// </summary>
    public string GetDisplayFieldValue(string fieldName, string targetName=null, Func<object,object> transform = null)
    {
      var def = Schema[fieldName];
      if (def==null)
        throw new DataException(StringConsts.CRUD_FIELD_NOT_FOUND_ERROR.Args(fieldName, Schema));

      return getDisplayFieldValue(targetName, def, transform);
    }

    /// <summary>
    /// Returns field value as string formatted per target DisplayFormat attribute
    /// </summary>
    public string GetDisplayFieldValue(int fieldIndex, string targetName=null, Func<object, object> transform = null)
    {
      var def = Schema[fieldIndex];
      if (def==null)
        throw new DataException(StringConsts.CRUD_FIELD_NOT_FOUND_ERROR.Args("[{0}]".Args(fieldIndex), Schema));

      return getDisplayFieldValue(targetName, def, transform);
    }

          /// <summary>
          /// Returns field value as string formatted per target DisplayFormat attribute
          /// </summary>
          private string getDisplayFieldValue(string targetName, Schema.FieldDef fdef, Func<object, object> transform =null)
          {
              var value = GetFieldValue(fdef);
              if (value==null) return null;

              var atr = fdef[targetName];
              if (transform != null)
                value = transform(value);

              if (atr==null || atr.DisplayFormat.IsNullOrWhiteSpace())
                return value.ToString();

              return atr.DisplayFormat.Args(value);
          }


    /// <summary>
    /// Override to get a list of permissible field values for the specified field for the specified target.
    /// This method is used by validation to extract dynamic pick list entries form data stores
    /// as dictated by business logic. The override must be efficient and typically rely on caching of
    /// values gotten from the datastore. This method should NOT return more than a manageable limited number of records (e.g. less than 100)
    /// in a single form drop-down/combo, as the large lookups are expected to be implemented using complex lookup models (e.g. dialog boxes in UI).
    /// Return a null to indicate an absence of a value list for the specified field.
    /// Return an empty JsonDataMap to indicate that dynamic value list is present, but there is nothing to check against - this is used to
    /// override a static fdef.ValueList which would be enforced otherwise.
    /// </summary>
    public virtual JsonDataMap GetDynamicFieldValueList(Schema.FieldDef fdef,
                                                        string targetName,
                                                        Atom isoLang)
    {
      return null;
    }

    /// <summary>
    /// Override to perform dynamic substitute of field def for the specified field.
    /// This method is used by client ui/scaffolding to extract dynamic definition for a field
    /// (i.e. field description, requirement, value list etc.) as dictated by business logic.
    /// This method IS NOT used by doc validation, only by client that feeds from doc metadata.
    /// The default implementation returns the original field def, you can return a substituted field def
    ///  for a specific business logic need
    /// </summary>
    public virtual Schema.FieldDef GetClientFieldDef(Schema.FieldDef fdef,
                                                      string targetName,
                                                      Atom isoLang)
    {
      return fdef;
    }

    /// <summary>
    /// Override to perform dynamic substitute of field value for the specified field.
    /// This method is used by client ui/scaffolding to extract field values for a field as dictated by business logic.
    /// This method IS NOT used by doc validation, only by client that feeds from doc metadata.
    /// The default implementation returns the original GetFieldValue(fdef), you can return a substituted field value
    ///  per particular business logic
    /// </summary>
    public virtual object GetClientFieldValue(Schema.FieldDef fdef,
                                              string targetName,
                                              Atom isoLang)
    {
      return GetFieldValue(fdef);
    }

    #endregion

    #region IJSONWritable

    /// <summary>
    /// Writes doc as JSON either as an array or map depending on JSONWritingOptions.RowsAsMap setting.
    /// Do not call this method directly, instead call rowset.ToJSON() or use JSONWriter class.
    /// Override to perform custom JSOn serialization
    /// </summary>
    public virtual void WriteAsJson(System.IO.TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
    {
      if (options==null || !options.RowsAsMap)
      {
        JsonWriter.WriteArray(wri, this, nestingLevel, options);
        return;
      }

      var map = new Dictionary<string, object>();

      foreach(var fd in Schema)
      {
        string name;

        var val = FilterJsonSerializerField(fd, options, out name);
        if (name.IsNullOrWhiteSpace()) continue;//field was excluded for Json serialization

        AddJsonSerializerField(fd, options, map, name, val);
      }

      if (this is IAmorphousData amorph)
      {
        if (amorph.AmorphousDataEnabled)
        {
          foreach(var kv in amorph.AmorphousData)
          {
            var key = kv.Key;
            while(map.ContainsKey(key)) key += "_";
            AddJsonSerializerField(null, options, map, key, kv.Value);
          }
        }
      }

      JsonWriter.WriteMap(wri, map, nestingLevel, options);//perform actual Json writing
    }

    /// <summary>
    /// Override to perform custom deserialization from Json.
    /// The default implementation delegates the work to JsonReader class
    /// </summary>
    /// <param name="data">Data to deserialize, must be JsonDataMap to succeed</param>
    /// <param name="fromUI">True is passed when the deserialization is coming from a datagram supplied by user interface</param>
    /// <param name="options">Specifies how to bind names</param>
    /// <returns>Tuple of (bool match, IJsonReadable self)</returns>
    public virtual (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is JsonDataMap map)
      {
        JsonReader.ToDoc(this, map, fromUI, options);
        return (true, this);
      }
      return (false, this);
    }

    #endregion

    #region Protected

    /// <summary>
    /// Override to filter-out some fields from serialization to JSON, or change field values.
    /// Return name null to indicate that field should be filtered-out(excluded from serialization to JSON)
    /// </summary>
    protected virtual object FilterJsonSerializerField(Schema.FieldDef def, JsonWritingOptions options, out string name)
    {
      var tname = options!=null ? options.RowMapTargetName : null;

      if (tname.IsNotNullOrWhiteSpace())
      {
        FieldAttribute attr;
        name = def.GetBackendNameForTarget(tname, out attr);
        if (attr!=null)
        {
          if(attr.StoreFlag==StoreFlag.None || attr.StoreFlag==StoreFlag.OnlyLoad)
          {
            name = null;
            return null;
          }

          var dflt = attr.Default;
          if (dflt!=null)
          {
            var value = GetFieldValue(def);
            if (dflt.Equals(value))
            {
              name = null;
              return null;
            }
            return value;
          }
        }
      }
      else
        name = def.Name;

      return name.IsNotNullOrWhiteSpace() ? GetFieldValue(def) : null;
    }

    /// <summary>
    /// Override to perform custom transformation of value/add extra values to JSON output map.
    /// For example, this is used to normalize phone numbers by adding a field with `_normalized` suffix to every field containing a phone.
    /// Default base implementation just writes value into named map key. FieldDef is null for amorphous fields
    /// </summary>
    protected virtual void AddJsonSerializerField(Schema.FieldDef def, JsonWritingOptions options, Dictionary<string, object> jsonMap, string name, object value)
    {
      jsonMap[name] = value;
    }

    #endregion



    #region .pvt

    private struct docFieldValueEnumerator : IEnumerator<object>
    {
      internal docFieldValueEnumerator(Doc doc)
      {
        m_Index = -1;
        m_Doc = doc;
      }

      public void Dispose() { }

      private int m_Index;
      private Doc m_Doc;

      public object Current => m_Doc[m_Index];

      public bool MoveNext()
      {
          if (m_Index == m_Doc.Schema.FieldCount-1) return false;
          m_Index++;
          return true;
      }

      public void Reset()
      {
          m_Index = -1;
      }
    }

    #endregion
  }

}
