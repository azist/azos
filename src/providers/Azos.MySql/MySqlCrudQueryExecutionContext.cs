/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using MySqlConnector;

namespace Azos.Data.Access.MySql
{
  /// <summary>
  /// Provides query execution environment in MySql context
  /// </summary>
  public struct MySqlCrudQueryExecutionContext : ICrudQueryExecutionContext
  {
    public readonly MySqlDataStoreBase  DataStore;
    public readonly MySqlConnection  Connection;
    public readonly MySqlTransaction Transaction;

    public MySqlCrudQueryExecutionContext(MySqlDataStoreBase  store, MySqlConnection cnn, MySqlTransaction trans)
    {
        DataStore = store;
        Connection = cnn;
        Transaction = trans;
    }


    /// <summary>
    /// Based on store settings, converts CLR value to MySql-acceptable value, i.e. GDID -> BYTE[].
    /// </summary>
    public (object value, MySqlDbType? dbType) CLRValueToDB(object value, string explicitDbType)
    {
      return CrudGenerator.CLRValueToDB(DataStore, value, explicitDbType);
    }

    /// <summary>
    /// Based on store settings, converts query parameters into MySQL-acceptable values, i.e. GDID -> BYTE[].
    /// This function is not idempotent
    /// </summary>
    public void ConvertParameters(MySqlParameterCollection pars)
    {
      CrudGenerator.ConvertParameters(DataStore, pars);
    }
  }
}
