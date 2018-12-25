/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using Azos.Scripting;

using System.Threading;
using System.Threading.Tasks;

using Azos.Collections;
using Azos.Apps;

namespace Azos.Tests.Unit.Collections
{

  [Runnable]
  public class CappedSetTests
  {
    [Run]
    public void Basic_NoComparer()
    {
      using(var set = new CappedSet<string>(NOPApplication.Instance))
      {
        Aver.IsTrue( set.Put("Gagarin") );
        Aver.IsTrue( set.Put("Titov") );
        Aver.IsTrue( set.Put("Glenn") );

        Aver.IsTrue( set.Contains("Glenn") );
        Aver.IsFalse( set.Contains("GLENN") );

        Aver.IsFalse( set.Put("Titov") );
        Aver.AreEqual(3, set.Count );
        Aver.AreEqual(3, set.ToArray().Length );

        DateTime cd;
        Aver.IsTrue( set.Get("Titov", out cd) );
        Aver.IsTrue( (Ambient.UTCNow - cd).TotalSeconds < 2d);//unless machine freezes :(

        Aver.IsFalse( set.Get("Neverflew", out cd) );

        set.Clear();

        Aver.AreEqual(0, set.Count );
        Aver.AreEqual(0, set.ToArray().Length );

        Aver.IsTrue( set.Put("Gagarin") );
        Aver.IsTrue( set.Put("GAGARIN") );

        Aver.IsFalse( set.Put("Gagarin") );
        Aver.IsTrue( set.Remove("Gagarin") );
        Aver.IsTrue( set.Put("Gagarin") );
      }
    }

    [Run]
    public void Basic_Comparer()
    {
      using(var set = new CappedSet<string>(NOPApplication.Instance, StringComparer.OrdinalIgnoreCase))
      {
        Aver.IsTrue( set.Put("Gagarin") );
        Aver.IsTrue( set.Put("Titov") );
        Aver.IsTrue( set.Put("Glenn") );

        Aver.IsTrue( set.Contains("Glenn") );
        Aver.IsTrue( set.Contains("GLENN") );

        Aver.IsFalse( set.Put("Titov") );
        Aver.AreEqual(3, set.Count );
        Aver.AreEqual(3, set.ToArray().Length );

        DateTime cd;
        Aver.IsTrue( set.Get("Titov", out cd) );
        Aver.IsTrue( (Ambient.UTCNow - cd).TotalSeconds < 2d);//unless machine freezes :(

        Aver.IsFalse( set.Get("Neverflew", out cd) );

        set.Clear();

        Aver.AreEqual(0, set.Count );
        Aver.AreEqual(0, set.ToArray().Length );

        Aver.IsTrue( set.Put("Gagarin") );
        Aver.IsFalse( set.Put("GAGARIN") );

        Aver.IsTrue( set.Remove("Gagarin") );
        Aver.IsTrue( set.Put("Gagarin") );
      }
    }


    //The test requires    CappedSet.THREAD_GRANULARITY_MS to be around 5000ms
    [Run]
    public void Max_Age()
    {
      using(var set = new CappedSet<int>(NOPApplication.Instance))
      {
        set.TimeLimitSec = 10;

        for(var i=0; i<1000; i++)
         set.Put(i);

        Aver.AreEqual(1000, set.Count);
        for(var i=0; i<40; i++)
        {
          Console.WriteLine(" count: {0}".Args(set.Count));
          Thread.Sleep(1000);
        }
        Aver.AreEqual(0, set.Count);

        Aver.IsFalse(set.Any());
      }
    }

    //The test requires    CappedSet.THREAD_GRANULARITY_MS to be around 5000ms
    [Run]
    public void Max_Size()
    {
      using(var set = new CappedSet<int>(NOPApplication.Instance))
      {
        set.SizeLimit = 10000;

        for(var i=0; i<150000; i++)
         set.Put(i);

        for(var i=0; i<40; i++)
        {
          Console.WriteLine(" count: {0}".Args(set.Count));
          Thread.Sleep(1000);
        }
        Aver.IsTrue(set.Count <= (set.SizeLimit + 1024));//1024 margin of error
      }
    }

    //The test requires    CappedSet.THREAD_GRANULARITY_MS to be around 5000ms
    [Run]
    public void Max_SizeandTime()
    {
      using(var set = new CappedSet<int>(NOPApplication.Instance))
      {
        set.TimeLimitSec = 30;//
        set.SizeLimit = 7000;

        for(var i=0; i<150000; i++)
         set.Put(i);

        for(var i=0; i<40; i++)
        {
          Console.WriteLine(" count: {0}".Args(set.Count));
          Thread.Sleep(1000);
        }
        Aver.AreEqual(0 , set.Count);
      }
    }



    [Run]
    public void Mutithreaded()
    {
      using(var set = new CappedSet<int>(NOPApplication.Instance))
      {
        set.TimeLimitSec = 30;//
        set.SizeLimit = 7000;

        Parallel.For(0, 1500000, (i) => { set.Put(i); set.Contains(i); });

        for(var i=0; i<40; i++)
        {
          Console.WriteLine(" count: {0}".Args(set.Count));
          Thread.Sleep(1000);
        }
        Aver.AreEqual(0, set.Count);
      }
    }


  }
}
