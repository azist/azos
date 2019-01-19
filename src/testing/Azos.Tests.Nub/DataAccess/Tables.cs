/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azos.Scripting;


using Azos.Data;


namespace Azos.Tests.Nub.DataAccess
{
    [Runnable]
    public class Tables
    {

        [Run]
        public void PopulateAndFindKey_TypedRows()
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


            Aver.AreEqual(1000, tbl.Count);

            var match1 = tbl.FindByKey("POP35");
            Aver.IsNotNull( match1 );
            Aver.AreObjectsEqual("Popov-35", match1["LastName"]); //example of dynamic row access

            var match2 = tbl.FindByKey("POP36") as Person;
            Aver.IsNotNull( match2 );
            Aver.AreEqual("Popov-36", match2.LastName);//example of typed row access

            var match3 = tbl.FindByKey("DoesNotExist");
            Aver.IsNull( match3 );
        }

        [Run]
        public void PopulateAndCloneThenFindKey_TypedRows()
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


            tbl = new Table(tbl);//make copy

            Aver.AreEqual(1000, tbl.Count);

            var match1 = tbl.FindByKey("POP35");
            Aver.IsNotNull( match1 );
            Aver.AreObjectsEqual("Popov-35", match1["LastName"]); //example of dynamic row access

            var match2 = tbl.FindByKey("POP36") as Person;
            Aver.IsNotNull( match2 );
            Aver.AreEqual("Popov-36", match2.LastName);//example of typed row access

            var match3 = tbl.FindByKey("DoesNotExist");
            Aver.IsNull( match3 );
        }


        [Run]
        public void PopulateAndFindKey_MixedRows()
        {
            var tbl = new Table(Schema.GetForTypedDoc(typeof(Person)));

            for(var i=0; i<1000; i++)
            {
                 var row =  new DynamicDoc(tbl.Schema);

                 row["ID"] = "DYN{0}".Args(i);
                 row["FirstName"] = "Oleg";
                 row["LastName"] = "DynamicPopov-{0}".Args(i);
                 row["DOB"] = new DateTime(1953, 12, 10);
                 row["YearsInSpace"] = 12;

                 tbl.Insert( row );

                 tbl.Insert( new Person{
                                    ID = "TYPED{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "TypedPopov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12
                                   });
            }

            Aver.AreEqual(2000, tbl.Count);

            var match1 = tbl.FindByKey("DYN35");
            Aver.IsNotNull( match1 );
            Aver.IsTrue( match1 is DynamicDoc );
            Aver.AreObjectsEqual("DynamicPopov-35", match1["LastName"]);

            var match2 = tbl.FindByKey("TYPED36") as Person;
            Aver.IsNotNull( match2 );
            Aver.AreObjectsEqual("TypedPopov-36", match2["LastName"]);

            var match3 = tbl.FindByKey("DoesNotExist");
            Aver.IsNull( match3 );
        }


        [Run]
        public void FindIndexByKey_DynamicRows()
        {
          var tbl = new Table(Schema.GetForTypedDoc(typeof(Person)));

          for (var i = 0; i < 10; i++)
          {
            var row = new DynamicDoc(tbl.Schema);

            row["ID"] = "POP{0}".Args(i);
            row["FirstName"] = "Oleg";
            row["LastName"] = "DynamicPopov-{0}".Args(i);
            row["DOB"] = new DateTime(1953, 12, 10);
            row["YearsInSpace"] = 12;

            tbl.Insert(row);
          }

          Aver.AreEqual(10, tbl.Count);

          var idx1 = tbl.FindIndexByKey("POP5");
          Aver.AreEqual(5, idx1);

          var idx2 = tbl.FindIndexByKey("POP6");
          Aver.AreEqual(6, idx2);

          var idx3 = tbl.FindIndexByKey("DoesNotExist");
          Aver.AreEqual(-1, idx3);
        }


        [Run]
        public void FindIndexByKey_TypedRows()
        {
          var tbl = new Table(Schema.GetForTypedDoc(typeof(Person)));

          for (var i = 0; i < 10; i++)
            tbl.Insert(new Person
            {
              ID = "POP{0}".Args(i),
              FirstName = "Oleg",
              LastName = "Popov-{0}".Args(i),
              DOB = new DateTime(1953, 12, 10),
              YearsInSpace = 12
            });

          Aver.AreEqual(10, tbl.Count);

          var idx1 = tbl.FindIndexByKey("POP5");
          Aver.AreEqual(5, idx1);

          var idx2 = tbl.FindIndexByKey("POP6");
          Aver.AreEqual(6, idx2);

          var idx3 = tbl.FindIndexByKey("DoesNotExist");
          Aver.AreEqual(-1, idx3);
        }


        [Run]
        public void PopulateAndFindKey_DynamicRows()
        {
            var tbl = new Table(Schema.GetForTypedDoc(typeof(Person)));

            for(var i=0; i<1000; i++)
            {
                 var row =  new DynamicDoc(tbl.Schema);

                 row["ID"] = "POP{0}".Args(i);
                 row["FirstName"] = "Oleg";
                 row["LastName"] = "Popov-{0}".Args(i);
                 row["DOB"] = new DateTime(1953, 12, 10);
                 row["YearsInSpace"] = 12;

                 tbl.Insert( row );
            }

            Aver.AreEqual(1000, tbl.Count);

            var match1 = tbl.FindByKey("POP35");
            Aver.IsNotNull( match1 );
            Aver.AreObjectsEqual("Popov-35", match1["LastName"]);

            var match2 = tbl.FindByKey("POP36") as DynamicDoc;
            Aver.IsNotNull( match2 );
            Aver.AreObjectsEqual("Popov-36", match2["LastName"]);

            var match3 = tbl.FindByKey("DoesNotExist");
            Aver.IsNull( match3 );
        }


        [Run]
        public void PopulateAndFindCompositeKey_TypedRows()
        {
            var tbl = new Table(Schema.GetForTypedDoc(typeof(WithCompositeKey)));

            for(var i=0; i<1000; i++)
             tbl.Insert( new WithCompositeKey{
                                    ID = "ID{0}".Args(i),
                                    StartDate = new DateTime(1953, 12, 10),
                                    Description = "Descr{0}".Args(i)
                                   });


            Aver.AreEqual(1000, tbl.Count);

            var match1 = tbl.FindByKey("ID35", new DateTime(1953, 12, 10));
            Aver.IsNotNull( match1 );
            Aver.AreObjectsEqual("Descr35", match1["Description"]);

            var match2 = tbl.FindByKey("ID35", new DateTime(1953, 07, 10));
            Aver.IsNull( match2 );
        }


        [Run]
        public void BuildUsingAdHockSchema()
        {
            var schema = new Schema("TEZT",
                           new Schema.FieldDef("ID", typeof(int), new List<FieldAttribute>{ new FieldAttribute(required: true, key: true)}),
                           new Schema.FieldDef("Description", typeof(string), new List<FieldAttribute>{ new FieldAttribute(required: true)})
            );

            var tbl = new Table(schema);

            for(var i=0; i<1000; i++)
            {
                 var row =  new DynamicDoc(tbl.Schema);

                 row["ID"] = i;
                 row["Description"] = "Item-{0}".Args(i);

                 tbl.Insert( row );
            }

            Aver.AreEqual(1000, tbl.Count);

            var match = tbl.FindByKey(178);
            Aver.IsNotNull( match );
            Aver.AreObjectsEqual("Item-178", match["Description"]);
        }



        [Run]
        public void PopulateAndUpdateExisting()
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

            var update =  new Person{
                                    ID = "POP17",
                                    FirstName = "Yaroslav",
                                    LastName = "Suzkever",
                                    DOB = new DateTime(1952, 12, 10),
                                    YearsInSpace = 14
                                   };

            var res = tbl.Update(update);//<-------------!!!!!!

            Aver.IsTrue( res.Index>=0 );

            var match = tbl.FindByKey("POP17") as Person;
            Aver.IsNotNull( match );
            Aver.AreEqual("Yaroslav", match.FirstName);
            Aver.AreEqual("Suzkever", match.LastName);

        }

        [Run]
        public void PopulateAndUpdateNonExisting()
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

            var update =  new Person{
                                    ID = "NONE17",
                                    FirstName = "Yaroslav",
                                    LastName = "Suzkever",
                                    DOB = new DateTime(1952, 12, 10),
                                    YearsInSpace = 14
                                   };

            var res = tbl.Update(update);//<-------------!!!!!!

            Aver.IsTrue( res.Index==-1 );

            var match = tbl.FindByKey("NONE17") as Person;
            Aver.IsNull( match );

        }


        [Run]
        public void PopulateAndUpsertExisting()
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

            var update =  new Person{
                                    ID = "POP17",
                                    FirstName = "Yaroslav",
                                    LastName = "Suzkever",
                                    DOB = new DateTime(1952, 12, 10),
                                    YearsInSpace = 14
                                   };

            var res = tbl.Upsert(update);//<-------------!!!!!!

            Aver.IsTrue( res.Updated );

            var match = tbl.FindByKey("POP17") as Person;
            Aver.IsNotNull( match );
            Aver.AreEqual("Yaroslav", match.FirstName);
            Aver.AreEqual("Suzkever", match.LastName);

        }

        [Run]
        public void PopulateAndUpsertExistingWithMigration()
        {
            var tbl = new Table(Schema.GetForTypedDoc(typeof(Person)));

            for(var i=0; i<20; i++)
             tbl.Insert( new Person{
                                    ID = "POP{0}".Args(i),
                                    FirstName = "Oleg",
                                    LastName = "Popov-{0}".Args(i),
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12
                                   });

            var updates = new[] {
                           new Person{
                                    ID = "POP17",
                                    FirstName = "Yaroslav",
                                    LastName = "Suzkever",
                                    DOB = new DateTime(1952, 12, 10),
                                    YearsInSpace = 14
                                   },

                           new Person{
                                    ID = "POP15",
                                    FirstName = "Yaroslav",
                                    LastName = "Suzkever",
                                    DOB = new DateTime(1952, 12, 10),
                                    YearsInSpace = 16
                                   }
                          };

            foreach (var r in updates)
            {
              var res = tbl.Upsert(r, rowUpgrade:(old,row) =>
              {
                ((Person)row).YearsInSpace = ((Person)row).YearsInSpace == 14 ? ((Person)old).YearsInSpace : ((Person)row).YearsInSpace;
                return row;
              });

              Aver.IsTrue( res.Updated );
            }

            var match = (Person)tbl.FindByKey("POP17");
            Aver.IsNotNull( match );
            Aver.AreEqual("Yaroslav", match.FirstName);
            Aver.AreEqual(12,         match.YearsInSpace);

            match = (Person)tbl.FindByKey("POP15");
            Aver.IsNotNull( match );
            Aver.AreEqual("Yaroslav", match.FirstName);
            Aver.AreEqual(16,         match.YearsInSpace);
        }

        [Run]
        public void PopulateAndUpsertNonExisting()
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

            var update =  new Person{
                                    ID = "GOODMAN17",
                                    FirstName = "John",
                                    LastName = "Jeffer",
                                    DOB = new DateTime(1952, 12, 10),
                                    YearsInSpace = 14
                                   };

            var res = tbl.Upsert(update);//<-------------!!!!!!

            Aver.IsFalse( res.Updated );

            Aver.AreEqual(1001, tbl.Count);

            var match = tbl.FindByKey("POP17") as Person;
            Aver.IsNotNull( match );
            Aver.AreEqual("Oleg", match.FirstName);
            Aver.AreEqual("Popov-17", match.LastName);

            match = tbl.FindByKey("GOODMAN17") as Person;
            Aver.IsNotNull( match );
            Aver.AreEqual("John", match.FirstName);
            Aver.AreEqual("Jeffer", match.LastName);

        }

        [Run]
        public void PopulateAndDeleteExisting()
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

            var delete =  new Person{
                                    ID = "POP17"
                                   };

            var idx = tbl.Delete( delete );//<-------------!!!!!!

            Aver.IsTrue( idx>=0 );
            Aver.AreEqual(999, tbl.Count);

            var match = tbl.FindByKey("POP17") as Person;
            Aver.IsNull( match );
        }

        [Run]
        public void PopulateAndDeleteNonExisting()
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

            var delete =  new Person{
                                    ID = "NONE17"
                                   };

            var idx = tbl.Delete( delete );//<-------------!!!!!!

            Aver.IsTrue( idx==-1 );
            Aver.AreEqual(1000, tbl.Count);

            var match = tbl.FindByKey("POP17") as Person;
            Aver.IsNotNull( match );
        }

        [Run]
        public void PopulateAndDeleteExisting_UsingValues()
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

            var idx = tbl.Delete( "POP17" );//<-------------!!!!!!

            Aver.IsTrue( idx>=0 );
            Aver.AreEqual(999, tbl.Count);

            var match = tbl.FindByKey("POP17") as Person;
            Aver.IsNull( match );
        }


        [Run]
        public void LogChanges_Insert()
        {
            var tbl = new Table(Schema.GetForTypedDoc(typeof(Person)));
            tbl.LogChanges = true;

            tbl.Insert( new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12
                                   });


            Aver.AreEqual(1, tbl.ChangeCount);

            Aver.IsTrue(DocChangeType.Insert == tbl.GetChangeAt(0).Value.ChangeType);
        }

        [Run]
        public void LogChanges_Update()
        {
            var tbl = new Table(Schema.GetForTypedDoc(typeof(Person)));


            tbl.Insert( new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12
                                   });
            tbl.LogChanges = true;

            tbl.Update( tbl[0] );


            Aver.AreEqual(1, tbl.ChangeCount);

            Aver.IsTrue(DocChangeType.Update == tbl.GetChangeAt(0).Value.ChangeType);
        }

        [Run]
        public void LogChanges_Upsert()
        {
            var tbl = new Table(Schema.GetForTypedDoc(typeof(Person)));

            tbl.LogChanges = true;

            tbl.Upsert( new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12
                                   });



            Aver.AreEqual(1, tbl.ChangeCount);

            Aver.IsTrue(DocChangeType.Upsert == tbl.GetChangeAt(0).Value.ChangeType);
        }

        [Run]
        public void LogChanges_Delete()
        {
            var tbl = new Table(Schema.GetForTypedDoc(typeof(Person)));


            tbl.Insert( new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12
                                   });
            tbl.LogChanges = true;

            tbl.Delete( tbl[0] );


            Aver.AreEqual(1, tbl.ChangeCount);
            Aver.AreEqual(0, tbl.Count);

            Aver.IsTrue(DocChangeType.Delete == tbl.GetChangeAt(0).Value.ChangeType);
        }

    }
}
