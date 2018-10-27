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

using Azos.DataAccess.CRUD;
using Azos.Scripting;
using Azos.Serialization.Slim;

namespace Azos.Tests.Unit.DataAccess
{
    [Runnable(TRUN.BASE, 3)]
    public class SchemaEQU
    {

        [Run]
        public void TypedRows()
        {
            var tbl = new Table(Schema.GetForTypedRow(typeof(Person)));


             tbl.Insert( new Person{
                                    ID = "POP1",
                                    FirstName = "Oleg",
                                    LastName = "Popov-1",
                                    DOB = new DateTime(1953, 12, 10),
                                    YearsInSpace = 12
                                   });

            var ser = new SlimSerializer();
            using(var ms = new MemoryStream())
            {
                ser.Serialize(ms, tbl);

                ms.Position = 0;

                var tbl2 = ser.Deserialize(ms) as Table;

                Aver.IsNotNull( tbl2 );
                Aver.IsFalse( object.ReferenceEquals(tbl ,tbl2) );

                Aver.IsFalse( object.ReferenceEquals(tbl.Schema ,tbl2.Schema) );


                Aver.IsTrue( tbl.Schema.IsEquivalentTo(tbl2.Schema));
            }
        }


        [Run]
        public void DynamicRows()
        {
            var tbl = new Table(Schema.GetForTypedRow(typeof(Person)));


                var row = new DynamicRow( tbl.Schema );

                row["ID"] = "POP1";
                row["FirstName"] = "Oleg";
                row["LastName"] = "Popov-1";
                row["DOB"] = new DateTime(1953, 12, 10);
                row["YearsInSpace"] = 12;

                tbl.Insert( row );


            var ser = new SlimSerializer();
            using(var ms = new MemoryStream())
            {
                ser.Serialize(ms, tbl);

                ms.Position = 0;

                var tbl2 = ser.Deserialize(ms) as Table;

                Aver.IsNotNull( tbl2 );
                Aver.IsFalse( object.ReferenceEquals(tbl ,tbl2) );

                Aver.IsFalse( object.ReferenceEquals(tbl.Schema ,tbl2.Schema) );

                Aver.IsTrue( tbl.Schema.IsEquivalentTo(tbl2.Schema));
            }
        }





    }
}
