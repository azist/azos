/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;

namespace Azos.Data.Access
{

#warning Who uses this class - delete?
  /*
  public sealed class NotImplementedCRUDDataStore : ICRUDDataStoreImplementation
  {

    public NotImplementedCRUDDataStore()
    {

    }


    public CRUDQueryHandler MakeScriptQueryHandler(QuerySource querySource)
    {
      throw new NotImplementedException();
    }

    public string ScriptFileSuffix
    {
      get { throw new NotImplementedException(); }
    }

    public CRUDDataStoreType StoreType
    {
      get { throw new NotImplementedException(); }
    }

    public ICRUDQueryResolver QueryResolver
    {
      get { throw new NotImplementedException(); }
    }

    public bool SupportsTrueAsynchrony
    {
      get { throw new NotImplementedException(); }
    }

    public Schema GetSchema(Query query)
    {
      throw new NotImplementedException();
    }

    public Task<Schema> GetSchemaAsync(Query query)
    {
      throw new NotImplementedException();
    }

    public List<RowsetBase> Load(params Query[] queries)
    {
      throw new NotImplementedException();
    }

    public Task<List<RowsetBase>> LoadAsync(params Query[] queries)
    {
      throw new NotImplementedException();
    }

    public RowsetBase LoadOneRowset(Query query)
    {
      throw new NotImplementedException();
    }

    public Task<RowsetBase> LoadOneRowsetAsync(Query query)
    {
      throw new NotImplementedException();
    }

    public Doc LoadOneDoc(Query query)
    {
      throw new NotImplementedException();
    }

    public Task<Doc> LoadOneDocAsync(Query query)
    {
      throw new NotImplementedException();
    }

    public Cursor OpenCursor(Query query)
    {
      throw new NotImplementedException();
    }

    public Task<Cursor> OpenCursorAsync(Query query)
    {
      throw new NotImplementedException();
    }

    public int Save(params RowsetBase[] rowsets)
    {
      throw new NotImplementedException();
    }

    public Task<int> SaveAsync(params RowsetBase[] rowsets)
    {
      throw new NotImplementedException();
    }

    public int ExecuteWithoutFetch(params Query[] queries)
    {
      throw new NotImplementedException();
    }

    public Task<int> ExecuteWithoutFetchAsync(params Query[] queries)
    {
      throw new NotImplementedException();
    }

    public int Insert(Doc row, FieldFilterFunc filter = null)
    {
      throw new NotImplementedException();
    }

    public Task<int> InsertAsync(Doc row, FieldFilterFunc filter = null)
    {
      throw new NotImplementedException();
    }

    public int Upsert(Doc row, FieldFilterFunc filter = null)
    {
      throw new NotImplementedException();
    }

    public Task<int> UpsertAsync(Doc row, FieldFilterFunc filter = null)
    {
      throw new NotImplementedException();
    }

    public int Update(Doc row, IDataStoreKey key = null, FieldFilterFunc filter = null)
    {
      throw new NotImplementedException();
    }

    public Task<int> UpdateAsync(Doc row, IDataStoreKey key = null, FieldFilterFunc filter = null)
    {
      throw new NotImplementedException();
    }

    public int Delete(Doc row, IDataStoreKey key = null)
    {
      throw new NotImplementedException();
    }

    public Task<int> DeleteAsync(Doc row, IDataStoreKey key = null)
    {
      throw new NotImplementedException();
    }

    public bool SupportsTransactions
    {
      get { throw new NotImplementedException(); }
    }

    public CRUDTransaction BeginTransaction(System.Data.IsolationLevel iso = IsolationLevel.ReadCommitted, TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose)
    {
      throw new NotImplementedException();
    }

    public Task<CRUDTransaction> BeginTransactionAsync(System.Data.IsolationLevel iso = IsolationLevel.ReadCommitted, TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose)
    {
      throw new NotImplementedException();
    }

    public StoreLogLevel LogLevel
    {
      get
      {
        throw new NotImplementedException();
      }
      set
      {
        throw new NotImplementedException();
      }
    }

    public string TargetName
    {
      get { throw new NotImplementedException(); }
    }

    public void TestConnection()
    {
      throw new NotImplementedException();
    }

    public ulong ComponentSID
    {
      get { throw new NotImplementedException(); }
    }

    public object ComponentDirector
    {
      get { throw new NotImplementedException(); }
    }

    public string ComponentCommonName
    {
      get { throw new NotImplementedException(); }
    }

    public void Dispose()
    {
    }

    public void Configure(Conf.IConfigSectionNode node)
    {
    }

    public bool InstrumentationEnabled
    {
      get
      {
        throw new NotImplementedException();
      }
      set
      {
        throw new NotImplementedException();
      }
    }

    public IEnumerable<KeyValuePair<string, Type>> ExternalParameters
    {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
    {
      throw new NotImplementedException();
    }

    public bool ExternalGetParameter(string name, out object value, params string[] groups)
    {
      throw new NotImplementedException();
    }

    public bool ExternalSetParameter(string name, object value, params string[] groups)
    {
      throw new NotImplementedException();
    }
  }
  */
}
