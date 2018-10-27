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


using Azos.DataAccess.CRUD;


namespace Azos.Tests.Unit.DataAccess
{
    [Runnable(TRUN.BASE, 3)]
    public class DynamicRows
    {

        [Run]
        public void BuildUsingTypedSchema()
        {
            var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));

            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] = "Popov";
            person["DOB"] = new DateTime(1953, 12, 10);
            person["YearsInSpace"] = 12;

            Aver.AreObjectsEqual( "POP1",                       person["ID"] );
            Aver.AreObjectsEqual( "Oleg",                       person["FirstName"] );
            Aver.AreObjectsEqual( "Popov",                      person["LastName"] );
            Aver.AreObjectsEqual( new DateTime(1953, 12, 10),   person["DOB"] );
            Aver.AreObjectsEqual( 12,                           person["YearsInSpace"] );
        }


        [Run]
        public void BuildUsingAdHockSchema()
        {
            var schema = new Schema("TEZT",
                           new Schema.FieldDef("ID", typeof(int), new List<FieldAttribute>{ new FieldAttribute(required: true, key: true)}),
                           new Schema.FieldDef("Description", typeof(string), new List<FieldAttribute>{ new FieldAttribute(required: true)})
            );

            var person = new DynamicRow(schema);

            person["ID"] = 123;
            person["Description"] = "Tank";

            Aver.AreObjectsEqual( 123,                       person["ID"] );
            Aver.AreObjectsEqual( "Tank",                    person["Description"] );
        }


        [Run]
        public void SetValuesAsDifferentTypes()
        {
            var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));

            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] = "Popov";
            person["DOB"] = new DateTime(1953, 12, 10);
            person["YearsInSpace"] = 12;

            Aver.AreObjectsEqual( "POP1",                       person["ID"] );
            Aver.AreObjectsEqual( "Oleg",                       person["FirstName"] );
            Aver.AreObjectsEqual( "Popov",                      person["LastName"] );
            Aver.AreObjectsEqual( new DateTime(1953, 12, 10),   person["DOB"] );
            Aver.AreObjectsEqual( 12,                           person["YearsInSpace"] );

            person["DOB"] = "05/15/2009 18:00";
            Aver.AreObjectsEqual( new DateTime(2009, 5, 15, 18, 0, 0),   person["DOB"] );

            person["DOB"] = null;
            Aver.IsNull( person["DOB"] );

            person["YearsInSpace"] = "-190";
            Aver.AreObjectsEqual( -190,   person["YearsInSpace"] );

            person["Description"] = 2+2;
            Aver.AreObjectsEqual( "4",   person["Description"] );

            person["Amount"] = "180.12";
            Aver.AreObjectsEqual( 180.12m,   person["Amount"] );

            person["GoodPerson"] = "true";
            Aver.AreObjectsEqual( true,   person["GoodPerson"] );

            person["LuckRatio"] = 123;
            Aver.AreObjectsEqual( 123d,   person["LuckRatio"] );

        }


        [Run]
        public void SetGetAndValidate_NoError()
        {
            var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));

            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] = "Popov";
            person["DOB"] = new DateTime(1953, 12, 10);
            person["YearsInSpace"] = 12;

            var error = person.Validate();
            Aver.IsNull( error );

            Aver.AreObjectsEqual( "POP1",                       person["ID"] );
            Aver.AreObjectsEqual( "Oleg",                       person["FirstName"] );
            Aver.AreObjectsEqual( "Popov",                      person["LastName"] );
            Aver.AreObjectsEqual( new DateTime(1953, 12, 10),   person["DOB"] );
            Aver.AreObjectsEqual( 12,                           person["YearsInSpace"] );
        }

        [Run]
        public void Validate_Error_StringRequired()
        {
            var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] = null;
            person["DOB"] = new DateTime(1953, 12, 10);
            person["YearsInSpace"] = 12;

            var error = person.Validate();
            Console.WriteLine( error );
            Aver.IsTrue(error is CRUDFieldValidationException);
            Aver.AreEqual("LastName", ((CRUDFieldValidationException)error).FieldName);
        }

        [Run]
        public void Validate_Error_NullableIntRequired()
        {
            var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] ="Popov";
            person["DOB"] = new DateTime(1953, 12, 10);
           // person["YearsInSpace"] = 12;  NOT SET!!!

            var error = person.Validate();
            Console.WriteLine( error );
            Aver.IsTrue(error is CRUDFieldValidationException);
            Aver.AreEqual("YearsInSpace", ((CRUDFieldValidationException)error).FieldName);
        }

        [Run]
        public void Validate_Error_DecimalMinMax_1()
        {
            var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] ="Popov";
            person["DOB"] = new DateTime(1953, 12, 10);
            person["YearsInSpace"] = 45;
            person["Amount"] = -100;

            var error = person.Validate();
            Console.WriteLine( error );
            Aver.IsTrue(error is CRUDFieldValidationException);
            Aver.AreEqual("Amount", ((CRUDFieldValidationException)error).FieldName);
            Aver.IsTrue( error.Message.Contains("is below") );
        }

        [Run]
        public void Validate_Error_DecimalMinMax_2()
        {
            var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] ="Popov";
            person["DOB"] = new DateTime(1953, 12, 10);
            person["YearsInSpace"] = 45;
            person["Amount"] = 2000000;

            var error = person.Validate();
            Console.WriteLine( error );
            Aver.IsTrue(error is CRUDFieldValidationException);
            Aver.AreEqual("Amount", ((CRUDFieldValidationException)error).FieldName);
            Aver.IsTrue( error.Message.Contains("is above") );
        }


        [Run]
        public void Validate_Error_DateTimeMinMax_1()
        {
            var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] ="Popov";
            person["DOB"] = new DateTime(1899, 12, 10);
            person["YearsInSpace"] = 45;
            person["Amount"] = 100;


            var error = person.Validate();
            Console.WriteLine( error );
            Aver.IsTrue(error is CRUDFieldValidationException);
            Aver.AreEqual("DOB", ((CRUDFieldValidationException)error).FieldName);
            Aver.IsTrue( error.Message.Contains("is below") );
        }

        [Run]
        public void Validate_Error_DateTimeMinMax_DifferentTarget()
        {
            var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] ="Popov";
            person["DOB"] = new DateTime(1899, 12, 10);
            person["YearsInSpace"] = 45;
            person["Amount"] = 100;

            var error = person.Validate("sparta_system");
            Aver.IsNull (error);
        }

        [Run]
        public void Validate_Error_MaxLength()
        {
            var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] ="Popov";
            person["DOB"] = new DateTime(1981, 2, 12);
            person["YearsInSpace"] = 45;
            person["Amount"] = 100;
            person["Description"] = "0123456789012345678901234567890";

            var error = person.Validate();
            Console.WriteLine( error );
            Aver.IsTrue(error is CRUDFieldValidationException);
            Aver.AreEqual("Description", ((CRUDFieldValidationException)error).FieldName);
            Aver.IsTrue( error.Message.Contains("exceeds max length") );
        }


        [Run]
        public void Validate_Error_ValueList()
        {
             var person = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            person["ID"] = "POP1";
            person["FirstName"] = "Oleg";
            person["LastName"] ="Popov";
            person["DOB"] = new DateTime(1981, 2, 12);
            person["YearsInSpace"] = 45;
            person["Amount"] = 100;
            person["Description"] = "0123";
            person["Classification"] = "INVALID";

            var error = person.Validate();
            Console.WriteLine( error );
            Aver.IsTrue(error is CRUDFieldValidationException);
            Aver.AreEqual("Classification", ((CRUDFieldValidationException)error).FieldName);
            Aver.IsTrue( error.Message.Contains("not in list of permitted values") );

            person["Classification"] = "good";
            Aver.IsNull( person.Validate() );
            person["Classification"] = "bad";
            Aver.IsNull( person.Validate() );
            person["Classification"] = "ugly";
            Aver.IsNull( person.Validate() );

        }



        [Run]
        public void Equality()
        {
            var person1 = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            person1["ID"] = "POP1";
            person1["FirstName"] = "Oleg";
            person1["LastName"] = "Popov";
            person1["DOB"] = new DateTime(1981, 2, 12);
            person1["YearsInSpace"] = 45;
            person1["Amount"] = 100;
            person1["Description"]="Wanted to go to the moon";

            var person2 = new DynamicRow(Schema.GetForTypedRow(typeof(Person)));
            person2["ID"] = "POP1";
            person2["FirstName"] = "Egor";
            person2["LastName"] = "Pedorov";
            person2["DOB"] = new DateTime(1982, 5, 2);
            person2["YearsInSpace"] = 4;
            person2["Amount"] = 1000000;

            Aver.IsTrue( person1.Equals(person2) );

            person2["ID"] = "POP2";

            Aver.IsFalse(person1.Equals(person2) );

        }



    }
}
