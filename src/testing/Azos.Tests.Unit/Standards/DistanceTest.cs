/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
using System;

using Azos.Scripting;
using Azos.Serialization.JSON;
using Azos.Standards;

namespace Azos.Tests.Unit.Standards
{
  [Runnable(TRUN.BASE)]
  class DistanceTest
  {
    [Run]
    public void Convert()
    {
      Distance d1 = new Distance(3.12m, Distance.UnitType.M);
      Aver.AreEqual(d1.Convert(Distance.UnitType.Cm).Value, 312);
    }

    [Run]
    public void Parse()
    {
      Aver.IsTrue(Distance.Parse("15.8 Cm").Unit == Distance.UnitType.Cm);
      Aver.AreEqual(Distance.Parse("15.8 Cm").Value, 15.8m);
      Aver.IsTrue(Distance.Parse("  15.8     Cm   ").Unit == Distance.UnitType.Cm);
      Aver.AreEqual(Distance.Parse("  15.8     Cm   ").Value, 15.8m);
      Aver.IsTrue(Distance.Parse("15.8 MM").Unit == Distance.UnitType.Mm);
      Aver.AreEqual(Distance.Parse("15.8 mM").Value, 15.8m);
    }

    [Run]
    [Aver.Throws()]
    public void ParseFail()
    {
      Aver.AreEqual(Distance.Parse("a 15.8 cm").Value, 15.8m);
    }

    [Run]
    [Aver.Throws(typeof(AzosException))]
    public void ParseIncorrect()
    {
      Aver.AreEqual(Distance.Parse("15.8 mdm").Value, 15.8m);
    }

    [Run]
    [Aver.Throws(typeof(AzosException))]
    public void ParseEmpty()
    {
      Aver.AreEqual(Distance.Parse("").Value, 15.8m);
    }

    [Run]
    [Aver.Throws(typeof(AzosException))]
    public void ParseNull()
    {
      Aver.AreEqual(Distance.Parse(null).Value, 15.8m);
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
      Distance d1 = new Distance(33, Distance.UnitType.M);
      Distance d2 = new Distance(3300, Distance.UnitType.Cm);
      Aver.IsTrue(d1.Equals(d2));
    }

    [Run]
    public void TestNotEquals()
    {
      Distance d1 = new Distance(3.25m, Distance.UnitType.Mm);
      Weight w1 = new Weight(15, Weight.UnitType.Kg);
      Aver.IsFalse(d1.Equals(w1));
    }

    [Run]
    public void TestHashCode()
    {
      Distance d1 = new Distance(3, Distance.UnitType.Mm);
      Aver.AreEqual(d1.GetHashCode(), d1.Value.GetHashCode());
    }

    [Run]
    public void TestToString()
    {
      Distance d1 = new Distance(3.25m, Distance.UnitType.Mm);
      Aver.AreEqual(d1.ToString(), "3.25 mm");
    }


    [Run]
    public void CompareTo()
    {
      Distance d1 = new Distance(35, Distance.UnitType.M);
      Distance d2 = new Distance(3300, Distance.UnitType.Cm);
      Aver.AreEqual(d1.CompareTo(d2), 1);
    }

    [Run]
    public void JSON()
    {
      var data = new { dist = new Distance(3.25m, Distance.UnitType.Mm) };
      var json = data.ToJSON();
      Console.WriteLine(json);
      Aver.AreEqual(@"{""dist"":{""unit"":""mm"",""value"":3.25}}", json);
    }

    [Run]
    public void Operators()
    {
      Distance d1 = new Distance(35, Distance.UnitType.M);
      Distance d2 = new Distance(1200, Distance.UnitType.Mm);
      Distance d3 = d1 + d2;
      Aver.AreEqual(d3.ToString(), "36.2 m");
      d3 = d1 - d2;
      Aver.AreEqual(d3.ToString(), "33.8 m");
      d3 = d1 * 2;
      Aver.AreEqual(d3.ToString(), "70 m");
      d3 = d1 / 2;
      Aver.AreEqual(d3.ToString(), "17.5 m");
      Aver.IsTrue(d1 == new Distance(35000, Distance.UnitType.Mm));
      Aver.IsTrue(d1 != d2);
      Aver.IsTrue(d1 >= d2);
      Aver.IsTrue(d1 > d2);
    }

  }
}
