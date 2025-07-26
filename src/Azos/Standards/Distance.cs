/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Standards
{
  /// <summary>
  /// Marker interface for units of measure such as:  Distance, Weight, Area, Volume, Time, and Temperature.
  /// Measures used to store, transmit and process data, such as product/item measurements in eCommerce, manufacturing, logistics etc.
  /// A typical use case is Bill of Materials (BOM) used in product manufacturing, product catalog/CPQ (Configure, Price, Quote),
  /// inventory/warehouse management and shipping systems. <br/><br/>
  /// Please note: the 3d modeling and other software may use different units, such as `double` or `float` for performance.
  /// The `IMeasure` interface is primarily built for accurate data storage, transmission and processing where maximum data veracity
  /// is required. When you need to work with other systems, such as 3d modeling, you may want to use `double` or `float` types as required, however
  /// the data conversion should be applied POST LOGICAL/BUSINESS PROCESSING which is based on `decimal` type in IMeasure interface
  /// </summary>
  public interface IMeasure
  {
    /// <summary>
    /// Measured value expressed in the unit of measure.
    /// The system purposely uses decimal and not double to avoid precision issues when multiple measurements need to be processed.
    /// The behavior is akin to how money is handled in the system, where precision is critical, because we
    /// may need to tally up many measurements and we do not want to lose precision.
    /// </summary>
    decimal Value { get; }

    /// <summary> Unit name string (e.g. "meter", "C" etc.)</summary>
    string UnitName {  get; }
  }


  //Need to implement IComparable so that value can be compared to FIELD.MIn/Max
  //Field MIN/MAX equip with some property like Min="$$$Distance=5 mm" Max="$$$Distance=120 inch"
  //Serialize both V and RAW in microns
  //Implement IRequired/IsAssigned (when UOM is Unknown, then IsAssigned is false)

  /// <summary>
  /// Represents length/distance along with its measurement unit type.
  /// Used for part/product/item measurement in manufacturing, eCommerce etc.
  /// The structure stores data with 1 micron (1e-6 of a meter) resolution.
  /// It is suitable for 99.9% of use cases in manufacturing, eCommerce, logistics, shipping etc.
  /// however it is not suitable for microscopic applications such as silicon lithography, microscopy, etc. as it does not provide
  /// resolution beyond 1/1000 of a millimeter
  /// </summary>
  /// <remarks>
  /// The `Distance` structure efficiently stores value as uniform microns in an 8-byte long int field with a sign.
  /// Most distances needed in civilian construction industry are less than 50 feet, which is 15.24 meters or 15,240,000 microns.
  /// 15 million micron value can be represented with 24 bits using varbit encoding, with extra varbit prefix included the value is typically
  /// 4 bytes of wire/storage data for the aforementioned values. The mathematics is performed on LONG values using 32/64 bit direct CPU registers
  /// avoiding any heap allocations, thus making this structure efficient for data storage, transmission and calculations/processing.
  /// </remarks>
  public struct Distance : IMeasure, IEquatable<Distance>, IComparable, IComparable<Distance>, IJsonWritable, IJsonReadable, IRequiredCheck
  {
    /// <summary>
    /// Supported distance unit types:
    /// </summary>
    public enum UnitType : byte
    {
      Undefined = 0,
      Micrometer = 1,  µm = Micrometer, mcr = Micrometer, Micron = Micrometer,
      Millimeter = 2,  mm = Millimeter,
      Centimeter = 3,  cm = Centimeter,
      Meter = 4,       m   = Meter,
      Kilometer = 5,   km = Kilometer,

      //Non Metric units
      Inch = 100, @in = Inch,
      Foot  = 101, ft  = Foot,
      Yard  = 102, yd  = Yard,
      Mile  = 103, mi  = Mile,
      NauticalMile = 104, nmi = NauticalMile, nm = NauticalMile
    }

    public static string GetUnitName(UnitType unit, bool useShort = true)
     => UNIT_NAMES.TryGetValue(unit, out var names) ? useShort ? names.s : names.l : nameof(UnitType.Undefined);

    private static readonly Dictionary<UnitType, (string s, string l)> UNIT_NAMES =  new Dictionary<UnitType, (string, string)>
    {
      {UnitType.Micron,     ("µm", "micron")},
      {UnitType.Millimeter, ("mm", "millimeter")},
      {UnitType.Centimeter, ("cm", "centimeter")},
      {UnitType.Meter,      ("m",  "meter")},
      {UnitType.Kilometer,  ("km", "kilometer")},
      {UnitType.Inch,       ("in", "inch")},
      {UnitType.Foot,       ("ft", "foot")},
      {UnitType.Yard,       ("yd", "yard")},
      {UnitType.Mile,       ("mi", "mile")},
      {UnitType.NauticalMile,  ("nmi", "nmile")},

    };


    public const decimal MICRON_IN_MILLIMETER =     1_000;
    public const decimal MICRON_IN_CENTIMETER =    10_000;
    public const decimal MICRON_IN_METER      = 1_000_000;
    public const decimal MICRON_IN_KILOMETER = 1_000 * MICRON_IN_METER;

    public const decimal MICRON_IN_INCH =  25_400;
    public const decimal MICRON_IN_FOOT = 304_800;
    public const decimal MICRON_IN_YARD = 914_400;
    public const decimal MICRON_IN_MILE = 5_280 * MICRON_IN_FOOT;
    public const decimal MICRON_IN_NAUTICAL_MILE = 6_076.115486m * MICRON_IN_FOOT;

    /// <summary>
    /// Converts a value expressed in the specified distance units into normalized micron long value
    /// </summary>
    public static long UnitToMicron(decimal value, UnitType unit)
    {
      switch (unit)
      {
        case UnitType.Micron:       return (long)value;
        case UnitType.Millimeter:   return (long)(value * MICRON_IN_MILLIMETER);
        case UnitType.Centimeter:   return (long)(value * MICRON_IN_CENTIMETER);
        case UnitType.Meter:        return (long)(value * MICRON_IN_METER);
        case UnitType.Kilometer:    return (long)(value * MICRON_IN_KILOMETER);
        case UnitType.Inch:         return (long)(value * MICRON_IN_INCH);
        case UnitType.Foot:         return (long)(value * MICRON_IN_FOOT);
        case UnitType.Yard:         return (long)(value * MICRON_IN_YARD);
        case UnitType.Mile:         return (long)(value * MICRON_IN_MILE);
        case UnitType.NauticalMile: return (long)(value * MICRON_IN_NAUTICAL_MILE);
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
        case UnitType.Micron:       return value;
        case UnitType.Millimeter:   return value / MICRON_IN_MILLIMETER;
        case UnitType.Centimeter:   return value / MICRON_IN_CENTIMETER;
        case UnitType.Meter:        return value / MICRON_IN_METER;
        case UnitType.Kilometer:    return value / MICRON_IN_KILOMETER;
        case UnitType.Inch:         return value / MICRON_IN_INCH;
        case UnitType.Foot:         return value / MICRON_IN_FOOT;
        case UnitType.Yard:         return value / MICRON_IN_YARD;
        case UnitType.Mile:         return value / MICRON_IN_MILE;
        case UnitType.NauticalMile: return value / MICRON_IN_NAUTICAL_MILE;
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
    }

    /// <summary>
    /// Creates an instance in the specified units of distance
    /// </summary>
    public Distance(decimal value, UnitType unit)
    {
      Unit = unit;
      ValueInMicrons = UnitToMicron(value, unit);
    }

    /// <summary>
    /// Normalized distance value expressed in whole microns.
    /// The maximum precision supported by this type is 1 micron which is 1 millions of a meter or 1 thousands of a millimeter
    /// </summary>
    public readonly long ValueInMicrons;

    /// <summary>
    /// Calculated value expressed in fractional units of distance
    /// </summary>
    public decimal Value => MicronToUnit(ValueInMicrons, Unit);
    decimal IMeasure.Value => Value;

    /// <summary>
    /// Units of distance measurement
    /// </summary>
    public readonly UnitType Unit;

    /// <summary>
    /// Returns true if this instance has a valid unit assigned, i.e. Unit is not Undefined
    /// </summary>
    public bool IsAssigned => Unit != UnitType.Undefined;
    bool IRequiredCheck.CheckRequired(string targetName) => IsAssigned;

    /// <summary>
    /// Provides unit name a s short string
    /// </summary>
    public string ShortUnitName => GetUnitName(Unit, true);
    string IMeasure.UnitName => ShortUnitName;

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



    /// <summary>
    /// Parses the string value into Distance or throws if value is bad
    /// </summary>
    public static Distance? Parse(string val)
    {
      if (TryParse(val, out var result)) return result;
      throw new AzosException(StringConsts.ARGUMENT_ERROR + "Unparsable(`{0}`)".Args(val));
    }

    /// <summary>
    /// Tries to parse the value returning true and the parsed value on success or false/null on failure
    /// </summary>
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
          var sval = val.Substring(0, i+1).Trim();
          var sunit = i == val.Length-1 ? "" : val.Substring(i+1).Trim();
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
    /// Returns true if two distances are within the specified tolerance absolute distance, e.g. "-10in is within 25in of +15in", however "-10in is not within 5in of +15in"
    /// </summary>
    public bool IsWithin(Distance other, Distance tolerance) => Math.Abs(this.ValueInMicrons - other.ValueInMicrons) <= Math.Abs(tolerance.ValueInMicrons);

    /// <summary>
    /// Returns true if both values represent the same distance in the same units
    /// </summary>
    public bool Equals(Distance other) => this.Unit == other.Unit && this.ValueInMicrons == other.ValueInMicrons;

    public override bool Equals(Object obj) => obj is Distance other ? this.Equals(other) : false;

    public override int GetHashCode() => ValueInMicrons.GetHashCode();

    public override string ToString() => $"{Value} {ShortUnitName}";

    public int CompareTo(Distance other) => ValueInMicrons.CompareTo(other.ValueInMicrons);
    public int CompareTo(object obj) => obj is Distance other ? this.CompareTo(other) : 0;

    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options)
    {
      //todo: this may need to be sensitive per API pragma: e.g. return canonical distance vs units
      JsonWriter.WriteMap(wri, nestingLevel, options, new DictionaryEntry("u", Unit),
                                                      new DictionaryEntry("v", Value),
                                                      new DictionaryEntry("r", ValueInMicrons));//raw value in Microns
    }

    (bool match, IJsonReadable self) IJsonReadable.ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is JsonDataMap map)
      {
        try
        {
          var result = map.ContainsKey("r") //prioritize RAW value as it is the most precise and fast
                         ? new Distance(map["u"].AsEnum(UnitType.Undefined, handling: ConvertErrorHandling.Throw),
                                        map["r"].AsLong(handling: ConvertErrorHandling.Throw))
                         : new Distance(map["v"].AsDecimal(handling: ConvertErrorHandling.Throw),
                                        map["u"].AsEnum(UnitType.Undefined, handling: ConvertErrorHandling.Throw));

          return (true, result);
        }
        catch {}
      }

      return (false, null);
    }


    public static Distance operator +(Distance a, Distance b) => new Distance(a.Unit, a.ValueInMicrons + b.ValueInMicrons);
    public static Distance operator -(Distance a, Distance b) => new Distance(a.Unit, a.ValueInMicrons - b.ValueInMicrons);

    public static decimal operator /(Distance a, Distance b) => (decimal)a.ValueInMicrons / (decimal)b.ValueInMicrons;
    public static Distance operator %(Distance a, Distance b) => new Distance(b.Unit, a.ValueInMicrons % b.ValueInMicrons);

    public static Distance operator *(Distance a, long b) => new Distance(a.Unit, a.ValueInMicrons * b);
    public static Distance operator *(long a, Distance b) => new Distance(b.Unit, a * b.ValueInMicrons);
    public static Distance operator /(Distance a, long b) => new Distance(a.Unit, a.ValueInMicrons / b);

    public static Distance operator *(Distance a, double b) => new Distance(a.Unit, (long)(a.ValueInMicrons * b));
    public static Distance operator *(double a, Distance b) => new Distance(b.Unit, (long)(a * b.ValueInMicrons));
    public static Distance operator /(Distance a, double b) => new Distance(a.Unit, (long)(a.ValueInMicrons / b));

    public static Distance operator *(Distance a, float b) => new Distance(a.Unit, (long)(a.ValueInMicrons * b));
    public static Distance operator *(float a, Distance b) => new Distance(b.Unit, (long)(a * b.ValueInMicrons));
    public static Distance operator /(Distance a, float b) => new Distance(a.Unit, (long)(a.ValueInMicrons / b));

    public static Distance operator *(Distance a, decimal b) => new Distance(a.Unit, (long)(a.ValueInMicrons * b));
    public static Distance operator *(decimal a, Distance b) => new Distance(b.Unit, (long)(a * b.ValueInMicrons));
    public static Distance operator /(Distance a, decimal b) => new Distance(a.Unit, (long)(a.ValueInMicrons / b));


    public static bool operator ==(Distance a, Distance b) => a.Equals(b);
    public static bool operator !=(Distance a, Distance b) => !a.Equals(b);
    public static bool operator >=(Distance a, Distance b) => a.ValueInMicrons >= b.ValueInMicrons;
    public static bool operator <=(Distance a, Distance b) => a.ValueInMicrons <= b.ValueInMicrons;
    public static bool operator >(Distance a, Distance b) => a.ValueInMicrons > b.ValueInMicrons;
    public static bool operator <(Distance a, Distance b) => a.ValueInMicrons < b.ValueInMicrons;
  }
}
