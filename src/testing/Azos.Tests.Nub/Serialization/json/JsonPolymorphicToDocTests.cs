using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JsonPolymorphicToDocTests
  {
    [Run]
    public void Case01_abstract()
    {
      var d1 = new ADoc{ Name = "ADoc name", A = 100 };
      var json = d1.ToJson();

      json.See();

      var got = JsonReader.ToDoc<BaseDoc>(json);

      Aver.IsNotNull(got);
      Aver.AreEqual("ADoc name", got.Name);
      Aver.IsTrue(got is ADoc);
      Aver.AreEqual(100, ((ADoc)got).A);
    }

    [Run]
    public void Case02_concrete()
    {
      var d1 = new ADoc { Name = "ADoc name", A = 100 };
      var json = d1.ToJson();

      var got = JsonReader.ToDoc<ADoc>(json);

      Aver.IsNotNull(got);
      Aver.AreEqual("ADoc name", got.Name);
      Aver.AreEqual(100, got.A);
    }

    [Run]
    public void Case03_abstract()
    {
      var d1 = new AADoc { Name = "AADoc name", A = 100, AA = 100.234 };
      var json = d1.ToJson();

      var got = JsonReader.ToDoc<BaseDoc>(json);

      Aver.IsNotNull(got);
      Aver.AreEqual("AADoc name", got.Name);
      Aver.IsTrue(got is AADoc);
      Aver.AreEqual(100, ((AADoc)got).A);
      Aver.AreEqual(100.234, ((AADoc)got).AA);
    }

    [Run]
    public void Case04_concrete()
    {
      var d1 = new AADoc { Name = "AADoc name", A = 100, AA = 100.234 };
      var json = d1.ToJson();

      var got = JsonReader.ToDoc<ADoc>(json);

      Aver.IsNotNull(got);
      Aver.AreEqual("AADoc name", got.Name);
      Aver.AreEqual(100, got.A);
      Aver.AreEqual(100.234, ((AADoc)got).AA);
    }

    [Run]
    public void Case05_concrete()
    {
      var d1 = new AADoc { Name = "AADoc name", A = 100, AA = 100.234 };
      var json = d1.ToJson();

      var got = JsonReader.ToDoc<AADoc>(json);

      Aver.IsNotNull(got);
      Aver.AreEqual("AADoc name", got.Name);
      Aver.AreEqual(100, got.A);
      Aver.AreEqual(100.234, got.AA);
    }

    //handles polymorphism of json types
    public class PolyHandler : JsonHandlerAttribute
    {
      public const string TYPE_MONIKER = "$type";

      public override TypeCastResult TypeCastOnRead(object v, Type toType, bool fromUI, JsonReader.NameBinding nameBinding)
      {
        if (v is JsonDataMap map)
        {
          var tn = map[TYPE_MONIKER].AsString();
          if (tn.IsNotNullOrWhiteSpace())
          {
            //we do not need this to appear in Amorphous data
            map.Remove(TYPE_MONIKER);

            //try to get the cast-to the type to the value specified in the $type parameter
            var t = Type.GetType(typeof(JsonPolymorphicToDocTests).Namespace + '.' + nameof(JsonPolymorphicToDocTests) + '+' + tn, false, true);

            //if cast-to type was found and is a subtype of BaseDoc for security
            if (t != null && typeof(BaseDoc).IsAssignableFrom(t)) return new TypeCastResult(t);
          }
        }
        return TypeCastResult.NothingChanged;
      }
    }

    [PolyHandler]
    public abstract class BaseDoc : TypedDoc
    {
      [Field]
      public string Name { get; set; }

      protected override void AddJsonSerializerField(Schema.FieldDef def, JsonWritingOptions options, Dictionary<string, object> jsonMap, string name, object value)
      {
        if (def.Name==nameof(Name))
         jsonMap[PolyHandler.TYPE_MONIKER] = GetType().Name;//add custom type moniker that would allow for polymorphism on deser

        base.AddJsonSerializerField(def, options, jsonMap, name, value);
      }
    }

    public class ADoc : BaseDoc
    {
      [Field]
      public int A { get; set; }
    }

    public class BDoc : BaseDoc
    {
      [Field]
      public int B { get; set; }
    }

    public class AADoc : ADoc
    {
      [Field]
      public double AA { get; set; }
    }

    public class BBDoc : BDoc
    {
      [Field]
      public double BB { get; set; }
    }

  }
}
