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

using MySqlConnector;

namespace Azos.Data.Access.MySql
{
  /// <summary>
  /// Generates CRUD SQL statements for MySql
  /// </summary>
  internal static class CrudGenerator
  {
    public static async Task<int> CRUDInsert(MySqlDataStoreBase store, MySqlConnection cnn, MySqlTransaction trans, Doc doc, FieldFilterFunc filter)
    {
      try
      {
          return await crudInsert(store, cnn, trans, doc, filter);
      }
      catch(Exception error)
      {
          throw new MySqlDataAccessException(
                        StringConsts.CRUD_STATEMENT_EXECUTION_ERROR.Args("insert", error.ToMessageWithType(), error),
                        error,
                        KeyViolationKind.Unspecified,
                        KeyViolationName(error));
      }
    }

    public static async Task<int> CRUDUpdate(MySqlDataStoreBase store, MySqlConnection cnn, MySqlTransaction trans, Doc doc, IDataStoreKey key, FieldFilterFunc filter)
    {
      try
      {
          return await crudUpdate(store, cnn, trans, doc, key, filter);
      }
      catch(Exception error)
      {
          throw new MySqlDataAccessException(
                        StringConsts.CRUD_STATEMENT_EXECUTION_ERROR.Args("update",
                        error.ToMessageWithType(), error),
                        error,
                        KeyViolationKind.Unspecified,
                        KeyViolationName(error)
                        );
      }
    }

    public static async Task<int> CRUDUpsert(MySqlDataStoreBase store, MySqlConnection cnn, MySqlTransaction trans, Doc doc, FieldFilterFunc filter)
    {
      try
      {
          return await crudUpsert(store, cnn, trans, doc, filter);
      }
      catch(Exception error)
      {
          throw new MySqlDataAccessException(
                      StringConsts.CRUD_STATEMENT_EXECUTION_ERROR.Args("upsert", error.ToMessageWithType(), error),
                      error,
                      KeyViolationKind.Unspecified,
                      KeyViolationName(error));
      }
    }

    public static async Task<int> CRUDDelete(MySqlDataStoreBase store, MySqlConnection cnn, MySqlTransaction trans, Doc doc, IDataStoreKey key)
    {
      try
      {
          return await crudDelete(store, cnn, trans, doc, key);
      }
      catch(Exception error)
      {
          throw new MySqlDataAccessException(StringConsts.CRUD_STATEMENT_EXECUTION_ERROR.Args("delete", error.ToMessageWithType(), error), error);
      }
    }


    #region .pvt impl.

    internal static string KeyViolationName(Exception error)
    {
      if (error==null) return null;
      var msg = error.Message;

      var i = msg.IndexOf("Duplicate", StringComparison.InvariantCultureIgnoreCase);
      if (i<0) return null;
      var j = msg.IndexOf("for key", StringComparison.InvariantCultureIgnoreCase);
      if (j>i && j<msg.Length)
        return msg.Substring(j);
      return null;
    }


    private static string getTableName(Schema schema, string target)
    {
      string tableName = schema.Name;

      if (schema.TypedDocType!=null)
        tableName = "tbl_" + schema.TypedDocType.Name;//without namespace

      var tableAttr = schema.GetSchemaAttrForTarget(target);
      if (tableAttr!=null && tableAttr.Name.IsNotNullOrWhiteSpace()) tableName = tableAttr.Name;
      return tableName;
    }

    private static int timeoutSec(MySqlDataStoreBase store)
    {
      var t = store.DefaultTimeoutMs;
      if (t <= 0) return 0;
      return t / 1000;
    }

    private static async Task<int> crudInsert(MySqlDataStoreBase store, MySqlConnection cnn, MySqlTransaction trans, Doc doc, FieldFilterFunc filter)
    {
      var target = store.TargetName;
      var cnames = new StringBuilder();
      var values = new StringBuilder();
      var vparams = new List<MySqlParameter>();
      var vpidx = 0;
      foreach (var fld in doc.Schema.FieldDefs)
      {
        var fattr = fld[target];
        if (fattr==null) continue;

        if (fattr.StoreFlag != StoreFlag.LoadAndStore && fattr.StoreFlag != StoreFlag.OnlyStore) continue;

        if (filter!=null)
        {
          if (!filter(doc, null, fld)) continue;
        }

        var fname = store.AdjustObjectNameCasing( fld.GetBackendNameForTarget(target) );

        var converted = getDbFieldValue(doc, fld, fattr, store);


        cnames.AppendFormat(" `{0}`,", fname);

        if ( converted.value != null)
        {
          var pname = string.Format("?VAL{0}", vpidx);

          values.AppendFormat(" {0},", pname);

          var par = new MySqlParameter();
          par.ParameterName = pname;
          par.Value = converted.value;
          if (converted.dbType.HasValue) par.MySqlDbType = converted.dbType.Value;
          vparams.Add(par);

          vpidx++;
        }
        else
        {
          values.Append(" NULL,");
        }
      }//foreach

      if (cnames.Length > 0)
      {
        cnames.Remove(cnames.Length - 1, 1);// remove ","
        values.Remove(values.Length - 1, 1);// remove ","
      }
      else
        return 0;//nothing to do


      string tableName = store.AdjustObjectNameCasing(getTableName(doc.Schema, target));

      using (var cmd = cnn.CreateCommand())
      {
        var sql = "INSERT INTO `{0}` ({1}) VALUES ({2})".Args( tableName, cnames, values);

        cmd.Transaction = trans;
        cmd.CommandText = sql;
        cmd.CommandTimeout = timeoutSec(store);
        cmd.Parameters.AddRange(vparams.ToArray());
        //ConvertParameters(store, cmd.Parameters);
        try
        {
          var affected = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
          GeneratorUtils.LogCommand(store, "insert-ok", cmd, null);
          return affected;
        }
        catch(Exception error)
        {
          GeneratorUtils.LogCommand(store, "insert-error", cmd, error);
          throw;
        }
      }//using command
    }




    private static async Task<int> crudUpdate(MySqlDataStoreBase store, MySqlConnection cnn, MySqlTransaction trans, Doc doc, IDataStoreKey key, FieldFilterFunc filter)
    {
      var target = store.TargetName;
      var values = new StringBuilder();
      var vparams = new List<MySqlParameter>();
      var vpidx = 0;
      foreach (var fld in doc.Schema.FieldDefs)
      {
        var fattr = fld[target];
        if (fattr==null) continue;

        var fname = fld.GetBackendNameForTarget(target);

        //20141008 DKh Skip update of key fields
        //20160124 DKh add update of keys if IDataStoreKey is present
        if (fattr.Key && !GeneratorUtils.HasFieldInNamedKey(fname, key)) continue;

        if (fattr.StoreFlag != StoreFlag.LoadAndStore && fattr.StoreFlag != StoreFlag.OnlyStore) continue;

        if (filter!=null)
        {
          if (!filter(doc, key, fld)) continue;
        }


        var converted = getDbFieldValue(doc, fld, fattr, store);


        if ( converted.value != null)
        {
          var pname = string.Format("?VAL{0}", vpidx);

          values.AppendFormat(" `{0}` = {1},", fname, pname);

          var par = new MySqlParameter();
          par.ParameterName = pname;
          par.Value = converted.value;
          if (converted.dbType.HasValue) par.MySqlDbType = converted.dbType.Value;
          vparams.Add(par);

          vpidx++;
        }
        else
        {
          values.AppendFormat(" `{0}` = NULL,", fname);
        }
      }//foreach

      if (values.Length > 0)
      {
        values.Remove(values.Length - 1, 1);// remove ","
      }
      else
        return 0;//nothing to do


      string tableName = store.AdjustObjectNameCasing(getTableName(doc.Schema, target));

      using (var cmd = cnn.CreateCommand())
      {
        var sql = string.Empty;

        var pk = key ?? doc.GetDataStoreKey(target);

        if (pk == null)
            throw new MySqlDataAccessException(StringConsts.KEY_UNAVAILABLE_ERROR);

        var where = GeneratorUtils.KeyToWhere(pk, cmd.Parameters);

        if (!string.IsNullOrEmpty(where))
            sql = "UPDATE `{0}` T1  SET {1} WHERE {2}".Args( tableName, values, where);
        else
            throw new MySqlDataAccessException(StringConsts.BROAD_UPDATE_ERROR);//20141008 DKh BROAD update

        cmd.Transaction = trans;
        cmd.CommandText = sql;
        cmd.CommandTimeout = timeoutSec(store);
        cmd.Parameters.AddRange(vparams.ToArray());
        ConvertParameters(store, cmd.Parameters);

        try
        {
            var affected = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            GeneratorUtils.LogCommand(store, "update-ok", cmd, null);
            return affected;
        }
        catch(Exception error)
        {
            GeneratorUtils.LogCommand(store, "update-error", cmd, error);
            throw;
        }
      }//using command
    }


    private static async Task<int> crudUpsert(MySqlDataStoreBase store, MySqlConnection cnn, MySqlTransaction trans, Doc doc, FieldFilterFunc filter)
    {
      var target = store.TargetName;
      var cnames = new StringBuilder();
      var values = new StringBuilder();
      var upserts = new StringBuilder();
      var vparams = new List<MySqlParameter>();
      var vpidx = 0;
      foreach (var fld in doc.Schema.FieldDefs)
      {
        var fattr = fld[target];
        if (fattr==null) continue;

        if (fattr.StoreFlag != StoreFlag.LoadAndStore && fattr.StoreFlag != StoreFlag.OnlyStore) continue;


        if (filter!=null)
        {
          if (!filter(doc, null, fld)) continue;
        }

        var fname = fld.GetBackendNameForTarget(target);

        fname = store.AdjustObjectNameCasing(fname);

        var converted = getDbFieldValue(doc, fld, fattr, store);


        cnames.AppendFormat(" `{0}`,", fname);

        if ( converted.value != null)
        {
          var pname = string.Format("?VAL{0}", vpidx);

          values.AppendFormat(" {0},", pname);

          if (!fattr.Key)
              upserts.AppendFormat(" `{0}` = {1},", fname, pname);

          var par = new MySqlParameter();
          par.ParameterName = pname;
          par.Value = converted.value;
          if (converted.dbType.HasValue) par.MySqlDbType = converted.dbType.Value;
          vparams.Add(par);

          vpidx++;
        }
        else
        {
            values.Append(" NULL,");
            upserts.AppendFormat(" `{0}` = NULL,", fname);
        }
      }//foreach

      if (cnames.Length > 0 && upserts.Length > 0)
      {
        cnames.Remove(cnames.Length - 1, 1);// remove ","
        upserts.Remove(upserts.Length - 1, 1);// remove ","
        values.Remove(values.Length - 1, 1);// remove ","
      }
      else
        return 0;//nothing to do


      string tableName = store.AdjustObjectNameCasing(getTableName(doc.Schema, target));

      using (var cmd = cnn.CreateCommand())
      {
        var sql =
        @"INSERT INTO `{0}` ({1}) VALUES ({2}) ON DUPLICATE KEY UPDATE {3}".Args( tableName, cnames, values, upserts);

        cmd.Transaction = trans;
        cmd.CommandText = sql;
        cmd.CommandTimeout = timeoutSec(store);
        cmd.Parameters.AddRange(vparams.ToArray());
        ConvertParameters(store, cmd.Parameters);

        try
        {
          var affected = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
          GeneratorUtils.LogCommand(store, "upsert-ok", cmd, null);
          return affected;
        }
        catch(Exception error)
        {
          GeneratorUtils.LogCommand(store, "upsert-error", cmd, error);
          throw;
        }
      }//using command
    }





    private static async Task<int> crudDelete(MySqlDataStoreBase store, MySqlConnection cnn, MySqlTransaction trans, Doc doc, IDataStoreKey key)
    {
      var target = store.TargetName;
      string tableName = store.AdjustObjectNameCasing(getTableName(doc.Schema, target));

      using (var cmd = cnn.CreateCommand())
      {
        var pk = key ?? doc.GetDataStoreKey(target);

        if (pk == null)
            throw new MySqlDataAccessException(StringConsts.KEY_UNAVAILABLE_ERROR);

        var where = GeneratorUtils.KeyToWhere(pk, cmd.Parameters);

        cmd.Transaction = trans;
        if (!string.IsNullOrEmpty(where))
            cmd.CommandText = string.Format("DELETE T1 FROM `{0}` T1 WHERE {1}",tableName, where);
        else
            cmd.CommandText = string.Format("DELETE T1 FROM `{0}` T1", tableName);

        cmd.CommandTimeout = timeoutSec(store);
        ConvertParameters(store, cmd.Parameters);

        try
        {
          var affected = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
          GeneratorUtils.LogCommand(store, "delete-ok", cmd, null);
          return affected;
        }
        catch(Exception error)
        {
          GeneratorUtils.LogCommand(store, "delete-error", cmd, error);
          throw;
        }
      }//using command
    }

    private static (object value, MySqlDbType? dbType) getDbFieldValue(Doc doc, Schema.FieldDef fld, FieldAttribute fattr, MySqlDataStoreBase store)
    {
      var result = doc.GetFieldValue(fld);
      return CLRValueToDB(store, result, fattr.BackendType);
    }

    internal static (object value, MySqlDbType? dbType) CLRValueToDB(MySqlDataStoreBase store, object value, string explicitDbType)
    {
      MySqlDbType? convertedDbType = null;

      if (explicitDbType.IsNotNullOrWhiteSpace())
        convertedDbType = explicitDbType.AsNullableEnum<MySqlDbType>();

      if (value==null) return (null, convertedDbType);

      if (value is GDID)
      {
        if (((GDID)value).IsZero)
        {
          return (null, MySqlDbType.Null);
        }

        if(store.FullGDIDS)
        {
          value = (object)((GDID)value).Bytes;//be very careful with byte ordering of GDID for index optimization
          convertedDbType = MySqlDbType.Binary;
        }
        else
        {
          value = (object)((GDID)value).ID;
          convertedDbType = MySqlDbType.UInt64;
        }

      }
      else if (value is bool)
      {
        if (store.StringBool)
        {
          value = (bool)value ? store.StringForTrue : store.StringForFalse;
          convertedDbType = MySqlDbType.String;
        }
      }
      else if (value is Atom atm)
      {
        if (atm.IsZero) return (null, MySqlDbType.Null);
        value = atm.ID;
        convertedDbType = MySqlDbType.UInt64;
      }
      else if (value is EntityId eid)
      {
        if (!eid.IsAssigned) return (null, MySqlDbType.Null);
        value = eid.AsString;
        convertedDbType = MySqlDbType.String;
      }

      return (value, convertedDbType);
    }

    internal static void ConvertParameters(MySqlDataStoreBase store, MySqlParameterCollection pars)
    {
      if (pars == null) return;
      for (var i = 0; i < pars.Count; i++)
      {
        var par = pars[i];
        var converted = CLRValueToDB(store, par.Value, null);
        par.Value = converted.value;
        if (converted.dbType.HasValue)
          par.MySqlDbType = converted.dbType.Value;
      }
    }

    //used for provider dev
    //////internal static void dbg(MySqlCommand cmd)
    //////{
    //////  Console.WriteLine(cmd.CommandText);
    //////  foreach (var p in cmd.Parameters.Cast<MySqlParameter>())
    //////    Console.WriteLine("{0}: OrclDbTyp.{1} = ({2}){3}".Args(p.ParameterName, p.MySqlDbType, p.Value.GetType().FullName, p.Value));
    //////}

    #endregion
  }
}
