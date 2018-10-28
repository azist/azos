
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using System.Data.SqlClient;

namespace Azos.Data.Access.MsSql
{
    /// <summary>
    /// Represents MsSQL CRUD transaction
    /// </summary>
    public sealed class MsSqlCRUDTransaction : CRUDTransaction
    {
        #region .ctor/.dctor

            internal MsSqlCRUDTransaction(MsSqlDataStore store, SqlConnection cnn, IsolationLevel iso, TransactionDisposeBehavior disposeBehavior) : base (store, disposeBehavior)
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

            internal MsSqlDataStore Store
            {
                get { return m_Store as MsSqlDataStore; }
            }

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
        {
            return Store.DoGetSchema(m_Connection, m_Transaction, query);
        }

        protected override Task<Schema> DoGetSchemaAsync(Query query)
        {
            return TaskUtils.AsCompletedTask( () => this.DoGetSchema(query) );
        }

        protected override List<RowsetBase> DoLoad(bool oneDoc, params Query[] queries)
        {
            return Store.DoLoad(m_Connection, m_Transaction, queries, oneDoc);
        }

        protected override Task<List<RowsetBase>> DoLoadAsync(bool oneDoc, params Query[] queries)
        {
            return TaskUtils.AsCompletedTask( () => this.DoLoad(oneDoc, queries) );
        }

        protected override Cursor DoOpenCursor(Query query)
        {
            return Store.DoOpenCursor(m_Connection, m_Transaction, query);
        }

        protected override Task<Cursor> DoOpenCursorAsync(Query query)
        {
            return TaskUtils.AsCompletedTask( () => this.DoOpenCursor(query) );
        }

        protected override int DoExecuteWithoutFetch(params Query[] queries)
        {
            return Store.DoExecuteWithoutFetch(m_Connection, m_Transaction, queries);
        }

        protected override Task<int> DoExecuteWithoutFetchAsync(params Query[] queries)
        {
            return TaskUtils.AsCompletedTask( () => this.DoExecuteWithoutFetch(queries) );
        }

        protected override int DoSave(params RowsetBase[] rowsets)
        {
            return Store.DoSave(m_Connection, m_Transaction, rowsets);
        }

        protected override Task<int> DoSaveAsync(params RowsetBase[] rowsets)
        {
            return TaskUtils.AsCompletedTask( () => this.DoSave(rowsets) );
        }

        protected override int DoInsert(Doc doc, FieldFilterFunc filter = null)
        {
            return Store.DoInsert(m_Connection, m_Transaction, doc, filter);
        }

        protected override Task<int> DoInsertAsync(Doc doc, FieldFilterFunc filter = null)
        {
            return TaskUtils.AsCompletedTask( () => this.DoInsert(doc, filter) );
        }

        protected override int DoUpsert(Doc doc, FieldFilterFunc filter = null)
        {
            return Store.DoUpsert(m_Connection, m_Transaction, doc, filter);
        }

        protected override Task<int> DoUpsertAsync(Doc doc, FieldFilterFunc filter = null)
        {
            return TaskUtils.AsCompletedTask( () => this.DoUpsert(doc, filter) );
        }

        protected override int DoUpdate(Doc doc, IDataStoreKey key, FieldFilterFunc filter = null)
        {
            return Store.DoUpdate(m_Connection, m_Transaction, doc, key, filter);
        }

        protected override Task<int> DoUpdateAsync(Doc doc, IDataStoreKey key, FieldFilterFunc filter = null)
        {
            return TaskUtils.AsCompletedTask( () => this.DoUpdate(doc, key, filter) );
        }

        protected override int DoDelete(Doc doc, IDataStoreKey key)
        {
            return Store.DoDelete(m_Connection, m_Transaction, doc, key);
        }

        protected override Task<int> DoDeleteAsync(Doc doc, IDataStoreKey key)
        {
            return TaskUtils.AsCompletedTask( () => this.DoDelete(doc, key) );
        }

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
