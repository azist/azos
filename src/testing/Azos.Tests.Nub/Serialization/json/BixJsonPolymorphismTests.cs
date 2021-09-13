/*<FILE_LICENSE>
* Azos (A to Z Application Operating System) Framework
* The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
* See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Reflection;

using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class BixJsonPolymorphismTests
  {
    public BixJsonPolymorphismTests()
    {
      Bixer.RegisterTypeSerializationCores(Assembly.GetExecutingAssembly());
      ///// Bixer.GuidTypeResolver.Resolve(new Guid("F96B12F7-F922-49FA-BBA3-51135D03BC7E")).See();
    }


    [BixJsonHandler(ThrowOnUnresolvedType = true)]
    [Bix("F96B12F7-F922-49FA-BBA3-51135D03BC7E")]
    public class RootA : AmorphousTypedDoc
    {
      [Field] public string A { get; set; }
      public override bool AmorphousDataEnabled => true;

      protected override void AddJsonSerializerField(Schema.FieldDef def, JsonWritingOptions options, Dictionary<string, object> jsonMap, string name, object value)
      {
        if (def?.Order == 0)
          BixJsonHandler.EmitJsonBixDiscriminator(this, jsonMap);

        base.AddJsonSerializerField(def, options, jsonMap, name, value);
      }
    }

    [Bix("28F68E64-9DC9-494B-9E9F-F3761B2A68B2")]
    public class RootB : RootA
    {
      [Field] public string B { get; set; }
    }


    [Run]
    public void Root_01()
    {
      var obj = new RootA(){  A = "This is roota" };
      var json = obj.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

      json.See("JSON: ");

      var got = JsonReader.ToDoc<RootA>(json);

      got.See("GOT: ");

      Aver.IsTrue(got is RootA);
      Aver.AreEqual(obj.A, got.A);
    }

    [Run]
    public void Root_01_withamorph()
    {
      var obj = new RootA() { A = "This is roota" };
      obj.AmorphousData["x"] = -123;
      var json = obj.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

      json.See("JSON: ");

      var got = JsonReader.ToDoc<RootA>(json);

      got.See("GOT: ");

      Aver.IsTrue(got is RootA);
      Aver.AreEqual(obj.A, got.A);
      Aver.AreEqual(-123, got.AmorphousData["x"].AsInt());
    }

    [Run]
    public void Root_02()
    {
      var obj = new RootB() { A = "from A", B = "I AM B" };
      var json = obj.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

      json.See("JSON: ");

      var got = JsonReader.ToDoc<RootA>(json);

      got.GetType().FullName.See();
      got.See("GOT: ");

      var gotB = got as RootB;
      Aver.IsNotNull(gotB);
      Aver.AreEqual(obj.A, gotB.A);
      Aver.AreEqual(obj.B, gotB.B);
    }


    public class Frame : TypedDoc
    {
      [BixJsonHandler(ThrowOnUnresolvedType = true)]
      [Field] public Data Payload { get; set; }
    }


    //////// [BixJsonHandler(ThrowOnUnresolvedType = true)] //you can comment attribute on property and uncomment here instead
    public abstract class Data : AmorphousTypedDoc
    {
      [Field] public string Common { get; set; }

      protected override void AddJsonSerializerField(Schema.FieldDef def, JsonWritingOptions options, Dictionary<string, object> jsonMap, string name, object value)
      {
        if (def?.Order == 0)
          BixJsonHandler.EmitJsonBixDiscriminator(this, jsonMap);

        base.AddJsonSerializerField(def, options, jsonMap, name, value);
      }
    }

    [Bix("1B1CA251-E3FF-4F44-A562-88FAAB8CC3C3")]
    public class DataA : Data
    {
      [Field] public string A { get; set;}
    }

    [Bix("9DF66D9E-0527-4E15-9C55-40C0B8957865")]
    public class DataB : Data
    {
      [Field] public string B { get; set; }
    }


    [Run]
    public void Frame_01()
    {
      var obj = new Frame() { Payload = new DataA{ Common = "Common1", A = "A1"} };
      var json = obj.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

      json.See("JSON: ");

      var got = JsonReader.ToDoc<Frame>(json);

      got.See("GOT: ");

      var data = got.Payload as DataA;
      Aver.IsNotNull(data);
      Aver.AreEqual("Common1", data.Common);
      Aver.AreEqual("A1", data.A);
    }

    [Run]
    public void Frame_02()
    {
      var obj = new Frame() { Payload = new DataB { Common = "Common2", B = "B1" } };
      var json = obj.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

      json.See("JSON: ");

      var got = JsonReader.ToDoc<Frame>(json);

      got.See("GOT: ");

      var data = got.Payload as DataB;
      Aver.IsNotNull(data);
      Aver.AreEqual("Common2", data.Common);
      Aver.AreEqual("B1", data.B);
    }

  }
}
