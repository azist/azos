/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azos.Scripting;

using Azos.Data;
using Azos.Serialization.Slim;
using Azos.Serialization.JSON;

namespace Azos.Tests.Unit.DataAccess
{
    [Runnable(TRUN.BASE, 3)]
    public class Serialization
    {

        [Run]
        public void Slim_SerializeTable_TypedRows()
        {
            var tbl = new Table(Schema.GetForTypedDoc(typeof(Person)));

            for(var i=0; i<1000; i++)
             tbl.Insert( new Person{
                                    ID = "POP{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "Popov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12
                                   });

            var ser = new SlimSerializer();
            using(var ms = new MemoryStream())
            {
                ser.Serialize(ms, tbl);
            Console.WriteLine("{0} rows took {1} bytes".Args(tbl.Count, ms.Position));
                ms.Position = 0;

                var tbl2 = ser.Deserialize(ms) as Table;

                Aver.IsNotNull( tbl2 );
                Aver.IsFalse( object.ReferenceEquals(tbl ,tbl2) );

                Aver.AreEqual( 1000, tbl2.Count);

                Aver.IsTrue( tbl.SequenceEqual( tbl2 ) );
            }
        }


        [Run]
        public void Slim_SerializeTable_DynamicRows()
        {
            var tbl = new Table(Schema.GetForTypedDoc(typeof(Person)));

            for(var i=0; i<1000; i++)
            {
                var row = new DynamicDoc( tbl.Schema );

                row["ID"] = "POP{0}".Args(i);
                row["FirstName"] = "Oleg";
                row["LastName"] = "Popov-{0}".Args(i);
                row["DOB"] = new DateTime(1953, 12, 10);
                row["YearsInSpace"] = 12;

                tbl.Insert( row );
            }

            var ser = new SlimSerializer();
            using(var ms = new MemoryStream())
            {
                ser.Serialize(ms, tbl);
            Console.WriteLine("{0} rows took {1} bytes".Args(tbl.Count, ms.Position));
                ms.Position = 0;

                var tbl2 = ser.Deserialize(ms) as Table;

                Aver.IsNotNull( tbl2 );
                Aver.IsFalse( object.ReferenceEquals(tbl ,tbl2) );

                Aver.AreEqual( 1000, tbl2.Count);
                Aver.IsTrue( tbl.SequenceEqual( tbl2 ) );
            }
        }

        [Run]
        public void Slim_SerializeTable_ComplexTypedRows()
        {
            var tbl = new Table(Schema.GetForTypedDoc(typeof(PersonWithNesting)));

            for(var i=0; i<1000; i++)
             tbl.Insert( new PersonWithNesting{
                                    ID = "POP{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "Popov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12,
                                    LatestHistory = new HistoryItem{ ID = "111", StartDate = DateTime.Now, Description="Chaplin" },
                                    History1  = new List<HistoryItem>
                                    {
                                      new HistoryItem{ ID = "789211", StartDate = DateTime.Now, Description="Chaplin with us" },
                                      new HistoryItem{ ID = "234234", StartDate = DateTime.Now, Description="Chaplin with you" }
                                    },
                                    History2  = new HistoryItem[2]
                                   });

            var ser = new SlimSerializer();
            using(var ms = new MemoryStream())
            {
                ser.Serialize(ms, tbl);


         Console.WriteLine("{0} rows took {1} bytes".Args(tbl.Count, ms.Position));

                ms.Position = 0;

                var tbl2 = ser.Deserialize(ms) as Table;

                Aver.IsNotNull( tbl2 );
                Aver.IsFalse( object.ReferenceEquals(tbl ,tbl2) );

                Aver.AreEqual( 1000, tbl2.Count);

                Aver.IsTrue( tbl.SequenceEqual( tbl2 ) );
            }
        }

        [Run]
        public void Slim_SerializeRow_ComplexTypedRow()
        {
            var row1 =  new PersonWithNesting{
                                    ID = "A1",
                                    FirstName = "Joseph",
                                    LastName = "Mc'Cloud",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12,
                                    LatestHistory = new HistoryItem{ ID = "111", StartDate = DateTime.Now, Description="Chaplin" },
                                    History1  = new List<HistoryItem>
                                    {
                                      new HistoryItem{ ID = "789211", StartDate = DateTime.Now, Description="Chaplin with us" },
                                      new HistoryItem{ ID = "234234", StartDate = DateTime.Now, Description="Chaplin with you" }
                                    },
                                    History2  = new HistoryItem[2]
                                   };

            var ser = new SlimSerializer();
            using(var ms = new MemoryStream())
            {
                ser.Serialize(ms, row1);


         Console.WriteLine("1 row took {0} bytes".Args(ms.Position));

                ms.Position = 0;

                var row2 = ser.Deserialize(ms) as PersonWithNesting;

                Aver.IsNotNull( row2 );
                Aver.IsFalse( object.ReferenceEquals(row1 , row2) );

                Aver.AreEqual("A1", row2.ID);
                Aver.AreEqual("Joseph",  row2.FirstName);
                Aver.AreEqual("Mc'Cloud",row2.LastName);
                Aver.AreEqual("111", row2.LatestHistory.ID);
                Aver.AreEqual(2, row2.History1.Count);
                Aver.AreEqual("234234", row2.History1[1].ID);
            }
        }



        [Run]
        public void JSON_SerializeRowset_TypedRows()
        {
            var rowset = new Rowset(Schema.GetForTypedDoc(typeof(Person)));

            for(var i=0; i<10; i++)
             rowset.Insert( new Person{
                                    ID = "POP{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "Popov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12
                                   });

            // Serialization without schema
            var json = rowset.ToJSON(Azos.Serialization.JSON.JSONWritingOptions.PrettyPrint);

            Console.WriteLine( json);

            var rowset2 = json.JSONToDynamic();

            Aver.AreEqual("Popov-1", rowset2.Rows[1][2]);

            RowsetBase rowset3 = new Rowset(Schema.GetForTypedDoc(typeof(Person)));
            var res = Rowset.FromJSON<Person>(json, ref rowset3);

            Aver.AreEqual(10, res);
            Aver.AreEqual(10, rowset3.Count);
            Aver.AreObjectsEqual("Popov-1", rowset3[1][2]);

            var options = new Azos.Serialization.JSON.JSONWritingOptions
            {
              RowsetMetadata  = true,
              IndentWidth     = 2,
              ObjectLineBreak = true,
              MemberLineBreak = true,
              SpaceSymbols    = true,
              ASCIITarget     = false
            };
            rowset3.Clear();
            var json2 = rowset.ToJSON(options);
            var res2  = Rowset.FromJSON<Person>(json2, ref rowset3);

            Aver.AreEqual(10, res);
            Aver.AreEqual(10, rowset3.Count);
            Aver.AreObjectsEqual("Popov-1", rowset3[1][2]);
        }

        [Run]
        public void JSON_SerializeRowset_ComplexTypedRows_Array()
        {
            var rowset = new Rowset(Schema.GetForTypedDoc(typeof(PersonWithNesting)));

            for(var i=0; i<10; i++)
             rowset.Insert( new PersonWithNesting{
                                    ID = "POP{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "Popov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12,
                                    LatestHistory = new HistoryItem{ ID = "111", StartDate = DateTime.Now, Description="Chaplin" },
                                    History1  = new List<HistoryItem>
                                    {
                                      new HistoryItem{ ID = "789211", StartDate = DateTime.Now, Description="Chaplin with us" },
                                      new HistoryItem{ ID = "234234", StartDate = DateTime.Now, Description="Chaplin with you" }
                                    },
                                    History2  = new HistoryItem[2]
                                   });

            var json = rowset.ToJSON( Azos.Serialization.JSON.JSONWritingOptions.PrettyPrint);// );

            Console.WriteLine( json);

            var rowset2 = json.JSONToDynamic();

            Aver.AreEqual("Popov-1", rowset2.Rows[1][2]);

        }


        [Run]
        public void JSON_SerializeRowset_ComplexTypedRows_Map()
        {
            var rowset = new Rowset(Schema.GetForTypedDoc(typeof(PersonWithNesting)));

            for(var i=0; i<10; i++)
             rowset.Insert( new PersonWithNesting{
                                    ID = "POP{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "Popov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12,
                                    LatestHistory = new HistoryItem{ ID = "111", StartDate = DateTime.Now, Description="Chaplin" },
                                    History1  = new List<HistoryItem>
                                    {
                                      new HistoryItem{ ID = "789211", StartDate = DateTime.Now, Description="Chaplin with us" },
                                      new HistoryItem{ ID = "234234", StartDate = DateTime.Now, Description="Chaplin with you" }
                                    },
                                    History2  = new HistoryItem[2]
                                   });

            var json = rowset.ToJSON( Azos.Serialization.JSON.JSONWritingOptions.PrettyPrintRowsAsMap);// );

            Console.WriteLine( json);

            var rowset2 = json.JSONToDynamic();

            Aver.AreEqual("Popov-1", rowset2.Rows[1].LastName);
            Aver.AreEqual("789211", rowset2.Rows[1].History1[0].ID);
        }


        [Run]
        public void JSON_SerializeRow_ComplexTypedRow_Map()
        {
            var row1 =  new PersonWithNesting{
                                    ID = "A1",
                                    FirstName = "Joseph",
                                    LastName = "Mc'Cloud",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12,
                                    LatestHistory = new HistoryItem{ ID = "111", StartDate = DateTime.Now, Description="Chaplin" },
                                    History1  = new List<HistoryItem>
                                    {
                                      new HistoryItem{ ID = "789211", StartDate = DateTime.Now, Description="Chaplin with us" },
                                      new HistoryItem{ ID = "234234", StartDate = DateTime.Now, Description="Chaplin with you" }
                                    },
                                    History2  = new HistoryItem[2]
                                   };

            var json = row1.ToJSON( Azos.Serialization.JSON.JSONWritingOptions.PrettyPrintRowsAsMap);//AS MAP!!!!

            Console.WriteLine(json);

            var row2 = json.JSONToDynamic();

            Aver.AreEqual("A1",      row2.ID);
            Aver.AreEqual("Joseph",  row2.FirstName);
            Aver.AreEqual("Mc'Cloud",row2.LastName);
            Aver.AreEqual("111",     row2.LatestHistory.ID);
            Aver.AreEqual(2,         row2.History1.Count);
            Aver.AreEqual("234234",  row2.History1[1].ID);
        }

        [Run]
        public void JSON_SerializeRow_ComplexTypedRow_Array()
        {
            var row1 =  new PersonWithNesting{
                                    ID = "A1",
                                    FirstName = "Joseph",
                                    LastName = "Mc'Cloud",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12,
                                    LatestHistory = new HistoryItem{ ID = "111", StartDate = DateTime.Now, Description="Chaplin" },
                                    History1  = new List<HistoryItem>
                                    {
                                      new HistoryItem{ ID = "789211", StartDate = DateTime.Now, Description="Chaplin with us" },
                                      new HistoryItem{ ID = "234234", StartDate = DateTime.Now, Description="Chaplin with you" }
                                    },
                                    History2  = new HistoryItem[2]
                                   };

            var json = row1.ToJSON( Azos.Serialization.JSON.JSONWritingOptions.PrettyPrint);//AS ARRAY

            Console.WriteLine(json);

            var row2 = json.JSONToDynamic();

            Aver.AreEqual("A1",      row2[row1.Schema["ID"].Order]);
            Aver.AreEqual("Joseph",  row2[row1.Schema["FirstName"].Order]);
            Aver.AreEqual("Mc'Cloud",row2[row1.Schema["LastName"].Order]);
            Aver.AreEqual("111",     row2[row1.Schema["LatestHistory"].Order][0]);
            Aver.AreEqual(2,         row2[row1.Schema["History1"].Order].Count);
            Aver.AreEqual("234234",  row2[row1.Schema["History1"].Order][1][0]);
        }

        [Run]
        public void JSON_SerializeRow_ComplexTypedRow_WithSchema()
        {
            var row1 =  new PersonWithNesting{
                                    ID = "A1",
                                    FirstName = "Joseph",
                                    LastName = "Mc'Cloud",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12,
                                    LatestHistory = new HistoryItem{ ID = "111", StartDate = DateTime.Now, Description="Chaplin" },
                                    History1  = new List<HistoryItem>
                                    {
                                      new HistoryItem{ ID = "789211", StartDate = DateTime.Now, Description="Chaplin with us" },
                                      new HistoryItem{ ID = "234234", StartDate = DateTime.Now, Description="Chaplin with you" }
                                    },
                                    History2  = new HistoryItem[2]
                                   };


            var tbl1 = new Rowset(row1.Schema);
            tbl1.Add(row1);


            var json = tbl1.ToJSON( new Azos.Serialization.JSON.JSONWritingOptions
                                   {
                                     RowsetMetadata = true,
                                      SpaceSymbols = true,
                                       IndentWidth = 2,
                                        MemberLineBreak = true,
                                         ObjectLineBreak = true,
                                          RowsAsMap = true,
                                           Purpose = JSONSerializationPurpose.Marshalling
                                   });//AS MAP

            Console.WriteLine(json);

            var tbl2 = json.JSONToDynamic();

            var row2 = tbl2.Rows[0];

            Aver.AreEqual("A1",      row2.ID);
            Aver.AreEqual("Joseph",  row2.FirstName);
            Aver.AreEqual("Mc'Cloud",row2.LastName);
            Aver.AreEqual("111",     row2.LatestHistory.ID);
            Aver.AreEqual(2,         row2.History1.Count);
            Aver.AreEqual("234234",  row2.History1[1].ID);
        }





    }
}
