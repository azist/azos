/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using MySqlConnector;

namespace Azos.Data.Access.MySql
{
  /// <summary>
  /// Provides MySql extensions
  /// </summary>
  public static class Extensions
  {
    public static string AsStringField(this MySqlDataReader reader, string fld)
    {
      var val = reader[fld];
      if (val is DBNull) return null;
      return val.AsString();
    }

    public static bool? AsBoolField(this MySqlDataReader reader, string fld)
    {
      var val = reader[fld];
      if (val is DBNull) return null;
      return val.AsNullableBool();
    }

    public static DateTime? AsDateTimeField(this MySqlDataReader reader, string fld)
    {
      var val = reader[fld];
      if (val is DBNull) return null;
      return val.AsNullableDateTime();
    }

    public static int? AsIntField(this MySqlDataReader reader, string fld)
    {
      var val = reader[fld];
      if (val is DBNull) return null;
      return val.AsNullableInt();
    }

    public static long? AsLongField(this MySqlDataReader reader, string fld)
    {
      var val = reader[fld];
      if (val is DBNull) return null;
      return val.AsNullableLong();
    }

    public static float? AsFloatField(this MySqlDataReader reader, string fld)
    {
      var val = reader[fld];
      if (val is DBNull) return null;
      return val.AsNullableFloat();
    }

    public static double? AsDoubleField(this MySqlDataReader reader, string fld)
    {
      var val = reader[fld];
      if (val is DBNull) return null;
      return val.AsNullableDouble();
    }

    public static decimal? AsDecimalField(this MySqlDataReader reader, string fld)
    {
      var val = reader[fld];
      if (val is DBNull) return null;
      return val.AsNullableDecimal();
    }

    public static MySqlCommand AddParam(this MySqlCommand cmd, string paramKey, object paramValue, bool isRequired = false)
    {
      paramKey.NonBlank(paramKey);
      if (isRequired) paramValue.NonNull(paramKey);

      cmd.NonNull(nameof(cmd)).Parameters.AddWithValue(paramKey, paramValue);

      return cmd;
    }

  }
}
