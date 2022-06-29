/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Scripting;
using System.Threading;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class TimedCallTests
  {
    [Run]
    public void Basic_1()
    {
      var was = false;
      TimedCall.Run(c => { /* does nothing */ }, 1000, () => was = true);
      Aver.IsFalse(was);

      TimedCall.Run(c => Thread.Sleep(750), 10, () => was = true);
      Aver.IsTrue(was);
    }

    [Run]
    public void Basic_2()
    {
      var was = false;
      var got = TimedCall.Run(c => { /* does nothing */ return 29;}, 1000, () => was = true);
      Aver.IsFalse(was);
      Aver.AreEqual(29, got);

      got = TimedCall.Run(c => {Thread.Sleep(750); return 175;}, 10, () => was = true);
      Aver.IsTrue(was);
      Aver.AreEqual(175, got);
    }

    [Run]
    public void Cancellation_1()
    {
      var cts = new CancellationTokenSource();

      var was = false;
      TimedCall.Run(c => { /* does nothing */ }, 1000, () => was = true, cts.Token);
      Aver.IsFalse(was);

      TimedCall.Run(c => Thread.Sleep(750), 10, () => was = true, cts.Token);
      Aver.IsTrue(was);
    }

    [Run]
    public void Cancellation_2()
    {
      var cts = new CancellationTokenSource();

      cts.Cancel();

      var was = false;
      TimedCall.Run(c => { /* does nothing */ }, 1000, () => was = true, cts.Token);
      Aver.IsFalse(was);

      TimedCall.Run(c => Thread.Sleep(750), 10, () => was = true, cts.Token);
      Aver.IsFalse(was);//it was canceled already
    }

    [Run, Aver.RunTime(MaxMs = 100)]
    public void Cancellation_3()
    {
      var cts = new CancellationTokenSource();

      cts.Cancel();

      var was = false;
      TimedCall.Run(c => { for(var i=0; i<100 && !c.IsCancellationRequested; i++) Thread.Sleep(25);}, 25_000, () => was = true, cts.Token);
      Aver.IsFalse(was);//it was canceled already
    }

  }
}
