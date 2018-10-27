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


using Azos.Data;


namespace Azos.Tests.Unit.DataAccess
{
    [Runnable(TRUN.BASE, 3)]
    public class FieldAttrPrototyping
    {
        [Run]
        public void FieldProperties()
        {
           var schema = Schema.GetForTypedDoc(typeof(RowB));
           Aver.AreEqual(7, schema["FirstName"]["A"].MinLength);
           Aver.AreEqual(10, schema["FirstName"]["A"].MaxLength);
           Aver.AreEqual(true, schema["FirstName"]["A"].Required);
           Aver.AreEqual(true, schema["FirstName"]["A"].Key);

           Aver.AreEqual(8, schema["FirstName"]["B"].MinLength);
           Aver.AreEqual(15, schema["FirstName"]["B"].MaxLength);
           Aver.AreEqual(true, schema["FirstName"]["B"].Required);
           Aver.AreEqual(false, schema["FirstName"]["B"].Key);
        }

        [Run]
        public void MetadataProperties()
        {
           var schema = Schema.GetForTypedDoc(typeof(RowB));
           Aver.IsTrue( schema["FirstName"]["A"].Metadata.AttrByName("ABC").ValueAsBool() );
           Aver.IsTrue( schema["FirstName"]["A"].Metadata.AttrByName("DEF").ValueAsBool() );
           Aver.IsTrue( schema["FirstName"]["A"].Metadata["Another"].Exists );
           Aver.IsTrue( schema["FirstName"]["B"].Metadata.AttrByName("ABC").ValueAsBool() );
           Aver.IsFalse( schema["FirstName"]["B"].Metadata.AttrByName("DEF").ValueAsBool() );

           Aver.IsTrue( schema["LastName"]["A"].Metadata.AttrByName("flag").ValueAsBool() );
           Aver.IsFalse( schema["LastName"]["B"].Metadata.AttrByName("flag").ValueAsBool() );
        }


        [Run]
        [Aver.Throws(typeof(DataException), Message="recursive field definition", MsgMatch= Aver.ThrowsAttribute.MatchType.Contains)]
        public void Recursive()
        {
           var schema = Schema.GetForTypedDoc(typeof(RowC));
        }



    }


              public class RowA : TypedDoc
              {
                [Field(targetName: "A", storeFlag: StoreFlag.LoadAndStore, key: true, minLength: 5, maxLength: 10, required: true, metadata: "abc=true")]
                [Field(targetName: "B", storeFlag: StoreFlag.OnlyLoad, key: true, minLength: 5, maxLength: 15, required: true, metadata: "abc=false")]
                public string FirstName{get; set;}

                [Field(targetName: "A", storeFlag: StoreFlag.OnlyStore, minLength: 15, maxLength: 25, required: true, metadata: "flag=true")]
                [Field(targetName: "B", storeFlag: StoreFlag.LoadAndStore, minLength: 15, maxLength: 20, required: false, metadata: "flag=false")]
                public string LastName{get; set;}
              }

              public class RowB : TypedDoc
              {
                [Field(protoType: typeof(RowA), protoFieldName: "FirstName", targetName: "A", minLength: 7, metadata: "def=true another{}")]
                [Field(protoType: typeof(RowA), protoFieldName: "FirstName", targetName: "B", key: false, minLength: 8, metadata: "abc=true def=false")]
                public string FirstName{get; set;}

                [Field(protoType: typeof(RowA), protoFieldName: "LastName", targetName: "A")]
                [Field(protoType: typeof(RowA), protoFieldName: "LastName", targetName: "B")]
                public string LastName{get; set;}
              }

              public class RowC : TypedDoc
              {
                [Field(protoType: typeof(RowE), protoFieldName: "FirstName")]
                public string FirstName{get; set;}
              }

              public class RowD : TypedDoc
              {
                [Field(protoType: typeof(RowC), protoFieldName: "FirstName")]
                public string FirstName{get; set;}
              }

              public class RowE : TypedDoc
              {
                [Field(protoType: typeof(RowD), protoFieldName: "FirstName")]
                public string FirstName{get; set;}
              }





}
