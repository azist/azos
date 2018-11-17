/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Threading;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Log;
using Azos.Instrumentation;

namespace Azos.Time
{
  /// <summary>
  /// Provides default implementation for IEventTimer
  /// </summary>
  public sealed class EventTimer : DaemonWithInstrumentation<IApplicationComponent>, IEventTimerImplementation
  {
    #region CONSTS
      public const int DEFAULT_RESOLUTION_MS = 500;
      public const int MIN_RESOLUTION_MS = 100;
      public const int MAX_RESOLUTION_MS = 5000;

      private const string THREAD_NAME = "EventTimer";
    #endregion

    #region .ctor
      public EventTimer(IApplication app) : base(app) { }
      public EventTimer(IApplicationComponent director) : base(director) { }

    #endregion

    #region Fields

      private int m_ResolutionMs = DEFAULT_RESOLUTION_MS;
      private bool m_InstrumentationEnabled;
      private Thread m_Thread;

      private Registry<Event> m_Events = new Registry<Event>();

    #endregion

    #region Properties

     public override string ComponentLogTopic => CoreConsts.TIME_TOPIC;

     /// <summary>
     /// Timer resolution in milliseconds
     /// </summary>
     [Config]
     [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_TIME)]
     public int ResolutionMs
     {
       get { return m_ResolutionMs; }
       set { m_ResolutionMs = value < MIN_RESOLUTION_MS ? MIN_RESOLUTION_MS : value > MAX_RESOLUTION_MS ? MAX_RESOLUTION_MS : value;  }
     }

     [Config]
     [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_TIME, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
     public override bool InstrumentationEnabled
     {
       get { return m_InstrumentationEnabled;}
       set { m_InstrumentationEnabled = value;}
     }

     /// <summary>
     /// Lists all events in the instance
     /// </summary>
     public IRegistry<Event> Events { get { return m_Events;}}

    #endregion

    #region Protected

      void IEventTimerImplementation.__InternalRegisterEvent(Event evt)
      {
        if (evt==null || evt.Timer!=this)
          throw new TimeException(StringConsts.ARGUMENT_ERROR+GetType().Name+"__InternalRegisterEvent(evt==null|does not belog to timer)");

        Event existing;
        m_Events.RegisterOrReplace(evt, out existing);
        if ( existing!=null )
        {
          existing.Dispose();
        }
      }

      void IEventTimerImplementation.__InternalUnRegisterEvent(Event evt)
      {
        if (evt==null || evt.Timer!=this)
          throw new TimeException(StringConsts.ARGUMENT_ERROR+GetType().Name+"__InternalUnRegisterEvent(evt==null|does not belog to timer)");

        m_Events.Unregister(evt);
      }

      protected override void DoConfigure(IConfigSectionNode node)
      {
        base.DoConfigure(node);
        foreach (var enode in node.Children.Where(n => n.IsSameName(Event.CONFIG_EVENT_SECTION)))
        {
           FactoryUtils.Make<Event>(enode, typeof(Event), new object[]{this, enode});
        }
      }

      protected override void DoStart()
      {
        base.DoStart();
        m_Thread = new Thread(threadSpin);
        m_Thread.Name = THREAD_NAME;
        m_Thread.IsBackground = true;
        m_Thread.Start();
      }

      protected override void DoWaitForCompleteStop()
      {
        base.DoWaitForCompleteStop();
        m_Thread.Join();
        m_Thread = null;
      }

    #endregion

    #region .pvt


      private DateTime m_LastStatDump = DateTime.MinValue;

      private void threadSpin()
      {
        const int INSTR_DUMP_MS = 7397;

        while(App.Active && Running)
        {
          var utcNow = App.TimeSource.UTCNow;
          visitAll(utcNow);

          if (m_InstrumentationEnabled && (utcNow-m_LastStatDump).TotalMilliseconds>INSTR_DUMP_MS)
          {
            dumpStats();
            m_LastStatDump = utcNow;
          }

          Thread.Sleep(m_ResolutionMs);
        }
      }


      private int m_stat_EventCount;
      private int m_stat_EventsFired;

      private void dumpStats()
      {
        Instrumentation.EventCount.Record( m_stat_EventCount );
        Instrumentation.FiredEventCount.Record( m_stat_EventsFired );
        m_stat_EventsFired = 0;
      }


      private void visitAll(DateTime utcNow)
      {
        try
        {
           foreach(var evt in m_Events)
           {
             try
             {
               var fired = evt.VisitAndCheck(utcNow);
               if (fired)
                 m_stat_EventsFired++;
             }
             catch(Exception evtError)
             {
               WriteLog(MessageType.Error, "Event: "+evt.Name, "Event processing leaked: " + evtError.ToMessageWithType(), evtError);
             }
           }

           m_stat_EventCount = m_Events.Count;

        }
        catch(Exception error)
        {
          WriteLog(MessageType.CatastrophicError, nameof(visitAll), "Exception leaked: " + error.ToMessageWithType(), error);
        }
      }

    #endregion
  }
}
