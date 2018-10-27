/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Azos.Scripting;

using Azos.ApplicationModel.Pile;
using Azos.DataAccess.Distributed;
using Azos.UTest.AppModel.Pile;

namespace Azos.Tests.Integration.AppModel.Pile
{
  [Runnable]
  public class PileTest
  {
      [Run]
      public void SegmentAddedDeleted()
      {
        using (var pile = new DefaultPile() { SegmentSize = PileCacheTestCore.SEG_SIZE})
        {
          pile.Start();

          var ipile = pile as IPile;

          var pp = PilePointer.Invalid;
          for(ulong i=0; i<PileCacheTestCore.SEG_SIZE && ipile.SegmentCount < 2; i++)
          {
            var ch = ChargeRow.MakeFake(new GDID(1, i));
            pp = ipile.Put(ch);
          }

          Aver.AreEqual(2, ipile.SegmentCount);

          ipile.Delete(pp);

          Aver.AreEqual(1, ipile.SegmentCount);
        }
      }


      [Run]
      public void Parallel_PutGet()
      {
        const int CNT = 1000000;
        var tuples = new Tuple<PilePointer, ChargeRow>[CNT];

        using (var pile = new DefaultPile())
        {
          pile.Start();

          var ipile = pile as IPile;

          Parallel.For(0, CNT, i => {
            var ch = ChargeRow.MakeFake(new GDID(0, (ulong)i));
            var pp = ipile.Put(ch);
            tuples[i] = new Tuple<PilePointer,ChargeRow>(pp, ch);
          });

          Aver.AreEqual(CNT, ipile.ObjectCount);

          Parallel.ForEach(tuples, t => {
            Aver.AreObjectsEqual(t.Item2, ipile.Get(t.Item1));
          });
        }
      }

      [Run]
      public void Parallel_PutDeleteGet_Checkerboard()
      {
        const int CNT = 1002030;//1000203;
        var tuples = new Tuple<PilePointer, ChargeRow>[CNT];

        using (var pile = new DefaultPile())
        {
          pile.Start();

          var ipile = pile as IPile;

          Parallel.For(0, CNT, i => {
            var ch = ChargeRow.MakeFake(new GDID(0, (ulong)i));
            var pp = ipile.Put(ch);
            tuples[i] = new Tuple<PilePointer,ChargeRow>(pp, ch);
          });

          Aver.AreEqual(CNT, ipile.ObjectCount);

          Parallel.For(0, CNT, i => {
            if (i % 3 != 0)
              ipile.Delete(tuples[i].Item1);
          });

          Aver.AreEqual(CNT/3, ipile.ObjectCount);

          var deletedHits = new ConcurrentDictionary<int, int>();

          Parallel.For(0, CNT, i => {
            if (i % 3 != 0)
            {
              try
              {
                deletedHits.AddOrUpdate(System.Threading.Thread.CurrentThread.ManagedThreadId, 1, (threadId, val) => val + 1);
                ipile.Get(tuples[i].Item1);
                Aver.Fail("Object is deleted but its pointer doesn't throw exception!");
              }
              catch (PileAccessViolationException) {}
            }
            else
            {
              var ch = ipile.Get(tuples[i].Item1);
              Aver.AreObjectsEqual(ch, tuples[i].Item2);
            }
          });

          foreach (var kvp in deletedHits)
          {
            Console.WriteLine("Thread '{0}' {1:n0} times accessed deleted pointer", kvp.Key, kvp.Value);
          }
        }
      }

      [Run("payloadSize=128    freeChunkSizes='64,128,144,152,170,180,190,200,250,512,1024,2000,3000,4000,5000,6000'")]
      [Run("payloadSize=32000  freeChunkSizes='64,88,128,134,170,180,190,200,250,512,1024,2000,3000,4000,5000,32000'")]
      public void PileSmallObjects(int payloadSize, params int[] freeChunkSizes)
      {
        using (var pile = new DefaultPile() { SegmentSize = PileCacheTestCore.SEG_SIZE, AllocMode = AllocationMode.ReuseSpace })
        {
          pile.FreeChunkSizes = freeChunkSizes;

          pile.Start();

          var pps = new List<PilePointer>();
          while (pile.SegmentCount < 2)
            pps.Add(pile.Put(generatePayload(payloadSize)));

          pile.Delete(pps.Last());
          pps.RemoveAt(pps.Count-1);

          Console.WriteLine("Just removed the last added payload and segment should be 1 now, real segment count is {0}", pile.SegmentCount);
          Aver.AreEqual(1, pile.SegmentCount);

          var objectsInFirstSegment = pps.Count;

          Console.WriteLine("put {0:N0} objects in first segment, pile.ObjectCount {1:N0}", objectsInFirstSegment, pile.ObjectCount);

          var deletedObjectCount = 0;
          for (int i = 0; i < pps.Count; i+=2, deletedObjectCount++)
            Aver.IsTrue(pile.Delete(pps[i]));

          var objectsInFirstSegmentAfterDelete = pile.ObjectCount;
          Console.WriteLine("Deleted {0:N0} objects from 1st segment, pile.ObjectCount {1:N0}", deletedObjectCount, pile.ObjectCount);
          Aver.AreEqual(objectsInFirstSegment / 2 , objectsInFirstSegmentAfterDelete);
          Console.WriteLine("---------------------------------------------------------");


          var crawlStatus = pile.Crawl(false);
          Console.WriteLine("crawl: {0}", crawlStatus);

          var pps1 = new List<PilePointer>();
          var c = 0;
          while (pile.SegmentCount < 3)
          {
            pps1.Add(pile.Put(generatePayload(payloadSize)));
            if (c%20000 ==0 )pile.Crawl(true); //we do crawl because otherwise the 25000 free index xlots get exhausted AND
            c++;                               //this unit tests does not run long enough to cause Crawl within allocator (5+ seconds)
          }                                    //so we induce Crawl by hand to rebiild indexes

          pile.Delete(pps1.Last());
          pps1.RemoveAt(pps1.Count-1);

          Console.WriteLine("Again just removed the last added payload and segment should be 2 now, real segment count is {0}", pile.SegmentCount);
          Aver.AreEqual(2, pile.SegmentCount);

          var objectsInSecondRound = pps1.Count;

          Console.WriteLine("Put {0:N0} objects in second round, pile.ObjectCount {1:N0}", objectsInSecondRound, pile.ObjectCount);

          Aver.AreWithin( objectsInFirstSegment + objectsInFirstSegmentAfterDelete, objectsInSecondRound, 2d, "#1");

          Aver.AreEqual(objectsInFirstSegmentAfterDelete + objectsInSecondRound, pile.ObjectCount, "#2");
        }
      }

      [Run("payloadSize=200")]
      public void PileDeleteInLastSegment(int payloadSize)
      {
        using (var pile = new DefaultPile() { SegmentSize = PileCacheTestCore.SEG_SIZE})
        {
          pile.Start();

          var pps = new List<PilePointer>();
          while (pile.SegmentCount < 2)
            pps.Add(pile.Put(generatePayload(payloadSize)));

          pile.Delete(pps.Last());
          pps.RemoveAt(pps.Count-1);

          Console.WriteLine("segment count: {0}, segment total count: {1}", pile.SegmentCount, pile.SegmentTotalCount);

          Aver.AreEqual(1, pile.SegmentCount);
          Aver.AreEqual(1, pile.SegmentTotalCount);

        }
      }

      [Run("payloadSize=200")]
      public void PileDeleteInMiddleSegment(int payloadSize)
      {
        using (var pile = new DefaultPile() { SegmentSize = PileCacheTestCore.SEG_SIZE})
        {
          pile.Start();

          var pps = new List<PilePointer>();
          while (pile.SegmentCount < 4)
            pps.Add(pile.Put(generatePayload(payloadSize)));

          pile.Delete(pps.Last());
          pps.RemoveAt(pps.Count-1);

          var objectsInsegmentCount = pps.Count / 3;

          Console.WriteLine("{0:N0} object in pile, {1:N0} object per segment", pile.ObjectCount, objectsInsegmentCount);

          Aver.AreEqual(3, pile.SegmentCount);
          Aver.AreEqual(3, pile.SegmentTotalCount);

          for (int i = objectsInsegmentCount; i < 2 * objectsInsegmentCount; i++)
          {
            pile.Delete(pps[i]);
          }

          Console.WriteLine("{0:N0} object in pile, {1:N0} segments, {2:N0} segments total", pile.ObjectCount, pile.SegmentCount, pile.SegmentTotalCount);

          Aver.AreEqual(2, pile.SegmentCount);
          Aver.AreEqual(3, pile.SegmentTotalCount);
        }
      }

      [Run("fromSize=32  toSize=16000  fromObjCount=300  toObjCount=500")]
      public void PutGetDelete_Sequential(int fromSize, int toSize, int fromObjCount, int toObjCount)
      {
        PileCacheTestCore.PutGetDelete_Sequential(fromSize, toSize, fromObjCount, toObjCount);
      }

      [Run("fromSize=32  toSize=3200   fromObjCount=1  toObjCount=20  taskCount=4")]
      [Run("fromSize=32  toSize=12800  fromObjCount=1  toObjCount=50  taskCount=8")]
      public void PutGetDelete_Parallel(int fromSize, int toSize, int fromObjCount, int toObjCount, int taskCount)
      {
        PileCacheTestCore.PutGetDelete_Parallel(fromSize, toSize, fromObjCount, toObjCount, taskCount);
      }

              private object generatePayload(int size = 8)
              {
                return new byte[size];
              }
  }
}
