/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using Azos.Scripting;

using Azos.Tests.Unit.Pile;
using Azos.Pile;

namespace Azos.Tests.Integration.Pile
{
    [Runnable]
    public class PileTest32Gb : HighMemoryLoadTest32RAM
    {
        [Run("fromSize=32  toSize=3200   fromObjCount=1  toObjCount=50   taskCount=8")]
        [Run("fromSize=32  toSize=12800  fromObjCount=1  toObjCount=100  taskCount=16")]
        public void PutGetDelete_Parallel(int fromSize, int toSize, int fromObjCount, int toObjCount, int taskCount)
        {
            PileCacheTestCore.PutGetDelete_Parallel(fromSize, toSize, fromObjCount, toObjCount, taskCount);
        }

        [Run("fromSize=32  toSize=64000  fromObjCount=300  toObjCount=1000")]
        public void PutGetDelete_Sequential(int fromSize, int toSize, int fromObjCount, int toObjCount)
        {
            PileCacheTestCore.PutGetDelete_Sequential(fromSize, toSize, fromObjCount, toObjCount);
        }

        [Run]
        public void Parallel_PutGetDelete_Random()
        {
            const int PUTTER_CNT = 2, PUTTER_OP_CNT = 2 * 10000;
            const int GETTER_CNT = 6, GETTER_OP_CNT = 2 * 30000;
            const int DELETER_CNT = 2, DELETER_OP_CNT = 2 * 10000;

            var data = new ConcurrentDictionary<PilePointer, string>();

            var getAccessViolations = new ConcurrentDictionary<int, int>();
            var deleteAccessViolations = new ConcurrentDictionary<int, int>();

            using (var pile = new DefaultPile())
            {
                pile.Start();

                var ipile = pile as IPile;

                // putter tasks
                var putters = new Task[PUTTER_CNT];
                for (int it = 0; it < PUTTER_CNT; it++)
                {
                    var task = new Task(() =>
                    {

                        for (int i = 0; i < PUTTER_OP_CNT; i++)
                        {
                            var str = Azos.Text.NaturalTextGenerator.Generate();
                            var pp = ipile.Put(str);
                            data.TryAdd(pp, str);
                        }

                    });

                    putters[it] = task;
                }

                // getter tasks
                var getters = new Task[GETTER_CNT];
                for (int it = 0; it < GETTER_CNT; it++)
                {
                    var task = new Task(() =>
                    {

                        for (int i = 0; i < GETTER_OP_CNT; i++)
                        {
                            if (data.Count == 0)
                            {
                                System.Threading.Thread.Yield();
                                continue;
                            }
                            var idx = App.Random.NextScaledRandomInteger(0, data.Count - 1);
                            var kvp = data.ElementAt(idx);
                            try
                            {

                                var str = ipile.Get(kvp.Key);
                                Aver.AreObjectsEqual(str, kvp.Value);
                            }
                            catch (PileAccessViolationException)
                            {
                                getAccessViolations.AddOrUpdate(System.Threading.Thread.CurrentThread.ManagedThreadId, 1, (mid, val) => val + 1);
                            }
                        }
                    });
                    getters[it] = task;
                }

                // deleter tasks
                var deleters = new Task[DELETER_CNT];
                for (int it = 0; it < DELETER_CNT; it++)
                {
                    var task = new Task(() =>
                    {

                        for (int i = 0; i < DELETER_OP_CNT; i++)
                        {
                            if (data.Count == 0)
                            {
                                System.Threading.Thread.Yield();
                                continue;
                            }
                            var idx = App.Random.NextScaledRandomInteger(0, data.Count - 1);
                            var kvp = data.ElementAt(idx);
                            try
                            {
                                ipile.Delete(kvp.Key);
                            }
                            catch (PileAccessViolationException)
                            {
                                deleteAccessViolations.AddOrUpdate(System.Threading.Thread.CurrentThread.ManagedThreadId, 1, (mid, val) => val + 1);
                            }
                        }
                    });
                    deleters[it] = task;
                }


                foreach (var task in putters) task.Start();
                foreach (var task in getters) task.Start();
                foreach (var task in deleters) task.Start();


                Task.WaitAll(putters.Concat(getters).Concat(deleters).ToArray());

                foreach (var kvp in getAccessViolations)
                    Console.WriteLine("Get thread '{0}' {1:n0} times accessed deleted pointer", kvp.Key, kvp.Value);

                foreach (var kvp in deleteAccessViolations)
                    Console.WriteLine("Del thread '{0}' {1:n0} times accessed deleted pointer", kvp.Key, kvp.Value);
            }
        }

        [Run("isParallel=false  cnt=1000000  minSz=0      maxSz=40      speed=true")]
        [Run("isParallel=false  cnt=100000   minSz=0      maxSz=50000   speed=true")]
        [Run("isParallel=false  cnt=50000    minSz=0      maxSz=150000  speed=true")]
        [Run("isParallel=false  cnt=10000    minSz=70000  maxSz=150000  speed=true")]

        [Run("isParallel=true  cnt=1000000  minSz=0      maxSz=40      speed=true")]
        [Run("isParallel=true  cnt=100000   minSz=0      maxSz=50000   speed=true")]
        [Run("isParallel=true  cnt=10000    minSz=70000  maxSz=150000  speed=true")]

        [Run("isParallel=false  cnt=1000000  minSz=0      maxSz=40      speed=false")]
        [Run("isParallel=false  cnt=100000   minSz=0      maxSz=50000   speed=false")]
        [Run("isParallel=false  cnt=10000    minSz=70000  maxSz=150000  speed=false")]
        [Run("isParallel=false  cnt=50000    minSz=0      maxSz=150000  speed=false")]

        [Run("isParallel=true  cnt=1000000  minSz=0      maxSz=40      speed=false")]
        [Run("isParallel=true  cnt=100000   minSz=0      maxSz=50000   speed=false")]
        [Run("isParallel=true  cnt=10000    minSz=70000  maxSz=150000  speed=false")]
        public void VarSizes_Checkboard(bool isParallel, int cnt, int minSz, int maxSz, bool speed)
        {
            PileCacheTestCore.VarSizes_Checkboard(isParallel, cnt, minSz, maxSz, speed);
        }

        [Run("isParallel=false  cnt=1000000  minSz=0      maxSz=256     speed=false  rnd=true")]
        [Run("isParallel=false  cnt=250000   minSz=0      maxSz=8000    speed=false  rnd=true")]
        [Run("isParallel=false  cnt=150000   minSz=0      maxSz=24000   speed=false  rnd=true")]
        [Run("isParallel=false  cnt=21000    minSz=65000  maxSz=129000  speed=false  rnd=true")]

        [Run("isParallel=true  cnt=1000000  minSz=0      maxSz=256     speed=false  rnd=true")]
        [Run("isParallel=true  cnt=250000   minSz=0      maxSz=8000    speed=false  rnd=true")]
        [Run("isParallel=true  cnt=150000   minSz=0      maxSz=24000   speed=false  rnd=true")]
        [Run("isParallel=true  cnt=21000    minSz=65000  maxSz=129000  speed=false  rnd=true")]

        [Run("isParallel=false  cnt=1000000  minSz=0      maxSz=256     speed=true  rnd=true")]
        [Run("isParallel=false  cnt=250000   minSz=0      maxSz=8000    speed=true  rnd=true")]
        [Run("isParallel=false  cnt=150000   minSz=0      maxSz=24000   speed=true  rnd=true")]
        [Run("isParallel=false  cnt=21000    minSz=65000  maxSz=129000  speed=true  rnd=true")]

        [Run("isParallel=true  cnt=1000000  minSz=0      maxSz=256     speed=true  rnd=true")]
        [Run("isParallel=true  cnt=250000   minSz=0      maxSz=8000    speed=true  rnd=true")]
        [Run("isParallel=true  cnt=150000   minSz=0      maxSz=24000   speed=true  rnd=true")]
        [Run("isParallel=true  cnt=21000    minSz=65000  maxSz=129000  speed=true  rnd=true")]

        [Run("isParallel=false  cnt=1000000  minSz=0      maxSz=256     speed=false  rnd=false")]
        [Run("isParallel=false  cnt=250000   minSz=0      maxSz=8000    speed=false  rnd=false")]
        [Run("isParallel=false  cnt=150000   minSz=0      maxSz=24000   speed=false  rnd=false")]
        [Run("isParallel=false  cnt=12000    minSz=65000  maxSz=129000  speed=false  rnd=false")]

        [Run("isParallel=true  cnt=1000000  minSz=0      maxSz=256     speed=false  rnd=false")]
        [Run("isParallel=true  cnt=250000   minSz=0      maxSz=8000    speed=false  rnd=false")]
        [Run("isParallel=true  cnt=12000    minSz=65000  maxSz=129000  speed=false  rnd=false")]

        [Run("isParallel=false  cnt=1000000  minSz=0      maxSz=256     speed=true  rnd=false")]
        [Run("isParallel=false  cnt=250000   minSz=0      maxSz=8000    speed=true  rnd=false")]
        [Run("isParallel=false  cnt=150000   minSz=0      maxSz=24000   speed=true  rnd=false")]
        [Run("isParallel=false  cnt=12000    minSz=65000  maxSz=129000  speed=true  rnd=false")]

        [Run("isParallel=true  cnt=250000  minSz=0  maxSz=8000  speed=true  rnd=false")]
        public void VarSizes_Increasing_Random(bool isParallel, int cnt, int minSz, int maxSz, bool speed, bool rnd)
        {
            PileCacheTestCore.VarSizes_Increasing_Random(isParallel, cnt, minSz, maxSz, speed, rnd);
        }
    }
}
