/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Data.Access
{
  /// <summary>
  /// Denotes types of CRUD stores
  /// </summary>
  public enum CrudDataStoreType
  {
    Relational,
    Document,
    KeyValue,
    Hybrid
  }


  /// <summary>
  /// Describes an entity that performs single (not in transaction/batch)CRUD operations
  /// </summary>
  public interface ICrudOperations
  {
    /// <summary>
    /// Returns true when backend supports true asynchronous operations, such as the ones that do not create extra threads/empty tasks
    /// </summary>
    bool SupportsTrueAsynchrony { get; }

    Schema GetSchema(Query query);
    Task<Schema> GetSchemaAsync(Query query);

    List<RowsetBase> Load(params Query[] queries);
    Task<List<RowsetBase>> LoadAsync(params Query[] queries);

    RowsetBase LoadOneRowset(Query query);
    Task<RowsetBase> LoadOneRowsetAsync(Query query);

    Doc LoadOneDoc(Query query);
    Task<Doc> LoadOneDocAsync(Query query);

    Cursor OpenCursor(Query query);
    Task<Cursor> OpenCursorAsync(Query query);

    int Save(params RowsetBase[] rowsets);
    Task<int> SaveAsync(params RowsetBase[] rowsets);

    /// <summary>
    /// Executes a query possibly returning a document which describes a result, such as affected entity count.
    /// The difference between this and LoasOneDoc methods, is that LoadOneDoc fetches data from the store,
    /// whereas as Execute is more generic and can be used for execution of any query similar to a stored-procedure.
    /// If you need to return a collection, then return a doc with collection field(s). This call may return null
    /// </summary>
    Doc Execute(Query query);

    /// <summary>
    /// Executes a query possibly returning a document which describes a result, such as affected entity count.
    /// The difference between this and LoasOneDoc methods, is that LoadOneDoc fetches data from the store,
    /// whereas as Execute is more generic and can be used for execution of any query similar to a stored-procedure.
    /// If you need to return a collection, then return a doc with collection field(s). This call may return null
    /// </summary>
    Task<Doc> ExecuteAsync(Query query);

    int Insert(Doc row, FieldFilterFunc filter = null);
    Task<int> InsertAsync(Doc row, FieldFilterFunc filter = null);

    int Upsert(Doc row, FieldFilterFunc filter = null);
    Task<int> UpsertAsync(Doc row, FieldFilterFunc filter = null);

    int Update(Doc row, IDataStoreKey key = null, FieldFilterFunc filter = null);
    Task<int> UpdateAsync(Doc row, IDataStoreKey key = null, FieldFilterFunc filter = null);

    int Delete(Doc row, IDataStoreKey key = null);
    Task<int> DeleteAsync(Doc row, IDataStoreKey key = null);
  }


  /// <summary>
  /// Describes an entity that performs single (not in transaction/batch)CRUD operations
  /// </summary>
  public interface ICrudTransactionOperations
  {
    /// <summary>
    /// Returns true when backend supports transactions. Even if false returned, CRUDDatastore supports CRUDTransaction return from BeginTransaction()
    ///  in which case statements may not be sent to destination until a call to Commit()
    /// </summary>
    bool SupportsTransactions { get; }

    /// <summary>
    /// Returns a transaction object for backend. Even if backend does not support transactions internally, CRUDTransactions save changes
    ///  into the store on commit only
    /// </summary>
    CrudTransaction BeginTransaction(IsolationLevel iso = IsolationLevel.ReadCommitted, TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose);

    /// <summary>
    /// Returns a transaction object for backend. Even if backend does not support transactions internally, CRUDTransactions save changes
    ///  into the store on commit only
    /// </summary>
    Task<CrudTransaction> BeginTransactionAsync(IsolationLevel iso = IsolationLevel.ReadCommitted, TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose);
  }


  /// <summary>
  /// Represents a DataStore that supports CRUD operations
  /// </summary>
  public interface ICrudDataStore : IDataStore, ICrudOperations, ICrudTransactionOperations, IApplicationComponent
  {
    /// <summary>
    /// Returns default script file suffix, which some providers may use to locate script files
    ///  i.e. for MySql:  ".my.sql" which gets added to script files like so:  name.[suffix].[script ext (i.e. sql)].
    /// This name should uniquely identify the provider
    /// </summary>
    string ScriptFileSuffix { get; }

    /// <summary>
    /// Provides classification for the underlying store
    /// </summary>
    CrudDataStoreType StoreType { get; }

    /// <summary>
    /// Resolver that turns query into handler
    /// </summary>
    ICrudQueryResolver QueryResolver { get; }
  }


  public interface ICrudDataStoreImplementation : ICrudDataStore, IDataStoreImplementation
  {
    CrudQueryHandler MakeScriptQueryHandler(QuerySource querySource);
  }


  /// <summary>
  /// Represents a class that resolves Query into suitable handler that can execute it
  /// </summary>
  public interface ICrudQueryResolver : IApplicationComponent, IConfigurable
  {
    /// <summary>
    /// Retrieves a handler for supplied query. The implementation must be thread-safe
    /// </summary>
    CrudQueryHandler Resolve(Query query);

    string ScriptAssembly { get; set; }

    IList<string> HandlerLocations { get; }

    Collections.IRegistry<CrudQueryHandler> Handlers { get; }

    /// <summary>
    /// Registers handler location.
    /// The Resolver must be not started yet. This method is NOT thread safe
    /// </summary>
    void RegisterHandlerLocation(string location);

    /// <summary>
    /// Unregisters handler location returning true if it was found and removed.
    /// The Resolve must be not started yet. This method is NOT thread safe
    /// </summary>
    bool UnregisterHandlerLocation(string location);
  }


  /// <summary>
  /// Represents a context (such as Sql Server connection + transaction scope, or Hadoop connect string etc.) for query execution.
  /// This is a marker interface implemented by particular providers
  /// </summary>
  public interface ICrudQueryExecutionContext
  {
  }

  /// <summary>
  /// Data document which is used to return rows affected from Execute()
  /// </summary>
  public class RowsAffectedDoc : TypedDoc
  {
    public RowsAffectedDoc(long affected) => RowsAffected = affected;

    [Field(Description = "Rows affected as reported by provider")]
    public long RowsAffected {  get; set; }

    [Field(Description = "Provider result object as returned by provider")]
    public object ProviderResult {  get; set;}
  }

}
