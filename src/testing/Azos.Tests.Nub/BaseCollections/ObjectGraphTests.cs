using System;
using System.Collections.Generic;
using System.Text;
using Azos.Collections;
using Azos.Scripting;

namespace Azos.Tests.Nub.BaseCollections
{
  [Runnable]
  public class ObjectGraphTests
  {
    [Run]
    public void Test1()
    {
      var got =  ObjectGraph.Scope("A", this, (i, g) => {
        Aver.AreEqual(0, g.CallDepth);
        Aver.IsFalse(g.Visited(i));
        Aver.IsTrue(g.InFlow(i));
        Aver.IsTrue(g.Current==i);
        return 123;
      });

      Aver.IsTrue(got.OK);
      Aver.AreEqual(123, got.result);
    }

    [Run]
    public void Test2()
    {
      var list = new List<int>();
      var got = ObjectGraph.Scope("A", this, list, (i, g, lst) => {
        Aver.AreEqual(0, g.CallDepth);
        Aver.IsFalse(g.Visited(i));
        Aver.IsTrue(g.InFlow(i));
        Aver.IsTrue(g.Current == i);
        var result = body(lst);
        Aver.AreEqual(1, g.Machine.m_CallDepth);
        return result;
      });

      Aver.IsTrue(got.OK);
      Aver.AreEqual(123, got.result);
      Aver.AreEqual(123, list.Count);
      list.See();
      list.ForEach((v, idx) => Aver.AreEqual(v, idx+1));
    }

    private int body(List<int> list)
    {
      var got = ObjectGraph.Scope("A", new object(), list, (i, g, lst) => {
        list.Add(g.CallDepth);
        Aver.AreEqual(list.Count, g.CallDepth);
        Aver.IsTrue(g.Visited(this));
        Aver.IsFalse(g.Visited(i));
        Aver.IsTrue(g.InFlow(i));
        Aver.IsTrue(g.InFlow(this));
        Aver.IsTrue(g.Current == i);

        return list.Count==123 ? 123 : body(list);
      });

      Aver.IsTrue(got.OK);
      return got.result;
    }

  }
}
