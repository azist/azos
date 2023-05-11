/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos;
using Azos.Data;
using Azos.Log;
using Azos.Scripting;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class BixonTests
  {
    public static readonly JsonWritingOptions WITH_TYPES = new JsonWritingOptions(JsonWritingOptions.PrettyPrintRowsAsMap) { EnableTypeHints = true };
    public static readonly JsonWritingOptions MARSHALLED = new JsonWritingOptions(JsonWritingOptions.PrettyPrintRowsAsMap) { Purpose = JsonSerializationPurpose.Marshalling };

    public BixonTests()
    {
      Bixer.RegisterTypeSerializationCores(System.Reflection.Assembly.GetExecutingAssembly());
    }

    [Run]
    public void RootNull()
    {
      using var w = new BixWriterBufferScope(1024);
      JsonDataMap obj = null;
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader);
      Aver.IsNull(got);
    }

    [Run]
    public void RootString()
    {
      using var w = new BixWriterBufferScope(1024);
      string obj = "My root message";
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as string;
      Aver.AreEqual(obj, got);
    }

    [Run]
    public void RootAtom()
    {
      using var w = new BixWriterBufferScope(1024);
      Atom obj = Atom.Encode("a1234");
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = (Atom)Bixon.ReadObject(r.Reader);
      Aver.AreEqual(obj, got);
    }

    [Run]
    public void RootDateTime()
    {
      using var w = new BixWriterBufferScope(1024);
      DateTime obj = DateTime.UtcNow;
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = (DateTime)Bixon.ReadObject(r.Reader);
      Aver.AreEqual(obj, got);
    }

    [Run]
    public void RootTimeSpan()
    {
      using var w = new BixWriterBufferScope(1024);
      TimeSpan obj = TimeSpan.FromSeconds(2389.0909d);
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = (TimeSpan)Bixon.ReadObject(r.Reader);
      Aver.AreEqual(obj, got);
    }

    [Run]
    public void RootGuid()
    {
      using var w = new BixWriterBufferScope(1024);
      Guid obj = Guid.NewGuid();
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = (Guid)Bixon.ReadObject(r.Reader);
      Aver.AreEqual(obj, got);
    }

    [Run]
    public void RootGDID()
    {
      using var w = new BixWriterBufferScope(1024);
      GDID obj = new GDID(0, 1234);
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = (GDID)Bixon.ReadObject(r.Reader);
      Aver.AreEqual(obj, got);
    }

    [Run]
    public void RootRGDID()
    {
      using var w = new BixWriterBufferScope(1024);
      RGDID obj = new RGDID(1890, new GDID(8, 80233475));
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = (RGDID)Bixon.ReadObject(r.Reader);
      Aver.AreEqual(obj, got);
    }

    [Run]
    public void RootByteArray()
    {
      using var w = new BixWriterBufferScope(1024);
      byte[] obj = new byte[]{1,2,3,4,5,6,7,8,9,0,91,92,93,94,95,96,97,98,99,120};
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = (byte[])Bixon.ReadObject(r.Reader);
      Aver.IsTrue(obj.MemBufferEquals(got));
    }

    [Run]
    public void RootBool1()
    {
      using var w = new BixWriterBufferScope(1024);
      bool obj = false;
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = (bool)Bixon.ReadObject(r.Reader);
      Aver.AreEqual(obj, got);
    }

    [Run]
    public void RootBool2()
    {
      using var w = new BixWriterBufferScope(1024);
      bool obj = true;
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = (bool)Bixon.ReadObject(r.Reader);
      Aver.AreEqual(obj, got);
    }

    [Run]
    public void RootByte()
    {
      using var w = new BixWriterBufferScope(1024);
      byte obj = 0xfe;
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = (byte)Bixon.ReadObject(r.Reader);
      Aver.AreEqual(obj, got);
    }

    [Run]
    public void RootSByte()
    {
      using var w = new BixWriterBufferScope(1024);
      sbyte obj = -125;
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = (sbyte)Bixon.ReadObject(r.Reader);
      Aver.AreEqual(obj, got);
    }

    [Run]
    public void RootShort()
    {
      using var w = new BixWriterBufferScope(1024);
      short obj = -32000;
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = (short)Bixon.ReadObject(r.Reader);
      Aver.AreEqual(obj, got);
    }

    [Run]
    public void RootUShort()
    {
      using var w = new BixWriterBufferScope(1024);
      ushort obj = 65530;
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = (ushort)Bixon.ReadObject(r.Reader);
      Aver.AreEqual(obj, got);
    }

    [Run]
    public void RootInt()
    {
      using var w = new BixWriterBufferScope(1024);
      int obj = -32_000_000;
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = (int)Bixon.ReadObject(r.Reader);
      Aver.AreEqual(obj, got);
    }

    [Run]
    public void RootUInt()
    {
      using var w = new BixWriterBufferScope(1024);
      uint obj = 3_000_000_000;
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = (uint)Bixon.ReadObject(r.Reader);
      Aver.AreEqual(obj, got);
    }

    [Run]
    public void RootLong()
    {
      using var w = new BixWriterBufferScope(1024);
      long obj = -3_000_000_000;
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = (long)Bixon.ReadObject(r.Reader);
      Aver.AreEqual(obj, got);
    }

    [Run]
    public void RootULong()
    {
      using var w = new BixWriterBufferScope(1024);
      ulong obj = 23_000_000_000_000;
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = (ulong)Bixon.ReadObject(r.Reader);
      Aver.AreEqual(obj, got);
    }

    [Run]
    public void RootFloat()
    {
      using var w = new BixWriterBufferScope(1024);
      float obj = -34.002f;
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = (float)Bixon.ReadObject(r.Reader);
      Aver.AreEqual(obj, got);
    }

    [Run]
    public void RootDouble()
    {
      using var w = new BixWriterBufferScope(1024);
      double obj = -34.002343242d;
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = (double)Bixon.ReadObject(r.Reader);
      Aver.AreEqual(obj, got);
    }

    [Run]
    public void RootDecimal()
    {
      using var w = new BixWriterBufferScope(1024);
      decimal obj = -34.002343242m;
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = (decimal)Bixon.ReadObject(r.Reader);
      Aver.AreEqual(obj, got);
    }


    [Run]
    public void RootAnonymousObject_01()
    {
      using var w = new BixWriterBufferScope(1024);
      var obj = new {a =1, b=2};
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as JsonDataMap;
      Aver.IsNotNull(got);
      Aver.AreEqual(1, (int)got["a"]);
      Aver.AreEqual(2, (int)got["b"]);
    }

    [Run]
    public void RootAnonymousObject_02()
    {
      using var w = new BixWriterBufferScope(1024);
      var obj = new { a = 1, c= new { gugaaruba="abcd" },  b = 2 };
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as JsonDataMap;
      Aver.IsNotNull(got);
      Aver.AreEqual(1, (int)got["a"]);
      Aver.AreEqual(2, (int)got["b"]);
      Aver.AreEqual("abcd", (string)((JsonDataMap)got["c"])["gugaaruba"]);
    }


    [Run]
    public void RootArray_01()
    {
      using var w = new BixWriterBufferScope(1024);
      var obj = new object[]{1, "ok", true, null, Atom.Encode("call21")};
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as JsonDataArray;
      Aver.IsNotNull(got);
      Aver.AreEqual(5, got.Count);
      Aver.AreEqual(1, (int)got[0]);
      Aver.AreEqual("ok", (string)got[1]);
      Aver.AreEqual(true, (bool)got[2]);
      Aver.IsNull(got[3]);
      Aver.AreEqual(Atom.Encode("call21"), (Atom)got[4]);
    }

    [Run]
    public void RootArray_02()
    {
      using var w = new BixWriterBufferScope(1024);
      var obj = new object[] { 1, "ok", true, new { a=1, b=-234}, Atom.Encode("call21") };
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as JsonDataArray;
      Aver.IsNotNull(got);
      Aver.AreEqual(5, got.Count);
      Aver.AreEqual(1, (int)got[0]);
      Aver.AreEqual("ok", (string)got[1]);
      Aver.AreEqual(true, (bool)got[2]);
      var map = got[3] as JsonDataMap;
      Aver.IsNotNull(map);
      Aver.AreEqual(1, (int)map["a"]);
      Aver.AreEqual(-234, (int)map["b"]);
      Aver.AreEqual(Atom.Encode("call21"), (Atom)got[4]);

      got.See(WITH_TYPES);
    }

    [Run]
    public void RootArray_03()
    {
      using var w = new BixWriterBufferScope(1024);
      var obj = new string[] { "ok", "beaver", "caller", "Castro", null, "Guriy Yurary"};
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as JsonDataArray;
      Aver.IsNotNull(got);
      Aver.AreEqual(6, got.Count);
      Aver.AreEqual("ok", (string)got[0]);
      Aver.IsNull(got[4]);
      Aver.AreEqual("Guriy Yurary", (string)got[5]);
    }

    [Run]
    public void RootArray_04()
    {
      using var w = new BixWriterBufferScope(1024);
      var obj = new decimal[] { 100m, 200m, -1.01m };
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as JsonDataArray;
      Aver.IsNotNull(got);
      Aver.AreEqual(3, got.Count);
      Aver.AreEqual(100m, (decimal)got[0]);
      Aver.AreEqual(200m, (decimal)got[1]);
      Aver.AreEqual(-1.01m, (decimal)got[2]);
    }



    [Run]
    public void RoundtripAllTypes_01()
    {
      using var w = new BixWriterBufferScope(1024);

      var map = new JsonDataMap
      {
        {"null-key", null},
        {"str", "string 1"},
        {"atom", Atom.Encode("abc")},
        {"dt", new DateTime(1980, 2, 3, 14, 10, 05, DateTimeKind.Utc)},
        {"tspan", TimeSpan.FromSeconds(15.5)},
        {"bin", new byte[]{1,2,3,4,5,6,7,8,9,0,10,20,30,40,50,60,70,80,90,100}},
        {"eid", new EntityId(Atom.Encode("sys"), Atom.Encode("type"), Atom.Encode("sch"), "address 1")},
        {"gdid", new GDID(1, 190)},
        {"rgdid", new RGDID(5, new GDID(7, 2190))},
        {"guid", Guid.NewGuid()},

        {"bool1", false},
        {"bool2", true},

        {"byte", (byte)100},
        {"sbyte", (sbyte)-100},

        {"short", (short)-32000},
        {"ushort", (ushort)65534},

        {"int", (int)-3200000},
        {"uint", (uint)6553400},

        {"long", (long)-3200000},
        {"ulong", (ulong)6553400},

        {"float", -45.1f},
        {"double", -7890.0923d},
        {"decimal", 185_000.00m},

        {"sub-map", new JsonDataMap(){ {"a", 12345}, {"b", null} }},
        {"sub-object-anonymous", new { z=1000_000, q=-2, m = Atom.Encode("subobj")} },

        {"arr", new object[]{ 1, 2, true, false, "ok", 345, Atom.Encode("zxy")}},
      };
      Bixon.WriteObject(w.Writer, map);

      w.Buffer.ToHexDump().See();

      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as JsonDataMap;

      got.See(WITH_TYPES);
      averMapsEqual(map, got);

      Aver.AreEqual(-2, (int)(got["sub-object-anonymous"] as JsonDataMap)["q"]);
      Aver.AreEqual(1000_000, (int)(got["sub-object-anonymous"] as JsonDataMap)["z"]);
      Aver.AreEqual(Atom.Encode("subobj"), (Atom)(got["sub-object-anonymous"] as JsonDataMap)["m"]);
    }


    private static void averMapsEqual(JsonDataMap map1, JsonDataMap map2)
    {
      Aver.AreEqual(map1.Count, map2.Count);

      foreach (var kvp in map1)
      {
        if (kvp.Key== "sub-object-anonymous") continue;

        if (kvp.Value is JsonDataMap map)
          averMapsEqual(map, (JsonDataMap)map2[kvp.Key]);
        else if (kvp.Value is byte[] buf)
          Aver.IsTrue(buf.MemBufferEquals((byte[])map2[kvp.Key]));
        else if (kvp.Value is object[] oarr)
          Aver.IsTrue(oarr.SequenceEqual((IEnumerable<object>)map2[kvp.Key]));
        else
          Aver.AreObjectsEqual(kvp.Value, map2[kvp.Key]);
      }
    }


    [Run]
    public void RootDocNotMarshalled_01()
    {
      using var w = new BixWriterBufferScope(1024);
      var doc = new bxonBaseDoc()
      {
        String1 = "Fidel Castro",
        Int1 = 123,
        NInt1 = 678
      };

      Bixon.WriteObject(w.Writer, doc); //Default options = doc types ARE NOT marshalled across
      w.Buffer.ToHexDump().See();

      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as JsonDataMap;
      got.See(WITH_TYPES);
      Aver.IsNotNull(got);
      Aver.AreEqual(9, got.Count);
      Aver.AreEqual(doc.String1, (string)got["String1"]);
    }

    [Run]
    public void RootDocMarshalled_02()
    {
      using var w = new BixWriterBufferScope(1024);
      var doc = new bxonBaseDoc()
      {
        String1 = "Fidel Castro",
        Int1 = 123,
        NInt1 = 678
      };

      Bixon.WriteObject(w.Writer, doc, MARSHALLED); //marshall doc type identity
      w.Buffer.ToHexDump().See();

      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as bxonBaseDoc;
      got.See(WITH_TYPES);
      Aver.IsNotNull(got);
      Aver.AreEqual(doc.String1, got.String1);
    }

    [Run]
    public void RootDocPolyNotMarshalled_03()
    {
      using var w = new BixWriterBufferScope(1024);
      var doc = new bxonBaseDoc()
      {
        String1 = "Fidel Castro",
        Int1 = 123,
        NInt1 = 678,
        Obj1 = new bxonADoc() { String1 = "Odessa mama", String2 = "Cancun Salsa" }
      };

      Bixon.WriteObject(w.Writer, doc); //NOT marshalled
      w.Buffer.ToHexDump().See();

      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as JsonDataMap;
      got.See(WITH_TYPES);
      Aver.IsNotNull(got);
      Aver.AreEqual(doc.String1, (string)got["String1"]);
      Aver.IsNotNull(got["Obj1"]);
      Aver.IsTrue(got["Obj1"] is JsonDataMap);
      Aver.AreEqual("Odessa mama", (string)((JsonDataMap)got["Obj1"])["String1"]);
      Aver.AreEqual("Cancun Salsa",(string)((JsonDataMap)got["Obj1"])["String2"]);
    }

    [Run]
    public void RootDocPolyMarshalled_04()
    {
      using var w = new BixWriterBufferScope(1024);
      var doc = new bxonBaseDoc()
      {
        String1 = "Fidel Castro",
        Int1 = 123,
        NInt1 = 678,
        Obj1 = new bxonADoc(){ String1 = "Odessa mama", String2 = "Cancun Salsa" }
      };

      Bixon.WriteObject(w.Writer, doc, MARSHALLED); //marshall doc type identity
      w.Buffer.ToHexDump().See();

      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as bxonBaseDoc;
      got.See(WITH_TYPES);
      Aver.IsNotNull(got);
      Aver.AreEqual(doc.String1, got.String1);
      Aver.IsNotNull(got.Obj1);
      Aver.IsTrue(got.Obj1 is bxonADoc);
      Aver.AreEqual("Odessa mama", ((bxonADoc)got.Obj1).String1);
      Aver.AreEqual("Cancun Salsa", ((bxonADoc)got.Obj1).String2);
    }

  }
}