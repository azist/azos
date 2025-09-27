/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;

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

      var satr = schema.SchemaAttrs.First();
      Aver.AreEqual("Schema-level description is here", satr.Description);
      Aver.AreEqual(1, satr.Metadata.Of("a").ValueAsInt());
      Aver.AreEqual(2, satr.Metadata.Of("b").ValueAsInt());
      Aver.AreEqual(3, satr.Metadata["sub"].Of("c").ValueAsInt());

      var fatr = schema["Name"].Attrs.First();
      Aver.AreEqual("My long description", fatr.Description);
      Aver.AreEqual("Apple", fatr.ParseValueList()["A"].AsString());
      Aver.AreEqual("Book", fatr.ParseValueList()["B"].AsString());
      Aver.AreEqual(1, fatr.Metadata["a"].Of("v").ValueAsInt());
      Aver.AreEqual(2, fatr.Metadata["b"].Of("v").ValueAsInt());
      Aver.AreEqual(3, fatr.Metadata["c"].Of("v").ValueAsInt());
      Aver.AreEqual(-249, fatr.Metadata["c"]["another"].Of("v").ValueAsInt());



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

    [Run]
    public void Case_4()
    {
      var schema = Schema.GetForTypedDoc(typeof(custom.TestDataDocD));

      var satr = schema.SchemaAttrs.First();
      Aver.AreEqual("Schema-level description is here", satr.Description);
      Aver.AreEqual(1, satr.Metadata.Of("a").ValueAsInt());
      Aver.AreEqual(2, satr.Metadata.Of("b").ValueAsInt());
      Aver.AreEqual(3, satr.Metadata["sub"].Of("c").ValueAsInt());

      var fatr = schema["Name"].Attrs.First();
      Aver.AreEqual("My long description", fatr.Description);
      Aver.AreEqual("PineApple", fatr.ParseValueList()["A"].AsString());
      Aver.AreEqual("Book", fatr.ParseValueList()["B"].AsString());
      Aver.AreEqual(1, fatr.Metadata["a"].Of("v").ValueAsInt());
      Aver.AreEqual(2, fatr.Metadata["b"].Of("v").ValueAsInt());
      Aver.AreEqual(3, fatr.Metadata["c"].Of("v").ValueAsInt());
      Aver.AreEqual(-249, fatr.Metadata["c"]["another"].Of("v").ValueAsInt());



      Aver.AreEqual("Description of field", schema["Field"].Attrs.First().Description);
    }
  }


  [Schema(Description = "./", MetadataContent = "./")]
  public class TestDataDocA : TypedDoc
  {
    [Field(description: "./", valueList: "./", metadata: "./")]
    public string Name { get; set; }

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

  namespace custom//for resource path testing
  {
      [Schema(Description = "./", MetadataContent = "./")]
      public class TestDataDocD : TypedDoc
      {
        [Field(description: "./", valueList: "./", metadata: "./")]
        public string Name { get; set; }

        [Field(description: "./")]
        public int Field { get; set; }
      }
  }

}
