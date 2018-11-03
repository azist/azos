using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Data.Access;

namespace Azos.Sky.Mdb
{
  /// <summary>
  /// Provides facade for ICrudOperations and ICRUDTransactionOperations
  /// executed against the particular shard returned by the MDB areas partition / routing
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

    public Task<int> DeleteAsync(Doc doc, IDataStoreKey key = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.DeleteAsync(doc, key);
    }

    public int ExecuteWithoutFetch(params Query[] queries)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.ExecuteWithoutFetch(queries);
    }

    public Task<int> ExecuteWithoutFetchAsync(params Query[] queries)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.ExecuteWithoutFetchAsync(queries);
    }

    public Schema GetSchema(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.GetSchema(query);
    }

    public Task<Schema> GetSchemaAsync(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.GetSchemaAsync(query);
    }

    public int Insert(Doc doc, FieldFilterFunc filter = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.Insert(doc, filter);
    }

    public Task<int> InsertAsync(Doc doc, FieldFilterFunc filter = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.InsertAsync(doc, filter);
    }

    public List<RowsetBase> Load(params Query[] queries)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.Load(queries);
    }

    public Task<List<RowsetBase>> LoadAsync(params Query[] queries)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.LoadAsync(queries);
    }

    public Doc LoadOneDoc(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.LoadOneDoc(query);
    }

    public Task<Doc> LoadOneDocAsync(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.LoadOneDocAsync(query);
    }

    public RowsetBase LoadOneRowset(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.LoadOneRowset(query);
    }

    public Task<RowsetBase> LoadOneRowsetAsync(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.LoadOneRowsetAsync(query);
    }

    public Cursor OpenCursor(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.OpenCursor(query);
    }

    public Task<Cursor> OpenCursorAsync(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.OpenCursorAsync(query);
    }

    public int Save(params RowsetBase[] rowsets)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.Save(rowsets);
    }

    public Task<int> SaveAsync(params RowsetBase[] rowsets)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.SaveAsync(rowsets);
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

    public Task<int> UpdateAsync(Doc doc, IDataStoreKey key = null, FieldFilterFunc filter = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.UpdateAsync(doc, key, filter);
    }

    public int Upsert(Doc doc, FieldFilterFunc filter = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.Upsert(doc, filter);
    }

    public Task<int> UpsertAsync(Doc doc, FieldFilterFunc filter = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.UpsertAsync(doc, filter);
    }

    public CRUDTransaction BeginTransaction(IsolationLevel iso = IsolationLevel.ReadCommitted,
                                            TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.BeginTransaction(iso, behavior);
    }

    public Task<CRUDTransaction> BeginTransactionAsync(IsolationLevel iso = IsolationLevel.ReadCommitted,
                                                       TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.BeginTransactionAsync(iso, behavior);
    }

    public bool SupportsTransactions
    {
      get { return Shard.Area.PhysicalDataStore.SupportsTransactions; }
    }
  }
}
