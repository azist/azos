/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Standards
{
  /// <summary>
  /// Size of objects in 2 dimensions: width and height
  /// </summary>
  public readonly struct Size2d : ICompositeMeasure,
                                   IEquatable<Size2d>,
                                   IComparable,
                                   IComparable<Size2d>,
                                   IJsonWritable,
                                   IJsonReadable,
                                   IRequiredCheck,
                                   IConfigurationPersistent,
                                   IFormattable
  {
    /// <summary> Creates size object from 2 distances </summary>
    public Size2d(Distance w, Distance h)
    {
      Width = w;
      Height = h;
    }

    public Size2d(JsonDataMap map)
    {
      if (map == null || map.Count == 0)
      {
        Width = default;
        Height = default;
        return;
      }

      Width = new Distance((map["w"] as JsonDataMap) ?? (map["width"] as JsonDataMap));
      Height = new Distance((map["h"] as JsonDataMap) ?? (map["height"] as JsonDataMap));
    }


    [ConfigCtor]
    public Size2d(IConfigSectionNode cfg)
    {
      if (cfg != null)
      {
        Width = new Distance(cfg.Of("width"));
        Height = new Distance(cfg.Of("height"));
        return;
      }
      Width = default;
      Height = default;
    }

    ConfigSectionNode IConfigurationPersistent.PersistConfiguration(ConfigSectionNode parentNode, string name)
    {
      var result = parentNode.NonEmpty(nameof(parentNode)).AddChildNode(name);
      ((IConfigurationPersistent)this.Width).PersistConfiguration(result, "width");
      ((IConfigurationPersistent)this.Height).PersistConfiguration(result, "height");
      return result;
    }


    /// <summary> Width (horizontal) measurement component </summary>
    public readonly Distance Width;

    /// <summary> Height (vertical) measurement component </summary>
    public readonly Distance Height;


    /// <summary> Returns [Width, Height] vector components </summary>
    public IEnumerable<MeasureComponent> Components
    {
      get
      {
        yield return new MeasureComponent("Width", Width);
        yield return new MeasureComponent("Height", Height);
      }
    }

    /// <summary>
    /// Returns an area expressed in square microns (micrometers squared) which is the smallest unit of measure in the `Distance` based system.
    /// The surface of Earth is approximately 510,100,000 square kilometers, which is 5.1 x 10^26 microns squared.
    /// Decimal holds up to 7.9 x 10^28 microns squared, which is more than enough to represent the surface of the entire Earth
    /// </summary>
    public decimal AreaInMicrons => (decimal)Width.ValueInMicrons * Height.ValueInMicrons;


    /// <summary>
    /// Returns true if this instance has a valid Width and Height assigned
    /// </summary>
    public bool IsAssigned => Width.IsAssigned && Height.IsAssigned;
    bool IRequiredCheck.CheckRequired(string targetName) => IsAssigned;


    /// <summary>
    /// Parses the string parsed into Distance or throws if parsed is bad
    /// </summary>
    public static Size2d? Parse(string val)
    {
      if (TryParse(val, out var result)) return result;
      throw new AzosException(StringConsts.ARGUMENT_ERROR + "Unparsable Size2d(`{0}`)".Args(val));
    }

    /// <summary>
    /// Tries to parse the parsed returning true and the parsed on success or false/null on failure
    /// </summary>
    public static bool TryParse(string val, out Size2d? result)
    {
      result = null;
      if (val.IsNullOrWhiteSpace()) return true;

      // 2mx3m  2m x 3m  2 m x 3 m  2m, 3m  678m,353yd
      var got = val.SplitKVP('x', ',');

      if (Distance.TryParse(got.Key, out var w) &&
          Distance.TryParse(got.Value, out var h) &&
          w.HasValue &&
          h.HasValue)
      {
        result = new Size2d(w.Value, h.Value);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Returns true if two values represent the equivalent physical size albeit in different units
    /// </summary>
    public bool IsEquivalent(Size2d other) => this.Width.ValueInMicrons == other.Width.ValueInMicrons &&
                                               this.Height.ValueInMicrons == other.Height.ValueInMicrons;


    /// <summary>
    /// Returns true if all components of two sizes are within the specified tolerance, absolute distance e.g. "-10in is within 25in of +15in", however "-10in is not within 5in of +15in"
    /// </summary>
    public bool IsWithin(Size2d other, Size2d tolerance)
      => this.Width.IsWithin(other.Width, tolerance.Width) && this.Height.IsWithin(other.Height, tolerance.Height);

    /// <summary>
    /// Returns true if both components of this sizes are within the specified fixed tolerance, absolute distance e.g. "-10in is within 25in of +15in", however "-10in is not within 5in of +15in"
    /// </summary>
    public bool IsWithin(Size2d other, Distance tolerance)
      => this.Width.IsWithin(other.Width, tolerance) && this.Height.IsWithin(other.Height, tolerance);


    /// <summary>
    /// Returns true if both values represent the same width and height distances in the same respective units
    /// </summary>
    public bool Equals(Size2d other) => this.Width.Equals(other.Width) && this.Height.Equals(other.Height);

    public override bool Equals(object obj) => obj is Size2d other ? this.Equals(other) : false;

    public override int GetHashCode() => Width.GetHashCode() ^ Height.GetHashCode();

    public override string ToString() => IsAssigned ? $"{Width} x {Height}" : string.Empty;

    /// <summary>
    /// Converts value to string using format specifier: "L:N" or "S:N" where L is long unit name, S is short unit name
    /// and N is the number of decimal places to round to. For example: "L:3" = use long unit name with 3 decimal places "15.205 kilometer x 5.01 kilometer"
    /// </summary>
    public string ToString(string format, IFormatProvider formatProvider)
      => IsAssigned ? $"{Width.ToString(format, formatProvider)} x {Height.ToString(format, formatProvider)}" : string.Empty;

    public int CompareTo(Size2d other) => AreaInMicrons.CompareTo(other.AreaInMicrons);
    public int CompareTo(object obj)
    {
      if (obj is Size2d other) return this.CompareTo(other);
      if (obj is string str && TryParse(str, out var osz) && osz.HasValue) return this.CompareTo(osz.Value);
      return 0;
    }

    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options)
    {
      if (!IsAssigned)
      {
        wri.Write("null");
        return;
      }

      JsonWriter.WriteMap(wri, nestingLevel, options, new DictionaryEntry("w", Width),
                                                      new DictionaryEntry("h", Height));
    }

    (bool match, IJsonReadable self) IJsonReadable.ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is JsonDataMap map) return (true, new Size2d(map));
      return (false, null);
    }


    public static Size2d operator +(Size2d a, Size2d b) => new Size2d(a.Width + b.Width, a.Height + b.Height);
    public static Size2d operator -(Size2d a, Size2d b) => new Size2d(a.Width - b.Width, a.Height - b.Height);

    public static decimal operator /(Size2d a, Size2d b) => a.AreaInMicrons / b.AreaInMicrons;
    public static decimal operator %(Size2d a, Size2d b) => a.AreaInMicrons % b.AreaInMicrons;


    public static Size2d operator *(Size2d a, long b) => new Size2d(a.Width * b, a.Height * b);
    public static Size2d operator *(long a, Size2d b) => new Size2d(a * b.Width, a * b.Height);
    public static Size2d operator /(Size2d a, long b) => new Size2d(a.Width / b, a.Height / b);

    public static Size2d operator *(Size2d a, double b) => new Size2d(a.Width * b, a.Height * b);
    public static Size2d operator *(double a, Size2d b) => new Size2d(a * b.Width, a * b.Height);
    public static Size2d operator /(Size2d a, double b) => new Size2d(a.Width / b, a.Height / b);

    public static Size2d operator *(Size2d a, float b) => new Size2d(a.Width * b, a.Height * b);
    public static Size2d operator *(float a, Size2d b) => new Size2d(a * b.Width, a * b.Height);
    public static Size2d operator /(Size2d a, float b) => new Size2d(a.Width / b, a.Height / b);

    public static Size2d operator *(Size2d a, decimal b) => new Size2d(a.Width * b, a.Height * b);
    public static Size2d operator *(decimal a, Size2d b) => new Size2d(a * b.Width, a * b.Height);
    public static Size2d operator /(Size2d a, decimal b) => new Size2d(a.Width / b, a.Height / b);


    public static implicit operator Size2d(string value) => Size2d.Parse(value) ?? default;
    public static implicit operator string(Size2d value) => value.ToString();

    public static bool operator ==(Size2d a, Size2d b) => a.Equals(b);
    public static bool operator !=(Size2d a, Size2d b) => !a.Equals(b);
    public static bool operator >=(Size2d a, Size2d b) => a.AreaInMicrons >= b.AreaInMicrons;
    public static bool operator <=(Size2d a, Size2d b) => a.AreaInMicrons <= b.AreaInMicrons;
    public static bool operator >(Size2d a, Size2d b) => a.AreaInMicrons > b.AreaInMicrons;
    public static bool operator <(Size2d a, Size2d b) => a.AreaInMicrons < b.AreaInMicrons;

  }
}
