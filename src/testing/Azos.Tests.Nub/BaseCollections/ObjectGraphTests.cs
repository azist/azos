/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Threading.Tasks;
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
      var got = ObjectGraph.Scope("A", this, (i, g) =>
      {
        Aver.AreEqual(0, g.CallDepth);
        Aver.IsFalse(g.Visited(i));
        Aver.IsTrue(g.InFlow(i));
        Aver.IsTrue(g.Current == i);
        return 123;
      });

      Aver.IsTrue(got.OK);
      Aver.AreEqual(123, got.result);
    }

    [Run]
    public void Test2()
    {
      var list = new List<int>();
      var got = ObjectGraph.Scope("A", this, list, (i, g, lst) =>
      {
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
      list.ForEach((v, idx) => Aver.AreEqual(v, idx + 1));
    }

    private int body(List<int> list)
    {
      var got = ObjectGraph.Scope("A", new object(), list, (i, g, lst) =>
      {
        list.Add(g.CallDepth);
        Aver.AreEqual(list.Count, g.CallDepth);
        Aver.IsTrue(g.Visited(this));
        Aver.IsFalse(g.Visited(i));
        Aver.IsTrue(g.InFlow(i));
        Aver.IsTrue(g.InFlow(this));
        Aver.IsTrue(g.Current == i);

        return list.Count == 123 ? 123 : body(list);
      });

      Aver.IsTrue(got.OK);
      return got.result;
    }

    [Run]
    public void Test3()
    {
      Parallel.For(0, 50_000, _ =>
      {
        var list = new List<int>();
        var got = ObjectGraph.Scope("A", this, list, (i, g, lst) =>
        {
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
        list.ForEach((v, idx) => Aver.AreEqual(v, idx + 1));
      });
    }


    public class Node
    {
      public int Visits;
      public string Tag;
      public Node Left;
      public Node Right;

      public string Visit()
      {
        var state = ObjectGraph.Scope("name", this, (s, graph) =>
        {
          Visits++;
          var result = "{0}(".Args(Tag);
          if (Left != null) result = result + Left.Visit(); else result = result + "null";
          if (Right != null) result = result + "," + Right.Visit(); else result = result + ",null";
          return result + ")";
        });
        return state.OK ? state.result : "CYCLE";
      }

      public string VisitOnce()
      {
        var state = ObjectGraph.Scope("name", this, (s, graph) =>
        {
          Visits++;
          var result = "{0}(".Args(Tag);
          if (Left != null)
          {
            if (graph.Visited(Left))
              result = result + "VISITED";
            else
              result = result + Left.VisitOnce();
          }
          else result = result + "null";

          if (Right != null)
          {
            if (graph.Visited(Right))
              result = result + ", VISITED";
            else
              result = result + "," + Right.VisitOnce();
          }
          else result = result + ",null";

          return result + ")";
        });
        return state.OK ? state.result : "CYCLE";
      }
    }

    [Run]
    public void Graph1()
    {
      var root = new Node()
      {
        Tag = "root",
        Left = new Node()
        {
          Tag = "A",
          Left = new Node() { Tag = "A-A" },
          Right = new Node() { Tag = "A-B" },
        },
        Right = new Node()
        {
          Tag = "B",
          Left = new Node()
          {
            Tag = "B-A",
            Left = new Node() { Tag = "B-A-A" },
            Right = new Node() { Tag = "B-A-B" },
          },
          Right = new Node() { Tag = "B-B" },
        }
      };

      var got = root.Visit();
      got.See();
      Aver.AreEqual("root(A(A-A(null,null),A-B(null,null)),B(B-A(B-A-A(null,null),B-A-B(null,null)),B-B(null,null)))", got);
      Aver.AreEqual(1, root.Visits);
    }

    [Run]
    public void Graph2()
    {
      var root = new Node()
      {
        Tag = "root",
        Left = new Node()
        {
          Tag = "A",
          Left = new Node() { Tag = "A-A" },
          Right = new Node() { Tag = "A-B" },
        },
        Right = new Node()
        {
          Tag = "B",
          Left = new Node()
          {
            Tag = "B-A",
            Left = new Node() { Tag = "B-A-A" },
            Right = new Node() { Tag = "B-A-B" },
          },
          Right = new Node() { Tag = "B-B" },
        }
      };

      root.Right.Right.Left = root.Right;//circular reference

      var got = root.Visit();
      got.See();
      Aver.AreEqual("root(A(A-A(null,null),A-B(null,null)),B(B-A(B-A-A(null,null),B-A-B(null,null)),B-B(CYCLE,null)))", got);
      Aver.AreEqual(1, root.Visits);
    }

    [Run]
    public void Graph3()
    {
      var root = new Node()
      {
        Tag = "root",
        Left = new Node()
        {
          Tag = "A",
          Left = new Node() { Tag = "A-A" },
          Right = new Node() { Tag = "A-B" },
        },
        Right = new Node()
        {
          Tag = "B",
          Left = new Node()
          {
            Tag = "B-A",
            Left = new Node() { Tag = "B-A-A" },
            Right = new Node() { Tag = "B-A-B" },
          },
          Right = new Node() { Tag = "B-B" },
        }
      };

      root.Right.Right.Left = root.Left.Left;//re-use of the other branch

      var got = root.Visit();
      got.See();
      Aver.AreEqual("root(A(A-A(null,null),A-B(null,null)),B(B-A(B-A-A(null,null),B-A-B(null,null)),B-B(A-A(null,null),null)))", got);
      Aver.AreEqual(1, root.Visits);
      Aver.AreEqual(2, root.Left.Left.Visits);//and it is not suppressed, because Visit does not check Visited()

      got = root.VisitOnce();
      got.See();
      Aver.AreEqual("root(A(A-A(null,null),A-B(null,null)),B(B-A(B-A-A(null,null),B-A-B(null,null)),B-B(VISITED,null)))", got);
      Aver.AreEqual(2, root.Visits);//increased by just one
      Aver.AreEqual(3, root.Left.Left.Visits);//and it is not visited 2nd time, so it increased just by one, not by 2
    }

  }
}
