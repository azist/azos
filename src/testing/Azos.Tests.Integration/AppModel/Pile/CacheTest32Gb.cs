/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Azos.ApplicationModel.Pile;
using Azos.Scripting;
using Azos.UTest.AppModel.Pile;

namespace Azos.Tests.Integration.AppModel.Pile
{
    [Runnable]
    public class CacheTest32Gb : HighMemoryLoadTest32RAM
    {
        [Run]
        public void T070_DoesNotSeeAgedOrExpired()
        {
            using (var cache = PileCacheTestCore.MakeCache())
            {
                var tA = cache.GetOrCreateTable<string>("A");
                var tB = cache.GetOrCreateTable<string>("B");
                var tC = cache.GetOrCreateTable<string>("C");
                tC.Options.DefaultMaxAgeSec = 4;
                var tD = cache.GetOrCreateTable<string>("D");

                Aver.IsTrue(PutResult.Inserted == tA.Put("key1", "value1"));    //does not expire by itself
                Aver.IsTrue(PutResult.Inserted == tB.Put("key1", "value1", 7)); //will expire in 7 seconds
                Aver.IsTrue(PutResult.Inserted == tC.Put("key1", "value1"));    //will expire in Options.DefaultMaxAgeSec
                Aver.IsTrue(PutResult.Inserted == tD.Put("key1", "value1", absoluteExpirationUTC: DateTime.UtcNow.AddSeconds(4)));//will expire at specific time

                Aver.AreObjectsEqual("value1", tA.Get("key1"));
                Aver.AreObjectsEqual("value1", tA.Get("key1", 3));

                Aver.AreObjectsEqual("value1", tB.Get("key1"));

                Aver.AreObjectsEqual("value1", tC.Get("key1"));

                Aver.AreObjectsEqual("value1", tD.Get("key1"));

                Thread.Sleep(20000);// wait long enough to cover a few swep cycles (that may be 5+sec long)

                Aver.AreObjectsEqual("value1", tA.Get("key1"));
                Aver.AreObjectsEqual(null, tA.Get("key1", 3)); //did not expire, but aged over get limit
                Aver.AreObjectsEqual(null, tB.Get("key1"));    //expired because of put with time limit
                Aver.AreObjectsEqual(null, tC.Get("key1"));    //expired because of Options
                Aver.AreObjectsEqual(null, tD.Get("key1"));    //expired because of absolute expiration on put
            }
        }

        [Run]
        public void T120_Rejuvenate()
        {
            using (var cache = PileCacheTestCore.MakeCache())
            {
                var tA = cache.GetOrCreateTable<string>("A");

                Aver.IsTrue(PutResult.Inserted == tA.Put("key1", "value1", 12));
                Aver.IsTrue(PutResult.Inserted == tA.Put("key2", "value2", 12));
                Aver.IsTrue(PutResult.Inserted == tA.Put("key3", "value3", 12));



                Aver.AreObjectsEqual("value1", tA.Get("key1"));
                Aver.AreObjectsEqual("value2", tA.Get("key2"));
                Aver.AreObjectsEqual("value3", tA.Get("key3"));

                for (var i = 0; i < 30; i++)
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("Second {0}   Load Factor {1}", i, tA.LoadFactor);
                    Aver.IsTrue(tA.Rejuvenate("key2"));
                }

                Aver.AreObjectsEqual(null, tA.Get("key1"));
                Aver.AreObjectsEqual("value2", tA.Get("key2")); //this is still here because it got rejuvenated
                Aver.AreObjectsEqual(null, tA.Get("key3"));

                Thread.Sleep(30000);
                Aver.AreObjectsEqual(null, tA.Get("key2")); //has died too
                Aver.AreEqual(0, tA.Count);
            }
        }

        [Run("cnt=1000000")]
        public void T130_KeyInt_ManyPutGet(int cnt)
        {
            PileCacheTestCore.KeyInt_ManyPutGet(cnt);
        }

        [Run("cnt=1000000")]
        public void T140_KeyGDID_ManyPutGet(int cnt)
        {
            PileCacheTestCore.KeyGDID_ManyPutGet(cnt);
        }

        [Run("cnt=1000000")]
        public void T150_KeyString_ManyPutGet(int cnt)
        {
            PileCacheTestCore.KeyString_ManyPutGet(cnt);
        }

        [Run("cnt=25  rec= 100000  payload=4096")] // 4096
        [Run("cnt=10  rec=3000000  payload=16")]
        [Run("cnt=10  rec=   1000  payload=1048576")] // 1024 * 1024
        [Run("cnt=10  rec=    200  payload=8388608")] // 8 * 1024 * 1024
        public void T160_ResizeTable(int cnt, int rec, int payload)
        {
            PileCacheTestCore.ResizeTable(cnt, rec, payload);
        }

        [Run("cnt=1000000  tbls=1")]
        [Run("cnt=1000000  tbls=16")]
        [Run("cnt=1000000  tbls=512")]
        public void T190_FID_PutGetCorrectness(int cnt, int tbls)
        {
            PileCacheTestCore.FID_PutGetCorrectness(cnt, tbls);
        }

        [Run("workers=5  tables=7  putCount=1000  durationSec=20")]
        public void T9000000_ParalellGetPutRemove(int workers, int tables, int putCount, int durationSec)
        {
            PileCacheTestCore.ParalellGetPutRemove(workers, tables, putCount, durationSec);
        }
    }
}
