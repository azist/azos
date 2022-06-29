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
  public class JsonPolymorphismTests
  {
    private const string JSON1 = @"
      {
       ""Entities"": [
            {""Name"": ""Buick"", ""Mileage"": 54000},
            {""Name"": ""Honda"", ""Mileage"": 12350},
            {""Name"": ""Berezka"", ""ColorSystem"": ""SECAM""}
        ] ,
       ""MyTv"":     {""Name"": ""Grundig"", ""ColorSystem"": ""PAL/NTSC""},
       ""MyCar"":    {""Name"": ""Ford"", ""Mileage"": 1000123},
       ""Favorite"": {""Name"": ""VAZ 2103"", ""Mileage"": 120123}
      }
      ";

    private const string JSON2 = @"
      {
         ""Item1"": {""Name"": ""Tatra"", ""Mileage"": 223322},
         ""Item2"": {""Name"": ""Horizont"", ""ColorSystem"": ""ME-SECAM""},
         ""Item3"": {""Name"": ""Harley"", ""Mileage"": 11000},
         ""Item4"": null,
       ""DocArray"": [
          null, null, null, {""Name"": ""Horse Cart"", ""Mileage"": 100}
       ],
       ""DocList"": [
            {""Name"": ""Nissan"", ""Mileage"": 2000},
            null,
            {""Name"": ""Grundig 100"", ""ColorSystem"": ""PAL""}
       ],
       ""ObjectArray"": [
            {""Name"": ""Peugeot"", ""Mileage"": 127000},
            {""Name"": ""Renault"", ""Mileage"": 27000},
            null,
            null
       ],
       ""ObjectList"": [
            {""Name"": ""Chevy Tahoe"", ""Mileage"": 1000},
            null,
            {""Name"": ""Electronica VL100"", ""ColorSystem"": ""BW""}
       ]
      }";

    private const string JSON3 = @"
      {
       ""Text"" : ""Learn Chinese!"",
       ""Docs"": [
            {
               ""Entities"": [
                    {""Name"": ""1Buick"", ""Mileage"": 54000},
                    {""Name"": ""1Honda"", ""Mileage"": 12350},
                    {""Name"": ""1Berezka"", ""ColorSystem"": ""1SECAM""}
                ] ,
                 ""MyTv"":     {""Name"": ""1Grundig"", ""ColorSystem"": ""1PAL/NTSC""},
                 ""MyCar"":    {""Name"": ""1Ford"", ""Mileage"": 1000123},
                 ""Favorite"": {""Name"": ""1VAZ 2103"", ""Mileage"": 120123}
            },
            {
               ""Entities"": [
                    {""Name"": ""2Buick"", ""Mileage"": -54000},
                    {""Name"": ""2Honda"", ""Mileage"": -12350},
                    {""Name"": ""2Berezka"", ""ColorSystem"": ""2SECAM""}
                ] ,
                 ""MyTv"":     {""Name"": ""2Grundig"", ""ColorSystem"": ""2PAL/NTSC""},
                 ""MyCar"":    {""Name"": ""2Ford"", ""Mileage"": -1000123},
                 ""Favorite"": {""Name"": ""2VAZ 2103"", ""Mileage"": -120123}
            }
      ]}
      ";

    /*
     NET fx 4.7.2 Parallel debug:    Did 350,000 in 3,906 ms at  89,606 ops/sec
     NET fx 4.7.2 Parallel release:  Did 350,000 in 1,917 ms at 182,577 ops/sec
     --------------------------------------------------------------------------
     Core 2.1.602 Parallel debug:    Did 350,000 in 4,471 ms at  78,282 ops/sec
     Core 2.1.602 Parallel release:  Did 350,000 in 2,441 ms at 143,384 ops/sec
    */
    [Run("!json-perf", "cnt=350000")]
    public void Perf(int cnt)
    {
      var got = JsonReader.ToDoc<DocWithArray>(JSON1);
      var sw = System.Diagnostics.Stopwatch.StartNew();
      System.Threading.Tasks.Parallel.For(0,cnt, i=>    //  for (var i=0; i<cnt; i++)
      {
        got = JsonReader.ToDoc<DocWithArray>(JSON1);
        Aver.IsNotNull(got);
      });

      var el = sw.ElapsedMilliseconds;
      Conout.SeeArgs("Did {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(cnt, el, cnt / (el / 1000d)));
    }


    [Run]
    public void PolymorphicArrays()
    {
      var got = JsonReader.ToDoc<DocWithArray>(JSON1); //<---ARRAYS

      got.See();

      Aver.IsNotNull(got);
      Aver.IsNotNull(got.Entities);
      Aver.AreEqual(3, got.Entities.Length);

      Aver.IsTrue(got.Entities[0] is Car);
      Aver.IsTrue(got.Entities[1] is Car);
      Aver.IsTrue(got.Entities[2] is TV);

      Aver.AreEqual(54000, ((Car)got.Entities[0]).Mileage);
      Aver.AreEqual(12350, ((Car)got.Entities[1]).Mileage);
      Aver.AreEqual("SECAM", ((TV)got.Entities[2]).ColorSystem);

      Aver.IsNotNull(got.MyTv);
      Aver.IsNotNull(got.MyCar);
      Aver.IsNotNull(got.Favorite);

      Aver.AreEqual("PAL/NTSC", got.MyTv.ColorSystem);
      Aver.AreEqual(1_000_123, got.MyCar.Mileage);
      Aver.IsTrue(got.Favorite is Car);//it did inject Car into BaseEntity
      Aver.AreEqual("VAZ 2103", ((Car)got.Favorite).Name);
    }

    [Run]
    public void PolymorphicLists()
    {
      var got = JsonReader.ToDoc<DocWithList>(JSON1);//<---LISTS

      got.See();

      Aver.IsNotNull(got);
      Aver.IsNotNull(got.Entities);
      Aver.AreEqual(3, got.Entities.Count);

      Aver.IsTrue(got.Entities[0] is Car);
      Aver.IsTrue(got.Entities[1] is Car);
      Aver.IsTrue(got.Entities[2] is TV);

      Aver.AreEqual(54000, ((Car)got.Entities[0]).Mileage);
      Aver.AreEqual(12350, ((Car)got.Entities[1]).Mileage);
      Aver.AreEqual("SECAM", ((TV)got.Entities[2]).ColorSystem);

      Aver.IsNotNull(got.MyTv);
      Aver.IsNotNull(got.MyCar);
      Aver.IsNotNull(got.Favorite);

      Aver.AreEqual("PAL/NTSC", got.MyTv.ColorSystem);
      Aver.AreEqual(1_000_123, got.MyCar.Mileage);
      Aver.IsTrue(got.Favorite is Car);//it did inject Car into BaseEntity
      Aver.AreEqual("VAZ 2103", ((Car)got.Favorite).Name);
    }

    [Run]
    public void PolymorphicCaseMix()
    {
      var got = JsonReader.ToDoc<ComplexDoc>(JSON2);

      got.See();

      Aver.IsNotNull(got);
      Aver.IsNotNull(got.Item1);
      Aver.IsNotNull(got.Item2);
      Aver.IsNotNull(got.Item3);
      Aver.IsNull(got.Item4);
      Aver.IsNull(got.Item5);

      Aver.IsNotNull(got.DocArray);
      Aver.IsNotNull(got.DocList);
      Aver.IsNotNull(got.ObjectArray);
      Aver.IsNotNull(got.ObjectList);

      Aver.AreEqual(4, got.DocArray.Length);
      Aver.AreEqual(3, got.DocList.Count);
      Aver.AreEqual(4, got.ObjectArray.Length);
      Aver.AreEqual(3, got.ObjectList.Count);

      Aver.IsNull(got.DocArray[0]);
      Aver.IsNull(got.DocArray[1]);
      Aver.IsNull(got.DocArray[2]);
      Aver.IsNotNull(got.DocArray[3]);
      Aver.AreEqual("Horse Cart", ((Car)got.DocArray[3]).Name);
      Aver.AreEqual(100, ((Car)got.DocArray[3]).Mileage);

      Aver.IsNotNull(got.DocList[0]);
      Aver.IsNull(got.DocList[1]);
      Aver.IsNotNull(got.DocList[2]);

      Aver.AreEqual("Nissan", ((Car)got.DocList[0]).Name);
      Aver.AreEqual(2000, ((Car)got.DocList[0]).Mileage);
      Aver.AreEqual("Grundig 100", ((TV)got.DocList[2]).Name);
      Aver.AreEqual("PAL", ((TV)got.DocList[2]).ColorSystem);

      Aver.IsNotNull(got.ObjectArray[0]);
      Aver.IsNotNull(got.ObjectArray[1]);
      Aver.IsNull(got.ObjectArray[2]);
      Aver.IsNull(got.ObjectArray[3]);

      Aver.AreEqual("Peugeot", ((Car)got.ObjectArray[0]).Name);
      Aver.AreEqual(127000, ((Car)got.ObjectArray[0]).Mileage);
      Aver.AreEqual("Renault", ((Car)got.ObjectArray[1]).Name);
      Aver.AreEqual(27000, ((Car)got.ObjectArray[1]).Mileage);


      Aver.IsNotNull(got.ObjectList[0]);
      Aver.IsNull(got.ObjectList[1]);
      Aver.IsNotNull(got.ObjectList[2]);

      Aver.AreEqual("Chevy Tahoe", ((Car)got.ObjectList[0]).Name);
      Aver.AreEqual(1000, ((Car)got.ObjectList[0]).Mileage);
      Aver.AreEqual("Electronica VL100", ((TV)got.ObjectList[2]).Name);
      Aver.AreEqual("BW", ((TV)got.ObjectList[2]).ColorSystem);
    }

    [Run]
    public void ArraysofDocsWithSubArrays()
    {
      var got = JsonReader.ToDoc<DocSubArrays>(JSON3);

      got.See();

      Aver.IsNotNull(got);
      Aver.AreEqual("Learn Chinese!", got.Text);
      Aver.IsNotNull(got.Docs);
      Aver.AreEqual(2, got.Docs.Length);

      var ones = got.Docs[0].Entities;
      Aver.IsNotNull(ones);
      Aver.AreEqual(3, ones.Length);

      Aver.IsTrue(ones[0] is Car);
      Aver.IsTrue(ones[1] is Car);
      Aver.IsTrue(ones[2] is TV);

      Aver.AreEqual(54000, ((Car)ones[0]).Mileage);
      Aver.AreEqual(12350, ((Car)ones[1]).Mileage);
      Aver.AreEqual("1SECAM", ((TV)ones[2]).ColorSystem);

      Aver.IsNotNull(got.Docs[0].MyTv);
      Aver.IsNotNull(got.Docs[0].MyCar);
      Aver.IsNotNull(got.Docs[0].Favorite);

      Aver.AreEqual("1PAL/NTSC", got.Docs[0].MyTv.ColorSystem);
      Aver.AreEqual(1_000_123, got.Docs[0].MyCar.Mileage);
      Aver.IsTrue(got.Docs[0].Favorite is Car);//it did inject Car into BaseEntity
      Aver.AreEqual("1VAZ 2103", ((Car)got.Docs[0].Favorite).Name);

      var twos = got.Docs[1].Entities;
      Aver.IsNotNull(twos);
      Aver.AreEqual(3, twos.Length);

      Aver.IsTrue(twos[0] is Car);
      Aver.IsTrue(twos[1] is Car);
      Aver.IsTrue(twos[2] is TV);

      Aver.AreEqual(-54000, ((Car)twos[0]).Mileage);
      Aver.AreEqual(-12350, ((Car)twos[1]).Mileage);
      Aver.AreEqual("2SECAM", ((TV)twos[2]).ColorSystem);

      Aver.IsNotNull(got.Docs[1].MyTv);
      Aver.IsNotNull(got.Docs[1].MyCar);
      Aver.IsNotNull(got.Docs[1].Favorite);

      Aver.AreEqual("2PAL/NTSC", got.Docs[1].MyTv.ColorSystem);
      Aver.AreEqual(-1_000_123, got.Docs[1].MyCar.Mileage);
      Aver.IsTrue(got.Docs[1].Favorite is Car);//it did inject Car into BaseEntity
      Aver.AreEqual("2VAZ 2103", ((Car)got.Docs[1].Favorite).Name);

    }

    /// <summary>
    /// Example of Json deserialization polymorphism based on JSON map structure
    /// </summary>
    public class CustomEntityTypeJsonHandlerAttribute : JsonHandlerAttribute
    {
      public override TypeCastResult TypeCastOnRead(object v, Type toType, bool fromUI, JsonReader.DocReadOptions options)
      {
        if (v is JsonDataMap map)
        {
          if (map.ContainsKey(nameof(Car.Mileage))) return new TypeCastResult(typeof(Car));
          if (map.ContainsKey(nameof(TV.ColorSystem))) return new TypeCastResult(typeof(TV));
        }
        return TypeCastResult.NothingChanged;
      }
    }


    [CustomEntityTypeJsonHandler]
    public abstract class BaseEntity : TypedDoc
    {
      [Field] public string Name {  get; set; }
    }

    public class Car : BaseEntity
    {
      [Field] public int Mileage { get; set; }
    }

    public class TV : BaseEntity
    {
      [Field] public string ColorSystem { get; set; }
    }

    public class DocWithArray : TypedDoc
    {
      [Field] public TV MyTv { get; set; }
      [Field] public Car MyCar { get; set; }
      [Field] public BaseEntity Favorite { get; set; }
      [Field] public BaseEntity[] Entities{  get; set;}
    }

    public class DocWithList : TypedDoc
    {
      [Field] public TV MyTv { get; set; }
      [Field] public Car MyCar { get; set; }
      [Field] public BaseEntity Favorite { get; set; }
      [Field] public List<BaseEntity> Entities { get; set; }
    }

    //Notice how we perform polymorphic JSon processing
    //for types which are not decorated with a custom attribute, we can add attribute right on the field
    //including collections: arrays and lists. Consequently it is possible to deserialize various
    //custom type instances in a an abstractly-typed collection using specific handler.
    public class ComplexDoc : TypedDoc
    {
      [Field] public BaseEntity Item1 { get; set; } //no need for attr because BaseEntity declares it on its own type
      [Field] [CustomEntityTypeJsonHandler] public Doc     Item2 { get; set; } //Doc is polymorphic - need to declare at field level
      [Field] [CustomEntityTypeJsonHandler] public object  Item3 { get; set; } //object is polymorphic
      [Field] [CustomEntityTypeJsonHandler] public object  Item4 { get; set; }
      [Field] [CustomEntityTypeJsonHandler] public object  Item5 { get; set; }


      [Field] [CustomEntityTypeJsonHandler] public Doc[]     DocArray { get; set; } //array of polymorphic Doc objects
      [Field] [CustomEntityTypeJsonHandler] public List<Doc> DocList  { get; set; } //list of polymorphic Doc objects

      [Field] [CustomEntityTypeJsonHandler] public object[]     ObjectArray { get; set; } //array of polymorphic objects
      [Field] [CustomEntityTypeJsonHandler] public List<object> ObjectList { get; set; } //list of polymorphic objects
    }

    public class DocSubArrays : TypedDoc
    {
      [Field] public string Text{ get; set; }
      [Field] public DocWithArray[] Docs{ get; set; }
    }

  }
}
