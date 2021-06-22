/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Azos.Platform;
using Azos.Scripting;

namespace Azos.Tests.Nub
{
  /// <summary>
  /// Due to incongruencies in .NET COre implementation vs .Net Fx these tests cover logical call flow runtime soundness
  /// https://github.com/dotnet/corefx/issues/34489
  /// </summary>
  [Runnable]
  public class AsyncFlowTests
  {
    private class directData
    {
      public string Tag{ get;set;}
    }

    private class directDataWrap
    {
      public directData Data { get; set; }
    }

    private static AsyncLocal<directData> ats_Local = new AsyncLocal<directData>();

    [Run]
    public void OneSyncMethodFlow_DirectData()
    {
      var data = new directData{ Tag="abcd1" };
      ats_Local.Value = data;
      Aver.AreEqual("abcd1", ats_Local.Value.Tag);
    }

    [Run]
    public async Task OneAsyncMethodFlow_DirectData()
    {
      var data = new directData { Tag = "a234" };
      ats_Local.Value = data;
      await Task.Delay(100);
      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("a234", ats_Local.Value.Tag);
      await Task.Delay(100);
      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("a234", ats_Local.Value.Tag);
      await Task.Delay(100);
      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("a234", ats_Local.Value.Tag);
    }

    [Run]
    public async Task OneAsyncMethodFlow_DirectData_ConfigureAwait()
    {
      var data = new directData { Tag = "a7931" };
      ats_Local.Value = data;
      await Task.Delay(100).ConfigureAwait(false);
      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("a7931", ats_Local.Value.Tag);
      await Task.Delay(100).ConfigureAwait(false);
      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("a7931", ats_Local.Value.Tag);
      await Task.Delay(100).ConfigureAwait(false);
      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("a7931", ats_Local.Value.Tag);
    }

    [Run]
    public async Task OneAsyncMethodFlow_DirectData_Loop()
    {
      var data = new directData { Tag = "xcv" };
      ats_Local.Value = data;
      for(var i=0; i<15; i++)
      {
        await Task.Delay(39);
        TaskUtils.LoadAllCoresFor(50);
        await Task.Yield();
        Thread.CurrentThread.ManagedThreadId.See();
        Aver.AreEqual("xcv", ats_Local.Value.Tag);
      }
    }

    [Run]
    public async Task AsyncNestedFlow_DirectData()
    {
      var data = new directData { Tag = "x800" };
      ats_Local.Value = data;

      await Task.Yield();
      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("x800", ats_Local.Value.Tag);
      TaskUtils.LoadAllCoresFor(500);
      await m2();
      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("x800", ats_Local.Value.Tag);
    }

    private async Task<int> m2()
    {
      TaskUtils.LoadAllCoresFor(250);

      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("x800", ats_Local.Value.Tag);

      TaskUtils.LoadAllCoresFor(500);
      var i = await m3();

      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("x800", ats_Local.Value.Tag); //<--- although m3() changed it to x900, when we get back here, we see old context
      return i+Ambient.Random.NextRandomInteger;
    }

    private async Task<int> m3()
    {
      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("x800", ats_Local.Value.Tag);
      await Task.Yield();
      var data = new directData { Tag = "x900" };//allocate completely new!!!
      ats_Local.Value = data; //<--- this will NOT be seen outside of this method, because we set AsyncLocal directly without wrap
      TaskUtils.LoadAllCoresFor(500);
      await Task.Yield();
      Thread.CurrentThread.ManagedThreadId.See();
      TaskUtils.LoadAllCoresFor(500);
      await Task.Yield();
      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("x900", ats_Local.Value.Tag);
      return 900;
    }

    private static AsyncLocal<directDataWrap> ats_LocalWrap = new AsyncLocal<directDataWrap>();

    [Run]
    public async Task AsyncNestedFlow_DirectDataWrap()
    {
      var data = new directDataWrap{ Data= new directData { Tag = "x1000" }};
      ats_LocalWrap.Value = data;

      await Task.Yield();
      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("x1000", ats_LocalWrap.Value.Data.Tag);
      TaskUtils.LoadAllCoresFor(500);
      await m2wrap();
      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("x2000", ats_LocalWrap.Value.Data.Tag);//<--- m2wrap() changed inner value to x2000 via m3wrap(), so we see it
    }

    private async Task<int> m2wrap()
    {
      TaskUtils.LoadAllCoresFor(250);

      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("x1000", ats_LocalWrap.Value.Data.Tag);

      TaskUtils.LoadAllCoresFor(500);
      var i = await m3wrap();

      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("x2000", ats_LocalWrap.Value.Data.Tag); //<--- m3wrap() changed inner value to x2000, so we see it
      return i + Ambient.Random.NextRandomInteger;
    }

    private async Task<int> m3wrap()
    {
      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("x1000", ats_LocalWrap.Value.Data.Tag);
      await Task.Yield();
      var data = new directData { Tag = "x2000" };
      ats_LocalWrap.Value.Data = data; //<-- here is the difference!!!
      TaskUtils.LoadAllCoresFor(500);
      await Task.Yield();
      Thread.CurrentThread.ManagedThreadId.See();
      TaskUtils.LoadAllCoresFor(500);
      await Task.Yield();
      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("x2000", ats_LocalWrap.Value.Data.Tag);
      return 900;
    }

    private static AsyncFlowMutableLocal<directData> ats_Mutable = new AsyncFlowMutableLocal<directData>();

    [Run]
    public async Task AsyncFlowMutableNestedFlow_DirectData()
    {
      var data = new directData { Tag = "z1" };
      ats_Mutable.Value = data;

      await Task.Yield();
      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("z1", ats_Mutable.Value.Tag);
      TaskUtils.LoadAllCoresFor(500);
      await m2mutable();
      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("z3", ats_Mutable.Value.Tag);  //<-- get the value mutated in inner parts of the flow
    }

    private async Task<int> m2mutable()
    {
      TaskUtils.LoadAllCoresFor(250);

      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("z1", ats_Mutable.Value.Tag);

      TaskUtils.LoadAllCoresFor(500);
      var i = await m3mutable();

      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("z3", ats_Mutable.Value.Tag);
      return i + Ambient.Random.NextRandomInteger;
    }

    private async Task<int> m3mutable()
    {
      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("z1", ats_Mutable.Value.Tag);
      await Task.Yield();
      var data = new directData { Tag = "z3" };
      ats_Mutable.Value = data; //<--- this will be seen outside of this method
      TaskUtils.LoadAllCoresFor(500);
      await Task.Yield();
      Thread.CurrentThread.ManagedThreadId.See();
      TaskUtils.LoadAllCoresFor(500);
      await Task.Yield();
      Thread.CurrentThread.ManagedThreadId.See();
      Aver.AreEqual("z3", ats_Mutable.Value.Tag);
      return 900;
    }

    private static AsyncFlowMutableLocal<directData> ats_MutableParallel = new AsyncFlowMutableLocal<directData>();
    private volatile bool m_Signal;

    //this test emulates different physical originating call contexts
    [Run("capture=true")]
    [Run("capture=false")]
    [Run("capture=true")]
    [Run("capture=false")]
    public void AsyncFlowMutableParallelThreads(bool capture)
    {
      var list = new List<Thread>();
      for(var i=0; i<System.Environment.ProcessorCount * 2; i++)
      {
        var t = new Thread(threadBody);
        t.IsBackground = false;
        list.Add(t);
        t.Start(capture);
      }

      m_Signal = true;

      list.ForEach(t => t.Join());
    }

    private void threadBody(object ptrCapture) //this is needed to ensure different physical thread root caller context
    {
      var capture = (bool)ptrCapture;
      while(!m_Signal) Thread.SpinWait(100);

      var self = "THREAD-{0}".Args(Guid.NewGuid());//create thread-specific value

      var data = new directData { Tag = self };
      ats_MutableParallel.Value = data;

      Aver.AreEqual(self, ats_MutableParallel.Value.Tag);//still equal self
      var task = m1mutate(capture);
      var innerCahngedValue = task.GetAwaiter().GetResult();//this triggers all sorts of processing, async chain etc..

      //but changes from below are propagated here
      Aver.AreNotEqual(self, ats_MutableParallel.Value.Tag);
      Aver.AreEqual(innerCahngedValue, ats_MutableParallel.Value.Tag);
    }

    private async Task<string> m1mutate(bool capture)//every flow roots in its own PHYSICAL thread, and it still maintains proper call flow
    {
      var d1 = Guid.NewGuid().ToString();
      var d2 = Guid.NewGuid().ToString();

      for(var i=0; i<100; i++)
      {
        var data = new directData { Tag = d1 }; //set D1
        ats_MutableParallel.Value = data;

        await Task.Yield();
        Aver.AreEqual(d1, ats_MutableParallel.Value.Tag);//still D1

        await m2mutate(d2, capture).ConfigureAwait(capture); //D2 is set from under another async
        await Task.Yield();

        Aver.AreEqual(d2, ats_MutableParallel.Value.Tag);  //<-- get the value mutated in inner parts of the flow
      }

      return d2;//return LAST value set = D2
    }

    private async Task m2mutate(string v, bool capture)
    {
      Ambient.Random.NextRandomWebSafeString();//do something
      await Task.Delay(10).ConfigureAwait(capture);
      Ambient.Random.NextRandomWebSafeString();//do something
      await Task.Yield();
      ats_MutableParallel.Value = new directData { Tag = v }; //allocate completely new value
      await Task.Yield();
    }

  }
}


