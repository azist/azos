/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Data;
using Azos.Log;
using Azos.Scripting;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Logging
{
  [Runnable]
  public class ArchiveConventionsTests
  {
    [Run]
    public void Ad_Test01()
    {
      var got = ArchiveConventions.DecodeArchiveDimensionsMap((string)null);
      Aver.IsNull(got);

      got = ArchiveConventions.DecodeArchiveDimensionsMap("       ");
      Aver.IsNull(got);

      got = ArchiveConventions.DecodeArchiveDimensionsMap((IArchiveLoggable)null);
      Aver.IsNull(got);

      got = ArchiveConventions.DecodeArchiveDimensionsMap("not a content produced by convention");
      Aver.IsNull(got);
    }

    [Run]
    public void Ad_Test02()
    {
      var encoded = ArchiveConventions.EncodeArchiveDimensions( new { a = 1, b = 3 });
      encoded.See();

      var decoded = ArchiveConventions.DecodeArchiveDimensionsMap(encoded);
      Aver.IsNotNull(decoded);
      Aver.AreEqual(2, decoded.Count);

      Aver.AreObjectsEqual(1, decoded["a"]);
      Aver.AreObjectsEqual(3, decoded["b"]);
    }

    [Run]
    public void Ad_Test03()
    {
      var encoded1 = ArchiveConventions.EncodeArchiveDimensions(new { a = 1, b = 3 });
      var encoded2 = ArchiveConventions.EncodeArchiveDimensions(new { b = 3, a = 1, c = (string)null });//notice a different sequence of keys
      encoded1.See();
      encoded2.See();
      Aver.AreEqual(encoded1, encoded2);//however the strings are equal, because keys are sorted and nulls are skipped

      var decoded = ArchiveConventions.DecodeArchiveDimensionsMap(encoded1);
      Aver.IsNotNull(decoded);
      Aver.AreEqual(2, decoded.Count);

      Aver.AreObjectsEqual(1, decoded["a"]);
      Aver.AreObjectsEqual(3, decoded["b"]);
    }

    [Run]
    public void SD_Test01()
    {
      var got = ArchiveConventions.DecodeStructuredDataMap((string)null);
      Aver.IsNull(got);

      got = ArchiveConventions.DecodeStructuredDataMap("       ");
      Aver.IsNull(got);

      got = ArchiveConventions.DecodeStructuredDataMap("not a content produced by convention");
      Aver.IsNull(got);
    }

    [Run]
    public void SD_Test02()
    {
      var encoded = ArchiveConventions.EncodeStructuredData(new { a = 1, b = 3 });
      encoded.See();

      var decoded = ArchiveConventions.DecodeStructuredDataMap(encoded);
      Aver.IsNotNull(decoded);
      Aver.AreEqual(2, decoded.Count);

      Aver.AreObjectsEqual(1, decoded["a"]);
      Aver.AreObjectsEqual(3, decoded["b"]);
    }

    [Run]
    public void SD_RoundtripAllTypes()
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

        {"sub", new JsonDataMap(){ {"a", 12345}, {"b", null} }},

        {"arr", new object[]{ 1, 2, true, false, "ok", 345, Atom.Encode("zxy")}},
      };
      ArchiveConventions.WriteArchivedDataMap(w.Writer, map);

      w.Buffer.ToHexDump().See();

      using var r = new BixReaderBufferScope(w.Buffer);
      var got = ArchiveConventions.ReadArchivedDataMap(r.Reader);

      got.See(new JsonWritingOptions(JsonWritingOptions.PrettyPrintRowsAsMap){ EnableTypeHints = true });
      averMapsEqual(map, got);
    }


    private static void averMapsEqual(JsonDataMap map1, JsonDataMap map2)
    {
      Aver.AreEqual(map1.Count, map2.Count);

      foreach (var kvp in map1)
      {
        if (kvp.Value is JsonDataMap map)
          averMapsEqual(map, (JsonDataMap)map2[kvp.Key]);
        else if (kvp.Value is byte[] buf)
          Aver.IsTrue(buf.MemBufferEquals((byte[])map2[kvp.Key]));
        else
          Aver.AreObjectsEqual(kvp.Value, map2[kvp.Key]);
      }
    }

  }
}