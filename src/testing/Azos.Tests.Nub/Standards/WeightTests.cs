/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Scripting;
using Azos.Serialization.JSON;
using Azos.Standards;

namespace Azos.Tests.Nub.Standards
{
  [Runnable]
  public class WeightTests
  {
    [Run]
    public void Convert()
    {
      Weight d1 = new Weight(3.12m, Weight.UnitType.Kg);
      Aver.AreEqual(d1.Convert(Weight.UnitType.G).Value, 3120);
    }

    [Run]
    public void Parse()
    {
      Aver.IsTrue(Weight.Parse("15.8 Kg").Unit == Weight.UnitType.Kg);
      Aver.AreEqual(Weight.Parse("15.8 KG").Value, 15.8m);
      Aver.IsTrue(Weight.Parse("  15.8     kg   ").Unit == Weight.UnitType.Kg);
      Aver.AreEqual(Weight.Parse("  15.8     Kg   ").Value, 15.8m);
      Aver.IsTrue(Weight.Parse("15.8 G").Unit == Weight.UnitType.G);
      Aver.AreEqual(Weight.Parse("15.8 G").Value, 15.8m);
    }

    [Run]
    [Aver.Throws()]
    public void ParseFail()
    {
      Aver.AreObjectsEqual(Weight.Parse("a 15.8 kg").Value, 15.8M);
    }

    [Run]
    [Aver.Throws(typeof(AzosException))]
    public void ParseIncorrect()
    {
      Aver.AreObjectsEqual(Weight.Parse("15.8 mdm").Value, 15.8);
    }

    [Run]
    [Aver.Throws(typeof(AzosException))]
    public void ParseEmpty()
    {
      Aver.AreObjectsEqual(Weight.Parse("").Value, 15.8);
    }

    [Run]
    [Aver.Throws(typeof(AzosException))]
    public void ParseNull()
    {
      Aver.AreObjectsEqual(Weight.Parse(null).Value, 15.8);
    }

    [Run]
    public void TryParse()
    {
      Weight? result;
      Aver.AreEqual(Weight.TryParse("15.8 Oz", out result), true);
      Aver.AreEqual(Weight.TryParse("not a 16.8 kg", out result), false);

      Weight.TryParse(" 15   Lb ", out result);
      Aver.AreEqual(result.Value.Value, 15);
    }

    [Run]
    public void TestEquals()
    {
      Weight d1 = new Weight(33, Weight.UnitType.Kg);
      Weight d2 = new Weight(33000, Weight.UnitType.G);
      Aver.IsTrue(d1.Equals(d2));
    }

    [Run]
    public void TestNotEquals()
    {
      Weight d1 = new Weight(3.25m, Weight.UnitType.G);
      Weight w1 = new Weight(15, Weight.UnitType.Kg);
      Aver.IsFalse(d1.Equals(w1));
    }

    [Run]
    public void TestHashCode()
    {
      Weight d1 = new Weight(309, Weight.UnitType.G);
      Aver.AreEqual(d1.GetHashCode(), d1.Value.GetHashCode());
    }

    [Run]
    public void TestToString()
    {
      Weight d1 = new Weight(3.25m, Weight.UnitType.Lb);
      Aver.AreEqual(d1.ToString(), "3.25 lb");
    }


    [Run]
    public void CompareTo()
    {
      Weight d1 = new Weight(35, Weight.UnitType.Kg);
      Weight d2 = new Weight(3300, Weight.UnitType.G);
      Aver.AreEqual(d1.CompareTo(d2), 1);
    }

    [Run]
    public void JSON()
    {
      var data = new { dist = new Weight(3.25m, Weight.UnitType.Kg) };
      var json = data.ToJson();
      Console.WriteLine(json);
      Aver.AreEqual(@"{""dist"":{""unit"":""kg"",""value"":3.25}}", json);
    }

    [Run]
    public void Operators()
    {
      Weight d1 = new Weight(35, Weight.UnitType.Kg);
      Weight d2 = new Weight(1200, Weight.UnitType.G);
      Weight d3 = d1 + d2;
      Aver.AreEqual(d3.ToString(), "36.2 kg");
      d3 = d1 - d2;
      Aver.AreEqual(d3.ToString(), "33.8 kg");
      d3 = d1 * 2;
      Aver.AreEqual(d3.ToString(), "70 kg");
      d3 = d1 / 2;
      Aver.AreEqual(d3.ToString(), "17.5 kg");
      Aver.IsTrue(d1 == new Weight(35000, Weight.UnitType.G));
      Aver.IsTrue(d1 != d2);
      Aver.IsTrue(d1 >= d2);
      Aver.IsTrue(d1 > d2);
    }

  }
}
