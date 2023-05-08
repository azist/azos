/*<FILE_LICENSE>
* Azos (A to Z Application Operating System) Framework
* The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
* See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Conf;
using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JsonTypeHintTests
  {
    [Run]
    public void AnonymousObject_1()
    {
      var obj = new
      {
        atom = Atom.Encode("abc"),
        eid = new EntityId(Atom.Encode("sys"), Atom.Encode("type"), Atom.Encode("Schema"), "address1"),
        gdid = new GDID(123, 456789),
        rgdid = new RGDID(456, new GDID(321, 98765)),
        guid = Guid.NewGuid(),
        dt = new DateTime(1980, 1, 2, 12, 30, 00,DateTimeKind.Utc),
        ts = TimeSpan.FromSeconds(4567),
        bin = new byte[]{1,1,2,2,3,4,5,6,7,8,9,0,12,13,14,1,5,16,17,18,255,129,100,0,0,0},
        str1 = "plain string",
        str2 = "$ZZZ:needs escape"
      };

      var plainJson = obj.ToJson(JsonWritingOptions.PrettyPrint);
      var hintedJson = obj.ToJson(new JsonWritingOptions(JsonWritingOptions.PrettyPrint) { EnableTypeHints = true });

      "\nPlain json: \n{0}\n\n vs Hinted json: \n{1}".SeeArgs(plainJson, hintedJson);

      var gotPlainDisabled = plainJson.JsonToDataObject(JsonReadingOptions.NoLimits) as JsonDataMap;
      var gotPlainEnabled = plainJson.JsonToDataObject(new JsonReadingOptions(JsonReadingOptions.NoLimits){ EnableTypeHints = true }) as JsonDataMap;

      var gotHintedDisabled = hintedJson.JsonToDataObject(JsonReadingOptions.NoLimits) as JsonDataMap;
      var gotHintedEnabled = hintedJson.JsonToDataObject(new JsonReadingOptions(JsonReadingOptions.NoLimits) { EnableTypeHints = true }) as JsonDataMap;

      Aver.IsNotNull(gotPlainDisabled);
      Aver.IsNotNull(gotPlainEnabled);
      Aver.IsNotNull(gotHintedDisabled);
      Aver.IsNotNull(gotHintedEnabled);
      Aver.AreEqual(10, gotPlainDisabled.Count);
      Aver.AreEqual(10, gotPlainEnabled.Count);
      Aver.AreEqual(10, gotHintedDisabled.Count);
      Aver.AreEqual(10, gotHintedEnabled.Count);

      gotHintedDisabled.See();
      gotHintedEnabled.See();


      Aver.IsTrue(gotPlainDisabled["atom"] is string);
      Aver.IsTrue(gotPlainEnabled["atom"] is string);
      Aver.IsTrue(gotHintedDisabled["atom"] is string);
      Aver.IsTrue(gotHintedEnabled["atom"] is Atom);
      Aver.AreEqual(obj.atom, (Atom)gotHintedEnabled["atom"]);

      Aver.IsTrue(gotPlainDisabled["bin"] is string);
      Aver.IsTrue(gotPlainEnabled["bin"] is string);
      Aver.IsTrue(gotHintedDisabled["bin"] is string);
      Aver.IsTrue(gotHintedEnabled["bin"] is byte[]);
      Aver.IsTrue(obj.bin.MemBufferEquals((byte[])gotHintedEnabled["bin"]));

      Aver.IsTrue(gotPlainDisabled["dt"] is string);
      Aver.IsTrue(gotPlainEnabled["dt"] is string);
      Aver.IsTrue(gotHintedDisabled["dt"] is string);
      Aver.IsTrue(gotHintedEnabled["dt"] is DateTime);
      Aver.AreEqual(obj.dt, (DateTime)gotHintedEnabled["dt"]);

      Aver.IsTrue(gotPlainDisabled["eid"] is string);
      Aver.IsTrue(gotPlainEnabled["eid"] is string);
      Aver.IsTrue(gotHintedDisabled["eid"] is string);
      Aver.IsTrue(gotHintedEnabled["eid"] is EntityId);
      Aver.AreEqual(obj.eid, (EntityId)gotHintedEnabled["eid"]);

      Aver.IsTrue(gotPlainDisabled["gdid"] is string);
      Aver.IsTrue(gotPlainEnabled["gdid"] is string);
      Aver.IsTrue(gotHintedDisabled["gdid"] is string);
      Aver.IsTrue(gotHintedEnabled["gdid"] is GDID);
      Aver.AreEqual(obj.gdid, (GDID)gotHintedEnabled["gdid"]);

      Aver.IsTrue(gotPlainDisabled["rgdid"] is string);
      Aver.IsTrue(gotPlainEnabled["rgdid"] is string);
      Aver.IsTrue(gotHintedDisabled["rgdid"] is string);
      Aver.IsTrue(gotHintedEnabled["rgdid"] is RGDID);
      Aver.AreEqual(obj.rgdid, (RGDID)gotHintedEnabled["rgdid"]);

      Aver.IsTrue(gotPlainDisabled["guid"] is string);
      Aver.IsTrue(gotPlainEnabled["guid"] is string);
      Aver.IsTrue(gotHintedDisabled["guid"] is string);
      Aver.IsTrue(gotHintedEnabled["guid"] is Guid);
      Aver.AreEqual(obj.guid, (Guid)gotHintedEnabled["guid"]);

      Aver.IsTrue(gotPlainDisabled["ts"] is string);
      Aver.IsTrue(gotPlainEnabled["ts"] is string);
      Aver.IsTrue(gotHintedDisabled["ts"] is string);
      Aver.IsTrue(gotHintedEnabled["ts"] is TimeSpan);
      Aver.AreEqual(obj.ts, (TimeSpan)gotHintedEnabled["ts"]);

      Aver.IsTrue(gotPlainDisabled["str1"] is string);
      Aver.IsTrue(gotPlainEnabled["str1"] is string);
      Aver.IsTrue(gotHintedDisabled["str1"] is string);
      Aver.IsTrue(gotHintedEnabled["str1"] is string);
      Aver.AreEqual(obj.str1, (string)gotHintedEnabled["str1"]);

      Aver.IsTrue(gotPlainDisabled["str2"] is string);
      Aver.IsTrue(gotPlainEnabled["str2"] is string);
      Aver.IsTrue(gotHintedDisabled["str2"] is string);
      Aver.IsTrue(gotHintedEnabled["str2"] is string);
      Aver.AreEqual(obj.str2, (string)gotHintedEnabled["str2"]);

    }

    [Run]
    public async Task AnonymousObject_async_2()
    {
      var obj = new
      {
        atom = Atom.Encode("abc"),
        eid = new EntityId(Atom.Encode("sys"), Atom.Encode("type"), Atom.Encode("Schema"), "address1"),
        gdid = new GDID(123, 456789),
        rgdid = new RGDID(456, new GDID(321, 98765)),
        guid = Guid.NewGuid(),
        dt = new DateTime(1980, 1, 2, 12, 30, 00, DateTimeKind.Utc),
        ts = TimeSpan.FromSeconds(4567),
        bin = new byte[] { 1, 1, 2, 2, 3, 4, 5, 6, 7, 8, 9, 0, 12, 13, 14, 1, 5, 16, 17, 18, 255, 129, 100, 0, 0, 0 },
        str1 = "plain string",
        str2 = "$ZZZ:needs escape"
      };

      var plainJson =obj.ToJson(JsonWritingOptions.PrettyPrint);
      var hintedJson = obj.ToJson(new JsonWritingOptions(JsonWritingOptions.PrettyPrint) { EnableTypeHints = true });

      "\nPlain json: \n{0}\n\n vs Hinted json: \n{1}".SeeArgs(plainJson, hintedJson);
      using var plainJsonStream = Azos.IO.StreamHookUse.CaseOfRandomAsyncStringReading(plainJson, 1, 50, 1, 10000);
      using var hintedJsonStream = Azos.IO.StreamHookUse.CaseOfRandomAsyncStringReading(hintedJson, 1, 50, 1, 10000);


      plainJsonStream.Position = 0;
      var gotPlainDisabled = await plainJsonStream.JsonToObjectAsync(ropt: JsonReadingOptions.NoLimits) as JsonDataMap;
      plainJsonStream.Position = 0;
      var gotPlainEnabled = await plainJsonStream.JsonToObjectAsync(ropt: new JsonReadingOptions(JsonReadingOptions.NoLimits) { EnableTypeHints = true }) as JsonDataMap;

      hintedJsonStream.Position = 0;
      var gotHintedDisabled = await hintedJsonStream.JsonToObjectAsync(ropt: JsonReadingOptions.NoLimits) as JsonDataMap;
      hintedJsonStream.Position = 0;
      var gotHintedEnabled = await hintedJsonStream.JsonToObjectAsync(ropt: new JsonReadingOptions(JsonReadingOptions.NoLimits) { EnableTypeHints = true }) as JsonDataMap;

      Aver.IsNotNull(gotPlainDisabled);
      Aver.IsNotNull(gotPlainEnabled);
      Aver.IsNotNull(gotHintedDisabled);
      Aver.IsNotNull(gotHintedEnabled);
      Aver.AreEqual(10, gotPlainDisabled.Count);
      Aver.AreEqual(10, gotPlainEnabled.Count);
      Aver.AreEqual(10, gotHintedDisabled.Count);
      Aver.AreEqual(10, gotHintedEnabled.Count);

      gotHintedDisabled.See();
      gotHintedEnabled.See();


      Aver.IsTrue(gotPlainDisabled["atom"] is string);
      Aver.IsTrue(gotPlainEnabled["atom"] is string);
      Aver.IsTrue(gotHintedDisabled["atom"] is string);
      Aver.IsTrue(gotHintedEnabled["atom"] is Atom);
      Aver.AreEqual(obj.atom, (Atom)gotHintedEnabled["atom"]);

      Aver.IsTrue(gotPlainDisabled["bin"] is string);
      Aver.IsTrue(gotPlainEnabled["bin"] is string);
      Aver.IsTrue(gotHintedDisabled["bin"] is string);
      Aver.IsTrue(gotHintedEnabled["bin"] is byte[]);
      Aver.IsTrue(obj.bin.MemBufferEquals((byte[])gotHintedEnabled["bin"]));

      Aver.IsTrue(gotPlainDisabled["dt"] is string);
      Aver.IsTrue(gotPlainEnabled["dt"] is string);
      Aver.IsTrue(gotHintedDisabled["dt"] is string);
      Aver.IsTrue(gotHintedEnabled["dt"] is DateTime);
      Aver.AreEqual(obj.dt, (DateTime)gotHintedEnabled["dt"]);

      Aver.IsTrue(gotPlainDisabled["eid"] is string);
      Aver.IsTrue(gotPlainEnabled["eid"] is string);
      Aver.IsTrue(gotHintedDisabled["eid"] is string);
      Aver.IsTrue(gotHintedEnabled["eid"] is EntityId);
      Aver.AreEqual(obj.eid, (EntityId)gotHintedEnabled["eid"]);

      Aver.IsTrue(gotPlainDisabled["gdid"] is string);
      Aver.IsTrue(gotPlainEnabled["gdid"] is string);
      Aver.IsTrue(gotHintedDisabled["gdid"] is string);
      Aver.IsTrue(gotHintedEnabled["gdid"] is GDID);
      Aver.AreEqual(obj.gdid, (GDID)gotHintedEnabled["gdid"]);

      Aver.IsTrue(gotPlainDisabled["rgdid"] is string);
      Aver.IsTrue(gotPlainEnabled["rgdid"] is string);
      Aver.IsTrue(gotHintedDisabled["rgdid"] is string);
      Aver.IsTrue(gotHintedEnabled["rgdid"] is RGDID);
      Aver.AreEqual(obj.rgdid, (RGDID)gotHintedEnabled["rgdid"]);

      Aver.IsTrue(gotPlainDisabled["guid"] is string);
      Aver.IsTrue(gotPlainEnabled["guid"] is string);
      Aver.IsTrue(gotHintedDisabled["guid"] is string);
      Aver.IsTrue(gotHintedEnabled["guid"] is Guid);
      Aver.AreEqual(obj.guid, (Guid)gotHintedEnabled["guid"]);

      Aver.IsTrue(gotPlainDisabled["ts"] is string);
      Aver.IsTrue(gotPlainEnabled["ts"] is string);
      Aver.IsTrue(gotHintedDisabled["ts"] is string);
      Aver.IsTrue(gotHintedEnabled["ts"] is TimeSpan);
      Aver.AreEqual(obj.ts, (TimeSpan)gotHintedEnabled["ts"]);

      Aver.IsTrue(gotPlainDisabled["str1"] is string);
      Aver.IsTrue(gotPlainEnabled["str1"] is string);
      Aver.IsTrue(gotHintedDisabled["str1"] is string);
      Aver.IsTrue(gotHintedEnabled["str1"] is string);
      Aver.AreEqual(obj.str1, (string)gotHintedEnabled["str1"]);

      Aver.IsTrue(gotPlainDisabled["str2"] is string);
      Aver.IsTrue(gotPlainEnabled["str2"] is string);
      Aver.IsTrue(gotHintedDisabled["str2"] is string);
      Aver.IsTrue(gotHintedEnabled["str2"] is string);
      Aver.AreEqual(obj.str2, (string)gotHintedEnabled["str2"]);

    }


  }
}
