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
    [Run]
    public void ReadTest()
    {
      var json = @"
      {
       ""EntityArray"": [
            {""Name"": ""Buick"", ""Mileage"": 54000},
            {""Name"": ""Honda"", ""Mileage"": 12350},
            {""Name"": ""Berezka"", ""ColorSystem"": ""SECAM""}
        ] ,
       ""MyTv"": {""Name"": ""Grundig"", ""ColorSystem"": ""PAL/NTSC""},
       ""MyCar"": {""Name"": ""Ford"", ""Mileage"": 1000123},
       ""Favorite"": {""Name"": ""VAZ 2103"", ""Mileage"": 120123}
      }
      ";

      var got = JsonReader.ToDoc<DocWithArray>(json);

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

  }
}
