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


namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class FieldAttrResourceReferences
  {
    [Run]
    public void Case_1()
    {
      var schema = Schema.GetForTypedDoc(typeof(TestDataDocA));

      Aver.AreEqual("Schema-level description is here", schema.SchemaAttrs.First().Description);

      Aver.AreEqual("My long description", schema["Name"].Attrs.First().Description);
      Aver.AreEqual("Apple", schema["Name"].Attrs.First().ParseValueList()["A"].AsString());
      Aver.AreEqual("Book", schema["Name"].Attrs.First().ParseValueList()["B"].AsString());

      Aver.AreEqual("Description of field", schema["Field"].Attrs.First().Description);
    }

    [Run]
    public void Case_2()
    {
      var schema = Schema.GetForTypedDoc(typeof(TestDataDocB));

      Aver.AreEqual("My longest description", schema["Name"].Attrs.First().Description);
      Aver.AreEqual("Adam", schema["Name"].Attrs.First().ParseValueList()["A"].AsString());
      Aver.AreEqual("Buba", schema["Name"].Attrs.First().ParseValueList()["B"].AsString());

    }

    [Run]
    public void Case_3()
    {
      var schema = Schema.GetForTypedDoc(typeof(TestDataDocC));

      Aver.AreEqual("My long description", schema["Name"].Attrs.First().Description);
      Aver.AreEqual("Apple", schema["Name"].Attrs.First().ParseValueList()["A"].AsString());
      Aver.AreEqual("Book", schema["Name"].Attrs.First().ParseValueList()["B"].AsString());

    }

  }


  [Schema(Description = "./")]
  public class TestDataDocA : TypedDoc
  {
    [Field(description: "./", valueList: "./")]
    public string Name{ get; set; }

    [Field(description: "./")]
    public int Field { get; set; }
  }

  public class TestDataDocB : TypedDoc
  {
    [Field(description: "./custom.aaa", valueList: "./custom.aaa")]
    public string Name { get; set; }
  }

  public class TestDataDocC : TypedDoc
  {
    [Field(typeof(TestDataDocA))]
    public string Name { get; set; }
  }

}
