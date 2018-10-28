
using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using System.Data.SqlClient;

namespace Azos.Data.Access.MsSql
{

  internal static class CRUDGenerator
  {

      public static int CRUDInsert(MsSqlDataStoreBase store, SqlConnection cnn, SqlTransaction trans, Doc doc, FieldFilterFunc filter)
      {
        try
        {
            return crudInsert(store, cnn, trans, doc, filter);
        }
        catch(Exception error)
        {
           throw new MsSqlDataAccessException(
                          StringConsts.CRUD_STATEMENT_EXECUTION_ERROR.Args("insert", error.ToMessageWithType(), error),
                          error,
                          KeyViolationKind.Unspecified,
                          KeyViolationName(error));
        }
      }

      public static int CRUDUpdate(MsSqlDataStoreBase store, SqlConnection cnn, SqlTransaction trans, Doc doc, IDataStoreKey key, FieldFilterFunc filter)
      {
        try
        {
            return crudUpdate(store, cnn, trans, doc, key, filter);
        }
        catch(Exception error)
        {
           throw new MsSqlDataAccessException(
                         StringConsts.CRUD_STATEMENT_EXECUTION_ERROR.Args("update",
                         error.ToMessageWithType(), error),
                         error,
                         KeyViolationKind.Unspecified,
                         KeyViolationName(error)
                         );
        }
      }

      public static int CRUDUpsert(MsSqlDataStoreBase store, SqlConnection cnn, SqlTransaction trans, Doc doc, FieldFilterFunc filter)
      {
        try
        {
            return crudUpsert(store, cnn, trans, doc, filter);
        }
        catch(Exception error)
        {
           throw new MsSqlDataAccessException(
                        StringConsts.CRUD_STATEMENT_EXECUTION_ERROR.Args("upsert", error.ToMessageWithType(), error),
                        error,
                        KeyViolationKind.Unspecified,
                        KeyViolationName(error));
        }
      }

      public static int CRUDDelete(MsSqlDataStoreBase store, SqlConnection cnn, SqlTransaction trans, Doc doc, IDataStoreKey key)
      {
        try
        {
            return crudDelete(store, cnn, trans, doc, key);
        }
        catch(Exception error)
        {
           throw new MsSqlDataAccessException(StringConsts.CRUD_STATEMENT_EXECUTION_ERROR.Args("delete", error.ToMessageWithType(), error), error);
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

      var tableAttr = schema.GetTableAttrForTarget(target);
      if (tableAttr!=null && tableAttr.Name.IsNotNullOrWhiteSpace()) tableName = tableAttr.Name;
      return tableName;
    }


    private static int crudInsert(MsSqlDataStoreBase store, SqlConnection cnn, SqlTransaction trans, Doc doc, FieldFilterFunc filter)
    {
      var target = store.TargetName;
      var cnames = new StringBuilder();
      var values = new StringBuilder();
      var vparams = new List<SqlParameter>();
      var vpidx = 0;
      foreach (var fld in doc.Schema.FieldDefs)
      {
        var fattr = fld[target];
        if (fattr==null) continue;

        if (fattr.StoreFlag != StoreFlag.LoadAndStore && fattr.StoreFlag != StoreFlag.OnlyStore) continue;

        if (filter!=null)//20160210 Dkh+SPol
        {
          if (!filter(doc, null, fld)) continue;
        }

        var fname = fld.GetBackendNameForTarget(target);

        var fvalue = getFieldValue(doc, fld.Order, store);


        cnames.AppendFormat(" `{0}`,", fname);

        if ( fvalue != null)
        {
                var pname = string.Format("@VAL{0}", vpidx);

                values.AppendFormat(" {0},", pname);

                var par = new SqlParameter();
                par.ParameterName = pname;
                par.Value = fvalue;
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


      string tableName = getTableName(doc.Schema, target);

      using(var cmd = cnn.CreateCommand())
      {
        var sql = "INSERT INTO `{0}` ({1}) VALUES ({2})".Args( tableName, cnames, values);

        cmd.Transaction = trans;
        cmd.CommandText = sql;
        cmd.Parameters.AddRange(vparams.ToArray());
        ConvertParameters(store, cmd.Parameters);
        try
        {
            var affected = cmd.ExecuteNonQuery();
            GeneratorUtils.LogCommand(store.LogLevel, "insert-ok", cmd, null);
            return affected;
        }
        catch(Exception error)
        {
            GeneratorUtils.LogCommand(store.LogLevel, "insert-error", cmd, error);
            throw;
        }
      }//using command
    }




    private static int crudUpdate(MsSqlDataStoreBase store, SqlConnection cnn, SqlTransaction trans, Doc doc, IDataStoreKey key, FieldFilterFunc filter)
    {
      var target = store.TargetName;
      var values = new StringBuilder();
      var vparams = new List<SqlParameter>();
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

        if (filter!=null)//20160210 Dkh+SPol
        {
          if (!filter(doc, key, fld)) continue;
        }


        var fvalue = getFieldValue(doc, fld.Order, store);


        if ( fvalue != null)
        {
                var pname = string.Format("@VAL{0}", vpidx);

                values.AppendFormat(" `{0}` = {1},", fname, pname);

                var par = new SqlParameter();
                par.ParameterName = pname;
                par.Value = fvalue;
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


      string tableName = getTableName(doc.Schema, target);

      using(var cmd = cnn.CreateCommand())
      {
        var sql = string.Empty;

        var pk = key ?? doc.GetDataStoreKey(target);

        if (pk == null)
            throw new MsSqlDataAccessException(StringConsts.KEY_UNAVAILABLE_ERROR);

        var where = GeneratorUtils.KeyToWhere(pk, cmd.Parameters);

        if (!string.IsNullOrEmpty(where))
            sql = "UPDATE `{0}` T1  SET {1} WHERE {2}".Args( tableName, values, where);
        else
            throw new MsSqlDataAccessException(StringConsts.BROAD_UPDATE_ERROR);//20141008 DKh BROAD update

        cmd.Transaction = trans;
        cmd.CommandText = sql;
        cmd.Parameters.AddRange(vparams.ToArray());
        ConvertParameters(store, cmd.Parameters);

        try
        {
            var affected = cmd.ExecuteNonQuery();
            GeneratorUtils.LogCommand(store.LogLevel, "update-ok", cmd, null);
            return affected;
        }
        catch(Exception error)
        {
            GeneratorUtils.LogCommand(store.LogLevel, "update-error", cmd, error);
            throw;
        }
      }//using command
    }


    private static int crudUpsert(MsSqlDataStoreBase store, SqlConnection cnn, SqlTransaction trans, Doc doc, FieldFilterFunc filter)
    {
      var target = store.TargetName;
      var cnames = new StringBuilder();
      var values = new StringBuilder();
      var upserts = new StringBuilder();
      var vparams = new List<SqlParameter>();
      var vpidx = 0;
      foreach (var fld in doc.Schema.FieldDefs)
      {
        var fattr = fld[target];
        if (fattr==null) continue;

        if (fattr.StoreFlag != StoreFlag.LoadAndStore && fattr.StoreFlag != StoreFlag.OnlyStore) continue;


        if (filter!=null)//20160210 Dkh+SPol
        {
          if (!filter(doc, null, fld)) continue;
        }

        var fname = fld.GetBackendNameForTarget(target);

        var fvalue = getFieldValue(doc, fld.Order, store);


        cnames.AppendFormat(" `{0}`,", fname);

        if ( fvalue != null)
        {
                var pname = string.Format("@VAL{0}", vpidx);

                values.AppendFormat(" {0},", pname);

                if (!fattr.Key)
                    upserts.AppendFormat(" `{0}` = {1},", fname, pname);

                var par = new SqlParameter();
                par.ParameterName = pname;
                par.Value = fvalue;
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


      string tableName = getTableName(doc.Schema, target);

      using(var cmd = cnn.CreateCommand())
      {
        var sql =
        @"INSERT INTO `{0}` ({1}) VALUES ({2}) ON DUPLICATE KEY UPDATE {3}".Args( tableName, cnames, values, upserts);

        cmd.Transaction = trans;
        cmd.CommandText = sql;
        cmd.Parameters.AddRange(vparams.ToArray());
        ConvertParameters(store, cmd.Parameters);

        try
        {
            var affected = cmd.ExecuteNonQuery();
            GeneratorUtils.LogCommand(store.LogLevel, "upsert-ok", cmd, null);
            return affected;
        }
        catch(Exception error)
        {
            GeneratorUtils.LogCommand(store.LogLevel, "upsert-error", cmd, error);
            throw;
        }
      }//using command
    }





    private static int crudDelete(MsSqlDataStoreBase store, SqlConnection cnn, SqlTransaction trans, Doc doc, IDataStoreKey key)
    {
      var target = store.TargetName;
      string tableName = getTableName(doc.Schema, target);

      using (var cmd = cnn.CreateCommand())
      {
        var pk = key ?? doc.GetDataStoreKey(target);

        if (pk == null)
            throw new MsSqlDataAccessException(StringConsts.KEY_UNAVAILABLE_ERROR);

        var where = GeneratorUtils.KeyToWhere(pk, cmd.Parameters);

        cmd.Transaction = trans;
        if (!string.IsNullOrEmpty(where))
            cmd.CommandText = string.Format("DELETE T1 FROM `{0}` T1 WHERE {1}",tableName, where);
        else
            cmd.CommandText = string.Format("DELETE T1 FROM `{0}` T1", tableName);

        ConvertParameters(store, cmd.Parameters);

        try
        {
            var affected = cmd.ExecuteNonQuery();
            GeneratorUtils.LogCommand(store.LogLevel, "delete-ok", cmd, null);
            return affected;
        }
        catch(Exception error)
        {
            GeneratorUtils.LogCommand(store.LogLevel, "delete-error", cmd, error);
            throw;
        }


      }//using command
    }

    private static object getFieldValue(Doc doc, int order, MsSqlDataStoreBase store)
    {
      var result = doc[order];

      SqlDbType? convertedDbType;
      return CLRValueToDB(store, result, out convertedDbType);
    }

    internal static object CLRValueToDB(MsSqlDataStoreBase store, object value, out SqlDbType? convertedDbType)
    {
      convertedDbType = null;

      if (value==null) return null;

      if (value is GDID)
      {
        if (((GDID)value).IsZero)
        {
          return null;
        }

        if(store.FullGDIDS)
        {
          value = (object)((GDID)value).Bytes;//be very careful with byte ordering of GDID for index optimization
          convertedDbType = SqlDbType.Binary;
        }
        else
        {
          value = (object)((GDID)value).ID;
          convertedDbType = SqlDbType.BigInt;
        }
      }
      else
      if (value is bool)
      {
        if (store.StringBool)
        {
          value = (bool)value ? store.StringForTrue : store.StringForFalse;
          convertedDbType = SqlDbType.VarChar;
        }
      }

      return value;
    }

    internal static void ConvertParameters(MsSqlDataStoreBase store, SqlParameterCollection pars)
    {
      if (pars==null) return;
      for(var i=0; i<pars.Count; i++)
      {
        var par = pars[i];
        SqlDbType? convertedDbType;
        par.Value = CLRValueToDB(store, par.Value, out convertedDbType);
        if (convertedDbType.HasValue)
         par.SqlDbType = convertedDbType.Value;
      }
    }


    #endregion


  }
}
