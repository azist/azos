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

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class FieldAttrTargetInheritTests
  {

    public class BasicDoc : TypedDoc
    {
      [Field(description: "Common description")]
      [Field("L1", null, Required = true)]
      [Field("L2", "L1", MaxLength = 190)]
      [Field("L3", "L2", MinLength = 1, Description = "Palm tree 3")]
      [Field("ALT", null, Description = "Description override")]//same as ANY_TARGET
      public string Data { get; set; }
    }

    [Run]
    public void Basic()
    {
      var schema = Schema.GetForTypedDoc<BasicDoc>();
      Aver.AreEqual(1, schema.FieldCount);
      var def = schema["Data"];
      Aver.IsNotNull(def);

      def.Attrs.See();

      Aver.AreEqual(5, def.Attrs.Count());
      var atrANY = def[null];
      Aver.AreEqual(FieldAttribute.ANY_TARGET, atrANY.TargetName);
      Aver.IsNull(atrANY.DeriveFromTargetName);
      Aver.AreEqual("Common description", atrANY.Description);
      Aver.IsFalse(atrANY.Required);


      var atrL1 = def["L1"];
      Aver.AreEqual("L1", atrL1.TargetName);
      Aver.AreEqual(FieldAttribute.ANY_TARGET, atrL1.DeriveFromTargetName);
      Aver.AreEqual("Common description", atrL1.Description);
      Aver.IsTrue(atrL1.Required);
      Aver.AreEqual(0, atrL1.MaxLength);
      Aver.AreEqual(0, atrL1.MinLength);

      var atrL2 = def["L2"];
      Aver.AreEqual("L2", atrL2.TargetName);
      Aver.AreEqual("L1", atrL2.DeriveFromTargetName);
      Aver.AreEqual("Common description", atrL2.Description);
      Aver.IsTrue(atrL2.Required);
      Aver.AreEqual(190, atrL2.MaxLength);
      Aver.AreEqual(0, atrL2.MinLength);

      var atrL3 = def["L3"];
      Aver.AreEqual("L3", atrL3.TargetName);
      Aver.AreEqual("L2", atrL3.DeriveFromTargetName);
      Aver.AreEqual("Palm tree 3", atrL3.Description);
      Aver.IsTrue(atrL3.Required);
      Aver.AreEqual(190, atrL3.MaxLength);
      Aver.AreEqual(1, atrL3.MinLength);

      var atrALT = def["ALT"];
      Aver.AreEqual("ALT", atrALT.TargetName);
      Aver.AreEqual(FieldAttribute.ANY_TARGET, atrALT.DeriveFromTargetName);
      Aver.AreEqual("Description override", atrALT.Description);
      Aver.IsFalse(atrALT.Required);
      Aver.AreEqual(0, atrALT.MaxLength);
      Aver.AreEqual(0, atrALT.MinLength);

    }

    public class ValListDoc : TypedDoc
    {
      [Field(description: "Common description", valueList:"a:apple, b:banana, z|zuk:zukini")]
      [Field("L1", null, ValueList = @"
        a:  advaita,
        c:  cherry  ,
        z: #del# ")]
      [Field("L2", "L1", ValueList = "z: zoom", Default = "Uncle Toad", Description = "luna")]
      public string Data1 { get; set; }

      [Field(description: "About2", valueList: "dct: doctor; car: car")]
      [Field("ORA", null, ValueList = "bar: bar; dct: #del#")]
      [Field("JVC", "*", Description = "JVC Video", ValueList = "bar: mar; dct: self")]
      public string Data2 { get; set; }
    }

    [Run]
    public void ValueLists()
    {
      var schema = Schema.GetForTypedDoc<ValListDoc>();
      Aver.AreEqual(2, schema.FieldCount);
      var def1 = schema["Data1"];
      Aver.IsNotNull(def1);

      var def2 = schema["Data2"];
      Aver.IsNotNull(def2);

      Aver.AreEqual("Common description", def1[null].Description);
      Aver.AreEqual(4, def1[null].ParseValueList().Count);
      Aver.AreEqual("apple", def1[null].ParseValueList()["a"].AsString());
      Aver.AreEqual("banana", def1[null].ParseValueList()["b"].AsString());
      Aver.AreEqual("zukini", def1[null].ParseValueList()["z"].AsString());
      Aver.AreEqual("zukini", def1[null].ParseValueList()["zuk"].AsString());
      Aver.IsNull(def1[null].Default);

      Aver.AreEqual("Common description", def1["L1"].Description);
      Aver.AreEqual(4, def1["L1"].ParseValueList().Count);
      Aver.AreEqual("advaita", def1["L1"].ParseValueList()["a"].AsString());
      Aver.AreEqual("banana", def1["L1"].ParseValueList()["b"].AsString());
      Aver.AreEqual("cherry", def1["L1"].ParseValueList()["c"].AsString());
      Aver.AreEqual("zukini", def1["L1"].ParseValueList()["zuk"].AsString());
      Aver.IsNull(def1["L1"].Default);

      Aver.AreEqual("luna", def1["L2"].Description);
      Aver.AreEqual(5, def1["L2"].ParseValueList().Count);
      Aver.AreEqual("advaita", def1["L2"].ParseValueList()["a"].AsString());
      Aver.AreEqual("banana", def1["L2"].ParseValueList()["b"].AsString());
      Aver.AreEqual("cherry", def1["L2"].ParseValueList()["c"].AsString());
      Aver.AreEqual("zoom", def1["L2"].ParseValueList()["z"].AsString());
      Aver.AreEqual("zukini", def1["L2"].ParseValueList()["zuk"].AsString());
      Aver.AreEqual("Uncle Toad", def1["L2"].Default as string);

      Aver.AreEqual("About2", def2[null].Description);
      Aver.AreEqual(2,        def2[null].ParseValueList().Count);
      Aver.AreEqual("doctor", def2[null].ParseValueList()["dct"].AsString());
      Aver.AreEqual("car",    def2[null].ParseValueList()["car"].AsString());

      Aver.AreEqual("About2", def2["ORA"].Description);
      Aver.AreEqual(2,        def2["ORA"].ParseValueList().Count);
      Aver.AreEqual("car",    def2["ORA"].ParseValueList()["car"].AsString());
      Aver.AreEqual("bar",    def2["ORA"].ParseValueList()["bar"].AsString());

      Aver.AreEqual("JVC Video", def2["JVC"].Description);
      Aver.AreEqual(3,           def2["JVC"].ParseValueList().Count);
      Aver.AreEqual("car",       def2["JVC"].ParseValueList()["car"].AsString());
      Aver.AreEqual("mar",       def2["JVC"].ParseValueList()["bar"].AsString());
      Aver.AreEqual("self",      def2["JVC"].ParseValueList()["dct"].AsString());
    }

    public class MetadataDoc : TypedDoc
    {
      [Field(description: "Common description", metadata: "a=1 b=2 sub{ z=100 } ")]
      [Field("L1", null, MetadataContent = "c=-2 b=18", Description = "abcd")]
      public string Data { get; set; }
    }

    [Run]
    public void MetadataMerge()
    {
      var schema = Schema.GetForTypedDoc<MetadataDoc>();
      Aver.AreEqual(1, schema.FieldCount);
      var def = schema["Data"];
      Aver.IsNotNull(def);

      def[null].Metadata.See();

      Aver.AreEqual("Common description", def[null].Description);
      Aver.AreEqual(2, def[null].Metadata.AttrCount);
      Aver.AreEqual(1, def[null].Metadata.Navigate("$a").ValueAsInt());
      Aver.AreEqual(2, def[null].Metadata.Navigate("$b").ValueAsInt());
      Aver.AreEqual(100, def[null].Metadata.Navigate("sub/$z").ValueAsInt());

      Aver.AreEqual("abcd", def["L1"].Description);
      Aver.AreEqual(3, def["L1"].Metadata.AttrCount);
      Aver.AreEqual(1, def["L1"].Metadata.Navigate("$a").ValueAsInt());
      Aver.AreEqual(18, def["L1"].Metadata.Navigate("$b").ValueAsInt());
      Aver.AreEqual(-2, def["L1"].Metadata.Navigate("$c").ValueAsInt());
      Aver.AreEqual(100, def["L1"].Metadata.Navigate("sub/$z").ValueAsInt());
    }


    public class Bad_RecurseDoc : TypedDoc
    {
      [Field(description: "Common description", metadata: "a=1 b=2 sub{ z=100 }")]
      [Field("L1", "L3", Description = "cycle!!!")]
      [Field("L2", "L1", Description ="in the middle")]
      [Field("L3", "L2", Description="cycle")]
      public string Data { get; set; }
    }

    public class Bad_RefNotExistsDoc : TypedDoc
    {
      [Field(description: "Common description", metadata: "a=1 b=2 sub{ z=100 }")]
      [Field("L1", "I dont exist", Description = "cycle!!!")]
      public string Data { get; set; }
    }


    [Run]
    public void BadRecursion()
    {
      try
      {
        var schema = Schema.GetForTypedDoc<Bad_RecurseDoc>();
      }
      catch(Exception error)
      {
        Conout.WriteLine("Expected and got: "+error.ToMessageWithType());
        Aver.IsNotNull(error.InnerException);
        Aver.IsTrue(error.InnerException.Message.Contains("Cyclical"));
        return;
      }
      Aver.Fail("Did not get expected exception");
    }

    [Run]
    public void BadRefNotExists()
    {
      try
      {
        var schema = Schema.GetForTypedDoc<Bad_RefNotExistsDoc>();
      }
      catch (Exception error)
      {
        Conout.WriteLine("Expected and got: " + error.ToMessageWithType());
        Aver.IsNotNull(error.InnerException);
        Aver.IsTrue(error.InnerException.Message.Contains("no matching"));
        return;
      }
      Aver.Fail("Did not get expected exception");
    }


  }
}
