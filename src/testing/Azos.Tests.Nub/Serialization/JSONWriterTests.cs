/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Azos.Scripting;

using Azos.Data;
using Azos.Serialization.JSON;
using JW=Azos.Serialization.JSON.JsonWriter;
using JDO=Azos.Serialization.JSON.JsonDynamicObject;

namespace Azos.Tests.Nub.Serialization
{
    [Runnable]
    public class JSONWriterTests
    {

        [Run]
        public void ISO8601Dates_1()
        {
            var date = new DateTime(1, 1, 1, 2, 2, 3, DateTimeKind.Utc);

            var sb = new StringBuilder();
            using(var wri = new StringWriter(sb))
             JW.EncodeDateTime(wri, date);

            Console.WriteLine(sb);

            Aver.AreEqual("\"0001-01-01T02:02:03Z\"", sb.ToString());
        }

        [Run]
        public void ISO8601Dates_2_ms()
        {
            var date = new DateTime(1, 1, 1, 2, 2, 3, 45, DateTimeKind.Utc);


            var sb = new StringBuilder();
            using(var wri = new StringWriter(sb))
             JW.EncodeDateTime(wri, date);

            Console.WriteLine(sb);

            Aver.AreEqual("\"0001-01-01T02:02:03.045Z\"", sb.ToString());
        }


        [Run]
        public void ISO8601Dates_Utc()
        {
            var date = new DateTime(2001, 12, 14, 18, 15, 12, DateTimeKind.Utc);

            var sb = new StringBuilder();
            using(var wri = new StringWriter(sb))
                JW.EncodeDateTime(wri, date);

            Console.WriteLine(sb);

            Aver.AreEqual("\"2001-12-14T18:15:12Z\"", sb.ToString());
        }

        [Run]
        public void ISO8601Dates_WithNegativeOffset()
        {
            var date = new DateTime(2001, 12, 14, 18, 15, 12, DateTimeKind.Local);

            var sb = new StringBuilder();
            using(var wri = new StringWriter(sb))
                JW.EncodeDateTime(wri, date, utcOffset: TimeSpan.FromHours(-3));

            Console.WriteLine(sb);

            Aver.AreEqual("\"2001-12-14T18:15:12-03:00\"".Args(), sb.ToString());
        }

        [Run]
        public void ISO8601Dates_WithPositiveOffset()
        {
            var date = new DateTime(2001, 12, 14, 18, 15, 12, DateTimeKind.Local);

            var sb = new StringBuilder();
            using(var wri = new StringWriter(sb))
                JW.EncodeDateTime(wri, date, utcOffset: TimeSpan.FromHours(3.5));

            Console.WriteLine(sb);

            Aver.AreEqual("\"2001-12-14T18:15:12+03:30\"".Args(), sb.ToString());
        }

        [Run]
        public void ISO8601Dates_WithNoOffset()
        {
            var date = new DateTime(2001, 12, 14, 18, 15, 12, DateTimeKind.Local);

            var sb = new StringBuilder();
            using(var wri = new StringWriter(sb))
                JW.EncodeDateTime(wri, date);

            Console.WriteLine(sb.ToString().Trim('"'));

            var got = DateTime.Parse(sb.ToString().Trim('"'));

            Console.WriteLine("got: {0}", got);

            Aver.AreEqual(date, got);
        }

        [Run]
        public void RootList_object()
        {
            var lst = new List<object>{ 1, -2, 12.8, true, 'x', "yes"};

            var json = JW.Write(lst);

            Console.WriteLine(json);

            Aver.AreEqual("[1,-2,12.8,true,\"x\",\"yes\"]", json);
        }

        [Run]
        public void RootList_object_withDates()
        {
            var date = new DateTime(1981, 12, 01, 14,23, 20,DateTimeKind.Utc);
            var lst = new List<object>{ -2, "yes", date};

            var json = JW.Write(lst);

            Console.WriteLine(json);

            Aver.AreEqual("[-2,\"yes\",\"1981-12-01T14:23:20Z\"]", json);

        }

        [Run]
        public void RootDictionary_object()
        {
            var dict = new Dictionary<object, object>{ {"name", "Lenin"}, {"in space", true}, {1905, true},{1917, true},{1961, false}, {"Bank", null} };

            var json = JW.Write(dict);

            Console.WriteLine(json);

            Aver.AreEqual("{\"name\":\"Lenin\",\"in space\":true,\"1905\":true,\"1917\":true,\"1961\":false,\"Bank\":null}",  json);

        }

        [Run]
        public void RootListOfDictionaries_object_SpaceSymbols()
        {
            var lst = new List<object>
                       {
                       12,
                       16,
                       new Dictionary<object, object>{ {"name", "Lenin"}, {"in space", true}},
                       new Dictionary<object, object>{ {"name", "Solovei"}, {"in space", false}},
                       true,
                       true,
                       -1789,
                       new Dictionary<object, object>{ {"name", "Dodik"}, {"in space", false}}
                       };


            var json = JW.Write(lst, new JsonWritingOptions{SpaceSymbols=true});

            Console.WriteLine(json);

            Aver.AreEqual("[12, 16, {\"name\": \"Lenin\", \"in space\": true}, {\"name\": \"Solovei\", \"in space\": false}, true, true, -1789, {\"name\": \"Dodik\", \"in space\": false}]", json);

        }

        [Run]
        public void RootDictionaryWithLists_object()
        {
            var lst = new Dictionary<object, object>
                       {
                         {"Important", true},
                         {"Patient", new Dictionary<string, object>{{"LastName", "Kozloff"}, {"FirstName","Alexander"}, {"Occupation","Idiot"}}},
                         {"Salaries", new List<object>{30000, 78000,125000, 4000000}},
                         {"Cars", new List<object>{"Buick", "Ferrari", "Lada", new Dictionary<string,object>{ {"Make","Zaporozhets"}, {"Model", "Gorbatiy"}, {"Year", 1971}  }    }},

                       };


            var json = JW.Write(lst, JsonWritingOptions.PrettyPrint);

            Console.WriteLine("-----------------------------------");
            Console.WriteLine(json);

            var expected=@"
{
  ""Important"": true,
  ""Patient"":
    {
      ""LastName"": ""Kozloff"",
      ""FirstName"": ""Alexander"",
      ""Occupation"": ""Idiot""
    },
  ""Salaries"": [30000, 78000, 125000, 4000000],
  ""Cars"": [""Buick"", ""Ferrari"", ""Lada"",
      {
        ""Make"": ""Zaporozhets"",
        ""Model"": ""Gorbatiy"",
        ""Year"": 1971
      }]
}";

            Console.WriteLine("-----------------------------------");
            Console.WriteLine(expected);
#warning Fix this !!!!!!!!
            Console.WriteLine(json.ToLinuxLines().DiffStrings(expected.ToLinuxLines()));

            Aver.AreEqual(expected, json);
        }


        [Run]
        public void Dynamic1()
        {

            dynamic dob = new JDO(Azos.Serialization.JSON.JSONDynamicObjectKind.Map);

            dob.FirstName = "Serge";
            dob.LastName = "Rachmaninoff";
            dob["Middle Name"] = "V";

            var json = JW.Write(dob);

            Console.WriteLine(json);

            Aver.AreEqual("{\"FirstName\":\"Serge\",\"LastName\":\"Rachmaninoff\",\"Middle Name\":\"V\"}", json);

        }

        [Run]
        public void Dynamic2_withList()
        {

            dynamic dob = new JDO(Azos.Serialization.JSON.JSONDynamicObjectKind.Map);

            dob.FirstName = "Al";
            dob.LastName = "Kutz";
            dob.Autos = new List<string>{"Buick", "Chevy", "Mazda", "Oka"};

            var json = JW.Write(dob);

            Console.WriteLine(json);

            Aver.AreEqual("{\"FirstName\":\"Al\",\"LastName\":\"Kutz\",\"Autos\":[\"Buick\",\"Chevy\",\"Mazda\",\"Oka\"]}", json);

        }

        [Run]
        public void Dynamic3_WriteRead()
        {

            dynamic dob = new JDO(Azos.Serialization.JSON.JSONDynamicObjectKind.Map);

            dob.FirstName = "Al";
            dob.LastName = "Kutz";
            dob.Autos = new List<string>{"Buick", "Chevy", "Mazda", "Oka"};

            string json = JW.Write(dob);

            var dob2 = json.JsonToDynamic();


            Aver.AreEqual(dob2.FirstName, dob.FirstName);
            Aver.AreEqual(dob2.LastName, dob.LastName);
            Aver.AreEqual(dob2.Autos.Count, dob.Autos.Count);


        }


        [Run]
        public void StringEscapes_1()
        {
            var json = JW.Write("Hello\n\rDolly!");

            Console.WriteLine(json);

            Aver.AreEqual("\"Hello\\n\\rDolly!\"", json);

        }

        [Run]
        public void StringEscapes_2_ASCII_NON_ASCII_Targets()
        {
            var lst = new List<object>{ "Hello\n\rDolly!", "Главное за сутки"};

            var json = JW.Write(lst, JsonWritingOptions.CompactASCII );

            Console.WriteLine(json);

            Aver.AreEqual("[\"Hello\\n\\rDolly!\",\"\\u0413\\u043b\\u0430\\u0432\\u043d\\u043e\\u0435 \\u0437\\u0430 \\u0441\\u0443\\u0442\\u043a\\u0438\"]", json);

            json = JW.Write(lst, JsonWritingOptions.Compact );

            Console.WriteLine(json);

            Aver.AreEqual("[\"Hello\\n\\rDolly!\",\"Главное за сутки\"]", json);

        }


        [Run]
        [Aver.Throws(typeof(JSONSerializationException),
                     Message="exceeds max nesting level",
                     MsgMatch=Aver.ThrowsAttribute.MatchType.Contains)]
        public void CyclicalGraphWithList()
        {
            var lst = new List<object>();
            lst.Add(1);
            lst.Add(-2);
            lst.Add(lst);

            var json = JW.Write(lst);

            Console.WriteLine(json);
        }

        [Run]
        [Aver.Throws(typeof(JSONSerializationException),
                           Message="exceeds max nesting level",
                          MsgMatch=Aver.ThrowsAttribute.MatchType.Contains)]
        public void CyclicalGraphWithIndirectList()
        {
            var lst = new List<object>();
            lst.Add(1);
            lst.Add(-2);
            lst.Add(new List<object>{ 1,2,3,4,5,6,lst,1});

            var json = JW.Write(lst);

            Console.WriteLine(json);
        }


        [Run]
        public void RootClass_TestPerson()
        {
            var date = new DateTime(1981, 12, 01, 14,23, 20,DateTimeKind.Utc);
            var data = new TestPerson { Name="Gagarin", DOB = date, Assets=1000000, IsRegistered=true, Luck=0.02312, Respect=PersonRespect.Guru};

            var json = JW.Write(data);

            Console.WriteLine(json);

            Aver.AreEqual("{\"Assets\":1000000,\"DOB\":\"1981-12-01T14:23:20Z\",\"IsRegistered\":true,\"Luck\":0.02312,\"Name\":\"Gagarin\",\"Respect\":\"Guru\"}", json);

        }

        [Run]
        public void RootClass_TestFamily()
        {
            var date = new DateTime(1981, 12, 01, 14,23, 20,DateTimeKind.Utc);
            var data = new TestFamily{
                              Husband = new TestPerson { Name="Gagarin", DOB = date, Assets=1000000, IsRegistered=true, Luck=0.02312, Respect= PersonRespect.Guru},
                              Wife = new TestPerson { Name="Tereshkova", DOB = date, Assets=2000000, IsRegistered=true, Luck=678.12, Respect= PersonRespect.Normal},
                              Kid = new TestPerson { Name="Savik Shuster", DOB = date, Assets=3000000, IsRegistered=false, Luck=-23.0032763, Respect= PersonRespect.Low},
                             };


            var json = JW.Write(data, new JsonWritingOptions{SpaceSymbols=true});

            Console.WriteLine(json);

            Aver.AreEqual(
    "{\"Husband\": {\"Assets\": 1000000, \"DOB\": \"1981-12-01T14:23:20Z\", \"IsRegistered\": true, \"Luck\": 0.02312, \"Name\": \"Gagarin\", \"Respect\": \"Guru\"}, \"Kid\": {\"Assets\": 3000000, \"DOB\": \"1981-12-01T14:23:20Z\", \"IsRegistered\": false, \"Luck\": -23.0032763, \"Name\": \"Savik Shuster\", \"Respect\": \"Low\"}, \"Wife\": {\"Assets\": 2000000, \"DOB\": \"1981-12-01T14:23:20Z\", \"IsRegistered\": true, \"Luck\": 678.12, \"Name\": \"Tereshkova\", \"Respect\": \"Normal\"}}"
            , json);

        }


        [Run]
        public void RootAnonymousClass_simple()
        {
            var data = new {Name="Kuklachev", Age=99, IsGood=true};


            var json = JW.Write(data);

            Console.WriteLine(json);

            Aver.AreEqual("{\"Age\":99,\"IsGood\":true,\"Name\":\"Kuklachev\"}", json);

        }

        [Run]
        public void RootAnonymousClass_withArray()
        {
            var data = new {Name="Kuklachev", Age=99, IsGood= new object []{ 1,2,true}};


            var json = JW.Write(data);

            Console.WriteLine(json);

            Aver.AreEqual("{\"Age\":99,\"IsGood\":[1,2,true],\"Name\":\"Kuklachev\"}", json);

        }

        [Run]
        public void RootAnonymousClass_withArrayandSubClass()
        {
            var data = new {Name="Kuklachev", Age=99, IsGood= new object []{ 1, new {Meduza="Gargona", Salary=123m},true}};


            var json = JW.Write(data);

            Console.WriteLine(json);

            Aver.AreEqual("{\"Age\":99,\"IsGood\":[1,{\"Meduza\":\"Gargona\",\"Salary\":123},true],\"Name\":\"Kuklachev\"}", json);

        }

        [Run]
        public void RootAutoPropFields()
        {
            var data = new ClassWithAutoPropFields{Name="Kuklachev", Age=99};


            var json = JW.Write(data);

            Console.WriteLine(json);

            Aver.AreEqual("{\"Age\":99,\"Name\":\"Kuklachev\"}", json);

        }


        [Run]
        public void Options_MapSkipNulls()
        {
            var map = new JsonDataMap();

            map["a"] = 23;
            map["b"] = true;
            map["c"] = null;
            map["d"] = (int?)11;
            map["e"] = "aaa";
            map["f"] = (int?)null;


            var json = JW.Write(map);

            Console.WriteLine(json);

            Aver.AreEqual(@"{""a"":23,""b"":true,""c"":null,""d"":11,""e"":""aaa"",""f"":null}", json);

            json = JW.Write(map, new JsonWritingOptions{ MapSkipNulls = true});

            Console.WriteLine(json);

            Aver.AreEqual(@"{""a"":23,""b"":true,""d"":11,""e"":""aaa""}", json);
        }

            private class OptRow: TypedDoc
            {
              [Field]
              public string ID { get; set;}

              [Field(targetName: "AAA", backendName: "aln")]
              [Field(targetName: "BBB", backendName: "bln")]
              public string LongName{get; set;}

              [Field(targetName: "AAA", storeFlag: Azos.Data.StoreFlag.None)]
              public string Hidden{get; set;}
            }


        [Run]
        public void Options_RowMapTargetName()
        {
            var row = new OptRow{ ID = "id123", LongName = "Long string", Hidden = "Cant see"};

            var json = JW.Write(row, new JsonWritingOptions{ RowsAsMap = true, RowMapTargetName = "AAA"});

            var map = JsonReader.DeserializeDataObject(json) as JsonDataMap;

            Aver.IsNotNull(map);
            Aver.AreEqual(2, map.Count);
            Aver.AreEqual("id123", map["ID"].AsString().AsString());
            Aver.AreEqual("Long string", map["aln"].AsString());

            json = JW.Write(row, new JsonWritingOptions{ RowsAsMap = true, RowMapTargetName = "BBB"});

            map = JsonReader.DeserializeDataObject(json) as JsonDataMap;

            Aver.IsNotNull(map);
            Aver.AreEqual(3, map.Count);
            Aver.AreEqual("id123", map["ID"].AsString());
            Aver.AreEqual("Long string", map["bln"].AsString());
            Aver.AreEqual("Cant see", map["Hidden"].AsString());

            json = JW.Write(row, new JsonWritingOptions{ RowsAsMap = true});

            map = JsonReader.DeserializeDataObject(json) as JsonDataMap;

            Aver.IsNotNull(map);
            Aver.AreEqual(3, map.Count);
            Aver.AreEqual("id123", map["ID"].AsString());
            Aver.AreEqual("Long string", map["LongName"].AsString());
            Aver.AreEqual("Cant see", map["Hidden"].AsString());
        }


            private class FieldWithDefaultsRow: TypedDoc
            {
              [Field]
              public string ID { get; set;}

              [Field(targetName: "AAA", backendName: "aln")]
              public string Name{get; set;}

              [Field(targetName: "AAA", dflt: true, backendName: "d_t")]
              public bool DefaultTrue{get; set;}

              [Field(targetName: "AAA", dflt: false, backendName: "d_f")]
              public bool DefaultFalse{get; set;}

              [Field(targetName: "AAA", dflt: 5, backendName: "d_five")]
              public int DefaultFive{get; set;}

              [Field(targetName: "AAA", dflt: 7.8d, backendName: "d_seven")]
              public double DefaultSeven{get; set;}
            }


        [Run]
        public void RowFieldWithDefaults()
        {
            var row = new FieldWithDefaultsRow{ Name = "123", DefaultTrue = true, DefaultFalse = false, DefaultFive = 5, DefaultSeven = 7.8d};

            var json = JW.Write(row, new JsonWritingOptions{ RowsAsMap = true, RowMapTargetName = "AAA"});

            Console.WriteLine(json);

            Aver.AreEqual(@"{""ID"":null,""aln"":""123""}", json);

            json = JW.Write(row, new JsonWritingOptions{ RowsAsMap = true, RowMapTargetName = "AAA", MapSkipNulls = true});

            Console.WriteLine(json);

            Aver.AreEqual(@"{""aln"":""123""}", json);


            row = new FieldWithDefaultsRow{ Name = null, DefaultTrue = true, DefaultFalse = false, DefaultFive = 5, DefaultSeven = 7.8d};
            json = JW.Write(row, new JsonWritingOptions{ RowsAsMap = true, RowMapTargetName = "AAA", MapSkipNulls = true});

            Console.WriteLine(json);

            Aver.AreEqual(@"{}", json);

            row = new FieldWithDefaultsRow{ Name = null, DefaultTrue = false, DefaultFalse = false, DefaultFive = 5, DefaultSeven = 7.8d};
            json = JW.Write(row, new JsonWritingOptions{ RowsAsMap = true, RowMapTargetName = "AAA", MapSkipNulls = true});

            Console.WriteLine(json);

            Aver.AreEqual(@"{""d_t"":false}", json);

             row = new FieldWithDefaultsRow{ Name = null, DefaultTrue = true, DefaultFalse = true, DefaultFive = 4, DefaultSeven = 7.1d};
            json = JW.Write(row, new JsonWritingOptions{ RowsAsMap = true, RowMapTargetName = "AAA", MapSkipNulls = true});

            Console.WriteLine(json);

            Aver.AreEqual(@"{""d_f"":true,""d_five"":4,""d_seven"":7.1}", json);
        }

    }

}
