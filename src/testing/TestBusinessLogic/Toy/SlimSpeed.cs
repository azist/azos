using System;


using Azos;
using Azos.Serialization;

namespace TestBusinessLogic.Toy
{
    [Serializable]
    public class SimpleObject
    {
      public Guid ID       { get; set; }
      public string Name   { get; set; }
      public DateTime DOB  { get; set; }
      public bool Flag     { get; set; }
      public int Int       { get; set; }
    }

    [Serializable]
    public class Person
    {
      public Person(){}

      public string Name{ get; set;}
      public int Age{ get; set;}
      public DateTime DOB{ get; set;}
      public bool Satisfied{ get; set;}
      public string Notes{ get; set;}
      public decimal Salary{ get; set;}
      public double Distance{get;set;}
      public float K{get;set;}
      public long? L1{ get;set;}
      public long? L2{ get;set;}
      public long? L3{ get;set;}
      public long? L4{ get;set;}
      public object[] DataArray{ get; set;}
    }

    internal static class SlimSpeed
    {
      public static void SerDeserSimpleObject()
      {
        const int CNT = 5_000_000;

        var payload =
          new SimpleObject
          {
            ID = Guid.NewGuid(), DOB =DateTime.Now, Flag =true,Int = 2345, Name = "Jack Eightman"
          };

        bodySlimSerializer(CNT, payload);
        bodyBinaryFormatter(CNT, payload);
      }


      public static void SerDeserPerson()
      {
        const int CNT = 5_000_000;

        var payload =
        new Person
        {
           Age=190, Name="Alex Burbes", DOB = new DateTime(234234234), Satisfied=true, Notes="wehwqrkqw wer we rwh r2rh", Salary = 900000m, Distance=2.00392d, K=22.23f,
           L1 =null, L2 =23847823749, L3=-433, L4=909090923
        };

        bodySlimSerializer(CNT, payload);
        bodyBinaryFormatter(CNT, payload);
      }


      public static void SerDeserPersonArray()
      {
        const int CNT = 1_000_000;

        var payload = new[]{
        new Person
        {
           Age=190, Name="Alex Burbes", DOB = new DateTime(234234234), Satisfied=true, Notes="wehwqrkqw wer we rwh r2rh", Salary = 900000m, Distance=2.00392d, K=22.23f,
           L1 =null, L2 =23847823749, L3=-433, L4=909090923
        },
        new Person
        {
           Age=14, Name="fusdh fAlex Burbes", DOB = new DateTime(22342344234234), Satisfied=false, Notes="da net", Salary = 1900000m, Distance=2322.00392d, K=0.23f,
           L1 =1, L2 =null, L3=-433, L4=-123871238909
        }
        };

        bodySlimSerializer(CNT, payload);
        bodyBinaryFormatter(CNT, payload);
      }

      private static void bodySlimSerializer(int CNT, object payload)
      {
        var ms = new System.IO.MemoryStream();
        var ser1 = new Azos.Serialization.Slim.SlimSerializer();
        ser1.TypeMode = Azos.Serialization.Slim.TypeRegistryMode.Batch;

        ser1.Serialize(ms, payload); //warmup outside timer

        var ser2 = new Azos.Serialization.Slim.SlimSerializer();
        ser2.TypeMode = Azos.Serialization.Slim.TypeRegistryMode.Batch;
        ms.Position =0;
        var got = ser2.Deserialize(ms); //warmup outside timer

        bodyCore(ms, CNT, payload, ser1, ser2);
      }

      private static void bodyBinaryFormatter(int CNT, object payload)
      {
        var ms = new System.IO.MemoryStream();
        var ser1 = new MSBinaryFormatter();

        ser1.Serialize(ms, payload); //warmup outside timer

        var ser2 = new MSBinaryFormatter();

        ms.Position =0;
        var got = ser2.Deserialize(ms); //warmup outside timer

        bodyCore(ms, CNT, payload, ser1, ser2);
     }

      private static void bodyCore(System.IO.MemoryStream ms, int CNT, object payload, ISerializer ser1, ISerializer ser2)
      {
        Console.WriteLine("Payload of type `{0}` tested on a single thread by '{1}' serializer".Args(
                            payload.GetType().DisplayNameWithExpandedGenericArgs(),
                            ser1.GetType().FullName));
        Console.WriteLine("-------------------------------------------");

        var sw = System.Diagnostics.Stopwatch.StartNew();

        for(var i=0; i<CNT; i++)
        {
          ms.Position=0;
          ser1.Serialize(ms, payload);
        }
        Console.WriteLine("Serialized {0:n0} bytes {1:n0} at {2:n0} ops/sec".Args(ms.Position, CNT, CNT / (sw.ElapsedMilliseconds / 1000d)));


        sw = System.Diagnostics.Stopwatch.StartNew();
        for(var i=0; i<CNT; i++)
        {
          ms.Position=0;
          ser2.Deserialize(ms);
        }
        Console.WriteLine("Deserialized {0:n0} at {1:n0} ops/sec".Args(CNT, CNT / (sw.ElapsedMilliseconds / 1000d)));

        Console.WriteLine();
        Console.WriteLine();
     }


    }
}
