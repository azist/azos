/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Apps;
using Azos.Data;
using Azos.Pile;
using Azos.Scripting;

namespace Azos.Tests.Unit.Pile
{
  [Runnable(TRUN.BASE, 7)]
  public class CacheTest
  {
      [Run(TRUN.BASE, null, 8)]
      public void T010_MainOperations()
      {
        using (var cache = PileCacheTestCore.MakeCache())
        {
          var tA = cache.GetOrCreateTable<string>("A");
          var tB = cache.GetOrCreateTable<string>("B");

          Aver.IsNotNull( tA );
          Aver.IsNotNull( tB );

          Aver.AreEqual(2, cache.Tables.Count );

          Aver.AreEqual( 0, tA.Count);
          Aver.AreEqual( 0, tB.Count);

          Aver.IsTrue(PutResult.Inserted == tA.Put("key1", "avalue1"));
          Aver.IsTrue(PutResult.Inserted == tA.Put("key2", "avalue2"));
          Aver.IsTrue(PutResult.Inserted == tB.Put("key1", "bvalue1"));

          Aver.AreEqual( 3, cache.Count);
          Aver.AreEqual( 2, tA.Count);
          Aver.AreEqual( 1, tB.Count);

          Aver.IsTrue( tA.ContainsKey("key1") );
          Aver.IsTrue( tA.ContainsKey("key2") );
          Aver.IsFalse( tA.ContainsKey("key3 that was never put") );

          Aver.IsTrue( tB.ContainsKey("key1") );
          Aver.IsFalse( tB.ContainsKey("key2") );


          Aver.AreObjectsEqual("avalue1", tA.Get("key1"));
          Aver.AreObjectsEqual("avalue2", tA.Get("key2"));
          Aver.IsNull(tA.Get("key3"));

          Aver.AreObjectsEqual("bvalue1", tB.Get("key1"));

          Aver.AreObjectsEqual("avalue1", cache.GetTable<string>("A").Get("key1"));
          Aver.AreObjectsEqual("bvalue1", cache.GetTable<string>("B").Get("key1"));

          Aver.IsTrue( tA.Remove("key1"));
          Aver.IsFalse( tA.Remove("key2342341"));

          Aver.AreEqual( 2, cache.Count);
          Aver.AreEqual( 1, tA.Count);
          Aver.AreEqual( 1, tB.Count);

          cache.PurgeAll();
          Aver.AreEqual( 0, cache.Count);
          Aver.AreEqual( 0, tA.Count);
          Aver.AreEqual( 0, tB.Count);

          Aver.AreEqual( 0, cache.Pile.UtilizedBytes );
          Aver.AreEqual( 0, cache.Pile.ObjectCount );
        }
      }


      [Run(TRUN.BASE, null, 8)]
      public void T020_Comparers()
      {
        using (var cache = PileCacheTestCore.MakeCache())
        {
          var tA = cache.GetOrCreateTable<string>("A", StringComparer.Ordinal);
          var tB = cache.GetOrCreateTable<string>("B", StringComparer.OrdinalIgnoreCase);

          Aver.IsTrue(PutResult.Inserted == tA.Put("key1", "avalue1"));
          Aver.IsTrue(PutResult.Inserted == tA.Put("Key1", "avalue2"));
          Aver.IsTrue(PutResult.Inserted == tB.Put("key1", "bvalue1"));
          Aver.IsTrue(PutResult.Replaced == tB.Put("Key1", "bvalue2"));

          Aver.AreEqual( 2, tA.Count);
          Aver.AreEqual( 1, tB.Count);

          Aver.AreObjectsEqual("avalue1", tA.Get("key1"));
          Aver.AreObjectsEqual("avalue2", tA.Get("Key1"));
          Aver.IsNull(tA.Get("key3"));

          Aver.AreObjectsEqual("bvalue2", tB.Get("key1"));
          Aver.AreObjectsEqual("bvalue2", tB.Get("Key1"));
        }
      }

      [Run(TRUN.BASE, null, 8)]
      [Aver.Throws(typeof(PileCacheException), Message="comparer mismatch", MsgMatch= Aver.ThrowsAttribute.MatchType.Contains)]
      public void T030_Comparers()
      {
        using (var cache = PileCacheTestCore.MakeCache())
        {
          var tA = cache.GetOrCreateTable<string>("A", StringComparer.Ordinal);
          var tB = cache.GetOrCreateTable<string>("A", StringComparer.OrdinalIgnoreCase);
        }
      }

      [Run(TRUN.BASE, null, 8)]
      public void T040_PileOwnership()
      {
        var cache = new LocalCache();
        cache.Pile = new DefaultPile(cache);
        cache.Configure(null);
        cache.Start();


        var tA = cache.GetOrCreateTable<string>("A");
        tA.Put("aaa", "avalue");
        Aver.AreObjectsEqual("avalue", tA.Get("aaa"));

        cache.WaitForCompleteStop();
        Aver.IsTrue(ServiceStatus.Inactive == cache.Status);
        Aver.IsTrue(ServiceStatus.Inactive == cache.Pile.Status);
      }

      [Run(TRUN.BASE, null, 8)]
      [Aver.Throws(typeof(PileCacheException), Message="has not been started", MsgMatch= Aver.ThrowsAttribute.MatchType.Contains)]
      public void T050_PileNonOwnershipErrorStart()
      {
        var pile = new DefaultPile();

        try
        {
          var cache = new LocalCache();
          cache.Pile = pile;
          cache.Configure(null);
          cache.Start();  //can not start cache that uses inactive pile which is not managed by this cache
        }
        finally
        {
          pile.Dispose();
        }

      }


      [Run(TRUN.BASE, null, 8)]
      public void T060_PileNonOwnership()
      {
        var pile = new DefaultPile();
        pile.Start();
        try
        {
              var cache = new LocalCache();
              cache.Pile = pile;
              cache.Configure(null);
              cache.Start();


              var tA = cache.GetOrCreateTable<string>("A");
              tA.Put("aaa", "avalue");
              tA.Put("bbb", "bvalue");
              Aver.AreObjectsEqual("avalue", tA.Get("aaa"));

              Aver.AreEqual(2, cache.Count);
              Aver.AreEqual(2, pile.ObjectCount);

              cache.WaitForCompleteStop();
              Aver.IsTrue(ServiceStatus.Inactive == cache.Status);

              Aver.IsTrue(ServiceStatus.Active == pile.Status);
              Aver.AreEqual(0, pile.ObjectCount);

              cache = new LocalCache();
              cache.Pile = pile;
              cache.Configure(null);
              cache.Start();

              var tAbc = cache.GetOrCreateTable<string>("Abc");
              tAbc.Put("aaa", "avalue");
              tAbc.Put("bbb", "bvalue");
              tAbc.Put("ccc", "cvalue");
              tAbc.Put("ddd", "cvalue");

              Aver.AreEqual(4, pile.ObjectCount);

              var cache2 = new LocalCache();
              cache2.Pile = pile;
              cache2.Configure(null);
              cache2.Start();


              var t2 = cache2.GetOrCreateTable<string>("A");
              t2.Put("aaa", "avalue");
              t2.Put("bbb", "bvalue");

              Aver.AreEqual(2, cache2.Count);
              Aver.AreEqual(6, pile.ObjectCount);

              cache.WaitForCompleteStop();
              Aver.IsTrue(ServiceStatus.Active == pile.Status);
              Aver.AreEqual(2, pile.ObjectCount);

              cache2.WaitForCompleteStop();
              Aver.IsTrue(ServiceStatus.Active == pile.Status);
              Aver.AreEqual(0, pile.ObjectCount);

              pile.WaitForCompleteStop();
              Aver.IsTrue(ServiceStatus.Inactive == pile.Status);
        }
        finally
        {
          pile.Dispose();
        }
      }


      [Run(TRUN.BASE, null, 8)]
      public void T080_PutGetWithoutMaxCap()
      {
        using (var cache = PileCacheTestCore.MakeCache())
        {
          var tA = cache.GetOrCreateTable<int>("A");
       ////   tA.Options.MaximumCapacity = 7;

          const int CNT = 4000;
          for(var i=0;i<CNT; i++)
          {
            var pr = tA.Put(i, "value"+i.ToString(), priority: 10);
           // Console.WriteLine("{0} -> {1}", i, pr);
            Aver.IsTrue( pr == PutResult.Inserted);//no overwrite because table keeps growing
          }

          for(var i=0;i<CNT; i++)
          {
            var v = tA.Get(i);
           // Console.WriteLine("{0} -> {1}", i, v);
            Aver.IsTrue( v.Equals("value"+i.ToString()));
          }
        }
      }

      [Run(TRUN.BASE, null, 8)]
      public void T090_PutGetWithMaxCap()
      {
        using (var cache = PileCacheTestCore.MakeCache())
        {
          var tA = cache.GetOrCreateTable<int>("A");
          tA.Options.MaximumCapacity = 7;

          const int CNT = 4000;
          var lst = new List<PutResult>();
          for(var i=0;i<CNT; i++)
          {
            var pr = tA.Put(i, "value"+i.ToString(), priority: 10);
          //  Console.WriteLine("{0} -> {1}", i, pr);
            Aver.IsTrue( pr == PutResult.Inserted || pr == PutResult.Overwritten);//the table is capped, so some values will be overwritten

            lst.Add(pr);
          }

          Aver.IsTrue( lst.Count(r => r==PutResult.Inserted)>0 );
          Aver.IsTrue( lst.Count(r => r==PutResult.Overwritten)>0 );


          for(var i=0;i<CNT; i++)
          {
            var v = tA.Get(i);
          //  Console.WriteLine("{0} -> {1}", i, v);
            Aver.IsTrue( v==null || v.Equals("value"+i.ToString()));
          }
        }
      }

      [Run(TRUN.BASE, null, 8)]
      public void T100_OverwriteWithMaxCap()
      {
        using (var cache = PileCacheTestCore.MakeCache())
        {
          var tA = cache.GetOrCreateTable<int>("A");
          tA.Options.MaximumCapacity = 7;

          const int CNT = 4000;
          var lst = new List<PutResult>();
          for(var i=0;i<CNT; i++)
          {
            var pr = tA.Put(i, "value"+i.ToString(), priority: 10);
          //  Console.WriteLine("{0} -> {1}", i, pr);
            Aver.IsTrue( pr == PutResult.Inserted || pr == PutResult.Overwritten);//the table is capped, so some values will be overwritten
            lst.Add(pr);
          }

          Aver.IsTrue( lst.Count(r => r==PutResult.Inserted)>0 );
          Aver.IsTrue( lst.Count(r => r==PutResult.Overwritten)>0 );

          lst.Clear();
          for(var i=0;i<CNT; i++)
          {
            var pr = tA.Put(i, "value"+i.ToString(), priority: 10);
          //  Console.WriteLine("{0} -> {1}", i, pr);
            Aver.IsTrue( pr == PutResult.Replaced || pr==PutResult.Overwritten  );
            lst.Add(pr);
          }


          Aver.IsTrue( lst.Count(r => r==PutResult.Replaced)>0 );
          Aver.IsTrue( lst.Count(r => r==PutResult.Overwritten)>0 );

          Aver.IsTrue( lst.Count(r => r==PutResult.Overwritten)>lst.Count(r => r==PutResult.Replaced) );

        }
      }


      [Run(TRUN.BASE, null, 8)]
      public void T110_PriorityCollideWithMaxCap()
      {
        using (var cache = PileCacheTestCore.MakeCache())
        {
          var tA = cache.GetOrCreateTable<int>("A");
          tA.Options.MaximumCapacity = 7;

          const int CNT = 4000;
          var lst = new List<PutResult>();
          for(var i=0;i<CNT; i++)
          {
            var pr = tA.Put(i, "value"+i.ToString(), priority: 10);//PRIORITY +10!
          //  Console.WriteLine("{0} -> {1}", i, pr);
            Aver.IsTrue( pr == PutResult.Inserted || pr == PutResult.Overwritten);//the table is capped, so some values will be overwritten
            lst.Add(pr);
          }

          Aver.IsTrue( lst.Count(r => r==PutResult.Inserted)>0 );
          Aver.IsTrue( lst.Count(r => r==PutResult.Overwritten)>0 );

          lst.Clear();
          for(var i=CNT;i<2*CNT; i++)
          {
            var pr = tA.Put(i, "value"+i.ToString(), priority: -10);//only collision, because priority is lowe r than what already exists
          //  Console.WriteLine("{0} -> {1}", i, pr);
            Aver.IsTrue( pr==PutResult.Collision  );
            lst.Add(pr);
          }
        }
      }


      [Run(TRUN.BASE, null, 8, "cnt=100")]
      public void T130_KeyInt_ManyPutGet(int cnt)
      {
        PileCacheTestCore.KeyInt_ManyPutGet(cnt);
      }


      [Run(TRUN.BASE, null, 8, "cnt=100")]
      public void T140_KeyGDID_ManyPutGet(int cnt)
      {
        PileCacheTestCore.KeyGDID_ManyPutGet(cnt);
      }


      [Run(TRUN.BASE, null, 8, "cnt=100")]
      public void T150_KeyString_ManyPutGet(int cnt)
      {
        PileCacheTestCore.KeyString_ManyPutGet(cnt);
      }

      [Run(TRUN.BASE, null, 8, "cnt=25  rec=10000   payload=4096")]
      [Run(TRUN.BASE, null, 8, "cnt=10  rec=300000  payload=16")]
      [Run(TRUN.BASE, null, 8, "cnt=10  rec=100     payload=1048576")]
      [Run(TRUN.BASE, null, 8, "cnt=10  rec=20      payload=8388608")]
      public void T160_ResizeTable(int cnt, int rec, int payload)
      {
        PileCacheTestCore.ResizeTable(cnt, rec, payload);
      }

      [Run(TRUN.BASE, null, 8)]
      public void T170_Config()
      {
        var conf1 =
@"
store
{
        default-table-options
        {
          initial-capacity=1000000
          detailed-instrumentation=true
        }

        table
        {
          name='A'
          minimum-capacity=800000
          maximum-capacity=987654321
          initial-capacity=780000
          growth-factor=2.3
          shrink-factor=0.55
          load-factor-lwm=0.1
          load-factor-hwm=0.9
          default-max-age-sec=145
          detailed-instrumentation=true
        }

        table
        {
          name='B'
          maximum-capacity=256000
          detailed-instrumentation=false
        }

}
";
        var c1 = conf1.AsLaconicConfig(handling: ConvertErrorHandling.Throw);
        using (var cache = PileCacheTestCore.MakeCache(c1))
        {
          var tA = cache.GetOrCreateTable<int>("A");

          var topt = cache.DefaultTableOptions;
          Aver.AreEqual(1000000,    topt.InitialCapacity);
          Aver.AreEqual(true,       topt.DetailedInstrumentation);

          topt = cache.TableOptions["a"];
          Aver.AreEqual(800000,    topt.MinimumCapacity);
          Aver.AreEqual(987654321, topt.MaximumCapacity);
          Aver.AreEqual(780000,    topt.InitialCapacity);
          Aver.AreEqual(2.3d,      topt.GrowthFactor);
          Aver.AreEqual(0.55d,     topt.ShrinkFactor);
          Aver.AreEqual(0.1d,      topt.LoadFactorLWM);
          Aver.AreEqual(0.9d,      topt.LoadFactorHWM);
          Aver.AreEqual(145,       topt.DefaultMaxAgeSec);
          Aver.AreEqual(true,      topt.DetailedInstrumentation);

          topt = cache.GetOrCreateTable<int>("A").Options;
          Aver.AreEqual(800000,    topt.MinimumCapacity);
          Aver.AreEqual(987654321, topt.MaximumCapacity);
          Aver.AreEqual(780000,    topt.InitialCapacity);
          Aver.AreEqual(2.3d,      topt.GrowthFactor);
          Aver.AreEqual(0.55d,     topt.ShrinkFactor);
          Aver.AreEqual(0.1d,      topt.LoadFactorLWM);
          Aver.AreEqual(0.9d,      topt.LoadFactorHWM);
          Aver.AreEqual(145,       topt.DefaultMaxAgeSec);
          Aver.AreEqual(true,      topt.DetailedInstrumentation);

          cache.GetOrCreateTable<int>("A").Options.DefaultMaxAgeSec = 197;
          Aver.AreEqual(197,       cache.GetOrCreateTable<int>("A").Options.DefaultMaxAgeSec);
          Aver.AreEqual(145,       cache.TableOptions["a"].DefaultMaxAgeSec);

          topt = cache.GetOrCreateTable<int>("b").Options;
          Aver.AreEqual(256000, topt.MaximumCapacity);
          Aver.AreEqual(false,  topt.DetailedInstrumentation);
        }
      }



      [Run(TRUN.BASE, null, 8)]
      public void T180_GetOrPut()
      {
        using (var cache = PileCacheTestCore.MakeCache())
        {
          var tA = cache.GetOrCreateTable<int>("A");

          tA.Put(1, "value 1");
          tA.Put(122, "value 122");

          PutResult? pResult;
          var v = tA.GetOrPut(2, (t, k, _) => "value "+k.ToString(), null, out pResult);
          Aver.AreObjectsEqual( "value 2", v);
          Aver.IsTrue( pResult.HasValue );
          Aver.IsTrue( PutResult.Inserted == pResult.Value);

          Aver.AreEqual(3, tA.Count);
          tA.Put(1, "value 1");
          tA.Put(2, "value 2");
          tA.Put(122, "value 122");


          v = tA.GetOrPut(1, (t, k, _) => "value "+k.ToString(), null, out pResult);
          Aver.AreObjectsEqual( "value 1", v);
          Aver.IsFalse( pResult.HasValue );

          Aver.AreEqual(3, tA.Count);
          tA.Put(1, "value 1");
          tA.Put(2, "value 2");
          tA.Put(122, "value 122");

          v = tA.GetOrPut(777, (t, k, _) => "value "+k.ToString(), null, out pResult);
          Aver.AreObjectsEqual( "value 777", v);
          Aver.IsTrue( pResult.HasValue );
          Aver.IsTrue( PutResult.Inserted == pResult.Value);


          Aver.AreEqual(4, tA.Count);
          tA.Put(1, "value 1");
          tA.Put(2, "value 2");
          tA.Put(122, "value 122");
          tA.Put(777, "value 777");

          pResult = tA.Put(2, "mod value 2");
          Aver.IsTrue( PutResult.Replaced == pResult.Value);


          Aver.AreEqual(4, tA.Count);
          tA.Put(1, "value 1");
          tA.Put(2, "mod value 2");
          tA.Put(122, "value 122");
          tA.Put(777, "value 777");
        }
      }

      [Run(TRUN.BASE, null, 100, "cnt=100000  tbls=1")]
      [Run("cnt=100000  tbls=16")]
      [Run("cnt=100000  tbls=512")]
      public void T190_FID_PutGetCorrectness(int cnt, int tbls)
      {
        PileCacheTestCore.FID_PutGetCorrectness(cnt, tbls);
      }

      [Run(TRUN.BASE, null, 100, "workers=16  tables=7   putCount=2500   durationSec=4")]
      [Run("workers=5   tables=7   putCount=100    durationSec=2")]

      [Run("workers=5   tables=20  putCount=50000  durationSec=2")]
      [Run("workers=16  tables=20  putCount=15000  durationSec=4")]
      public void T9000000_ParalellGetPutRemove(int workers, int tables, int putCount, int durationSec)
      {
        PileCacheTestCore.ParalellGetPutRemove(workers, tables, putCount, durationSec);
      }
  }
}
