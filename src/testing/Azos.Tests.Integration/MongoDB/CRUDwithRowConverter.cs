/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

using Azos.Data;
using Azos.Serialization.BSON;
using Azos.Data.Access.MongoDb.Connector;
using Azos.Financial;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Integration.MongoDb
{
  [Runnable]
  public class CRUDwithRowConverter
  {


    [Run]
    public void T_01_Insert()
    {
      var BIN = new byte[] { 0x00, 0x79, 0x14 };

      using(var client= new MongoClient("My Test"))
      {
        var db = client.DefaultLocalServer["db1"];

        db["t1"].Drop();

        var t1 = db["t1"];

        var row = new TestRow()
        {
          _id = 1,

          String1 = "Mudaker",
          String2 = null,
          Date1 = new DateTime(1980, 07, 12, 10, 13, 27, DateTimeKind.Utc),
          Date2 = null,
          Bool1 = true,
          Bool2 = null,
          Guid1 = new Guid("{9195F7DB-FE21-4BB2-B006-2496F4E24D14}"),
          Guid2 = null,
          Gdid1 = new GDID(0, 12345),
          Gdid2 = null,
          Float1 = 127.0123f,
          Float2 = null,
          Double1 = 122345.012d,
          Double2 = null,
          Decimal1 = 1234567.098M,
          Decimal2 = null,
          Amount1 = new Amount("din", 123.11M),
          Amount2 = null,
          Bytes1 = BIN,
          Bytes2 = null,

          Byte1 = 23,
          SByte1 = -3,
          Short1 = -32761,
          UShort1 = 65535,
          Int1 = 4324,
          Uint1 = 42345,
          Long1 = 993,
          ULong1 = 8829383762,
          ETest1 = ETest.Two,
          EFlags1 = EFlags.First | EFlags.Third,

          Byte2 = null,
          SByte2 = null,
          Short2 = null,
          UShort2 = null,
          Int2 = null,
          Uint2 = null,
          Long2 = null,
          ULong2 = null,
          ETest2 = null,
          EFlags2 = null
        };

        var rc = new DataDocConverter();
        var doc = rc.DataDocToBSONDocument(row, "A");
        Aver.AreEqual(1, t1.Insert(doc).TotalDocumentsAffected);

        var got = db["t1"].FindOne(Query.ID_EQ_Int32(1));
        Aver.IsNotNull( got );

        var row1 = new TestRow();
        rc.BSONDocumentToDataDoc(got, row1, "A");

        Aver.AreObjectsEqual(row, row1);
      }

    }

    [Run]
    public void T_01_Insert_Parallel()
    {
      const int CNT = 37197;
      var BIN = new byte[] { 0x00, 0x79, 0x14 };

      using(var client= new MongoClient("My Test"))
      {
        var db = client.DefaultLocalServer["db1"];

        db["t1"].Drop();

        var t1 = db["t1"];

        var rows = new TestRow[CNT];

        var rc = new DataDocConverter();

        var sw = new Stopwatch();

        sw.Start();
        Parallel.For(0, CNT, i =>
        {
          var row = new TestRow()
          {
            _id = i,

            String1 = "Mudaker_" + i.ToString(),
            String2 = null,
            Date1 = new DateTime(1980, 07, 12, 10, 13, 27, DateTimeKind.Utc),
            Date2 = null,
            Bool1 = true,
            Bool2 = null,
            Guid1 = new Guid("{9195F7DB-FE21-4BB2-B006-2496F4E24D14}"),
            Guid2 = null,
            Gdid1 = new GDID(0, 12345),
            Gdid2 = null,
            Float1 = 127.0123f,
            Float2 = null,
            Double1 = 122345.012d,
            Double2 = null,
            Decimal1 = 1234567.098M,
            Decimal2 = null,
            Amount1 = new Amount("din", 123.11M + (decimal)i),
            Amount2 = null,
            Bytes1 = BIN,
            Bytes2 = null,

            Byte1 = 23,
            SByte1 = -3,
            Short1 = -32761,
            UShort1 = 65535,
            Int1 = 4324,
            Uint1 = 42345,
            Long1 = 993,
            ULong1 = 8829383762,
            ETest1 = ETest.Two,
            EFlags1 = EFlags.First | EFlags.Third,

            Byte2 = null,
            SByte2 = null,
            Short2 = null,
            UShort2 = null,
            Int2 = null,
            Uint2 = null,
            Long2 = null,
            ULong2 = null,
            ETest2 = null,
            EFlags2 = null
          };

          rows[i] = row;

          var doc = rc.DataDocToBSONDocument(row, "A");
          Aver.AreEqual(1, t1.Insert(doc).TotalDocumentsAffected);

        });
        sw.Stop();
        Console.WriteLine("{0:N0} row inserted in {1:N3} sec on {2:N0} ops/sec", CNT, sw.Elapsed.TotalSeconds, CNT / sw.Elapsed.TotalSeconds);

        sw.Restart();
        var RCNT = CNT * 2;
        Parallel.For(0, RCNT, i => {
          var j = i % CNT;
          var got = db["t1"].FindOne(Query.ID_EQ_Int32(j));
          Aver.IsNotNull( got );

          var row1 = new TestRow();
          rc.BSONDocumentToDataDoc(got, row1, "A");

          Aver.AreObjectsEqual(rows[row1._id], row1);
        });
        sw.Stop();
        Console.WriteLine("{0:N0} row red in {1:N3} sec on {2:N0} ops/sec", RCNT, sw.Elapsed.TotalSeconds, RCNT / sw.Elapsed.TotalSeconds);
      }

    }

    [Run]
    public void T_02_Update()
    {
      var BIN = new byte[] { 0x00, 0x79, 0x14 };

      using(var client= new MongoClient("My Test"))
      {
        var db = client.DefaultLocalServer["db1"];

        db["t1"].Drop();

        var t1 = db["t1"];

        var row = new TestRow()
        {
          _id = 1,

          String1 = "Mudaker",
          String2 = null,
          Date1 = new DateTime(1980, 07, 12, 10, 13, 27, DateTimeKind.Utc),
          Date2 = null,
          Bool1 = true,
          Bool2 = null,
          Guid1 = new Guid("{9195F7DB-FE21-4BB2-B006-2496F4E24D14}"),
          Guid2 = null,
          Gdid1 = new GDID(0, 12345),
          Gdid2 = null,
          Float1 = 127.0123f,
          Float2 = null,
          Double1 = 122345.012d,
          Double2 = null,
          Decimal1 = 1234567.098M,
          Decimal2 = null,
          Amount1 = new Amount("din", 123.11M),
          Amount2 = null,
          Bytes1 = BIN,
          Bytes2 = null,

          Byte1 = 23,
          SByte1 = -3,
          Short1 = -32761,
          UShort1 = 65535,
          Int1 = 4324,
          Uint1 = 42345,
          Long1 = 993,
          ULong1 = 8829383762,
          ETest1 = ETest.Two,
          EFlags1 = EFlags.First | EFlags.Third,

          Byte2 = null,
          SByte2 = null,
          Short2 = null,
          UShort2 = null,
          Int2 = null,
          Uint2 = null,
          Long2 = null,
          ULong2 = null,
          ETest2 = null,
          EFlags2 = null
        };

        var rc = new DataDocConverter();
        var doc = rc.DataDocToBSONDocument(row, "A");
        Aver.AreEqual(1, t1.Insert(doc).TotalDocumentsAffected);

        row.String1 = "makaka";
        row.Int1 = 9789;

        doc = rc.DataDocToBSONDocument(row, "A");

        var r = t1.Save(doc);
        Aver.AreEqual(1, r.TotalDocumentsAffected);
        Aver.AreEqual(1, r.TotalDocumentsUpdatedAffected);
        Aver.IsNull(r.WriteErrors);

        var got = db["t1"].FindOne(Query.ID_EQ_Int32(1));
        Aver.IsNotNull( got );

        var row1 = new TestRow();
        rc.BSONDocumentToDataDoc(got, row1, "A");

        Aver.AreObjectsEqual(row, row1);

      }

    }

    [Run]
    public void T_02_Update_Parallel()
    {
      const int CNT = 11973;
      var BIN = new byte[] { 0x00, 0x79, 0x14 };

      using(var client= new MongoClient("My Test"))
      {
        var db = client.DefaultLocalServer["db1"];

        db["t1"].Drop();

        var t1 = db["t1"];

        var rows = new TestRow[CNT];

        var rc = new DataDocConverter();

        var sw = new Stopwatch();

        sw.Start();

        Parallel.For(0, CNT, i =>
        {
          var row = new TestRow()
          {
            _id = i,

            String1 = "Mudaker",
            String2 = null,
            Date1 = new DateTime(1980, 07, 12, 10, 13, 27, DateTimeKind.Utc),
            Date2 = null,
            Bool1 = true,
            Bool2 = null,
            Guid1 = new Guid("{9195F7DB-FE21-4BB2-B006-2496F4E24D14}"),
            Guid2 = null,
            Gdid1 = new GDID(0, 12345),
            Gdid2 = null,
            Float1 = 127.0123f,
            Float2 = null,
            Double1 = 122345.012d,
            Double2 = null,
            Decimal1 = 1234567.098M,
            Decimal2 = null,
            Amount1 = new Amount("din", 123.11M),
            Amount2 = null,
            Bytes1 = BIN,
            Bytes2 = null,

            Byte1 = 23,
            SByte1 = -3,
            Short1 = -32761,
            UShort1 = 65535,
            Int1 = 4324,
            Uint1 = 42345,
            Long1 = 993,
            ULong1 = 8829383762,
            ETest1 = ETest.Two,
            EFlags1 = EFlags.First | EFlags.Third,

            Byte2 = null,
            SByte2 = null,
            Short2 = null,
            UShort2 = null,
            Int2 = null,
            Uint2 = null,
            Long2 = null,
            ULong2 = null,
            ETest2 = null,
            EFlags2 = null
          };

          rows[i] = row;

          var doc = rc.DataDocToBSONDocument(row, "A");
          Aver.AreEqual(1, t1.Insert(doc).TotalDocumentsAffected);
        });
        sw.Stop();
        Console.WriteLine("{0:N0} row inserted in {1:N3} sec on {2:N0} ops/sec", CNT, sw.Elapsed.TotalSeconds, CNT / sw.Elapsed.TotalSeconds);

        sw.Restart();
        Parallel.For(0, CNT, i => {
          var row = rows[i];
          row.String1 = "makaka" + i.ToString();
          row.Int1 = 9789 + (i * 100);

          var doc = rc.DataDocToBSONDocument(row, "A");

          var r = t1.Save(doc);
          Aver.AreEqual(1, r.TotalDocumentsAffected);
          Aver.AreEqual(1, r.TotalDocumentsUpdatedAffected);
          Aver.IsNull(r.WriteErrors);
        });
        sw.Stop();
        Console.WriteLine("{0:N0} row updated in {1:N3} sec on {2:N0} ops/sec", CNT, sw.Elapsed.TotalSeconds, CNT / sw.Elapsed.TotalSeconds);

        sw.Restart();
        var RCNT = CNT * 3;
        Parallel.For(0, RCNT, i => {
          var j = i % CNT;
          var got = db["t1"].FindOne(Query.ID_EQ_Int32(j));
          Aver.IsNotNull( got );

          var row1 = new TestRow();
          rc.BSONDocumentToDataDoc(got, row1, "A");

          Aver.AreObjectsEqual(rows[j], row1);
        });
        sw.Stop();
        Console.WriteLine("{0:N0} row red in {1:N3} sec on {2:N0} ops/sec", RCNT, sw.Elapsed.TotalSeconds, RCNT / sw.Elapsed.TotalSeconds);

      }

    }

    [Run]
    public void T_03_Update()
    {
      var BIN = new byte[] { 0x00, 0x79, 0x14 };

      using(var client= new MongoClient("My Test"))
      {
        var db = client.DefaultLocalServer["db1"];

        db["t1"].Drop();

        var t1 = db["t1"];

        var row = new TestRow()
        {
          _id = 1,

          String1 = "Mudaker",
        };

        var rc = new DataDocConverter();
        var doc = rc.DataDocToBSONDocument(row, "A");
        Aver.AreEqual(1, t1.Insert(doc).TotalDocumentsAffected);

        var updateResult = t1.Update
        (
          new UpdateEntry
          (
            Query.ID_EQ_Int32(1),
            new Update("{'String1': '$$VAL'}", true, new TemplateArg("VAL", BSONElementType.String, "makaka")),
            false,
            false
          )
        );

        var got = db["t1"].FindOne(Query.ID_EQ_Int32(1));
        Aver.IsNotNull( got );

        var row1 = new TestRow();
        rc.BSONDocumentToDataDoc(got, row1, "A");

        Aver.AreEqual("makaka", row1.String1);
      }

    }

    [Run]
    public void T_04_Array()
    {
      using(var client= new MongoClient("My Test"))
      {
        var db = client.DefaultLocalServer["db1"];

        db["t1"].Drop();

        var t1 = db["t1"];

        var row = new ArrayRow
        {
          _id = 1,
          Map = new JSONDataMap{{"Name","Xerson"},{"Age",123}},
          List = new List<object>{ 1,true, "YEZ!", -123.01},
          ObjectArray = new object[]{123, -12, 789d, null, new object[] { 54.67d, "alpIna"}},
          MapArray = new JSONDataMap[]{ new JSONDataMap{{"a",1},{"b",true}},  new JSONDataMap{{"kosmos",234.12},{"b",null}} },
          MapList = new List<JSONDataMap>{ new JSONDataMap{{"abc",0},{"buba", -40.0789}},  new JSONDataMap{{"nothing",null}} }
        };

        var rc = new DataDocConverter();
        var doc = rc.DataDocToBSONDocument(row, "A");
        Aver.AreEqual(1, t1.Insert(doc).TotalDocumentsAffected);

        var got = db["t1"].FindOne(Query.ID_EQ_Int32(1));
        Aver.IsNotNull( got );

        var row1 = new ArrayRow();
        rc.BSONDocumentToDataDoc(got, row1, "A");

        Aver.AreObjectsEqual(row, row1);
      }

    }

  }

  #region Mocks

    public enum ETest { Zero = 0x77, One, Two }

    [Flags]
    public enum EFlags { First = 0x01, Second = 0x02, Third = 0x04, Fifth = 0x0f, FirstSecond = First | Second }

    public class ArrayRow : TypedDoc
    {
      [Field] public int _id {get; set;}

      [Field] public JSONDataMap  Map{get; set;}
      [Field] public object[]  ObjectArray{get; set;}
      [Field] public JSONDataMap[]  MapArray{get; set;}
      [Field] public List<object> List{get; set;}
      [Field] public List<JSONDataMap> MapList{get; set;}
    }

    public class TestRow : TypedDoc
    {
      [Field] public int _id {get; set;}

      [Field] public string String1{get; set;}

      [Field(targetName: "A", backendName: "s2")]
      [Field(targetName: "B", backendName: "STRING-2")]
      public string String2{get; set;}

      [Field] public byte Byte1{get; set;}
      [Field] public sbyte SByte1{get; set;}
      [Field] public short Short1{get; set;}
      [Field] public ushort UShort1{get; set;}
      [Field] public int Int1{get; set;}
      [Field] public uint Uint1{get; set;}
      [Field] public long Long1{get; set;}
      [Field] public ulong ULong1{get; set;}

      [Field] public byte?   Byte2{get; set;}
      [Field] public sbyte?  SByte2{get; set;}
      [Field] public short?  Short2{get; set;}
      [Field] public ushort? UShort2{get; set;}
      [Field] public int?    Int2{get; set;}
      [Field] public uint?   Uint2{get; set;}
      [Field] public long?   Long2{get; set;}
      [Field] public ulong?  ULong2{get; set;}



      [Field] public DateTime Date1{get; set;}
      [Field] public DateTime? Date2{get; set;}
      [Field] public bool Bool1{get; set;}
      [Field] public bool? Bool2{get; set;}
      [Field] public Guid Guid1{get; set;}
      [Field] public Guid? Guid2{get; set;}
      [Field] public GDID Gdid1{get; set;}
      [Field] public GDID? Gdid2{get; set;}

      [Field] public float Float1{get; set;}
      [Field] public float? Float2{get; set;}
      [Field] public double Double1{get; set;}
      [Field] public double? Double2{get; set;}
      [Field] public decimal Decimal1{get; set;}
      [Field] public decimal? Decimal2{get; set;}
      [Field] public Amount Amount1{get; set;}
      [Field] public Amount? Amount2{get; set;}
      [Field] public byte[] Bytes1{get; set;}
      [Field] public byte[] Bytes2{get; set;}

      [Field] public ETest ETest1 {get; set;}
      [Field] public ETest? ETest2 {get; set;}

      [Field] public EFlags EFlags1 {get; set;}
      [Field] public EFlags? EFlags2 {get; set;}

      public override bool Equals(Doc other)
      {
        var or = other as TestRow;
        if (or==null) return false;

        foreach(var f in this.Schema)
        {
          var v1 = this.GetFieldValue(f);
          var v2 = or.GetFieldValue(f);

          if (v1==null)
          {
          if (v2==null) continue;
          else return false;
          }
          else if (v2 == null)
          return false;

          if (v1 is byte[])
          {
            return ((byte[])v1).SequenceEqual((byte[])v2);
          }

          if (!v1.Equals( v2 )) return false;
        }

        return true;
      }
    }



  #endregion

}
