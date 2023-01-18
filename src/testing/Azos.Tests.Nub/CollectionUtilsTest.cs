/*<FILE_LICENSE>
* Azos (A to Z Application Operating System) Framework
* The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
* See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Linq;

using Azos.Scripting;

namespace Azos.Tests.Nub
{

  [Runnable]
  public class CollectionUtilsTest
  {
    private class Tezt
    {
      public int A;
      public string B;
      public double C;
    }

    [Run]
    public void FirstMin()
    {
      var data = new[]{ new Tezt{A=0, B="Car", C=12.1d},
                          new Tezt{A=2, B="Tzar", C=-21.7d},
                          new Tezt{A=-3, B="Zuma", C=10000d},
                        };

      var min = data.FirstMin(d => d.A);
      Aver.AreEqual(-3, min.A);
      Aver.AreEqual("Zuma", min.B);
      Aver.AreEqual(10000d, min.C);

      min = data.FirstMin(d => d.B);
      Aver.AreEqual(0, min.A);
      Aver.AreEqual("Car", min.B);
      Aver.AreEqual(12.1d, min.C);

      min = data.FirstMin(d => d.C);
      Aver.AreEqual(2, min.A);
      Aver.AreEqual("Tzar", min.B);
      Aver.AreEqual(-21.7d, min.C);

      double c;
      min = data.FirstMin(d => d.C, out c);
      Aver.AreEqual(2, min.A);
      Aver.AreEqual("Tzar", min.B);
      Aver.AreEqual(-21.7d, min.C);
      Aver.AreEqual(-21.7d, c);

    }

    [Run]
    public void FirstMax()
    {
      var data = new[]{ new Tezt{A=0, B="Car", C=12.1d},
                          new Tezt{A=2, B="Tzar", C=-21.7d},
                          new Tezt{A=-3, B="Zuma", C=10000d},
                        };

      var max = data.FirstMax(d => d.A);
      Aver.AreEqual(2, max.A);
      Aver.AreEqual("Tzar", max.B);
      Aver.AreEqual(-21.7d, max.C);

      max = data.FirstMax(d => d.B);
      Aver.AreEqual(-3, max.A);
      Aver.AreEqual("Zuma", max.B);
      Aver.AreEqual(10000d, max.C);

      max = data.FirstMax(d => d.C);
      Aver.AreEqual(-3, max.A);
      Aver.AreEqual("Zuma", max.B);
      Aver.AreEqual(10000d, max.C);

      double c;
      max = data.FirstMax(d => d.C, out c);
      Aver.AreEqual(-3, max.A);
      Aver.AreEqual("Zuma", max.B);
      Aver.AreEqual(10000d, max.C);
      Aver.AreEqual(10000d, c);
    }

    [Run]
    public void FirstOrAnyOrDefault()
    {
      var data = new[]{ new Tezt{A=0, B="Car", C=12.1d},
                          new Tezt{A=2, B="Tzar", C=-21.7d},
                          new Tezt{A=-3, B="Zuma", C=10000d},
                        };

      var m = data.FirstOrAnyOrDefault(elm => elm.B == "Tzar");
      Aver.AreEqual(2, m.A);

      m = data.FirstOrAnyOrDefault(elm => elm.B == "Sidor");
      Aver.AreEqual(0, m.A);

      data = null;
      m = data.FirstOrAnyOrDefault(elm => elm.B == "Sidor");
      Aver.IsNull(m);

      data = new Tezt[0];
      m = data.FirstOrAnyOrDefault(elm => elm.B == "Sidor");
      Aver.IsNull(m);
    }

    [Run]
    public void AppendToNewArray_0()
    {
      int[] existing = null;
      var got = existing.AppendToNew(null);
      Aver.IsNotNull(got);
      Aver.AreEqual(0, got.Length);
    }

    [Run]
    public void AppendToNewArray_1()
    {
      var existing = new int[0];
      var got = existing.AppendToNew();
      Aver.IsNotNull(got);
      Aver.AreNotSameRef(existing, got);
    }

    [Run]
    public void AppendToNewArray_2()
    {
      var existing = new int[] { 1, 2, 3 };
      var got = existing.AppendToNew();
      Aver.IsNotNull(got);
      Aver.AreNotSameRef(existing, got);
    }

    [Run]
    public void AppendToNewArray_3()
    {
      var existing = new int[] { 1, 2, 3 };
      var got = existing.AppendToNew(-5);
      Aver.IsNotNull(got);
      Aver.AreNotSameRef(existing, got);
      Aver.AreEqual(4, got.Length);
      Aver.AreEqual(1, got[0]);
      Aver.AreEqual(2, got[1]);
      Aver.AreEqual(3, got[2]);
      Aver.AreEqual(-5, got[3]);
    }

    [Run]
    public void AppendToNewArray_4()
    {
      var existing = new int[] { 1, 2, 3 };
      var got = existing.AppendToNew(new int[] { -90, -1 });
      Aver.IsNotNull(got);
      Aver.AreNotSameRef(existing, got);
      Aver.AreEqual(5, got.Length);
      Aver.AreEqual(1, got[0]);
      Aver.AreEqual(2, got[1]);
      Aver.AreEqual(3, got[2]);
      Aver.AreEqual(-90, got[3]);
      Aver.AreEqual(-1, got[4]);
    }

    [Run]
    public void ConcatArray_0()
    {
      var got = 2.ConcatArray();
      Aver.IsNotNull(got);
      Aver.AreEqual(1, got.Length);
      Aver.AreEqual(2, got[0]);
    }

    [Run]
    public void ConcatArray_1()
    {
      var got = 2.ConcatArray(3);
      Aver.IsNotNull(got);
      Aver.AreEqual(2, got.Length);
      Aver.AreEqual(2, got[0]);
      Aver.AreEqual(3, got[1]);
    }

    [Run]
    public void ConcatArray_3()
    {
      var got = 2.ConcatArray(3, -1, 90);
      Aver.IsNotNull(got);
      Aver.AreEqual(4, got.Length);
      Aver.AreEqual(2, got[0]);
      Aver.AreEqual(3, got[1]);
      Aver.AreEqual(-1, got[2]);
      Aver.AreEqual(90, got[3]);
    }

    [Run]
    public void SkipLastTest_1()
    {
      var seq = Enumerable.Range(1, 5);
      Aver.AreEqual("1,2,3,4", string.Join(",", seq.SkipLast().ToArray()));
    }

    [Run]
    public void SkipLastTest_2()
    {
      var seq = Enumerable.Range(1, 5);
      Aver.AreEqual("1,2,3,4", string.Join(",", seq.SkipLast(1).ToArray()));
      Aver.AreEqual("1,2,3", string.Join(",", seq.SkipLast(2).ToArray()));
      Aver.AreEqual("", string.Join(",", seq.SkipLast(5).ToArray()));
      Aver.AreEqual("", string.Join(",", seq.SkipLast(5000).ToArray()));
    }

    [Run]
    public void AddOneAtEndTest_1()
    {
      var seq = Enumerable.Range(1, 2).AddOneAtEnd(10).ToArray();

      Aver.AreEqual(3, seq.Length);
      Aver.AreEqual(1, seq[0]);
      Aver.AreEqual(2, seq[1]);
      Aver.AreEqual(10, seq[2]);
    }

    [Run]
    public void AddOneAtEndTest_2()
    {
      IEnumerable<int> orig = null;
      var seq = orig.AddOneAtEnd(102).ToArray();

      Aver.AreEqual(1, seq.Length);
      Aver.AreEqual(102, seq[0]);
    }

    [Run]
    public void AddOneAtStartTest_1()
    {
      var seq = Enumerable.Range(1, 2).AddOneAtStart(10).ToArray();

      Aver.AreEqual(3, seq.Length);
      Aver.AreEqual(10, seq[0]);
      Aver.AreEqual(1, seq[1]);
      Aver.AreEqual(2, seq[2]);
    }

    [Run]
    public void AddOneAtStartTest_2()
    {
      IEnumerable<int> orig = null;
      var seq = orig.AddOneAtStart(102).ToArray();

      Aver.AreEqual(1, seq.Length);
      Aver.AreEqual(102, seq[0]);
    }

    [Run]
    public void BatchByTests_1()
    {
      var seq = 1.ToEnumerable(3, 5, 9, 10, 20, 30, 40, 50).BatchBy(2).ToArray();

      Aver.AreEqual(5, seq.Length);

      Aver.AreArraysEquivalent(new[] { 1, 3, 5, 9, 10, 20, 30, 40, 50 }, seq.SelectMany(e => e).ToArray());

      Aver.AreArraysEquivalent(new[] { 1, 3 }, seq[0].ToArray());
      Aver.AreArraysEquivalent(new[] { 5, 9 }, seq[1].ToArray());
      Aver.AreArraysEquivalent(new[] { 10, 20 }, seq[2].ToArray());
      Aver.AreArraysEquivalent(new[] { 30, 40 }, seq[3].ToArray());
      Aver.AreArraysEquivalent(new[] { 50 }, seq[4].ToArray());
    }

    [Run]
    public void BatchByTests_2()
    {
      var seq = 1.ToEnumerable(3, 5, 9, 10, 20, 30, 40).BatchBy(2).ToArray();

      Aver.AreEqual(4, seq.Length);

      Aver.AreArraysEquivalent(new[] { 1, 3, 5, 9, 10, 20, 30, 40 }, seq.SelectMany(e => e).ToArray());

      Aver.AreArraysEquivalent(new[] { 1, 3 }, seq[0].ToArray());
      Aver.AreArraysEquivalent(new[] { 5, 9 }, seq[1].ToArray());
      Aver.AreArraysEquivalent(new[] { 10, 20 }, seq[2].ToArray());
      Aver.AreArraysEquivalent(new[] { 30, 40 }, seq[3].ToArray());
    }

    [Run]
    public void ShuffleTests_1()
    {
      var existing = 1.ToEnumerable(3, 5, 9, 10, 20, 30, 40, 50).ToList();
      var got = existing.RandomShuffle();

      Aver.AreEqual(existing.Count, got.Count);
      Aver.AreArraysEquivalent(existing.OrderBy(x => x).ToArray(), got.OrderBy(x => x).ToArray());

      //existing.See("existing:");
      //got.See("got");
    }

    [Run]
    public void ShuffleTests_2()
    {
      var existing = Enumerable.Range(10,100).ToList();
      var got = existing.RandomShuffle();

      Aver.AreEqual(existing.Count, got.Count);
      Aver.AreArraysEquivalent(existing.OrderBy(x => x).ToArray(), got.OrderBy(x => x).ToArray());

      //existing.See("existing:");
      //got.See("got");
    }

    [Run]
    public void ShuffleTests_3()
    {
      var existing = new List<Tezt>{ new Tezt{A=0, B="Car", C=12.1d},
                          new Tezt{A=2, B="Tzar", C=-21.7d},
                          new Tezt{A=-3, B="Zuma", C=10000d},
                        };
      var got = existing.RandomShuffle();

      Aver.AreEqual(existing.Count, got.Count);
      Aver.AreArrayObjectsEquivalent(existing.OrderBy(x => x.A).ToArray(), got.OrderBy(x => x.A).ToArray());

      //existing.See("existing:");
      //got.See("got");
    }

  }
}
