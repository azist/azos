/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Diagnostics;

using Azos.Scripting;
using Azos.Serialization.JSON;
using Azos.Collections;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JSONTests
  {
    [Run]
    public void ReadSimple()
    {
      var obj = "{a: 2}".JsonToDynamic();

      Aver.AreEqual(2, obj.a);
    }

    [Run]
    public void ReadSimple2()
    {
      var obj = "{a: -2, b: true, c: false, d: 'hello'}".JsonToDynamic();

      Aver.AreEqual(-2, obj.a);
      Aver.AreEqual(true, obj.b);
      Aver.AreEqual(false, obj.c);
      Aver.AreEqual("hello", obj.d);
    }

    [Run]
    public void ReadSimpleNameWithSpace()
    {
      var obj = @"{a: -2, 'b or \'': 'yes, ok', c: false, d: 'hello' }".JsonToDynamic();

      Aver.AreEqual(-2, obj.a);
      Aver.AreEqual("yes, ok", obj["b or '"]);
      Aver.AreEqual(false, obj.c);
      Aver.AreEqual("hello", obj.d);
      //Aver.AreEqual(2147483650, obj.e);
    }

    [Run]
    public void JSONDataMapFromURLEncoded_1()
    {
      var map = JsonDataMap.FromUrlEncodedString("name=Alex&title=Professor");

      Aver.AreObjectsEqual("Alex", map["name"]);
      Aver.AreObjectsEqual("Professor", map["title"]);
    }

    [Run]
    public void JSONDataMapFromURLEncoded_2()
    {
      var map = JsonDataMap.FromUrlEncodedString("one=a%2Bb+%3E+123&two=Hello+%26+Welcome.");

      Aver.AreObjectsEqual("a+b > 123", map["one"]);
      Aver.AreObjectsEqual("Hello & Welcome.", map["two"]);
    }

    [Run]
    public void JSONDataMapFromURLEncoded_3()
    {
      var map = JsonDataMap.FromUrlEncodedString("one=a%2Bb+%3E+123+%3D+true&two=Hello+%26+Welcome.%E4%B9%85%E6%9C%89%E5%BD%92%E5%A4%A9%E6%84%BF");

      Aver.AreObjectsEqual("a+b > 123 = true", map["one"]);
      Aver.AreObjectsEqual("Hello & Welcome.久有归天愿", map["two"]);
    }

    [Run]
    public void JSONDataMapFromURLEncoded_PlusSign()
    {
      var map = JsonDataMap.FromUrlEncodedString("a=I+Am John&b=He Is++Not");

      Aver.AreObjectsEqual("I Am John", map["a"]);
      Aver.AreObjectsEqual("He Is  Not", map["b"]);
    }

    [Run]
    public void JSONDataMapFromURLEncoded_PlusAnd20Mix()
    {
      var map = JsonDataMap.FromUrlEncodedString("a=I+Am%20John&b=He%20Is++Not");

      Aver.AreObjectsEqual("I Am John", map["a"]);
      Aver.AreObjectsEqual("He Is  Not", map["b"]);
    }

    [Run]
    public void JSONDataMapFromURLEncoded_Empty()
    {
      Aver.AreEqual(0, JsonDataMap.FromUrlEncodedString(null).Count);
      Aver.AreEqual(0, JsonDataMap.FromUrlEncodedString(string.Empty).Count);
      Aver.AreEqual(0, JsonDataMap.FromUrlEncodedString(" ").Count);
      Aver.AreEqual(0, JsonDataMap.FromUrlEncodedString("\r \n").Count);
      Aver.AreEqual(0, JsonDataMap.FromUrlEncodedString("\t \t ").Count);
    }

    [Run]
    public void JSONDataMapFromURLEncoded_WOAmKey()
    {
      var dict = JsonDataMap.FromUrlEncodedString("a");

      Aver.AreEqual(1, dict.Count);
      Aver.AreObjectsEqual(null, dict["a"]);
    }

    [Run]
    public void JSONDataMapFromURLEncoded_WOAmpKeyVal()
    {
      var dict = JsonDataMap.FromUrlEncodedString("a=1");

      Aver.AreEqual(1, dict.Count);
      Aver.AreObjectsEqual("1", dict["a"]);
    }

    [Run]
    public void JSONDataMapFromURLEncoded_WOAmpVal()
    {
      var dict = JsonDataMap.FromUrlEncodedString("=1");

      Aver.AreEqual(0, dict.Count);
    }

    [Run]
    public void JSONDataMapFromURLEncoded_DoubleEq()
    {
      var dict = JsonDataMap.FromUrlEncodedString("a==1");

      Aver.AreEqual(1, dict.Count);
      Aver.AreObjectsEqual("=1", dict["a"]);
    }

    [Run("query='a=1&b=rrt'")]
    [Run("query='a=1&b=rrt&'")]
    [Run("query='&a=1&b=rrt&'")]
    [Run("query='&&a=1&&b=rrt&&&'")]
    public void JSONDataMapFromURLEncoded_KeyVal(string query)
    {
      var dict = JsonDataMap.FromUrlEncodedString(query);

      Aver.AreEqual(2, dict.Count);
      Aver.AreObjectsEqual("1", dict["a"]);
      Aver.AreObjectsEqual("rrt", dict["b"]);
    }

    [Run]
    public void JSONDataMapFromURLEncoded_KeyEmptyEqNormal()
    {
      var dict = JsonDataMap.FromUrlEncodedString("a=&b&&=&=14&c=3459");

      Aver.AreEqual(3, dict.Count);
      Aver.AreObjectsEqual(string.Empty, dict["a"]);
      Aver.IsNull(dict["b"]);
      Aver.AreObjectsEqual("3459", dict["c"]);
    }

    [Run]
    public void JSONDataMapFromURLEncoded_Esc()
    {
      string[] strs = { " ", "!", "=", "&", "\"zele/m\\h()an\"" };

      foreach (var str in strs)
      {
        var query = "a=" + Uri.EscapeDataString(str);

        var dict = JsonDataMap.FromUrlEncodedString(query);

        Aver.AreEqual(1, dict.Count);
        Aver.AreObjectsEqual(str, dict["a"]);
      }
    }

    [Run]
    public void ReadRootComplexObject()
    {
      var obj = @"
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
".JsonToDynamic();

      Aver.AreEqual("Oleg", obj.FirstName);
      Aver.AreEqual("Ogurtsov", obj.LastName);
      Aver.AreEqual("V.", obj["Middle Name"]);
      Aver.AreEqual("Shamanov", obj["Crazy\nName"]);
      Aver.AreEqual(6, obj.LuckyNumbers.Count);
      Aver.AreEqual(6, obj.LuckyNumbers.List.Count);
      Aver.AreEqual(7, obj.LuckyNumbers[3]);
      Aver.AreEqual("USSR", obj.History[1].Who[1]);
    }

    [Run]
    public void ParallelDeserializationManyComplex()
    {
      const int TOTAL = 1000;
      var src = @"
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
      var watch = Stopwatch.StartNew();

      System.Threading.Tasks.Parallel.For
      (0, TOTAL,
          (i) =>
          {
            var obj = src.JsonToDynamic();
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
      "Long JSON->dynamic deserialization test took {0}ms for {1} objects @ {2}op/sec"
                        .SeeArgs(time, TOTAL, TOTAL / (time / 1000d));
    }

    [Run]
    public void JSONDoubleTest()
    {
      var map = JsonReader.DeserializeDataObject("{ pi: 3.14159265359, exp1: 123e4, exp2: 2e-5 }") as JsonDataMap;
      Aver.AreEqual(3, map.Count);
      Aver.AreObjectsEqual(3.14159265359D, map["pi"]);
      Aver.AreObjectsEqual(123e4D, map["exp1"]);
      Aver.AreObjectsEqual(2e-5D, map["exp2"]);
    }

    [Run]
    public void StringMap_Compact()
    {
      var map = new StringMap
        {
          {"p1", "v1"},
          {"p2", "v2"},
          {"Age", "149"}
        };

      var json = map.ToJson(JsonWritingOptions.Compact);
      json.See();

      Aver.AreEqual(@"{""p1"":""v1"",""p2"":""v2"",""Age"":""149""}", json);

      dynamic read = json.JsonToDynamic();
      Aver.IsNotNull(read);

      Aver.AreEqual("v1", read.p1);
      Aver.AreEqual("v2", read.p2);
      Aver.AreEqual("149", read.Age);
    }

    [Run]
    public void StringMap_Pretty()
    {
      var map = new StringMap
        {
          {"p1", "v1"},
          {"p2", "v2"},
          {"Age", "149"}
        };

      var json = map.ToJson(JsonWritingOptions.PrettyPrint);
      json.See();

      dynamic read = json.JsonToDynamic();
      Aver.IsNotNull(read);

      Aver.AreEqual("v1", read.p1);
      Aver.AreEqual("v2", read.p2);
      Aver.AreEqual("149", read.Age);
    }

  }
}
