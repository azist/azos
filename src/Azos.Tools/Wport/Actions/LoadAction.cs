/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Azos.Conf;
using Azos.Time;

namespace Azos.Tools.Wport.Actions
{
  public sealed class LoadAction : ActionBase
  {
    public LoadAction(IApplication application, Uri uri) : base(application, uri)
    {
    }

    [Config("$t;$threads;$thread-count", Default = 1)]
    public int ThreadCount{ get; set;}

    [Config("$c;$count", Default = 100)]
    public int TotalRequestCount { get; set; }

    public override void Configure(IConfigSectionNode node)
    {
      node = node["load", "l"];
      base.Configure(node);
    }

    private long m_RunningTotal;
    private long m_RunningTotalOK;
    private long m_RunningTotalError;

    public override void Run()
    {
      m_RunningTotal = 0;
      var threadCount = ThreadCount.KeepBetween(1, 1024);
      var totalCount = TotalRequestCount.KeepBetween(1, 2_000_000_000);
      if (threadCount > totalCount) threadCount = totalCount;
      var perThread = totalCount / threadCount;
      totalCount = perThread * threadCount;

      Console.WriteLine("Starting processing of {0:n0} requests on {1:n0} threads, each doing {2:n0} calls".Args(totalCount, threadCount, perThread));

      var tasks = new List<Task>();
      for(int i = 0; i < threadCount; i++)
        tasks.Add(new worker(this, perThread).Task);


      var time = Timeter.StartNew();
      while(tasks.Any(t => !t.IsCompleted) && App.Active)
      {
        Thread.Sleep(1000);
        var totalNow = Interlocked.Read(ref m_RunningTotal);
        Console.WriteLine("... made {0,10:n0} requests in {1:n} sec at {2:n0} ops/sec; OK: {3:n0} Error: {4:n0}".Args(totalNow, time.ElapsedSec, totalNow / time.ElapsedSec,
              Interlocked.Read(ref m_RunningTotalOK), Interlocked.Read(ref m_RunningTotalError)));
      }
      time.Stop();

      Console.WriteLine();
      Console.WriteLine("-------------------------------------------------------------------");
      Console.WriteLine("  Made  {0,10:n0} requests in {1:n} sec at {2:n0} ops/sec".Args(totalCount, time.ElapsedSec, totalCount / time.ElapsedSec));
      Console.WriteLine("  OK    {0,10:n0} requests in {1:n} sec at {2:n0} ops/sec".Args(m_RunningTotalOK, time.ElapsedSec, m_RunningTotalOK / time.ElapsedSec));
      Console.WriteLine("  ERROR {0,10:n0} requests in {1:n} sec at {2:n0} ops/sec".Args(m_RunningTotalError, time.ElapsedSec, m_RunningTotalError / time.ElapsedSec));
    }

    private class worker
    {
      public worker(LoadAction action, int count)
      {
        Action = action;
        Count = count;
        m_Thread = new Thread(tbody);
        m_Thread.IsBackground = false;

        m_Client = new HttpClient();
        m_Done = new TaskCompletionSource();

        m_Thread.Start();
      }

      private readonly LoadAction Action;
      private readonly int  Count;
      private Thread m_Thread;
      private HttpClient m_Client;
      private TaskCompletionSource m_Done;

      public Task Task => m_Done.Task;

      private void tbody()
      {
         for(var i=0; Action.App.Active && i < Count; i++)
         {
           try
           {
             Interlocked.Increment(ref Action.m_RunningTotal);
             m_Client.GetStringAsync(Action.Uri).Await();
             Interlocked.Increment(ref Action.m_RunningTotalOK);
           }
           catch
           {
             Interlocked.Increment(ref Action.m_RunningTotalError);
           }
         }

        //run test
        m_Done.SetResult();
      }
    }

  }
}
