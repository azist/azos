/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Data.Access;

namespace Azos.Sky.Mdb
{
  /// <summary>
  /// Provides facade for ICrudOperations and ICRUDTransactionOperations
  /// executed against the particular shard returned by the Mdb areas partition / routing
  /// </summary>
  public struct CRUDOperations : ICRUDOperations, ICRUDTransactionOperations
  {

    internal CRUDOperations(MdbArea.Partition.Shard shard)
    {
      this.Shard = shard;
    }

    /// <summary>
    /// The shard that services this instance
    /// </summary>
    public readonly MdbArea.Partition.Shard Shard;



    public int Delete(Doc doc, IDataStoreKey key = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.Delete(doc, key);
    }

    public async Task<int> DeleteAsync(Doc doc, IDataStoreKey key = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return await Shard.Area.PhysicalDataStore.DeleteAsync(doc, key).ConfigureAwait(false);
    }

    public int ExecuteWithoutFetch(params Query[] queries)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.ExecuteWithoutFetch(queries);
    }

    public async Task<int> ExecuteWithoutFetchAsync(params Query[] queries)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return await Shard.Area.PhysicalDataStore.ExecuteWithoutFetchAsync(queries).ConfigureAwait(false);
    }

    public Schema GetSchema(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.GetSchema(query);
    }

    public async Task<Schema> GetSchemaAsync(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return await Shard.Area.PhysicalDataStore.GetSchemaAsync(query).ConfigureAwait(false);
    }

    public int Insert(Doc doc, FieldFilterFunc filter = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.Insert(doc, filter);
    }

    public async Task<int> InsertAsync(Doc doc, FieldFilterFunc filter = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return await Shard.Area.PhysicalDataStore.InsertAsync(doc, filter).ConfigureAwait(false);
    }

    public List<RowsetBase> Load(params Query[] queries)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.Load(queries);
    }

    public async Task<List<RowsetBase>> LoadAsync(params Query[] queries)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return await Shard.Area.PhysicalDataStore.LoadAsync(queries).ConfigureAwait(false);
    }

    public Doc LoadOneDoc(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.LoadOneDoc(query);
    }

    public async Task<Doc> LoadOneDocAsync(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return await Shard.Area.PhysicalDataStore.LoadOneDocAsync(query).ConfigureAwait(false);
    }

    public RowsetBase LoadOneRowset(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.LoadOneRowset(query);
    }

    public async Task<RowsetBase> LoadOneRowsetAsync(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return await Shard.Area.PhysicalDataStore.LoadOneRowsetAsync(query).ConfigureAwait(false);
    }

    public Cursor OpenCursor(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.OpenCursor(query);
    }

    public async Task<Cursor> OpenCursorAsync(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return await Shard.Area.PhysicalDataStore.OpenCursorAsync(query).ConfigureAwait(false);
    }

    public int Save(params RowsetBase[] rowsets)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.Save(rowsets);
    }

    public async Task<int> SaveAsync(params RowsetBase[] rowsets)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return await Shard.Area.PhysicalDataStore.SaveAsync(rowsets).ConfigureAwait(false);
    }

    public bool SupportsTrueAsynchrony
    {
      get { return Shard.Area.PhysicalDataStore.SupportsTrueAsynchrony; }
    }

    public int Update(Doc doc, IDataStoreKey key = null, FieldFilterFunc filter = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.Update(doc, key, filter);
    }

    public async Task<int> UpdateAsync(Doc doc, IDataStoreKey key = null, FieldFilterFunc filter = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return await Shard.Area.PhysicalDataStore.UpdateAsync(doc, key, filter).ConfigureAwait(false);
    }

    public int Upsert(Doc doc, FieldFilterFunc filter = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.Upsert(doc, filter);
    }

    public async Task<int> UpsertAsync(Doc doc, FieldFilterFunc filter = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return await Shard.Area.PhysicalDataStore.UpsertAsync(doc, filter).ConfigureAwait(false);
    }

    public CRUDTransaction BeginTransaction(IsolationLevel iso = IsolationLevel.ReadCommitted,
                                            TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.BeginTransaction(iso, behavior);
    }

    public async Task<CRUDTransaction> BeginTransactionAsync(IsolationLevel iso = IsolationLevel.ReadCommitted,
                                                             TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return await Shard.Area.PhysicalDataStore.BeginTransactionAsync(iso, behavior).ConfigureAwait(false);
    }

    public bool SupportsTransactions
    {
      get { return Shard.Area.PhysicalDataStore.SupportsTransactions; }
    }
  }
}
