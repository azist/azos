/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Scripting;
using Azos.CodeAnalysis;
using Azos.CodeAnalysis.Source;
using Azos.CodeAnalysis.JSON;
using Azos.Serialization.JSON;
using JL = Azos.CodeAnalysis.JSON.JsonLexer;
using JP = Azos.CodeAnalysis.JSON.JsonParser;

namespace Azos.Tests.Nub.CodeAnalysis
{
  [Runnable]
  public class JSONParserTests
  {
    [Run]
    public void RootLiteral_String()
    {
      var src = @"'abc'";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.AreObjectsEqual("abc", parser.ResultContext.ResultObject);
    }

    [Run]
    public void RootLiteral_Int()
    {
      var src = @"12";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.AreObjectsEqual(12, parser.ResultContext.ResultObject);
    }

    [Run]
    public void RootLiteral_NegativeDecimalInt()
    {
      var src = @"-16";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.AreObjectsEqual(-16, parser.ResultContext.ResultObject);
    }

    [Run]
    public void RootLiteral_PositiveDecimalInt()
    {
      var src = @"+16";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.AreObjectsEqual(16, parser.ResultContext.ResultObject);
    }

    [Run]
    public void RootLiteral_NegativeHexInt()
    {
      var src = @"-0xf";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.AreObjectsEqual(-15, parser.ResultContext.ResultObject);
    }

    [Run]
    public void RootLiteral_PositiveHexInt()
    {
      var src = @"+0xf";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.AreObjectsEqual(15, parser.ResultContext.ResultObject);
    }

    [Run]
    public void RootLiteral_Double()
    {
      var src = @"12.7";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.AreObjectsEqual(12.7, parser.ResultContext.ResultObject);
    }

    [Run]
    public void RootLiteral_NegativeDouble()
    {
      var src = @"-12.7";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.AreObjectsEqual(-12.7, parser.ResultContext.ResultObject);
    }

    [Run]
    public void RootLiteral_PositiveDouble()
    {
      var src = @"+12.7";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.AreObjectsEqual(12.7, parser.ResultContext.ResultObject);
    }

    [Run]
    public void RootLiteral_ScientificDouble()
    {
      var src = @"12e2";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.AreObjectsEqual(12e2d, parser.ResultContext.ResultObject);
    }

    [Run]
    public void RootLiteral_NegativeScientificDouble()
    {
      var src = @"-12e2";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.AreObjectsEqual(-12e2d, parser.ResultContext.ResultObject);
    }

    [Run]
    public void RootLiteral_PositiveScientificDouble()
    {
      var src = @"+12e2";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.AreObjectsEqual(12e2d, parser.ResultContext.ResultObject);
    }

    [Run]
    public void RootLiteral_True()
    {
      var src = @"true";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.AreObjectsEqual(true, parser.ResultContext.ResultObject);
    }

    [Run]
    public void RootLiteral_False()
    {
      var src = @"false";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.AreObjectsEqual(false, parser.ResultContext.ResultObject);
    }

    [Run]
    public void RootLiteral_Null()
    {
      var src = @"null";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.IsNull(parser.ResultContext.ResultObject);
    }

    [Run]
    public void RootArray()
    {
      var src = @"[1,2,3]";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.IsTrue(parser.ResultContext.ResultObject is JsonDataArray);
      var arr = (JsonDataArray)parser.ResultContext.ResultObject;

      Aver.AreEqual(3, arr.Count);
      Aver.AreObjectsEqual(1, arr[0]);
      Aver.AreObjectsEqual(2, arr[1]);
      Aver.AreObjectsEqual(3, arr[2]);
    }

    [Run]
    public void RootEmptyArray()
    {
      var src = @"[]";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.IsTrue(parser.ResultContext.ResultObject is JsonDataArray);
      var arr = (JsonDataArray)parser.ResultContext.ResultObject;

      Aver.AreEqual(0, arr.Count);
    }

    [Run]
    public void RootObject()
    {
      var src = @"{a: 1, b: true, c: null}";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.IsTrue(parser.ResultContext.ResultObject is JsonDataMap);
      var obj = (JsonDataMap)parser.ResultContext.ResultObject;

      Aver.AreEqual(3, obj.Count);
      Aver.AreObjectsEqual(1, obj["a"]);
      Aver.AreObjectsEqual(true, obj["b"]);
      Aver.IsNull(obj["c"]);
    }

    [Run]
    public void RootEmptyObject()
    {
      var src = @"{}";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.IsTrue(parser.ResultContext.ResultObject is JsonDataMap);
      var obj = (JsonDataMap)parser.ResultContext.ResultObject;

      Aver.AreEqual(0, obj.Count);
    }

    [Run]
    public void RootObjectWithArray()
    {
      var src = @"{age: 12, numbers: [4,5,6,7,8,9], name: ""Vasya""}";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.IsTrue(parser.ResultContext.ResultObject is JsonDataMap);
      var obj = (JsonDataMap)parser.ResultContext.ResultObject;

      Aver.AreEqual(3, obj.Count);
      Aver.AreObjectsEqual(12, obj["age"]);
      Aver.AreObjectsEqual("Vasya", obj["name"]);
      Aver.AreObjectsEqual(6, ((JsonDataArray)obj["numbers"]).Count);
      Aver.AreObjectsEqual(7, ((JsonDataArray)obj["numbers"])[3]);
    }

    [Run]
    public void RootObjectWithSubObjects()
    {
      var src = @"{age: 120, numbers: {positive: true, bad: 12.7}, name: ""Vasya""}";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.IsTrue(parser.ResultContext.ResultObject is JsonDataMap);
      var obj = (JsonDataMap)parser.ResultContext.ResultObject;

      Aver.AreEqual(3, obj.Count);
      Aver.AreObjectsEqual(120, obj["age"]);
      Aver.AreObjectsEqual("Vasya", obj["name"]);
      Aver.AreObjectsEqual(true, ((JsonDataMap)obj["numbers"])["positive"]);
      Aver.AreObjectsEqual(12.7, ((JsonDataMap)obj["numbers"])["bad"]);
    }

    [Run]
    public void ParseError1()
    {
      var src = @"{age 120}";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.AreEqual(1, parser.Messages.Count);
      Aver.AreEqual((int)JsonMsgCode.eColonOperatorExpected, parser.Messages[0].Code);
    }

    [Run]
    [Aver.Throws(typeof(CodeProcessorException), Message = "eColonOperatorExpected")]
    public void ParseError2()
    {
      var src = @"{age 120}";

      var parser = new JP(new JL(new StringSource(src)), throwErrors: true);

      parser.Parse();
    }

    [Run]
    [Aver.Throws(typeof(CodeProcessorException), Message = "ePrematureEOF")]
    public void ParseError3()
    {
      var src = @"{age: 120";

      var parser = new JP(new JL(new StringSource(src)), throwErrors: true);

      parser.Parse();
    }

    [Run]
    [Aver.Throws(typeof(CodeProcessorException), Message = "eUnterminatedObject")]
    public void ParseError4()
    {
      var src = @"{age: 120 d";

      var parser = new JP(new JL(new StringSource(src)), throwErrors: true);

      parser.Parse();
    }

    [Run]
    [Aver.Throws(typeof(CodeProcessorException), Message = "eUnterminatedArray")]
    public void ParseError5()
    {
      var src = @"['age', 120 d";

      var parser = new JP(new JL(new StringSource(src)), throwErrors: true);

      parser.Parse();
    }

    [Run]
    [Aver.Throws(typeof(CodeProcessorException), Message = "eSyntaxError")]
    public void ParseError6()
    {
      var src = @"[age: 120 d";

      var parser = new JP(new JL(new StringSource(src)), throwErrors: true);

      parser.Parse();
    }

    [Run]
    [Aver.Throws(typeof(CodeProcessorException), Message = "eObjectKeyExpected")]
    public void ParseError7()
    {
      var src = @"{ true: 120}";

      var parser = new JP(new JL(new StringSource(src)), throwErrors: true);

      parser.Parse();
    }

    [Run]
    [Aver.Throws(typeof(CodeProcessorException), Message = "eDuplicateObjectKey")]
    public void ParseError8()
    {
      var src = @"{ a: 120, b: 140, a: 12}";

      var parser = new JP(new JL(new StringSource(src)), throwErrors: true);

      parser.Parse();
    }

    [Run]
    public void RootComplexObject()
    {
      var src =
@"
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
    #HOT_TOPIC
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

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.IsTrue(parser.ResultContext.ResultObject is JsonDataMap);
      var obj = (JsonDataMap)parser.ResultContext.ResultObject;

      Aver.AreEqual(7, obj.Count);
      Aver.AreObjectsEqual("Oleg", obj["FirstName"]);
      Aver.AreObjectsEqual("Ogurtsov", obj["LastName"]);
      Aver.AreObjectsEqual("V.", obj["Middle Name"]);
      Aver.AreObjectsEqual("Shamanov", obj["Crazy\nName"]);

      var lucky = obj["LuckyNumbers"] as JsonDataArray;
      Aver.IsNotNull(lucky);
      Aver.AreEqual(6, lucky.Count);
      Aver.AreObjectsEqual(4, lucky[0]);
      Aver.AreObjectsEqual(9, lucky[5]);

      var history = obj["History"] as JsonDataArray;
      Aver.IsNotNull(history);
      Aver.AreEqual(2, history.Count);

      var ww2 = history[1] as JsonDataMap;
      Aver.IsNotNull(ww2);
      Aver.AreEqual(3, ww2.Count);

      var who = ww2["Who"] as JsonDataArray;
      Aver.IsNotNull(who);
      Aver.AreEqual(6, who.Count);
      Aver.AreObjectsEqual("USA", who[2]);
    }

    [Run]
    public void RootComplexArray()
    {
      var src =
@"[
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
    #HOT_TOPIC
    {Date: '05/14/1905', What: 'Tsushima'},
    #MODERN_TOPIC
    {Date: '09/01/1939', What: 'WW2 Started', Who: ['Germany','USSR', 'USA', 'Japan', 'Italy', 'Others']}
  ] ,
  Note:
$'This note text
can span many lines
and
this \r\n is not escape'
 },
 123
]";

      var parser = new JP(new JL(new StringSource(src)));

      parser.Parse();

      Aver.IsTrue(parser.ResultContext.ResultObject is JsonDataArray);
      var arr = (JsonDataArray)parser.ResultContext.ResultObject;
      Aver.AreEqual(2, arr.Count);
      Aver.AreObjectsEqual(123, arr[1]);

      var obj = (JsonDataMap)arr[0];

      Aver.AreEqual(7, obj.Count);
      Aver.AreObjectsEqual("Oleg", obj["FirstName"]);
      Aver.AreObjectsEqual("Ogurtsov", obj["LastName"]);
      Aver.AreObjectsEqual("V.", obj["Middle Name"]);
      Aver.AreObjectsEqual("Shamanov", obj["Crazy\nName"]);

      var lucky = obj["LuckyNumbers"] as JsonDataArray;
      Aver.IsNotNull(lucky);
      Aver.AreEqual(6, lucky.Count);
      Aver.AreObjectsEqual(4, lucky[0]);
      Aver.AreObjectsEqual(9, lucky[5]);

      var history = obj["History"] as JsonDataArray;
      Aver.IsNotNull(history);
      Aver.AreEqual(2, history.Count);

      var ww2 = history[1] as JsonDataMap;
      Aver.IsNotNull(ww2);
      Aver.AreEqual(3, ww2.Count);

      var who = ww2["Who"] as JsonDataArray;
      Aver.IsNotNull(who);
      Aver.AreEqual(6, who.Count);
      Aver.AreObjectsEqual("USA", who[2]);
    }

  }
}
