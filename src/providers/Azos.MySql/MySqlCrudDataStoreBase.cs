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

using MySqlConnector;

namespace Azos.Data.Access.MySql
{
  /// <summary>
  /// Provides a base implementation for MySql CRUD data stores that supports CRUD query handler resolution, execution and mapping.
  /// This store does not support auto-generation of Insert/Update/Delete statements as this is not practical
  /// for general-purposes cases due to inconsistent/unpredictable use of data types in a non-uniform schema designs.
  /// Contrast this class with <seealso cref="MySqlCanonicalDataStore"/>
  /// </summary>
  public abstract class MySqlCrudDataStoreBase : MySqlDataStoreBase, ICRUDDataStoreImplementation
  {
    #region CONSTS
    public const string SCRIPT_FILE_SUFFIX = ".mys.sql";
    #endregion

    #region .ctor/.dctor
    protected MySqlCrudDataStoreBase(IApplication app) : base(app) => ctor();
    protected MySqlCrudDataStoreBase(IApplication app, string cs) : base(app) => ctor(cs);
    protected MySqlCrudDataStoreBase(IApplicationComponent director) : base(director) => ctor();
    protected MySqlCrudDataStoreBase(IApplicationComponent director, string cs) : base(director) => ctor(cs);

    private void ctor(string cs = null)
    {
      m_QueryResolver = new QueryResolver(this);
      ConnectString = cs;
    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_QueryResolver);
      base.Destructor();
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
      var cnn = await GetConnection().ConfigureAwait(false);
      return new MySqlCrudTransaction(this, cnn, iso, behavior);//transaction owns the connection
    }

    public Schema GetSchema(Query query)
     => GetSchemaAsync(query).GetAwaiter().GetResult();

    public async Task<Schema> GetSchemaAsync(Query query)
    {
      using (var cnn = await GetConnection().ConfigureAwait(false))
        return await DoGetSchemaAsync(cnn, null, query).ConfigureAwait(false);
    }

    public List<RowsetBase> Load(params Query[] queries)
     => LoadAsync(queries).GetAwaiter().GetResult();

    public async Task<List<RowsetBase>> LoadAsync(params Query[] queries)
    {
      using (var cnn = await GetConnection().ConfigureAwait(false))
        return await DoLoadAsync(cnn, null, queries).ConfigureAwait(false);
    }

    public RowsetBase LoadOneRowset(Query query)
     => Load(query).FirstOrDefault();

    public async Task<RowsetBase> LoadOneRowsetAsync(Query query)
     => (await LoadAsync(query).ConfigureAwait(false)).FirstOrDefault();

    public Doc LoadOneDoc(Query query)
     => LoadOneDocAsync(query).GetAwaiter().GetResult();

    public async Task<Doc> LoadOneDocAsync(Query query)
    {
      var rowset = await LoadOneRowsetAsync(query).ConfigureAwait(false);
      return rowset?.FirstOrDefault();
    }

    public Cursor OpenCursor(Query query)
     => OpenCursorAsync(query).GetAwaiter().GetResult();

    public async Task<Cursor> OpenCursorAsync(Query query)
    {
      /*using (*/var cnn = await GetConnection().ConfigureAwait(false);//)
        return await DoOpenCursorAsync(cnn, null, query).ConfigureAwait(false);
    }

    public int Save(params RowsetBase[] rowsets)
     => SaveAsync(rowsets).GetAwaiter().GetResult();

    public async Task<int> SaveAsync(params RowsetBase[] rowsets)
    {
      using (var cnn = await GetConnection().ConfigureAwait(false))
        return await DoSaveAsync(cnn,  null, rowsets).ConfigureAwait(false);
    }

    public int Insert(Doc row, FieldFilterFunc filter = null)
     => InsertAsync(row, filter).GetAwaiter().GetResult();

    public async Task<int> InsertAsync(Doc row, FieldFilterFunc filter = null)
    {
      using (var cnn = await GetConnection().ConfigureAwait(false))
        return await DoInsertAsync(cnn,  null, row, filter).ConfigureAwait(false);
    }

    public int Upsert(Doc row, FieldFilterFunc filter = null)
     => UpsertAsync(row, filter).GetAwaiter().GetResult();

    public async Task<int> UpsertAsync(Doc row, FieldFilterFunc filter = null)
    {
      using (var cnn = await GetConnection().ConfigureAwait(false))
        return await DoUpsertAsync(cnn,  null, row, filter).ConfigureAwait(false);
    }

    public int Update(Doc row, IDataStoreKey key = null, FieldFilterFunc filter = null)
     =>  UpdateAsync(row, key, filter).GetAwaiter().GetResult();

    public async Task<int> UpdateAsync(Doc row, IDataStoreKey key = null, FieldFilterFunc filter = null)
    {
      using (var cnn = await GetConnection().ConfigureAwait(false))
        return await DoUpdateAsync(cnn,  null, row, key, filter).ConfigureAwait(false);
    }

    public int Delete(Doc row, IDataStoreKey key = null)
     => DeleteAsync(row, key).GetAwaiter().GetResult();

    public async Task<int> DeleteAsync(Doc row, IDataStoreKey key = null)
    {
      using (var cnn = await GetConnection().ConfigureAwait(false))
        return await DoDeleteAsync(cnn,  null, row, key).ConfigureAwait(false);
    }

    public int ExecuteWithoutFetch(params Query[] queries)
     => ExecuteWithoutFetchAsync(queries).GetAwaiter().GetResult();

    public async Task<int> ExecuteWithoutFetchAsync(params Query[] queries)
    {
      using (var cnn = await GetConnection().ConfigureAwait(false))
        return await DoExecuteWithoutFetchAsync(cnn, null, queries).ConfigureAwait(false);
    }

    public CRUDQueryHandler MakeScriptQueryHandler(QuerySource querySource)
     => new MySqlCrudScriptQueryHandler(this, querySource);

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
    protected internal async virtual Task<Schema> DoGetSchemaAsync(MySqlConnection cnn, MySqlTransaction transaction, Query query)
    {
      if (query==null) return null;

      var handler = QueryResolver.Resolve(query);
      try
      {
        return await handler.GetSchemaAsync( new MySqlCrudQueryExecutionContext(this, cnn, transaction), query).ConfigureAwait(false);
      }
      catch (Exception error)
      {
        throw new MySqlDataAccessException(
                        StringConsts.GET_SCHEMA_ERROR + error.ToMessageWithType(),
                        error,
                        KeyViolationKind.Unspecified,
                        CrudGenerator.KeyViolationName(error));
      }
    }

    /// <summary>
    ///  Performs CRUD load. Override to do custom Query interpretation
    /// </summary>
    protected internal async virtual Task<List<RowsetBase>> DoLoadAsync(MySqlConnection cnn, MySqlTransaction transaction, Query[] queries, bool oneDoc = false)
    {
      var result = new List<RowsetBase>();
      if (queries==null) return result;

      foreach(var query in queries)
      {
        var handler = QueryResolver.Resolve(query);
        try
        {
          var rowset = await handler.ExecuteAsync( new MySqlCrudQueryExecutionContext(this, cnn, transaction), query, oneDoc).ConfigureAwait(false);
          result.Add(rowset);
        }
        catch (Exception error)
        {
          throw new MySqlDataAccessException(
                          StringConsts.LOAD_ERROR + error.ToMessageWithType(),
                          error,
                          KeyViolationKind.Unspecified,
                          CrudGenerator.KeyViolationName(error));
        }
      }

      return result;
    }


    /// <summary>
    ///  Performs CRUD load. Override to do custom Query interpretation
    /// </summary>
    protected internal async virtual Task<Cursor> DoOpenCursorAsync(MySqlConnection cnn, MySqlTransaction transaction, Query query)
    {
      var context = new MySqlCrudQueryExecutionContext(this, cnn, transaction);
      var handler = QueryResolver.Resolve(query);
      try
      {
        return await handler.OpenCursorAsync( context, query).ConfigureAwait(false);
      }
      catch (Exception error)
      {
        throw new MySqlDataAccessException(
                        StringConsts.OPEN_CURSOR_ERROR + error.ToMessageWithType(),
                        error,
                        KeyViolationKind.Unspecified,
                        CrudGenerator.KeyViolationName(error));
      }
    }

    /// <summary>
    ///  Performs CRUD execution of queries that do not return result set. Override to do custom Query interpretation
    /// </summary>
    protected internal async virtual Task<int> DoExecuteWithoutFetchAsync(MySqlConnection cnn, MySqlTransaction transaction, Query[] queries)
    {
      if (queries==null) return 0;

      var affected = 0;

      foreach(var query in queries)
      {
        var handler = QueryResolver.Resolve(query);
        try
        {
          affected += await handler.ExecuteWithoutFetchAsync(new MySqlCrudQueryExecutionContext(this, cnn, transaction), query).ConfigureAwait(false);
        }
        catch (Exception error)
        {
          throw new MySqlDataAccessException(
                          StringConsts.EXECUTE_WITHOUT_FETCH_ERROR + error.ToMessageWithType(),
                          error,
                          KeyViolationKind.Unspecified,
                          CrudGenerator.KeyViolationName(error));
        }
      }

      return affected;
    }

    /// <summary>
    /// Performs CRUD batch save. Override to do custom batch saving
    /// </summary>
    protected internal virtual Task<int> DoSaveAsync(MySqlConnection cnn, MySqlTransaction transaction, RowsetBase[] rowsets)
     => throw new NotImplementedException(StringConsts.CRUD_CAPABILITY_NOT_IMPLEMENETD_ERROR.Args(nameof(DoSaveAsync),
                                                                                                 GetType().FullName,
                                                                                                 nameof(MySqlCanonicalDataStore)));

    /// <summary>
    /// Performs CRUD row insert. Override to do custom insertion
    /// </summary>
    protected internal virtual Task<int> DoInsertAsync(MySqlConnection cnn, MySqlTransaction transaction, Doc row, FieldFilterFunc filter = null)
     => throw new NotImplementedException(StringConsts.CRUD_CAPABILITY_NOT_IMPLEMENETD_ERROR.Args(nameof(DoInsertAsync),
                                                                                                 GetType().FullName,
                                                                                                 nameof(MySqlCanonicalDataStore)));

    /// <summary>
    /// Performs CRUD row upsert. Override to do custom upsertion
    /// </summary>
    protected internal virtual Task<int> DoUpsertAsync(MySqlConnection cnn, MySqlTransaction transaction, Doc row, FieldFilterFunc filter = null)
     => throw new NotImplementedException(StringConsts.CRUD_CAPABILITY_NOT_IMPLEMENETD_ERROR.Args(nameof(DoUpsertAsync),
                                                                                                 GetType().FullName,
                                                                                                 nameof(MySqlCanonicalDataStore)));

    /// <summary>
    /// Performs CRUD row update. Override to do custom update
    /// </summary>
    protected internal virtual Task<int> DoUpdateAsync(MySqlConnection cnn, MySqlTransaction transaction, Doc row, IDataStoreKey key = null, FieldFilterFunc filter = null)
     => throw new NotImplementedException(StringConsts.CRUD_CAPABILITY_NOT_IMPLEMENETD_ERROR.Args(nameof(DoUpdateAsync),
                                                                                                 GetType().FullName,
                                                                                                 nameof(MySqlCanonicalDataStore)));

    /// <summary>
    /// Performs CRUD row deletion. Override to do custom deletion
    /// </summary>
    protected internal virtual Task<int> DoDeleteAsync(MySqlConnection cnn, MySqlTransaction transaction, Doc row, IDataStoreKey key = null)
     => throw new NotImplementedException(StringConsts.CRUD_CAPABILITY_NOT_IMPLEMENETD_ERROR.Args(nameof(DoDeleteAsync),
                                                                                                 GetType().FullName,
                                                                                                 nameof(MySqlCanonicalDataStore)));

    protected void CheckReadOnly(Schema schema, string operation)
    {
      if (schema.ReadOnly)
        throw new MySqlDataAccessException(StringConsts.CRUD_READONLY_SCHEMA_ERROR.Args(schema.Name, operation));
    }
    #endregion
  }
}
