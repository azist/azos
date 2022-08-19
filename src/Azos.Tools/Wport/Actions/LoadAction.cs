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

    private long m_RunningTotalBytes;

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


      long pTotal = 0;
      long pOpsSec = 0;
      var time = Timeter.StartNew();
      while(tasks.Any(t => !t.IsCompleted) && App.Active)
      {
        Thread.Sleep(1000);
        var totalNow = Interlocked.Read(ref m_RunningTotal);
        var totalDelta = totalNow - pTotal; pTotal = totalNow;
        var opsSec = (long)(totalNow / time.ElapsedSec);
        var opsSecDelta = opsSec - pOpsSec; pOpsSec = opsSec;

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write("... {0,8:n0}".Args(totalNow));
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(" (+{0:n0})".Args(totalDelta).PadRight(8));
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(" requests in {0:n} sec ".Args(time.ElapsedSec));
        Console.Write(" at ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("{0:n0}".Args(opsSec));
        if (opsSecDelta != 0)
        {
          Console.ForegroundColor = opsSecDelta > 0 ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed;
          Console.Write(" ({0}{1:n0})".Args(opsSecDelta > 0 ? "+" : "", opsSecDelta).PadRight(8));
        }
        else
        {
          Console.Write(" ".PadRight(8));
        }
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(" ops/sec;");
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.Write(" OK: ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("{0:n0}".Args(Interlocked.Read(ref m_RunningTotalOK)));
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write(" Error: ");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("{0:n0}".Args(Interlocked.Read(ref m_RunningTotalError)));
        Console.WriteLine();
      }
      time.Stop();

      Console.WriteLine();
      Console.ForegroundColor = ConsoleColor.DarkGray;
      Console.WriteLine("-------------------------------------------------------------------");
      Console.ForegroundColor = ConsoleColor.Gray;
      Console.WriteLine("  Made:  {0,10:n0} requests in {1:n} sec at {2:n0} ops/sec".Args(m_RunningTotal, time.ElapsedSec, m_RunningTotal / time.ElapsedSec));
      Console.WriteLine("  Transferred: {0:n0} char/bytes in {1:n} sec at {2:n0} char/sec".Args(IOUtils.FormatByteSizeWithPrefix(m_RunningTotalBytes), time.ElapsedSec, m_RunningTotalBytes / time.ElapsedSec));
      Console.WriteLine("  OK:    {0,10:n0} requests in {1:n} sec at {2:n0} ops/sec".Args(m_RunningTotalOK, time.ElapsedSec, m_RunningTotalOK / time.ElapsedSec));
      if (m_RunningTotalError > 0)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  ERROR: {0,10:n0} requests in {1:n} sec at {2:n0} ops/sec".Args(m_RunningTotalError, time.ElapsedSec, m_RunningTotalError / time.ElapsedSec));
        Console.ResetColor();
      }
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
            var got = m_Client.GetStringAsync(Action.Uri).AwaitResult();
            Interlocked.Add(ref Action.m_RunningTotalBytes, got.Length);
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
