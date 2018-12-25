/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Pile;
using Azos.Scripting;

namespace Azos.Tests.Unit.Pile
{
  [Runnable(TRUN.BASE, 9)]
  public class PileMutableTests : IRunHook
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


      [Run(TRUN.BASE, null, 8)]
      public void FitPreallocate()
      {
        using (var pile = new DefaultPile(NOPApplication.Instance))
        {
          pile.Start();

          var obj1 = new Payload{ID = 1, Name = "1", Data = null};

          var p1 = pile.Put(obj1, preallocateBlockSize: 4000);

          Aver.AreEqual(4000,  pile.SizeOf(p1));

          var obj2 = new Payload{ID = 2, Name = "2", Data = new byte []{1,2,3,4,5,6,7,8}};
          Aver.IsTrue( pile.Put(p1, obj2) );

          Aver.AreEqual(1, pile.ObjectCount );
          var got = pile.Get(p1) as Payload;

          Aver.AreEqual(2, got.ID);
          Aver.AreEqual("2", got.Name);
          Aver.IsNotNull(got.Data);
          Aver.AreEqual(8, got.Data.Length);

          Aver.IsTrue( pile.AllocatedMemoryBytes > 0);
          Aver.IsTrue( pile.Delete(p1) );
          Aver.IsTrue( pile.AllocatedMemoryBytes == 0 );
          Aver.IsTrue( pile.ObjectCount == 0);
        }
      }

      [Run(TRUN.BASE, null, 8)]
      public void FitPreallocateString()
      {
        using (var pile = new DefaultPile(NOPApplication.Instance))
        {
          pile.Start();

          var obj1 = "abcdefgh";

          var p1 = pile.Put(obj1, preallocateBlockSize: 4000);
          var got = pile.Get(p1) as string;
          Aver.AreEqual("abcdefgh", got);

          Aver.AreEqual(4000,  pile.SizeOf(p1));

          var obj2 = "abcdefghijklmnopqrst0912345";
          Aver.IsTrue( pile.Put(p1, obj2) );

          Aver.AreEqual(1, pile.ObjectCount );
          got = pile.Get(p1) as string;

          Aver.AreEqual("abcdefghijklmnopqrst0912345", got);

          Aver.IsTrue( pile.AllocatedMemoryBytes > 0);
          Aver.IsTrue( pile.Delete(p1) );
          Aver.IsTrue( pile.AllocatedMemoryBytes == 0 );
          Aver.IsTrue( pile.ObjectCount == 0);
        }
      }

      [Run(TRUN.BASE, null, 8)]
      public void LinkStringNoPreallocate()
      {
        using (var pile = new DefaultPile(NOPApplication.Instance))
        {
          pile.Start();

          var obj1 = "abcdefgh";

          var p1 = pile.Put(obj1);
          var got = pile.Get(p1) as string;
          Aver.AreEqual("abcdefgh", got);

          var obj2 = "abcdefghijklmnopqrst0912345";
          Aver.IsTrue( pile.Put(p1, obj2) );

          Aver.AreEqual(1, pile.ObjectCount );
          got = pile.Get(p1) as string;

          Aver.AreEqual("abcdefghijklmnopqrst0912345", got);

          Aver.IsTrue( pile.AllocatedMemoryBytes > 0);
          Aver.IsTrue( pile.Delete(p1) );
          Aver.IsTrue( pile.AllocatedMemoryBytes == 0 );
          Aver.IsTrue( pile.ObjectCount == 0);
        }
      }


      [Run(TRUN.BASE, null, 8)]
      public void FitPreallocateByteArray()
      {
        using (var pile = new DefaultPile(NOPApplication.Instance))
        {
          pile.Start();

          var obj1 = new byte[3];

          var p1 = pile.Put(obj1, preallocateBlockSize: 4000);
          var got = pile.Get(p1) as byte[];
          Aver.AreEqual(3, got.Length);

          Aver.AreEqual(4000,  pile.SizeOf(p1));

          var obj2 = new byte[571];
          Aver.IsTrue( pile.Put(p1, obj2) );

          Aver.AreEqual(1, pile.ObjectCount );
          got = pile.Get(p1) as byte[];

          Aver.AreEqual(571, got.Length);

          Aver.IsTrue( pile.AllocatedMemoryBytes > 0);
          Aver.IsTrue( pile.Delete(p1) );
          Aver.IsTrue( pile.AllocatedMemoryBytes == 0 );
          Aver.IsTrue( pile.ObjectCount == 0);
        }
      }


      [Run(TRUN.BASE, null, 8)]
      public void LinkByteArrayNoPreallocate()
      {
        using (var pile = new DefaultPile(NOPApplication.Instance))
        {
          pile.Start();

          var obj1 = new byte[12];

          var p1 = pile.Put(obj1);
          var got = pile.Get(p1) as byte[];
          Aver.AreEqual(12, got.Length);

          var obj2 = new byte[389];
          Aver.IsTrue( pile.Put(p1, obj2) );

          Aver.AreEqual(1, pile.ObjectCount );
          got = pile.Get(p1) as byte[];

          Aver.AreEqual(389, got.Length);

          Aver.IsTrue( pile.AllocatedMemoryBytes > 0);
          Aver.IsTrue( pile.Delete(p1) );
          Aver.IsTrue( pile.AllocatedMemoryBytes == 0 );
          Aver.IsTrue( pile.ObjectCount == 0);
        }
      }



      [Run(TRUN.BASE, null, 8)]
      public void LinkNoPreallocate()
      {
        using (var pile = new DefaultPile(NOPApplication.Instance))
        {
          pile.Start();

          var obj1 = new Payload{ID = 1, Name = "1", Data = null};

          var p1 = pile.Put(obj1);

          Aver.IsTrue( pile.SizeOf(p1) < 128);

          var obj2 = new Payload{ID = 2, Name = "2", Data = new byte [128]};
          Aver.IsTrue( pile.Put(p1, obj2) );

          Aver.AreEqual(1, pile.ObjectCount );
          var got = pile.Get(p1) as Payload;

          Aver.AreEqual(2, got.ID);
          Aver.AreEqual("2", got.Name);
          Aver.IsNotNull(got.Data);
          Aver.AreEqual(128, got.Data.Length);

          Aver.IsTrue( pile.AllocatedMemoryBytes > 0);
          Aver.IsTrue( pile.Delete(p1) );
          Aver.IsTrue( pile.AllocatedMemoryBytes == 0 );
          Aver.IsTrue( pile.ObjectCount == 0);
        }
      }

      [Run(TRUN.BASE, null, 8)]
      public void LinkPreallocate()
      {
        using (var pile = new DefaultPile(NOPApplication.Instance))
        {
          pile.Start();

          var obj1 = new Payload{ID = 1, Name = "1", Data = null};

          var p1 = pile.Put(obj1, preallocateBlockSize: 2000);

          Aver.AreEqual(2000,  pile.SizeOf(p1));

          var obj2 = new Payload{ID = 2, Name = "2", Data = new byte [3000]};
          Aver.IsTrue( pile.Put(p1, obj2) );

          Aver.AreEqual(1, pile.ObjectCount );
          var got = pile.Get(p1) as Payload;

          Aver.AreEqual(2, got.ID);
          Aver.AreEqual("2", got.Name);
          Aver.IsNotNull(got.Data);
          Aver.AreEqual(3000, got.Data.Length);

          Aver.IsTrue( pile.AllocatedMemoryBytes > 0);
          Aver.IsTrue( pile.Delete(p1) );
          Aver.IsTrue( pile.AllocatedMemoryBytes == 0 );
          Aver.IsTrue( pile.ObjectCount == 0);
        }
      }




      [Run(TRUN.BASE, null, 8)]
      public void Reallocate_BackOriginal()
      {
        using (var pile = new DefaultPile(NOPApplication.Instance))
        {
          pile.Start();

          var obj1 = new Payload{ID = 1, Name = "1", Data = null};

          var p1 = pile.Put(obj1, preallocateBlockSize: 2000);

          Aver.AreEqual(2000,  pile.SizeOf(p1));
          var a = pile.AllocatedMemoryBytes;
          var u = pile.UtilizedBytes;
          var o = pile.OverheadBytes;
          Console.WriteLine("Allocated: {0:n0}  Utilized: {1:n0}  Overhead: {2:n0}", a, u, o);

          Aver.AreEqual(2000, u);

          Aver.IsTrue( pile.Put(p1, new Payload{Data=new byte[8000]}) );

          var a2 = pile.AllocatedMemoryBytes;
          var u2 = pile.UtilizedBytes;
          var o2 = pile.OverheadBytes;
          Console.WriteLine("Allocated: {0:n0}  Utilized: {1:n0}  Overhead: {2:n0}", a2, u2, o2);

          Aver.IsTrue( u2 > 10000);
          Aver.IsTrue( u2 > u);
          pile.Put(p1, obj1);

          u2 = pile.UtilizedBytes;
          Console.WriteLine("Allocated: {0:n0}  Utilized: {1:n0}  Overhead: {2:n0}", a2, u2, o2);
          Aver.AreEqual(2000, u2);

        }
      }


      [Run(TRUN.BASE, null, 8)]
      public void Reallocate_Delete()
      {
        using (var pile = new DefaultPile(NOPApplication.Instance))
        {
          pile.Start();

          var obj1 = new Payload{ID = 1, Name = "1", Data = null};

          var p1 = pile.Put(obj1, preallocateBlockSize: 2000);

          Aver.AreEqual(2000,  pile.SizeOf(p1));
          var a = pile.AllocatedMemoryBytes;
          var u = pile.UtilizedBytes;
          var o = pile.OverheadBytes;
          Console.WriteLine("Allocated: {0:n0}  Utilized: {1:n0}  Overhead: {2:n0}", a, u, o);

          Aver.AreEqual(2000, u);

          Aver.IsTrue( pile.Put(p1, new Payload{Data=new byte[8000]}) );

          var a2 = pile.AllocatedMemoryBytes;
          var u2 = pile.UtilizedBytes;
          var o2 = pile.OverheadBytes;
          Console.WriteLine("Allocated: {0:n0}  Utilized: {1:n0}  Overhead: {2:n0}", a2, u2, o2);

          Aver.IsTrue( u2 > 10000);
          Aver.IsTrue( u2 > u);
          pile.Delete(p1);

          u2 = pile.UtilizedBytes;
          Console.WriteLine("Allocated: {0:n0}  Utilized: {1:n0}  Overhead: {2:n0}", a2, u2, o2);
          Aver.AreEqual(0, u2);

        }
      }



      //todo test
      //delete, size of, compact, deadlocks/multithreaded
      //add statistics fro LINKS count?

      //todo serializer format for string
      //todo serialier format for byte[]
      //todo Is PilePointer supported by slim format? Pilepointer[]?


      public class Payload
      {
        public int ID;
        public string Name;
        public byte[] Data;
      }

      [Run(TRUN.BASE, null, 8, "len=50000  deleteEvery=2  parallel=8")]
      [Run(TRUN.BASE, null, 8, "len=50000  deleteEvery=3  parallel=8")]
      [Run(TRUN.BASE, null, 8, "len=50000  deleteEvery=5  parallel=8")]
      public void StringCorrectess(int len, int deleteEvery, int parallel)
      {
        using (var pile = new DefaultPile(NOPApplication.Instance))
        {
          pile.Start();

          var bag = new ConcurrentBag<PilePointer>();
          var deleted = 0;
          Parallel.For(0, len, new ParallelOptions{ MaxDegreeOfParallelism=parallel},
          (i) =>
          {
            var str = new string('a', i);

            PilePointer pp = PilePointer.Invalid;
            if (i%2==0)
             if (bag.TryTake(out pp))
              pile.Put(pp, str);

            pp = pile.Put(str, preallocateBlockSize: i+100);

            var got = pile.Get(pp) as string;

            Aver.AreEqual(str, got);

            if (i%deleteEvery==0)
            {
             PilePointer dp;
             if (bag.TryTake(out dp))
             {
               if (pile.Delete(dp, false))
                Interlocked.Increment(ref deleted);
             }
             bag.Add(pp);
            }
          });

          Console.WriteLine("Deleted {0:n0}", deleted);
        }
      }


      [Run(TRUN.BASE, null, 8, "len=300")]
      public void ReallocateInPlace(int len)
      {
        using (var pile = new DefaultPile(NOPApplication.Instance))
        {
          pile.Start();
          var lst = new List<PilePointer>();
          for(var i=0; i<len; i++)
          {
             var ub = i * 10;
             var ptr = pile.Put(new byte[0], preallocateBlockSize: ub);

             lst.Add(ptr);
             for(var j=0; j<ub+(ub/2); j++)
             {
               Aver.IsTrue( pile.Put(ptr, new byte[j]) );
             }
          }

          Aver.AreEqual(lst.Count, pile.ObjectCount);
          var n = 0;
          foreach(var pp in lst)
          {
            Console.WriteLine(   "iteration#{0} of {1}".Args(n, lst.Count) );
            Aver.IsTrue( pile.Delete(pp) );
            n++;
          }

          Aver.AreEqual(0, pile.AllocatedMemoryBytes);
          Aver.AreEqual(0, pile.ObjectCount);
          Aver.AreEqual(0, pile.ObjectLinkCount);
        }
      }

      [Run("cnt=1        tcount=8  seconds=20")]
      [Run("cnt=12       tcount=8  seconds=20")]
      [Run("cnt=256      tcount=8  seconds=30")]
      [Run("cnt=1024     tcount=8  seconds=30")]
      [Run("cnt=1048576  tcount=12 seconds=60")]
      public void TestThreadSafety(int cnt, int tcount, int seconds)
      {
        using (var pile = new DefaultPile(NOPApplication.Instance))
        {
          var App = pile.App;

          // pile.SegmentSize = 64 * 1024 * 1024;
          pile.Start();

          var data = new PilePointer[cnt];
          for(var i=0; i<cnt; i++)
            data[i] = pile.Put(new byte[0], preallocateBlockSize: Memory.PTR_RAW_BYTE_SIZE + (137 * (i%17)));

          var lst = new List<Task>();

          long statRead = 0;
          long statPut = 0;
          long statDelete = 0;

          var sw = Stopwatch.StartNew();
          var sd = DateTime.UtcNow;
          var deleteLock = new object();
          for(var i=0; i<tcount; i++)
           lst.Add( Task.Factory.StartNew( () =>
           {
             while(true)
             {
               var now = DateTime.UtcNow;
               if ((now - sd).TotalSeconds>seconds) break;

               var it = App.Random.NextScaledRandomInteger(10,100);
               for(var j=0; j<it; j++)
               {
                 var pp = data[App.Random.NextScaledRandomInteger(0, cnt-1)];
                 Aver.IsTrue( pile.Get(pp) is byte[] );
                 Interlocked.Increment(ref statRead);
               }

               it = App.Random.NextScaledRandomInteger(10,100);
               for(var j=0; j<it; j++)
               {
                 var pp = data[App.Random.NextScaledRandomInteger(0, cnt-1)];
                 Aver.IsTrue( pile.Put(pp, new byte[App.Random.NextScaledRandomInteger(0,3791)], link: true) );
                 Interlocked.Increment(ref statPut);
               }

               if (App.Random.NextScaledRandomInteger(0,100)>50 && Monitor.TryEnter(deleteLock))
               try
               {
                 var newData = (PilePointer[])data.Clone();
                 it = App.Random.NextScaledRandomInteger(0, 10 + (cnt / 2));
                 var toDelete = new List<PilePointer>();
                 for(var j=0; j<it; j++)
                 {
                   var idx = App.Random.NextScaledRandomInteger(0, cnt-1);
                   toDelete.Add( newData[idx] );
                   newData[idx] = pile.Put(new byte[12], preallocateBlockSize: App.Random.NextScaledRandomInteger(24, 1024));
                 }
                 data = newData;//atomic;
                 Thread.Sleep(1000);
                 foreach(var pp in toDelete)
                 {
                   Aver.IsTrue( pile.Delete(pp) );
                   Interlocked.Increment(ref statDelete);
                 }
               }
               finally
               {
                 Monitor.Exit(deleteLock);
               }

             }
           }, TaskCreationOptions.LongRunning));

          Task.WaitAll(lst.ToArray());

          var el = sw.ElapsedMilliseconds;

          Console.WriteLine("Read {0:n0} at {1:n0} ops/sec".Args(statRead, statRead /(el / 1000d)));
          Console.WriteLine("Put {0:n0} at {1:n0} ops/sec".Args(statPut, statPut /(el / 1000d)));
          Console.WriteLine("Deleted {0:n0} at {1:n0} ops/sec".Args(statDelete, statDelete /(el / 1000d)));

          for(var i=0; i<data.Length; i++)
           Aver.IsTrue( pile.Delete(data[i]) );


          Aver.AreEqual(0, pile.ObjectCount);
          Aver.AreEqual(0, pile.ObjectLinkCount);
          Aver.AreEqual(0, pile.AllocatedMemoryBytes);

        }
      }
  }
}
