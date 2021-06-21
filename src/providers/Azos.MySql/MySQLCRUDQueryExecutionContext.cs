/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using MySql.Data.MySqlClient;

namespace Azos.Data.Access.MySql
{
  /// <summary>
  /// Provides query execution environment in MySql context
  /// </summary>
  public struct MySqlCRUDQueryExecutionContext : ICRUDQueryExecutionContext
  {
    public readonly MySQLDataStoreBase  DataStore;
    public readonly MySqlConnection  Connection;
    public readonly MySqlTransaction Transaction;

    public MySqlCRUDQueryExecutionContext(MySQLDataStoreBase  store, MySqlConnection cnn, MySqlTransaction trans)
    {
        DataStore = store;
        Connection = cnn;
        Transaction = trans;
    }


    /// <summary>
    /// Based on store settings, converts CLR value to MySQL-acceptable value, i.e. GDID -> BYTE[].
    /// </summary>
    public object CLRValueToDB(MySQLDataStoreBase store, object value, out MySqlDbType? convertedDbType)
    {
      return CRUDGenerator.CLRValueToDB(DataStore, value, out convertedDbType);
    }

    /// <summary>
    /// Based on store settings, converts query parameters into MySQL-acceptable values, i.e. GDID -> BYTe[].
    /// This function is not idempotent
    /// </summary>
    public void ConvertParameters(MySqlParameterCollection pars)
    {
      CRUDGenerator.ConvertParameters(DataStore, pars);
    }
  }
}
