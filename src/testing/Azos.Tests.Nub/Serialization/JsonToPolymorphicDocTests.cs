using System;
using System.Collections.Generic;
using System.Text;
using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JsonToPolymorphicDocTests
  {
    private const string JSON1 = @"
      {
       ""EntityArray"": [
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


    [Run]
    public void PolymorphicArrays()
    {
      var got = JsonReader.ToDoc<DocWithArray>(JSON1);

      got.See();

      Aver.IsNotNull(got);
      Aver.IsNotNull(got.EntityArray);
      Aver.AreEqual(3, got.EntityArray.Length);

      Aver.IsTrue(got.EntityArray[0] is Car);
      Aver.IsTrue(got.EntityArray[1] is Car);
      Aver.IsTrue(got.EntityArray[2] is TV);

      Aver.AreEqual(54000, ((Car)got.EntityArray[0]).Mileage);
      Aver.AreEqual(12350, ((Car)got.EntityArray[1]).Mileage);
      Aver.AreEqual("SECAM", ((TV)got.EntityArray[2]).ColorSystem);

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
      var got = JsonReader.ToDoc<DocWithArray>(JSON1);

      got.See();

      Aver.IsNotNull(got);
      Aver.IsNotNull(got.EntityArray);
      Aver.AreEqual(3, got.EntityArray.Length);

      Aver.IsTrue(got.EntityArray[0] is Car);
      Aver.IsTrue(got.EntityArray[1] is Car);
      Aver.IsTrue(got.EntityArray[2] is TV);

      Aver.AreEqual(54000, ((Car)got.EntityArray[0]).Mileage);
      Aver.AreEqual(12350, ((Car)got.EntityArray[1]).Mileage);
      Aver.AreEqual("SECAM", ((TV)got.EntityArray[2]).ColorSystem);

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
      Aver.IsNull(got.DocArray[3]);
      Aver.IsNotNull(got.DocArray[4]);
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

    /// <summary>
    /// Example of Json deserialization polymorphism based on JSON map structure
    /// </summary>
    public class CustomEntityTypeJsonHandlerAttribute : JsonHandlerAttribute
    {
      public override TypeCastResult TypeCastOnRead(object v, Type toType, bool fromUI, JsonReader.NameBinding nameBinding)
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
      [Field] public BaseEntity[] EntityArray{  get; set;}
    }

    public class DocWithList : TypedDoc
    {
      [Field] public TV MyTv { get; set; }
      [Field] public Car MyCar { get; set; }
      [Field] public BaseEntity Favorite { get; set; }
      [Field] public List<BaseEntity> EntityList { get; set; }
    }

    public class ComplexDoc : TypedDoc
    {
      [Field] public BaseEntity Item1 { get; set; }
      [Field] public Doc        Item2 { get; set; }
      [Field] public object     Item3 { get; set; }
      [Field] public object Item4 { get; set; }
      [Field] public object Item5 { get; set; }


      [Field] public Doc[]     DocArray { get; set; }
      [Field] public List<Doc> DocList  { get; set; }

      [Field] public object[]     ObjectArray { get; set; }
      [Field] public List<object> ObjectList { get; set; }
    }

  }
}
