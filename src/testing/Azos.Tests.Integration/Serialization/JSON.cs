/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Threading.Tasks;

using Azos.Scripting;

using Azos.IO;
using Azos.CodeAnalysis;
using Azos.CodeAnalysis.Source;
using Azos.CodeAnalysis.JSON;
using Azos.Serialization.JSON;
using JL=Azos.CodeAnalysis.JSON.JSONLexer;
using JP=Azos.CodeAnalysis.JSON.JSONParser;
using JW=Azos.Serialization.JSON.JsonWriter;
using JDO=Azos.Serialization.JSON.JsonDynamicObject;



namespace Azos.Tests.Integration.Serialization
{


    [Runnable]
    public class JSON
    {
        private const int BENCHMARK_ITERATIONS = 10000;//250000;

        private const string SOURCE1 = @"
 {FirstName: ""Oleg"",  //comments dont hurt
  'LastName': ""Ogurtsov"",
  ""Middle Name"": 'V.',
  ""Crazy\nName"": 'Shamanov',
  LuckyNumbers: [4,5,6,7,8,9],
  /* comments
  do not break stuff */
  |* in this JSON superset *|
  History:
  [
    #HOT_TOPIC    //ability to use directive pragmas
    {Date: '05/14/1905', What: 'Tsushima'},
    #MODERN_TOPIC
    {Date: '09/01/1939', What: 'WW2 Started', Who: ['Germany','USSR', 'USA', 'Japan', 'Italy', 'Others']}
  ] ,
  Note:
$'This note text
can span many lines
and
this \r\n is not escape'
 }
";

private const string SOURCE2 = @"
 {FirstName: ""Oleg"",
  ""LastName"": ""Ogurtsov"",
  ""Middle Name"": ""V."",
  ""Crazy\nName"": ""Shamanov"",
  LuckyNumbers: [4,5,6,7,8,9],
  History:
  [
    {Date: ""05/14/1905"", What: ""Tsushima""},
    {Date: ""09/01/1939"", What: ""WW2 Started"", Who: [""Germany"",""USSR"", ""USA"", ""Japan"", ""Italy"", ""Others""]}
  ] ,
  Note: ""This note text can span many lines""
 }
";




        [Run]
        public void ParallelDeserializationOfManyComplexObjects_JSONPlus()
        {
          parallelDeserializationOfManyComplexObjects(SOURCE1, 12);
        }

         [Run]
        public void ParallelDeserializationOfManyComplexObjects_JSONRegular()
        {
          parallelDeserializationOfManyComplexObjects(SOURCE2, 12);
        }


        private void parallelDeserializationOfManyComplexObjects(string src, int threadCount)
        {
            const int TOTAL = 1000000;//100;//500000;

            var watch = Stopwatch.StartNew();

            System.Threading.Tasks.Parallel.For
            (0, TOTAL,
                new ParallelOptions(){ MaxDegreeOfParallelism = threadCount},
                (i)=>
                {
                    var obj = src.JsonToDynamic();
              //      var obj = System.Web.Helpers.Json.Decode(src);
                    Aver.AreEqual("Oleg", obj.FirstName);
                    Aver.AreEqual("Ogurtsov", obj.LastName);
                    Aver.AreEqual("V.", obj["Middle Name"]);
                    Aver.AreEqual("Shamanov", obj["Crazy\nName"]);
                    Aver.AreEqual(6, obj.LuckyNumbers.Count);
                    Aver.AreEqual(6, obj.LuckyNumbers.List.Count);
                    Aver.AreEqual(7, obj.LuckyNumbers[3]);
                    Aver.AreEqual("USSR", obj.History[1].Who[1]);
                }
            );

            var time = watch.ElapsedMilliseconds;
            Console.WriteLine("Long JSON->dynamic deserialization test took {0}ms for {1} objects @ {2}op/sec"
                              .Args(time, TOTAL, TOTAL / (time / 1000d))
                              );
        }


        [Run]
        public void Benchmark_Serialize_PersonClass()
        {
          var data = new APerson{ FirstName = "Dima", LastName="Sokolov", Age = 16, DOB = DateTime.Now};
          serializeBenchmark("PersonClass", data);
        }

        [Run]
        public void Benchmark_Serialize_DataObjectClass()
        {
          var data = new DataObject{ fBool = true, fBoolNullable = null, fByteArray = new byte[128], fDateTime = DateTime.Now, fDouble = 122.03,
                                     fFloat = 12f, fString = "my message", fStringArray = new string[128], fDecimal = 121.12m, fULong= 2312 };
          serializeBenchmark("DataObjectClass", data);
        }

        [Run]
        public void Benchmark_Serialize_ListPrimitive()
        {
          var data = new List<object>{ 1, true, 12.34, "yes!", DateTime.Now, 'a'};
          serializeBenchmark("ListPrimitive", data);
        }

        [Run]
        public void Benchmark_Serialize_DictionaryPrimitive()
        {
          var data = new Dictionary<string, object>{ {"a",1} , {"b",true}, {"c", 12.34}, {"Message","yes!"}, {"when",DateTime.Now}, {"status",'a'}};
          serializeBenchmark("DictionaryPrimitive", data);
        }


        [Run]
        public void Benchmark_Serialize_ListObjects()
        {
          var p1 = new APerson{ FirstName = "Dima", LastName="Sokolov", Age = 16, DOB = DateTime.Now};
          var p2 = new APerson{ FirstName = "Fima", LastName="Orloff", Age = 23, DOB = DateTime.Now};
          var p3 = new APerson{ FirstName = "Dodik", LastName="Stevenson", Age = 99, DOB = DateTime.Now};
          var data = new List<object>{ 1, true, p1, 12.34, p2, "yes!", p3, DateTime.Now, 'a'};
          serializeBenchmark("ListObjects", data);
        }














            private void serializeBenchmark(string name, object data)
            {
                  var sw = Stopwatch.StartNew();

                  for (var i=0; i<BENCHMARK_ITERATIONS;i++)
                  {
                    //Console.WriteLine( JW.Write(data) );
                    JW.Write(data);
                  }

                  var nfxTime =  sw.ElapsedMilliseconds;

                  //---DataContractJsonSerializer
                  var dcs = new DataContractJsonSerializer( data.GetType());

                  sw = Stopwatch.StartNew();
                  var ms = new MemoryStream();
                  var dcsTime =  long.MaxValue;
                  try
                  {
                      for (var i=0; i<BENCHMARK_ITERATIONS;i++)
                      {
                         dcs.WriteObject( ms, data);
                    //     Console.WriteLine( Encoding.Default.GetString(ms.ToArray()));
                    //     throw new Exception();

                         ms.Position = 0;
                      }
                      dcsTime =  sw.ElapsedMilliseconds;
                  }
                  catch(Exception error)
                  {
                    Console.WriteLine("DataContractJSONSerializer does not support this test case: " + error.ToMessageWithType());
                  }


              var _nfx = BENCHMARK_ITERATIONS / (nfxTime /1000d);
              var _dcs =  BENCHMARK_ITERATIONS / (dcsTime /1000d);
              Console.WriteLine(
@"Serialize.{0}
    Azos: {1} op/sec 
    MS DataContractSer: {2} op/sec
    Ratio Azos/DC: {3} 
     ".Args(name, _nfx, _dcs, _nfx / _dcs));

            }


    }
}
