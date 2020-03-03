using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Azos.Scripting;
using Azos.Time;

namespace Azos.Tests.Nub.Time
{
  [Runnable]
  public class TimeterTests
  {
    [Run]
    public void Test1()
    {
      var time = Timeter.StartNew();
      Aver.IsTrue(time.IsStarted);

      Thread.Sleep(500);

      var ems = time.ElapsedMs;
      var el = time.Elapsed;

      Aver.IsTrue(ems >= 500 && ems <= 525);

      Aver.IsTrue(el.TotalMilliseconds >= 500 && el.TotalMilliseconds <= 525);

      "Elapsed {0} ms / {1} timespan".SeeArgs(ems, el);
    }

    [Run]
    public void Test2()
    {
      var time = new Timeter();
      Aver.IsFalse(time.IsStarted);
      Aver.AreEqual(0, time.ElapsedRaw);
      Aver.AreEqual(0, time.ElapsedMs);
      Thread.Sleep(500);
      Aver.IsFalse(time.IsStarted);
      Aver.AreEqual(0, time.ElapsedRaw);
      Aver.AreEqual(0, time.ElapsedMs);

      time.Start();
      Aver.IsTrue(time.IsStarted);
      Thread.Sleep(500);
      time.Stop();

      var ems = time.ElapsedMs;
      var el = time.Elapsed;
      Aver.IsFalse(time.IsStarted);
      Aver.IsTrue(ems >= 500 && ems <= 525);
      Aver.IsTrue(el.TotalMilliseconds >= 500 && el.TotalMilliseconds <= 525);
      Thread.Sleep(500);

      ems = time.ElapsedMs;
      el = time.Elapsed;
      Aver.IsFalse(time.IsStarted);
      Aver.IsTrue(ems >= 500 && ems <= 525);
      Aver.IsTrue(el.TotalMilliseconds >= 500 && el.TotalMilliseconds <= 525);

      time.Start();
      Aver.IsTrue(time.IsStarted);
      Thread.Sleep(250);

      ems = time.ElapsedMs;
      el = time.Elapsed;
      Aver.IsTrue(time.IsStarted);
      Aver.IsTrue(ems >= 750 && ems <= 800);
      Aver.IsTrue(el.TotalMilliseconds >= 750 && el.TotalMilliseconds <= 800);
      time.Stop();
      Aver.IsFalse(time.IsStarted);
      ems = time.ElapsedMs;
      el = time.Elapsed;
      "Elapsed {0} ms / {1} timespan".SeeArgs(ems, el);
      Aver.IsTrue(ems >= 750 && ems <= 800);
      Aver.IsTrue(el.TotalMilliseconds >= 750 && el.TotalMilliseconds <= 800);


      Thread.Sleep(300);

      ems = time.ElapsedMs;
      el = time.Elapsed;
      "Elapsed {0} ms / {1} timespan".SeeArgs(ems, el);
      Aver.IsFalse(time.IsStarted);
      Aver.IsTrue(ems >= 750 && ems <= 800);
      Aver.IsTrue(el.TotalMilliseconds >= 750 && el.TotalMilliseconds <= 800);

    }
  }
}
