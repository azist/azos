using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azos.Platform;
using Azos.Scripting;

namespace Azos.Tests.Nub.Plat
{
  [Runnable]
  public class ConstrainedSetTests
  {
    private class my
    {
      public int Tag{ get; set;}
    }


    [Run]
    public void SingleThreaded()
    {
      var all = new List<my>();
      var set = new FiniteSetLookup<int, my>( k =>{ var v = new my{ Tag = k }; all.Add(v); return v;} );

      for(var i=0; i<1000; i++)
      {
        var key = i % 7;
        var got = set[key];
        Aver.AreEqual(key, got.Tag);
      }

      Aver.AreEqual(7, all.Count);
      Aver.AreEqual(7, set.Count);


      var deleted = 0;
      for (var i = 0; i < 1000; i++)
      {
        var key = i % 5;
        if (set.Remove(key)) deleted++;
      }
      Aver.AreEqual(2, set.Count);
      Aver.AreEqual(5, deleted);
    }

    [Run]
    public void MultiThreaded()
    {
      var all = new List<my>();
      var set = new FiniteSetLookup<int, my>(k => { var v = new my { Tag = k }; all.Add(v); return v; });

      Parallel.For(0, 1000,  i =>
      {
        var key = i % 7;
        var got = set[key];
        Aver.AreEqual(key, got.Tag);
      });

      Aver.AreEqual(7, all.Count);
      Aver.AreEqual(7, set.Count);

      var deleted = 0;
      Parallel.For(0, 1000, i =>
      {
        var key = i % 5;
        if (set.Remove(key)) Interlocked.Increment(ref deleted);
      });

      Aver.AreEqual(2, set.Count);
      Aver.AreEqual(5, deleted);
    }
  }
}
