/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Azos.Scripting;

using Azos.Serialization.JSON;
using Azos.DataAccess.Distributed;
using Azos.Serialization.Slim;
using static Azos.Aver.ThrowsAttribute;

namespace Azos.Tests.Unit.DataAccess
{
    [Runnable(TRUN.BASE, 3)]
    public class Distributed
    {
        [Run]
        public void Create_Seal()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Aver.IsTrue(ParcelState.Creating == parcel.State);
            parcel.Seal(FakeNOPBank.Instance);
            Aver.IsTrue(ParcelState.Sealed == parcel.State);
        }



        [Run]
        [Aver.Throws(typeof(ParcelSealValidationException), Message="Aroyan", MsgMatch=MatchType.Contains) ]
        public void Create_Seal_ValidationError()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "NoAroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Aver.IsTrue(ParcelState.Creating == parcel.State);
            parcel.Seal(FakeNOPBank.Instance);
        }


        [Run]
        public void Create_Seal_SerializeDeserialize_Read()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Aver.IsTrue(ParcelState.Creating == parcel.State);
            parcel.Seal(FakeNOPBank.Instance);



            var ser = new SlimSerializer( Parcel.STANDARD_KNOWN_SERIALIZER_TYPES );

            var ms = new MemoryStream();

            ser.Serialize(ms, parcel);
            ms.Position = 0;
            var parcel2 = ser.Deserialize( ms ) as PeopleNamesParcel;

            Aver.IsNotNull(parcel2);
            var payload2 = parcel2.Payload;
            Aver.IsNotNull(payload2);


            Aver.AreEqual(payload2.Count, names.Count);
            Aver.IsTrue( payload2.SequenceEqual( names) );

        }



        [Run]
        public void FullLifecycle()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Aver.IsTrue(ParcelState.Creating == parcel.State);
            parcel.Seal(FakeNOPBank.Instance);

            var ser = new SlimSerializer( Parcel.STANDARD_KNOWN_SERIALIZER_TYPES );

            var ms = new MemoryStream();

            ser.Serialize(ms, parcel);
            ms.Position = 0;
            var parcel2 = ser.Deserialize( ms ) as PeopleNamesParcel;

            Aver.IsNotNull(parcel2);

            Aver.IsTrue(ParcelState.Sealed == parcel2.State);
            parcel2.Open();
            Aver.IsTrue(ParcelState.Modifying == parcel2.State);

            parcel2.Payload[1]="Boyarskiy";
            parcel2.Seal(FakeNOPBank.Instance);

            ms.Position = 0;
            ser.Serialize(ms, parcel2);
            ms.Position = 0;
            var parcel3 = ser.Deserialize( ms ) as PeopleNamesParcel;

            Aver.IsTrue( new List<string>{"Kozloff", "Boyarskiy", "Aroyan", "Gurevich"}.SequenceEqual( parcel3.Payload) );
        }


        [Run]
        [Aver.Throws(typeof(DistributedDataAccessException), Message="Validate", MsgMatch=MatchType.Contains) ]
        public void StateError_2()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Aver.IsTrue(ParcelState.Creating == parcel.State);
            parcel.Seal(FakeNOPBank.Instance);
            parcel.Validate(FakeNOPBank.Instance);
        }

        [Run]
        [Aver.Throws(typeof(DistributedDataAccessException), Message="Argument", MsgMatch=MatchType.Contains) ]
        public void StateError_3()
        {
            var parcel = new PeopleNamesParcel(new GDID(0, 123), null);
        }


        [Run]
        [Aver.Throws(typeof(SlimSerializationException), Message="OnSerializing", MsgMatch=MatchType.Contains) ]
        public void StateError_4()
        {
            var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};

            var parcel = new PeopleNamesParcel(new GDID(0, 123), names);

            Aver.IsTrue(ParcelState.Creating == parcel.State);
            //not sealed
            var ser = new SlimSerializer( Parcel.STANDARD_KNOWN_SERIALIZER_TYPES );

            var ms = new MemoryStream();

            ser.Serialize(ms, parcel);//can not serialize open parcel

        }


        [Run]
        public void Parcel_DeepClone_1()
        {
           var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};
           var p1 = new PeopleNamesParcel(new GDID(0, 123), names);

           p1.Seal(FakeNOPBank.Instance);
           var p2 = p1.DeepClone() as PeopleNamesParcel;

           Aver.IsFalse( object.ReferenceEquals(p1, p2) );
           Aver.IsFalse( object.ReferenceEquals(p1.Payload, p2.Payload) );

           Aver.IsTrue(p1.Payload.SequenceEqual(p2.Payload));
           Aver.AreEqual( p1.GDID, p2.GDID );
        }


        [Run]
        public void Parcel_DeepClone_2_Equals_ToString()
        {
           var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};
           var p1 = new PeopleNamesParcel(new GDID(0, 123), names);

           p1.Seal(FakeNOPBank.Instance);
           var p2 = p1.DeepClone() as PeopleNamesParcel;

           Aver.IsFalse( object.ReferenceEquals(p1, p2) );
           Aver.IsTrue( p1.Equals(p2) );

           Console.WriteLine(p1.ToString());
        }


        [Run]
        public void Parcel_DeepClone_Benchmark()
        {
           const int CNT = 25000;

           var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};
           var p1 = new PeopleNamesParcel(new GDID(0, 123), names);

           p1.Seal(FakeNOPBank.Instance);

           var sw = System.Diagnostics.Stopwatch.StartNew();
           for(var i=0; i<CNT; i++)
           {
             var p2 = p1.DeepClone() as PeopleNamesParcel;

             Aver.IsFalse( object.ReferenceEquals(p1, p2) );
             Aver.IsFalse( object.ReferenceEquals(p1.Payload, p2.Payload) );

             Aver.IsTrue(p1.Payload.Count == p2.Payload.Count);
             Aver.AreEqual( p1.GDID, p2.GDID );
           }
           sw.Stop();
           Console.WriteLine("NOTE: Core i7 3.2Ghz 14000 ops/sec on subsequent runs w/o optimization");
           Console.WriteLine("Cloned {0} times in {1} ms at {2} ops/sec", CNT, sw.ElapsedMilliseconds, CNT / (sw.ElapsedMilliseconds / 1000d));
        }

        [Run]
        public void Parcel_DeepClone_Benchmark_Parallel()
        {
           const int CNT = 100000;

           var names = new List<string>{"Kozloff", "Sergeev", "Aroyan", "Gurevich"};
           var p1 = new PeopleNamesParcel(new GDID(0, 123), names);

           p1.Seal(FakeNOPBank.Instance);

           var sw = System.Diagnostics.Stopwatch.StartNew();
           System.Threading.Tasks.Parallel.For(0, CNT,
           (i)=>
           {
             var p2 = p1.DeepClone() as PeopleNamesParcel;

             Aver.IsFalse( object.ReferenceEquals(p1, p2) );
             Aver.IsFalse( object.ReferenceEquals(p1.Payload, p2.Payload) );

             Aver.IsTrue(p1.Payload.Count == p2.Payload.Count);
             Aver.AreEqual( p1.GDID, p2.GDID );
           });
           sw.Stop();
           Console.WriteLine("NOTE: Core i7 x 6 cores 3.2Ghz 47000+ ops/sec on subsequent runs w/o optimization in non-server GC mode");
           Console.WriteLine("Cloned {0} times in {1} ms at {2} ops/sec", CNT, sw.ElapsedMilliseconds, CNT / (sw.ElapsedMilliseconds / 1000d));
        }



//============================================================================================
      [Run]
        public void HumanParcel_DeepClone_Benchmark()
        {
           const int CNT = 25000;

           var human = new Human
           {
              ID = new GDID(0,123),
              Name="Abosum Bora Sukolomokalapop",
              DOB = DateTime.Now,
              Status = HumanStatus.Ok,
               Addr1 = "234 Babai Drive # 12345",
                Addr2 = "Suite 23",
                 AddrCity = "Zmeevo",
                  AddrState = "NY",
                   AddrZip = "90210",
                    Father = new Human{ID=new GDID(1,12312), Name = "Farukh Na Chazz"},
                    Mother = new Human{ID=new GDID(1,1342312), Name = "Fatime Suka Dodik"}

           };
           var p1 = new HumanParcel(new GDID(0, 123), human);

           p1.Seal(FakeNOPBank.Instance);

           var sw = System.Diagnostics.Stopwatch.StartNew();
           for(var i=0; i<CNT; i++)
           {
             var p2 = p1.DeepClone() as HumanParcel;

             Aver.IsFalse( object.ReferenceEquals(p1, p2) );
             Aver.IsFalse( object.ReferenceEquals(p1.Payload, p2.Payload) );

             Aver.IsTrue(p1.Payload.Name == p2.Payload.Name);
             Aver.AreEqual( p1.GDID, p2.GDID );
           }
           sw.Stop();
           Console.WriteLine("NOTE: Core i7 3.2Ghz 14000 ops/sec on subsequent runs w/o optimization");
           Console.WriteLine("Cloned {0} times in {1} ms at {2} ops/sec", CNT, sw.ElapsedMilliseconds, CNT / (sw.ElapsedMilliseconds / 1000d));
        }




//============================================================================================

        [Run]
        public void ShardingPointer_IsAssigned()
        {
          ShardingPointer p1 = new ShardingPointer();
          ShardingPointer p2 = new ShardingPointer(typeof(HumanParcel), 124);

          Aver.IsFalse( p1.IsAssigned );
          Aver.IsTrue( p2.IsAssigned );
        }

    }

}
