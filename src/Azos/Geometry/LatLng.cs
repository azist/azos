/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.IO;
using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Geometry
{
  /// <summary>
  /// Represents a named position on the Earth map
  /// </summary>
  public struct LatLng : IEquatable<LatLng>, Collections.INamed, IJsonReadable, IJsonWritable, IConfigurationPersistent
  {
    public const double MIN_LAT = -90.0d;
    public const double MAX_LAT = +90.0d;

    public const double MIN_LNG = -180.0d;
    public const double MAX_LNG = +180.0d;

    public const double DEG_TO_RAD =  Math.PI / 180d;

    public const double EARTH_RADIUS_KM = 6371d;

    public const double EARTH_DIAMETER_KM = 2 * EARTH_RADIUS_KM;

    public const double EARTH_CIRCUMFERENCE_KM = EARTH_DIAMETER_KM * Math.PI;


    private string m_Name;
    private double m_Lat;
    private double m_Lng;

    [ConfigCtor]
    public LatLng(IConfigSectionNode cfg)
    {
      m_Name = cfg.NonNull(nameof(cfg)).ValOf("name");
      m_Lat = 0d;
      m_Lng = 0d;
      Lat = parseDeg(cfg.Of("lat").Value);
      Lng = parseDeg(cfg.Of("lng").Value);
    }

    public ConfigSectionNode PersistConfiguration(ConfigSectionNode parentNode, string name)
    {
      var result = parentNode.NonEmpty(nameof(parentNode)).AddChildNode(name);
      result.AddAttributeNode("name", m_Name);
      result.AddAttributeNode("lat", ComponentToString(Lat));
      result.AddAttributeNode("lng", ComponentToString(Lng));
      return result;
    }

    public LatLng(double lat, double lng, string name = null)
    {
      m_Name = name;
      m_Lat = 0d;
      m_Lng = 0d;
      Lat = lat;
      Lng = lng;
    }

    public LatLng(string val, string name = null)
    {
      if (val.IsNullOrWhiteSpace()) throw new AzosException(StringConsts.ARGUMENT_ERROR+"LatLng.ctor(val==null|empty)");
      var segs = val.Split(',');
      if (segs.Length<2) throw new AzosException(StringConsts.ARGUMENT_ERROR+"LatLng.ctor('lat,lng') expected");

      m_Name = name;
      m_Lat = 0d;
      m_Lng = 0d;

      try
      {
        Lat = parseDeg(segs[0]);
        Lng = parseDeg(segs[1]);
      }
      catch(Exception error)
      {
        throw new AzosException(StringConsts.ARGUMENT_ERROR+"LatLng.ctor('{0}'): {1}".Args(val, error.ToMessageWithType()));
      }
    }


    /// <summary>
    /// Name of this location
    /// </summary>
    public string Name => m_Name.Default(ToString());

    /// <summary>
    /// Latitude in degrees
    /// </summary>
    public double Lat
    {
      get{ return m_Lat;}
      private set
      {
        if (value < MIN_LAT) value = MIN_LAT;
        else
        if (value > MAX_LAT) value = MAX_LAT;
        m_Lat = value;
      }
    }

    /// <summary>
    /// Longitude in degrees
    /// </summary>
    public double Lng
    {
      get{ return m_Lng;}
      private set
      {
        if (value < MIN_LNG) value = MIN_LNG;
        else
        if (value > MAX_LNG) value = MAX_LNG;
        m_Lng = value;
      }
    }

    /// <summary>
    /// Latitude in radians
    /// </summary>
    public double RadLat => m_Lat * DEG_TO_RAD;

    /// <summary>
    /// Longitude in radians
    /// </summary>
    public double RadLng => m_Lng * DEG_TO_RAD;


    /// <summary>
    /// Performs great circle "as the crow flies" distance computation between this and another points.
    /// The distance is returned in Kilometer SI units. The computation is only valid for the Earth
    /// </summary>
    public double HaversineEarthDistanceKm(LatLng other)
    {
      var dLat = this.RadLat - other.RadLat;
      var dLng = this.RadLng - other.RadLng;

      var a  = Math.Pow(Math.Sin(dLat/2d), 2d) + Math.Cos(this.RadLat) * Math.Cos(other.RadLat) * Math.Pow(Math.Sin(dLng/2d), 2d);
      var c  = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a)); // great circle distance in radians

      return  EARTH_RADIUS_KM * c;
    }

    /// <summary>
    /// Converts a coordinate component (lat or lng) into standard degree/minute/second string
    /// </summary>
    public static string ComponentToString(double degVal)
    {
      var d = (int)degVal;
      degVal = Math.Abs(degVal-d) * 60d;
      var m = (int)degVal;
      degVal = Math.Abs(degVal-m) * 60d;
      var s = (int)degVal;
      return "{0}°{1}'{2}''".Args(d, m, s);
    }

    public override string ToString() => "{0}, {1}".Args(ComponentToString(Lat), ComponentToString(Lng));

    public override int GetHashCode() => m_Lat.GetHashCode() ^ m_Lng.GetHashCode();

    /// <summary>
    /// Performs exact double Lat/Lng pair comparison, while Equals(other) performs rounded-to-second precision comparison
    /// </summary>
    public bool ExactlyEquals(LatLng other) => this.m_Lat == other.m_Lat &&
                                                this.m_Lng == other.m_Lng &&
                                                this.m_Name == other.m_Name;

    //The equality test is performed in the confines of Degree/Min/Sec precion specifier which is
    //about 1 arcsecond (32 meters at equator) precision
    public bool Equals(LatLng other) => this.m_Name == other.m_Name &&
                                         ComponentToString(this.Lat) == ComponentToString(other.Lat) &&
                                         ComponentToString(this.Lng) == ComponentToString(other.Lng);

    public override bool Equals(object obj) => obj is LatLng ll ? this.Equals(ll) : false;

    public static bool operator ==(LatLng a, LatLng b) =>  a.Equals(b);
    public static bool operator !=(LatLng a, LatLng b) => !a.Equals(b);


    private double parseDeg(string val)
    {
      if (val.IsNullOrWhiteSpace()) return 0d;

      if (val.Contains('°'))
      {
        var ideg = val.IndexOf('°');
        var deg = val.Substring(0, ideg);
        val = val.Substring(ideg+1);
        var imin = val.IndexOf("'");
        var min = "";
        if (imin>0)
        {
          min = val.Substring(0, imin);
          val = val.Substring(imin+1);
        }
        var isec = val.IndexOf("''");
        var sec = "";
        if (imin>0)
        {
          sec = val.Substring(0, isec);
        }

        var dd = deg.AsDouble(handling: ConvertErrorHandling.Throw);

        return dd < 0 ?
                 dd -
                 (min.AsDouble(handling: ConvertErrorHandling.Throw)/60d) -
                 (sec.AsDouble(handling: ConvertErrorHandling.Throw)/3600d)
                 :
                 dd +
                 (min.AsDouble(handling: ConvertErrorHandling.Throw)/60d) +
                 (sec.AsDouble(handling: ConvertErrorHandling.Throw)/3600d);
      }
      return double.Parse(val, System.Globalization.NumberStyles.Number);
    }

    (bool match, IJsonReadable self) IJsonReadable.ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is JsonDataMap map)
      {
        var name = map["name"].AsString();
        var location = map["location"].AsString();

        try
        {
          return (true, new LatLng(location, name));
        }
        catch
        {
          //returns (false, null) below
        }
      }

      return (false, null);
    }

    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options)
    {
      if (options.Purpose == JsonSerializationPurpose.Marshalling)
      {
        JsonWriter.WriteMap(wri, nestingLevel, options, new DictionaryEntry("name", m_Name),
                                                        new DictionaryEntry("location", "{0:R}, {1:R}".Args(Lat, Lng)));
      }
      else
      {
        JsonWriter.WriteMap(wri, nestingLevel, options, new DictionaryEntry("name", m_Name),
                                                        new DictionaryEntry("location", "{0}, {1}".Args(ComponentToString(Lat), ComponentToString(Lng))));
      }
    }


  }

}
