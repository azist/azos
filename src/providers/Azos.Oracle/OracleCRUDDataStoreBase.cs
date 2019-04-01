/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;

using Oracle.ManagedDataAccess.Client;

namespace Azos.Data.Access.Oracle
{
  /// <summary>
  /// Provides Oracle CRUD data store base that supports CRUD query handler resolution, execution and mapping.
  /// This store does not support auto-generation of Insert/Update/Delete statements as this is not practical
  /// for general Oracle cases due to inconsistent/unpredictable use of data types in a non-uniform schema designs
  /// </summary>
  public abstract class OracleCRUDDataStoreBase : OracleDataStoreBase, ICRUDDataStoreImplementation
  {
    #region CONSTS
    public const string SCRIPT_FILE_SUFFIX = ".ora.sql";
    #endregion

    #region .ctor/.dctor
    protected OracleCRUDDataStoreBase(IApplication app) : base(app) => ctor();
    protected OracleCRUDDataStoreBase(IApplication app, string cs) : base(app) => ctor(cs);
    protected OracleCRUDDataStoreBase(IApplicationComponent director) : base(director) => ctor();
    protected OracleCRUDDataStoreBase(IApplicationComponent director, string cs) : base(director) => ctor(cs);

    private void ctor(string cs = null)
    {
      m_QueryResolver = new QueryResolver(this);
      ConnectString = cs;
    }
    #endregion

    #region Fields
    private QueryResolver m_QueryResolver;
    #endregion

    #region ICRUDDataStore
    public bool SupportsTransactions   => true;
    public bool SupportsTrueAsynchrony => true;
    public string ScriptFileSuffix     => SCRIPT_FILE_SUFFIX;
    public CRUDDataStoreType StoreType => CRUDDataStoreType.Relational;

    public CRUDTransaction BeginTransaction(IsolationLevel iso = IsolationLevel.ReadCommitted,
                                            TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose)
     => BeginTransactionAsync(iso, behavior).GetAwaiter().GetResult();

    public async Task<CRUDTransaction> BeginTransactionAsync(IsolationLevel iso = IsolationLevel.ReadCommitted,
                                                             TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose)
    {
      var cnn = GetConnection();
      return new OracleCRUDTransaction(this, cnn, iso, behavior);
    }


    public Schema GetSchema(Query query)
     => GetSchemaAsync(query).GetAwaiter().GetResult();

    public async Task<Schema> GetSchemaAsync(Query query)
    {
      using (var cnn = GetConnection())
        return await DoGetSchemaAsync(cnn, null, query);
    }

    public List<RowsetBase> Load(params Query[] queries)
     => LoadAsync(queries).GetAwaiter().GetResult();

    public async Task<List<RowsetBase>> LoadAsync(params Query[] queries)
    {
      using (var cnn = GetConnection())
        return await DoLoadAsync(cnn, null, queries);
    }

    public RowsetBase LoadOneRowset(Query query)
     => Load(query).FirstOrDefault();

    public async Task<RowsetBase> LoadOneRowsetAsync(Query query)
     => (await LoadAsync(query)).FirstOrDefault();

    public Doc LoadOneDoc(Query query)
     => LoadOneDocAsync(query).GetAwaiter().GetResult();

    public async Task<Doc> LoadOneDocAsync(Query query)
    {
      var rowset = await LoadOneRowsetAsync(query);
      return rowset?.FirstOrDefault();
    }

    public Cursor OpenCursor(Query query)
     => OpenCursorAsync(query).GetAwaiter().GetResult();

    public async Task<Cursor> OpenCursorAsync(Query query)
    {
      using (var cnn = GetConnection())
        return await DoOpenCursorAsync(cnn, null, query);
    }


    public int Save(params RowsetBase[] rowsets)
     => SaveAsync(rowsets).GetAwaiter().GetResult();

    public async Task<int> SaveAsync(params RowsetBase[] rowsets)
    {
      using (var cnn = GetConnection())
        return await DoSaveAsync(cnn,  null, rowsets);
    }

    public int Insert(Doc row, FieldFilterFunc filter = null)
     => InsertAsync(row, filter).GetAwaiter().GetResult();

    public async Task<int> InsertAsync(Doc row, FieldFilterFunc filter = null)
    {
      using (var cnn = GetConnection())
        return await DoInsertAsync(cnn,  null, row, filter);
    }

    public int Upsert(Doc row, FieldFilterFunc filter = null)
     => UpsertAsync(row, filter).GetAwaiter().GetResult();

    public async Task<int> UpsertAsync(Doc row, FieldFilterFunc filter = null)
    {
      using (var cnn = GetConnection())
        return await DoUpsertAsync(cnn,  null, row, filter);
    }

    public int Update(Doc row, IDataStoreKey key = null, FieldFilterFunc filter = null)
     =>  UpdateAsync(row, key, filter).GetAwaiter().GetResult();

    public async Task<int> UpdateAsync(Doc row, IDataStoreKey key = null, FieldFilterFunc filter = null)
    {
      using (var cnn = GetConnection())
        return await DoUpdateAsync(cnn,  null, row, key, filter);
    }

    public int Delete(Doc row, IDataStoreKey key = null)
     => DeleteAsync(row, key).GetAwaiter().GetResult();

    public async Task<int> DeleteAsync(Doc row, IDataStoreKey key = null)
    {
      using (var cnn = GetConnection())
        return await DoDeleteAsync(cnn,  null, row, key);
    }

    public int ExecuteWithoutFetch(params Query[] queries)
     => ExecuteWithoutFetchAsync(queries).GetAwaiter().GetResult();

    public async Task<int> ExecuteWithoutFetchAsync(params Query[] queries)
    {
      using (var cnn = GetConnection())
        return await DoExecuteWithoutFetchAsync(cnn, null, queries);
    }

    public CRUDQueryHandler MakeScriptQueryHandler(QuerySource querySource)
     => new OracleCRUDScriptQueryHandler(this, querySource);

    public ICRUDQueryResolver QueryResolver => m_QueryResolver;

    #endregion



    #region Protected + Overrides

    public override void Configure(IConfigSectionNode node)
    {
      m_QueryResolver.Configure(node);
      base.Configure(node);
    }

    /// <summary>
    ///  Performs CRUD load without fetching data only returning schema. Override to do custom Query interpretation
    /// </summary>
    protected internal async virtual Task<Schema> DoGetSchemaAsync(OracleConnection cnn, OracleTransaction transaction, Query query)
    {
      if (query==null) return null;

      var handler = QueryResolver.Resolve(query);
      try
      {
        return await handler.GetSchemaAsync( new OracleCRUDQueryExecutionContext(this, cnn, transaction), query);
      }
      catch (Exception error)
      {
        throw new OracleDataAccessException(
                        StringConsts.GET_SCHEMA_ERROR + error.ToMessageWithType(),
                        error,
                        KeyViolationKind.Unspecified,
                        CRUDGenerator.KeyViolationName(error));
      }
    }


    /// <summary>
    ///  Performs CRUD load. Override to do custom Query interpretation
    /// </summary>
    protected internal async virtual Task<List<RowsetBase>> DoLoadAsync(OracleConnection cnn, OracleTransaction transaction, Query[] queries, bool oneDoc = false)
    {
      var result = new List<RowsetBase>();
      if (queries==null) return result;

      foreach(var query in queries)
      {
        var handler = QueryResolver.Resolve(query);
        try
        {
          var rowset = await handler.ExecuteAsync( new OracleCRUDQueryExecutionContext(this, cnn, transaction), query, oneDoc);
          result.Add(rowset);
        }
        catch (Exception error)
        {
          throw new OracleDataAccessException(
                          StringConsts.LOAD_ERROR + error.ToMessageWithType(),
                          error,
                          KeyViolationKind.Unspecified,
                          CRUDGenerator.KeyViolationName(error));
        }
      }

      return result;
    }


    /// <summary>
    ///  Performs CRUD load. Override to do custom Query interpretation
    /// </summary>
    protected internal async virtual Task<Cursor> DoOpenCursorAsync(OracleConnection cnn, OracleTransaction transaction, Query query)
    {
      var context = new OracleCRUDQueryExecutionContext(this, cnn, transaction);
      var handler = QueryResolver.Resolve(query);
      try
      {
        return await handler.OpenCursorAsync( context, query);
      }
      catch (Exception error)
      {
        throw new OracleDataAccessException(
                        StringConsts.OPEN_CURSOR_ERROR + error.ToMessageWithType(),
                        error,
                        KeyViolationKind.Unspecified,
                        CRUDGenerator.KeyViolationName(error));
      }
    }

    /// <summary>
    ///  Performs CRUD execution of queries that do not return result set. Override to do custom Query interpretation
    /// </summary>
    protected internal async virtual Task<int> DoExecuteWithoutFetchAsync(OracleConnection cnn, OracleTransaction transaction, Query[] queries)
    {
      if (queries==null) return 0;

      var affected = 0;

      foreach(var query in queries)
      {
        var handler = QueryResolver.Resolve(query);
        try
        {
          affected += await handler.ExecuteWithoutFetchAsync(new OracleCRUDQueryExecutionContext(this, cnn, transaction), query);
        }
        catch (Exception error)
        {
          throw new OracleDataAccessException(
                          StringConsts.EXECUTE_WITHOUT_FETCH_ERROR + error.ToMessageWithType(),
                          error,
                          KeyViolationKind.Unspecified,
                          CRUDGenerator.KeyViolationName(error));
        }
      }

      return affected;
    }

    /// <summary>
    /// Performs CRUD batch save. Override to do custom batch saving
    /// </summary>
    protected internal async virtual Task<int> DoSaveAsync(OracleConnection cnn, OracleTransaction transaction, RowsetBase[] rowsets)
    {
      if (rowsets==null) return 0;

      var affected = 0;

      foreach(var rset in rowsets)
      {
        foreach(var change in rset.Changes)
        {
          switch(change.ChangeType)
          {
              case DocChangeType.Insert: affected += await DoInsertAsync(cnn, transaction, change.Doc); break;
              case DocChangeType.Update: affected += await DoUpdateAsync(cnn, transaction, change.Doc, change.Key); break;
              case DocChangeType.Upsert: affected += await DoUpsertAsync(cnn, transaction, change.Doc); break;
              case DocChangeType.Delete: affected += await DoDeleteAsync(cnn, transaction, change.Doc, change.Key); break;
          }
        }
      }


      return affected;
    }

    /// <summary>
    /// Performs CRUD row insert. Override to do custom insertion
    /// </summary>
    protected internal async virtual Task<int> DoInsertAsync(OracleConnection cnn, OracleTransaction transaction, Doc row, FieldFilterFunc filter = null)
    {
      checkReadOnly(row.Schema, "insert");
      return await CRUDGenerator.CRUDInsert(this, cnn, transaction, row, filter);
    }

    /// <summary>
    /// Performs CRUD row upsert. Override to do custom upsertion
    /// </summary>
    protected internal async virtual Task<int> DoUpsertAsync(OracleConnection cnn, OracleTransaction transaction, Doc row, FieldFilterFunc filter = null)
    {
      checkReadOnly(row.Schema, "upsert");
      return await CRUDGenerator.CRUDUpsert(this, cnn, transaction, row, filter);
    }

    /// <summary>
    /// Performs CRUD row update. Override to do custom update
    /// </summary>
    protected internal async virtual Task<int> DoUpdateAsync(OracleConnection cnn, OracleTransaction transaction, Doc row, IDataStoreKey key = null, FieldFilterFunc filter = null)
    {
      checkReadOnly(row.Schema, "update");
      return await CRUDGenerator.CRUDUpdate(this, cnn, transaction, row, key, filter);
    }

    /// <summary>
    /// Performs CRUD row deletion. Override to do custom deletion
    /// </summary>
    protected internal async virtual Task<int> DoDeleteAsync(OracleConnection cnn, OracleTransaction transaction, Doc row, IDataStoreKey key = null)
    {
      checkReadOnly(row.Schema, "delete");
      return await CRUDGenerator.CRUDDelete(this, cnn, transaction, row, key);
    }


    #endregion

    #region .pvt
    private void checkReadOnly(Schema schema, string operation)
    {
        if (schema.ReadOnly)
            throw new OracleDataAccessException(StringConsts.CRUD_READONLY_SCHEMA_ERROR.Args(schema.Name, operation));
    }
    #endregion
  }
}
