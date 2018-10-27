/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
  
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using Azos.Pile;
using Azos.Conf;


namespace Azos.Tests.Unit.Pile
{
    public static class PileCacheTestCore
    {
        public const int SEG_SIZE = DefaultPile.SEG_SIZE_MIN;

        #region Nested

        private class dummy
        {
            public byte[] bin;
        }

        #endregion

        #region Cache

        public static void KeyInt_ManyPutGet(int cnt)
        {
            using (var cache = MakeCache())
            {
                var tA = cache.GetOrCreateTable<int>("A");

                for (var i = 0; i < cnt; i++)
                    tA.Put(i, "value-" + i.ToString());

                for (var i = 0; i < cnt; i++)
                    Aver.AreObjectsEqual("value-" + i.ToString(), tA.Get(i));
            }
        }

        public static void KeyGDID_ManyPutGet(int cnt)
        {
            using (var cache = MakeCache())
            {
                var tA = cache.GetOrCreateTable<GDID>("A");

                for (var i = 0; i < cnt; i++)
                    tA.Put(new GDID(0, (ulong)i), "value-" + i.ToString());

                for (var i = 0; i < cnt; i++)
                    Aver.AreObjectsEqual("value-" + i.ToString(), tA.Get(new GDID(0, (ulong)i)));
            }
        }

        public static void KeyString_ManyPutGet(int cnt)
        {
            using (var cache = MakeCache())
            {
                cache.TableOptions.Register(new TableOptions("A")
                {
                    InitialCapacity = cnt//*2,
                                         //   MaximumCapacity = cnt*2
                });

                var tA = cache.GetOrCreateTable<string>("A", StringComparer.Ordinal);

                for (var i = 0; i < cnt; i++)
                {
                    tA.Put(i.ToString(), "value-" + i.ToString());
                }

                var hit = 0;
                var miss = 0;
                for (var i = 0; i < cnt; i++)
                {
                    var d = tA.Get(i.ToString());
                    if (d == null) miss++;
                    else
                    {
                        hit++;
                        Aver.AreObjectsEqual("value-" + i.ToString(), d);
                    }
                }
                Console.WriteLine("Did {0:n0} ->  hits: {1:n0} ({2:n0}%)  ;  misses: {3:n0} ({4:n0}%)", cnt, hit, (hit / (double)cnt) * 100d, miss, (miss / (double)cnt) * 100d);

                Aver.IsTrue(hit > miss);
                Aver.IsTrue((miss / (double)cnt) <= 0.1d);//misses <=10%
            }
        }

        public static void ResizeTable(int cnt, int rec, int payload)
        {
            using (var cache = MakeCache())
            {
                var tA = cache.GetOrCreateTable<int>("A");

                Parallel.For(0, cnt,
                (c) =>
                {
                    var put = App.Random.NextScaledRandomInteger(1, rec);
                    for (var i = 0; i < put; i++)
                        tA.Put(i, new byte[App.Random.NextScaledRandomInteger(1, payload)]);

                    var remove = App.Random.NextScaledRandomInteger(0, rec);
                    for (var i = 0; i < remove; i++)
                        tA.Remove(i);

                    Console.WriteLine("{0}% done", (100 * c) / cnt);
                });

                for (var i = 0; i < rec; i++)
                    tA.Remove(i);

                Aver.AreEqual(0, tA.Count);
                Aver.AreEqual(0, cache.Pile.UtilizedBytes);
                Aver.AreEqual(0, cache.Pile.ObjectCount);
            }
        }

        public static void FID_PutGetCorrectness(int cnt, int tbls)
        {
            var sw = new System.Diagnostics.Stopwatch();
            var dicts = new ConcurrentDictionary<FID, string>[tbls];
            for (var i = 0; i < dicts.Length; i++) dicts[i] = new ConcurrentDictionary<FID, string>();
            var notInserted = 0;
            using (var cache = MakeCache())
            {
                sw.Start();
                Parallel.For(0, cnt, (i) =>
                {
                    var t = i % tbls;
                    var tbl = cache.GetOrCreateTable<FID>(t.ToString());

                    var key = FID.Generate();
                    var data = Azos.Parsing.NaturalTextGenerator.Generate(0);

                    var pr = tbl.Put(key, data);
                    dicts[t].TryAdd(key, data);
                    if (pr != PutResult.Inserted)
                        Interlocked.Increment(ref notInserted);
                });

                var elapsed = sw.ElapsedMilliseconds;
                var inserted = cnt - notInserted;
                Console.WriteLine("Population of {0:n0} in {1:n0} msec at {2:n0}ops/sec", cnt, elapsed, cnt / (elapsed / 1000d));
                Console.WriteLine("  inserted: {0:n0} ({1:n0}%)", inserted, (int)(100 * (inserted / (double)cnt)));
                Console.WriteLine("  not-inserted: {0:n0} ({1:n0}%)", notInserted, (int)(100 * (notInserted) / (double)cnt));
                sw.Restart();

                var found = 0;
                var notfound = 0;

                for (var i = 0; i < tbls; i++)
                {
                    var tbl = cache.GetOrCreateTable<FID>(i.ToString());
                    var dict = dicts[i];

                    Parallel.ForEach(dict, (kvp) =>
                    {
                        var data = tbl.Get(kvp.Key);
                        if (data != null)
                        {
                            Aver.AreObjectsEqual(data, kvp.Value);
                            Interlocked.Increment(ref found);
                        }
                        else
                            Interlocked.Increment(ref notfound);
                    });
                }

                elapsed = sw.ElapsedMilliseconds;
                var totalGot = found + notfound;
                Console.WriteLine("Got of {0:n0} in {1:n0} msec at {2:n0}ops/sec", totalGot, elapsed, totalGot / (elapsed / 1000d));
                Console.WriteLine("  found: {0:n0} ({1:n0}%)", found, (int)(100 * (found / (double)totalGot)));
                Console.WriteLine("  not-found: {0:n0} ({1:n0}%)", notfound, (int)(100 * (notfound) / (double)totalGot));

                Aver.IsTrue((found / (double)inserted) > 0.9d);
            }//using cache
        }

        public static void ParalellGetPutRemove(int workers, int tables, int putCount, int durationSec)
        {
            var start = DateTime.UtcNow;

            using (var cache = MakeCache())
            {
                var tasks = new Task[workers];

                var totalGet = 0;
                var totalPut = 0;
                var totalRem = 0;

                var getFound = 0;
                var putInsert = 0;
                var removed = 0;

                for (var i = 0; i < workers; i++)
                    tasks[i] =
                     Task.Factory.StartNew(
                      () =>
                      {
                          while (true)
                          {
                              var now = DateTime.UtcNow;
                              if ((now - start).TotalSeconds >= durationSec) return;

                              var t = Azos.App.Random.NextScaledRandomInteger(0, tables);
                              var tbl = cache.GetOrCreateTable<string>("tbl_" + t);

                              var get = Azos.App.Random.NextScaledRandomInteger(putCount * 2, putCount * 4);
                              var put = Azos.App.Random.NextScaledRandomInteger(putCount / 2, putCount);
                              var remove = Azos.App.Random.NextScaledRandomInteger(0, putCount);

                              Interlocked.Add(ref totalGet, get);
                              Interlocked.Add(ref totalPut, put);
                              Interlocked.Add(ref totalRem, remove);

                              for (var j = 0; j < get; j++)
                                  if (null != tbl.Get(Azos.App.Random.NextScaledRandomInteger(0, 1000000).ToString())) Interlocked.Increment(ref getFound);

                              for (var j = 0; j < put; j++)
                                  if (PutResult.Inserted == tbl.Put(
                                                            Azos.App.Random.NextScaledRandomInteger(0, 1000000).ToString(),
                                                            Azos.Parsing.NaturalTextGenerator.Generate()
                                                          )) Interlocked.Increment(ref putInsert);

                              for (var j = 0; j < remove; j++)
                                  if (tbl.Remove(
                                          Azos.App.Random.NextScaledRandomInteger(0, 1000000).ToString()
                                         )) Interlocked.Increment(ref removed);


                          }
                      }
                     , TaskCreationOptions.LongRunning);

                Task.WaitAll(tasks);

                Console.WriteLine("{0} workers, {1} tables, {2} put, {3} sec duration", workers, tables, putCount, durationSec);
                Console.WriteLine("-----------------------------------------------------------");

                Console.WriteLine("Total Gets: {0:n0}, found {1:n0}", totalGet, getFound);
                Console.WriteLine("Total Puts: {0:n0}, inserted {1:n0}", totalPut, putInsert);
                Console.WriteLine("Total Removes: {0:n0}, removed {1:n0}", totalRem, removed);

                Console.WriteLine("Pile utilized bytes: {0:n0}", cache.Pile.UtilizedBytes);
                Console.WriteLine("Pile object count: {0:n0}", cache.Pile.ObjectCount);

                cache.PurgeAll();

                Aver.AreEqual(0, cache.Count);
                Aver.AreEqual(0, cache.Pile.UtilizedBytes);
                Aver.AreEqual(0, cache.Pile.ObjectCount);


                Console.WriteLine();
            }
        }

        #endregion

        #region Pile

        public static void VarSizes_Checkboard(bool isParallel, int cnt, int minSz, int maxSz, bool speed)
        {
            using (var pile = new DefaultPile())
            {
                pile.Start();
                pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;

                var tasks = new List<Task>();
                for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
                    tasks.Add(
                           Task.Run(() =>
                          {
                              var dict = new Dictionary<PilePointer, dummy>();
                              var priorOdd = PilePointer.Invalid;
                              for (var i = 0; i < cnt; i++)
                              {
                                  var even = (i & 0x01) == 0;
                                  var data = new dummy { bin = new byte[12 + Azos.App.Random.NextScaledRandomInteger(minSz, maxSz)] };
                                  data.bin.WriteBEInt32(0, App.Random.NextRandomInteger);
                                  data.bin.WriteBEInt32(data.bin.Length - 4, App.Random.NextRandomInteger);
                                  var ptr = pile.Put(data);
                                  Aver.IsTrue(ptr.Valid);

                                  if (even)
                                      dict.Add(ptr, data);
                                  else
                                  {
                                      if (priorOdd.Valid)
                                          Aver.IsTrue(pile.Delete(priorOdd));
                                      priorOdd = ptr;
                                  }

                                  if (i % 1000 == 0)
                                      Console.WriteLine("Thread{0} did {1}; allocated {2} bytes, utilized {3} bytes by {4} objects {5} bytes/obj. ",
                                               Thread.CurrentThread.ManagedThreadId,
                                                i,
                                                pile.AllocatedMemoryBytes,
                                                pile.UtilizedBytes,
                                                pile.ObjectCount,
                                                pile.UtilizedBytes / pile.ObjectCount);

                              }
                              Console.WriteLine("Thread {0} Population done, now checking the buffers... {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now);

                              foreach (var entry in dict)
                                  Aver.IsTrue(Azos.IOMiscUtils.MemBufferEquals(entry.Value.bin, (pile.Get(entry.Key) as dummy).bin));

                              Console.WriteLine("Thread {0} DONE. {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now);
                          })
                     );//add

                Task.WaitAll(tasks.ToArray());
            }
        }

        public static void VarSizes_Increasing_Random(bool isParallel, int cnt, int minSz, int maxSz, bool speed, bool rnd)
        {
            using (var pile = new DefaultPile())
            {
                pile.Start();
                pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;

                var sw = Stopwatch.StartNew();
                var tasks = new List<Task>();
                for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
                    tasks.Add(
                           Task.Run(() =>
                          {
                              var dict = new Dictionary<PilePointer, dummy>();
                              var lst = new List<PilePointer>();
                              var priorOdd = PilePointer.Invalid;
                              for (var i = 0; i < cnt; i++)
                              {
                                  var buf = new byte[12 +
                                            minSz +
                                            (
                                            rnd
                                             ? (Azos.App.Random.NextScaledRandomInteger(0, (int)(maxSz * (i / (double)cnt))))
                                             : (int)(maxSz * (i / (double)cnt))
                                            )
                                           ];
                                  var data = new dummy { bin = buf };
                                  data.bin.WriteBEInt32(0, App.Random.NextRandomInteger);
                                  data.bin.WriteBEInt32(data.bin.Length - 4, App.Random.NextRandomInteger);
                                  var ptr = pile.Put(data);
                                  Aver.IsTrue(ptr.Valid);

                                  dict.Add(ptr, data);
                                  lst.Add(ptr);

                                  if (i > cnt / 3)
                                  {
                                      if (App.Random.NextRandomInteger > 0)
                                      {
                                          var ri = App.Random.NextScaledRandomInteger(0, lst.Count - 1);
                                          var pp = lst[ri];
                                          if (!pp.Valid) continue;

                                          Aver.IsTrue(pile.Delete(pp));
                                          dict.Remove(pp);
                                          lst[ri] = PilePointer.Invalid;

                                      }
                                  }


                                  if (i % 1000 == 0)
                                      Console.WriteLine("Thread{0} did {1}; allocated {2} bytes, utilized {3} bytes by {4} objects {5} bytes/obj. ",
                                               Thread.CurrentThread.ManagedThreadId,
                                                i,
                                                pile.AllocatedMemoryBytes,
                                                pile.UtilizedBytes,
                                                pile.ObjectCount,
                                                pile.UtilizedBytes / pile.ObjectCount);

                              }
                              Console.WriteLine("Thread {0} Population done, now checking the buffers... {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now);

                              foreach (var entry in dict)
                                  Aver.IsTrue(Azos.IOMiscUtils.MemBufferEquals(entry.Value.bin, (pile.Get(entry.Key) as dummy).bin));

                              Console.WriteLine("Thread {0} DONE. {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now);
                          })
                     );//add

                Task.WaitAll(tasks.ToArray());
                var el = sw.ElapsedMilliseconds;
                var gt = cnt * tasks.Count;
                Console.WriteLine("Total objects: {0:n0} in {1:n0} ms at {2:n0} obj/sec".Args(gt, el, gt / (el / 1000d)));
            }
        }

        public static void PutGetDelete_Parallel(int fromSize, int toSize, int fromObjCount, int toObjCount, int taskCount)
        {
            Console.WriteLine("test will take about 1 minute");

            int objectsPut = 0, objectsDeleted = 0;//, objectGot = 0;

            using (var pile = new DefaultPile() { SegmentSize = SEG_SIZE })
            {
                pile.Start();

                Parallel.For(0, taskCount, _ =>
                  {
                      var startTime = DateTime.Now;
                      var objects = new Dictionary<PilePointer, byte[]>();
                      var objectCount = 0;
                      var objectsSumSize = 0;

                      var dtStop = DateTime.Now.AddMinutes(.5);
                      while (dtStop >= DateTime.Now)
                      {
                          // insert routine
                          var insertCount = Azos.App.Random.NextScaledRandomInteger(fromObjCount, toObjCount);
                          for (int i = 0; i < insertCount; i++)
                          {
                              var payloadSize = Azos.App.Random.NextScaledRandomInteger(fromSize, toSize);
                              var payload = new byte[payloadSize];
                              payload[0] = (byte)Azos.App.Random.NextScaledRandomInteger(0, 255);
                              payload[payloadSize - 1] = (byte)Azos.App.Random.NextScaledRandomInteger(0, 255);
                              var pp = pile.Put(payload);
                              objects.Add(pp, payload);
                              objectCount++;
                              objectsSumSize += payloadSize;
                          }
                          Interlocked.Add(ref objectsPut, insertCount);

                          // get
                          if (objectCount > 0)
                          {
                              var getCount = App.Random.NextScaledRandomInteger(5 * fromObjCount, 5 * toObjCount);
                              for (int i = 0; i < getCount; i++)
                              {
                                  var objectIdx = App.Random.NextScaledRandomInteger(0, objectCount - 1);
                                  var obj = objects.ElementAt(objectIdx);
                                  var objPayloadFromPile = (byte[])pile.Get(obj.Key);
                                  Aver.AreEqual(obj.Value[0], objPayloadFromPile[0]);
                                  Aver.AreEqual(obj.Value[obj.Value.Length - 1], objPayloadFromPile[obj.Value.Length - 1]);
                                  Aver.AreEqual(obj.Value.Length, objPayloadFromPile.Length);
                              }
                          }

                          // delete
                          var deleteCount = App.Random.NextScaledRandomInteger(fromObjCount, toObjCount);
                          if (deleteCount > objectCount) deleteCount = objectCount;
                          for (int i = 0; i < deleteCount; i++)
                          {
                              if (objectCount == 0) break;

                              var objectIdx = App.Random.NextScaledRandomInteger(0, objectCount - 1);
                              var obj = objects.ElementAt(objectIdx);

                              Aver.IsTrue(pile.Delete(obj.Key));
                              objects.Remove(obj.Key);
                              objectCount--; objectsSumSize -= obj.Value.Length;
                          }
                          Interlocked.Add(ref objectsDeleted, deleteCount);
                      }
                  });
            }

            Console.WriteLine("put {0:N0}, deleted {1:N0} object", objectsPut, objectsDeleted);
        }

        public static void PutGetDelete_Sequential(int fromSize, int toSize, int fromObjCount, int toObjCount)
        {
            Console.WriteLine("test will take about 1 minute");

            var startTime = DateTime.Now;
            var objects = new Dictionary<PilePointer, byte[]>();
            var objectCount = 0;
            var objectsSumSize = 0;

            using (var pile = new DefaultPile() { SegmentSize = PileCacheTestCore.SEG_SIZE })
            {
                pile.Start();

                var dtStop = DateTime.Now.AddMinutes(1);
                while (dtStop >= DateTime.Now)
                {
                    // insert routine
                    var insertCount = Azos.App.Random.NextScaledRandomInteger(fromObjCount, toObjCount);
                    for (int i = 0; i < insertCount; i++)
                    {
                        var payloadSize = Azos.App.Random.NextScaledRandomInteger(fromSize, toSize);
                        var payload = new byte[payloadSize];
                        payload[0] = (byte)Azos.App.Random.NextScaledRandomInteger(0, 255);
                        payload[payloadSize - 1] = (byte)Azos.App.Random.NextScaledRandomInteger(0, 255);
                        var pp = pile.Put(payload);
                        objects.Add(pp, payload);
                        objectCount++;
                        objectsSumSize += payloadSize;
                    }

                    // get
                    if (objectCount > 0)
                    {
                        var getCount = App.Random.NextScaledRandomInteger(5 * fromObjCount, 5 * toObjCount);
                        for (int i = 0; i < getCount; i++)
                        {
                            var objectIdx = App.Random.NextScaledRandomInteger(0, objectCount - 1);
                            var obj = objects.ElementAt(objectIdx);
                            var objPayloadFromPile = (byte[])pile.Get(obj.Key);
                            Aver.AreEqual(obj.Value[0], objPayloadFromPile[0]);
                            Aver.AreEqual(obj.Value[obj.Value.Length - 1], objPayloadFromPile[obj.Value.Length - 1]);
                            Aver.AreEqual(obj.Value.Length, objPayloadFromPile.Length);
                        }
                    }

                    // delete
                    var deleteCount = App.Random.NextScaledRandomInteger(fromObjCount, toObjCount);
                    for (int i = 0; i < deleteCount; i++)
                    {
                        if (objectCount == 0) break;

                        var objectIdx = App.Random.NextScaledRandomInteger(0, objectCount - 1);
                        var obj = objects.ElementAt(objectIdx);

                        Aver.IsTrue(pile.Delete(obj.Key));
                        objects.Remove(obj.Key);
                        objectCount--; objectsSumSize -= obj.Value.Length;
                    }
                }
            }
        }


        #endregion

        public static LocalCache MakeCache(IConfigSectionNode conf = null)
        {
          var cache = new LocalCache();
          cache.Pile = new DefaultPile(cache);
          cache.Configure(conf);
          cache.Start();
          return cache;
        }

        public static LocalCache MakeDurableCache(IConfigSectionNode conf = null)
        {
          var result = MakeCache(conf);
          result.DefaultTableOptions = new TableOptions("*")
          {
            CollisionMode = CollisionMode.Durable
          };
          return result;
        }
    }
}
