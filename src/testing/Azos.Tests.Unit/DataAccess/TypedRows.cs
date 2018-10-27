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


namespace Azos.Tests.Unit.DataAccess
{
    [Runnable(TRUN.BASE, 3)]
    public class TypedRows
    {
        [Run]
        public void RowFieldValueEnumerator()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12
                                   };

            var cnt = person.Count();

            Aver.AreEqual( cnt, person.Schema.FieldCount);
            Aver.AreObjectsEqual( "Popov", person.ElementAt(2));
        }




        [Run]
        public void Validate_NoError()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12
                                   };
            var error = person.Validate();
            Aver.IsNull( error );
        }


        [Run]
        public void SetAndGetByName()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12
                                   };

            person["dEscRipTION"] = "moon"; //field names are case-insensitive

            Aver.AreObjectsEqual(person.ID, person["ID"]);
            Aver.AreObjectsEqual(person.FirstName, person["FirstName"]);
            Aver.AreObjectsEqual(person.LastName, person["LastName"]);
            Aver.AreObjectsEqual(person.DOB, person["DOB"]);
            Aver.AreObjectsEqual(person.YearsInSpace, person["YearsInSpace"]);
            Aver.AreObjectsEqual(person.Description, person["Description"]);
        }

        [Run]
        public void SetAndGetByIndex()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12
                                   };

            Aver.AreObjectsEqual(person.ID,               person[person.Schema["ID"].Order]);
            Aver.AreObjectsEqual(person.FirstName,        person[person.Schema["FirstName"].Order]);
            Aver.AreObjectsEqual(person.LastName,         person[person.Schema["LastName"].Order]);
            Aver.AreObjectsEqual(person.DOB,              person[person.Schema["DOB"].Order]);
            Aver.AreObjectsEqual(person.YearsWithCompany, person[person.Schema["YearsWithCompany"].Order]);
            Aver.AreObjectsEqual(person.YearsInSpace,     person[person.Schema["YearsInSpace"].Order]);

        }


        [Run]
        public void Validate_Error_StringRequired()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = null,
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12
                                   };
            var error = person.Validate();
            Console.WriteLine( error );
            Aver.IsTrue(error is CRUDFieldValidationException);
            Aver.AreEqual("LastName", ((CRUDFieldValidationException)error).FieldName);
        }

        [Run]
        public void Validate_Error_NullableIntRequired()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10)
                                   };
            var error = person.Validate();
            Console.WriteLine( error );
            Aver.IsTrue(error is CRUDFieldValidationException);
            Aver.AreEqual("YearsInSpace", ((CRUDFieldValidationException)error).FieldName);
        }

        [Run]
        public void Validate_Error_DecimalMinMax_1()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 45,
                                    Amount = -100
                                   };
            var error = person.Validate();
            Console.WriteLine( error );
            Aver.IsTrue(error is CRUDFieldValidationException);
            Aver.AreEqual("Amount", ((CRUDFieldValidationException)error).FieldName);
            Aver.IsTrue( error.Message.Contains("is below") );
        }

        [Run]
        public void Validate_Error_DecimalMinMax_2()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 45,
                                    Amount = 2000000
                                   };
            var error = person.Validate();
            Console.WriteLine( error );
            Aver.IsTrue(error is CRUDFieldValidationException);
            Aver.AreEqual("Amount", ((CRUDFieldValidationException)error).FieldName);
            Aver.IsTrue( error.Message.Contains("is above") );
        }


        [Run]
        public void Validate_Error_DateTimeMinMax_1()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1899, 12, 10),
                                    YearsInSpace = 45,
                                    Amount = 100
                                   };
            var error = person.Validate();
            Console.WriteLine( error );
            Aver.IsTrue(error is CRUDFieldValidationException);
            Aver.AreEqual("DOB", ((CRUDFieldValidationException)error).FieldName);
            Aver.IsTrue( error.Message.Contains("is below") );
        }

        [Run]
        public void Validate_Error_DateTimeMinMax_DifferentTarget()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1899, 12, 10),
                                    YearsInSpace = 45,
                                    Amount = 100
                                   };
            var error = person.Validate("sparta_system");
            Aver.IsNull (error);
        }

        [Run]
        public void Validate_Error_MaxLength()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1981, 2, 12),
                                    YearsInSpace = 45,
                                    Amount = 100,
                                    Description="0123456789012345678901234567890"
                                   };
            var error = person.Validate();
            Console.WriteLine( error );
            Aver.IsTrue(error is CRUDFieldValidationException);
            Aver.AreEqual("Description", ((CRUDFieldValidationException)error).FieldName);
            Aver.IsTrue( error.Message.Contains("exceeds max length") );
        }


        [Run]
        public void Validate_Error_ValueList()
        {
            var person = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1981, 2, 12),
                                    YearsInSpace = 45,
                                    Amount = 100,
                                    Description="012345",
                                    Classification = "INVALID"
                                   };
            var error = person.Validate();
            Console.WriteLine( error );
            Aver.IsTrue(error is CRUDFieldValidationException);
            Aver.AreEqual("Classification", ((CRUDFieldValidationException)error).FieldName);
            Aver.IsTrue( error.Message.Contains("not in list of permitted values") );

            person.Classification = "good";
            Aver.IsNull( person.Validate() );
            person.Classification = "bad";
            Aver.IsNull( person.Validate() );
            person.Classification = "ugly";
            Aver.IsNull( person.Validate() );

        }






        [Run]
        public void Equality()
        {
            var person1 = new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1981, 2, 12),
                                    YearsInSpace = 45,
                                    Amount = 100,
                                    Description="Wanted to go to the moon"
                                   };
            var person2 = new Person{
                                    ID = "POP1",
                                    FirstName = "Egor",
                                    LastName = "Pedorov",
                                    DOB = new DateTime(1982, 5, 2),
                                    YearsInSpace = 4,
                                    Amount = 1000000
                                   };
            Aver.IsTrue( person1.Equals(person2) );

            person2.ID = "POP2";

            Aver.IsFalse(person1.Equals(person2) );

        }



        [Run]
        public void Validate_PersonWithNesting_Error_ComplexFieldRequired()
        {
            var person = new PersonWithNesting{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 45,
                                    Amount = 100,
                                    History1  = new List<HistoryItem>(),
                                   // History2  = new HistoryItem[2]
                                   };
            var error = person.Validate();
            Console.WriteLine( error );
            Aver.IsTrue(error is CRUDFieldValidationException);
            Aver.AreEqual("History2", ((CRUDFieldValidationException)error).FieldName);
        }

        [Run]
        public void Validate_PersonWithNesting_Error_ComplexFieldCustomValidation()
        {
            var person = new PersonWithNesting{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 45,
                                    Amount = 100,
                                    LatestHistory = new HistoryItem{ ID = "111", StartDate = DateTime.Now, Description="Someone is an idiot!" },
                                    History1  = new List<HistoryItem>(),
                                    History2  = new HistoryItem[2]
                                   };
            var error = person.Validate();
            Console.WriteLine( error );
            Aver.IsTrue(error is CRUDFieldValidationException);
            Aver.AreEqual("Description", ((CRUDFieldValidationException)error).FieldName);
            Aver.IsTrue(error.Message.Contains("Chaplin"));
        }

        [Run]
        public void Validate_PersonWithNesting_Error_ComplexFieldSubFieldRequired()
        {
            var person = new PersonWithNesting{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 45,
                                    Amount = 100,
                                    LatestHistory = new HistoryItem{ ID = null, StartDate = DateTime.Now, Description="Chaplin is here" },
                                    History1  = new List<HistoryItem>(),
                                    History2  = new HistoryItem[2]
                                   };
            var error = person.Validate();
            Console.WriteLine( error );
            Aver.IsTrue(error is CRUDFieldValidationException);
            Aver.AreEqual("ID", ((CRUDFieldValidationException)error).FieldName);
        }




    }
}
