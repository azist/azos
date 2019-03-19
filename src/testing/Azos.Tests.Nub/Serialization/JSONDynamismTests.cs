/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Scripting;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Serialization
{
    [Runnable]
    public class JSONDynamismTests
    {
        [Run]
        public void Map_GetSet_ByMember()
        {
            dynamic obj = new JsonDynamicObject(JSONDynamicObjectKind.Map);

            obj.A = 7;
            Aver.AreEqual(7, obj.A);
        }

        [Run]
        public void Map_GetSet_ByIndexer()
        {
            dynamic obj = new JsonDynamicObject(JSONDynamicObjectKind.Map);

            obj["A"] = 7;
            Aver.AreEqual(7, obj["A"]);
        }

        [Run]
        public void Map_GetSet_IntAdd()
        {
            dynamic obj = new JsonDynamicObject(JSONDynamicObjectKind.Map);

            obj.A = 3;
            obj.B = 5;
            obj.C = obj.A + obj.B;

            Aver.AreEqual(8, obj.C);
        }


        [Run]
        public void Map_GetSet_DoubleAddInt()
        {
            dynamic obj = new JsonDynamicObject(JSONDynamicObjectKind.Map);

            obj.A = 3.4d;
            obj.B = 5;
            obj.C = obj.A + obj.B;

            Aver.AreEqual(8.4d, obj.C);
        }

        [Run]
        public void Map_GetSet_DateTimeAddDays()
        {
            dynamic obj = new JsonDynamicObject(JSONDynamicObjectKind.Map);

            obj.StartDate = new DateTime(1980, 12, 05);
            obj.Interval = 5;
            obj.EndDate = obj.StartDate.AddDays(obj.Interval);

            Aver.AreEqual(new DateTime(1980, 12, 10), obj.EndDate);
        }


        [Run]
        public void Map_GetMemberNames()
        {
            dynamic obj = new JsonDynamicObject(JSONDynamicObjectKind.Map);

            obj.A = 3;
            obj.B = 5;
            obj.C = obj.A + obj.B;

            IEnumerable<string> nms = obj.GetDynamicMemberNames();
            var names = nms.ToList();
            Aver.AreEqual(3, names.Count);
            Aver.AreEqual("A", names[0]);
            Aver.AreEqual("B", names[1]);
            Aver.AreEqual("C", names[2]);
        }


        [Run]
        public void Array_Autogrow_1()
        {
            dynamic obj = new JsonDynamicObject(JSONDynamicObjectKind.Array);

            Aver.AreEqual(0, obj.Count);
            Aver.AreEqual(0, obj.Length);

            obj[0] = 100;
            obj[1] = 120;

            Aver.AreEqual(2, obj.Count);
            Aver.AreEqual(100, obj[0]);
            Aver.AreEqual(120, obj[1]);

        }

        [Run]
        public void Array_Autogrow_2()
        {
            dynamic obj = new JsonDynamicObject(JSONDynamicObjectKind.Array);

            Aver.AreEqual(0, obj.Count);
            Aver.AreEqual(0, obj.Length);

            obj[150] = 100;

            Aver.AreEqual(151, obj.Count);
            Aver.AreEqual(100, obj[150]);

        }


        [Run]
        public void Array_GetBeyondRange()
        {
            dynamic obj = new JsonDynamicObject(JSONDynamicObjectKind.Array);

            Aver.AreEqual(0, obj.Count);
            Aver.AreEqual(0, obj.Length);

            obj[0] = 100;
            obj[1] = 120;

            Aver.AreEqual(2, obj.Count);
            Aver.AreEqual(100, obj[0]);
            Aver.AreEqual(120, obj[1]);

            Aver.IsNull(obj[10001]);

        }

        [Run]
        public void WithSubDocumentsAndConversionAccessors()
        {
          // http://stackoverflow.com/questions/2954531/lots-of-first-chance-microsoft-csharp-runtimebinderexceptions-thrown-when-dealin
          dynamic obj = new JsonDynamicObject(JSONDynamicObjectKind.Map);
          obj.type = "abc";
          obj.startDate = "5/15/2001 6:00pm";
          obj.target = new JsonDynamicObject(JSONDynamicObjectKind.Map);
          obj.target.id = "A678";
          obj.target.image = "hello";
          obj.target.type = "good";
          obj.target.description = "Thank You";
          obj.target.Age = 123;
          obj.target.Salary = 125000m;


          string s1 = obj.ToJSON();

            var ro1 = s1.JsonToDynamic();
            Aver.AreEqual("abc", ro1.type);
            Aver.AreEqual(new DateTime(2001,5,15,18,00,00), ((string)ro1.startDate).AsDateTime());
            Aver.AreEqual("A678", ro1.target.id);
            Aver.AreEqual("hello", ro1.target.image);
            Aver.AreEqual("good", ro1.target.type);
            Aver.AreEqual("Thank You", ro1.target.description);

            Aver.AreEqual(123, ro1.target.Age);

            Aver.AreEqual(125000, ro1.target.Salary);


          string s2 = ((object)obj).ToJson();

            var ro2 = s2.JsonToDynamic();
            Aver.AreEqual("abc", ro2.type);
            Aver.AreEqual(new DateTime(2001,5,15,18,00,00), ((string)ro2.startDate).AsDateTime());
            Aver.AreEqual("A678", ro2.target.id);
            Aver.AreEqual("hello", ro2.target.image);
            Aver.AreEqual("good", ro2.target.type);
            Aver.AreEqual("Thank You", ro2.target.description);

        }

        [Run]
        public void ToTypedRow_FromString()
        {
            var str = @"{name: ""Orlov"", dob: ""02/12/2007 6:45 PM"", certified: true, serviceyears: 12, salary: 145000}";

            MySimpleData row = str.JsonToDynamic();

            Aver.AreEqual("Orlov", row.Name);
            Aver.AreEqual(new DateTime(2007, 2,12,18,45,0), row.DOB);
            Aver.AreEqual(true, row.Certified);
            Aver.AreEqual(12, row.ServiceYears);
            Aver.AreEqual(145000m, row.Salary);
        }

        [Run]
        public void ToAmorphousTypedRow_FromString()
        {
            var str = @"{name: ""Orlov"", dob: ""02/12/2007 6:45 PM"", certified: true, serviceyears: 12, salary: 145000, extra: -1, yes: true}";

            MySimpleAmorphousData row = str.JsonToDynamic();

            Aver.AreEqual("Orlov", row.Name);
            Aver.AreEqual(new DateTime(2007, 2,12,18,45,0), row.DOB);
            Aver.AreEqual(true, row.Certified);
            Aver.AreEqual(12, row.ServiceYears);
            Aver.AreEqual(145000m, row.Salary);
            Aver.AreEqual(2, row.AmorphousData.Count);
            Aver.AreObjectsEqual(-1, row.AmorphousData["extra"]);
            Aver.AreObjectsEqual(true, row.AmorphousData["yes"]);
        }

        [Run]
        public void ToDynamicRow_FromString()
        {
          var str = @"{name: ""Orlov"", dob: ""02/12/2007 6:45 PM"", certified: true, serviceyears: 12, salary: 145000}";

          var row = new DynamicDoc(Schema.GetForTypedDoc(typeof(MySimpleData)));

          JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

          Aver.AreObjectsEqual("Orlov", row["Name"]);
          Aver.AreObjectsEqual(new DateTime(2007, 2, 12, 18, 45, 0), row["DOB"]);
          Aver.AreObjectsEqual(true, row["Certified"]);
          Aver.AreObjectsEqual(12, row["ServiceYears"]);
          Aver.AreObjectsEqual(145000m, row["Salary"]);
        }

        [Run]
        public void ToAmorphousDynamicRow_FromString()
        {
          var str = @"{name: ""Orlov"", dob: ""02/12/2007 6:45 PM"", certified: true, serviceyears: 12, salary: 145000, extra: -1, yes: true}";

          var row = new AmorphousDynamicDoc(Schema.GetForTypedDoc(typeof(MySimpleData)));

          JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

          Aver.AreObjectsEqual("Orlov", row["Name"]);
          Aver.AreObjectsEqual(new DateTime(2007, 2, 12, 18, 45, 0), row["DOB"]);
          Aver.AreObjectsEqual(true, row["Certified"]);
          Aver.AreObjectsEqual(12, row["ServiceYears"]);
          Aver.AreObjectsEqual(145000m, row["Salary"]);
          Aver.AreObjectsEqual(2, row.AmorphousData.Count);
          Aver.AreObjectsEqual(-1, row.AmorphousData["extra"]);
          Aver.AreObjectsEqual(true, row.AmorphousData["yes"]);
        }

                    [Run]
                    public void ToAmorphousTypedRow_FromString_PerfParallel()
                    {
                        const int CNT = 500000;

                        var str = @"{name: ""Orlov"", dob: ""02/12/2007 6:45 PM"", certified: true, serviceyears: 12, salary: 145000, extra: -1, yes: true}";

                        var tmr = System.Diagnostics.Stopwatch.StartNew();
                        System.Threading.Tasks.Parallel.For(0, CNT,
                          (i) =>
                          {
                             MySimpleAmorphousData row = str.JsonToDynamic();

                             Aver.AreEqual("Orlov", row.Name);
                             Aver.AreEqual(new DateTime(2007, 2,12,18,45,0), row.DOB);
                             Aver.AreEqual(true, row.Certified);
                             Aver.AreEqual(12, row.ServiceYears);
                             Aver.AreEqual(145000m, row.Salary);
                             Aver.AreEqual(2, row.AmorphousData.Count);
                             Aver.AreObjectsEqual(-1, row.AmorphousData["extra"]);
                             Aver.AreObjectsEqual(true, row.AmorphousData["yes"]);
                         }
                       );

                       var elp = tmr.ElapsedMilliseconds;
                       Console.WriteLine("Total: {0} in {1} ms = {2} ops/sec".Args(CNT, elp, CNT/(elp/1000d)));


                    }


        [Run]
        public void ToTypedRowWithGDID_FromString()
        {
            var str = @"{ id: ""12:4:5"", name: ""Orlov""}";

            DataWithGDID row = str.JsonToDynamic();

            Aver.AreEqual(new GDID(12, 4, 5), row.ID);
            Aver.AreEqual("Orlov", row.Name);
        }

        [Run]
        public void ToTypedRowWithGDID_FromOtherRow()
        {
            var row1 = new DataWithGDID
            {
              ID = new GDID(1,189),
              Name = "Graf Orlov"
            };

            var str = row1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

            Console.WriteLine(str);

            DataWithGDID row2 = str.JsonToDynamic();

            Aver.AreEqual(new GDID(1, 189), row2.ID);
            Aver.AreEqual("Graf Orlov", row2.Name);
        }

        [Run]
        public void ToTypedRow_FromOtherRow()
        {
            var row1 = new MySimpleData
            {
              Name = "Graf Orlov",
              DOB = new DateTime(1980,12,11,19,23,11),
              Certified = true,
              ServiceYears = 37,
              Salary = 123455.8712m
            };

            var str = row1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

            Console.WriteLine(str);

            MySimpleData row2 = str.JsonToDynamic();

            Aver.IsTrue(row1.Equals(row2));
        }

        [Run]
        public void ToAmorphousTypedRow_FromOtherRow()
        {
            var row1 = new MySimpleAmorphousData
            {
              Name = "Graf Orlov",
              DOB = new DateTime(1980,12,11,19,23,11),
              Certified = true,
              ServiceYears = 37,
              Salary = 123455.8712m
            };
            row1.AmorphousData["frage"] = "Was machst du mit dem schwert?";
            row1.AmorphousData["antwort"] = "Ich kämpfe damit";

            var str = row1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

            Console.WriteLine(str);

            MySimpleAmorphousData row2 = str.JsonToDynamic();

            Aver.IsTrue(row1.Equals(row2));

            Aver.AreObjectsEqual("Was machst du mit dem schwert?", row2.AmorphousData["frage"]);
            Aver.AreObjectsEqual("Ich kämpfe damit", row2.AmorphousData["antwort"]);
        }

        [Run]
        public void ToTypedRow_MyComplexData()
        {
            var row1 = new MyComplexData
            { ID = 12345,
              D1 = new MySimpleData
              {
                Name = "Graf Orlov",
                DOB = new DateTime(1980,12,11,19,23,11),
                Certified = true,
                ServiceYears = 37,
                Salary = 123455.8712m
              },
              D2 = new MySimpleData
              {
                Name = "Oleg Popov",
                DOB = new DateTime(1981,11,01,14,08,19),
                Certified = true,
                ServiceYears = 37,
                Salary = 123455.8712m
              }
            };

            var str = row1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

            Console.WriteLine(str);

            MyComplexData row2 = str.JsonToDynamic();

            Aver.IsTrue(row1.Equals(row2));
        }

        [Run]
        public void ToTypedRow_MyComplexData_Null()
        {
            var row1 = new MyComplexData
            { ID = 12345,
              D1 = new MySimpleData
              {
                Name = "Graf Orlov",
                DOB = new DateTime(1980,12,11,19,23,11),
                Certified = true,
                ServiceYears = 37,
                Salary = 123455.8712m
              },
              D2 = null
            };

            var str = row1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

            Console.WriteLine(str);

            MyComplexData row2 = str.JsonToDynamic();

            Aver.IsTrue(row1.D1.Equals(row2.D1));
            Aver.IsNull(row2.D2);
        }


        [Run]
        public void ToTypedRow_MyComplexDataWithArray()
        {
            var row1 = new MyComplexDataWithArray
            { ID = 12345,
              Data = new MySimpleData[]{
                new MySimpleData
                {
                  Name = "Graf Orlov",
                  DOB = new DateTime(1980,12,11,19,23,11),
                  Certified = true,
                  ServiceYears = 37,
                  Salary = 123455.8712m
                },
                new MySimpleData
                {
                  Name = "Oleg Popov",
                  DOB = new DateTime(1981,11,01,14,08,19),
                  Certified = true,
                  ServiceYears = 37,
                  Salary = 123455.8712m
                }
              }
            };

            var str = row1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

            Console.WriteLine(str);

            MyComplexDataWithArray row2 = str.JsonToDynamic();

            Aver.IsTrue(row1.Equals(row2));
        }

        [Run]
        public void ToTypedRow_MyComplexDataWithList()
        {
            var row1 = new MyComplexDataWithList
            { ID = 12345,
              Data = new List<MySimpleData>{
                new MySimpleData
                {
                  Name = "Graf Orlov",
                  DOB = new DateTime(1980,12,11,19,23,11),
                  Certified = true,
                  ServiceYears = 37,
                  Salary = 123455.8712m
                },
                new MySimpleData
                {
                  Name = "Oleg Popov",
                  DOB = new DateTime(1981,11,01,14,08,19),
                  Certified = true,
                  ServiceYears = 37,
                  Salary = 123455.8712m
                }
              }
            };

            var str = row1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

            Console.WriteLine(str);

            MyComplexDataWithList row2 = str.JsonToDynamic();

            Aver.IsTrue(row1.Equals(row2));
        }




        [Run]
        public void ToTypedRow_MyComplexDataWithPrimitiveArray()
        {
            var row1 = new MyComplexDataWithPrimitiveArray
            { ID = 12345,
              Data = new int[]{ 1,7,12,3,8,9,0,2134,43,6,2,5}
            };

            var str = row1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

            Console.WriteLine(str);

            MyComplexDataWithPrimitiveArray row2 = str.JsonToDynamic();

            Aver.IsTrue(row1.Equals(row2));
        }


        [Run]
        public void ToTypedRow_MyComplexDataWithPrimitiveList()
        {
            var row1 = new MyComplexDataWithPrimitiveList
            { ID = 12345,
              Data = new List<int>{ 1,7,12,3,8,9,0,2134,43,6,2,5}
            };

            var str = row1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

            Console.WriteLine(str);

            MyComplexDataWithPrimitiveList row2 = str.JsonToDynamic();

            Aver.IsTrue(row1.Equals(row2));
        }


        [Run]
        public void ToTypedRow_RowWithSUbDocuments_1()
        {
            var json ="{data: null, map: null}";

            RowWithSubDocuments row = json.JsonToDynamic();
            Aver.IsNull(row.Data);
            Aver.IsNull(row.Map);
        }

         [Run]
        public void ToTypedRow_RowWithSUbDocuments_2()
        {
            var json ="{data: {a: 1, b: 2}, map: null}";

            RowWithSubDocuments row = json.JsonToDynamic();

            Aver.AreObjectsEqual(1, ((JsonDataMap)row.Data)["a"]);
            Aver.AreObjectsEqual(2, ((JsonDataMap)row.Data)["b"]);
            Aver.IsNull(row.Map);
        }

         [Run]
        public void ToTypedRow_RowWithSUbDocuments_3()
        {
            var json ="{data: '{a: 1, b: 2}', map: null}";

            RowWithSubDocuments row = json.JsonToDynamic();

            Aver.AreObjectsEqual(1, ((JsonDataMap)row.Data)["a"]);
            Aver.AreObjectsEqual(2, ((JsonDataMap)row.Data)["b"]);
            Aver.IsNull(row.Map);
        }

         [Run]
        public void ToTypedRow_RowWithSUbDocuments_4()
        {
            var json ="{data: '{a: 1, b: 2}', map: {c: 3, d: 4}}";

            RowWithSubDocuments row = json.JsonToDynamic();

            Aver.AreObjectsEqual(3, row.Map["c"]);
            Aver.AreObjectsEqual(4, row.Map["d"]);
        }

         [Run]
        public void ToTypedRow_RowWithSUbDocuments_5()
        {
            var json ="{data: '{a: 1, b: 2}', map: '{c: 3, d: 4}'}";

            RowWithSubDocuments row = json.JsonToDynamic();

            Aver.AreObjectsEqual(3, row.Map["c"]);
            Aver.AreObjectsEqual(4, row.Map["d"]);
        }


                     private class DataWithGDID : TypedDoc
                     {
                       [Field] public Azos.Data.GDID ID { get; set;}
                       [Field] public string Name { get; set;}

                       public override bool Equals(Doc other)
                       {
                         var b = other as DataWithGDID;
                         if (b==null) return false;
                         return this.Name==b.Name &&
                                this.ID.Equals(b.ID);
                       }
                     }


                     private class MySimpleData : TypedDoc
                     {
                       [Field] public string Name { get; set;}
                       [Field] public DateTime DOB { get; set;}
                       [Field] public bool Certified { get; set;}
                       [Field] public int ServiceYears { get; set;}
                       [Field] public decimal Salary { get; set;}

                       public override bool Equals(Doc other)
                       {
                         var b = other as MySimpleData;
                         if (b==null) return false;
                         return this.Name==b.Name &&
                                this.DOB==b.DOB&&
                                this.Certified==b.Certified&&
                                this.ServiceYears==b.ServiceYears&&
                                this.Salary==b.Salary;
                       }
                     }

                     private class MySimpleAmorphousData : AmorphousTypedDoc
                     {
                       [Field] public string Name { get; set;}
                       [Field] public DateTime DOB { get; set;}
                       [Field] public bool Certified { get; set;}
                       [Field] public int ServiceYears { get; set;}
                       [Field] public decimal Salary { get; set;}

                       public override bool Equals(Doc other)
                       {
                         var b = other as MySimpleAmorphousData;
                         if (b==null) return false;
                         return this.Name==b.Name &&
                                this.DOB==b.DOB&&
                                this.Certified==b.Certified&&
                                this.ServiceYears==b.ServiceYears&&
                                this.Salary==b.Salary;
                       }
                     }



                     private class MyComplexData : TypedDoc
                     {
                       [Field] public long ID { get; set;}
                       [Field] public MySimpleData D1 { get; set;}
                       [Field] public MySimpleData D2 { get; set;}

                       public override bool Equals(Doc other)
                       {
                         var b = other as MyComplexData;
                         if (b==null) return false;
                         return this.ID==b.ID &&
                                this.D1.Equals(b.D1)&&
                                this.D2.Equals(b.D2);
                       }
                     }

                     private class MyComplexDataWithArray : TypedDoc
                     {
                       [Field] public long ID { get; set;}
                       [Field] public MySimpleData[] Data { get; set;}

                       public override bool Equals(Doc other)
                       {
                         var b = other as MyComplexDataWithArray;
                         if (b==null) return false;
                         return this.ID==b.ID &&
                                this.Data.SequenceEqual(b.Data);
                       }
                     }

                     private class MyComplexDataWithList : TypedDoc
                     {
                       [Field] public long ID { get; set;}
                       [Field] public List<MySimpleData> Data { get; set;}

                       public override bool Equals(Doc other)
                       {
                         var b = other as MyComplexDataWithList;
                         if (b==null) return false;
                         return this.ID==b.ID &&
                                this.Data.SequenceEqual(b.Data);
                       }
                     }

                     private class MyComplexDataWithPrimitiveArray : TypedDoc
                     {
                       [Field] public long ID { get; set;}
                       [Field] public int[] Data { get; set;}

                       public override bool Equals(Doc other)
                       {
                         var b = other as MyComplexDataWithPrimitiveArray;
                         if (b==null) return false;
                         return this.ID==b.ID &&
                                this.Data.SequenceEqual(b.Data);
                       }
                     }

                     private class MyComplexDataWithPrimitiveList : TypedDoc
                     {
                       [Field] public long ID { get; set;}
                       [Field] public List<int> Data { get; set;}

                       public override bool Equals(Doc other)
                       {
                         var b = other as MyComplexDataWithPrimitiveList;
                         if (b==null) return false;
                         return this.ID==b.ID &&
                                this.Data.SequenceEqual(b.Data);
                       }
                     }


                     private class RowWithSubDocuments : TypedDoc
                     {
                       [Field] public IJsonDataObject Data { get; set;}
                       [Field] public JsonDataMap Map { get; set;}
                     }


                     private class RowWithBinaryData : TypedDoc
                     {
                       [Field] public string FileName { get; set;}
                       [Field] public string ContentType { get; set;}
                       [Field] public byte[] Content { get; set;}
                     }

        [Run]
        public void ToFromRowWithBinaryData()
        {
            var r1 = new RowWithBinaryData
            {
              FileName = "SaloInstructions.txt",
              ContentType = "text/plain",
              Content = new byte[]{1,2,3,120}
            };

            var json =  r1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

            Console.WriteLine(json);

            RowWithBinaryData r2 = json.JsonToDynamic();

            Aver.AreEqual(r1.FileName, r2.FileName);
            Aver.AreEqual(r1.ContentType, r2.ContentType);
            Aver.IsTrue( r1.Content.SequenceEqual(r2.Content) );
        }




    }
}
