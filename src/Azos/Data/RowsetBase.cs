/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Serialization.JSON;

namespace Azos.Data
{
  /// <summary>
  /// Provides base for rowsets.
  /// Rowsets are mutable lists of documents where all documents(rows) must adhere to the same schema (hence called "rows"),
  /// however a rowset may contain a mix of dynamic and typed documents as long as they have the same schema.
  /// Rowsets are not thread-safe
  /// </summary>
  [Serializable]
  public abstract class RowsetBase : IOfSchema, IList<Doc>, IComparer<Doc>, IJsonWritable, IValidatable
  {
    #region .ctor/static

    /// <summary>
    /// Reads either Table or Rowset from JSON created by WriteAsJSON. Metadata must be present
    /// </summary>
    public static RowsetBase FromJSON(string json, bool schemaOnly = false, bool readOnlySchema = false)
    {
      return FromJSON( JsonReader.DeserializeDataObject(json) as JsonDataMap, schemaOnly, readOnlySchema );
    }


    /// <summary>
    /// Reads either Table or Rowset from JSON created by WriteAsJSON. Metadata must be present
    /// </summary>
    public static RowsetBase FromJSON(JsonDataMap jsonMap, bool schemaOnly = false, bool readOnlySchema = false)
    {
      bool dummy;
      return FromJSON(jsonMap, out dummy, schemaOnly, readOnlySchema);
    }

    /// <summary>
    /// Reads either Table or Rowset from JSON created by WriteAsJSON. Metadata must be present.
    /// allMatched==false when some data did not match schema (i.e. too little fields or extra fields supplied)
    /// </summary>
    public static RowsetBase FromJSON(JsonDataMap jsonMap,
                                      out bool allMatched,
                                      bool schemaOnly = false,
                                      bool readOnlySchema = false,
                                      SetFieldFunc setFieldFunc = null)
    {
      if (jsonMap == null || jsonMap.Count == 0)
        throw new DataException(StringConsts.ARGUMENT_ERROR + "RowsetBase.FromJSON(jsonMap=null)");

      var schMap = jsonMap["Schema"] as JsonDataMap;
      if (schMap==null)
        throw new DataException(StringConsts.ARGUMENT_ERROR + "RowsetBase.FromJSON(jsonMap!schema)");

      var schema = Schema.FromJSON(schMap, readOnlySchema);
      var isTable = jsonMap["IsTable"].AsBool();

      allMatched = true;
      var result = isTable ? (RowsetBase)new Table(schema) : (RowsetBase)new Rowset(schema);
      if (schemaOnly) return result;

      var rows = jsonMap["Rows"] as JsonDataArray;
      if (rows==null) return result;


      foreach(var jrow in rows)
      {
        var jdo = jrow as IJsonDataObject;
        if (jdo==null)
        {
          allMatched = false;
          continue;
        }

        var doc = new DynamicDoc(schema);

        if (!Doc.TryFillFromJSON(doc, jdo, setFieldFunc)) allMatched = false;

        result.Add( doc );
      }

      return result;
    }

    /// <summary>
    /// Reads either Table or Rowset from JSON created by WriteAsJSON.
    /// </summary>
    /// <returns>Total number of rows found in JSON. If this number is less than
    /// result.Count, then not all rows matched the schema of the resulting rowset.</returns>
    /// <remarks>
    /// The schema of "result" must match the schema of the typed row T.
    /// It's the responsibility of the caller to clear the "result" prior to
    /// calling this function - the function appends rows to existing rowset.
    /// </remarks>
    public static int FromJSON<T>(string json,
                                  ref RowsetBase result,
                                  SetFieldFunc setFieldFunc = null)
      where T : TypedDoc, new()
    {
      var map = JsonReader.DeserializeDataObject(json) as JsonDataMap;
      return FromJSON<T>(map, ref result, setFieldFunc);
    }

    /// <summary>
    /// Reads either Table or Rowset from JSON created by WriteAsJSON.
    /// </summary>
    /// <returns>Total number of rows found in JSON. If this number is less than
    /// result.Count, then not all rows matched the schema of the resulting rowset.</returns>
    /// <remarks>
    /// The schema of "result" must match the schema of the typed row T.
    /// It's the responsibility of the caller to clear the "result" prior to
    /// calling this function - the function appends rows to existing rowset.
    /// </remarks>
    public static int FromJSON<T>(JsonDataMap jsonMap,
                                  ref RowsetBase result,
                                  SetFieldFunc setFieldFunc = null)
      where T : TypedDoc, new()
    {
      if (jsonMap == null || jsonMap.Count == 0)
        throw new DataException(StringConsts.ARGUMENT_ERROR + "RowsetBase.FromJSON(jsonMap=null)");
      if (result == null)
        throw new DataException(StringConsts.ARGUMENT_ERROR + "RowsetBase.FromJSON(result=null)");

      var typedDoc = new T();
      if (result.Schema != typedDoc.Schema)
        throw new DataException(StringConsts.ARGUMENT_ERROR + "RowsetBase.FromJSON(): invalid result schema");

      var rows = jsonMap["Rows"] as JsonDataArray;
      if (rows==null) return 0;

      foreach(var jrow in rows)
      {
        var jdo = jrow as IJsonDataObject;
        if (jdo==null)
          continue;

        var doc = new T();

        if (Doc.TryFillFromJSON(doc, jdo, setFieldFunc))
          result.Add(doc);
      }

      return rows.Count;
    }

    /// <summary>
    /// Creates an empty rowset
    /// </summary>
    protected RowsetBase(Schema schema)
    {
        m_Schema = schema;
        m_InstanceGUID = Guid.NewGuid();
    }

  #endregion

  #region Fields

    private Guid m_InstanceGUID;
    protected Schema m_Schema;
    protected internal List<Doc> m_List;
    protected internal List<DocChange> m_Changes;

    private JsonDynamicObject m_DataContext;
  #endregion


  #region Properties

    /// <summary>
    /// Returns globaly-unique instance ID.
    /// This ID is useful as a key for storing rowsets in object stores and posting data back from web client to server.
    /// </summary>
    public Guid InstanceGUID { get { return m_InstanceGUID;} }


    /// <summary>
    /// Returns a schema for rows that this rowset contains
    /// </summary>
    public Schema Schema { get { return m_Schema;} }

    /// <summary>
    /// Returns row count in this rowset
    /// </summary>
    public int Count { get { return m_List.Count; }}


    /// <summary>
    /// Returns data as non-generic readonly IList
    /// </summary>
    public System.Collections.IList AsReadonlyIList{ get{ return new iListReadOnly(m_List);}}

    /// <summary>
    /// Gets/Sets whether this rowset keeps track of all modifications done to it.
    /// This property must be set to true to be able to save changes into ICRUDDataStore
    /// </summary>
    public bool LogChanges
    {
        get { return m_Changes != null; }
        set
        {
            if (value && m_Changes==null) m_Changes = new List<DocChange>();
            else
            if (!value && m_Changes!=null) m_Changes = null;
        }
    }

    /// <summary>
    /// Returns accumulated modifications performed on the rowset, or empty enumerator if no modifications have been made or
    ///  LogModifications = false
    /// </summary>
    public IEnumerable<DocChange> Changes
    {
        get { return m_Changes ?? Enumerable.Empty<DocChange>(); }
    }

    /// <summary>
    /// Returns a count of accumulated modifications performed on the rowset, or zero when no modifications have been made or
    ///  LogModifications = false
    ///  </summary>
    public int ChangeCount
    {
        get { return m_Changes!=null?m_Changes.Count : 0; }
    }


    /// <summary>
    /// Provides dynamic view as JSONDataMap of rowset's data context - attributes applicable to the whole rowset
    /// </summary>
    public JsonDataMap ContextMap
    {
        get
        {
          var data = this.Context;//laizily created if needed

          return m_DataContext.Data as JsonDataMap;
        }
    }

    /// <summary>
    /// Provides dynamic view of rowset's data context - attributes applicable to the whole rowset
    /// </summary>
    public dynamic Context
    {
        get
        {
          if (m_DataContext==null)
            m_DataContext = new JsonDynamicObject(JSONDynamicObjectKind.Map, false);

          return m_DataContext;
        }
    }

  #endregion

  #region Public

    /// <summary>
    /// Inserts the doc. Returns insertion index
    /// </summary>
    public int Insert(Doc doc)
    {
        Check(doc);
        var idx = DoInsert(doc);
        if (idx>=0 && m_Changes!=null) m_Changes.Add( new DocChange(DocChangeType.Insert, doc, null) );
        return idx;
    }


    /// <summary>
    /// Updates the doc, Returns the row index or -1
    /// </summary>
    public UpdateResult Update(Doc doc, Access.IDataStoreKey key = null, Func<Doc, Doc, Doc> docUpgrade = null)
    {
        Check(doc);
        var idx = DoUpdate(doc, key, docUpgrade);
        if (idx>=0 && m_Changes!=null) m_Changes.Add( new DocChange(DocChangeType.Update, doc, key) );
        return new UpdateResult(idx, true);
    }


    /// <summary>
    /// Tries to find a doc for update and if found, updates it and returns true,
    ///  otherwise inserts the doc (if schemas match) and returns false. Optionally pass updateWhere condition
    ///   that may check whether update needs to be performed
    /// </summary>
    public UpdateResult Upsert(Doc doc, Func<Doc, bool> updateWhere = null, Func<Doc, Doc, Doc> rowUpgrade = null)
    {
        Check(doc);
        var update = DoUpsert(doc, updateWhere, rowUpgrade);
        if (m_Changes!=null) m_Changes.Add( new DocChange(DocChangeType.Upsert, doc, null) );
        return update;
    }

    /// <summary>
    /// Tries to find a row with the same set of key fields in this table and if found, deletes it and returns its index, otherwise -1
    /// </summary>
    public int Delete(Doc doc, Access.IDataStoreKey key = null)
    {
        Check(doc);
        var idx = DoDelete(doc, key);

        if (idx>=0 && m_Changes!=null)
          m_Changes.Add( new DocChange(DocChangeType.Delete, doc, null) );

        return idx;
    }

    /// <summary>
    /// Tries to find a row with the same set of key fields in this table and if found, deletes it and returns its index, otherwise -1
    /// </summary>
    public int Delete(params object[] keys)
    {
        return Delete(KeyDocFromValues(keys));
    }

    /// <summary>
    /// Deletes all rows from table without logging the deleted modifications even when LogModifications=true
    /// </summary>
    public void Purge()
    {
        m_List.Clear();
        m_List.TrimExcess();
    }

    /// <summary>
    /// Deletes all rows from table. This method is similar to Purge() but does logging (when enabled)
    /// </summary>
    public void DeleteAll()
    {
        if (m_Changes!=null)
            foreach(var row in m_List)
              m_Changes.Add( new DocChange(DocChangeType.Delete, row, null) );

        Purge();
    }

    /// <summary>
    /// Clears modifications accumulated by this instance
    /// </summary>
    public void PurgeChanges()
    {
        if (m_Changes!=null) m_Changes.Clear();
    }

    /// <summary>
    /// Creates key row out of field values for keys
    /// </summary>
    public Doc KeyDocFromValues(params object[] keys)
    {
        return DoKeyDocFromValues(new DynamicDoc(Schema), keys);
    }

    /// <summary>
    /// Tries to find a row by specified keyset and returns it or null if not found
    /// </summary>
    public Doc FindByKey(Doc keyDoc)
    {
        return FindByKey(keyDoc, null);
    }

    /// <summary>
    /// Tries to find a row by specified keyset and returns it or null if not found.
    /// This method does not perform well on Rowsets instances as a rowset is unordered list which does linear search.
    /// In contrast, Tables are always ordered and perform binary search instead
    /// </summary>
    public Doc FindByKey(params object[] keys)
    {
        return FindByKey(KeyDocFromValues(keys), null);
    }


    /// <summary>
    /// Tries to find a row by specified keyset and extra WHERE clause and returns it or null if not found.
    /// This method does not perform well on Rowsets instances as a rowset is unordered list which does linear search.
    /// In contrast, Tables are always ordered and perform binary search instead
    /// </summary>
    public Doc FindByKey(Doc doc, Func<Doc, bool> extraWhere)
    {
        Check(doc);

        int insertIndex;
        int idx = SearchForDoc(doc, out insertIndex);
        if (idx<0)
          return null;

        if (extraWhere!=null)
        {
          if (!extraWhere(m_List[idx])) return null;//where did notmatch
        }

        return m_List[idx];
    }

    /// <summary>
    /// Tries to find a doc index by specified keyset and extra WHERE clause and returns it or null if not found
    /// </summary>
    public Doc FindByKey(Func<Doc, bool> extraWhere, params object[] keys)
    {
        return FindByKey(KeyDocFromValues(keys), extraWhere);
    }


    /// <summary>
    /// Tries to find a doc index by specified keyset and returns it or null if not found
    /// </summary>
    public int FindIndexByKey(Doc keyDoc)
    {
        return FindIndexByKey(keyDoc, null);
    }

    /// <summary>
    /// Tries to find a row index by specified keyset and returns it or null if not found.
    /// This method does not perform well on Rowsets instances as a rowset is unordered list which does linear search.
    /// In contrast, Tables are always ordered and perform binary search instead
    /// </summary>
    public int FindIndexByKey(params object[] keys)
    {
        return FindIndexByKey(KeyDocFromValues(keys), null);
    }


    /// <summary>
    /// Tries to find a row index by specified keyset and extra WHERE clause and returns it or null if not found.
    /// This method does not perform well on Rowsets instances as a rowset is unordered list which does linear search.
    /// In contrast, Tables are always ordered and perform binary search instead
    /// </summary>
    public int FindIndexByKey(Doc doc, Func<Doc, bool> extraWhere)
    {
        Check(doc);

        int insertIndex;
        int idx = SearchForDoc(doc, out insertIndex);
        if (idx<0)
          return idx;

        return extraWhere != null && !extraWhere(m_List[idx]) ? -1 : idx;
    }

    /// <summary>
    /// Tries to find a row index by specified keyset and extra WHERE clause and returns it or null if not found
    /// </summary>
    public int FindIndexByKey(Func<Doc, bool> extraWhere, params object[] keys)
    {
        return FindIndexByKey(KeyDocFromValues(keys), extraWhere);
    }

    /// <summary>
    /// Retrievs a change by index or null if index is out of bounds or changes are not logged
    /// </summary>
    public DocChange? GetChangeAt(int idx)
    {
      if (m_Changes==null) return null;

      if (idx>=m_Changes.Count) return null;

      return m_Changes[idx];
    }

    /// <summary>
    /// Validates all rows in this rowset.
    /// Override to perform custom validations.
    /// The method is not expected to throw exception in case of failed validation, rather return exception instance because
    ///  throwing exception really hampers validation performance when many rows need to be validated
    /// </summary>
    public virtual ValidState Validate(ValidState state, string scope = null)
    {
        foreach(var row in this)
        {
            state = row.Validate(state, scope);
            if (state.ShouldStop) break;
        }

        return state;
    }


  #endregion

  #region IComparer<Row> Members

  /// <summary>
  /// Compares two rows
  /// </summary>
  public abstract int Compare(Doc docA, Doc docB);
  #endregion


  #region IEnumerable Members

    public IEnumerator<Doc> GetEnumerator()
    {
      return m_List.GetEnumerator();
    }


    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return m_List.GetEnumerator();
    }
  #endregion


  #region IList

    public int IndexOf(Doc doc)
    {
        Check(doc);
        return m_List.IndexOf(doc);
    }

    /// <summary>
    /// Inserts row at index
    /// </summary>
    public virtual void Insert(int index, Doc doc)
    {
        Check(doc);
        m_List.Insert(index, doc);
        if (m_Changes!=null) m_Changes.Add( new DocChange(DocChangeType.Insert, doc, null) );
    }

    /// <summary>
    /// Deletes row
    /// </summary>
    public void RemoveAt(int index)
    {
        Delete( m_List[index] );
    }

    /// <summary>
    /// This method performs update on set
    /// </summary>
    public virtual Doc this[int index]
    {
        get
        {
            return m_List[index];
        }
        set
        {
            Check(value);
            var existing = m_List[index];
            if (object.ReferenceEquals(existing, value)) return;
            m_List[index] = value;
            if (m_Changes!=null)
            {
              m_Changes.Add( new DocChange(DocChangeType.Delete, existing, null) );
              m_Changes.Add( new DocChange(DocChangeType.Insert, value, null) );
            }
        }
    }

    /// <summary>
    /// Inserts a row
    /// </summary>
    public void Add(Doc doc)
    {
        Insert(doc);
    }

    /// <summary>
    /// Purges table
    /// </summary>
    public void Clear()
    {
        Purge();
    }

    public bool Contains(Doc doc)
    {
        return m_List.Contains(doc);
    }

    public void CopyTo(Doc[] array, int arrayIndex)
    {
        m_List.CopyTo(array, arrayIndex);
    }

    public bool IsReadOnly
    {
        get { return false; }
    }

    /// <summary>
    /// Performs row delete
    /// </summary>
    public bool Remove(Doc item)
    {
        return Delete(item)>=0;
    }
  #endregion


  #region IJSONWritable

    /// <summary>
    /// Writes rowset as JSON including schema information. Do not call this method directly, instead call rowset.ToJSON() or use JSONWriter class
    /// </summary>
    public void WriteAsJson(System.IO.TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
    {
        var tp = GetType();

        var metadata = options!=null && options.RowsetMetadata;

        var map = new Dictionary<string, object>//A root JSON object is needed for security, as some browsers can EXECUTE a valid JSON root array
        {
          {"Rows", m_List}
        };

        if (metadata)
        {
            map.Add("Instance", m_InstanceGUID.ToString("D"));
            map.Add("Type", tp.FullName);
            map.Add("IsTable", typeof(Table).IsAssignableFrom( tp ));
            map.Add("Schema", m_Schema);
        }

        JsonWriter.WriteMap(wri, map, nestingLevel, options);
    }


  #endregion


  #region .Protected

    /// <summary>
    /// Checks argument for being non-null and of the same schema with this rowset
    /// </summary>
    protected void Check(Doc doc)
    {
      if (doc == null || m_Schema != doc.Schema)
        throw new DataException(StringConsts.CRUD_ROWSET_OPERATION_ROW_IS_NULL_OR_SCHEMA_MISMATCH_ERROR);
    }


    /// <summary>
    /// Provides rowsearching. Override to do binary search in sorted rowsets
    /// </summary>
    /// <param name="doc">A doc to search for</param>
    /// <param name="index">An index where search collapsed without finding the match. Used for sorted insertions</param>
    protected virtual int SearchForDoc(Doc doc, out int index)
    {
      for(int i=0; i<m_List.Count; i++)
      {
        if (m_List[i].Equals(doc))
        {
              index = i;
              return i;
        }
      }

      index = m_List.Count;
      return -1;
    }


    /// <summary>
    /// Tries to insert a document. If another doc with the same set of key fields already in the table returns -1, otherwise
    ///  returns insertion index
    /// </summary>
    protected virtual int DoInsert(Doc doc)
    {
        int idx = 0;
        if (SearchForDoc(doc, out idx) >=0) return -1;

        m_List.Insert(idx, doc);

        return idx;
    }

    /// <summary>
    /// Tries to find a row with the same set of key fields in this table and if found, replaces it and returns its index,
    /// otherwise returns -1
    /// </summary>
    /// <param name="doc">Document instance</param>
    /// <param name="key">Primary key</param>
    /// <param name="docUpgrade">
    ///   When not null, is called with old and new instance of the doc to be updated. It returns
    ///   the doc to be saved. Note that the returned doc must have the same key and schema or else the function will throw.
    /// </param>
    protected virtual int DoUpdate(Doc doc, Access.IDataStoreKey key = null, Func<Doc, Doc, Doc> docUpgrade = null)
    {
        int dummy;
        var idx = SearchForDoc(doc, out dummy);
        if (idx<0) return -1;

        DoUpgrade(dummy, doc, docUpgrade);

        return idx;
    }

    /// <summary>
    /// Apply docUpgrade function to the doc stored at index "idx" and the new "doc" passed as second argument,
    /// and store the returned doc back at index "idx".
    /// </summary>
    protected virtual void DoUpgrade(int idx, Doc newDoc, Func<Doc, Doc, Doc> docUpgrade)
    {
        if (docUpgrade == null)
        {
          m_List[idx] = newDoc;
          return;
        }

        var upgradedRow = docUpgrade(m_List[idx], newDoc);
        // Ensure that the Schema hasn't been changed
        Check(upgradedRow);
        // Check that the row's key is unmodified
        int dummy;
        int idxUpgraded = SearchForDoc(upgradedRow, out dummy);
        if (idxUpgraded != idx)
          throw new DataException(StringConsts.CRUD_ROW_UPGRADE_KEY_MUTATION_ERROR);

        m_List[idx] = upgradedRow;
    }

    /// <summary>
    /// Tries to find a doc with the same set of key fields in this table and if found, replaces it and returns true,
    ///  otherwise inserts the doc (if schemas match) and returns false. Optionally pass updateWhere condition
    ///   that may check whether update needs to be performed
    /// </summary>
    protected virtual UpdateResult DoUpsert(Doc doc, Func<Doc, bool> updateWhere, Func<Doc, Doc, Doc> docUpgrade = null)
    {
        int insertIndex;
        int idx = SearchForDoc(doc, out insertIndex);
        if (idx<0)
        {
          m_List.Insert(insertIndex, doc);
          return new UpdateResult(insertIndex, false);
        }

        if (updateWhere!=null && !updateWhere(m_List[idx]))
          return new UpdateResult(-1, false);//where did not match

        DoUpgrade(idx, doc, docUpgrade);
        return new UpdateResult(idx, true);
    }

    /// <summary>
    /// Tries to find a row with the same set of key fields in this table and if found, deletes it and returns its index, otherwise -1
    /// </summary>
    protected virtual int DoDelete(Doc doc, Access.IDataStoreKey key = null)
    {
        int dummy;
        int idx = SearchForDoc(doc, out dummy);
        if (idx<0) return -1;

        m_List.RemoveAt(idx);
        return idx;
    }


    protected T DoKeyDocFromValues<T>(T kdoc, params object[] keys) where T : Doc
    {
        Check(kdoc);
        var idx = 0;
        foreach(var kdef in Schema.AnyTargetKeyFieldDefs)
        {
          if (idx>keys.Length-1)
            throw new DataException(StringConsts.CRUD_FIND_BY_KEY_LENGTH_ERROR);

          kdoc[kdef.Order] = keys[idx];
          idx++;
        }

        return kdoc;
    }

  #endregion

  }

  public struct UpdateResult
  {
    public UpdateResult(int idx, bool updated) { Index=idx; Updated=updated; }

    public readonly int  Index;
    public readonly bool Updated;
  }

}
