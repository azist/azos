/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using System.Data.SqlClient;

namespace Azos.Data.Access.MsSql
{
    /// <summary>
    /// Represents MsSql CRUD transaction
    /// </summary>
    public sealed class MsSqlCRUDTransaction : CrudTransaction
    {
      #region .ctor/.dctor
      internal MsSqlCRUDTransaction(MsSqlCRUDDataStoreBase store, SqlConnection cnn, IsolationLevel iso, TransactionDisposeBehavior disposeBehavior) : base (store, disposeBehavior)
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
      private SqlConnection m_Connection;
      private SqlTransaction m_Transaction;
      #endregion

      #region Properties
      internal MsSqlCRUDDataStoreBase Store => (MsSqlCRUDDataStoreBase)m_Store;

      /// <summary>
      /// Returns the underlying MySQL connection that this transaction works through
      /// </summary>
      public SqlConnection Connection { get {return m_Connection;} }

      /// <summary>
      /// Returns the underlying MySQL transaction that this instance represents. Do not call Commit/Rollback method on this property directly
      /// </summary>
      public SqlTransaction Transaction { get {return m_Transaction;} }
      #endregion


      protected override Schema DoGetSchema(Query query)
       => Store.DoGetSchemaAsync(m_Connection, m_Transaction, query).GetAwaiter().GetResult();

      protected override Task<Schema> DoGetSchemaAsync(Query query)
       => Store.DoGetSchemaAsync(m_Connection, m_Transaction, query);

      protected override List<RowsetBase> DoLoad(bool oneDoc, params Query[] queries)
       => Store.DoLoadAsync(m_Connection, m_Transaction, queries, oneDoc).GetAwaiter().GetResult();

      protected override Task<List<RowsetBase>> DoLoadAsync(bool oneDoc, params Query[] queries)
       => Store.DoLoadAsync(m_Connection, m_Transaction, queries, oneDoc);

      protected override Cursor DoOpenCursor(Query query)
       => Store.DoOpenCursorAsync(m_Connection, m_Transaction, query).GetAwaiter().GetResult();

      protected override Task<Cursor> DoOpenCursorAsync(Query query)
       => Store.DoOpenCursorAsync(m_Connection, m_Transaction, query);

      protected override int DoExecuteWithoutFetch(params Query[] queries)
       => Store.DoExecuteWithoutFetchAsync(m_Connection, m_Transaction, queries).GetAwaiter().GetResult();

      protected override Task<int> DoExecuteWithoutFetchAsync(params Query[] queries)
       => Store.DoExecuteWithoutFetchAsync(m_Connection, m_Transaction, queries);

      protected override int DoSave(params RowsetBase[] rowsets)
       => Store.DoSaveAsync(m_Connection, m_Transaction, rowsets).GetAwaiter().GetResult();

      protected override Task<int> DoSaveAsync(params RowsetBase[] rowsets)
       => Store.DoSaveAsync(m_Connection, m_Transaction, rowsets);

      protected override int DoInsert(Doc doc, FieldFilterFunc filter = null)
        => Store.DoInsertAsync(m_Connection, m_Transaction, doc, filter).GetAwaiter().GetResult();

      protected override Task<int> DoInsertAsync(Doc doc, FieldFilterFunc filter = null)
        => Store.DoInsertAsync(m_Connection, m_Transaction, doc, filter);

      protected override int DoUpsert(Doc doc, FieldFilterFunc filter = null)
        => Store.DoUpsertAsync(m_Connection, m_Transaction, doc, filter).GetAwaiter().GetResult();

      protected override Task<int> DoUpsertAsync(Doc doc, FieldFilterFunc filter = null)
        => Store.DoUpsertAsync(m_Connection, m_Transaction, doc, filter);

      protected override int DoUpdate(Doc doc, IDataStoreKey key, FieldFilterFunc filter = null)
        => Store.DoUpdateAsync(m_Connection, m_Transaction, doc, key, filter).GetAwaiter().GetResult();

      protected override Task<int> DoUpdateAsync(Doc doc, IDataStoreKey key, FieldFilterFunc filter = null)
        => Store.DoUpdateAsync(m_Connection, m_Transaction, doc, key, filter);

      protected override int DoDelete(Doc doc, IDataStoreKey key)
        => Store.DoDeleteAsync(m_Connection, m_Transaction, doc, key).GetAwaiter().GetResult();

      protected override Task<int> DoDeleteAsync(Doc doc, IDataStoreKey key)
        => Store.DoDeleteAsync(m_Connection, m_Transaction, doc, key);

      protected override void DoCommit()
      {
          m_Transaction.Commit();
      }

      protected override void DoRollback()
      {
          m_Transaction.Rollback();
      }
    }
}
