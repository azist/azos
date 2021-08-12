/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Azos.Data.Access.Sharding
{
  /// <summary>
  /// Provides facade for ICrudOperations and ICrudTransactionOperations
  /// routed into a particular shard
  /// </summary>
  public struct CrudOperations : ICrudOperations, ICrudTransactionOperations
  {
    internal CrudOperations(IShard shard)
    {
      Shard = shard.NonNull(nameof(shard));
    }

    /// <summary>
    /// The shard that services this instance
    /// </summary>
    public readonly IShard Shard;

    internal IShardedCrudDataStoreImplementation Store => (IShardedCrudDataStoreImplementation)Shard.Store;

    public bool SupportsTrueAsynchrony => Store.PhysicalStore.SupportsTrueAsynchrony;
    public bool SupportsTransactions => Store.PhysicalStore.SupportsTransactions;

    public int Delete(Doc doc, IDataStoreKey key = null)
    {
      using (Store.MakeCallContext(Shard))
        return Store.PhysicalStore.Delete(doc, key);
    }

    public async Task<int> DeleteAsync(Doc doc, IDataStoreKey key = null)
    {
      using (Store.MakeCallContext(Shard))
        return await Store.PhysicalStore.DeleteAsync(doc, key).ConfigureAwait(false);
    }

    public int ExecuteWithoutFetch(params Query[] queries)
    {
      using (Store.MakeCallContext(Shard))
        return Store.PhysicalStore.ExecuteWithoutFetch(queries);
    }

    public async Task<int> ExecuteWithoutFetchAsync(params Query[] queries)
    {
      using (Store.MakeCallContext(Shard))
        return await Store.PhysicalStore.ExecuteWithoutFetchAsync(queries).ConfigureAwait(false);
    }

    public Schema GetSchema(Query query)
    {
      using (Store.MakeCallContext(Shard))
        return Store.PhysicalStore.GetSchema(query);
    }

    public async Task<Schema> GetSchemaAsync(Query query)
    {
      using (Store.MakeCallContext(Shard))
        return await Store.PhysicalStore.GetSchemaAsync(query).ConfigureAwait(false);
    }

    public int Insert(Doc doc, FieldFilterFunc filter = null)
    {
      using (Store.MakeCallContext(Shard))
        return Store.PhysicalStore.Insert(doc, filter);
    }

    public async Task<int> InsertAsync(Doc doc, FieldFilterFunc filter = null)
    {
      using (Store.MakeCallContext(Shard))
        return await Store.PhysicalStore.InsertAsync(doc, filter).ConfigureAwait(false);
    }

    public List<RowsetBase> Load(params Query[] queries)
    {
      using (Store.MakeCallContext(Shard))
        return Store.PhysicalStore.Load(queries);
    }

    public async Task<List<RowsetBase>> LoadAsync(params Query[] queries)
    {
      using (Store.MakeCallContext(Shard))
        return await Store.PhysicalStore.LoadAsync(queries).ConfigureAwait(false);
    }

    public Doc LoadOneDoc(Query query)
    {
      using (Store.MakeCallContext(Shard))
        return Store.PhysicalStore.LoadOneDoc(query);
    }

    public async Task<Doc> LoadOneDocAsync(Query query)
    {
      using (Store.MakeCallContext(Shard))
        return await Store.PhysicalStore.LoadOneDocAsync(query).ConfigureAwait(false);
    }

    public RowsetBase LoadOneRowset(Query query)
    {
      using (Store.MakeCallContext(Shard))
        return Store.PhysicalStore.LoadOneRowset(query);
    }

    public async Task<RowsetBase> LoadOneRowsetAsync(Query query)
    {
      using (Store.MakeCallContext(Shard))
        return await Store.PhysicalStore.LoadOneRowsetAsync(query).ConfigureAwait(false);
    }

    public Cursor OpenCursor(Query query)
    {
      using (Store.MakeCallContext(Shard))
        return Store.PhysicalStore.OpenCursor(query);
    }

    public async Task<Cursor> OpenCursorAsync(Query query)
    {
      using (Store.MakeCallContext(Shard))
        return await Store.PhysicalStore.OpenCursorAsync(query).ConfigureAwait(false);
    }

    public int Save(params RowsetBase[] rowsets)
    {
      using (Store.MakeCallContext(Shard))
        return Store.PhysicalStore.Save(rowsets);
    }

    public async Task<int> SaveAsync(params RowsetBase[] rowsets)
    {
      using (Store.MakeCallContext(Shard))
        return await Store.PhysicalStore.SaveAsync(rowsets).ConfigureAwait(false);
    }


    public int Update(Doc doc, IDataStoreKey key = null, FieldFilterFunc filter = null)
    {
      using (Store.MakeCallContext(Shard))
        return Store.PhysicalStore.Update(doc, key, filter);
    }

    public async Task<int> UpdateAsync(Doc doc, IDataStoreKey key = null, FieldFilterFunc filter = null)
    {
      using (Store.MakeCallContext(Shard))
        return await Store.PhysicalStore.UpdateAsync(doc, key, filter).ConfigureAwait(false);
    }

    public int Upsert(Doc doc, FieldFilterFunc filter = null)
    {
      using (Store.MakeCallContext(Shard))
        return Store.PhysicalStore.Upsert(doc, filter);
    }

    public async Task<int> UpsertAsync(Doc doc, FieldFilterFunc filter = null)
    {
      using (Store.MakeCallContext(Shard))
        return await Store.PhysicalStore.UpsertAsync(doc, filter).ConfigureAwait(false);
    }

    public CrudTransaction BeginTransaction(IsolationLevel iso = IsolationLevel.ReadCommitted,
                                            TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose)
    {
      using (Store.MakeCallContext(Shard))
        return Store.PhysicalStore.BeginTransaction(iso, behavior);
    }

    public async Task<CrudTransaction> BeginTransactionAsync(IsolationLevel iso = IsolationLevel.ReadCommitted,
                                                             TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose)
    {
      using (Store.MakeCallContext(Shard))
        return await Store.PhysicalStore.BeginTransactionAsync(iso, behavior).ConfigureAwait(false);
    }

  }
}
