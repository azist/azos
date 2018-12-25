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
  [Runnable]
  public class PilePrimitiveTests : IRunHook
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
      public void MulticulturalString()
      {
        using (var pile = new DefaultPile(NOPApplication.Instance))
        {
          pile.Start();

            var str = @"
外国語の学習と教授

Language Learning and Teaching

Изучение и обучение иностранных языков

Tere Daaheng Aneng Karimah

語文教學・语文教学

Enseñanza y estudio de idiomas

Изучаване и Преподаване на Чужди Езици

ქართული ენის შესწავლა და სწავლება

'læŋɡwidʒ 'lɘr:niŋ ænd 'ti:tʃiŋ

Lus kawm thaib qhia

Ngôn Ngữ, Sự học,

‭‫ללמוד וללמד את השֵפה

L'enseignement et l'étude des langues

말배우기와 가르치기

Nauka języków obcych

Γλωσσική Εκμὰθηση και Διδασκαλία

‭‫ﺗﺪﺭﯾﺲ ﻭ ﯾﺎﺩﮔﯿﺮﯼ ﺯﺑﺎﻥ

Sprachlernen und -lehren

‭‫ﺗﻌﻠﻢ ﻭﺗﺪﺭﻳﺲ ﺍﻟﻌﺮﺑﻴﺔ

เรียนและสอนภาษา";

            var pp = pile.Put(str);

            var got = pile.Get(pp) as string;

            Aver.AreEqual(str, got);
        }
      }


      [Run("cnt=100")]
      [Run("cnt=1024")]
      [Run("cnt=98304")]   //   98 * 1024
      [Run("cnt=262144")]  //  256 * 1024
      [Run("cnt=524288")]  //  512 * 1024
      [Run("cnt=1048576")] // 1024 * 1024
      public void LongMulticulturalString(int cnt)
      {
        using (var pile = new DefaultPile(NOPApplication.Instance))
        {
          pile.Start();

            var str = new string('久', cnt);

            var pp = pile.Put(str);

            var got = pile.Get(pp) as string;

            Aver.AreEqual(str, got);
        }
      }



      [Run("len=25000  deleteEvery=2   parallel=1")]
      [Run("len=25000  deleteEvery=3   parallel=1")]
      [Run("len=25000  deleteEvery=4   parallel=1")]
      [Run("len=25000  deleteEvery=5   parallel=1")]
      [Run("len=25000  deleteEvery=6   parallel=1")]
      [Run("len=25000  deleteEvery=7   parallel=1")]
      [Run("len=25000  deleteEvery=8   parallel=1")]
      [Run("len=25000  deleteEvery=9   parallel=1")]
      [Run("len=25000  deleteEvery=10  parallel=1")]
      [Run("len=25000  deleteEvery=15  parallel=1")]
      [Run("len=25000  deleteEvery=16  parallel=1")]
      [Run("len=66560  deleteEvery=3   parallel=1")]
      [Run("len=66560  deleteEvery=7   parallel=1")]
      [Run("len=66560  deleteEvery=8   parallel=1")]
      [Run("len=66560  deleteEvery=15  parallel=1")]
      [Run("len=66560  deleteEvery=16  parallel=1")]

      [Run("len=25000  deleteEvery=2   parallel=8")]
      [Run("len=25000  deleteEvery=3   parallel=8")]
      [Run("len=25000  deleteEvery=4   parallel=8")]
      [Run("len=25000  deleteEvery=5   parallel=8")]
      [Run("len=25000  deleteEvery=6   parallel=8")]
      [Run("len=25000  deleteEvery=7   parallel=8")]
      [Run("len=25000  deleteEvery=8   parallel=8")]
      [Run("len=25000  deleteEvery=9   parallel=8")]
      [Run("len=25000  deleteEvery=10  parallel=8")]
      [Run("len=25000  deleteEvery=15  parallel=8")]
      [Run("len=25000  deleteEvery=16  parallel=8")]
      [Run("len=66560  deleteEvery=3   parallel=8")]
      [Run("len=66560  deleteEvery=7   parallel=8")]
      [Run("len=66560  deleteEvery=8   parallel=8")]
      [Run("len=66560  deleteEvery=15  parallel=8")]
      [Run("len=66560  deleteEvery=16  parallel=8")]
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
            var str = new string('a', len-i);

            var pp = pile.Put(str);

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



      [Run("len=25000  deleteEvery=2   parallel=1")]
      [Run("len=25000  deleteEvery=3   parallel=1")]
      [Run("len=25000  deleteEvery=4   parallel=1")]
      [Run("len=25000  deleteEvery=5   parallel=1")]
      [Run("len=25000  deleteEvery=6   parallel=1")]
      [Run("len=25000  deleteEvery=7   parallel=1")]
      [Run("len=25000  deleteEvery=8   parallel=1")]
      [Run("len=25000  deleteEvery=9   parallel=1")]
      [Run("len=25000  deleteEvery=10  parallel=1")]
      [Run("len=25000  deleteEvery=15  parallel=1")]
      [Run("len=25000  deleteEvery=16  parallel=1")]
      [Run("len=66560  deleteEvery=3   parallel=1")]
      [Run("len=66560  deleteEvery=7   parallel=1")]
      [Run("len=66560  deleteEvery=8   parallel=1")]
      [Run("len=66560  deleteEvery=15  parallel=1")]
      [Run("len=66560  deleteEvery=16  parallel=1")]

      [Run("len=25000  deleteEvery=2   parallel=8")]
      [Run("len=25000  deleteEvery=3   parallel=8")]
      [Run("len=25000  deleteEvery=4   parallel=8")]
      [Run("len=25000  deleteEvery=5   parallel=8")]
      [Run("len=25000  deleteEvery=6   parallel=8")]
      [Run("len=25000  deleteEvery=7   parallel=8")]
      [Run("len=25000  deleteEvery=8   parallel=8")]
      [Run("len=25000  deleteEvery=9   parallel=8")]
      [Run("len=25000  deleteEvery=10  parallel=8")]
      [Run("len=25000  deleteEvery=15  parallel=8")]
      [Run("len=25000  deleteEvery=16  parallel=8")]
      [Run("len=66560  deleteEvery=3   parallel=8")]
      [Run("len=66560  deleteEvery=7   parallel=8")]
      [Run("len=66560  deleteEvery=8   parallel=8")]
      [Run("len=66560  deleteEvery=15  parallel=8")]
      [Run("len=66560  deleteEvery=16  parallel=8")]
      public void ByteCorrectess(int len, int deleteEvery, int parallel)
      {
        using (var pile = new DefaultPile(NOPApplication.Instance))
        {
          pile.Start();

          var bag = new ConcurrentBag<PilePointer>();
          var deleted = 0;
          Parallel.For(0, len, new ParallelOptions{ MaxDegreeOfParallelism=parallel},
          (i) =>
          {
            var original = new byte[len-i];

            var pp = pile.Put(original);

            var got = pile.Get(pp) as byte[];

            Aver.AreEqual(original.Length, got.Length);

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




      [Run("cnt=1  size=1  parallel=1")] //warmup
      [Run("cnt=2000000  size=32  parallel=1")] //1 threads put 2,000,000 strings of 32 bytes in 1,143 msec at 1,749,781 ops/sec
                                  //1 threads got 2,000,000 strings of 32 bytes in 1,167 msec at 1,713,796 ops/sec
                                  //-------------------------------------------------------------------------------
                                  //1 threads put 2,000,000 strings of 32 bytes in 446 msec at 4,484,305 ops/sec
                                  //1 threads got 2,000,000 strings of 32 bytes in 349 msec at 5,730,659 ops/sec


      [Run("cnt=32000000  size=32  parallel=8")] //8 threads put 32,000,000 strings of 32 bytes in 5,198 msec at 6,156,214 ops/sec
                                  //8 threads got 32,000,000 strings of 32 bytes in 4,309 msec at 7,426,317 ops/sec
                                  //---------------------------------------------------------------------------------
                                  //8 threads put 32,000,000 strings of 32 bytes in 3,421 msec at 9,353,990 ops/sec
                                  //8 threads got 32,000,000 strings of 32 bytes in 2,072 msec at 15,444,015 ops/sec

      [Run("cnt=32000000  size=32  parallel=12")] //12 threads put 32,000,000 strings of 32 bytes in 4,536 msec at 7,054,674 ops/sec
                                   //12 threads got 32,000,000 strings of 32 bytes in 3,910 msec at 8,184,143 ops/sec
                                   //--------------------------------------------------------------------------------
                                   //12 threads put 32,000,000 strings of 32 bytes in 2,947 msec at 10,858,500 ops/sec
                                   //12 threads got 32,000,000 strings of 32 bytes in 1,818 msec at 17,601,760 ops/sec


      public void Strings(int CNT, int size, int parallel)
      {
        using (var pile = new DefaultPile(NOPApplication.Instance))
        {
          pile.Start();

          var str = new string('a', size);
          var pp = pile.Put(str);

          var sw = Stopwatch.StartNew();
          Parallel.For(0, CNT, new ParallelOptions{ MaxDegreeOfParallelism = parallel},
          (i) =>
          {
             pile.Put(str);
          });

          var el = sw.ElapsedMilliseconds;
          Console.WriteLine("{0:n0} threads put {1:n0} strings of {2:n0} bytes in {3:n0} msec at {4:n0} ops/sec".Args(parallel, CNT, size, el, CNT / (el/1000d)));


          sw.Restart();
          Parallel.For(0, CNT, new ParallelOptions{ MaxDegreeOfParallelism = parallel},
          (i) =>
          {
             pile.Get(pp);
          });

          el = sw.ElapsedMilliseconds;
          Console.WriteLine("{0:n0} threads got {1:n0} strings of {2:n0} bytes in {3:n0} msec at {4:n0} ops/sec".Args(parallel, CNT, size, el, CNT / (el/1000d)));

        }
      }


      [Run("cnt=1  size=1  parallel=1")] //warmup
      [Run("cnt=2000000  size=32  parallel=1")] //1 threads put 2,000,000 byte[32] in 1,307 msec at 1,530,222 ops/sec
                                  //1 threads got 2,000,000 byte[32] in 1,152 msec at 1,736,111 ops/sec
                                  //-------------------------------------------------------------------------------
                                  //1 threads put 2,000,000 byte[32] in 348 msec at 5,747,126 ops/sec
                                  //1 threads got 2,000,000 byte[32] in 235 msec at 8,510,638 ops/sec


      [Run("cnt=32000000  size=32  parallel=8")] //8 threads put 32,000,000 byte[32] in 5,741 msec at 5,573,942 ops/sec
                                  //8 threads got 32,000,000 byte[32] in 4,091 msec at 7,822,048 ops/sec
                                  //---------------------------------------------------------------------------------
                                  //8 threads put 32,000,000 byte[32] in 3,273 msec at 9,776,963 ops/sec
                                  //8 threads got 32,000,000 byte[32] in 2,450 msec at 13,061,224 ops/sec

      [Run("cnt=32000000  size=32  parallel=12")] //12 threads put 32,000,000 byte[32] in 5,008 msec at 6,389,776 ops/sec
                                   //12 threads got 32,000,000 byte[32] in 3,787 msec at 8,449,960 ops/sec
                                   //--------------------------------------------------------------------------------
                                   //12 threads put 32,000,000 byte[32] in 2,963 msec at 10,799,865 ops/sec
                                   //12 threads got 32,000,000 byte[32] in 2,253 msec at 14,203,285 ops/sec


      public void ByteBuf(int CNT, int size, int parallel)
      {
        using (var pile = new DefaultPile(NOPApplication.Instance))
        {
          pile.Start();

          var str = new byte[size];
          var pp = pile.Put(str);

          var sw = Stopwatch.StartNew();
          Parallel.For(0, CNT, new ParallelOptions{ MaxDegreeOfParallelism = parallel},
          (i) =>
          {
             pile.Put(str);
          });

          var el = sw.ElapsedMilliseconds;
          Console.WriteLine("{0:n0} threads put {1:n0} byte[{2:n0}] in {3:n0} msec at {4:n0} ops/sec".Args(parallel, CNT, size, el, CNT / (el/1000d)));


          sw.Restart();
          Parallel.For(0, CNT, new ParallelOptions{ MaxDegreeOfParallelism = parallel},
          (i) =>
          {
             pile.Get(pp);
          });

          el = sw.ElapsedMilliseconds;
          Console.WriteLine("{0:n0} threads got {1:n0} byte[{2:n0}] in {3:n0} msec at {4:n0} ops/sec".Args(parallel, CNT, size, el, CNT / (el/1000d)));
        }
      }
  }
}
