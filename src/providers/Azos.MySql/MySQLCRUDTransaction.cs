/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

namespace Azos.Data.Access.MySql
{
  /// <summary>
  /// Represents MySQL CRUD transaction
  /// </summary>
  public sealed class MySqlCRUDTransaction : CRUDTransaction
  {
    #region .ctor/.dctor
    internal MySqlCRUDTransaction(MySqlCrudDataStoreBase store, MySqlConnection cnn, IsolationLevel iso, TransactionDisposeBehavior disposeBehavior) : base (store, disposeBehavior)
    {
        m_Connection = cnn;
        m_Transaction = cnn.BeginTransaction(iso);
    }

    protected override void Destructor()
    {
        base.Destructor();
        m_Connection.Dispose();
    }
    #endregion

    #region Fields
    private MySqlConnection m_Connection;
    private MySqlTransaction m_Transaction;
    #endregion

    #region Properties
    internal MySqlCrudDataStoreBase Store => m_Store.CastTo<MySqlCrudDataStoreBase>();

    /// <summary>
    /// Returns the underlying MySQL connection that this transaction works through
    /// </summary>
    public MySqlConnection Connection => m_Connection;

    /// <summary>
    /// Returns the underlying MySQL transaction that this instance represents. Do not call Commit/Rollback method on this property directly
    /// </summary>
    public MySqlTransaction Transaction => m_Transaction;
    #endregion


    protected override Schema DoGetSchema(Query query)
      => Store.DoGetSchemaAsync(m_Connection, m_Transaction, query).GetAwaiter().GetResult();

    protected override async Task<Schema> DoGetSchemaAsync(Query query)
      => await Store.DoGetSchemaAsync(m_Connection, m_Transaction, query).ConfigureAwait(false);

    protected override List<RowsetBase> DoLoad(bool oneDoc, params Query[] queries)
      => Store.DoLoadAsync(m_Connection, m_Transaction, queries, oneDoc).GetAwaiter().GetResult();

    protected override async Task<List<RowsetBase>> DoLoadAsync(bool oneDoc, params Query[] queries)
      => await Store.DoLoadAsync(m_Connection, m_Transaction, queries, oneDoc).ConfigureAwait(false);

    protected override Cursor DoOpenCursor(Query query)
      => Store.DoOpenCursorAsync(m_Connection, m_Transaction, query).GetAwaiter().GetResult();

    protected override async Task<Cursor> DoOpenCursorAsync(Query query)
      => await Store.DoOpenCursorAsync(m_Connection, m_Transaction, query).ConfigureAwait(false);

    protected override int DoExecuteWithoutFetch(params Query[] queries)
      => Store.DoExecuteWithoutFetchAsync(m_Connection, m_Transaction, queries).GetAwaiter().GetResult();

    protected override async Task<int> DoExecuteWithoutFetchAsync(params Query[] queries)
      => await Store.DoExecuteWithoutFetchAsync(m_Connection, m_Transaction, queries).ConfigureAwait(false);

    protected override int DoSave(params RowsetBase[] rowsets)
      => Store.DoSaveAsync(m_Connection, m_Transaction, rowsets).GetAwaiter().GetResult();

    protected override async Task<int> DoSaveAsync(params RowsetBase[] rowsets)
      => await Store.DoSaveAsync(m_Connection, m_Transaction, rowsets).ConfigureAwait(false);

    protected override int DoInsert(Doc doc, FieldFilterFunc filter = null)
      => Store.DoInsertAsync(m_Connection, m_Transaction, doc, filter).GetAwaiter().GetResult();

    protected override async Task<int> DoInsertAsync(Doc doc, FieldFilterFunc filter = null)
      => await Store.DoInsertAsync(m_Connection, m_Transaction, doc, filter).ConfigureAwait(false);

    protected override int DoUpsert(Doc doc, FieldFilterFunc filter = null)
      => Store.DoUpsertAsync(m_Connection, m_Transaction, doc, filter).GetAwaiter().GetResult();

    protected override async Task<int> DoUpsertAsync(Doc doc, FieldFilterFunc filter = null)
      => await Store.DoUpsertAsync(m_Connection, m_Transaction, doc, filter).ConfigureAwait(false);

    protected override int DoUpdate(Doc doc, IDataStoreKey key, FieldFilterFunc filter = null)
      => Store.DoUpdateAsync(m_Connection, m_Transaction, doc, key, filter).GetAwaiter().GetResult();

    protected override async Task<int> DoUpdateAsync(Doc doc, IDataStoreKey key, FieldFilterFunc filter = null)
      => await Store.DoUpdateAsync(m_Connection, m_Transaction, doc, key, filter).ConfigureAwait(false);

    protected override int DoDelete(Doc doc, IDataStoreKey key)
      => Store.DoDeleteAsync(m_Connection, m_Transaction, doc, key).GetAwaiter().GetResult();

    protected override async Task<int> DoDeleteAsync(Doc doc, IDataStoreKey key)
      => await Store.DoDeleteAsync(m_Connection, m_Transaction, doc, key).ConfigureAwait(false);

    protected override void DoCommit() =>  m_Transaction.Commit();
    protected override void DoRollback() => m_Transaction.Rollback();

  }
}
