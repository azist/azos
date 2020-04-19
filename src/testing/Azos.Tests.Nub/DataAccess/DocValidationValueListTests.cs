/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Conf;
using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class DocValidationValueListTests
  {
    [Run]
    public void ParseValList_1()
    {
      var sut = FieldAttribute.ParseValueListString("a:apple, b: banana");
      Aver.IsNotNull(sut);
      Aver.IsFalse(sut.CaseSensitive);
      Aver.AreEqual(2, sut.Count);
      Aver.AreEqual("apple", sut["a"].AsString());
      Aver.AreEqual("banana", sut["b"].AsString());
      Aver.AreEqual("apple", sut["A"].AsString());
      Aver.AreEqual("banana", sut["B"].AsString());
    }

    [Run]
    public void ParseValList_2()
    {
      var sut = FieldAttribute.ParseValueListString("a:apple, b: banana", caseSensitiveKeys: true);
      Aver.IsNotNull(sut);
      Aver.IsTrue(sut.CaseSensitive);
      Aver.AreEqual(2, sut.Count);
      Aver.AreEqual("apple", sut["a"].AsString());
      Aver.AreEqual("banana", sut["b"].AsString());
      Aver.AreEqual(null, sut["A"].AsString());
      Aver.AreEqual(null, sut["B"].AsString());
    }

    [Run]
    public void ParseValList_3()
    {
      var sut = FieldAttribute.ParseValueListString("a:apple; b: banana, c: cherry");//notice different delimiters
      Aver.IsNotNull(sut);
      Aver.AreEqual(3, sut.Count);
      Aver.AreEqual("apple", sut["a"].AsString());
      Aver.AreEqual("banana", sut["b"].AsString());
      Aver.AreEqual("cherry", sut["C"].AsString());
    }

    [Run]
    public void ParseValList_4()
    {
      var sut = FieldAttribute.ParseValueListString(" a :apple  ;  \n  \r\r\r\r   b: banana   , c: Chicken   ");//notice different delimiters and spaces
      Aver.IsNotNull(sut);
      Aver.AreEqual(3, sut.Count);
      Aver.AreEqual("apple", sut["a"].AsString());
      Aver.AreEqual("banana", sut["b"].AsString());
      Aver.AreEqual("Chicken", sut["C"].AsString());
    }

    [Run]
    public void ParseValList_5()
    {
      var sut = FieldAttribute.ParseValueListString("1|01|911:fire;02|912:police;03:ambulance");//notice different delimiters and spaces
      Aver.IsNotNull(sut);
      Aver.AreEqual(6, sut.Count);
      Aver.AreEqual("fire", sut["1"].AsString());
      Aver.AreEqual("fire", sut["01"].AsString());
      Aver.AreEqual("fire", sut["911"].AsString());

      Aver.AreEqual("police", sut["02"].AsString());
      Aver.AreEqual("police", sut["912"].AsString());

      Aver.AreEqual("ambulance", sut["03"].AsString());
    }

    [Run]
    public void ParseValList_6()
    {
      var sut = FieldAttribute.ParseValueListString("1| 01 |   911 : fire; 02 | 912: police;03:ambulance");
      Aver.IsNotNull(sut);
      Aver.AreEqual(6, sut.Count);
      Aver.AreEqual("fire", sut["1"].AsString());
      Aver.AreEqual("fire", sut["01"].AsString());
      Aver.AreEqual("fire", sut["911"].AsString());

      Aver.AreEqual("police", sut["02"].AsString());
      Aver.AreEqual("police", sut["912"].AsString());

      Aver.AreEqual("ambulance", sut["03"].AsString());
    }

    [Run]
    public void ParseValList_7()
    {
      var sut = FieldAttribute.ParseValueListString("1|| ||   | | | |||| || 01 |   911 : fire, 02 | 912: police,03:ambulance");
      Aver.IsNotNull(sut);
      Aver.AreEqual(6, sut.Count);
      Aver.AreEqual("fire", sut["1"].AsString());
      Aver.AreEqual("fire", sut["01"].AsString());
      Aver.AreEqual("fire", sut["911"].AsString());

      Aver.AreEqual("police", sut["02"].AsString());
      Aver.AreEqual("police", sut["912"].AsString());

      Aver.AreEqual("ambulance", sut["03"].AsString());
    }

    [Run, Aver.Throws(typeof(DataException), Message = "duplicate key `1`")]
    public void ParseValList_8()
    {
      var sut = FieldAttribute.ParseValueListString("1:one;2:two;1:again");
    }

    [Run, Aver.Throws(typeof(DataException), Message = "duplicate key `01`")]
    public void ParseValList_9()
    {
      var sut = FieldAttribute.ParseValueListString("1|01|001:one;2|02|01:two;3:three");
    }

    [Run]
    public void TypedDoc_1()
    {
      var schema = Schema.GetForTypedDoc<Doc1>();
      var a = schema["Field1"][null];
      Aver.IsTrue(a.HasValueList);
      var vl1 = a.ParseValueList();
      var vl2 = a.ParseValueList();
      var vl3 = a.ParseValueList(true);
      var vl4 = a.ParseValueList(true);
      Aver.AreSameRef(vl1, vl2);
      Aver.AreNotSameRef(vl1, vl3);
      Aver.AreSameRef(vl3, vl4);
      Aver.AreEqual(4, vl1.Count);

      Aver.AreEqual("apple", vl1["a"].AsString());
      Aver.AreEqual("banana", vl1["b"].AsString());
      Aver.AreEqual("cherry", vl1["c"].AsString());
      Aver.AreEqual("dynamo", vl1["d"].AsString());
      Aver.AreEqual("dynamo", vl1["D"].AsString());//case insensitive
    }

    [Run]
    public void TypedDoc_2()
    {
      var doc = new Doc1{ };      //nothing is required
      Aver.IsNull(doc.Validate());
    }

    [Run]
    public void TypedDoc_3()
    {
      var doc = new Doc1 { Field1 = "a"};//In list
      Aver.IsNull(doc.Validate());
      doc.Field1 = "d";
      Aver.IsNull(doc.Validate());
    }

    [Run]
    public void TypedDoc_4()
    {
      var doc = new Doc1 { Field1 = "Z" };//NOT In list
      var ve = doc.Validate() as FieldValidationException;
      Aver.IsNotNull(ve);
      Conout.WriteLine(ve.ToMessageWithType());
      Aver.AreEqual("Field1", ve.FieldName);
      doc.Field1 = "c";
      Aver.IsNull(doc.Validate());
    }

    [Run]
    public void TypedDoc_5()
    {
      var doc = new Doc1 { Field3 = "a" };//In list
      Aver.IsNull(doc.Validate());
      doc.Field3 = "b";
      Aver.IsNull(doc.Validate());
      doc.Field3 = "n";
      Aver.IsNull(doc.Validate());
    }

    [Run]
    public void TypedDoc_6()
    {
      var doc = new Doc1 { Field3 = "I can not be here" };//NOT In list
      var ve = doc.Validate() as FieldValidationException;
      Aver.IsNotNull(ve);
      Conout.WriteLine(ve.ToMessageWithType());
      Aver.AreEqual("Field3", ve.FieldName);
      doc.Field3 = "n";//'Nancy' obtained from dynamic imperative call
      Aver.IsNull(doc.Validate());
    }

    public class Doc1 : TypedDoc
    {
      [Field(valueList: "a:apple,b:banana;c:cherry,d:dynamo")]
      public string Field1{  get ; set; }

      [Field(valueList: "(0):apple; b(anana): Banana Fruit; c(herry): cherry fruit; d:dynamo")]
      public string Field2 { get; set; }


      [Field(description: "this uses custom validation using imperative code")]
      public string Field3{  get; set;}

      public override JsonDataMap GetDynamicFieldValueList(Schema.FieldDef fdef, string targetName, string isoLang)
      {
        if (fdef.Name==nameof(Field3)) return new JsonDataMap{ {"a", "Adam"}, {"b", "Boris"}, { "n", "Nancy" } };
        return base.GetDynamicFieldValueList(fdef, targetName, isoLang);
      }
    }

  }
}
