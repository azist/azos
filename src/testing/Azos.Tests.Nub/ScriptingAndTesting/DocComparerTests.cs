using System;
using System.Collections.Generic;
using System.Text;
using Azos.Data;
using Azos.Scripting;

namespace Azos.Tests.Nub.ScriptingAndTesting
{
  [Runnable]
  public class DocComparerTests
  {
    [Run]
    public void TwoEmpty()
    {
      var comparer = new DocComparer();

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
      var comparer = new DocComparer();

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
      var comparer = new DocComparer();

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
      var comparer = new DocComparer();

      var d1 = new Doc1() { S1 = "in d1"};
      var d2 = new Doc1() { S1 = "this is in d2" };

      var diff = comparer.Compare(d1, d2);

      diff.See();
      Aver.IsFalse(diff.AreSame);
      Aver.IsTrue(diff.AreDifferent);
    }





    public class Doc1 : TypedDoc
    {
      [Field]
      public string S1{ get; set;}
    }



  }
}
