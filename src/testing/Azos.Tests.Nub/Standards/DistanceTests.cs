/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Conf;
using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;
using Azos.Standards;

namespace Azos.Tests.Nub.Standards
{
  [Runnable]
  public class DistanceTests
  {

    [Run]
    public void Test1()
    {
      Aver.AreEqual("µm", Distance.GetUnitName(Distance.UnitType.Micron, true));
      Aver.AreEqual("micron", Distance.GetUnitName(Distance.UnitType.Micron, false));
    }

    [Run]
    public void Test2()
    {
      Aver.AreEqual(1m, Distance.MicronToUnit(1_000_000, Distance.UnitType.Meter));
      Aver.AreEqual(100m, Distance.MicronToUnit(1_000_000, Distance.UnitType.Centimeter));
      Aver.AreEqual(1000m, Distance.MicronToUnit(1_000_000, Distance.UnitType.Millimeter));
      Aver.AreEqual(1000000m, Distance.MicronToUnit(1_000_000, Distance.UnitType.Micron));
      Aver.AreEqual(39.370078740157480314960629921m, Distance.MicronToUnit(1_000_000, Distance.UnitType.Inch));
      Aver.AreEqual(1.0936132983377077865266841645m, Distance.MicronToUnit(1_000_000, Distance.UnitType.Yard));
      Aver.AreEqual(3.2808398950131233595800524934m, Distance.MicronToUnit(1_000_000, Distance.UnitType.Foot));
    }

    [Run]
    public void Test3()
    {
      Aver.AreEqual(1_000_000L, Distance.UnitToMicron(1m,       Distance.UnitType.Meter));
      Aver.AreEqual(1_000_000L, Distance.UnitToMicron(100m,     Distance.UnitType.Centimeter));
      Aver.AreEqual(1_000_000L, Distance.UnitToMicron(1000m,    Distance.UnitType.Millimeter));
      Aver.AreEqual(1_000_000L, Distance.UnitToMicron(1000000m, Distance.UnitType.Micron));
      Aver.AreEqual(1_000_000L, Distance.UnitToMicron(39.3701m,  Distance.UnitType.Inch));
      Aver.AreEqual(1_000_000L, Distance.UnitToMicron(1.0936133m,   Distance.UnitType.Yard));
      Aver.AreEqual(1_000_000L, Distance.UnitToMicron(3.2808399m,   Distance.UnitType.Foot));
    }

    [Run]
    public void Test4()
    {
      Aver.AreEqual(1_500_000m, (3 * new Distance(500_000, Distance.UnitType.µm)).Value);
      Aver.AreEqual(1, (new Distance(500_000, Distance.UnitType.µm) * 2).ValueIn(Distance.UnitType.Meter));
    }

    [Run]
    public void Test5()
    {
      Aver.AreEqual(0.25m, (new Distance(500_000, Distance.UnitType.µm) / 2).ValueIn(Distance.UnitType.Meter));
      Aver.AreEqual(250m, (new Distance(500_000, Distance.UnitType.µm) / 2).ValueIn(Distance.UnitType.mm));
    }

    [Run]
    public void Test6()
    {
      Aver.IsTrue( new Distance(10, Distance.UnitType.Meter) <= new Distance(11, Distance.UnitType.Meter));
      Aver.IsTrue(new Distance(10, Distance.UnitType.Meter) <= new Distance(10, Distance.UnitType.Meter));
      Aver.IsTrue( new Distance(10, Distance.UnitType.Meter) < new Distance(11, Distance.UnitType.Meter));
      Aver.IsTrue(new Distance(11, Distance.UnitType.Meter) > new Distance(10, Distance.UnitType.Meter));
      Aver.IsTrue(new Distance(10, Distance.UnitType.Meter) >= new Distance(10, Distance.UnitType.Meter));
      Aver.IsTrue(new Distance(11, Distance.UnitType.Meter) >= new Distance(10, Distance.UnitType.Meter));

      Aver.IsTrue(new Distance(10, Distance.UnitType.Meter) <= new Distance(1100, Distance.UnitType.cm));
      Aver.IsTrue(new Distance(10, Distance.UnitType.Meter) <= new Distance(1000, Distance.UnitType.cm));
      Aver.IsTrue(new Distance(10, Distance.UnitType.Meter) < new Distance(1100,  Distance.UnitType.cm));
      Aver.IsTrue(new Distance(11, Distance.UnitType.Meter) > new Distance(1000,  Distance.UnitType.cm));
      Aver.IsTrue(new Distance(10, Distance.UnitType.Meter) >= new Distance(1000, Distance.UnitType.cm));
      Aver.IsTrue(new Distance(11, Distance.UnitType.Meter) >= new Distance(1000, Distance.UnitType.cm));
    }

    [Run]
    public void Test7()
    {
      Aver.AreEqual(new Distance(500_000, Distance.UnitType.µm), new Distance(Distance.UnitType.µm, 500_000));
      Aver.AreNotEqual(new Distance(500_000, Distance.UnitType.µm), new Distance(Distance.UnitType.m, 500_000));
      Aver.IsTrue(new Distance(500_000, Distance.UnitType.µm).IsEquivalent(new Distance(Distance.UnitType.m, 500_000)));
    }

    [Run]
    public void Test8_1()
    {
      Aver.IsTrue(Distance.TryParse("12cm", out var result));
      Aver.IsTrue(result.HasValue);
      Aver.AreEqual(12m, result.Value.Value);
      Aver.IsTrue(Distance.UnitType.Centimeter == result.Value.Unit);
    }

    [Run]
    public void Test8_2()
    {
      Aver.IsTrue(Distance.TryParse("12 cm", out var result));
      Aver.IsTrue(result.HasValue);
      Aver.AreEqual(12m, result.Value.Value);
      Aver.IsTrue(Distance.UnitType.Centimeter == result.Value.Unit);
    }

    [Run]
    public void Test8_3()
    {
      Aver.IsTrue(Distance.TryParse("12 centimeter", out var result));
      Aver.IsTrue(result.HasValue);
      Aver.AreEqual(12m, result.Value.Value);
      Aver.IsTrue(Distance.UnitType.Centimeter == result.Value.Unit);
    }

    [Run]
    public void Test8_4()
    {
      Aver.IsTrue(Distance.TryParse("12centimeter", out var result));
      Aver.IsTrue(result.HasValue);
      Aver.AreEqual(12m, result.Value.Value);
      Aver.IsTrue(Distance.UnitType.Centimeter == result.Value.Unit);
    }

    [Run]
    public void Test8_5()
    {
      Aver.IsTrue(Distance.TryParse("12cm", out var result));
      Aver.IsTrue(result.HasValue);
      Aver.AreEqual(12m, result.Value.Value);
      Aver.IsTrue(Distance.UnitType.Centimeter == result.Value.Unit);
    }

    [Run]
    public void Test8_6()
    {
      Aver.IsTrue(Distance.TryParse("0.5cm", out var result));
      Aver.IsTrue(result.HasValue);
      Aver.AreEqual(0.5m, result.Value.Value);
      Aver.IsTrue(Distance.UnitType.Centimeter == result.Value.Unit);
    }

    [Run]
    public void Test8_7()
    {
      Aver.IsTrue(Distance.TryParse("0.5 Centimeter", out var result));
      Aver.IsTrue(result.HasValue);
      Aver.AreEqual(0.5m, result.Value.Value);
      Aver.IsTrue(Distance.UnitType.Centimeter == result.Value.Unit);
    }

    [Run]
    public void Test8_8()
    {
      Aver.IsTrue(Distance.TryParse("5.cm", out var result));
      Aver.IsTrue(result.HasValue);
      Aver.AreEqual(5m, result.Value.Value);
      Aver.IsTrue(Distance.UnitType.Centimeter == result.Value.Unit);
    }

    [Run]
    public void Test8_9()
    {
      Aver.IsTrue(Distance.TryParse("5.0cm", out var result));
      Aver.IsTrue(result.HasValue);
      Aver.AreEqual(5m, result.Value.Value);
      Aver.IsTrue(Distance.UnitType.Centimeter == result.Value.Unit);
    }

    [Run]
    public void Test8_10()
    {
      Aver.IsTrue(Distance.TryParse("5.0   cm      ", out var result));
      Aver.IsTrue(result.HasValue);
      Aver.AreEqual(5m, result.Value.Value);
      Aver.IsTrue(Distance.UnitType.Centimeter == result.Value.Unit);
    }

    [Run]
    public void Test8_11()
    {
      Aver.IsFalse(Distance.TryParse("5 .0   cm      ", out var result));
      Aver.IsFalse(Distance.TryParse("5 .cm", out var result1));
      Aver.IsFalse(Distance.TryParse("cm 5", out var result2));
      Aver.IsFalse(Distance.TryParse("cm", out var result3));

      Aver.IsTrue(Distance.TryParse("5", out var result4));//microns
    }



    [Run]
    public void Convert()
    {
      Distance d1 = new Distance(3.12m, Distance.UnitType.Meter);
      Aver.AreEqual(d1.Convert(Distance.UnitType.Centimeter).Value, 312);
      Aver.AreEqual(d1.Convert(Distance.UnitType.Kilometer).Value, 0.00312m);
      Aver.AreEqual(d1.Convert(Distance.UnitType.Mile).Value, 0.0019386781197804819852063947m);
      Aver.AreEqual(d1.Convert(Distance.UnitType.NauticalMile).Value, 0.001684665226661056402753462m);
      Aver.AreEqual(d1.Convert(Distance.UnitType.Yard).Value, 3.4120734908136482939632545932m);
      Aver.AreEqual(d1.Convert(Distance.UnitType.Foot).Value, 10.23622047244094488188976378m);
      Aver.AreEqual(d1.Convert(Distance.UnitType.Inch).Value, 122.83464566929133858267716535m);
    }

    [Run]
    public void Parse01()
    {
      Aver.IsTrue(Distance.Parse(" ") == null);
      Aver.IsTrue(Distance.Parse(null) == null);
      Aver.IsTrue(Distance.Parse("15.8 Cm").Value.Unit == Distance.UnitType.Centimeter);
      Aver.AreEqual(Distance.Parse("15.8 Cm").Value.Value, 15.8m);
      Aver.IsTrue(Distance.Parse("  15.8     Cm   ").Value.Unit == Distance.UnitType.Centimeter);
      Aver.AreEqual(Distance.Parse("  15.8     Cm   ").Value.Value, 15.8m);
      Aver.IsTrue(Distance.Parse("15.8 MM").Value.Unit == Distance.UnitType.Millimeter);
      Aver.AreEqual(Distance.Parse("15.8 mM").Value.Value, 15.8m);


      Aver.AreEqual(Math.Round(Distance.Parse("10.5 mile").Value.Value, 4), 10.5m);
      Aver.IsTrue(Distance.Parse("10.5 mile").Value.Unit == Distance.UnitType.Mile);

      Aver.AreEqual(Math.Round(Distance.Parse("10.5 mi").Value.Value, 4), 10.5m);
      Aver.IsTrue(Distance.Parse("10.5 mi").Value.Unit == Distance.UnitType.Mile);

      Aver.AreEqual(Math.Round(Distance.Parse("10.5 nmi").Value.Value, 4), 10.5m);
      Aver.IsTrue(Distance.Parse("10.5 nmi").Value.Unit == Distance.UnitType.NauticalMile);

      Aver.AreEqual(Math.Round(Distance.Parse("10.5 nmile").Value.Value, 4), 10.5m);
      Aver.IsTrue(Distance.Parse("10.5 nmile").Value.Unit == Distance.UnitType.NauticalMile);


      Aver.AreEqual(Distance.Parse("10.5km").Value.Value, 10.5m);
      Aver.IsTrue(Distance.Parse("10.5km").Value.Unit == Distance.UnitType.Kilometer);

      Aver.AreEqual(Distance.Parse("10.5Km").Value.Value, 10.5m);
      Aver.IsTrue(Distance.Parse("10.5Km").Value.Unit == Distance.UnitType.Kilometer);

      Aver.AreEqual(Distance.Parse("10.5  km").Value.Value, 10.5m);
      Aver.IsTrue(Distance.Parse("10.5  km").Value.Unit == Distance.UnitType.Kilometer);

      Aver.AreEqual(Distance.Parse("  10.5  KM  ").Value.Value, 10.5m);
      Aver.IsTrue(Distance.Parse("  10.5  KM  ").Value.Unit == Distance.UnitType.Kilometer);

      Aver.AreEqual(Distance.Parse("10.5kilometer").Value.Value, 10.5m);
      Aver.IsTrue(Distance.Parse("10.5kilometer").Value.Unit == Distance.UnitType.Kilometer);
    }


    [Run]
    public void Parse02()
    {
      Aver.IsTrue(Distance.Parse("1").Value.Unit == Distance.UnitType.Micron);
      Aver.IsTrue(Distance.Parse("1").Value.Value == 1m);

      Aver.IsTrue(Distance.Parse("1m").Value.Unit == Distance.UnitType.Meter);
      Aver.IsTrue(Distance.Parse("1m").Value.Value == 1m);

      Aver.IsTrue(Distance.Parse("1 m").Value.Unit == Distance.UnitType.Meter);
      Aver.IsTrue(Distance.Parse("1 m").Value.Value == 1m);

      Aver.IsTrue(Distance.Parse("1meter").Value.Unit == Distance.UnitType.Meter);
      Aver.IsTrue(Distance.Parse("1meter").Value.Value == 1m);

      Aver.IsTrue(Distance.Parse("1 meter").Value.Unit == Distance.UnitType.Meter);
      Aver.IsTrue(Distance.Parse("1 meter").Value.Value == 1m);
    }

    [Run]
    [Aver.Throws()]
    public void ParseFail()
    {
      Distance.Parse("a 15.8 cm");
    }

    [Run]
    [Aver.Throws(typeof(AzosException))]
    public void ParseIncorrect()
    {
     Distance.Parse("15.8 mdm");
    }

    [Run]
    public void TryParse()
    {
      Distance? result;
      Aver.AreEqual(Distance.TryParse("15.8 Cm", out result), true);
      Aver.AreEqual(Distance.TryParse("not a 16.8 kg", out result), false);

      Distance.TryParse(" 15.8   Cm ", out result);
      Aver.AreEqual(result.Value.Value, 15.8m);
    }

    [Run]
    public void TestEquals()
    {
      Distance d1 = new Distance(33, Distance.UnitType.Meter);
      Distance d2 = new Distance(3300, Distance.UnitType.Centimeter);
      Distance d3 = new Distance(33, Distance.UnitType.Meter);
      Aver.IsFalse(d1.Equals(d2));
      Aver.IsTrue(d1.Equals(d3));
      Aver.IsTrue(d1.IsEquivalent(d2));
    }

    [Run]
    public void TestNotEquals()
    {
      Distance d1 = new Distance(3.25m, Distance.UnitType.Millimeter);
      Weight w1 = new Weight(15, Weight.UnitType.Kg);
      Aver.IsFalse(d1.Equals(w1));
    }

    [Run]
    public void TestHashCode()
    {
      Distance d1 = new Distance(3, Distance.UnitType.Micron);
      Distance d2 = new Distance(3, Distance.UnitType.Micron);
      Aver.AreEqual(d1.GetHashCode(), d2.GetHashCode());
    }

    [Run]
    public void TestToString()
    {
      Distance d1 = new Distance(3.25m, Distance.UnitType.Millimeter);
      Aver.AreEqual(d1.ToString(), "3.25 mm");
    }

    [Run]
    public void CompareTo()
    {
      Distance d1 = new Distance(35, Distance.UnitType.Meter);
      Distance d2 = new Distance(3300, Distance.UnitType.Centimeter);
      Aver.AreEqual(d1.CompareTo(d2), 1);
    }

    [Run]
    public void JSON01()
    {
      var d1 = new Distance(3.25m, Distance.UnitType.Yard);
      var json = d1.ToJson();
      json.See();
      var map = json.JsonToDataObject() as JsonDataMap;
      var d2 = new Distance();
      var got =((IJsonReadable)d2).ReadAsJson(map, false, null);
      Aver.IsTrue(got.match);
      Aver.IsTrue(d1 == (Distance)got.self);
    }

    [Run]
    public void JSON02()
    {
      var d1 = new Distance(125.7m, Distance.UnitType.NauticalMile);
      var json = d1.ToJson();
      json.See();
      var map = json.JsonToDataObject() as JsonDataMap;
      var d2 = new Distance();
      var got = ((IJsonReadable)d2).ReadAsJson(map, false, null);
      Aver.IsTrue(got.match);

      Aver.AreEqual(125.7m, Math.Round(((Distance)got.self).Value, 4));
    }

    [Run]
    public void JSON03()
    {
      var d1 = new Distance(10m, Distance.UnitType.Foot) + new Distance(3m/8m, Distance.UnitType.Inch);
      var json = d1.ToJson();
      json.See();
      var map = json.JsonToDataObject() as JsonDataMap;
      var d2 = new Distance();
      var got = ((IJsonReadable)d2).ReadAsJson(map, false, null);
      Aver.IsTrue(got.match);
      //10 foot 3/8 inch = 3057.525 mm
      Aver.AreEqual(3057.525m, Math.Round(((Distance)got.self).Convert(Distance.UnitType.Millimeter).Value, 4));
    }


    class AllDoc : TypedDoc
    {
      [Field, Config] public Distance microns { get; set; }
      [Field, Config] public Distance millimeters { get; set; }
      [Field, Config] public Distance centimeters { get; set; }
      [Field, Config] public Distance meters { get; set; }
      [Field, Config] public Distance kilometers { get; set; }
      [Field, Config] public Distance inches { get; set; }
      [Field, Config] public Distance yards { get; set; }
      [Field, Config] public Distance feet { get; set; }
      [Field, Config] public Distance miles { get; set; }
      [Field, Config] public Distance nauticalMiles { get; set; }

      [Field, Config] public Distance AnotherDistance { get; set; }

    }


    [Run]
    public void JSON_CONFIG_All()
    {
      var d1 = new AllDoc
      {
        microns = new Distance(1_000_000, Distance.UnitType.Micron),
        millimeters = new Distance(1000, Distance.UnitType.Millimeter),
        centimeters = new Distance(100, Distance.UnitType.Centimeter),
        meters = new Distance(1, Distance.UnitType.Meter),
        kilometers = new Distance(0.001m, Distance.UnitType.Kilometer),
        inches = new Distance(39.370078740157480314960629921m, Distance.UnitType.Inch),
        yards = new Distance(1.0936132983377077865266841645m, Distance.UnitType.Yard),
        feet = new Distance(3.2808398950131233595800524934m, Distance.UnitType.Foot),
        miles = new Distance(0.0019386781197804819852063947m, Distance.UnitType.Mile),
        nauticalMiles = new Distance(0.001684665226661056402753462m, Distance.UnitType.NauticalMile),
        AnotherDistance = default(Distance)
      };

      var json = d1.ToJson();
      json.See();

      var d2 = JsonReader.ToDoc<AllDoc>(json);
      d2.See();
      d1.AverNoDiff(d2);


      var cfg = Conf.Configuration.NewEmptyRoot("test");
      var node = d2.PersistConfiguration(cfg, "data");
      node.ToLaconicString().See();

      var d3 = new AllDoc();
      d3.Configure(node);
      d3.See();

      Aver.AreEqual(d1.microns, d3.microns);
      Aver.AreEqual(d1.millimeters, d3.millimeters);
      Aver.AreEqual(d1.centimeters, d3.centimeters);
      Aver.AreEqual(d1.meters, d3.meters);
      Aver.AreEqual(d1.kilometers, d3.kilometers);

      Aver.AreEqual(Math.Round(d1.inches.Value, 2), Math.Round(d3.inches.Value, 2));
      Aver.AreEqual(Math.Round(d1.feet.Value, 2), Math.Round(d3.feet.Value, 2));



    }


    [Run]
    public void Operators()
    {
      Distance d1 = new Distance(35, Distance.UnitType.Meter);
      Distance d2 = new Distance(1200, Distance.UnitType.Millimeter);
      Distance d3 = d1 + d2;
      Aver.AreEqual(d3.ToString(), "36.2 m");
      d3 = d1 - d2;
      Aver.AreEqual(d3.ToString(), "33.8 m");
      d3 = d1 * 2;
      Aver.AreEqual(d3.ToString(), "70 m");
      d3 = d1 / 2;
      Aver.AreEqual(d3.ToString(), "17.5 m");
      Aver.IsTrue(d1.IsEquivalent(new Distance(35000, Distance.UnitType.Millimeter)));
      Aver.IsTrue(d1 != d2);
      Aver.IsTrue(d1 >= d2);
      Aver.IsTrue(d1 > d2);
    }

  }
}
