using System;
using System.Threading;
using System.Threading.Tasks;

using Azos;

namespace TestBusinessLogic.Toy
{
    public static class GlueSpeed
    {
      public static void Echo(string node, int count, int parallel)
      {
        var client = new JokeContractClient(node);

        client.UnsecureEcho("aaa");

        var sw = System.Diagnostics.Stopwatch.StartNew();

        Parallel.For(0, count, new ParallelOptions{ MaxDegreeOfParallelism = parallel}, i =>
        {
          client.UnsecureEcho("aaa");
        });

        Console.WriteLine("Called Unsecure Echo {0} at {1:n0} ops/sec".Args(count, count / (sw.ElapsedMilliseconds / 1000d)));
      }


      public static void EchoThreaded(string node, int count, int parallel)
      {
        var tcount = count / parallel;

        var latch = 0;

        var threads = new Thread[parallel];
        for(var i=0; i < threads.Length; i++)
          threads[i] = new Thread( () =>
          {
            var client = new JokeContractClient(node);
            client.ReserveTransport = true;
            client.UnsecureEcho("aaa");

            while(Thread.VolatileRead(ref latch)==0);  //could have used Barrier class
            for(var j=0; j<tcount; j++)
              client.UnsecEchoMar("aaa");
              //client.Notify(null);

            client.Dispose();
          });

        foreach(var t in threads) t.Start();

        Thread.Sleep(2000);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        Thread.VolatileWrite(ref latch, 1);


        foreach(var t in threads) t.Join();

        Console.WriteLine("Called Unsecure Echo {0} at {1:n0} ops/sec".Args(count, count / (sw.ElapsedMilliseconds / 1000d)));
      }


    }
}
