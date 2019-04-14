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

using Azos.Serialization.JSON;
using System.Data.SqlClient;
using System.Data;

namespace Azos.Data.Access.MsSql
{
  internal static class CRUDGenerator
  {

      public static async Task<int> CRUDInsert(MsSqlDataStoreBase store, SqlConnection cnn, SqlTransaction trans, Doc doc, FieldFilterFunc filter)
      {
        try
        {
            return await crudInsert(store, cnn, trans, doc, filter);
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

      public static async Task<int> CRUDUpdate(MsSqlDataStoreBase store, SqlConnection cnn, SqlTransaction trans, Doc doc, IDataStoreKey key, FieldFilterFunc filter)
      {
        try
        {
            return await crudUpdate(store, cnn, trans, doc, key, filter);
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

      public static async Task<int> CRUDUpsert(MsSqlDataStoreBase store, SqlConnection cnn, SqlTransaction trans, Doc doc, FieldFilterFunc filter)
      {
        try
        {
            return await crudUpsert(store, cnn, trans, doc, filter);
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

      public static async Task<int> CRUDDelete(MsSqlDataStoreBase store, SqlConnection cnn, SqlTransaction trans, Doc doc, IDataStoreKey key)
      {
        try
        {
            return await crudDelete(store, cnn, trans, doc, key);
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
        tableName = "TBL_" + schema.TypedDocType.Name;//without namespace

      var tableAttr = schema.GetTableAttrForTarget(target);
      if (tableAttr!=null && tableAttr.Name.IsNotNullOrWhiteSpace()) tableName = tableAttr.Name;

      return tableName;
    }


    private static async Task<int> crudInsert(MsSqlDataStoreBase store, SqlConnection cnn, SqlTransaction trans, Doc doc, FieldFilterFunc filter)
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

        if (filter!=null)
        {
          if (!filter(doc, null, fld)) continue;
        }

        var fname = store.AdjustObjectNameCasing( fld.GetBackendNameForTarget(target) );

        var converted = getDbFieldValue(doc, fld, fattr, store);


        cnames.AppendFormat(" [{0}],", fname);

        if ( converted.value != null)
        {
          var pname = string.Format("@VAL{0}", vpidx);

          values.AppendFormat(" {0},", pname);

          var par = new SqlParameter();
//Console.WriteLine(doc.Schema.ToJson());
//Console.WriteLine("{0}|{1}: OrclDbTyp.{2} = ({3}){4}".Args(fld.NonNullableType.FullName, pname, converted.dbType, converted.value.GetType().FullName, converted.value));
          par.ParameterName = pname;
          par.Value = converted.value;
          if (converted.dbType.HasValue) par.SqlDbType = converted.dbType.Value;
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


      string tableName = store.AdjustObjectNameCasing( getTableName(doc.Schema, target) );

      using(var cmd = cnn.CreateCommand())
      {
        var sql = "INSERT INTO [{0}] ({1}) VALUES ({2})".Args( tableName, cnames, values);

        cmd.Transaction = trans;
        cmd.CommandText = sql;
        cmd.Parameters.AddRange(vparams.ToArray());
      //  ConvertParameters(store, cmd.Parameters);
        try
        {
            var affected = await cmd.ExecuteNonQueryAsync();
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




    private static async Task<int> crudUpdate(MsSqlDataStoreBase store, SqlConnection cnn, SqlTransaction trans, Doc doc, IDataStoreKey key, FieldFilterFunc filter)
    {
      var target = store.TargetName;
      var values = new StringBuilder();
      var vparams = new List<SqlParameter>();
      var vpidx = 0;
      foreach (var fld in doc.Schema.FieldDefs)
      {
        var fattr = fld[target];
        if (fattr==null) continue;

        var fname =  fld.GetBackendNameForTarget(target);

        //20141008 DKh Skip update of key fields
        //20160124 DKh add update of keys if IDataStoreKey is present
        if (fattr.Key && !GeneratorUtils.HasFieldInNamedKey(fname, key)) continue;

        fname = store.AdjustObjectNameCasing(fname);

        if (fattr.StoreFlag != StoreFlag.LoadAndStore && fattr.StoreFlag != StoreFlag.OnlyStore) continue;

        if (filter!=null)
        {
          if (!filter(doc, key, fld)) continue;
        }


        var converted = getDbFieldValue(doc, fld, fattr, store);


        if ( converted.value != null)
        {
          var pname = string.Format("@VAL{0}", vpidx);

          values.AppendFormat(" [{0}] = {1},", fname, pname);

          var par = new SqlParameter();
          par.ParameterName = pname;
          par.Value = converted.value;
          if (converted.dbType.HasValue) par.SqlDbType = converted.dbType.Value;
          vparams.Add(par);

          vpidx++;
        }
        else
        {
         values.AppendFormat(" [{0}] = NULL,", fname);
        }
      }//foreach

      if (values.Length > 0)
      {
        values.Remove(values.Length - 1, 1);// remove ","
      }
      else
        return 0;//nothing to do


      string tableName = store.AdjustObjectNameCasing( getTableName(doc.Schema, target) );

      using(var cmd = cnn.CreateCommand())
      {
        var sql = string.Empty;

        var pk = key ?? doc.GetDataStoreKey(target);

        if (pk == null)
            throw new MsSqlDataAccessException(StringConsts.KEY_UNAVAILABLE_ERROR);

        var where = GeneratorUtils.KeyToWhere(pk, cmd.Parameters);

        if (!string.IsNullOrEmpty(where))
            sql = "UPDATE [{0}] T1  SET {1} WHERE {2}".Args( tableName, values, where);
        else
            throw new MsSqlDataAccessException(StringConsts.BROAD_UPDATE_ERROR);//20141008 DKh BROAD update

        cmd.Transaction = trans;
        cmd.CommandText = sql;
        cmd.Parameters.AddRange(vparams.ToArray());
        //  ConvertParameters(store, cmd.Parameters);

//dbg(cmd);

        try
        {
            var affected = await cmd.ExecuteNonQueryAsync();
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


    private static async Task<int> crudUpsert(MsSqlDataStoreBase store, SqlConnection cnn, SqlTransaction trans, Doc doc, FieldFilterFunc filter)
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

        fname = store.AdjustObjectNameCasing( fname );

        var converted = getDbFieldValue(doc, fld, fattr, store);


        cnames.AppendFormat(" [{0}],", fname);

        if ( converted.value != null)
        {
                var pname = string.Format("@VAL{0}", vpidx);

                values.AppendFormat(" {0},", pname);

                if (!fattr.Key)
                    upserts.AppendFormat(" [{0}] = {1},", fname, pname);

                var par = new SqlParameter();
                par.ParameterName = pname;
                par.Value = converted;
                if (converted.dbType.HasValue) par.SqlDbType = converted.dbType.Value;
                vparams.Add(par);

                vpidx++;
        }
        else
        {
                values.Append(" NULL,");
                upserts.AppendFormat(" [{0}] = NULL,", fname);
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


      string tableName = store.AdjustObjectNameCasing( getTableName(doc.Schema, target) );

      using(var cmd = cnn.CreateCommand())
      {
        var sql =
        @"INSERT INTO [{0}] ({1}) VALUES ({2}) ON DUPLICATE KEY UPDATE {3}".Args( tableName, cnames, values, upserts);

        cmd.Transaction = trans;
        cmd.CommandText = sql;
        cmd.Parameters.AddRange(vparams.ToArray());
     //   ConvertParameters(store, cmd.Parameters);

        try
        {
            var affected = await cmd.ExecuteNonQueryAsync();
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





    private static async Task<int> crudDelete(MsSqlDataStoreBase store, SqlConnection cnn, SqlTransaction trans, Doc doc, IDataStoreKey key)
    {
      var target = store.TargetName;
      string tableName = store.AdjustObjectNameCasing( getTableName(doc.Schema, target) );

      using (var cmd = cnn.CreateCommand())
      {
        var pk = key ?? doc.GetDataStoreKey(target);

        if (pk == null)
            throw new MsSqlDataAccessException(StringConsts.KEY_UNAVAILABLE_ERROR);

        var where = GeneratorUtils.KeyToWhere(pk, cmd.Parameters);

        cmd.Transaction = trans;
        if (!string.IsNullOrEmpty(where))
            cmd.CommandText = string.Format("DELETE FROM [{0}] T1 WHERE {1}",tableName, where);
        else
            cmd.CommandText = string.Format("DELETE FROM [{0}] T1", tableName);

        ConvertParameters(store, cmd.Parameters);

//dbg(cmd);

        try
        {
            var affected = await cmd.ExecuteNonQueryAsync();
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

    private static (object value, SqlDbType? dbType) getDbFieldValue(Doc doc, Schema.FieldDef fld, FieldAttribute fattr, MsSqlDataStoreBase store)
    {
      var result = doc.GetFieldValue(fld);
      return CLRValueToDB(store, result, fattr.BackendType);
    }

    internal static (object value, SqlDbType? dbType) CLRValueToDB(MsSqlDataStoreBase store, object value, string explicitDbType)
    {
      SqlDbType? convertedDbType = null;

      if (explicitDbType.IsNotNullOrWhiteSpace())
        convertedDbType = explicitDbType.AsNullableEnum<SqlDbType>();

      if (value==null) return (null, convertedDbType);

      if (value is ulong ulng)
      {
        value = (decimal)ulng;
        convertedDbType = SqlDbType.Decimal;

      }else if (value is DateTime clrDt && convertedDbType== SqlDbType.Date)
      {
        // Expected DATE got NUmber
        // convertedDbType = OracleDbType.Int64;
        // value = clrDt.Ticks;// new global::Oracle.ManagedDataAccess.Types.OracleDate(clrDt);

        //Expected NUMBER got DATE
       // value = new global::Oracle.ManagedDataAccess.Types.OracleTimeStamp(clrDt);
       // value = new global::Oracle.ManagedDataAccess.Types.OracleDate(clrDt);
       // convertedDbType = OracleDbType.NVarchar2;
       // value = clrDt.ToString("yyyyMMddHHmmss");
      }
      else if (value is GDID gdid)
      {
        if (gdid.IsZero)
        {
          return (null, convertedDbType);
        }

        if(store.FullGDIDS)
        {
          value = gdid.Bytes;//be very careful with byte ordering of GDID for index optimization
          convertedDbType = SqlDbType.Binary;
          //todo 20190106 DKh: This needs to be tested for performance
        }
        else
        {
          value = (decimal)gdid.ID;
          convertedDbType = SqlDbType.Decimal;
        }
      }
      else if (value is bool)
      {
        if (store.StringBool)
        {
          value = (bool)value ? store.StringForTrue : store.StringForFalse;
          convertedDbType = SqlDbType.Char;
        }
      }

      return (value, convertedDbType);
    }

    internal static void ConvertParameters(MsSqlDataStoreBase store, SqlParameterCollection pars)
    {
      if (pars==null) return;
      for(var i=0; i<pars.Count; i++)
      {
        var par = pars[i];
        var converted = CLRValueToDB(store, par.Value, null);
        par.Value = converted.value;
        if (converted.dbType.HasValue)
         par.SqlDbType = converted.dbType.Value;
      }
    }

    internal static void dbg(SqlCommand cmd)
    {
      Console.WriteLine(cmd.CommandText);
      foreach (var p in cmd.Parameters.Cast<SqlParameter>())
        Console.WriteLine("{0}: SqlDbTyp.{1} = ({2}){3}".Args(p.ParameterName, p.SqlDbType, p.Value.GetType().FullName, p.Value));
    }

    #endregion


  }
}
