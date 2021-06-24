/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Threading.Tasks;

namespace Azos.Data.Access
{
  /// <summary>
  /// Represents an entity that can execute a query. The implementation may be called by multiple threads and must be safe
  /// </summary>
  public abstract class CRUDQueryHandler : Collections.INamed
  {
    protected CRUDQueryHandler(ICRUDDataStore store, string name)
    {
      m_Store = store.NonNull(nameof(store));
      m_Name = name.NonBlank(nameof(name));
    }

    protected CRUDQueryHandler(ICRUDDataStore store, QuerySource source) : this(store, source.NonNull(nameof(source)).Name)
    {
      Source = source;
    }

    private string m_Name;
    private ICRUDDataStore m_Store;
    protected readonly QuerySource Source;

    /// <summary>
    /// Returns query name that this handler handles
    /// </summary>
    public string Name => m_Name;

    /// <summary>
    /// Store instance that handler is under
    /// </summary>
    public ICRUDDataStore Store => m_Store;

    /// <summary>
    /// Executes query without fetching any data but schema. The implementation may be called by multiple threads and must be safe
    /// </summary>
    public abstract Schema GetSchema(ICRUDQueryExecutionContext context, Query query);

    /// <summary>
    /// Executes query without fetching any data but schema. The implementation may be called by multiple threads and must be safe
    /// </summary>
    public abstract Task<Schema> GetSchemaAsync(ICRUDQueryExecutionContext context, Query query);

    /// <summary>
    /// Executes query. The implementation may be called by multiple threads and must be safe
    /// </summary>
    public abstract RowsetBase Execute(ICRUDQueryExecutionContext context, Query query, bool oneRow = false);

    /// <summary>
    /// Executes query. The implementation may be called by multiple threads and must be safe
    /// </summary>
    public abstract Task<RowsetBase> ExecuteAsync(ICRUDQueryExecutionContext context, Query query, bool oneRow = false);

    /// <summary>
    /// Executes query into Cursor. The implementation may be called by multiple threads and must be safe
    /// </summary>
    public abstract Cursor OpenCursor(ICRUDQueryExecutionContext context, Query query);

    /// <summary>
    /// Executes query into Cursor. The implementation may be called by multiple threads and must be safe
    /// </summary>
    public abstract Task<Cursor> OpenCursorAsync(ICRUDQueryExecutionContext context, Query query);

    /// <summary>
    /// Executes query that does not return results. The implementation may be called by multiple threads and must be safe.
    /// Returns rows affected
    /// </summary>
    public abstract int ExecuteWithoutFetch(ICRUDQueryExecutionContext context, Query query);

    /// <summary>
    /// Executes query that does not return results. The implementation may be called by multiple threads and must be safe.
    /// Returns rows affected
    /// </summary>
    public abstract Task<int> ExecuteWithoutFetchAsync(ICRUDQueryExecutionContext context, Query query);
  }


  /// <summary>
  /// Represents an entity that can execute a query. The implementation may be called by multiple threads and must be safe
  /// </summary>
  public abstract class CRUDQueryHandler<TStore> : CRUDQueryHandler where TStore : ICRUDDataStore
  {
    protected CRUDQueryHandler(TStore store, string name) : base(store, name) {}
    protected CRUDQueryHandler(TStore store, QuerySource source) : base(store, source) { }

    /// <summary>
    /// Store instance that handler is under
    /// </summary>
    public new TStore Store => (TStore)base.Store;
  }
}
