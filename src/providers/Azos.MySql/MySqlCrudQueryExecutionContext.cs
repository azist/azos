/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Serialization.JSON;

using MySqlConnector;

namespace Azos.Data.Access.MySql
{
  /// <summary>
  /// Provides query execution environment in MySql context
  /// </summary>
  public sealed class MySqlCrudQueryExecutionContext : ICrudQueryExecutionContext
  {
    private object m_State;

    public readonly MySqlDataStoreBase  DataStore;
    public readonly MySqlConnection     Connection;
    public readonly MySqlTransaction    Transaction;

    public MySqlCrudQueryExecutionContext(MySqlDataStoreBase  store, MySqlConnection cnn, MySqlTransaction trans)
    {
      DataStore = store;
      Connection = cnn;
      Transaction = trans;
    }


    /// <summary>
    /// Sets the state object. Since CrudQueryHandlers are designed for multi-threaded use, you can not use their fields,
    /// however sometimes it is necessary to carry over some correlation/state data between calls as a part of query execution.
    /// The SetState(object)/GetState(of T) methods are used by query implementations for that purpose
    /// </summary>
    public void SetState(object state) => m_State = state;

    /// <summary>
    /// Gets the state object as T. The call needs to be paired with SetState(T) Since CrudQueryHandlers are designed
    /// for multi-threaded use, you can not use their fields, however sometimes it is necessary to carry over some correlation/state
    /// data between calls as a part of query execution. The SetState(object)/GetState(of T) methods are used by query implementations for that purpose
    /// </summary>
    public T GetState<T>() => m_State.CastTo<T>("ExecutionContext.State");

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
