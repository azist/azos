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
    [Run("v='17.5in'  val='17.5'  micron='444500' mm='444.5' cm='44.45' m='0.4445' km='0.0004445' inches='17.5' ft='1.4583333333333333333333333333' yd='0.4861111111111111111111111111' mi='0.0002761994949494949494949495' nmi='0.0002400107991188588368666391'")]
    // 1 nautical mile doesn't fit nicely into microns, so precision is lost meaning v won't equal val and nmi though it **should be** in a perfect world
    [Run("v='1nmi'  val='0.9999999999282937365062217018'  micron='1852000000' mm='1852000' cm='185200' m='1852' km='1.852' inches='72913.385826771653543307086614173' ft='6076.1154855643044619422572178478' yd='2025.3718285214348206474190726159' mi='1.1507794480235425117314881094' nmi='0.9999999999282937365062217018'")]
    public void TestUnitConversions(string v, decimal val, decimal micron, decimal mm, decimal cm, decimal m, decimal km, decimal inches, decimal ft, decimal yd, decimal mi, decimal nmi)
    {
      Distance d = v;
      Aver.AreEqual(val,    d.Value);
      Aver.AreEqual(micron, d.ValueIn(Distance.UnitType.Micron));
      Aver.AreEqual(mm,     d.ValueIn(Distance.UnitType.Millimeter));
      Aver.AreEqual(cm,     d.ValueIn(Distance.UnitType.Centimeter));
      Aver.AreEqual(m,      d.ValueIn(Distance.UnitType.Meter));
      Aver.AreEqual(km,     d.ValueIn(Distance.UnitType.Kilometer));
      Aver.AreEqual(inches, d.ValueIn(Distance.UnitType.Inch));
      Aver.AreEqual(ft,     d.ValueIn(Distance.UnitType.Foot));
      Aver.AreEqual(yd,     d.ValueIn(Distance.UnitType.Yard));
      Aver.AreEqual(mi,     d.ValueIn(Distance.UnitType.Mile));
      Aver.AreEqual(nmi,    d.ValueIn(Distance.UnitType.NauticalMile));
      //...
    }

    [Run("a='2m' b='-1m' c='100cm'")]
    public void Add(string a, string b, string c) => Aver.IsTrue(((Distance)c).IsEquivalent((Distance)a + (Distance)b));

    [Run("a='2m' b='-1m' c='300cm'")]
    [Run("a='5.5ft' b='1in' c='65in'")]
    [Run("a='5.5ft' b='-1in' c='67in'")]
    [Run("a='2.8m' b='18in' c='2.3428m'")]
    public void Subtract(string a, string b, string c) => Aver.IsTrue(((Distance)c).IsEquivalent((Distance)a - (Distance)b));

    // ......
    [Run("a='3m' b='3' c='9000000'")]
    [Run("a='13cm' b='8' c='1040000'")]
    [Run("a='18in' b='3' c='1371600'")]
    [Run("a='0.5mi' b='2' c='1609344000'")]
    public void Multiply(string a, string b, string c) => Aver.AreEqual((((Distance)a) * b.AsLong()).ValueInMicrons, c.AsLong());

    [Run("a='23cm' b='2.3' c='100000'")]
    [Run("a='46in' b='3.84' c='304270'")]
    [Run("a='5m' b='12' c='416666'")]
    [Run("a='35mm' b='6' c='5833'")]
    public void Divide(string a, string b, string c) => Aver.AreEqual((((Distance)a) / b.AsDecimal()).ValueInMicrons, c.AsLong());

    [Run("a='2m' b='3m' c='2000000'")]
    [Run("a='5.5ft' b='1in' c='0'")]
    [Run("a='5.5ft' b='-1in' c='0'")]
    [Run("a='2.8m' b='18in' c='56800'")]
    public void Modulo(string a, string b, string c) => Aver.AreEqual(((Distance)a % (Distance)b).ValueInMicrons, c.AsLong());

    [Run("a='15in' b='-10in' c='25in'")]
    [Run("a='1yd' b='1m' c='9.8cm'")]
    [Run("a='0.63km' b='0.12573482mi' c='427.6495m'")]
    // This specific test is ensuring that nautical miles are within 1 micon precision, which accounts for the fact that 1 nautical mile does not convert to microns evenly
    [Run("a='1nmi' b='0.9999999999282937365062217018nmi' c='1µm'")]
    public void IsWithin(string a, string b, string c) => Aver.IsTrue(((Distance)a).IsWithin((Distance)b, (Distance)c));

    //[Run("a='5m' b='5'")]
    //public void ToString(string a, string b) => Aver.AreEqual(b, ((Distance)a).ToString());

    //[Run("a='2ft' b='2ft' c='609600'")]
    //public void TestArea(string a, string b, string c)
    //{
    //  Aver.AreEqual(c.AsLong(), ((Distance)a * ((Distance)b)).ValueInMicrons);
    //}

    //IsWithin()..

    //toString
    //toString(format)

    //Create a data document and full cycle using:
    // JSON, BXON, Config


  }
}
