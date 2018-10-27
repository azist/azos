/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azos.Data.Access
{

    /// <summary>
    /// Specifies how transaction scope behaves on scope exit
    /// </summary>
    public enum TransactionDisposeBehavior
    {
        CommitOnDispose = 0,
        RollbackOnDispose
    }

    /// <summary>
    /// Denotes transaction statuses
    /// </summary>
    public enum TransactionStatus
    {
        Open = 0,
        Comitted,
        RolledBack
    }

    /// <summary>
    /// Represents an abstract base for CRUDTransactions that perform particular backend CRUD work in overriden classes
    /// </summary>
    public abstract class CRUDTransaction : DisposableObject, ICRUDOperations
    {
      #region .ctor/.dctor

        protected CRUDTransaction(
              ICRUDDataStoreImplementation store,
              TransactionDisposeBehavior disposeBehavior = TransactionDisposeBehavior.CommitOnDispose)
        {
            m_Store = store;
            m_DisposeBahavior = disposeBehavior;
        }


        protected override void Destructor()
        {
            if (m_Status==TransactionStatus.Open)
            {
                if (m_DisposeBahavior==TransactionDisposeBehavior.CommitOnDispose)
                  Commit();
                else
                  Rollback();
            }
        }

      #endregion

      #region Fields

        protected ICRUDDataStoreImplementation m_Store;
        private TransactionStatus m_Status;
        private TransactionDisposeBehavior m_DisposeBahavior;

      #endregion


      #region Properties


        /// <summary>
        /// References the store instance that started this transaction
        /// </summary>
        public ICRUDDataStore DataStore
        {
            get { return m_Store;}
        }

        /// <summary>
        /// Returns current transaction status
        /// </summary>
        public TransactionStatus Status { get{ return m_Status; } }

        /// <summary>
        /// Specifies how transaction should be finalized on dispose: comitted or rolledback if it is still open
        /// </summary>
        public TransactionDisposeBehavior DisposeBehavior { get{ return m_DisposeBahavior; } }


        /// <summary>
        /// Returns true when backend supports true asynchronous operations, such as the ones that do not create extra threads/empty tasks
        /// </summary>
        public bool SupportsTrueAsynchrony{ get{ return m_Store.SupportsTrueAsynchrony;}}

      #endregion

      #region Public

        #region ICRUDOperations

            public Schema GetSchema(Query query)
            {
                CheckOpenStatus("GetSchema");
                return DoGetSchema(query);
            }

            public Task<Schema> GetSchemaAsync(Query query)
            {
                CheckOpenStatus("GetSchema");
                return DoGetSchemaAsync(query);
            }

            public List<RowsetBase> Load(params Query[] queries)
            {
                CheckOpenStatus("Load");
                return DoLoad(false, queries);
            }

            public Task<List<RowsetBase>> LoadAsync(params Query[] queries)
            {
                CheckOpenStatus("Load");
                return DoLoadAsync(false, queries);
            }


            public int ExecuteWithoutFetch(params Query[] queries)
            {
                CheckOpenStatus("ExecuteWithoutFetch");
                return DoExecuteWithoutFetch(queries);
            }

            public Task<int> ExecuteWithoutFetchAsync(params Query[] queries)
            {
                CheckOpenStatus("ExecuteWithoutFetch");
                return DoExecuteWithoutFetchAsync(queries);
            }

            public RowsetBase LoadOneRowset(Query query)
            {
                return Load(query).FirstOrDefault();
            }

            public Task<RowsetBase> LoadOneRowsetAsync(Query query)
            {
                return LoadAsync(query).ContinueWith( antecedent => antecedent.Result.FirstOrDefault());
            }

            public Doc LoadOneDoc(Query query)
            {
                RowsetBase rset = DoLoad(true, query).FirstOrDefault();
                if (rset!=null) return rset.FirstOrDefault();
                return null;
            }

            public Task<Doc> LoadOneDocAsync(Query query)
            {
                return DoLoadAsync(true, query).ContinueWith(
                  antecedent =>
                  {
                    var rset = antecedent.Result.FirstOrDefault();
                    if (rset!=null) return rset.FirstOrDefault();
                    return null;
                  });
            }

            public Cursor OpenCursor(Query query)
            {
                return DoOpenCursor(query);
            }

            public Task<Cursor> OpenCursorAsync(Query query)
            {
                return DoOpenCursorAsync(query);
            }

            public int Save(params RowsetBase[] tables)
            {
                CheckOpenStatus("Save");
                return DoSave(tables);
            }

            public Task<int> SaveAsync(params RowsetBase[] tables)
            {
                CheckOpenStatus("Save");
                return DoSaveAsync(tables);
            }

            public int Insert(Doc doc, FieldFilterFunc filter = null)
            {
                CheckOpenStatus("Insert");
                return DoInsert(doc, filter);
            }

            public Task<int> InsertAsync(Doc doc, FieldFilterFunc filter = null)
            {
                CheckOpenStatus("Insert");
                return DoInsertAsync(doc, filter);
            }


            public int Upsert(Doc doc, FieldFilterFunc filter = null)
            {
                CheckOpenStatus("Upsert");
                return DoUpsert(doc, filter);
            }

            public Task<int> UpsertAsync(Doc doc, FieldFilterFunc filter = null)
            {
                CheckOpenStatus("Upsert");
                return DoUpsertAsync(doc, filter);
            }


            public int Update(Doc doc, IDataStoreKey key = null, FieldFilterFunc filter = null)
            {
                CheckOpenStatus("Update");
                return DoUpdate(doc, key, filter);
            }

            public Task<int> UpdateAsync(Doc doc, IDataStoreKey key = null, FieldFilterFunc filter = null)
            {
                CheckOpenStatus("Update");
                return DoUpdateAsync(doc, key, filter);
            }


            public int Delete(Doc doc, IDataStoreKey key = null)
            {
                CheckOpenStatus("Delete");
                return DoDelete(doc, key);
            }

            public Task<int> DeleteAsync(Doc doc, IDataStoreKey key = null)
            {
                CheckOpenStatus("Delete");
                return DoDeleteAsync(doc, key);
            }

        #endregion

        public void Commit()
        {
            CheckOpenStatus("Commit");
            DoCommit();
            m_Status = TransactionStatus.Comitted;
        }

        public void Rollback()
        {
            CheckOpenStatus("Rollback");
            DoRollback();
            m_Status = TransactionStatus.RolledBack;
        }
      #endregion

      #region Protected


        protected void CheckOpenStatus(string operation)
        {
            if (m_Status!=TransactionStatus.Open)
                throw new DataAccessException(StringConsts.CRUD_TRANSACTION_IS_NOT_OPEN_ERROR.Args(operation, m_Status));
        }


        protected abstract Schema DoGetSchema(Query query);
        protected abstract Task<Schema> DoGetSchemaAsync(Query query);

        protected abstract List<RowsetBase> DoLoad(bool oneRow, params Query[] queries);
        protected abstract Task<List<RowsetBase>> DoLoadAsync(bool oneRow, params Query[] queries);

        protected abstract Cursor DoOpenCursor(Query query);
        protected abstract Task<Cursor> DoOpenCursorAsync(Query query);

        protected abstract int DoExecuteWithoutFetch(params Query[] queries);
        protected abstract Task<int> DoExecuteWithoutFetchAsync(params Query[] queries);

        protected abstract int  DoSave(params RowsetBase[] tables);
        protected abstract Task<int>  DoSaveAsync(params RowsetBase[] tables);

        protected abstract int  DoInsert(Doc doc, FieldFilterFunc filter = null);
        protected abstract Task<int>  DoInsertAsync(Doc doc, FieldFilterFunc filter = null);

        protected abstract int  DoUpsert(Doc doc, FieldFilterFunc filter = null);
        protected abstract Task<int>  DoUpsertAsync(Doc doc, FieldFilterFunc filter = null);

        protected abstract int  DoUpdate(Doc doc, IDataStoreKey key, FieldFilterFunc filter = null);
        protected abstract Task<int>  DoUpdateAsync(Doc doc, IDataStoreKey key, FieldFilterFunc filter = null);

        protected abstract int  DoDelete(Doc doc, IDataStoreKey key);
        protected abstract Task<int>  DoDeleteAsync(Doc doc, IDataStoreKey key);

        protected abstract void  DoCommit();

        protected abstract void  DoRollback();

      #endregion
    }
}
