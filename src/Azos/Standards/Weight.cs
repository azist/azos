/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Globalization;
using System.IO;

using Azos.Serialization.JSON;

namespace Azos.Standards
{

#warning Needs revision, why is .Value needed?
  /// <summary>
  /// Represents weight with unit type.
  /// All operations are done with precision of 1 milligram
  /// </summary>
  public struct Weight : IEquatable<Weight>, IComparable<Weight>, IJsonWritable
  {
    public enum UnitType
    {
      G = 0,
      Oz,
      Lb,
      Kg
    }

    public const decimal G_IN_OZ = 28.3495m;
    public const decimal G_IN_LB = 453.592m;
    public const decimal G_IN_KG = 1000.0m;
    public const int VALUE_PRECISION = 3;

    public readonly decimal ValueInGrams;
    public readonly decimal Value;
    public readonly UnitType Unit;

    public string UnitName => Enum.GetName(typeof(UnitType), Unit).ToLower();

    public Weight(decimal value, UnitType unit)
    {
      Unit = unit;
      //Value = value;
      switch (Unit)
      {
        case UnitType.G:
          ValueInGrams = Math.Round(value, VALUE_PRECISION);
          Value = ValueInGrams;
          break;
        case UnitType.Oz:
          ValueInGrams = Math.Round(value * G_IN_OZ, VALUE_PRECISION);
          Value = ValueInGrams / G_IN_OZ;
          break;
        case UnitType.Lb:
          ValueInGrams = Math.Round(value * G_IN_LB, VALUE_PRECISION);
          Value = ValueInGrams / G_IN_LB;
          break;
        case UnitType.Kg:
          ValueInGrams = Math.Round(value * G_IN_KG, VALUE_PRECISION);
          Value = ValueInGrams / G_IN_KG;
          break;
        default: throw new AzosException(StringConsts.STANDARDS_WEIGHT_UNIT_TYPE_ERROR.Args(Unit));
      }
    }

    public decimal ValueIn(UnitType toUnit)
    {
      switch (toUnit)
      {
        case UnitType.G:  return ValueInGrams;
        case UnitType.Oz: return ValueInGrams / G_IN_OZ;
        case UnitType.Lb: return ValueInGrams / G_IN_LB;
        case UnitType.Kg: return ValueInGrams / G_IN_KG;
        default: throw new AzosException(StringConsts.STANDARDS_WEIGHT_UNIT_TYPE_ERROR.Args(Unit));
      }
    }

    public Weight Convert(UnitType toUnit)
    {
      if (Unit == toUnit) return this;
      return new Weight(ValueIn(toUnit), toUnit);
    }

    public static Weight Parse(string val)
    {
      if (val == null || val.Length < 2)
        throw new AzosException(StringConsts.ARGUMENT_ERROR + typeof(Weight).FullName + ".Parse({0})".Args(val));

      var nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };
      string valueString;
      string unitString;
      getPair(val, out valueString, out unitString);

      if (valueString == null || unitString == null)
        throw new AzosException(StringConsts.ARGUMENT_ERROR + typeof(Weight).FullName + ".Parse({0})".Args(val));

      return new Weight(decimal.Parse(valueString, nfi), (UnitType)Enum.Parse(typeof(UnitType), unitString));
    }

    public static bool TryParse(string val, out Weight? result)
    {
      var nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };
      string valueString;
      string unitString;
      decimal value;
      UnitType unit;

      getPair(val, out valueString, out unitString);
      if (decimal.TryParse(valueString, NumberStyles.Number, nfi, out value) && Enum.TryParse<UnitType>(unitString, out unit))
      {
        result = new Weight(value, unit);
        return true;
      }
      result = null;
      return false;
    }

    public override bool Equals(Object obj)
    {
      if (obj == null || GetType() != obj.GetType())
        return false;

      var d = (Weight)obj;
      return (ValueInGrams == d.ValueInGrams);
    }

    public override int GetHashCode()
    {
      return ValueInGrams.GetHashCode();
    }

    public override string ToString()
    {
      return "{0} {1}".Args(Value.ToString("#,#.###"), UnitName);
    }

    public bool Equals(Weight other)
    {
      return (ValueInGrams == other.ValueInGrams);
    }

    public int CompareTo(Weight other)
    {
      return ValueInGrams.CompareTo(other.ValueInGrams);
    }

    public void WriteAsJson(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
    {
      JSONWriter.WriteMap(wri, nestingLevel, options, new DictionaryEntry("unit", UnitName), new  DictionaryEntry("value", Value));
    }

    public static Weight operator +(Weight obj1, Weight obj2)
    {
      decimal v = obj1.ValueInGrams + obj2.ValueInGrams;
      return new Weight(v, UnitType.G).Convert(obj1.Unit);
    }

    public static Weight operator -(Weight obj1, Weight obj2)
    {
      decimal v = obj1.ValueInGrams - obj2.ValueInGrams;
      return new Weight(v, UnitType.G).Convert(obj1.Unit);
    }

    public static Weight operator *(Weight obj1, decimal obj2)
    {
      decimal v = obj1.ValueInGrams * obj2;
      return new Weight(v, UnitType.G).Convert(obj1.Unit);
    }

    public static Weight operator /(Weight obj1, decimal obj2)
    {
      decimal v = obj1.ValueInGrams / obj2;
      return new Weight(v, UnitType.G).Convert(obj1.Unit);
    }

    public static bool operator ==(Weight obj1, Weight obj2)
    {
      return obj1.ValueInGrams == obj2.ValueInGrams;
    }

    public static bool operator !=(Weight obj1, Weight obj2)
    {
      return obj1.ValueInGrams != obj2.ValueInGrams;
    }

    public static bool operator >=(Weight obj1, Weight obj2)
    {
      return obj1.ValueInGrams >= obj2.ValueInGrams;
    }

    public static bool operator <=(Weight obj1, Weight obj2)
    {
      return obj1.ValueInGrams <= obj2.ValueInGrams;
    }

    public static bool operator >(Weight obj1, Weight obj2)
    {
      return obj1.ValueInGrams > obj2.ValueInGrams;
    }

    public static bool operator <(Weight obj1, Weight obj2)
    {
      return obj1.ValueInGrams < obj2.ValueInGrams;
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
