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
  public class BixJsonPolymorphism2Tests
  {
    public BixJsonPolymorphism2Tests()
    {
      Bixer.RegisterTypeSerializationCores(Assembly.GetExecutingAssembly());
      ///// Bixer.GuidTypeResolver.Resolve(new Guid("F96B12F7-F922-49FA-BBA3-51135D03BC7E")).See();
    }


    [BixJsonHandler(ThrowOnUnresolvedType = true)] //<---on abstract BASE type
    [Bix("{6F52EF83-6D4D-4CEC-AB0D-0191DC1D118D}")]
    public abstract class Baze : AmorphousTypedDoc
    {
      [Field] public string Common { get; set; }
      public override bool AmorphousDataEnabled => true;

      protected override void AddJsonSerializerField(Schema.FieldDef def, JsonWritingOptions options, Dictionary<string, object> jsonMap, string name, object value)
      {
        if (def?.Order == 0)
          BixJsonHandler.EmitJsonBixDiscriminator(this, jsonMap);

        base.AddJsonSerializerField(def, options, jsonMap, name, value);
      }
    }

    [Bix("{99FF4A76-30EC-45EC-A6C7-67EB26423DB7}")]
    public class ClassA : Baze
    {
      [Field] public string A { get; set; }
    }

    [Bix("{2024586B-30AD-4D4E-A86D-267DA91D96BD}")]
    public class ClassB : Baze
    {
      [Field] public string B { get; set; }
    }


    [Run]
    public void Case_01()
    {
      var obj = new ClassA(){ Common = "YES",  A = "This is classA" };
      var json = obj.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

      json.See("JSON: ");

      var got = JsonReader.ToDoc<Baze>(json);

      got.See("GOT: ");

      Aver.IsTrue(got is ClassA);
      Aver.AreEqual("YES", got.Common);
      Aver.AreEqual(obj.A, ((ClassA)got).A);
    }

    [Run]
    public void Case_02()
    {
      var obj = new ClassB() { Common = "YESorNO", B = "This is classB" };
      var json = obj.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

      json.See("JSON: ");

      var got = JsonReader.ToDoc<Baze>(json);

      got.See("GOT: ");

      Aver.IsTrue(got is ClassB);
      Aver.AreEqual("YESorNO", got.Common);
      Aver.AreEqual(obj.B, ((ClassB)got).B);
    }

    [Run]
    public void Case_03()
    {
      var obj = new
      {
        a = new ClassA() { Common = "YES", A = "This is classA" },
        b = new ClassB() { Common = "YESorNO", B = "This is classB" }
      };

      var json = obj.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

      json.See("JSON: ");

      var gotMap = json.JsonToDataObject() as JsonDataMap;
      Aver.IsNotNull(gotMap);

      var gotA = JsonReader.ToDoc(typeof(Baze), gotMap["a"] as JsonDataMap);
      var gotB = JsonReader.ToDoc(typeof(Baze), gotMap["b"] as JsonDataMap);

      gotA.See("GOT A: ");
      gotB.See("GOT B: ");


      Aver.IsTrue(gotA is ClassA);
      Aver.AreEqual("YES", ((Baze)gotA).Common);
      Aver.AreEqual(obj.a.A, ((ClassA)gotA).A);

      Aver.IsTrue(gotB is ClassB);
      Aver.AreEqual("YESorNO", ((Baze)gotB).Common);
      Aver.AreEqual(obj.b.B, ((ClassB)gotB).B);
    }


  }
}
