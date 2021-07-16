/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Scripting;
using Azos.Serialization.JSON;
using Azos.Standards;

namespace Azos.Tests.Nub.Standards
{
  [Runnable]
  class DistanceTests
  {
    [Run]
    public void Convert()
    {
      Distance d1 = new Distance(3.12m, Distance.UnitType.Meter);
      Aver.AreEqual(d1.Convert(Distance.UnitType.Centimeter).Value, 312);
    }

    [Run]
    public void Parse()
    {
      Aver.IsTrue(Distance.Parse(" ") == null);
      Aver.IsTrue(Distance.Parse(null) == null);
      Aver.IsTrue(Distance.Parse("15.8 Cm").Value.Unit == Distance.UnitType.Centimeter);
      Aver.AreEqual(Distance.Parse("15.8 Cm").Value.Value, 15.8m);
      Aver.IsTrue(Distance.Parse("  15.8     Cm   ").Value.Unit == Distance.UnitType.Centimeter);
      Aver.AreEqual(Distance.Parse("  15.8     Cm   ").Value.Value, 15.8m);
      Aver.IsTrue(Distance.Parse("15.8 MM").Value.Unit == Distance.UnitType.Millimeter);
      Aver.AreEqual(Distance.Parse("15.8 mM").Value.Value, 15.8m);
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
    public void JSON()
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
      Aver.IsTrue(d1 == new Distance(35000, Distance.UnitType.Millimeter));
      Aver.IsTrue(d1 != d2);
      Aver.IsTrue(d1 >= d2);
      Aver.IsTrue(d1 > d2);
    }

  }
}
