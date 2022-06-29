/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class ConfigVectorTests
  {
    [Run]
    public void Test_Implicit()
    {
      ConfigVector v = "{r: {a:1, b:2}}";
      Aver.AreEqual(1, v.Node.Of("a").ValueAsInt());
      Aver.AreEqual(2, v.Node.Of("b").ValueAsInt());
    }


    public class Tezt : TypedDoc
    {
      [Field(required: true)]
      public ConfigVector C1{ get; set;}

      [Field(maxLength: 32)]
      public ConfigVector C2 { get; set; }
    }


    [Run]
    public void Test_Roundtrip()
    {
      var d = new Tezt
      {
        C1 = "{r: {a:1, b:2}}",
        C2 = null
      };

      Aver.IsNull(d.Validate());

      var json = JsonWriter.Write(d, JsonWritingOptions.PrettyPrintRowsAsMap);

      json.See();

      var got = JsonReader.ToDoc<Tezt>(json);

      Aver.AreEqual(1, got.C1.Node.Of("a").ValueAsInt());
      Aver.AreEqual(2, got.C1.Node.Of("b").ValueAsInt());
    }

    [Run]
    [Aver.Throws(typeof(ConfigException))]
    public void Test_Malformed()
    {
      var json = "{C1: 'crap'}";

      var got = JsonReader.ToDoc<Tezt>(json);

      Aver.AreEqual("crap", got.C1.Content);

      var ve = got.Validate() as FieldValidationException;
      Aver.IsNotNull(ve);
      Aver.AreEqual("C1", ve.FieldName);
      Aver.IsTrue(ve.Message.Contains("Invalid"));

      got.C1.Node.Of("a");
    }

    [Run]
    public void Test_Long()
    {
      var json = "{C1: '{a: {}}', C2: '{r: {m: \"very long message which is longer than 32 characters\"}}'}";

      var got = JsonReader.ToDoc<Tezt>(json);

      Aver.AreEqual("very long message which is longer than 32 characters", got.C2.Node.ValOf("m"));

      var ve = got.Validate() as FieldValidationException;
      Aver.IsNotNull(ve);
      Aver.AreEqual("C2", ve.FieldName);
      Aver.IsTrue(ve.Message.Contains("exceeds"));
      ve.See();
    }

    [Run]
    public void Test_FullCycle()
    {
      var d = new Tezt
      {
        C1 = "{r: {a:1, b:2}}",
        C2 = "{r: {a:-1, b:-2}}"
      };

      var json = JsonWriter.Write(d, JsonWritingOptions.PrettyPrintRowsAsMap);

      json.See();

      var got = JsonReader.ToDoc<Tezt>(json);

      var n1 = got.C1.Node;
      Aver.AreEqual(1, got.C1.Node.Of("a").ValueAsInt());
      Aver.AreEqual(2, got.C1.Node.Of("b").ValueAsInt());
      Aver.AreSameRef(n1, got.C1.Node);

      got.C1.Content = "{r: {a:10, b:20}}";
      var n2 = got.C1.Node;
      Aver.AreNotSameRef(n1, n2);
      Aver.AreSameRef(n2, got.C1.Node);

      got.C1.Node = "r{z=900}".AsLaconicConfig();
      var n3 = got.C1.Node;
      Aver.AreNotSameRef(n2, n3);
      Aver.AreEqual(900, got.C1.Node.Of("z").ValueAsInt());

      Aver.AreEqual(-1, got.C2.Node.Of("a").ValueAsInt());
      Aver.AreEqual(-2, got.C2.Node.Of("b").ValueAsInt());

      got.See();
    }


    [Run]
    public void Test_FullCycle_Laconic()
    {
      var d = new Tezt
      {
        C1 = "r {a=1 b=2}",
        C2 = "r {a=-1 b=-2}"
      };

      var json = JsonWriter.Write(d, JsonWritingOptions.PrettyPrintRowsAsMap);

      json.See();

      var got = JsonReader.ToDoc<Tezt>(json);

      var n1 = got.C1.Node;
      Aver.AreEqual(1, got.C1.Node.Of("a").ValueAsInt());
      Aver.AreEqual(2, got.C1.Node.Of("b").ValueAsInt());
      Aver.AreSameRef(n1, got.C1.Node);

      got.C1.Content = "{r: {a:10, b:20}}";
      var n2 = got.C1.Node;
      Aver.AreNotSameRef(n1, n2);
      Aver.AreSameRef(n2, got.C1.Node);

      got.C1.Node = "r{z=900}".AsLaconicConfig();
      var n3 = got.C1.Node;
      Aver.AreNotSameRef(n2, n3);
      Aver.AreEqual(900, got.C1.Node.Of("z").ValueAsInt());

      Aver.AreEqual(-1, got.C2.Node.Of("a").ValueAsInt());
      Aver.AreEqual(-2, got.C2.Node.Of("b").ValueAsInt());

      got.See();
    }

  }
}
