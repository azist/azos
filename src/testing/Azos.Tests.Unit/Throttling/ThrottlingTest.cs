/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


#warning REVISE What to do
/***

using System;
using System.Linq;
using Azos.Scripting;

using Azos.Conf;

namespace Azos.Tests.Unit.Throttling
{
    [Runnable]
    class ThrottlingTestFixtureTest
    {
        private readonly int HALF_SEC_TICKS = 5000000;

        [Run]
        public void ThrottleSlidingWindowTest()
        {
            SlidingWindowThrottle throttler = new SlidingWindowThrottle(
                name: "test", throttleLimit: 100, throttleInterval: 3, bucketsPerSecond: 2);

            const int count = 8;

            int i;
            for (i=0; i < count; i++)
                throttler.Try(new DateTime((long)(HALF_SEC_TICKS * (i + 1))), i + 1);

            Aver.AreEqual(33, throttler.Sum);

            DateTime time = new DateTime((long)(HALF_SEC_TICKS * (count+1)));

            time = time.AddSeconds(2);
            Aver.AreEqual(true, throttler.Try(time, ++i)); // i=9
            Aver.AreEqual(17, throttler.Sum);

            time = time.AddSeconds(3);
            Aver.AreEqual(true, throttler.Try(time, ++i)); // i=10
            Aver.AreEqual(10, throttler.Sum);

            time = time.AddSeconds(9);
            Aver.AreEqual(true, throttler.Try(time, ++i)); // i=11
            Aver.AreEqual(11, throttler.Sum);

            time = time.AddSeconds(2);
            Aver.AreEqual(true, throttler.Try(time, ++i)); // i=12
            Aver.AreEqual(23, throttler.Sum);

            time = time.AddSeconds(2);
            Aver.AreEqual(true, throttler.Try(time, ++i)); // i=13
            Aver.AreEqual(25, throttler.Sum);

            time = time.AddSeconds(1);
            Aver.AreEqual(true, throttler.Try(time, ++i)); // i=14
            Aver.AreEqual(27, throttler.Sum);

            time = time.AddSeconds(2);
            Aver.AreEqual(true, throttler.Try(time, ++i)); // i=15
            Aver.AreEqual(29, throttler.Sum);

            Aver.AreEqual(29.0 / 3, throttler.Avg);
        }

        [Run]
        public void ThrottleTimeSpacingTest()
        {
            TimeSpacingThrottle throttler =
                new TimeSpacingThrottle(name: "test", throttleLimit: 5, throttleInterval: 10);

            DateTime time = new DateTime((long)HALF_SEC_TICKS);

            Aver.AreEqual(true, throttler.Try(time, 1));
            Aver.AreEqual(false, throttler.Try(time, 1));

            time = time.AddSeconds(1);
            Aver.AreEqual(false, throttler.Try(time, 1));
            time = time.AddSeconds(1);
            Aver.AreEqual(true, throttler.Try(time, 2));

            time = time.AddSeconds(2);
            Aver.AreEqual(false, throttler.Try(time, 1));
            time = time.AddSeconds(2);
            Aver.AreEqual(true, throttler.Try(time, 1));
        }

        [Run]
        public void ThrottleConfigTest()
        {
            var xml = @"
                <my-config ns='Azos.Throttling' type='$(/$ns).ThrottlingService' name='MyTestService'>
                    <throttle type='$(/$ns).TimeSpacingThrottle'   name='t1' limit='10' interval='1'/>
                    <throttle type='$(/$ns).TimeSpacingThrottle'   name='t2' limit='5'  interval='2'/>
                    <throttle type='$(/$ns).SlidingWindowThrottle' name='t3' limit='10' interval='1' buckets-per-sec='4'/>
                </my-config>";

            var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

            var svc = FactoryUtils.MakeAndConfigure(conf.Root) as ThrottlingService;
            svc.Start();

            var t1 = svc.Get<TimeSpacingThrottle>("t1");
            Aver.AreEqual(10, t1.Limit);
            Aver.AreEqual(1, t1.Interval);

            var t2 = svc.Get<TimeSpacingThrottle>("t2");
            Aver.AreEqual(5, t2.Limit);
            Aver.AreEqual(2, t2.Interval);

            var t3 = svc.Get<SlidingWindowThrottle>("t3");
            Aver.AreEqual(10, t3.Limit);
            Aver.AreEqual(1, t3.Interval);
            Aver.AreEqual(4, t3.BucketsPerSec);

            new TimeSpacingThrottle("t4", 100, unit: "kaka").Register(svc);
            var t4 = svc.Get<TimeSpacingThrottle>("t4");
            Aver.AreEqual("kaka", t4.Unit);

            svc.WaitForCompleteStop();
        }
    }
}



***/