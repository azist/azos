/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

using Azos.Data;
using Azos.Serialization.JSON;
using System.Linq;

namespace Azos.Standards
{
  /// <summary>
  /// Represents length distance with unit type.
  /// Used for part/product/item measurement in manufacturing, eCommerce etc.
  /// The structure stores data with 1 micron (1e-6 of a meter) resolution
  /// </summary>
  public struct Distance : IEquatable<Distance>, IComparable<Distance>, IJsonWritable, IJsonReadable
  {
    /// <summary>
    /// Supported distance unit types:
    /// </summary>
    public enum UnitType
    {
      Undefined = 0,
      Micrometer = 1,  µm = Micrometer, mcr = Micrometer, Micron = Micrometer,
      Millimeter = 2,  mm = Millimeter,
      Centimeter = 3,  cm = Centimeter,
      Meter = 4,       m   = Meter,

      Inch  = 5, @in = Inch,
      Foot  = 6, ft  = Foot,
      Yard  = 7, yd  = Yard
    }

    public static string GetUnitName(UnitType unit, bool useShort = true)
     => UNIT_NAMES.TryGetValue(unit, out var names) ? useShort ? names.s : names.l : nameof(UnitType.Undefined);

    private static readonly Dictionary<UnitType, (string s, string l)> UNIT_NAMES =  new Dictionary<UnitType, (string, string)>
    {
      {UnitType.Micron,     ("µm", "micron")},
      {UnitType.Millimeter, ("mm", "millimeter")},
      {UnitType.Centimeter, ("cm", "centimeter")},
      {UnitType.Meter,      ("m",  "meter")},
      {UnitType.Inch,       ("in", "inch")},
      {UnitType.Foot,       ("ft", "foot")},
      {UnitType.Yard,       ("yd", "yard")},
    };



    public const int VALUE_PRECISION = 3;

    public const decimal MICRON_IN_MILLIMETER =     1_000;
    public const decimal MICRON_IN_CENTIMETER =    10_000;
    public const decimal MICRON_IN_METER      = 1_000_000;

    public const decimal MICRON_IN_INCH =  25_400;
    public const decimal MICRON_IN_FOOT = 304_800;
    public const decimal MICRON_IN_YARD = 914_400;

    /// <summary>
    /// Converts a value expressed in the specified distance units into normalized micron long value
    /// </summary>
    public static long UnitToMicron(decimal value, UnitType unit)
    {
      switch (unit)
      {
        case UnitType.Micron:     return (long)value;
        case UnitType.Millimeter: return (long)(value * MICRON_IN_MILLIMETER);
        case UnitType.Centimeter: return (long)(value * MICRON_IN_CENTIMETER);
        case UnitType.Meter:      return (long)(value * MICRON_IN_METER);
        case UnitType.Inch:       return (long)(value * MICRON_IN_INCH);
        case UnitType.Foot:       return (long)(value * MICRON_IN_FOOT);
        case UnitType.Yard:       return (long)(value * MICRON_IN_YARD);
        default: throw new AzosException(StringConsts.STANDARDS_DISTANCE_UNIT_TYPE_ERROR.Args(unit));
      }
    }

    /// <summary>
    /// Converts a normalized value expressed in long microns into decimal value expressed in the specified distance units
    /// </summary>
    public static decimal MicronToUnit(long value, UnitType unit)
    {
      switch (unit)
      {
        case UnitType.Micron: return value;
        case UnitType.Millimeter: return Math.Round(value / MICRON_IN_MILLIMETER, VALUE_PRECISION);
        case UnitType.Centimeter: return Math.Round(value / MICRON_IN_CENTIMETER, VALUE_PRECISION);
        case UnitType.Meter:      return Math.Round(value / MICRON_IN_METER, VALUE_PRECISION);
        case UnitType.Inch:       return Math.Round(value / MICRON_IN_INCH, VALUE_PRECISION);
        case UnitType.Foot:       return Math.Round(value / MICRON_IN_FOOT, VALUE_PRECISION);
        case UnitType.Yard:       return Math.Round(value / MICRON_IN_YARD, VALUE_PRECISION);
        default: throw new AzosException(StringConsts.STANDARDS_DISTANCE_UNIT_TYPE_ERROR.Args(unit));
      }
    }



    /// <summary>
    /// Creates an instance from the serialized value expressed in microns (e.g. stored in db as long)
    /// </summary>
    public Distance(UnitType unit, long micronValue)
    {
      Unit = unit;
      ValueInMicrons = micronValue;
      Value = MicronToUnit(micronValue, unit);
    }

    /// <summary>
    /// Creates an instance in the specified units of distance
    /// </summary>
    public Distance(decimal value, UnitType unit)
    {
      Unit = unit;
      Value = value;
      ValueInMicrons = UnitToMicron(value, unit);
    }

    /// <summary>
    /// Normalized distance value expressed in whole microns
    /// </summary>
    public readonly long ValueInMicrons;

    /// <summary>
    /// Calculated value expressed in factional units of distance
    /// </summary>
    public readonly decimal Value;

    /// <summary>
    /// Units of distance measurement
    /// </summary>
    public readonly UnitType Unit;

    /// <summary>
    /// Provides unit name a s short string
    /// </summary>
    public string ShortUnitName => GetUnitName(Unit, true);

    /// <summary>
    /// Provides unit name as long string
    /// </summary>
    public string LongUnitName => GetUnitName(Unit, false);

    /// <summary>
    /// Returns the value in different unit
    /// </summary>
    public decimal ValueIn(UnitType toUnit) => MicronToUnit(ValueInMicrons, toUnit);

    /// <summary>
    /// Converts this instance to different unit type
    /// </summary>
    public Distance Convert(UnitType toUnit) => Unit == toUnit ? this : new Distance(toUnit, ValueInMicrons);


    public static Distance? Parse(string val)
    {
      if (TryParse(val, out var result)) return result;
      throw new AzosException(StringConsts.ARGUMENT_ERROR + "Unparsable(`{0}`})".Args(val));
    }

    public static bool TryParse(string val, out Distance? result)
    {
      result = null;
      if (val.IsNullOrWhiteSpace()) return true;

      //  20.3m  20.3 meter
      var i = -1;
      for(i = val.Length-1; i>0; i--)
      {
        var c = val[i];
        if ((c>='0' && c<='9')||(c=='.'))
        {
          var sval = val.Substring(0, i+1);
          var sunit = i == val.Length-1 ? "" : val.Substring(i+1);
          if (sunit.IsNullOrWhiteSpace()) sunit = nameof(UnitType.Micron);

          if (!decimal.TryParse(sval, out var dval)) return false;
          if (!Enum.TryParse<UnitType>(sunit, out var unit))
          {
            unit = UNIT_NAMES.FirstOrDefault(kvp => kvp.Value.s.EqualsOrdIgnoreCase(sunit) || kvp.Value.l.EqualsOrdIgnoreCase(sunit)).Key;
          }

          if (unit == UnitType.Undefined)
          {
            return false;
          }

          result = new Distance(dval, unit);
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Returns true if two values represent the equivalent physical distance albeit in different units
    /// </summary>
    public bool IsEquivalent(Distance other) => this.ValueInMicrons == other.ValueInMicrons;

    /// <summary>
    /// Returns true if both values represent the same distance in the same units
    /// </summary>
    public bool Equals(Distance other) => this.Unit == other.Unit && this.ValueInMicrons == other.ValueInMicrons;

    public override bool Equals(Object obj) => obj is Distance other ? this.Equals(other) : false;

    public override int GetHashCode() => ValueInMicrons.GetHashCode();

    public override string ToString() => "{0} {1}".Args(Value, ShortUnitName);

    public int CompareTo(Distance other) => ValueInMicrons.CompareTo(other.ValueInMicrons);

    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options)
    {
      //todo: this may need to be sensitive per API pragma: e.g. return canonical distance vs units
      JsonWriter.WriteMap(wri, nestingLevel, options, new DictionaryEntry("u", Unit), new DictionaryEntry("v", Value));
    }

    (bool match, IJsonReadable self) IJsonReadable.ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is JsonDataMap map)
      {
        try
        {
          return (true, new Distance(map["v"].AsDecimal(handling: ConvertErrorHandling.Throw),
                                     map["u"].AsEnum(UnitType.Undefined, handling: ConvertErrorHandling.Throw)));
        }
        catch
        {
          //passthrough false
        }
      }

      return (false, null);
    }

    public static Distance operator +(Distance a, Distance b) => new Distance(a.Unit, a.ValueInMicrons + b.ValueInMicrons);
    public static Distance operator -(Distance a, Distance b) => new Distance(a.Unit, a.ValueInMicrons - b.ValueInMicrons);
    public static Distance operator *(Distance a, Distance b) => new Distance(a.Unit, a.ValueInMicrons * b.ValueInMicrons);
    public static Distance operator /(Distance a, Distance b) => new Distance(a.Unit, a.ValueInMicrons / b.ValueInMicrons);
    public static Distance operator %(Distance a, Distance b) => new Distance(a.Unit, a.ValueInMicrons % b.ValueInMicrons);

    public static Distance operator *(Distance a, double b) => new Distance(a.Unit, (long)(a.ValueInMicrons * b));
    public static Distance operator *(double a, Distance b) => new Distance(b.Unit, (long)(a * b.ValueInMicrons));
    public static Distance operator /(Distance a, double b) => new Distance(a.Unit, (long)(a.ValueInMicrons / b));

    public static bool operator ==(Distance a, Distance b) => a.Equals(b);
    public static bool operator !=(Distance a, Distance b) => !a.Equals(b);
    public static bool operator >=(Distance a, Distance b) => a.ValueInMicrons >= b.ValueInMicrons;
    public static bool operator <=(Distance a, Distance b) => a.ValueInMicrons <= b.ValueInMicrons;
    public static bool operator >(Distance a, Distance b) => a.ValueInMicrons > b.ValueInMicrons;
    public static bool operator <(Distance a, Distance b) => a.ValueInMicrons < b.ValueInMicrons;
  }
}
