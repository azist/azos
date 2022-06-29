/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.ScriptingAndTesting
{
  [Runnable]
  public class DocLogicalComparerBasicTests
  {
    [Run]
    public void TwoEmpty()
    {
      var comparer = new DocLogicalComparer();

      var d1 = new Doc1() {};
      var d2 = new Doc1(){};

      var diff = comparer.Compare(d1, d2);

      diff.See();
      Aver.IsTrue(diff.AreSame);
      Aver.IsFalse(diff.AreDifferent);
    }

    [Run]
    public void OneField_d1_Different()
    {
      var comparer = new DocLogicalComparer();

      var d1 = new Doc1() { S1 = "aaa" };
      var d2 = new Doc1() { };

      var diff = comparer.Compare(d1, d2);

      diff.See();
      Aver.IsFalse(diff.AreSame);
      Aver.IsTrue(diff.AreDifferent);
    }

    [Run]
    public void OneField_d2_Different()
    {
      var comparer = new DocLogicalComparer();

      var d1 = new Doc1() {  };
      var d2 = new Doc1() { S1 = "abc"};

      var diff = comparer.Compare(d1, d2);

      diff.See();
      Aver.IsFalse(diff.AreSame);
      Aver.IsTrue(diff.AreDifferent);
    }

    [Run]
    public void OneField_d1d2_Different()
    {
      var comparer = new DocLogicalComparer();

      var d1 = new Doc1() { S1 = "in d1"};
      var d2 = new Doc1() { S1 = "this is in d2" };

      var diff = comparer.Compare(d1, d2);

      diff.See();
      Aver.IsFalse(diff.AreSame);
      Aver.IsTrue(diff.AreDifferent);
    }

    [Run]
    public void FullCycle_doc1()
    {
      var comparer = new DocLogicalComparer();

      var d1 = new Doc1() { S1 = "in d1" };

      var json = d1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

      var d2 = JsonReader.ToDoc<Doc1>(json);

      d1.See("d1");
      json.See("JSON");
      d2.See("d2");

      var diff = comparer.Compare(d1, d2);

      diff.See();
      Aver.IsTrue(diff.AreSame);
    }

    [Run]
    public void FullCycle_doc2()
    {
      var comparer = new DocLogicalComparer();

      var d1 = new Doc2() { S1 = "in d1", B1 = true, I1 = 1234, DT1 =new DateTime(1980, 09, 15, 0,0,0,DateTimeKind.Utc), NI1 = null, NL1 = 9_000_000_000L, S2 = null, NDT1 = null };

      var json = d1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

      var d2 = JsonReader.ToDoc<Doc2>(json);

      json.See("JSON");

      var diff = comparer.Compare(d1, d2);

      diff.See();
      Aver.IsTrue(diff.AreSame);
    }

    [Run]
    public void FullCycle_doc3()
    {
      var comparer = new DocLogicalComparer();

      var d1 = new Doc3
      {
        D1 = new Doc1{ S1 = "asdf"},
        D2 = new Doc2() { S1 = "in d1", B1 = true, I1 = 1234, DT1 = new DateTime(1980, 09, 15, 0, 0, 0, DateTimeKind.Utc), NI1 = null, NL1 = 9_000_000_000L, S2 = null, NDT1 = null }
      };

      var json = d1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

      var d2 = JsonReader.ToDoc<Doc3>(json);

      json.See("JSON");

      var diff = comparer.Compare(d1, d2);

      diff.See();
      Aver.IsTrue(diff.AreSame);
    }

    [Run]
    public void FullCycle_doc4()
    {
      var comparer = new DocLogicalComparer();

      var d1 = new Doc4
      {
        COL1 = new[]{ null, null, new Doc1 { S1 = "asdf" }, new Doc1 { S1 = "forgfoot" }, new Doc1 { S1 = "eat borsch!" } },
        COL2 = new[]{ new Doc2() { S1 = "in d1", B1 = true, I1 = 1234, DT1 = new DateTime(1980, 09, 15, 0, 0, 0, DateTimeKind.Utc), NI1 = null, NL1 = 9_000_000_000L, S2 = null, NDT1 = null }}
      };

      var json = d1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

      var d2 = JsonReader.ToDoc<Doc4>(json);

      json.WriteLine();

      var diff = comparer.Compare(d1, d2);

      diff.See();
      Aver.IsTrue(diff.AreSame);
    }

    [Run]
    public void FullCycle_doc5()
    {
      var comparer = new DocLogicalComparer();

      var d1 = new Doc5
      {
        COL1 = new List<Doc1> { null, null, new Doc1 { S1 = "asdf" }, new Doc1 { S1 = "forgfoot" }, new Doc1 { S1 = "eat borsch!" } },
        COL2 = new List<Doc2> { new Doc2() { S1 = "in d1", B1 = true, I1 = 1234, DT1 = new DateTime(1980, 09, 15, 0, 0, 0, DateTimeKind.Utc), NI1 = null, NL1 = 9_000_000_000L, S2 = null, NDT1 = null } }
      };

      var json = d1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

      var d2 = JsonReader.ToDoc<Doc5>(json);

      json.WriteLine();

      var diff = comparer.Compare(d1, d2);

      diff.See();
      Aver.IsTrue(diff.AreSame);
    }

    [Run]
    public void FullCycle_doc5_into_doc4()
    {
      var comparer = new DocLogicalComparer();

      var d1 = new Doc5
      {
        COL1 = new List<Doc1> { null, null, new Doc1 { S1 = "asdf" }, new Doc1 { S1 = "forgfoot" }, new Doc1 { S1 = "eat borsch!" } },
        COL2 = new List<Doc2> { new Doc2() { S1 = "in d1", B1 = true, I1 = 1234, DT1 = new DateTime(1980, 09, 15, 0, 0, 0, DateTimeKind.Utc), NI1 = null, NL1 = 9_000_000_000L, S2 = null, NDT1 = null } }
      };

      var json = d1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

      //Notice how we round-trip LIST into []
      var d2 = JsonReader.ToDoc<Doc4>(json);

      json.WriteLine();

      var diff = comparer.Compare(d1, d2);

      diff.See();
      Aver.IsTrue(diff.AreSame);
    }


    public class Doc1 : TypedDoc
    {
      [Field]
      public string S1{ get; set;}
    }


    public class Doc2 : TypedDoc
    {
      [Field] public string S1 { get; set; }
      [Field] public string S2 { get; set; }
      [Field] public int I1 { get; set; }
      [Field] public bool B1 { get; set; }
      [Field] public int? NI1 { get; set; }
      [Field] public long? NL1 { get; set; }
      [Field] public DateTime DT1 { get; set; }
      [Field] public DateTime? NDT1 { get; set; }
    }


    public class Doc3 : TypedDoc//composite
    {
      [Field] public Doc1 D1 { get; set; }
      [Field] public Doc2 D2 { get; set; }
    }


    public class Doc4 : TypedDoc//composite
    {
      [Field] public Doc1[] COL1 { get; set; }
      [Field] public Doc2[] COL2 { get; set; }
    }


    public class Doc5 : TypedDoc//composite
    {
      [Field] public List<Doc1> COL1 { get; set; }
      [Field] public List<Doc2> COL2 { get; set; }
    }

  }
}
