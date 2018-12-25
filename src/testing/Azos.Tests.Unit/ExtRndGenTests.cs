/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Scripting;
using Azos.Instrumentation.Analytics;

namespace Azos.Tests.Unit
{
    [Runnable(TRUN.BASE)]
    public class ExtRndTests
    {
        [Run]
        public void Loop1()
        {
            var rnd = new Random();
            for(var i=0; i<1000;i++)
            {
    if (i%3==0)
    Ambient.Random.FeedExternalEntropySample(Platform.Computer.CurrentProcessorUsagePct);

               var n1 = Ambient.Random.NextScaledRandomInteger(0,100);
               var n2 = rnd.Next(100);
               Console.WriteLine( "{0} {1}".Args(n1, n2) );

    //           System.Threading.Thread.Sleep(n2);
            }
        }

        [Run]
        public void LoopParallel()
        {
            Parallel.For(0, 1000, (i)=>
            {

    if (i%3==0)
                Ambient.Random.FeedExternalEntropySample(Platform.Computer.CurrentProcessorUsagePct);


               var n1 = Ambient.Random.NextScaledRandomInteger(0,1000);
               Console.WriteLine( n1 );
               System.Threading.Thread.SpinWait(Ambient.Random.NextScaledRandomInteger(10,250));
            });
        }

        [Run]
        public void WebSafeStrings()
        {
          for(var i=0; i<25; i++)
          {
            var s = Ambient.Random.NextRandomWebSafeString();
            Console.WriteLine(s);
            Aver.IsTrue( s.Length >= 16 && s.Length <= 32);
          }
        }

        [Run]
        public void NextRandom16Bytes()
        {
          for(var i=0; i<25; i++)
          {
            var a = Ambient.Random.NextRandom16Bytes;
            Console.WriteLine(a.ToDumpString(DumpFormat.Hex));
          }
        }


        [Run]
        public void Hystogram_Dump_1()
        {
          const int CNT = 100000;
          const int MAX_RND = 100;

          var hist = new Histogram<int>("Random Histogram",
                new Dimension<int>(
                    "ValBucket",
                    partCount: MAX_RND,
                    partitionFunc: (dim, v) => {
                        return v;// % 100;
                    },
                    partitionNameFunc: (i) => i.ToString()
                )
            );

         // var rnd = new Random();
          for(var i=0; i<CNT; i++)
          {
         //   var r = rnd.Next(100);// App.Random.NextScaledRandomInteger(0,100);
            var r = Ambient.Random.NextScaledRandomInteger(0, MAX_RND);
            hist.Sample( r );
         //   App.Random.FeedExternalEntropySample( (int)Platform.Computer.GetMemoryStatus().AvailablePhysicalBytes);
          }

          string output = hist.ToStringReport();
          Console.WriteLine( output );

          var countPerRandomSeed = CNT / (double)MAX_RND;
          var tolerance = countPerRandomSeed * 0.15d;//Guarantees uniform random distribution. The lower the number, the more uniform gets
          foreach(var he in hist)
           Aver.IsTrue( he.Count >countPerRandomSeed-tolerance && he.Count < countPerRandomSeed+tolerance);
        }

    }
}
