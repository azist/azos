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
  public class DistanceTests2
  {
    [Run("v='17.5in'  val='17.5'  micron='' mm='' cm='' m='' km=''  in='' ft='' yd='' mi='' nmi=''")]
    public void TestUnitConversions(string v, decimal val, decimal micron, decimal mm)
    {
      Distance d = v;
      Aver.AreEqual(val, d.Value);
      Aver.AreEqual(micron, d.ValueIn(Distance.UnitType.Micron));
      Aver.AreEqual(mm, d.ValueIn(Distance.UnitType.Millimeter));
      //...
    }

    [Run("a='2m' b='-1m' c='100cm'")]
    public void Add(string a, string b, string c) => Aver.IsTrue(((Distance)c).IsEquivalent((Distance)a + (Distance)b));

    [Run("a='2m' b='-1m' c='300cm'")]
    [Run("a='5.5ft' b='1in' c='65in'")]
    [Run("a='5.5ft' b='-1in' c='67in'")]
    public void Subtract(string a, string b, string c) => Aver.IsTrue(((Distance)c).IsEquivalent((Distance)a - (Distance)b));

    // ......


    [Run("a='2ft' b='2ft' c='609600'")]
    public void TestArea(string a, string b, string c)
    {
      Aver.AreEqual(c.AsLong(), ((Distance)a * ((Distance)b)).ValueInMicrons);
    }

    //IsWithin()..

    //toString
    //toString(format)

    //Create a data document and full cycle using:
    // JSON, BXON, Config


  }
}
