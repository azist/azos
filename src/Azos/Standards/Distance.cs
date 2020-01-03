/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.IO;
using System.Globalization;

using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Standards
{
#warning Needs revision, why is .Value needed?
  /// <summary>
  /// Represents length distance with unit type.
  /// All operations are done with precision of 1 micrometer (10^(-3) mm)
  /// </summary>
  public struct Distance : IEquatable<Distance>, IComparable<Distance>, IJsonWritable, IJsonReadable
  {
    public enum UnitType{ Unspecified = 0, Cm , In, Ft, Mm, M, Yd }

    public const decimal MM_IN_CM = 10.0m;
    public const decimal MM_IN_IN = 25.4m;
    public const decimal MM_IN_FT = 304.8m;
    public const decimal MM_IN_M = 1000.0m;
    public const decimal MM_IN_YD = 914.4m;
    public const int VALUE_PRECISION = 3;


    public readonly decimal ValueInMm;
    public readonly decimal Value;
    public readonly UnitType Unit;

    public string UnitName => Enum.GetName(typeof(UnitType), Unit).ToLower();

    public Distance(decimal value, UnitType unit)
    {
      Unit = unit;
      Value = value;
      switch (Unit)
      {
        case UnitType.Cm: ValueInMm = Math.Round(value * MM_IN_CM, VALUE_PRECISION); break;
        case UnitType.In: ValueInMm = Math.Round(value * MM_IN_IN, VALUE_PRECISION); break;
        case UnitType.Ft: ValueInMm = Math.Round(value * MM_IN_FT, VALUE_PRECISION); break;
        case UnitType.Mm: ValueInMm = Math.Round(value, VALUE_PRECISION); break;
        case UnitType.M: ValueInMm = Math.Round(value * MM_IN_M, VALUE_PRECISION); break;
        case UnitType.Yd: ValueInMm = Math.Round(value * MM_IN_YD, VALUE_PRECISION); break;
        default: throw new AzosException(StringConsts.STANDARDS_DISTANCE_UNIT_TYPE_ERROR.Args(Unit));
      }
    }

    public decimal ValueIn(UnitType toUnit)
    {
      switch (toUnit)
      {
        case UnitType.Mm: return ValueInMm;
        case UnitType.Cm: return ValueInMm / MM_IN_CM;
        case UnitType.In: return ValueInMm / MM_IN_IN;
        case UnitType.Ft: return ValueInMm / MM_IN_FT;
        case UnitType.M:  return ValueInMm / MM_IN_M;
        case UnitType.Yd: return ValueInMm / MM_IN_YD;
        default: throw new AzosException(StringConsts.STANDARDS_DISTANCE_UNIT_TYPE_ERROR.Args(toUnit));
      }
    }

    public Distance Convert(UnitType toUnit)
    {
      if (Unit == toUnit) return this;
      return new Distance(ValueIn(toUnit), toUnit);
    }

    public static Distance Parse(string val)
    {
      if (val == null || val.Length < 2)
        throw new AzosException(StringConsts.ARGUMENT_ERROR + typeof(Distance).FullName + ".Parse({0})".Args(val));

      var nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };
      string valueString;
      string unitString;
      getPair(val, out valueString, out unitString);

      if (valueString == null || unitString == null)
        throw new AzosException(StringConsts.ARGUMENT_ERROR + typeof(Distance).FullName + ".Parse({0})".Args(val));

      return new Distance(decimal.Parse(valueString, nfi), (UnitType)Enum.Parse(typeof(UnitType), unitString));
    }

    public static bool TryParse(string val, out Distance? result)
    {
      var nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };
      string valueString;
      string unitString;
      decimal value;
      UnitType unit;

      getPair(val, out valueString, out unitString);
      if (decimal.TryParse(valueString, NumberStyles.Number, nfi, out value) && Enum.TryParse<UnitType>(unitString, out unit))
      {
        result = new Distance(value, unit);
        return true;
      }
      result = null;
      return false;
    }

    public override bool Equals(Object obj)
    {
      if (obj == null || GetType() != obj.GetType())
        return false;

      Distance d = (Distance)obj;
      return (ValueInMm == d.ValueInMm);
    }

    public override int GetHashCode()
    {
      return ValueInMm.GetHashCode();
    }

    public override string ToString()
    {
      return "{0} {1}".Args(Value.ToString("#,#.###"), UnitName);
    }

    public bool Equals(Distance other)
    {
      return (ValueInMm == other.ValueInMm);
    }

    public int CompareTo(Distance other)
    {
      return ValueInMm.CompareTo(other.ValueInMm);
    }

    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options)
    {
      JsonWriter.WriteMap(wri, nestingLevel, options, new DictionaryEntry("unit", UnitName), new DictionaryEntry("value", Value));
    }

    (bool match, IJsonReadable self) IJsonReadable.ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is JsonDataMap map)
      {
        try
        {
          return (true, new Distance(map["value"].AsDecimal(handling: ConvertErrorHandling.Throw),
                                     map["unit"].AsEnum(UnitType.Unspecified, handling: ConvertErrorHandling.Throw)));
        }
        catch
        {
          //passthrough false
        }
      }

      return (false, null);
    }


    public static Distance operator +(Distance obj1, Distance obj2)
    {
      decimal v = obj1.ValueInMm + obj2.ValueInMm;
      return new Distance(v, UnitType.Mm).Convert(obj1.Unit);
    }

    public static Distance operator -(Distance obj1, Distance obj2)
    {
      decimal v = obj1.ValueInMm - obj2.ValueInMm;
      return new Distance(v, UnitType.Mm).Convert(obj1.Unit);
    }

    public static Distance operator *(Distance obj1, decimal obj2)
    {
      decimal v = obj1.ValueInMm * obj2;
      return new Distance(v, UnitType.Mm).Convert(obj1.Unit);
    }

    public static Distance operator /(Distance obj1, decimal obj2)
    {
      decimal v = obj1.ValueInMm / obj2;
      return new Distance(v, UnitType.Mm).Convert(obj1.Unit);
    }

    public static bool operator ==(Distance obj1, Distance obj2)
    {
      return obj1.ValueInMm == obj2.ValueInMm;
    }

    public static bool operator !=(Distance obj1, Distance obj2)
    {
      return obj1.ValueInMm != obj2.ValueInMm;
    }

    public static bool operator >=(Distance obj1, Distance obj2)
    {
      return obj1.ValueInMm >= obj2.ValueInMm;
    }

    public static bool operator <=(Distance obj1, Distance obj2)
    {
      return obj1.ValueInMm <= obj2.ValueInMm;
    }

    public static bool operator >(Distance obj1, Distance obj2)
    {
      return obj1.ValueInMm > obj2.ValueInMm;
    }

    public static bool operator <(Distance obj1, Distance obj2)
    {
      return obj1.ValueInMm < obj2.ValueInMm;
    }


    private static void getPair(string val, out string valueString, out string unitString)
    {
      valueString = null;
      unitString = null;
      const string VALUE_ENDS = "0123456789 ";
      string normString = val.Trim().ToUpper();

      foreach (string unitName in Enum.GetNames(typeof(UnitType)))
      {
        int idx = normString.IndexOf(unitName.ToUpper()); // we search case-insensitive
        if (idx == normString.Length - unitName.Length    // search unit name at the end of string
            && VALUE_ENDS.IndexOf(normString[idx - 1]) >= 0) // left part (value) should ends with VALUE_ENDS char
        {
          valueString = normString.Substring(0, idx).Trim();
          unitString = unitName;
          return;
        }
      }
    }

  }

}
