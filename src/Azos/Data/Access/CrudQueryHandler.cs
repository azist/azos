/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Threading.Tasks;
using Azos.Instrumentation;
using Azos.Time;

namespace Azos.Data.Access
{
  /// <summary>
  /// Represents an entity that can execute a query. The implementation may be called by multiple threads and must be thread-safe
  /// </summary>
  public abstract class CrudQueryHandler : Collections.INamed
  {
    protected CrudQueryHandler(ICrudDataStore store, string name)
    {
      m_Store = store.NonNull(nameof(store));
      m_Name = name.NonBlank(nameof(name));
    }

    protected CrudQueryHandler(ICrudDataStore store, QuerySource source) : this(store, source.NonNull(nameof(source)).Name)
    {
      Source = source;
    }

    private string m_Name;
    private ICrudDataStore m_Store;
    protected readonly QuerySource Source;

    /// <summary>
    /// Returns query name that this handler handles
    /// </summary>
    public string Name => m_Name;

    /// <summary>
    /// Store instance that handler is under
    /// </summary>
    public ICrudDataStore Store => m_Store;

    /// <summary>
    /// Executes query without fetching any data but schema. The implementation may be called by multiple threads and must be safe
    /// </summary>
    public abstract Schema GetSchema(ICrudQueryExecutionContext context, Query query);

    /// <summary>
    /// Executes query without fetching any data but schema. The implementation may be called by multiple threads and must be safe
    /// </summary>
    public abstract Task<Schema> GetSchemaAsync(ICrudQueryExecutionContext context, Query query);

    /// <summary>
    /// Executes query. The implementation may be called by multiple threads and must be safe
    /// </summary>
    public abstract RowsetBase Execute(ICrudQueryExecutionContext context, Query query, bool oneRow = false);

    /// <summary>
    /// Executes query. The implementation may be called by multiple threads and must be safe
    /// </summary>
    public abstract Task<RowsetBase> ExecuteAsync(ICrudQueryExecutionContext context, Query query, bool oneRow = false);

    /// <summary>
    /// Executes query into Cursor. The implementation may be called by multiple threads and must be safe
    /// </summary>
    public abstract Cursor OpenCursor(ICrudQueryExecutionContext context, Query query);

    /// <summary>
    /// Executes query into Cursor. The implementation may be called by multiple threads and must be safe
    /// </summary>
    public abstract Task<Cursor> OpenCursorAsync(ICrudQueryExecutionContext context, Query query);

    /// <summary>
    /// Executes query that does not return results. The implementation may be called by multiple threads and must be safe.
    /// Returns rows affected
    /// </summary>
    public abstract int ExecuteWithoutFetch(ICrudQueryExecutionContext context, Query query);

    /// <summary>
    /// Executes query that does not return results. The implementation may be called by multiple threads and must be safe.
    /// Returns rows affected
    /// </summary>
    public abstract Task<int> ExecuteWithoutFetchAsync(ICrudQueryExecutionContext context, Query query);
  }

  /// <summary>
  /// Represents an entity that can execute a query. The implementation may be called by multiple threads and must be safe
  /// </summary>
  public abstract class CRUDQueryHandler<TStore> : CrudQueryHandler where TStore : ICrudDataStore
  {
    protected CRUDQueryHandler(TStore store, string name) : base(store, name) {}
    protected CRUDQueryHandler(TStore store, QuerySource source) : base(store, source) { }

    /// <summary>
    /// Store instance that handler is under
    /// </summary>
    public new TStore Store => (TStore)base.Store;
  }

  /// <summary>
  /// Represents an entity that can execute a query. The implementation may be called by multiple threads and must be safe.
  /// Adds instrumentation logic, generally providers should derive their implementations from this base,
  /// </summary>
  /// <remarks>
  /// The handler is purposed for ASYNC-first use, having its sync methods short circuit to async versions.
  /// Override async methods while implementing logic
  /// </remarks>
  public abstract class InstrumentedCrudQueryHandler<TStore, TStoreContext> : CRUDQueryHandler<TStore>
                          where TStore : ICrudDataStore
                          where TStoreContext : ICrudQueryExecutionContext
  {
    /// <summary>
    /// Added to queries when the schema of supported AST (Abstract Syntax Tree) should be returned instead of schema of data itself.
    /// Used for advanced filters with AST
    /// </summary>
    public const string AST_REFLECTION_QUERY_PARAM = "_____AST_REFLECTION_QUERY_PARAM";

    public InstrumentedCrudQueryHandler(TStore store, string name) : base(store, name) { }
    public InstrumentedCrudQueryHandler(TStore store, QuerySource source) : base(store, source) { }

    protected IInstrumentation Instruments => Store.App.Instrumentation;
    protected bool Instrumented => Instruments.Enabled && ((Store as IInstrumentable)?.InstrumentationEnabled ?? false);

    protected T Profile<T>(Func<T> body)
    {
      var sw = Timeter.StartNew();
      var result = body();
      if (Instrumented)
      {
        var el = sw.ElapsedMs;
        Instrumentation.DataQueryLatency.Emit(Instruments, Store.TargetName, GetType().Name, el);
      }
      return result;
    }

    protected async Task<T> ProfileAsync<T>(Func<Task<T>> body)
    {
      var sw = Timeter.StartNew();
      var result = await body();
      if (Instrumented)
      {
        var el = sw.ElapsedMs;
        Instrumentation.DataQueryLatency.Emit(Instruments, Store.TargetName, GetType().Name, el);
      }
      return result;
    }

    public override RowsetBase Execute(ICrudQueryExecutionContext context, Query query, bool oneRow = false)
      => Profile(() => ExecuteAsync(context, query, oneRow).GetAwaiter().GetResult());

    public sealed override async Task<RowsetBase> ExecuteAsync(ICrudQueryExecutionContext context, Query query, bool oneRow = false)
      => await ProfileAsync(() => ExecuteAsync((TStoreContext)context, query, oneRow));

    public virtual Task<RowsetBase> ExecuteAsync(TStoreContext context, Query query, bool oneRow = false)
      => throw new NotImplementedException($"{this.GetType().FullName}.{nameof(ExecuteAsync)}");


    public override Cursor OpenCursor(ICrudQueryExecutionContext context, Query query)
     => Profile(() => OpenCursorAsync(context, query).GetAwaiter().GetResult());

    public sealed override async Task<Cursor> OpenCursorAsync(ICrudQueryExecutionContext context, Query query)
     => await ProfileAsync(() => OpenCursorAsync((TStoreContext)context, query));

    public virtual Task<Cursor> OpenCursorAsync(TStoreContext context, Query query)
      => throw new NotImplementedException($"{this.GetType().FullName}.{nameof(OpenCursorAsync)}");



    public override int ExecuteWithoutFetch(ICrudQueryExecutionContext context, Query query)
      => Profile(() => ExecuteWithoutFetchAsync(context, query).GetAwaiter().GetResult());

    public sealed override async Task<int> ExecuteWithoutFetchAsync(ICrudQueryExecutionContext context, Query query)
      => await ProfileAsync(() => ExecuteWithoutFetchAsync((TStoreContext)context, query));

    public virtual Task<int> ExecuteWithoutFetchAsync(TStoreContext context, Query query)
      => throw new NotImplementedException($"{this.GetType().FullName}.{nameof(ExecuteWithoutFetchAsync)}");



    public override Schema GetSchema(ICrudQueryExecutionContext context, Query query)
      => Profile(() => GetSchemaAsync(context, query).GetAwaiter().GetResult());

    public sealed override async Task<Schema> GetSchemaAsync(ICrudQueryExecutionContext context, Query query)
     => await ProfileAsync(() => GetSchemaAsync((TStoreContext)context, query));

    /// <summary>
    /// Default implementation looks for AST_REFLECTION_QUERY_PARAM, and if it is present, then fetches schema from
    /// the AST not query, otherwise gets schema from query itself
    /// </summary>
    public virtual async Task<Schema> GetSchemaAsync(TStoreContext context, Query query)
    {
      if (query.Any(p => p.Name.EqualsOrdSenseCase(AST_REFLECTION_QUERY_PARAM)))
      {
        var astSchema = GetAstReflectionSchema(context, query);
        return astSchema;
      }

      var result = await GetQuerySchemaAsync(context, query);
      return result;
    }

    /// <summary>
    /// Override to get schema from query. Default implementation throws "NotImplemented" exception
    /// </summary>
    public virtual Task<Schema> GetQuerySchemaAsync(TStoreContext context, Query query)
    {
      throw new NotSupportedException($"{this.GetType().FullName}.{nameof(GetSchemaAsync)}");
    }

    /// <summary>
    /// Override to reflect on AST. ASTs are used for complex ad hoc expressions, so you can specify
    /// what fields an AST supports
    /// </summary>
    protected virtual Schema GetAstReflectionSchema(TStoreContext context, Query query)
    {
      throw new NotSupportedException($"{this.GetType().FullName}.{nameof(GetAstReflectionSchema)}");
    }
  }
}
