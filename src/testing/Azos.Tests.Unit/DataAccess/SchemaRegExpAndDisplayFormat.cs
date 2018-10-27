/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Azos.Scripting;

using Azos.DataAccess.CRUD;


namespace Azos.Tests.Unit.DataAccess
{
    [Runnable(TRUN.BASE, 3)]
    public class SchemaRegExpAndDisplayFormat
    {
        [Run]
        public void ValidateRegexp()
        {
            var row = new MyCar
            {
               Code = "adsd"
            };

            var ve = row.Validate();
            Aver.IsNotNull(ve);
            Aver.IsTrue( ve.Message.Contains("Allowed characters: A-Z,0-9,-"));
            Console.WriteLine( ve.ToMessageWithType());

            row.Code = "AZ-90";
            ve = row.Validate();
            Aver.IsNull(ve);
        }


        [Run]
        public void DisplayFormat()
        {
            var row = new MyCar
            {
               Code = "ABZ-01", 
               Milage = 150000
            };

            Aver.AreEqual("150000", row["Milage"].ToString());
            Aver.AreEqual("Milage: 150,000 miles", row.GetDisplayFieldValue("Milage"));
        }

        [Run]
        public void FieldValueDescription()
        {
            var row = new MyCar();

            row.Sex = "F";
            Aver.AreEqual("Female", row.GetFieldValueDescription("Sex"));
            
            row.Sex = "M";
            Aver.AreEqual("Male", row.GetFieldValueDescription("Sex"));
            
            row.Sex = "U";
            Aver.AreEqual("Unknown", row.GetFieldValueDescription("Sex"));
        }

        [Run]
        public void SchemaEquivalence()
        {
            Aver.IsTrue( Schema.GetForTypedRow(typeof(MyCar)).IsEquivalentTo(Schema.GetForTypedRow(typeof(MyCar2)), false ));
            Aver.IsFalse( Schema.GetForTypedRow(typeof(MyCar)).IsEquivalentTo(Schema.GetForTypedRow(typeof(MyCarDiffOrder)), false ));

            Aver.IsFalse( Schema.GetForTypedRow(typeof(MyCar)).IsEquivalentTo(Schema.GetForTypedRow(typeof(MyCar3)), false ));
            Aver.IsFalse( Schema.GetForTypedRow(typeof(MyCar)).IsEquivalentTo(Schema.GetForTypedRow(typeof(MyCar4)), false ));
            Aver.IsFalse( Schema.GetForTypedRow(typeof(MyCar)).IsEquivalentTo(Schema.GetForTypedRow(typeof(MyCar5)), false ));
        }
    }


    public class MyCar : TypedRow
    {
      
      [Field(formatRegExp: @"^[A-Z0-9\-]+$",
             formatDescr: @"Allowed characters: A-Z,0-9,-")]
      public string Code{ get; set;}

      [Field(displayFormat: "Milage: {0:n0} miles")]
      public int Milage{ get; set;}

      [Field(valueList:"M: Male, F: Female, U: Unknown")]
      public string Sex{ get; set;}
    }

    public class MyCar2 : TypedRow
    {
      
      [Field(formatRegExp: @"^[A-Z0-9\-]+$",
             formatDescr: @"Allowed characters: A-Z,0-9,-")]
      public string Code{ get; set;}

      [Field(displayFormat: "Milage: {0:n0} miles")]
      public int Milage{ get; set;}

      [Field(valueList:"M: Male, F: Female, U: Unknown")]
      public string Sex{ get; set;}
    }

    public class MyCarDiffOrder : TypedRow
    {
      
      [Field(formatRegExp: @"^[A-Z0-9\-]+$",
             formatDescr: @"Allowed characters: A-Z,0-9,-")]
      public string Code{ get; set;}

      [Field(displayFormat: "Milage: {0:n0} miles")]
      public int Milage{ get; set;}

      [Field(valueList:"M: Male, U: Unknown, F: Female")]
      public string Sex{ get; set;}
    }

    public class MyCar3 : TypedRow
    {
      
      [Field(formatRegExp: @"^[A-Z0-8\-]+$",  //difference in regexp
             formatDescr: @"Allowed characters: A-Z,0-9,-")]
      public string Code{ get; set;}

      [Field(displayFormat: "Milage: {0:n0} miles")]
      public int Milage{ get; set;}
    }

    public class MyCar4 : TypedRow
    {
      
      [Field(formatRegExp: @"^[A-Z0-9\-]+$",
             formatDescr: @"Allowed DIFFERENT characters: A-Z,0-9,-")]
      public string Code{ get; set;}

      [Field(displayFormat: "Milage: {0:n0} miles")]
      public int Milage{ get; set;}
    }


    public class MyCar5 : TypedRow
    {
      
      [Field(formatRegExp: @"^[A-Z0-9\-]+$",
             formatDescr: @"Allowed characters: A-Z,0-9,-")]
      public string Code{ get; set;}

      [Field(displayFormat: "Milage: {0:n0} kilometers")]
      public int Milage{ get; set;}
    }


}
