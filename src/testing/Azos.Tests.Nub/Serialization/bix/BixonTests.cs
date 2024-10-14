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
using Azos.Scripting;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;
using Azos.Time;
using Azos.Log;

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

    private class specialObject
    {
      public int Id { get; set; }
      public string Str { get; set; }
      public Atom Atm { get; set; }
    }


    [Run]
    public void RootCustomUnsupportedObjectWrittenAsJson()
    {
      using var w = new BixWriterBufferScope(1024);
      specialObject obj = new specialObject{ Id = 980, Str = "abcdef", Atm = Atom.Encode("z234") };
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as JsonDataMap;
      Aver.IsNotNull(got);
      got.See(WITH_TYPES);
      Aver.AreEqual(980, (int)got["Id"]);
      Aver.AreEqual("abcdef", (string)got["Str"]);
      Aver.AreEqual(Atom.Encode("z234"), (Atom)got["Atm"]);
    }


    [Run]
    public void RootJsonDataMap_01()
    {
      using var w = new BixWriterBufferScope(1024);
      var obj = new JsonDataMap(caseSensitive: true) { { "1", 2 }, { "29", "twenty nine" }, { "-900", Atom.Encode("mmm") } };
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as JsonDataMap;
      Aver.IsNotNull(got);
      Aver.IsTrue(got.CaseSensitive);
      Aver.AreEqual(3, got.Count);
      Aver.AreEqual(2, (int)got["1"]);
      Aver.AreEqual("twenty nine", (string)got["29"]);
      Aver.AreEqual(Atom.Encode("mmm"), (Atom)got["-900"]);
    }

    [Run]
    public void RootJsonDataMap_02_insensetive()
    {
      using var w = new BixWriterBufferScope(1024);
      var obj = new JsonDataMap(caseSensitive: false) { { "1", 2 }, { "29", "twenty nineZZ" }, { "-900", Atom.Encode("xcv") } };
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as JsonDataMap;
      Aver.IsNotNull(got);
      Aver.IsFalse(got.CaseSensitive);
      Aver.AreEqual(3, got.Count);
      Aver.AreEqual(2, (int)got["1"]);
      Aver.AreEqual("twenty nineZZ", (string)got["29"]);
      Aver.AreEqual(Atom.Encode("xcv"), (Atom)got["-900"]);
    }


    [Run, Aver.Throws(ExceptionType = typeof(BixException))]
    public void CirculareReference_01()
    {
      using var w = new BixWriterBufferScope(1024);
      var obj = new JsonDataMap(caseSensitive: false) { { "1", 2 }, { "29", "twenty nineZZ" }, { "-900", Atom.Encode("xcv") }};
      obj["loop"] = obj;//circular reference

      Bixon.WriteObject(w.Writer, obj);//crash
    }

    [Run, Aver.Throws(ExceptionType = typeof(BixException))]
    public void CirculareReference_02()
    {
      using var w = new BixWriterBufferScope(1024);

      var loop = new JsonDataMap { { "a", 1 } };
      var obj = new JsonDataMap(caseSensitive: false) { { "1", 2 }, { "29", "twenty nineZZ" }, { "-900", Atom.Encode("xcv") }, { "loop", loop } };
      loop["root"] = obj;//infinite loop

      Bixon.WriteObject(w.Writer, obj);//crash
    }

    [Run, Aver.Throws(ExceptionType = typeof(BixException))]
    public void CirculareReference_03()
    {
      using var w = new BixWriterBufferScope(1024);
      var obj = new JsonDataArray{ 1 ,2,3};
      obj.Add(obj);//circular reference

      Bixon.WriteObject(w.Writer, obj);//crash
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
    public void RootList_01()
    {
      using var w = new BixWriterBufferScope(1024);
      var obj = new List<object>{ 1, "ok", true, null, Atom.Encode("call21")};
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
    public void RootList_02()
    {
      using var w = new BixWriterBufferScope(1024);
      var obj = new List<string> { "1", "ok", "true"};
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as JsonDataArray;
      Aver.IsNotNull(got);
      Aver.AreEqual(3, got.Count);
      Aver.AreEqual("1", (string)got[0]);
      Aver.AreEqual("ok", (string)got[1]);
      Aver.AreEqual("true", (string)got[2]);
    }

    [Run]
    public void RootList_03()
    {
      using var w = new BixWriterBufferScope(1024);
      var obj = new List<Doc> { new bxonADoc{ String1 = "A"}, new bxonBDoc { String1 = "B" } };
      Bixon.WriteObject(w.Writer, obj, MARSHALLED);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as JsonDataArray;
      Aver.IsNotNull(got);
      Aver.AreEqual(2, got.Count);
      Aver.AreEqual("A", ((bxonADoc)got[0]).String1);
      Aver.AreEqual("B", ((bxonBDoc)got[1]).String1);
    }

    [Run]
    public void RootDict_01()
    {
      using var w = new BixWriterBufferScope(1024);
      var obj = new Dictionary<int, object> { {1, 2}, {29, "twenty nine"}, {-900, Atom.Encode("mmm")} };
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as JsonDataMap;
      Aver.IsNotNull(got);
      Aver.IsTrue(got.CaseSensitive);
      Aver.AreEqual(3, got.Count);
      Aver.AreEqual(2, (int)got["1"]);
      Aver.AreEqual("twenty nine", (string)got["29"]);
      Aver.AreEqual(Atom.Encode("mmm"), (Atom)got["-900"]);
    }

    [Run]
    public void RootDict_02()
    {
      using var w = new BixWriterBufferScope(1024);
      var obj = new Dictionary<object, object> { { 1, 2 }, { 29, "twenty nine" }, { -900, Atom.Encode("mmm") } };
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as JsonDataMap;
      Aver.IsNotNull(got);
      Aver.IsTrue(got.CaseSensitive);
      Aver.AreEqual(3, got.Count);
      Aver.AreEqual(2, (int)got["1"]);
      Aver.AreEqual("twenty nine", (string)got["29"]);
      Aver.AreEqual(Atom.Encode("mmm"), (Atom)got["-900"]);
    }

    [Run]
    public void RootDict_03()
    {
      using var w = new BixWriterBufferScope(1024);
      var obj = new JsonDataMap(caseSensitive: false){ { "1", 2 }, { "29", "twenty nine" }, { "-900", Atom.Encode("mmm") } };
      Bixon.WriteObject(w.Writer, obj);
      w.Buffer.ToHexDump().See();
      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as JsonDataMap;
      Aver.IsNotNull(got);
      Aver.IsFalse(got.CaseSensitive);
      Aver.AreEqual(3, got.Count);
      Aver.AreEqual(2, (int)got["1"]);
      Aver.AreEqual("twenty nine", (string)got["29"]);
      Aver.AreEqual(Atom.Encode("mmm"), (Atom)got["-900"]);
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
      Aver.AreEqual(12, got.Count);//11+bix
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

    [Run]
    public void RootDocComplex_01()
    {
      using var w = new BixWriterBufferScope(1024);
      var doc = new bxonBaseDoc()
      {
        String1 = "Fidel Castro",
        Int1 = 123,
        NInt1 = -678_000_000,
        Obj1 = "String text of no sense",
        Jdm1 = new JsonDataMap{ {"a", 1}, {"b", 2}, {"zzz", new bxonBDoc{ Atom1 = Atom.Encode("titikaka")}} },
        Jar1 = new JsonDataArray { 1, 2, 5, 7, 9, 0, new { d=true, f=-500}, null, new GDID(1,2)},
        ObjArr1 = new object[]
        {
          null,
          true,
          7890m,
          new DateTime(1990, 5, 2, 14, 32, 00, DateTimeKind.Utc),
          new bxonADoc() { String1 = "Odessa mama", String2 = "Salsa Mexicana" },
          new bxonBDoc() { String1 = "Corn kebab", Flag1 = true, Obj2 = new byte[]{255,255, 0, 1,2,3,4,5,6,7,8,9,0,127,128,129,130,131,132,133,134} },
        }
      };

      Bixon.WriteObject(w.Writer, doc, MARSHALLED); //marshall doc type identity
      w.Buffer.ToHexDump().See();

      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as bxonBaseDoc;
      got.See(WITH_TYPES);
      Aver.IsNotNull(got);
      Aver.AreEqual(doc.String1, got.String1);
      Aver.AreEqual(doc.Int1, got.Int1);
      Aver.AreEqual(doc.NInt1, got.NInt1);

      Aver.IsNotNull(got.Jdm1);
      Aver.AreEqual(3, got.Jdm1.Count);
      Aver.AreEqual(1, (int)got.Jdm1["a"]);
      Aver.AreEqual(2, (int)got.Jdm1["b"]);
      Aver.AreEqual("titikaka", ((bxonBDoc)got.Jdm1["zzz"]).Atom1.Value);

      Aver.IsNotNull(got.Jar1);
      Aver.AreEqual(9, got.Jar1.Count);
      Aver.AreEqual(5, (int)got.Jar1[2]);
      Aver.AreEqual(new GDID(1, 2), (GDID)got.Jar1[8]);


      Aver.IsNotNull(got.ObjArr1);
      Aver.AreEqual(6, got.ObjArr1.Length);
      Aver.IsNull(got.ObjArr1[0]);
      Aver.AreEqual(true, (bool)got.ObjArr1[1]);
      Aver.AreEqual(7890m, (decimal)got.ObjArr1[2]);
      Aver.AreEqual(1990, ((DateTime)got.ObjArr1[3]).Year);
      Aver.AreEqual("Odessa mama", ((bxonADoc)got.ObjArr1[4]).String1);
      Aver.AreEqual("Salsa Mexicana", ((bxonADoc)got.ObjArr1[4]).String2);
      Aver.AreEqual("Corn kebab", ((bxonBDoc)got.ObjArr1[5]).String1);
      Aver.AreEqual(true, ((bxonBDoc)got.ObjArr1[5]).Flag1);
      Aver.IsTrue(((byte[])((bxonBDoc)doc.ObjArr1[5]).Obj2).MemBufferEquals((byte[])((bxonBDoc)got.ObjArr1[5]).Obj2));

      Aver.AreEqual("String text of no sense", (string)got.Obj1);
    }

    [Run]
    public void RootDocComplex_02_NoMaterialize()
    {
      using var w = new BixWriterBufferScope(1024);
      var doc = new bxonBaseDoc()
      {
        String1 = "Fidel Castro",
        Int1 = 123,
        NInt1 = -678_000_000,
        Obj1 = "String text of no sense",
        Jdm1 = new JsonDataMap { { "a", 1 }, { "b", 2 }, { "zzz", new bxonBDoc { Atom1 = Atom.Encode("titikaka") } } },
        Jar1 = new JsonDataArray { 1, 2, 5, 7, 9, 0, new { d = true, f = -500 }, null, new GDID(1, 2) },
        ObjArr1 = new object[]
        {
          null,
          true,
          7890m,
          new DateTime(1990, 5, 2, 14, 32, 00, DateTimeKind.Utc),
          new bxonADoc() { String1 = "Odessa mama", String2 = "Salsa Mexicana" },
          new bxonBDoc() { String1 = "Corn kebab", Flag1 = true, Obj2 = new byte[]{255,255, 0, 1,2,3,4,5,6,7,8,9,0,127,128,129,130,131,132,133,134} },
        }
      };

      Bixon.WriteObject(w.Writer, doc, MARSHALLED); //marshall doc type identity, however down below we DO NOT materialize docs, but keep JsonDataMaps
      w.Buffer.ToHexDump().See();

      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader, new JsonReader.DocReadOptions(JsonReader.DocReadOptions.By.BixonDoNotMaterializeDocuments, null)) as JsonDataMap;
      got.See(WITH_TYPES);
      Aver.IsNotNull(got);
      Aver.AreEqual(doc.String1, (string)got["String1"]);
      Aver.AreEqual(doc.Int1, (int)got["Int1"]);
      Aver.AreEqual(doc.NInt1, (int)got["NInt1"]);

      Aver.IsNotNull(got["Jdm1"]);
      Aver.AreEqual(3, ((JsonDataMap)got["Jdm1"]).Count);
      Aver.AreEqual(1, (int)((JsonDataMap)got["Jdm1"])["a"]);
      Aver.AreEqual(2, (int)((JsonDataMap)got["Jdm1"])["b"]);
      Aver.AreEqual("titikaka", ((Atom)((JsonDataMap)((JsonDataMap)got["Jdm1"])["zzz"])["Atom1"]).Value);

      Aver.IsNotNull(got["Jar1"]);
      Aver.AreEqual(9, ((JsonDataArray)got["Jar1"]).Count);
      Aver.AreEqual(5, (int)((JsonDataArray)got["Jar1"])[2]);
      Aver.AreEqual(new GDID(1, 2), (GDID)((JsonDataArray)got["Jar1"])[8]);


      Aver.IsNotNull(got["ObjArr1"]);
      Aver.AreEqual(6, ((JsonDataArray)got["ObjArr1"]).Count);
      Aver.IsNull(((JsonDataArray)got["ObjArr1"])[0]);
      Aver.AreEqual(true, (bool)((JsonDataArray)got["ObjArr1"])[1]);
      Aver.AreEqual(7890m, (decimal)((JsonDataArray)got["ObjArr1"])[2]);
      Aver.AreEqual(1990, ((DateTime)((JsonDataArray)got["ObjArr1"])[3]).Year);
      Aver.AreEqual("Odessa mama", (string)((JsonDataMap)((JsonDataArray)got["ObjArr1"])[4])["String1"]);
      Aver.AreEqual("Salsa Mexicana", (string)((JsonDataMap)((JsonDataArray)got["ObjArr1"])[4])["String2"]);
      Aver.AreEqual("Corn kebab", (string)((JsonDataMap)((JsonDataArray)got["ObjArr1"])[5])["String1"]);
      Aver.AreEqual(true, (bool)((JsonDataMap)((JsonDataArray)got["ObjArr1"])[5])["Flag1"]);
      Aver.IsTrue(((byte[])((bxonBDoc)doc.ObjArr1[5])["Obj2"]).MemBufferEquals((byte[])((JsonDataMap)((JsonDataArray)got["ObjArr1"])[5])["Obj2"]));

      Aver.AreEqual("String text of no sense", (string)got["Obj1"]);
    }





    /*
2023 May 12 .Net 6 Release
--------------------------
Complex:
  JSON wrote 32,000 in 1.0 sec at 31,409 ops/sec; 1,590 chars
  JSON read 32,000 in 2.6 sec at 12,312 ops/sec
  BIXON wrote 32,000 in 0.9 sec at 35,354 ops/sec; 1,201 bytes
  BIXON read 32,000 in 0.7 sec at 42,765 ops/sec

Simple:
  JSON wrote 250,000 in 0.8 sec at 308,273 ops/sec; 307 chars
  JSON read 250,000 in 2.3 sec at 110,971 ops/sec
  BIXON wrote 250,000 in 0.9 sec at 272,497 ops/sec; 265 bytes
  BIXON read 250,000 in 0.5 sec at 516,437 ops/sec
    */

    /*
2023 June 19 .Net 6 Release
--------------------------
Complex:
JSON wrote 32,000 in 1.1 sec at 30,307 ops/sec; 1,590 chars
JSON read 32,000 in 2.6 sec at 12,445 ops/sec
BIXON wrote 32,000 in 0.9 sec at 34,575 ops/sec; 1,201 bytes
BIXON read 32,000 in 0.7 sec at 45,064 ops/sec

Simple:
JSON wrote 250,000 in 0.8 sec at 333,280 ops/sec; 307 chars
JSON read 250,000 in 2.3 sec at 110,569 ops/sec
BIXON wrote 250,000 in 0.9 sec at 268,040 ops/sec; 265 bytes
BIXON read 250,000 in 0.5 sec at 549,256 ops/sec
    */



   // [Run("count=32000")]
    [Run("count=5000")]
    public void Benchmark_Complex(int count)
    {

      var doc = new bxonBaseDoc()
      {
        String1 = "Fidel Castro",
        Int1 = 123,
        NInt1 = -678_000_000,
        Atom1 = Atom.Encode("a1234567"),
        Obj1 = new byte[] {1,1,1,1,1,1,1,1,1,1,1,2,3,4,3,12,3,2,4,23,4,3,3,3,5,10,200,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
        Jdm1 = new JsonDataMap { { "a", 1 }, { "b", 2 }, { "zzz", new bxonBDoc { Atom1 = Atom.Encode("abcd") } } },
        Jar1 = new JsonDataArray { Atom.Encode("a1"), new{a = Atom.Encode("a2") }, new{z=1}, 0, new{c=true},0,0,0,null,null,1, 2, 5, 7, Atom.Encode("a1234"), 0, new { d = true, f = -500 }, null, new GDID(1, 2) },
        ObjArr1 = new object[]
        {
          null,
          true,
          7890m,
          new DateTime(1990, 5, 2, 14, 32, 00, DateTimeKind.Utc),
          new bxonADoc() { String1 = "Odessa mama", String2 = "Salsa Mexicana" },
          new bxonBDoc() { String1 = "Corn kebab", Flag1 = true, Obj2 = new byte[]{255,255, 0, 1,2,3,4,5,6,7,8,9,0,127,128,129,130,131,132,133,134} },
          new bxonBDoc() { String1 = "sdferert ertert", Flag1 = false, Obj2 = new byte[]{1,0,3} },
          new bxonBDoc() { String1 = "student of choice", Flag1 = true, Obj2 = new byte[]{0,0,0,0,0,0,0,0,255,0,0,0,0,0,0,0,0,254} },
        }
      };

      //Warmup
      var jwo = new JsonWritingOptions(JsonWritingOptions.CompactRowsAsMap){ EnableTypeHints = true, Purpose = JsonSerializationPurpose.Marshalling};
      var json = doc.ToJson(jwo);
      var gotFromJson = JsonReader.ToDoc<bxonBaseDoc>(json, false);
      Aver.IsNotNull(gotFromJson);

      using var w = new BixWriterBufferScope(1024);
      Bixon.WriteObject(w.Writer, doc, jwo); //marshall doc type identity
      w.Buffer.ToHexDump().See();

      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as bxonBaseDoc;
      got.See(WITH_TYPES);

      var jsonTime = Timeter.StartNew();
      for(var i=0; i<count; i++)
      {
        json = doc.ToJson(jwo);
      }
      jsonTime.Stop();
      "JSON wrote {0:n0} in {1:n1} sec at {2:n0} ops/sec; {3:n0} chars".SeeArgs(count, jsonTime.ElapsedSec, count / jsonTime.ElapsedSec, json.Length);

      jsonTime = Timeter.StartNew();
      for (var i = 0; i < count; i++)
      {
        gotFromJson = JsonReader.ToDoc<bxonBaseDoc>(json, false);
      }
      jsonTime.Stop();
      "JSON read {0:n0} in {1:n1} sec at {2:n0} ops/sec".SeeArgs(count, jsonTime.ElapsedSec, count / jsonTime.ElapsedSec);


      var bixonTime = Timeter.StartNew();
      for (var i = 0; i < count; i++)
      {
        w.Reset();
        Bixon.WriteObject(w.Writer, doc, jwo); //marshall doc type identity
      }
      bixonTime.Stop();
      "BIXON wrote {0:n0} in {1:n1} sec at {2:n0} ops/sec; {3:n0} bytes".SeeArgs(count, bixonTime.ElapsedSec, count / bixonTime.ElapsedSec, w.Buffer.Length);

      bixonTime = Timeter.StartNew();
      for (var i = 0; i < count; i++)
      {
        r.Reset();
        got = Bixon.ReadObject(r.Reader) as bxonBaseDoc;
      }
      bixonTime.Stop();
      "BIXON read {0:n0} in {1:n1} sec at {2:n0} ops/sec".SeeArgs(count, bixonTime.ElapsedSec, count / bixonTime.ElapsedSec); ;

    }

   //  [Run("count=250000")]
    [Run("count=8000")]
    public void Benchmark_Simple(int count)
    {
      var doc = new
      {
        String1 = "Fidel Castro",
        Int1 = 123,
        NInt1 = -678_000_000,
        Atom1 = Atom.Encode("a1234567"),
        ArchiveDims = new JsonDataMap
        {
          {"rel", new EntityId(Atom.Encode("entrp"), Atom.Encode("comp"), Atom.Encode("mnic"), "company mnemonic")},
          {"cust", new EntityId(Atom.Encode("cust"), Atom.Encode("acc"), Atom.Encode("gdid"), "0:9:23")},
          {"prod", new EntityId(Atom.Encode("prod"), Atom.Encode("tree"), Atom.Encode("path"), "/parm/rx/antibiotic/pnc/doxicillin")}
        },

        Params = new JsonDataMap
        {
          {"q", 123},
          {"f", true},
          {"site", "hudson"},
          {"apr", -123.02m},
        }

      };

      //Warmup
      var jwo = new JsonWritingOptions(JsonWritingOptions.CompactRowsAsMap) { EnableTypeHints = true, Purpose = JsonSerializationPurpose.Marshalling };
      var jro = new JsonReadingOptions(JsonReadingOptions.Default) { EnableTypeHints = true };
      var json = doc.ToJson(jwo);
      var gotFromJson = JsonReader.Deserialize(json, jro);
      Aver.IsNotNull(gotFromJson);
      gotFromJson.See(WITH_TYPES);

      using var w = new BixWriterBufferScope(1024);
      Bixon.WriteObject(w.Writer, doc, jwo); //marshall doc type identity
      w.Buffer.ToHexDump().See();

      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader);
      Aver.IsNotNull(got);
      got.See(WITH_TYPES);

      var jsonTime = Timeter.StartNew();
      for (var i = 0; i < count; i++)
      {
        json = doc.ToJson(jwo);
      }
      jsonTime.Stop();
      "JSON wrote {0:n0} in {1:n1} sec at {2:n0} ops/sec; {3:n0} chars".SeeArgs(count, jsonTime.ElapsedSec, count / jsonTime.ElapsedSec, json.Length);

      jsonTime = Timeter.StartNew();
      for (var i = 0; i < count; i++)
      {
        gotFromJson = JsonReader.Deserialize(json, jro);
        Aver.IsNotNull(gotFromJson);
      }
      jsonTime.Stop();
      "JSON read {0:n0} in {1:n1} sec at {2:n0} ops/sec".SeeArgs(count, jsonTime.ElapsedSec, count / jsonTime.ElapsedSec);


      var bixonTime = Timeter.StartNew();
      for (var i = 0; i < count; i++)
      {
        w.Reset();
        Bixon.WriteObject(w.Writer, doc, jwo); //marshall doc type identity
      }
      bixonTime.Stop();
      "BIXON wrote {0:n0} in {1:n1} sec at {2:n0} ops/sec; {3:n0} bytes".SeeArgs(count, bixonTime.ElapsedSec, count / bixonTime.ElapsedSec, w.Buffer.Length);

      bixonTime = Timeter.StartNew();
      for (var i = 0; i < count; i++)
      {
        r.Reset();
        got = Bixon.ReadObject(r.Reader);
        Aver.IsNotNull(got);
      }
      bixonTime.Stop();
      "BIXON read {0:n0} in {1:n1} sec at {2:n0} ops/sec".SeeArgs(count, bixonTime.ElapsedSec, count / bixonTime.ElapsedSec); ;
    }

    [Run]
    public void LogMessageArray()
    {
      var data = new {OK = true, data = /*(IEnumerable<object>)*/new Message[]
      {
         new Message{ Guid = Guid.NewGuid(), Type = MessageType.DebugA, Topic = "123", From = "from1",  Text = "Text1" } ,
         new Message{ Guid = Guid.NewGuid(), Type = MessageType.DebugD, Topic = "321", From = "from2",  Text = "Text2" } ,
         new Message{ Guid = Guid.NewGuid(), Type = MessageType.TraceA, Topic = "topic_1", From = "from3",  Text = "Text3" } ,
      }
      };


      using var w = new BixWriterBufferScope(1024);
      Bixon.WriteObject(w.Writer, data);
      w.Buffer.ToHexDump().See();

      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as JsonDataMap;

      got.See();
      Aver.IsNotNull(got);

    }

    [Run]
    public void LogFactArray()
    {
      var data = new
      {
        OK = true,
        data = (IEnumerable<Fact>) new Fact[]
        {
           new Fact{ Id = Guid.NewGuid(), RecordType = MessageType.DebugA, Topic = Atom.Encode("t1"), FactType = Atom.Encode("f1"), Source = 1 } ,
           new Fact{ Id = Guid.NewGuid(), RecordType = MessageType.DebugD, Topic = Atom.Encode("t2"), FactType = Atom.Encode("f2"), Source = 2 } ,
           new Fact{ Id = Guid.NewGuid(), RecordType = MessageType.TraceA, Topic = Atom.Encode("t3"), FactType = Atom.Encode("f3"), Source = 3 } ,
        }.OrderBy(one => one.UtcTimestamp)
      };


      using var w = new BixWriterBufferScope(1024);
      Bixon.WriteObject(w.Writer, data);
      w.Buffer.ToHexDump().See();

      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as JsonDataMap;

      got.See();
      Aver.IsNotNull(got);

    }


    //Re: #921 JPK, DKh 20241013
    [Run]
    public void RootDocWithNlsMap_01()
    {
      using var w = new BixWriterBufferScope(1024);
      var doc = new bxonBaseDoc()
      {
        String1 = "Mark Twain",
        Int1 = 123,
        NInt1 = 678,
        Nls1 = new NLSMap("{ eng:{ n:'yes', d:'confirmation'} }"),
        Nls2 = null
      };

      Bixon.WriteObject(w.Writer, doc, MARSHALLED); //marshall doc type identity
      w.Buffer.ToHexDump().See();

      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as bxonBaseDoc;
      doc.See(WITH_TYPES);
      got.See(WITH_TYPES);
      Aver.IsNotNull(got);
      Aver.AreEqual("Mark Twain", got.String1);
      Aver.AreEqual("yes", got.Nls1[CoreConsts.ISO_LANG_ENGLISH].Name);
      Aver.IsNull(got.Nls2);
    }

    //Re: #921 JPK, DKh 20241013
    [Run]
    public void RootDocWithNlsMap_02()
    {
      using var w = new BixWriterBufferScope(1024);
      var doc = new bxonBaseDoc()
      {
        String1 = "Mark Twain",
        Int1 = 123,
        NInt1 = 678,
        Nls1 = new NLSMap("{ eng:{ n:'yes', d:'confirmation'} }"),
        Nls2 = new NLSMap("{ deu:{ n:'yah', d:'das ist good'} }")
      };

      Bixon.WriteObject(w.Writer, doc, MARSHALLED); //marshall doc type identity
      w.Buffer.ToHexDump().See();

      using var r = new BixReaderBufferScope(w.Buffer);
      var got = Bixon.ReadObject(r.Reader) as bxonBaseDoc;
      doc.See(WITH_TYPES);
      got.See(WITH_TYPES);
      Aver.IsNotNull(got);
      Aver.AreEqual("Mark Twain", got.String1);
      Aver.AreEqual("yes", got.Nls1[CoreConsts.ISO_LANG_ENGLISH].Name);
      Aver.IsNotNull(got.Nls2);
      Aver.AreEqual("yah", got.Nls2.Value[CoreConsts.ISO_LANG_GERMAN].Name);
      Aver.AreEqual("das ist good", got.Nls2.Value[CoreConsts.ISO_LANG_GERMAN].Description);
    }

  }
}