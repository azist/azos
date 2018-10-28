/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Azos.Pile;
using Azos.Data;
using Azos.Scripting;

namespace Azos.Tests.Integration.Pile
{
  public class PileFragmentationTest : IRunHook
  {
    bool IRunHook.Prologue(Runner runner, FID id, MethodInfo method, RunAttribute attr, ref object[] args)
    {
      GC.Collect();
      return false;
    }

    bool IRunHook.Epilogue(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
    {
      GC.Collect();
      return false;
    }

    [Run("cnt=100000  durationSec=30  speed=true   payloadSizeMin=2  payloadSizeMax=8000  deleteFreq=3   isParallel=true")]
    [Run("cnt=100000  durationSec=30  speed=false  payloadSizeMin=2  payloadSizeMax=200   deleteFreq=10  isParallel=true")]
    public static void Put_RandomDelete_ByteArray(int cnt, int durationSec, bool speed, int payloadSizeMin, int payloadSizeMax, int deleteFreq, bool isParallel)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var wlc = 0;
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) return;

                var dict = new Dictionary<int, CheckByteArray>();


                Console.WriteLine("Starting a batch of {0}".Args(cnt));
                for (int i = 0; i < cnt; i++)
                {
                  var payloadSize = App.Random.NextScaledRandomInteger(payloadSizeMin, payloadSizeMax);
                  var val = new byte[payloadSize];
                  val[0] = (byte)App.Random.NextRandomInteger;
                  val[payloadSize - 1] = (byte)App.Random.NextRandomInteger;

                  var ptr = pile.Put(val);

                  var element = new CheckByteArray(ptr, payloadSize - 1, val[0], val[payloadSize - 1]);
                  dict.Add(i, element);

                  if (dict.Count > 0 && i % deleteFreq == 0)
                  {
                    while (true)
                    {
                      var idx = i - App.Random.NextScaledRandomInteger(0, i);

                      CheckByteArray stored;
                      if (dict.TryGetValue(idx, out stored))
                      {
                        ptr = stored.Ptr;
                        pile.Delete(ptr);
                        dict.Remove(idx);
                        break;
                      }
                    }
                  }

                  if (dict.Count > 16 && App.Random.NextScaledRandomInteger(0, 100) > 98)
                  {
                    var toRead = App.Random.NextScaledRandomInteger(8, 64);
                    wlc++;
                    if (wlc % 125 == 0)
                      Console.WriteLine("Thread {0} is reading {1} elements, total {2}"
                        .Args(Thread.CurrentThread.ManagedThreadId, toRead, dict.Count));
                    for (var k = 0; k < toRead; k++)
                    {
                      var kvp = dict.Skip(App.Random.NextScaledRandomInteger(0, dict.Count - 1)).First();
                      var buf = pile.Get(kvp.Value.Ptr) as byte[];
                      Aver.AreEqual(kvp.Value.FirstByte, buf[0]);
                      Aver.AreEqual(kvp.Value.LastByte, buf[kvp.Value.IdxLast]);
                    }
                  }
                }

                Console.WriteLine("Thread {0} is doing final read of {1} elements".Args(Thread.CurrentThread.ManagedThreadId, dict.Count));
                foreach (var kvp in dict)
                {
                  var buf = pile.Get(kvp.Value.Ptr) as byte[];
                  Aver.AreEqual(kvp.Value.FirstByte, buf[0]);
                  Aver.AreEqual(kvp.Value.LastByte, buf[kvp.Value.IdxLast]);
                }
              }
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [Run("speed=true   durationSec=30  payloadSizeMin=2  payloadSizeMax=1000  deleteFreq=3  isParallel=true")]
    [Run("speed=false  durationSec=30  payloadSizeMin=2  payloadSizeMax=1000  deleteFreq=3  isParallel=true")]
    public static void DeleteOne_ByteArray(bool speed, int durationSec, int payloadSizeMin, int payloadSizeMax, int deleteFreq, bool isParallel)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var list = new List<CheckByteArray>();
              var i = 0;
              var wlc = 0;
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) break;

                var payloadSize = App.Random.NextScaledRandomInteger(payloadSizeMin, payloadSizeMax);
                var val = new byte[payloadSize];
                val[0] = (byte)App.Random.NextRandomInteger;
                val[payloadSize - 1] = (byte)App.Random.NextRandomInteger;

                var ptr = pile.Put(val);

                var element = new CheckByteArray(ptr, payloadSize - 1, val[0], val[payloadSize - 1]);
                list.Add(element);

                // delete ONE random element
                if (i > 0 && i % deleteFreq == 0)
                {
                  var idx = App.Random.NextScaledRandomInteger(0, list.Count - 1);
                  ptr = list[idx].Ptr;
                  pile.Delete(ptr);
                  list.RemoveAt(idx);
                }

                // get several random elements
                if (list.Count > 64 && App.Random.NextScaledRandomInteger(0, 100) > 98)
                {
                  var toRead = App.Random.NextScaledRandomInteger(8, 64);
                  wlc++;
                  if (wlc % 125 == 0)
                    Console.WriteLine("Thread {0} is reading {1} elements, total {2}"
                      .Args(Thread.CurrentThread.ManagedThreadId, toRead, list.Count));
                  for (var k = 0; k < toRead; k++)
                  {
                    element = list[App.Random.NextScaledRandomInteger(0, list.Count - 1)];
                    var buf = pile.Get(element.Ptr) as byte[];
                    Aver.AreEqual(element.FirstByte, buf[0]);
                    Aver.AreEqual(element.LastByte, buf[element.IdxLast]);
                  }
                }

                if (i == Int32.MaxValue)
                  i = 0;
                else
                  i++;

                if (list.Count == Int32.MaxValue)
                  list = new List<CheckByteArray>();
              }

              // total check
              Console.WriteLine("Thread {0} is doing final read of {1} elements, ObjectCount {2}"
                .Args(Thread.CurrentThread.ManagedThreadId, list.Count, pile.ObjectCount));
              foreach (var element in list)
              {
                var buf = pile.Get(element.Ptr) as byte[];
                Aver.AreEqual(element.FirstByte, buf[0]);
                Aver.AreEqual(element.LastByte, buf[element.IdxLast]);
              }
              return;
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [Run("speed=true   durationSec=30  deleteFreq=3  isParallel=true")]
    [Run("speed=false  durationSec=30  deleteFreq=3  isParallel=true")]
    public static void DeleteOne_TRow(bool speed, int durationSec, int deleteFreq, bool isParallel)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var list = new List<CheckTRow>();
              var i = 0;
              var wlc = 0;
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) break;

                var val = PersonRow.MakeFake(new GDID(0, (ulong)i));

                var ptr = pile.Put(val);

                var element = new CheckTRow(ptr, val.ID, val.Address1);
                list.Add(element);

                // delete ONE random element
                if (i > 0 && i % deleteFreq == 0)
                {
                  var idx = App.Random.NextScaledRandomInteger(0, list.Count - 1);
                  ptr = list[idx].Ptr;
                  pile.Delete(ptr);
                  list.RemoveAt(idx);
                }

                // get several random elements
                if (list.Count > 64 && App.Random.NextScaledRandomInteger(0, 100) > 98)
                {
                  var toRead = App.Random.NextScaledRandomInteger(8, 64);
                  wlc++;
                  if (wlc % 125 == 0)
                    Console.WriteLine("Thread {0} is reading {1} elements, total {2}"
                      .Args(Thread.CurrentThread.ManagedThreadId, toRead, list.Count));
                  for (var k = 0; k < toRead; k++)
                  {
                    element = list[App.Random.NextScaledRandomInteger(0, list.Count - 1)];
                    var buf = pile.Get(element.Ptr) as PersonRow;
                    Aver.IsTrue(element.Id.Equals(buf.ID));
                    Aver.IsTrue(element.Address.Equals(buf.Address1));
                  }
                }

                if (i == Int32.MaxValue)
                  i = 0;
                else
                  i++;

                if (list.Count == Int32.MaxValue)
                  list = new List<CheckTRow>();
              }

              // total check
              Console.WriteLine("Thread {0} is doing final read of {1} elements, ObjectCount {2}"
                .Args(Thread.CurrentThread.ManagedThreadId, list.Count, pile.ObjectCount));
              foreach (var element in list)
              {
                var buf = pile.Get(element.Ptr) as PersonRow;
                Aver.IsTrue(element.Id.Equals(buf.ID));
                Aver.IsTrue(element.Address.Equals(buf.Address1));
              }
              return;
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [Run("speed=true   durationSec=30  payloadSizeMin=2  payloadSizeMax=1000  isParallel=true")]
    [Run("speed=false  durationSec=30  payloadSizeMin=2  payloadSizeMax=1000  isParallel=true")]
    public static void Chessboard_ByteArray(bool speed, int durationSec, int payloadSizeMin, int payloadSizeMax, bool isParallel)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var list = new List<CheckByteArray>();
              var i = 0;
              var wlc = 0;
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) break;

                var payloadSize = App.Random.NextScaledRandomInteger(payloadSizeMin, payloadSizeMax);
                var val = new byte[payloadSize];
                val[0] = (byte)App.Random.NextRandomInteger;
                val[payloadSize - 1] = (byte)App.Random.NextRandomInteger;

                var ptr = pile.Put(val);

                var element = new CheckByteArray(ptr, payloadSize - 1, val[0], val[payloadSize - 1]);
                list.Add(element);

                // delete previous element
                if (list.Count > 1 && i % 2 == 0)
                {
                  ptr = list[list.Count - 2].Ptr;
                  pile.Delete(ptr);
                  list.RemoveAt(list.Count - 2);
                }

                // get several random elements
                if (list.Count > 64 && App.Random.NextScaledRandomInteger(0, 100) > 98)
                {
                  var toRead = App.Random.NextScaledRandomInteger(8, 64);
                  wlc++;
                  if (wlc % 125 == 0)
                    Console.WriteLine("Thread {0} is reading {1} elements, total {2}, Pile objects {3}, Pile segments {4} Pile Bytes {5}"
                      .Args(Thread.CurrentThread.ManagedThreadId, toRead, list.Count, pile.ObjectCount, pile.SegmentCount, pile.AllocatedMemoryBytes));
                  for (var k = 0; k < toRead; k++)
                  {
                    element = list[App.Random.NextScaledRandomInteger(0, list.Count - 1)];
                    var buf = pile.Get(element.Ptr) as byte[];
                    Aver.AreEqual(element.FirstByte, buf[0]);
                    Aver.AreEqual(element.LastByte, buf[element.IdxLast]);
                  }
                }

                if (i == Int32.MaxValue)
                  i = 0;
                else
                  i++;

                if (list.Count == Int32.MaxValue)
                  list = new List<CheckByteArray>();
              }

              // total check
              Console.WriteLine("Thread {0} is doing final read of {1} elements, ObjectCount {2}"
                .Args(Thread.CurrentThread.ManagedThreadId, list.Count, pile.ObjectCount));
              foreach (var element in list)
              {
                var buf = pile.Get(element.Ptr) as byte[];
                Aver.AreEqual(element.FirstByte, buf[0]);
                Aver.AreEqual(element.LastByte, buf[element.IdxLast]);
              }
              return;
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [Run("speed=true   durationSec=30  isParallel=true")]
    [Run("speed=false  durationSec=30  isParallel=true")]
    public static void Chessboard_TRow(bool speed, int durationSec, bool isParallel)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var list = new List<CheckTRow>();
              var i = 0;
              var wlc = 0;
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) break;

                var val = PersonRow.MakeFake(new GDID(0, (ulong)i));

                var ptr = pile.Put(val);

                var element = new CheckTRow(ptr, val.ID, val.Address1);
                list.Add(element);

                // delete previous element
                if (list.Count > 1 && i % 2 == 0)
                {
                  ptr = list[list.Count - 2].Ptr;
                  pile.Delete(ptr);
                  list.RemoveAt(list.Count - 2);
                }

                // get several random elements
                if (list.Count > 64 && App.Random.NextScaledRandomInteger(0, 100) > 98)
                {
                  var toRead = App.Random.NextScaledRandomInteger(8, 64);
                  wlc++;
                  if (wlc % 125 == 0)
                    Console.WriteLine("Thread {0} is reading {1} elements, total {2}, Pile objects {3}, Pile segments {4} Pile Bytes {5}"
                      .Args(Thread.CurrentThread.ManagedThreadId, toRead, list.Count, pile.ObjectCount, pile.SegmentCount, pile.AllocatedMemoryBytes));
                  for (var k = 0; k < toRead; k++)
                  {
                    element = list[App.Random.NextScaledRandomInteger(0, list.Count - 1)];
                    var buf = pile.Get(element.Ptr) as PersonRow;
                    Aver.IsTrue(element.Id.Equals(buf.ID));
                    Aver.IsTrue(element.Address.Equals(buf.Address1));
                  }
                }

                if (i == Int32.MaxValue)
                  i = 0;
                else
                  i++;

                if (list.Count == Int32.MaxValue)
                  list = new List<CheckTRow>();
              }

              // total check
              Console.WriteLine("Thread {0} is doing final read of {1} elements, objectCount {2}"
                .Args(Thread.CurrentThread.ManagedThreadId, list.Count, pile.ObjectCount));
              foreach (var element in list)
              {
                var buf = pile.Get(element.Ptr) as PersonRow;
                Aver.IsTrue(element.Id.Equals(buf.ID));
                Aver.IsTrue(element.Address.Equals(buf.Address1));
              }
              return;
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [Run("speed=true   durationSec=30  putMin=100  putMax=200  delFactor=4  payloadSizeMin=2  payloadSizeMax=1000  isParallel=true")]
    [Run("speed=false  durationSec=30  putMin=100  putMax=200  delFactor=4  payloadSizeMin=2  payloadSizeMax=1000  isParallel=true")]
    public static void DeleteSeveral_ByteArray(bool speed, int durationSec, int putMin, int putMax, int delFactor, int payloadSizeMin, int payloadSizeMax, bool isParallel)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var list = new List<CheckByteArray>();
              var wlc = 0;
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) break;

                var putCount = App.Random.NextScaledRandomInteger(putMin, putMax);
                for (int i = 0; i < putCount; i++)
                {
                  var payloadSize = App.Random.NextScaledRandomInteger(payloadSizeMin, payloadSizeMax);
                  var val = new byte[payloadSize];
                  val[0] = (byte)App.Random.NextRandomInteger;
                  val[payloadSize - 1] = (byte)App.Random.NextRandomInteger;

                  var ptr = pile.Put(val);

                  list.Add(new CheckByteArray(ptr, payloadSize - 1, val[0], val[payloadSize - 1]));
                }

                int delCount = putCount / delFactor;
                for (int i = 0; i < delCount; i++)
                {
                  var idx = App.Random.NextScaledRandomInteger(0, list.Count - 1);
                  var ptr = list[idx].Ptr;
                  pile.Delete(ptr);
                  list.RemoveAt(idx);
                }

                // get several random elements
                if (list.Count > 64 && App.Random.NextScaledRandomInteger(0, 100) > 98)
                {
                  var toRead = App.Random.NextScaledRandomInteger(8, 64);
                  wlc++;
                  if (wlc % 125 == 0)
                    Console.WriteLine("Thread {0} is reading {1} elements, total {2}"
                      .Args(Thread.CurrentThread.ManagedThreadId, toRead, list.Count));
                  for (var k = 0; k < toRead; k++)
                  {
                    var element = list[App.Random.NextScaledRandomInteger(0, list.Count - 1)];
                    var buf = pile.Get(element.Ptr) as byte[];
                    Aver.AreEqual(element.FirstByte, buf[0]);
                    Aver.AreEqual(element.LastByte, buf[element.IdxLast]);
                  }
                }

                if (list.Count == Int32.MaxValue)
                  list = new List<CheckByteArray>();
              }

              // total check
              Console.WriteLine("Thread {0} is doing final read of {1} elements, objectCount {2}"
                .Args(Thread.CurrentThread.ManagedThreadId, list.Count, pile.ObjectCount));
              foreach (var element in list)
              {
                var buf = pile.Get(element.Ptr) as byte[];
                Aver.AreEqual(element.FirstByte, buf[0]);
                Aver.AreEqual(element.LastByte, buf[element.IdxLast]);
              }
              return;
            }));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [Run("speed=true   durationSec=30  putMin=100  putMax=200  delFactor=4  isParallel=true")]
    [Run("speed=false  durationSec=30  putMin=100  putMax=200  delFactor=4  isParallel=true")]
    public static void DeleteSeveral_TRow(bool speed, int durationSec, int putMin, int putMax, int delFactor, bool isParallel)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (isParallel ? (System.Environment.ProcessorCount - 1) : 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var list = new List<CheckTRow>();
              var wlc = 0;
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) break;

                var putCount = App.Random.NextScaledRandomInteger(putMin, putMax);
                for (int i = 0; i < putCount; i++)
                {
                  var val = PersonRow.MakeFake(new GDID());
                  var ptr = pile.Put(val);
                  list.Add(new CheckTRow(ptr, val.ID, val.Address1));
                }

                // delete several random elements
                int delCount = putCount / delFactor;
                for (int i = 0; i < delCount; i++)
                {
                  var idx = App.Random.NextScaledRandomInteger(0, list.Count - 1);
                  var ptr = list[idx].Ptr;
                  pile.Delete(ptr);
                  list.RemoveAt(idx);
                }

                // get several random elements
                if (list.Count > 64 && App.Random.NextScaledRandomInteger(0, 100) > 98)
                {
                  var toRead = App.Random.NextScaledRandomInteger(8, 64);
                  wlc++;
                  if (wlc % 125 == 0)
                    Console.WriteLine("Thread {0} is reading {1} elements, total {2}"
                      .Args(Thread.CurrentThread.ManagedThreadId, toRead, list.Count));
                  for (var k = 0; k < toRead; k++)
                  {
                    var element = list[App.Random.NextScaledRandomInteger(0, list.Count - 1)];
                    var buf = pile.Get(element.Ptr) as PersonRow;
                    Aver.IsTrue(element.Id.Equals(buf.ID));
                    Aver.IsTrue(element.Address.Equals(buf.Address1));
                  }
                }
              }

              // total check
              Console.WriteLine("Thread {0} is doing final read of {1} elements, objectCount {2}"
                .Args(Thread.CurrentThread.ManagedThreadId, list.Count, pile.ObjectCount));
              foreach (var element in list)
              {
                var buf = pile.Get(element.Ptr) as PersonRow;
                Aver.IsTrue(element.Id.Equals(buf.ID));
                Aver.IsTrue(element.Address.Equals(buf.Address1));
              }
              return;
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
    }

    [Run("speed=true  durationSec=30  payloadSizeMin=2  payloadSizeMax=1000  countMin=1000    countMax=200000")]
    [Run("speed=true  durationSec=30  payloadSizeMin=2  payloadSizeMax=1000  countMin=1000    countMax=200000")]
    [Run("speed=true  durationSec=30  payloadSizeMin=2  payloadSizeMax=1000  countMin=100000  countMax=200000")]
    [Run("speed=true  durationSec=30  payloadSizeMin=2  payloadSizeMax=1000  countMin=100000  countMax=200000")]
    public static void NoGrowth_ByteArray(bool speed, int durationSec, int payloadSizeMin, int payloadSizeMax, int countMin, int countMax)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (System.Environment.ProcessorCount - 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var list = new List<CheckByteArray>();
              bool put = true;
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) return;

                if (put)
                {
                  var cnt = App.Random.NextScaledRandomInteger(countMin, countMax);
                  for (int j = 0; j < cnt; j++)
                  {
                    var payloadSize = App.Random.NextScaledRandomInteger(payloadSizeMin, payloadSizeMax);
                    var val = new byte[payloadSize];
                    val[0] = (byte)App.Random.NextRandomInteger;
                    val[payloadSize - 1] = (byte)App.Random.NextRandomInteger;

                    var ptr = pile.Put(val);

                    var element = new CheckByteArray(ptr, payloadSize - 1, val[0], val[payloadSize - 1]);
                    list.Add(element);
                  }
                  Console.WriteLine("Thread {0} put {1} objects".Args(Thread.CurrentThread.ManagedThreadId, list.Count));
                  put = false;
                }
                else
                {
                  Console.WriteLine("Thread {0} deleted {1} objects".Args(Thread.CurrentThread.ManagedThreadId, list.Count));
                  for(var j=0; j < list.Count; j++)
                  {
                    var element = list[j];
                    var buf = pile.Get(element.Ptr) as byte[];
                    Aver.AreEqual(element.FirstByte, buf[0]);
                    Aver.AreEqual(element.LastByte, buf[element.IdxLast]);
                    pile.Delete(element.Ptr);

                  }
                  list.Clear();
                  put = true;
                }
              }
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
      Console.WriteLine("Test finished.");
    }

    [Run("speed=true  durationSec=30  countMin=100000  countMax=200000")]
    [Run("speed=true  durationSec=30  countMin=100000  countMax=200000")]
    public static void NoGrowth_TRow(bool speed, int durationSec, int countMin, int countMax)
    {
      using (var pile = new DefaultPile())
      {
        pile.AllocMode = speed ? AllocationMode.FavorSpeed : AllocationMode.ReuseSpace;
        pile.Start();
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task>();
        for (var t = 0; t < (System.Environment.ProcessorCount - 1); t++)
          tasks.Add(Task.Factory.StartNew(() =>
            {
              var list = new List<CheckTRow>();
              bool put = true;
              while (true)
              {
                if ((DateTime.UtcNow - startTime).TotalSeconds >= durationSec) return;

                if (put)
                {
                  var cnt = App.Random.NextScaledRandomInteger(countMin, countMax);
                  for (int j = 0; j < cnt; j++)
                  {
                    var val = PersonRow.MakeFake(new GDID());

                    var ptr = pile.Put(val);

                    var element = new CheckTRow(ptr, val.ID, val.Address1);
                    list.Add(element);
                  }
                  Console.WriteLine("Thread {0} put {1} objects".Args(Thread.CurrentThread.ManagedThreadId, list.Count));
                  put = false;
                }
                else
                {
                  Console.WriteLine("Thread {0} deleted {1} objects".Args(Thread.CurrentThread.ManagedThreadId, list.Count));
                  for (var j = 0; j < list.Count; j++)
                  {
                    var element = list[j];
                    var buf = pile.Get(element.Ptr) as PersonRow;
                    Aver.AreEqual(element.Id, buf.ID);
                    Aver.AreEqual(element.Address, buf.Address1);
                    pile.Delete(element.Ptr);
                  }
                  list.Clear();
                  put = true;
                }
              }
            }, TaskCreationOptions.LongRunning));
        Task.WaitAll(tasks.ToArray());
      }
      Console.WriteLine("Test finished.");
    }

    public struct CheckByteArray
    {
      public CheckByteArray(PilePointer pp, int il, byte fb, byte lb)
      {
        Ptr = pp;
        IdxLast = il;
        FirstByte = fb;
        LastByte = lb;
      }
      public readonly PilePointer Ptr;
      public readonly int IdxLast;
      public readonly byte FirstByte;
      public readonly byte LastByte;
    }

    public struct CheckTRow
    {
      public CheckTRow(PilePointer ptr, GDID id, string address)
      {
        Ptr = ptr;
        Id = id;
        Address = address;
      }
      public readonly PilePointer Ptr;
      public readonly GDID Id;
      public readonly string Address;
    }
  }
}
