/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


using Azos.Apps;
using Azos.Data;
using Azos.Pile;

using Azos.Scripting;

namespace Azos.Tests.Unit.Pile
{
  [Runnable]
  public class PileTests2 : IRunHook
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

      [Run]
      public void ManyRow_PutThenRead()
      {
        using (var pile = new DefaultPile())
        {
          pile.SegmentSize = DefaultPile.SEG_SIZE_MIN;
          pile.Start();
          var ipile = pile as IPile;

          var lst = new List<KeyValuePair<PilePointer, CheckoutRow>>();

          var sw = Stopwatch.StartNew();
          while(ipile.SegmentCount<2)
          {
            var rowIn = CheckoutRow.MakeFake(new GDID(0, (ulong)lst.Count));
            var pp = ipile.Put(rowIn);
            lst.Add( new KeyValuePair<PilePointer, CheckoutRow>(pp, rowIn) );
          }

          var wms = sw.ElapsedMilliseconds;


          Console.WriteLine("Created {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(lst.Count, wms, lst.Count / (wms / 1000d)));
          Console.WriteLine("Occupied bytes {0:n0}".Args(pile.AllocatedMemoryBytes));


          sw.Restart();
          foreach(var kvp in lst)
          {
            Aver.IsNotNull( ipile.Get(kvp.Key) );
          }
          var rms = sw.ElapsedMilliseconds;
          Console.WriteLine("Read {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(lst.Count, rms, lst.Count / (rms / 1000d)));

          foreach(var kvp in lst)
          {
            var rowIn = kvp.Value;
            var rowOut = ipile.Get(kvp.Key) as CheckoutRow;
            Aver.AreObjectsEqual(rowIn, rowOut);
          }
        }
      }


      [Run]
      public void ManyRow_PutReadDeleteRead()
      {
        using (var pile = new DefaultPile())
        {
          pile.SegmentSize = DefaultPile.SEG_SIZE_MIN;
          pile.Start();
          var ipile = pile as IPile;

          var lst = new List<KeyValuePair<PilePointer, CheckoutRow>>();

          int totalPut = 0;
          int totalDelete = 0;

          var sw = Stopwatch.StartNew();
          while(ipile.SegmentCount<4)
          {
            var rowIn = CheckoutRow.MakeFake(new GDID(0, (ulong)lst.Count));
            var pp = ipile.Put(rowIn);
            totalPut++;
            lst.Add( new KeyValuePair<PilePointer, CheckoutRow>(pp, rowIn) );

            var chance = App.Random.NextRandomInteger > 1000000000; //periodically delete
            if (!chance) continue;

            var idx = App.Random.NextScaledRandomInteger(0, lst.Count-1);
            var kvp = lst[idx];
            lst.RemoveAt(idx);
            Aver.AreObjectsEqual(kvp.Value, ipile.Get(kvp.Key));

            Aver.IsTrue( ipile.Delete(kvp.Key) );
            totalDelete++;
          }

          var wms = sw.ElapsedMilliseconds;


          Console.WriteLine("Created {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(totalPut, wms, totalPut / (wms / 1000d)));
          Console.WriteLine("Read and deleted {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(totalDelete, wms, totalDelete / (wms / 1000d)));
          Console.WriteLine("Left {0:n0}".Args(lst.Count));
          Console.WriteLine("Occupied bytes {0:n0}".Args(pile.AllocatedMemoryBytes));


          sw.Restart();
          foreach(var kvp in lst)
          {
            Aver.IsNotNull( ipile.Get(kvp.Key) );
          }
          var rms = sw.ElapsedMilliseconds;
          Console.WriteLine("Read {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(lst.Count, rms, lst.Count / (rms / 1000d)));


          Console.WriteLine("Will verify all rows...");
          foreach(var kvp in lst)
          {
            var rowIn = kvp.Value;
            var rowOut = ipile.Get(kvp.Key) as CheckoutRow;
            Aver.AreObjectsEqual(rowIn, rowOut);
          }
        }
      }


      [Run]
      public void ManyMixed_PutReadDeleteRead()
      {
        using (var pile = new DefaultPile())
        {
          pile.SegmentSize = DefaultPile.SEG_SIZE_MIN;
          pile.Start();
          var ipile = pile as IPile;

          var lst = new List<KeyValuePair<PilePointer, CheckoutRow>>();

          int totalBuff = 0;
          long totalBuffSz = 0;
          int totalPut = 0;
          int totalDelete = 0;

          var pBuff = PilePointer.Invalid;

          var sw = Stopwatch.StartNew();
          while(ipile.SegmentCount<16)
          {
            var chance = App.Random.NextRandomInteger > 0;
            if (chance)
            {
              var fakeBuf = new byte[App.Random.NextScaledRandomInteger(0, 128*1024)];
              totalBuff++;
              totalBuffSz += fakeBuf.Length;
              if (pBuff.Valid && App.Random.NextRandomInteger > 0) ipile.Delete(pBuff); //periodically delete buffers
              pBuff = ipile.Put(fakeBuf);
            }

            var rowIn = CheckoutRow.MakeFake(new GDID(0, (ulong)lst.Count));
            var pp = ipile.Put(rowIn);
            totalPut++;
            lst.Add( new KeyValuePair<PilePointer, CheckoutRow>(pp, rowIn) );

            chance = App.Random.NextRandomInteger > 1000000000; //periodically delete rows
            if (!chance) continue;

            var idx = App.Random.NextScaledRandomInteger(0, lst.Count-1);
            var kvp = lst[idx];
            lst.RemoveAt(idx);
            Aver.AreObjectsEqual(kvp.Value, ipile.Get(kvp.Key));

            Aver.IsTrue( ipile.Delete(kvp.Key) );
            totalDelete++;
          }

          var wms = sw.ElapsedMilliseconds;

          Console.WriteLine("Buff Created {0:n0} size {1:n0} bytes".Args(totalBuff, totalBuffSz));
          Console.WriteLine("Row Created {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(totalPut, wms, totalPut / (wms / 1000d)));
          Console.WriteLine("Read and deleted {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(totalDelete, wms, totalDelete / (wms / 1000d)));
          Console.WriteLine("Left {0:n0}".Args(lst.Count));
          Console.WriteLine("Occupied bytes {0:n0}".Args(pile.AllocatedMemoryBytes));
          Console.WriteLine("Utilized bytes {0:n0}".Args(pile.UtilizedBytes));
          Console.WriteLine("Objects {0:n0}".Args(pile.ObjectCount));


          sw.Restart();
          foreach(var kvp in lst)
          {
            Aver.IsNotNull( ipile.Get(kvp.Key) );
          }
          var rms = sw.ElapsedMilliseconds;
          Console.WriteLine("Read {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(lst.Count, rms, lst.Count / (rms / 1000d)));


          Console.WriteLine("Will verify all rows...");
          foreach(var kvp in lst)
          {
            var rowIn = kvp.Value;
            var rowOut = ipile.Get(kvp.Key) as CheckoutRow;
            Aver.AreObjectsEqual(rowIn, rowOut);
          }
        }
      }


      [Run("tcount=3")]
      [Run("tcount=7")]
      [Run("tcount=11")]
      [Run("tcount=39")]
      public void Parallel_ManyMixed_PutReadDeleteRead(int tcount)
      {
        using (var pile = new DefaultPile())
        {
          pile.SegmentSize = DefaultPile.SEG_SIZE_MIN;
          pile.Start();
          var ipile = pile as IPile;

          var lst = new List<KeyValuePair<PilePointer, CheckoutRow>>();

          int totalBuff = 0;
          long totalBuffSz = 0;
          int totalPut = 0;
          int totalDelete = 0;


          var sw = Stopwatch.StartNew();

          var tasks =new List<Task>();

          while(tasks.Count<tcount)
          {
              var task = Task.Factory.StartNew( ()=>
              {
                        var llst = new List<KeyValuePair<PilePointer, CheckoutRow>>();
                        var pBuff = PilePointer.Invalid;

                        while(ipile.SegmentCount<64)
                        {
                          var chance = App.Random.NextRandomInteger > 0;
                          if (chance)
                          {
                            var fakeBuf = new byte[App.Random.NextScaledRandomInteger(0, 128*1024)];
                            Interlocked.Increment(ref totalBuff);
                            Interlocked.Add( ref totalBuffSz, fakeBuf.Length);
                            if (pBuff.Valid && App.Random.NextRandomInteger > 0) ipile.Delete(pBuff); //periodically delete buffers
                            pBuff = ipile.Put(fakeBuf);
                          }

                          var rowIn = CheckoutRow.MakeFake(new GDID(0, (ulong)llst.Count));
                          var pp = ipile.Put(rowIn);
                          Interlocked.Increment( ref totalPut);
                          llst.Add( new KeyValuePair<PilePointer, CheckoutRow>(pp, rowIn) );

                          chance = App.Random.NextRandomInteger > 1000000000; //periodically delete rows
                          if (!chance) continue;

                          var idx = App.Random.NextScaledRandomInteger(0, llst.Count-1);
                          var kvp = llst[idx];
                          llst.RemoveAt(idx);
                          Aver.AreObjectsEqual(kvp.Value, ipile.Get(kvp.Key));

                          Aver.IsTrue( ipile.Delete(kvp.Key) );
                          Interlocked.Increment(ref totalDelete);
                        }
                        lock(lst)
                         lst.AddRange(llst);
              });
              tasks.Add(task);
          }
          Task.WaitAll(tasks.ToArray());

          var wms = sw.ElapsedMilliseconds;

          Console.WriteLine("Buff Created {0:n0} size {1:n0} bytes".Args(totalBuff, totalBuffSz));
          Console.WriteLine("Row Created {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(totalPut, wms, totalPut / (wms / 1000d)));
          Console.WriteLine("Read and deleted {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(totalDelete, wms, totalDelete / (wms / 1000d)));
          Console.WriteLine("Left {0:n0}".Args(lst.Count));
          Console.WriteLine("Occupied bytes {0:n0}".Args(pile.AllocatedMemoryBytes));
          Console.WriteLine("Utilized bytes {0:n0}".Args(pile.UtilizedBytes));
          Console.WriteLine("Objects {0:n0}".Args(pile.ObjectCount));


          sw.Restart();
          foreach(var kvp in lst)
          {
            Aver.IsNotNull( ipile.Get(kvp.Key) );
          }
          var rms = sw.ElapsedMilliseconds;
          Console.WriteLine("Read {0:n0} in {1:n0} ms at {2:n0} ops/sec".Args(lst.Count, rms, lst.Count / (rms / 1000d)));


          Console.WriteLine("Will verify all rows...");
          Parallel.ForEach(lst, kvp =>
          {
            var rowIn = kvp.Value;
            var rowOut = ipile.Get(kvp.Key) as CheckoutRow;
            Aver.AreObjectsEqual(rowIn, rowOut);
          });
        }
      }
  }
}
