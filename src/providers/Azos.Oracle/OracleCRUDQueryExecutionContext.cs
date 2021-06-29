/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Oracle.ManagedDataAccess.Client;

namespace Azos.Data.Access.Oracle
{
    /// <summary>
    /// Provides query execution environment in Oracle context
    /// </summary>
    public struct OracleCRUDQueryExecutionContext : ICrudQueryExecutionContext
    {
       public readonly OracleDataStoreBase  DataStore;
       public readonly OracleConnection  Connection;
       public readonly OracleTransaction Transaction;

       public OracleCRUDQueryExecutionContext(OracleDataStoreBase  store, OracleConnection cnn, OracleTransaction trans)
       {
            DataStore = store;
            Connection = cnn;
            Transaction = trans;
       }


       /// <summary>
       /// Based on store settings, converts CLR value to Oracle-acceptable value, i.e. GDID -> BYTE[].
       /// </summary>
       public (object value, OracleDbType? dbType) CLRValueToDB(object value, string explicitDbType)
       {
          return CRUDGenerator.CLRValueToDB(DataStore, value, explicitDbType);
       }

       /// <summary>
       /// Based on store settings, converts query parameters into MySQL-acceptable values, i.e. GDID -> BYTE[].
       /// This function is not idempotent
       /// </summary>
       public void ConvertParameters(OracleParameterCollection pars)
       {
          CRUDGenerator.ConvertParameters(DataStore, pars);
       }

    }
}
