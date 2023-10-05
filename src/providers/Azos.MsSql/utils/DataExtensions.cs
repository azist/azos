/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Time;

using Microsoft.Data.SqlClient;

namespace Azos.Data.Access.MsSql
{
  /// <summary>
  /// Provides Microsoft SQL extensions
  /// </summary>
  public static class DataExtensions
  {
    public static string AsStringField(this SqlDataReader reader, string fld, string dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
    {
      var val = reader[fld];
      if (val is DBNull) return null;
      return val.AsString(dflt, handling);
    }

    public static GDID AsGdidField(this SqlDataReader reader, string fld, GDID? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
    {
      var val = reader[fld];
      if (val is DBNull) return dflt ?? GDID.ZERO;
      return val.AsGDID(dflt ?? GDID.ZERO, handling);
    }

    public static GDID? AsNullableGdidField(this SqlDataReader reader, string fld, GDID? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
    {
      var val = reader[fld];
      if (val is DBNull) return null;
      return val.AsNullableGDID(dflt, handling);
    }

    public static Atom? AsAtomField(this SqlDataReader reader, string fld, Atom? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
    {
      var val = reader[fld];
      if (val is DBNull) return null;
      return val.AsNullableAtom(dflt, handling);
    }

    public static Guid? AsGuidField(this SqlDataReader reader, string fld, Guid? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
    {
      var val = reader[fld];
      if (val is DBNull) return null;
      return val.AsGUID(dflt ?? Guid.Empty, handling);
    }

    public static EntityId? AsEntityIdField(this SqlDataReader reader, string fld, EntityId? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
    {
      var val = reader[fld];
      if (val is DBNull) return null;
      return val.AsNullableEntityId(dflt, handling);
    }

    public static bool? AsBoolField(this SqlDataReader reader, string fld, bool? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
    {
      var val = reader[fld];
      if (val is DBNull) return null;
      return val.AsNullableBool(dflt, handling);
    }

    /// <summary>
    /// If styles not specified, defaults to UTC
    /// </summary>
    public static DateTime? AsDateTimeField(this SqlDataReader reader,
                                           string fld,
                                           DateTime? dflt = null,
                                           ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault,
                                           System.Globalization.DateTimeStyles? styles = null)
    {
      var val = reader[fld];
      if (val is DBNull) return null;

      if (!styles.HasValue)
      {
        styles = CoreConsts.UTC_TIMESTAMP_STYLES;
      }

      if(val is DateTime d &&
        (styles.Value.HasFlag(System.Globalization.DateTimeStyles.AssumeUniversal) ||
        styles.Value.HasFlag(System.Globalization.DateTimeStyles.AdjustToUniversal))) // 20220302 dkh
      {
        val = new DateTime(d.Ticks, DateTimeKind.Utc);
      }

      return val.AsNullableDateTime(dflt, handling, styles.Value);
    }

    public static DateRange? AsDateRangeFields(this SqlDataReader reader,
                                           string fldStart,
                                           string fldEnd,
                                           DateTime? dfltStart = null,
                                           DateTime? dfltEnd = null,
                                           ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault,
                                           System.Globalization.DateTimeStyles? styles = null)
    {
      return new DateRange(reader.AsDateTimeField(fldStart, dfltStart, handling, styles),
                           reader.AsDateTimeField(fldEnd, dfltEnd, handling, styles));
    }

    public static int? AsIntField(this SqlDataReader reader, string fld, int? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
    {
      var val = reader[fld];
      if (val is DBNull) return null;
      return val.AsNullableInt(dflt, handling);
    }

    public static long? AsLongField(this SqlDataReader reader, string fld, long? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
    {
      var val = reader[fld];
      if (val is DBNull) return null;
      return val.AsNullableLong(dflt, handling);
    }

    public static float? AsFloatField(this SqlDataReader reader, string fld, float? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
    {
      var val = reader[fld];
      if (val is DBNull) return null;
      return val.AsNullableFloat(dflt, handling);
    }

    public static double? AsDoubleField(this SqlDataReader reader, string fld, double? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
    {
      var val = reader[fld];
      if (val is DBNull) return null;
      return val.AsNullableDouble(dflt, handling);
    }

    public static decimal? AsDecimalField(this SqlDataReader reader, string fld, decimal? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.ReturnDefault)
    {
      var val = reader[fld];
      if (val is DBNull) return null;
      return val.AsNullableDecimal(dflt, handling);
    }

    public static SqlCommand AddParam(this SqlCommand cmd, string paramKey, object paramValue, bool isRequired = false)
    {
      paramKey.NonBlank(paramKey);
      if (isRequired) paramValue.NonNull(paramKey);

      cmd.NonNull(nameof(cmd)).Parameters.AddWithValue(paramKey, paramValue);

      return cmd;
    }

  }
}
