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

using Azos.Scripting;

using Azos.IO;
using Azos.Serialization.JSON;
using Azos.Collections;




namespace Azos.Tests.Unit.Serialization
{
    [Runnable(TRUN.BASE)]
    public class JSON
    {
        [Run]
        public void ReadSimple()
        {
            var obj = "{a: 2}".JSONToDynamic();

            Aver.AreEqual(2, obj.a);

        }

        [Run]
        public void ReadSimple2()
        {
            var obj = "{a: -2, b: true, c: false, d: 'hello'}".JSONToDynamic();

            Aver.AreEqual(-2, obj.a);
            Aver.AreEqual(true, obj.b);
            Aver.AreEqual(false, obj.c);
            Aver.AreEqual("hello", obj.d);

        }

        [Run]
        public void ReadSimpleNameWithSpace()
        {
            var obj = @"{a: -2, 'b or \'': 'yes, ok', c: false, d: 'hello' }".JSONToDynamic();

            Aver.AreEqual(-2, obj.a);
            Aver.AreEqual("yes, ok", obj["b or '"]);
            Aver.AreEqual(false, obj.c);
            Aver.AreEqual("hello", obj.d);
            //Aver.AreEqual(2147483650, obj.e);
        }







        [Run]
        public void JSONDataMapFromURLEncoded_1()
        {
            var map = JSONDataMap.FromURLEncodedString("name=Alex&title=Professor");

            Aver.AreObjectsEqual("Alex", map["name"]);
            Aver.AreObjectsEqual("Professor", map["title"]);
        }

        [Run]
        public void JSONDataMapFromURLEncoded_2()
        {
            var map = JSONDataMap.FromURLEncodedString("one=a%2Bb+%3E+123&two=Hello+%26+Welcome.");

            Aver.AreObjectsEqual("a+b > 123", map["one"]);
            Aver.AreObjectsEqual("Hello & Welcome.", map["two"]);
        }

        [Run]
        public void JSONDataMapFromURLEncoded_3()
        {
            var map = JSONDataMap.FromURLEncodedString("one=a%2Bb+%3E+123+%3D+true&two=Hello+%26+Welcome.%E4%B9%85%E6%9C%89%E5%BD%92%E5%A4%A9%E6%84%BF");

            Aver.AreObjectsEqual("a+b > 123 = true", map["one"]);
            Aver.AreObjectsEqual("Hello & Welcome.久有归天愿", map["two"]);
        }

        [Run]
        public void JSONDataMapFromURLEncoded_PlusSign()
        {
            var map = JSONDataMap.FromURLEncodedString("a=I+Am John&b=He Is++Not");

            Aver.AreObjectsEqual("I Am John", map["a"]);
            Aver.AreObjectsEqual("He Is  Not", map["b"]);
        }

        [Run]
        public void JSONDataMapFromURLEncoded_PlusAnd20Mix()
        {
            var map = JSONDataMap.FromURLEncodedString("a=I+Am%20John&b=He%20Is++Not");

            Aver.AreObjectsEqual("I Am John", map["a"]);
            Aver.AreObjectsEqual("He Is  Not", map["b"]);
        }

        [Run]
        public void JSONDataMapFromURLEncoded_Empty()
        {
          Aver.AreEqual(0, JSONDataMap.FromURLEncodedString(null).Count);
          Aver.AreEqual(0, JSONDataMap.FromURLEncodedString(string.Empty).Count);
          Aver.AreEqual(0, JSONDataMap.FromURLEncodedString(" ").Count);
          Aver.AreEqual(0, JSONDataMap.FromURLEncodedString("\r \n").Count);
          Aver.AreEqual(0, JSONDataMap.FromURLEncodedString("\t \t ").Count);
        }

        [Run]
        public void JSONDataMapFromURLEncoded_WOAmKey()
        {
          var dict = JSONDataMap.FromURLEncodedString("a");

          Aver.AreEqual(1, dict.Count);
          Aver.AreObjectsEqual(null, dict["a"]);
        }

        [Run]
        public void JSONDataMapFromURLEncoded_WOAmpKeyVal()
        {
          var dict = JSONDataMap.FromURLEncodedString("a=1");

          Aver.AreEqual(1, dict.Count);
          Aver.AreObjectsEqual("1", dict["a"]);
        }

        [Run]
        public void JSONDataMapFromURLEncoded_WOAmpVal()
        {
          var dict = JSONDataMap.FromURLEncodedString("=1");

          Aver.AreEqual(0, dict.Count);
        }

        [Run]
        public void JSONDataMapFromURLEncoded_DoubleEq()
        {
          var dict = JSONDataMap.FromURLEncodedString("a==1");

          Aver.AreEqual(1, dict.Count);
          Aver.AreObjectsEqual("=1", dict["a"]);
        }

        [Run("query='a=1&b=rrt'")]
        [Run("query='a=1&b=rrt&'")]
        [Run("query='&a=1&b=rrt&'")]
        [Run("query='&&a=1&&b=rrt&&&'")]
        public void JSONDataMapFromURLEncoded_KeyVal(string query)
        {
          var dict = JSONDataMap.FromURLEncodedString(query);

          Aver.AreEqual(2, dict.Count);
          Aver.AreObjectsEqual("1", dict["a"]);
          Aver.AreObjectsEqual("rrt", dict["b"]);
        }

        [Run]
        public void JSONDataMapFromURLEncoded_KeyEmptyEqNormal()
        {
          var dict = JSONDataMap.FromURLEncodedString("a=&b&&=&=14&c=3459");

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

            var dict = JSONDataMap.FromURLEncodedString(query);

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
".JSONToDynamic();

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
                (i)=>
                {
                    var obj = src.JSONToDynamic();
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
        public void JSONDoubleTest()
        {
            var map = JSONReader.DeserializeDataObject( "{ pi: 3.14159265359, exp1: 123e4, exp2: 2e-5 }" ) as JSONDataMap;
            Aver.AreEqual(3, map.Count);
            Aver.AreObjectsEqual(3.14159265359D, map["pi"]);
            Aver.AreObjectsEqual(123e4D, map["exp1"]);
            Aver.AreObjectsEqual(2e-5D, map["exp2"]);
        }



        [Run]
        public void NLSMap_Basic1_String()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}}";

            var nls = new NLSMap(content);

            Aver.IsTrue (nls["eng"].IsAssigned);
            Aver.IsFalse(nls["rus"].IsAssigned);

            Aver.AreEqual("Cucumber", nls["eng"].Name);
            Aver.AreEqual(null, nls["rus"].Name);

            Aver.AreEqual("It is green", nls["eng"].Description);
            Aver.AreEqual(null, nls["rus"].Description);
        }

        [Run]
        public void NLSMap_Basic1_Config()
        {
            var content="nls{ eng{n='Cucumber' d='It is green'}}".AsLaconicConfig();

            var nls = new NLSMap(content);

            Aver.IsTrue (nls["eng"].IsAssigned);
            Aver.IsFalse(nls["rus"].IsAssigned);

            Aver.AreEqual("Cucumber", nls["eng"].Name);
            Aver.AreEqual(null, nls["rus"].Name);

            Aver.AreEqual("It is green", nls["eng"].Description);
            Aver.AreEqual(null, nls["rus"].Description);
        }


        [Run]
        public void NLSMap_Basic2()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";

            var nls = new NLSMap(content);

            Aver.IsTrue (nls["eng"].IsAssigned);
            Aver.IsTrue (nls["deu"].IsAssigned);
            Aver.IsFalse(nls["rus"].IsAssigned);

            Aver.AreEqual("Cucumber", nls["eng"].Name);
            Aver.AreEqual("Gurke", nls["deu"].Name);

            Aver.AreEqual("It is green", nls["eng"].Description);
            Aver.AreEqual("Es ist grün", nls["deu"].Description);
        }

        [Run]
        public void NLSMap_OverrideBy()
        {
            var content1="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";
            var nls1 = new NLSMap(content1);

            var content2="{eng: {n: 'Cacamber',d: 'It is brown'}, rus: {n: 'Ogurez', d: 'On zeleniy'}}";
            var nls2 = new NLSMap(content2);

            var nls = nls1.OverrideBy(nls2);

            Aver.AreEqual(3, nls.Count);
            Aver.AreEqual("Cacamber", nls["eng"].Name);
            Aver.AreEqual("Gurke", nls["deu"].Name);
            Aver.AreEqual("Ogurez", nls["rus"].Name);
        }

        [Run]
        public void NLSMap_OverrideByEmpty1()
        {
            var nls1 = new NLSMap();

            var content2="{eng: {n: 'Cacamber',d: 'It is brown'}, rus: {n: 'Ogurez', d: 'On zeleniy'}}";
            var nls2 = new NLSMap(content2);

            var nls = nls1.OverrideBy(nls2);

            Aver.AreEqual(2, nls.Count);
            Aver.AreEqual("Cacamber", nls["eng"].Name);
            Aver.AreEqual(null, nls["deu"].Name);
            Aver.AreEqual("Ogurez", nls["rus"].Name);
        }

        [Run]
        public void NLSMap_OverrideByEmpty2()
        {
             var content1="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";
            var nls1 = new NLSMap(content1);

            var nls2 = new NLSMap();

            var nls = nls1.OverrideBy(nls2);

            Aver.AreEqual(2, nls.Count);
            Aver.AreEqual("Cucumber", nls["eng"].Name);
            Aver.AreEqual("Gurke", nls["deu"].Name);
            Aver.AreEqual(null, nls["rus"].Name);
        }


        [Run]
        public void NLSMap_SerializeAll()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";

            var nls = new NLSMap(content);

            var json = nls.ToJSON();
            Console.WriteLine(json);

            dynamic read = json.JSONToDynamic();
            Aver.IsNotNull(read);

            Aver.AreEqual("Cucumber", read.ENG.n);
            Aver.AreEqual("Gurke", read.DEU.n);
        }

        [Run]
        public void NLSMap_SerializeOnlyOneExisting()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";

            var nls = new NLSMap(content);


            var options = new JSONWritingOptions{ NLSMapLanguageISO = "deu", Purpose = JSONSerializationPurpose.UIFeed};
            var json = nls.ToJSON(options);
            Console.WriteLine(json);

            dynamic read = json.JSONToDynamic();
            Aver.IsNotNull(read);

            Aver.AreEqual("Gurke", read.n);
            Aver.AreEqual("Es ist grün", read.d);
        }

        [Run]
        public void NLSMap_SerializeOnlyOneNoneExisting()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";

            var nls = new NLSMap(content);


            var options = new JSONWritingOptions{ NLSMapLanguageISO = "rus", Purpose = JSONSerializationPurpose.UIFeed};
            var json = nls.ToJSON(options);
            Console.WriteLine(json);

            dynamic read = json.JSONToDynamic();
            Aver.IsNotNull(read);

            Aver.AreEqual("Cucumber", read.n);
            Aver.AreEqual("It is green", read.d);
        }


         [Run]
        public void NLSMap_JSONSerializationRoundtrip()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";

            var nls = new NLSMap(content);


            var json = nls.ToJSON();
            Console.WriteLine(json);

            var nls2 = new NLSMap(json);

            Aver.AreEqual(2, nls2.Count);
            Aver.AreEqual("Cucumber", nls2["eng"].Name);
            Aver.AreEqual("Gurke", nls2["deu"].Name);
            Aver.AreEqual(null, nls["rus"].Name);
        }


        [Run]
        public void NLSMap_Get_Name()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";

            var nls = new NLSMap(content);

            var name = nls.Get(NLSMap.GetParts.Name);
            Aver.AreEqual("Cucumber", name);

            name = nls.Get(NLSMap.GetParts.Name, "deu");
            Aver.AreEqual("Gurke", name);

            name = nls.Get(NLSMap.GetParts.Name, "XXX");
            Aver.AreEqual("Cucumber", name);

            name = nls.Get(NLSMap.GetParts.Name, "XXX", dfltLangIso: "ZZZ");
            Aver.IsNull(name);
        }

        [Run]
        public void NLSMap_Get_Descr()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";

            var nls = new NLSMap(content);

            var descr = nls.Get(NLSMap.GetParts.Description);
            Aver.AreEqual("It is green", descr);

            descr = nls.Get(NLSMap.GetParts.Description, "deu");
            Aver.AreEqual("Es ist grün", descr);

            descr = nls.Get(NLSMap.GetParts.Description, "XXX");
            Aver.AreEqual("It is green", descr);

            descr = nls.Get(NLSMap.GetParts.Description, "XXX", dfltLangIso: "ZZZ");
            Aver.IsNull(descr);
        }

        [Run]
        public void NLSMap_Get_NameOrDescr()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}, rus: { d: 'On Zeleniy'}}";

            var nls = new NLSMap(content);

            var nord = nls.Get(NLSMap.GetParts.NameOrDescription);
            Aver.AreEqual("Cucumber", nord);

            nord = nls.Get(NLSMap.GetParts.NameOrDescription, "deu");
            Aver.AreEqual("Gurke", nord);

            nord = nls.Get(NLSMap.GetParts.NameOrDescription, "XXX");
            Aver.AreEqual("Cucumber", nord);

            nord = nls.Get(NLSMap.GetParts.NameOrDescription, "rus");
            Aver.AreEqual("On Zeleniy", nord);

            nord = nls.Get(NLSMap.GetParts.NameOrDescription, "XXX", dfltLangIso: "ZZZ");
            Aver.IsNull(nord);
        }

        [Run]
        public void NLSMap_Get_DescrOrName()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}, rus: { d: 'On Zeleniy'}}";

            var nls = new NLSMap(content);

            var dorn = nls.Get(NLSMap.GetParts.DescriptionOrName);
            Aver.AreEqual("It is green", dorn);

            dorn = nls.Get(NLSMap.GetParts.DescriptionOrName, "deu");
            Aver.AreEqual("Es ist grün", dorn);

            dorn = nls.Get(NLSMap.GetParts.DescriptionOrName, "XXX");
            Aver.AreEqual("It is green", dorn);

            dorn = nls.Get(NLSMap.GetParts.DescriptionOrName, "rus");
            Aver.AreEqual("On Zeleniy", dorn);

            dorn = nls.Get(NLSMap.GetParts.DescriptionOrName, "XXX", dfltLangIso: "ZZZ");
            Aver.IsNull(dorn);
        }


        [Run]
        public void NLSMap_Get_NameAndDescr()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}, rus: { d: 'On Zeleniy'}}";

            var nls = new NLSMap(content);

            var nand = nls.Get(NLSMap.GetParts.NameAndDescription);
            Aver.AreEqual("Cucumber - It is green", nand);

            nand = nls.Get(NLSMap.GetParts.NameAndDescription, "deu");
            Aver.AreEqual("Gurke - Es ist grün", nand);

            nand = nls.Get(NLSMap.GetParts.NameAndDescription, "XXX");
            Aver.AreEqual("Cucumber - It is green", nand);

             nand = nls.Get(NLSMap.GetParts.NameAndDescription, "YYY", concat: "::");
            Aver.AreEqual("Cucumber::It is green", nand);

            nand = nls.Get(NLSMap.GetParts.NameAndDescription, "rus");
            Aver.AreEqual("On Zeleniy", nand);

            nand = nls.Get(NLSMap.GetParts.NameAndDescription, "XXX", dfltLangIso: "ZZZ");
            Aver.IsNull(nand);
        }

        [Run]
        public void NLSMap_Get_DescrAndName()
        {
            var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}, rus: { d: 'On Zeleniy'}}";

            var nls = new NLSMap(content);

            var dan = nls.Get(NLSMap.GetParts.DescriptionAndName);
            Aver.AreEqual("It is green - Cucumber", dan);

            dan = nls.Get(NLSMap.GetParts.DescriptionAndName, "deu");
            Aver.AreEqual("Es ist grün - Gurke", dan);

            dan = nls.Get(NLSMap.GetParts.DescriptionAndName, "XXX");
            Aver.AreEqual("It is green - Cucumber", dan);

             dan = nls.Get(NLSMap.GetParts.DescriptionAndName, "YYY", concat: "::");
            Aver.AreEqual("It is green::Cucumber", dan);

            dan = nls.Get(NLSMap.GetParts.DescriptionAndName, "rus");
            Aver.AreEqual("On Zeleniy", dan);

            dan = nls.Get(NLSMap.GetParts.DescriptionAndName, "XXX", dfltLangIso: "ZZZ");
            Aver.IsNull(dan);
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

            var json = map.ToJSON(JSONWritingOptions.Compact);
            Console.WriteLine(json);

            Aver.AreEqual(@"{""p1"":""v1"",""p2"":""v2"",""Age"":""149""}",json);


            dynamic read = json.JSONToDynamic();
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

            var json = map.ToJSON(JSONWritingOptions.PrettyPrint);
            Console.WriteLine(json);

            dynamic read = json.JSONToDynamic();
            Aver.IsNotNull(read);

            Aver.AreEqual("v1", read.p1);
            Aver.AreEqual("v2", read.p2);
            Aver.AreEqual("149", read.Age);
        }


    }
}
