/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;

using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class RGDIDTests
  {
    [Run]
    public void RGDID_1()
    {
      var rgdid = new RGDID(123456, new GDID(2, 5, 89078));
      Aver.AreEqual((uint)123456, rgdid.Route);
      Aver.AreEqual((uint)2, rgdid.Gdid.Era);
      Aver.AreEqual(5, rgdid.Gdid.Authority);
      Aver.AreEqual((ulong)89078, rgdid.Gdid.Counter);
    }

    [Run]
    [Aver.Throws(typeof(DataException), Message = "GDID can not be created from the supplied")]
    public void RGDID_2()
    {
      var rgdid = new RGDID(1, new GDID(0, 16, 89078));
    }

    [Run]
    [Aver.Throws(typeof(DataException), Message = "GDID can not be created from the supplied")]
    public void RGDID_3()
    {
      var gdid = new RGDID(1, new GDID(0, 12, GDID.COUNTER_MAX + 1));
    }

    [Run]
    public void RGDID_4()
    {
      Aver.AreEqual(new RGDID(1, new GDID(0, 12, 567)), new RGDID(1, new GDID(0, 12, 567)));
      Aver.AreEqual(new RGDID(2, new GDID(0, 12, 567)), new RGDID(2, new GDID(0, 12, 567)));

      Aver.AreEqual(new RGDID(1, new GDID(0, 12, 567)).GetHashCode(), new RGDID(1, new GDID(0, 12, 567)).GetHashCode());
      Aver.AreEqual(new RGDID(2, new GDID(0, 12, 567)).GetHashCode(), new RGDID(2, new GDID(0, 12, 567)).GetHashCode());


      Aver.AreNotEqual(new RGDID(2, new GDID(0, 12, 567)), new RGDID(1, new GDID(0, 12, 567)));
      Aver.AreNotEqual(new RGDID(1, new GDID(0, 12, 567)), new RGDID(1, new GDID(1, 12, 567)));

      Aver.AreNotEqual(new RGDID(2, new GDID(0, 12, 567)).GetHashCode(), new RGDID(1, new GDID(0, 12, 567)).GetHashCode());
      Aver.AreNotEqual(new RGDID(1, new GDID(0, 12, 567)).GetHashCode(), new RGDID(1, new GDID(1, 12, 567)).GetHashCode());
    }


    [Run]
    public void RGDID_10()
    {
      var rgdid = new RGDID(1, new GDID(0, 12, 567));
      var s = rgdid.ToString();
      s.See();
      Aver.AreEqual("1:0:12:567", s);

      rgdid = new RGDID(33, new GDID(2, 1, 890));
      s = rgdid.ToString();
      s.See();
      Aver.AreEqual("33:2:1:890", s);
    }

    [Run]
    public void RGDID_11()
    {
      var rgdid = new RGDID(0xabbafeef, new GDID(0x01020304, 0xfacaca00aa55aa55));

      Aver.IsTrue("ab,ba,fe,ef,01,02,03,04,fa,ca,ca,00,aa,55,aa,55".AsByteArray().SequenceEqual(rgdid.Bytes));
    }

    [Run]
    public void RGDID_JSON_1()
    {
      var gdid = new RGDID(900, new GDID(2, 3, 57));
      var s = gdid.ToJson();
      s.See();
      Aver.AreEqual("\"900:2:3:57\"", s);
    }

    [Run]
    public void RGDID_JSON_2()
    {
      var obj = new { id = new RGDID(800, new GDID(22, 3, 57)), Name = "Tezter" };
      var s = obj.ToJson();
      s.See();
      Aver.AreEqual("{\"id\":\"800:22:3:57\",\"Name\":\"Tezter\"}", s);
    }

    [Run]
    public void RGDID_TryParse()
    {
      RGDID parsed;
      Aver.IsTrue(RGDID.TryParse("9:1:2:3", out parsed));
      Aver.AreEqual((uint)9, parsed.Route);
      Aver.AreEqual((uint)1, parsed.Gdid.Era);
      Aver.AreEqual(2, parsed.Gdid.Authority);
      Aver.AreEqual((ulong)3, parsed.Gdid.Counter);

      Aver.IsTrue(RGDID.TryParse("33:231:2:3123", out parsed));
      Aver.AreEqual((uint)33, parsed.Route);
      Aver.AreEqual((uint)231, parsed.Gdid.Era);
      Aver.AreEqual(2, parsed.Gdid.Authority);
      Aver.AreEqual((ulong)3123, parsed.Gdid.Counter);

      Aver.IsTrue(RGDID.TryParse("   7:231:2:3123   ", out parsed));
      Aver.AreEqual((uint)7, parsed.Route);
      Aver.AreEqual((uint)231, parsed.Gdid.Era);
      Aver.AreEqual(2, parsed.Gdid.Authority);
      Aver.AreEqual((ulong)3123, parsed.Gdid.Counter);

      Aver.IsTrue(RGDID.TryParse("4 :31 : 2:  3123", out parsed));
      Aver.AreEqual((uint)4, parsed.Route);
      Aver.AreEqual((uint)31, parsed.Gdid.Era);
      Aver.AreEqual(2, parsed.Gdid.Authority);
      Aver.AreEqual((ulong)3123, parsed.Gdid.Counter);

      Aver.IsFalse(RGDID.TryParse("1:2:3", out parsed));
      Aver.IsFalse(RGDID.TryParse("0:-1:2:3123", out parsed));
      Aver.IsFalse(RGDID.TryParse("-1:1:2:3123", out parsed));
      Aver.IsFalse(RGDID.TryParse("1:1:18:3123", out parsed));
      Aver.IsFalse(RGDID.TryParse(":0:18:3123", out parsed));
      Aver.IsFalse(RGDID.TryParse(":::3123", out parsed));
      Aver.IsFalse(RGDID.TryParse("0:::3", out parsed));
      Aver.IsFalse(RGDID.TryParse("1:::", out parsed));
      Aver.IsFalse(RGDID.TryParse("1:-:-:-", out parsed));
      Aver.IsFalse(RGDID.TryParse("1: :  : ", out parsed));
      Aver.IsFalse(RGDID.TryParse("  : ", out parsed));
      Aver.IsFalse(RGDID.TryParse(" 3 ", out parsed));

      //0x 00 00 00 00 10 00 00 00 00 00 00 4B
      //   ----era---- ------ulong -----------
      Aver.IsTrue(RGDID.TryParse("0xababcaca00000000100000000000004B", out parsed));
      Aver.AreEqual(new RGDID(0xababcaca, new GDID(0, 1, 0x4b)), parsed);
      Aver.IsTrue(RGDID.TryParse("abaBCAca00000000100000000000004B", out parsed));
      Aver.AreEqual(new RGDID(0xababcaca, new GDID(0, 1, 0x4b)), parsed);
      Aver.IsTrue(RGDID.TryParse(new RGDID(4567, new GDID(0, 1, 0x4b)).ToHexString(), out parsed));
      Aver.AreEqual(new RGDID(4567, new GDID(0, 1, 0x4b)), parsed);

      RGDID? nullable;
      Aver.IsTrue(RGDID.TryParse("0Xcafebeaf00000000100000000000004b", out nullable));
      Aver.IsTrue(nullable.HasValue);
      Aver.AreEqual(new RGDID(0xcafebeaf, new GDID(0, 1, 0x4b)), nullable.Value);

      Aver.IsTrue(RGDID.TryParse("cafebeaf00000000100000000000004b", out nullable));
      Aver.IsTrue(nullable.HasValue);
      Aver.AreEqual(new RGDID(0xcafebeaf, new GDID(0, 1, 0x4b)), nullable.Value);

      Aver.IsTrue(RGDID.TryParse(new RGDID(12345, new GDID(0, 1, 0x4b)).ToHexString(), out nullable));
      Aver.IsTrue(nullable.HasValue);
      Aver.AreEqual(new RGDID(12345, new GDID(0, 1, 0x4b)), nullable.Value);

      Aver.IsFalse(RGDID.TryParse("0x00000000100000000000004b", out nullable));//too short
      Aver.IsFalse(nullable.HasValue);

      Aver.IsFalse(RGDID.TryParse("0x0000030303030300303100000000000000000000000000000", out nullable));//too long
      Aver.IsFalse(nullable.HasValue);
    }

    [Run]
    public void RGDID_BinBuffer()
    {
      var gdid = new RGDID(0xbabafaca, new GDID(1, 2, 0x4b));
      var buf = gdid.Bytes;
      buf.ToDumpString(DumpFormat.Hex).See();
      var gdid2 = new RGDID(buf);
      Aver.AreEqual(gdid, gdid2);
    }

    [Run]
    public void RGDID_BinBufferAndTryParseBin()
    {
      var gdid = new RGDID(1234567, new GDID(347827, 15, 0xaedb3434b));
      var buf = gdid.Bytes;
      var hex = "0x" + buf.ToDumpString(DumpFormat.Hex).Replace(" ", "");

      hex.See();

      RGDID gdid2;
      Aver.IsTrue(RGDID.TryParse(hex, out gdid2));
      Aver.AreEqual(gdid, gdid2);
    }

    [Run]
    public void RGDID_Zero()
    {
      var zero = RGDID.ZERO;
      Aver.IsTrue(zero.IsZero);

      zero = new RGDID(123456, new GDID(0, 0, 0));
      Aver.IsTrue(zero.IsZero);//it is ZERO because GDID component may not be zero for non-zero RGDID

      zero = new RGDID(0, new GDID(0, 1, 0));
      Aver.IsFalse(zero.IsZero);
    }

    [Run]
    public void RGDID_RequiredRow_1()
    {
      var row = new RGDIDRow();
      var err = row.Validate();

      Aver.IsNotNull(err);
      Aver.IsTrue(err is FieldValidationException);
      Aver.AreEqual("RGDID", ((FieldValidationException)err).FieldName);

      row.RGDID = new RGDID(1, new GDID(1, 1, 1));
      err = row.Validate();

      Aver.IsNull(err);
      row.RGDID = RGDID.ZERO;
      err = row.Validate();

      Aver.IsNotNull(err);
      Aver.IsTrue(err is FieldValidationException);
      Aver.AreEqual("RGDID", ((FieldValidationException)err).FieldName);
    }

    [Run]
    public void RGDID_RequiredRow_2()
    {
      var row = new NullableRGDIDRow();
      var err = row.Validate();

      Aver.IsNotNull(err);
      Aver.IsTrue(err is FieldValidationException);
      Aver.AreEqual("RGDID", ((FieldValidationException)err).FieldName);

      row.RGDID = new RGDID(123, new GDID(1, 1, 1));
      err = row.Validate();

      Aver.IsNull(err);

      row.RGDID = RGDID.ZERO;
      err = row.Validate();

      Aver.IsNotNull(err);
      Aver.IsTrue(err is FieldValidationException);
      Aver.AreEqual("RGDID", ((FieldValidationException)err).FieldName);

      row.RGDID = null;//Nullable
      err = row.Validate();

      Aver.IsNotNull(err);
      Aver.IsTrue(err is FieldValidationException);
      Aver.AreEqual("RGDID", ((FieldValidationException)err).FieldName);
    }


    public class RGDIDRow : TypedDoc
    {
      [Field(required: true)] public RGDID RGDID { get; set; }
    }

    public class NullableRGDIDRow : TypedDoc
    {
      [Field(required: true)] public RGDID? RGDID { get; set; }
    }

  }
}
