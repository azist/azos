/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos;
using Azos.Data;
using Azos.Data.Access;
using Azos.Serialization.JSON;

namespace Azos.Tests.Integration.CRUD
{
    internal static class TestLogic
    {
        public static void ExecuteCustomCommandHandler(ICRUDDataStore store)
        {
          var query = new Query("CustomTestCommandHandler") { new Query.Param("Msg", "we are on the moon!") };
          var result = store.Load(query);

          Aver.AreEqual(1, result.Count);
          var rowset = result[0];
          Aver.AreEqual(2, rowset.Count);

          Aver.AreEqual("Jack", rowset[0]["First_Name"].AsString());
          Aver.AreEqual("Mary", rowset[1]["First_Name"].AsString());
          Aver.AreEqual("we are on the moon!", rowset[1]["Address1"].AsString());
        }


        public static void QueryInsertQuery(ICRUDDataStore store)
        {
            var query = new Query("CRUD.Queries.Patient.List") { new Query.Param("LN", "%loff") };
            var result = store.Load( query );

            Aver.AreEqual(1, result.Count);
            var rowset = result[0];
            Aver.AreEqual(0, rowset.Count);

            var row = new DynamicDoc(rowset.Schema);

            row["ssn"] = "999-88-9012";
            row["First_Name"] = "Jack";
            row["Last_Name"] = "Kozloff";
            row["DOB"] = new DateTime(1980, 1, 12);

            Aver.IsNull( row.Validate());

            store.Insert(row);


            result = store.Load( query );

            Aver.AreEqual(1, result.Count);
            rowset = result[0];

            Aver.AreEqual(1, rowset.Count);
            Aver.AreObjectsEqual("Jack", rowset[0]["First_Name"]);

        }

        public static void ASYNC_QueryInsertQuery(ICRUDDataStore store)
        {
            var query = new Query("CRUD.Queries.Patient.List") { new Query.Param("LN", "%loff") };
            var task = store.LoadAsync( query );

            Aver.AreEqual(1, task.Result.Count);
            var rowset = task.Result[0];
            Aver.AreEqual(0, rowset.Count);

            var row = new DynamicDoc(rowset.Schema);

            row["ssn"] = "999-88-9012";
            row["First_Name"] = "Jack";
            row["Last_Name"] = "Kozloff";
            row["DOB"] = new DateTime(1980, 1, 12);

            Aver.IsNull( row.Validate());

            store.InsertAsync(row).Wait();


            task = store.LoadAsync( query );

            Aver.AreEqual(1, task.Result.Count);
            rowset = task.Result[0];

            Aver.AreEqual(1, rowset.Count);
            Aver.AreObjectsEqual("Jack", rowset[0]["First_Name"]);

        }

        public static void QueryInsertQuery_TypedRow(ICRUDDataStore store)
        {
            var query = new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff") };
            var result = store.Load( query );

            Aver.AreEqual(1, result.Count);
            var rowset = result[0];
            Aver.AreEqual(0, rowset.Count);

            var row = new Patient();

            row.SSN = "999-88-9012";
            row.First_Name = "Jack";
            row.Last_Name = "Kozloff";
            row.DOB = new DateTime(1980, 1, 12);

            Aver.IsNull( row.Validate());

            store.Insert(row);


            result = store.Load( query );

            Aver.AreEqual(1, result.Count);
            rowset = result[0];

            Aver.AreEqual(1, rowset.Count);
            Aver.IsTrue( rowset[0] is Patient );
            Aver.AreObjectsEqual("Jack", rowset[0]["First_Name"]);

        }

        public static void QueryInsertQuery_TypedRowDerived(ICRUDDataStore store)
        {
            var query = new Query("CRUD.Queries.Patient.List", typeof(SuperPatient) ) { new Query.Param("LN", "%loff") };
            var result = store.Load( query );

            Aver.AreEqual(1, result.Count);
            var rowset = result[0];
            Aver.AreEqual(0, rowset.Count);

            var row = new SuperPatient();

            row.SSN = "999-88-9012";
            row.First_Name = "Jack";
            row.Last_Name = "Kozloff";
            row.DOB = new DateTime(1980, 1, 12);
            row.Superman = true;

            Aver.IsNull( row.Validate());

            store.Insert(row);


            result = store.Load( query );

            Aver.AreEqual(1, result.Count);
            rowset = result[0];

            Aver.AreEqual(1, rowset.Count);
            Aver.IsTrue( rowset[0] is SuperPatient );
            Aver.AreObjectsEqual("Jack", rowset[0]["First_Name"]);

        }


        public static void QueryInsertQuery_DynamicRow(ICRUDDataStore store)
        {
            var query = new Query<DynamicDoc>("CRUD.Queries.Patient.List") { new Query.Param("LN", "%ruman") };
            var result = store.Load( query );

            Aver.AreEqual(1, result.Count);
            var rowset = result[0];
            Aver.AreEqual(0, rowset.Count);

            var row = new Patient();

            row.SSN = "999-88-9012";
            row.First_Name = "Mans";
            row.Last_Name = "Skolopendruman";
            row.DOB = new DateTime(1970, 1, 12);

            Aver.IsNull( row.Validate());

            store.Insert(row);


            var row2 = store.LoadDoc( query );

            Aver.IsNotNull(row2);
            Aver.IsTrue( row2 is DynamicDoc );
            Aver.AreObjectsEqual("Mans", row2["First_Name"]);

        }


        public static void InsertManyUsingLogChanges_TypedRow(ICRUDDataStore store)
        {
            var rowset = new Rowset( Schema.GetForTypedDoc(typeof(Patient)));
            rowset.LogChanges = true;

            for(var i=0; i<1000; i++)
            {
                rowset.Insert( new Patient
                               {
                                 SSN = "999-88-9012",
                                 First_Name = "Jack",
                                 Last_Name = "Kozloff"+i,
                                 DOB = new DateTime(1980, 1, 12)
                               });
            }

            for(var i=0; i<327; i++)
            {
                rowset.Insert( new Patient
                               {
                                 SSN = "999-88-9012",
                                 First_Name = "Jack",
                                 Last_Name = "Abramovich"+i,
                                 DOB = new DateTime(2001, 1, 12)
                               });
            }

            store.Save( rowset );

            var result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff%") } )[0];

            Aver.AreEqual(1000, result.Count);

            result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%ovich%") } )[0];

            Aver.AreEqual(327, result.Count);
        }

        public static void ASYNC_InsertManyUsingLogChanges_TypedRow(ICRUDDataStore store)
        {
            var rowset = new Rowset( Schema.GetForTypedDoc(typeof(Patient)));
            rowset.LogChanges = true;

            for(var i=0; i<1000; i++)
            {
                rowset.Insert( new Patient
                               {
                                 SSN = "999-88-9012",
                                 First_Name = "Jack",
                                 Last_Name = "Kozloff"+i,
                                 DOB = new DateTime(1980, 1, 12)
                               });
            }

            for(var i=0; i<327; i++)
            {
                rowset.Insert( new Patient
                               {
                                 SSN = "999-88-9012",
                                 First_Name = "Jack",
                                 Last_Name = "Abramovich"+i,
                                 DOB = new DateTime(2001, 1, 12)
                               });
            }

            store.SaveAsync( rowset ).Wait();

            var task = store.LoadAsync( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff%") } );

            Aver.AreEqual(1000, task.Result[0].Count);

            task = store.LoadAsync( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%ovich%") } );

            Aver.AreEqual(327, task.Result[0].Count);
        }




        public static void InsertInTransaction_Commit_TypedRow(ICRUDDataStore store)
        {
            var transaction = store.BeginTransaction();

            for(var i=0; i<25; i++)
            {
                transaction.Insert( new Patient
                               {
                                 SSN = "999-88-9012",
                                 First_Name = "Jack",
                                 Last_Name = "Kozloff"+i,
                                 DOB = new DateTime(1980, 1, 12)
                               });
            }


            var result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff%") } )[0];

            Aver.AreEqual(0, result.Count);

            transaction.Commit();

            result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff%") } )[0];

            Aver.AreEqual(25, result.Count);
        }


        public static void ASYNC_InsertInTransaction_Commit_TypedRow(ICRUDDataStore store)
        {
            var transaction = store.BeginTransactionAsync().Result;

            var tasks = new List<Task>();
            for(var i=0; i<25; i++)
            {
                tasks.Add(
                  transaction.InsertAsync( new Patient
                                 {
                                   SSN = "999-88-9012",
                                   First_Name = "Jack",
                                   Last_Name = "Kozloff"+i,
                                   DOB = new DateTime(1980, 1, 12)
                                 }));
            }

            Task.WaitAll(tasks.ToArray());


            var task = store.LoadAsync( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff%") } );

            Aver.AreEqual(0, task.Result[0].Count);

            transaction.Commit();

            task = store.LoadAsync( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff%") } );

            Aver.AreEqual(25, task.Result[0].Count);
        }



        public static void InsertInTransaction_Rollback_TypedRow(ICRUDDataStore store)
        {
            var transaction = store.BeginTransaction();

            for(var i=0; i<25; i++)
            {
                transaction.Insert( new Patient
                               {
                                 SSN = "999-88-9012",
                                 First_Name = "Jack",
                                 Last_Name = "Kozloff"+i,
                                 DOB = new DateTime(1980, 1, 12)
                               });
            }


            var result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff%") } )[0];

            Aver.AreEqual(0, result.Count);

            transaction.Rollback();

            result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff%") } )[0];

            Aver.AreEqual(0, result.Count);
        }


        public static void InsertThenUpdate_TypedRow(ICRUDDataStore store)
        {
            for(var i=0; i<10; i++)
            {
                store.Insert( new Patient
                               {
                                 SSN = i.ToString(),
                                 First_Name = "Jack",
                                 Last_Name = "Kozloff_"+i,
                                 DOB = new DateTime(1980, 1, 12)
                               });
            }


            var result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_5") } )[0];

            Aver.AreEqual(1, result.Count);
            var row = result[0] as Patient;
            Aver.AreEqual("5", row.SSN.Trim());

            row.Last_Name = "Gagarin";
            store.Update( row );

            result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_5") } )[0];
            Aver.AreEqual(0, result.Count);

            result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%garin") } )[0];
            Aver.AreEqual(1, result.Count);
            Aver.AreObjectsEqual("5", result[0]["SSN"].AsString().Trim());
            Aver.AreObjectsEqual("Gagarin", result[0]["Last_Name"]);
        }

        public static void ASYNC_InsertThenUpdate_TypedRow(ICRUDDataStore store)
        {
            for(var i=0; i<10; i++)
            {
                store.InsertAsync( new Patient
                               {
                                 SSN = i.ToString(),
                                 First_Name = "Jack",
                                 Last_Name = "Kozloff_"+i,
                                 DOB = new DateTime(1980, 1, 12)
                               }).Wait();
            }


            var result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_5") } )[0];

            Aver.AreEqual(1, result.Count);
            var row = result[0] as Patient;
            Aver.AreEqual("5", row.SSN.Trim());

            row.Last_Name = "Gagarin";
            store.UpdateAsync( row ).Wait();

            result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_5") } )[0];
            Aver.AreEqual(0, result.Count);

            result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%garin") } )[0];
            Aver.AreEqual(1, result.Count);
            Aver.AreObjectsEqual("5", result[0]["SSN"].AsString().Trim());
            Aver.AreObjectsEqual("Gagarin", result[0]["Last_Name"]);
        }



        public static void InsertThenDelete_TypedRow(ICRUDDataStore store)
        {
            for(var i=0; i<10; i++)
            {
                store.Insert( new Patient
                               {
                                 SSN = i.ToString(),
                                 First_Name = "Jack",
                                 Last_Name = "Kozloff_"+i,
                                 DOB = new DateTime(1980, 1, 12)
                               });
            }

            var result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_5") } )[0];

            Aver.AreEqual(1, result.Count);
            var row = result[0] as Patient;
            Aver.AreEqual("5", row.SSN.Trim());

            store.Delete( row );

            result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_5") } )[0];
            Aver.AreEqual(0, result.Count);

            result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_%") } )[0];
            Aver.AreEqual(9, result.Count);

        }

        public static void ASYNC_InsertThenDelete_TypedRow(ICRUDDataStore store)
        {
            for(var i=0; i<10; i++)
            {
                store.InsertAsync( new Patient
                               {
                                 SSN = i.ToString(),
                                 First_Name = "Jack",
                                 Last_Name = "Kozloff_"+i,
                                 DOB = new DateTime(1980, 1, 12)
                               }).Wait();
            }


            var result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_5") } )[0];

            Aver.AreEqual(1, result.Count);
            var row = result[0] as Patient;
            Aver.AreEqual("5", row.SSN.Trim());

            store.DeleteAsync( row ).Wait();

            result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_5") } )[0];
            Aver.AreEqual(0, result.Count);

            result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_%") } )[0];
            Aver.AreEqual(9, result.Count);

        }


        public static void InsertThenUpsert_TypedRow(ICRUDDataStore store)
        {
            for(var i=0; i<10; i++)
            {
                store.Insert( new Patient
                               {
                                 SSN = i.ToString(),
                                 First_Name = "Jack",
                                 Last_Name = "Kozloff_"+i,
                                 DOB = new DateTime(1980, 1, 12)
                               });
            }


            var result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_5") } )[0];

            Aver.AreEqual(1, result.Count);
            var row = result[0] as Patient;
            Aver.AreEqual("5", row.SSN.Trim());
            Aver.AreEqual(null, row.Phone);

            row.Phone = "22-94-92";
            store.Upsert( row );

            result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_5") } )[0];
            Aver.AreEqual(1, result.Count);
            Aver.AreObjectsEqual("22-94-92", result[0]["Phone"]);

            result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_%") } )[0];
            Aver.AreEqual(10, result.Count);

            row = new Patient
                               {
                                 SSN = "-100",
                                 First_Name = "Vlad",
                                 Last_Name = "Lenin",
                                 DOB = new DateTime(1871, 4, 20)
                               };

            store.Upsert(row);

            result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%") } )[0];
            Aver.AreEqual(11, result.Count);

            result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "Lenin") } )[0];
            Aver.AreEqual(1, result.Count);
            Aver.AreObjectsEqual("Vlad", result[0]["First_Name"]);

        }


        public static void ASYNC_InsertThenUpsert_TypedRow(ICRUDDataStore store)
        {
            for(var i=0; i<10; i++)
            {
                store.InsertAsync( new Patient
                               {
                                 SSN = i.ToString(),
                                 First_Name = "Jack",
                                 Last_Name = "Kozloff_"+i,
                                 DOB = new DateTime(1980, 1, 12)
                               }).Wait();
            }


            var result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_5") } )[0];

            Aver.AreEqual(1, result.Count);
            var row = result[0] as Patient;
            Aver.AreEqual("5", row.SSN.Trim());
            Aver.AreEqual(null, row.Phone);

            row.Phone = "22-94-92";
            store.UpsertAsync( row ).Wait();

            result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_5") } )[0];
            Aver.AreEqual(1, result.Count);
            Aver.AreObjectsEqual("22-94-92", result[0]["Phone"]);

            result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%loff_%") } )[0];
            Aver.AreEqual(10, result.Count);

            row = new Patient
                               {
                                 SSN = "-100",
                                 First_Name = "Vlad",
                                 Last_Name = "Lenin",
                                 DOB = new DateTime(1871, 4, 20)
                               };

            store.UpsertAsync(row).Wait();

            result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "%") } )[0];
            Aver.AreEqual(11, result.Count);

            result = store.Load( new Query("CRUD.Queries.Patient.List", typeof(Patient) ) { new Query.Param("LN", "Lenin") } )[0];
            Aver.AreEqual(1, result.Count);
            Aver.AreObjectsEqual("Vlad", result[0]["First_Name"]);

        }





        public static void GetSchemaAndTestVariousTypes(ICRUDDataStore store)
        {
            var schema = store.GetSchema(new Query("CRUD.Queries.Types.Load"));

            var row = new DynamicDoc(schema);
            row["GDID"] = new GDID(0, 145);
            row["SCREEN_NAME"] = "User1";
            row["STRING_NAME"] = "Some user 1";
            row["CHAR_NAME"] = "Some user 2";
            row["BOOL_CHAR"] = 'T';
            row["BOOL_BOOL"] = 'T';

            row["AMOUNT"] = 145670.23m;

            row["DOB"] = new DateTime(1980,12,1);

            store.Insert( row );

            var row2 = store.LoadOneDoc(new Query("CRUD.Queries.Types.Load", new GDID(0, 145)));

            Aver.IsNotNull(row2);
            Aver.AreObjectsEqual(145, row2["GDID"].AsInt());
            Aver.AreObjectsEqual("User1", row2["Screen_Name"].AsString().Trim());
            Aver.AreObjectsEqual("Some user 1", row2["String_Name"].AsString().Trim());
            Aver.AreObjectsEqual("Some user 2", row2["Char_Name"].AsString().Trim());

            Aver.AreEqual(true, row2["BOOL_Char"].AsBool());
            Aver.AreEqual(true, row2["BOOL_BOOL"].AsBool());

            Aver.AreObjectsEqual(145670.23m, row2["Amount"].AsDecimal());

            Aver.AreEqual(1980, row2["DOB"].AsDateTime().Year);


        }


        public static void ASYNC_GetSchemaAndTestVariousTypes(ICRUDDataStore store)
        {
            var schema = store.GetSchemaAsync(new Query("CRUD.Queries.Types.Load")).Result;

            var row = new DynamicDoc(schema);
            row["GDID"] = new GDID(0, 145);
            row["SCREEN_NAME"] = "User1";
            row["STRING_NAME"] = "Some user 1";
            row["CHAR_NAME"] = "Some user 2";
            row["BOOL_CHAR"] = 'T';
            row["BOOL_BOOL"] = 'T';

            row["AMOUNT"] = 145670.23m;

            row["DOB"] = new DateTime(1980,12,1);

            store.Insert( row );

            var row2 = store.LoadOneDoc(new Query("CRUD.Queries.Types.Load", new GDID(0, 145)));

            Aver.IsNotNull(row2);
            Aver.AreObjectsEqual(145, row2["GDID"].AsInt());
            Aver.AreObjectsEqual("User1", row2["Screen_Name"].AsString().Trim());
            Aver.AreObjectsEqual("Some user 1", row2["String_Name"].AsString().Trim());
            Aver.AreObjectsEqual("Some user 2", row2["Char_Name"].AsString().Trim());

            Aver.AreEqual(true, row2["BOOL_Char"].AsBool());
            Aver.AreEqual(true, row2["BOOL_BOOL"].AsBool());

            Aver.AreObjectsEqual(145670.23m, row2["Amount"].AsDecimal());

            Aver.AreEqual(1980, row2["DOB"].AsDateTime().Year);


        }


        public static void TypedRowTestVariousTypes(ICRUDDataStore store)
        {

            var row = new Types();
            row.GDID = new GDID(0, 234);
            row.Screen_Name = "User1";
            row.String_Name = "Some user 1";
            row.Char_Name = "Some user 2";
            row.Bool_Char = true; //notice TRUE for both char and bool columns below
            row.Bool_Bool = true;

            row["AMOUNT"] = 145670.23m;

            row["DOB"] = new DateTime(1980,12,1);
            row["Age"] = 145;

            store.Insert( row );

            var row2 = store.LoadDoc(new Query<Types>("CRUD.Queries.Types.Load", new GDID(0, 234)));

            Aver.IsNotNull(row2);
            Aver.AreEqual(new GDID(0,0,234), row2.GDID);
            Aver.AreEqual("User1", row2.Screen_Name);
            Aver.AreEqual("Some user 1", row2.String_Name);
            Aver.AreEqual("Some user 2", row2.Char_Name);

            Aver.AreEqual(true, row2.Bool_Char.Value);
            Aver.AreEqual(true, row2.Bool_Bool.Value);

            Aver.AreEqual(145670.23m, row2.Amount);

            Aver.AreEqual(1980, row2.DOB.Value.Year);

            Aver.AreEqual(145, row2.Age);

            row.Age = null;
            row.Bool_Bool = null;
            row.DOB = null;
            store.Update(row);

            var row3 = store.LoadDoc(new Query<Types>("CRUD.Queries.Types.Load", new GDID(0, 234)));
            Aver.IsFalse(row3.Age.HasValue);
            Aver.IsFalse(row3.Bool_Bool.HasValue);
            Aver.IsFalse(row3.DOB.HasValue);

            Aver.IsNull( row3["Age"].AsNullableInt());
            Aver.IsNull( row3["DOB"].AsNullableDateTime());
            Aver.IsNull( row3["Bool_Bool"].AsNullableBool());
        }


        public static void TypedRowTest_FullGDID(ICRUDDataStore store)
        {

            var row = new FullGDID();
            row.GDID = new GDID(123, 2, 8907893234);
            row.VARGDID = row.GDID;
            row["STRING_NAME"] = "AAA";

            store.Insert( row );

            var row2 = store.LoadOneDoc(new Query("CRUD.Queries.FullGDID.Load", new GDID(123, 2, 8907893234), typeof(FullGDID))) as FullGDID;

            Aver.IsNotNull(row2);
            Aver.AreEqual(new GDID(123, 2, 8907893234), row2.GDID);
            Aver.AreEqual(new GDID(123, 2, 8907893234), row2.VARGDID);
            Aver.AreObjectsEqual("AAA", row2["String_Name"]);
        }


        public static void GetSchemaAndTestFullGDID(ICRUDDataStore store)
        {
            var schema = store.GetSchema(new Query("CRUD.Queries.FullGDID.Load"));

            var row = new DynamicDoc(schema);

            var key = new GDID(179, 1, 1234567890);
            Console.WriteLine( key.Bytes.ToDumpString(DumpFormat.Hex));

            row["GDID"] = new GDID(179, 1, 1234567890);
            Console.WriteLine( ((byte[])row["GDID"]).ToDumpString(DumpFormat.Hex) );

            row["VARGDID"] = new GDID(12, 9, 9876543210);
            row["STRING_NAME"] = "DA DA DA!";

            store.Insert( row );

            var row2 = store.LoadOneDoc(new Query("CRUD.Queries.FullGDID.Load", key, typeof(FullGDID))) as FullGDID;

            Aver.IsNotNull(row2);
            Aver.AreEqual(key, row2.GDID);
            Aver.AreEqual(new GDID(12, 9, 9876543210), row2.VARGDID);
            Aver.AreObjectsEqual("DA DA DA!", row2["String_Name"]);
        }

        public static void InsertWithPredicate(ICRUDDataStore store)
        {
            var patient = TestLogic.GetDefaultPatient();
            var affected = store.Insert(patient, (r, k, f) => f.Name != "State" && f.Name != "Zip");
            Aver.AreEqual(1, affected);

            var query = new Query<Patient>("CRUD.Queries.Patient.List") { new Query.Param("LN", "%%") };
            var persisted = store.LoadDoc(query);
            Aver.IsNotNull(persisted);
            Aver.AreEqual(patient.First_Name, persisted.First_Name);
            Aver.AreEqual(patient.Last_Name, persisted.Last_Name);
            Aver.AreEqual(patient.SSN, persisted.SSN.Trim());
            Aver.AreEqual(patient.City, persisted.City);
            Aver.AreEqual(patient.Address1, persisted.Address1);
            Aver.AreEqual(patient.Address2, persisted.Address2);
            Aver.AreEqual(patient.Amount, persisted.Amount);
            Aver.AreEqual(patient.Doctor_Phone, persisted.Doctor_Phone.Trim());
            Aver.AreEqual(patient.Phone, persisted.Phone.Trim());
            Aver.AreEqual(patient.DOB, persisted.DOB);
            Aver.AreEqual(patient.Note, persisted.Note);

            Aver.IsNull(persisted.State);
            Aver.IsNull(persisted.Zip);
        }

        public static void UpdateWithPredicate(ICRUDDataStore store)
        {
            var patient = TestLogic.GetDefaultPatient();
            var affected = store.Insert(patient);
            Aver.AreEqual(1, affected);
            var query = new Query<Patient>("CRUD.Queries.Patient.List") { new Query.Param("LN", "%%") };
            var persisted = store.LoadDoc(query);

            persisted.Zip = "010203";
            persisted.First_Name = "John";
            persisted.Last_Name = "Smith";
            affected = store.Update(persisted, null, (r, k, f) => f.Name != "First_Name" && f.Name != "Zip");
            Aver.AreEqual(1, affected);

            var updated = store.LoadDoc(query);
            Aver.IsNotNull(updated);
            Aver.AreEqual(persisted.SSN, updated.SSN.Trim());
            Aver.AreEqual(persisted.City, updated.City);
            Aver.AreEqual(persisted.Address1, updated.Address1);
            Aver.AreEqual(persisted.Address2, updated.Address2);
            Aver.AreEqual(persisted.Amount, updated.Amount);
            Aver.AreEqual(persisted.Doctor_Phone, updated.Doctor_Phone.Trim());
            Aver.AreEqual(persisted.Phone, updated.Phone.Trim());
            Aver.AreEqual(persisted.DOB, updated.DOB);
            Aver.AreEqual(persisted.Note, updated.Note);

            Aver.AreEqual(patient.First_Name, updated.First_Name);
            Aver.AreEqual(persisted.Last_Name, updated.Last_Name);
            Aver.AreEqual(patient.Zip, updated.Zip);
        }

        public static void UpsertWithPredicate(ICRUDDataStore store)
        {
            var patient = TestLogic.GetDefaultPatient();
            var affected = store.Insert(patient);
            Aver.AreEqual(1, affected);
            var query = new Query<Patient>("CRUD.Queries.Patient.List") { new Query.Param("LN", "%%") };
            var persisted = store.LoadDoc(query);

            persisted.Zip = "010203";
            persisted.First_Name = "John";
            persisted.Last_Name = "Smith";
            affected = store.Upsert(persisted, (r, k, f) => f.Name != "Zip");
            Aver.AreEqual(2, affected);

            var updated = store.LoadDoc(query);
            Aver.IsNotNull(updated);
            Aver.AreEqual(persisted.SSN, updated.SSN.Trim());
            Aver.AreEqual(persisted.City, updated.City);
            Aver.AreEqual(persisted.Address1, updated.Address1);
            Aver.AreEqual(persisted.Address2, updated.Address2);
            Aver.AreEqual(persisted.Amount, updated.Amount);
            Aver.AreEqual(persisted.Doctor_Phone, updated.Doctor_Phone.Trim());
            Aver.AreEqual(persisted.Phone, updated.Phone.Trim());
            Aver.AreEqual(persisted.DOB, updated.DOB);
            Aver.AreEqual(persisted.Note, updated.Note);

            Aver.AreEqual(persisted.First_Name, updated.First_Name);
            Aver.AreEqual(persisted.Last_Name, updated.Last_Name);
            Aver.AreEqual(patient.Zip, updated.Zip); // notice ZIP remains the same
        }


        public static void Populate_OpenCursor(ICRUDDataStore store)
        {
            const int CNT = 1000;

            for(var i=0; i<CNT; i++)
            {
              var patient = new TupleData
              {
                 COUNTER = i,
                 DATA = i.ToString()+"-DATA"
              };
              store.Insert( patient );
            }



            var query = new Query<TupleData>("CRUD.Queries.Tuple.LoadAll");
            var result = store.LoadOneRowset( query );

            Aver.AreEqual(CNT, result.Count);

            Aver.AreObjectsEqual(0, result[0]["COUNTER"].AsInt());
            Aver.AreObjectsEqual(CNT-1, result[result.Count-1]["COUNTER"].AsInt());

            {
                using(var cursor = store.OpenCursor( query ))
                {
                   Aver.IsFalse( cursor.Disposed );
                   var cnt = 0;
                   foreach(var row in cursor.AsEnumerableOf<TupleData>())
                    cnt++;

                   Aver.AreEqual(CNT, cnt);
                   Aver.IsTrue( cursor.Disposed );//foreach must have closed the cursor
                }
            }

            {
                var cursor = store.OpenCursor( query );

                Aver.IsFalse( cursor.Disposed );

                var cen = cursor.GetEnumerator();
                cen.MoveNext();
                Aver.IsNotNull( cen.Current );

                Console.WriteLine( cen.Current.Schema.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap) );

                Aver.AreObjectsEqual(0, cen.Current["COUNTER"].AsInt());
                Aver.AreObjectsEqual("0-DATA", cen.Current["DATA"]);

                cen.MoveNext();
                Aver.IsNotNull( cen.Current );
                Aver.AreObjectsEqual(1, cen.Current["COUNTER"].AsInt());
                Aver.AreObjectsEqual("1-DATA", cen.Current["DATA"]);

                cen.MoveNext();
                Aver.IsNotNull( cen.Current );
                Aver.AreObjectsEqual(2, cen.Current["COUNTER"].AsInt());
                Aver.AreObjectsEqual("2-DATA", cen.Current["DATA"]);


                cursor.Dispose();
                Aver.IsTrue( cursor.Disposed );
            }

            {
                using(var cursor = store.OpenCursor( query ))
                {
                   Aver.IsFalse( cursor.Disposed );
                   var cnt = 0;
                   foreach(var row in cursor.AsEnumerableOf<TupleData>())
                    cnt++;

                   Aver.AreEqual(CNT, cnt);
                   Aver.IsTrue( cursor.Disposed );//foreach must have closed the cursor
                   try
                   {
                     foreach(var row in cursor.AsEnumerableOf<TupleData>())
                     Aver.Fail("Must have failed");
                   }
                   catch
                   {

                   }

                }
            }

            {
               var cursor = store.OpenCursor( query );

                Aver.IsFalse( cursor.Disposed );

                var cen = cursor.GetEnumerator();
                cen.MoveNext();
                Aver.IsNotNull( cen.Current );
                Aver.AreObjectsEqual(0, cen.Current["COUNTER"].AsInt());

                try
                {
                  Aver.IsFalse( cursor.Disposed );

                  var cen2 = cursor.GetEnumerator();
                  Aver.Fail("This should not have heppened as cant iterate cursor the second time");
                }
                catch
                {

                }

                cursor.Dispose();
                Aver.IsTrue( cursor.Disposed );
            }

        }

        public static void Populate_ASYNC_OpenCursor(ICRUDDataStore store)
        {
            const int CNT = 1000;

            for(var i=0; i<CNT; i++)
            {
              var patient = new TupleData
              {
                 COUNTER = i,
                 DATA = i.ToString()+"-DATA"
              };
              store.Insert( patient );
            }



            var query = new Query<TupleData>("CRUD.Queries.Tuple.LoadAll");
            var result = store.LoadOneRowset( query );

            Aver.AreEqual(CNT, result.Count);

            Aver.AreObjectsEqual(0, result[0]["COUNTER"].AsInt());
            Aver.AreObjectsEqual(CNT-1, result[result.Count-1]["COUNTER"].AsInt());

            var task = store.OpenCursorAsync( query )
                              .ContinueWith( antecedent =>
                                {
                                    var cursor = antecedent.Result;;
                                    Aver.IsFalse( cursor.Disposed );
                                    var cnt = 0;
                                    foreach(var row in cursor.AsEnumerableOf<TupleData>())
                                    cnt++;

                                    Aver.AreEqual(CNT, cnt);
                                    Aver.IsTrue( cursor.Disposed );//foreach must have closed the cursor
                                });
            task.Wait();

        }


        public static Patient GetDefaultPatient()
        {
            var patient = new Patient
                            {
                                First_Name = "Ivan",
                                Last_Name ="Poddubny",
                                SSN = "123456",
                                City = "New York",
                                Address1 = "addr_1",
                                Address2 = "addr_2",
                                Amount = 123,
                                Phone = "(123)456-78-90",
                                State = "NY",
                                DOB = new DateTime(1984, 11, 12),
                                Note = "...",
                                Zip = "350004"
                            };

            return patient;
        }
    }
}
