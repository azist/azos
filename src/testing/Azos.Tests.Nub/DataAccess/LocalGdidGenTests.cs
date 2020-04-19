/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Data;
using Azos.Data.Idgen;
using Azos.Scripting;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class LocalGdidGenTests
  {
    [Run]
    public void Basic_OneGdid()
    {
      var sut = new LocalGdidGenerator(NOPApplication.Instance);
      //sut.Era = 39;
      //sut.Authority = 7;

      var set =new HashSet<GDID>();

      const int CNT = 100;

      for(var i=0; i<CNT; i++)
      {
        var got = sut.GenerateOneGdid("a", "a");
        "{0}  -  {1}".SeeArgs(got, got.ToHexString());
        Aver.IsTrue(set.Add(got));
      }

      Aver.AreEqual(CNT, set.Count);
    }

    [Run]
    public void Basic_ManyGdid()
    {
      var sut = new LocalGdidGenerator(NOPApplication.Instance);

      var set = new HashSet<GDID>();

      const int CNT = 100;

      for (var i = 0; i < CNT; i++)
      {
        var got = sut.TryGenerateManyConsecutiveGdids("a", "a", 16);
        Aver.IsNotNull(got);
        Aver.AreEqual(16, got.Length);
        got.See();
        got.ForEach(g => Aver.IsTrue(set.Add(g)));
      }

      Aver.AreEqual(CNT * 16, set.Count);
    }

    [Run]
    public void DifferentNames()
    {
      var sut = new LocalGdidGenerator(NOPApplication.Instance);
      sut.Era = 39;
      sut.Authority = 7;

      var got = sut.GenerateOneGdid("a", "a");
      Aver.AreEqual(39u, got.Era);
      Aver.AreEqual(7, got.Authority);
      got.See();

      var got2 = sut.GenerateOneGdid("a", "a");
      Aver.AreEqual(39u, got2.Era);
      Aver.AreEqual(7, got2.Authority);
      Aver.AreNotEqual(got, got2);
      got2.See();

      var got3 = sut.GenerateOneGdid("a", "b");
      Aver.AreEqual(39u, got3.Era);
      Aver.AreEqual(7, got3.Authority);
      Aver.AreNotEqual(got, got3);   //because of a different timeslice even though it is a different seq name
      Aver.AreNotEqual(got2, got3);
      got3.See();

      var got4 = sut.GenerateOneGdid("baba", "a");
      Aver.AreEqual(39u, got4.Era);
      Aver.AreEqual(7, got4.Authority);
      Aver.AreNotEqual(got3, got4); //because of a different timeslice even though it is a different namespace name
      Aver.AreNotEqual(got2, got4);
      Aver.AreNotEqual(got, got4);
      got3.See();


      Aver.AreEqual(2, sut.SequenceScopeNames.Count());
      Aver.IsTrue(sut.SequenceScopeNames.Contains("a"));
      Aver.IsTrue(sut.SequenceScopeNames.Contains("baba"));

      Aver.AreEqual(2, sut.GetSequenceInfos("a").Count());
      Aver.IsTrue(sut.GetSequenceInfos("a").Any(si=> si.Name == "a"));
      Aver.IsTrue(sut.GetSequenceInfos("a").Any(si => si.Name == "b"));

      Aver.AreEqual(1, sut.GetSequenceInfos("baba").Count());
      Aver.IsTrue(sut.GetSequenceInfos("baba").Any(si => si.Name == "a"));
    }

    [Run]
    public void Reallocate_OneGdid()
    {
      var set = new HashSet<GDID>();

      var sut = new LocalGdidGenerator(NOPApplication.Instance);

      const int CNT = 1000;

      for (var i = 0; i < CNT; i++)
      {
        var got = sut.GenerateOneGdid("a", "a");
        Aver.IsTrue(set.Add(got));
      }

      Aver.AreEqual(CNT, set.Count);

      sut = new LocalGdidGenerator(NOPApplication.Instance);

      for (var i = 0; i < CNT; i++)
      {
        var got = sut.GenerateOneGdid("a", "a");
        Aver.IsTrue(set.Add(got));
      }

      Aver.AreEqual(CNT*2, set.Count);
    }

    [Run]
    public void Parallel_OneGdid()
    {
      var set = new HashSet<GDID>();

      var sut = new LocalGdidGenerator(NOPApplication.Instance);

      const int CNT = 1_250_000;

      Parallel.For(0, CNT, new ParallelOptions(), ()=> new HashSet<GDID>(), (i, _, ls) =>
      {
        var got = sut.GenerateOneGdid("a", "a");
        Aver.IsTrue(ls.Add(got));
        return ls;
      }, ls => {
        lock(set)
        {
          ls.ForEach(e => Aver.IsTrue(set.Add(e)));
        }
      });

      Aver.AreEqual(CNT, set.Count);

      sut = new LocalGdidGenerator(NOPApplication.Instance);

      Parallel.For(0, CNT, new ParallelOptions(), () => new HashSet<GDID>(), (i, _, ls) =>
      {
        var got = sut.GenerateOneGdid("a", "a");
        Aver.IsTrue(ls.Add(got));
        return ls;
      }, ls => {
        lock (set)
        {
          ls.ForEach(e => Aver.IsTrue(set.Add(e)));
        }
      });

      Aver.AreEqual(CNT * 2, set.Count);
    }
  }
}
