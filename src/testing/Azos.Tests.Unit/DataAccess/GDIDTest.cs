/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.DataAccess.CRUD;
using Azos.DataAccess.Distributed;
using Azos.Scripting;
using Azos.Serialization.JSON;
using static Azos.Aver.ThrowsAttribute;

namespace Azos.Tests.Unit.DataAccess
{
  [Runnable(TRUN.BASE, 3)]
  public class GDIDTest
  {
      [Run]
      public void GDID_1()
      {
        var gdid = new GDID(2, 5, 89078);
        Aver.AreEqual((uint)2,     gdid.Era);
        Aver.AreEqual(5,     gdid.Authority);
        Aver.AreEqual((ulong)89078, gdid.Counter);
      }

      [Run]
      [Aver.Throws(typeof(DistributedDataAccessException), Message="GDID can not be created from the supplied", MsgMatch=MatchType.Contains) ]
      public void GDID_2()
      {
        var gdid = new GDID(0, 16, 89078);
      }

      [Run]
      [Aver.Throws(typeof(DistributedDataAccessException), Message="GDID can not be created from the supplied", MsgMatch=MatchType.Contains) ]
      public void GDID_3()
      {
        var gdid = new GDID(0, 12, GDID.COUNTER_MAX+1);
      }

      [Run]
      public void GDID_4()
      {
        var gdid = new GDID(0, 15, GDID.COUNTER_MAX);
        Aver.AreEqual(15,               gdid.Authority);
        Aver.AreEqual(GDID.COUNTER_MAX, gdid.Counter);
      }

      [Run]
      public void GDID_5()
      {
        var gdid = new GDID(0, 0, GDID.COUNTER_MAX);
        Aver.AreEqual(0,                gdid.Authority);
        Aver.AreEqual(GDID.COUNTER_MAX, gdid.Counter);
      }

      [Run]
      public void GDID_6()
      {
        var gdid = new GDID(0, 0, 0);
        Aver.AreEqual(0, gdid.Authority);
        Aver.AreEqual((ulong)0, gdid.Counter);
      }


      [Run]
      public void GDID_7()
      {
        var gdid1 = new GDID(0, 0, 12321);
        var gdid2 = new GDID(0, 1, 0);
        Aver.AreEqual(-1, gdid1.CompareTo(gdid2));
        Aver.IsFalse( gdid1.Equals(gdid2));
      }

      [Run]
      public void GDID_8()
      {
        var gdid1 = new GDID(0, 1, 12321);
        var gdid2 = new GDID(0, 1, 0);
        Aver.AreEqual(1, gdid1.CompareTo(gdid2));
        Aver.IsFalse( gdid1.Equals(gdid2));
      }

      [Run]
      public void GDID_9()
      {
        var gdid1 = new GDID(0, 3, 57);
        var gdid2 = new GDID(0, 3, 57);
        Aver.AreEqual(0, gdid1.CompareTo(gdid2));
        Aver.IsTrue( gdid1.Equals(gdid2));

        var gdid3 = new GDID(1, 3, 57);
        var gdid4 = new GDID(2, 3, 57);
        Aver.AreEqual(-1, gdid3.CompareTo(gdid4));
        Aver.IsFalse( gdid3.Equals(gdid4));
      }

      [Run]
      public void GDID_10()
      {
        var gdid = new GDID(1293, 3, 57);
        var s = gdid.ToString();
        Console.WriteLine(s);
        Aver.AreEqual("1293:3:57", s);
      }

      [Run]
      public void GDID_11()
      {
        var gdid = new GDID(0x01020304, 0xfacaca00aa55aa55);

        Aver.IsTrue( "01,02,03,04,fa,ca,ca,00,aa,55,aa,55".AsByteArray().SequenceEqual( gdid.Bytes ) );
      }


      [Run]
      public void GDID_JSON_1()
      {
        var gdid = new GDID(2, 3, 57);
        var s = gdid.ToJSON();
        Console.WriteLine(s);
        Aver.AreEqual("\"2:3:57\"", s);
      }

      [Run]
      public void GDID_JSON_2()
      {
        var obj = new{ id = new GDID(22, 3, 57), Name = "Tezter"};
        var s = obj.ToJSON();
        Console.WriteLine(s);
        Aver.AreEqual("{\"id\":\"22:3:57\",\"Name\":\"Tezter\"}", s);
      }

      [Run()]
      public void GDID_TryParse()
      {
        GDID parsed;
        Aver.IsTrue( GDID.TryParse("1:2:3", out parsed) );
        Aver.AreEqual((uint)1, parsed.Era);
        Aver.AreEqual(2, parsed.Authority);
        Aver.AreEqual((ulong)3, parsed.Counter);

        Aver.IsTrue( GDID.TryParse("231:2:3123", out parsed) );
        Aver.AreEqual((uint)231, parsed.Era);
        Aver.AreEqual(2, parsed.Authority);
        Aver.AreEqual((ulong)3123, parsed.Counter);

        Aver.IsTrue( GDID.TryParse("   231:2:3123   ", out parsed) );
        Aver.AreEqual((uint)231, parsed.Era);
        Aver.AreEqual(2, parsed.Authority);
        Aver.AreEqual((ulong)3123, parsed.Counter);

        Aver.IsTrue( GDID.TryParse("31 : 2:  3123", out parsed) );
        Aver.AreEqual((uint)31, parsed.Era);
        Aver.AreEqual(2, parsed.Authority);
        Aver.AreEqual((ulong)3123, parsed.Counter);

        Aver.IsFalse( GDID.TryParse("-1:2:3123", out parsed) );
        Aver.IsFalse( GDID.TryParse("1:18:3123", out parsed) );
        Aver.IsFalse( GDID.TryParse(":18:3123", out parsed) );
        Aver.IsFalse( GDID.TryParse("::3123", out parsed) );
        Aver.IsFalse( GDID.TryParse("1::3", out parsed) );
        Aver.IsFalse( GDID.TryParse("1::", out parsed) );
        Aver.IsFalse( GDID.TryParse("1:-:-", out parsed) );
        Aver.IsFalse( GDID.TryParse("1: : ", out parsed) );

        Aver.IsFalse( GDID.TryParse("FTGEJK-IR", out parsed) );


        //0x 00 00 00 00 10 00 00 00 00 00 00 4B
        //   ----era---- ------ulong -----------
        Aver.IsTrue( GDID.TryParse("0x00000000100000000000004B", out parsed) );
        Aver.AreEqual( new GDID(0,1,0x4b), parsed);
        Aver.IsTrue( GDID.TryParse("00000000100000000000004B", out parsed) );
        Aver.AreEqual( new GDID(0,1,0x4b), parsed);
        Aver.IsTrue( GDID.TryParse(new GDID(0, 1, 0x4b).ToHexString(), out parsed) );
        Aver.AreEqual( new GDID(0,1,0x4b), parsed);

        GDID? nullable;
        Aver.IsTrue( GDID.TryParse("0X00000000100000000000004b", out nullable) );
        Aver.IsTrue( nullable.HasValue);
        Aver.AreEqual( new GDID(0,1,0x4b), nullable.Value);

        Aver.IsTrue( GDID.TryParse("00000000100000000000004b", out nullable) );
        Aver.IsTrue( nullable.HasValue);
        Aver.AreEqual( new GDID(0,1,0x4b), nullable.Value);

        Aver.IsTrue( GDID.TryParse(new GDID(0, 1, 0x4b).ToHexString(), out nullable) );
        Aver.IsTrue( nullable.HasValue);
        Aver.AreEqual( new GDID(0,1,0x4b), nullable.Value);

        Aver.IsFalse( GDID.TryParse("0x0001000000000000", out nullable) );//too short
        Aver.IsFalse( nullable.HasValue);

        Aver.IsFalse( GDID.TryParse("0x00000303030303003031000000000000", out nullable) );//too long
        Aver.IsFalse( nullable.HasValue);
      }

      [Run()]
      public void GDID_BinBuffer()
      {
        var gdid = new GDID(0,1,0x4b);
        var buf = gdid.Bytes;
        Console.WriteLine(buf.ToDumpString(DumpFormat.Hex));
        var gdid2 = new GDID(buf);
        Aver.AreEqual(gdid, gdid2);
      }

      [Run()]
      public void GDID_BinBufferAndTryParseBin()
      {
        var gdid = new GDID(347827,15,0xaedb3434b);
        var buf = gdid.Bytes;
        var hex = "0x"+buf.ToDumpString(DumpFormat.Hex).Replace(" ","");

        Console.WriteLine(hex);

        GDID gdid2;
        Aver.IsTrue(GDID.TryParse(hex, out gdid2));
        Aver.AreEqual(gdid, gdid2);
      }


      [Run()]
      public void GDID_RangeComparer()
      {
        Aver.AreEqual(-1, GDIDRangeComparer.Instance.Compare( new GDID(0, 1, 2), new GDID(1,1,2)));
        Aver.AreEqual(+1, GDIDRangeComparer.Instance.Compare( new GDID(1, 1, 2), new GDID(0,1,2)));

        Aver.AreEqual(-1, GDIDRangeComparer.Instance.Compare( new GDID(0, 1, 2), new GDID(0,1,3)));
        Aver.AreEqual(+1, GDIDRangeComparer.Instance.Compare( new GDID(1, 1, 2), new GDID(1,1,0)));

        Aver.AreEqual(0, GDIDRangeComparer.Instance.Compare( new GDID(1, 1, 2), new GDID(1,1,2)));

        //notice: Authority is ignored
        Aver.AreEqual(0, GDIDRangeComparer.Instance.Compare( new GDID(1, 13, 2), new GDID(1,8,2)));

      }

      [Run()]
      public void GDID_Zero()
      {
        var zero = GDID.Zero;
        Aver.IsTrue( zero.IsZero );

        zero = new GDID(0,1,0);
        Aver.IsFalse( zero.IsZero );
      }

      [Run()]
      public void GDID_RequiredRow_1()
      {
        var row = new GDIDRow();
        var err = row.Validate();

        Aver.IsNotNull(err);
        Aver.IsTrue(err is CRUDFieldValidationException);
        Aver.AreEqual("GDID", ((CRUDFieldValidationException)err).FieldName);

        row.GDID = new GDID(1,1,1);
        err = row.Validate();

        Aver.IsNull(err);
        row.GDID = GDID.Zero;
        err = row.Validate();

        Aver.IsNotNull(err);
        Aver.IsTrue(err is CRUDFieldValidationException);
        Aver.AreEqual("GDID", ((CRUDFieldValidationException)err).FieldName);
      }

      [Run()]
      public void GDID_RequiredRow_2()
      {
        var row = new NullableGDIDRow();
        var err = row.Validate();

        Aver.IsNotNull(err);
        Aver.IsTrue(err is CRUDFieldValidationException);
        Aver.AreEqual("GDID", ((CRUDFieldValidationException)err).FieldName);

        row.GDID = new GDID(1,1,1);
        err = row.Validate();

        Aver.IsNull(err);

        row.GDID = GDID.Zero;
        err = row.Validate();

        Aver.IsNotNull(err);
        Aver.IsTrue(err is CRUDFieldValidationException);
        Aver.AreEqual("GDID", ((CRUDFieldValidationException)err).FieldName);

        row.GDID = null;//Nullable
        err = row.Validate();

        Aver.IsNotNull(err);
        Aver.IsTrue(err is CRUDFieldValidationException);
        Aver.AreEqual("GDID", ((CRUDFieldValidationException)err).FieldName);
      }

      public class GDIDRow: TypedRow
      {
        [Field(required: true)] public GDID GDID {get; set;}
      }

      public class NullableGDIDRow: TypedRow
      {
        [Field(required: true)] public GDID? GDID {get; set;}
      }


      [Run()]
      public void GDIDSymbol()
      {
        var link = new ELink(new GDID(1280, 2, 8902382));

        var gs = link.AsGDIDSymbol();

        Aver.AreEqual(link.GDID, gs.GDID);
        Aver.AreEqual(link.Link, gs.Symbol);

        var gs2 = new GDIDSymbol(gs.GDID, gs.Symbol);
        Aver.AreEqual(gs, gs2);
        Aver.IsTrue( gs.Equals(gs2));
        Aver.IsFalse( gs.Equals(this));
        Aver.IsFalse( gs.Equals(null));

        Aver.AreEqual(gs.GetHashCode(), gs2.GetHashCode());

        Console.WriteLine( gs.ToString());
      }

  }
}
