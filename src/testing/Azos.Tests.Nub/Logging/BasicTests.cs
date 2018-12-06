/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Linq;
using System.Threading;

using Azos.Apps;
using Azos.Conf;
using Azos.Log;
using Azos.Log.Sinks;
using Azos.Scripting;

namespace Azos.Tests.Nub.Logging
{
  [Runnable]
  public class BasicTests
  {
    private const int DAEMON_FLUSH_WAIT_MS = 200;

    //In this test we try to setup our own logger instance manually
    //injecting memory buffer by code
    [Run]
    public void CreateByCodeOwnDaemon()
    {
      using(var logger = new LogDaemon(NOPApplication.Instance))
      {
        logger.WriteInterval = 0;//set the minimum flush period

        Aver.Throws<AzosException>( ()=> logger.Start() ); //can not start daemon with no sinks
        Aver.IsFalse( logger.Running );

        Aver.IsFalse( logger.Sinks.Any() );

        using(var sink = new MemoryBufferSink(logger))
        {
          Console.WriteLine(sink.Name);
          Aver.IsTrue(sink.Name.StartsWith("MemoryBufferSink."));//anonymous sinks get their name from their:   'type.fid'

          logger.Start();//now it can start
          Aver.IsTrue(logger.Running);

          Aver.AreSameRef(sink, logger.Sinks.First());//now there is a sink registered which is this one
          Aver.IsTrue(sink.Running);//it was auto-started by the logger

          logger.Write(MessageType.DebugB, "This is a message #1");//so this messages goes in it

          Thread.Sleep(DAEMON_FLUSH_WAIT_MS);//make sure async flush happens

          Aver.IsTrue( sink.Buffered.Any() );//because sink was active

          var logged = sink.Buffered.FirstOrDefault(); // we get the first message buffered

          Aver.AreEqual("This is a message #1", logged.Text);
          Aver.IsTrue(MessageType.DebugB == logged.Type);

          sink.WaitForCompleteStop();//stop the sink
          Aver.IsFalse(sink.Running);
          logger.Write(MessageType.Debug, "This is a message #2");

          Thread.Sleep(DAEMON_FLUSH_WAIT_MS);//make sure async flush happens

          Aver.AreEqual(1, sink.Buffered.Count());//because sink was not turned on for 2nd, only the first message got in, 2nd got lost
        }

        Aver.IsFalse(logger.Sinks.Any());//again, no sinks left
      }
    }

    //In this test we try to setup our own logger instance manually
    //injecting memory buffer from config vector
    [Run]
    public void CreateByConfigurationOwnDaemon()
    {
      var conf = @"
log
{
  name='YourFriendLogger'
  write-interval-ms=0

  sink
  {
    name='Memorizer'
    type='Azos.Log.Sinks.MemoryBufferSink, Azos'
  }
}".AsLaconicConfig();

      using (var logger = FactoryUtils.MakeAndConfigureComponent<LogDaemon>(NOPApplication.Instance, conf, typeof(LogDaemon)))
      {
        Aver.AreEqual("YourFriendLogger", logger.Name);//name got set
        Aver.AreEqual(LogDaemon.MIN_INTERVAL_MSEC, logger.WriteInterval);//got set the minimum flush period from config
        Aver.IsTrue(logger.Sinks.Any());

        logger.Start();
        Aver.IsTrue(logger.Running);

        var sink = logger.Sinks.FirstOrDefault();
        Aver.IsNotNull(sink);
        Aver.AreEqual("Memorizer", sink.Name);
        Aver.IsTrue(sink.Running);//it was auto-started by the logger

        var memsink = sink as MemoryBufferSink;
        Aver.IsNotNull(memsink);

        logger.Write(MessageType.DebugC, "Yes #1");//so this messages goes in it

        Thread.Sleep(DAEMON_FLUSH_WAIT_MS);//make sure async flush happens

        Aver.IsTrue(memsink.Buffered.Any());//because sink was now turned on

        var logged = memsink.Buffered.FirstOrDefault(); // get the first message

        Aver.AreEqual("Yes #1", logged.Text);
        Aver.IsTrue(MessageType.DebugC == logged.Type);
      }
    }


    //In this test we try to setup our own logger instance manually
    //injecting memory buffer from config vector
    [Run]
    public void CreateByConfigurationOwnDaemonWithTwoSinks()
    {
      var conf = @"
log
{
  name='YourFriendLogger'
  write-interval-ms=0

  sink
  {
    name='CatchAll'
    order=100
    type='Azos.Log.Sinks.MemoryBufferSink, Azos'
  }

  sink
  {
    name='Problems'
    min-level=Warning
    order=0
    type='Azos.Log.Sinks.MemoryBufferSink, Azos'
  }
}".AsLaconicConfig();

      using (var logger = FactoryUtils.MakeAndConfigureComponent<LogDaemon>(NOPApplication.Instance, conf, typeof(LogDaemon)))
      {
        Aver.AreEqual("YourFriendLogger", logger.Name);//name got set
        Aver.AreEqual(LogDaemon.MIN_INTERVAL_MSEC, logger.WriteInterval);//got set the minimum flush period from config
        Aver.AreEqual(2, logger.Sinks.Count);

        logger.Start();
        Aver.IsTrue(logger.Running);

        var s0 = logger.Sinks["Problems"] as MemoryBufferSink;
        var s1 = logger.Sinks["CatchAll"] as MemoryBufferSink;//catchall order =100

        Aver.IsNotNull(s0);
        Aver.IsNotNull(s1);
        Aver.AreSameRef(s0, logger.Sinks[0]);
        Aver.AreSameRef(s1, logger.Sinks[1]);

        Aver.IsNull( logger.Sinks["TheOneWhich isNotthere"]);
        Aver.IsNull( logger.Sinks[324234] );

        logger.Write(MessageType.Info, "This was info");
        logger.Write(MessageType.Debug, "Now debug");
        logger.Write(MessageType.Error, "And now error");

        Thread.Sleep(DAEMON_FLUSH_WAIT_MS);//make sure async flush happens

        Aver.AreEqual(3, s1.Buffered.Count());
        Aver.AreEqual(1, s0.Buffered.Count());

        Aver.AreEqual("And now error", s0.Buffered.FirstOrDefault().Text);
        Aver.AreEqual("This was info", s1.Buffered.FirstOrDefault().Text);
        Aver.AreEqual("Now debug", s1.Buffered.Skip(1).FirstOrDefault().Text);
        Aver.AreEqual("And now error", s1.Buffered.Skip(2).FirstOrDefault().Text);
      }
    }


  }
}