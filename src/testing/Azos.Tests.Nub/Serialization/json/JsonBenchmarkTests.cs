using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Scripting;
using Azos.Time;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JsonBenchmarkTests
  {
    [Run("cnt=250000 par=false")]
    [Run("cnt=250000 par=true")]
    public void Test_SimpleObject(int cnt, bool par)
    {
      var json=@"{ a: 1, b: ""something"", c: null, d: {}, e: 23.7}";

      void body()
      {
        var got= JsonReader.DeserializeDataObject(json);
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

    [Run("cnt=150000 par=false")]
    [Run("cnt=150000 par=true")]
    public void Test_ModerateObject(int cnt, bool par)
    {
      var json = @"{ a: 1, b: true, c: 3, d: { a: ""qweqweqwewqeqw"", b: ""werwerwrwrwe6778687"" }, e: [ 1, 2, null, null, 3, 4, {a: 1}, {a: 2}] }";

      void body()
      {
        var got = JsonReader.DeserializeDataObject(json);
        Aver.IsNotNull(got);
      }

      var time = Timeter.StartNew();

      if (par)
        Parallel.For(0, cnt, i => body());
      else
        for (var i = 0; i < cnt; i++) body();

      time.Stop();

      "Did {0:n0} in {1:n1} sec at {2:n0} ops/sec".SeeArgs(cnt, time.ElapsedSec, cnt / time.ElapsedSec);

    }

    [Run("cnt=95000 par=false")]
    [Run("cnt=95000 par=true")]
    public void Test_ComplexObject(int cnt, bool par)
    {
      var json = @"
      { a: 1, b: true, c: 3, 
        d: { a: ""qweqweqwewqeqw"", b: ""werwerwrwrwe6778687"" },
        e: [ 1, 2, null, null, 3, 4, {a: 1}, {a: 2}],
        ""person"": {""first-name"": ""oleg"", ""last-name"": ""ogurzov"", age: 127},
        ""doctor"": {""first-name"": ""Venni"", ""last-name"": ""Smekhov"", age: 27},
        ""patient"": {""first-name"": ""oleg"", ""last-name"": ""popov"", age: 98, prokofiev: { influence: ""shostakovich"", when: ""12 January 1980 3:47 pm""}},
        ""singer"": {""first-name"": ""Alla"", ""last-name"": ""Pugacheva"", age: 127},
        flag1: true,
        flag2: true,
        flag3: null,
        data: {array: [

                     { a: 1, b: true, c: 3, d: { a: ""qweqweqwewqeqw"", b: ""werwerwrwrwe6778687"" }, e: [ 1, 2, null, null, 3, 4, {a: 1}, {a: 2}] },
                     { a: -5, b: false, c: 23, d: { a: ""34 34 5343 34 qweqweqwewqeqw"", b: ""w687"" }, e: [ null, 2, null, null, 3, 4, {a: 1}, {a: 2}] }

                      ]},
        flag4: true,
        flag5: false,
        flag6: true,
        notes: [""aaa"", ""bbb"",""ddd"", null, null, [{a: 1},{},{},{},{},{},[],[2, true, true],[1,0],[]]]
       }";

      void body()
      {
        var got = JsonReader.DeserializeDataObject(json);
        Aver.IsNotNull(got);
      }

      var time = Timeter.StartNew();

      if (par)
        Parallel.For(0, cnt, i => body());
      else
        for (var i = 0; i < cnt; i++) body();

      time.Stop();

      "Did {0:n0} in {1:n1} sec at {2:n0} ops/sec".SeeArgs(cnt, time.ElapsedSec, cnt / time.ElapsedSec);

    }
  }
}

/*

RLEASE MODE
 Started 02/26/2020 21:09:57
Starting Azos.Tests.Nub::Azos.Tests.Nub.Serialization.JsonBenchmarkTests ...
  - Test_SimpleObject  {cnt=250000 par=false} Did 250,000 in 2.0 sec at 126,621 ops/sec
[OK]
  - Test_SimpleObject  {cnt=250000 par=true} [1] Did 250,000 in 0.3 sec at 967,133 ops/sec
[OK]
  - Test_ModerateObject  {cnt=150000 par=false} Did 150,000 in 2.4 sec at 61,476 ops/sec
[OK]
  - Test_ModerateObject  {cnt=150000 par=true} [1] Did 150,000 in 0.3 sec at 487,722 ops/sec
[OK]
  - Test_ComplexObject  {cnt=95000 par=false} Did 95,000 in 9.8 sec at 9,694 ops/sec
[OK]
  - Test_ComplexObject  {cnt=95000 par=true} [1] Did 95,000 in 1.2 sec at 77,224 ops/sec
[OK]


DEBUG MODE
Started 02/26/2020 21:09:04
Starting Azos.Tests.Nub::Azos.Tests.Nub.Serialization.JsonBenchmarkTests ...
  - Test_SimpleObject  {cnt=250000 par=false} Did 250,000 in 4.1 sec at 60,947 ops/sec
[OK]
  - Test_SimpleObject  {cnt=250000 par=true} [1] Did 250,000 in 0.5 sec at 520,612 ops/sec
[OK]
  - Test_ModerateObject  {cnt=150000 par=false} Did 150,000 in 5.4 sec at 27,704 ops/sec
[OK]
  - Test_ModerateObject  {cnt=150000 par=true} [1] Did 150,000 in 0.6 sec at 232,450 ops/sec
[OK]
  - Test_ComplexObject  {cnt=95000 par=false} Did 95,000 in 24.7 sec at 3,847 ops/sec
[OK]
  - Test_ComplexObject  {cnt=95000 par=true} [1] Did 95,000 in 2.9 sec at 32,917 ops/sec
[OK]


*/
