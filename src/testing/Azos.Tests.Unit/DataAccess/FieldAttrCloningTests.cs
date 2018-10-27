/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Data;
using Azos.Scripting;
using static Azos.Aver.ThrowsAttribute;

namespace Azos.Tests.Unit.DataAccess
{
    [Runnable(TRUN.BASE, 3)]
    public class FieldAttrCloningTests
    {
        [Run]
        public void T1()
        {
            var schema1 = Schema.GetForTypedDoc<Row1>();
            Aver.AreEqual(3, schema1.FieldCount);

            Aver.IsTrue( schema1["First_Name"][null].Required );
            Aver.AreEqual(45, schema1["First_Name"][null].MaxLength);

            Aver.IsTrue( schema1["Last_Name"][null].Required );
            Aver.AreEqual(75, schema1["Last_Name"][null].MaxLength);

            Aver.IsFalse( schema1["Age"][null].Required );
            Aver.AreObjectsEqual(150, schema1["Age"][null].Max);




            var schema2 = Schema.GetForTypedDoc<Row2>();
            Aver.AreEqual(3, schema2.FieldCount);

            Aver.IsTrue( schema2["First_Name"][null].Required );
            Aver.AreEqual(47, schema2["First_Name"][null].MaxLength);

            Aver.IsTrue( schema2["Last_Name"][null].Required );
            Aver.AreEqual(75, schema2["Last_Name"][null].MaxLength);

            Aver.IsFalse( schema2["Age"][null].Required );
            Aver.AreObjectsEqual(150, schema2["Age"][null].Max);
        }

        [Run]
        [Aver.Throws(typeof(DataException), Message="only a single", MsgMatch=MatchType.Contains)]
        public void T2()
        {
            var schema1 = Schema.GetForTypedDoc<Row3>();
        }

        [Run]
        [Aver.Throws(typeof(DataException), Message="there is no field", MsgMatch=MatchType.Contains)]
        public void T3()
        {
            var schema1 = Schema.GetForTypedDoc<Row4>();
        }

        [Run]
        [Aver.Throws(typeof(DataException), Message="recursive field", MsgMatch=MatchType.Contains)]
        public void T4()
        {
            var schema1 = Schema.GetForTypedDoc<Row5>();
        }

        [Run]
        [Aver.Throws(typeof(DataException), Message="recursive field", MsgMatch=MatchType.Contains)]
        public void T5()
        {
            var schema1 = Schema.GetForTypedDoc<Row6>();
        }

        [Run]
        [Aver.Throws(typeof(DataException), Message="recursive field", MsgMatch=MatchType.Contains)]
        public void T6()
        {
            var schema1 = Schema.GetForTypedDoc<Row7>();
        }


        public class Row1 : TypedDoc
        {
          [Field(required: true, maxLength: 45)]
          public string First_Name{ get; set;}

          [Field(required: true, maxLength: 75)]
          public string Last_Name{ get; set;}

          [Field(required: false, max: 150)]
          public int? Age{ get; set;}
        }

        public class Row2 : TypedDoc
        {
          [Field(typeof(Row1), "First_Name", maxLength: 47)]
          public string First_Name{ get; set;}

          [Field(typeof(Row1))]
          public string Last_Name{ get; set;}

          [Field(typeof(Row1))]
          public int? Age{ get; set;}
        }

        public class Row3 : TypedDoc
        {
          [Field(typeof(Row1))]
          [Field] //cant have mnore than one
          public string Last_Name{ get; set;}
        }

        public class Row4 : TypedDoc
        {
          [Field(typeof(Row1))]
          public string DOESNOTEXIST{ get; set;}
        }

        public class Row5 : TypedDoc
        {
          [Field(required: true, maxLength: 45)]
          public string First_Name{ get; set;}

          [Field(required: true, maxLength: 75)]
          public string Last_Name{ get; set;}

          [Field(typeof(Row7))]
          public int? Age{ get; set;}
        }

        public class Row6 : TypedDoc
        {
          [Field(typeof(Row5), "First_Name", maxLength: 47)]
          public string First_Name{ get; set;}

          [Field(typeof(Row5))]
          public string Last_Name{ get; set;}

          [Field(typeof(Row5))]
          public int? Age{ get; set;}
        }

         public class Row7 : TypedDoc
        {
          [Field(typeof(Row6))]
          public int? Age{ get; set;}
        }

    }
}
