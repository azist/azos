/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Serialization.BSON
{
  public static class BSONExtensions
  {

    /// <summary>
    /// Maps JsonDataMap to corresponding Bson document
    /// </summary>
    public static BSONDocument ToBson(this JsonDataMap map)
    {
      if (map==null) return null;

      var result = new BSONDocument();

      foreach (var kvp in map)
        result.Add(kvp.Key, kvp.Value, skipNull: false, required: false);

      return result;
    }

    /// <summary>
    /// Adds BsonElement of relevant subtype for passed CLR primitive value.
    /// Non-supported types are encoded as BsonStringElement(JSON)
    /// </summary>
    public static BSONDocument Add(this BSONDocument document, string name, object value, bool skipNull = false, bool required = false)
    {
      if (value == null) return onNullOrEmpty(document, name, skipNull, required);

      switch (Type.GetTypeCode(value.GetType()))
      {
        case TypeCode.Empty:
        case TypeCode.DBNull:   return document.Set(new BSONNullElement(name));
        case TypeCode.Boolean:  return document.Set(new BSONBooleanElement(name, (bool)value));
        case TypeCode.Char:     return document.Set(new BSONStringElement(name, value.ToString()));
        case TypeCode.SByte:    return document.Set(new BSONInt32Element(name, (sbyte)value));
        case TypeCode.Byte:     return document.Set(new BSONInt32Element(name, (byte)value));
        case TypeCode.Int16:    return document.Set(new BSONInt32Element(name, (short)value));
        case TypeCode.UInt16:   return document.Set(new BSONInt32Element(name, (ushort)value));
        case TypeCode.Int32:    return document.Set(new BSONInt32Element(name, (int)value));
        case TypeCode.UInt32:   return document.Set(new BSONInt32Element(name, (int)(uint)value));
        case TypeCode.Int64:    return document.Set(new BSONInt64Element(name, (long)value));
        case TypeCode.UInt64:   return document.Set(new BSONInt64Element(name, (long)(ulong)value));
        case TypeCode.Single:   return document.Set(new BSONDoubleElement(name, (float)value));
        case TypeCode.Double:   return document.Set(new BSONDoubleElement(name, (double)value));
        case TypeCode.Decimal:  return document.Set(DataDocConverter.Decimal_CLRtoBSON(name, (decimal)value));
        case TypeCode.DateTime: return document.Set(new BSONDateTimeElement(name, (DateTime)value));
        case TypeCode.String:   return document.Set(new BSONStringElement(name, (string)value));
        case TypeCode.Object:
        {
          if (value is Guid guid)
          {
            if (guid == Guid.Empty) return onNullOrEmpty(document, name, skipNull, required);
            return document.Set(new BSONBinaryElement(name, new BSONBinary(BSONBinaryType.UUID, ((Guid)value).ToNetworkByteOrder())));
          }
          else if (value is GDID gdid)
          {
            if (gdid.IsZero) return onNullOrEmpty(document, name, skipNull, required);
            return document.Set(DataDocConverter.GDID_CLRtoBSON(name, gdid));
          }
          else if (value is Atom atom) return document.Set(new BSONInt64Element(name, (long)atom.ID));
          else if (value is TimeSpan)     return document.Set(new BSONInt64Element(name, ((TimeSpan)value).Ticks));
          else if (value is BSONDocument) return document.Set(new BSONDocumentElement(name, (BSONDocument)value));
          else if (value is JsonDataMap jdm) return document.Set(new BSONDocumentElement(name, jdm.ToBson()));
          else if (value is byte[])       return document.Set(new BSONBinaryElement(name, new BSONBinary(BSONBinaryType.GenericBinary, (byte[])value)));
          throw new BSONException("BSONDocument.Add(not supported object type '{0}')".Args(value.GetType().Name));
        }
        default: return document.Set(new BSONStringElement(name, value.ToJson(JsonWritingOptions.CompactRowsAsMap)));
      }
    }

    private static BSONDocument onNullOrEmpty(BSONDocument document, string name, bool skipNull, bool required)
    {
      if (required) throw new BSONException("BSONDocument.Add(required=true&&value=null)");
      if (!skipNull) return document.Set(new BSONNullElement(name));
      return document;
    }
  }
}
