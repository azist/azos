/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
  
using System;
using System.Linq;
using System.Collections.Generic;

using Azos.Scripting;
using Azos.Data;

namespace Azos.Tests.Unit.DataAccess
{
    [Runnable(TRUN.BASE, 3)]
    public class RowCopyFields
    {
        [Run]
        public void CopyFields_TypedRow()
        {
            var from = new Person
            {
                FirstName = "Ivan",
                LastName = "Petrov",
                Amount = 10,
                Classification = "class1",
                Description = null,
                DOB = new DateTime(1990, 2, 16),
                GoodPerson = true,
                ID = "abc",
                LuckRatio = 12345.6789D,
                YearsInSpace = 20,
                YearsWithCompany = null
            };

            var to = new Person
            {
                Description = "descr",
                YearsWithCompany = 30
            };

            from.CopyFields(to);

            Aver.AreEqual(to.FirstName, from.FirstName);
            Aver.AreEqual(to.LastName, from.LastName);
            Aver.AreEqual(to.Amount, from.Amount);
            Aver.AreEqual(to.Classification, from.Classification);
            Aver.AreEqual(to.Description, from.Description);
            Aver.AreEqual(to.DOB, from.DOB);
            Aver.AreEqual(to.GoodPerson, from.GoodPerson);
            Aver.AreEqual(to.ID, from.ID);
            Aver.AreEqual(to.LuckRatio, from.LuckRatio);
            Aver.AreEqual(to.YearsInSpace, from.YearsInSpace);
            Aver.AreEqual(to.YearsWithCompany, from.YearsWithCompany);
        }

        [Run]
        public void CopyFields_TypedRow_Filter()
        {
            var from = new Person
            {
                FirstName = "Ivan",
                Amount = 10,
                DOB = new DateTime(1990, 2, 16),
                GoodPerson = true,
                LuckRatio = 12345.6789D,
                YearsInSpace = 20,
                YearsWithCompany = null
            };

            var to = new Person
            {
                Description = "descr",
                DOB = new DateTime(1980, 2, 16),
                GoodPerson = false,
                LuckRatio = 12345.6789D,
                YearsWithCompany = 30
            };

            from.CopyFields(to, false, false, (n, f) => f.Name != "DOB" && f.Name != "GoodPerson" );

            Aver.AreEqual(to.FirstName, from.FirstName);
            Aver.AreEqual(to.LastName, from.LastName);
            Aver.AreEqual(to.Amount, from.Amount);
            Aver.AreEqual(to.Classification, from.Classification);
            Aver.AreEqual(to.Description, from.Description);
            Aver.AreEqual(new DateTime(1980, 2, 16), to.DOB);
            Aver.AreEqual(false, to.GoodPerson);
            Aver.AreEqual(to.ID, from.ID);
            Aver.AreEqual(to.LuckRatio, from.LuckRatio);
            Aver.AreEqual(to.YearsInSpace, from.YearsInSpace);
            Aver.AreEqual(to.YearsWithCompany, from.YearsWithCompany);
        }

        [Run]
        public void CopyFields_TypedRow_Complex()
        {
            var from = new ExtendedPerson
            {
                FirstName = "Ivan",
                Parent = new Person { FirstName = "John", Amount = 12, GoodPerson = true },
                Children = new List<Person> { new Person { FirstName = "John" }, new Person { LuckRatio = 12.3D } }
            };

            var to = new ExtendedPerson
            {
                FirstName = "Anna",
                Description = "descr",
                YearsWithCompany = 30,
                Parent = new Person { FirstName = "Maria" }
            };

            from.CopyFields(to);

            Aver.AreEqual(to.FirstName, from.FirstName);
            Aver.AreEqual(to.LastName, from.LastName);
            Aver.AreEqual(to.Amount, from.Amount);
            Aver.AreEqual(to.Classification, from.Classification);
            Aver.AreEqual(to.Description, from.Description);
            Aver.AreEqual(to.DOB, from.DOB);
            Aver.AreEqual(to.GoodPerson, from.GoodPerson);
            Aver.AreEqual(to.ID, from.ID);
            Aver.AreEqual(to.LuckRatio, from.LuckRatio);
            Aver.AreEqual(to.YearsInSpace, from.YearsInSpace);
            Aver.AreEqual(to.YearsWithCompany, from.YearsWithCompany);
            Aver.AreEqual(to.Info, from.Info);
            Aver.AreObjectsEqual(to.Parent, from.Parent);
            Aver.AreObjectsEqual(to.Children, from.Children);
        }

        [Run]
        public void CopyFields_TypedRow_To_Extended()
        {
            var from = new Person
            {
                FirstName = "Ivan",
                Amount = 10,
                DOB = new DateTime(1990, 2, 16),
                YearsWithCompany = null
            };

            var to = new ExtendedPerson
            {
                FirstName = "John",
                Description = "descr",
                YearsWithCompany = 30,
                Info = "extended info",
                Count = long.MaxValue
            };

            from.CopyFields(to);

            Aver.AreEqual(to.FirstName, from.FirstName);
            Aver.AreEqual(to.LastName, from.LastName);
            Aver.AreEqual(to.Amount, from.Amount);
            Aver.AreEqual(to.Classification, from.Classification);
            Aver.AreEqual(to.Description, from.Description);
            Aver.AreEqual(to.DOB, from.DOB);
            Aver.AreEqual(to.GoodPerson, from.GoodPerson);
            Aver.AreEqual(to.ID, from.ID);
            Aver.AreEqual(to.LuckRatio, from.LuckRatio);
            Aver.AreEqual(to.YearsInSpace, from.YearsInSpace);
            Aver.AreEqual(to.YearsWithCompany, from.YearsWithCompany);
            Aver.AreEqual("extended info", to.Info);
            Aver.AreEqual(long.MaxValue, to.Count);
        }

        [Run]
        public void CopyFields_Extended_To_TypedRow()
        {
            var from = new ExtendedPerson
            {
                FirstName = "John",
                Description = "descr",
                YearsWithCompany = 30,
                Info = "extended info",
                Count = long.MaxValue
            };

            var to = new Person
            {
                FirstName = "Ivan",
                Amount = 10,
                DOB = new DateTime(1990, 2, 16),
                YearsWithCompany = null
            };

            from.CopyFields(to);

            Aver.AreEqual(to.FirstName, from.FirstName);
            Aver.AreEqual(to.LastName, from.LastName);
            Aver.AreEqual(to.Amount, from.Amount);
            Aver.AreEqual(to.Classification, from.Classification);
            Aver.AreEqual(to.Description, from.Description);
            Aver.AreEqual(to.DOB, from.DOB);
            Aver.AreEqual(to.GoodPerson, from.GoodPerson);
            Aver.AreEqual(to.ID, from.ID);
            Aver.AreEqual(to.LuckRatio, from.LuckRatio);
            Aver.AreEqual(to.YearsInSpace, from.YearsInSpace);
            Aver.AreEqual(to.YearsWithCompany, from.YearsWithCompany);
        }

        [Run]
        public void CopyFields_TypedRow_To_Amorphous_NotIncludeAmorphous()
        {
            var from = new Person
            {
                FirstName = "Ivan",
                LastName = "Petrov",
                Amount = 10,
                Classification = "class1",
                YearsWithCompany = null
            };

            var to = new AmorphousDynamicRow(Schema.GetForTypedDoc(typeof(Empty)));
            to.AmorphousData["field1"] = 123;
            to.AmorphousData["FirstName"] = "John";

            from.CopyFields(to, false);

            Aver.AreEqual(0, to.Schema.FieldCount);
            Aver.AreEqual(2, to.AmorphousData.Count);
            Aver.AreObjectsEqual(123, to.AmorphousData["field1"]);
            Aver.AreObjectsEqual("John", to.AmorphousData["FirstName"]);
        }

        [Run]
        public void CopyFields_TypedRow_To_Amorphous_IncludeAmorphous()
        {
            var from = new Person
            {
                FirstName = "Ivan",
                LastName = "Petrov",
                Amount = 10,
                Classification = "class1",
                YearsWithCompany = null
            };

            var to = new AmorphousDynamicRow(Schema.GetForTypedDoc(typeof(Empty)));
            to.AmorphousData["field1"] = 123;
            to.AmorphousData["FirstName"] = "John";

            from.CopyFields(to, true);

            Aver.AreEqual(0, to.Schema.FieldCount);
            Aver.AreEqual(12, to.AmorphousData.Count);
            Aver.AreObjectsEqual(123, to.AmorphousData["field1"]);
            Aver.AreObjectsEqual(from.FirstName, to.AmorphousData["FirstName"]);
            Aver.AreObjectsEqual(from.LastName, to.AmorphousData["LastName"]);
            Aver.AreObjectsEqual(from.Amount, to.AmorphousData["Amount"]);
            Aver.AreObjectsEqual(from.Classification, to.AmorphousData["Classification"]);
            Aver.AreObjectsEqual(from.YearsWithCompany, to.AmorphousData["YearsWithCompany"]);
        }

        [Run]
        public void CopyFields_ExtendedTypedRow_To_Amorphous_IncludeAmorphous()
        {
            var from = new ExtendedPerson
            {
                FirstName = "Ivan",
                Amount = 10,
                YearsWithCompany = null,
                Count = 4567,
                Info = "extended info"
            };

            var to = new AmorphousDynamicRow(Schema.GetForTypedDoc(typeof(Person)));
            to.AmorphousData["field1"] = 123;
            to.AmorphousData["FirstName"] = "John";

            from.CopyFields(to, true);

            Aver.AreEqual(11, to.Schema.FieldCount);
            Aver.AreEqual(6, to.AmorphousData.Count);
            Aver.AreObjectsEqual(123, to.AmorphousData["field1"]);
            Aver.AreObjectsEqual("John", to.AmorphousData["FirstName"]);
            Aver.AreObjectsEqual(from.Count, to.AmorphousData["Count"]);
            Aver.AreObjectsEqual(from.Info, to.AmorphousData["Info"]);
            Aver.AreObjectsEqual(null, to.AmorphousData["Parent"]);
            Aver.AreObjectsEqual(null, to.AmorphousData["Children"]);
            Aver.AreObjectsEqual(from.FirstName, to["FirstName"]);
            Aver.AreObjectsEqual(from.Amount, to["Amount"]);
            Aver.AreObjectsEqual(from.YearsWithCompany, to["YearsWithCompany"]);
        }

        [Run]
        public void CopyFields_DynamicRow()
        {
            var from = new DynamicRow(Schema.GetForTypedDoc(typeof(Person)));
            from["FirstName"] = "Ivan";
            from["LastName"] = "Petrov";
            from["Amount"] = 10;
            from["Classification"] = "class1";
            from["Description"] = null;
            from["DOB"] = new DateTime(1990, 2, 16);
            from["GoodPerson"] = true;
            from["ID"] = "abc";
            from["LuckRatio"] = 12345.6789D;
            from["YearsInSpace"] = 20;
            from["YearsWithCompany"] = null;

            var to = new DynamicRow(Schema.GetForTypedDoc(typeof(Person)));
            to["Description"] = "descr";
            to["YearsWithCompany"] = 30;

            from.CopyFields(to);

            Aver.AreObjectsEqual(to["FirstName"], from["FirstName"]);
            Aver.AreObjectsEqual(to["LastName"], from["LastName"]);
            Aver.AreObjectsEqual(to["Amount"], from["Amount"]);
            Aver.AreObjectsEqual(to["Classification"], from["Classification"]);
            Aver.AreObjectsEqual(to["Description"], from["Description"]);
            Aver.AreObjectsEqual(to["DOB"], from["DOB"]);
            Aver.AreObjectsEqual(to["GoodPerson"], from["GoodPerson"]);
            Aver.AreObjectsEqual(to["ID"], from["ID"]);
            Aver.AreObjectsEqual(to["LuckRatio"], from["LuckRatio"]);
            Aver.AreObjectsEqual(to["YearsInSpace"], from["YearsInSpace"]);
            Aver.AreObjectsEqual(to["YearsWithCompany"], from["YearsWithCompany"]);
        }

        [Run]
        public void CopyFields_DynamicRow_To_Extended()
        {
            var schema = Schema.GetForTypedDoc(typeof(Person));
            var from = new DynamicRow(schema);
            from["FirstName"] = "Ivan";
            from["Amount"] = 10;
            from["DOB"] = new DateTime(1990, 2, 16);
            from["GoodPerson"] = true;

            var fieldDefs = schema.FieldDefs.ToList();
            fieldDefs.Add(new Schema.FieldDef("Info", typeof(string), new QuerySource.ColumnDef("Info")));
            fieldDefs.Add(new Schema.FieldDef("Count", typeof(long), new QuerySource.ColumnDef("Info")));
            var extendedSchema = new Schema("sname", fieldDefs.ToArray());

            var to = new DynamicRow(extendedSchema);
            to["Info"] = "extended info";
            to["Count"] = long.MaxValue;

            from.CopyFields(to);

            Aver.AreObjectsEqual(to["FirstName"], from["FirstName"]);
            Aver.AreObjectsEqual(to["Amount"], from["Amount"]);
            Aver.AreObjectsEqual(to["DOB"], from["DOB"]);
            Aver.AreObjectsEqual(to["GoodPerson"], from["GoodPerson"]);
            Aver.AreObjectsEqual("extended info", to["Info"]);
            Aver.AreObjectsEqual(long.MaxValue, to["Count"]);
        }

        [Run]
        public void CopyFields_ExtendedDynamicRow_To_DynamicRow()
        {
            var schema = Schema.GetForTypedDoc(typeof(Person));
            var fieldDefs = schema.FieldDefs.ToList();
            fieldDefs.Add(new Schema.FieldDef("Info", typeof(string), new QuerySource.ColumnDef("Info")));
            fieldDefs.Add(new Schema.FieldDef("Count", typeof(long), new QuerySource.ColumnDef("Info")));
            var extendedSchema = new Schema("sname", fieldDefs.ToArray());

            var from = new DynamicRow(extendedSchema);
            from["FirstName"] = "Ivan";
            from["Amount"] = 10;
            from["DOB"] = new DateTime(1990, 2, 16);
            from["GoodPerson"] = true;
            from["Info"] = "extended info";
            from["Count"] = long.MaxValue;

            var to = new DynamicRow(schema);

            from.CopyFields(to);

            Aver.AreObjectsEqual(to["FirstName"], from["FirstName"]);
            Aver.AreObjectsEqual(to["Amount"], from["Amount"]);
            Aver.AreObjectsEqual(to["DOB"], from["DOB"]);
            Aver.AreObjectsEqual(to["GoodPerson"], from["GoodPerson"]);
        }

        [Run]
        public void CopyFields_DynamicRow_To_Amorphous_NotIncludeAmorphous()
        {
            var schema = Schema.GetForTypedDoc(typeof(Person));
            var from = new DynamicRow(schema);
            from["FirstName"] = "Ivan";
            from["Amount"] = 10;
            from["DOB"] = new DateTime(1990, 2, 16);
            from["GoodPerson"] = true;

            var to = new AmorphousDynamicRow(Schema.GetForTypedDoc(typeof(Empty)));
            to.AmorphousData["field1"] = 123;
            to.AmorphousData["FirstName"] = "John";

            from.CopyFields(to, false);

            Aver.AreEqual(0, to.Schema.FieldCount);
            Aver.AreEqual(2, to.AmorphousData.Count);
            Aver.AreObjectsEqual(123, to.AmorphousData["field1"]);
            Aver.AreObjectsEqual("John", to.AmorphousData["FirstName"]);
        }

        [Run]
        public void CopyFields_DynamicRow_To_Amorphous_IncludeAmorphous()
        {
            var schema = Schema.GetForTypedDoc(typeof(Person));
            var from = new DynamicRow(schema);
            from["FirstName"] = "Ivan";
            from["Amount"] = 10;
            from["DOB"] = new DateTime(1990, 2, 16);
            from["GoodPerson"] = true;

            var to = new AmorphousDynamicRow(Schema.GetForTypedDoc(typeof(Empty)));
            to.AmorphousData["field1"] = 123;
            to.AmorphousData["FirstName"] = "John";

            from.CopyFields(to, true);

            Aver.AreEqual(0, to.Schema.FieldCount);
            Aver.AreEqual(12, to.AmorphousData.Count);
            Aver.AreObjectsEqual(123, to.AmorphousData["field1"]);
            Aver.AreObjectsEqual(from["FirstName"], to.AmorphousData["FirstName"]);
            Aver.AreObjectsEqual(from["Amount"], to.AmorphousData["Amount"]);
            Aver.AreObjectsEqual(from["DOB"], to.AmorphousData["DOB"]);
            Aver.AreObjectsEqual(from["GoodPerson"], to.AmorphousData["GoodPerson"]);
        }

        [Run]
        public void CopyFields_AmorphousDynamicRow_To_TypedRow()
        {
            var from = new AmorphousDynamicRow(Schema.GetForTypedDoc(typeof(Person)));
            from["FirstName"] = "Ivan";
            from["LuckRatio"] = 12345.6789D;
            from.AmorphousData["field1"] = "some data";

            var to = new Person
            {
                FirstName = "Jack",
                Description = "descr",
                YearsWithCompany = 30
            };

            from.CopyFields(to);

            Aver.AreObjectsEqual(to["FirstName"], from["FirstName"]);
            Aver.AreObjectsEqual(to["LuckRatio"], from["LuckRatio"]);
            Aver.AreObjectsEqual(to["YearsWithCompany"], from["YearsWithCompany"]);
            Aver.AreObjectsEqual(null, to.Schema["field1"]);
        }

        [Run]
        public void CopyFields_AmorphousDynamicRow_To_DynamicRow()
        {
            var from = new AmorphousDynamicRow(Schema.GetForTypedDoc(typeof(Person)));
            from["FirstName"] = "Ivan";
            from["LuckRatio"] = 12345.6789D;
            from.AmorphousData["field1"] = "some data";

            var to = new DynamicRow(Schema.GetForTypedDoc(typeof(Person)));
            to["Description"] = "descr";
            to["YearsWithCompany"] = 30;

            from.CopyFields(to);

            Aver.AreObjectsEqual(to["FirstName"], from["FirstName"]);
            Aver.AreObjectsEqual(to["LuckRatio"], from["LuckRatio"]);
            Aver.AreObjectsEqual(to["Description"], from["Description"]);
            Aver.AreObjectsEqual(to["YearsWithCompany"], from["YearsWithCompany"]);
            Aver.AreObjectsEqual(null, to.Schema["field1"]);
        }

        [Run]
        public void CopyFields_AmorphousDynamicRow_NotIncludeAmorphous()
        {
            var from = new AmorphousDynamicRow(Schema.GetForTypedDoc(typeof(Person)));
            from["FirstName"] = "Ivan";
            from["LuckRatio"] = 12345.6789D;
            from.AmorphousData["field1"] = "some data";

            var to = new AmorphousDynamicRow(Schema.GetForTypedDoc(typeof(Person)));
            from["FirstName"] = "Jack";
            from["YearsInSpace"] = 20;

            from.CopyFields(to, false);

            Aver.AreObjectsEqual(to["FirstName"], from["FirstName"]);
            Aver.AreObjectsEqual(to["LuckRatio"], from["LuckRatio"]);
            Aver.AreObjectsEqual(to["YearsInSpace"], from["YearsInSpace"]);
            Aver.AreObjectsEqual(null, to.Schema["field1"]);
            Aver.AreEqual(0, to.AmorphousData.Count);
        }

        [Run]
        public void CopyFields_AmorphousDynamicRow_IncludeAmorphous()
        {
            var from = new AmorphousDynamicRow(Schema.GetForTypedDoc(typeof(Empty)));
            from.AmorphousData["field1"] = "some data";
            from.AmorphousData["field2"] = 123;

            var to = new AmorphousDynamicRow(Schema.GetForTypedDoc(typeof(Empty)));
            to.AmorphousData["field2"] = "234";
            to.AmorphousData["field3"] = 1.2D;

            from.CopyFields(to, true);

            Aver.AreEqual(3, to.AmorphousData.Count);
            Aver.AreObjectsEqual(from.AmorphousData["field1"], to.AmorphousData["field1"]);
            Aver.AreObjectsEqual(from.AmorphousData["field2"], to.AmorphousData["field2"]);
            Aver.AreObjectsEqual(1.2D, to.AmorphousData["field3"]);
        }

        [Run]
        public void CopyFields_AmorphousDynamicRow_Filter()
        {
            var from = new AmorphousDynamicRow(Schema.GetForTypedDoc(typeof(Empty)));
            from.AmorphousData["field1"] = "some data";
            from.AmorphousData["field2"] = 123;
            from.AmorphousData["field3"] = "info";

            var to = new AmorphousDynamicRow(Schema.GetForTypedDoc(typeof(Empty)));
            to.AmorphousData["field2"] = "234";
            to.AmorphousData["field3"] = 1.2D;
            to.AmorphousData["field4"] = 12345;

            from.CopyFields(to, true, false, null, (s, n) => n != "field2" );

            Aver.AreEqual(4, to.AmorphousData.Count);
            Aver.AreObjectsEqual(from.AmorphousData["field1"], to.AmorphousData["field1"]);
            Aver.AreObjectsEqual("234", to.AmorphousData["field2"]);
            Aver.AreObjectsEqual(to.AmorphousData["field3"], to.AmorphousData["field3"]);
            Aver.AreObjectsEqual(12345, to.AmorphousData["field4"]);
        }
    }
}
