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
using System.Threading;
using System.Threading.Tasks;



using Azos.Apps;
using Azos.Pile;

using Azos.Scripting;

namespace Azos.Tests.Unit.Pile
{
  [Runnable]
  public class MMFPileTest : MMFPileTestBase
  {
      [Run]
      public void Initial()
      {
        using (var pile = MakeMMFPile())
        {
          var ipile = pile as IPile;

          Aver.AreEqual(0, ipile.ObjectCount);
          Aver.AreEqual(0, ipile.AllocatedMemoryBytes);
          Aver.AreEqual(0, ipile.UtilizedBytes);
          Aver.AreEqual(0, ipile.OverheadBytes);
          Aver.AreEqual(0, ipile.SegmentCount);
        }
      }

      [Run]
      public void PutWOStart()
      {
        using (var pile = MakeMMFPile())
        {
          var ipile = pile as IPile;

          var row = ChargeRow.MakeFake(new GDID(0, 1));

          var pp = ipile.Put(row);

          Aver.IsFalse(pp.Valid);

          Aver.AreEqual(0, ipile.ObjectCount);
          Aver.AreEqual(0, ipile.AllocatedMemoryBytes);
          Aver.AreEqual(0, ipile.UtilizedBytes);
          Aver.AreEqual(0, ipile.OverheadBytes);
          Aver.AreEqual(0, ipile.SegmentCount);
        }
      }

      [Run]
      public void GetWOStart()
      {
        using (var pile = MakeMMFPile())
        {
          var ipile = pile as IPile;
          var obj = ipile.Get(PilePointer.Invalid);
          Aver.IsNull(obj);
        }
      }

      [Run]
      public void PutOne()
      {
        using (var pile = MakeMMFPile())
        {
          pile.Start();

          var ipile = pile as IPile;

          var row = CheckoutRow.MakeFake(new GDID(0, 1));

          var pp = ipile.Put(row);

          Aver.AreEqual(1, ipile.ObjectCount);
          Aver.AreEqual(DefaultPile.SEG_SIZE_DFLT, ipile.AllocatedMemoryBytes);
          Aver.AreEqual(1, ipile.SegmentCount);
        }
      }

      [Run]
      public void PutGetOne()
      {
        using (var pile = MakeMMFPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          var rowIn = CheckoutRow.MakeFake(new GDID(0, 1));

          var pp = ipile.Put(rowIn);

          var rowOut = ipile.Get(pp) as CheckoutRow;

          Aver.AreObjectsEqual(rowIn, rowOut);
        }
      }

      [Run]
      public void PutGetTwo()
      {
        using (var pile = MakeMMFPile())
        {
          pile.Start();

          var ipile = pile as IPile;

          var rowIn1 = CheckoutRow.MakeFake(new GDID(0, 1));
          var rowIn2 = CheckoutRow.MakeFake(new GDID(0, 2));

          var pp1 = ipile.Put(rowIn1);
          var pp2 = ipile.Put(rowIn2);

          Aver.IsTrue(2 == ipile.ObjectCount);
          Aver.AreEqual(DefaultPile.SEG_SIZE_DFLT, ipile.AllocatedMemoryBytes);
          Aver.AreEqual(1, ipile.SegmentCount);

          var rowOut1 = pile.Get(pp1) as CheckoutRow;
          var rowOut2 = pile.Get(pp2) as CheckoutRow;

          Aver.AreObjectsEqual(rowIn1, rowOut1);
          Aver.AreObjectsEqual(rowIn2, rowOut2);
        }
      }




      [Run]
      public void PutGetRawObject()
      {
        using (var pile = MakeMMFPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          var buf = new byte[]{1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0};

          var pp = ipile.Put(buf);

          byte svr;
          var buf2 = ipile.GetRawBuffer(pp, out svr); //main point: we dont get any exceptions

          Aver.IsTrue(buf2.Length >= buf.Length);
        }
      }



      [Run]
      [Aver.Throws(typeof(PileAccessViolationException))]
      public void GetNoObject()
      {
        using (var pile = MakeMMFPile())
        {
          pile.Start();
          var ipile = pile as IPile;
          ipile.Get(PilePointer.Invalid);
        }
      }

      [Run]
      [Aver.Throws(typeof(PileAccessViolationException))]
      public void DeleteInvalid()
      {
        using (var pile = MakeMMFPile())
        {
          pile.Start();
          var ipile = pile as IPile;
          ipile.Delete(PilePointer.Invalid);
        }
      }

      [Run]
      [Aver.Throws(typeof(PileAccessViolationException))]
      public void DeleteExisting()
      {
        using (var pile = MakeMMFPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          var rowIn = ChargeRow.MakeFake(new GDID(0, 1));

          var pp = ipile.Put(rowIn);

          ipile.Delete(pp);

          Aver.AreEqual(0, ipile.ObjectCount);
          Aver.AreEqual(0, ipile.AllocatedMemoryBytes);
          Aver.AreEqual(0, ipile.UtilizedBytes);
          Aver.AreEqual(0, ipile.OverheadBytes);
          Aver.AreEqual(0, ipile.SegmentCount);

          var rowOut = ipile.Get(pp);
        }
      }

      [Run]
      [Aver.Throws(typeof(PileAccessViolationException))]
      public void Purge()
      {
        using (var pile = MakeMMFPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          var rowIn = ChargeRow.MakeFake(new GDID(0, 1));

          var pp = ipile.Put(rowIn);

          ipile.Purge();

          Aver.AreEqual(0, ipile.ObjectCount);
          Aver.AreEqual(0, ipile.SegmentCount);

          var rowOut = ipile.Get(pp);


        }
      }

      [Run]
      public void PutCheckerboardPattern2()
      {
        using (var pile = MakeMMFPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          const ulong CNT = 100;

          var ppp = new Tuple<PilePointer, ChargeRow>[CNT];

          for (ulong i = 0; i < CNT; i++)
          {
            var ch = ChargeRow.MakeFake(new GDID(0, i));
            ppp[i] = new Tuple<PilePointer,ChargeRow>( ipile.Put(ch), ch);
          }

          Aver.AreEqual(CNT, (ulong)ipile.ObjectCount);

          for(ulong i = 0; i < CNT; i++)
          {
            var ch = ipile.Get(ppp[i].Item1);
            Aver.AreObjectsEqual(ch, ppp[i].Item2);
          }

          for(ulong i = 0; i < CNT; i+=2)
            ipile.Delete(ppp[i].Item1);

          Aver.AreEqual(CNT/2, (ulong)ipile.ObjectCount);

          for(ulong i = 0; i < CNT; i++)
          {
            if (i % 2 == 0)
            {
              try
              {
                ipile.Get(ppp[i].Item1);
                Aver.Fail("Object is deleted but its pointer doesn't throw exception!");
              }
              catch (PileAccessViolationException) {}
            }
            else
            {
              var ch = ipile.Get(ppp[i].Item1);
              Aver.AreObjectsEqual(ch, ppp[i].Item2);
            }
          }
        }
      }

      [Run]
      public void PutCheckerboardPattern3()
      {
        using (var pile = MakeMMFPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          const ulong CNT = 123;

          var ppp = new Tuple<PilePointer, string>[CNT];

          for (ulong i = 0; i < CNT; i++)
          {
            var str = Azos.Parsing.NaturalTextGenerator.Generate(179);
            ppp[i] = new Tuple<PilePointer,string>( ipile.Put(str), str);
          }

          Aver.AreEqual(CNT, (ulong)ipile.ObjectCount);

          for(ulong i = 0; i < CNT; i++)
          {
            if (i % 3 != 0)
              ipile.Delete(ppp[i].Item1);
          }

          Aver.AreEqual(CNT/3, (ulong)ipile.ObjectCount);

          for(ulong i = 0; i < CNT; i++)
          {
            if (i % 3 != 0)
            {
              try
              {
                ipile.Get(ppp[i].Item1);
                Aver.Fail("Object is deleted but its pointer doesn't throw exception!");
              }
              catch (PileAccessViolationException) {}
            }
            else
            {
              var ch = ipile.Get(ppp[i].Item1);
              Aver.AreObjectsEqual(ppp[i].Item2, ch);
            }
          }

          ////Console.WriteLine("ObjectCount: {0}", ipile.ObjectCount);
          ////Console.WriteLine("AllocatedMemoryBytes: {0}", ipile.AllocatedMemoryBytes);
          ////Console.WriteLine("UtilizedBytes: {0}", ipile.UtilizedBytes);
          ////Console.WriteLine("OverheadBytes: {0}", ipile.OverheadBytes);
          ////Console.WriteLine("SegmentCount: {0}", ipile.SegmentCount);
        }
      }

      [Run("isParallel=false  cnt=100000  minSz=0      maxSz=40      speed=true")]
      [Run("isParallel=false  cnt=10000   minSz=0      maxSz=50000   speed=true")]
      [Run("isParallel=false  cnt=1000    minSz=70000  maxSz=150000  speed=true")]
      [Run("isParallel=false  cnt=5000    minSz=0      maxSz=150000  speed=true")]

      [Run("isParallel=true  cnt=100000  minSz=0      maxSz=40      speed=true")]
      [Run("isParallel=true  cnt=10000   minSz=0      maxSz=50000   speed=true")]
      [Run("isParallel=true  cnt=1000    minSz=70000  maxSz=150000  speed=true")]
      [Run("isParallel=true  cnt=5000    minSz=0      maxSz=150000  speed=true")]

      [Run("isParallel=false  cnt=100000  minSz=0      maxSz=40      speed=false")]
      [Run("isParallel=false  cnt=10000   minSz=0      maxSz=50000   speed=false")]
      [Run("isParallel=false  cnt=1000    minSz=70000  maxSz=150000  speed=false")]
      [Run("isParallel=false  cnt=5000    minSz=0      maxSz=150000  speed=false")]

      [Run("isParallel=true  cnt=100000  minSz=0      maxSz=40      speed=false")]
      [Run("isParallel=true  cnt=10000   minSz=0      maxSz=50000   speed=false")]
      [Run("isParallel=true  cnt=1000    minSz=70000  maxSz=150000  speed=false")]
      [Run("isParallel=true  cnt=5000    minSz=0      maxSz=150000  speed=false")]
      public void VarSizes_Checkboard(bool isParallel, int cnt, int minSz, int maxSz, bool speed)
      {
        PileCacheTestCore.VarSizes_Checkboard(isParallel, cnt, minSz, maxSz, speed);
      }

      [Run("isParallel=false  cnt=100000  minSz=0      maxSz=256     speed=false  rnd=false")]
      [Run("isParallel=false  cnt=25000   minSz=0      maxSz=8000    speed=false  rnd=false")]
      [Run("isParallel=false  cnt=15000   minSz=0      maxSz=24000   speed=false  rnd=false")]
      [Run("isParallel=false  cnt=2100    minSz=65000  maxSz=129000  speed=false  rnd=false")]

      [Run("isParallel=false  cnt=100000  minSz=0      maxSz=256     speed=false  rnd=true")]
      [Run("isParallel=false  cnt=25000   minSz=0      maxSz=8000    speed=false  rnd=true")]
      [Run("isParallel=false  cnt=15000   minSz=0      maxSz=24000   speed=false  rnd=true")]
      [Run("isParallel=false  cnt=2100    minSz=65000  maxSz=129000  speed=false  rnd=true")]

      [Run("isParallel=false  cnt=100000  minSz=0      maxSz=256     speed=true  rnd=false")]
      [Run("isParallel=false  cnt=25000   minSz=0      maxSz=8000    speed=true  rnd=false")]
      [Run("isParallel=false  cnt=15000   minSz=0      maxSz=24000   speed=true  rnd=false")]
      [Run("isParallel=false  cnt=2100    minSz=65000  maxSz=129000  speed=true  rnd=false")]

      [Run("isParallel=false  cnt=100000  minSz=0      maxSz=256     speed=true  rnd=true")]
      [Run("isParallel=false  cnt=25000   minSz=0      maxSz=8000    speed=true  rnd=true")]
      [Run("isParallel=false  cnt=15000   minSz=0      maxSz=24000   speed=true  rnd=true")]
      [Run("isParallel=false  cnt=2100    minSz=65000  maxSz=129000  speed=true  rnd=true")]

      [Run("isParallel=true  cnt=100000  minSz=0      maxSz=256     speed=false  rnd=false")]
      [Run("isParallel=true  cnt=25000   minSz=0      maxSz=8000    speed=false  rnd=false")]
      [Run("isParallel=true  cnt=15000   minSz=0      maxSz=24000   speed=false  rnd=false")]
      [Run("isParallel=true  cnt=2100    minSz=65000  maxSz=129000  speed=false  rnd=false")]

      [Run("isParallel=true  cnt=100000  minSz=0      maxSz=256     speed=false  rnd=true")]
      [Run("isParallel=true  cnt=25000   minSz=0      maxSz=8000    speed=false  rnd=true")]
      [Run("isParallel=true  cnt=15000   minSz=0      maxSz=24000   speed=false  rnd=true")]
      [Run("isParallel=true  cnt=2100    minSz=65000  maxSz=129000  speed=false  rnd=true")]

      [Run("isParallel=true  cnt=100000  minSz=0      maxSz=256     speed=true  rnd=false")]
      [Run("isParallel=true  cnt=25000   minSz=0      maxSz=8000    speed=true  rnd=false")]
      [Run("isParallel=true  cnt=15000   minSz=0      maxSz=24000   speed=true  rnd=false")]
      [Run("isParallel=true  cnt=2100    minSz=65000  maxSz=129000  speed=true  rnd=false")]

      [Run("isParallel=true  cnt=100000  minSz=0      maxSz=256     speed=true  rnd=true")]
      [Run("isParallel=true  cnt=25000   minSz=0      maxSz=8000    speed=true  rnd=true")]
      [Run("isParallel=true  cnt=15000   minSz=0      maxSz=24000   speed=true  rnd=true")]
      [Run("isParallel=true  cnt=2100    minSz=65000  maxSz=129000  speed=true  rnd=true")]
      public void VarSizes_Increasing_Random(bool isParallel, int cnt, int minSz, int maxSz, bool speed, bool rnd)
      {
        PileCacheTestCore.VarSizes_Increasing_Random(isParallel, cnt, minSz, maxSz, speed, rnd);
      }

      [Run]
      public void Configuration()
      {
        var conf = @"
 app
 {
   memory-management
   {
     pile
     {
       alloc-mode=favorspeed
       free-list-size=100000
       max-segment-limit=79
       segment-size=395313143 //will be rounded to 16 byte boundary: 395,313,152
       max-memory-limit=123666333000
       data-directory-root=$'@@@ROOT'
       free-chunk-sizes='128, 256, 512, 1024, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 16000, 32000, 64000,  256000'
     }

     pile
     {
       name='specialNamed'
       identity=1900
       free-list-size=99000
       max-segment-limit=73
       segment-size=395313147 //will be rounded to 16 byte boundary: 395,313,152
       max-memory-limit=127666333000
       data-directory-root=$'@@@ROOT'
       free-chunk-sizes='77, 124, 180, 190, 200, 210, 220, 230, 1000, 2000, 3000, 4000, 5000, 32000, 64000,  257000'
     }
   }
 }".Replace("@@@ROOT", LOCAL_ROOT).AsLaconicConfig(handling: ConvertErrorHandling.Throw);

        using(var app = new ServiceBaseApplication(null, conf))
        {
          using (var pile = new MMFPile())
          {
            pile.Configure(null);

            Aver.IsTrue(AllocationMode.FavorSpeed == pile.AllocMode);
            Aver.AreEqual(100000, pile.FreeListSize);
            Aver.AreEqual(79, pile.MaxSegmentLimit);
            Aver.AreEqual(395313152, pile.SegmentSize);
            Aver.AreEqual(123666333000, pile.MaxMemoryLimit);

            Aver.AreEqual(128, pile.FreeChunkSizes[00]);
            Aver.AreEqual(256, pile.FreeChunkSizes[01]);
            Aver.AreEqual(512, pile.FreeChunkSizes[02]);
            Aver.AreEqual(1024, pile.FreeChunkSizes[03]);
            Aver.AreEqual(2000, pile.FreeChunkSizes[04]);
            Aver.AreEqual(3000, pile.FreeChunkSizes[05]);
            Aver.AreEqual(4000, pile.FreeChunkSizes[06]);
            Aver.AreEqual(5000, pile.FreeChunkSizes[07]);
            Aver.AreEqual(6000, pile.FreeChunkSizes[08]);
            Aver.AreEqual(7000, pile.FreeChunkSizes[09]);
            Aver.AreEqual(8000, pile.FreeChunkSizes[10]);
            Aver.AreEqual(9000, pile.FreeChunkSizes[11]);
            Aver.AreEqual(16000, pile.FreeChunkSizes[12]);
            Aver.AreEqual(32000, pile.FreeChunkSizes[13]);
            Aver.AreEqual(64000, pile.FreeChunkSizes[14]);
            Aver.AreEqual(256000, pile.FreeChunkSizes[15]);

            pile.Start();//just to test that it starts ok
          }

          using (var pile = new MMFPile("specialNamed"))
          {
            pile.Configure(null);

            Aver.AreEqual(1900, pile.Identity);

            Aver.IsTrue(AllocationMode.ReuseSpace == pile.AllocMode);
            Aver.AreEqual(99000, pile.FreeListSize);
            Aver.AreEqual(73, pile.MaxSegmentLimit);
            Aver.AreEqual(395313152, pile.SegmentSize);
            Aver.AreEqual(127666333000, pile.MaxMemoryLimit);

            Aver.AreEqual(77, pile.FreeChunkSizes[00]);
            Aver.AreEqual(124, pile.FreeChunkSizes[01]);
            Aver.AreEqual(180, pile.FreeChunkSizes[02]);
            Aver.AreEqual(190, pile.FreeChunkSizes[03]);
            Aver.AreEqual(200, pile.FreeChunkSizes[04]);
            Aver.AreEqual(210, pile.FreeChunkSizes[05]);
            Aver.AreEqual(220, pile.FreeChunkSizes[06]);
            Aver.AreEqual(230, pile.FreeChunkSizes[07]);
            Aver.AreEqual(1000, pile.FreeChunkSizes[08]);
            Aver.AreEqual(2000, pile.FreeChunkSizes[09]);
            Aver.AreEqual(3000, pile.FreeChunkSizes[10]);
            Aver.AreEqual(4000, pile.FreeChunkSizes[11]);
            Aver.AreEqual(5000, pile.FreeChunkSizes[12]);
            Aver.AreEqual(32000, pile.FreeChunkSizes[13]);
            Aver.AreEqual(64000, pile.FreeChunkSizes[14]);
            Aver.AreEqual(257000, pile.FreeChunkSizes[15]);

            pile.Start();//just to test that it starts ok


          }

        }//using app
      }
  }
}
