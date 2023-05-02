/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Azos.Scripting;
using Azos.Time;
using Azos.Serialization.JSON;
using Azos.Data;
using Azos.Serialization.Bix;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JsonBenchmarkDocTests
  {
    static JsonBenchmarkDocTests()
    {
      JsonReader.____SetReaderBackend(new Azos.Serialization.JSON.Backends.JazonReaderBackend());
      Bixer.RegisterTypeSerializationCores(System.Reflection.Assembly.GetExecutingAssembly());
    }

    //[Run("cnt=50000 par=false")]
    //[Run("cnt=50000 par=true")]
    [Run("cnt=5000 par=false")]
    [Run("cnt=5000 par=true")]
    public void Test_TypicalPerson(int cnt, bool par)
    {
      var json= @"{ 
FirstName: 'Dodik',
LastName: 'Bonderman',
MiddleName: 'Boruch',
Address1: '150 Yasha Kutz Drive #380A',
Address2: '2nd flr',
City: 'Yerevan',
State: 'Arunachala',
Zip: 'WAQ-223322',
Phone: '+44-223-322-234',
Text: null,
EMail: 'dodik@gesheft.machen',
DOB: 'January 1, 1980',
YearsInService: 115,
CanVote: false,
CanTravel: true,
Score: 234.009805,
Income:  150000.00,
Assets: 3673456.18
}";

      void body()
      {
        var got = JsonReader.ToDoc<TypicalPerson>(json);
       // got.See();
        Aver.IsNotNull(got);
      }

      var time = Timeter.StartNew();

      if (par)
       Parallel.For(0, cnt, i => body());
      else
       for(var i=0; i<cnt; i++) body();

      time.Stop();

      "Did {0:n0} in {1:n1} sec at {2:n0} ops/sec".SeeArgs(cnt, time.ElapsedSec, cnt / time.ElapsedSec);
    }



    //[Run("cnt=50000 par=false")]
    //[Run("cnt=50000 par=true")]
    [Run("cnt=5000 par=false")]
    [Run("cnt=5000 par=true")]
    public void Test_TypicalFamilyWithPolymorphicShapes(int cnt, bool par)
    {
      var json = @"
      {
       Title: 'Boruch Yudson Moshe Dayan Family',
       YearEstablished: 1981,
       NotableMembers: [
  {
    FirstName: 'Dodik',
    LastName: 'Bonderman',
    MiddleName: 'Boruch',
    Address1: '150 Yasha Kutz Drive #380A',
    Address2: '2nd flr',
    City: 'Yerevan',
    State: 'Arunachala',
    Zip: 'WAQ-223322',
    Phone: '+44-223-322-234',
    Text: null,
    EMail: 'dodik@gesheft.machen',
    DOB: 'January 1, 1980',
    YearsInService: 115,
    CanVote: false,
    CanTravel: true,
    Score: 234.009805,
    Income:  150000.00,
    Assets: 3673456.18
  },
  null,
  null,
  null,
  {
    FirstName: 'Sarah',
    LastName: 'Kukusher',
    MiddleName: 'Feiga',
    Address1: '123 Cook you hard dr',
    Address2: '3nd flr',
    City: 'Shit Cove',
    State: 'Bidenshtadt',
    Zip: '90210',
    Phone: '+1-818-555-2323',
    Text: '440-999-2234',
    EMail: 'morrah@daemonize.linux',
    DOB: 'January 9, 1901',
    YearsInService: 134,
    CanVote: true,
    CanTravel: false,
    Score: 12e4,
    Income:  198000,
    Assets: 29000000
  },

], //Notable Members
FoundingMother: {FirstName: 'Mata', LastName: 'Hari',  MiddleName: 'George', City: 'Cockville', },
FoundingFather: {FirstName: 'Alex', LastName: 'Murari',  MiddleName: 'Anna Maria', City: 'Moody Glen Cove Shtadt', EMail: 'king.leer.345@english.muffin.hudson.edu'},

Shapes:[
 {'__bix': '8c54139e-a437-482b-b015-c8b8bc1d2ad4', 'Name': 'Circle1', 'Color': 'Green Black', 'Radius': 98},
 {'__bix': '749939d7-3256-4c76-92c0-70cf88c3d714', 'Name': 'sq1', 'Color': 'orange', 'Side': 12},
 null, null, null, null, null, null,
 {'__bix': '749939d7-3256-4c76-92c0-70cf88c3d714', 'Name': 'sq2', 'Color': 'lime', 'Side': 92.578},
]}";

      void body()
      {
        var got = JsonReader.ToDoc<Family>(json);
        ////////got.See();
        Aver.IsNotNull(got);
        Aver.IsNotNull(got.NotableMembers);
        Aver.AreEqual(5, got.NotableMembers.Count);
        Aver.AreEqual("Bonderman", got.NotableMembers[0].LastName);
        Aver.AreEqual("Kukusher", got.NotableMembers[4].LastName);
        Aver.AreEqual("Murari", got.FoundingFather.LastName);

        Aver.IsNotNull(got.Shapes);
        Aver.AreEqual(9, got.Shapes.Count);
        Aver.IsNotNull(got.Shapes[0]);
        Aver.IsNotNull(got.Shapes[1]);
        Aver.IsNull(got.Shapes[2]);
        Aver.IsNotNull(got.Shapes[8]);

        Aver.IsTrue(got.Shapes[0] is Circle);
        Aver.IsTrue(got.Shapes[1] is Square);
        Aver.IsTrue(got.Shapes[8] is Square);

        Aver.AreEqual(98, ((Circle)got.Shapes[0]).Radius);
        Aver.AreWithin(92.578f, ((Square)got.Shapes[8]).Side, 0.001f);
      }

      var time = Timeter.StartNew();

      if (par)
        Parallel.For(0, cnt, i => body());
      else
        for (var i = 0; i < cnt; i++) body();

      time.Stop();

      "Did {0:n0} in {1:n1} sec at {2:n0} ops/sec".SeeArgs(cnt, time.ElapsedSec, cnt / time.ElapsedSec);
    }


    public class TypicalPerson : AmorphousTypedDoc
    {
      [Field] public string FirstName { get; set; }
      [Field] public string LastName { get; set; }
      [Field] public string MiddleName { get; set; }

      [Field] public string Address1 { get; set; }
      [Field] public string Address2 { get; set; }
      [Field] public string City { get; set; }
      [Field] public string State { get; set; }
      [Field] public string Zip { get; set; }
      [Field] public string Phone { get; set; }
      [Field] public string Text { get; set; }
      [Field] public string EMail { get; set; }

      [Field] public DateTime? DOB { get; set; }
      [Field] public int? YearsInService { get; set; }
      [Field] public bool? CanVote { get; set; }
      [Field] public bool? CanTravel { get; set; }

      [Field] public double? Score { get; set; }

      [Field] public decimal? Income { get; set; }
      [Field] public decimal? Assets { get; set; }
    }

    public class Family : AmorphousTypedDoc
    {
      [Field] public string Title { get; set; }
      [Field] public int? YearEstablished { get; set; }
      [Field] public List<TypicalPerson> NotableMembers { get; set; }
      [Field] public TypicalPerson FoundingMother { get; set; }
      [Field] public TypicalPerson FoundingFather { get; set; }

      [Field] public List<Shape> Shapes { get; set; }
    }

    [BixJsonHandler(ThrowOnUnresolvedType = true)]
    public abstract class Shape : AmorphousTypedDoc
    {
      [Field] public string Name { get; set; }
      [Field] public string Color { get; set; }

      protected override void AddJsonSerializerField(Schema.FieldDef def, JsonWritingOptions options, Dictionary<string, object> jsonMap, string name, object value)
      {
        if (def.Order==0) BixJsonHandler.EmitJsonBixDiscriminator(this, jsonMap);
        base.AddJsonSerializerField(def, options, jsonMap, name, value);
      }
    }

    [Bix("8c54139e-a437-482b-b015-c8b8bc1d2ad4")]
    public class Circle : Shape
    {
      [Field] public int Radius { get; set; }
    }

    [Bix("749939d7-3256-4c76-92c0-70cf88c3d714")]
    public class Square : Shape
    {
      [Field] public float Side { get; set; }
    }

  }
}

/*
RELEASE .NET 6 4/6/2023
Started 04/06/2023 15:37:27
Starting Azos.Tests.Nub::Azos.Tests.Nub.Serialization.JsonBenchmarkDocTests ...
  - Test_TypicalPerson  {cnt=50000 par=false} Did 50,000 in 1.4 sec at 36,217 ops/sec
[OK]
  - Test_TypicalPerson  {cnt=50000 par=true} [1] Did 50,000 in 0.2 sec at 328,096 ops/sec
[OK]
  - Test_TypicalFamilyWithPolymorphicShapes  {cnt=50000 par=false} Did 50,000 in 4.1 sec at 12,141 ops/sec
[OK]
  - Test_TypicalFamilyWithPolymorphicShapes  {cnt=50000 par=true} [1] Did 50,000 in 0.5 sec at 96,604 ops/sec
[OK]
... done JsonBenchmarkDocTests

RELEASE .Net 6 Started 03/31/2023 12:58:46
Starting Azos.Tests.Nub::Azos.Tests.Nub.Serialization.JsonBenchmarkDocTests ...
  - Test_TypicalPerson  {cnt=50000 par=false} Did 50,000 in 1.3 sec at 38,576 ops/sec
[OK]
  - Test_TypicalPerson  {cnt=50000 par=true} [1] Did 50,000 in 0.2 sec at 327,856 ops/sec
[OK]
  - Test_TypicalFamilyWithPolymorphicShapes  {cnt=50000 par=false} Did 50,000 in 4.0 sec at 12,628 ops/sec
[OK]
  - Test_TypicalFamilyWithPolymorphicShapes  {cnt=50000 par=true} [1] Did 50,000 in 0.5 sec at 101,583 ops/sec
[OK]
... done JsonBenchmarkDocTests


RELEASE Started 03/10/2020 22:26:28
Starting Azos.Tests.Nub::Azos.Tests.Nub.Serialization.JsonBenchmarkDocTests ...
  - Test_TypicalPerson  {cnt=50000 par=false} Did 50,000 in 1.6 sec at 32,185 ops/sec
[OK]
  - Test_TypicalPerson  {cnt=50000 par=true} [1] Did 50,000 in 0.2 sec at 263,860 ops/sec
[OK]

RELEASE Started 03/09/2020 19:53:12
Starting Azos.Tests.Nub::Azos.Tests.Nub.Serialization.JsonBenchmarkDocTests ...
  - Test_TypicalPerson  {cnt=50000 par=false} Did 50,000 in 1.7 sec at 28,730 ops/sec
[OK]
  - Test_TypicalPerson  {cnt=50000 par=true} [1] Did 50,000 in 0.2 sec at 236,023 ops/sec
[OK]


 RELEASE 20200305 before optimization to lambda and other
   - Test_TypicalPerson  {cnt=50000 par=false} Did 50,000 in 2.1 sec at 24,176 ops/sec
[OK]
  - Test_TypicalPerson  {cnt=50000 par=true} [1] Did 50,000 in 0.2 sec at 211,996 ops/sec
[OK]
... done JsonBenchmarkDocTests


*/
