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
  public class JsonDocWithCustomHandlerTests
  {
    public const string JSON1 = @"
    {
      ""Name"": ""US NASA Space program"",
      ""Struct1"": {
        ""n"": ""Luna 1"",
        ""y"": 1959
      },
      ""Struct2"": {
        ""n"": ""Apollo 11"",
        ""y"": 1969
      }
    }";

    [Run]
    public void CustomStructs()
    {
      var got = JsonReader.ToDoc<DocWithCustomStruct>(JSON1);
      got.See();
      Aver.IsNotNull(got);
      Aver.AreEqual("US NASA Space program", got.Name);

      Aver.AreEqual("Luna 1", got.Struct1.Name);
      Aver.AreEqual(1959, got.Struct1.Year);

      Aver.AreEqual("Apollo 11", got.Struct2.Mission);
      Aver.AreEqual(1969, got.Struct2.When);
    }

    [Run]
    public void CustomInjectionWithNameElision()
    {
      var d1 = new DocWithCustomType
      {
        Name = "Veggies",
        Item1 = new EntityA { Name = "tomato", A = 250 },
        Item2 = new EntityA { Name = "onion", A = 1250 },
        Item3 = new EntityB { Name = "potato", B = -900 }
      };

      var json = d1.ToJson();

      var got = JsonReader.ToDoc<DocWithCustomType>(json);

      got.See();

      Aver.IsNotNull(got);
      Aver.IsNotNull(got.Item1);
      Aver.IsNotNull(got.Item2);
      Aver.IsNotNull(got.Item3);

      Aver.IsTrue(got.Item1 is EntityA);
      Aver.IsTrue(got.Item2 is EntityA);
      Aver.IsTrue(got.Item3 is EntityB);


      Aver.AreEqual("Veggies", got.Name);

      Aver.AreEqual("tomato", got.Item1.Name);
      Aver.AreEqual("onion", got.Item2.Name);
      Aver.AreEqual("potato", got.Item3.Name);

      Aver.AreEqual(250,  ((EntityA)got.Item1).A);
      Aver.AreEqual(1250, ((EntityA)got.Item2).A);
      Aver.AreEqual(-900, ((EntityB)got.Item3).B);
    }


    //Shows how to handle custom deserialization via a "policy" embodied in a custom handler
    public class Struct1Handler : JsonHandlerAttribute
    {
      public override TypeCastResult TypeCastOnRead(object v, Type toType, bool fromUI, JsonReader.DocReadOptions options)
      {
        if (v is JsonDataMap map)
        {
          var result = new CustomStruct1{ Name = map["n"].AsString(), Year = map["y"].AsInt()};
          return new TypeCastResult(TypeCastOutcome.HandledCast, result, typeof(CustomStruct1));
        }
        return TypeCastResult.NothingChanged;
      }
    }

    [Struct1Handler]//<-- notice the use of handler on type-level declaration
    public struct CustomStruct1
    {
      public string Name { get; set;}
      public int    Year { get; set; }
    }

    public class Struct2Handler : JsonHandlerAttribute
    {
      public override TypeCastResult TypeCastOnRead(object v, Type toType, bool fromUI, JsonReader.DocReadOptions options)
      {
        if (v is JsonDataMap map)
        {
          var result = new CustomStruct2 { Mission = map["n"].AsString(), When = map["y"].AsInt() };
          return new TypeCastResult(TypeCastOutcome.HandledCast, result, typeof(CustomStruct2));
        }
        return TypeCastResult.NothingChanged;
      }
    }

    public struct CustomStruct2
    {
      public string Mission { get; set; }
      public int When { get; set; }
    }

    public class DocWithCustomStruct : TypedDoc
    {
      [Field]
      public string Name { get; set;}

      [Field]
      public CustomStruct1 Struct1{ get; set;}

      [Field, Struct2Handler] //<-- notice the use of handler on field-level declaration
      public CustomStruct2 Struct2 { get; set; }

    }

    public class EntityHandler : JsonHandlerAttribute
    {
      public override TypeCastResult TypeCastOnRead(object v, Type toType, bool fromUI, JsonReader.DocReadOptions options)
      {
        if (v is JsonDataMap map)
        {
          var t = map["_type"].AsString();
          map.Remove("_type"); //block the field
          if (t == nameof(EntityA)) return new TypeCastResult(typeof(EntityA));
          if (t == nameof(EntityB)) return new TypeCastResult(typeof(EntityB));
        }
        return TypeCastResult.NothingChanged;
      }
    }

    [EntityHandler]
    public abstract class Entity : AmorphousTypedDoc
    {
      [Field]
      public string Name { get; set; }

      protected override void AddJsonSerializerField(Schema.FieldDef def, JsonWritingOptions options, Dictionary<string, object> jsonMap, string name, object value)
      {
        if (name==nameof(Name))
          jsonMap["_type"] = GetType().Name;

        base.AddJsonSerializerField(def, options, jsonMap, name, value);
      }
    }

    public class EntityA : Entity
    {
      [Field]
      public int A { get; set; }
    }

    public class EntityB : Entity
    {
      [Field]
      public int B { get; set; }
    }

    public class DocWithCustomType : AmorphousTypedDoc
    {
      [Field]
      public string Name { get; set; }

      [Field]
      public Entity Item1 { get; set; }

      [Field]
      public Entity Item2 { get; set; }

      [Field]
      public Entity Item3 { get; set; }
    }

  }
}
