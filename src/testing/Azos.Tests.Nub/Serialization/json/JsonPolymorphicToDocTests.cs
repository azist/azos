/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JsonPolymorphicToDocTests
  {
    [Run]
    public void Case01_abstract_T()
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
    public void Case02_abstract_typeof()
    {
      var d1 = new ADoc { Name = "ADoc name", A = 100 };
      var json = d1.ToJson();

      json.See();

      var got = JsonReader.ToDoc(typeof(BaseDoc), json) as ADoc;

      Aver.IsNotNull(got);
      Aver.AreEqual("ADoc name", got.Name);
      Aver.IsTrue(got is ADoc);
      Aver.AreEqual(100, ((ADoc)got).A);
    }

    [Run]
    public void Case03_new()
    {
      var d1 = new ADoc { Name = "ADoc name", A = 100 };
      var json = d1.ToJson();

      json.See();

      var got = new ADoc();
      JsonReader.ToDoc(got, json);

      Aver.IsNotNull(got);
      Aver.AreEqual("ADoc name", got.Name);
      Aver.AreEqual(100, got.A);
    }

    [Run]
    public void Case04_concrete()
    {
      var d1 = new ADoc { Name = "ADoc name", A = 100 };
      var json = d1.ToJson();

      var got = JsonReader.ToDoc<ADoc>(json);

      Aver.IsNotNull(got);
      Aver.AreEqual("ADoc name", got.Name);
      Aver.AreEqual(100, got.A);
    }

    [Run]
    public void Case05_abstract()
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
    public void Case06_concrete()
    {
      var d1 = new AADoc { Name = "AADoc name", A = 100, AA = 100.234 };
      var json = d1.ToJson();

      var got = JsonReader.ToDoc<ADoc>(json);//AA deser to A

      Aver.IsNotNull(got);
      Aver.AreEqual("AADoc name", got.Name);
      Aver.AreEqual(100, got.A);
      Aver.AreEqual(100.234, ((AADoc)got).AA);
    }

    [Run]
    public void Case07_concrete()
    {
      var d1 = new AADoc { Name = "AADoc name", A = 100, AA = 100.234 };
      var json = d1.ToJson();

      var got = JsonReader.ToDoc<AADoc>(json);//AA deser to AA

      Aver.IsNotNull(got);
      Aver.AreEqual("AADoc name", got.Name);
      Aver.AreEqual(100, got.A);
      Aver.AreEqual(100.234, got.AA);
    }

    [Run]
    public void Case08_complex_array()
    {
      var d1 = new BBDoc
      {
         B = 900, BB = -800.123, Name = "Moon",
         ArrayOthers = new BaseDoc[]{
           new ADoc{Name = "A0", A = 0},
           null,
           new BDoc{Name = "B2", B = 2},
           new BBDoc{Name = "BB3", B = 3},
           new AADoc{Name = "AA4", A = 4}
         }
      };

      var json = d1.ToJson();
      json.See();

      var got = JsonReader.ToDoc<BaseDoc>(json);//BB deser to BaseDoc

      Aver.IsNotNull(got);
      Aver.IsTrue(got is BBDoc);
      Aver.AreEqual("Moon", got.Name);
      Aver.AreEqual(900, ((BBDoc)got).B);
      Aver.AreEqual(-800.123, ((BBDoc)got).BB);
      Aver.IsNotNull(got.ArrayOthers);
      Aver.AreEqual(5, got.ArrayOthers.Length);

      Aver.IsTrue(got.ArrayOthers[0] is ADoc);
      Aver.IsNull(got.ArrayOthers[1]);
      Aver.IsTrue(got.ArrayOthers[2] is BDoc);
      Aver.IsTrue(got.ArrayOthers[3] is BBDoc);
      Aver.IsTrue(got.ArrayOthers[4] is AADoc);

      Aver.AreEqual("A0", got.ArrayOthers[0].Name);
      Aver.AreEqual("B2", got.ArrayOthers[2].Name);
      Aver.AreEqual("BB3", got.ArrayOthers[3].Name);
      Aver.AreEqual("AA4", got.ArrayOthers[4].Name);

      Aver.AreEqual(0, ((ADoc)got.ArrayOthers[0]).A);
      Aver.AreEqual(2, ((BDoc)got.ArrayOthers[2]).B);
      Aver.AreEqual(3, ((BBDoc)got.ArrayOthers[3]).B);
      Aver.AreEqual(4, ((AADoc)got.ArrayOthers[4]).A);

      got.See();
    }

    [Run]
    public void Case09_complex_list()
    {
      var d1 = new BBDoc
      {
        B = 900,
        BB = -800.123,
        Name = "Moon",
        ListOthers = new List<BaseDoc>{
           new ADoc{Name = "A0", A = 0},
           null,
           new BDoc{Name = "B2", B = 2},
           new BBDoc{Name = "BB3", B = 3},
           new AADoc{Name = "AA4", A = 4}
         }
      };

      var json = d1.ToJson();
      json.See();

      var got = JsonReader.ToDoc<BaseDoc>(json);//BB deser to BaseDoc

      Aver.IsNotNull(got);
      Aver.IsTrue(got is BBDoc);
      Aver.AreEqual("Moon", got.Name);
      Aver.AreEqual(900, ((BBDoc)got).B);
      Aver.AreEqual(-800.123, ((BBDoc)got).BB);
      Aver.IsNotNull(got.ListOthers);
      Aver.AreEqual(5, got.ListOthers.Count);

      Aver.IsTrue(got.ListOthers[0] is ADoc);
      Aver.IsNull(got.ListOthers[1]);
      Aver.IsTrue(got.ListOthers[2] is BDoc);
      Aver.IsTrue(got.ListOthers[3] is BBDoc);
      Aver.IsTrue(got.ListOthers[4] is AADoc);

      Aver.AreEqual("A0", got.ListOthers[0].Name);
      Aver.AreEqual("B2", got.ListOthers[2].Name);
      Aver.AreEqual("BB3", got.ListOthers[3].Name);
      Aver.AreEqual("AA4", got.ListOthers[4].Name);

      Aver.AreEqual(0, ((ADoc)got.ListOthers[0]).A);
      Aver.AreEqual(2, ((BDoc)got.ListOthers[2]).B);
      Aver.AreEqual(3, ((BBDoc)got.ListOthers[3]).B);
      Aver.AreEqual(4, ((AADoc)got.ListOthers[4]).A);

      got.See();
    }

    [Run]
    public void Case10_logical_diff_basic()
    {
      var d1 = new BBDoc
      {
        B = 900,
        BB = -800.123,
        Name = "Moon"
      };

      var json = d1.ToJson();
      json.See();

      var got = JsonReader.ToDoc<BaseDoc>(json);//BB deser to BaseDoc

      var cmp = new DocLogicalComparer{  LoopByA = true, LoopByB=true, LoopByAmorphous = false};
      var result = cmp.Compare(d1,got);

      Aver.IsTrue(result.AreSame);
    }

    [Run]
    public void Case10_logical_diff_withcollections_1_rank()
    {
      var d1 = new BBDoc
      {
        B = 900,
        BB = -800.123,
        Name = "Saturn",
        ListOthers = new List<BaseDoc>{
           new ADoc{Name = "A0", A = 0},
           null,
           new BDoc{Name = "B2", B = 2},
           new BBDoc{Name = "BB3", B = 3},
           null,
           null,
           new AADoc{Name = "AA4", A = 4}
         },
        ArrayOthers = new BaseDoc[]{
           new AADoc{Name = "A0", A = -9, AA = 12343.0123},
           null,
           null,
           new BDoc{Name = "B2", B = 2}
         }
      };

      var json = d1.ToJson();
      json.See();

      var got = JsonReader.ToDoc<BaseDoc>(json);//BB deser to BaseDoc

      var cmp = new DocLogicalComparer { LoopByA = true, LoopByB = true, LoopByAmorphous = false };
      var result = cmp.Compare(d1, got);

      Aver.IsTrue(result.AreSame);
    }

    [Run]
    public void Case11_logical_diff_withcollections_2_rank()
    {
      var d1 = new BBDoc
      {
        B = 900,
        BB = -800.123,
        Name = "Saturn",
        ListOthers = new List<BaseDoc>{
           new ADoc{Name = "A0", A = 0, ArrayOthers = new BaseDoc[]{ new ADoc{Name="Elm1"}, null, null, new BDoc{Name="Elm2"}}},
           null,
           new BDoc{Name = "B2", B = 2, ListOthers = new List<BaseDoc>{}},//empty list
           new BBDoc{Name = "BB3", B = 3},
           null,
           new AADoc{
              Name = "AA4", A = 4,
              ArrayOthers = new BaseDoc[]{ new ADoc{Name="Elm1"}, new BDoc{Name="Elm2"}},
              ListOthers = new List<BaseDoc>{ new BBDoc { Name = "updown", BB = 345.1 }, new BDoc { Name = "cu-cu", B = -546 } }
           },
           null
         },
        ArrayOthers = new BaseDoc[]{
           new AADoc{Name = "A0", A = -9, AA = 12343.0123},
           null,
           null,
           new BDoc{Name = "B2", B = 2, ListOthers = new List<BaseDoc>{
              new ADoc{Name="23", A =23},
              new ADoc{Name="24", A =24},
              new BDoc{Name="25", B =25,
                ListOthers = new List<BaseDoc>{
                   new BBDoc { Name = "town-down", BB = -1000 }
                }
              },
           }}
         }
      };

      var json = d1.ToJson();
      json.See();

      var got = JsonReader.ToDoc<BaseDoc>(json);//BB deser to BaseDoc


      got.See();

      var cmp = new DocLogicalComparer { LoopByA = true, LoopByB = true, LoopByAmorphous = false };
      var result = cmp.Compare(d1, got);

      Aver.IsTrue(result.AreSame);
    }


    //handles polymorphism of json types
    public class PolyHandler : JsonHandlerAttribute
    {
      public const string TYPE_MONIKER = "$type";

      public override TypeCastResult TypeCastOnRead(object v, Type toType, bool fromUI, JsonReader.DocReadOptions options)
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

      [Field]
      public BaseDoc[] ArrayOthers{  get; set;}

      [Field]
      public List<BaseDoc> ListOthers { get; set; }


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
