/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

using Azos.Log;

namespace Azos.Data.Access.MsSql
{
  /// <summary>
  /// Facilitates various SQL-construction and logging tasks
  /// </summary>
  public static class GeneratorUtils
  {

    public static string KeyToWhere(IDataStoreKey key, SqlParameterCollection parameters)
    {
      string where = null;

      if (key is CounterDataStoreKey)
      {
        where = "T1.COUNTER = @CTR";
        var par = new SqlParameter();
        par.ParameterName = "@CTR";
        par.Value = ((CounterDataStoreKey)key).Counter;

        parameters.Add(par);
      }
      else
      if (key is GDID gdid)
      {
        where = "T1.GDID = @CTR";
        var par = new SqlParameter();
        par.ParameterName = "@CTR";
        par.Value = (decimal)gdid.ID;

        parameters.Add(par);
      }
      else
        if (key is NameValueDataStoreKey)
        {
          var dict = key as NameValueDataStoreKey;
          var s = new StringBuilder();
          var idx = 0;

          foreach (var e in dict)
          {
            s.AppendFormat(" (T1.[{0}] = @P{1}) AND", e.Key, idx);
            var par = new SqlParameter();
            par.ParameterName = "@P" + idx.ToString();
            par.Value = e.Value;
            parameters.Add(par);

            idx++;
          }

          if (s.Length > 0) s.Remove(s.Length - 3, 3);//cut "AND"

          where = s.ToString();
        }
        else
          throw new MsSqlDataAccessException(StringConsts.INVALID_KEY_TYPE_ERROR);

      return where;
    }


    public static bool HasFieldInNamedKey(string fieldName, IDataStoreKey key)
    {
      var nvk = key as NameValueDataStoreKey;
      if (nvk==null || fieldName.IsNullOrWhiteSpace()) return false;
      return nvk.ContainsKey(fieldName);
    }

    public static void LogCommand(MsSqlDataStoreBase store, string from, SqlCommand cmd, Exception error)
    {
        if (store.DataLogLevel==StoreLogLevel.None) return;

        MessageType mt = store.DataLogLevel==StoreLogLevel.Debug ? MessageType.DebugSQL : MessageType.TraceSQL;

        var descr = new StringBuilder(512);
        descr.Append("Transaction: ");
        if (cmd.Transaction==null)
            descr.AppendLine("null");
        else
            descr.AppendLine(cmd.Transaction.IsolationLevel.ToString());
        foreach(var p in cmd.Parameters.Cast<SqlParameter>())
        {
            descr.AppendFormat("Parameter {0} = {1}", p.ParameterName, p.Value!=null?p.Value.ToString():"null");
        }

        var msg = new Message
        {
            Type = mt,
            From = from,
            Topic = MsSqlConsts.MSSQL_TOPIC,
            Exception = error,
            Text = cmd.CommandText,
            Parameters = descr.ToString()
        };

        store.App.Log.Write( msg );
    }


  }
}
