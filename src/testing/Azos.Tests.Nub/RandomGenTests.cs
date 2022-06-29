/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Threading.Tasks;

using Azos.Scripting;
using Azos.Instrumentation.Analytics;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class RandomGenTests
  {
    //This tests checks that no exceptions surface
    [Run]
    public void LoopParallel()
    {
      Parallel.For(0, 1000, (i) =>
      {

        if (i % 3 == 0)
          Ambient.Random.FeedExternalEntropySample(1);


        var n1 = Ambient.Random.NextScaledRandomInteger(0, 1000);
        if (i % 10 == 0)
          n1.See();
        System.Threading.Thread.SpinWait(Ambient.Random.NextScaledRandomInteger(10, 250));
      });
    }

    //check for no exceptions and correctness of length
    [Run]
    public void WebSafeStrings()
    {
      for (var i = 0; i < 25; i++)
      {
        var s = Ambient.Random.NextRandomWebSafeString();
        s.See();
        Aver.IsTrue(s.Length >= 16 && s.Length <= 32);
      }
    }

    //no exceptions
    [Run]
    public void NextRandom16Bytes()
    {
      for (var i = 0; i < 25; i++)
      {
        var a = Ambient.Random.NextRandom16Bytes;
        a.ToDumpString(DumpFormat.Hex).See();
      }
    }

    //Ensures distribution
    [Run]
    public void Hystogram_Dump_1()
    {
      const int CNT = 100000;
      const int MAX_RND = 100;

      var hist = new Histogram<int>("Random Histogram",
            new Dimension<int>(
                "ValBucket",
                partCount: MAX_RND,
                partitionFunc: (dim, v) =>
                {
                  return v;// % 100;
                  },
                partitionNameFunc: (i) => i.ToString()
            )
        );

      for (var i = 0; i < CNT; i++)
      {
        var r = Ambient.Random.NextScaledRandomInteger(0, MAX_RND);
        hist.Sample(r);
      }

      string output = hist.ToStringReport();
      output.See();

      var countPerRandomSeed = CNT / (double)MAX_RND;
      var tolerance = countPerRandomSeed * 0.15d;//Guarantees uniform random distribution. The lower the number, the more uniform gets
      foreach (var he in hist)
        Aver.IsTrue(he.Count > countPerRandomSeed - tolerance && he.Count < countPerRandomSeed + tolerance);
    }

  }
}
