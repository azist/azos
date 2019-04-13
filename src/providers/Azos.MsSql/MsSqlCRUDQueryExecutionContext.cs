/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Data;
using System.Data.SqlClient;

namespace Azos.Data.Access.MsSql
{
    /// <summary>
    /// Provides query execution environment in Oracle context
    /// </summary>
    public struct MsSqlCRUDQueryExecutionContext : ICRUDQueryExecutionContext
    {
       public readonly MsSqlDataStoreBase  DataStore;
       public readonly SqlConnection  Connection;
       public readonly SqlTransaction Transaction;

       public MsSqlCRUDQueryExecutionContext(MsSqlDataStoreBase  store, SqlConnection cnn, SqlTransaction trans)
       {
            DataStore = store;
            Connection = cnn;
            Transaction = trans;
       }


       /// <summary>
       /// Based on store settings, converts CLR value to Oracle-acceptable value, i.e. GDID -> BYTE[].
       /// </summary>
       public (object value, SqlDbType? dbType) CLRValueToDB(object value, string explicitDbType)
       {
          return CRUDGenerator.CLRValueToDB(DataStore, value, explicitDbType);
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
