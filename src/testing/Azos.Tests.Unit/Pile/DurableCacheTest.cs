/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Azos.Pile;
using Azos.Scripting;


namespace Azos.Tests.Unit.Pile
{
  [Runnable]
  public class DurableCacheTest
  {
      [Run("count=    100")]
      [Run("count=   1000")]
      [Run("count=  25000")]
      [Run("count= 250000")]
      [Run("count=1000000")]
      public void AddAndRetainAll(int COUNT)
      {
        using (var cache = PileCacheTestCore.MakeDurableCache())
        {
          var tA = cache.GetOrCreateTable<Guid>("A");
          var tB = cache.GetOrCreateTable<string>("B");

          Aver.AreObjectsEqual( CollisionMode.Durable, tA.CollisionMode );
          Aver.AreObjectsEqual( CollisionMode.Durable, tB.CollisionMode );


          var dict = new Dictionary<Guid, string>();

          for(var i=0; i<COUNT; i++)
          {
            var key = Guid.NewGuid();
            var value = "this is a value of "+key;
            dict.Add(key, value);

            var pr = tA.Put(key, value);
            Aver.IsTrue( PutResult.Inserted == pr);

            pr = tB.Put(key.ToString(), value);
            Aver.IsTrue( PutResult.Inserted == pr);
          }

          Aver.AreEqual(COUNT, dict.Count);
          Aver.AreEqual(COUNT, tA.Count);
          Aver.AreEqual(COUNT, tB.Count);

          foreach(var kvp in  dict)
          {
            Aver.IsTrue  ( tA.ContainsKey(kvp.Key) );
            Aver.AreEqual( kvp.Value, tA.Get(kvp.Key) as string);
            Aver.AreEqual( kvp.Value, tB.Get(kvp.Key.ToString()) as string);
          }
        }
      }


      [Run("count=2500    durationSec=30  delMargin=0.25d")]
      [Run("count=2500    durationSec=30  delMargin=0.75d")]
      [Run("count=125000  durationSec=60  delMargin=0.25d")]
      [Run("count=125000  durationSec=60  delMargin=0.75d")]
      public void AddAndRemove(int COUNT, int durationSec, double delMargin)
      {
        using (var cache = PileCacheTestCore.MakeDurableCache())
        {
           var tA = cache.GetOrCreateTable<Guid>("A");
           var dict = new Dictionary<Guid, string>();

           var sw = System.Diagnostics.Stopwatch.StartNew();
           var ls = 0L;
           while(sw.Elapsed.TotalSeconds < durationSec)
           {
              while(dict.Count < COUNT)
              {
                var key = Guid.NewGuid();
                var value = "value is "+key;
                dict.Add(key, value);

                var pr = tA.Put(key, value);
                Aver.AreObjectsEqual(PutResult.Inserted, pr);
              }

              var todelete = App.Random.NextScaledRandomInteger(0, (int)(COUNT * delMargin));
              for(var i=0; i<todelete; i++)
              {
                var key = dict.Keys.Skip(App.Random.NextScaledRandomInteger(0, dict.Count-2)).First();
                Aver.IsTrue( dict.Remove(key) );
                Aver.IsTrue( tA.Remove(key) );
                Aver.AreEqual( dict.Count, tA.Count);
              }

              if (sw.ElapsedMilliseconds-ls>1000)
              {
               Console.WriteLine("Elapsed: {0:n0} of {1:n0};   Removed {2:n0}, count is {3:n0}", sw.Elapsed.TotalSeconds, durationSec,  todelete, tA.Count);
               ls = sw.ElapsedMilliseconds;
              }

              Aver.AreEqual( dict.Count, tA.Count);
              foreach(var kvp in  dict)
              {
                Aver.AreEqual( kvp.Value, tA.Get(kvp.Key) as string);
              }
           }

           //delete everything
           foreach(var kvp in dict)
           {
             Aver.IsTrue  ( tA.ContainsKey(kvp.Key) );
             Aver.IsTrue  ( tA.Remove( kvp.Key ) );
             Aver.IsFalse ( tA.ContainsKey(kvp.Key) );
           }
           Aver.AreEqual(0, cache.Pile.ObjectCount);//Pile is empty

        }
      }

      [Run("count=   500")]
      [Run("count= 25000")]
      [Run("count=125000")]
      public void SweepOld(int COUNT)
      {
        using (var cache = PileCacheTestCore.MakeDurableCache())
        {
           var tA = cache.GetOrCreateTable<long>("A");

           for(var i=0; i<COUNT; i++)
           {
              var key = i * 7;
              var value = "value of "+i.ToString();
              var odd = (i & 1) != 0;
              var pr = tA.Put(key, value, maxAgeSec: odd ? 1 : 0 );
              Aver.AreObjectEqualTest(PutResult.Inserted, pr);
           }

           Aver.AreEqual(COUNT, tA.Count);
           Thread.Sleep(15000);

           for(var i=0; i<COUNT; i++)
           {
              var key = i * 7;
              var value = "value of "+i.ToString();
              var odd = (i & 1) != 0;
              var got = tA.Get(key) as string;

              if (odd)
               Aver.IsNull( got );
              else
               Aver.AreEqual(value, got);
           }

           Aver.AreEqual(COUNT / 2, cache.Pile.ObjectCount);//Half of objects died
        }
      }

      [Run("count=125000")]
      public void SweepOldRejuvenateSome(int COUNT)
      {
        using (var cache = PileCacheTestCore.MakeDurableCache())
        {
           var tA = cache.GetOrCreateTable<long>("A");

           for(var i=0; i<COUNT; i++)
           {
              var key = i * 7;
              var value = "value of "+i.ToString();
              var pr = tA.Put(key, value, maxAgeSec: 20 );
              Aver.AreObjectEqualTest(PutResult.Inserted, pr);
           }

           Aver.AreEqual(COUNT, tA.Count);

           const int WAIT = 45;
           for(var j=0; j<WAIT;j++)
           {
             Console.WriteLine("Wait loop iteration #{0} of {1}", j, WAIT);
             for(var i=0; i<COUNT; i++)
             {
               var odd = (i & 1) != 0;
               if (odd) continue;//do not rejuvenate ODD
               var key = i * 7;
               Aver.IsTrue( tA.Rejuvenate(key) );
             }
             Thread.Sleep(1000);
           }

           for(var i=0; i<COUNT; i++)
           {
              var key = i * 7;
              var value = "value of "+i.ToString();
              var odd = (i & 1) != 0;
              var got = tA.Get(key) as string;

              if (odd)
               Aver.IsNull( got );
              else
               Aver.AreEqual(value, got);
           }

           Aver.AreEqual(COUNT / 2, cache.Pile.ObjectCount);//Half of objects died
        }
      }




      [Run("count=   100")]
      [Run("count=  1000")]
      [Run("count= 25000")]
      [Run("count=125000")]
      public void AddThenEnumerateAll(int COUNT)
      {
        using (var cache = PileCacheTestCore.MakeDurableCache())
        {
          var tA = cache.GetOrCreateTable<Guid>("A");

          var dict = new Dictionary<Guid, string>();

          for(var i=0; i<COUNT; i++)
          {
            var key = Guid.NewGuid();
            var value = "this is a value of "+key;
            dict.Add(key, value);

            var pr = tA.Put(key, value);
            Aver.IsTrue( PutResult.Inserted == pr);

          }

          Aver.AreEqual(COUNT, dict.Count);
          Aver.AreEqual(COUNT, tA.Count);

          foreach(var kvp in  dict)
          {
            Aver.IsTrue  ( tA.ContainsKey(kvp.Key) );
            Aver.AreEqual( kvp.Value, tA.Get(kvp.Key) as string);
          }

          var hs = new HashSet<Guid>();

          var cnt = 0;
          foreach(var entry in tA.AsEnumerable(false))//no materialization
          {
            var value = dict[entry.Key];

            Aver.IsTrue( hs.Add(entry.Key) );//ensure no duplication in enumerator

            Aver.IsNull(entry.Value);
            Aver.AreEqual(value, tA.Get(entry.Key) as string);
            cnt++;
          }

          Aver.AreEqual(dict.Count, cnt);//ensure the same# of elements

          hs = new HashSet<Guid>();
          cnt = 0;
          foreach(var entry in tA.AsEnumerable(true))//with materialization
          {
            var value = dict[entry.Key];

            Aver.IsTrue( hs.Add(entry.Key) );//ensure no duplication in enumerator

            Aver.AreEqual(value, entry.Value as string);
            cnt++;
          }
          Aver.AreEqual(dict.Count, cnt);//ensure the same# of elements

          //------- Manual enumerator -------
          var enumerator = tA.AsEnumerable(false).GetEnumerator();
          cnt = 0;
          hs = new HashSet<Guid>();
          while(enumerator.MoveNext())
          {

            var value = dict[enumerator.Current.Key];

            Aver.IsTrue( hs.Add(enumerator.Current.Key) );//ensure no duplication in enumerator

            Aver.IsNull(enumerator.Current.Value);
            Aver.AreEqual(value, tA.Get(enumerator.Current.Key) as string);
            cnt++;
          }

          Aver.AreEqual(dict.Count, cnt);//ensure the same# of elements

          //Reset enumerator
          enumerator.Reset();//<-----------------------------
          cnt = 0;
          hs = new HashSet<Guid>();
          while(enumerator.MoveNext())
          {

            var value = dict[enumerator.Current.Key];

            Aver.IsTrue( hs.Add(enumerator.Current.Key) );//ensure no duplication in enumerator

            Aver.IsNull(enumerator.Current.Value);
            Aver.AreEqual(value, tA.Get(enumerator.Current.Key) as string);
            cnt++;
          }

          Aver.AreEqual(dict.Count, cnt);//ensure the same# of elements
        }
      }
  }
}
