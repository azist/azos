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

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JsonBenchmarkDocTests
  {
    static JsonBenchmarkDocTests()
    {
      JsonReader.____SetReaderBackend(new Azos.Serialization.JSON.Backends.JazonReaderBackend());
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
    }

  }
}

/*

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
