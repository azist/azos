
using System.Data;
using System.Data.SqlClient;

namespace Azos.Data.Access.MsSql
{
    /// <summary>
    /// Provides query execution environment in MySql context
    /// </summary>
    public struct MySqlCRUDQueryExecutionContext : ICRUDQueryExecutionContext
    {
       public readonly MsSqlDataStoreBase  DataStore;
       public readonly SqlConnection  Connection;
       public readonly SqlTransaction Transaction;

       public MySqlCRUDQueryExecutionContext(MsSqlDataStoreBase  store, SqlConnection cnn, SqlTransaction trans)
       {
            DataStore = store;
            Connection = cnn;
            Transaction = trans;
       }


       /// <summary>
       /// Based on store settings, converts CLR value to MySQL-acceptable value, i.e. GDID -> BYTE[].
       /// </summary>
       public object CLRValueToDB(MsSqlDataStoreBase store, object value, out SqlDbType? convertedDbType)
       {
          return CRUDGenerator.CLRValueToDB(DataStore, value, out convertedDbType);
       }

       /// <summary>
       /// Based on store settings, converts query parameters into MySQL-acceptable values, i.e. GDID -> BYTe[].
       /// This function is not idempotent
       /// </summary>
       public void ConvertParameters(SqlParameterCollection pars)
       {
          CRUDGenerator.ConvertParameters(DataStore, pars);
       }

    }
}
